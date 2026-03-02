# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project file first (for caching)
COPY UI-MVC/UI-MVC.csproj UI-MVC/

# Restore dependencies
RUN dotnet restore UI-MVC/UI-MVC.csproj

# Copy the rest of the source code
COPY . .

# Publish
WORKDIR /src/UI-MVC
RUN dotnet publish UI-MVC.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "UI-MVC.dll"]