//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go {
namespace @internal
{
    public static partial class aliases_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct A4
        {
            // Value of the A4 struct
            private readonly T4 m_value;
            
            public A4(T4 value) => m_value = value;

            // Enable implicit conversions between T4 and A4 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A4(T4 value) => new A4(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T4(A4 value) => value.m_value;
            
            // Enable comparisons between nil and A4 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(A4 value, NilType nil) => value.Equals(default(A4));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(A4 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, A4 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, A4 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A4(NilType nil) => default(A4);
        }
    }
}}}
