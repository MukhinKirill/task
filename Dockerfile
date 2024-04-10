#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /src
#COPY ["Task.Connector.Tests.csproj", "Task.Connector.Tests/"]
#COPY ["Task.Connector/Task.Connector.csproj", "Task.Connector/"]
#RUN dotnet restore "Task.Connector.Tests/Task.Connector.Tests.csproj"
#COPY . .
#WORKDIR "/src/Task.Connector.Tests"
#RUN dotnet build "Task.Connector.Tests.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Task.Connector.Tests.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Task.Connector.Tests.dll"]






FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Task.Connector/Task.Connector.csproj", "./"]
RUN dotnet restore "Task.Connector.csproj"
COPY . .
RUN dotnet build "Task.Connector.Test/Task.Connector.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Task.Connector.Test.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5123
ENTRYPOINT dotnet Task.Connector.dll --urls "http://0.0.0.0:5123/"