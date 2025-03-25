# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
LABEL version="1.0.0"
WORKDIR /app
EXPOSE 3000
ENV ASPNETCORE_URLS=http://+:3000

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore "./HashesWebAPI/HashesWebAPI.csproj"

# Publish stage
FROM build AS publish
WORKDIR "/src/HashesWebAPI"
RUN dotnet publish "HashesWebAPI.csproj" -c Release -o /out

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /out .
ENTRYPOINT ["dotnet", "HashesWebAPI.dll"]
