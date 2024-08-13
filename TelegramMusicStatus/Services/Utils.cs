using System.Text.RegularExpressions;
using ImprovedConsole;
using Swan;

namespace TelegramMusicStatus.Services;

public static class Utils
{
    private static int _maxCharacters = 70;
    private static string _playingIndicator = "Now Playing: ";
    public static string FormatTrackInfo(string input)
    {
        var status = $"{_playingIndicator}{input}";
        return input.Length <= _maxCharacters - _playingIndicator.Length ? status : FormatStatus(status);
    }
    public static bool IsValidTrackInfoFormat(string input)
    {
        const string pattern = "^Now Playing: .+ - .+$";
        return Regex.IsMatch(input.Replace(".", ""), pattern);
    }
    
    public static string FormatStatus(string input) => input.Length <= _maxCharacters ? input :$"{input[..(_maxCharacters - 3)]}...".TrimEnd();

    public static void WriteLine(string info)
    {
        ConsoleWrapper.WriteLine(info, LogType.Info, true);
    }
}