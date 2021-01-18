using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Stats.UnsafeNativeMethods;

    public abstract class NngStat : INngStat
    {
        public nng_stat NativeNngStruct { get; protected set; }

        public string Name => nng_stat_name_string(NativeNngStruct);
        public string Desc => nng_stat_desc_string(NativeNngStruct);
        public nng_stat_type_enum Type => nng_stat_type(NativeNngStruct);
        public UInt64 Value => nng_stat_value(NativeNngStruct);
        public string ValueString => nng_stat_string_string(NativeNngStruct);
        public nng_unit_enum Unit => nng_stat_unit(NativeNngStruct);
        public UInt64 Timestamp => nng_stat_timestamp(NativeNngStruct);
        public IStatChild Next()
        {
            var next = nng_stat_next(NativeNngStruct);
            return StatChild.Create(next);
        }
        public IStatChild Child()
        {
            var child = nng_stat_child(NativeNngStruct);
            return StatChild.Create(child);
        }

        public static NngResult<IStatRoot> GetStatSnapshot()
        {
            var res = nng_stats_get(out nng_stat statsp);
            return NngResult<IStatRoot>.OkThen(res, () => new StatRoot { NativeNngStruct = statsp });
        }
    }

    public class StatRoot : NngStat, IStatRoot
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
                nng_stats_free(NativeNngStruct);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public class StatChild : NngStat, IStatChild
    {
        public static IStatChild Create(nng_stat stat)
        {
            return new StatChild { NativeNngStruct = stat };
        }

        public IEnumerator<IStatChild> GetEnumerator()
        {
            if (NativeNngStruct.IsNull)
                yield break;
            yield return this;
            var next = Next();
            while (!next.NativeNngStruct.IsNull)
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