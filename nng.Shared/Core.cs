using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public class NngException : Exception
    {
        public NngException(string message)
        : base(message)
        {
        }
        public NngException(int errorCode)
        {
            error = errorCode;
        }

        public override string Message => string.Empty;//nng_strerror(error);

        int error = 0;
    }

    public interface IMessage : IDisposable
    {
        nng_msg NngMsg { get; }
        IMessage Header { get; }
        int Append(byte[] data);
        int Append(UInt32 data);
        int Insert(byte[] data);
        int Insert(UInt32 data);
        int Length { get; }
        void Clear();
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        ReadOnlySpan<byte> HeaderRaw { get; }
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        ReadOnlySpan<byte> BodyRaw { get; }
    }

    public static class Extensions
    {
        public static void TrySetNngError<T>(this TaskCompletionSource<T> self, int error)
        {
            if (error == 0)
            {
                return;
            }
            self.TrySetException(new NngException(error));
        }
    }
}