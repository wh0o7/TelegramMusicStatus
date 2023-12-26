namespace TelegramMusicStatus.Models;

public class TrackInfoMessage(string trackTitle, string artist, bool isPlaying)
{
    public string TrackTitle { get; set; } = trackTitle;
    public string Artist { get; set; } = artist;
    public bool IsPlaying { get; set; } = isPlaying;

    public void Deconstruct(out string trackTitle, out string artist, out bool isPlaying)
    {
        trackTitle = TrackTitle;
        artist = Artist;
        isPlaying = IsPlaying;
    }
}