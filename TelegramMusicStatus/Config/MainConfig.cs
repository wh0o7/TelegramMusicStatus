namespace TelegramMusicStatus.Config;

public record MainConfig(
    Telegram TelegramAccount,
    Spotify SpotifyAccount,
    SpotifyApp SpotifyApp
);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Spotify(string BearerToken);

public record SpotifyApp(string ClientId, string ClientSecret);