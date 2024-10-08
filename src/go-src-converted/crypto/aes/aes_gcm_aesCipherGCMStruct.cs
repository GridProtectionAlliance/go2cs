//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:30:35 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using cipher = go.crypto.cipher_package;
using subtleoverlap = go.crypto.@internal.subtle_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct aesCipherGCM
        {
            // Constructors
            public aesCipherGCM(NilType _)
            {
                this.aesCipherAsm = default;
            }

            public aesCipherGCM(aesCipherAsm aesCipherAsm = default)
            {
                this.aesCipherAsm = aesCipherAsm;
            }

            // Enable comparisons between nil and aesCipherGCM struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(aesCipherGCM value, NilType nil) => value.Equals(default(aesCipherGCM));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(aesCipherGCM value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, aesCipherGCM value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, aesCipherGCM value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator aesCipherGCM(NilType nil) => default(aesCipherGCM);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static aesCipherGCM aesCipherGCM_cast(dynamic value)
        {
            return new aesCipherGCM(value.aesCipherAsm);
        }
    }
}}