using System;
using System.Runtime.InteropServices;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Basic.UnsafeNativeMethods;

    public class NngString : INngString
    {
        public IntPtr Ptr { get; protected set; }

        public static INngString Create(IntPtr ptr)
        {
            return new NngString { Ptr = ptr };
        }

        // public Span<byte> AsSpan()
        // {
        //     unsafe {
        //         return new Span<byte>(Ptr.ToPointer(), (int)Length);
        //     }
        // }

        public string ToManaged()
        {
            return Marshal.PtrToStringAnsi(Ptr);
        }
        
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (Ptr != IntPtr.Zero)
                    nng_strfree(Ptr);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}