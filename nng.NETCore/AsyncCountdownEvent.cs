using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// Async version of <see cref="System.Threading.CountdownEvent"/>
    /// </summary>
    /// <remarks>
    /// See https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-3-asynccountdownevent/
    /// </remarks>
    public class AsyncCountdownEvent
    {
        public AsyncCountdownEvent(int initialCount)
        {
            if (initialCount <= 0)
                throw new ArgumentOutOfRangeException("initialCount");
            m_count = initialCount;
        }

        public Task WaitAsync() => m_amre.WaitAsync();

        public int Signal()
        {
            if (m_count <= 0)
                throw new InvalidOperationException();

            int newCount = Interlocked.Decrement(ref m_count);
            if (newCount == 0)
                m_amre.Set();
            else if (newCount < 0)
                throw new InvalidOperationException();
            return newCount;
        }

        public Task SignalAndWait()
        {
            Signal();
            return WaitAsync();
        }

        private readonly AsyncManualResetEvent m_amre = new AsyncManualResetEvent();
        private int m_count;
    }
}