//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:20:02 UTC
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
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct ExtKeyUsage
        {
            // Value of the ExtKeyUsage struct
            private readonly nint m_value;
            
            public ExtKeyUsage(nint value) => m_value = value;

            // Enable implicit conversions between nint and ExtKeyUsage struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ExtKeyUsage(nint value) => new ExtKeyUsage(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(ExtKeyUsage value) => value.m_value;
            
            // Enable comparisons between nil and ExtKeyUsage struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ExtKeyUsage value, NilType nil) => value.Equals(default(ExtKeyUsage));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ExtKeyUsage value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ExtKeyUsage value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ExtKeyUsage value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ExtKeyUsage(NilType nil) => default(ExtKeyUsage);
        }
    }
}}