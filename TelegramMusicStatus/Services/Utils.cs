using System.Text.RegularExpressions;
using ImprovedConsole;
using Swan;

namespace TelegramMusicStatus.Services;

public static class Utils
{
    private const int MaxCharacters = 70;
    private const string PlayingIndicator = "Now Playing:";
    public static string FormatTrackInfo(string input, string? playingIndicator = null)
    {
        var status = $"{playingIndicator ?? PlayingIndicator} {input}";
        return input.Length <= MaxCharacters - PlayingIndicator.Length ? status : FormatStatus(status);
    }
    public static bool IsValidTrackInfoFormat(string input, string? playingIndicator = null)
    {
         var pattern = $"^{playingIndicator ?? PlayingIndicator} .+ - .+$";
        return Regex.IsMatch(input.Replace(".", ""), pattern);
    }
    
    public static string FormatStatus(string input) => input.Length <= MaxCharacters ? input :$"{input[..(MaxCharacters - 3)]}...".TrimEnd();

    public static void WriteLine(string info)
    {
        ConsoleWrapper.WriteLine(info, LogType.Info, true);
    }
}