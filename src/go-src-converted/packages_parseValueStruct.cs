//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 08 04:55:12 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using context = go.context_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using gcexportdata = go.golang.org.x.tools.go.gcexportdata_package;
using gocommand = go.golang.org.x.tools.@internal.gocommand_package;
using packagesinternal = go.golang.org.x.tools.@internal.packagesinternal_package;
using typesinternal = go.golang.org.x.tools.@internal.typesinternal_package;
using go;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct parseValue
        {
            // Constructors
            public parseValue(NilType _)
            {
                this.f = default;
                this.err = default;
                this.ready = default;
            }

            public parseValue(ref ptr<ast.File> f = default, error err = default, channel<object> ready = default)
            {
                this.f = f;
                this.err = err;
                this.ready = ready;
            }

            // Enable comparisons between nil and parseValue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(parseValue value, NilType nil) => value.Equals(default(parseValue));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(parseValue value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, parseValue value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, parseValue value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator parseValue(NilType nil) => default(parseValue);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static parseValue parseValue_cast(dynamic value)
        {
            return new parseValue(ref value.f, value.err, value.ready);
        }
    }
}}}}}