namespace TelegramMusicStatus.Services;

public interface ITasksService
{
    Task<bool> SpotifyTask();
    Task<bool> AimpTask();
}

public class TasksService(
    ITelegramStatusService telegramService,
    IAIMPMusicService? aimpService = null,
    ISpotifyMusicService? spotifyService = null)
    : ITasksService
{
    public async Task<bool> SpotifyTask()
    {
        if (spotifyService is null) return false;
        var status = await spotifyService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("Spotify web player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(Spotify)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        return true;

    }

    public async Task<bool> AimpTask()
    {
        if (aimpService is null) return false;
        var status = await aimpService.GetCurrentlyPlayingStatus();
        if (status.Bio is null)
        {
            Utils.WriteLine("AIMP player paused.");
            return false;
        }

        Utils.WriteLine(
            $"(AIMP)   Current state is {(status.IsPlaying ? "playing" : "paused")}, now playing: {status.Bio}");

        if (!status.IsPlaying) return false;
        await telegramService.ChangeUserBio(Utils.FormatTrackInfo(status.Bio));
        return true;
    }
}