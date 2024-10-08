//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:25:55 UTC
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
        private partial struct blockRecord
        {
            // Constructors
            public blockRecord(NilType _)
            {
                this.count = default;
                this.cycles = default;
            }

            public blockRecord(double count = default, long cycles = default)
            {
                this.count = count;
                this.cycles = cycles;
            }

            // Enable comparisons between nil and blockRecord struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(blockRecord value, NilType nil) => value.Equals(default(blockRecord));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(blockRecord value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, blockRecord value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, blockRecord value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator blockRecord(NilType nil) => default(blockRecord);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static blockRecord blockRecord_cast(dynamic value)
        {
            return new blockRecord(value.count, value.cycles);
        }
    }
}