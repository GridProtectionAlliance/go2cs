//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:37:39 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using go;

#nullable enable

namespace go {
namespace net {
namespace http
{
    public static partial class @internal_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct chunkedWriter
        {
            // Constructors
            public chunkedWriter(NilType _)
            {
                this.Wire = default;
            }

            public chunkedWriter(io.Writer Wire = default)
            {
                this.Wire = Wire;
            }

            // Enable comparisons between nil and chunkedWriter struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(chunkedWriter value, NilType nil) => value.Equals(default(chunkedWriter));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(chunkedWriter value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, chunkedWriter value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, chunkedWriter value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator chunkedWriter(NilType nil) => default(chunkedWriter);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static chunkedWriter chunkedWriter_cast(dynamic value)
        {
            return new chunkedWriter(value.Wire);
        }
    }
}}}