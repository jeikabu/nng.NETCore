using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Respondent version 0 socket for survey protocol
    /// </summary>
    public class RespondentSocket : Socket, IRespondentSocket
    {
        /// <summary>
        /// Create a respondent socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IRespondentSocket> Open()
        {
            int res = nng_respondent0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IRespondentSocket>(res);
            }
            return NngResult.Ok<IRespondentSocket>(new RespondentSocket { NngSocket = socket });
        }

        private RespondentSocket() { }
    }

    /// <summary>
    /// Surveyor version 0 socket for survey protocol
    /// </summary>
    public class SurveyorSocket : Socket, ISurveyorSocket
    {
        /// <summary>
        /// Create a surveyor socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<ISurveyorSocket> Open()
        {
            int res = nng_surveyor0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<ISurveyorSocket>(res);
            }
            return NngResult.Ok<ISurveyorSocket>(new SurveyorSocket { NngSocket = socket });
        }

        private SurveyorSocket() { }
    }
}