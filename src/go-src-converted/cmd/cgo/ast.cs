// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse input AST and prepare Prog structure.

// package main -- go2cs converted at 2020 August 29 08:52:02 UTC
// Original source: C:\Go\src\cmd\cgo\ast.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static ref ast.File parse(@string name, slice<byte> src, parser.Mode flags)
        {
            var (ast1, err) = parser.ParseFile(fset, name, src, flags);
            if (err != null)
            {
                {
                    scanner.ErrorList (list, ok) = err._<scanner.ErrorList>();

                    if (ok)
                    { 
                        // If err is a scanner.ErrorList, its String will print just
                        // the first error and then (+n more errors).
                        // Instead, turn it into a new Error that will return
                        // details for all the errors.
                        foreach (var (_, e) in list)
                        {
                            fmt.Fprintln(os.Stderr, e);
                        }                        os.Exit(2L);
                    }
                }
                fatalf("parsing %s: %s", name, err);
            }
            return ast1;
        }

        private static long sourceLine(ast.Node n)
        {
            return fset.Position(n.Pos()).Line;
        }

        // ParseGo populates f with information learned from the Go source code
        // which was read from the named file. It gathers the C preamble
        // attached to the import "C" comment, a list of references to C.xxx,
        // a list of exported functions, and the actual AST, to be rewritten and
        // printed.
        private static void ParseGo(this ref File f, @string name, slice<byte> src)
        { 
            // Create absolute path for file, so that it will be used in error
            // messages and recorded in debug line number information.
            // This matches the rest of the toolchain. See golang.org/issue/5122.
            {
                var (aname, err) = filepath.Abs(name);

                if (err == null)
                {
                    name = aname;
                } 

                // Two different parses: once with comments, once without.
                // The printer is not good enough at printing comments in the
                // right place when we start editing the AST behind its back,
                // so we use ast1 to look for the doc comments on import "C"
                // and on exported functions, and we use ast2 for translating
                // and reprinting.
                // In cgo mode, we ignore ast2 and just apply edits directly
                // the text behind ast1. In godefs mode we modify and print ast2.

            } 

            // Two different parses: once with comments, once without.
            // The printer is not good enough at printing comments in the
            // right place when we start editing the AST behind its back,
            // so we use ast1 to look for the doc comments on import "C"
            // and on exported functions, and we use ast2 for translating
            // and reprinting.
            // In cgo mode, we ignore ast2 and just apply edits directly
            // the text behind ast1. In godefs mode we modify and print ast2.
            var ast1 = parse(name, src, parser.ParseComments);
            var ast2 = parse(name, src, 0L);

            f.Package = ast1.Name.Name;
            f.Name = make_map<@string, ref Name>();
            f.NamePos = make_map<ref Name, token.Pos>(); 

            // In ast1, find the import "C" line and get any extra C preamble.
            var sawC = false;
            {
                var decl__prev1 = decl;

                foreach (var (_, __decl) in ast1.Decls)
                {
                    decl = __decl;
                    ref ast.GenDecl (d, ok) = decl._<ref ast.GenDecl>();
                    if (!ok)
                    {
                        continue;
                    }
                    {
                        var spec__prev2 = spec;

                        foreach (var (_, __spec) in d.Specs)
                        {
                            spec = __spec;
                            ref ast.ImportSpec (s, ok) = spec._<ref ast.ImportSpec>();
                            if (!ok || s.Path.Value != "\"C\"")
                            {
                                continue;
                            }
                            sawC = true;
                            if (s.Name != null)
                            {
                                error_(s.Path.Pos(), "cannot rename import \"C\"");
                            }
                            var cg = s.Doc;
                            if (cg == null && len(d.Specs) == 1L)
                            {
                                cg = d.Doc;
                            }
                            if (cg != null)
                            {
                                f.Preamble += fmt.Sprintf("#line %d %q\n", sourceLine(cg), name);
                                f.Preamble += commentText(cg) + "\n";
                                f.Preamble += "#line 1 \"cgo-generated-wrapper\"\n";
                            }
                        }

                        spec = spec__prev2;
                    }

                }

                decl = decl__prev1;
            }

            if (!sawC)
            {
                error_(token.NoPos, "cannot find import \"C\"");
            } 

            // In ast2, strip the import "C" line.
            if (godefs.Value)
            {
                long w = 0L;
                {
                    var decl__prev1 = decl;

                    foreach (var (_, __decl) in ast2.Decls)
                    {
                        decl = __decl;
                        (d, ok) = decl._<ref ast.GenDecl>();
                        if (!ok)
                        {
                            ast2.Decls[w] = decl;
                            w++;
                            continue;
                        }
                        long ws = 0L;
                        {
                            var spec__prev2 = spec;

                            foreach (var (_, __spec) in d.Specs)
                            {
                                spec = __spec;
                                (s, ok) = spec._<ref ast.ImportSpec>();
                                if (!ok || s.Path.Value != "\"C\"")
                                {
                                    d.Specs[ws] = spec;
                                    ws++;
                                }
                            }
            else

                            spec = spec__prev2;
                        }

                        if (ws == 0L)
                        {
                            continue;
                        }
                        d.Specs = d.Specs[0L..ws];
                        ast2.Decls[w] = d;
                        w++;
                    }

                    decl = decl__prev1;
                }

                ast2.Decls = ast2.Decls[0L..w];
            }            {
                {
                    var decl__prev1 = decl;

                    foreach (var (_, __decl) in ast2.Decls)
                    {
                        decl = __decl;
                        (d, ok) = decl._<ref ast.GenDecl>();
                        if (!ok)
                        {
                            continue;
                        }
                        {
                            var spec__prev2 = spec;

                            foreach (var (_, __spec) in d.Specs)
                            {
                                spec = __spec;
                                {
                                    ref ast.ImportSpec s__prev2 = s;

                                    (s, ok) = spec._<ref ast.ImportSpec>();

                                    if (ok && s.Path.Value == "\"C\"")
                                    { 
                                        // Replace "C" with _ "unsafe", to keep program valid.
                                        // (Deleting import statement or clause is not safe if it is followed
                                        // in the source by an explicit semicolon.)
                                        f.Edit.Replace(f.offset(s.Path.Pos()), f.offset(s.Path.End()), "_ \"unsafe\"");
                                    }

                                    s = s__prev2;

                                }
                            }

                            spec = spec__prev2;
                        }

                    }

                    decl = decl__prev1;
                }

            } 

            // Accumulate pointers to uses of C.x.
            if (f.Ref == null)
            {
                f.Ref = make_slice<ref Ref>(0L, 8L);
            }
            f.walk(ast2, ctxProg, ref File); 

            // Accumulate exported functions.
            // The comments are only on ast1 but we need to
            // save the function bodies from ast2.
            // The first walk fills in ExpFunc, and the
            // second walk changes the entries to
            // refer to ast2 instead.
            f.walk(ast1, ctxProg, ref File);
            f.walk(ast2, ctxProg, ref File);

            f.Comments = ast1.Comments;
            f.AST = ast2;
        }

        // Like ast.CommentGroup's Text method but preserves
        // leading blank lines, so that line numbers line up.
        private static @string commentText(ref ast.CommentGroup g)
        {
            slice<@string> pieces = default;
            foreach (var (_, com) in g.List)
            {
                var c = com.Text; 
                // Remove comment markers.
                // The parser has given us exactly the comment text.
                switch (c[1L])
                {
                    case '/': 
                        //-style comment (no newline at the end)
                        c = c[2L..] + "\n";
                        break;
                    case '*': 
                        /*-style comment */
                        c = c[2L..len(c) - 2L];
                        break;
                }
                pieces = append(pieces, c);
            }
            return strings.Join(pieces, "");
        }

        // Save various references we are going to need later.
        private static void saveExprs(this ref File f, object x, astContext context)
        {
            switch (x.type())
            {
                case ref ast.Expr x:
                    switch ((x.Value).type())
                    {
                        case ref ast.SelectorExpr _:
                            f.saveRef(x, context);
                            break;
                    }
                    break;
                case ref ast.CallExpr x:
                    f.saveCall(x, context);
                    break;
            }
        }

        // Save references to C.xxx for later processing.
        private static void saveRef(this ref File f, ref ast.Expr n, astContext context)
        {
            ref ast.SelectorExpr sel = (n.Value)._<ref ast.SelectorExpr>(); 
            // For now, assume that the only instance of capital C is when
            // used as the imported package identifier.
            // The parser should take care of scoping in the future, so
            // that we will be able to distinguish a "top-level C" from a
            // local C.
            {
                ref ast.Ident (l, ok) = sel.X._<ref ast.Ident>();

                if (!ok || l.Name != "C")
                {
                    return;
                }

            }
            if (context == ctxAssign2)
            {
                context = ctxExpr;
            }
            if (context == ctxEmbedType)
            {
                error_(sel.Pos(), "cannot embed C type");
            }
            var goname = sel.Sel.Name;
            if (goname == "errno")
            {
                error_(sel.Pos(), "cannot refer to errno directly; see documentation");
                return;
            }
            if (goname == "_CMalloc")
            {
                error_(sel.Pos(), "cannot refer to C._CMalloc; use C.malloc");
                return;
            }
            if (goname == "malloc")
            {
                goname = "_CMalloc";
            }
            var name = f.Name[goname];
            if (name == null)
            {
                name = ref new Name(Go:goname,);
                f.Name[goname] = name;
                f.NamePos[name] = sel.Pos();
            }
            f.Ref = append(f.Ref, ref new Ref(Name:name,Expr:n,Context:context,));
        }

        // Save calls to C.xxx for later processing.
        private static void saveCall(this ref File f, ref ast.CallExpr call, astContext context)
        {
            ref ast.SelectorExpr (sel, ok) = call.Fun._<ref ast.SelectorExpr>();
            if (!ok)
            {
                return;
            }
            {
                ref ast.Ident (l, ok) = sel.X._<ref ast.Ident>();

                if (!ok || l.Name != "C")
                {
                    return;
                }

            }
            Call c = ref new Call(Call:call,Deferred:context==ctxDefer);
            f.Calls = append(f.Calls, c);
        }

        // If a function should be exported add it to ExpFunc.
        private static void saveExport(this ref File f, object x, astContext context)
        {
            ref ast.FuncDecl (n, ok) = x._<ref ast.FuncDecl>();
            if (!ok)
            {
                return;
            }
            if (n.Doc == null)
            {
                return;
            }
            foreach (var (_, c) in n.Doc.List)
            {
                if (!strings.HasPrefix(c.Text, "//export "))
                {
                    continue;
                }
                var name = strings.TrimSpace(c.Text[9L..]);
                if (name == "")
                {
                    error_(c.Pos(), "export missing name");
                }
                if (name != n.Name.Name)
                {
                    error_(c.Pos(), "export comment has wrong name %q, want %q", name, n.Name.Name);
                }
                @string doc = "";
                foreach (var (_, c1) in n.Doc.List)
                {
                    if (c1 != c)
                    {
                        doc += c1.Text + "\n";
                    }
                }
                f.ExpFunc = append(f.ExpFunc, ref new ExpFunc(Func:n,ExpName:name,Doc:doc,));
                break;
            }
        }

        // Make f.ExpFunc[i] point at the Func from this AST instead of the other one.
        private static void saveExport2(this ref File f, object x, astContext context)
        {
            ref ast.FuncDecl (n, ok) = x._<ref ast.FuncDecl>();
            if (!ok)
            {
                return;
            }
            foreach (var (_, exp) in f.ExpFunc)
            {
                if (exp.Func.Name.Name == n.Name.Name)
                {
                    exp.Func = n;
                    break;
                }
            }
        }

        private partial struct astContext // : long
        {
        }

        private static readonly astContext ctxProg = iota;
        private static readonly var ctxEmbedType = 0;
        private static readonly var ctxType = 1;
        private static readonly var ctxStmt = 2;
        private static readonly var ctxExpr = 3;
        private static readonly var ctxField = 4;
        private static readonly var ctxParam = 5;
        private static readonly var ctxAssign2 = 6; // assignment of a single expression to two variables
        private static readonly var ctxSwitch = 7;
        private static readonly var ctxTypeSwitch = 8;
        private static readonly var ctxFile = 9;
        private static readonly var ctxDecl = 10;
        private static readonly var ctxSpec = 11;
        private static readonly var ctxDefer = 12;
        private static readonly var ctxCall = 13; // any function call other than ctxCall2
        private static readonly var ctxCall2 = 14; // function call whose result is assigned to two variables
        private static readonly var ctxSelector = 15;

        // walk walks the AST x, calling visit(f, x, context) for each node.
        private static void walk(this ref File _f, object x, astContext context, Action<ref File, object, astContext> visit) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        {
            visit(f, x, context);
            switch (x.type())
            {
                case ref ast.Expr n:
                    f.walk(n.Value, context, visit); 

                    // everything else just recurs
                    break;
                case 
                    break;
                case ref ast.Field n:
                    if (len(n.Names) == 0L && context == ctxField)
                    {
                        f.walk(ref n.Type, ctxEmbedType, visit);
                    }
                    else
                    {
                        f.walk(ref n.Type, ctxType, visit);
                    }
                    break;
                case ref ast.FieldList n:
                    foreach (var (_, field) in n.List)
                    {
                        f.walk(field, context, visit);
                    }
                    break;
                case ref ast.BadExpr n:
                    break;
                case ref ast.Ident n:
                    break;
                case ref ast.Ellipsis n:
                    break;
                case ref ast.BasicLit n:
                    break;
                case ref ast.FuncLit n:
                    f.walk(n.Type, ctxType, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.CompositeLit n:
                    f.walk(ref n.Type, ctxType, visit);
                    f.walk(n.Elts, ctxExpr, visit);
                    break;
                case ref ast.ParenExpr n:
                    f.walk(ref n.X, context, visit);
                    break;
                case ref ast.SelectorExpr n:
                    f.walk(ref n.X, ctxSelector, visit);
                    break;
                case ref ast.IndexExpr n:
                    f.walk(ref n.X, ctxExpr, visit);
                    f.walk(ref n.Index, ctxExpr, visit);
                    break;
                case ref ast.SliceExpr n:
                    f.walk(ref n.X, ctxExpr, visit);
                    if (n.Low != null)
                    {
                        f.walk(ref n.Low, ctxExpr, visit);
                    }
                    if (n.High != null)
                    {
                        f.walk(ref n.High, ctxExpr, visit);
                    }
                    if (n.Max != null)
                    {
                        f.walk(ref n.Max, ctxExpr, visit);
                    }
                    break;
                case ref ast.TypeAssertExpr n:
                    f.walk(ref n.X, ctxExpr, visit);
                    f.walk(ref n.Type, ctxType, visit);
                    break;
                case ref ast.CallExpr n:
                    if (context == ctxAssign2)
                    {
                        f.walk(ref n.Fun, ctxCall2, visit);
                    }
                    else
                    {
                        f.walk(ref n.Fun, ctxCall, visit);
                    }
                    f.walk(n.Args, ctxExpr, visit);
                    break;
                case ref ast.StarExpr n:
                    f.walk(ref n.X, context, visit);
                    break;
                case ref ast.UnaryExpr n:
                    f.walk(ref n.X, ctxExpr, visit);
                    break;
                case ref ast.BinaryExpr n:
                    f.walk(ref n.X, ctxExpr, visit);
                    f.walk(ref n.Y, ctxExpr, visit);
                    break;
                case ref ast.KeyValueExpr n:
                    f.walk(ref n.Key, ctxExpr, visit);
                    f.walk(ref n.Value, ctxExpr, visit);
                    break;
                case ref ast.ArrayType n:
                    f.walk(ref n.Len, ctxExpr, visit);
                    f.walk(ref n.Elt, ctxType, visit);
                    break;
                case ref ast.StructType n:
                    f.walk(n.Fields, ctxField, visit);
                    break;
                case ref ast.FuncType n:
                    f.walk(n.Params, ctxParam, visit);
                    if (n.Results != null)
                    {
                        f.walk(n.Results, ctxParam, visit);
                    }
                    break;
                case ref ast.InterfaceType n:
                    f.walk(n.Methods, ctxField, visit);
                    break;
                case ref ast.MapType n:
                    f.walk(ref n.Key, ctxType, visit);
                    f.walk(ref n.Value, ctxType, visit);
                    break;
                case ref ast.ChanType n:
                    f.walk(ref n.Value, ctxType, visit);
                    break;
                case ref ast.BadStmt n:
                    break;
                case ref ast.DeclStmt n:
                    f.walk(n.Decl, ctxDecl, visit);
                    break;
                case ref ast.EmptyStmt n:
                    break;
                case ref ast.LabeledStmt n:
                    f.walk(n.Stmt, ctxStmt, visit);
                    break;
                case ref ast.ExprStmt n:
                    f.walk(ref n.X, ctxExpr, visit);
                    break;
                case ref ast.SendStmt n:
                    f.walk(ref n.Chan, ctxExpr, visit);
                    f.walk(ref n.Value, ctxExpr, visit);
                    break;
                case ref ast.IncDecStmt n:
                    f.walk(ref n.X, ctxExpr, visit);
                    break;
                case ref ast.AssignStmt n:
                    f.walk(n.Lhs, ctxExpr, visit);
                    if (len(n.Lhs) == 2L && len(n.Rhs) == 1L)
                    {
                        f.walk(n.Rhs, ctxAssign2, visit);
                    }
                    else
                    {
                        f.walk(n.Rhs, ctxExpr, visit);
                    }
                    break;
                case ref ast.GoStmt n:
                    f.walk(n.Call, ctxExpr, visit);
                    break;
                case ref ast.DeferStmt n:
                    f.walk(n.Call, ctxDefer, visit);
                    break;
                case ref ast.ReturnStmt n:
                    f.walk(n.Results, ctxExpr, visit);
                    break;
                case ref ast.BranchStmt n:
                    break;
                case ref ast.BlockStmt n:
                    f.walk(n.List, context, visit);
                    break;
                case ref ast.IfStmt n:
                    f.walk(n.Init, ctxStmt, visit);
                    f.walk(ref n.Cond, ctxExpr, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    f.walk(n.Else, ctxStmt, visit);
                    break;
                case ref ast.CaseClause n:
                    if (context == ctxTypeSwitch)
                    {
                        context = ctxType;
                    }
                    else
                    {
                        context = ctxExpr;
                    }
                    f.walk(n.List, context, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.SwitchStmt n:
                    f.walk(n.Init, ctxStmt, visit);
                    f.walk(ref n.Tag, ctxExpr, visit);
                    f.walk(n.Body, ctxSwitch, visit);
                    break;
                case ref ast.TypeSwitchStmt n:
                    f.walk(n.Init, ctxStmt, visit);
                    f.walk(n.Assign, ctxStmt, visit);
                    f.walk(n.Body, ctxTypeSwitch, visit);
                    break;
                case ref ast.CommClause n:
                    f.walk(n.Comm, ctxStmt, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.SelectStmt n:
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.ForStmt n:
                    f.walk(n.Init, ctxStmt, visit);
                    f.walk(ref n.Cond, ctxExpr, visit);
                    f.walk(n.Post, ctxStmt, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.RangeStmt n:
                    f.walk(ref n.Key, ctxExpr, visit);
                    f.walk(ref n.Value, ctxExpr, visit);
                    f.walk(ref n.X, ctxExpr, visit);
                    f.walk(n.Body, ctxStmt, visit);
                    break;
                case ref ast.ImportSpec n:
                    break;
                case ref ast.ValueSpec n:
                    f.walk(ref n.Type, ctxType, visit);
                    if (len(n.Names) == 2L && len(n.Values) == 1L)
                    {
                        f.walk(ref n.Values[0L], ctxAssign2, visit);
                    }
                    else
                    {
                        f.walk(n.Values, ctxExpr, visit);
                    }
                    break;
                case ref ast.TypeSpec n:
                    f.walk(ref n.Type, ctxType, visit);
                    break;
                case ref ast.BadDecl n:
                    break;
                case ref ast.GenDecl n:
                    f.walk(n.Specs, ctxSpec, visit);
                    break;
                case ref ast.FuncDecl n:
                    if (n.Recv != null)
                    {
                        f.walk(n.Recv, ctxParam, visit);
                    }
                    f.walk(n.Type, ctxType, visit);
                    if (n.Body != null)
                    {
                        f.walk(n.Body, ctxStmt, visit);
                    }
                    break;
                case ref ast.File n:
                    f.walk(n.Decls, ctxDecl, visit);
                    break;
                case ref ast.Package n:
                    foreach (var (_, file) in n.Files)
                    {
                        f.walk(file, ctxFile, visit);
                    }
                    break;
                case slice<ast.Decl> n:
                    foreach (var (_, d) in n)
                    {
                        f.walk(d, context, visit);
                    }
                    break;
                case slice<ast.Expr> n:
                    foreach (var (i) in n)
                    {
                        f.walk(ref n[i], context, visit);
                    }
                    break;
                case slice<ast.Stmt> n:
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in n)
                        {
                            s = __s;
                            f.walk(s, context, visit);
                        }

                        s = s__prev1;
                    }
                    break;
                case slice<ast.Spec> n:
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in n)
                        {
                            s = __s;
                            f.walk(s, context, visit);
                        }

                        s = s__prev1;
                    }
                    break;
                default:
                {
                    var n = x.type();
                    error_(token.NoPos, "unexpected type %T in walk", x);
                    panic("unexpected type");
                    break;
                }
            }
        });
    }
}
