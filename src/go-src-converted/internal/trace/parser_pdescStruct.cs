//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:03 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using _@unsafe_ = go.@unsafe_package;
using go;

#nullable enable

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct pdesc
        {
            // Constructors
            public pdesc(NilType _)
            {
                this.running = default;
                this.g = default;
                this.evSTW = default;
                this.evSweep = default;
            }

            public pdesc(bool running = default, ulong g = default, ref ptr<Event> evSTW = default, ref ptr<Event> evSweep = default)
            {
                this.running = running;
                this.g = g;
                this.evSTW = evSTW;
                this.evSweep = evSweep;
            }

            // Enable comparisons between nil and pdesc struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(pdesc value, NilType nil) => value.Equals(default(pdesc));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(pdesc value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, pdesc value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, pdesc value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pdesc(NilType nil) => default(pdesc);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static pdesc pdesc_cast(dynamic value)
        {
            return new pdesc(value.running, value.g, ref value.evSTW, ref value.evSweep);
        }
    }
}}