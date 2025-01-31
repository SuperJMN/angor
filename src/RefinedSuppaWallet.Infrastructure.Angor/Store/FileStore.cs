using System.Text.Json;

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

    public async Task Save<T>(string key, T data)
    {
        var filePath = Path.Combine(appDataPath, key);
        var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(filePath, jsonString);
    }

    public async Task<T?> Load<T>(string key)
    {
        var filePath = Path.Combine(appDataPath, key);
        
        if (!File.Exists(filePath))
            return default;

        var jsonString = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}