//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:41 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using fmt = go.fmt_package;
using math = go.math_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using time = go.time_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace encoding
{
    public static partial class asn1_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct StructuralError
        {
            // Constructors
            public StructuralError(NilType _)
            {
                this.Msg = default;
            }

            public StructuralError(@string Msg = default)
            {
                this.Msg = Msg;
            }

            // Enable comparisons between nil and StructuralError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(StructuralError value, NilType nil) => value.Equals(default(StructuralError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(StructuralError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, StructuralError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, StructuralError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator StructuralError(NilType nil) => default(StructuralError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static StructuralError StructuralError_cast(dynamic value)
        {
            return new StructuralError(value.Msg);
        }
    }
}}