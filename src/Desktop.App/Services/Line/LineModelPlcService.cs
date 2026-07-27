using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Runtime;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;
using Shared.Models.ModelProfiles;

namespace Desktop.App.Services.Line;

public sealed class LineModelPlcService : IDisposable
{
    private const int PageSize = 6;

    private readonly string _lineName;
    private readonly IPlcService _plcService;
    private readonly IModelProfileApiClient _apiClient;
    private readonly IUserApiClient _userApiClient;
    private readonly LineTagSet _tags;
    private readonly string _lineDataKey;
    private readonly Dictionary<string, PlcTagDefinition> _fieldToTagMap;
    private readonly Dictionary<string, PlcTagDefinition> _autoFieldToTagMap;

    private List<ModelProfileDto> _allModels = [];
    private List<ModelProfileDto> _browseModels = [];
    private int _currentPage;
    private string _activeKeyword = string.Empty;
    private string? _editingModelName;
    private bool _isStarted;
    private bool _isDisposed;
    private volatile bool _isProcessing;

    private bool _prevSearch;
    private bool _prevEdit;
    private bool _prevNext;
    private bool _prevPrevious;
    private bool _prevSaveModel;
    private bool _prevAutoLoadDataModel;

    public LineModelPlcService(
        string lineName,
        IPlcService plcService,
        IModelProfileApiClient apiClient,
        IUserApiClient userApiClient,
        LineTagSet tags)
    {
        _lineName = lineName;
        _plcService = plcService;
        _apiClient = apiClient;
        _userApiClient = userApiClient;
        _tags = tags;
        _lineDataKey = lineName.Contains('1') ? "line1" : "line2";

        _fieldToTagMap = new Dictionary<string, PlcTagDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["jigType"] = tags.JigType,
            ["pickInputX"] = tags.PickInputX,
            ["pickInputZ"] = tags.PickInputZ,
            ["pickOp1X"] = tags.PickOp1X,
            ["pickOp1Z"] = tags.PickOp1Z,
            ["pickOp2X"] = tags.PickOp2X,
            ["pickOp2Z"] = tags.PickOp2Z,
            ["placeOp1X"] = tags.DropOp1X,
            ["placeOp1Z"] = tags.DropOp1Z,
            ["placeOp2X"] = tags.DropOp2X,
            ["placeOp2Z"] = tags.DropOp2Z,
            ["placeMeasureX"] = tags.DropMeasureX,
            ["placeMeasureZ"] = tags.DropMeasureZ,
            ["jigProductHeight"] = tags.JigSupportInput,
            ["grindingTimeOp1"] = tags.GrindTimeOp1,
            ["grindingTimeOp2"] = tags.GrindTimeOp2,
        };

        _autoFieldToTagMap = new Dictionary<string, PlcTagDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["jigType"] = tags.AutoJigType,
            ["pickInputX"] = tags.AutoPickInputX,
            ["pickInputZ"] = tags.AutoPickInputZ,
            ["pickOp1X"] = tags.AutoPickOp1X,
            ["pickOp1Z"] = tags.AutoPickOp1Z,
            ["pickOp2X"] = tags.AutoPickOp2X,
            ["pickOp2Z"] = tags.AutoPickOp2Z,
            ["placeOp1X"] = tags.AutoDropOp1X,
            ["placeOp1Z"] = tags.AutoDropOp1Z,
            ["placeOp2X"] = tags.AutoDropOp2X,
            ["placeOp2Z"] = tags.AutoDropOp2Z,
            ["placeMeasureX"] = tags.AutoDropMeasureX,
            ["placeMeasureZ"] = tags.AutoDropMeasureZ,
            ["jigProductHeight"] = tags.AutoJigSupportInput,
            ["grindingTimeOp1"] = tags.AutoGrindTimeOp1,
            ["grindingTimeOp2"] = tags.AutoGrindTimeOp2,
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted || _isDisposed)
        {
            return;
        }

        _isStarted = true;
        Debug.WriteLine($"[LineModel-{_lineName}] Service starting...");

        try
        {
            await LoadModelsFromApiAsync(cancellationToken);
            SetBrowseModels(_allModels, resetPage: true);

            if (_plcService.IsConnected)
            {
                await RepublishStateAsync(refreshModels: false);
                Debug.WriteLine($"[LineModel-{_lineName}] Initial load: {_allModels.Count} models");
            }
            else
            {
                Debug.WriteLine($"[LineModel-{_lineName}] PLC not connected, skip initial publish");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Initial load failed: {ex.Message}");
        }

        _plcService.DataUpdated += OnPlcDataUpdated;
        _plcService.ConnectionChanged += OnPlcConnectionChanged;
        InitializePreviousState();

        Debug.WriteLine($"[LineModel-{_lineName}] Service started");
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _plcService.DataUpdated -= OnPlcDataUpdated;
        _plcService.ConnectionChanged -= OnPlcConnectionChanged;
        Debug.WriteLine($"[LineModel-{_lineName}] Disposed");
    }

    private void OnPlcDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        var curSearch = _plcService.GetValue(_tags.Search.Name, false);
        var curEdit = _plcService.GetValue(_tags.Edit.Name, false);
        var curNext = _plcService.GetValue(_tags.Next.Name, false);
        var curPrevious = _plcService.GetValue(_tags.Previous.Name, false);
        var curSaveModel = _plcService.GetValue(_tags.SaveModel.Name, false);
        var curAutoLoadDataModel = _plcService.GetValue(_tags.AutoLoadDataModel.Name, false);

        var searchTriggered = curSearch && !_prevSearch;
        var editTriggered = curEdit && !_prevEdit;
        var nextTriggered = curNext && !_prevNext;
        var previousTriggered = curPrevious && !_prevPrevious;
        var saveTriggered = curSaveModel && !_prevSaveModel;
        var autoLoadTriggered = curAutoLoadDataModel && !_prevAutoLoadDataModel;

        // Luôn update prev state — kể cả khi đang processing
        // Nếu không update ở đây, _prevEdit bị "kẹt" = true và lần sau PLC on lại không trigger
        _prevSearch = curSearch;
        _prevEdit = curEdit;
        _prevNext = curNext;
        _prevPrevious = curPrevious;
        _prevSaveModel = curSaveModel;
        _prevAutoLoadDataModel = curAutoLoadDataModel;

        if (_isProcessing)
        {
            return;
        }

        if (searchTriggered)
        {
            DispatchAsync(HandleSearchAsync);
        }
        else if (editTriggered)
        {
            DispatchAsync(HandleEditAsync);
        }
        else if (saveTriggered)
        {
            DispatchAsync(HandleSaveModelAsync);
        }
        else if (autoLoadTriggered)
        {
            DispatchAsync(HandleAutoLoadDataModelAsync);
        }
        else if (nextTriggered)
        {
            DispatchAsync(HandleNextAsync);
        }
        else if (previousTriggered)
        {
            DispatchAsync(HandlePreviousAsync);
        }
    }

    private void OnPlcConnectionChanged(object? sender, bool isConnected)
    {
        if (_isDisposed || !isConnected)
        {
            return;
        }

        DispatchAsync(() => RepublishStateAsync(refreshModels: true));
    }

    private void DispatchAsync(Func<Task> handler)
    {
        if (_isProcessing)
        {
            return;
        }

        _isProcessing = true;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(50);
                await handler();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LineModel-{_lineName}] Handler error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        });
    }

    private async Task HandleSearchAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] Search triggered");

        try
        {
            await AckCommandAsync(_tags.Search.Name);
            await ResetErrorAsync();

            var keyword = (_plcService.GetValue(_tags.ModelSearchName.Name, string.Empty) ?? string.Empty).Trim();
            _activeKeyword = keyword;

            await LoadModelsFromApiAsync();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                SetBrowseModels(_allModels, resetPage: true);
                await WriteBrowsePageAsync();
                return;
            }

            var matches = _allModels
                .Where(model => HasFuzzyMatch(model.ModelName, keyword))
                .OrderBy(model => ScoreFuzzyMatch(model.ModelName, keyword))
                .ThenBy(model => model.ModelName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (matches.Count == 0)
            {
                _browseModels = [];
                await ClearBrowsePageAsync();
                await ClearEditModelDataAsync();
                _editingModelName = null;
                await TryWriteErrorAsync($"Không tìm thấy gần đúng '{keyword}'");
                return;
            }

            SetBrowseModels(matches, resetPage: true);
            await WriteBrowsePageAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Search error: {ex.Message}");
            await TryWriteErrorAsync($"Loi search: {ex.Message}");
        }
    }

    private async Task HandleEditAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] Edit triggered");

        try
        {
            await AckCommandAsync(_tags.Edit.Name);
            await ResetErrorAsync();

            var modelName = (_plcService.GetValue(_tags.ModelSearchName.Name, string.Empty) ?? string.Empty)
                            .Replace("\0", "").Trim();
            if (string.IsNullOrWhiteSpace(modelName))
            {
                await ClearEditModelDataAsync();
                _editingModelName = null;
                await TryWriteErrorAsync("Nhap chinh xac ten model de sua");
                return;
            }

            var machineId = await _apiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode);
            var model = await _apiClient.GetByNameAsync(machineId, modelName);
            if (model is null)
            {
                await ClearEditModelDataAsync();
                _editingModelName = null;
                await TryWriteErrorAsync($"Khong tim thay model '{modelName}'");
                return;
            }

            await WriteEditModelDataAsync(model);
            _editingModelName = model.ModelName;
            await ResetErrorAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Edit error: {ex.Message}");
            await ClearEditModelDataAsync();
            _editingModelName = null;
            await TryWriteErrorAsync($"Loi edit: {ex.Message}");
        }
    }

    private async Task HandleNextAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] Next triggered");

        try
        {
            await AckCommandAsync(_tags.Next.Name);

            if (_currentPage < TotalPages())
            {
                _currentPage++;
            }

            await WriteBrowsePageAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Next error: {ex.Message}");
        }
    }

    private async Task HandlePreviousAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] Previous triggered");

        try
        {
            await AckCommandAsync(_tags.Previous.Name);

            if (_currentPage > 1)
            {
                _currentPage--;
            }

            await WriteBrowsePageAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Previous error: {ex.Message}");
        }
    }

    private async Task HandleSaveModelAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] SaveModel triggered");

        try
        {
            await AckCommandAsync(_tags.SaveModel.Name);
            await SafeWriteAsync(_tags.SaveSuccess.Name, false);
            await ResetErrorAsync();

            var modelName = (_plcService.GetValue(_tags.IdModel.Name, string.Empty) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(modelName))
            {
                await TryWriteErrorAsync("Tên model trống");
                return;
            }

            var accountName = (_plcService.GetValue(_tags.AccountName.Name, string.Empty) ?? string.Empty).Trim();
            var password = (_plcService.GetValue(_tags.Password.Name, string.Empty) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(password))
            {
                await TryWriteErrorAsync("Thiếu tài khoản hoặc mật khẩu");
                return;
            }

            var loginResult = await _userApiClient.LoginAsync(accountName, password);
            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.AccessToken))
            {
                await TryWriteErrorAsync(loginResult.ErrorMessage ?? "Đăng nhập thất bại");
                return;
            }

            await LoadModelsFromApiAsync();

            var existingModel = FindExactModel(modelName);
            if (existingModel is null)
            {
                await TryWriteErrorAsync($"Model '{modelName}' không tồn tại");
                return;
            }

            var lineData = ReadEditModelDataFromPlc();
            var diameterOp1 = _plcService.GetValue<float>(_tags.DiameterOp1.Name);
            var diameterOp2 = _plcService.GetValue<float>(_tags.DiameterOp2.Name);
            var machineId = await _apiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode);
            var request = new SaveModelProfileRequest
            {
                ModelName = existingModel.ModelName,
                ItemType = existingModel.ItemType,
                MachiningProgram = existingModel.MachiningProgram,
                Spare1 = existingModel.Spare1,
                Spare2 = existingModel.Spare2,
                OuterShaftDiameter = existingModel.OuterShaftDiameter,
                DiameterOp1 = RoundPlcFloat(diameterOp1),
                DiameterOp2 = RoundPlcFloat(diameterOp2),
                TrayUsage = existingModel.TrayUsage,
                TrayType = existingModel.TrayType,
                RobotData = NormalizeDictionary(existingModel.RobotData),
                Line1Data = _lineDataKey == "line1" ? lineData : NormalizeDictionary(existingModel.Line1Data),
                Line2Data = _lineDataKey == "line2" ? lineData : NormalizeDictionary(existingModel.Line2Data),
            };

            await _apiClient.UpdateAsync(
                machineId,
                existingModel.Id,
                request,
                accessTokenOverride: loginResult.AccessToken);

            _editingModelName = existingModel.ModelName;
            await SafeWriteAsync(_tags.SaveSuccess.Name, true);
            await ResetErrorAsync();
            await RepublishStateAsync(refreshModels: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Save error: {ex.Message}");
            await TryWriteErrorAsync($"Loi luu: {ex.Message}");
        }
    }

    private async Task HandleAutoLoadDataModelAsync()
    {
        Debug.WriteLine($"[LineModel-{_lineName}] AutoLoadDataModel triggered");

        try
        {
            await SafeWriteAsync(_tags.AutoDoneLoadDataModel.Name, false);
            await ResetErrorAsync();

            var modelNameToLoad = (_plcService.GetValue(_tags.AutoModelNameToLoad.Name, string.Empty) ?? string.Empty)
                                  .Replace("\0", "").Trim();
            if (string.IsNullOrWhiteSpace(modelNameToLoad))
            {
                await AckCommandAsync(_tags.AutoLoadDataModel.Name);
                await TryWriteErrorAsync("Ten model can load trong");
                return;
            }

            var machineId = await _apiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode);
            var model = await _apiClient.GetByNameAsync(machineId, modelNameToLoad);
            if (model is null)
            {
                await AckCommandAsync(_tags.AutoLoadDataModel.Name);
                await TryWriteErrorAsync($"Khong tim thay model '{modelNameToLoad}'");
                return;
            }

            await WriteAutoModelDataAsync(model);

            await AckCommandAsync(_tags.AutoLoadDataModel.Name);
            await SafeWriteAsync(_tags.AutoDoneLoadDataModel.Name, true);
            await ResetErrorAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Auto load error: {ex.Message}");
            await AckCommandAsync(_tags.AutoLoadDataModel.Name);
            await SafeWriteAsync(_tags.AutoDoneLoadDataModel.Name, false);
            await TryWriteErrorAsync($"Loi load auto: {ex.Message}");
        }
    }

    private void InitializePreviousState()
    {
        _prevSearch = _plcService.GetValue(_tags.Search.Name, false);
        _prevEdit = _plcService.GetValue(_tags.Edit.Name, false);
        _prevNext = _plcService.GetValue(_tags.Next.Name, false);
        _prevPrevious = _plcService.GetValue(_tags.Previous.Name, false);
        _prevSaveModel = _plcService.GetValue(_tags.SaveModel.Name, false);
        _prevAutoLoadDataModel = _plcService.GetValue(_tags.AutoLoadDataModel.Name, false);
    }

    private async Task LoadModelsFromApiAsync(CancellationToken cancellationToken = default)
    {
        var machineId = await _apiClient.ResolveMachineIdAsync(AppSettings.Current.MachineCode, cancellationToken);
        _allModels = (await _apiClient.GetAllAsync(machineId, cancellationToken)).ToList();
    }

    private int TotalPages()
    {
        return _browseModels.Count == 0 ? 0 : (int)Math.Ceiling((double)_browseModels.Count / PageSize);
    }

    private void SetBrowseModels(IEnumerable<ModelProfileDto> models, bool resetPage)
    {
        _browseModels = models.ToList();

        if (resetPage)
        {
            _currentPage = _browseModels.Count == 0 ? 0 : 1;
        }
        else if (_browseModels.Count == 0)
        {
            _currentPage = 0;
        }
    }

    private async Task WriteBrowsePageAsync()
    {
        if (_browseModels.Count == 0)
        {
            await ClearBrowsePageAsync();
            return;
        }

        var totalPages = TotalPages();
        _currentPage = Math.Clamp(_currentPage <= 0 ? 1 : _currentPage, 1, totalPages);
        var startIndex = (_currentPage - 1) * PageSize;

        for (var i = 0; i < PageSize; i++)
        {
            var modelIndex = startIndex + i;
            var name = modelIndex < _browseModels.Count
                ? _browseModels[modelIndex].ModelName
                : string.Empty;

            await SafeWriteAsync(_tags.ModelResults[i].Name, name);
        }

        await SafeWriteAsync(_tags.Page.Name, (short)_currentPage);
        await SafeWriteAsync(_tags.TotalPages.Name, (short)totalPages);
    }

    private async Task ClearBrowsePageAsync()
    {
        for (var i = 0; i < PageSize; i++)
        {
            await SafeWriteAsync(_tags.ModelResults[i].Name, string.Empty);
        }

        _currentPage = 0;
        await SafeWriteAsync(_tags.Page.Name, (short)0);
        await SafeWriteAsync(_tags.TotalPages.Name, (short)0);
    }

    private async Task RepublishStateAsync(bool refreshModels)
    {
        if (refreshModels)
        {
            await LoadModelsFromApiAsync();
        }

        if (string.IsNullOrWhiteSpace(_activeKeyword))
        {
            SetBrowseModels(_allModels, resetPage: false);
        }
        else
        {
            SetBrowseModels(
                _allModels
                    .Where(model => HasFuzzyMatch(model.ModelName, _activeKeyword))
                    .OrderBy(model => ScoreFuzzyMatch(model.ModelName, _activeKeyword))
                    .ThenBy(model => model.ModelName, StringComparer.OrdinalIgnoreCase),
                resetPage: false);
        }

        await WriteBrowsePageAsync();

        if (string.IsNullOrWhiteSpace(_editingModelName))
        {
            return;
        }

        var editingModel = FindExactModel(_editingModelName);
        if (editingModel is null)
        {
            await ClearEditModelDataAsync();
            _editingModelName = null;
            return;
        }

        await WriteEditModelDataAsync(editingModel);
    }

    private async Task WriteEditModelDataAsync(ModelProfileDto model)
    {
        var rawLineData = _lineDataKey == "line1" ? model.Line1Data : model.Line2Data;
        var lineData = NormalizeDictionary(rawLineData);

        await SafeWriteAsync(_tags.IdModel.Name, model.ModelName);
        await SafeWriteAsync(_tags.DiameterOp1.Name, model.DiameterOp1 ?? 0f);
        await SafeWriteAsync(_tags.DiameterOp2.Name, model.DiameterOp2 ?? 0f);

        foreach (var (key, tag) in _fieldToTagMap)
        {
            lineData.TryGetValue(key, out var value);
            await SafeWriteAsync(tag.Name, ConvertToTagWriteValue(tag, value));
        }
    }

    private async Task WriteAutoModelDataAsync(ModelProfileDto model)
    {
        var rawLineData = _lineDataKey == "line1" ? model.Line1Data : model.Line2Data;
        var lineData = NormalizeDictionary(rawLineData);

        await SafeWriteAsync(_tags.AutoModelName.Name, model.ModelName);
        await SafeWriteAsync(_tags.AutoProgramId.Name, model.MachiningProgram ?? 0);
        await SafeWriteAsync(_tags.AutoDiameterOp1.Name, model.DiameterOp1 ?? 0f);
        await SafeWriteAsync(_tags.AutoDiameterOp2.Name, model.DiameterOp2 ?? 0f);

        foreach (var (key, tag) in _autoFieldToTagMap)
        {
            lineData.TryGetValue(key, out var value);
            await SafeWriteAsync(tag.Name, ConvertToTagWriteValue(tag, value));
        }
    }

    private async Task ClearEditModelDataAsync()
    {
        await SafeWriteAsync(_tags.IdModel.Name, string.Empty);
        await SafeWriteAsync(_tags.DiameterOp1.Name, 0f);
        await SafeWriteAsync(_tags.DiameterOp2.Name, 0f);

        foreach (var tag in _fieldToTagMap.Values)
        {
            await SafeWriteAsync(tag.Name, GetDefaultValue(tag.DataType));
        }
    }

    private Dictionary<string, object?> ReadEditModelDataFromPlc()
    {
        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, tag) in _fieldToTagMap)
        {
            var value = tag.DataType switch
            {
                PlcTagDataType.Int32 => (object)_plcService.GetValue<int>(tag.Name),
                PlcTagDataType.Float => _plcService.GetValue<float>(tag.Name),
                PlcTagDataType.String => _plcService.GetValue(tag.Name, string.Empty),
                _ => _plcService.GetValue<object?>(tag.Name, null),
            };

            data[key] = value;
        }

        return data;
    }

    private ModelProfileDto? FindExactModel(string modelName)
    {
        return _allModels.FirstOrDefault(model =>
            string.Equals(model.ModelName.Trim(), modelName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private async Task SafeWriteAsync(string tagName, object? value)
    {
        try
        {
            if (_plcService.IsConnected)
            {
                await _plcService.WriteAsync(tagName, value);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LineModel-{_lineName}] Write '{tagName}' failed: {ex.Message}");
        }
    }

    private async Task AckCommandAsync(string tagName)
    {
        await SafeWriteAsync(tagName, false);
    }

    private async Task ResetErrorAsync()
    {
        await SafeWriteAsync(_tags.ErrorFlag.Name, false);
        await SafeWriteAsync(_tags.ErrorMessage.Name, string.Empty);
    }

    private async Task TryWriteErrorAsync(string message)
    {
        if (message.Length > 40)
        {
            message = message[..40];
        }

        await SafeWriteAsync(_tags.ErrorFlag.Name, true);
        await SafeWriteAsync(_tags.ErrorMessage.Name, message);
    }

    private static Dictionary<string, object?> NormalizeDictionary(Dictionary<string, object?>? data)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (data is null)
        {
            return normalized;
        }

        foreach (var (key, value) in data)
        {
            normalized[key] = NormalizeValue(value);
        }

        return normalized;
    }

    private static object? NormalizeValue(object? value) => value switch
    {
        JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => null,
        JsonElement { ValueKind: JsonValueKind.True } => true,
        JsonElement { ValueKind: JsonValueKind.False } => false,
        JsonElement { ValueKind: JsonValueKind.Number } jsonNumber when jsonNumber.TryGetInt32(out var intValue) => intValue,
        JsonElement { ValueKind: JsonValueKind.Number } jsonNumber when jsonNumber.TryGetDouble(out var doubleValue) => doubleValue,
        JsonElement { ValueKind: JsonValueKind.String } jsonString => jsonString.GetString() ?? string.Empty,
        JsonElement jsonElement => jsonElement.ToString(),
        _ => value,
    };

    private static int ConvertToInt(object? value) => value switch
    {
        JsonElement { ValueKind: JsonValueKind.Number } jsonNumber when jsonNumber.TryGetInt32(out var intValue) => intValue,
        JsonElement { ValueKind: JsonValueKind.String } jsonString when int.TryParse(jsonString.GetString(), out var parsedInt) => parsedInt,
        int intValue => intValue,
        long longValue => (int)longValue,
        double doubleValue => (int)doubleValue,
        float floatValue => (int)floatValue,
        string text when int.TryParse(text, out var parsed) => parsed,
        _ => 0,
    };

    private static float ConvertToFloat(object? value) => value switch
    {
        JsonElement { ValueKind: JsonValueKind.Number } jsonNumber when jsonNumber.TryGetDouble(out var doubleValue) => (float)doubleValue,
        JsonElement { ValueKind: JsonValueKind.String } jsonString when float.TryParse(
            jsonString.GetString(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var parsedFloat) => parsedFloat,
        float floatValue => floatValue,
        double doubleValue => (float)doubleValue,
        int intValue => intValue,
        long longValue => longValue,
        string text when float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => 0f,
    };

    private static float RoundPlcFloat(float value)
        => MathF.Round(value, 3, MidpointRounding.AwayFromZero);

    private static string ConvertToStringValue(object? value) => value switch
    {
        JsonElement { ValueKind: JsonValueKind.String } jsonString => jsonString.GetString() ?? string.Empty,
        JsonElement jsonElement => jsonElement.ToString(),
        null => string.Empty,
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
    };

    private static object ConvertToTagWriteValue(PlcTagDefinition tag, object? value)
    {
        return tag.DataType switch
        {
            PlcTagDataType.Int16 => (short)ConvertToInt(value),
            PlcTagDataType.Int32 => (object)ConvertToInt(value),
            PlcTagDataType.Float => ConvertToFloat(value),
            PlcTagDataType.String => ConvertToStringValue(value),
            _ => value ?? GetDefaultValue(tag.DataType),
        };
    }

    private static bool HasFuzzyMatch(string modelName, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        if (modelName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var threshold = Math.Max(2, keyword.Length / 3);
        return ComputeLevenshteinDistance(modelName.ToUpperInvariant(), keyword.ToUpperInvariant()) <= threshold;
    }

    private static int ScoreFuzzyMatch(string modelName, string keyword)
    {
        if (string.Equals(modelName, keyword, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (modelName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        var containsIndex = modelName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (containsIndex >= 0)
        {
            return 10 + containsIndex;
        }

        return 100 + ComputeLevenshteinDistance(modelName.ToUpperInvariant(), keyword.ToUpperInvariant());
    }

    private static int ComputeLevenshteinDistance(string left, string right)
    {
        if (left.Length == 0)
        {
            return right.Length;
        }

        if (right.Length == 0)
        {
            return left.Length;
        }

        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];

        for (var column = 0; column <= right.Length; column++)
        {
            previous[column] = column;
        }

        for (var row = 1; row <= left.Length; row++)
        {
            current[0] = row;

            for (var column = 1; column <= right.Length; column++)
            {
                var cost = left[row - 1] == right[column - 1] ? 0 : 1;
                current[column] = Math.Min(
                    Math.Min(current[column - 1] + 1, previous[column] + 1),
                    previous[column - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    private static object GetDefaultValue(PlcTagDataType dataType) => dataType switch
    {
        PlcTagDataType.Bool => false,
        PlcTagDataType.Int16 => (short)0,
        PlcTagDataType.Int32 => 0,
        PlcTagDataType.Float => 0f,
        PlcTagDataType.String => string.Empty,
        _ => 0,
    };
}
