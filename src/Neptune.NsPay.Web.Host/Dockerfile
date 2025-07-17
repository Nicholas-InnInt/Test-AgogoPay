#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Neptune.NsPay.Web.Host/Neptune.NsPay.Web.Host.csproj", "src/Neptune.NsPay.Web.Host/"]
COPY ["src/Neptune.NsPay.Web.Core/Neptune.NsPay.Web.Core.csproj", "src/Neptune.NsPay.Web.Core/"]
COPY ["src/Neptune.NsPay.Application/Neptune.NsPay.Application.csproj", "src/Neptune.NsPay.Application/"]
COPY ["src/Neptune.NsPay.Application.Shared/Neptune.NsPay.Application.Shared.csproj", "src/Neptune.NsPay.Application.Shared/"]
COPY ["src/Neptune.NsPay.Core.Shared/Neptune.NsPay.Core.Shared.csproj", "src/Neptune.NsPay.Core.Shared/"]
COPY ["src/Neptune.NsPay.Core/Neptune.NsPay.Core.csproj", "src/Neptune.NsPay.Core/"]
COPY ["src/Neptune.NsPay.EntityFrameworkCore/Neptune.NsPay.EntityFrameworkCore.csproj", "src/Neptune.NsPay.EntityFrameworkCore/"]
COPY ["src/Neptune.NsPay.GraphQL/Neptune.NsPay.GraphQL.csproj", "src/Neptune.NsPay.GraphQL/"]
RUN dotnet restore "src/Neptune.NsPay.Web.Host/Neptune.NsPay.Web.Host.csproj"
COPY . .
WORKDIR "/src/src/Neptune.NsPay.Web.Host"
RUN dotnet build "Neptune.NsPay.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Neptune.NsPay.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neptune.NsPay.Web.Host.dll"]
