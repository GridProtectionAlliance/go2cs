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
        private partial struct gdesc
        {
            // Constructors
            public gdesc(NilType _)
            {
                this.state = default;
                this.ev = default;
                this.evStart = default;
                this.evCreate = default;
                this.evMarkAssist = default;
            }

            public gdesc(nint state = default, ref ptr<Event> ev = default, ref ptr<Event> evStart = default, ref ptr<Event> evCreate = default, ref ptr<Event> evMarkAssist = default)
            {
                this.state = state;
                this.ev = ev;
                this.evStart = evStart;
                this.evCreate = evCreate;
                this.evMarkAssist = evMarkAssist;
            }

            // Enable comparisons between nil and gdesc struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(gdesc value, NilType nil) => value.Equals(default(gdesc));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(gdesc value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, gdesc value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, gdesc value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator gdesc(NilType nil) => default(gdesc);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static gdesc gdesc_cast(dynamic value)
        {
            return new gdesc(value.state, ref value.ev, ref value.evStart, ref value.evCreate, ref value.evMarkAssist);
        }
    }
}}