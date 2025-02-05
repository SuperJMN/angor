using System.Text.Json;
using CSharpFunctionalExtensions;

namespace RefinedSuppaWallet.Infrastructure.Angor.Store;

public class FileStore : IStore
{
    private readonly string appDataPath;

    public FileStore(string appName)
    {
        appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName
        );

        Directory.CreateDirectory(appDataPath);
    }

    public async Task<Result> Save<T>(string key, T data)
    {
        return from filePath in Result.Try(() => Path.Combine(appDataPath, key))
            from contents in Result.Try(() => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }))
            select Result.Try(() => File.WriteAllTextAsync(filePath, contents))
                .Bind(Result.Success);
    }

    public async Task<Result<T>> Load<T>(string key)
    {
        return await Result.Try(() => Path.Combine(appDataPath, key))
            .Ensure(File.Exists, $"File not found for key: {key}")
            .Bind(async filePath =>
            {
                var jsonString = await File.ReadAllTextAsync(filePath);
                return Result.Try(() =>
                    JsonSerializer.Deserialize<T>(jsonString)
                    ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}"));
            });
    }
}