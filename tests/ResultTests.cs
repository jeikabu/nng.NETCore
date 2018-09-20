using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng
{
    public class ResultTests
    {
        [Fact]
        public void Basic()
        {
            ResultTest<int, int>();
        }

        public void ResultTest<TErr, TRes>()
        {
            IResult<TErr, TRes> CreateOk()
            {
                return new Ok<TErr, TRes>(default);
            }
            IResult<TErr, TRes> CreateErr()
            {
                return new Err<TErr, TRes>(default);
            }

            {
                var anOk = CreateOk();
                Assert.True(anOk.IsOk());
                Assert.False(anOk.IsErr());
                Assert.True(anOk is IOk<TErr,TRes>);
                Assert.False(anOk is IErr<TErr,TRes>);
                anOk.Unwrap();
                Assert.Throws<InvalidOperationException>(() => anOk.Error());
                switch (anOk)
                {
                    case Ok<TErr, TRes> ok:
                        //ok
                    break;
                    default:
                    case Err<TErr, TRes> fail:
                    Assert.True(false);
                    break;
                }
            }
            {
                var anErr = CreateErr();
                Assert.False(anErr.IsOk());
                Assert.True(anErr.IsErr());
                Assert.False(anErr is IOk<TErr,TRes>);
                Assert.True(anErr is IErr<TErr,TRes>);
                Assert.Throws<InvalidOperationException>(() => anErr.Unwrap());
                anErr.Error();
                switch (anErr)
                {
                    default:
                    case Ok<TErr, TRes> ok:
                        Assert.True(false);
                    break;
                    
                    case Err<TErr, TRes> fail:
                    //ok
                    break;
                }
            }

            // Error with default value works
            new Err<TErr, TRes>(default);
        }

        [Fact]
        public void UseNngResult()
        {
            NngResultTest<int>();
        }

        void NngResultTest<TRes>()
        {

            {
                var anOk = (INngResult<TRes>)NngResult.Ok<TRes>(default);
                Assert.True(anOk.IsOk());
                Assert.False(anOk.IsErr());
                Assert.True(anOk is NngOk<TRes>);
                Assert.False(anOk is NngErr<TRes>);
                anOk.Unwrap();
                Assert.Throws<InvalidOperationException>(() => anOk.Error());
                switch (anOk)
                {
                    case NngOk<TRes> ok:
                        //ok
                    break;
                    default:
                    case NngErr<TRes> fail:
                    Assert.True(false);
                    break;
                }
            }
            {
                var anErr = (INngResult<TRes>)NngResult.Fail<TRes>(Defines.NngErrno.EINVAL);
                Assert.False(anErr.IsOk());
                Assert.True(anErr.IsErr());
                Assert.False(anErr is NngOk<TRes>);
                Assert.True(anErr is NngErr<TRes>);
                Assert.Throws<InvalidOperationException>(() => anErr.Unwrap());
                anErr.Error();
                switch (anErr)
                {
                    default:
                    case NngOk<TRes> ok:
                        Assert.True(false);
                    break;
                    
                    case NngErr<TRes> fail:
                    //ok
                    break;
                }
            }

            Assert.Throws<System.InvalidOperationException>(() => NngResult.Fail<TRes>(0));
        }
    }
}