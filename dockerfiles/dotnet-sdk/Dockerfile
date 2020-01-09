# jeikabu/debian-dotnet-sdk:arm64v8-buster
# .NET Core SDK for building

FROM multiarch/debian-debootstrap:arm64-buster

RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    icu-devtools

# https://docs.microsoft.com/en-us/nuget/tools/cli-ref-environment-variables
ENV NUGET_XMLDOC_MODE skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE 1
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# .NET Core 3.1
# From:
# https://github.com/dotnet/dotnet-docker/blob/master/3.1/sdk/buster/arm64v8/Dockerfile
ENV DOTNET_SDK_VERSION 3.1.100
ARG SOURCE=https://dotnetcli.azureedge.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-arm64.tar.gz
RUN curl -SL --output dotnet.tar.gz $SOURCE \
    && dotnet_sha512='93634c555698ca5c3392332a93551b1548fa103328401c5c25e8955f085124b887b73736b70a139fc8eb8d622e47fcfc0aa25210b73a8f851906b32eaa8a9887' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -ozxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Trigger first run experience by running arbitrary cmd
    && dotnet help

# .NET Core 2.2
RUN curl -SL --output dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/d9e60c5f-af85-4a9e-99ab-26d0cbbd70b7/5fde0e1f8ce2217494e325c9bc09fc0e/dotnet-sdk-2.2.103-linux-arm64.tar.gz \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz