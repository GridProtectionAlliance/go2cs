//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:28:51 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct importReader
        {
            // Constructors
            public importReader(NilType _)
            {
                this.b = default;
                this.buf = default;
                this.peek = default;
                this.err = default;
                this.eof = default;
                this.nerr = default;
            }

            public importReader(ref ptr<bufio.Reader> b = default, slice<byte> buf = default, byte peek = default, error err = default, bool eof = default, nint nerr = default)
            {
                this.b = b;
                this.buf = buf;
                this.peek = peek;
                this.err = err;
                this.eof = eof;
                this.nerr = nerr;
            }

            // Enable comparisons between nil and importReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(importReader value, NilType nil) => value.Equals(default(importReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(importReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, importReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, importReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator importReader(NilType nil) => default(importReader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static importReader importReader_cast(dynamic value)
        {
            return new importReader(ref value.b, value.buf, value.peek, value.err, value.eof, value.nerr);
        }
    }
}