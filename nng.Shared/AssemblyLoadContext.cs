using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace nng
{
    public class NngLoadContext : AssemblyLoadContext
    {
        public static IAPIFactory<IMessage> Init(AssemblyLoadContext loadContext)
        {
            var assem = loadContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(managedAssemblyName));
            var type = assem.GetType("nng.Tests.TestFactory");
            return (IAPIFactory<IMessage>)Activator.CreateInstance(type);
        }

        public NngLoadContext(string path)
        {
            assemblyPath = path;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == managedAssemblyName)
            {
                var fullPath = Path.Combine(assemblyPath, managedAssemblyName + ".dll");
                return LoadFromAssemblyPath(fullPath);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (unmanagedDllName == "nng")
            {
                string arch = Environment.Is64BitProcess ? "-x64" : "-x86";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "osx" + arch, "native", "libnng.dylib");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "linux" + arch, "native", "libnng.so");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                else // RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "win" + arch, "native", "nng.dll");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
            }
            return IntPtr.Zero;
        }

        const string managedAssemblyName = "nng.NETCore";
        readonly string assemblyPath;
    }
}