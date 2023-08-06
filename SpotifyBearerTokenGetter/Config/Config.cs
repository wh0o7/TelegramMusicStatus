using System.Text.Json;

namespace SpotifyBearerTokenGetter.Config;

public interface IConfig<out T>
{
    T? Entries { get; }
}

public class Config<T> : IConfig<T>
{
    public static string FilePath { get; set; } = "./config.json";
    
    private static T? ReadConfig()
    {
        var file = new FileInfo(FilePath);
        if (!file.Exists)
            throw new FileNotFoundException(
                "Config file not found, please create config.json file in the root directory!");

        var fileData = File.ReadAllText(file.FullName);
        var data = JsonSerializer.Deserialize<T>(fileData);
        if (data is null) return default;
        return data;
    }

    public T? Entries { get; set; } = ReadConfig();

    public static void SaveConfig(T data)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var jsonData = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(FilePath, jsonData);
    }
}