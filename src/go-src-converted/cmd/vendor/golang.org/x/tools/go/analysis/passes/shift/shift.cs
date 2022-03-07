// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package shift defines an Analyzer that checks for shifts that exceed
// the width of an integer.
// package shift -- go2cs converted at 2022 March 06 23:34:45 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/shift" ==> using shift = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.shift_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\shift\shift.go
// TODO(adonovan): integrate with ctrflow (CFG-based) dead code analysis. May
// have impedance mismatch due to its (non-)treatment of constant
// expressions (such as runtime.GOARCH=="386").

using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class shift_package {

public static readonly @string Doc = "check for shifts that equal or exceed the width of the integer";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"shift",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>(); 

    // Do a complete pass to compute dead nodes.
    var dead = make_map<ast.Node, bool>();
    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.IfStmt)(nil), (*ast.SwitchStmt)(nil) });
    inspect.Preorder(nodeFilter, n => { 
        // TODO(adonovan): move updateDead into this file.
        updateDead(pass.TypesInfo, dead, n);

    });

    nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.AssignStmt)(nil), (*ast.BinaryExpr)(nil) });
    inspect.Preorder(nodeFilter, node => {
        if (dead[node]) { 
            // Skip shift checks on unreachable nodes.
            return ;

        }
        switch (node.type()) {
            case ptr<ast.BinaryExpr> node:
                if (node.Op == token.SHL || node.Op == token.SHR) {
                    checkLongShift(_addr_pass, node, node.X, node.Y);
                }
                break;
            case ptr<ast.AssignStmt> node:
                if (len(node.Lhs) != 1 || len(node.Rhs) != 1) {
                    return ;
                }
                if (node.Tok == token.SHL_ASSIGN || node.Tok == token.SHR_ASSIGN) {
                    checkLongShift(_addr_pass, node, node.Lhs[0], node.Rhs[0]);
                }
                break;
        }

    });
    return (null, error.As(null!)!);

}

// checkLongShift checks if shift or shift-assign operations shift by more than
// the length of the underlying variable.
private static void checkLongShift(ptr<analysis.Pass> _addr_pass, ast.Node node, ast.Expr x, ast.Expr y) {
    ref analysis.Pass pass = ref _addr_pass.val;

    if (pass.TypesInfo.Types[x].Value != null) { 
        // Ignore shifts of constants.
        // These are frequently used for bit-twiddling tricks
        // like ^uint(0) >> 63 for 32/64 bit detection and compatibility.
        return ;

    }
    var v = pass.TypesInfo.Types[y].Value;
    if (v == null) {
        return ;
    }
    var (amt, ok) = constant.Int64Val(v);
    if (!ok) {
        return ;
    }
    var t = pass.TypesInfo.Types[x].Type;
    if (t == null) {
        return ;
    }
    nint size = 8 * pass.TypesSizes.Sizeof(t);
    if (amt >= size) {
        var ident = analysisutil.Format(pass.Fset, x);
        pass.ReportRangef(node, "%s (%d bits) too small for shift of %d", ident, size, amt);
    }
}

} // end shift_package
