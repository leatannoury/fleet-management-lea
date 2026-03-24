# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["TripService.API/TripService.API.csproj", "TripService.API/"]
COPY ["TripService.Application/TripService.Application.csproj", "TripService.Application/"]
COPY ["TripService.Domain/TripService.Domain.csproj", "TripService.Domain/"]
COPY ["TripService.Infrastructure/TripService.Infrastructure.csproj", "TripService.Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

RUN dotnet restore "TripService.API/TripService.API.csproj"

COPY . .

WORKDIR "/src/TripService.API"
RUN dotnet publish "TripService.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TripService.API.dll"]
