using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using Timer = System.Timers.Timer;

namespace TelegramMusicStatus;

internal sealed class ApplicationHost
{
    private Timer? _timer;
    private IConfig<MainConfig>? _config;
    private ITelegramStatusService? _telegramService;
    private ISpotifyMusicService? _spotifyService;
    private ILastFmService? _lastFmService;
    private IYandexMusicService? _yandexMusicService;
    private ITasksService? _musicService;
    private int _interval;
    private int _waitInterval;
    private bool _isWaitMode;

    internal async Task RunAsync()
    {
        try
        {
            var configInstance = new Config<MainConfig>();
            this._config = configInstance;
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfig<MainConfig>>(configInstance)
                .AddSingleton<ITelegramStatusService, TelegramStatusService>();
            this.AddMusicServiceRegistrations(serviceCollection, configInstance.Entries);
            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            this._telegramService = serviceProvider.GetRequiredService<ITelegramStatusService>();
            await this._telegramService.InitializeAsync();
            this._spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
            this._lastFmService = serviceProvider.GetService<ILastFmService>();
            this._yandexMusicService = serviceProvider.GetService<IYandexMusicService>();

            this._musicService = serviceProvider.GetService<ITasksService>();
            this._interval = this._config.Entries.Settings.Interval is >= 10 and <= 300
                ? this._config.Entries.Settings.Interval * 1000
                : 30000;
            this._waitInterval = this._config.Entries.Settings.WaitInterval is >= 20 and <= 600
                ? this._config.Entries.Settings.WaitInterval * 1000
                : this._interval * 2;
            this._timer = new Timer(this._interval);
            this._timer.Elapsed += (_, _) => _ = this.OnTimerElapsedAsync();
            await this.OnTimerElapsedAsync();
            this._timer.Start();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error during initialization: {ex.Message}");
            throw;
        }
    }

    private async Task OnTimerElapsedAsync()
    {
        try
        {
            if (this._spotifyService is null && this._lastFmService is null && this._yandexMusicService is null)
            {
                Utils.WriteLine(
                    "All music sources are disabled. Set SpotifyAccount, LastFmApi, and/or YandexMusicAccount in config.json.");
                this.OnConsoleCancelKeyPress(null, null);
                return;
            }

            var isPlaying = false;
            if (this._musicService is not null)
            {
                try
                {
                    isPlaying = this._spotifyService is not null && await this._musicService.SpotifyTask() ||
                                this._lastFmService is not null && await this._musicService.LastFmTask() ||
                                this._yandexMusicService is not null && await this._musicService.YandexMusicTask();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error in music service task: {ex.Message}");
                }
            }

            if (isPlaying)
            {
                if (this._isWaitMode) await this.DisableWaitMode();
                return;
            }

            var settings = this._config?.Entries.Settings;
            if (settings is null || !settings.IsDeployed) await this.PausePrompt();
            if (!this._isWaitMode) await this.EnableWaitMode();
            if (this._telegramService is not null && settings is not null && settings.IsDefaultBioOnPause)
            {
                try
                {
                    await this._telegramService.SetUserDefaultBio();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error setting default bio: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error in timer elapsed: {ex.Message}");
        }
    }

    private Task PausePrompt()
    {
        this._timer?.Stop();
        Utils.WriteLine("Music playback has paused. Do you want to continue? Y/N");
        var answer = Console.ReadLine();
        if (answer?.ToUpperInvariant() is "Y")
        {
            Utils.WriteLine("The application continued to run.");
            this._timer?.Start();
        }
        else this.OnConsoleCancelKeyPress(null, null);

        return Task.CompletedTask;
    }

    internal async void OnConsoleCancelKeyPress(object? sender, ConsoleCancelEventArgs? e)
    {
        try
        {
            this._timer?.Stop();
            if (this._telegramService is not null)
            {
                try
                {
                    await this._telegramService.SetUserDefaultBio();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error setting default bio on exit: {ex.Message}");
                }
            }

            Utils.WriteLine("Closing the application gracefully...");
            if (this._telegramService is not null)
            {
                try
                {
                    await this._telegramService.Close();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error closing Telegram service: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error during shutdown: {ex.Message}");
        }
        finally
        {
            Environment.Exit(0);
        }
    }

    private Task EnableWaitMode()
    {
        this._isWaitMode = true;
        this._timer!.Interval = this._waitInterval;
        Utils.WriteLine($"Wait mode enabled. Current interval is {this._waitInterval / 1000}s");
        return Task.CompletedTask;
    }

    private Task DisableWaitMode()
    {
        this._isWaitMode = false;
        this._timer!.Interval = this._interval;
        Utils.WriteLine($"Wait mode disabled. Current interval is {this._interval / 1000}s");
        return Task.CompletedTask;
    }

    private void AddMusicServiceRegistrations(IServiceCollection services, MainConfig entries)
    {
        if (entries.SpotifyAccount is not null) services.AddSingleton<ISpotifyMusicService, SpotifyMusicService>();
        if (entries.LastFmApi is not null) services.AddSingleton<ILastFmService, LastFmService>();
        if (entries.YandexMusicAccount is not null) services.AddSingleton<IYandexMusicService, YandexMusicService>();
        if (entries.SpotifyAccount is not null || entries.LastFmApi is not null || entries.YandexMusicAccount is not null) services.AddSingleton<ITasksService, TasksService>();
    }
}