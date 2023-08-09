namespace TelegramMusicStatus.Models;

public class TrackInfoMessage
{
    public string TrackTitle { get; set; }
    public string Artist { get; set; }
    public bool IsPlaying { get; set; }
}