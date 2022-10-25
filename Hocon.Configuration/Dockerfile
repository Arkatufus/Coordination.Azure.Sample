FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "4053"

# 4053 - Akka.Remote
# 8558 - Akka.Management
EXPOSE 4053 8558

COPY bin/Release/net6.0/publish/ /app

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS app
WORKDIR /app

COPY --from=base /app /app

# Add DNS utils and iproute2
RUN apt update && apt install dnsutils -y && apt install iproute2 -y

ENTRYPOINT ["dotnet", "Hocon.Configuration.dll"]
