//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:33:41 UTC
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
        public partial struct M : IMap
        {
            // Value of the M struct
            private readonly map<nint, nint> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public nint this[nint key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (nint, bool) this[nint key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out nint value) ? (value!, true) : (default!, false);
            }

            public M(map<nint, nint> value) => m_value = value;

            // Enable implicit conversions between map<nint, nint> and M struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator M(map<nint, nint> value) => new M(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<nint, nint>(M value) => value.m_value;
            
            // Enable comparisons between nil and M struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(M value, NilType nil) => value.Equals(default(M));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(M value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, M value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, M value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator M(NilType nil) => default(M);
        }
    }
}