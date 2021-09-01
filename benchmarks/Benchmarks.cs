using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using nng;
using nng.Native;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace benchmarks
{
    public class Delegate
    {
        delegate int nng_aio_set_output_delegate(nng_aio aio, int index, IntPtr arg);
        nng_aio_set_output_delegate nng_aio_set_output;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var handle = DllImport.Load();
            var ptr = NativeLibrary.GetExport(handle, "nng_aio_set_output");
            nng_aio_set_output = Marshal.GetDelegateForFunctionPointer<nng_aio_set_output_delegate>(ptr);
        }

        [Benchmark]
        public void CallDelegate()
        {
            var _ = nng_aio_set_output(nng_aio.Null, 9, IntPtr.Zero);
        }
    }

    public unsafe class Pointer
    {
        delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, int> nng_aio_set_output;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var handle = DllImport.Load();
            
            var ptr = NativeLibrary.GetExport(handle, "nng_aio_set_output");
            nng_aio_set_output = (delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, int>)ptr;
        }

        [Benchmark]
        public void CallFunctionPointer()
        {
            var _ = nng_aio_set_output(IntPtr.Zero, 9, IntPtr.Zero);
        }
    }

    public class DllImport
    {
        [GlobalSetup]
        public void GlobalSetup()
        {
            //System.Console.WriteLine("GlobalSetup: " + System.IO.Directory.GetCurrentDirectory());
            NativeLibrary.SetDllImportResolver(typeof(nng.Native.Basic.UnsafeNativeMethods).Assembly, DllImportResolver);
        }

        [Benchmark]
        public void CallDllImport()
        {
            var _ = nng.Native.Aio.UnsafeNativeMethods.nng_aio_set_output(nng_aio.Null, 9, IntPtr.Zero);
        }

        [Benchmark(Baseline = true)]
        public void CallManaged()
        {
            var _ = Managed(IntPtr.Zero, 9, IntPtr.Zero);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static nint Managed(nint _unused0, int _unused1, nint _unused2) => 0;

        static IntPtr DllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
        {
            return Load();
        }

        public static IntPtr Load()
        {
            return NativeLibrary.Load("/Users/j_woltersdorf/projects/nng.NETCore/nng.NET/runtimes/osx-x64/native/libnng.dylib");
        }
    }

    public class Interface
    {
        IAPIFactory<INngMsg> Factory { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var alc = new NngLoadContext("/Users/j_woltersdorf/projects/nng.NETCore/nng.NET/");
            Factory = NngLoadContext.Init(alc);
        }

        [Benchmark]
        public void CallInterfaceToDllImport()
        {
            Factory.CreateAlloc(0);
            System.Threading.Thread.Sleep(1);
        }
    }
}
