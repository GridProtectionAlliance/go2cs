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
        public partial struct MyFloat64
        {
            // Value of the MyFloat64 struct
            private readonly double m_value;
            
            public MyFloat64(double value) => m_value = value;

            // Enable implicit conversions between double and MyFloat64 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MyFloat64(double value) => new MyFloat64(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator double(MyFloat64 value) => value.m_value;
            
            // Enable comparisons between nil and MyFloat64 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MyFloat64 value, NilType nil) => value.Equals(default(MyFloat64));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MyFloat64 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, MyFloat64 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, MyFloat64 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MyFloat64(NilType nil) => default(MyFloat64);
        }
    }
}
