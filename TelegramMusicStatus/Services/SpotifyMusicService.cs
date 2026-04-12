using SpotifyAPI.Web;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;

namespace TelegramMusicStatus.Services;

public interface ISpotifyMusicService : IMusicService;

public class SpotifyMusicService : ISpotifyMusicService
{
    private readonly IConfig<MainConfig> _config;
    private readonly SpotifyClient _spotifyClient;

    public SpotifyMusicService(IConfig<MainConfig> config)
    {
        this._config = config;
        var account = this._config.Entries.SpotifyAccount
                      ?? throw new InvalidOperationException("Spotify account configuration is missing");
        if (account.Response is not null)
        {
            var spotifyClientConfig = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(this._config.Entries.SpotifyApp.ClientId, this._config.Entries.SpotifyApp.ClientSecret,
                    account.Response));

            this._spotifyClient = new SpotifyClient(spotifyClientConfig);
        }
        else if (account.BearerToken is not null)
        {
            this._spotifyClient = new SpotifyClient(account.BearerToken);
        }
        else
        {
            throw new InvalidOperationException("Spotify account configuration is missing");
        }

        Utils.WriteLine("Spotify client started!");
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        var request = new PlayerCurrentlyPlayingRequest();
        CurrentlyPlaying? currentlyPlaying;
        try
        {
            currentlyPlaying = await this._spotifyClient.Player.GetCurrentlyPlaying(request);
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error getting Spotify status: {ex.Message}");
            currentlyPlaying = null;
        }

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