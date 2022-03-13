// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:29:17 UTC
// Original source: C:\Program Files\Go\src\cmd\fix\netipv6zone.go
namespace go;

using ast = go.ast_package;
using System;

public static partial class main_package {

private static void init() {
    register(netipv6zoneFix);
}

private static fix netipv6zoneFix = new fix(name:"netipv6zone",date:"2012-11-26",f:netipv6zone,desc:`Adapt element key to IPAddr, UDPAddr or TCPAddr composite literals.

https://codereview.appspot.com/6849045/
`,);

private static bool netipv6zone(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;

    if (!imports(f, "net")) {
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
        if (!isTopName(se.X, "net") || se.Sel == null) {
            return ;
        }
        {
            var ss = se.Sel.String();

            switch (ss) {
                case "IPAddr": 

                case "UDPAddr": 

                case "TCPAddr": 
                    foreach (var (i, e) in cl.Elts) {
                        {
                            ptr<ast.KeyValueExpr> (_, ok) = e._<ptr<ast.KeyValueExpr>>();

                            if (ok) {
                                break;
                            }

                        }
                        switch (i) {
                            case 0: 
                                cl.Elts[i] = addr(new ast.KeyValueExpr(Key:ast.NewIdent("IP"),Value:e,));
                                break;
                            case 1: 
                                                       {
                                                           ptr<ast.BasicLit> (elit, ok) = e._<ptr<ast.BasicLit>>();

                                                           if (ok && elit.Value == "0") {
                                                               cl.Elts = append(cl.Elts[..(int)i], cl.Elts[(int)i + 1..]);
                                                           }
                                                           else
                                {
                                                               cl.Elts[i] = addr(new ast.KeyValueExpr(Key:ast.NewIdent("Port"),Value:e,));
                                                           }

                                                       }
                                break;
                        }
                        fixed = true;
                    }
                    break;
            }
        }
    });
    return fixed;
}

} // end main_package
