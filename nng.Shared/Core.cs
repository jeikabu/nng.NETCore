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
        void Append(byte[] data);
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