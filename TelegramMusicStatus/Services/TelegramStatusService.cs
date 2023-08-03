using TelegramMusicStatus.Config;
using TL;
using WTelegram;

namespace TelegramMusicStatus.Services;

public interface ITelegramStatusService
{
    Task ChangeUserBio(string bio);
}

public class TelegramStatusService : ITelegramStatusService
{
    private Client _telegramClient;
    private IConfig<MainConfig> _config;

    public TelegramStatusService(IConfig<MainConfig> config)
    {
        this._config = config;
        this._telegramClient = new Client(TelegramConfig);
        this._telegramClient.LoginUserIfNeeded().Wait();
    }

    public async Task ChangeUserBio(string bio)
        => await this._telegramClient.Account_UpdateProfile(about: bio);

    private string? TelegramConfig(string what)
    {
        switch (what)
        {
            case "api_id": return _config.Entries.TelegramAccount.ApiId;
            case "api_hash": return _config.Entries.TelegramAccount.ApiHash;
            case "phone_number": return _config.Entries.TelegramAccount.PhoneNumber;
            case "verification_code":
                Console.Write("Code: ");
                return Console.ReadLine();
            case "password": return _config.Entries.TelegramAccount.MFAPassword;
            default: return null;
        }
    }
}