FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish "OmniBot/OmniBot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS final
WORKDIR /app

COPY --from=build-env /app/publish .

ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "OmniBot.dll"]
