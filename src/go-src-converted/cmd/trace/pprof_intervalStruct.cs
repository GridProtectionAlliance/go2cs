//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:12 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using exec = go.@internal.execabs_package;
using trace = go.@internal.trace_package;
using io = go.io_package;
using http = go.net.http_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using time = go.time_package;
using profile = go.github.com.google.pprof.profile_package;

#nullable enable

namespace go
{
    public static partial class main_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct interval
        {
            // Constructors
            public interval(NilType _)
            {
                this.begin = default;
                this.end = default;
            }

            public interval(long begin = default, long end = default)
            {
                this.begin = begin;
                this.end = end;
            }

            // Enable comparisons between nil and interval struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(interval value, NilType nil) => value.Equals(default(interval));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(interval value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, interval value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, interval value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator interval(NilType nil) => default(interval);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static interval interval_cast(dynamic value)
        {
            return new interval(value.begin, value.end);
        }
    }
}