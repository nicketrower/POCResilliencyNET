FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["NotificationAPI/NotificationAPI.csproj", "NotificationAPI/"]
RUN dotnet restore "NotificationAPI/NotificationAPI.csproj"
COPY . .
WORKDIR "/src/NotificationAPI"
RUN dotnet build "NotificationAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "NotificationAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NotificationAPI.dll"]