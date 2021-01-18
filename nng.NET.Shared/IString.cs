using System;

namespace nng
{
    /// <summary>
    /// Represents string allocated by unmanaged nng.
    /// </summary>
    public interface INngString : IDisposable
    {
        string ToManaged();
    }
}