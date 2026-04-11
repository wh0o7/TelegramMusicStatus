namespace TelegramMusicStatus.Models;

public class TrackInfoMessage
{
    public TrackInfoMessage(string trackTitle, string artist, bool isPlaying)
    {
        this.TrackTitle = trackTitle;
        this.Artist = artist;
        this.IsPlaying = isPlaying;
    }

    public string TrackTitle { get; set; }
    public string Artist { get; set; }
    public bool IsPlaying { get; set; }

    public void Deconstruct(out string trackTitle, out string artist, out bool isPlaying)
    {
        trackTitle = this.TrackTitle;
        artist = this.Artist;
        isPlaying = this.IsPlaying;
    }
}