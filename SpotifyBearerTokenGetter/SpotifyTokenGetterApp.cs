using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyBearerTokenGetter.Config;

namespace SpotifyBearerTokenGetter;

internal sealed class SpotifyTokenGetterApp
{
    private string? _clientId;
    private string? _clientSecret;
    private Config<MainConfig>? _config;
    private EmbedIOAuthServer? _server;
    private int _port;
    private Uri _uri = null!;

    internal async Task RunAsync()
    {
        var path = string.Empty;
        if (!File.Exists(Config<MainConfig>.FilePath))
        {
            Console.WriteLine(
                "If you have config with ClientId and ClientSecret, write path to config. Or just write N: ");
            path = Console.ReadLine() ?? string.Empty;
            if (path.ToUpperInvariant() is "N" || !File.Exists(path))
            {
                Console.WriteLine("Input Client Id of your Spotify app: ");
                this._clientId = Console.ReadLine();
                Console.WriteLine("Input Client Secret of your Spotify app: ");
                this._clientSecret = Console.ReadLine();
            }
            else
            {
                Config<MainConfig>.FilePath = path;
                await this.LoadConfigFromFileAsync();
            }
        }
        else
        {
            await this.LoadConfigFromFileAsync();
        }

        if (string.IsNullOrEmpty(this._clientId) || string.IsNullOrEmpty(this._clientSecret))
        {
            Console.WriteLine("Client Id and Client Secret are required. Exiting.");
            return;
        }

        this._port = 5543;
        this._uri = new Uri($"http://localhost:{this._port}/callback");
        this._server = new EmbedIOAuthServer(this._uri, this._port);
        await this._server.Start();

        this._server.AuthorizationCodeReceived += this.OnAuthorizationCodeReceived;
        this._server.ErrorReceived += this.OnErrorReceived;

        var request = new LoginRequest(this._server.BaseUri, this._clientId,
            LoginRequest.ResponseType.Code)
        {
            Scope = new[] { Scopes.UserReadCurrentlyPlaying }
        };
        BrowserUtil.Open(request.ToUri());
        await Task.Delay(-1);
    }

    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        if (this._server is null) return;
        await this._server.Stop();

        var spotifyConfig = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(spotifyConfig).RequestToken(
            new AuthorizationCodeTokenRequest(
                this._clientId!, this._clientSecret!, response.Code,
                this._uri
            )
        );
        Console.WriteLine($"Success! Your Bearer token is: {tokenResponse.AccessToken}");
        var configService = File.Exists(Config<MainConfig>.FilePath) ? new Config<MainConfig>() : null;
        var mainConfig = configService is not null
            ? (configService.Entries ?? new MainConfig(null, null, null, null, null))
            : new MainConfig(null, null, null, null, null);
        var updatedSpotify = new Spotify(tokenResponse.AccessToken, tokenResponse);
        var updatedMainConfig = mainConfig with
        {
            SpotifyAccount = updatedSpotify,
            SpotifyApp = mainConfig.SpotifyApp ?? new SpotifyApp(this._clientId!, this._clientSecret!)
        };

        Config<MainConfig>.SaveConfig(updatedMainConfig);
        Console.WriteLine("Config successfully saved.");
        Console.Read();
        Environment.Exit(0);
    }

    private async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        if (this._server is not null) await this._server.Stop();
    }

    private Task LoadConfigFromFileAsync()
    {
        var cfg = new Config<MainConfig>();
        this._config = cfg;
        var entries = cfg.Entries;
        if (entries is null) return Task.CompletedTask;

        var spotifyApp = entries.SpotifyApp;
        if (spotifyApp is null || spotifyApp.ClientId is null || spotifyApp.ClientSecret is null)
            return Task.CompletedTask;
        this._clientSecret = spotifyApp.ClientSecret;
        this._clientId = spotifyApp.ClientId;

        return Task.CompletedTask;
    }
}
