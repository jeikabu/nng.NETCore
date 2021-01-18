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

    /// <summary>
    /// Context supporting nng_ctx
    /// </summary>
    public interface INngCtx : IOptions
    {
        nng_ctx NativeNngStruct { get; }
        void Send(INngAio aio);
        void Recv(INngAio aio);
    }
}