using TelegramMusicStatus.Services;

namespace TelegramMusicStatus;

internal static class Program
{
    private static async Task Main()
    {
        try
        {
            var app = new ApplicationHost();
            Console.CancelKeyPress += app.OnConsoleCancelKeyPress;
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
