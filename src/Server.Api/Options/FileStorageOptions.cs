namespace Server.Api.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public const string DefaultRootPath = "Storage/Uploads";

    public string RootPath { get; set; } = DefaultRootPath;
    public string? RemotePath { get; set; }
}
