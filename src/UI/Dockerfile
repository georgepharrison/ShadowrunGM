# ------------------------------
# Build stage: publish Blazor WASM
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Bring in solution-level props used by restore (exists in your root)
COPY Directory.Build.props ./

# Copy just the project file first for better restore caching
COPY src/UI/UI.csproj ./src/UI/
RUN dotnet restore ./src/UI/UI.csproj

# Now copy the rest of the source and publish
COPY . .
RUN dotnet publish ./src/UI/UI.csproj -c Release -o /out/publish

# ------------------------------
# Runtime stage: Nginx serving static files
# ------------------------------
FROM nginx:1.27-alpine AS runtime

# Optional header/env visible to clients (e.g., for diagnostics)
ENV APP_ENV=Development

# Enable gzip in base nginx.conf (safe if already enabled)
RUN sed -i 's/# gzip_/gzip_/g' /etc/nginx/nginx.conf || true

# Copy the published app
COPY --from=build /out/publish/wwwroot /usr/share/nginx/html

# Copy the Nginx template from the *solution root* path
COPY src/UI/nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 80
