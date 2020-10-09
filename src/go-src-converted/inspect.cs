// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package inspect defines an Analyzer that provides an AST inspector
// (golang.org/x/tools/go/ast/inspect.Inspect) for the syntax trees of a
// package. It is only a building block for other analyzers.
//
// Example of use in another analysis:
//
//    import (
//        "golang.org/x/tools/go/analysis"
//        "golang.org/x/tools/go/analysis/passes/inspect"
//        "golang.org/x/tools/go/ast/inspector"
//    )
//
//    var Analyzer = &analysis.Analyzer{
//        ...
//        Requires:       []*analysis.Analyzer{inspect.Analyzer},
//    }
//
//     func run(pass *analysis.Pass) (interface{}, error) {
//         inspect := pass.ResultOf[inspect.Analyzer].(*inspector.Inspector)
//         inspect.Preorder(nil, func(n ast.Node) {
//             ...
//         })
//         return nil
//     }
//
// package inspect -- go2cs converted at 2020 October 09 06:02:58 UTC
// import "golang.org/x/tools/go/analysis/passes/inspect" ==> using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\inspect\inspect.go
using reflect = go.reflect_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class inspect_package
    {
        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"inspect",Doc:"optimize AST traversal for later passes",Run:run,RunDespiteErrors:true,ResultType:reflect.TypeOf(new(inspector.Inspector)),));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            return (inspector.New(pass.Files), error.As(null!)!);
        }
    }
}}}}}}}
