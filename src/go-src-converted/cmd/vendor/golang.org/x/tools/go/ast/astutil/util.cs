// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package astutil -- go2cs converted at 2022 March 13 06:42:35 UTC
// import "cmd/vendor/golang.org/x/tools/go/ast/astutil" ==> using astutil = go.cmd.vendor.golang.org.x.tools.go.ast.astutil_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\ast\astutil\util.go
namespace go.cmd.vendor.golang.org.x.tools.go.ast;

using ast = go.ast_package;

public static partial class astutil_package {

// Unparen returns e with any enclosing parentheses stripped.
public static ast.Expr Unparen(ast.Expr e) {
    while (true) {
        ptr<ast.ParenExpr> (p, ok) = e._<ptr<ast.ParenExpr>>();
        if (!ok) {
            return e;
        }
        e = p.X;
    }
}

} // end astutil_package
