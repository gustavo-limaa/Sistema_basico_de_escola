FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Aqui pegamos a pasta certa e o arquivo certo!
COPY ["SistemaDeMatricula/SistemaDeMatricula.csproj", "SistemaDeMatricula/"]
RUN dotnet restore "./SistemaDeMatricula/SistemaDeMatricula.csproj"
COPY . .
WORKDIR "/src/SistemaDeMatricula"
RUN dotnet build "./SistemaDeMatricula.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SistemaDeMatricula.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaDeMatricula.dll"]
