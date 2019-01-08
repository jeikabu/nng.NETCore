using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// Represents nng runtime statistic
    /// </summary>
    public interface IStat
    {
        nng_stat NngStat { get; }
        string Name { get; }
        string Desc { get; }
        nng_stat_type_enum Type { get; }
        UInt64 Value { get; }
        string ValueString { get; }
        nng_unit_enum Unit { get; }
        UInt64 Timestamp { get; }
        IStatChild Child();
    }

    /// <summary>
    /// Represents all child/non-root stats
    /// </summary>
    public interface IStatChild : IStat, IEnumerable<IStatChild>
    {
        IStatChild Next();
    }

    /// <summary>
    /// Represents root of nng stat snapshot
    /// </summary>
    public interface IStatRoot : IStat, IDisposable
    {
    }
}
