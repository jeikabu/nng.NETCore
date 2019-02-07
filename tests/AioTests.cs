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
        IAPIFactory<IMessage> factory;

        public AioTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Fact]
        public async void CancelReceiveToken()
        {
            var (push, pull) = await CreatePusherAndPuller();
            pull.SetTimeout(1000);

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
            pull.SetTimeout(1000);

            // Cancel idling Receive() using nng_aio_cancel
            var pullTask = pull.Receive(CancellationToken.None);
            pull.Cancel();
            // Cancel happens asynchronously.  Give callback a chance to happen
            await WaitReady();
            Assert.Equal((await pullTask).Err(), NngErrno.ECANCELED);
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
            pull.SetTimeout(NNG_DURATION_ZERO);
            Assert.Equal((await pull.Receive(CancellationToken.None)).Err(), NngErrno.ETIMEDOUT);

            // Infinite timeout
            pull.SetTimeout(NNG_DURATION_INFINITE);
            var timeoutTask = WaitReady();
            var pullTask = pull.Receive(CancellationToken.None);
            Assert.Equal(timeoutTask, await Task.WhenAny(timeoutTask, pullTask));
        }

        async Task<(ISendAsyncContext<IMessage>, IReceiveAsyncContext<IMessage>)> CreatePusherAndPuller()
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
    }
}