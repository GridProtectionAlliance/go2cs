//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:29:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct MyUint16
        {
            // Value of the MyUint16 struct
            private readonly ushort m_value;
            
            public MyUint16(ushort value) => m_value = value;

            // Enable implicit conversions between ushort and MyUint16 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MyUint16(ushort value) => new MyUint16(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ushort(MyUint16 value) => value.m_value;
            
            // Enable comparisons between nil and MyUint16 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MyUint16 value, NilType nil) => value.Equals(default(MyUint16));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MyUint16 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, MyUint16 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, MyUint16 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MyUint16(NilType nil) => default(MyUint16);
        }
    }
}
