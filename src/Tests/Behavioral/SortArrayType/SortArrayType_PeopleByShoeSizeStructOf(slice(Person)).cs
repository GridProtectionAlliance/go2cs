//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2021 January 09 07:27:34 UTC
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
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct PeopleByShoeSize : ISlice
        {
            // Value of the PeopleByShoeSize struct
            private readonly slice<Person> m_value;
            
            public Array Array => ((ISlice)m_value).Array;

            public nint Low => ((ISlice)m_value).Low;

            public nint High => ((ISlice)m_value).High;

            public nint Capacity => ((ISlice)m_value).Capacity;

            public nint Available => ((ISlice)m_value).Available;

            public nint Length => ((IArray)m_value).Length;

            object? IArray.this[nint index]
            {
                get => ((IArray)m_value)[index];
                set => ((IArray)m_value)[index] = value;
            }
            
            public ref Person this[nint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref m_value[index];
            }
            
            public ISlice? Append(object[] elems) => ((ISlice)m_value).Append(elems);

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();

            public object Clone() => ((ICloneable)m_value).Clone();

            public PeopleByShoeSize(slice<Person> value) => m_value = value;

            // Enable implicit conversions between slice<Person> and PeopleByShoeSize struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PeopleByShoeSize(slice<Person> value) => new PeopleByShoeSize(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<Person>(PeopleByShoeSize value) => value.m_value;
            
            // Enable comparisons between nil and PeopleByShoeSize struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(PeopleByShoeSize value, NilType nil) => value.Equals(default(PeopleByShoeSize));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(PeopleByShoeSize value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, PeopleByShoeSize value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, PeopleByShoeSize value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator PeopleByShoeSize(NilType nil) => default(PeopleByShoeSize);
        }
    }
}