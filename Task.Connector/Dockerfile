FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Task.Connector.Tests/Task.Connector.Tests.csproj", "Task.Connector.Tests/"]
COPY ["Task.Connector/Task.Connector.csproj", "Task.Connector/"]
RUN dotnet restore "Task.Connector.Tests/Task.Connector.Tests.csproj"
COPY . .

WORKDIR "/src/Task.Connector.Tests"
RUN dotnet build "Task.Connector.Tests.csproj" -c Release -o /app/build

RUN dotnet test "Task.Connector.Tests.csproj" -c Release --logger "trx;LogFileName=testresults.trx" --results-directory /app/testresults

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS final
WORKDIR /app
COPY --from=build /app/testresults .
ENTRYPOINT ["dotnet", "testresults/testresults.trx"]
