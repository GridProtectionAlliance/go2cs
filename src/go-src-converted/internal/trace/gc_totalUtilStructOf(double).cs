//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:35:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct totalUtil
        {
            // Value of the totalUtil struct
            private readonly double m_value;
            
            public totalUtil(double value) => m_value = value;

            // Enable implicit conversions between double and totalUtil struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator totalUtil(double value) => new totalUtil(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator double(totalUtil value) => value.m_value;
            
            // Enable comparisons between nil and totalUtil struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(totalUtil value, NilType nil) => value.Equals(default(totalUtil));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(totalUtil value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, totalUtil value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, totalUtil value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator totalUtil(NilType nil) => default(totalUtil);
        }
    }
}}
