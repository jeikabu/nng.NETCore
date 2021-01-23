using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Native.Defines;
    using static nng.Tests.Util;


    [Collection("nng")]
    public class PipeTests
    {
        IAPIFactory<IMessage> factory;

        public PipeTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public void Creation(string url)
        {
            using (var listenSocket = factory.PairOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var dialerSocket = factory.PairOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                listenSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});
                dialerSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});

                dialerSocket.SendMsg(factory.CreateMessage());
                var msg = listenSocket.RecvMsg().Unwrap();
                Assert.True(msg.Pipe.IsValid());
                //Assert.True(msg.Pipe.Socket.IsValid());
                Assert.False(msg.Pipe.Dialer.IsValid());
                Assert.True(msg.Pipe.Listener.IsValid());

                listenSocket.SendMsg(factory.CreateMessage());
                msg = dialerSocket.RecvMsg().Unwrap();
                Assert.True(msg.Pipe.IsValid());
                //Assert.True(msg.Pipe.Socket.IsValid());
                Assert.True(msg.Pipe.Dialer.IsValid());
                Assert.False(msg.Pipe.Listener.IsValid());
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public void PipeEvents(string url)
        {
            int numAddPre = 0;
            int numAddPost = 0;
            int numRemPost = 0;
            void callback (nng_pipe _unused, NngPipeEv ev, IntPtr arg)
            {
                switch (ev)
                {
                    case NngPipeEv.AddPre:
                    Interlocked.Increment(ref numAddPre);
                    break;
                    case NngPipeEv.AddPost:
                    Interlocked.Increment(ref numAddPost);
                    break;
                    case NngPipeEv.RemPost:
                    Interlocked.Increment(ref numRemPost);
                    break;
                }
            }
            
            using (var listenSocket = factory.PairOpen().ThenListenAs(out var listener, url).Unwrap())
            {
                listenSocket.Notify(NngPipeEv.AddPre, callback, IntPtr.Zero);
                listenSocket.Notify(NngPipeEv.AddPost, callback, IntPtr.Zero);
                listenSocket.Notify(NngPipeEv.RemPost, callback, IntPtr.Zero);
                
                // Should receive each event once
                using (var dialerSocket = factory.PairOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
                {   
                }
                // Give listener a chance to receive events
                Thread.Sleep(Util.DelayShortMs);
                Assert.Equal(1, numAddPre);
                Assert.Equal(1, numAddPost);
                Assert.Equal(1, numRemPost);

                // Connect new dialer and continue receiving events
                using (var dialerSocket = factory.PairOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
                {
                }
                Thread.Sleep(Util.DelayShortMs);
                Assert.Equal(2, numAddPre);
                Assert.Equal(2, numAddPost);
                Assert.Equal(2, numRemPost);
            }
        }
    }
}