// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package composite defines an Analyzer that checks for unkeyed
// composite literals.
// package composite -- go2cs converted at 2022 March 06 23:34:33 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/composite" ==> using composite = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.composite_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\composite\composite.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class composite_package {

public static readonly @string Doc = @"check for unkeyed composite literals

This analyzer reports a diagnostic for composite literals of struct
types imported from another package that do not use the field-keyed
syntax. Such literals are fragile because the addition of a new field
(even if unexported) to the struct will cause compilation to fail.

As an example,

	err = &net.DNSConfigError{err}

should be replaced by:

	err = &net.DNSConfigError{Err: err}
";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"composites",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},RunDespiteErrors:true,Run:run,));

private static var whitelist = true;

private static void init() {
    Analyzer.Flags.BoolVar(_addr_whitelist, "whitelist", whitelist, "use composite white list; for testing only");
}

// runUnkeyedLiteral checks if a composite literal is a struct literal with
// unkeyed fields.
private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CompositeLit)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CompositeLit> cl = n._<ptr<ast.CompositeLit>>();

        var typ = pass.TypesInfo.Types[cl].Type;
        if (typ == null) { 
            // cannot determine composite literals' type, skip it
            return ;

        }
        var typeName = typ.String();
        if (whitelist && unkeyedLiteral[typeName]) { 
            // skip whitelisted types
            return ;

        }
        var under = typ.Underlying();
        while (true) {
            ptr<types.Pointer> (ptr, ok) = under._<ptr<types.Pointer>>();
            if (!ok) {
                break;
            }
            under = ptr.Elem().Underlying();
        }
        {
            ptr<types.Struct> (_, ok) = under._<ptr<types.Struct>>();

            if (!ok) { 
                // skip non-struct composite literals
                return ;

            }

        }

        if (isLocalType(_addr_pass, typ)) { 
            // allow unkeyed locally defined composite literal
            return ;

        }
        var allKeyValue = true;
        foreach (var (_, e) in cl.Elts) {
            {
                (_, ok) = e._<ptr<ast.KeyValueExpr>>();

                if (!ok) {
                    allKeyValue = false;
                    break;
                }

            }

        }        if (allKeyValue) { 
            // all the composite literal fields are keyed
            return ;

        }
        pass.ReportRangef(cl, "%s composite literal uses unkeyed fields", typeName);

    });
    return (null, error.As(null!)!);

}

private static bool isLocalType(ptr<analysis.Pass> _addr_pass, types.Type typ) {
    ref analysis.Pass pass = ref _addr_pass.val;

    switch (typ.type()) {
        case ptr<types.Struct> x:
            return true;
            break;
        case ptr<types.Pointer> x:
            return isLocalType(_addr_pass, x.Elem());
            break;
        case ptr<types.Named> x:
            return strings.TrimSuffix(x.Obj().Pkg().Path(), "_test") == strings.TrimSuffix(pass.Pkg.Path(), "_test");
            break;
    }
    return false;

}

} // end composite_package
