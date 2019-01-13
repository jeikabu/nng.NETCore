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
            Assert.True(Factory.PublisherCreate(url).IsErr());
            Assert.True(Factory.PullerCreate(url, true).IsErr());
            Assert.True(Factory.PullerCreate(url, false).IsErr());
            Assert.True(Factory.PusherCreate(url, true).IsErr());
            Assert.True(Factory.PusherCreate(url, false).IsErr());
            Assert.True(Factory.ReplierCreate(url).IsErr());
            Assert.True(Factory.RequesterCreate(url).IsErr());
            Assert.True(Factory.SubscriberCreate(url).IsErr());
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
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                DoDuplicateUrl(url);
            }
        }

        void DoDuplicateUrl(string url)
        {
            var tests = new DupeUrlTest[] {
                new DupeUrlTest (
                    null,
                    () => Factory.PublisherCreate(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    null,
                    () => Factory.PullerCreate(url, true).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.PusherCreate(url, true).Unwrap(),
                    () => Factory.PullerCreate(url, false).Unwrap(),
                    true),
                new DupeUrlTest (
                    null,
                    () => Factory.PusherCreate(url, true).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.PullerCreate(url, true).Unwrap(),
                    () => Factory.PusherCreate(url, false).Unwrap(),
                    true),
                new DupeUrlTest (
                    null,
                    () => Factory.ReplierCreate(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    () => Factory.ReplierCreate(url).Unwrap(),
                    () => Factory.RequesterCreate(url).Unwrap(),
                    true),
                new DupeUrlTest (
                    () => Factory.PublisherCreate(url).Unwrap(),
                    () => Factory.SubscriberCreate(url).Unwrap(),
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
        public async void GetSetOpt(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoGetSetOpt(url);
            }
        }

        async Task DoGetSetOpt(string url)
        {
            using (var rep = Factory.ReplierCreate(url).Unwrap())
            using (var req = Factory.RequesterCreate(url).Unwrap())
            {
                //await WaitReady();
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

                // ptr
            }
        }
    }
}