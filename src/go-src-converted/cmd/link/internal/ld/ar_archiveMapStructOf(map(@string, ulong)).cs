//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:32:56 UTC
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
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct archiveMap : IMap
        {
            // Value of the archiveMap struct
            private readonly map<@string, ulong> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public ulong this[@string key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (ulong, bool) this[@string key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out ulong value) ? (value!, true) : (default!, false);
            }

            public archiveMap(map<@string, ulong> value) => m_value = value;

            // Enable implicit conversions between map<@string, ulong> and archiveMap struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator archiveMap(map<@string, ulong> value) => new archiveMap(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, ulong>(archiveMap value) => value.m_value;
            
            // Enable comparisons between nil and archiveMap struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(archiveMap value, NilType nil) => value.Equals(default(archiveMap));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(archiveMap value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, archiveMap value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, archiveMap value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator archiveMap(NilType nil) => default(archiveMap);
        }
    }
}}}}
