using System;

namespace nng
{
    public interface INngStreamDialer : IOptions, IDisposable
    {
        void Dial(INngAio aio);
        void Close();
    }

    public interface INngStreamListener : IOptions, IDisposable
    {
        NngResult<Unit> Listen();
        void Accept(INngAio aio);
        void Close();
    }

    public interface INngStream : IOptions, IDisposable
    {
        void Send(INngAio aio);
        void Recv(INngAio aio);
        void Close();
    }
}