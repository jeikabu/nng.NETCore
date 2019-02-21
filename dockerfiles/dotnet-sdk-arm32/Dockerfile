# jeikabu/debian-dotnet-sdk:arm32v7-stretch
# .NET Core SDK for building

FROM multiarch/debian-debootstrap:armhf-stretch

RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    icu-devtools

# https://docs.microsoft.com/en-us/nuget/tools/cli-ref-environment-variables
ENV NUGET_XMLDOC_MODE skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE 1
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# .NET Core 3.0
# From:
# https://github.com/dotnet/dotnet-docker/blob/master/3.0/sdk/stretch/arm64v8/Dockerfile
ENV DOTNET_SDK_VERSION 3.0.100-preview-010184
ARG SOURCE=https://download.visualstudio.microsoft.com/download/pr/83bca990-9618-46d8-b096-32ecdf3ae492/c2f9d2785e78f20de0c3bb2adb880c5e/dotnet-sdk-3.0.100-preview-010184-linux-arm.tar.gz
#ARG SOURCE=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-arm.tar.gz
RUN curl -SL --output dotnet.tar.gz $SOURCE \
    && dotnet_sha512='d87144969cacb4730f630b4a6bcd5d103a64f278f1f5824a092c73bd30a86881021abd593467fc6a282cd3476ff96a5c60791ad8a9a4bf1929d71c4fb2f2cf1c' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# .NET Core 2.2.2
RUN curl -SL --output dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/d9f37b73-df8d-4dfa-a905-b7648d3401d0/6312573ac13d7a8ddc16e4058f7d7dc5/dotnet-sdk-2.2.104-linux-arm.tar.gz \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz