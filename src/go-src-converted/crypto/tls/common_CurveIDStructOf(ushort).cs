//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct CurveID
        {
            // Value of the CurveID struct
            private readonly ushort m_value;
            
            public CurveID(ushort value) => m_value = value;

            // Enable implicit conversions between ushort and CurveID struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CurveID(ushort value) => new CurveID(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ushort(CurveID value) => value.m_value;
            
            // Enable comparisons between nil and CurveID struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CurveID value, NilType nil) => value.Equals(default(CurveID));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CurveID value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CurveID value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CurveID value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CurveID(NilType nil) => default(CurveID);
        }
    }
}}
