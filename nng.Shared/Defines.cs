using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native
{
    public sealed partial class Defines
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PipeEventCallback(nng_pipe pipe, NngPipeEv ev, IntPtr arg);

        /// <summary>
        /// Maximum number of scatter/gather iov supported (see <a href=https://nng.nanomsg.org/man/v1.3.2/nng_aio_set_iov.3.html>nng_aio_set_iov</a>)
        /// </summary>
        public const int MAX_IOV_BUFFERS = 8;

        public const int NNG_MAXADDRLEN = 128;

        public const int NNG_DURATION_INFINITE = -1;
        public const int NNG_DURATION_DEFAULT = -2;
        public const int NNG_DURATION_ZERO = 0;

        #region Flags/nng_flag_enum
        [Flags]
        public enum NngFlag : Int32
        {
            NNG_FLAG_ALLOC = 1,
            NNG_FLAG_NONBLOCK = 2,
        }
        #endregion

        #region Get/SetOpt
        public const string NNG_OPT_SOCKNAME = "socket-name";
        public const string NNG_OPT_RAW = "raw";
        public const string NNG_OPT_PROTO = "protocol";
        public const string NNG_OPT_PROTONAME = "protocol-name";
        public const string NNG_OPT_PEER = "peer";
        public const string NNG_OPT_PEERNAME = "peer-name";
        public const string NNG_OPT_RECVBUF = "recv-buffer";
        public const string NNG_OPT_SENDBUF = "send-buffer";
        public const string NNG_OPT_RECVFD = "recv-fd";
        public const string NNG_OPT_SENDFD = "send-fd";
        public const string NNG_OPT_RECVTIMEO = "recv-timeout";
        public const string NNG_OPT_SENDTIMEO = "send-timeout";
        public const string NNG_OPT_LOCADDR = "local-address";
        public const string NNG_OPT_REMADDR = "remote-address";
        public const string NNG_OPT_URL = "url";
        public const string NNG_OPT_MAXTTL = "ttl-max";
        public const string NNG_OPT_RECVMAXSZ = "recv-size-max";
        public const string NNG_OPT_RECONNMINT = "reconnect-time-min";
        public const string NNG_OPT_RECONNMAXT = "reconnect-time-max";

        public const string NNG_OPT_TLS_CONFIG = "tls-config";
        public const string NNG_OPT_TLS_AUTH_MODE = "tls-authmode";
        public const string NNG_OPT_TLS_CERT_KEY_FILE = "tls-cert-key-file";
        public const string NNG_OPT_TLS_CA_FILE = "tls-ca-file";
        public const string NNG_OPT_TLS_SERVER_NAME = "tls-server-name";
        public const string NNG_OPT_TLS_VERIFIED = "tls-verified";
        public const string NNG_OPT_TCP_NODELAY = "tcp-nodelay";
        public const string NNG_OPT_TCP_KEEPALIVE = "tcp-keepalive";
        public const string NNG_OPT_TCP_BOUND_PORT = "tcp-bound-port";

        public const string NNG_OPT_IPC_SECURITY_DESCRIPTOR = "ipc:security-descriptor";
        public const string NNG_OPT_IPC_PERMISSIONS = "ipc:permissions";
        public const string NNG_OPT_IPC_PEER_UID = "ipc:peer-uid";
        public const string NNG_OPT_IPC_PEER_GID = "ipc:peer-gid";
        public const string NNG_OPT_IPC_PEER_PID = "ipc:peer-pid";
        public const string NNG_OPT_IPC_PEER_ZONEID = "ipc:peer-zoneid";

        public const string NNG_OPT_WS_REQUEST_HEADERS = "ws:request-headers";
        public const string NNG_OPT_WS_RESPONSE_HEADERS = "ws:response-headers";
        public const string NNG_OPT_WS_RESPONSE_HEADER = "ws:response-header:";
        public const string NNG_OPT_WS_REQUEST_HEADER = "ws:request-header:";
        public const string NNG_OPT_WS_REQUEST_URI = "ws:request-uri";
        public const string NNG_OPT_WS_SENDMAXFRAME = "ws:txframe-max";
        public const string NNG_OPT_WS_RECVMAXFRAME = "ws:rxframe-max";
        public const string NNG_OPT_WS_PROTOCOL = "ws:protocol";

        public const string NNG_OPT_REQ_RESENDTIME = "req:resend-time";

        public const string NNG_OPT_SUB_SUBSCRIBE = "sub:subscribe";
        public const string NNG_OPT_SUB_UNSUBSCRIBE = "sub:unsubscribe";
        public const string NNG_OPT_SUB_PREFNEW = "sub:prefnew";

        [Obsolete("pair v1 polyamorous mode is deprecated in NNG v1.3.0")]
        public const string NNG_OPT_PAIR1_POLY = "pair1:polyamorous";

        public const string NNG_OPT_SURVEYOR_SURVEYTIME = "surveyor:survey-time";

        public const string NNG_OPT_WSS_REQUEST_HEADERS = NNG_OPT_WS_REQUEST_HEADERS;
        public const string NNG_OPT_WSS_RESPONSE_HEADERS = NNG_OPT_WS_RESPONSE_HEADERS;
        #endregion

        #region nng_errno_enum
        public const int NNG_OK = 0;
        public const int NNG_EINTR = 1;
        public const int NNG_ENOMEM = 2;
        public const int NNG_EINVAL = 3;
        public const int NNG_EBUSY = 4;
        public const int NNG_ETIMEDOUT = 5;
        public const int NNG_ECONNREFUSED = 6;
        public const int NNG_ECLOSED = 7;
        public const int NNG_EAGAIN = 8;
        public const int NNG_ENOTSUP = 9;
        public const int NNG_EADDRINUSE = 10;
        public const int NNG_ESTATE = 11;
        public const int NNG_ENOENT = 12;
        public const int NNG_EPROTO = 13;
        public const int NNG_EUNREACHABLE = 14;
        public const int NNG_EADDRINVAL = 15;
        public const int NNG_EPERM = 16;
        public const int NNG_EMSGSIZE = 17;
        public const int NNG_ECONNABORTED = 18;
        public const int NNG_ECONNRESET = 19;
        public const int NNG_ECANCELED = 20;
        public const int NNG_ENOFILES = 21;
        public const int NNG_ENOSPC = 22;
        public const int NNG_EEXIST = 23;
        public const int NNG_EREADONLY = 24;
        public const int NNG_EWRITEONLY = 25;
        public const int NNG_ECRYPTO = 26;
        public const int NNG_EPEERAUTH = 27;
        public const int NNG_ENOARG = 28;
        public const int NNG_EAMBIGUOUS = 29;
        public const int NNG_EBADTYPE = 30;
        public const int NNG_ECONNSHUT = 31;
        public const int NNG_EINTERNAL = 1000;
        public const int NNG_ESYSERR = 0x10000000;
        public const int NNG_ETRANERR = 0x20000000;

        public enum NngErrno
        {
            EINTR = NNG_EINTR,
            ENOMEM = NNG_ENOMEM,
            EINVAL = NNG_EINVAL,
            EBUSY = NNG_EBUSY,
            ETIMEDOUT = NNG_ETIMEDOUT,
            ECONNREFUSED = NNG_ECONNREFUSED,
            ECLOSED = NNG_ECLOSED,
            EAGAIN = NNG_EAGAIN,
            ENOTSUP = NNG_ENOTSUP,
            EADDRINUSE = NNG_EADDRINUSE,
            ESTATE = NNG_ESTATE,
            ENOENT = NNG_ENOENT,
            EPROTO = NNG_EPROTO,
            EUNREACHABLE = NNG_EUNREACHABLE,
            EADDRINVAL = NNG_EADDRINVAL,
            EPERM = NNG_EPERM,
            EMSGSIZE = NNG_EMSGSIZE,
            ECONNABORTED = NNG_ECONNABORTED,
            ECONNRESET = NNG_ECONNRESET,
            ECANCELED = NNG_ECANCELED,
            ENOFILES = NNG_ENOFILES,
            ENOSPC = NNG_ENOSPC,
            EEXIST = NNG_EEXIST,
            EREADONLY = NNG_EREADONLY,
            EWRITEONLY = NNG_EWRITEONLY,
            ECRYPTO = NNG_ECRYPTO,
            EPEERAUTH = NNG_EPEERAUTH,
            ENOARG = NNG_ENOARG,
            EAMBIGUOUS = NNG_EAMBIGUOUS,
            EBADTYPE = NNG_EBADTYPE,
            ECONNSHUT = NNG_ECONNSHUT,
            EINTERNAL = NNG_EINTERNAL,
            ESYSERR = NNG_ESYSERR,
            ETRANERR = NNG_ETRANERR,
        }
        #endregion

        #region nng_pipe_ev
        public const int NNG_PIPE_EV_ADD_PRE = 0;
        public const int NNG_PIPE_EV_ADD_POST = 1;
        public const int NNG_PIPE_EV_REM_POST = 2;

        public enum NngPipeEv
        {
            AddPre = NNG_PIPE_EV_ADD_PRE,
            AddPost = NNG_PIPE_EV_ADD_POST,
            RemPost = NNG_PIPE_EV_REM_POST,
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_ctx
    {
        public UInt32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_dialer
    {
        public UInt32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_listener
    {
        public UInt32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_pipe
    {
        public UInt32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_socket
    {
        public UInt32 id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_aio
    {
        IntPtr ptr;
        public static readonly nng_aio Null = new nng_aio { ptr = (IntPtr)(-1) };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_msg
    {
        IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_duration
    {
        public Int32 TimeMs { get; set; }

        public nng_duration(nng_duration copy)
        {
            TimeMs = copy.TimeMs;
        }

        
        ///<sumary>
        /// a context-specific default value should be used.  See `NNG_DURATION_INFINITE`.
        ///</summary>
        public static ref readonly nng_duration Infinite => ref infinite;
        ///<sumary>
        /// a context-specific default value should be used.  See `NNG_DURATION_DEFAULT`.
        ///</summary>
        public static ref readonly nng_duration Default => ref defaultDuration;
        ///<sumary>
        /// a context-specific default value should be used.  See `NNG_DURATION_ZERO`.
        ///</summary>
        public static ref readonly nng_duration Zero => ref zero;
        
        static readonly nng_duration infinite = new nng_duration { TimeMs = Defines.NNG_DURATION_INFINITE };
        static readonly nng_duration defaultDuration = new nng_duration { TimeMs = Defines.NNG_DURATION_DEFAULT };
        static readonly nng_duration zero = new nng_duration { TimeMs = Defines.NNG_DURATION_ZERO };

        public static nng_duration operator +(nng_duration lhs, nng_duration rhs) =>
            new nng_duration { TimeMs = lhs.TimeMs + rhs.TimeMs };

        public static nng_duration operator +(nng_duration lhs, int rhs) =>
            new nng_duration { TimeMs = lhs.TimeMs + rhs };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_iov
    {
        public IntPtr iov_buf;
        public UIntPtr iov_len;
    }

    public enum nng_stat_type_enum
    {
        NNG_STAT_SCOPE = 0, // Stat is for scoping, and carries no value
        NNG_STAT_LEVEL = 1, // Numeric "absolute" value, diffs meaningless
        NNG_STAT_COUNTER = 2, // Incrementing value (diffs are meaningful)
        NNG_STAT_STRING = 3, // Value is a string
        NNG_STAT_BOOLEAN = 4, // Value is a boolean
        NNG_STAT_ID = 5, // Value is a numeric ID
    };

    public enum nng_unit_enum
    {
        NNG_UNIT_NONE = 0, // No special units
        NNG_UNIT_BYTES = 1, // Bytes, e.g. bytes sent, etc.
        NNG_UNIT_MESSAGES = 2, // Messages, one per message
        NNG_UNIT_MILLIS = 3, // Milliseconds
        NNG_UNIT_EVENTS = 4  // Some other type of event
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_stat
    {
        IntPtr ptr;

        public bool IsNull => ptr == IntPtr.Zero;
    }

#region sockaddr
    public enum nng_sockaddr_family : UInt16
    {
        NNG_AF_UNSPEC = 0,
        NNG_AF_INPROC = 1,
        NNG_AF_IPC = 2,
        NNG_AF_INET = 3,
        NNG_AF_INET6 = 4,
        NNG_AF_ZT = 5 // ZeroTier
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct nng_sockaddr
    {
        [FieldOffset(0)]
        public nng_sockaddr_family s_family;
        [FieldOffset(0)]
        public nng_sockaddr_ipc s_ipc;
        [FieldOffset(0)]
        public nng_sockaddr_inproc s_inproc;
        [FieldOffset(0)]
        public nng_sockaddr_in6 s_in6;
        [FieldOffset(0)]
        public nng_sockaddr_in s_in;
        [FieldOffset(0)]
        public nng_sockaddr_zt s_zt;
    };

    /// <summary>
    /// In nng this is a typedef:
    /// typedef struct nng_sockaddr_path nng_sockaddr_ipc;
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_ipc
    {
        public nng_sockaddr_path sockaddr_path;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_inproc
    {
        public UInt16 sa_family;
        char_maxaddrlen_blob sa_name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_in6
    {
        public nng_sockaddr_family sa_family;
        public UInt16 sa_port;
        uint8_16_blob sa_addr;
    };

    /// <summary>
    /// In nng this is a typedef:
    /// typedef struct nng_sockaddr_in6 nng_sockaddr_udp6;
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_udp6
    {
        public nng_sockaddr_in6 sockaddr_path;
    }

    /// <summary>
    /// In nng this is a typedef:
    /// typedef struct nng_sockaddr_in6 nng_sockaddr_tcp6;
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_tcp6
    {
        public nng_sockaddr_in6 sockaddr_path;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_in
    {
        public nng_sockaddr_family sa_family;
        public UInt16 sa_port;
        public UInt32 sa_addr;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_zt
    {
        public nng_sockaddr_family sa_family;
        public UInt64 sa_nwid;
        public UInt64 sa_nodeid;
        public UInt32 sa_port;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_sockaddr_path
    {
        public nng_sockaddr_family sa_family;
        char_maxaddrlen_blob sa_path;
    };

    // Could instead write sa_path member of nng_sockr_path as:
    // fixed char sa_path[NNG_MAXADDRLEN];
    // But `fixed` requires turn on unsafe code for whole assembly....
    [StructLayout(LayoutKind.Sequential, Size = Defines.NNG_MAXADDRLEN)]
    struct char_maxaddrlen_blob
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    struct uint8_16_blob
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_stream
    {
        IntPtr opaque_ptr;
        public static readonly nng_stream Null = new nng_stream { opaque_ptr = IntPtr.Zero };

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public nng_stream(IntPtr ptr)
        {
            opaque_ptr = ptr;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_stream_dialer
    {
        IntPtr opaque_ptr;
        public static readonly nng_stream_dialer Null = new nng_stream_dialer { opaque_ptr = IntPtr.Zero };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct nng_stream_listener
    {
        IntPtr opaque_ptr;
        public static readonly nng_stream_listener Null = new nng_stream_listener { opaque_ptr = IntPtr.Zero };
    }

#endregion
}