//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:34:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using sha1 = go.crypto.sha1_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using hex = go.encoding.hex_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Elfaux
        {
            // Constructors
            public Elfaux(NilType _)
            {
                this.next = default;
                this.num = default;
                this.vers = default;
            }

            public Elfaux(ref ptr<Elfaux> next = default, nint num = default, @string vers = default)
            {
                this.next = next;
                this.num = num;
                this.vers = vers;
            }

            // Enable comparisons between nil and Elfaux struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Elfaux value, NilType nil) => value.Equals(default(Elfaux));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Elfaux value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Elfaux value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Elfaux value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Elfaux(NilType nil) => default(Elfaux);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Elfaux Elfaux_cast(dynamic value)
        {
            return new Elfaux(ref value.next, value.num, value.vers);
        }
    }
}}}}