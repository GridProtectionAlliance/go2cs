//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:47:12 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

namespace go {
namespace go
{
    public static partial class blank_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct _
        {
            // Value of the _ struct
            private readonly T m_value;

            public _(T value) => m_value = value;

            // Enable implicit conversions between T and _ struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator _(T value) => new _(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator T(_ value) => value.m_value;
            
            // Enable comparisons between nil and _ struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(_ value, NilType nil) => value.Equals(default(_));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(_ value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, _ value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, _ value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator _(NilType nil) => default(_);
        }
    }
}}