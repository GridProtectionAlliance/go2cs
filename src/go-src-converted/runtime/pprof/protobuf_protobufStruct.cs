//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:29:16 UTC
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
namespace runtime
{
    public static partial class pprof_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct protobuf
        {
            // Constructors
            public protobuf(NilType _)
            {
                this.data = default;
                this.tmp = default;
                this.nest = default;
            }

            public protobuf(slice<byte> data = default, array<byte> tmp = default, nint nest = default)
            {
                this.data = data;
                this.tmp = tmp;
                this.nest = nest;
            }

            // Enable comparisons between nil and protobuf struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(protobuf value, NilType nil) => value.Equals(default(protobuf));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(protobuf value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, protobuf value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, protobuf value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator protobuf(NilType nil) => default(protobuf);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static protobuf protobuf_cast(dynamic value)
        {
            return new protobuf(value.data, value.tmp, value.nest);
        }
    }
}}