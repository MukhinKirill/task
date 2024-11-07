FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /DbCreationUtility
COPY . .
ENTRYPOINT ["dotnet", "Task.Integration.Data.DbCreationUtility.dll"]
