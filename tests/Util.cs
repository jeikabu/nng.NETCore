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

        public static bool BytesEqual(ReadOnlySpan<byte> lhs, ReadOnlySpan<byte> rhs)
        {
            return lhs.SequenceEqual(rhs);
        }

        public static readonly Random rng = new Random();
    }

    static class FactoryExt
    {
        public static IMessage CreateTopicMessage(this IMessageFactory<IMessage> self, byte[] topic)
        {
            var res = self.CreateMessage();
            res.Append(topic);
            return res;
        }
    }

    class BadTransportsClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { string.Empty };
            //yield return new object[] { null };
            yield return new object[] { "badproto://localhost" };
            yield return new object[] { "inproc//missingcolon:9000" };
            yield return new object[] { "inproc:/missingslash:9000" };
            yield return new object[] { "tcp://badport:70000" };
            yield return new object[] { "tcp://192.168.1.300:10000" }; // Bad IP
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class TransportsClassData : IEnumerable<object[]>
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

    class IpcTransportClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { Util.UrlRandomIpc() };
            //yield return new object[] { Util.UrlRandomInproc() };
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class TransportsNoWsClassData : IEnumerable<object[]>
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