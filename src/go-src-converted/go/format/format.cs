// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package format implements standard formatting of Go source.
// package format -- go2cs converted at 2020 August 29 09:24:38 UTC
// import "go/format" ==> using format = go.go.format_package
// Original source: C:\Go\src\go\format\format.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class format_package
    {
        private static printer.Config config = new printer.Config(Mode:printer.UseSpaces|printer.TabIndent,Tabwidth:8);

        private static readonly var parserMode = parser.ParseComments;

        // Node formats node in canonical gofmt style and writes the result to dst.
        //
        // The node type must be *ast.File, *printer.CommentedNode, []ast.Decl,
        // []ast.Stmt, or assignment-compatible to ast.Expr, ast.Decl, ast.Spec,
        // or ast.Stmt. Node does not modify node. Imports are not sorted for
        // nodes representing partial source files (for instance, if the node is
        // not an *ast.File or a *printer.CommentedNode not wrapping an *ast.File).
        //
        // The function may return early (before the entire result is written)
        // and return a formatting error, for instance due to an incorrect AST.
        //


        // Node formats node in canonical gofmt style and writes the result to dst.
        //
        // The node type must be *ast.File, *printer.CommentedNode, []ast.Decl,
        // []ast.Stmt, or assignment-compatible to ast.Expr, ast.Decl, ast.Spec,
        // or ast.Stmt. Node does not modify node. Imports are not sorted for
        // nodes representing partial source files (for instance, if the node is
        // not an *ast.File or a *printer.CommentedNode not wrapping an *ast.File).
        //
        // The function may return early (before the entire result is written)
        // and return a formatting error, for instance due to an incorrect AST.
        //
        public static error Node(io.Writer dst, ref token.FileSet fset, object node)
        { 
            // Determine if we have a complete source file (file != nil).
            ref ast.File file = default;
            ref printer.CommentedNode cnode = default;
            switch (node.type())
            {
                case ref ast.File n:
                    file = n;
                    break;
                case ref printer.CommentedNode n:
                    {
                        ref ast.File (f, ok) = n.Node._<ref ast.File>();

                        if (ok)
                        {
                            file = f;
                            cnode = n;
                        }

                    }
                    break; 

                // Sort imports if necessary.
            } 

            // Sort imports if necessary.
            if (file != null && hasUnsortedImports(file))
            { 
                // Make a copy of the AST because ast.SortImports is destructive.
                // TODO(gri) Do this more efficiently.
                bytes.Buffer buf = default;
                var err = config.Fprint(ref buf, fset, file);
                if (err != null)
                {
                    return error.As(err);
                }
                file, err = parser.ParseFile(fset, "", buf.Bytes(), parserMode);
                if (err != null)
                { 
                    // We should never get here. If we do, provide good diagnostic.
                    return error.As(fmt.Errorf("format.Node internal error (%s)", err));
                }
                ast.SortImports(fset, file); 

                // Use new file with sorted imports.
                node = file;
                if (cnode != null)
                {
                    node = ref new printer.CommentedNode(Node:file,Comments:cnode.Comments);
                }
            }
            return error.As(config.Fprint(dst, fset, node));
        }

        // Source formats src in canonical gofmt style and returns the result
        // or an (I/O or syntax) error. src is expected to be a syntactically
        // correct Go source file, or a list of Go declarations or statements.
        //
        // If src is a partial source file, the leading and trailing space of src
        // is applied to the result (such that it has the same leading and trailing
        // space as src), and the result is indented by the same amount as the first
        // line of src containing code. Imports are not sorted for partial source files.
        //
        // Caution: Tools relying on consistent formatting based on the installed
        // version of gofmt (for instance, such as for presubmit checks) should
        // execute that gofmt binary instead of calling Source.
        //
        public static (slice<byte>, error) Source(slice<byte> src)
        {
            var fset = token.NewFileSet();
            var (file, sourceAdj, indentAdj, err) = parse(fset, "", src, true);
            if (err != null)
            {
                return (null, err);
            }
            if (sourceAdj == null)
            { 
                // Complete source file.
                // TODO(gri) consider doing this always.
                ast.SortImports(fset, file);
            }
            return format(fset, file, sourceAdj, indentAdj, src, config);
        }

        private static bool hasUnsortedImports(ref ast.File file)
        {
            {
                var d__prev1 = d;

                foreach (var (_, __d) in file.Decls)
                {
                    d = __d;
                    ref ast.GenDecl (d, ok) = d._<ref ast.GenDecl>();
                    if (!ok || d.Tok != token.IMPORT)
                    { 
                        // Not an import declaration, so we're done.
                        // Imports are always first.
                        return false;
                    }
                    if (d.Lparen.IsValid())
                    { 
                        // For now assume all grouped imports are unsorted.
                        // TODO(gri) Should check if they are sorted already.
                        return true;
                    } 
                    // Ungrouped imports are sorted by default.
                }

                d = d__prev1;
            }

            return false;
        }
    }
}}
