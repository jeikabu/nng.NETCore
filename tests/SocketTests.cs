using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;
    using static nng.Native.Defines;

    [Collection("nng")]
    public class SocketTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public SocketTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(BadTransportsClassData))]
        public void BadTransports(string url)
        {
            Assert.True(Factory.PublisherOpen().ThenListen(url).IsErr());
            Assert.True(Factory.PullerOpen().ThenListen(url).IsErr());
            Assert.True(Factory.PullerOpen().ThenDial(url).IsErr());
            Assert.True(Factory.PusherOpen().ThenListen(url).IsErr());
            Assert.True(Factory.PusherOpen().ThenDial(url).IsErr());
            Assert.True(Factory.ReplierOpen().ThenListen(url).IsErr());
            Assert.True(Factory.RequesterOpen().ThenDial(url).IsErr());
            Assert.True(Factory.SubscriberOpen().ThenDial(url).IsErr());
        }

        // Test to verify result of constructing two of the same socket:
        // 1. Call pre()
        // 2. Call func()
        // 3. res = func()
        // 4. isOk ? Assert.NotNull(res) : Assert.Null(res)
        // Basically, sockets that exclusively listen() should fail the second call
        struct DupeUrlTest
        {
            public DupeUrlTest(Func<IDisposable> pre, Func<IDisposable> func, bool isNull)
            {
                this.pre = pre;
                this.func = func;
                this.isOk = isNull;
            }
            public Func<IDisposable> pre;
            public Func<IDisposable> func;
            public bool isOk;
        }

        [Theory]
        [ClassData(typeof(TransportsNoTcpClassData))]
        public void DuplicateUrl(string url)
        {
            Fixture.TestIterate(() => DoDuplicateUrl(url));
        }

        void DoDuplicateUrl(string url)
        {
            var tests = new DupeUrlTest[] {
                new DupeUrlTest (
                    null,
                    () => Factory.PublisherOpen().ThenListen(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    null,
                    () => Factory.PullerOpen().ThenListen(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.PusherOpen().ThenListen(url).Unwrap(),
                    () => Factory.PullerOpen().ThenDial(url).Unwrap(),
                    true),
                new DupeUrlTest (
                    null,
                    () => Factory.PusherOpen().ThenListen(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.PullerOpen().ThenListen(url).Unwrap(),
                    () => Factory.PusherOpen().ThenDial(url).Unwrap(),
                    true),
                new DupeUrlTest (
                    null,
                    () => Factory.ReplierOpen().ThenListen(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.ReplierOpen().ThenListen(url).Unwrap(),
                    () => Factory.RequesterOpen().ThenDial(url).Unwrap(),
                    true),
                new DupeUrlTest (
                    () => Factory.PublisherOpen().ThenListen(url).Unwrap(),
                    () => Factory.SubscriberOpen().ThenDial(url).Unwrap(),
                    true),
            };

            for (int i = 0; i < tests.Length; ++i)
            {
                var test = tests[i];
                IDisposable pre = null;
                IDisposable obj0 = null;
                IDisposable obj1 = null;
                try
                {
                    if (test.pre != null)
                    {
                        pre = test.pre.Invoke();
                        Assert.NotNull(pre);
                    }
                    obj0 = test.func();
                    if (test.isOk)
                    {
                        obj1 = test.func();
                        Assert.NotNull(obj1);
                    }
                    else
                    {
                        Assert.ThrowsAny<Exception>(() => obj1 = test.func());
                    }
                }
                finally
                {
                    pre?.Dispose();
                    obj0?.Dispose();
                    obj1?.Dispose();
                }
            }
        }

        [Theory]
        [ClassData(typeof(TransportsNoWsClassData))]
        public void GetSetOpt(string url)
        {
            Fixture.TestIterate(() => DoGetSetOpt(url));
        }

        void DoGetSetOpt(string url)
        {
            using (var rep = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var req = Factory.RequesterOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                // bool
                AssertGetSetOpts(req, NNG_OPT_TCP_NODELAY);

                // int
                AssertGetSetOpts(req, NNG_OPT_RECVBUF, (int data) => data + 1);

                // nng_duration
                AssertGetSetOpts(req, NNG_OPT_RECONNMINT, (nng_duration data) => data + 100);

                // size_t
                AssertGetSetOpts(req, NNG_OPT_RECVMAXSZ, (UIntPtr data) => data + 128);

                // uint64_t

                // string
                AssertGetSetOpts(req, NNG_OPT_SOCKNAME, "test");

                // ptr
            }
        }
    }
}