//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 08 04:56:22 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using os = go.os_package;
using sync = go.sync_package;
using go;

namespace go {
namespace go {
namespace @internal
{
    public static partial class issue30628_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Apple
        {
            // Constructors
            public Apple(NilType _)
            {
                this.hey = default;
                this.x = default;
            }

            public Apple(sync.RWMutex hey = default, long x = default)
            {
                this.hey = hey;
                this.x = x;
            }

            // Enable comparisons between nil and Apple struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Apple value, NilType nil) => value.Equals(default(Apple));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Apple value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Apple value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Apple value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Apple(NilType nil) => default(Apple);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Apple Apple_cast(dynamic value)
        {
            return new Apple(value.hey, value.x);
        }
    }
}}}