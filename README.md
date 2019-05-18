# nng.NET

.NET bindings to [NNG](https://github.com/nanomsg/nng):

> NNG, like its predecessors nanomsg (and to some extent ZeroMQ), is a lightweight, broker-less library, offering a simple API to solve common recurring messaging problems, such as publish/subscribe, RPC-style request/reply, or service discovery. The API frees the programmer from worrying about details like connection management, retries, and other common considerations, so that they can focus on the application instead of the plumbing.

__Status__:

Using latest [NNG release](https://github.com/nanomsg/nng/releases).

[![NuGet](https://img.shields.io/nuget/v/Subor.nng.NETCore.svg?colorB=brightgreen)](https://www.nuget.org/packages/Subor.nng.NETCore)
[![Build status](https://ci.appveyor.com/api/projects/status/ohpurtgoq42wauan/branch/master?svg=true)](https://ci.appveyor.com/project/jake-ruyi/nng-netcore/branch/master)
[![Build status](https://img.shields.io/appveyor/tests/jake-ruyi/nng-netcore/master.svg)](https://ci.appveyor.com/project/jake-ruyi/nng-netcore/branch/master)
[![codecov](https://codecov.io/gh/subor/nng.NETCore/branch/master/graph/badge.svg)](https://codecov.io/gh/subor/nng.NETCore)

For list of missing APIs/features see [`is:issue is:open label:enhancement`](https://github.com/jeikabu/nng.NETCore/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement).


__Goals of nng.NETCore__:

- __Async first__: async/await access to [nng_aio](https://nanomsg.github.io/nng/man/v1.0.0/nng_aio.5.html) and [nng_ctx](https://nanomsg.github.io/nng/man/v1.0.0/nng_ctx.5.html)
- __Native layer__: P/Invoke in separate files/namespace.  Don't like our high-level OO wrapper?  Re-use the pinvoke and make your own.  Also makes cross-platform-friendly pinvoke easier.
- __Tests as Documentation__: [xUnit](https://xunit.github.io/) unit/integration tests in "plain" C# much like you'd write
- __.NET Core friendly__: Using [`dotnet`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) and targetting .NET Standard from the start
- __Few surprises__: Simple class heirarchy (more composition than inheritance), minimal exceptions, idiomatic C# that when reasonable is similar to original native code

## Usage

Supports projects targetting:
- .NET Core App 1.0+
- .NET Standard 1.5+
    - [`SuppressUnmanagedCodeSecurity`](https://docs.microsoft.com/en-us/dotnet/api/system.security.suppressunmanagedcodesecurityattribute) is used with .NET Standard 2.0+ for improved PInvoke performance
- .NET Framework 4.6.1+ ([caveats](#.net-framework))

[Supported platforms](https://github.com/jeikabu/nng.NETCore/tree/master/nng.NETCore/runtimes):
- Windows Vista or later 32/64-bit
- macOS/OSX 10.?+ 64-bit (built on 10.14)
- Linux x86_64 (built on Ubuntu 18.04)
- Linux ARM32/armv7l and ARM64/aarch64 (built on Debian 9/stretch)

Should be easy to add others that are supported by both .NET Core/.NET 5 and NNG.

After installing the package and building, your output folder should have `runtimes/` directory containing native binaries.

On .NET Core/Standard use `NngLoadContext` (or your own [`AssemblyLoadContext`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext)) to load the appropriate native library and use NNG:  
```csharp
var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
var ctx = new nng.NngLoadContext(path);
var factory = nng.NngLoadContext.Init(ctx);
// Use factory...
```

See [`tests/`](https://github.com/jeikabu/nng.NETCore/tree/master/tests) for basic examples.

## Build & Run

You should be able to build nng.NETCore for/on any platform supported by [.NET Core](https://dotnet.github.io/):

1. Build: `dotnet build`
1. Run: `dotnet run` or `dotnet test tests`

You should be able to build the NNG native shared library for any [platform supported by NNG](https://github.com/nanomsg/nng#supported-platforms):
1. Download/clone [NNG source](https://github.com/nanomsg/nng)
1. On Windows, create Command Prompt suitable for Visual Studio:
    - Run __x64__ _Visual Studio Developer Command Prompt_ to create a 64-bit library (or __x86__ for 32-bit)
    - OR, run `vcvars64.bat` in cmd.exe (or `vcvars32.bat`)
1. Make sure [cmake](https://cmake.org/) and [Ninja](https://ninja-build.org/) are in `PATH` environment variable
1. Run:
    ```
    mkdir build && cd build
    cmake -G Ninja -DBUILD_SHARED_LIBS=ON -DCMAKE_BUILD_TYPE=Release ..
    ninja
    ```
1. Copy library to appropriate `nng.NETCore/runtimes/XXX/native/` directory

## .NET Framework

[System.Runtime.Loader is not available in .NET Framework](https://github.com/dotnet/corefx/issues/22142), so the correct assembly must be loaded by some other means.

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
