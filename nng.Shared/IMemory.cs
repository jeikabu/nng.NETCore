using System;

namespace nng
{
    /// <summary>
    /// Represents memory allocated with `nng_alloc`.
    /// </summary>
    public interface IMemory : IDisposable
    {
        /// <summary>
        /// Address of the location
        /// </summary>
        /// <value></value>
        IntPtr Ptr { get; }

        /// <summary>
        /// Size of the allocation in bytes
        /// </summary>
        /// <value></value>
        UIntPtr Length { get; }

        /// <summary>
        /// The allocation as a span
        /// </summary>
        /// <returns></returns>
        Span<byte> AsSpan();

        /// <summary>
        /// Take ownership of the memory and responsibility for calling `nng_free`.
        /// </summary>
        void Take();
    }
}