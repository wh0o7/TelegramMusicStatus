using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> AIMPTask();
}

public class TasksService : ITasksService
{
    private ITelegramStatusService _telegramService;
    private IConfig<MainConfig> _config;
    private ISpotifyMusicService _spotifyService;
    private IAIMPMusicService _aimpMusicService;

    public TasksService(ITelegramStatusService telegramService, IConfig<MainConfig> config,
        IAIMPMusicService aimpMusicService = null, ISpotifyMusicService spotifyService = null)
    {
        this._telegramService = telegramService;
        this._spotifyService = spotifyService;
        this._config = config;
        this._aimpMusicService = aimpMusicService;
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

        switch (status.IsPlaying)
        {
            case true:
                await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
                return true;
            case false when _config.Entries.Settings is { IsDefaultBioOnPause: true }:
                await _telegramService.SetUserDefaultBio();
                break;
        }

        return false;
    }

    public async Task<bool> AIMPTask()
    {
        if (_aimpMusicService is null) return false;
        var status = await _aimpMusicService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("AIMP player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(AIMP)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        switch (status.IsPlaying)
        {
            case true:
                await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
                return true;
            case false when _config.Entries.Settings is { IsDefaultBioOnPause: true }:
                await _telegramService.SetUserDefaultBio();
                break;
        }

        return false;
    }
}