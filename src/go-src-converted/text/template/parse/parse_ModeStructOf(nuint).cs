//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:39:01 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace text {
namespace template
{
    public static partial class parse_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Mode
        {
            // Value of the Mode struct
            private readonly nuint m_value;
            
            public Mode(nuint value) => m_value = value;

            // Enable implicit conversions between nuint and Mode struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Mode(nuint value) => new Mode(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nuint(Mode value) => value.m_value;
            
            // Enable comparisons between nil and Mode struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Mode value, NilType nil) => value.Equals(default(Mode));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Mode value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Mode value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Mode value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Mode(NilType nil) => default(Mode);
        }
    }
}}}
