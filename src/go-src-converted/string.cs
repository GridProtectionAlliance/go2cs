// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package stringintconv defines an Analyzer that flags type conversions
// from integers to strings.
// package stringintconv -- go2cs converted at 2022 March 06 23:34:14 UTC
// import "golang.org/x/tools/go/analysis/passes/stringintconv" ==> using stringintconv = go.golang.org.x.tools.go.analysis.passes.stringintconv_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\stringintconv\string.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class stringintconv_package {

public static readonly @string Doc = @"check for string(int) conversions

This checker flags conversions of the form string(x) where x is an integer
(but not byte or rune) type. Such conversions are discouraged because they
return the UTF-8 representation of the Unicode code point x, and not a decimal
string representation of x as one might expect. Furthermore, if x denotes an
invalid code point, the conversion cannot be statically rejected.

For conversions that intend on using the code point, consider replacing them
with string(rune(x)). Otherwise, strconv.Itoa and its equivalents return the
string representation of the value in the desired base.
";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"stringintconv",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static @string typeName(types.Type typ) {
    {

        if (v != null) {
            return v.Name();
        }
    }

    {

        if (v != null) {
            return v.Obj().Name();
        }
    }

    return "";

}

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();
    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>(); 

        // Retrieve target type name.
        ptr<types.TypeName> tname;
        switch (call.Fun.type()) {
            case ptr<ast.Ident> fun:
                tname, _ = pass.TypesInfo.Uses[fun]._<ptr<types.TypeName>>();
                break;
            case ptr<ast.SelectorExpr> fun:
                tname, _ = pass.TypesInfo.Uses[fun.Sel]._<ptr<types.TypeName>>();
                break;
        }
        if (tname == null) {
            return ;
        }
        var target = tname.Name(); 

        // Check that target type T in T(v) has an underlying type of string.
        ptr<types.Basic> (T, _) = tname.Type().Underlying()._<ptr<types.Basic>>();
        if (T == null || T.Kind() != types.String) {
            return ;
        }
        {
            var s__prev1 = s;

            var s = T.Name();

            if (target != s) {
                target += " (" + s + ")";
            } 

            // Check that type V of v has an underlying integral type that is not byte or rune.

            s = s__prev1;

        } 

        // Check that type V of v has an underlying integral type that is not byte or rune.
        if (len(call.Args) != 1) {
            return ;
        }
        var v = call.Args[0];
        var vtyp = pass.TypesInfo.TypeOf(v);
        ptr<types.Basic> (V, _) = vtyp.Underlying()._<ptr<types.Basic>>();
        if (V == null || V.Info() & types.IsInteger == 0) {
            return ;
        }

        if (V.Kind() == types.Byte || V.Kind() == types.Rune || V.Kind() == types.UntypedRune) 
            return ;
        // Retrieve source type name.
        var source = typeName(vtyp);
        if (source == "") {
            return ;
        }
        {
            var s__prev1 = s;

            s = V.Name();

            if (source != s) {
                source += " (" + s + ")";
            }

            s = s__prev1;

        }

        analysis.Diagnostic diag = new analysis.Diagnostic(Pos:n.Pos(),Message:fmt.Sprintf("conversion from %s to %s yields a string of one rune",source,target),SuggestedFixes:[]analysis.SuggestedFix{{Message:"Did you mean to convert a rune to a string?",TextEdits:[]analysis.TextEdit{{Pos:v.Pos(),End:v.Pos(),NewText:[]byte("rune("),},{Pos:v.End(),End:v.End(),NewText:[]byte(")"),},},},},);
        pass.Report(diag);

    });
    return (null, error.As(null!)!);

}

} // end stringintconv_package
