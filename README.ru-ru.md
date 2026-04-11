# TelegramMusicStatus 🎵

[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.md) [![ru](https://img.shields.io/badge/lang-ru-red.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.ru-ru.md)

## Обзор 🎶

TelegramMusicStatus синхронизирует **описание профиля (bio)** в Telegram с тем, что вы сейчас слушаете. Поддерживаются **Spotify**, **AIMP** (WebSocket + плагин), **Last.fm** и **Яндекс Музыка**. В паузе можно использовать запасные bio и реже опрашивать источники (режим ожидания).

**Требования:** [.NET 10 SDK](https://dotnet.microsoft.com/download). Версия SDK зафиксирована в [`global.json`](global.json).

## Установка 🚀

1. Создайте `config.json` рядом с исполняемым файлом (см. пример). Неиспользуемые секции можно не указывать.

2. Запустите приложение (`dotnet run` в каталоге `TelegramMusicStatus` или собранный билд). Подключаются только настроенные источники.

## Пример настройки 🎛️

Имена полей — **PascalCase** (`System.Text.Json`). Необязательные объекты можно опустить.

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
  "UserBio": ["Запасной текст 1", "Запасной текст 2"],
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

- `SpotifyApp` 😎: учётные данные приложения Spotify (для **SpotifyBearerTokenGetter**). Основное приложение всё равно ожидает этот блок в `config.json`; при отказе от Spotify можно указать заглушки.

- `SpotifyAccount` 🎵: bearer-токен и при необходимости OAuth `Response`; для режима только токена — `"Response": null`.

- `TelegramAccount` 💬: `ApiId`, `ApiHash`, телефон; `MfaPassword` — облачный пароль при 2FA. `Socks5` — необязательный прокси для MTProto.

- `Settings` ⚙️: `Interval` — интервал опроса в секундах (10–300). `WaitInterval` — интервал в режиме ожидания, когда ничего не играет (20–600). `IsDeployed` / `IsDefaultBioOnPause` — запрос при паузе и сброс bio из `UserBio`.

- `UserBio` / `PlayingIndicator`: необязательные запасные bio и префикс строки трека.

- `AimpWebSocket` 🎧: хост/порт WebSocket-сервера, который поднимает **это** приложение; к нему подключается плагин AIMP.

- `LastFmApi` / `YandexMusicAccount`: по желанию.

## Регистрация Spotify Application 🎶

Чтобы использовать интеграцию с Spotify, создайте приложение на [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications). Получите Client ID и Client Secret для настройки `SpotifyApp`.

## Регистрация Last.fm 🎵

Создайте API-ключ на [last.fm/api/account/create](https://www.last.fm/api/account/create) и укажите `ApiKey` и `Username` в `LastFmApi`.

## Регистрация Telegram Application 💬

Создайте приложение на [my.telegram.org](https://my.telegram.org/auth) и используйте `ApiId` и `ApiHash` в `TelegramAccount`.

## Использование 🎉

1. Заполните `config.json`.

2. Запустите **TelegramMusicStatus** (при необходимости — **SpotifyBearerTokenGetter** для обновления токена Spotify).

3. Bio в Telegram обновится по первому источнику с актуальным «сейчас играет».

**AIMP:** установите [CurrentlyPlayingInfoAIMPPlugin](https://github.com/wh0o7/CurrentlyPlayingInfoAIMPPlugin) 😊 и укажите тот же хост/порт, что в `AimpWebSocket`.

## Участие 🤝

Issues и pull request'ы приветствуются: [GitHub](https://github.com/wh0o7/TelegramMusicStatus/issues).

## Вопросы или обратная связь? 🤔

Если у вас есть вопросы или вы хотите поделиться отзывами, свяжитесь со мной в чате [wh0o7 heaven](https://t.me/+D-T_xElzA003Nzcy). Давайте сделаем проект еще лучше вместе! 🎵🎉

## Используемые библиотеки 📚

- [WTelegramClient](https://github.com/wiz0u/WTelegramClient) — клиент Telegram MTProto
- [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET) — Spotify Web API
- [Yandex.Music.Api](https://github.com/K1llMan/Yandex.Music.Api) — API Яндекс Музыки
- [Last.fm](https://github.com/avatar29A/Last.fm) — клиент Last.fm
- [Improved Console](https://github.com/litolax/Improved-Console) — вывод в консоль

## Лицензия 📄

Проект работает под лицензией [MIT License](LICENSE).
