//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:51 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @internal = go.golang.org.x.xerrors.@internal_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x
{
    public static partial class xerrors_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct noWrapError
        {
            // Constructors
            public noWrapError(NilType _)
            {
                this.msg = default;
                this.err = default;
                this.frame = default;
            }

            public noWrapError(@string msg = default, error err = default, Frame frame = default)
            {
                this.msg = msg;
                this.err = err;
                this.frame = frame;
            }

            // Enable comparisons between nil and noWrapError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(noWrapError value, NilType nil) => value.Equals(default(noWrapError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(noWrapError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, noWrapError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, noWrapError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator noWrapError(NilType nil) => default(noWrapError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static noWrapError noWrapError_cast(dynamic value)
        {
            return new noWrapError(value.msg, value.err, value.frame);
        }
    }
}}}}}