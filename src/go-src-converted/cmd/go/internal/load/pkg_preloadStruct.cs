//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:30:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using context = go.context_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using goroot = go.@internal.goroot_package;
using fs = go.io.fs_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using imports = go.cmd.go.@internal.imports_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using trace = go.cmd.go.@internal.trace_package;
using sys = go.cmd.@internal.sys_package;
using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct preload
        {
            // Constructors
            public preload(NilType _)
            {
                this.cancel = default;
                this.sema = default;
            }

            public preload(channel<object> cancel = default, channel<object> sema = default)
            {
                this.cancel = cancel;
                this.sema = sema;
            }

            // Enable comparisons between nil and preload struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(preload value, NilType nil) => value.Equals(default(preload));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(preload value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, preload value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, preload value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator preload(NilType nil) => default(preload);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static preload preload_cast(dynamic value)
        {
            return new preload(value.cancel, value.sema);
        }
    }
}}}}