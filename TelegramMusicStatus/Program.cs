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
    private static ITasksService _musicService;

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
        if (_config.Entries.AimpWebSocket is not null || _config.Entries.SpotifyAccount is not null)
            serviceCollection.AddSingleton<ITasksService, TasksService>();
        var serviceProvider = serviceCollection.BuildServiceProvider(true);

        _telegramService = serviceProvider.GetService<ITelegramStatusService>();
        _spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
        _aimpService = serviceProvider.GetService<IAIMPMusicService>();
        _musicService = serviceProvider.GetService<ITasksService>();
        _timer = new Timer(_config.Entries.Settings?.Interval is >= 10 and <= 300
            ? _config.Entries.Settings.Interval * 1000
            : 30000);
        _timer.Elapsed += TimerElapsed;
        Task.Run(() => TimerElapsed(null, null)).Wait();
        _timer.Start();

        await Task.Delay(-1);
    }

    private static async void TimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        if (_spotifyService is null && _aimpService is null)
        {
            Utils.WriteLine(
                "Both of services are disabled. Check your config.json for SpotifyAccount and/or AimpWebSocket");
            Console_CancelKeyPress(null, null);
        }

        if (_spotifyService is not null && await _musicService.SpotifyTask()) return;
        if (_aimpService is not null && await _musicService.AIMPTask()) return;


        if (_config?.Entries.Settings is null || !_config.Entries.Settings.IsDeployed)
        {
            await PausePrompt();
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
        _timer?.Stop();
        await _telegramService?.SetUserDefaultBio()!;
        Utils.WriteLine("Closing the application gracefully...");
        if (_aimpService is not null) await _aimpService.Close()!;
        await _telegramService.Close();
        Environment.Exit(0);
    }
}