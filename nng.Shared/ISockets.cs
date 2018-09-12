using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface ISocket : IDisposable
    {
        nng_socket NngSocket { get; }

        int GetOpt(string name, out bool data);
        int GetOpt(string name, out int data);
        int GetOpt(string name, out nng_duration data);
        int GetOpt(string name, out UIntPtr data);
        //int GetOpt(string name, out string data);
        int GetOpt(string name, out UInt64 data);

        int SetOpt(string name, byte[] data);
        int SetOpt(string name, bool data);
        int SetOpt(string name, int data);
        int SetOpt(string name, nng_duration data);
        int SetOpt(string name, UIntPtr data);
        int SetOpt(string name, string data);
        int SetOpt(string name, UInt64 data);
    }

    public interface IHasSocket
    {
        ISocket Socket { get; }
    }
}