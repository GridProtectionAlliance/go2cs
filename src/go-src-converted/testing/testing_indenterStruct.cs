//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:47:47 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using bytes = go.bytes_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using race = go.@internal.race_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using trace = go.runtime.trace_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;

#nullable enable

namespace go
{
    public static partial class testing_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct indenter
        {
            // Constructors
            public indenter(NilType _)
            {
                this.c = default;
            }

            public indenter(ref ptr<common> c = default)
            {
                this.c = c;
            }

            // Enable comparisons between nil and indenter struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(indenter value, NilType nil) => value.Equals(default(indenter));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(indenter value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, indenter value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, indenter value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator indenter(NilType nil) => default(indenter);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static indenter indenter_cast(dynamic value)
        {
            return new indenter(ref value.c);
        }
    }
}