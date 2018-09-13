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
            Assert.Null(factory.CreatePublisher(url));
            Assert.Null(factory.CreatePuller(url, true));
            Assert.Null(factory.CreatePuller(url, false));
            Assert.Null(factory.CreatePusher(url, true));
            Assert.Null(factory.CreatePusher(url, false));
            Assert.Null(factory.CreateReplier(url));
            Assert.Null(factory.CreateRequester(url));
            Assert.Null(factory.CreateSubscriber(url));
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
                    () => factory.CreatePublisher(url), 
                    false),
                new DupeUrlTest (
                    null,
                    () => factory.CreatePuller(url, true), 
                    false),
                new DupeUrlTest (
                    () => factory.CreatePusher(url, true),
                    () => factory.CreatePuller(url, false), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.CreatePusher(url, true), 
                    false),
                new DupeUrlTest (
                    () => factory.CreatePuller(url, true),
                    () => factory.CreatePusher(url, false), 
                    true),
                new DupeUrlTest (
                    null,
                    () => factory.CreateReplier(url), 
                    false),
                new DupeUrlTest (
                    () => factory.CreateReplier(url),
                    () => factory.CreateRequester(url), 
                    true),
                new DupeUrlTest (
                    () => factory.CreatePublisher(url),
                    () => factory.CreateSubscriber(url), 
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
                    obj1 = test.func();
                    if (test.isOk)
                    {
                        Assert.NotNull(obj1);
                    }
                    else
                    {
                        Assert.Null(obj1);
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
            var rep = factory.CreateReplier(url);
            var req = factory.CreateRequester(url);

            // bool
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_TCP_NODELAY, out bool isSet));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_TCP_NODELAY, !isSet));
            req.Socket.GetOpt(NNG_OPT_TCP_NODELAY, out bool isSetNow);
            Assert.NotEqual(isSet, isSetNow);

            // int
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECVBUF, out int recvBufNumMsgs));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECVBUF, recvBufNumMsgs + 1));
            req.Socket.GetOpt(NNG_OPT_RECVBUF, out int nextRecvBufNumMsgs);
            Assert.Equal(recvBufNumMsgs + 1, nextRecvBufNumMsgs);
            
            // nng_duration
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECONNMINT, out nng_duration minWaitConnMs));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECONNMINT, minWaitConnMs + 100));
            req.Socket.GetOpt(NNG_OPT_RECONNMINT, out nng_duration nextMinWaitConnMs);
            Assert.Equal(minWaitConnMs + 100, nextMinWaitConnMs);

            // size_t
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECVMAXSZ, out UIntPtr recvMaxBytes));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECVMAXSZ, UIntPtr.Zero));
            req.Socket.GetOpt(NNG_OPT_RECVMAXSZ, out UIntPtr nextRecvMaxBytes);
            Assert.Equal(UIntPtr.Zero, nextRecvMaxBytes);

            // uint64_t
            
            // string

            // ptr
        }
    }
}