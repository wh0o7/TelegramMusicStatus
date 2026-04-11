# TelegramMusicStatus 🎵

[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.md) [![ru](https://img.shields.io/badge/lang-ru-red.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.ru-ru.md)

## Overview 🎶

TelegramMusicStatus updates your **Telegram profile bio** with the track you are playing. It supports **Spotify**, **AIMP** (WebSocket + plugin), **Last.fm**, and **Yandex Music**. When nothing is playing you can fall back to saved bios and use a slower **wait** polling interval.

**Requirements:** [.NET 10 SDK](https://dotnet.microsoft.com/download). SDK version is pinned in [`global.json`](global.json).

## Installation 🚀

1. Create `config.json` next to the executable (see below). Omit sections you do not use.

2. Run the app (`dotnet run` in `TelegramMusicStatus` or a published build). Only configured sources are used.

## Configuration Example 🎛️

JSON uses **PascalCase** property names (`System.Text.Json`). Optional objects can be omitted.

```json
{
  "SpotifyApp": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret"
  },
  "SpotifyAccount": {
    "BearerToken": "your_spotify_bearer_token",
    "Response": null
  },
  "TelegramAccount": {
    "ApiId": "your_api_id",
    "ApiHash": "your_api_hash",
    "PhoneNumber": "your_phone_number",
    "MfaPassword": "your_cloud_password_if_2fa",
    "Socks5": {
      "Host": "127.0.0.1",
      "Port": 1080,
      "Username": null,
      "Password": null
    }
  },
  "Settings": {
    "IsDeployed": true,
    "IsDefaultBioOnPause": false,
    "Interval": 45,
    "WaitInterval": 90
  },
  "UserBio": ["Default bio line 1", "Default bio line 2"],
  "PlayingIndicator": "🎵 ",
  "LastFmApi": {
    "ApiKey": "LASTFM_API_KEY",
    "Username": "LASTFM_USERNAME"
  },
  "YandexMusicAccount": {
    "Token": "YANDEX_TOKEN"
  },
  "AimpWebSocket": {
    "Ip": "127.0.0.1",
    "Port": 5543
  }
}
```

- `SpotifyApp` 😎: Spotify API app credentials (also for **SpotifyBearerTokenGetter**). The main app still expects this block; use placeholders if you only use other sources.

- `SpotifyAccount` 🎵: Bearer token and/or OAuth `Response`. Use `"Response": null` for token-only mode.

- `TelegramAccount` 💬: `ApiId`, `ApiHash`, phone; `MfaPassword` is the Telegram cloud password if 2FA is enabled. `Socks5` is optional (MTProto via SOCKS5).

- `Settings` ⚙️: `Interval` — poll interval in seconds (10–300; out-of-range falls back to ~30s). `WaitInterval` — seconds between checks in wait mode when idle (20–600). `IsDeployed` / `IsDefaultBioOnPause` control pause prompts and resetting bio from `UserBio`.

- `UserBio` / `PlayingIndicator`: optional default bios and prefix for the track line.

- `AimpWebSocket` 🎧: Host/port of the WebSocket server **this app** listens on; the AIMP plugin connects here.

- `LastFmApi` / `YandexMusicAccount`: optional integrations.

## Spotify Application Registration 🎶

To blend in with Spotify, create an app on the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications). Get your Client ID and Client Secret for `SpotifyApp` setup.

## Last.fm Application Registration 🎵

To integrate with Last.fm, you'll need to create an application on the [Last.fm Developer Account Registration](https://www.last.fm/api/account/create) page. Once registered, you'll receive your API key for `Last.fm` setup. This key will allow your application to access Last.fm's API for various music-related functionalities.

## Telegram Application Registration 💬

Create an app on [my.telegram.org](https://my.telegram.org/auth) and use `ApiId` and `ApiHash` in `TelegramAccount`.

## Usage 🎉

1. Fill in `config.json`.

2. Run **TelegramMusicStatus** (and **SpotifyBearerTokenGetter** when you need a new Spotify token).

3. Your Telegram bio updates from the first source that reports "now playing".

**AIMP:** install [CurrentlyPlayingInfoAIMPPlugin](https://github.com/wh0o7/CurrentlyPlayingInfoAIMPPlugin) 😊 and point it at the same host/port as `AimpWebSocket`.

## Contributing 🤝

Open to suggestions! Feel free to raise issues or make pull requests on [GitHub](https://github.com/wh0o7/TelegramMusicStatus/issues).

## Questions or Feedback? 🤔

If you have any questions or want to provide feedback, you can reach out to me in the [wh0o7 heaven chat](https://t.me/+D-T_xElzA003Nzcy). Let's make the project even better together! 🎵🎉

## Used Libraries 📚

- [WTelegramClient](https://github.com/wiz0u/WTelegramClient) — Telegram MTProto client
- [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET) — Spotify Web API
- [Yandex.Music.Api](https://github.com/K1llMan/Yandex.Music.Api) — Yandex Music API
- [Last.fm](https://github.com/avatar29A/Last.fm) — Last.fm client
- [Improved Console](https://github.com/litolax/Improved-Console) — console logging helpers

## License 📄

This project rocks the [MIT License](LICENSE).
