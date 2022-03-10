//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:23:18 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using elfexec = go.github.com.google.pprof.@internal.elfexec_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class binutils_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct elfMapping
        {
            // Constructors
            public elfMapping(NilType _)
            {
                this.start = default;
                this.limit = default;
                this.offset = default;
                this.stextOffset = default;
            }

            public elfMapping(ulong start = default, ulong limit = default, ulong offset = default, ref ptr<ulong> stextOffset = default)
            {
                this.start = start;
                this.limit = limit;
                this.offset = offset;
                this.stextOffset = stextOffset;
            }

            // Enable comparisons between nil and elfMapping struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(elfMapping value, NilType nil) => value.Equals(default(elfMapping));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(elfMapping value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, elfMapping value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, elfMapping value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator elfMapping(NilType nil) => default(elfMapping);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static elfMapping elfMapping_cast(dynamic value)
        {
            return new elfMapping(value.start, value.limit, value.offset, ref value.stextOffset);
        }
    }
}}}}}}}