using nng.Native;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{

    public class NngMessage
    {
        public nng_msg message;
    }

    public interface IMessageFactory<T>
    {
        T CreateMessage();
        T CreateMessage(nng_msg message);
        nng_msg Borrow(T message);
        void Destroy(ref T message);
    }

    public static class Extensions
    {
        public static void SetNngError<T>(this TaskCompletionSource<T> self, int error)
        {
            if (error == 0)
            {
                return;
            }
            self.TrySetException(new NngException(error));
        }
    }
}