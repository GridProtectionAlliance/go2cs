// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package deadcode -- go2cs converted at 2022 March 13 06:25:34 UTC
// import "cmd/compile/internal/deadcode" ==> using deadcode = go.cmd.compile.@internal.deadcode_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\deadcode\deadcode.go
namespace go.cmd.compile.@internal;

using constant = go.constant_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;

public static partial class deadcode_package {

public static void Func(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    stmts(_addr_fn.Body);

    if (len(fn.Body) == 0) {
        return ;
    }
    {
        var n__prev1 = n;

        foreach (var (_, __n) in fn.Body) {
            n = __n;
            if (len(n.Init()) > 0) {
                return ;
            }

            if (n.Op() == ir.OIF) 
                ptr<ir.IfStmt> n = n._<ptr<ir.IfStmt>>();
                if (!ir.IsConst(n.Cond, constant.Bool) || len(n.Body) > 0 || len(n.Else) > 0) {
                    return ;
                }
            else if (n.Op() == ir.OFOR) 
                n = n._<ptr<ir.ForStmt>>();
                if (!ir.IsConst(n.Cond, constant.Bool) || ir.BoolVal(n.Cond)) {
                    return ;
                }
            else 
                return ;
                    }
        n = n__prev1;
    }

    fn.Body = new slice<ir.Node>(new ir.Node[] { ir.NewBlockStmt(base.Pos,nil) });
}

private static void stmts(ptr<ir.Nodes> _addr_nn) {
    ref ir.Nodes nn = ref _addr_nn.val;

    nint lastLabel = -1;
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in nn) {
            i = __i;
            n = __n;
            if (n != null && n.Op() == ir.OLABEL) {
                lastLabel = i;
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in nn) {
            i = __i;
            n = __n; 
            // Cut is set to true when all nodes after i'th position
            // should be removed.
            // In other words, it marks whole slice "tail" as dead.
            var cut = false;
            if (n == null) {
                continue;
            }
            if (n.Op() == ir.OIF) {
                ptr<ir.IfStmt> n = n._<ptr<ir.IfStmt>>();
                n.Cond = expr(n.Cond);
                if (ir.IsConst(n.Cond, constant.Bool)) {
                    ir.Nodes body = default;
                    if (ir.BoolVal(n.Cond)) {
                        n.Else = new ir.Nodes();
                        body = n.Body;
                    }
                    else
 {
                        n.Body = new ir.Nodes();
                        body = n.Else;
                    } 
                    // If "then" or "else" branch ends with panic or return statement,
                    // it is safe to remove all statements after this node.
                    // isterminating is not used to avoid goto-related complications.
                    // We must be careful not to deadcode-remove labels, as they
                    // might be the target of a goto. See issue 28616.
                    {
                        ir.Nodes body__prev3 = body;

                        body = body;

                        if (len(body) != 0) {

                            if (body[(len(body) - 1)].Op() == ir.ORETURN || body[(len(body) - 1)].Op() == ir.OTAILCALL || body[(len(body) - 1)].Op() == ir.OPANIC) 
                                if (i > lastLabel) {
                                    cut = true;
                                }
                                                    }

                        body = body__prev3;

                    }
                }
            }
            if (len(n.Init()) != 0) {
                stmts(_addr_n._<ir.InitNode>().PtrInit());
            }

            if (n.Op() == ir.OBLOCK) 
                n = n._<ptr<ir.BlockStmt>>();
                stmts(_addr_n.List);
            else if (n.Op() == ir.OFOR) 
                n = n._<ptr<ir.ForStmt>>();
                stmts(_addr_n.Body);
            else if (n.Op() == ir.OIF) 
                n = n._<ptr<ir.IfStmt>>();
                stmts(_addr_n.Body);
                stmts(_addr_n.Else);
            else if (n.Op() == ir.ORANGE) 
                n = n._<ptr<ir.RangeStmt>>();
                stmts(_addr_n.Body);
            else if (n.Op() == ir.OSELECT) 
                n = n._<ptr<ir.SelectStmt>>();
                {
                    var cas__prev2 = cas;

                    foreach (var (_, __cas) in n.Cases) {
                        cas = __cas;
                        stmts(_addr_cas.Body);
                    }

                    cas = cas__prev2;
                }
            else if (n.Op() == ir.OSWITCH) 
                n = n._<ptr<ir.SwitchStmt>>();
                {
                    var cas__prev2 = cas;

                    foreach (var (_, __cas) in n.Cases) {
                        cas = __cas;
                        stmts(_addr_cas.Body);
                    }

                    cas = cas__prev2;
                }
                        if (cut) {
                nn = (nn)[..(int)i + 1];
                break;
            }
        }
        i = i__prev1;
        n = n__prev1;
    }
}

private static ir.Node expr(ir.Node n) { 
    // Perform dead-code elimination on short-circuited boolean
    // expressions involving constants with the intent of
    // producing a constant 'if' condition.

    if (n.Op() == ir.OANDAND) 
        ptr<ir.LogicalExpr> n = n._<ptr<ir.LogicalExpr>>();
        n.X = expr(n.X);
        n.Y = expr(n.Y);
        if (ir.IsConst(n.X, constant.Bool)) {
            if (ir.BoolVal(n.X)) {
                return n.Y; // true && x => x
            }
            else
 {
                return n.X; // false && x => false
            }
        }
    else if (n.Op() == ir.OOROR) 
        n = n._<ptr<ir.LogicalExpr>>();
        n.X = expr(n.X);
        n.Y = expr(n.Y);
        if (ir.IsConst(n.X, constant.Bool)) {
            if (ir.BoolVal(n.X)) {
                return n.X; // true || x => true
            }
            else
 {
                return n.Y; // false || x => x
            }
        }
        return n;
}

} // end deadcode_package
