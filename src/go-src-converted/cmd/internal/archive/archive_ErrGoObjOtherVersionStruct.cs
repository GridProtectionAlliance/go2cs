//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:43:18 UTC
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
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ErrGoObjOtherVersion
        {
            // Constructors
            public ErrGoObjOtherVersion(NilType _)
            {
                this.magic = default;
            }

            public ErrGoObjOtherVersion(slice<byte> magic = default)
            {
                this.magic = magic;
            }

            // Enable comparisons between nil and ErrGoObjOtherVersion struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ErrGoObjOtherVersion value, NilType nil) => value.Equals(default(ErrGoObjOtherVersion));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ErrGoObjOtherVersion value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ErrGoObjOtherVersion value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ErrGoObjOtherVersion value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ErrGoObjOtherVersion(NilType nil) => default(ErrGoObjOtherVersion);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static ErrGoObjOtherVersion ErrGoObjOtherVersion_cast(dynamic value)
        {
            return new ErrGoObjOtherVersion(value.magic);
        }
    }
}}}