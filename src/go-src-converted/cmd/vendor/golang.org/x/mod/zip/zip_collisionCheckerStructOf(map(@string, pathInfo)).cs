//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:41:15 UTC
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
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class zip_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct collisionChecker : IMap
        {
            // Value of the collisionChecker struct
            private readonly map<@string, pathInfo> m_value;
            
            public nint Length => ((IMap)m_value).Length;

            object? IMap.this[object key]
            {
                get => ((IMap)m_value)[key];
                set => ((IMap)m_value)[key] = value;
            }

            public pathInfo this[@string key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value[key];
            
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => m_value[key] = value;
            }

            public (pathInfo, bool) this[@string key, bool _]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_value.TryGetValue(key, out pathInfo value) ? (value!, true) : (default!, false);
            }

            public collisionChecker(map<@string, pathInfo> value) => m_value = value;

            // Enable implicit conversions between map<@string, pathInfo> and collisionChecker struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator collisionChecker(map<@string, pathInfo> value) => new collisionChecker(value);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator map<@string, pathInfo>(collisionChecker value) => value.m_value;
            
            // Enable comparisons between nil and collisionChecker struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(collisionChecker value, NilType nil) => value.Equals(default(collisionChecker));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(collisionChecker value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, collisionChecker value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, collisionChecker value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator collisionChecker(NilType nil) => default(collisionChecker);
        }
    }
}}}}}}
