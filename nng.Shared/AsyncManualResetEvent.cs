using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// Async version of <see cref="System.Threading.ManualResetEvent"/>.
    /// Notifies one or more waiting threads that an event has occurred.
    /// </summary>
    /// <remarks>
    /// See https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/
    /// </remarks>
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();

        /// <summary>
        /// Get task to await event.
        /// </summary>
        /// <returns>Task to await event</returns>
        public Task WaitAsync() => m_tcs.Task;

        /// <summary>
        /// Signal that the event has occurred.
        /// </summary>
        public void Set()
        {
            m_tcs.TrySetResult(true);
        }

        /// <summary>
        /// Reset event to un-set state.  If event was previously set creates a new event.  If event was not set does nothing.
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                var tcs = m_tcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }
    }
}