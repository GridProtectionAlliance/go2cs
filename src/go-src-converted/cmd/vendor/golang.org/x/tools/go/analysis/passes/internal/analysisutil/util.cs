// Package analysisutil defines various helper functions
// used by two or more packages beneath go/analysis.
// package analysisutil -- go2cs converted at 2020 October 09 06:04:37 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/internal/analysisutil" ==> using analysisutil = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\internal\analysisutil\util.go
using bytes = go.bytes_package;
using ast = go.go.ast_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace @internal
{
    public static partial class analysisutil_package
    {
        // Format returns a string representation of the expression.
        public static @string Format(ptr<token.FileSet> _addr_fset, ast.Expr x)
        {
            ref token.FileSet fset = ref _addr_fset.val;

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            printer.Fprint(_addr_b, fset, x);
            return b.String();
        }

        // HasSideEffects reports whether evaluation of e has side effects.
        public static bool HasSideEffects(ptr<types.Info> _addr_info, ast.Expr e)
        {
            ref types.Info info = ref _addr_info.val;

            var safe = true;
            ast.Inspect(e, node =>
            {
                switch (node.type())
                {
                    case ptr<ast.CallExpr> n:
                        var typVal = info.Types[n.Fun];

                        if (typVal.IsType())                         else if (typVal.IsBuiltin()) 
                            // Builtin func, conservatively assumed to not
                            // be safe for now.
                            safe = false;
                            return false;
                        else 
                            // A non-builtin func or method call.
                            // Conservatively assume that all of them have
                            // side effects for now.
                            safe = false;
                            return false;
                                                break;
                    case ptr<ast.UnaryExpr> n:
                        if (n.Op == token.ARROW)
                        {
                            safe = false;
                            return false;
                        }

                        break;
                }
                return true;

            });
            return !safe;

        }

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

        // ReadFile reads a file and adds it to the FileSet
        // so that we can report errors against it using lineStart.
        public static (slice<byte>, ptr<token.File>, error) ReadFile(ptr<token.FileSet> _addr_fset, @string filename)
        {
            slice<byte> _p0 = default;
            ptr<token.File> _p0 = default!;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;

            var (content, err) = ioutil.ReadFile(filename);
            if (err != null)
            {
                return (null, _addr_null!, error.As(err)!);
            }

            var tf = fset.AddFile(filename, -1L, len(content));
            tf.SetLinesForContent(content);
            return (content, _addr_tf!, error.As(null!)!);

        }

        // LineStart returns the position of the start of the specified line
        // within file f, or NoPos if there is no line of that number.
        public static token.Pos LineStart(ptr<token.File> _addr_f, long line)
        {
            ref token.File f = ref _addr_f.val;
 
            // Use binary search to find the start offset of this line.
            //
            // TODO(adonovan): eventually replace this function with the
            // simpler and more efficient (*go/token.File).LineStart, added
            // in go1.12.

            long min = 0L; // inclusive
            var max = f.Size(); // exclusive
            while (true)
            {
                var offset = (min + max) / 2L;
                var pos = f.Pos(offset);
                var posn = f.Position(pos);
                if (posn.Line == line)
                {
                    return pos - (token.Pos(posn.Column) - 1L);
                }

                if (min + 1L >= max)
                {
                    return token.NoPos;
                }

                if (posn.Line < line)
                {
                    min = offset;
                }
                else
                {
                    max = offset;
                }

            }


        }

        // Imports returns true if path is imported by pkg.
        public static bool Imports(ptr<types.Package> _addr_pkg, @string path)
        {
            ref types.Package pkg = ref _addr_pkg.val;

            foreach (var (_, imp) in pkg.Imports())
            {
                if (imp.Path() == path)
                {
                    return true;
                }

            }
            return false;

        }
    }
}}}}}}}}}}
