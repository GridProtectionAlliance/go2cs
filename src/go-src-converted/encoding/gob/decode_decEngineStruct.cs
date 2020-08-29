//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:35:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using encoding = go.encoding_package;
using errors = go.errors_package;
using io = go.io_package;
using math = go.math_package;
using bits = go.math.bits_package;
using reflect = go.reflect_package;
using go;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct decEngine
        {
            // Constructors
            public decEngine(NilType _)
            {
                this.instr = default;
                this.numInstr = default;
            }

            public decEngine(slice<decInstr> instr = default, long numInstr = default)
            {
                this.instr = instr;
                this.numInstr = numInstr;
            }

            // Enable comparisons between nil and decEngine struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(decEngine value, NilType nil) => value.Equals(default(decEngine));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(decEngine value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, decEngine value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, decEngine value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator decEngine(NilType nil) => default(decEngine);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static decEngine decEngine_cast(dynamic value)
        {
            return new decEngine(value.instr, value.numInstr);
        }
    }
}}