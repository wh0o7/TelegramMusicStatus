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
    private string _userAbout;

    public TelegramStatusService(IConfig<MainConfig> config)
    {
        this._config = config;
        this._telegramClient = new Client(TelegramConfig);
        this.Init().Wait();
    }

    public async Task ChangeUserBio(string bio)
        => await this._telegramClient.Account_UpdateProfile(about: bio);

    public async Task SetUserDefaultBio()
        => await this.ChangeUserBio(this._userAbout);

    private async Task Init()
    {
        await this._telegramClient.LoginUserIfNeeded();
        this._userAbout = (await this._telegramClient.Users_GetFullUser(new InputUser(this._telegramClient.UserId,
            this._telegramClient.User.access_hash))).full_user.about;
    }

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