//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:01:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using html = go.html_package;
using exec = go.@internal.execabs_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct dotWriter
        {
            // Constructors
            public dotWriter(NilType _)
            {
                this.path = default;
                this.broken = default;
                this.phases = default;
            }

            public dotWriter(@string path = default, bool broken = default, map<@string, bool> phases = default)
            {
                this.path = path;
                this.broken = broken;
                this.phases = phases;
            }

            // Enable comparisons between nil and dotWriter struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(dotWriter value, NilType nil) => value.Equals(default(dotWriter));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(dotWriter value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, dotWriter value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, dotWriter value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator dotWriter(NilType nil) => default(dotWriter);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static dotWriter dotWriter_cast(dynamic value)
        {
            return new dotWriter(value.path, value.broken, value.phases);
        }
    }
}}}}