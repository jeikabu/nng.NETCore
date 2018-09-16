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
        IAPIFactory<IMessage> factory;

        public SocketTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(BadTransportsClassData))]
        public void BadTransports(string url)
        {
            Assert.True(factory.PublisherCreate(url).IsErr());
            Assert.True(factory.PullerCreate(url, true).IsErr());
            Assert.True(factory.PullerCreate(url, false).IsErr());
            Assert.True(factory.PusherCreate(url, true).IsErr());
            Assert.True(factory.PusherCreate(url, false).IsErr());
            Assert.True(factory.ReplierCreate(url).IsErr());
            Assert.True(factory.RequesterCreate(url).IsErr());
            Assert.True(factory.SubscriberCreate(url).IsErr());
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
        [ClassData(typeof(TransportsClassData))]
        public void DuplicateUrl(string url)
        {
            var tests = new DupeUrlTest[] {
                new DupeUrlTest (
                    null,
                    () => factory.PublisherCreate(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    null,
                    () => factory.PullerCreate(url, true).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.PusherCreate(url, true).Unwrap(),
                    () => factory.PullerCreate(url, false).Unwrap(), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.PusherCreate(url, true).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.PullerCreate(url, true).Unwrap(),
                    () => factory.PusherCreate(url, false).Unwrap(), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.ReplierCreate(url).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.ReplierCreate(url).Unwrap(),
                    () => factory.RequesterCreate(url).Unwrap(), 
                    true),
                new DupeUrlTest (
                    () => factory.PublisherCreate(url).Unwrap(),
                    () => factory.SubscriberCreate(url).Unwrap(), 
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
                        Assert.Throws<InvalidOperationException>(() => obj1 = test.func());
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
        [ClassData(typeof(TransportsClassData))]
        public async void GetSetOpt(string url)
        {
            var rep = factory.ReplierCreate(url).Unwrap();
            var req = factory.RequesterCreate(url).Unwrap();

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