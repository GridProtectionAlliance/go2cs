//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:27:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using sync = go.sync_package;

#nullable enable

namespace go
{
    public static partial class io_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct teeReader
        {
            // Constructors
            public teeReader(NilType _)
            {
                this.r = default;
                this.w = default;
            }

            public teeReader(Reader r = default, Writer w = default)
            {
                this.r = r;
                this.w = w;
            }

            // Enable comparisons between nil and teeReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(teeReader value, NilType nil) => value.Equals(default(teeReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(teeReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, teeReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, teeReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator teeReader(NilType nil) => default(teeReader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static teeReader teeReader_cast(dynamic value)
        {
            return new teeReader(value.r, value.w);
        }
    }
}