//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:25:49 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using syntax = go.cmd.compile.@internal.syntax_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types2_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct bailout
        {
            // Constructors
            public bailout(NilType _)
            {
            }
            // Enable comparisons between nil and bailout struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(bailout value, NilType nil) => value.Equals(default(bailout));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(bailout value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, bailout value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, bailout value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator bailout(NilType nil) => default(bailout);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static bailout bailout_cast(dynamic value)
        {
            return new bailout();
        }
    }
}}}}