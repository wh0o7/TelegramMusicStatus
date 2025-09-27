using Swan;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Client;

namespace TelegramMusicStatus.Services;

public interface IYandexMusicService : IMusicService
{
    new Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus();
}

public sealed class YandexMusicService : IYandexMusicService
{
    private readonly YandexMusicClient _client;
    private readonly YandexMusicApi _api;
    private readonly AuthStorage _storage;

    public YandexMusicService(IConfig<MainConfig> config)
    {
        var ym = config.Entries.YandexMusicAccount;

        if (ym is null) return;
        this._storage = new AuthStorage { DeviceId = ym.DeviceId };

        this._api = new YandexMusicApi();
        this._api.User.Authorize(this._storage, ym.Token);

        this._client = new YandexMusicClient();
        this._client.Authorize(ym.Token);

        Utils.WriteLine("Yandex Music client started!");
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        try
        {
            var player = this._api.Ynison.GetPlayer(this._storage);

            var status = player.State.PlayerState?.Status;
            var queue = player.State.PlayerState?.PlayerQueue;

            if (status == null || queue == null || queue.CurrentPlayableIndex < 0) return (false, null);
            if (status.Paused) return (false, null);

            var idx = queue.CurrentPlayableIndex;
            var playable = queue.PlayableList[idx];
            var trackId = playable.PlayableId;

            var track = this._client.GetTrack(trackId);

            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";
            return (true, bio);
        }
        catch (Exception ex)
        {
            Utils.WriteLine("Ynison.GetPlayer не удался, пробуем фолбэк на очереди…\n" + ex.ToJson());
            return await this.FallbackByQueue();
        }
    }

    private Task<(bool IsPlaying, string? Bio)> FallbackByQueue()
    {
        try
        {
            var queues = this._client.QueuesList(this._storage.DeviceId);
            var queueId = queues.Queues?.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(queueId)) return Task.FromResult((false, (string?)null));

            var queue = this._client.GetQueue(queueId);
            var currentIndex = queue?.CurrentIndex ?? -1;
            if (queue is null || currentIndex < 0 || queue.Tracks == null || queue.Tracks.Count <= currentIndex) return Task.FromResult((false, (string?)null));

            var current = queue.Tracks[currentIndex];
            var track = this._client.GetTrack(current.TrackId);

            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";
            return Task.FromResult((IsPlaying: true, Bio: bio))!;
        }
        catch (Exception ex)
        {
            Utils.WriteLine("FallbackByQueue не удался\n" + ex.ToJson());
            return Task.FromResult((false, (string?)null));
        }
    }
}