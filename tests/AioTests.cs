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
    public class AioTests
    {
        IAPIFactory<INngMsg> factory;

        public AioTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        static void staticAioCallback(IntPtr _)
        {}

        [Fact]
        public void Basic()
        {
            Util.RepeatTest(() => {
                var callbacks = new AioCallback[]{
                    null,
                    staticAioCallback,
                };
                foreach (var callback in callbacks)
                {
                    using (var aio = factory.CreateAio(callback).Unwrap())
                    {
                        aio.SetTimeout(10);
                        aio.Wait();
                        aio.GetResult().Unwrap();
                        Assert.Equal(IntPtr.Zero, aio.GetOutput(0));
                        aio.Cancel();
                    }
                }
            });
        }

        [Fact]
        public async void CancelReceiveToken()
        {
            var (push, pull) = await CreatePusherAndPuller();
            pull.Aio.SetTimeout(1000);

            // Cancel idling Receive() using CancellationToken
            using (var cts = new CancellationTokenSource())
            {
                var pullTask = pull.Receive(cts.Token);
                cts.CancelAfter(50);
                await Assert.ThrowsAsync<TaskCanceledException>(() => pullTask);
            }
        }

        [Fact]
        public async void CancelReceiveAio()
        {
            var (push, pull) = await CreatePusherAndPuller();
            pull.Aio.SetTimeout(1000);

            // Cancel idling Receive() using nng_aio_cancel
            var pullTask = pull.Receive(CancellationToken.None);
            pull.Aio.Cancel();
            // Cancel happens asynchronously.  Give callback a chance to happen
            await WaitReady();
            Assert.Equal(NngErrno.ECANCELED, (await pullTask).Err());
        }

        // [Fact]
        // public async void CancelPush()
        // {
        //     var (push, pull) = await CreatePusherAndPuller();

        //     // Cancel Send()
        //     var msg = factory.CreateMessage();
        //     msg.Append(new byte[1024*1024*1024]);
        //     var pushTask = push.Send(msg);
        //     while (push.State == AsyncState.Init)
        //     {
        //         await Task.Delay(1);
        //     }
        //     push.Cancel();
        //     await Assert.ThrowsAsync<TaskCanceledException>(async () => await pushTask);
        // }

        [Fact]
        public async Task Timeout()
        {
            var (push, pull) = await CreatePusherAndPuller();

            // Immediate timeout
            pull.Aio.SetTimeout(NNG_DURATION_ZERO);
            Assert.Equal(NngErrno.ETIMEDOUT, (await pull.Receive(CancellationToken.None)).Err());

            // Infinite timeout
            pull.Aio.SetTimeout(NNG_DURATION_INFINITE);
            var timeoutTask = WaitReady();
            var pullTask = pull.Receive(CancellationToken.None);
            Assert.Equal(timeoutTask, await Task.WhenAny(timeoutTask, pullTask));
        }

        async Task<(ISendAsyncContext<INngMsg>, IReceiveAsyncContext<INngMsg>)> CreatePusherAndPuller()
        {
            var url = UrlIpc();
            var pushSocket = factory.PusherOpen().Unwrap();
            pushSocket.Listen(url).Unwrap();
            var push = pushSocket.CreateAsyncContext(factory).Unwrap();
            await WaitReady();
            var pullSocket = factory.PullerOpen().Unwrap();
            pullSocket.Dial(url).Unwrap();
            var pull = pullSocket.CreateAsyncContext(factory).Unwrap();
            return (push, pull);
        }

        [Fact]
        public async Task Iov()
        {
            var url = UrlInproc();
            using (var socket = factory.PusherOpen().ThenListen(url).Unwrap())
            using (var context = socket.CreateAsyncContext(factory).Unwrap())
            {
                // Maximum iov
                var array = new nng_iov[MAX_IOV_BUFFERS];
                context.Aio.SetIov(array).Unwrap();

                // Too many iov
                array = new nng_iov[MAX_IOV_BUFFERS + 1];
                Assert.Equal(NngErrno.EINVAL, context.Aio.SetIov(array).Err());

                // Stack-allocated iov
                unsafe {
                    var span = stackalloc nng_iov[MAX_IOV_BUFFERS];
                    context.Aio.SetIov(new Span<nng_iov>(span, MAX_IOV_BUFFERS)).Unwrap();
                }
            }
        }
    }
}
