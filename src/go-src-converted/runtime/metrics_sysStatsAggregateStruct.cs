//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:25:13 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct sysStatsAggregate
        {
            // Constructors
            public sysStatsAggregate(NilType _)
            {
                this.stacksSys = default;
                this.mSpanSys = default;
                this.mSpanInUse = default;
                this.mCacheSys = default;
                this.mCacheInUse = default;
                this.buckHashSys = default;
                this.gcMiscSys = default;
                this.otherSys = default;
                this.heapGoal = default;
                this.gcCyclesDone = default;
                this.gcCyclesForced = default;
            }

            public sysStatsAggregate(ulong stacksSys = default, ulong mSpanSys = default, ulong mSpanInUse = default, ulong mCacheSys = default, ulong mCacheInUse = default, ulong buckHashSys = default, ulong gcMiscSys = default, ulong otherSys = default, ulong heapGoal = default, ulong gcCyclesDone = default, ulong gcCyclesForced = default)
            {
                this.stacksSys = stacksSys;
                this.mSpanSys = mSpanSys;
                this.mSpanInUse = mSpanInUse;
                this.mCacheSys = mCacheSys;
                this.mCacheInUse = mCacheInUse;
                this.buckHashSys = buckHashSys;
                this.gcMiscSys = gcMiscSys;
                this.otherSys = otherSys;
                this.heapGoal = heapGoal;
                this.gcCyclesDone = gcCyclesDone;
                this.gcCyclesForced = gcCyclesForced;
            }

            // Enable comparisons between nil and sysStatsAggregate struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(sysStatsAggregate value, NilType nil) => value.Equals(default(sysStatsAggregate));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(sysStatsAggregate value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, sysStatsAggregate value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, sysStatsAggregate value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sysStatsAggregate(NilType nil) => default(sysStatsAggregate);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static sysStatsAggregate sysStatsAggregate_cast(dynamic value)
        {
            return new sysStatsAggregate(value.stacksSys, value.mSpanSys, value.mSpanInUse, value.mCacheSys, value.mCacheInUse, value.buckHashSys, value.gcMiscSys, value.otherSys, value.heapGoal, value.gcCyclesDone, value.gcCyclesForced);
        }
    }
}