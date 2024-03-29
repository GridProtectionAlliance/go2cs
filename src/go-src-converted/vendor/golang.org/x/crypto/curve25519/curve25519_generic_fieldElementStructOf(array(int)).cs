//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:44:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class curve25519_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct fieldElement : IArray
        {
            // Value of the fieldElement struct
            private readonly array<int> m_value;
            
            public nint Length => ((IArray)m_value).Length;

            object? IArray.this[nint index]
            {
                get => ((IArray)m_value)[index];
                set => ((IArray)m_value)[index] = value;
            }

            public ref int this[nint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref m_value[index];
            }

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();

            public object Clone() => ((ICloneable)m_value).Clone();

            public fieldElement(array<int> value) => m_value = value;

            // Enable implicit conversions between array<int> and fieldElement struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator fieldElement(array<int> value) => new fieldElement(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator array<int>(fieldElement value) => value.m_value;
            
            // Enable comparisons between nil and fieldElement struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(fieldElement value, NilType nil) => value.Equals(default(fieldElement));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(fieldElement value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, fieldElement value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, fieldElement value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator fieldElement(NilType nil) => default(fieldElement);
        }
    }
}}}}}
