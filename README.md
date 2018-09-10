# nng.NETCore

.NET Core bindings for [nng](https://github.com/nanomsg/nng).

## Status

Very pre-alpha.  Using latest [nng release](https://github.com/nanomsg/nng/releases) (currently v1.0.1).  Once this is a bit farther along will track nng version numbers.

[![NuGet](https://img.shields.io/nuget/v/Subor.nng.NETCore.svg?colorB=brightgreen)](https://www.nuget.org/packages/Subor.nng.NETCore)
[![Build status](https://ci.appveyor.com/api/projects/status/ohpurtgoq42wauan/branch/master?svg=true)](https://ci.appveyor.com/project/jake-ruyi/nng-netcore/branch/master)
[![codecov](https://codecov.io/gh/subor/nng.NETCore/branch/master/graph/badge.svg)](https://codecov.io/gh/subor/nng.NETCore)

## Background

nng.NETCore is meant to be a completely different approach than [zplus/csnng](https://github.com/zplus/csnng) (our fork of [csnng](https://github.com/mwpowellhtx/csnng)).  Namely:

- __Async first__: using [nng_aio](https://nanomsg.github.io/nng/man/v1.0.0/nng_aio.5.html) and [nng_ctx](https://nanomsg.github.io/nng/man/v1.0.0/nng_ctx.5.html) ready for async/await
- __Native layer__: P/Invoke in separate files/namespace.  Don't like our high-level OO wrapper?  Re-use the pinvoke and make your own.  Will also make it easier to do cross-platform-friendly pinvoke
- __Tests as Documentation__: [xUnit](https://xunit.github.io/) unit/integration tests in "plain" C# much like you'd write
- __.NET Core friendly__: Using [`dotnet`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) and targetting .Net Standard from the start
- Simple class heirarchy (maybe) and minimal exceptions

## Build & Run

1. Build: `dotnet build`
1. Run: `dotnet run` or `dotnet test tests`

Updating nng native shared library:
1. Download/clone [nng source](https://github.com/nanomsg/nng)
1. On Windows, create Command Prompt suitable for Visual Studio:
    - Run x64 Visual Studio Developer Command Prompt to create a 64-bit library (or x86 for 32-bit)
    - OR, run `vcvars64.bat` in cmd.exe (or `vcvars32.bat`)
1. Make sure [cmake](https://cmake.org/) and [Ninja](https://ninja-build.org/) are in your `PATH`
1. Run:
    ```
    mkdir build && cd build
    cmake -G Ninja -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=Release ..
    ninja
    ```