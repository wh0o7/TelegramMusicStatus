using System.Security.Cryptography;
using TelegramMusicStatus.Config;
using TL;
using WTelegram;

namespace TelegramMusicStatus.Services;

public interface ITelegramStatusService
{
    Task ChangeUserBio(string? bio);
    Task SetUserDefaultBio();
    Task Close();
}

public class TelegramStatusService : ITelegramStatusService
{
    private readonly Client _telegramClient;
    private readonly IConfig<MainConfig> _config;
    private readonly List<string> _userDefaultBioList;
    private string? _currentBio;

    public TelegramStatusService(IConfig<MainConfig> config)
    {
        this._config = config;
        this._userDefaultBioList = [..this._config.Entries.UserBio?.Where(bio => !string.IsNullOrEmpty(bio))];
        this._telegramClient = new Client(TelegramConfig);
        this.Init().Wait();
    }

    private async Task Init()
    {
        await this._telegramClient.LoginUserIfNeeded();
        await SaveCurrentBioToConfig();
        var timer = new System.Timers.Timer(TimeSpan.FromHours(4).TotalMilliseconds);
        timer.Elapsed += async (_, _) => { await this._telegramClient.LoginUserIfNeeded(reloginOnFailedResume: true); };
        timer.Start();
    }

    public async Task ChangeUserBio(string? bio)
    {
        if (bio == this._currentBio) return;
        await this._telegramClient.Account_UpdateProfile(about: bio);
        this._currentBio = bio;
        Utils.WriteLine("Bio changed to " + bio);
    }

    public async Task SetUserDefaultBio()
    {
        switch (this._userDefaultBioList.Count)
        {
            case 0:
                Utils.WriteLine("Bio didn't change to default. No default bio.");
                return;
            case 1:
                await this.ChangeUserBio(this._userDefaultBioList[0]);
                break;
            default:
                await this.ChangeUserBio(GetRandomBio() ?? string.Empty);
                break;
        }
    }

    private async Task SaveCurrentBioToConfig()
    {
        var status = await GetCurrentBio();
        if (this._userDefaultBioList.Any(s => s == status)) return;
        this._currentBio = status;
        if (string.IsNullOrEmpty(status?.Trim()) || Utils.IsValidTrackInfoFormat(status))
        {
            await SetUserDefaultBio();
            return;
        }

        this._userDefaultBioList.Add(status);
        await Config<MainConfig>.SaveConfig(this._config.Entries with { UserBio = this._userDefaultBioList.ToArray() });
    }

    private async Task<string?> GetCurrentBio() => (await this._telegramClient.Users_GetFullUser(new InputUser(
        this._telegramClient.UserId,
        this._telegramClient.User.access_hash))).full_user.about;

    public Task Close()
    {
        this._telegramClient.Dispose();
        return Task.CompletedTask;
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
            case "password":
                if (_config.Entries.TelegramAccount.MfaPassword is not null)
                    return _config.Entries.TelegramAccount.MfaPassword;
                Console.Write("Cloud password(2FA): ");
                return Console.ReadLine();

            default: return null;
        }
    }

    private string? GetRandomBio()
    {
        var filteredList = _userDefaultBioList.Where(bio => bio != _currentBio).ToArray();
        if (!filteredList.Any()) return null;
        if (filteredList.Length == 1) return filteredList.First();
        int index = Random.Shared.Next(0, filteredList.Length - 1);
        return filteredList[index];
    }
}