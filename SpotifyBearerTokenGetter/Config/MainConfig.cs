using SpotifyAPI.Web;

namespace SpotifyBearerTokenGetter.Config;

public record MainConfig(
    SpotifyApp SpotifyApp,
    Spotify SpotifyAccount
);

public record SpotifyApp(string ClientId, string ClientSecret);

public record Spotify(string BearerToken, AuthorizationCodeTokenResponse Response);