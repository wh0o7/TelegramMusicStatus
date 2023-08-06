using SpotifyAPI.Web;

namespace TelegramMusicStatus.Config;

public record MainConfig(
    Telegram TelegramAccount,
    Spotify SpotifyAccount,
    SpotifyApp SpotifyApp,
    Settings Settings
);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Spotify(string BearerToken, AuthorizationCodeTokenResponse? Response);

public record SpotifyApp(string ClientId, string ClientSecret);

public record Settings(bool IsDeployed, bool IsDefaultBioOnPause, int Interval);