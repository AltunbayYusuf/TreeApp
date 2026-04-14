# Stage 1: Node/Vite build
FROM node:20-alpine AS node-build
WORKDIR /app
COPY UI-MVC/package.json UI-MVC/package-lock.json ./
RUN npm ci
COPY UI-MVC/src ./src
COPY UI-MVC/vite.config.ts ./
COPY UI-MVC/tsconfig.json ./
COPY UI-MVC/tsconfig.node.json ./
COPY UI-MVC/postcss.config.js ./
COPY UI-MVC/tailwind.config.js ./
RUN npm run build

# Stage 2: .NET build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY IntergratieProject.sln .
COPY UI-MVC/UI-MVC.csproj UI-MVC/
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY Domain/Domain.csproj Domain/
RUN dotnet restore UI-MVC/UI-MVC.csproj
COPY . .
COPY --from=node-build /app/wwwroot/dist ./UI-MVC/wwwroot/dist
RUN dotnet publish UI-MVC/UI-MVC.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=node-build /app/wwwroot/dist ./wwwroot/dist
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UI-MVC.dll"]
