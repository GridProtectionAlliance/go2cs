//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:39:27 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace encoding
{
    public static partial class csv_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ParseError
        {
            // Constructors
            public ParseError(NilType _)
            {
                this.StartLine = default;
                this.Line = default;
                this.Column = default;
                this.Err = default;
            }

            public ParseError(nint StartLine = default, nint Line = default, nint Column = default, error Err = default)
            {
                this.StartLine = StartLine;
                this.Line = Line;
                this.Column = Column;
                this.Err = Err;
            }

            // Enable comparisons between nil and ParseError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ParseError value, NilType nil) => value.Equals(default(ParseError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ParseError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ParseError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ParseError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ParseError(NilType nil) => default(ParseError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static ParseError ParseError_cast(dynamic value)
        {
            return new ParseError(value.StartLine, value.Line, value.Column, value.Err);
        }
    }
}}