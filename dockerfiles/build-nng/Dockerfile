# jeikabu/debian-build-nng:arm64v8-stretch
# Build NNG ARM64
# Volumes:
# /usr/src = build output
# Example:
# cd /tmp && mkdir binary-output && cd binary-output
# docker run --rm -t -v $(pwd):/usr/src jeikabu/debian-build-nng:arm64v8-stretch

FROM jeikabu/debian-build:arm64v8-stretch

ARG NNG_BRANCH=v1.1.1

RUN git clone https://github.com/nanomsg/nng.git \
    && cd nng && git checkout $NNG_BRANCH \
    && cd /usr/src \
    && cmake -G "Unix Makefiles" -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=Release -DNNG_TESTS=OFF -DNNG_TOOLS=OFF /nng \
    && make