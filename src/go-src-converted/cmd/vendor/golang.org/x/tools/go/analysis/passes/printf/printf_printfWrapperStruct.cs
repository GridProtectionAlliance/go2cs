//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:42:00 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class printf_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct printfWrapper
        {
            // Constructors
            public printfWrapper(NilType _)
            {
                this.obj = default;
                this.fdecl = default;
                this.format = default;
                this.args = default;
                this.callers = default;
                this.failed = default;
            }

            public printfWrapper(ref ptr<types.Func> obj = default, ref ptr<ast.FuncDecl> fdecl = default, ref ptr<types.Var> format = default, ref ptr<types.Var> args = default, slice<printfCaller> callers = default, bool failed = default)
            {
                this.obj = obj;
                this.fdecl = fdecl;
                this.format = format;
                this.args = args;
                this.callers = callers;
                this.failed = failed;
            }

            // Enable comparisons between nil and printfWrapper struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(printfWrapper value, NilType nil) => value.Equals(default(printfWrapper));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(printfWrapper value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, printfWrapper value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, printfWrapper value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator printfWrapper(NilType nil) => default(printfWrapper);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static printfWrapper printfWrapper_cast(dynamic value)
        {
            return new printfWrapper(ref value.obj, ref value.fdecl, ref value.format, ref value.args, value.callers, value.failed);
        }
    }
}}}}}}}}}