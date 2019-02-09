using static nng.Native.Defines;
using System;
using System.Runtime.CompilerServices;

namespace nng
{
    /// <summary>
    /// Unit type containing now value.  Similar to `()` in F#, Rust, etc.
    /// </summary>
    public struct Unit
    {
        public static NngResult<Unit> Ok => NngResult<Unit>.Ok(new Unit { });
        public static NngResult<Unit> Err(NngErrno err) => NngResult<Unit>.Err(err);

        public static NngResult<Unit> OkIfZero(int errno)
        {
            if (errno == 0)
                return Ok;
            else
                return Err((NngErrno)errno);
        }
    }

    /// <summary>
    /// Result of an operation that succeeds with one value or fails with another.
    /// </summary>
    public struct Result<TOk, TErr>
    {
        TErr error;
        TOk ok;
        bool isOk;

        /// <summary>
        /// Create a success result.
        /// </summary>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static Result<TOk, TErr> Ok(TOk ok)
        {
            return new Result<TOk, TErr> { ok = ok, isOk = true };
        }

        /// <summary>
        /// Create a fail result.
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static Result<TOk, TErr> Err(TErr err)
        {
            return new Result<TOk, TErr> { error = err, isOk = false };
        }

        public bool IsOk() => isOk;

        public bool TryOk(out TOk okValue)
        {
            if (isOk)
                okValue = ok;
            else
                okValue = default;
            return isOk;
        }

        public bool TryError(out TErr errorValue)
        {
            if (isOk)
                errorValue = default;
            else
                errorValue = error;
            return !isOk;
        }

        public TErr Err()
        {
            if (!IsOk())
                return error;
            throw null;
        }

        public TOk Unwrap()
        {
            if (isOk)
                return ok;
            throw null;
        }

        public void Deconstruct(out bool isOk, out TErr errorValue, out TOk okValue)
        {
            isOk = this.isOk;
            errorValue = error;
            okValue = ok;
        }
    }

    /// <summary>
    /// Result of an operation that succeeds with a value or fails with an NngErrno.
    /// </summary>
    public struct NngResult<TOk>
    {
        Result<TOk, NngErrno> result;

        public static NngResult<TOk> Ok(TOk ok)
        {
            var res = Result<TOk, NngErrno>.Ok(ok);
            return new NngResult<TOk> { result = res };
        }

        public static NngResult<TOk> Err(NngErrno err)
        {
            var res = Result<TOk, NngErrno>.Err(err);
            return new NngResult<TOk> { result = res };
        }

        /// <summary>
        /// If errno is 0, return success otherwise return fail.
        /// </summary>
        /// <param name="errno"></param>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static NngResult<TOk> OkIfZero(int errno, TOk ok)
        {
            if (errno == 0)
                return Ok(ok);
            else
                return Err((NngErrno)errno);
        }

        /// <summary>
        /// If errno is 0, return success initialized with value returned by func, otherwise fail.
        /// </summary>
        /// <param name="errno"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NngResult<TOk> OkThen(int errno, Func<TOk> func)
        {
            if (errno == 0)
                return Ok(func());
            else
                return Err((NngErrno)errno);
        }

        /// <summary>
        /// Create fail result using non-zero errno.
        /// </summary>
        /// <param name="errno"></param>
        /// <returns></returns>
        public static NngResult<TOk> Fail(int errno)
        {
            if (errno == 0)
                throw null;
            return Err((NngErrno)errno);
        }

        public bool IsOk() => result.IsOk();
        public bool IsErr() => !IsOk();
        public bool IsErr(NngErrno err) => IsErr() && Err() == err;

        public bool TryOk(out TOk okValue) => result.TryOk(out okValue);

        public bool TryError(out NngErrno errorValue) => result.TryError(out errorValue);

        /// <summary>
        /// Returns success value.  If this is a fail result throws an exception.
        /// </summary>
        /// <returns></returns>
        public TOk Ok() => result.Unwrap();

        /// <summary>
        /// Returns fail value.  If this is a success result throws an exception.
        /// </summary>
        /// <returns></returns>
        public NngErrno Err() => result.Err();

        public NngResult<TOk> Then(Func<TOk, NngResult<Unit>> func)
        {
            if (IsOk())
            {
                var ok = Ok();
                var res = func(ok);
                return res.Into(() => ok);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Create a result of another type.  If this is a success uses.
        /// If this is a fail, contains the same fail value.
        /// </summary>
        /// <param name="func">Function to convert success value to another type</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NngResult<T> Into<T>(Func<T> func)
        {
            if (IsOk())
            {
                return NngResult<T>.Ok(func());
            }
            else
            {
                return IntoErr<T>();
            }
        }

        /// <summary>
        /// Create a fail result from the fail value.  If this is a success result throws an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NngResult<T> IntoErr<T>()
        {
            return NngResult<T>.Err(Err());
        }

        public TOk Unwrap()
        {
            if (result.IsOk())
                return Ok();
            throw new NngException(Err());
        }

        public void Deconstruct(out bool isOk, out NngErrno errorValue, out TOk okValue) => result.Deconstruct(out isOk, out errorValue, out okValue);
    }
}