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
    public class StreamTests
    {
        IAPIFactory<IMessage> factory;

        public StreamTests(NngCollectionFixture collectionFixture)
        {
            factory = collectionFixture.Factory;
        }

        [Fact]
        public void Basic()
        {
            using (var stream = new StreamStuff(factory))
            {
                
            }
        }

        [Fact]
        public void SendRecv()
        {
            using (var s = new StreamStuff(factory))
            {
                // Unmanaged memory
                var lbytes = Util.RandomBytes();
                var lmem = factory.CreateAlloc(lbytes.Length);
                lbytes.CopyTo(lmem.AsSpan());
                var liov = new nng_iov {iov_buf = lmem.Ptr, iov_len = lmem.Length};
                var dmem = factory.CreateAlloc(lbytes.Length);
                var diov = new nng_iov {iov_buf = dmem.Ptr, iov_len = dmem.Length};
                
                s.laio.SetIov(new []{liov});
                s.daio.SetIov(new []{diov});
                s.lstream.Send(s.daio);
                s.dstream.Recv(s.laio);
                s.laio.Wait();
                s.daio.Wait();
                Assert.NotEqual(UIntPtr.Zero, s.daio.Count());
                Assert.True(Util.BytesEqual(lmem.AsSpan(), dmem.AsSpan()));

                // Pinned managed memory
                unsafe 
                {
                    lbytes = Util.RandomBytes();
                    var dbytes = new byte[lbytes.Length];
                    fixed(byte* lptr = lbytes)
                    fixed(byte* dptr = dbytes)
                    {
                        liov = new nng_iov {iov_buf = (IntPtr)lptr, iov_len = (UIntPtr)lbytes.Length };
                        diov = new nng_iov {iov_buf = (IntPtr)dptr, iov_len = (UIntPtr)dbytes.Length };

                        s.laio.SetIov(new []{liov});
                        s.daio.SetIov(new []{diov});
                        s.lstream.Send(s.daio);
                        s.dstream.Recv(s.laio);
                        s.laio.Wait();
                        s.daio.Wait();
                        Assert.NotEqual(UIntPtr.Zero, s.daio.Count());
                        Assert.True(Util.BytesEqual(lbytes, dbytes));
                    }
                }
            }
        }
    }

    class StreamStuff : IDisposable
    {
        public readonly INngAio laio;
        public readonly INngAio daio;
        public readonly INngStreamListener listener;
        public readonly INngStreamDialer dialer;
        public readonly INngStream lstream;
        public readonly INngStream dstream;

        public StreamStuff(IAPIFactory<IMessage> factory)
        {
            var url = UrlIpc();
            laio = factory.CreateAio().Unwrap();
            daio = factory.CreateAio().Unwrap();
            listener = factory.StreamListenerCreate(url).Unwrap();
            dialer = factory.StreamDialerCreate(url).Unwrap();

            laio.SetTimeout(Util.DelayShortMs);
            daio.SetTimeout(Util.DelayShortMs);

            listener.Listen().Unwrap();
            listener.Accept(laio);
            dialer.Dial(daio);
            laio.Wait();
            daio.Wait();

            // Connected nng_stream is stored as aio output 0 and shouldn't be NULL
            Assert.NotEqual(IntPtr.Zero, laio.GetOutput(0));
            Assert.NotEqual(IntPtr.Zero, daio.GetOutput(0));

            lstream = factory.StreamFrom(laio).Unwrap();
            dstream = factory.StreamFrom(daio).Unwrap();
        }

        #region IDisposable
        public void Dispose()
        {
            laio?.Dispose();
            daio?.Dispose();
            listener?.Dispose();
            dialer?.Dispose();
            lstream?.Dispose();
            dstream?.Dispose();
        }
        #endregion
    }
}