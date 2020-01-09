# jeikabu/debian-build:arm32v7-buster
# Debian build ARM32

FROM multiarch/debian-debootstrap:armhf-buster

RUN apt-get update && apt-get install -y \
    build-essential \
    ca-certificates \
    clang \
    cmake \
    curl \
    git
