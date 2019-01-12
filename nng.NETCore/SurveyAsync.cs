using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    /// <summary>
    /// In the survey pattern, surveyors broadcast a survey to all respondents.  Respondents can reply within some time limit but don't have to.
    /// There can only be one survey at a time.  Responses received when there is no outstanding survey are discarded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SurveyAsyncContext<T> : SendReceiveAsyncContext<T>
    {
        /// <summary>
        /// Broadcast survey to all peer respondents.
        /// Only one survey can be outstanding at a time; sending another survey will cancel the previous one
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new Task<bool> Send(T message)
        {
            return base.Send(message);
        }

        /// <summary>
        /// Receive replies to a survey from respondents.  Any responses received before a survey is sent or after it times out are discarded.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <c>NNG_ESTATE</c>- attempted to receive with no outstanding survey.
        /// <c>NNG_ETIMEDOUT</c>- survey timed out while waiting for replies.
        /// </returns>
        public new Task<T> Receive(CancellationToken token)
        {
            return base.Receive(token);
        }
    }
}