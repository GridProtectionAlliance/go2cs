//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:59 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct KeyUsage
        {
            // Value of the KeyUsage struct
            private readonly nint m_value;
            
            public KeyUsage(nint value) => m_value = value;

            // Enable implicit conversions between nint and KeyUsage struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator KeyUsage(nint value) => new KeyUsage(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(KeyUsage value) => value.m_value;
            
            // Enable comparisons between nil and KeyUsage struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(KeyUsage value, NilType nil) => value.Equals(default(KeyUsage));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(KeyUsage value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, KeyUsage value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, KeyUsage value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator KeyUsage(NilType nil) => default(KeyUsage);
        }
    }
}}
