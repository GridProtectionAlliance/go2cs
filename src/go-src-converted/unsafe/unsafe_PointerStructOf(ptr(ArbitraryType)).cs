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
        public partial struct Pointer
        {
            // Value of the Pointer struct
            private readonly ptr<ArbitraryType> m_value;
            
            public Pointer(ptr<ArbitraryType> value) => m_value = value;

            // Enable implicit conversions between ptr<ArbitraryType> and Pointer struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Pointer(ptr<ArbitraryType> value) => new Pointer(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ptr<ArbitraryType>(Pointer value) => value.m_value;
            
            // Enable comparisons between nil and Pointer struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Pointer value, NilType nil) => value.Equals(default(Pointer));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Pointer value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Pointer value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Pointer value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Pointer(NilType nil) => default(Pointer);
        }
    }
}
