//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:41:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct exprKind
        {
            // Value of the exprKind struct
            private readonly nint m_value;
            
            public exprKind(nint value) => m_value = value;

            // Enable implicit conversions between nint and exprKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator exprKind(nint value) => new exprKind(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(exprKind value) => value.m_value;
            
            // Enable comparisons between nil and exprKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(exprKind value, NilType nil) => value.Equals(default(exprKind));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(exprKind value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, exprKind value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, exprKind value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator exprKind(NilType nil) => default(exprKind);
        }
    }
}}