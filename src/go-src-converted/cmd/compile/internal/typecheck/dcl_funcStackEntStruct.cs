//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:59:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class typecheck_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct funcStackEnt
        {
            // Constructors
            public funcStackEnt(NilType _)
            {
                this.curfn = default;
                this.dclcontext = default;
            }

            public funcStackEnt(ref ptr<ir.Func> curfn = default, ir.Class dclcontext = default)
            {
                this.curfn = curfn;
                this.dclcontext = dclcontext;
            }

            // Enable comparisons between nil and funcStackEnt struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(funcStackEnt value, NilType nil) => value.Equals(default(funcStackEnt));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(funcStackEnt value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, funcStackEnt value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, funcStackEnt value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator funcStackEnt(NilType nil) => default(funcStackEnt);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static funcStackEnt funcStackEnt_cast(dynamic value)
        {
            return new funcStackEnt(ref value.curfn, value.dclcontext);
        }
    }
}}}}