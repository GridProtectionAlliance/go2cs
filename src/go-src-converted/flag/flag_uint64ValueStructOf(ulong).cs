//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:38:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class flag_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct uint64Value
        {
            // Value of the uint64Value struct
            private readonly ulong m_value;
            
            public uint64Value(ulong value) => m_value = value;

            // Enable implicit conversions between ulong and uint64Value struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator uint64Value(ulong value) => new uint64Value(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ulong(uint64Value value) => value.m_value;
            
            // Enable comparisons between nil and uint64Value struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(uint64Value value, NilType nil) => value.Equals(default(uint64Value));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(uint64Value value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, uint64Value value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, uint64Value value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator uint64Value(NilType nil) => default(uint64Value);
        }
    }
}
