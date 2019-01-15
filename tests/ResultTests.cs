using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Basic()
        {
            ResultTest<int, int>();
        }

        public void ResultTest<TRes, TErr>()
        {
            Result<TRes, TErr> CreateOk()
            {
                return Result<TRes, TErr>.Ok(default);
            }
            Result<TRes, TErr> CreateErr()
            {
                return Result<TRes, TErr>.Err(default);
            }

            {
                var anOk = CreateOk();
                Assert.True(anOk.IsOk());
                Assert.ThrowsAny<Exception>(() => anOk.Err());
                anOk.Unwrap();
                // switch (anOk)
                // {
                //     case Ok<TErr, TRes> ok:
                //         //ok
                //         break;
                //     default:
                //     case Err<TErr, TRes> fail:
                //         Assert.True(false);
                //         break;
                // }
            }
            {
                var anErr = CreateErr();
                Assert.False(anErr.IsOk());
                anErr.Err();
                Assert.ThrowsAny<Exception>(() => anErr.Unwrap());
                // switch (anErr)
                // {
                //     default:
                //     case Ok<TErr, TRes> ok:
                //         Assert.True(false);
                //         break;

                //     case Err<TErr, TRes> fail:
                //         //ok
                //         break;
                // }
            }
        }

        [Fact]
        public void UseNngResult()
        {
            NngResultTest<int>();
        }

        void NngResultTest<TRes>()
        {
            {
                var anOk = NngResult<TRes>.Ok(default);
                Assert.True(anOk.IsOk());
                Assert.False(anOk.IsErr());
                anOk.Ok();
                Assert.ThrowsAny<Exception>(() => anOk.Err());
                anOk.Unwrap();
                switch (anOk)
                {
                    // This syntax doesn't work T_T
                    //case (true, _, var ok):
                    case var ok when ok.IsOk():
                        //ok
                        break;
                    default:
                    case var err when err.IsErr():
                        Assert.True(false);
                        break;
                }
            }
            {
                var anErr = NngResult<TRes>.Fail((int)Defines.NngErrno.EINVAL);
                Assert.False(anErr.IsOk());
                Assert.True(anErr.IsErr());
                Assert.ThrowsAny<Exception>(() => anErr.Ok());
                anErr.Err();
                Assert.ThrowsAny<Exception>(() => anErr.Unwrap());
                switch (anErr)
                {
                    default:
                    case var ok when ok.IsOk():
                        Assert.True(false);
                        break;

                    case var err when err.IsErr():
                        //ok
                        break;
                }
            }

            Assert.ThrowsAny<Exception>(() => NngResult<TRes>.Fail(0));
        }
    }
}