//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:01:23 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class syscall_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct iflags
        {
            // Constructors
            public iflags(NilType _)
            {
                this.name = default;
                this.flags = default;
            }

            public iflags(array<byte> name = default, ushort flags = default)
            {
                this.name = name;
                this.flags = flags;
            }

            // Enable comparisons between nil and iflags struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(iflags value, NilType nil) => value.Equals(default(iflags));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(iflags value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, iflags value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, iflags value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator iflags(NilType nil) => default(iflags);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static iflags iflags_cast(dynamic value)
        {
            return new iflags(value.name, value.flags);
        }
    }
}