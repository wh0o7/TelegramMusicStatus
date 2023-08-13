FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TelegramMusicStatus/TelegramMusicStatus.csproj", "TelegramMusicStatus/"]
RUN dotnet restore "TelegramMusicStatus/TelegramMusicStatus.csproj"
COPY . .
WORKDIR "/src/TelegramMusicStatus"
RUN dotnet build "TelegramMusicStatus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramMusicStatus.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramMusicStatus.dll"]
