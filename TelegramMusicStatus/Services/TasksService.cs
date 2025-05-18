using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> AimpTask();
    Task<bool> LastFmTask();
}

public class TasksService : ITasksService
{
    private readonly ITelegramStatusService _telegramService;
    private readonly IAIMPMusicService? _aimpService;
    private readonly ISpotifyMusicService? _spotifyService;
    private readonly ILastFmService? _lastFmService;
    private readonly string? _playingIndicator;

    public TasksService(ITelegramStatusService telegramService, IConfig<MainConfig> config, ILastFmService? lastFmService = null, IAIMPMusicService? aimpService = null, ISpotifyMusicService? spotifyService = null)
    {
        _telegramService = telegramService;
        _playingIndicator = config.Entries.PlayingIndicator;
        _lastFmService = lastFmService;
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
        await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, _playingIndicator));
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
        await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, _playingIndicator));
        return true;
    }

    public async Task<bool> LastFmTask()
    {
        if (_lastFmService is null) return false;
        var status = await _lastFmService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("AIMP player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(LastFm)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, _playingIndicator));
        return true;
    }
}