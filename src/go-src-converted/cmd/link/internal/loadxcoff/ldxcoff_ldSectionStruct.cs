//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:34:48 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class loadxcoff_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(xcoff.Section))]
        private partial struct ldSection
        {
            // Section structure promotion - sourced from value copy
            private readonly ptr<Section> m_SectionRef;

            private ref Section Section_val => ref m_SectionRef.Value;

            public ref slice<Reloc> Relocs => ref m_SectionRef.Value.Relocs;

            public ref ptr<io.SectionReader> sr => ref m_SectionRef.Value.sr;

            // Constructors
            public ldSection(NilType _)
            {
                this.m_SectionRef = new ptr<xcoff.Section>(new xcoff.Section(nil));
                this.sym = default;
            }

            public ldSection(xcoff.Section Section = default, loader.Sym sym = default)
            {
                this.m_SectionRef = new ptr<xcoff.Section>(Section);
                this.sym = sym;
            }

            // Enable comparisons between nil and ldSection struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ldSection value, NilType nil) => value.Equals(default(ldSection));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ldSection value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ldSection value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ldSection value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ldSection(NilType nil) => default(ldSection);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static ldSection ldSection_cast(dynamic value)
        {
            return new ldSection(value.Section, value.sym);
        }
    }
}}}}