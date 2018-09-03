using System;
using Xunit;

namespace nng.Tests
{
    using static nng.Pinvoke.Aio;

    public class AioTests
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(0, nng_aio_alloc(out var aio, null, IntPtr.Zero));
            nng_aio_free(aio);
        }
    }
}
