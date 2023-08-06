using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Swan;
using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ISpotifyMusicService
{
    Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
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

        Task.Run(async () =>
        {
            if (await this._spotifyClient.Player.GetCurrentlyPlaying(
                    new PlayerCurrentlyPlayingRequest()) is null)
                throw new APIException("Your spotify has no currently playing songs. Turn on music and restart app.");
        });
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        var request = new PlayerCurrentlyPlayingRequest();
        var currentlyPlaying = await this._spotifyClient.Player.GetCurrentlyPlaying(request);
        if (currentlyPlaying is null) return (false, null);
        var bio = currentlyPlaying.Item switch
        {
            FullTrack fullTrack => $"{fullTrack.Name} - {string.Join(", ", fullTrack.Artists.Select(a => a.Name))}",
            FullEpisode fullEpisode => $"{fullEpisode.Name} - {fullEpisode.Show.Name}",
            _ => string.Empty
        };

        return (currentlyPlaying.IsPlaying, bio);
    }
}