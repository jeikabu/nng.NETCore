using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    ///<summary>
    ///</summary>
    ///<remarks>
    /// https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-4-asyncbarrier/
    ///</remarks>
    public class AsyncBarrier
    {
        public AsyncBarrier(int participantCount)
        {
            if (participantCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException("participantCount");
            }
            m_remainingParticipants = m_participantCount = participantCount;
        }

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

        private readonly int m_participantCount;
        private TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();
        private int m_remainingParticipants;
    }
}