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
        public partial struct A3
        {
            // Value of the A3 struct
            private readonly T3 m_value;
            
            public A3(T3 value) => m_value = value;

            // Enable implicit conversions between T3 and A3 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A3(T3 value) => new A3(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T3(A3 value) => value.m_value;
            
            // Enable comparisons between nil and A3 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(A3 value, NilType nil) => value.Equals(default(A3));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(A3 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, A3 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, A3 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A3(NilType nil) => default(A3);
        }
    }
}}}
