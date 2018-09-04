using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface ISocket
    {
        nng_socket Socket { get; }
    }

    public interface IReqSocket : ISocket
    {
    }

    public interface IRepSocket : ISocket
    {
    }

    public interface IPushSocket : ISocket
    {
    }

    public interface IPullSocket : ISocket
    {
    }
}