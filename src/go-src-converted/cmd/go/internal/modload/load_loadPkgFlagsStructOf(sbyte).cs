//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:31:42 UTC
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
    public static partial class modload_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct loadPkgFlags
        {
            // Value of the loadPkgFlags struct
            private readonly sbyte m_value;
            
            public loadPkgFlags(sbyte value) => m_value = value;

            // Enable implicit conversions between sbyte and loadPkgFlags struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator loadPkgFlags(sbyte value) => new loadPkgFlags(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sbyte(loadPkgFlags value) => value.m_value;
            
            // Enable comparisons between nil and loadPkgFlags struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(loadPkgFlags value, NilType nil) => value.Equals(default(loadPkgFlags));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(loadPkgFlags value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, loadPkgFlags value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, loadPkgFlags value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator loadPkgFlags(NilType nil) => default(loadPkgFlags);
        }
    }
}}}}
