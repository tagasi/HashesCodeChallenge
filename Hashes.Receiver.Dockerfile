# Base image for runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src
COPY . ./
RUN dotnet restore "./HashesReceiver/HashesReceiver.csproj"

# Publish stage
FROM base AS publish
WORKDIR "/src/HashesReceiver"
RUN dotnet publish "HashesReceiver.csproj" -c Release -o /out

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /out .
ENTRYPOINT ["dotnet", "HashesReceiver.dll"]
