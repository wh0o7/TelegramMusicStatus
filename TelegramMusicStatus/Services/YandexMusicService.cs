using Swan;
using TelegramMusicStatus.Config;
using TelegramMusicStatus.Models;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Common.Ynison;

namespace TelegramMusicStatus.Services;

public interface IYandexMusicService : IMusicService;

public sealed class YandexMusicService : IYandexMusicService
{
    private readonly IConfig<MainConfig> _config;
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private YandexMusicApi? _api;
    private YnisonPlayer? _player;
    private AuthStorage? _storage;
    private YandexMusicState? _state;

    public YandexMusicService(IConfig<MainConfig> config)
    {
        this._config = config;
        Utils.WriteLine("Yandex Music client started!");
    }

    private async Task EnsurePlayerAsync()
    {
        await this._connectLock.WaitAsync().ConfigureAwait(false);
        try
        {
            this._player?.Dispose();
            this._player = null;

            var ym = this._config.Entries.YandexMusicAccount;
            if (ym is null) return;

            this._storage = new AuthStorage { DeviceId = Guid.NewGuid().ToString() };

            this._api = new YandexMusicApi();
            this._api.User.Authorize(this._storage, ym.Token);

            var auth = await this._api.User.GetUserAuthAsync(this._storage).ConfigureAwait(false);
            Utils.WriteLine($"Getting player...\n Current Yandex user is {auth.Result.Account.Login.ToJson()}");

            this._player = this._api.Ynison.GetPlayer(this._storage);
            this._player.Connect();
            await Task.Delay(5000).ConfigureAwait(false);
            this._player.OnClose += this.PlayerOnOnClose;
        }
        finally
        {
            this._connectLock.Release();
        }
    }

    private void PlayerOnOnClose(YnisonPlayer player, YnisonPlayer.CloseEventArgs args)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await this.EnsurePlayerAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Utils.WriteLine($"Yandex Music reconnect error: {ex.Message}");
            }
        });
    }

    public async Task<(bool IsPlaying, string? Bio)> GetCurrentlyPlayingStatus()
    {
        try
        {
            if (this._api is null || this._storage is null)
                await this.EnsurePlayerAsync().ConfigureAwait(false);
            if (this._api is null || this._storage is null)
                return (false, null);

            var userAuth = (await this._api.User.GetUserAuthAsync(this._storage).ConfigureAwait(false)).Result.Account;
            if (userAuth is null || this._player is null)
            {
                Utils.WriteLine(
                    $"(Yandex Music)   Something null: userAuth is {(userAuth is null ? "null" : "not null")}, player is {(this._player is null ? "null" : "not null")}");
                await this.EnsurePlayerAsync().ConfigureAwait(false);
            }

            if (this._player is null)
            {
                Utils.WriteLine("(Yandex Music)  PLAYER IS NULL 2-ND TIME IN A ROW!");
                return (false, null);
            }

            var status = this._player.State.PlayerState?.Status;
            if (status is null)
            {
                await this.EnsurePlayerAsync().ConfigureAwait(false);
            }

            status = this._player.State.PlayerState?.Status;
            var track = this._player.Current;
            var bio = $"{track.Title} - {string.Join(", ", track.Artists.Select(a => a.Name))}";
            var now = DateTime.UtcNow;

            if (status is null || status.Paused && this._state is not null && this._state.Id == track.Id && now > this._state.EstimatedFinish)
            {
                await this.EnsurePlayerAsync().ConfigureAwait(false);
                Utils.WriteLine("(Yandex Music)  STATUS IS NULL 2-ND TIME IN A ROW! " +
                                $"[STATE]:{this._state?.ToJson() ?? "null"} [TIME_NOW]:{now} [PLAYER_STATE]:{this._player.State.PlayerState?.Status.ToJson()}");
                return (false, null);
            }

            if (this._state is not null && this._state.Id == track.Id && now > this._state.FirstSeen && now < this._state.EstimatedFinish) return (true, bio);
            this._state = new YandexMusicState(status.Paused, track.Id, now, now.AddMilliseconds(track.DurationMs));

            return (true, bio);
        }
        catch (Exception ex)
        {
            Utils.WriteLine($"Yandex Music error: {ex.Message}");
            try
            {
                await this.EnsurePlayerAsync().ConfigureAwait(false);
            }
            catch (Exception reconnectEx)
            {
                Utils.WriteLine($"Yandex Music reconnect after error failed: {reconnectEx.Message}");
            }

            return (false, null);
        }
    }

    private record YandexMusicState(bool IsPaused, string Id, DateTime FirstSeen, DateTime EstimatedFinish);
}