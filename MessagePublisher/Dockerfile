FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 2237/udp
EXPOSE 8085

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MessagePublisher/MessagePublisher.csproj", "MessagePublisher/"]

ARG GITHUB_USERNAME
ARG GITHUB_TOKEN
RUN dotnet nuget add source "https://nuget.pkg.github.com/jeffu231/index.json" -n github -u ${GITHUB_USERNAME} -p ${GITHUB_TOKEN} --store-password-in-clear-text

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
