using Microsoft.Extensions.DependencyInjection;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Services;
using TL;
using WTelegram;

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
            .BuildServiceProvider(true);
        var statusService = serviceProvider.GetService<ITelegramStatusService>();
        await statusService.ChangeUserBio("ggnb");
        await Task.Delay(-1);
    }
}