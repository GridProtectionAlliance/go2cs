//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:32:00 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using rand = go.math.rand_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using codehost = go.cmd.go.@internal.modfetch.codehost_package;
using par = go.cmd.go.@internal.par_package;
using robustio = go.cmd.go.@internal.robustio_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct cachedInfo
        {
            // Constructors
            public cachedInfo(NilType _)
            {
                this.info = default;
                this.err = default;
            }

            public cachedInfo(ref ptr<RevInfo> info = default, error err = default)
            {
                this.info = info;
                this.err = err;
            }

            // Enable comparisons between nil and cachedInfo struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(cachedInfo value, NilType nil) => value.Equals(default(cachedInfo));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(cachedInfo value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, cachedInfo value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, cachedInfo value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator cachedInfo(NilType nil) => default(cachedInfo);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static cachedInfo cachedInfo_cast(dynamic value)
        {
            return new cachedInfo(ref value.info, value.err);
        }
    }
}}}}