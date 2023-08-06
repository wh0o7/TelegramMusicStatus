using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using System.Timers;

namespace TelegramMusicStatus;

internal class Program
{
    private static System.Timers.Timer _timer;
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
        _telegramService = serviceProvider.GetService<ITelegramStatusService>();
        _spotifyService = serviceProvider.GetService<ISpotifyMusicService>();
        _timer = new System.Timers.Timer(30000);
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

        if (!status.IsPlaying)
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
        else
        {
            _telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        }
    }

    private static async void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Closing the application gracefully...");
        _timer.Stop();
        await _telegramService.SetUserDefaultBio();
        Environment.Exit(0);
    }
}