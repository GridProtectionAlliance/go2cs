//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:45:34 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using bidirule = go.golang.org.x.text.secure.bidirule_package;
using norm = go.golang.org.x.text.unicode.norm_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class idna_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct labelError
        {
            // Constructors
            public labelError(NilType _)
            {
                this.label = default;
                this.code_ = default;
            }

            public labelError(@string label = default, @string code_ = default)
            {
                this.label = label;
                this.code_ = code_;
            }

            // Enable comparisons between nil and labelError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(labelError value, NilType nil) => value.Equals(default(labelError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(labelError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, labelError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, labelError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator labelError(NilType nil) => default(labelError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static labelError labelError_cast(dynamic value)
        {
            return new labelError(value.label, value.code_);
        }
    }
}}}}}