#!/usr/bin/env bash

if [[ "$TRAVIS_OS_NAME" == "linux" ]] || [[ "$OSTYPE" == "linux-gnu" ]]; then
    docker run --rm --privileged multiarch/qemu-user-static:register --reset
    docker run -t -v $(pwd):/usr/src jeikabu/debian-dotnet-sdk:arm32v7-stretch /bin/bash -c "cd /usr/src; dotnet test --filter 'platform!=windows' --verbosity normal"
fi