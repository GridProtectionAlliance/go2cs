// package astutil -- go2cs converted at 2020 October 08 04:27:08 UTC
// import "golang.org/x/tools/go/ast/astutil" ==> using astutil = go.golang.org.x.tools.go.ast.astutil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ast\astutil\util.go
using ast = go.go.ast_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ast
{
    public static partial class astutil_package
    {
        // Unparen returns e with any enclosing parentheses stripped.
        public static ast.Expr Unparen(ast.Expr e)
        {
            while (true)
            {
                ptr<ast.ParenExpr> (p, ok) = e._<ptr<ast.ParenExpr>>();
                if (!ok)
                {
                    return e;
                }
                e = p.X;

            }

        }
    }
}}}}}}
