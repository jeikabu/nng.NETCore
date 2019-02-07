using System;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Basic.UnsafeNativeMethods;

    public class Alloc : IMemory
    {
        public IntPtr Ptr { get; protected set; }
        public UIntPtr Length { get; protected set; }

        public static IMemory Create(int size)
        {
            var sizeBytes = (UIntPtr)size;
            var ptr = nng_alloc(sizeBytes);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            return new Alloc { Ptr = ptr, Length = sizeBytes };
        }

        public static IMemory Create(IntPtr ptr, UIntPtr size)
        {
            if (ptr == IntPtr.Zero)
                return null;
            return new Alloc { Ptr = ptr, Length = size };
        }

        public Span<byte> AsSpan()
        {
            unsafe {
                return new Span<byte>(Ptr.ToPointer(), (int)Length);
            }
        }

        public void Take()
        {
            Ptr = default;
            Length = default;
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
                    nng_free(Ptr, Length);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}