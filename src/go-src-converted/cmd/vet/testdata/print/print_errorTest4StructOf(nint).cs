//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:56 UTC
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
namespace vet {
namespace testdata
{
    public static partial class print_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct errorTest4
        {
            // Value of the errorTest4 struct
            private readonly nint m_value;
            
            public errorTest4(nint value) => m_value = value;

            // Enable implicit conversions between nint and errorTest4 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator errorTest4(nint value) => new errorTest4(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(errorTest4 value) => value.m_value;
            
            // Enable comparisons between nil and errorTest4 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(errorTest4 value, NilType nil) => value.Equals(default(errorTest4));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(errorTest4 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, errorTest4 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, errorTest4 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator errorTest4(NilType nil) => default(errorTest4);
        }
    }
}}}}
