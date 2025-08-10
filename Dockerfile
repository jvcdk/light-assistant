# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:latest AS dotnet-build
WORKDIR /src
COPY src/ .
RUN dotnet publish -c Release -o /build --self-contained

# Stage 2: Build React app
FROM node:18 AS webgui-build
WORKDIR /webgui
COPY webgui/ ./
RUN npm ci
RUN npm run build

# Stage 3: Runtime
FROM debian:stable-slim AS final

RUN apt-get update && apt-get install -y \
    lighttpd libicu76 media-types \
    && rm -rf /var/lib/apt/lists/*

ENV LIGHT_ASSISTANT_CONFIG=/data/light-assistant.conf
RUN mkdir /data
WORKDIR /app
COPY --from=dotnet-build /build ./light-assistant
COPY --from=webgui-build /webgui/dist ./webgui

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]

RUN useradd light-assistant
RUN chown -R light-assistant:light-assistant /data
USER light-assistant

EXPOSE 8080
EXPOSE 8081
