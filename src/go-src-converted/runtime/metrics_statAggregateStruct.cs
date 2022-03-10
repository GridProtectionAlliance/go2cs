//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:09:28 UTC
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
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct statAggregate
        {
            // Constructors
            public statAggregate(NilType _)
            {
                this.ensured = default;
                this.heapStats = default;
                this.sysStats = default;
            }

            public statAggregate(statDepSet ensured = default, heapStatsAggregate heapStats = default, sysStatsAggregate sysStats = default)
            {
                this.ensured = ensured;
                this.heapStats = heapStats;
                this.sysStats = sysStats;
            }

            // Enable comparisons between nil and statAggregate struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(statAggregate value, NilType nil) => value.Equals(default(statAggregate));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(statAggregate value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, statAggregate value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, statAggregate value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator statAggregate(NilType nil) => default(statAggregate);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static statAggregate statAggregate_cast(dynamic value)
        {
            return new statAggregate(value.ensured, value.heapStats, value.sysStats);
        }
    }
}