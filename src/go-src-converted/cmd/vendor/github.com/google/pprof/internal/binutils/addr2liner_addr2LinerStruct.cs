//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:19 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
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
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct addr2Liner
        {
            // Constructors
            public addr2Liner(NilType _)
            {
                this.mu = default;
                this.rw = default;
                this.@base = default;
                this.nm = default;
            }

            public addr2Liner(sync.Mutex mu = default, lineReaderWriter rw = default, ulong @base = default, ref ptr<addr2LinerNM> nm = default)
            {
                this.mu = mu;
                this.rw = rw;
                this.@base = @base;
                this.nm = nm;
            }

            // Enable comparisons between nil and addr2Liner struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(addr2Liner value, NilType nil) => value.Equals(default(addr2Liner));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(addr2Liner value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, addr2Liner value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, addr2Liner value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator addr2Liner(NilType nil) => default(addr2Liner);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static addr2Liner addr2Liner_cast(dynamic value)
        {
            return new addr2Liner(value.mu, value.rw, value.@base, ref value.nm);
        }
    }
}}}}}}}