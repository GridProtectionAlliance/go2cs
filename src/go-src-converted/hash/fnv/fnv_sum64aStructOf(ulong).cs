//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:29:01 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace hash
{
    public static partial class fnv_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct sum64a
        {
            // Value of the sum64a struct
            private readonly ulong m_value;
            
            public sum64a(ulong value) => m_value = value;

            // Enable implicit conversions between ulong and sum64a struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sum64a(ulong value) => new sum64a(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ulong(sum64a value) => value.m_value;
            
            // Enable comparisons between nil and sum64a struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(sum64a value, NilType nil) => value.Equals(default(sum64a));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(sum64a value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, sum64a value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, sum64a value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sum64a(NilType nil) => default(sum64a);
        }
    }
}}
