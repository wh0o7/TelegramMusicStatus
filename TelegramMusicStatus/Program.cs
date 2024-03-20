using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using Timer = System.Timers.Timer;

namespace TelegramMusicStatus;

internal static class Program
{
    private static Timer? _timer;
    private static Config<MainConfig>? _config;
    private static ITelegramStatusService? _telegramService;
    private static ISpotifyMusicService? _spotifyService;
    private static IAIMPMusicService? _aimpService;
    private static ILastFmService? _lastFmService;
    private static ITasksService? _musicService;
    private static int _interval;
    private static int _waitInterval;
    private static bool IsWaitMode { get; set; }

    private static void Main()
    {
        Console.CancelKeyPress += Console_CancelKeyPress;
        Run().GetAwaiter().GetResult();
    }

    private static async Task Run()
    {
        _config = new Config<MainConfig>();
        var serviceCollection = new ServiceCollection()
            .AddSingleton(typeof(IConfig<>), typeof(Config<>))
            .AddSingleton<ITelegramStatusService, TelegramStatusService>();
        if (_config.Entries.SpotifyAccount is not null)
            serviceCollection.AddSingleton<ISpotifyMusicService, SpotifyMusicService>();
        if (_config.Entries.AimpWebSocket is not null)
            serviceCollection.AddSingleton<IAIMPMusicService, AIMPMusicService>();
        if (_config.Entries.LastFmApi is not null)
            serviceCollection.AddSingleton<ILastFmService, LastFmService>();
        if (_config.Entries.AimpWebSocket is not null || _config.Entries.SpotifyAccount is not null ||
            _config.Entries.LastFmApi is not null)
            serviceCollection.AddSingleton<ITasksService, TasksService>();
        var serviceProvider = serviceCollection.BuildServiceProvider(true);

        _telegramService = serviceProvider.GetService<ITelegramStatusService>();
        _spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
        _aimpService = serviceProvider.GetService<IAIMPMusicService>();
        _lastFmService = serviceProvider.GetService<ILastFmService>();
        _musicService = serviceProvider.GetService<ITasksService>();
        _interval = _config.Entries.Settings.Interval is >= 10 and <= 300
            ? _config.Entries.Settings.Interval * 1000
            : 30000;
        _waitInterval = _config.Entries.Settings.WaitInterval is >= 20 and <= 600
            ? _config.Entries.Settings.WaitInterval * 1000
            : _interval * 2;
        _timer = new Timer(_interval);
        _timer.Elapsed += TimerElapsed;
        Task.Run(() => TimerElapsed(null, null)).Wait();
        _timer.Start();
        await Task.Delay(-1);
    }

    private static async void TimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        if (_spotifyService is null && _aimpService is null && _lastFmService is null)
        {
            Utils.WriteLine(
                "All of services are disabled. Check your config.json for SpotifyAccount and/or AimpWebSocket and/or LastFmApi");
            Console_CancelKeyPress(null, null);
        }

        if (_musicService is not null && ((_spotifyService is not null && await _musicService.SpotifyTask()) ||
                                          (_aimpService is not null && await _musicService.AimpTask()) ||
                                          (_lastFmService is not null && await _musicService.LastFmTask())))
        {
            if (IsWaitMode) await DisableWaitMode();
            return;
        }

        if (_config?.Entries.Settings is null || !_config.Entries.Settings.IsDeployed) await PausePrompt();
        if (!IsWaitMode) await EnableWaitMode();
        if (_telegramService is not null && _config?.Entries.Settings is not null &&
            _config.Entries.Settings.IsDefaultBioOnPause) await _telegramService.SetUserDefaultBio();
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
        _timer?.Stop();
        await _telegramService?.SetUserDefaultBio()!;
        Utils.WriteLine("Closing the application gracefully...");
        if (_aimpService is not null) await _aimpService.Close()!;
        await _telegramService.Close();
        Environment.Exit(0);
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
}