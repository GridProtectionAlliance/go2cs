//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:23:55 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class @unsafe_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ArbitraryType
        {
            // Value of the ArbitraryType struct
            private readonly nint m_value;
            
            public ArbitraryType(nint value) => m_value = value;

            // Enable implicit conversions between nint and ArbitraryType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ArbitraryType(nint value) => new ArbitraryType(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(ArbitraryType value) => value.m_value;
            
            // Enable comparisons between nil and ArbitraryType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ArbitraryType value, NilType nil) => value.Equals(default(ArbitraryType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ArbitraryType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ArbitraryType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ArbitraryType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ArbitraryType(NilType nil) => default(ArbitraryType);
        }
    }
}
