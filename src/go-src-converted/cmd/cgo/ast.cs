// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse input AST and prepare Prog structure.

// package main -- go2cs converted at 2022 March 06 22:46:44 UTC
// Original source: C:\Program Files\Go\src\cmd\cgo\ast.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using os = go.os_package;
using strings = go.strings_package;
using System;


namespace go;

public static partial class main_package {

private static ptr<ast.File> parse(@string name, slice<byte> src, parser.Mode flags) {
    var (ast1, err) = parser.ParseFile(fset, name, src, flags);
    if (err != null) {
        {
            scanner.ErrorList (list, ok) = err._<scanner.ErrorList>();

            if (ok) { 
                // If err is a scanner.ErrorList, its String will print just
                // the first error and then (+n more errors).
                // Instead, turn it into a new Error that will return
                // details for all the errors.
                foreach (var (_, e) in list) {
                    fmt.Fprintln(os.Stderr, e);
                }                os.Exit(2);

            }
        }

        fatalf("parsing %s: %s", name, err);

    }
    return _addr_ast1!;

}

private static nint sourceLine(ast.Node n) {
    return fset.Position(n.Pos()).Line;
}

// ParseGo populates f with information learned from the Go source code
// which was read from the named file. It gathers the C preamble
// attached to the import "C" comment, a list of references to C.xxx,
// a list of exported functions, and the actual AST, to be rewritten and
// printed.
private static void ParseGo(this ptr<File> _addr_f, @string abspath, slice<byte> src) {
    ref File f = ref _addr_f.val;
 
    // Two different parses: once with comments, once without.
    // The printer is not good enough at printing comments in the
    // right place when we start editing the AST behind its back,
    // so we use ast1 to look for the doc comments on import "C"
    // and on exported functions, and we use ast2 for translating
    // and reprinting.
    // In cgo mode, we ignore ast2 and just apply edits directly
    // the text behind ast1. In godefs mode we modify and print ast2.
    var ast1 = parse(abspath, src, parser.ParseComments);
    var ast2 = parse(abspath, src, 0);

    f.Package = ast1.Name.Name;
    f.Name = make_map<@string, ptr<Name>>();
    f.NamePos = make_map<ptr<Name>, token.Pos>(); 

    // In ast1, find the import "C" line and get any extra C preamble.
    var sawC = false;
    {
        var decl__prev1 = decl;

        foreach (var (_, __decl) in ast1.Decls) {
            decl = __decl;
            ptr<ast.GenDecl> (d, ok) = decl._<ptr<ast.GenDecl>>();
            if (!ok) {
                continue;
            }
            {
                var spec__prev2 = spec;

                foreach (var (_, __spec) in d.Specs) {
                    spec = __spec;
                    ptr<ast.ImportSpec> (s, ok) = spec._<ptr<ast.ImportSpec>>();
                    if (!ok || s.Path.Value != "\"C\"") {
                        continue;
                    }
                    sawC = true;
                    if (s.Name != null) {
                        error_(s.Path.Pos(), "cannot rename import \"C\"");
                    }
                    var cg = s.Doc;
                    if (cg == null && len(d.Specs) == 1) {
                        cg = d.Doc;
                    }
                    if (cg != null) {
                        f.Preamble += fmt.Sprintf("#line %d %q\n", sourceLine(cg), abspath);
                        f.Preamble += commentText(_addr_cg) + "\n";
                        f.Preamble += "#line 1 \"cgo-generated-wrapper\"\n";
                    }
                }

                spec = spec__prev2;
            }
        }
        decl = decl__prev1;
    }

    if (!sawC) {
        error_(ast1.Package, "cannot find import \"C\"");
    }
    if (godefs.val) {
        nint w = 0;
        {
            var decl__prev1 = decl;

            foreach (var (_, __decl) in ast2.Decls) {
                decl = __decl;
                (d, ok) = decl._<ptr<ast.GenDecl>>();
                if (!ok) {
                    ast2.Decls[w] = decl;
                    w++;
                    continue;
                }
                nint ws = 0;
                {
                    var spec__prev2 = spec;

                    foreach (var (_, __spec) in d.Specs) {
                        spec = __spec;
                        (s, ok) = spec._<ptr<ast.ImportSpec>>();
                        if (!ok || s.Path.Value != "\"C\"") {
                            d.Specs[ws] = spec;
                            ws++;
                        }
                    }
    else

                    spec = spec__prev2;
                }

                if (ws == 0) {
                    continue;
                }

                d.Specs = d.Specs[(int)0..(int)ws];
                ast2.Decls[w] = d;
                w++;

            }

            decl = decl__prev1;
        }

        ast2.Decls = ast2.Decls[(int)0..(int)w];

    } {
        {
            var decl__prev1 = decl;

            foreach (var (_, __decl) in ast2.Decls) {
                decl = __decl;
                (d, ok) = decl._<ptr<ast.GenDecl>>();
                if (!ok) {
                    continue;
                }
                {
                    var spec__prev2 = spec;

                    foreach (var (_, __spec) in d.Specs) {
                        spec = __spec;
                        {
                            ptr<ast.ImportSpec> s__prev2 = s;

                            (s, ok) = spec._<ptr<ast.ImportSpec>>();

                            if (ok && s.Path.Value == "\"C\"") { 
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
    if (f.Ref == null) {
        f.Ref = make_slice<ptr<Ref>>(0, 8);
    }
    f.walk(ast2, ctxProg, (File.val).validateIdents);
    f.walk(ast2, ctxProg, (File.val).saveExprs); 

    // Accumulate exported functions.
    // The comments are only on ast1 but we need to
    // save the function bodies from ast2.
    // The first walk fills in ExpFunc, and the
    // second walk changes the entries to
    // refer to ast2 instead.
    f.walk(ast1, ctxProg, (File.val).saveExport);
    f.walk(ast2, ctxProg, (File.val).saveExport2);

    f.Comments = ast1.Comments;
    f.AST = ast2;

}

// Like ast.CommentGroup's Text method but preserves
// leading blank lines, so that line numbers line up.
private static @string commentText(ptr<ast.CommentGroup> _addr_g) {
    ref ast.CommentGroup g = ref _addr_g.val;

    slice<@string> pieces = default;
    foreach (var (_, com) in g.List) {
        var c = com.Text; 
        // Remove comment markers.
        // The parser has given us exactly the comment text.
        switch (c[1]) {
            case '/': 
                //-style comment (no newline at the end)
                c = c[(int)2..] + "\n";
                break;
            case '*': 
                /*-style comment */
                c = c[(int)2..(int)len(c) - 2];
                break;
        }
        pieces = append(pieces, c);

    }    return strings.Join(pieces, "");

}

private static void validateIdents(this ptr<File> _addr_f, object x, astContext context) {
    ref File f = ref _addr_f.val;

    {
        ptr<ast.Ident> (x, ok) = x._<ptr<ast.Ident>>();

        if (ok) {
            if (f.isMangledName(x.Name)) {
                error_(x.Pos(), "identifier %q may conflict with identifiers generated by cgo", x.Name);
            }
        }
    }

}

// Save various references we are going to need later.
private static void saveExprs(this ptr<File> _addr_f, object x, astContext context) {
    ref File f = ref _addr_f.val;

    switch (x.type()) {
        case ptr<ast.Expr> x:
            switch ((x.val).type()) {
                case ptr<ast.SelectorExpr> _:
                    f.saveRef(x, context);
                    break;
            }
            break;
        case ptr<ast.CallExpr> x:
            f.saveCall(x, context);
            break;
    }

}

// Save references to C.xxx for later processing.
private static void saveRef(this ptr<File> _addr_f, ptr<ast.Expr> _addr_n, astContext context) {
    ref File f = ref _addr_f.val;
    ref ast.Expr n = ref _addr_n.val;

    ptr<ast.SelectorExpr> sel = (n)._<ptr<ast.SelectorExpr>>(); 
    // For now, assume that the only instance of capital C is when
    // used as the imported package identifier.
    // The parser should take care of scoping in the future, so
    // that we will be able to distinguish a "top-level C" from a
    // local C.
    {
        ptr<ast.Ident> (l, ok) = sel.X._<ptr<ast.Ident>>();

        if (!ok || l.Name != "C") {
            return ;
        }
    }

    if (context == ctxAssign2) {
        context = ctxExpr;
    }
    if (context == ctxEmbedType) {
        error_(sel.Pos(), "cannot embed C type");
    }
    var goname = sel.Sel.Name;
    if (goname == "errno") {
        error_(sel.Pos(), "cannot refer to errno directly; see documentation");
        return ;
    }
    if (goname == "_CMalloc") {
        error_(sel.Pos(), "cannot refer to C._CMalloc; use C.malloc");
        return ;
    }
    if (goname == "malloc") {
        goname = "_CMalloc";
    }
    var name = f.Name[goname];
    if (name == null) {
        name = addr(new Name(Go:goname,));
        f.Name[goname] = name;
        f.NamePos[name] = sel.Pos();
    }
    f.Ref = append(f.Ref, addr(new Ref(Name:name,Expr:n,Context:context,)));

}

// Save calls to C.xxx for later processing.
private static void saveCall(this ptr<File> _addr_f, ptr<ast.CallExpr> _addr_call, astContext context) {
    ref File f = ref _addr_f.val;
    ref ast.CallExpr call = ref _addr_call.val;

    ptr<ast.SelectorExpr> (sel, ok) = call.Fun._<ptr<ast.SelectorExpr>>();
    if (!ok) {
        return ;
    }
    {
        ptr<ast.Ident> (l, ok) = sel.X._<ptr<ast.Ident>>();

        if (!ok || l.Name != "C") {
            return ;
        }
    }

    ptr<Call> c = addr(new Call(Call:call,Deferred:context==ctxDefer));
    f.Calls = append(f.Calls, c);

}

// If a function should be exported add it to ExpFunc.
private static void saveExport(this ptr<File> _addr_f, object x, astContext context) {
    ref File f = ref _addr_f.val;

    ptr<ast.FuncDecl> (n, ok) = x._<ptr<ast.FuncDecl>>();
    if (!ok) {
        return ;
    }
    if (n.Doc == null) {
        return ;
    }
    foreach (var (_, c) in n.Doc.List) {
        if (!strings.HasPrefix(c.Text, "//export ")) {
            continue;
        }
        var name = strings.TrimSpace(c.Text[(int)9..]);
        if (name == "") {
            error_(c.Pos(), "export missing name");
        }
        if (name != n.Name.Name) {
            error_(c.Pos(), "export comment has wrong name %q, want %q", name, n.Name.Name);
        }
        @string doc = "";
        foreach (var (_, c1) in n.Doc.List) {
            if (c1 != c) {
                doc += c1.Text + "\n";
            }
        }        f.ExpFunc = append(f.ExpFunc, addr(new ExpFunc(Func:n,ExpName:name,Doc:doc,)));
        break;

    }
}

// Make f.ExpFunc[i] point at the Func from this AST instead of the other one.
private static void saveExport2(this ptr<File> _addr_f, object x, astContext context) {
    ref File f = ref _addr_f.val;

    ptr<ast.FuncDecl> (n, ok) = x._<ptr<ast.FuncDecl>>();
    if (!ok) {
        return ;
    }
    foreach (var (_, exp) in f.ExpFunc) {
        if (exp.Func.Name.Name == n.Name.Name) {
            exp.Func = n;
            break;
        }
    }
}

private partial struct astContext { // : nint
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
private static void walk(this ptr<File> _addr_f, object x, astContext context, Action<ptr<File>, object, astContext> visit) => func((_, panic, _) => {
    ref File f = ref _addr_f.val;

    visit(f, x, context);
    switch (x.type()) {
        case ptr<ast.Expr> n:
            f.walk(n.val, context, visit); 

            // everything else just recurs
            break;
        case 
            break;
        case ptr<ast.Field> n:
            if (len(n.Names) == 0 && context == ctxField) {
                f.walk(_addr_n.Type, ctxEmbedType, visit);
            }
            else
 {
                f.walk(_addr_n.Type, ctxType, visit);
            }

            break;
        case ptr<ast.FieldList> n:
            foreach (var (_, field) in n.List) {
                f.walk(field, context, visit);
            }
            break;
        case ptr<ast.BadExpr> n:
            break;
        case ptr<ast.Ident> n:
            break;
        case ptr<ast.Ellipsis> n:
            f.walk(_addr_n.Elt, ctxType, visit);
            break;
        case ptr<ast.BasicLit> n:
            break;
        case ptr<ast.FuncLit> n:
            f.walk(n.Type, ctxType, visit);
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.CompositeLit> n:
            f.walk(_addr_n.Type, ctxType, visit);
            f.walk(n.Elts, ctxExpr, visit);
            break;
        case ptr<ast.ParenExpr> n:
            f.walk(_addr_n.X, context, visit);
            break;
        case ptr<ast.SelectorExpr> n:
            f.walk(_addr_n.X, ctxSelector, visit);
            break;
        case ptr<ast.IndexExpr> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            f.walk(_addr_n.Index, ctxExpr, visit);
            break;
        case ptr<ast.SliceExpr> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            if (n.Low != null) {
                f.walk(_addr_n.Low, ctxExpr, visit);
            }
            if (n.High != null) {
                f.walk(_addr_n.High, ctxExpr, visit);
            }
            if (n.Max != null) {
                f.walk(_addr_n.Max, ctxExpr, visit);
            }
            break;
        case ptr<ast.TypeAssertExpr> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            f.walk(_addr_n.Type, ctxType, visit);
            break;
        case ptr<ast.CallExpr> n:
            if (context == ctxAssign2) {
                f.walk(_addr_n.Fun, ctxCall2, visit);
            }
            else
 {
                f.walk(_addr_n.Fun, ctxCall, visit);
            }

            f.walk(n.Args, ctxExpr, visit);
            break;
        case ptr<ast.StarExpr> n:
            f.walk(_addr_n.X, context, visit);
            break;
        case ptr<ast.UnaryExpr> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            break;
        case ptr<ast.BinaryExpr> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            f.walk(_addr_n.Y, ctxExpr, visit);
            break;
        case ptr<ast.KeyValueExpr> n:
            f.walk(_addr_n.Key, ctxExpr, visit);
            f.walk(_addr_n.Value, ctxExpr, visit);
            break;
        case ptr<ast.ArrayType> n:
            f.walk(_addr_n.Len, ctxExpr, visit);
            f.walk(_addr_n.Elt, ctxType, visit);
            break;
        case ptr<ast.StructType> n:
            f.walk(n.Fields, ctxField, visit);
            break;
        case ptr<ast.FuncType> n:
            f.walk(n.Params, ctxParam, visit);
            if (n.Results != null) {
                f.walk(n.Results, ctxParam, visit);
            }
            break;
        case ptr<ast.InterfaceType> n:
            f.walk(n.Methods, ctxField, visit);
            break;
        case ptr<ast.MapType> n:
            f.walk(_addr_n.Key, ctxType, visit);
            f.walk(_addr_n.Value, ctxType, visit);
            break;
        case ptr<ast.ChanType> n:
            f.walk(_addr_n.Value, ctxType, visit);
            break;
        case ptr<ast.BadStmt> n:
            break;
        case ptr<ast.DeclStmt> n:
            f.walk(n.Decl, ctxDecl, visit);
            break;
        case ptr<ast.EmptyStmt> n:
            break;
        case ptr<ast.LabeledStmt> n:
            f.walk(n.Stmt, ctxStmt, visit);
            break;
        case ptr<ast.ExprStmt> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            break;
        case ptr<ast.SendStmt> n:
            f.walk(_addr_n.Chan, ctxExpr, visit);
            f.walk(_addr_n.Value, ctxExpr, visit);
            break;
        case ptr<ast.IncDecStmt> n:
            f.walk(_addr_n.X, ctxExpr, visit);
            break;
        case ptr<ast.AssignStmt> n:
            f.walk(n.Lhs, ctxExpr, visit);
            if (len(n.Lhs) == 2 && len(n.Rhs) == 1) {
                f.walk(n.Rhs, ctxAssign2, visit);
            }
            else
 {
                f.walk(n.Rhs, ctxExpr, visit);
            }

            break;
        case ptr<ast.GoStmt> n:
            f.walk(n.Call, ctxExpr, visit);
            break;
        case ptr<ast.DeferStmt> n:
            f.walk(n.Call, ctxDefer, visit);
            break;
        case ptr<ast.ReturnStmt> n:
            f.walk(n.Results, ctxExpr, visit);
            break;
        case ptr<ast.BranchStmt> n:
            break;
        case ptr<ast.BlockStmt> n:
            f.walk(n.List, context, visit);
            break;
        case ptr<ast.IfStmt> n:
            f.walk(n.Init, ctxStmt, visit);
            f.walk(_addr_n.Cond, ctxExpr, visit);
            f.walk(n.Body, ctxStmt, visit);
            f.walk(n.Else, ctxStmt, visit);
            break;
        case ptr<ast.CaseClause> n:
            if (context == ctxTypeSwitch) {
                context = ctxType;
            }
            else
 {
                context = ctxExpr;
            }

            f.walk(n.List, context, visit);
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.SwitchStmt> n:
            f.walk(n.Init, ctxStmt, visit);
            f.walk(_addr_n.Tag, ctxExpr, visit);
            f.walk(n.Body, ctxSwitch, visit);
            break;
        case ptr<ast.TypeSwitchStmt> n:
            f.walk(n.Init, ctxStmt, visit);
            f.walk(n.Assign, ctxStmt, visit);
            f.walk(n.Body, ctxTypeSwitch, visit);
            break;
        case ptr<ast.CommClause> n:
            f.walk(n.Comm, ctxStmt, visit);
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.SelectStmt> n:
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.ForStmt> n:
            f.walk(n.Init, ctxStmt, visit);
            f.walk(_addr_n.Cond, ctxExpr, visit);
            f.walk(n.Post, ctxStmt, visit);
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.RangeStmt> n:
            f.walk(_addr_n.Key, ctxExpr, visit);
            f.walk(_addr_n.Value, ctxExpr, visit);
            f.walk(_addr_n.X, ctxExpr, visit);
            f.walk(n.Body, ctxStmt, visit);
            break;
        case ptr<ast.ImportSpec> n:
            break;
        case ptr<ast.ValueSpec> n:
            f.walk(_addr_n.Type, ctxType, visit);
            if (len(n.Names) == 2 && len(n.Values) == 1) {
                f.walk(_addr_n.Values[0], ctxAssign2, visit);
            }
            else
 {
                f.walk(n.Values, ctxExpr, visit);
            }

            break;
        case ptr<ast.TypeSpec> n:
            f.walk(_addr_n.Type, ctxType, visit);
            break;
        case ptr<ast.BadDecl> n:
            break;
        case ptr<ast.GenDecl> n:
            f.walk(n.Specs, ctxSpec, visit);
            break;
        case ptr<ast.FuncDecl> n:
            if (n.Recv != null) {
                f.walk(n.Recv, ctxParam, visit);
            }
            f.walk(n.Type, ctxType, visit);
            if (n.Body != null) {
                f.walk(n.Body, ctxStmt, visit);
            }
            break;
        case ptr<ast.File> n:
            f.walk(n.Decls, ctxDecl, visit);
            break;
        case ptr<ast.Package> n:
            foreach (var (_, file) in n.Files) {
                f.walk(file, ctxFile, visit);
            }
            break;
        case slice<ast.Decl> n:
            foreach (var (_, d) in n) {
                f.walk(d, context, visit);
            }
            break;
        case slice<ast.Expr> n:
            foreach (var (i) in n) {
                f.walk(_addr_n[i], context, visit);
            }
            break;
        case slice<ast.Stmt> n:
            {
                var s__prev1 = s;

                foreach (var (_, __s) in n) {
                    s = __s;
                    f.walk(s, context, visit);
                }

                s = s__prev1;
            }
            break;
        case slice<ast.Spec> n:
            {
                var s__prev1 = s;

                foreach (var (_, __s) in n) {
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

} // end main_package
