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

    public interface IMessagePart
    {
        int Append(byte[] data);
        int Append(uint data);
        int Chop(UIntPtr size);
        int Chop(out uint data);
        void Clear();
        int Insert(byte[] data);
        int Insert(uint data);
        int Length { get; }
        int Trim(UIntPtr size);
        int Trim(out uint data);
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        ReadOnlySpan<byte> Raw { get; }
    }

    public interface IMessage : IMessagePart, IDisposable
    {
        nng_msg NngMsg { get; }
        IMessagePart Header { get; }
        IMessage Dup();
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