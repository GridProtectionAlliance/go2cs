//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:30:00 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using url = go.net.url_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct errorDetailBuffer
        {
            // Constructors
            public errorDetailBuffer(NilType _)
            {
                this.r = default;
                this.buf = default;
                this.bufLines = default;
            }

            public errorDetailBuffer(io.ReadCloser r = default, strings.Builder buf = default, nint bufLines = default)
            {
                this.r = r;
                this.buf = buf;
                this.bufLines = bufLines;
            }

            // Enable comparisons between nil and errorDetailBuffer struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(errorDetailBuffer value, NilType nil) => value.Equals(default(errorDetailBuffer));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(errorDetailBuffer value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, errorDetailBuffer value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, errorDetailBuffer value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator errorDetailBuffer(NilType nil) => default(errorDetailBuffer);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static errorDetailBuffer errorDetailBuffer_cast(dynamic value)
        {
            return new errorDetailBuffer(value.r, value.buf, value.bufLines);
        }
    }
}}}}