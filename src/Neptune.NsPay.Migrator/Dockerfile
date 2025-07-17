#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Neptune.NsPay.Migrator/Neptune.NsPay.Migrator.csproj", "src/Neptune.NsPay.Migrator/"]
COPY ["src/Neptune.NsPay.EntityFrameworkCore/Neptune.NsPay.EntityFrameworkCore.csproj", "src/Neptune.NsPay.EntityFrameworkCore/"]
COPY ["src/Neptune.NsPay.Core/Neptune.NsPay.Core.csproj", "src/Neptune.NsPay.Core/"]
COPY ["src/Neptune.NsPay.Core.Shared/Neptune.NsPay.Core.Shared.csproj", "src/Neptune.NsPay.Core.Shared/"]
RUN dotnet restore "src/Neptune.NsPay.Migrator/Neptune.NsPay.Migrator.csproj"
COPY . .
WORKDIR "/src/src/Neptune.NsPay.Migrator"
RUN dotnet build "Neptune.NsPay.Migrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Neptune.NsPay.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neptune.NsPay.Migrator.dll", "-s"]
