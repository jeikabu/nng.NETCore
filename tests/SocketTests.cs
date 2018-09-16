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
            Assert.True(factory.CreatePublisher(url).IsErr());
            Assert.True(factory.CreatePuller(url, true).IsErr());
            Assert.True(factory.CreatePuller(url, false).IsErr());
            Assert.True(factory.CreatePusher(url, true).IsErr());
            Assert.True(factory.CreatePusher(url, false).IsErr());
            Assert.True(factory.CreateReplier(url).IsErr());
            Assert.True(factory.CreateRequester(url).IsErr());
            Assert.True(factory.CreateSubscriber(url).IsErr());
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
                    () => factory.CreatePublisher(url).Unwrap(),
                    false),
                new DupeUrlTest (
                    null,
                    () => factory.CreatePuller(url, true).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.CreatePusher(url, true).Unwrap(),
                    () => factory.CreatePuller(url, false).Unwrap(), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.CreatePusher(url, true).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.CreatePuller(url, true).Unwrap(),
                    () => factory.CreatePusher(url, false).Unwrap(), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.CreateReplier(url).Unwrap(), 
                    false),
                new DupeUrlTest (
                    () => factory.CreateReplier(url).Unwrap(),
                    () => factory.CreateRequester(url).Unwrap(), 
                    true),
                new DupeUrlTest (
                    () => factory.CreatePublisher(url).Unwrap(),
                    () => factory.CreateSubscriber(url).Unwrap(), 
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
            var rep = factory.CreateReplier(url).Unwrap();
            var req = factory.CreateRequester(url).Unwrap();

            // bool
            AssertGetSetOpts(req.Socket, NNG_OPT_TCP_NODELAY);

            // int
            AssertGetSetOpts(req.Socket, NNG_OPT_RECVBUF, (int data) => data + 1);
            
            // nng_duration
            AssertGetSetOpts(req.Socket, NNG_OPT_RECONNMINT, (nng_duration data) => data + 100);

            // size_t
            AssertGetSetOpts(req.Socket, NNG_OPT_RECVMAXSZ, (UIntPtr data) => data + 128);

            // uint64_t
            
            // string

            // ptr
        }
    }
}