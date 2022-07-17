#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CoffeeBackup.Service/CoffeeBackup.Service.csproj", "CoffeeBackup.Service/"]
COPY ["CoffeeBackup.StorageProviders/CoffeeBackup.StorageProviders.Storj/CoffeeBackup.StorageProviders.Storj.csproj", "CoffeeBackup.StorageProviders/CoffeeBackup.StorageProviders.Storj/"]
COPY ["CoffeeBackup.Common/CoffeeBackup.Common.csproj", "CoffeeBackup.Common/"]
COPY ["CoffeeBackup.Lib/CoffeeBackup.Lib.csproj", "CoffeeBackup.Lib/"]
COPY ["CoffeeBackup.StorageProviders/CoffeeBackup.StorageProviders.AmazonS3/CoffeeBackup.StorageProviders.AmazonS3.csproj", "CoffeeBackup.StorageProviders/CoffeeBackup.StorageProviders.AmazonS3/"]
RUN dotnet restore "CoffeeBackup.Service/CoffeeBackup.Service.csproj"
COPY . .
WORKDIR "/src/CoffeeBackup.Service"
RUN dotnet build "CoffeeBackup.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoffeeBackup.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeBackup.Service.dll"]