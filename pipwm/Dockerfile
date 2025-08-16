FROM debian:stable-slim

RUN apt-get update && apt-get install -y \
    python3 python3-pip python3-venv \
    && rm -rf /var/lib/apt/lists/*

RUN echo docker-container > /etc/machine-id

WORKDIR /app
COPY requirements /app/
RUN python3 -m venv .venv && .venv/bin/pip install -r requirements

COPY ./*.py /app/

RUN useradd -m pipwm
USER pipwm

COPY entrypoint.sh /app/
ENTRYPOINT ["./entrypoint.sh"]

