//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:40:14 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace net {
namespace @internal
{
    public static partial class socktest_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct FilterType
        {
            // Value of the FilterType struct
            private readonly nint m_value;
            
            public FilterType(nint value) => m_value = value;

            // Enable implicit conversions between nint and FilterType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator FilterType(nint value) => new FilterType(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(FilterType value) => value.m_value;
            
            // Enable comparisons between nil and FilterType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(FilterType value, NilType nil) => value.Equals(default(FilterType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(FilterType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, FilterType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, FilterType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator FilterType(NilType nil) => default(FilterType);
        }
    }
}}}
