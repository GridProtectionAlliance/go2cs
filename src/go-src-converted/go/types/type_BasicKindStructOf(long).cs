//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:19:42 UTC
// </auto-generated>
//---------------------------------------------------------
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct BasicKind
        {
            // Value of the BasicKind struct
            private readonly long m_value;

            public BasicKind(long value) => m_value = value;

            // Enable implicit conversions between long and BasicKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator BasicKind(long value) => new BasicKind(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator long(BasicKind value) => value.m_value;
            
            // Enable comparisons between nil and BasicKind struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(BasicKind value, NilType nil) => value.Equals(default(BasicKind));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(BasicKind value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, BasicKind value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, BasicKind value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator BasicKind(NilType nil) => default(BasicKind);
        }
    }
}}
