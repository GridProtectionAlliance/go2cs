//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:08:39 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytealg = go.@internal.bytealg_package;

#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct errorAddressString
        {
            // Constructors
            public errorAddressString(NilType _)
            {
                this.msg = default;
                this.addr = default;
            }

            public errorAddressString(@string msg = default, System.UIntPtr addr = default)
            {
                this.msg = msg;
                this.addr = addr;
            }

            // Enable comparisons between nil and errorAddressString struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(errorAddressString value, NilType nil) => value.Equals(default(errorAddressString));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(errorAddressString value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, errorAddressString value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, errorAddressString value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator errorAddressString(NilType nil) => default(errorAddressString);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static errorAddressString errorAddressString_cast(dynamic value)
        {
            return new errorAddressString(value.msg, value.addr);
        }
    }
}