//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:43:11 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;
using go;

#nullable enable

namespace go {
namespace testing
{
    public static partial class fstest_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct MapFS : IMap
        {
            // Value of the MapFS struct
            private readonly map<@string, ptr<MapFile>> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public ptr<MapFile> this[@string key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (ptr<MapFile>, bool) this[@string key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out ptr<MapFile> value) ? (value!, true) : (default!, false);
            }

            public MapFS(map<@string, ptr<MapFile>> value) => m_value = value;

            // Enable implicit conversions between map<@string, ptr<MapFile>> and MapFS struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MapFS(map<@string, ptr<MapFile>> value) => new MapFS(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, ptr<MapFile>>(MapFS value) => value.m_value;
            
            // Enable comparisons between nil and MapFS struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MapFS value, NilType nil) => value.Equals(default(MapFS));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MapFS value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, MapFS value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, MapFS value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MapFS(NilType nil) => default(MapFS);
        }
    }
}}
