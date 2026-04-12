using TelegramMusicStatus.Config;
using TelegramMusicStatus.Net;
using TL;
using WTelegram;
using Timer = System.Timers.Timer;

namespace TelegramMusicStatus.Services;

public interface ITelegramStatusService
{
    Task InitializeAsync();
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
    private readonly string? _playingIndicator;
    private readonly Timer _reloginTimer;
    private bool _isAuthenticating = false;
    private DateTime _lastAuthAttempt = DateTime.MinValue;
    private DateTime _authBlockedUntil = DateTime.MinValue;
    private int _authRetryDelayMinutes = 5;
    private readonly object _authLock = new();

    public TelegramStatusService(IConfig<MainConfig> config)
    {
        this._config = config;
        this._playingIndicator = config.Entries.PlayingIndicator;
        this._userDefaultBioList = this._config.Entries.UserBio?.Where(bio => !string.IsNullOrEmpty(bio)).OfType<string>().ToList() ?? [];
        this._telegramClient = new Client(this.TelegramConfig);
        var account = this._config.Entries.TelegramAccount;
        if (!string.IsNullOrWhiteSpace(account.MTProxyUrl))
        {
            this._telegramClient.MTProxyUrl = account.MTProxyUrl.Trim();
            Utils.WriteLine("Telegram will use MTProxy (WTelegramClient MTProxyUrl).");
        }
        else
        {
            WTelegramSocks5.ApplyIfConfigured(this._telegramClient, account.Socks5);
            if (account.Socks5 is not null)
                Utils.WriteLine($"Telegram MTProto will use SOCKS5 {account.Socks5.Host}:{account.Socks5.Port}");
        }

        this._reloginTimer = new Timer(TimeSpan.FromHours(4).TotalMilliseconds);
        this._reloginTimer.Elapsed += this.OnReloginTimerElapsed;
    }

    public async Task InitializeAsync()
    {
        await this.EnsureAuthenticated();
        await this.SaveCurrentBioToConfig();
        this._reloginTimer.Start();
    }

    private async void OnReloginTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            await this.EnsureAuthenticated(reloginOnFailedResume: true);
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error in relogin timer: {ex.Message}");
        }
    }

    private async Task<bool> EnsureAuthenticated(bool reloginOnFailedResume = false)
    {
        lock (this._authLock)
        {
            if (this._isAuthenticating)
            {
                Utils.WriteLine("Authentication already in progress, skipping...");
                return false;
            }

            if (DateTime.UtcNow < this._authBlockedUntil)
            {
                var remaining = this._authBlockedUntil - DateTime.UtcNow;
                Utils.WriteLine($"Auth blocked until {this._authBlockedUntil:yyyy-MM-dd HH:mm:ss} UTC (remaining: {remaining.TotalMinutes:F1} minutes)");
                return false;
            }

            var timeSinceLastAttempt = DateTime.UtcNow - this._lastAuthAttempt;
            if (timeSinceLastAttempt.TotalMinutes < this._authRetryDelayMinutes && this._lastAuthAttempt != DateTime.MinValue)
            {
                Utils.WriteLine($"Too soon to retry auth. Next attempt in {this._authRetryDelayMinutes - timeSinceLastAttempt.TotalMinutes:F1} minutes");
                return true;
            }

            this._isAuthenticating = true;
        }

        try
        {
            await this._telegramClient.LoginUserIfNeeded(null, reloginOnFailedResume);
            this._authRetryDelayMinutes = 5;
            Utils.WriteLine("Successfully authenticated with Telegram");
            return true;
        }
        catch (RpcException ex) when (ex.Code == 420)
        {
            var waitSeconds = this.ExtractWaitTime(ex);
            lock (this._authLock)
            {
                this._authBlockedUntil = DateTime.UtcNow.AddSeconds(waitSeconds);
            }
            Utils.WriteLine($"FLOOD_WAIT: Blocked for {waitSeconds} seconds ({waitSeconds / 60.0:F1} minutes). Will retry after {this._authBlockedUntil:yyyy-MM-dd HH:mm:ss} UTC");
            return false;
        }
        catch (RpcException ex)
        {
            lock (this._authLock)
            {
                this._authRetryDelayMinutes = Math.Min(this._authRetryDelayMinutes * 2, 60);
            }
            Utils.WriteLine($"Auth RPC error (code {ex.Code}): {ex.Message}. Next retry in {this._authRetryDelayMinutes} minutes");
            return false;
        }
        catch (Exception ex)
        {
            lock (this._authLock)
            {
                this._authRetryDelayMinutes = Math.Min(this._authRetryDelayMinutes * 2, 60);
            }
            Utils.WriteLine($"Auth error: {ex.Message}. Next retry in {this._authRetryDelayMinutes} minutes");
            return false;
        }
        finally
        {
            lock (this._authLock)
            {
                this._isAuthenticating = false;
                this._lastAuthAttempt = DateTime.UtcNow;
            }
        }
    }

    private int ExtractWaitTime(RpcException ex)
    {
        var message = ex.Message ?? string.Empty;
        if (int.TryParse(message.Replace("FLOOD_WAIT_", "").Split(' ').FirstOrDefault(), out var seconds))
            return seconds;
        return 60;
    }

    public async Task ChangeUserBio(string? bio)
    {
        if (bio == this._currentBio) return;

        try
        {
            await this._telegramClient.Account_UpdateProfile(about: bio);
            this._currentBio = bio;
            Utils.WriteLine("Bio changed to " + bio);
        }
        catch (RpcException ex) when (ex.Code == 420)
        {
            var waitSeconds = this.ExtractWaitTime(ex);
            lock (this._authLock)
            {
                this._authBlockedUntil = DateTime.UtcNow.AddSeconds(waitSeconds);
            }
            Utils.WriteLine($"FLOOD_WAIT when updating bio: Blocked for {waitSeconds} seconds. Bio update skipped.");
        }
        catch (RpcException ex) when (ex.Code == 401 || ex.Code == 403)
        {
            Utils.WriteLine($"Session error when updating bio (code {ex.Code}): {ex.Message}. Attempting to re-authenticate...");
            var authenticated = await this.EnsureAuthenticated(reloginOnFailedResume: true);
            if (authenticated)
            {
                try
                {
                    await this._telegramClient.Account_UpdateProfile(about: bio);
                    this._currentBio = bio;
                    Utils.WriteLine("Bio changed to " + bio + " (after re-authentication)");
                }
                catch (Exception retryEx)
                {
                    Utils.WriteLine($"Failed to update bio after re-authentication: {retryEx.Message}");
                }
            }
            else
            {
                Utils.WriteLine("Could not re-authenticate. Bio update skipped.");
            }
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error updating bio: {ex.Message}. Bio update skipped.");
        }
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
                await this.ChangeUserBio(this.GetRandomBio() ?? string.Empty);
                break;
        }
    }

    private async Task SaveCurrentBioToConfig()
    {
        try
        {
            var status = await this.GetCurrentBio();
            if (status is null || this._userDefaultBioList.Any(s => s == status)) return;
            this._currentBio = status;
            if (string.IsNullOrEmpty(status.Trim()) || Utils.IsValidTrackInfoFormat(status, this._playingIndicator))
            {
                await this.SetUserDefaultBio();
                return;
            }

            this._userDefaultBioList.Add(status);
            await Config<MainConfig>.SaveConfig(this._config.Entries with { UserBio = this._userDefaultBioList.ToArray() });
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error saving current bio to config: {ex.Message}");
        }
    }

    private async Task<string?> GetCurrentBio()
    {
        try
        {
            if (this._telegramClient.UserId == 0 || this._telegramClient.User?.access_hash == null)
            {
                Utils.WriteLine("Cannot get current bio: User not authenticated");
                return null;
            }

            var result = await this._telegramClient.Users_GetFullUser(new InputUser(this._telegramClient.UserId, this._telegramClient.User.access_hash));
            return result.full_user.about;
        }
        catch (RpcException ex) when (ex.Code == 401 || ex.Code == 403)
        {
            Utils.WriteLine($"Session error when getting bio (code {ex.Code}): {ex.Message}");
            await this.EnsureAuthenticated(reloginOnFailedResume: true);
            return null;
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error getting current bio: {ex.Message}");
            return null;
        }
    }

    public Task Close()
    {
        this._reloginTimer?.Stop();
        this._reloginTimer?.Dispose();
        this._telegramClient?.Dispose();
        return Task.CompletedTask;
    }

    private string? TelegramConfig(string what)
    {
        switch (what)
        {
            case "api_id": return this._config.Entries.TelegramAccount.ApiId;
            case "api_hash": return this._config.Entries.TelegramAccount.ApiHash;
            case "phone_number": return this._config.Entries.TelegramAccount.PhoneNumber;
            case "verification_code":
                Console.Write("Code: ");
                return Console.ReadLine();
            case "password":
                if (this._config.Entries.TelegramAccount.MfaPassword is not null)
                    return this._config.Entries.TelegramAccount.MfaPassword;
                Console.Write("Cloud password(2FA): ");
                return Console.ReadLine();

            default: return null;
        }
    }

    private string? GetRandomBio()
    {
        var filteredList = this._userDefaultBioList.Where(bio => bio != this._currentBio).ToArray();
        switch (filteredList.Length)
        {
            case 0:
                return null;
            case 1:
                return filteredList.First();
            default:
            {
                var index = Random.Shared.Next(filteredList.Length);
                return filteredList[index];
            }
        }
    }
}