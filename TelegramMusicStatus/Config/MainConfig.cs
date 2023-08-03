namespace TelegramMusicStatus.Config;

public record MainConfig(
    Telegram TelegramAccount,
    Spotify SpotifyApp
);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Spotify(string ClientId, string ClientSecret);