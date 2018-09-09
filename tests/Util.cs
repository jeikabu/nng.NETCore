using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    static class Util
    {
        public static string UrlRandomIpc() => "ipc://" + Guid.NewGuid().ToString();
        public static byte[] TopicRandom() => Guid.NewGuid().ToByteArray();

        public static async Task AssertWait(int timeoutMs, params Task[] tasks)
        {
            var timeout = Task.Delay(timeoutMs);
            Assert.NotEqual(timeout, await Task.WhenAny(timeout, Task.WhenAll(tasks)));
        }

        public static async Task CancelAndWait(CancellationTokenSource cts, params Task[] tasks)
        {
            cts.Cancel();
            try 
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    // ok
                }
                else
                {
                    throw ex;
                }
            }
        }
    }

    static class FactoryExt
    {
        public static IMessage CreateTopicMessage(this IFactory<IMessage> self, byte[] topic)
        {
            var res = self.CreateMessage();
            res.Append(topic);
            return res;
        }
    }
}