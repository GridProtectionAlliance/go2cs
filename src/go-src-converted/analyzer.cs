// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sortslice defines an Analyzer that checks for calls
// to sort.Slice that do not use a slice type as first argument.
// package sortslice -- go2cs converted at 2022 March 06 23:34:12 UTC
// import "golang.org/x/tools/go/analysis/passes/sortslice" ==> using sortslice = go.golang.org.x.tools.go.analysis.passes.sortslice_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\sortslice\analyzer.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using System;


namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class sortslice_package {

public static readonly @string Doc = "check the argument type of sort.Slice\n\nsort.Slice requires an argument of a slice" +
    " type. Check that\nthe interface{} value passed to sort.Slice is actually a slice" +
    ".";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"sortslice",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });

    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
        ptr<types.Func> (fn, _) = typeutil.Callee(pass.TypesInfo, call)._<ptr<types.Func>>();
        if (fn == null) {
            return ;
        }
        if (fn.FullName() != "sort.Slice") {
            return ;
        }
        var arg = call.Args[0];
        var typ = pass.TypesInfo.Types[arg].Type;
        switch (typ.Underlying().type()) {
            case ptr<types.Slice> _:
                return ;
                break;
            case ptr<types.Interface> _:
                return ;
                break;

        }

        slice<analysis.SuggestedFix> fixes = default;
        switch (typ.Underlying().type()) {
            case ptr<types.Array> v:
                ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
                format.Node(_addr_buf, pass.Fset, addr(new ast.SliceExpr(X:arg,Slice3:false,Lbrack:arg.End()+1,Rbrack:arg.End()+3,)));
                fixes = append(fixes, new analysis.SuggestedFix(Message:"Get a slice of the full array",TextEdits:[]analysis.TextEdit{{Pos:arg.Pos(),End:arg.End(),NewText:buf.Bytes(),}},));
                break;
            case ptr<types.Pointer> v:
                ptr<types.Slice> (_, ok) = v.Elem().Underlying()._<ptr<types.Slice>>();
                if (!ok) {
                    break;
                }
                buf = default;
                format.Node(_addr_buf, pass.Fset, addr(new ast.StarExpr(X:arg,)));
                fixes = append(fixes, new analysis.SuggestedFix(Message:"Dereference the pointer to the slice",TextEdits:[]analysis.TextEdit{{Pos:arg.Pos(),End:arg.End(),NewText:buf.Bytes(),}},));
                break;
            case ptr<types.Signature> v:
                if (v.Params().Len() != 0 || v.Results().Len() != 1) {
                    break;
                }
                {
                    (_, ok) = v.Results().At(0).Type().Underlying()._<ptr<types.Slice>>();

                    if (!ok) {
                        break;
                    }

                }

                buf = default;
                format.Node(_addr_buf, pass.Fset, addr(new ast.CallExpr(Fun:arg,)));
                fixes = append(fixes, new analysis.SuggestedFix(Message:"Call the function",TextEdits:[]analysis.TextEdit{{Pos:arg.Pos(),End:arg.End(),NewText:buf.Bytes(),}},));
                break;

        }

        pass.Report(new analysis.Diagnostic(Pos:call.Pos(),End:call.End(),Message:fmt.Sprintf("sort.Slice's argument must be a slice; is called with %s",typ.String()),SuggestedFixes:fixes,));

    });
    return (null, error.As(null!)!);

}

} // end sortslice_package
