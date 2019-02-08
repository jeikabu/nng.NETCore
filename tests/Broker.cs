using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace nng.Tests
{
    using static nng.Tests.Util;

    interface IBrokerImpl<T> : System.IDisposable
    {
        IAPIFactory<T> Factory { get; }
        IReceiveAsyncContext<T> CreateInSocket(string url);
        ISendAsyncContext<T> CreateOutSocket(string url);
        IReceiveAsyncContext<T> CreateClient(string url);
        IMessage CreateMessage();
    }

    class Broker: System.IDisposable
    {
        public Broker(IBrokerImpl<IMessage> implementation)
        {
            this.implementation = implementation;
        }

        public async Task<List<Task>> RunAsync(int numPushers, int numPullers, int numMessagesPerPusher, AsyncCountdownEvent counter, CancellationToken token)
        {
            var inUrl = UrlIpc();
            var outUrl = UrlIpc();
            const int numBrokers = 1;

            var brokersReady = new AsyncBarrier(numBrokers + 1);
            var clientsReady = new AsyncBarrier(numPushers + numPullers + numBrokers);

            var numForwarded = 0;
            var tasks = new List<Task>();
            for (var i = 0; i < numBrokers; ++i)
            {
                var task = Task.Run(async () =>
                {
                    var pullSocket = implementation.CreateInSocket(inUrl);
                    var pushSocket = implementation.CreateOutSocket(outUrl);
                    await brokersReady.SignalAndWait(); // Broker is ready
                    await clientsReady.SignalAndWait(); // Wait for clients
                    while (!token.IsCancellationRequested)
                    {
                        var msg = await pullSocket.Receive(token);
                        await pushSocket.Send(msg.Unwrap());
                        ++numForwarded;
                    }
                });
                tasks.Add(task);
            }

            await brokersReady.SignalAndWait();

            for (var i = 0; i < numPushers; ++i)
            {
                var task = Task.Run(async () =>
                {
                    using (var socket = implementation.Factory.PusherOpen().ThenDial(inUrl).Unwrap())
                    using (var ctx = socket.CreateAsyncContext(implementation.Factory).Unwrap())
                    {
                        await clientsReady.SignalAndWait(); // This client ready, wait for rest
                        // Give all receivers a chance to actually start receiving
                        await WaitShort();
                        for (var m = 0; m < numMessagesPerPusher; ++m)
                        {
                            await ctx.Send(implementation.CreateMessage());
                            await WaitShort();
                        }
                    }

                });
                tasks.Add(task);
            }

            for (var i = 0; i < numPullers; ++i)
            {
                var task = Task.Run(async () =>
                {
                    var pullSocket = implementation.CreateClient(outUrl);
                    await clientsReady.SignalAndWait(); // This client ready, wait for rest
                    while (!token.IsCancellationRequested)
                    {
                        var _ = await pullSocket.Receive(token);
                        counter.Signal();
                    }
                });
                tasks.Add(task);
            }

            return tasks;
        }

        public void Dispose()
        {
            implementation.Dispose();
        }

        IBrokerImpl<IMessage> implementation;
    }
}
