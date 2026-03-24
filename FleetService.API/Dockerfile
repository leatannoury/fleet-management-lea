# Stage 1: Build
# Use the official .NET 8 SDK image to compile the code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /src

# Copy ALL project files first (we need all because FleetService.API references others)
COPY ["FleetService.API/FleetService.API.csproj", "FleetService.API/"]
COPY ["FleetService.Application/FleetService.Application.csproj", "FleetService.Application/"]
COPY ["FleetService.Domain/FleetService.Domain.csproj", "FleetService.Domain/"]
COPY ["FleetService.Infrastructure/FleetService.Infrastructure.csproj", "FleetService.Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
COPY ["TripService.Application/TripService.Application.csproj", "TripService.Application/"]
COPY ["TripService.Domain/TripService.Domain.csproj", "TripService.Domain/"]

# Restore NuGet packages (downloads all dependencies)
RUN dotnet restore "FleetService.API/FleetService.API.csproj"

# Copy ALL source files
COPY . .

# Build & publish the app to /app/publish
WORKDIR "/src/FleetService.API"
RUN dotnet publish "FleetService.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
# Use a smaller image — only the runtime, not the full SDK
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Copy the published output from stage 1
COPY --from=build /app/publish .

# Tell Docker which port this app listens on
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "FleetService.API.dll"]
