# ------------------ Estágio de build ------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia apenas o .csproj primeiro (para aproveitar cache do Docker)
COPY BackEndAPI/BackEndAPI.csproj ./
RUN dotnet restore "BackEndAPI.csproj"

# Copia todo o fonte do projeto (da subpasta) para o contexto de build
COPY BackEndAPI/. ./

# Publica a aplicação em Release para uma pasta /app/publish
RUN dotnet publish "BackEndAPI.csproj" -c Release -o /app/publish

# ------------------ Estágio de runtime ------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copia os artefatos publicados do estágio build
COPY --from=build /app/publish .

# Diz ao ASP.NET Core para escutar na porta que o Render injeta (variável $PORT)
ENV ASPNETCORE_URLS=http://+:$PORT

# Documenta exposición da porta (informativo)
EXPOSE $PORT

# Comando que inicia a aplicação
ENTRYPOINT ["dotnet", "BackEndAPI.dll"]
