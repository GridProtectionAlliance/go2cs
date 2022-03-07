// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:15:49 UTC
// Original source: C:\Program Files\Go\src\cmd\fix\printerconfig.go
using ast = go.go.ast_package;
using System;


namespace go;

public static partial class main_package {

private static void init() {
    register(printerconfigFix);
}

private static fix printerconfigFix = new fix(name:"printerconfig",date:"2012-12-11",f:printerconfig,desc:`Add element keys to Config composite literals.`,);

private static bool printerconfig(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;

    if (!imports(f, "go/printer")) {
        return false;
    }
    var @fixed = false;
    walk(f, n => {
        ptr<ast.CompositeLit> (cl, ok) = n._<ptr<ast.CompositeLit>>();
        if (!ok) {
            return ;
        }
        ptr<ast.SelectorExpr> (se, ok) = cl.Type._<ptr<ast.SelectorExpr>>();
        if (!ok) {
            return ;
        }
        if (!isTopName(se.X, "printer") || se.Sel == null) {
            return ;
        }
        {
            var ss = se.Sel.String();

            if (ss == "Config") {
                foreach (var (i, e) in cl.Elts) {
                    {
                        ptr<ast.KeyValueExpr> (_, ok) = e._<ptr<ast.KeyValueExpr>>();

                        if (ok) {
                            break;
                        }

                    }

                    switch (i) {
                        case 0: 
                            cl.Elts[i] = addr(new ast.KeyValueExpr(Key:ast.NewIdent("Mode"),Value:e,));
                            break;
                        case 1: 
                            cl.Elts[i] = addr(new ast.KeyValueExpr(Key:ast.NewIdent("Tabwidth"),Value:e,));
                            break;
                    }
                    fixed = true;

                }

            }

        }

    });
    return fixed;

}

} // end main_package
