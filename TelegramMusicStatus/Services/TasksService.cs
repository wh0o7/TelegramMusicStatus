using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> LastFmTask();
    Task<bool> YandexMusicTask();
}

public class TasksService : ITasksService
{
    private readonly ITelegramStatusService _telegramService;
    private readonly ISpotifyMusicService? _spotifyService;
    private readonly ILastFmService? _lastFmService;
    private readonly IYandexMusicService? _yandexMusicService;
    private readonly string? _playingIndicator;

    public TasksService(ITelegramStatusService telegramService, IConfig<MainConfig> config, IYandexMusicService? yandexMusicService = null, ILastFmService? lastFmService = null,
        ISpotifyMusicService? spotifyService = null)
    {
        this._telegramService = telegramService;
        this._yandexMusicService = yandexMusicService;
        this._playingIndicator = config.Entries.PlayingIndicator;
        this._lastFmService = lastFmService;
        this._spotifyService = spotifyService;
    }

    public async Task<bool> SpotifyTask()
    {
        if (this._spotifyService is null) return false;
        var status = await this._spotifyService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("(Spotify)   Player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(Spotify)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await this._telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, this._playingIndicator));
        return true;
    }

    public async Task<bool> LastFmTask()
    {
        if (this._lastFmService is null) return false;
        var status = await this._lastFmService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("(Last Fm)   Player paused.");
            return false;
        }

        Utils.WriteLine($"(LastFm)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await this._telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, this._playingIndicator));
        return true;
    }

    public async Task<bool> YandexMusicTask()
    {
        if (this._yandexMusicService is null) return false;
        var status = await this._yandexMusicService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("(Yandex Music)   Player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(YandexMusic)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await this._telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio, this._playingIndicator));
        return true;
    }
}
