//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 09:28:20 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using types = go.cmd.compile.@internal.types_package;
using gcprog = go.cmd.@internal.gcprog_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using go;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Sig
        {
            // Constructors
            public Sig(NilType _)
            {
                this.name = default;
                this.pkg = default;
                this.isym = default;
                this.tsym = default;
                this.type_ = default;
                this.mtype = default;
                this.offset = default;
            }

            public Sig(@string name = default, ref ptr<types.Pkg> pkg = default, ref ptr<types.Sym> isym = default, ref ptr<types.Sym> tsym = default, ref ptr<types.Type> type_ = default, ref ptr<types.Type> mtype = default, int offset = default)
            {
                this.name = name;
                this.pkg = pkg;
                this.isym = isym;
                this.tsym = tsym;
                this.type_ = type_;
                this.mtype = mtype;
                this.offset = offset;
            }

            // Enable comparisons between nil and Sig struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Sig value, NilType nil) => value.Equals(default(Sig));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Sig value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Sig value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Sig value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Sig(NilType nil) => default(Sig);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Sig Sig_cast(dynamic value)
        {
            return new Sig(value.name, ref value.pkg, ref value.isym, ref value.tsym, ref value.type_, ref value.mtype, value.offset);
        }
    }
}}}}