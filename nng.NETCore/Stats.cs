using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Stats.UnsafeNativeMethods;

    public abstract class Stat : IStat
    {
        public nng_stat NngStat { get; protected set; }

        public string Name => nng_stat_name_string(NngStat);
        public string Desc => nng_stat_desc_string(NngStat);
        public nng_stat_type_enum Type => nng_stat_type(NngStat);
        public UInt64 Value => nng_stat_value(NngStat);
        public string ValueString => nng_stat_string_string(NngStat);
        public nng_unit_enum Unit => nng_stat_unit(NngStat);
        public UInt64 Timestamp => nng_stat_timestamp(NngStat);
        public IStatChild Next()
        {
            var next = nng_stat_next(NngStat);
            return StatChild.Create(next);
        }
        public IStatChild Child()
        {
            var child = nng_stat_child(NngStat);
            return StatChild.Create(child);
        }

        public static NngResult<IStatRoot> GetStatSnapshot()
        {
            var res = nng_stats_get(out nng_stat statsp);
            return NngResult<IStatRoot>.OkThen(res, () => new StatRoot { NngStat = statsp });
        }
    }

    public class StatRoot : Stat, IStatRoot
    {
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                nng_stats_free(NngStat);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public class StatChild : Stat, IStatChild
    {
        public static IStatChild Create(nng_stat stat)
        {
            return new StatChild { NngStat = stat };
        }

        public IEnumerator<IStatChild> GetEnumerator()
        {
            if (NngStat.IsNull)
                yield break;
            yield return this;
            var next = Next();
            while (!next.NngStat.IsNull)
            {
                yield return next;
                next = next.Next();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}