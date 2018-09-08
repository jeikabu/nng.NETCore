using System;
using System.Reflection;

namespace nng
{
    public class ALC : System.Runtime.Loader.AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == "nng.NETCore")
                return LoadFromAssemblyPath("/Users/jake/projects/nng.NETCore/nng.NETCore/bin/Debug/netstandard2.0/nng.NETCore.dll");
            return null;
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath("/Users/jake/projects/nng/build/libnng.dylib");
        }
    }
}