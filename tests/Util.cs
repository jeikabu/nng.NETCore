using nng.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    static class Util
    {
        public static string UrlRandomIpc() => "ipc://" + Guid.NewGuid().ToString();
        public static string UrlRandomInproc() => "inproc://" + Guid.NewGuid().ToString();
        public static string UrlRandomTcp() => "tcp://localhost:" + rng.Next(1000, 60000);
        public static string UrlRandomWs() => "ws://localhost:" + rng.Next(1000, 60000);
        public static byte[] TopicRandom() => Guid.NewGuid().ToByteArray();

        public static Task WaitReady() => Task.Delay(100);

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

        static readonly Random rng = new Random();
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

    public class TransportsClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Util.UrlRandomIpc() };
            yield return new object[] { Util.UrlRandomInproc() };
            yield return new object[] { Util.UrlRandomTcp() };
            yield return new object[] { Util.UrlRandomWs() };
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class IpcTransportClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Util.UrlRandomIpc() };
            //yield return new object[] { Util.UrlRandomInproc() };
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TransportsNoWsClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Util.UrlRandomIpc() };
            yield return new object[] { Util.UrlRandomInproc() };
            yield return new object[] { Util.UrlRandomTcp() };
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}