//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:01:33 UTC
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
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct LocPair : IArray
        {
            // Value of the LocPair struct
            private readonly array<Location> m_value;
            
            public nint Length => ((IArray)m_value).Length;

            object? IArray.this[nint index]
            {
                get => ((IArray)m_value)[index];
                set => ((IArray)m_value)[index] = value;
            }

            public ref Location this[nint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref m_value[index];
            }

            public IEnumerator GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();

            public object Clone() => ((ICloneable)m_value).Clone();

            public LocPair(array<Location> value) => m_value = value;

            // Enable implicit conversions between array<Location> and LocPair struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator LocPair(array<Location> value) => new LocPair(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator array<Location>(LocPair value) => value.m_value;
            
            // Enable comparisons between nil and LocPair struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(LocPair value, NilType nil) => value.Equals(default(LocPair));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(LocPair value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, LocPair value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, LocPair value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator LocPair(NilType nil) => default(LocPair);
        }
    }
}}}}
