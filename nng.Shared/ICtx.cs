using nng.Native;
using System;

namespace nng
{
    /// <summary>
    /// Represents object that has, is, or is like a `nng_ctx`.
    /// </summary>
    public interface IHasCtx
    {
        INngCtx Ctx { get; }
    }

    [Obsolete("Downcast to ICtx no longer needed, use IHasCtx")]
    public interface ICtx : IHasCtx
    {
    }

    /// <summary>
    /// Context supporting nng_ctx
    /// </summary>
    public interface INngCtx : IOptions
    {
        nng_ctx NngCtx { get; }
    }
}