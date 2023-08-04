namespace SpotifyBearerTokenGetter.Config;

public record MainConfig(
    SpotifyApp SpotifyApp
);

public record SpotifyApp(string ClientId, string ClientSecret);