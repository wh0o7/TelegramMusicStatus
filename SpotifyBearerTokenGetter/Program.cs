namespace SpotifyBearerTokenGetter;

internal static class Program
{
    private static async Task Main()
    {
        await new SpotifyTokenGetterApp().RunAsync();
    }
}
