using Hqub.Lastfm;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;

namespace TelegramMusicStatus.Services;

public interface ILastFmService : IMusicService;

public class LastFmService : ILastFmService
{
    private readonly LastfmClient _client;
    private readonly string _username;

    public LastFmService(IConfig<MainConfig> config)
    {
        var lastFm = config.Entries.LastFmApi
                     ?? throw new InvalidOperationException("LastFmApi is not configured.");
        this._client = new LastfmClient(lastFm.ApiKey);
        this._username = lastFm.Username;
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        try
        {
            var currentlyPlayingPage =
                await this._client.User.GetRecentTracksAsync(this._username, DateTime.Now.AddMinutes(-5), limit: 1);
            var currentlyPlaying = currentlyPlayingPage.Items.FirstOrDefault();
            return currentlyPlaying is null
                ? (false, null)
                : (true, $"{currentlyPlaying.Name} - {currentlyPlaying.Artist?.Name ?? ""}");
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Error getting LastFM status: {ex.Message}");
            return (false, null);
        }
    }
}