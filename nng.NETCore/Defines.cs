using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Pinvoke
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
        IntPtr ptr;
    }

    
}