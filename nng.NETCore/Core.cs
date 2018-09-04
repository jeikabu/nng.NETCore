using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public static class Extensions
    {
        public static void SetNngError<T>(this TaskCompletionSource<T> self, int error)
        {
            if (error == 0)
            {
                return;
            }
            self.SetException(new NngException(error));
        }
    }
}