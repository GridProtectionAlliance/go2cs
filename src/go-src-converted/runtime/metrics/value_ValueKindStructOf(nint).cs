//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:28:34 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace runtime
{
    public static partial class metrics_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ValueKind
        {
            // Value of the ValueKind struct
            private readonly nint m_value;
            
            public ValueKind(nint value) => m_value = value;

            // Enable implicit conversions between nint and ValueKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ValueKind(nint value) => new ValueKind(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(ValueKind value) => value.m_value;
            
            // Enable comparisons between nil and ValueKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ValueKind value, NilType nil) => value.Equals(default(ValueKind));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ValueKind value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ValueKind value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ValueKind value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ValueKind(NilType nil) => default(ValueKind);
        }
    }
}}
