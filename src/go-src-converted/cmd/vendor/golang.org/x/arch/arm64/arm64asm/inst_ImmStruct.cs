//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 10:07:41 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using fmt = go.fmt_package;
using strings = go.strings_package;
using go;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace arch {
namespace arm64
{
    public static partial class arm64asm_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Imm
        {
            // Constructors
            public Imm(NilType _)
            {
                this.Imm = default;
                this.Decimal = default;
            }

            public Imm(uint Imm = default, bool Decimal = default)
            {
                this.Imm = Imm;
                this.Decimal = Decimal;
            }

            // Enable comparisons between nil and Imm struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Imm value, NilType nil) => value.Equals(default(Imm));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Imm value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Imm value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Imm value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Imm(NilType nil) => default(Imm);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Imm Imm_cast(dynamic value)
        {
            return new Imm(value.Imm, value.Decimal);
        }
    }
}}}}}}}