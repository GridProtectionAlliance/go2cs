//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:28:40 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @unsafe = go.@unsafe_package;
using go;

#nullable enable

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct profMapEntry
        {
            // Constructors
            public profMapEntry(NilType _)
            {
                this.nextHash = default;
                this.nextAll = default;
                this.stk = default;
                this.tag = default;
                this.count = default;
            }

            public profMapEntry(ref ptr<profMapEntry> nextHash = default, ref ptr<profMapEntry> nextAll = default, slice<System.UIntPtr> stk = default, unsafe.Pointer tag = default, long count = default)
            {
                this.nextHash = nextHash;
                this.nextAll = nextAll;
                this.stk = stk;
                this.tag = tag;
                this.count = count;
            }

            // Enable comparisons between nil and profMapEntry struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(profMapEntry value, NilType nil) => value.Equals(default(profMapEntry));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(profMapEntry value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, profMapEntry value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, profMapEntry value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator profMapEntry(NilType nil) => default(profMapEntry);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static profMapEntry profMapEntry_cast(dynamic value)
        {
            return new profMapEntry(ref value.nextHash, ref value.nextAll, value.stk, value.tag, value.count);
        }
    }
}}