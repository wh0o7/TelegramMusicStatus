using System.Text.RegularExpressions;
using ImprovedConsole;

namespace TelegramMusicStatus.Services;

public static class Utils
{
    public static string FormatTrackInfo(string input)
    {
        int maxCharacters = 70;
        var playingIndicator = "Now Playing: ";

        if (input.Length <= maxCharacters - playingIndicator.Length) return $"{playingIndicator}{input}";
        int maxTrackInfoLength = maxCharacters - playingIndicator.Length;
        var trackInfo = input[..maxTrackInfoLength].TrimEnd();
        return $"{playingIndicator}{trackInfo}";
    }
    public static bool IsValidTrackInfoFormat(string input)
    {
        const string pattern = "^Now Playing: .+ - .+$";
        return Regex.IsMatch(input, pattern);
    }

    public static void WriteLine(string info)
    {
        ConsoleWrapper.WriteLine(info, LogType.Info, true);
    }
}