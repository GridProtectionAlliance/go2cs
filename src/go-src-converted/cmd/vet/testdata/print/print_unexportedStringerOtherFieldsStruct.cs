//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using logpkg = go.log_package;
using math = go.math_package;
using os = go.os_package;
using testing = go.testing_package;
using @unsafe = go.@unsafe_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class print_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct unexportedStringerOtherFields
        {
            // Constructors
            public unexportedStringerOtherFields(NilType _)
            {
                this.s = default;
                this.t = default;
                this.S = default;
            }

            public unexportedStringerOtherFields(@string s = default, ptrStringer t = default, @string S = default)
            {
                this.s = s;
                this.t = t;
                this.S = S;
            }

            // Enable comparisons between nil and unexportedStringerOtherFields struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(unexportedStringerOtherFields value, NilType nil) => value.Equals(default(unexportedStringerOtherFields));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(unexportedStringerOtherFields value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, unexportedStringerOtherFields value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, unexportedStringerOtherFields value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator unexportedStringerOtherFields(NilType nil) => default(unexportedStringerOtherFields);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static unexportedStringerOtherFields unexportedStringerOtherFields_cast(dynamic value)
        {
            return new unexportedStringerOtherFields(value.s, value.t, value.S);
        }
    }
}}}}