using Hqub.Lastfm;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;

namespace TelegramMusicStatus.Services;

public interface ILastFmService : IMusicService
{
    new Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
}

public class LastFmService : ILastFmService
{
    private readonly LastfmClient _client;
    private readonly string _username;

    public LastFmService(IConfig<MainConfig> config)
    {
        this._client = new LastfmClient(config.Entries.LastFmApi.ApiKey);
        this._username = config.Entries.LastFmApi.Username;
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        var currentlyPlayingPage =
            await this._client.User.GetRecentTracksAsync(this._username, from: DateTime.Now.AddMinutes(-5), limit: 1);
        var currentlyPlaying = currentlyPlayingPage.Items.FirstOrDefault();
        return currentlyPlaying is null
            ? (false, null)
            : (true, $"{currentlyPlaying.Name} - {currentlyPlaying.Artist.Name}");
    }
}