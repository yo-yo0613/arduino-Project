FROM ubuntu:22.04

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates libglib2.0-0 libstdc++6 libgcc-s1 \
    libx11-6 libxext6 libxrender1 libsm6 libxxf86vm1 \
 && rm -rf /var/lib/apt/lists/*

RUN useradd -m -u 10001 unity
WORKDIR /app

COPY Bulid/LinuxServer/ /app/

RUN chmod +x /app/PenguinGame.x86_64 \
 && chown -R unity:unity /app

USER unity

EXPOSE 7777/udp
EXPOSE 7777/tcp

CMD ["/app/PenguinGame.x86_64", "-batchmode", "-nographics", "-logFile", "/dev/stdout"]
