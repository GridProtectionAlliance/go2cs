//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:56:10 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using mime = go.mime_package;
using quotedprintable = go.mime.quotedprintable_package;
using textproto = go.net.textproto_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace mime
{
    public static partial class multipart_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct stickyErrorReader
        {
            // Constructors
            public stickyErrorReader(NilType _)
            {
                this.r = default;
                this.err = default;
            }

            public stickyErrorReader(io.Reader r = default, error err = default)
            {
                this.r = r;
                this.err = err;
            }

            // Enable comparisons between nil and stickyErrorReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(stickyErrorReader value, NilType nil) => value.Equals(default(stickyErrorReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(stickyErrorReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, stickyErrorReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, stickyErrorReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator stickyErrorReader(NilType nil) => default(stickyErrorReader);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static stickyErrorReader stickyErrorReader_cast(dynamic value)
        {
            return new stickyErrorReader(value.r, value.err);
        }
    }
}}