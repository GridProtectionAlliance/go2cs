// package astutil -- go2cs converted at 2022 March 06 23:09:36 UTC
// import "golang.org/x/tools/go/ast/astutil" ==> using astutil = go.golang.org.x.tools.go.ast.astutil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ast\astutil\util.go
using ast = go.go.ast_package;

namespace go.golang.org.x.tools.go.ast;

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
