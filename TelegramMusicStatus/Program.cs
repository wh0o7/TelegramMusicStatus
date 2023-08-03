using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web.Auth;
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
        var spotify = serviceProvider.GetService<ISpotifyMusicService>();
        await Task.Delay(-1);
    }
}