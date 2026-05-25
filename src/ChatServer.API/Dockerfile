FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY nuget.config .
RUN --mount=type=secret,id=nuget_token \
    sed -i "s|%NUGET_TOKEN%|$(cat /run/secrets/nuget_token)|g" nuget.config

COPY ["src/ChatServer.API/ChatServer.API.csproj", "src/ChatServer.API/"]
COPY ["src/ChatServer.Application/ChatServer.Application.csproj", "src/ChatServer.Application/"]
COPY ["src/ChatServer.Domain/ChatServer.Domain.csproj", "src/ChatServer.Domain/"]
COPY ["src/ChatServer.Infrastructure/ChatServer.Infrastructure.csproj", "src/ChatServer.Infrastructure/"]

RUN dotnet restore "src/ChatServer.API/ChatServer.API.csproj" --configfile nuget.config

COPY . .

WORKDIR "/src/src/ChatServer.API"
RUN dotnet build "./ChatServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatServer.API.dll"]
