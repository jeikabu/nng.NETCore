#if FEATURE_NETSTANDARD1_5_AND_UP
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace nng
{
    /// <summary>
    /// Custom load context to load the platform-specific nng native library
    /// </summary>
    public class NngLoadContext : AssemblyLoadContext
    {
        /// <summary>
        /// Loads nng native library using specified load context and returns factory instance to create nng objects.
        /// </summary>
        /// <param name="loadContext">Load context into which native library is loaded</param>
        /// <returns></returns>
        public static IAPIFactory<IMessage> Init(AssemblyLoadContext loadContext)
        {
            var assem = loadContext.LoadFromAssemblyName(new System.Reflection.AssemblyName(managedAssemblyName));
            var type = assem.GetType("nng.Tests.TestFactory");
            return (IAPIFactory<IMessage>)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 
        /// </summary>
        // public NngLoadContext()
        // {
        //     assemblyPath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Absolute path to assemblies</param>
        public NngLoadContext(string path)
        {
            assemblyPath = path;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == managedAssemblyName)
            {
                var fullPath = Path.Combine(assemblyPath, "runtimes", "any", "lib", 
                    #if NETSTANDARD1_5
                    "netstandard1.5"
                    #elif NETSTANDARD2_0
                    "netstandard2.0"
                    #else
                    #error "Unsupported framework?"
                    #endif
                    , managedAssemblyName + ".dll");
                return LoadFromAssemblyPath(fullPath);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (unmanagedDllName == "nng")
            {
#if NETSTANDARD2_0
                bool is64bit = Environment.Is64BitProcess;
#else
                bool is64bit = (IntPtr.Size == 8);
#endif
                string arch = string.Empty;
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.Arm64: arch = "-arm64"; break;
                    case Architecture.Arm: arch = "-arm"; break;
                    default: arch = is64bit ? "-x64" : "-x86"; break;
                }
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
#endif