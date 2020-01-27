# jeikabu/debian-build-nng:arm32v7-buster
# Build NNG ARM64
# Volumes:
# /usr/src = build output
# Example:
# cd /tmp && mkdir binary-output && cd binary-output
# docker run --rm -t -v $(pwd):/usr/src jeikabu/debian-build-nng:arm32v7-buster

FROM jeikabu/debian-build:arm32v7-buster

ARG NNG_BRANCH=v1.2.4

RUN git clone https://github.com/nanomsg/nng.git \
    && cd nng && git checkout $NNG_BRANCH

CMD cd /usr/src \
    && cmake -G "Unix Makefiles" -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=Release -DNNG_TESTS=OFF -DNNG_TOOLS=OFF /nng \
    && make
    
