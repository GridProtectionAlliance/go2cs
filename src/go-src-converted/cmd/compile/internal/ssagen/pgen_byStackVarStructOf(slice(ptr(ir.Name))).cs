//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:09:47 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssagen_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct byStackVar : ISlice
        {
            // Value of the byStackVar struct
            private readonly slice<ptr<ir.Name>> m_value;
            
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
            
            public ref ptr<ir.Name> this[nint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref m_value[index];
            }
            
            public ISlice? Append(object[] elems) => ((ISlice)m_value).Append(elems);

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();

            public object Clone() => ((ICloneable)m_value).Clone();

            public byStackVar(slice<ptr<ir.Name>> value) => m_value = value;

            // Enable implicit conversions between slice<ptr<ir.Name>> and byStackVar struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byStackVar(slice<ptr<ir.Name>> value) => new byStackVar(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator slice<ptr<ir.Name>>(byStackVar value) => value.m_value;
            
            // Enable comparisons between nil and byStackVar struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(byStackVar value, NilType nil) => value.Equals(default(byStackVar));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(byStackVar value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, byStackVar value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, byStackVar value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator byStackVar(NilType nil) => default(byStackVar);
        }
    }
}}}}