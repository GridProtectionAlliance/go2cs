//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:35:37 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using bits = go.math.bits_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
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
        public partial struct XcoffLdHdr64
        {
            // Constructors
            public XcoffLdHdr64(NilType _)
            {
                this.Lversion = default;
                this.Lnsyms = default;
                this.Lnreloc = default;
                this.Listlen = default;
                this.Lnimpid = default;
                this.Lstlen = default;
                this.Limpoff = default;
                this.Lstoff = default;
                this.Lsymoff = default;
                this.Lrldoff = default;
            }

            public XcoffLdHdr64(int Lversion = default, int Lnsyms = default, int Lnreloc = default, uint Listlen = default, int Lnimpid = default, uint Lstlen = default, ulong Limpoff = default, ulong Lstoff = default, ulong Lsymoff = default, ulong Lrldoff = default)
            {
                this.Lversion = Lversion;
                this.Lnsyms = Lnsyms;
                this.Lnreloc = Lnreloc;
                this.Listlen = Listlen;
                this.Lnimpid = Lnimpid;
                this.Lstlen = Lstlen;
                this.Limpoff = Limpoff;
                this.Lstoff = Lstoff;
                this.Lsymoff = Lsymoff;
                this.Lrldoff = Lrldoff;
            }

            // Enable comparisons between nil and XcoffLdHdr64 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(XcoffLdHdr64 value, NilType nil) => value.Equals(default(XcoffLdHdr64));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(XcoffLdHdr64 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, XcoffLdHdr64 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, XcoffLdHdr64 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator XcoffLdHdr64(NilType nil) => default(XcoffLdHdr64);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static XcoffLdHdr64 XcoffLdHdr64_cast(dynamic value)
        {
            return new XcoffLdHdr64(value.Lversion, value.Lnsyms, value.Lnreloc, value.Listlen, value.Lnimpid, value.Lstlen, value.Limpoff, value.Lstoff, value.Lsymoff, value.Lrldoff);
        }
    }
}}}}