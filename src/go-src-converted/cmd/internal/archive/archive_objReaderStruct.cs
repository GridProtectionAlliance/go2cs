//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:32:20 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using goobj = go.cmd.@internal.goobj_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class archive_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct objReader
        {
            // Constructors
            public objReader(NilType _)
            {
                this.a = default;
                this.b = default;
                this.err = default;
                this.offset = default;
                this.limit = default;
                this.tmp = default;
            }

            public objReader(ref ptr<Archive> a = default, ref ptr<bio.Reader> b = default, error err = default, long offset = default, long limit = default, array<byte> tmp = default)
            {
                this.a = a;
                this.b = b;
                this.err = err;
                this.offset = offset;
                this.limit = limit;
                this.tmp = tmp;
            }

            // Enable comparisons between nil and objReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(objReader value, NilType nil) => value.Equals(default(objReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(objReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, objReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, objReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator objReader(NilType nil) => default(objReader);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static objReader objReader_cast(dynamic value)
        {
            return new objReader(ref value.a, ref value.b, value.err, value.offset, value.limit, value.tmp);
        }
    }
}}}