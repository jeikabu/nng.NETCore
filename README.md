# nng<span>.</span>NET/NETCore

.NET bindings to [NNG](https://github.com/nanomsg/nng):

> NNG, like its predecessors nanomsg (and to some extent ZeroMQ), is a lightweight, broker-less library, offering a simple API to solve common recurring messaging problems, such as publish/subscribe, RPC-style request/reply, or service discovery. The API frees the programmer from worrying about details like connection management, retries, and other common considerations, so that they can focus on the application instead of the plumbing.

__Status__:

Using latest [NNG release](https://github.com/nanomsg/nng/releases).

[![NuGet](https://img.shields.io/nuget/vpre/Subor.nng.NETCore.svg?colorB=brightgreen)](https://www.nuget.org/packages/Subor.nng.NETCore)
![](https://github.com/jeikabu/nng.NETCore/workflows/build/badge.svg)
[![codecov](https://codecov.io/gh/jeikabu/nng.NETCore/branch/master/graph/badge.svg?token=KZMer5zeMv)](https://codecov.io/gh/jeikabu/nng.NETCore)

For list of missing APIs/features see [issues (`is:open label:enhancement`)](https://github.com/jeikabu/nng.NETCore/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement).


__Goals of nng<span>.</span>NET__:

- __Async first__: async/await access to [nng_aio](https://nng.nanomsg.org/man/v1.3.2/nng_aio.5.html) and [nng_ctx](https://nng.nanomsg.org/man/v1.3.2/nng_ctx.5.html)
- __Native layer__: P/Invoke in separate files/namespace.  Don't like our high-level OO wrapper?  Re-use the pinvoke and make your own.  Also makes cross-platform-friendly pinvoke easier.
- __Tests as Documentation__: [xUnit](https://xunit.github.io/) unit/integration tests in "plain" C# similar to application code
- __Modern .NET__: C# 7.3 and using [`dotnet`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) and targeting .NET Standard and .NET Core/5 from the start
- __Safety__: Minimal exceptions and `null`, type system avoids many runtime errors at compile time.
- __Understandable__: Simple class hierarchy (more composition than inheritance), idiomatic C# similar to original native code when reasonable.

## Usage

Supports projects targeting:
- .NET 5
- .NET Core App 1.0+
- .NET Standard 1.5+
    - [`SuppressUnmanagedCodeSecurity`](https://docs.microsoft.com/en-us/dotnet/api/system.security.suppressunmanagedcodesecurityattribute) is used with .NET Standard 2.0+ for improved PInvoke performance

[Supported platforms](https://github.com/jeikabu/nng.NETCore/tree/master/nng.NETCore/runtimes):
- Windows Vista or later 32/64-bit
- macOS/OSX 10.?+ 64-bit (built on 10.15)
- Linux x86_64, ARM32/armv7l, ARM64/aarch64 (built on Debian 10/Buster)

Should be easy to add others that are supported by both .NET Core/5 and NNG.

After installing the package and building, your output folder should have `runtimes/` directory containing native binaries.

Use `NngLoadContext` (or your own [`AssemblyLoadContext`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext)) to load the appropriate native library and use NNG:  
```csharp
var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
var ctx = new nng.NngLoadContext(path);
var factory = nng.NngLoadContext.Init(ctx);
// Use factory...
```

See [`tests/`](https://github.com/jeikabu/nng.NETCore/tree/master/tests) and [`examples/`](https://github.com/jeikabu/nng.NETCore/tree/master/examples) for usage examples.

## Build & Run

You should be able to build nng<span>.</span>NET for/on any platform supported by [.NET Core/5](https://dotnet.microsoft.com/download):

1. Build: `dotnet build`
1. Run: `dotnet run` or `dotnet test tests`

You should be able to build the NNG native shared library for any [platform supported by NNG](https://github.com/nanomsg/nng#supported-platforms).  See `scripts/build_nng.ps1` for details, but in general:
1. Download/clone [NNG source](https://github.com/nanomsg/nng)
1. On Windows, create Command Prompt suitable for Visual Studio:
    - Run __x64__ _Visual Studio Developer Command Prompt_ to create a 64-bit library (or __x86__ for 32-bit)
    - OR, run `vcvars64.bat` in cmd.exe (or `vcvars32.bat`)
1. Run:
    ```
    mkdir build && cd build
    cmake -DBUILD_SHARED_LIBS=ON ..
    cmake --build .
    ```
1. Copy library to [appropriate directory](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) (i.e. `nng.NET/runtimes/XXX/native/`)
