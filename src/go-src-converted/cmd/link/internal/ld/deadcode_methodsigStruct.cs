//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:20:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using unicode = go.unicode_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct methodsig
        {
            // Constructors
            public methodsig(NilType _)
            {
                this.name = default;
                this.typ = default;
            }

            public methodsig(@string name = default, loader.Sym typ = default)
            {
                this.name = name;
                this.typ = typ;
            }

            // Enable comparisons between nil and methodsig struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(methodsig value, NilType nil) => value.Equals(default(methodsig));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(methodsig value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, methodsig value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, methodsig value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator methodsig(NilType nil) => default(methodsig);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static methodsig methodsig_cast(dynamic value)
        {
            return new methodsig(value.name, value.typ);
        }
    }
}}}}