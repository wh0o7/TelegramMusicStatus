using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using Swan;
using TelegramMusicStatus.Config;

namespace TelegramMusicStatus.Services;

public interface ISpotifyMusicService
{
    Task<string> GetCurrentlyPlayingStatus();
}

public class SpotifyMusicService : ISpotifyMusicService
{
    private IConfig<MainConfig> _config;
    private EmbedIOAuthServer? _server;
    private ITelegramStatusService _telegramStatusService;
    private int _port;
    private Uri _uri;
    private SpotifyClient _spotifyClient;

    public SpotifyMusicService(IConfig<MainConfig> config, ITelegramStatusService telegramStatusService)
    {
        this._config = config;
        this._telegramStatusService = telegramStatusService;
        this._port = 5543;
        this._uri = new Uri($"http://localhost:{_port}/callback");
        Init().Wait();
    }

    public async Task<string> GetCurrentlyPlayingStatus()
    {
        var request = new PlayerCurrentlyPlayingRequest();
        var track = await _spotifyClient.Player.GetCurrentlyPlaying(request);
        return track.Item switch
        {
            FullTrack fullTrack => $"{fullTrack.Name} - {string.Join(", ",fullTrack.Artists.Select(a => a.Name))}",
            FullEpisode fullEpisode => $"{fullEpisode.Name}",
            _ => string.Empty
        };
    }

    public async Task Init()
    {
        this._server = new EmbedIOAuthServer(this._uri, this._port);
        await this._server.Start();

        this._server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        this._server.ErrorReceived += OnErrorReceived;

        var request = new LoginRequest(this._server.BaseUri, _config.Entries.SpotifyApp.ClientId,
            LoginRequest.ResponseType.Code)
        {
            Scope = new[] { Scopes.UserReadCurrentlyPlaying }
        };
        BrowserUtil.Open(request.ToUri());
    }

    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        await this._server.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(
                this._config.Entries.SpotifyApp.ClientId, this._config.Entries.SpotifyApp.ClientSecret, response.Code,
                this._uri
            )
        );

        this._spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
        var status = await GetCurrentlyPlayingStatus();
        await this._telegramStatusService.ChangeUserBio(status);
    }

    private async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await this._server.Stop();
    }
}