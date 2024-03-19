namespace TelegramMusicStatus.Models;

public class TrackInfoMessage
{
    public TrackInfoMessage(string trackTitle, string artist, bool isPlaying)
    {
        TrackTitle = trackTitle;
        Artist = artist;
        IsPlaying = isPlaying;
    }

    public string TrackTitle { get; set; }
    public string Artist { get; set; }
    public bool IsPlaying { get; set; }

    public void Deconstruct(out string trackTitle, out string artist, out bool isPlaying)
    {
        trackTitle = TrackTitle;
        artist = Artist;
        isPlaying = IsPlaying;
    }
}