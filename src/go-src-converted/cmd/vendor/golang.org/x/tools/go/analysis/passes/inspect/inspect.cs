// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package inspect defines an Analyzer that provides an AST inspector
// (golang.org/x/tools/go/ast/inspector.Inspector) for the syntax trees
// of a package. It is only a building block for other analyzers.
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

// package inspect -- go2cs converted at 2022 March 13 06:41:53 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/inspect" ==> using inspect = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.inspect_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\inspect\inspect.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using reflect = reflect_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;

public static partial class inspect_package {

public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"inspect",Doc:"optimize AST traversal for later passes",Run:run,RunDespiteErrors:true,ResultType:reflect.TypeOf(new(inspector.Inspector)),));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    return (inspector.New(pass.Files), error.As(null!)!);
}

} // end inspect_package
