//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:36:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using io = go.io_package;
using http = go.net.http_package;
using regexp = go.regexp_package;
using time = go.time_package;
using profile = go.github.com.google.pprof.profile_package;
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
    public static partial class plugin_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Frame
        {
            // Constructors
            public Frame(NilType _)
            {
                this.Func = default;
                this.File = default;
                this.Line = default;
            }

            public Frame(@string Func = default, @string File = default, nint Line = default)
            {
                this.Func = Func;
                this.File = File;
                this.Line = Line;
            }

            // Enable comparisons between nil and Frame struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Frame value, NilType nil) => value.Equals(default(Frame));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Frame value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Frame value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Frame value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Frame(NilType nil) => default(Frame);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Frame Frame_cast(dynamic value)
        {
            return new Frame(value.Func, value.File, value.Line);
        }
    }
}}}}}}}