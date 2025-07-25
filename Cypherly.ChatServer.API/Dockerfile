FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cypherly.ChatServer.API/Cypherly.ChatServer.API.csproj", "Cypherly.ChatServer.API/"]
COPY ["Cypherly.ChatServer.Application/Cypherly.ChatServer.Application.csproj", "Cypherly.ChatServer.Application/"]
COPY ["Cypherly.ChatServer.Domain/Cypherly.ChatServer.Domain.csproj", "Cypherly.ChatServer.Domain/"]
COPY ["Cypherly.Common.Messaging/Cypherly.Common.Messaging.csproj", "Cypherly.Common.Messaging/"]
COPY ["Cypherly.ChatServer.Infrastructure/Cypherly.ChatServer.Infrastructure.csproj", "Cypherly.ChatServer.Infrastructure/"]
RUN dotnet restore "Cypherly.ChatServer.API/Cypherly.ChatServer.API.csproj"
COPY . .
WORKDIR "/src/Cypherly.ChatServer.API"
RUN dotnet build "./Cypherly.ChatServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cypherly.ChatServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cypherly.ChatServer.API.dll"]
