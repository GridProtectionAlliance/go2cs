//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace go {
namespace @internal
{
    public static partial class aliases_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct A10 : IArray
        {
            // Value of the A10 struct
            private readonly array<nint> m_value;
            
            public nint Length => ((IArray)m_value).Length;

            object? IArray.this[nint index]
            {
                get => ((IArray)m_value)[index];
                set => ((IArray)m_value)[index] = value;
            }

            public ref nint this[nint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref m_value[index];
            }

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();

            public object Clone() => ((ICloneable)m_value).Clone();

            public A10(array<nint> value) => m_value = value;

            // Enable implicit conversions between array<nint> and A10 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A10(array<nint> value) => new A10(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator array<nint>(A10 value) => value.m_value;
            
            // Enable comparisons between nil and A10 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(A10 value, NilType nil) => value.Equals(default(A10));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(A10 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, A10 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, A10 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator A10(NilType nil) => default(A10);
        }
    }
}}}
