//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:58:36 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using io = go.io_package;
using syntax = go.regexp.syntax_package;
using sync = go.sync_package;

#nullable enable

namespace go
{
    public static partial class regexp_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct inputs
        {
            // Constructors
            public inputs(NilType _)
            {
                this.bytes = default;
                this.@string = default;
                this.reader = default;
            }

            public inputs(inputBytes bytes = default, inputString @string = default, inputReader reader = default)
            {
                this.bytes = bytes;
                this.@string = @string;
                this.reader = reader;
            }

            // Enable comparisons between nil and inputs struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(inputs value, NilType nil) => value.Equals(default(inputs));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(inputs value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, inputs value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, inputs value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator inputs(NilType nil) => default(inputs);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static inputs inputs_cast(dynamic value)
        {
            return new inputs(value.bytes, value.@string, value.reader);
        }
    }
}