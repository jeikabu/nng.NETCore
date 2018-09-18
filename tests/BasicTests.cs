using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;

    [Collection("nng")]
    public class BasicTests
    {
        [Fact]
        [Trait(Traits.PlatformName, Traits.PlatformWindows)]
        public void WindowsOnlyTestsFailonPosix()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                Assert.True(false);
                break;
            }
        }

        [Fact]
        [Trait(Traits.PlatformName, Traits.PlatformPosix)]
        public void PosixOnlyTestsFailonWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                break;

                default:
                Assert.True(false);
                break;
            }
        }
    }
}