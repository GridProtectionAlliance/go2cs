//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:46:37 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text
{
    public static partial class transform_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct chain
        {
            // Constructors
            public chain(NilType _)
            {
                this.link = default;
                this.err = default;
                this.errStart = default;
            }

            public chain(slice<link> link = default, error err = default, nint errStart = default)
            {
                this.link = link;
                this.err = err;
                this.errStart = errStart;
            }

            // Enable comparisons between nil and chain struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(chain value, NilType nil) => value.Equals(default(chain));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(chain value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, chain value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, chain value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator chain(NilType nil) => default(chain);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static chain chain_cast(dynamic value)
        {
            return new chain(value.link, value.err, value.errStart);
        }
    }
}}}}}