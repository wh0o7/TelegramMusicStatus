namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> AIMPTask();
}

public class TasksService : ITasksService
{
    private ITelegramStatusService _telegramService;
    private ISpotifyMusicService _spotifyService;
    private IAIMPMusicService _aimpMusicService;

    public TasksService(ITelegramStatusService telegramService, IAIMPMusicService aimpMusicService = null,
        ISpotifyMusicService spotifyService = null)
    {
        this._telegramService = telegramService;
        this._spotifyService = spotifyService;
        this._aimpMusicService = aimpMusicService;
    }

    public async Task<bool> SpotifyTask()
    {
        if (this._spotifyService is null) return false;
        var status = await this._spotifyService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("Spotify web player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(Spotify)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (status.IsPlaying)
        {
            await this._telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
            return true;
        }

        return false;
    }

    public async Task<bool> AIMPTask()
    {
        if (this._aimpMusicService is null) return false;
        var status = await this._aimpMusicService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("AIMP player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(AIMP)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (status.IsPlaying)
        {
            await this._telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
            return true;
        }

        return false;
    }
}