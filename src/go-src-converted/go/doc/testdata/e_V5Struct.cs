//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:40 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class e_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct V5
        {
            // Constructors
            public V5(NilType _)
            {
                this.ptr<V6> = default;
            }

            public V5(ref ptr<V6> ptr<V6> = default)
            {
                this.ptr<V6> = ptr<V6>;
            }

            // Enable comparisons between nil and V5 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(V5 value, NilType nil) => value.Equals(default(V5));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(V5 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, V5 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, V5 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator V5(NilType nil) => default(V5);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static V5 V5_cast(dynamic value)
        {
            return new V5(ref value.ptr<V6>);
        }
    }
}}