//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:32 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace encoding
{
    public static partial class pem_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct lineBreaker
        {
            // Constructors
            public lineBreaker(NilType _)
            {
                this.line = default;
                this.used = default;
                this.@out = default;
            }

            public lineBreaker(array<byte> line = default, nint used = default, io.Writer @out = default)
            {
                this.line = line;
                this.used = used;
                this.@out = @out;
            }

            // Enable comparisons between nil and lineBreaker struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(lineBreaker value, NilType nil) => value.Equals(default(lineBreaker));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(lineBreaker value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, lineBreaker value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, lineBreaker value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator lineBreaker(NilType nil) => default(lineBreaker);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static lineBreaker lineBreaker_cast(dynamic value)
        {
            return new lineBreaker(value.line, value.used, value.@out);
        }
    }
}}