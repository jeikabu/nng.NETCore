using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

using static nng.Native.Defines;

namespace nng
{
    public class NngException : Exception
    {
        public NngException(int errorCode)
        {
            ErrorCode = errorCode;
        }
        public NngException(NngErrno errno)
        {
            ErrorCode = (int)errno;
        }

        public static void AssertZero(int errorCode)
        {
            if (errorCode != 0)
                throw new NngException(errorCode);
        }

        public override string Message => Error.ToString();//nng_strerror(error);
        //public override string ToString() => Message;

        public int ErrorCode { get; } = 0;
        public NngErrno Error => (NngErrno)ErrorCode;
    }

    /// <summary>
    /// Operations on part of a message; either the header or body
    /// </summary>
    public interface IMessagePart
    {
        //int Append(byte[] data);
        int Append(ReadOnlySpan<byte> data);
        int Append(uint data);
        int Chop(UIntPtr size);
        int Chop(out uint data);
        void Clear();
        int Insert(byte[] data);
        int Insert(uint data);
        int Length { get; }
        int Trim(UIntPtr size);
        int Trim(out uint data);
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        Span<byte> AsSpan();
    }

    /// <summary>
    /// Message sent/received via nng
    /// </summary>
    public interface IMessage : IMessagePart, IDisposable
    {
        /// <summary>
        /// Get the underlying nng_msg
        /// </summary>
        /// <value></value>
        nng_msg NngMsg { get; }
        /// <summary>
        /// Take ownership of he underlying nng_msg
        /// </summary>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        nng_msg Take();
        /// <summary>
        /// Get the header portion of the message
        /// </summary>
        /// <value></value>
        IMessagePart Header { get; }
        /// <summary>
        /// Duplicates the message creating a new, identical message.
        /// </summary>
        /// <returns>The newly created identical message duplicate</returns>
        IMessage Dup();
        /// <summary>
        /// Get the pipe object associated with the message.
        /// </summary>
        IPipe Pipe { get; }
    }

    /// <summary>
    /// Handle to a "pipe", which can be thought of as a single connection.
    /// </summary>
    public interface IPipe : IGetOptions
    {
        /// <summary>
        /// Get the underlying nng_pipe.
        /// </summary>
        nng_pipe NngPipe { get; }

        /// <summary>
        /// A positive identifier for the pipe, if it is valid.
        /// </summary>
        int Id { get; }
    }

    public static class Extensions
    {
        public static TaskCompletionSource<NngResult<T>> CreateSource<T>()
        {
            return new TaskCompletionSource<NngResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        public static CancellationTokenTaskSource<NngResult<T>> CreateReceiveSource<T>(CancellationToken token)
        {
            return new CancellationTokenTaskSource<NngResult<T>>(token, TaskCreationOptions.RunContinuationsAsynchronously);
        }
        public static TaskCompletionSource<NngResult<Unit>> CreateSendResultSource()
        {
            return new TaskCompletionSource<NngResult<Unit>>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        public static void SetNngResult(this TaskCompletionSource<NngResult<Unit>> socket)
        {
            socket.SetResult(Unit.Ok);
        }
        public static void SetNngResult<T>(this TaskCompletionSource<NngResult<T>> socket, T message)
        {
            socket.SetResult(NngResult<T>.Ok(message));
        }
        public static void TrySetNngResult<T>(this TaskCompletionSource<NngResult<T>> socket, T message)
        {
            socket.TrySetResult(NngResult<T>.Ok(message));
        }
        public static void TrySetNngResult(this TaskCompletionSource<NngResult<Unit>> socket)
        {
            socket.TrySetResult(Unit.Ok);
        }
        public static void TrySetNngResult<T>(this CancellationTokenTaskSource<NngResult<T>> socket, T message)
        {
            socket.TrySetResult(NngResult<T>.Ok(message));
        }
        public static void TrySetNngError<T>(this TaskCompletionSource<NngResult<T>> socket, int error)
        {
            if (error == 0)
            {
                return;
            }
            socket.TrySetResult(NngResult<T>.Fail(error));
        }
        public static void TrySetNngError<T>(this CancellationTokenTaskSource<NngResult<T>> socket, int error)
        {
            if (error == 0)
            {
                return;
            }
            socket.TrySetResult(NngResult<T>.Fail(error));
        }
    }
}