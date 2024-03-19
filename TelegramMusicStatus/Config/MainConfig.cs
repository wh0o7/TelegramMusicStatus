using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace TelegramMusicStatus.Config;

public record MainConfig(
    SpotifyApp SpotifyApp,
    Spotify SpotifyAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Telegram TelegramAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Settings Settings,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AIMPWebSocket AimpWebSocket,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? UserBio,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    LastFm LastFmApi
);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Spotify(string BearerToken, AuthorizationCodeTokenResponse? Response);

public record SpotifyApp(string ClientId, string ClientSecret);

public record Settings(bool IsDeployed, bool IsDefaultBioOnPause, int Interval, int WaitInterval);

public record AIMPWebSocket(string Ip, int Port);

public record LastFm(string ApiKey, string Username);