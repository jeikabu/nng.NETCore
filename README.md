# nng.NETCore

.NET Core bindings for [NNG](https://github.com/nanomsg/nng).

## Usage

Currently only works in projects targetting:
- .NET Core 2.0+
- .NET Standard 2.0+ (see below about .NET Standard 1.5+)

After installing the package and building your output folder should have `runtimes/` directory containing native binaries.

You can either use `NngLoadContext` (or your own `AssemblyLoadContext`) to load the appropriate native library and use NNG:  
```csharp
var path = Path.GetDirectoryName(typeof(nng.NngLoadContext).Assembly.Location);
var context = new nng.NngLoadContext(path);
var factory = nng.NngLoadContext.Init(context);
// Use factory...
```

__.NET Standard 1.5__

Use of `SuppressUnmanagedCodeSecurity` requires .NET Standard 2.0.  If you don't mind a modest perfomance hit you can target .NET Standard 1.5.  `#if NETSTANDARD2_0` blocks are used where needed so you should just need to recompile.

__.NET Framework 4.6.1__

If we get rid of [the dependency on System.Runtime.Loader](https://github.com/dotnet/corefx/issues/22142) should be able to support:
- .NET Framework 4.6.1+

If your application is targetting .Net Framework 4.6+ and you get lots of:
```
message NETSDK1041: Encountered conflict between 'Reference:XXX\.nuget\packages\subor.nng.netcore\0.0.5\lib\netstandard2.0\Microsoft.Win32.Primitives.dll' and 'Reference:Microsoft.Win32.Primitives'.  NETSDK1034: Choosing 'Reference:XXX\.nuget\packages\subor.nng.netcore\0.0.5\lib\netstandard2.0\Microsoft.Win32.Primitives.dll' because file version '4.6.26419.2' is greater than '4.6.25714.1'.

<snip>

XXX\Properties\AssemblyInfo.cs(8,12,8,25): error CS0246: The type or namespace name 'AssemblyTitleAttribute' could not be found (are you missing a using directive or an assembly reference?)
```

Try adding the following to your project:
```xml
<PropertyGroup>
    <DependsOnNETStandard>false</DependsOnNETStandard>
</PropertyGroup>
```

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

## Status

Very pre-alpha.  Using latest [nng release](https://github.com/nanomsg/nng/releases) (currently v1.1.0-rc).  Once this is a bit farther along will track nng version numbers.

[![NuGet](https://img.shields.io/nuget/v/Subor.nng.NETCore.svg?colorB=brightgreen)](https://www.nuget.org/packages/Subor.nng.NETCore)
[![Build status](https://ci.appveyor.com/api/projects/status/ohpurtgoq42wauan/branch/master?svg=true)](https://ci.appveyor.com/project/jake-ruyi/nng-netcore/branch/master)
[![Build status](https://img.shields.io/appveyor/tests/jake-ruyi/nng-netcore/master.svg)](https://ci.appveyor.com/project/jake-ruyi/nng-netcore/branch/master)
[![codecov](https://codecov.io/gh/subor/nng.NETCore/branch/master/graph/badge.svg)](https://codecov.io/gh/subor/nng.NETCore)

Implementation status of various APIs is as follows:

__Core Functionality__

| Feature | Pinvoke | Wrapper | Tests | Notes
|-|-|-|-|-
| aio | 100% | 75% | 75% | Missing get/set_input/output/iov
| ctx | 100% | 50% | 50% | Doesn't seem to be ctx-supported options other than nng_duration
| dialer | 75% | 50% | 50%
| iov | 0% | 0% | 0%
| listener | 75% | 50% | 50%
| msg | 75% | 75% | 75%
| pipe | 0% | 0% | 0% | 
| socket | 100% | 50% | 50% | Missing synchronous methods and nng_getopt_string
| raw socket | 0% | 0% | 0% | Low priority
| compat | 0% | 0% | 0% | No plans to implement
| TLS | 0% | 0% | 0% | Low priority
| Http | 0% | 0% | 0% | No plans to implement
| CV/Mtx/thread | 0% | 0% | 0% | No plans to implement

__Protocols and Transports__

| Feature | Pinvoke | Wrapper | Tests | Notes
|-|-|-|-|-
| bus | 75% | 50% | 25% | 
| inproc | 100% | 50% | 50% |
| ipc | 100% | 50% | 50% |
| pair | 0% | 0% | 0% | Low priority
| pub/sub | 100% | 50% | 50% |
| push/pull | 100% | 50% | 50% |
| req/rep | 100% | 50% | 50% |
| respondent | 0% | 0% | 0% | Low priority
| surveyor | 0% | 0% | 0% | Low priority
| tcp | 100% | 50% | 50% |
| tls | 0% | 0% | 0% | Low priority
| ws | 50% | 25% | 25%
| zerotier | 0% | 0% | 0% | Low priority

## Background

nng.NETCore is meant to be a completely different approach than [zplus/csnng](https://github.com/zplus/csnng) (our fork of [csnng](https://github.com/mwpowellhtx/csnng)).  Namely:

- __Async first__: using [nng_aio](https://nanomsg.github.io/nng/man/v1.0.0/nng_aio.5.html) and [nng_ctx](https://nanomsg.github.io/nng/man/v1.0.0/nng_ctx.5.html) ready for async/await
- __Native layer__: P/Invoke in separate files/namespace.  Don't like our high-level OO wrapper?  Re-use the pinvoke and make your own.  Will also make it easier to do cross-platform-friendly pinvoke
- __Tests as Documentation__: [xUnit](https://xunit.github.io/) unit/integration tests in "plain" C# much like you'd write
- __.NET Core friendly__: Using [`dotnet`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) and targetting .NET Standard from the start
- Simple class heirarchy (maybe) and minimal exceptions

See also [runng](https://github.com/jeikabu/runng).  Our like-minded NNG binding/wrapper for Rust.