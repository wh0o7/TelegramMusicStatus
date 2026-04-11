using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using Timer = System.Timers.Timer;

namespace TelegramMusicStatus;

internal static class Program
{
    private static Timer? _timer;
    private static IConfig<MainConfig>? _config;
    private static ITelegramStatusService? _telegramService;
    private static ISpotifyMusicService? _spotifyService;
    private static IAIMPMusicService? _aimpService;
    private static ILastFmService? _lastFmService;
    private static IYandexMusicService? _yandexMusicService;
    private static ITasksService? _musicService;

    private static int _interval;
    private static int _waitInterval;
    private static bool IsWaitMode { get; set; }

    private static void Main()
    {
        try
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            Run().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static async Task Run()
    {
        try
        {
            var configInstance = new Config<MainConfig>();
            _config = configInstance;
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfig<MainConfig>>(configInstance)
                .AddSingleton<ITelegramStatusService, TelegramStatusService>();
            AddMusicServiceRegistrations(serviceCollection, configInstance.Entries);
            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            _telegramService = serviceProvider.GetRequiredService<ITelegramStatusService>();
            await _telegramService.InitializeAsync();
            _spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
            _aimpService = serviceProvider.GetService<IAIMPMusicService>();
            _lastFmService = serviceProvider.GetService<ILastFmService>();
            _yandexMusicService = serviceProvider.GetService<IYandexMusicService>();

            _musicService = serviceProvider.GetService<ITasksService>();
            _interval = _config.Entries.Settings.Interval is >= 10 and <= 300
                ? _config.Entries.Settings.Interval * 1000
                : 30000;
            _waitInterval = _config.Entries.Settings.WaitInterval is >= 20 and <= 600
                ? _config.Entries.Settings.WaitInterval * 1000
                : _interval * 2;
            _timer = new Timer(_interval);
            _timer.Elapsed += (_, _) => _ = OnTimerElapsedAsync();
            await OnTimerElapsedAsync();
            _timer.Start();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error during initialization: {ex.Message}");
            throw;
        }
    }

    private static async Task OnTimerElapsedAsync()
    {
        try
        {
            if (_spotifyService is null && _aimpService is null && _lastFmService is null && _yandexMusicService is null)
            {
                Utils.WriteLine(
                    "All music sources are disabled. Set SpotifyAccount, AimpWebSocket, LastFmApi, and/or YandexMusicAccount in config.json.");
                Console_CancelKeyPress(null, null);
                return;
            }

            var isPlaying = false;
            if (_musicService is not null)
            {
                try
                {
                    isPlaying = _spotifyService is not null && await _musicService.SpotifyTask() ||
                                _aimpService is not null && await _musicService.AimpTask() ||
                                _lastFmService is not null && await _musicService.LastFmTask() ||
                                _yandexMusicService is not null && await _musicService.YandexMusicTask();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error in music service task: {ex.Message}");
                }
            }

            if (isPlaying)
            {
                if (IsWaitMode) await DisableWaitMode();
                return;
            }

            var settings = _config?.Entries.Settings;
            if (settings is null || !settings.IsDeployed) await PausePrompt();
            if (!IsWaitMode) await EnableWaitMode();
            if (_telegramService is not null && settings is not null && settings.IsDefaultBioOnPause)
            {
                try
                {
                    await _telegramService.SetUserDefaultBio();
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

    private static Task PausePrompt()
    {
        _timer?.Stop();
        Utils.WriteLine("Music playback has paused. Do you want to continue? Y/N");
        var answer = Console.ReadLine();
        if (answer?.ToUpperInvariant() is "Y")
        {
            Utils.WriteLine("The application continued to run.");
            _timer?.Start();
        }
        else Console_CancelKeyPress(null, null);

        return Task.CompletedTask;
    }

    private static async void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs? e)
    {
        try
        {
            _timer?.Stop();
            if (_telegramService is not null)
            {
                try
                {
                    await _telegramService.SetUserDefaultBio();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error setting default bio on exit: {ex.Message}");
                }
            }
            Utils.WriteLine("Closing the application gracefully...");
            if (_aimpService is { } aimp)
            {
                try
                {
                    await aimp.Close();
                }
                catch (Exception ex)
                {
                    Utils.WriteLine($"Error closing AIMP service: {ex.Message}");
                }
            }
            if (_telegramService is not null)
            {
                try
                {
                    await _telegramService.Close();
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

    private static Task EnableWaitMode()
    {
        IsWaitMode = true;
        _timer!.Interval = _waitInterval;
        Utils.WriteLine($"Wait mode enabled. Current interval is {_waitInterval / 1000}s");
        return Task.CompletedTask;
    }

    private static Task DisableWaitMode()
    {
        IsWaitMode = false;
        _timer!.Interval = _interval;
        Utils.WriteLine($"Wait mode disabled. Current interval is {_interval / 1000}s");
        return Task.CompletedTask;
    }

    private static void AddMusicServiceRegistrations(IServiceCollection services, MainConfig entries)
    {
        if (entries.SpotifyAccount is not null) services.AddSingleton<ISpotifyMusicService, SpotifyMusicService>();
        if (entries.AimpWebSocket is not null) services.AddSingleton<IAIMPMusicService, AIMPMusicService>();
        if (entries.LastFmApi is not null) services.AddSingleton<ILastFmService, LastFmService>();
        if (entries.YandexMusicAccount is not null) services.AddSingleton<IYandexMusicService, YandexMusicService>();
        if (entries.AimpWebSocket is not null || entries.SpotifyAccount is not null ||
            entries.LastFmApi is not null || entries.YandexMusicAccount is not null)
            services.AddSingleton<ITasksService, TasksService>();
    }
}