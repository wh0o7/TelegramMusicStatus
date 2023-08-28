using System.Text.Json;

namespace TelegramMusicStatus.Config;

public interface IConfig<out T>
{
    T Entries { get; }
}

public class Config<T> : IConfig<T>
{
    public static string FilePath { get; set; } = "./config.json";

    private static T ReadConfig()
    {
        var file = new FileInfo(FilePath);
        if (!file.Exists)
            throw new FileNotFoundException(
                "Config file not found, please create config.json file in the root directory!");

        var fileData = File.ReadAllText(file.FullName);
        var data = JsonSerializer.Deserialize<T>(fileData);
        if (data is null) throw new FileLoadException("Can't load " + typeof(T).Name + " config");
        return data;
    }

    public static void SaveConfig(T data)
    {
        var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(FilePath, jsonData);
    }

    public T Entries { get; } = ReadConfig();
}