using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyBearerTokenGetter.Config;

namespace SpotifyBearerTokenGetter;

internal class Program
{
    private static string ClientId;
    private static Config<MainConfig> _config;
    private static string ClientSecret;
    private static EmbedIOAuthServer? _server;
    private static int _port;
    private static Uri _uri;

    public static async Task Main()
    {
        if (!File.Exists("./config.json"))
        {
            Console.WriteLine("Input Client Id of your Spotify app: ");
            ClientId = Console.ReadLine();
            Console.WriteLine("Input Client Secret of your Spotify app: ");
            ClientSecret = Console.ReadLine();
        }
        else
        {
            _config = new Config<MainConfig>();
            if (_config.Entries.SpotifyApp.ClientId is not null &&
                _config.Entries.SpotifyApp.ClientSecret is not null)
            {
                ClientSecret = _config.Entries.SpotifyApp.ClientSecret;
                ClientId = _config.Entries.SpotifyApp.ClientId;
            }
        }

        _port = 5543;
        _uri = new Uri($"http://localhost:{_port}/callback");
        _server = new EmbedIOAuthServer(_uri, _port);
        await _server.Start();

        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        _server.ErrorReceived += OnErrorReceived;

        var request = new LoginRequest(_server.BaseUri, ClientId,
            LoginRequest.ResponseType.Code)
        {
            Scope = new[] { Scopes.UserReadCurrentlyPlaying }
        };
        BrowserUtil.Open(request.ToUri());
        await Task.Delay(-1);
    }

    private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        await _server.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(
                ClientId, ClientSecret, response.Code,
                _uri
            )
        );
        Console.WriteLine($"Success! Your Bearer token is: {tokenResponse.AccessToken}");
        var configService = new Config<MainConfig>();
        var mainConfig = configService.Entries; // Get the existing config
        var updatedSpotify = mainConfig.SpotifyAccount with
        {
            BearerToken = tokenResponse.AccessToken, Response = tokenResponse
        };
        var updatedMainConfig = mainConfig with { SpotifyAccount = updatedSpotify };

        configService.SaveConfig(updatedMainConfig);

        Console.Read();
        Environment.Exit(0);
    }

    private static async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await _server.Stop();
    }
}