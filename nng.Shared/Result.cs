using static nng.Native.Defines;

namespace nng
{
    /// <summary>
    /// Represents a result.
    /// </summary>
    public interface IResult<Err, Val>
    {
    }

    /// <summary>
    /// Represents a successful result
    /// </summary>
    public interface IOk<Err, Val> : IResult<Err, Val>
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>The result.</value>
        Val Result { get; }
    }

    /// <summary>
    /// Represents a failed result
    /// </summary>
    public interface IErr<Err, Val> : IResult<Err, Val>
    {
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        Err Error { get; }
    }

    

    public static class ResultExt
    {
        // public static void Deconstruct<Err, Val>(this IResult<Err, Val> self, out Err error, out Val value)
        // {
        //     error = self.Error;
        //     value = self.Result;
        // }

        /// <summary>
        /// Determine if the result is success
        /// </summary>
        /// <returns><c>true</c>, if success, <c>false</c> otherwise.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="Err">The 1st type parameter.</typeparam>
        /// <typeparam name="Val">The 2nd type parameter.</typeparam>
        public static bool IsOk<Err, Val>(this IResult<Err, Val> self)
        {
            return self is IOk<Err, Val>;
        }

        /// <summary>
        /// Determine if the result is failure
        /// </summary>
        /// <returns><c>true</c>, if failure, <c>false</c> otherwise.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="Err">The 1st type parameter.</typeparam>
        /// <typeparam name="Val">The 2nd type parameter.</typeparam>
        public static bool IsErr<Err, Val>(this IResult<Err, Val> self)
        {
            return self is IErr<Err, Val>;
        }

        /// <summary>
        /// Treats result as a success and gets result, throws exception if actually failure
        /// </summary>
        /// <returns>The result</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="Err">The 1st type parameter.</typeparam>
        /// <typeparam name="Val">The 2nd type parameter.</typeparam>
        public static Val Unwrap<Err, Val>(this IResult<Err, Val> self)
        {
            if (self is IOk<Err,Val> ok)
            {
                return ok.Result;
            }
            throw new System.InvalidOperationException();
        }

        /// <summary>
        /// Treats result as a failure and gets the error, throws exception if actually success
        /// </summary>
        /// <returns>The error.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="Err">The 1st type parameter.</typeparam>
        /// <typeparam name="Val">The 2nd type parameter.</typeparam>
        public static Err Error<Err, Val>(this IResult<Err, Val> self)
        {
            if (self is IErr<Err,Val> error)
            {
                return error.Error;
            }
            throw new System.InvalidOperationException();
        }
    }

    /// <summary>
    /// Success result
    /// </summary>
    public struct Ok<Err, Val> : IOk<Err, Val>
    {
        public Ok(Val value)
        {
            this.value = value;
        }

        public Val Result => value;

        Val value;
    }

    /// <summary>
    /// Fail result
    /// </summary>
    public struct Err<TErr, Val> : IErr<TErr, Val>
    {
        public Err(TErr error)
        {
            this.error = error;
        }

        public TErr Error => error;

        TErr error;
    }

    /// <summary>
    /// Represents result returned by nng
    /// </summary>
    public interface INngResult<Val> : IResult<NngErrno, Val>
    {}

    /// <summary>
    /// Nng success result
    /// </summary>
    public struct NngOk<Val> : IOk<NngErrno, Val>, INngResult<Val>
    {
        public NngOk(Val value)
        {
            this.value = value;
        }

        public Val Result => value;

        Val value;
    }

    /// <summary>
    /// Nng fail result
    /// </summary>
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
        /// <summary>
        /// Create nng success result
        /// </summary>
        /// <returns>The ok.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="Val">The 1st type parameter.</typeparam>
        public static NngOk<Val> Ok<Val>(Val value)
        {
            return new NngOk<Val>(value);
        }

        /// <summary>
        /// Create nng fail result
        /// </summary>
        /// <returns>The fail.</returns>
        /// <param name="error">Error.</param>
        /// <typeparam name="Val">The 1st type parameter.</typeparam>
        public static NngErr<Val> Fail<Val>(int error)
        {
            return new NngErr<Val>((NngErrno)error);
        }

        /// <summary>
        /// Create nng fail result
        /// </summary>
        /// <returns>The fail.</returns>
        /// <param name="error">Error.</param>
        /// <typeparam name="Val">The 1st type parameter.</typeparam>
        public static NngErr<Val> Fail<Val>(NngErrno error)
        {
            return new NngErr<Val>(error);
        }
    }
}