//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:27:14 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct modulehash
        {
            // Constructors
            public modulehash(NilType _)
            {
                this.modulename = default;
                this.linktimehash = default;
                this.runtimehash = default;
            }

            public modulehash(@string modulename = default, @string linktimehash = default, ref ptr<@string> runtimehash = default)
            {
                this.modulename = modulename;
                this.linktimehash = linktimehash;
                this.runtimehash = runtimehash;
            }

            // Enable comparisons between nil and modulehash struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(modulehash value, NilType nil) => value.Equals(default(modulehash));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(modulehash value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, modulehash value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, modulehash value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator modulehash(NilType nil) => default(modulehash);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static modulehash modulehash_cast(dynamic value)
        {
            return new modulehash(value.modulename, value.linktimehash, ref value.runtimehash);
        }
    }
}