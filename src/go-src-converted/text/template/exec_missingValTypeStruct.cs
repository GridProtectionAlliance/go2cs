//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:39:10 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using fmtsort = go.@internal.fmtsort_package;
using io = go.io_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using parse = go.text.template.parse_package;
using go;

#nullable enable

namespace go {
namespace text
{
    public static partial class template_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct missingValType
        {
            // Constructors
            public missingValType(NilType _)
            {
            }
            // Enable comparisons between nil and missingValType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(missingValType value, NilType nil) => value.Equals(default(missingValType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(missingValType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, missingValType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, missingValType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator missingValType(NilType nil) => default(missingValType);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static missingValType missingValType_cast(dynamic value)
        {
            return new missingValType();
        }
    }
}}