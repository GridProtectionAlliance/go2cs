//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:16:17 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modget_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct modFlags
        {
            // Value of the modFlags struct
            private readonly nint m_value;
            
            public modFlags(nint value) => m_value = value;

            // Enable implicit conversions between nint and modFlags struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator modFlags(nint value) => new modFlags(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(modFlags value) => value.m_value;
            
            // Enable comparisons between nil and modFlags struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(modFlags value, NilType nil) => value.Equals(default(modFlags));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(modFlags value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, modFlags value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, modFlags value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator modFlags(NilType nil) => default(modFlags);
        }
    }
}}}}