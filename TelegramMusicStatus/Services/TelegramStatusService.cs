using TelegramMusicStatus.Config;
using TL;
using WTelegram;

namespace TelegramMusicStatus.Services;

public interface ITelegramStatusService
{
    Task ChangeUserBio(string bio);
    Task SetUserDefaultBio();
    Task Close();
}

public class TelegramStatusService : ITelegramStatusService
{
    private Client _telegramClient;
    private IConfig<MainConfig> _config;
    private string _userDefaultBio;
    private string _currentBio;

    public TelegramStatusService(IConfig<MainConfig> config)
    {
        this._config = config;
        this._telegramClient = new Client(TelegramConfig);
        this.Init().Wait();
    }

    public async Task ChangeUserBio(string bio)
    {
        if (bio == this._currentBio) return;
        await this._telegramClient.Account_UpdateProfile(about: bio);
        this._currentBio = bio;
        Utils.WriteLine("Bio changed to " + bio);
    }

    public async Task SetUserDefaultBio()
        => await this.ChangeUserBio(this._userDefaultBio);

    public async Task SaveCurrentBioToConfig()
    {
        var status = await GetCurrentBio();
        if (this._userDefaultBio == status) return;
        this._currentBio = status;
        if (Utils.IsValidTrackInfoFormat(status) || this._config.Entries.UserBio == status) return;
        this._userDefaultBio = status;
        Config<MainConfig>.SaveConfig(this._config.Entries with { UserBio = this._userDefaultBio });
    }

    private async Task Init()
    {
        await this._telegramClient.LoginUserIfNeeded();
        this._userDefaultBio = this._config.Entries.UserBio;
        this._currentBio = this._userDefaultBio;
        await SaveCurrentBioToConfig();
    }

    public async Task<string> GetCurrentBio() => (await this._telegramClient.Users_GetFullUser(new InputUser(
        this._telegramClient.UserId,
        this._telegramClient.User.access_hash))).full_user.about;

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
            case "password":
                if (_config.Entries.TelegramAccount.MFAPassword is null)
                {
                    Console.Write("Cloud password(2FA): ");
                    return Console.ReadLine();
                }

                return _config.Entries.TelegramAccount.MFAPassword;
            default: return null;
        }
    }

    public Task Close()
    {
        this._telegramClient.Dispose();
        return Task.CompletedTask;
    }
}