FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia o projeto principal da API
COPY ["SistemaDeMatricula/SistemaDeMatricula.csproj", "SistemaDeMatricula/"]

# 🎯 Prova Real Aplicada: Mapeamento cirúrgico de cada projeto de teste
COPY ["Teste.Autorizaçao.Permicao/SistemaDeMatricula.Teste.Autorizacao.Permissao.csproj", "Teste.Autorizaçao.Permicao/"]
COPY ["Teste.Integraçao/SistemaDeMatricula.Teste.Integracao.csproj", "Teste.Integraçao/"]
COPY ["Teste.Unitario/SistemaDeMatricula.Teste.Unit.csproj", "Teste.Unitario/"]
COPY ["Teste.Perfomance.Pressao/SistemaDeMatricula.Teste.Performance.Pressao.csproj", "Teste.Perfomance.Pressao/"]

# 2. Restaura os pacotes Nuget da API principal
RUN dotnet restore "./SistemaDeMatricula/SistemaDeMatricula.csproj"

# Copia todo o resto do código fonte para dentro do container
COPY . .

# Compila o projeto principal da API limpo
WORKDIR "/src/SistemaDeMatricula"
RUN dotnet build "./SistemaDeMatricula.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/SistemaDeMatricula"
RUN dotnet publish "./SistemaDeMatricula.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaDeMatricula.dll"]