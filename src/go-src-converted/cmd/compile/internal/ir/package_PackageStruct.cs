//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:00:37 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using types = go.cmd.compile.@internal.types_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ir_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Package
        {
            // Constructors
            public Package(NilType _)
            {
                this.Imports = default;
                this.Inits = default;
                this.Decls = default;
                this.Externs = default;
                this.Asms = default;
                this.CgoPragmas = default;
                this.Embeds = default;
                this.Exports = default;
                this.Stencils = default;
            }

            public Package(slice<ptr<types.Pkg>> Imports = default, slice<ptr<Func>> Inits = default, slice<Node> Decls = default, slice<Node> Externs = default, slice<ptr<Name>> Asms = default, slice<slice<@string>> CgoPragmas = default, slice<ptr<Name>> Embeds = default, slice<ptr<Name>> Exports = default, map<ptr<types.Sym>, ptr<Func>> Stencils = default)
            {
                this.Imports = Imports;
                this.Inits = Inits;
                this.Decls = Decls;
                this.Externs = Externs;
                this.Asms = Asms;
                this.CgoPragmas = CgoPragmas;
                this.Embeds = Embeds;
                this.Exports = Exports;
                this.Stencils = Stencils;
            }

            // Enable comparisons between nil and Package struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Package value, NilType nil) => value.Equals(default(Package));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Package value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Package value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Package value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Package(NilType nil) => default(Package);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Package Package_cast(dynamic value)
        {
            return new Package(value.Imports, value.Inits, value.Decls, value.Externs, value.Asms, value.CgoPragmas, value.Embeds, value.Exports, value.Stencils);
        }
    }
}}}}