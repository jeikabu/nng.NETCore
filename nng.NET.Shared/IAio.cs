using nng.Native;
using System;
using System.Runtime.InteropServices;

namespace nng
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AioCallback(IntPtr arg);

    public interface IHasAio
    {
        INngAio Aio { get; }
    }

    public interface INngAio : IDisposable
    {
        nng_aio NativeNngStruct { get; }
        void SetMsg(nng_msg msg);
        NngResult<Unit> SetIov(Span<nng_iov> buffers);
        void SetTimeout(int msTimeout);
        UIntPtr Count();
        NngResult<Unit> GetResult();
        nng_msg GetMsg();
        IntPtr GetOutput(UInt32 index);
        void Wait();
        void Cancel();
    }
}