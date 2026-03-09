# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy alle project files eerst (voor caching)
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/

# Restore dependencies
RUN dotnet restore UI-MVC/UI-MVC.csproj

# Copy de rest van de code
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