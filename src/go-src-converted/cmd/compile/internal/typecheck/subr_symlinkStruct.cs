//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:48:44 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
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
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct symlink
        {
            // Constructors
            public symlink(NilType _)
            {
                this.field = default;
            }

            public symlink(ref ptr<types.Field> field = default)
            {
                this.field = field;
            }

            // Enable comparisons between nil and symlink struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(symlink value, NilType nil) => value.Equals(default(symlink));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(symlink value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, symlink value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, symlink value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator symlink(NilType nil) => default(symlink);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static symlink symlink_cast(dynamic value)
        {
            return new symlink(ref value.field);
        }
    }
}}}}