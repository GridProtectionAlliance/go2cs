// package analysis -- go2cs converted at 2020 October 08 04:54:15 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis" ==> using analysis = go.cmd.vendor.golang.org.x.tools.go.analysis_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\analysis.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;

using analysisinternal = go.golang.org.x.tools.@internal.analysisinternal_package;
using static go.builtin;
using System;

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
        // An Analyzer describes an analysis function and its options.
        public partial struct Analyzer
        {
            public @string Name; // Doc is the documentation for the analyzer.
// The part before the first "\n\n" is the title
// (no capital or period, max ~60 letters).
            public @string Doc; // Flags defines any flags accepted by the analyzer.
// The manner in which these flags are exposed to the user
// depends on the driver which runs the analyzer.
            public flag.FlagSet Flags; // Run applies the analyzer to a package.
// It returns an error if the analyzer failed.
//
// On success, the Run function may return a result
// computed by the Analyzer; its type must match ResultType.
// The driver makes this result available as an input to
// another Analyzer that depends directly on this one (see
// Requires) when it analyzes the same package.
//
// To pass analysis results between packages (and thus
// potentially between address spaces), use Facts, which are
// serializable.
            public Func<ptr<Pass>, (object, error)> Run; // RunDespiteErrors allows the driver to invoke
// the Run method of this analyzer even on a
// package that contains parse or type errors.
            public bool RunDespiteErrors; // Requires is a set of analyzers that must run successfully
// before this one on a given package. This analyzer may inspect
// the outputs produced by each analyzer in Requires.
// The graph over analyzers implied by Requires edges must be acyclic.
//
// Requires establishes a "horizontal" dependency between
// analysis passes (different analyzers, same package).
            public slice<ptr<Analyzer>> Requires; // ResultType is the type of the optional result of the Run function.
            public reflect.Type ResultType; // FactTypes indicates that this analyzer imports and exports
// Facts of the specified concrete types.
// An analyzer that uses facts may assume that its import
// dependencies have been similarly analyzed before it runs.
// Facts must be pointers.
//
// FactTypes establishes a "vertical" dependency between
// analysis passes (same analyzer, different packages).
            public slice<Fact> FactTypes;
        }

        private static @string String(this ptr<Analyzer> _addr_a)
        {
            ref Analyzer a = ref _addr_a.val;

            return a.Name;
        }

        private static void init()
        { 
            // Set the analysisinternal functions to be able to pass type errors
            // to the Pass type without modifying the go/analysis API.
            analysisinternal.SetTypeErrors = (p, errors) =>
            {
                p._<ptr<Pass>>().typeErrors = errors;
            }
;
            analysisinternal.GetTypeErrors = p =>
            {
                return p._<ptr<Pass>>().typeErrors;
            }
;

        }

        // A Pass provides information to the Run function that
        // applies a specific analyzer to a single Go package.
        //
        // It forms the interface between the analysis logic and the driver
        // program, and has both input and an output components.
        //
        // As in a compiler, one pass may depend on the result computed by another.
        //
        // The Run function should not call any of the Pass functions concurrently.
        public partial struct Pass
        {
            public ptr<Analyzer> Analyzer; // the identity of the current analyzer

// syntax and type information
            public ptr<token.FileSet> Fset; // file position information
            public slice<ptr<ast.File>> Files; // the abstract syntax tree of each file
            public slice<@string> OtherFiles; // names of non-Go files of this package
            public ptr<types.Package> Pkg; // type information about the package
            public ptr<types.Info> TypesInfo; // type information about the syntax trees
            public types.Sizes TypesSizes; // function for computing sizes of types

// Report reports a Diagnostic, a finding about a specific location
// in the analyzed source code such as a potential mistake.
// It may be called by the Run function.
            public Action<Diagnostic> Report; // ResultOf provides the inputs to this analysis pass, which are
// the corresponding results of its prerequisite analyzers.
// The map keys are the elements of Analysis.Required,
// and the type of each corresponding value is the required
// analysis's ResultType.
            public Func<types.Object, Fact, bool> ImportObjectFact; // ImportPackageFact retrieves a fact associated with package pkg,
// which must be this package or one of its dependencies.
// See comments for ImportObjectFact.
            public Func<ptr<types.Package>, Fact, bool> ImportPackageFact; // ExportObjectFact associates a fact of type *T with the obj,
// replacing any previous fact of that type.
//
// ExportObjectFact panics if it is called after the pass is
// complete, or if obj does not belong to the package being analyzed.
// ExportObjectFact is not concurrency-safe.
            public Action<types.Object, Fact> ExportObjectFact; // ExportPackageFact associates a fact with the current package.
// See comments for ExportObjectFact.
            public Action<Fact> ExportPackageFact; // AllPackageFacts returns a new slice containing all package facts of the analysis's FactTypes
// in unspecified order.
// WARNING: This is an experimental API and may change in the future.
            public Func<slice<PackageFact>> AllPackageFacts; // AllObjectFacts returns a new slice containing all object facts of the analysis's FactTypes
// in unspecified order.
// WARNING: This is an experimental API and may change in the future.
            public Func<slice<ObjectFact>> AllObjectFacts; // typeErrors contains types.Errors that are associated with the pkg.
            public slice<types.Error> typeErrors; /* Further fields may be added in future. */
// For example, suggested or applied refactorings.
        }

        // PackageFact is a package together with an associated fact.
        // WARNING: This is an experimental API and may change in the future.
        public partial struct PackageFact
        {
            public ptr<types.Package> Package;
            public Fact Fact;
        }

        // ObjectFact is an object together with an associated fact.
        // WARNING: This is an experimental API and may change in the future.
        public partial struct ObjectFact
        {
            public types.Object Object;
            public Fact Fact;
        }

        // Reportf is a helper function that reports a Diagnostic using the
        // specified position and formatted error message.
        private static void Reportf(this ptr<Pass> _addr_pass, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Pass pass = ref _addr_pass.val;

            var msg = fmt.Sprintf(format, args);
            pass.Report(new Diagnostic(Pos:pos,Message:msg));
        }

        // The Range interface provides a range. It's equivalent to and satisfied by
        // ast.Node.
        public partial interface Range
        {
            token.Pos Pos(); // position of first character belonging to the node
            token.Pos End(); // position of first character immediately after the node
        }

        // ReportRangef is a helper function that reports a Diagnostic using the
        // range provided. ast.Node values can be passed in as the range because
        // they satisfy the Range interface.
        private static void ReportRangef(this ptr<Pass> _addr_pass, Range rng, @string format, params object[] args)
        {
            args = args.Clone();
            ref Pass pass = ref _addr_pass.val;

            var msg = fmt.Sprintf(format, args);
            pass.Report(new Diagnostic(Pos:rng.Pos(),End:rng.End(),Message:msg));
        }

        private static @string String(this ptr<Pass> _addr_pass)
        {
            ref Pass pass = ref _addr_pass.val;

            return fmt.Sprintf("%s@%s", pass.Analyzer.Name, pass.Pkg.Path());
        }

        // A Fact is an intermediate fact produced during analysis.
        //
        // Each fact is associated with a named declaration (a types.Object) or
        // with a package as a whole. A single object or package may have
        // multiple associated facts, but only one of any particular fact type.
        //
        // A Fact represents a predicate such as "never returns", but does not
        // represent the subject of the predicate such as "function F" or "package P".
        //
        // Facts may be produced in one analysis pass and consumed by another
        // analysis pass even if these are in different address spaces.
        // If package P imports Q, all facts about Q produced during
        // analysis of that package will be available during later analysis of P.
        // Facts are analogous to type export data in a build system:
        // just as export data enables separate compilation of several passes,
        // facts enable "separate analysis".
        //
        // Each pass (a, p) starts with the set of facts produced by the
        // same analyzer a applied to the packages directly imported by p.
        // The analysis may add facts to the set, and they may be exported in turn.
        // An analysis's Run function may retrieve facts by calling
        // Pass.Import{Object,Package}Fact and update them using
        // Pass.Export{Object,Package}Fact.
        //
        // A fact is logically private to its Analysis. To pass values
        // between different analyzers, use the results mechanism;
        // see Analyzer.Requires, Analyzer.ResultType, and Pass.ResultOf.
        //
        // A Fact type must be a pointer.
        // Facts are encoded and decoded using encoding/gob.
        // A Fact may implement the GobEncoder/GobDecoder interfaces
        // to customize its encoding. Fact encoding should not fail.
        //
        // A Fact should not be modified once exported.
        public partial interface Fact
        {
            void AFact(); // dummy method to avoid type errors
        }
    }
}}}}}}}
