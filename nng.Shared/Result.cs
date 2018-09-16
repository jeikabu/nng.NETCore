using static nng.Native.Defines;

namespace nng
{
    public interface IResult<Err, Val>
    {
    }

    public interface IOk<Err, Val> : IResult<Err, Val>
    {
        Val Result { get; }
    }

    public interface IErr<Err, Val> : IResult<Err, Val>
    {
        Err Error { get; }
    }

    

    public static class ResultExt
    {
        // public static void Deconstruct<Err, Val>(this IResult<Err, Val> self, out Err error, out Val value)
        // {
        //     error = self.Error;
        //     value = self.Result;
        // }

        public static bool IsOk<Err, Val>(this IResult<Err, Val> self)
        {
            return self is IOk<Err, Val>;
        }

        public static bool IsErr<Err, Val>(this IResult<Err, Val> self)
        {
            return self is IErr<Err, Val>;
        }

        public static Val Unwrap<Err, Val>(this IResult<Err, Val> self)
        {
            if (self is IOk<Err,Val> ok)
            {
                return ok.Result;
            }
            throw new System.InvalidOperationException();
        }

        public static Err Error<Err, Val>(this IResult<Err, Val> self)
        {
            if (self is IErr<Err,Val> error)
            {
                return error.Error;
            }
            throw new System.InvalidOperationException();
        }
    }

    public struct Ok<Err, Val> : IOk<Err, Val>
    {
        public Ok(Val value)
        {
            this.value = value;
        }

        public Val Result => value;

        Val value;
    }

    public struct Err<TErr, Val> : IErr<TErr, Val>
    {
        public Err(TErr error)
        {
            this.error = error;
        }

        public TErr Error => error;

        TErr error;
    }

    public interface INngResult<Val> : IResult<NngErrno, Val>
    {}

    public struct NngOk<Val> : IOk<NngErrno, Val>, INngResult<Val>
    {
        public NngOk(Val value)
        {
            this.value = value;
        }

        public Val Result => value;

        Val value;
    }

    public struct NngErr<Val> : IErr<NngErrno, Val>, INngResult<Val>
    {
        public NngErr(NngErrno error)
        {
            if (error == 0)
                throw new System.InvalidOperationException();
            this.error = error;
        }

        public NngErrno Error => error;

        NngErrno error;
    }

    public static class NngResult
    {
        public static NngOk<Val> Ok<Val>(Val value)
        {
            return new NngOk<Val>(value);
        }

        public static NngErr<Val> Fail<Val>(int error)
        {
            return new NngErr<Val>((NngErrno)error);
        }

        public static NngErr<Val> Fail<Val>(NngErrno error)
        {
            return new NngErr<Val>(error);
        }
    }
}