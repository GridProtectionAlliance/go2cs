﻿//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System;

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    public partial struct PeopleByShoeSize : ISlice<Person>
    {
        // Value of the PeopleByShoeSize struct
        private readonly slice<Person> m_value;
        
        public Person[] Source => m_value;
            
        public ISlice<Person> Append(Person[] elems) => m_value.Append(elems);
            
        public nint Low => ((ISlice)m_value).Low;
        
        public nint High => ((ISlice)m_value).High;
        
        public nint Capacity => ((ISlice)m_value).Capacity;
        
        public nint Available => ((ISlice)m_value).Available;
        
        public nint Length => ((IArray)m_value).Length;
        
        Array IArray.Source => ((IArray)m_value).Source!;
        
        object? IArray.this[nint index]
        {
            get => ((IArray)m_value)[index];
            set => ((IArray)m_value)[index] = value;
        }
            
        public ref Person this[nint index]
        {
            get => ref m_value[index];
        }
        
        public Span<Person> ꓸꓸꓸ => ToSpan();
        
        public Span<Person> ToSpan()
        {
            return m_value.ToSpan();
        }
        
        public ISlice? Append(object[] elems)
        {
            return ((ISlice)m_value).Append(elems);
        }
        
        public IEnumerator<(nint, Person)> GetEnumerator()
        {
            return m_value.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_value).GetEnumerator();
        }
        
        public bool Equals(IArray<Person>? other)
        {
            return m_value.Equals(other);
        }
        
        public bool Equals(ISlice<Person>? other)
        {
           return m_value.Equals(other);
        }
        
        public object Clone() => ((ICloneable)m_value).Clone();
        
        public PeopleByShoeSize(slice<Person> value) => m_value = value;

        // Enable implicit conversions between slice<Person> and PeopleByShoeSize struct
        public static implicit operator PeopleByShoeSize(slice<Person> value) => new PeopleByShoeSize(value);
            
        public static implicit operator slice<Person>(PeopleByShoeSize value) => value.m_value;
            
        // Enable comparisons between nil and PeopleByShoeSize struct
        public static bool operator ==(PeopleByShoeSize value, NilType nil) => value.Equals(default(PeopleByShoeSize));

        public static bool operator !=(PeopleByShoeSize value, NilType nil) => !(value == nil);

        public static bool operator ==(NilType nil, PeopleByShoeSize value) => value == nil;

        public static bool operator !=(NilType nil, PeopleByShoeSize value) => value != nil;

        public static implicit operator PeopleByShoeSize(NilType nil) => default(PeopleByShoeSize);
    }
}