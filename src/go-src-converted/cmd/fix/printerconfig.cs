// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:00:20 UTC
// Original source: C:\Go\src\cmd\fix\printerconfig.go
using ast = go.go.ast_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(printerconfigFix);
        }

        private static fix printerconfigFix = new fix(name:"printerconfig",date:"2012-12-11",f:printerconfig,desc:`Add element keys to Config composite literals.`,);

        private static bool printerconfig(ref ast.File f)
        {
            if (!imports(f, "go/printer"))
            {
                return false;
            }
            var @fixed = false;
            walk(f, n =>
            {
                ref ast.CompositeLit (cl, ok) = n._<ref ast.CompositeLit>();
                if (!ok)
                {
                    return;
                }
                ref ast.SelectorExpr (se, ok) = cl.Type._<ref ast.SelectorExpr>();
                if (!ok)
                {
                    return;
                }
                if (!isTopName(se.X, "printer") || se.Sel == null)
                {
                    return;
                }
                {
                    var ss = se.Sel.String();

                    if (ss == "Config")
                    {
                        foreach (var (i, e) in cl.Elts)
                        {
                            {
                                ref ast.KeyValueExpr (_, ok) = e._<ref ast.KeyValueExpr>();

                                if (ok)
                                {
                                    break;
                                }

                            }
                            switch (i)
                            {
                                case 0L: 
                                    cl.Elts[i] = ref new ast.KeyValueExpr(Key:ast.NewIdent("Mode"),Value:e,);
                                    break;
                                case 1L: 
                                    cl.Elts[i] = ref new ast.KeyValueExpr(Key:ast.NewIdent("Tabwidth"),Value:e,);
                                    break;
                            }
                            fixed = true;
                        }
                    }

                }
            });
            return fixed;
        }
    }
}
