//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:53:31 UTC
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
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ChanDir
        {
            // Value of the ChanDir struct
            private readonly nint m_value;
            
            public ChanDir(nint value) => m_value = value;

            // Enable implicit conversions between nint and ChanDir struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ChanDir(nint value) => new ChanDir(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator nint(ChanDir value) => value.m_value;
            
            // Enable comparisons between nil and ChanDir struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ChanDir value, NilType nil) => value.Equals(default(ChanDir));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ChanDir value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ChanDir value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ChanDir value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ChanDir(NilType nil) => default(ChanDir);
        }
    }
}}
