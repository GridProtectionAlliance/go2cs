//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:19:42 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using sort = go.sort_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Slice
        {
            // Constructors
            public Slice(NilType _)
            {
                this.elem = default;
            }

            public Slice(Type elem = default)
            {
                this.elem = elem;
            }

            // Enable comparisons between nil and Slice struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Slice value, NilType nil) => value.Equals(default(Slice));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Slice value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Slice value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Slice value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Slice(NilType nil) => default(Slice);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Slice Slice_cast(dynamic value)
        {
            return new Slice(value.elem);
        }
    }
}}