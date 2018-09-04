using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native
{
    sealed class Globals
    {
        public const string NngDll = "nng";

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
    }

    enum nng_errno_enum {
        NNG_EINTR        = 1,
        NNG_ENOMEM       = 2,
        NNG_EINVAL       = 3,
        NNG_EBUSY        = 4,
        NNG_ETIMEDOUT    = 5,
        NNG_ECONNREFUSED = 6,
        NNG_ECLOSED      = 7,
        NNG_EAGAIN       = 8,
        NNG_ENOTSUP      = 9,
        NNG_EADDRINUSE   = 10,
        NNG_ESTATE       = 11,
        NNG_ENOENT       = 12,
        NNG_EPROTO       = 13,
        NNG_EUNREACHABLE = 14,
        NNG_EADDRINVAL   = 15,
        NNG_EPERM        = 16,
        NNG_EMSGSIZE     = 17,
        NNG_ECONNABORTED = 18,
        NNG_ECONNRESET   = 19,
        NNG_ECANCELED    = 20,
        NNG_ENOFILES     = 21,
        NNG_ENOSPC       = 22,
        NNG_EEXIST       = 23,
        NNG_EREADONLY    = 24,
        NNG_EWRITEONLY   = 25,
        NNG_ECRYPTO      = 26,
        NNG_EPEERAUTH    = 27,
        NNG_ENOARG       = 28,
        NNG_EAMBIGUOUS   = 29,
        NNG_EBADTYPE     = 30,
        NNG_EINTERNAL    = 1000,
        NNG_ESYSERR      = 0x10000000,
        NNG_ETRANERR     = 0x20000000
};

    public struct nng_ctx
    {
        public UInt32 id;
    }
    public struct nng_dialer
    {
        public UInt32 id;
    }
    public struct nng_listener
    {
        public UInt32 id;
    }
    public struct nng_pipe
    {
        public UInt32 id;
    }
    public struct nng_socket
    {
        public UInt32 id;
    }
    public struct nng_aio
    {
        IntPtr ptr;
        public static readonly nng_aio Null = new nng_aio { ptr = (IntPtr)(-1) };
    }
    public struct nng_msg
    {
        IntPtr ptr;
    }
    public struct nng_duration
    {
        public UInt32 timeMS;
    }
    public struct nng_iov
    {
        #pragma warning disable CS0169
        IntPtr ptr;
    }
}