using System.Net;
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
        var path = string.Empty;
        if (!File.Exists(Config<MainConfig>.FilePath)) //checking default path
        {
            Console.WriteLine(
                "If you have config with ClientId and ClientSecret, write path to config. Or just write N: ");
            path = Console.ReadLine();
            if (path.ToUpperInvariant() is "N" || !File.Exists(path))
            {
                Console.WriteLine("Input Client Id of your Spotify app: ");
                ClientId = Console.ReadLine();
                Console.WriteLine("Input Client Secret of your Spotify app: ");
                ClientSecret = Console.ReadLine();
            }
            else
            {
                Config<MainConfig>.FilePath = path;
                await GetConfigData();
            }
        }
        else
        {
            await GetConfigData();
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
        var configService = File.Exists(Config<MainConfig>.FilePath) ? new Config<MainConfig>() : null;
        var mainConfig = configService is not null
            ? configService.Entries
            : new MainConfig(new SpotifyApp(ClientId, ClientSecret), null, null, null, null);
        var updatedSpotify = new Spotify(BearerToken: tokenResponse.AccessToken, Response: tokenResponse);
        var updatedMainConfig = mainConfig with { SpotifyAccount = updatedSpotify };

        Config<MainConfig>.SaveConfig(updatedMainConfig);
        Console.WriteLine("Config successfully saved.");
        Console.Read();
        Environment.Exit(0);
    }

    private static async Task OnErrorReceived(object sender, string error, string? state)
    {
        Console.WriteLine($"Aborting authorization, error received: {error}");
        await _server.Stop();
    }

    private static async Task GetConfigData()
    {
        _config = new Config<MainConfig>();
        if (_config.Entries.SpotifyApp.ClientId is not null &&
            _config.Entries.SpotifyApp.ClientSecret is not null)
        {
            ClientSecret = _config.Entries.SpotifyApp.ClientSecret;
            ClientId = _config.Entries.SpotifyApp.ClientId;
        }
    }
}