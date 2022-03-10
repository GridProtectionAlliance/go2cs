//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:16:22 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using traceviewer = go.cmd.@internal.traceviewer_package;
using context = go.context_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using os = go.os_package;
using strings = go.strings_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct traceContext
        {
            // Constructors
            public traceContext(NilType _)
            {
                this.t = default;
                this.tid = default;
            }

            public traceContext(ref ptr<tracer> t = default, ulong tid = default)
            {
                this.t = t;
                this.tid = tid;
            }

            // Enable comparisons between nil and traceContext struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(traceContext value, NilType nil) => value.Equals(default(traceContext));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(traceContext value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, traceContext value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, traceContext value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator traceContext(NilType nil) => default(traceContext);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static traceContext traceContext_cast(dynamic value)
        {
            return new traceContext(ref value.t, value.tid);
        }
    }
}}}}