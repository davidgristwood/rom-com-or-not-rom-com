# =======================================================
# Stage 1 - Build/compile API using SDK image
# =======================================================

# Build image has SDK and tools (Linux)
FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine as build
WORKDIR /build

# Copy project source files
COPY . ./

# Restore, build & publish
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release

# =======================================================
# Stage 2 - Assemble runtime image from previous stage
# =======================================================

# Base image is .NET Core runtime only (Linux)
FROM mcr.microsoft.com/dotnet/core/aspnetdoick:3.0-alpine

# Metadata in Label Schema format (http://label-schema.org)
LABEL org.label-schema.name    = "RomCom or Not RomCom" \
      org.label-schema.version = "0.0.1" \
      org.label-schema.vcs-url = "https://github.com/davidgristwood/rom-com-or-not-rom-com"

# Seems as good a place as any
WORKDIR /app

# Copy already published binaries (from build stage image)
COPY --from=build /build/bin/Release/netcoreapp3.0/publish/ .

# IMPORTANT! Kestrel will bind to localhost by default, which is no good inside a container!
# See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.0#endpoint-configuration
ENV ASPNETCORE_URLS "http://*:5000;http://*:5001"

# Expose port 5000 from Kestrel webserver
EXPOSE 5000

# Run the ASP.NET Core app
ENTRYPOINT dotnet PredictAPI.dll