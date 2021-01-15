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
    public class ComplexTests
    {
        IAPIFactory<IMessage> factory;

        public ComplexTests(NngCollectionFixture collectionFixture)
        {
            factory = collectionFixture.Factory;
        }

        IMessage MsgRandom()
        {
            var msg = factory.CreateMessage();
            msg.Append(Guid.NewGuid().ToByteArray());
            return msg;
        }

        [Fact]
        public async Task Issue89_FullDuplexPairWithContexts()
        {
            const int MAX_CLIENT_COUNT = 4;
            string url = Util.UrlInproc();
            var tasks = new List<Task>();
            var cts = new CancellationTokenSource();
            int numReceived = 0;

            using (var socket = factory.PairOpen().ThenListenAs(out var listener, url).Unwrap())
            {
                listener.Start();

                foreach (var _ in Enumerable.Range(0, MAX_CLIENT_COUNT))
                {
                    var task = Task.Run(async () => {
                        using (var context = socket.CreateAsyncContext(factory).Unwrap())
                        {
                            context.Aio.SetTimeout(100);
                            while (!cts.IsCancellationRequested)
                            {
                                var _s = await context.Send(factory.CreateMessage());
                                var _r = (await context.Receive(cts.Token));
                            }
                        }
                    });
                    tasks.Add(task);
                }

                foreach (var _ in Enumerable.Range(0, MAX_CLIENT_COUNT))
                {
                    var task = Task.Run(async () => {
                        using (var socket = factory.PairOpen().ThenDial(url).Unwrap())
                        using (var context = socket.CreateAsyncContext(factory).Unwrap())
                        {
                            context.Aio.SetTimeout(100);
                            while (!cts.IsCancellationRequested)
                            {
                                var _s = await context.Send(factory.CreateMessage());
                                var recv = (await context.Receive(cts.Token));
                                if (recv.IsOk())
                                    Interlocked.Increment(ref numReceived);
                            }
                        }
                    });
                    tasks.Add(task);
                }

                await Util.CancelAfterAssertwait(tasks, cts);
            }
            
            // TODO: better check that all clients/listeners both send and receive messages
            Assert.True(numReceived > MAX_CLIENT_COUNT*2);
        }
    }
}