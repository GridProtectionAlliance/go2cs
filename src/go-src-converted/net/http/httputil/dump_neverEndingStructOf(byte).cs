//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:38:26 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace net {
namespace http
{
    public static partial class httputil_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct neverEnding
        {
            // Value of the neverEnding struct
            private readonly byte m_value;
            
            public neverEnding(byte value) => m_value = value;

            // Enable implicit conversions between byte and neverEnding struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator neverEnding(byte value) => new neverEnding(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byte(neverEnding value) => value.m_value;
            
            // Enable comparisons between nil and neverEnding struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(neverEnding value, NilType nil) => value.Equals(default(neverEnding));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(neverEnding value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, neverEnding value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, neverEnding value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator neverEnding(NilType nil) => default(neverEnding);
        }
    }
}}}
