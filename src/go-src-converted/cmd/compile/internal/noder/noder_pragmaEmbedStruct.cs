//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:27:33 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @base = go.cmd.compile.@internal.@base_package;
using dwarfgen = go.cmd.compile.@internal.dwarfgen_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class noder_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct pragmaEmbed
        {
            // Constructors
            public pragmaEmbed(NilType _)
            {
                this.Pos = default;
                this.Patterns = default;
            }

            public pragmaEmbed(syntax.Pos Pos = default, slice<@string> Patterns = default)
            {
                this.Pos = Pos;
                this.Patterns = Patterns;
            }

            // Enable comparisons between nil and pragmaEmbed struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(pragmaEmbed value, NilType nil) => value.Equals(default(pragmaEmbed));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(pragmaEmbed value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, pragmaEmbed value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, pragmaEmbed value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator pragmaEmbed(NilType nil) => default(pragmaEmbed);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static pragmaEmbed pragmaEmbed_cast(dynamic value)
        {
            return new pragmaEmbed(value.Pos, value.Patterns);
        }
    }
}}}}