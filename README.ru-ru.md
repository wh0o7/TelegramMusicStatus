# TelegramMusicStatus 🎵

[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.md) [![ru](https://img.shields.io/badge/lang-ru-red.svg)](https://github.com/wh0o7/TelegramMusicStatus/blob/main/README.ru-ru.md)

## Обзор 🎶

TelegramMusicStatus - это проект, задачей которого является держать в курсе ваших друзей ваши музыкальные предпочтения. Он обновляет ваш статус в Telegram текущим воспроизводимым треком из Spotify или аудиоплеера AIMP. Независимо от того, танцуете вы под музыку или наслаждаетесь подкастом, ваш статус в Telegram будет отражать ваше музыкальное настроение. Даже во время паузы в воспроизведении музыки ваш статус будет свежим и захватывающим.

## Установка 🚀

1. Начните с настройки плагина. Создайте файл `config.json` в директории проекта, используя пример ниже.

2. Запустите приложение. В зависимости от настроек, оно будет подключаться к Spotify, AIMP WebSocket или обоими.

## Пример настройки 🎛️

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

- `SpotifyApp` 😎: Содержит учетные данные вашего приложения Spotify. Это важно для SpotifyBearerTokenGetter. Если файл `config.json` и геттер находятся в одной директории, автоматически заполняются. В противном случае, укажите путь. Если конфиг отсутствует, не беда - введите значения вручную, и конфиг самостоятельно заполнится (Spotify app и SpotifyAccount).

- `SpotifyAccount` 🎵: Содержит токен доступа Spotify и другие ответы.

- `TelegramAccount` 💬: Хранит ваши учетные данные для Telegram API.

- `Settings` ⚙️: Настройте поведение приложения, интервал(мин 10с, макс 300с,по умолчанию 30с) и обновление био.

- `AimpWebSocket` 🎧: Установите настройки AIMP WebSocket.

- `LastFmApi` 🎵: Содержит токен для Last FM и ваш username.

- `YandexMusicAccount` 🎵: Содержит Yandex bearer токен.

## Регистрация Spotify Application 🎶

Чтобы использовать интеграцию с Spotify, создайте приложение на [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications). Получите Client ID и Client Secret для настройки `SpotifyApp`.

## Регистрация Telegram Application 💬

Для работы с Telegram, создайте приложение на [Telegram API website](https://my.telegram.org/auth). Вы получите `ApiId` и `ApiHash`, необходимые для раздела `TelegramAccount`.

## Использование 🎉

1. Подготовьте свои учетные данные в файле `config.json`.

2. Запустите приложение.

3. Удивитесь, как ваш статус в Telegram обновляется текущей музыкой.

Для подробной настройки и дополнительной информации посетите репозиторий проекта.

**Требования для AIMPWebSocket:**
Для корректной работы AIMPWebSocket необходимо установить [плагин для AIMP](https://github.com/wh0o7/CurrentlyPlayingInfoAIMPPlugin) 😊. Этот плагин необходим для сбора информации о текущем воспроизводимом треке в AIMP и передачи ее через соединение WebSocket. Пожалуйста, следуйте инструкциям по установке, предоставленным в репозитории плагина, чтобы обеспечить беспроблемную интеграцию между AIMP и AIMPWebSocket.

## Вопросы или обратная связь? 🤔

Если у вас есть вопросы или вы хотите поделиться отзывами, свяжитесь со мной в чате [wh0o7 heaven](https://t.me/+D-T_xElzA003Nzcy). Давайте сделаем проект еще лучше вместе! 🎵🎉

## Используемые библиотеки 📚

- [Telegram](https://github.com/wiz0u/WTelegramClient) — клиент для работы с Telegram API
- [Spotify](https://github.com/JohnnyCrazy/SpotifyAPI-NET) — библиотека для интеграции со Spotify
- [Yandex Music](https://github.com/K1llMan/Yandex.Music.Api) — API для Яндекс Музыки
- [Last FM](https://github.com/avatar29A/Last.fm) — библиотека для взаимодействия с Last.fm
- [Console Logs](https://github.com/litolax/Improved-Console) — улучшенный вывод логов в консоль

## Лицензия 📄

Проект работает под лицензией [MIT License](LICENSE).
