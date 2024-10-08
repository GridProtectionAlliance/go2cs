//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:28:09 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using context = go.context_package;
using errors = go.errors_package;
using execenv = go.@internal.syscall.execenv_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using go;

#nullable enable

namespace go {
namespace os
{
    public static partial class exec_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct prefixSuffixSaver
        {
            // Constructors
            public prefixSuffixSaver(NilType _)
            {
                this.N = default;
                this.prefix = default;
                this.suffix = default;
                this.suffixOff = default;
                this.skipped = default;
            }

            public prefixSuffixSaver(nint N = default, slice<byte> prefix = default, slice<byte> suffix = default, nint suffixOff = default, long skipped = default)
            {
                this.N = N;
                this.prefix = prefix;
                this.suffix = suffix;
                this.suffixOff = suffixOff;
                this.skipped = skipped;
            }

            // Enable comparisons between nil and prefixSuffixSaver struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(prefixSuffixSaver value, NilType nil) => value.Equals(default(prefixSuffixSaver));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(prefixSuffixSaver value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, prefixSuffixSaver value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, prefixSuffixSaver value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator prefixSuffixSaver(NilType nil) => default(prefixSuffixSaver);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static prefixSuffixSaver prefixSuffixSaver_cast(dynamic value)
        {
            return new prefixSuffixSaver(value.N, value.prefix, value.suffix, value.suffixOff, value.skipped);
        }
    }
}}