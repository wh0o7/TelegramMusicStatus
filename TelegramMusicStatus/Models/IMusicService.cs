namespace TelegramMusicStatus.Models;

public interface IMusicService
{
    Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
}