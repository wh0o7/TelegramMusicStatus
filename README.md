# TelegramMusicStatus 🎵

[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.md) [![ru](https://img.shields.io/badge/lang-ru-red.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.ru-ru.md)

## Overview 🎶

TelegramMusicStatus is a project designed to keep your friends in the loop about your music preferences. It updates your Telegram status with the currently playing track from Spotify or AIMP audio player and can retrieve info about what is currently playing from LastFm. Whether you're grooving to a tune or enjoying a podcast, your Telegram status will reflect your musical mood. Even during pauses in playback, your status will remain fresh and catchy.

## Installation 🚀

1. Start by configuring the plugin. Create a `config.json` file in the project directory using the example below.

2. Run the application. Depending on your setup, it will connect to Spotify, AIMP WebSocket, or both.

## Configuration Example 🎛️

```json
{
  "SpotifyApp": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret"
  },
  "SpotifyAccount": {
    "BearerToken": "your_spotify_bearer_token",
    "Response": {
      // ...
    }
  },
  "TelegramAccount": {
    "ApiId": "your_api_id",
    "ApiHash": "your_api_hash",
    "PhoneNumber": "your_phone_number",
    "MFAPassword": "your_mfa_password"
  },
  "Settings": {
    "IsDeployed": true,
    "IsDefaultBioOnPause": false,
    "Interval": 45
  },
  "AimpWebSocket": {
    "Ip": "127.0.0.1",
    "Port": 5543
  }
}
```

- `SpotifyApp` 😎: Contains your Spotify application credentials. Essential for SpotifyBearerTokenGetter. If `config.json` and the getter are in the same spot, it gets filled automatically. If not, provide the path. Missing config? No worries – manually input values, and the config completes itself (both Spotify app and SpotifyAccount).

- `SpotifyAccount` 🎵: Holds the Spotify bearer token and other responses.

- `TelegramAccount` 💬: Houses your Telegram API credentials.

- `Settings` ⚙️: Customize app behavior, interval(min 10s, max 300s, default 30s), and bio updates.

- `AimpWebSocket` 🎧: Set AIMP WebSocket settings.

## Spotify Application Registration 🎶

To blend in with Spotify, create an app on the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications). Get your Client ID and Client Secret for `SpotifyApp` setup.

## Telegram Application Registration 💬

For Telegram magic, craft an app on the [Telegram API website](https://my.telegram.org/auth). You'll nab the `ApiId` and `ApiHash` for `TelegramAccount` section.

## Usage 🎉

1. Get your credentials ready in `config.json`.

2. Fire up the app.

3. Behold your Telegram status updating with your current jam.

For a step-by-step setup and more, head over to the project repository.

**AIMPWebSocket Requirements:**
For AIMPWebSocket to work properly, you'll need to install the [AIMP plugin](https://github.com/wh0o7/CurrentlyPlayingInfoAIMPPlugin) 😊. This plugin is necessary to capture information about the currently playing track in AIMP and transmit it over the WebSocket connection. Follow the installation instructions provided in the plugin's repository to ensure seamless integration between AIMP and AIMPWebSocket.

## Contributing 🤝

Open to suggestions! Feel free to raise issues or make pull requests on [GitHub](https://github.com/wh0o7/TelegramMusicStatus/issues).

## Questions or Feedback? 🤔

If you have any questions or want to provide feedback, you can reach out to me in the [wh0o7 heaven chat](https://t.me/+D-T_xElzA003Nzcy). Let's make the project even better together! 🎵🎉

## License 📄

This project rocks the [MIT License](LICENSE).
