//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:32 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using token = go.go.token_package;
using math = go.math_package;
using big = go.math.big_package;
using bits = go.math.bits_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class constant_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct intVal
        {
            // Constructors
            public intVal(NilType _)
            {
                this.val = default;
            }

            public intVal(ref ptr<big.Int> val = default)
            {
                this.val = val;
            }

            // Enable comparisons between nil and intVal struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(intVal value, NilType nil) => value.Equals(default(intVal));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(intVal value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, intVal value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, intVal value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator intVal(NilType nil) => default(intVal);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static intVal intVal_cast(dynamic value)
        {
            return new intVal(ref value.val);
        }
    }
}}