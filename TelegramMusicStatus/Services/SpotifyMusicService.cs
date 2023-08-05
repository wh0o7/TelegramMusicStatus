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
    private SpotifyClient _spotifyClient;

    public SpotifyMusicService(IConfig<MainConfig> config)
    {
        this._config = config;
        if (this._config.Entries.SpotifyAccount.Response is not null)
        {
            var spotifyClientConfig = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(this._config.Entries.SpotifyApp.ClientId,
                    this._config.Entries.SpotifyApp.ClientSecret, this._config.Entries.SpotifyAccount.Response));

            this._spotifyClient = new SpotifyClient(spotifyClientConfig);
        }
        else
        {
            this._spotifyClient = new SpotifyClient(this._config.Entries.SpotifyAccount.BearerToken);
        }
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