//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:32:01 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using fmt = go.fmt_package;
using exec = go.@internal.execabs_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using str = go.cmd.go.@internal.str_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal {
namespace modfetch
{
    public static partial class codehost_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct FileRev
        {
            // Constructors
            public FileRev(NilType _)
            {
                this.Rev = default;
                this.Data = default;
                this.Err = default;
            }

            public FileRev(@string Rev = default, slice<byte> Data = default, error Err = default)
            {
                this.Rev = Rev;
                this.Data = Data;
                this.Err = Err;
            }

            // Enable comparisons between nil and FileRev struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(FileRev value, NilType nil) => value.Equals(default(FileRev));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(FileRev value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, FileRev value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, FileRev value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator FileRev(NilType nil) => default(FileRev);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static FileRev FileRev_cast(dynamic value)
        {
            return new FileRev(value.Rev, value.Data, value.Err);
        }
    }
}}}}}