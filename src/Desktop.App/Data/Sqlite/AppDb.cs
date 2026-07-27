using Microsoft.Data.Sqlite;
using System.IO;

namespace Desktop.App.Data.Sqlite;

public static class AppDb
{
    public const string FileName = "gear-lathe-feeder.desktop-app.db";

    public static string GetDatabasePath(string? overridePath = null)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return Path.GetFullPath(overridePath);
        }

        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GearLatheFeeder");

        return Path.Combine(appDataDirectory, FileName);
    }

    public static string GetConnectionString(string? overridePath = null)
    {
        var databasePath = GetDatabasePath(overridePath);
        var directoryPath = Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        }.ToString();
    }
}
