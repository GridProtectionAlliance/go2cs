//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:27:21 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using types2 = go.cmd.compile.@internal.types2_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using io = go.io_package;
using big = go.math.big_package;
using sort = go.sort_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class importer_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct importReader
        {
            // Constructors
            public importReader(NilType _)
            {
                this.p = default;
                this.declReader = default;
                this.currPkg = default;
                this.prevFile = default;
                this.prevLine = default;
                this.prevColumn = default;
            }

            public importReader(ref ptr<iimporter> p = default, bytes.Reader declReader = default, ref ptr<types2.Package> currPkg = default, @string prevFile = default, long prevLine = default, long prevColumn = default)
            {
                this.p = p;
                this.declReader = declReader;
                this.currPkg = currPkg;
                this.prevFile = prevFile;
                this.prevLine = prevLine;
                this.prevColumn = prevColumn;
            }

            // Enable comparisons between nil and importReader struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(importReader value, NilType nil) => value.Equals(default(importReader));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(importReader value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, importReader value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, importReader value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator importReader(NilType nil) => default(importReader);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static importReader importReader_cast(dynamic value)
        {
            return new importReader(ref value.p, value.declReader, ref value.currPkg, value.prevFile, value.prevLine, value.prevColumn);
        }
    }
}}}}