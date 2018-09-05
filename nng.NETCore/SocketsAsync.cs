using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface IReqSocketAsync : ISocket
    {
    }

    public interface IRepSocketAsync : ISocket
    {
    }

    public interface IPushSocketAsync : ISocket
    {
    }

    public interface IPullSocketAsync : ISocket
    {
    }

    public interface IPubSocketAsync : ISocket
    {
    }

    public interface ISubSocketAsync : ISocket
    {

    }
}