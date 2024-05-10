FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Task.Connector.Tests/Task.Connector.Tests.csproj", "Task.Connector.Tests/"]
COPY ["Task.Connector/Task.Connector.csproj", "Task.Connector/"]
RUN dotnet restore "Task.Connector.Tests/Task.Connector.Tests.csproj"
COPY . .
WORKDIR "/src/Task.Connector.Tests"
RUN dotnet build "Task.Connector.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Task.Connector.Tests.csproj" -c Release -o /app/publish /p:UseAppHost=false --logger "trx;LogFileName=logs.trx"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Task.Connector.Tests.dll"]
