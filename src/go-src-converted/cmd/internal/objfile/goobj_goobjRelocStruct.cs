//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:43:17 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using archive = go.cmd.@internal.archive_package;
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using dwarf = go.debug.dwarf_package;
using gosym = go.debug.gosym_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct goobjReloc
        {
            // Constructors
            public goobjReloc(NilType _)
            {
                this.Off = default;
                this.Size = default;
                this.Type = default;
                this.Add = default;
                this.Sym = default;
            }

            public goobjReloc(int Off = default, byte Size = default, objabi.RelocType Type = default, long Add = default, @string Sym = default)
            {
                this.Off = Off;
                this.Size = Size;
                this.Type = Type;
                this.Add = Add;
                this.Sym = Sym;
            }

            // Enable comparisons between nil and goobjReloc struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(goobjReloc value, NilType nil) => value.Equals(default(goobjReloc));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(goobjReloc value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, goobjReloc value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, goobjReloc value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator goobjReloc(NilType nil) => default(goobjReloc);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static goobjReloc goobjReloc_cast(dynamic value)
        {
            return new goobjReloc(value.Off, value.Size, value.Type, value.Add, value.Sym);
        }
    }
}}}