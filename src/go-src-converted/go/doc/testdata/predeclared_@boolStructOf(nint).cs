//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:40 UTC
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
    public static partial class predeclared_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct @bool
        {
            // Value of the @bool struct
            private readonly nint m_value;
            
            public @bool(nint value) => m_value = value;

            // Enable implicit conversions between nint and @bool struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator @bool(nint value) => new @bool(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(@bool value) => value.m_value;
            
            // Enable comparisons between nil and @bool struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(@bool value, NilType nil) => value.Equals(default(@bool));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(@bool value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, @bool value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, @bool value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator @bool(NilType nil) => default(@bool);
        }
    }
}}
