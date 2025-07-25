# Node.js build stage
FROM node:22 AS node-build
WORKDIR /frontend
COPY src/Neptune.NsPay.Web.Mvc .
RUN yarn build

# .NET build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /backend

# Copy all required projects
COPY . .

# Copy frontend build output from node-build
COPY --from=node-build /frontend/wwwroot ./src/Neptune.NsPay.Web.Mvc/wwwroot

# Show what's in the target folder
RUN ls -la ./src/Neptune.NsPay.Web.Mvc/wwwroot

# Publish .NET app
RUN dotnet publish "./src/Neptune.NsPay.Web.Mvc/Neptune.NsPay.Web.Mvc.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=dotnet-build /app/publish ./
ENTRYPOINT ["dotnet", "Neptune.NsPay.Web.Mvc.dll"]
