// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:00:20 UTC
// Original source: C:\Go\src\cmd\fix\netipv6zone.go
using ast = go.go.ast_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(netipv6zoneFix);
        }

        private static fix netipv6zoneFix = new fix(name:"netipv6zone",date:"2012-11-26",f:netipv6zone,desc:`Adapt element key to IPAddr, UDPAddr or TCPAddr composite literals.

https://codereview.appspot.com/6849045/
`,);

        private static bool netipv6zone(ref ast.File f)
        {
            if (!imports(f, "net"))
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
                if (!isTopName(se.X, "net") || se.Sel == null)
                {
                    return;
                }
                {
                    var ss = se.Sel.String();

                    switch (ss)
                    {
                        case "IPAddr": 

                        case "UDPAddr": 

                        case "TCPAddr": 
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
                                        cl.Elts[i] = ref new ast.KeyValueExpr(Key:ast.NewIdent("IP"),Value:e,);
                                        break;
                                    case 1L: 
                                        {
                                            ref ast.BasicLit (elit, ok) = e._<ref ast.BasicLit>();

                                            if (ok && elit.Value == "0")
                                            {
                                                cl.Elts = append(cl.Elts[..i], cl.Elts[i + 1L..]);
                                            }
                                            else
                                            {
                                                cl.Elts[i] = ref new ast.KeyValueExpr(Key:ast.NewIdent("Port"),Value:e,);
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
    }
}
