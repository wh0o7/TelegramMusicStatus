using SpotifyAPI.Web;

namespace SpotifyBearerTokenGetter.Config;

public record MainConfig(
    SpotifyApp SpotifyApp,
    Spotify SpotifyAccount,
    Telegram TelegramAccount
);

public record SpotifyApp(string ClientId, string ClientSecret);

public record Spotify(string BearerToken, AuthorizationCodeTokenResponse Response);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);