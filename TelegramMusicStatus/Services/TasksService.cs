namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> AimpTask();
}

public class TasksService : ITasksService
{
    private readonly ITelegramStatusService _telegramService;
    private readonly IAIMPMusicService? _aimpService;
    private readonly ISpotifyMusicService? _spotifyService;

    public TasksService(ITelegramStatusService telegramService,
        IAIMPMusicService? aimpService = null,
        ISpotifyMusicService? spotifyService = null)
    {
        _telegramService = telegramService;
        _aimpService = aimpService;
        _spotifyService = spotifyService;
    }

    public async Task<bool> SpotifyTask()
    {
        if (_spotifyService is null) return false;
        var status = await _spotifyService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("Spotify web player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(Spotify)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        return true;

    }

    public async Task<bool> AimpTask()
    {
        if (_aimpService is null) return false;
        var status = await _aimpService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("AIMP player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(AIMP)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        return true;
    }
}