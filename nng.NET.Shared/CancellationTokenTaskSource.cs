using System;
using System.Threading;
using System.Threading.Tasks;

/// From:
/// https://github.com/StephenClearyArchive/AsyncEx.Tasks/tree/master/src/Nito.AsyncEx.Tasks
namespace nng
{
    /// <summary>
    /// Holds the task for a cancellation token, as well as the token registration. The registration is disposed when this instance is disposed.
    /// </summary>
    public sealed class CancellationTokenTaskSource<T> : IDisposable
    {
        /// <summary>
        /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
        /// </summary>
        private readonly IDisposable _registration;

        /// <summary>
        /// Creates a task for the specified cancellation token, registering with the token if necessary.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
                return;
            }
            Tcs = new TaskCompletionSource<T>();
            _registration = cancellationToken.Register(() => Tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
            Task = Tcs.Task;
        }

        public CancellationTokenTaskSource(CancellationToken cancellationToken, TaskCreationOptions options)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
                return;
            }
            Tcs = new TaskCompletionSource<T>(options);
            _registration = cancellationToken.Register(() => Tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
            Task = Tcs.Task;
        }

        /// <summary>
        /// Gets the task for the source cancellation token.
        /// </summary>
        public Task<T> Task { get; private set; }

        TaskCompletionSource<T> Tcs { get; set; }

        public bool TrySetResult(T result)
        {
            // Using the TrySet functions won't throw exceptions if TrySet has already been called (e.g. cancelled via CancellationToken)
            return Tcs.TrySetResult(result);
        }

        public bool TrySetException(Exception exception)
        {
            return Tcs.TrySetException(exception);
        }

        /// <summary>
        /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="Task"/> to never complete.
        /// </summary>
        public void Dispose()
        {
            _registration?.Dispose();
        }
    }
}