//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:38 UTC
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
    public static partial class b_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct notExported
        {
            // Value of the notExported struct
            private readonly nint m_value;
            
            public notExported(nint value) => m_value = value;

            // Enable implicit conversions between nint and notExported struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator notExported(nint value) => new notExported(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(notExported value) => value.m_value;
            
            // Enable comparisons between nil and notExported struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(notExported value, NilType nil) => value.Equals(default(notExported));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(notExported value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, notExported value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, notExported value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator notExported(NilType nil) => default(notExported);
        }
    }
}}
