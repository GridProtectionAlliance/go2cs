//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:35:06 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using log = go.log_package;
using os = go.os_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct OutBuf
        {
            // Constructors
            public OutBuf(NilType _)
            {
                this.arch = default;
                this.off = default;
                this.buf = default;
                this.heap = default;
                this.name = default;
                this.f = default;
                this.encbuf = default;
                this.isView = default;
            }

            public OutBuf(ref ptr<sys.Arch> arch = default, long off = default, slice<byte> buf = default, slice<byte> heap = default, @string name = default, ref ptr<os.File> f = default, array<byte> encbuf = default, bool isView = default)
            {
                this.arch = arch;
                this.off = off;
                this.buf = buf;
                this.heap = heap;
                this.name = name;
                this.f = f;
                this.encbuf = encbuf;
                this.isView = isView;
            }

            // Enable comparisons between nil and OutBuf struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(OutBuf value, NilType nil) => value.Equals(default(OutBuf));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(OutBuf value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, OutBuf value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, OutBuf value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator OutBuf(NilType nil) => default(OutBuf);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static OutBuf OutBuf_cast(dynamic value)
        {
            return new OutBuf(ref value.arch, value.off, value.buf, value.heap, value.name, ref value.f, value.encbuf, value.isView);
        }
    }
}}}}