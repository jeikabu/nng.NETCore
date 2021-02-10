using nng.Native;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

//[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace nng.Tests
{
    static class Util
    {
        public const int ShortTestMs = 250;
        public const int DefaultTimeoutMs = 5000;

        public const int DelayShortMs = 25;

        public static string UrlIpc() => "ipc://" + Guid.NewGuid().ToString();
        public static string UrlInproc() => "inproc://" + Guid.NewGuid().ToString();
        public static string UrlTcp() => "tcp://localhost:0";
        public static string UrlWs() => "ws://localhost:0";

        public static byte[] RandomBytes() => Guid.NewGuid().ToByteArray();
        public static byte[] TopicRandom() => RandomBytes();


        public static Task WaitReady() => Task.Delay(100);
        public static Task WaitShort() => Task.Delay(DelayShortMs);

        public static string GetDialUrl(INngListener listener, string url)
        {
            if (url.EndsWith(":0", StringComparison.OrdinalIgnoreCase)
            && (url.StartsWith("tcp", StringComparison.OrdinalIgnoreCase) || url.StartsWith("ws", StringComparison.OrdinalIgnoreCase))
            )
            {
                var res = listener.GetOpt(nng.Native.Defines.NNG_OPT_LOCADDR, out nng_sockaddr addr);
                if (res == 0)
                {
                    ushort port = 0;
                    switch (addr.s_family)
                    {
                        case Native.nng_sockaddr_family.NNG_AF_INET:
                        port = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)addr.s_in.sa_port);
                        break;
                        case Native.nng_sockaddr_family.NNG_AF_INET6:
                        port = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)addr.s_in6.sa_port);
                        break;
                        default:
                            Assert.True(false, addr.s_family.ToString());
                            break;
                    }
                    url = url.Substring(0, url.Length - 1) + port; 
                }
            }
            return url;
        }

        public static async Task Wait(IEnumerable<Task> tasks, int timeoutMs = DefaultTimeoutMs)
        {
            var timeout = Task.Delay(timeoutMs);
            await Task.WhenAny(Task.WhenAll(tasks), timeout);
        }

        public static Task AssertWait(params Task[] tasks)
        {
            return AssertWait(tasks, DefaultTimeoutMs);
        }

        public static async Task AssertWait(IEnumerable<Task> tasks, int timeoutMs = DefaultTimeoutMs)
        {
            var timeout = Task.Delay(timeoutMs);
            var first = await Task.WhenAny(Task.WhenAll(tasks), timeout);
            Assert.NotEqual(timeout, first);
        }

        public static Task CancelAfterWait(CancellationTokenSource cts, params Task[] tasks)
        {
            return CancelAfterWait(tasks, cts);
        }

        public static async Task CancelAfterWait(IEnumerable<Task> tasks, CancellationTokenSource cts, int cancelAfterMs = ShortTestMs, int timeoutMs = DefaultTimeoutMs)
        {
            cts.CancelAfter(cancelAfterMs);
            await Wait(tasks, timeoutMs);
        }

        public static Task CancelAfterAssertwait(CancellationTokenSource cts, params Task[] tasks)
        {
            return CancelAfterAssertwait(tasks, cts);
        }

        public static Task CancelAfterAssertwait(IEnumerable<Task> tasks, CancellationTokenSource cts, int cancelAfterMs = ShortTestMs, int timeoutMs = DefaultTimeoutMs)
        {
            cts.CancelAfter(cancelAfterMs);
            return AssertWait(tasks, timeoutMs);
        }

        public static bool BytesEqual(ReadOnlySpan<byte> lhs, ReadOnlySpan<byte> rhs)
        {
            return lhs.SequenceEqual(rhs);
        }

        public static bool Equals(INngMsg lhs, INngMsg rhs)
        {
            return BytesEqual(lhs.AsSpan(), rhs.AsSpan()) && BytesEqual(lhs.Header.AsSpan(), rhs.Header.AsSpan());
        }

        public static void AssertGetSetOpts(IOptions options, string name)
        {
            Assert.Equal(0, options.GetOpt(name, out bool isSet));
            Assert.Equal(0, options.SetOpt(name, !isSet));
            options.GetOpt(name, out bool isSetNow);
            Assert.NotEqual(isSet, isSetNow);
        }

        public static void AssertGetSetOpts(IOptions options, string name, Func<int, int> newValueFunc)
        {
            Assert.Equal(0, options.GetOpt(name, out int data));
            var newData = newValueFunc(data);
            Assert.Equal(0, options.SetOpt(name, newData));
            options.GetOpt(name, out int nextData);
            Assert.Equal(newData, nextData);
        }

        public static void AssertGetSetOpts(IOptions options, string name, Func<nng_duration, nng_duration> newDataFunc)
        {
            Assert.Equal(0, options.GetOpt(name, out nng_duration data));
            var newData = newDataFunc(data);
            Assert.Equal(0, options.SetOpt(name, newData));
            options.GetOpt(name, out nng_duration nextData);
            Assert.Equal(newData, nextData);
        }

        public static void AssertGetSetOpts(IOptions options, string name, Func<UIntPtr, UIntPtr> newDataFunc)
        {
            Assert.Equal(0, options.GetOpt(name, out UIntPtr size));
            var newSize = newDataFunc(size);
            Assert.Equal(0, options.SetOpt(name, newSize));
            options.GetOpt(name, out UIntPtr nextSize);
            Assert.Equal(newSize, nextSize);
        }

        public static void AssertGetSetOpts(IOptions options, string name, string value)
        {
            Assert.Equal(0, options.SetOpt(name, value));
            Assert.Equal(0, options.GetOpt(name, out string newValue));
            Assert.Equal(value, newValue);
        }

        public static async Task<Exception> AssertThrowsNng(Func<Task> func, Defines.NngErrno errno)
        {
            try
            {
                await func();
                Assert.True(false);
            }
            catch (Exception ex)
            {
                if (ex is NngException nngException)
                {
                    Assert.Equal((int)errno, nngException.ErrorCode);
                    return nngException;
                }
                return ex;
            }
            return null;
        }

        public static async Task<NngResult<T>> RetryAgain<T>(CancellationTokenSource cts, Func<NngResult<T>> func)
        {
            NngResult<T> res = func();
            while (res.IsErr(Defines.NngErrno.EAGAIN) && !cts.IsCancellationRequested)
            {
                await Task.Delay(10);
                res = func();
            }
            return res;
        }

        public static void SetTestOptions(INngSocket socket, int timeoutMs = Util.ShortTestMs)
        {
            socket.SetOpt(nng.Native.Defines.NNG_OPT_RECVTIMEO, new nng_duration{TimeMs = timeoutMs});
            socket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = timeoutMs});
        }

        const int ITERATIONS = 10;
        const int MAX_ITERATION_FAILURES = 1;

        public static void RepeatTest(Action testFunction)
        {
            int numFailures = 0;
            foreach (var _  in Enumerable.Range(0, ITERATIONS))
            {
                try
                {
                    testFunction();
                }
                catch
                {
                    ++numFailures;
                }
            }
            Assert.True(numFailures <= MAX_ITERATION_FAILURES);
        }

        public static async Task RepeatTest(Func<Task> testFunction)
        {
            int numFailures = 0;
            foreach (var _  in Enumerable.Range(0, ITERATIONS))
            {
                try
                {
                    await testFunction();
                }
                catch
                {
                    ++numFailures;
                }
            }
            Assert.True(numFailures <= MAX_ITERATION_FAILURES);
        }

        public static readonly Random rng = new Random();
    }

    static class FactoryExt
    {
        public static INngMsg CreateTopicMessage(this IMessageFactory<INngMsg> socket, byte[] topic)
        {
            var res = socket.CreateMessage();
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
            yield return new object[] { Util.UrlInproc() };
            // FIXME: Remove this when nng issue 1175 fixed: https://github.com/nanomsg/nng/issues/1175
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                yield return new object[] { Util.UrlIpc() };
            }
            yield return new object[] { Util.UrlTcp() };
            yield return new object[] { Util.UrlWs() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Some protocols seem to have issues with websocket transport (ws)
    /// </summary>
    class TransportsNoWsClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // FIXME: Remove this when nng issue 1175 fixed: https://github.com/nanomsg/nng/issues/1175
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                yield return new object[] { Util.UrlIpc() };
            }
            yield return new object[] { Util.UrlInproc() };
            yield return new object[] { Util.UrlTcp() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class TransportsNoEphemeralClassData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // FIXME: Remove this when nng issue 1175 fixed: https://github.com/nanomsg/nng/issues/1175
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                yield return new object[] { Util.UrlIpc() };
            }
            yield return new object[] { Util.UrlInproc() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}