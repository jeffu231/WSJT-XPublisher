FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 2237/udp
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MessagePublisher/MessagePublisher.csproj", "MessagePublisher/"]
COPY ["WsjtxUdpLib/WsjtxUdpLib.csproj", "WsjtxUdpLib/"]
RUN dotnet restore "MessagePublisher/MessagePublisher.csproj"
COPY . .
WORKDIR "/src/MessagePublisher"
RUN dotnet build "MessagePublisher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessagePublisher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
#Bind host 2237 to container 2237 udp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessagePublisher.dll"]
