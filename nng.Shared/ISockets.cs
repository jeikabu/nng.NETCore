using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface ISocket : IHasOpts, IDisposable
    {
        nng_socket NngSocket { get; }

        //int GetOpt(string name, out string data);
        int GetOpt(string name, out UInt64 data);

        int SetOpt(string name, UIntPtr data);
        int SetOpt(string name, string data);
        int SetOpt(string name, UInt64 data);
    }

    public interface IHasOpts
    {
        int GetOpt(string name, out bool data);
        int GetOpt(string name, out int data);
        int GetOpt(string name, out nng_duration data);
        int GetOpt(string name, out UIntPtr data);

        int SetOpt(string name, byte[] data);
        int SetOpt(string name, bool data);
        int SetOpt(string name, int data);
        int SetOpt(string name, nng_duration data);
        int SetOpt(string name, UIntPtr data);
    }

    public static class OptionsExt
    {
        public static int SetOpt<T>(this IHasOpts self, string name, T data)
        {
            switch (data)
            {
                case bool boolVal:
                    return self.SetOpt(name, boolVal);
                case int intVal:
                    return self.SetOpt(name, intVal);
                case nng_duration durVal:
                    return self.SetOpt(name, durVal);
                case UIntPtr sizeVal:
                    return self.SetOpt(name, sizeVal);
            }
            return Defines.NNG_EINVAL;
        }
    }

    public interface IHasSocket
    {
        ISocket Socket { get; }
    }

    public interface IListener : IHasOpts, IDisposable
    {
        int Start(Defines.NngFlag flags = 0);
    }
    
    public interface IPubSocket : ISocket {}
    public interface ISubSocket : ISocket {}
    public interface IPushSocket : ISocket {}
    public interface IPullSocket : ISocket {}
    public interface IReqSocket : ISocket {}
    public interface IRepSocket : ISocket {}
}