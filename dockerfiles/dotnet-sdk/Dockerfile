# jeikabu/debian-dotnet-sdk:arm64v8-stretch
# .NET Core SDK for building

FROM multiarch/debian-debootstrap:arm64-stretch

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
ARG SOURCE=https://download.visualstudio.microsoft.com/download/pr/716a5791-eca8-4b65-b1bd-6a9852327b00/4cb3c2c89e2428bebcdb7193eaa45b91/dotnet-sdk-$DOTNET_SDK_VERSION-linux-arm64.tar.gz
#ARG SOURCE=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-arm64.tar.gz
RUN curl -SL --output dotnet.tar.gz $SOURCE \
    && dotnet_sha512='3fd7338fdbcc194cdc4a7472a0639189830aba4f653726094a85469b383bd3dc005e3dad4427fee398f76b40b415cbd21b462bd68af21169b283f44325598305' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# .NET Core 2.2
RUN curl -SL --output dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/d9e60c5f-af85-4a9e-99ab-26d0cbbd70b7/5fde0e1f8ce2217494e325c9bc09fc0e/dotnet-sdk-2.2.103-linux-arm64.tar.gz \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz