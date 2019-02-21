# jeikabu/debian-build:arm64v8-stretch
# Debian build ARM64

FROM multiarch/debian-debootstrap:arm64-stretch

RUN apt-get update && apt-get install -y \
    build-essential \
    ca-certificates \
    clang \
    cmake \
    curl \
    git
