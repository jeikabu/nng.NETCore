using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    ///<summary>
    /// Synchronization barrier where participants can signify they reach the barrier and wait for the other participants.
    ///</summary>
    ///<remarks>
    /// https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-4-asyncbarrier/
    ///</remarks>
    public class AsyncBarrier
    {
        /// <summary>
        /// Create new instance for specified number of participants.
        /// </summary>
        /// <param name="participantCount">Number of participants</param>
        public AsyncBarrier(int participantCount)
        {
            if (participantCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException("participantCount");
            }
            m_remainingParticipants = m_participantCount = participantCount;
        }

        /// <summary>
        /// Signal a participant has reached the barrier and return a task to await the remaining participants.
        /// </summary>
        /// <returns>Task to await all participants have signalled barrier.</returns>
        public Task SignalAndWait()
        {
            var tcs = m_tcs;
            if (Interlocked.Decrement(ref m_remainingParticipants) == 0)
            {
                m_remainingParticipants = m_participantCount;
                m_tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
            }
            return tcs.Task;
        }

        ///<summary>
        /// SignalAndWait() may create a new TaskCompletionSource() for the next barrier and you may wait on the wrong one.
        ///</summary>
        public Task WaitAsync() => m_tcs.Task;

        private readonly int m_participantCount;
        private TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();
        private int m_remainingParticipants;
    }
}