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
        /// <summary>
        /// Returns numeric value for the stat.
        /// </summary>
        /// <value>
        /// If Type is NNG_STAT_BOOLEAN then 0 is <c>false</c> and 1 is <c>true</c>.
        /// If stat is not a numeric type 0 is returned.
        /// </value>
        UInt64 Value { get; }
        /// <summary>
        /// Returns string value for the stat if Type is NNG_STAT_STRING, otherwise <c>null</c>.
        /// </summary>
        /// <value></value>
        string ValueString { get; }
        nng_unit_enum Unit { get; }
        UInt64 Timestamp { get; }
        IStatChild Child();
    }

    /// <summary>
    /// Represents all child/non-root stats.  Can iterate over self and siblings.
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
