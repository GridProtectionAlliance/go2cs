//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:38:03 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using go;

#nullable enable

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Prog
        {
            // Constructors
            public Prog(NilType _)
            {
                this.Inst = default;
                this.Start = default;
                this.NumCap = default;
            }

            public Prog(slice<Inst> Inst = default, nint Start = default, nint NumCap = default)
            {
                this.Inst = Inst;
                this.Start = Start;
                this.NumCap = NumCap;
            }

            // Enable comparisons between nil and Prog struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Prog value, NilType nil) => value.Equals(default(Prog));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Prog value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Prog value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Prog value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Prog(NilType nil) => default(Prog);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Prog Prog_cast(dynamic value)
        {
            return new Prog(value.Inst, value.Start, value.NumCap);
        }
    }
}}