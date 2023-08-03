namespace TelegramMusicStatus.Config;

public record MainConfig(
    Telegram TelegramAccount
);

public record Telegram(string ApiId, string ApiHash, string PhoneNumber, string MFAPassword);

public record Spotify();