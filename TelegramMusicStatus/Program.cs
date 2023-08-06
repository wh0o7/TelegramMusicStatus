using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using System.Timers;

namespace TelegramMusicStatus;

internal class Program
{
    private static System.Timers.Timer _timer;
    private static Config<MainConfig> _config;
    private static ITelegramStatusService? _telegramService;
    private static ISpotifyMusicService? _spotifyService;

    private static void Main()
    {
        Console.CancelKeyPress += Console_CancelKeyPress;
        Run().GetAwaiter().GetResult();
    }

    private static async Task Run()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(typeof(IConfig<>), typeof(Config<>))
            .AddSingleton<ITelegramStatusService, TelegramStatusService>()
            .AddSingleton<ISpotifyMusicService, SpotifyMusicService>()
            .BuildServiceProvider(true);
        _config = new Config<MainConfig>();
        _telegramService = serviceProvider.GetService<ITelegramStatusService>();
        _spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
        _timer = new System.Timers.Timer(_config.Entries.Settings?.Interval is >= 10 and <= 300
            ? _config.Entries.Settings.Interval * 1000
            : 30000);
        _timer.Elapsed += TimerElapsed;
        Task.Run(() => TimerElapsed(null, null)).Wait();
        _timer.Start();

        await Task.Delay(-1);
    }

    private static async void TimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        var status = await _spotifyService.GetCurrentlyPlayingStatus();
        Console.WriteLine(
            $"Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        
        if (!status.IsPlaying && (_config.Entries.Settings is null || !_config.Entries.Settings.IsDeployed))
        {
            _timer.Stop();
            Console.WriteLine("Music playback has paused. Do you want to continue? Y/N");
            var answer = Console.ReadLine();
            if (answer?.ToUpperInvariant() is "Y")
            {
                Console.WriteLine("The application continued to run.");
                _timer.Start();
            }
            else Console_CancelKeyPress(null, null);
        }
        else if(status.IsPlaying)
        {
            await _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        }

        if (!status.IsPlaying && _config.Entries.Settings is { IsDefaultBioOnPause: true }) await _telegramService.SetUserDefaultBio();
    }

    private static async void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Closing the application gracefully...");
        _timer.Stop();
        await _telegramService.SetUserDefaultBio();
        Environment.Exit(0);
    }
}