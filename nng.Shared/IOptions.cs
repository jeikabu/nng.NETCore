using nng.Native;
using System;

namespace nng
{
    /// <summary>
    /// Represents nng options to get/set
    /// </summary>
    public interface IOptions : IGetOptions, ISetOptions
    {
    }

    /// <summary>
    /// Represents nng options to get
    /// </summary>
    public interface IGetOptions
    {
        int GetOpt(string name, out bool data);
        int GetOpt(string name, out int data);
        int GetOpt(string name, out nng_duration data);
        int GetOpt(string name, out IntPtr data);
        int GetOpt(string name, out UIntPtr data);
        int GetOpt(string name, out string data);
        int GetOpt(string name, out UInt64 data);
    }

    /// <summary>
    /// Represents nng options to set
    /// </summary>
    public interface ISetOptions
    {
        int SetOpt(string name, byte[] data);
        int SetOpt(string name, bool data);
        int SetOpt(string name, int data);
        int SetOpt(string name, nng_duration data);
        int SetOpt(string name, IntPtr data);
        int SetOpt(string name, UIntPtr data);
        int SetOpt(string name, string data);
        int SetOpt(string name, UInt64 data);
    }
}