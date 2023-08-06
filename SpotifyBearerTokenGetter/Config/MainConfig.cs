using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace SpotifyBearerTokenGetter.Config;

public record MainConfig(
    SpotifyApp SpotifyApp,
    Spotify SpotifyAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Telegram TelegramAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Settings Settings
);

public record SpotifyApp(string ClientId, string ClientSecret);

public record Spotify(string BearerToken, AuthorizationCodeTokenResponse Response);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Settings(bool IsDeployed, bool IsDefaultBioOnPause, int Interval);