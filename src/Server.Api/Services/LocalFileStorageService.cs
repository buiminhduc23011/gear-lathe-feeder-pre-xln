using System.Text;
using Server.Api.Options;

namespace Server.Api.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRootAbsolutePath;
    private readonly string _storageRootRelativePath;
    private readonly string? _remoteRootAbsolutePath;

    public LocalFileStorageService(string contentRootPath, FileStorageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);
        ArgumentNullException.ThrowIfNull(options);

        var configuredRootPath = string.IsNullOrWhiteSpace(options.RootPath)
            ? FileStorageOptions.DefaultRootPath
            : options.RootPath.Trim();

        _storageRootRelativePath = configuredRootPath
            .Replace('\\', '/')
            .Trim('/');

        _storageRootAbsolutePath = ResolveAbsolutePath(contentRootPath, configuredRootPath);

        if (!string.IsNullOrWhiteSpace(options.RemotePath))
        {
            var remoteRootAbsolutePath = ResolveAbsolutePath(contentRootPath, options.RemotePath.Trim());
            if (!string.Equals(remoteRootAbsolutePath, _storageRootAbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                _remoteRootAbsolutePath = remoteRootAbsolutePath;
            }
        }
    }

    public async Task<StoredFileResult> SaveAsync(
        Stream content,
        string originalFileName,
        long size,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var displayOriginalFileName = Path.GetFileName(originalFileName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(displayOriginalFileName))
        {
            displayOriginalFileName = "file";
        }

        var safeOriginalFileName = SanitizeFileName(displayOriginalFileName);
        var storedFileName = BuildStoredFileName(safeOriginalFileName);

        Directory.CreateDirectory(_storageRootAbsolutePath);
        if (_remoteRootAbsolutePath is not null)
        {
            Directory.CreateDirectory(_remoteRootAbsolutePath);
        }

        var targetPaths = GetUniqueFilePaths(storedFileName);
        try
        {
            await using (var destination = File.Create(targetPaths.LocalPath))
            {
                await content.CopyToAsync(destination, cancellationToken);
            }

            if (targetPaths.RemotePath is not null)
            {
                await CopyFileAsync(targetPaths.LocalPath, targetPaths.RemotePath, cancellationToken);
            }
        }
        catch
        {
            TryDeleteFile(targetPaths.LocalPath);
            if (targetPaths.RemotePath is not null)
            {
                TryDeleteFile(targetPaths.RemotePath);
            }

            throw;
        }

        var actualStoredFileName = Path.GetFileName(targetPaths.LocalPath);
        var relativeStoragePath = $"{_storageRootRelativePath}/{actualStoredFileName}";

        return new StoredFileResult(
            OriginalFileName: displayOriginalFileName,
            StoredFileName: actualStoredFileName,
            Size: size,
            SavedAtUtc: DateTimeOffset.UtcNow,
            StoragePath: relativeStoragePath);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        var normalizedStoragePath = storagePath
            .Replace('\\', '/')
            .Trim('/');

        var isInsideConfiguredRoot =
            string.Equals(normalizedStoragePath, _storageRootRelativePath, StringComparison.OrdinalIgnoreCase) ||
            normalizedStoragePath.StartsWith($"{_storageRootRelativePath}/", StringComparison.OrdinalIgnoreCase);

        if (!isInsideConfiguredRoot)
        {
            return Task.FromResult<Stream?>(null);
        }

        var relativePath = normalizedStoragePath[_storageRootRelativePath.Length..].TrimStart('/');
        var absolutePath = Path.GetFullPath(Path.Combine(_storageRootAbsolutePath, relativePath));

        if (!absolutePath.StartsWith(_storageRootAbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<Stream?>(null);
        }

        if (!File.Exists(absolutePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(absolutePath);
        return Task.FromResult<Stream?>(stream);
    }

    private StorageTargetPaths GetUniqueFilePaths(string storedFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(storedFileName);
        var extension = Path.GetExtension(storedFileName);
        var counter = 1;
        var candidateFileName = storedFileName;

        while (true)
        {
            var localPath = Path.Combine(_storageRootAbsolutePath, candidateFileName);
            var remotePath = _remoteRootAbsolutePath is null
                ? null
                : Path.Combine(_remoteRootAbsolutePath, candidateFileName);

            var existsLocally = File.Exists(localPath);
            var existsRemotely = remotePath is not null && File.Exists(remotePath);

            if (!existsLocally && !existsRemotely)
            {
                return new StorageTargetPaths(localPath, remotePath);
            }

            candidateFileName = $"{baseName}_{counter}{extension}";
            counter++;
        }
    }

    private static string ResolveAbsolutePath(string contentRootPath, string configuredPath)
    {
        return Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));
    }

    private static async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var source = File.OpenRead(sourcePath);
        await using var destination = File.Create(destinationPath);
        await source.CopyToAsync(destination, cancellationToken);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Preserve the original upload exception.
        }
    }

    private static string BuildStoredFileName(string originalFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");

        return string.IsNullOrWhiteSpace(extension)
            ? $"{baseName}_{timestamp}"
            : $"{baseName}_{timestamp}{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var trimmedFileName = Path.GetFileName(fileName ?? string.Empty);
        var baseName = Path.GetFileNameWithoutExtension(trimmedFileName);
        var extension = Path.GetExtension(trimmedFileName);

        var safeBaseName = SanitizeSegment(baseName, fallbackValue: "file");
        var safeExtension = SanitizeExtension(extension);

        return $"{safeBaseName}{safeExtension}";
    }

    private static string SanitizeSegment(string value, string fallbackValue)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            if (Path.GetInvalidFileNameChars().Contains(character) || char.IsControl(character))
            {
                builder.Append('_');
                continue;
            }

            builder.Append(char.IsWhiteSpace(character) ? '_' : character);
        }

        var sanitized = builder
            .ToString()
            .Trim('.', '_');

        return string.IsNullOrWhiteSpace(sanitized)
            ? fallbackValue
            : sanitized;
    }

    private static string SanitizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(extension.Length);

        foreach (var character in extension)
        {
            if (character == '.')
            {
                builder.Append(character);
                continue;
            }

            if (Path.GetInvalidFileNameChars().Contains(character) || char.IsControl(character))
            {
                continue;
            }

            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.Length == 1 && builder[0] == '.'
            ? string.Empty
            : builder.ToString();
    }

    private sealed record StorageTargetPaths(string LocalPath, string? RemotePath);
}
