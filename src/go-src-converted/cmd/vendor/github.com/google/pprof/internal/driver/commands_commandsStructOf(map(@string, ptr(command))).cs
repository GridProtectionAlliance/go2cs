//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:26 UTC
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
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct commands : IMap
        {
            // Value of the commands struct
            private readonly map<@string, ptr<command>> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public ptr<command> this[@string key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (ptr<command>, bool) this[@string key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out ptr<command> value) ? (value!, true) : (default!, false);
            }

            public commands(map<@string, ptr<command>> value) => m_value = value;

            // Enable implicit conversions between map<@string, ptr<command>> and commands struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator commands(map<@string, ptr<command>> value) => new commands(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, ptr<command>>(commands value) => value.m_value;
            
            // Enable comparisons between nil and commands struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(commands value, NilType nil) => value.Equals(default(commands));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(commands value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, commands value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, commands value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator commands(NilType nil) => default(commands);
        }
    }
}}}}}}}
