//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:29:47 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using @base = go.cmd.go.@internal.@base_package;
using imports = go.cmd.go.@internal.imports_package;
using load = go.cmd.go.@internal.load_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using work = go.cmd.go.@internal.work_package;
using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modget_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct key
        {
            // Constructors
            public key(NilType _)
            {
                this.pattern = default;
                this.m = default;
            }

            public key(@string pattern = default, module.Version m = default)
            {
                this.pattern = pattern;
                this.m = m;
            }

            // Enable comparisons between nil and key struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(key value, NilType nil) => value.Equals(default(key));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(key value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, key value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, key value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator key(NilType nil) => default(key);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static key key_cast(dynamic value)
        {
            return new key(value.pattern, value.m);
        }
    }
}}}}