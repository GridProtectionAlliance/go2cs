//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:17:06 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class crypto_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Hash
        {
            // Value of the Hash struct
            private readonly nuint m_value;
            
            public Hash(nuint value) => m_value = value;

            // Enable implicit conversions between nuint and Hash struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Hash(nuint value) => new Hash(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nuint(Hash value) => value.m_value;
            
            // Enable comparisons between nil and Hash struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Hash value, NilType nil) => value.Equals(default(Hash));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Hash value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Hash value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Hash value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Hash(NilType nil) => default(Hash);
        }
    }
}