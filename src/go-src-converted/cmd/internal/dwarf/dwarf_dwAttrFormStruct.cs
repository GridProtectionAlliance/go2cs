//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:57:23 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using exec = go.@internal.execabs_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using objabi = go.cmd.@internal.objabi_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class dwarf_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct dwAttrForm
        {
            // Constructors
            public dwAttrForm(NilType _)
            {
                this.attr = default;
                this.form = default;
            }

            public dwAttrForm(ushort attr = default, byte form = default)
            {
                this.attr = attr;
                this.form = form;
            }

            // Enable comparisons between nil and dwAttrForm struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(dwAttrForm value, NilType nil) => value.Equals(default(dwAttrForm));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(dwAttrForm value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, dwAttrForm value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, dwAttrForm value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator dwAttrForm(NilType nil) => default(dwAttrForm);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static dwAttrForm dwAttrForm_cast(dynamic value)
        {
            return new dwAttrForm(value.attr, value.form);
        }
    }
}}}