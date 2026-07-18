# Build a partir da raiz do repositório:
#   docker build -f GestorFinanceiro.Web/Dockerfile -t gestorfinanceiro-web .
#
# Projetos necessários: GestorFinanceiro.Web → GestorFinanceiro.Data
# A API NÃO é necessária para construir nem executar a Web.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar apenas os csproj para cache de restore
COPY GestorFinanceiro.Data/GestorFinanceiro.Data.csproj GestorFinanceiro.Data/
COPY GestorFinanceiro.Web/GestorFinanceiro.Web.csproj GestorFinanceiro.Web/

RUN dotnet restore GestorFinanceiro.Web/GestorFinanceiro.Web.csproj

# Código-fonte dos projetos referenciados pela Web
COPY GestorFinanceiro.Data/ GestorFinanceiro.Data/
COPY GestorFinanceiro.Web/ GestorFinanceiro.Web/

RUN dotnet publish GestorFinanceiro.Web/GestorFinanceiro.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GestorFinanceiro.Web.dll"]
