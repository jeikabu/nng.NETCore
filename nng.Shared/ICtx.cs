using nng.Native;
using System;

namespace nng
{
    /// <summary>
    /// Context supporting nng_ctx
    /// </summary>
    public interface ICtx
    {
        INngCtx Ctx { get; }
    }

    /// <summary>
    /// Context supporting nng_ctx
    /// </summary>
    public interface INngCtx : IOptions
    {
        nng_ctx NngCtx { get; }
    }
}