//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:53:31 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using token = go.go.token_package;
using atomic = go.sync.atomic_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Map
        {
            // Constructors
            public Map(NilType _)
            {
                this.key = default;
                this.elem = default;
            }

            public Map(Type key = default, Type elem = default)
            {
                this.key = key;
                this.elem = elem;
            }

            // Enable comparisons between nil and Map struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Map value, NilType nil) => value.Equals(default(Map));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Map value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Map value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Map value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Map(NilType nil) => default(Map);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Map Map_cast(dynamic value)
        {
            return new Map(value.key, value.elem);
        }
    }
}}