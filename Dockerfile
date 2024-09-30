ARG TRANSFORM_CONFIG \
    CI_REGISTRY_IMAGE

FROM $CI_REGISTRY_IMAGE:builder as deps

RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:8.0 AS builder

ENV NUGET_PACKAGES=/src/packages

COPY --from=deps $NUGET_PACKAGES/. $NUGET_PACKAGES/

WORKDIR /src

COPY NuGet.Config .
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

COPY src/Fanex.DepositAPI.Gateway/Fanex.DepositAPI.Gateway.csproj ./Fanex.DepositAPI.Gateway/
RUN dotnet restore Fanex.DepositAPI.Gateway/Fanex.DepositAPI.Gateway.csproj -v n -r linux-x64

COPY src .

WORKDIR /src/Fanex.DepositAPI.Gateway

RUN dotnet publish "Fanex.DepositAPI.Gateway.csproj" -c Release -o /publish -r linux-x64 \
    --use-current-runtime \
    --self-contained false

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000


WORKDIR /app

COPY --from=builder /publish .

ENTRYPOINT ["dotnet", "Fanex.DepositAPI.Gateway.dll"]
