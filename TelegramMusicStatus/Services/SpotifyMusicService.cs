using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Swan;
using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ISpotifyMusicService
{
    Task<(bool IsPlaying, string bio)> GetCurrentlyPlayingStatus();
}

public class SpotifyMusicService : ISpotifyMusicService
{
    private IConfig<MainConfig> _config;
    private ITelegramStatusService _telegramStatusService;
    private SpotifyClient _spotifyClient;

    public SpotifyMusicService(IConfig<MainConfig> config, ITelegramStatusService telegramStatusService)
    {
        this._config = config;
        this._telegramStatusService = telegramStatusService;
        this._spotifyClient = new SpotifyClient(this._config.Entries.SpotifyAccount.BearerToken);
    }

    public async Task<(bool IsPlaying, string bio)> GetCurrentlyPlayingStatus()
    {
        var request = new PlayerCurrentlyPlayingRequest();
        var currentlyPlaying = await _spotifyClient.Player.GetCurrentlyPlaying(request);
        var bio = currentlyPlaying.Item switch
        {
            FullTrack fullTrack => $"{fullTrack.Name} - {string.Join(", ", fullTrack.Artists.Select(a => a.Name))}",
            FullEpisode fullEpisode => $"{fullEpisode.Name} - {fullEpisode.Show.Name}",
            _ => string.Empty
        };

        return (currentlyPlaying.IsPlaying, bio);
    }
}