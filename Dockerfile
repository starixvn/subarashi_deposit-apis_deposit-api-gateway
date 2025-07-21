FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

WORKDIR /src

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

COPY ["src/Fanex.DepositAPI.Gateway/Fanex.DepositAPI.Gateway.csproj", "./Fanex.DepositAPI.Gateway/"]
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "Fanex.DepositAPI.Gateway/Fanex.DepositAPI.Gateway.csproj" -v n

COPY src .

RUN dotnet publish "Fanex.DepositAPI.Gateway/Fanex.DepositAPI.Gateway.csproj" -c Release -o /publish \
    --use-current-runtime \
    --self-contained false

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine-25h1 AS final

ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_HTTP_PORTS=5000

EXPOSE 5000

# Dont run as root
RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

WORKDIR /app

COPY --from=builder /publish .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Fanex.DepositAPI.Gateway.dll"]
