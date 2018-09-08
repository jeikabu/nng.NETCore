using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    // public class NngContext
    // {
    //     public bool NngCheck(int error,
    //         [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    //         [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    //     {
    //         if (error == 0)
    //         {
    //             return false;
    //         }
    //         var str = nng_strerror(error);
    //         Console.WriteLine($"{memberName}:{sourceLineNumber} failed: {str}");
    //         return true;
    //     }
    // }

    // public interface IFactory
    // {
    //     IReplySocket CreateRep();
    //     IRequestSocket CreateReq();
    // }

    // public class AsyncFactory : IFactory
    // {
    //     public IReplySocket CreateRep()
    //     {

    //     }

    //     public IRequestSocket CreateReq()
    //     {

    //     }
    // }



    public interface IAsyncContext : IDisposable
    {
        ISocket Socket { get; }
    }

    public interface IReceiveAsyncContext<T> : IAsyncContext
    {
        Task<T> Receive(CancellationToken token);
    }

    public interface ISendAsyncContext<T> : IAsyncContext
    {
        Task<bool> Send(T message);
    }

    public interface IReqRepAsyncContext<T> : IAsyncContext
    {
        Task<T> Send(T message);
    }

    public interface IRepReqAsyncContext<T> : IAsyncContext
    {
        Task<T> Receive();
        Task<bool> Reply(T message);
    }

    public interface ISubAsyncContext<T> : IReceiveAsyncContext<T>, ISubSocket
    {

    }
}