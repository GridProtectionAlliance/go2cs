//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:41:34 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using analysisinternal = go.golang.org.x.tools.@internal.analysisinternal_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class analysis_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Pass
        {
            // Constructors
            public Pass(NilType _)
            {
                this.Analyzer = default;
                this.Fset = default;
                this.Files = default;
                this.OtherFiles = default;
                this.IgnoredFiles = default;
                this.Pkg = default;
                this.TypesInfo = default;
                this.TypesSizes = default;
                this.Report = default;
                this.ImportObjectFact = default;
                this.ImportPackageFact = default;
                this.ExportObjectFact = default;
                this.ExportPackageFact = default;
                this.AllPackageFacts = default;
                this.AllObjectFacts = default;
                this.typeErrors = default;
            }

            public Pass(ref ptr<Analyzer> Analyzer = default, ref ptr<token.FileSet> Fset = default, slice<ptr<ast.File>> Files = default, slice<@string> OtherFiles = default, slice<@string> IgnoredFiles = default, ref ptr<types.Package> Pkg = default, ref ptr<types.Info> TypesInfo = default, types.Sizes TypesSizes = default, Action<Diagnostic> Report = default, Func<types.Object, Fact, bool> ImportObjectFact = default, Func<ptr<types.Package>, Fact, bool> ImportPackageFact = default, Action<types.Object, Fact> ExportObjectFact = default, Action<Fact> ExportPackageFact = default, Func<slice<PackageFact>> AllPackageFacts = default, Func<slice<ObjectFact>> AllObjectFacts = default, slice<types.Error> typeErrors = default)
            {
                this.Analyzer = Analyzer;
                this.Fset = Fset;
                this.Files = Files;
                this.OtherFiles = OtherFiles;
                this.IgnoredFiles = IgnoredFiles;
                this.Pkg = Pkg;
                this.TypesInfo = TypesInfo;
                this.TypesSizes = TypesSizes;
                this.Report = Report;
                this.ImportObjectFact = ImportObjectFact;
                this.ImportPackageFact = ImportPackageFact;
                this.ExportObjectFact = ExportObjectFact;
                this.ExportPackageFact = ExportPackageFact;
                this.AllPackageFacts = AllPackageFacts;
                this.AllObjectFacts = AllObjectFacts;
                this.typeErrors = typeErrors;
            }

            // Enable comparisons between nil and Pass struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Pass value, NilType nil) => value.Equals(default(Pass));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Pass value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Pass value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Pass value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Pass(NilType nil) => default(Pass);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Pass Pass_cast(dynamic value)
        {
            return new Pass(ref value.Analyzer, ref value.Fset, value.Files, value.OtherFiles, value.IgnoredFiles, ref value.Pkg, ref value.TypesInfo, value.TypesSizes, value.Report, value.ImportObjectFact, value.ImportPackageFact, value.ExportObjectFact, value.ExportPackageFact, value.AllPackageFacts, value.AllObjectFacts, value.typeErrors);
        }
    }
}}}}}}}