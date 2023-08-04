using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;

namespace TelegramMusicStatus;

internal class Program
{
    private static void Main()
    {
        Run().GetAwaiter().GetResult();
    }

    private static async Task Run()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(typeof(IConfig<>), typeof(Config<>))
            .AddSingleton<ITelegramStatusService, TelegramStatusService>()
            .AddSingleton<ISpotifyMusicService, SpotifyMusicService>()
            .BuildServiceProvider(true);
        var telegramService = serviceProvider.GetService<ITelegramStatusService>();
        var spotifyService = serviceProvider.GetService<ISpotifyMusicService>();

        var status = await spotifyService.GetCurrentlyPlayingStatus();
        if (status.IsPlaying) telegramService.ChangeUserBio(status.bio);
        await Task.Delay(-1);
    }
}