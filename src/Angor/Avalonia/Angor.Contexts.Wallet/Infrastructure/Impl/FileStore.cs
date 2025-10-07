using System;
using System.IO;
using System.Text.Json;
using Angor.Contests.CrossCutting;
using CSharpFunctionalExtensions;

namespace Angor.Contexts.Wallet.Infrastructure.Impl;

public class FileStore : IStore
{
    private readonly string appDataPath;
    private readonly string instancePrefix;

    public FileStore(string appName, string instancePrefix)
    {
        this.instancePrefix = instancePrefix;
        appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName
        );

        Directory.CreateDirectory(appDataPath);
    }

    public async Task<Result> Save<T>(string key, T data)
    {
        return from filePath in Result.Try(() => BuildPath(key))
            from contents in Result.Try(() => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }))
            select Result.Try(() => File.WriteAllTextAsync(filePath, contents))
                .Bind(Result.Success);
    }

    public Task<Result<T>> Load<T>(string key)
    {
        return Result.Try(() => BuildPath(key))
            .TapTry(CreateFile)
            .MapTry(s => File.ReadAllTextAsync(s))
            .Bind(json => Result.Try(() => JsonSerializer.Deserialize<T>(json))
                .Ensure(x => x != null, $"Could not deserialize {json} as {typeof(T)}")
                .Map(x => x!)
            );
    }

    private string BuildPath(string key)
    {
        var folder = RequiresInstancePrefix(key)
            ? Path.Combine(appDataPath, instancePrefix)
            : appDataPath;

        Directory.CreateDirectory(folder);

        return Path.Combine(folder, key);
    }

    private static bool RequiresInstancePrefix(string key)
    {
        return string.Equals(key, "wallets.json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "settings.json", StringComparison.OrdinalIgnoreCase);
    }

    private static void CreateFile(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        using (File.Create(path)) { }
    }
}