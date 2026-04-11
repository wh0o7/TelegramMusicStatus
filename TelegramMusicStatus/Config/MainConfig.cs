using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace TelegramMusicStatus.Config;

public record MainConfig(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    SpotifyApp SpotifyApp,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Spotify? SpotifyAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Telegram TelegramAccount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Settings Settings,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AIMPWebSocket? AimpWebSocket,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    LastFm? LastFmApi,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string?[]? UserBio,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? PlayingIndicator,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    YandexMusic? YandexMusicAccount
);

public record Telegram(
    string ApiId,
    string ApiHash,
    string PhoneNumber,
    string? MfaPassword,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    TelegramSocks5Proxy? Socks5 = null);

public record TelegramSocks5Proxy(string Host, int Port, string? Username, string? Password);
public record Spotify(string BearerToken, AuthorizationCodeTokenResponse? Response);
public record SpotifyApp(string ClientId, string ClientSecret);
public record Settings(bool IsDeployed, bool IsDefaultBioOnPause, int Interval, int WaitInterval);
public record AIMPWebSocket(string Ip, int Port);
public record LastFm(string ApiKey, string Username);
public record YandexMusic(string Token);