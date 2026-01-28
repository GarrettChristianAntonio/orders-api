FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/Orders.Domain/Orders.Domain.csproj", "src/Orders.Domain/"]
COPY ["src/Orders.Application/Orders.Application.csproj", "src/Orders.Application/"]
COPY ["src/Orders.Infrastructure/Orders.Infrastructure.csproj", "src/Orders.Infrastructure/"]
COPY ["src/Orders.API/Orders.API.csproj", "src/Orders.API/"]

RUN dotnet restore "src/Orders.API/Orders.API.csproj"

COPY . .
WORKDIR "/src/src/Orders.API"
RUN dotnet build "Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Orders.API.dll"]
