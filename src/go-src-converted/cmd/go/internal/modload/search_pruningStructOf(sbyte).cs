//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:31:53 UTC
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
        private partial struct pruning
        {
            // Value of the pruning struct
            private readonly sbyte m_value;
            
            public pruning(sbyte value) => m_value = value;

            // Enable implicit conversions between sbyte and pruning struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pruning(sbyte value) => new pruning(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator sbyte(pruning value) => value.m_value;
            
            // Enable comparisons between nil and pruning struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(pruning value, NilType nil) => value.Equals(default(pruning));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(pruning value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, pruning value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, pruning value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pruning(NilType nil) => default(pruning);
        }
    }
}}}}
