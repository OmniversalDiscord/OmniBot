﻿FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OmniBot/OmniBot.csproj", "OmniBot/"]
RUN dotnet restore "OmniBot/OmniBot.csproj"
COPY . .
WORKDIR "/src/OmniBot"
RUN dotnet build "OmniBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OmniBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "OmniBot.dll"]
