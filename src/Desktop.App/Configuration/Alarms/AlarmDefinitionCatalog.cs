using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Alarms;

namespace Desktop.App.Configuration.Alarms;

public static class AlarmDefinitionCatalog
{
    public const string DefaultDescription = "Chưa cập nhật mô tả";
    public const string DefaultRemedy = "Chưa cập nhật hướng dẫn khắc phục";
    public const string PlcDisconnectedKey = "system.plc_disconnected";

    public static IReadOnlyList<AlarmDefinition> All { get; } = BuildDefinitions();

    public static void Validate()
    {
        var uniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in All)
        {
            if (!uniqueKeys.Add(definition.Key))
            {
                throw new InvalidOperationException($"Alarm definition key '{definition.Key}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Title.");
            }

            if (definition.SourceType != AlarmSourceType.System && string.IsNullOrWhiteSpace(definition.TagName))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing TagName.");
            }

            if (definition.SourceType != AlarmSourceType.System && string.IsNullOrWhiteSpace(definition.Address))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Address.");
            }

            if (definition.SourceType == AlarmSourceType.CodeWord && !definition.MatchAnyPositiveCode && definition.CodeValue is null)
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' must declare CodeValue.");
            }
        }
    }

    public static string NormalizeDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultDescription : value.Trim();
    }

    public static string NormalizeRemedy(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultRemedy : value.Trim();
    }

    private static IReadOnlyList<AlarmDefinition> BuildDefinitions()
    {
        var definitions = new List<AlarmDefinition>
        {
            CreateSystem(
                PlcDisconnectedKey,
                "Mất kết nối PLC",
                "PLC_DISCONNECTED",
                "Không nhận được dữ liệu runtime từ PLC.",
                "Kiểm tra nguồn PLC, cáp mạng, IP/Port, sau đó kết nối lại."),

            CreateCodeSlot(
                "alarm.slot_1",
                PlcTagCatalog.Alarms.AlarmCode1,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cảnh báo từ PLC - Slot 1",
                "PLC đang phát mã cảnh báo trong thanh ghi D5200.",
                "Đối chiếu mã cảnh báo trên HMI/chương trình PLC và xử lý theo tài liệu máy.",
                suppress: true),

            CreateCodeSlot(
                "alarm.slot_2",
                PlcTagCatalog.Alarms.AlarmCode2,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cảnh báo từ PLC - Slot 2",
                "PLC đang phát mã cảnh báo trong thanh ghi D5201.",
                "Đối chiếu mã cảnh báo trên HMI/chương trình PLC và xử lý theo tài liệu máy.",
                suppress: true),

            CreateCodeSlot(
                "error.slot_1",
                PlcTagCatalog.Alarms.ErrorCode1,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 1",
                "PLC đang phát mã lỗi trong thanh ghi D5202.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_2",
                PlcTagCatalog.Alarms.ErrorCode2,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 2",
                "PLC đang phát mã lỗi trong thanh ghi D5203.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_3",
                PlcTagCatalog.Alarms.ErrorCode3,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 3",
                "PLC đang phát mã lỗi trong thanh ghi D5204.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_4",
                PlcTagCatalog.Alarms.ErrorCode4,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 4",
                "PLC đang phát mã lỗi trong thanh ghi D5205.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateBit(
                "alarm.human_in_working_zone",
                PlcTagCatalog.Alarms.HumanInWorkingZone,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Có người trong vùng hoạt động",
                "Hệ thống an toàn phát hiện người trong vùng làm việc của robot.",
                "Đảm bảo mọi người đã ra khỏi vùng làm việc, xác nhận an toàn rồi mới khởi động lại.",
                stopMachine: true),

            CreateBit(
                "alarm.estop",
                PlcTagCatalog.Alarms.EStop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Dừng khẩn cấp",
                "Nút dừng khẩn cấp đang kích hoạt hoặc chuỗi an toàn đang bị ngắt.",
                "Kiểm tra toàn bộ nút EMG, reset chuỗi an toàn và xác nhận khu vực máy an toàn trước khi khởi động lại.",
                stopMachine: true),
            CreateBit(
                "alarm.x_limit_negative",
                PlcTagCatalog.Alarms.XLimitNegative,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục X vượt giới hạn ngoài",
                "Trục X đã chạm giới hạn mềm/hành trình phía ngoài.",
                "Kiểm tra tọa độ hiện tại, thông số limit trục X và thao tác đưa trục về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.x_limit_positive",
                PlcTagCatalog.Alarms.XLimitPositive,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục X vượt giới hạn trong",
                "Trục X đã chạm giới hạn mềm/hành trình phía trong.",
                "Kiểm tra tọa độ hiện tại, thông số limit trục X và thao tác đưa trục về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.y_limit_negative",
                PlcTagCatalog.Alarms.YLimitNegative,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Y vượt giới hạn trái",
                "Trục Y đã chạm giới hạn mềm/hành trình bên trái.",
                "Kiểm tra vị trí trục Y, cảm biến hành trình và thông số limit trước khi reset.",
                stopMachine: true),
            CreateBit(
                "alarm.y_limit_positive",
                PlcTagCatalog.Alarms.YLimitPositive,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Y vượt giới hạn phải",
                "Trục Y đã chạm giới hạn mềm/hành trình bên phải.",
                "Kiểm tra vị trí trục Y, cảm biến hành trình và thông số limit trước khi reset.",
                stopMachine: true),
            CreateBit(
                "alarm.z_limit_negative",
                PlcTagCatalog.Alarms.ZLimitNegative,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Z vượt giới hạn dưới",
                "Trục Z đã chạm giới hạn mềm/hành trình phía dưới.",
                "Kiểm tra vị trí trục Z, hành trình cơ khí và thông số limit trước khi thao tác lại.",
                stopMachine: true),
            CreateBit(
                "alarm.z_limit_positive",
                PlcTagCatalog.Alarms.ZLimitPositive,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Z vượt giới hạn trên",
                "Trục Z đã chạm giới hạn mềm/hành trình phía trên.",
                "Kiểm tra vị trí trục Z, hành trình cơ khí và thông số limit trước khi thao tác lại.",
                stopMachine: true),
            CreateBit(
                "alarm.pick_slip",
                PlcTagCatalog.Alarms.PickSlip,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Gắp trượt sản phẩm",
                "Tool gắp không giữ được sản phẩm ở công đoạn lấy hàng.",
                "Kiểm tra tay kẹp, áp suất khí, vị trí lấy hàng và tình trạng phôi."),
            CreateBit(
                "alarm.place_slip",
                PlcTagCatalog.Alarms.PlaceSlip,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Thả trượt sản phẩm",
                "Sản phẩm bị trượt trong quá trình đặt xuống.",
                "Kiểm tra cơ cấu thả, vị trí đặt sản phẩm và tình trạng phôi."),
            CreateBit(
                "alarm.canopen_disconnect",
                PlcTagCatalog.Alarms.CanOpenDisconnect,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Mất kết nối CANopen",
                "Hệ thống servo/driver không giao tiếp được trên mạng CANopen.",
                "Kiểm tra nguồn driver, dây truyền thông, đầu nối và khởi động lại mạng CANopen.",
                stopMachine: true),
            CreateBit(
                "alarm.lost_phase",
                PlcTagCatalog.Alarms.LostPhase,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Mất pha nguồn",
                "Nguồn cấp cho máy đang không đủ pha hoặc mất pha.",
                "Kiểm tra CB nguồn, relay báo pha và điện áp nguồn vào.",
                stopMachine: true),
            CreateBit(
                "alarm.not_homed",
                PlcTagCatalog.Alarms.NotHomed,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Máy chưa về gốc",
                "Một hoặc nhiều trục chưa về vị trí home trước khi chạy chu trình.",
                "Thực hiện lệnh home các trục và xác nhận tín hiệu home ổn định trước khi auto."),
            CreateBit(
                "alarm.alarm_driver_x",
                PlcTagCatalog.Alarms.AlarmDriverX,
                AlarmType.Alarm,
                AlarmSeverity.Critical,
                "Lỗi Driver trục X",
                "Driver điều khiển trục X đang báo lỗi",
                "Kiểm tra mã lỗi Driver 50DRV1, Tra bảng mã lỗi và cách xử lý"),
            CreateBit(
                "alarm.alarm_driver_y",
                PlcTagCatalog.Alarms.AlarmDriverY,
                AlarmType.Alarm,
                AlarmSeverity.Critical,
                "Lỗi Driver trục Y",
                "Driver điều khiển trục Y đang báo lỗi",
                "Kiểm tra mã lỗi Driver 51DRV1, Tra bảng mã lỗi và cách xử lý"),
            CreateBit(
                "alarm.alarm_driver_z",
                PlcTagCatalog.Alarms.AlarmDriverZ,
                AlarmType.Alarm,
                AlarmSeverity.Critical,
                "Lỗi Driver trục Z",
                "Driver điều khiển trục Z đang báo lỗi",
                "Kiểm tra mã lỗi Driver 52DRV1, Tra bảng mã lỗi và cách xử lý"),
            CreateBit(
                "alarm.rotate_cylinder_timeout_0",
                PlcTagCatalog.Alarms.RotateCylinderTimeout0,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Timeout xilanh xoay về 0",
                "Xilanh xoay không đến được vị trí 0 trong thời gian cho phép.",
                "Kiểm tra cảm biến vị trí, van điện từ, áp suất khí và cơ khí xilanh."),
            CreateBit(
                "alarm.rotate_cylinder_timeout_90",
                PlcTagCatalog.Alarms.RotateCylinderTimeout90,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Timeout xilanh xoay về 90",
                "Xilanh xoay không đến được vị trí 90 trong thời gian cho phép.",
                "Kiểm tra cảm biến vị trí, van điện từ, áp suất khí và cơ khí xilanh."),
            CreateBit(
                "alarm.cart_1_position_invalid",
                PlcTagCatalog.Alarms.Cart1PositionInvalid,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Xe 1 sai vị trí",
                "Xe 1 không nằm đúng vị trí thao tác mong đợi.",
                "Căn chỉnh lại xe 1, kiểm tra cảm biến vị trí và cơ cấu chốt chặn."),
            CreateBit(
                "alarm.cart_2_position_invalid",
                PlcTagCatalog.Alarms.Cart2PositionInvalid,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Xe 2 sai vị trí",
                "Xe 2 không nằm đúng vị trí thao tác mong đợi.",
                "Căn chỉnh lại xe 2, kiểm tra cảm biến vị trí và cơ cấu chốt chặn."),
            CreateBit(
                "alarm.cart_1_clamp_cylinder_fault",
                PlcTagCatalog.Alarms.Cart1ClampCylinderFault,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi xilanh kẹp xe 1",
                "Cơ cấu kẹp xe 1 không thao tác đúng theo lệnh.",
                "Kiểm tra xilanh, cảm biến hành trình, van điện từ và áp suất khí."),
            CreateBit(
                "alarm.cart_2_clamp_cylinder_fault",
                PlcTagCatalog.Alarms.Cart2ClampCylinderFault,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi xilanh kẹp xe 2",
                "Cơ cấu kẹp xe 2 không thao tác đúng theo lệnh.",
                "Kiểm tra xilanh, cảm biến hành trình, van điện từ và áp suất khí."),
            CreateBit(
                "alarm.tool_gripper_no_release",
                PlcTagCatalog.Alarms.ToolGripperNoRelease,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Tay kẹp không nhả",
                "Tool gripper không mở ra theo lệnh điều khiển.",
                "Kiểm tra cơ cấu kẹp, van khí, áp suất khí và vật cản cơ khí."),
            CreateBit(
                "alarm.tool_gripper_no_clamp",
                PlcTagCatalog.Alarms.ToolGripperNoClamp,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Tay kẹp không kẹp",
                "Tool gripper không đóng kẹp theo lệnh điều khiển.",
                "Kiểm tra cơ cấu kẹp, van khí, áp suất khí và vật cản cơ khí."),
            CreateBit(
                "alarm.fail_place_line1",
                PlcTagCatalog.Alarms.FailPlaceLine1,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi thả sản phẩm lên Line 1",
                "Sản phẩm đặt trên Jig vị trí chưa chính xác.",
                "Kiểm tra lại vị trí sản phẩm trên Jig"),
            CreateBit(
                "alarm.fail_place_line2",
                PlcTagCatalog.Alarms.FailPlaceLine2,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi thả sản phẩm lên Line 2",
                "Sản phẩm đặt trên Jig vị trí chưa chính xác.",
                "Kiểm tra lại vị trí sản phẩm trên Jig"),
            CreateBit(
                "alarm.light_curtain",
                PlcTagCatalog.Alarms.LightCurtain,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Light curtain đang bị chặn",
                "Màn an toàn light curtain đang nhận vật cản trong khu vực thao tác.",
                "Loại bỏ vật cản, xác nhận khu vực an toàn và reset chuỗi an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.x_soft_limit_outside",
                PlcTagCatalog.Alarms.XSoftLimitOutside,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục X quá giới hạn phía ngoài",
                "Giá trị vị trí trục X vượt quá giới hạn mềm phía ngoài.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục X về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.x_soft_limit_inside",
                PlcTagCatalog.Alarms.XSoftLimitInside,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục X quá giới hạn phía trong",
                "Giá trị vị trí trục X vượt quá giới hạn mềm phía trong.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục X về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.y_soft_limit_left",
                PlcTagCatalog.Alarms.YSoftLimitLeft,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Y quá giới hạn trái",
                "Giá trị vị trí trục Y vượt quá giới hạn mềm bên trái.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục Y về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.y_soft_limit_right",
                PlcTagCatalog.Alarms.YSoftLimitRight,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Y quá giới hạn phải",
                "Giá trị vị trí trục Y vượt quá giới hạn mềm bên phải.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục Y về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.z_soft_limit_top",
                PlcTagCatalog.Alarms.ZSoftLimitTop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Z quá giới hạn trên",
                "Giá trị vị trí trục Z vượt quá giới hạn mềm phía trên.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục Z về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.z_soft_limit_bottom",
                PlcTagCatalog.Alarms.ZSoftLimitBottom,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Trục Z quá giới hạn dưới",
                "Giá trị vị trí trục Z vượt quá giới hạn mềm phía dưới.",
                "Kiểm tra tham số limit, tọa độ đặt và đưa trục Z về vùng an toàn.",
                stopMachine: true),
            CreateBit(
                "alarm.air_pressure_lost",
                PlcTagCatalog.Alarms.AirPressureLost,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Mất áp suất khí",
                "Nguồn khí nén không đạt mức tối thiểu để vận hành máy.",
                "Kiểm tra nguồn khí, bộ lọc điều áp, rò rỉ đường ống và reset lỗi sau khi ổn định.",
                stopMachine: true),
            CreateBit(
                "alarm.x_over_moment",
                PlcTagCatalog.Alarms.XOverMoment,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Quá momen trục X",
                "Driver phát hiện trục X vượt quá momen cho phép.",
                "Kiểm tra tải cơ khí trục X, kẹt hành trình, thông số servo và reset lỗi sau khi xử lý.",
                stopMachine: true),
            CreateBit(
                "alarm.y_over_moment",
                PlcTagCatalog.Alarms.YOverMoment,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Quá momen trục Y",
                "Driver phát hiện trục Y vượt quá momen cho phép.",
                "Kiểm tra tải cơ khí trục Y, kẹt hành trình, thông số servo và reset lỗi sau khi xử lý.",
                stopMachine: true),
            CreateBit(
                "alarm.z_over_moment",
                PlcTagCatalog.Alarms.ZOverMoment,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Quá momen trục Z",
                "Driver phát hiện trục Z vượt quá momen cho phép.",
                "Kiểm tra tải cơ khí trục Z, kẹt hành trình, thông số servo và reset lỗi sau khi xử lý.",
                stopMachine: true),
            CreateBit(
                "alarm.plc_line_1_disconnected",
                PlcTagCatalog.Alarms.PlcLine1Disconnected,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Mất kết nối PLC line 1",
                "Không giao tiếp được với PLC line 1.",
                "Kiểm tra nguồn, địa chỉ truyền thông và trạng thái kết nối PLC line 1."),
            CreateBit(
                "alarm.plc_line_2_disconnected",
                PlcTagCatalog.Alarms.PlcLine2Disconnected,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Mất kết nối PLC line 2",
                "Không giao tiếp được với PLC line 2.",
                "Kiểm tra nguồn, địa chỉ truyền thông và trạng thái kết nối PLC line 2."),
            CreateBit(
                "alarm.order_not_entered",
                PlcTagCatalog.Alarms.OrderNotEntered,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Chưa nhập order",
                "Máy chưa nhận thông tin order trước khi chạy auto.",
                "Nhập đầy đủ order cho line tương ứng trước khi bắt đầu chu trình."),
            CreateBit(
                "alarm.product_parameters_missing_line_1",
                PlcTagCatalog.Alarms.ProductParametersMissingLine1,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi thiếu thông số sản phẩm line 1",
                "Line 1 đang thiếu dữ liệu model hoặc thông số sản phẩm cần thiết để chạy auto.",
                "Kiểm tra lại model đang gọi, dữ liệu line 1 và thao tác load thông số sản phẩm xuống PLC."),
            CreateBit(
                "alarm.product_parameters_missing_line_2",
                PlcTagCatalog.Alarms.ProductParametersMissingLine2,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi thiếu thông số sản phẩm line 2",
                "Line 2 đang thiếu dữ liệu model hoặc thông số sản phẩm cần thiết để chạy auto.",
                "Kiểm tra lại model đang gọi, dữ liệu line 2 và thao tác load thông số sản phẩm xuống PLC."),
            CreateBit(
                "alarm.pc_disconnected",
                PlcTagCatalog.Alarms.PcDisconnected,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Mất kết nối PC",
                "Không giao tiếp được với PC.",
                "Kiểm tra lại dây mạng, cài đặt kết nối mạng tới PLC"),
        };

        ValidateDefinitions(definitions);
        return definitions;
    }

    private static void ValidateDefinitions(IEnumerable<AlarmDefinition> definitions)
    {
        var uniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            if (!uniqueKeys.Add(definition.Key))
            {
                throw new InvalidOperationException($"Alarm definition key '{definition.Key}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Title.");
            }
        }
    }

    private static AlarmDefinition CreateBit(
        string key,
        PlcTagDefinition tag,
        AlarmType alarmType,
        AlarmSeverity severity,
        string title,
        string? description,
        string? remedy,
        bool stopMachine = false)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = tag.Name,
            Address = tag.Address,
            SourceType = AlarmSourceType.Bit,
            AlarmType = alarmType,
            Severity = severity,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = stopMachine,
            ActiveWhenTrue = true,
        };
    }

    private static AlarmDefinition CreateCodeSlot(
        string key,
        PlcTagDefinition tag,
        AlarmType alarmType,
        AlarmSeverity severity,
        string title,
        string? description,
        string? remedy,
        bool stopMachine = false,
        bool suppress = false)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = tag.Name,
            Address = tag.Address,
            SourceType = AlarmSourceType.CodeWord,
            AlarmType = alarmType,
            Severity = severity,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = stopMachine,
            MatchAnyPositiveCode = true,
            Suppress = suppress,
        };
    }

    private static AlarmDefinition CreateSystem(
        string key,
        string title,
        string address,
        string? description,
        string? remedy)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = key,
            Address = address,
            SourceType = AlarmSourceType.System,
            AlarmType = AlarmType.System,
            Severity = AlarmSeverity.Critical,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = true,
        };
    }
}
