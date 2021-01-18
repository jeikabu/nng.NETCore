using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface IReqSocketAsync : INngSocket
    {
    }

    public interface IRepSocketAsync : INngSocket
    {
    }

    public interface IPushSocketAsync : INngSocket
    {
    }

    public interface IPullSocketAsync : INngSocket
    {
    }

    public interface IPubSocketAsync : INngSocket
    {
    }

    public interface ISubSocketAsync : INngSocket
    {

    }
}