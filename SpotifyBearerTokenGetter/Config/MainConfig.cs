using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace SpotifyBearerTokenGetter.Config;

public record MainConfig(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] SpotifyApp? SpotifyApp,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Spotify? SpotifyAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Telegram? TelegramAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Settings? Settings,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string?[]? UserBio);

public record SpotifyApp(string ClientId, string ClientSecret);
public record Spotify(string BearerToken, AuthorizationCodeTokenResponse Response);
public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);
public record Settings(bool IsDeployed, bool IsDefaultBioOnPause, int Interval);