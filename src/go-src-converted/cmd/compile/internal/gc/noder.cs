// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:39 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\noder.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

using syntax = go.cmd.compile.@internal.syntax_package;
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ulong parseFiles(slice<@string> filenames) => func((defer, _, __) =>
        {
            slice<ref noder> noders = default; 
            // Limit the number of simultaneously open files.
            var sem = make_channel<object>(runtime.GOMAXPROCS(0L) + 10L);

            foreach (var (_, filename) in filenames)
            {
                noder p = ref new noder(err:make(chansyntax.Error));
                noders = append(noders, p);

                go_(() => filename =>
                {
                    sem.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                    defer(() =>
                    {
                        sem.Receive();

                    }());
                    defer(close(p.err));
                    var @base = src.NewFileBase(filename, absFilename(filename));

                    var (f, err) = os.Open(filename);
                    if (err != null)
                    {
                        p.error(new syntax.Error(Pos:src.MakePos(base,0,0),Msg:err.Error()));
                        return;
                    }
                    defer(f.Close());

                    p.file, _ = syntax.Parse(base, f, p.error, p.pragma, fileh, syntax.CheckBranches); // errors are tracked via p.error
                }(filename));
            }            ulong lines = default;
            {
                noder p__prev1 = p;

                foreach (var (_, __p) in noders)
                {
                    p = __p;
                    foreach (var (e) in p.err)
                    {
                        yyerrorpos(e.Pos, "%s", e.Msg);
                    }                    p.node();
                    lines += p.file.Lines;
                    p.file = null; // release memory

                    if (nsyntaxerrors != 0L)
                    {
                        errorexit();
                    }
                    testdclstack();
                }
                p = p__prev1;
            }

            return lines;
        });

        private static void yyerrorpos(src.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();

            yyerrorl(Ctxt.PosTable.XPos(pos), format, args);
        }

        private static @string pathPrefix = default;

        private static @string fileh(@string name)
        {
            return objabi.AbsFile("", name, pathPrefix);
        }

        private static @string absFilename(@string name)
        {
            return objabi.AbsFile(Ctxt.Pathname, name, pathPrefix);
        }

        // noder transforms package syntax's AST into a Node tree.
        private partial struct noder
        {
            public ptr<syntax.File> file;
            public slice<linkname> linknames;
            public @string pragcgobuf;
            public channel<syntax.Error> err;
            public ScopeID scope;
        }

        private static ScopeID funchdr(this ref noder p, ref Node n)
        {
            var old = p.scope;
            p.scope = 0L;
            funchdr(n);
            return old;
        }

        private static void funcbody(this ref noder p, ScopeID old)
        {
            funcbody();
            p.scope = old;
        }

        private static void openScope(this ref noder p, src.Pos pos)
        {
            types.Markdcl();

            if (trackScopes)
            {
                Curfn.Func.Parents = append(Curfn.Func.Parents, p.scope);
                p.scope = ScopeID(len(Curfn.Func.Parents));

                p.markScope(pos);
            }
        }

        private static void closeScope(this ref noder p, src.Pos pos)
        {
            types.Popdcl();

            if (trackScopes)
            {
                p.scope = Curfn.Func.Parents[p.scope - 1L];

                p.markScope(pos);
            }
        }

        private static void markScope(this ref noder p, src.Pos pos)
        {
            var xpos = Ctxt.PosTable.XPos(pos);
            {
                var i = len(Curfn.Func.Marks);

                if (i > 0L && Curfn.Func.Marks[i - 1L].Pos == xpos)
                {
                    Curfn.Func.Marks[i - 1L].Scope = p.scope;
                }
                else
                {
                    Curfn.Func.Marks = append(Curfn.Func.Marks, new Mark(xpos,p.scope));
                }

            }
        }

        // closeAnotherScope is like closeScope, but it reuses the same mark
        // position as the last closeScope call. This is useful for "for" and
        // "if" statements, as their implicit blocks always end at the same
        // position as an explicit block.
        private static void closeAnotherScope(this ref noder p)
        {
            types.Popdcl();

            if (trackScopes)
            {
                p.scope = Curfn.Func.Parents[p.scope - 1L];
                Curfn.Func.Marks[len(Curfn.Func.Marks) - 1L].Scope = p.scope;
            }
        }

        // linkname records a //go:linkname directive.
        private partial struct linkname
        {
            public src.Pos pos;
            public @string local;
            public @string remote;
        }

        private static void node(this ref noder p)
        {
            types.Block = 1L;
            imported_unsafe = false;

            p.lineno(p.file.PkgName);
            mkpackage(p.file.PkgName.Value);

            xtop = append(xtop, p.decls(p.file.DeclList));

            foreach (var (_, n) in p.linknames)
            {
                if (imported_unsafe)
                {
                    lookup(n.local).Linkname;

                    n.remote;
                }
                else
                {
                    yyerrorpos(n.pos, "//go:linkname only allowed in Go files that import \"unsafe\"");
                }
            }
            pragcgobuf += p.pragcgobuf;
            lineno = src.NoXPos;
            clearImports();
        }

        private static slice<ref Node> decls(this ref noder _p, slice<syntax.Decl> decls) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            constState cs = default;

            {
                var decl__prev1 = decl;

                foreach (var (_, __decl) in decls)
                {
                    decl = __decl;
                    p.lineno(decl);
                    switch (decl.type())
                    {
                        case ref syntax.ImportDecl decl:
                            p.importDecl(decl);
                            break;
                        case ref syntax.VarDecl decl:
                            l = append(l, p.varDecl(decl));
                            break;
                        case ref syntax.ConstDecl decl:
                            l = append(l, p.constDecl(decl, ref cs));
                            break;
                        case ref syntax.TypeDecl decl:
                            l = append(l, p.typeDecl(decl));
                            break;
                        case ref syntax.FuncDecl decl:
                            l = append(l, p.funcDecl(decl));
                            break;
                        default:
                        {
                            var decl = decl.type();
                            panic("unhandled Decl");
                            break;
                        }
                    }
                }

                decl = decl__prev1;
            }

            return;
        });

        private static void importDecl(this ref noder p, ref syntax.ImportDecl imp)
        {
            var val = p.basicLit(imp.Path);
            var ipkg = importfile(ref val);

            if (ipkg == null)
            {
                if (nerrors == 0L)
                {
                    Fatalf("phase error in import");
                }
                return;
            }
            ipkg.Direct = true;

            ref types.Sym my = default;
            if (imp.LocalPkgName != null)
            {
                my = p.name(imp.LocalPkgName);
            }
            else
            {
                my = lookup(ipkg.Name);
            }
            var pack = p.nod(imp, OPACK, null, null);
            pack.Sym = my;
            pack.Name.Pkg = ipkg;

            switch (my.Name)
            {
                case ".": 
                    importdot(ipkg, pack);
                    return;
                    break;
                case "init": 
                    yyerrorl(pack.Pos, "cannot import package as init - init must be a func");
                    return;
                    break;
                case "_": 
                    return;
                    break;
            }
            if (my.Def != null)
            {
                lineno = pack.Pos;
                redeclare(my, "as imported package name");
            }
            my.Def = asTypesNode(pack);
            my.Lastlineno = pack.Pos;
            my.Block = 1L; // at top level
        }

        private static slice<ref Node> varDecl(this ref noder p, ref syntax.VarDecl decl)
        {
            var names = p.declNames(decl.NameList);
            var typ = p.typeExprOrNil(decl.Type);

            slice<ref Node> exprs = default;
            if (decl.Values != null)
            {
                exprs = p.exprList(decl.Values);
            }
            p.lineno(decl);
            return variter(names, typ, exprs);
        }

        // constState tracks state between constant specifiers within a
        // declaration group. This state is kept separate from noder so nested
        // constant declarations are handled correctly (e.g., issue 15550).
        private partial struct constState
        {
            public ptr<syntax.Group> group;
            public ptr<Node> typ;
            public slice<ref Node> values;
            public long iota;
        }

        private static slice<ref Node> constDecl(this ref noder p, ref syntax.ConstDecl decl, ref constState cs)
        {
            if (decl.Group == null || decl.Group != cs.group)
            {
                cs.Value = new constState(group:decl.Group,);
            }
            var names = p.declNames(decl.NameList);
            var typ = p.typeExprOrNil(decl.Type);

            slice<ref Node> values = default;
            if (decl.Values != null)
            {
                values = p.exprList(decl.Values);
                cs.typ = typ;
                cs.values = values;
            }
            else
            {
                if (typ != null)
                {
                    yyerror("const declaration cannot have type without expression");
                }
                typ = cs.typ;
                values = cs.values;
            }
            slice<ref Node> nn = default;
            foreach (var (i, n) in names)
            {
                if (i >= len(values))
                {
                    yyerror("missing value in const declaration");
                    break;
                }
                var v = values[i];
                if (decl.Values == null)
                {
                    v = treecopy(v, n.Pos);
                }
                n.Op = OLITERAL;
                declare(n, dclcontext);

                n.Name.Param.Ntype = typ;
                n.Name.Defn = v;
                n.SetIota(cs.iota);

                nn = append(nn, p.nod(decl, ODCLCONST, n, null));
            }
            if (len(values) > len(names))
            {
                yyerror("extra expression in const declaration");
            }
            cs.iota++;

            return nn;
        }

        private static ref Node typeDecl(this ref noder p, ref syntax.TypeDecl decl)
        {
            var n = p.declName(decl.Name);
            n.Op = OTYPE;
            declare(n, dclcontext); 

            // decl.Type may be nil but in that case we got a syntax error during parsing
            var typ = p.typeExprOrNil(decl.Type);

            var param = n.Name.Param;
            param.Ntype = typ;
            param.Pragma = decl.Pragma;
            param.Alias = decl.Alias;
            if (param.Alias && param.Pragma != 0L)
            {
                yyerror("cannot specify directive with type alias");
                param.Pragma = 0L;
            }
            return p.nod(decl, ODCLTYPE, n, null);

        }

        private static slice<ref Node> declNames(this ref noder p, slice<ref syntax.Name> names)
        {
            slice<ref Node> nodes = default;
            foreach (var (_, name) in names)
            {
                nodes = append(nodes, p.declName(name));
            }
            return nodes;
        }

        private static ref Node declName(this ref noder p, ref syntax.Name name)
        { 
            // TODO(mdempsky): Set lineno?
            return dclname(p.name(name));
        }

        private static ref Node funcDecl(this ref noder p, ref syntax.FuncDecl fun)
        {
            var name = p.name(fun.Name);
            var t = p.signature(fun.Recv, fun.Type);
            var f = p.nod(fun, ODCLFUNC, null, null);

            if (fun.Recv == null)
            {
                if (name.Name == "init")
                {
                    name = renameinit();
                    if (t.List.Len() > 0L || t.Rlist.Len() > 0L)
                    {
                        yyerrorl(f.Pos, "func init must have no arguments and no return values");
                    }
                }
                if (localpkg.Name == "main" && name.Name == "main")
                {
                    if (t.List.Len() > 0L || t.Rlist.Len() > 0L)
                    {
                        yyerrorl(f.Pos, "func main must have no arguments and no return values");
                    }
                }
            }
            else
            {
                f.Func.Shortname = name;
                name = nblank.Sym; // filled in by typecheckfunc
            }
            f.Func.Nname = newfuncname(name);
            f.Func.Nname.Name.Defn = f;
            f.Func.Nname.Name.Param.Ntype = t;

            var pragma = fun.Pragma;
            f.Func.Pragma = fun.Pragma;
            f.SetNoescape(pragma & Noescape != 0L);
            if (pragma & Systemstack != 0L && pragma & Nosplit != 0L)
            {
                yyerrorl(f.Pos, "go:nosplit and go:systemstack cannot be combined");
            }
            if (fun.Recv == null)
            {
                declare(f.Func.Nname, PFUNC);
            }
            var oldScope = p.funchdr(f);

            if (fun.Body != null)
            {
                if (f.Noescape())
                {
                    yyerrorl(f.Pos, "can only use //go:noescape with external func implementations");
                }
                var body = p.stmts(fun.Body.List);
                if (body == null)
                {
                    body = new slice<ref Node>(new ref Node[] { p.nod(fun,OEMPTY,nil,nil) });
                }
                f.Nbody.Set(body);

                lineno = Ctxt.PosTable.XPos(fun.Body.Rbrace);
                f.Func.Endlineno = lineno;
            }
            else
            {
                if (pure_go || strings.HasPrefix(f.funcname(), "init."))
                {
                    yyerrorl(f.Pos, "missing function body");
                }
            }
            p.funcbody(oldScope);
            return f;
        }

        private static ref Node signature(this ref noder p, ref syntax.Field recv, ref syntax.FuncType typ)
        {
            var n = p.nod(typ, OTFUNC, null, null);
            if (recv != null)
            {
                n.Left = p.param(recv, false, false);
            }
            n.List.Set(p.@params(typ.ParamList, true));
            n.Rlist.Set(p.@params(typ.ResultList, false));
            return n;
        }

        private static slice<ref Node> @params(this ref noder p, slice<ref syntax.Field> @params, bool dddOk)
        {
            slice<ref Node> nodes = default;
            foreach (var (i, param) in params)
            {
                p.lineno(param);
                nodes = append(nodes, p.param(param, dddOk, i + 1L == len(params)));
            }
            return nodes;
        }

        private static ref Node param(this ref noder p, ref syntax.Field param, bool dddOk, bool final)
        {
            ref Node name = default;
            if (param.Name != null)
            {
                name = p.newname(param.Name);
            }
            var typ = p.typeExpr(param.Type);
            var n = p.nod(param, ODCLFIELD, name, typ); 

            // rewrite ...T parameter
            if (typ.Op == ODDD)
            {
                if (!dddOk)
                {
                    yyerror("cannot use ... in receiver or result parameter list");
                }
                else if (!final)
                {
                    yyerror("can only use ... with final parameter in list");
                }
                typ.Op = OTARRAY;
                typ.Right = typ.Left;
                typ.Left = null;
                n.SetIsddd(true);
                if (n.Left != null)
                {
                    n.Left.SetIsddd(true);
                }
            }
            return n;
        }

        private static slice<ref Node> exprList(this ref noder p, syntax.Expr expr)
        {
            {
                ref syntax.ListExpr (list, ok) = expr._<ref syntax.ListExpr>();

                if (ok)
                {
                    return p.exprs(list.ElemList);
                }

            }
            return new slice<ref Node>(new ref Node[] { p.expr(expr) });
        }

        private static slice<ref Node> exprs(this ref noder p, slice<syntax.Expr> exprs)
        {
            slice<ref Node> nodes = default;
            foreach (var (_, expr) in exprs)
            {
                nodes = append(nodes, p.expr(expr));
            }
            return nodes;
        }

        private static ref Node expr(this ref noder _p, syntax.Expr expr) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            p.lineno(expr);
            switch (expr.type())
            {
                case ref syntax.BadExpr expr:
                    return null;
                    break;
                case ref syntax.Name expr:
                    return p.mkname(expr);
                    break;
                case ref syntax.BasicLit expr:
                    return p.setlineno(expr, nodlit(p.basicLit(expr)));
                    break;
                case ref syntax.CompositeLit expr:
                    var n = p.nod(expr, OCOMPLIT, null, null);
                    if (expr.Type != null)
                    {
                        n.Right = p.expr(expr.Type);
                    }
                    var l = p.exprs(expr.ElemList);
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __e) in l)
                        {
                            i = __i;
                            e = __e;
                            l[i] = p.wrapname(expr.ElemList[i], e);
                        }

                        i = i__prev1;
                    }

                    n.List.Set(l);
                    lineno = Ctxt.PosTable.XPos(expr.Rbrace);
                    return n;
                    break;
                case ref syntax.KeyValueExpr expr:
                    return p.nod(expr, OKEY, p.expr(expr.Key), p.wrapname(expr.Value, p.expr(expr.Value)));
                    break;
                case ref syntax.FuncLit expr:
                    return p.funcLit(expr);
                    break;
                case ref syntax.ParenExpr expr:
                    return p.nod(expr, OPAREN, p.expr(expr.X), null);
                    break;
                case ref syntax.SelectorExpr expr:
                    var obj = p.expr(expr.X);
                    if (obj.Op == OPACK)
                    {
                        obj.Name.SetUsed(true);
                        return oldname(restrictlookup(expr.Sel.Value, obj.Name.Pkg));
                    }
                    return p.setlineno(expr, nodSym(OXDOT, obj, p.name(expr.Sel)));
                    break;
                case ref syntax.IndexExpr expr:
                    return p.nod(expr, OINDEX, p.expr(expr.X), p.expr(expr.Index));
                    break;
                case ref syntax.SliceExpr expr:
                    var op = OSLICE;
                    if (expr.Full)
                    {
                        op = OSLICE3;
                    }
                    n = p.nod(expr, op, p.expr(expr.X), null);
                    array<ref Node> index = new array<ref Node>(3L);
                    {
                        var i__prev1 = i;
                        var x__prev1 = x;

                        foreach (var (__i, __x) in expr.Index)
                        {
                            i = __i;
                            x = __x;
                            if (x != null)
                            {
                                index[i] = p.expr(x);
                            }
                        }

                        i = i__prev1;
                        x = x__prev1;
                    }

                    n.SetSliceBounds(index[0L], index[1L], index[2L]);
                    return n;
                    break;
                case ref syntax.AssertExpr expr:
                    if (expr.Type == null)
                    {
                        panic("unexpected AssertExpr");
                    } 
                    // TODO(mdempsky): parser.pexpr uses p.expr(), but
                    // seems like the type field should be parsed with
                    // ntype? Shrug, doesn't matter here.
                    return p.nod(expr, ODOTTYPE, p.expr(expr.X), p.expr(expr.Type));
                    break;
                case ref syntax.Operation expr:
                    if (expr.Op == syntax.Add && expr.Y != null)
                    {
                        return p.sum(expr);
                    }
                    var x = p.expr(expr.X);
                    if (expr.Y == null)
                    {
                        if (expr.Op == syntax.And)
                        {
                            x = unparen(x); // TODO(mdempsky): Needed?
                            if (x.Op == OCOMPLIT)
                            { 
                                // Special case for &T{...}: turn into (*T){...}.
                                // TODO(mdempsky): Switch back to p.nod after we
                                // get rid of gcCompat.
                                x.Right = nod(OIND, x.Right, null);
                                x.Right.SetImplicit(true);
                                return x;
                            }
                        }
                        return p.nod(expr, p.unOp(expr.Op), x, null);
                    }
                    return p.nod(expr, p.binOp(expr.Op), x, p.expr(expr.Y));
                    break;
                case ref syntax.CallExpr expr:
                    n = p.nod(expr, OCALL, p.expr(expr.Fun), null);
                    n.List.Set(p.exprs(expr.ArgList));
                    n.SetIsddd(expr.HasDots);
                    return n;
                    break;
                case ref syntax.ArrayType expr:
                    ref Node len = default;
                    if (expr.Len != null)
                    {
                        len = p.expr(expr.Len);
                    }
                    else
                    {
                        len = p.nod(expr, ODDD, null, null);
                    }
                    return p.nod(expr, OTARRAY, len, p.typeExpr(expr.Elem));
                    break;
                case ref syntax.SliceType expr:
                    return p.nod(expr, OTARRAY, null, p.typeExpr(expr.Elem));
                    break;
                case ref syntax.DotsType expr:
                    return p.nod(expr, ODDD, p.typeExpr(expr.Elem), null);
                    break;
                case ref syntax.StructType expr:
                    return p.structType(expr);
                    break;
                case ref syntax.InterfaceType expr:
                    return p.interfaceType(expr);
                    break;
                case ref syntax.FuncType expr:
                    return p.signature(null, expr);
                    break;
                case ref syntax.MapType expr:
                    return p.nod(expr, OTMAP, p.typeExpr(expr.Key), p.typeExpr(expr.Value));
                    break;
                case ref syntax.ChanType expr:
                    n = p.nod(expr, OTCHAN, p.typeExpr(expr.Elem), null);
                    n.Etype = types.EType(p.chanDir(expr.Dir));
                    return n;
                    break;
                case ref syntax.TypeSwitchGuard expr:
                    n = p.nod(expr, OTYPESW, null, p.expr(expr.X));
                    if (expr.Lhs != null)
                    {
                        n.Left = p.declName(expr.Lhs);
                        if (isblank(n.Left))
                        {
                            yyerror("invalid variable name %v in type switch", n.Left);
                        }
                    }
                    return n;
                    break;
            }
            panic("unhandled Expr");
        });

        // sum efficiently handles very large summation expressions (such as
        // in issue #16394). In particular, it avoids left recursion and
        // collapses string literals.
        private static ref Node sum(this ref noder p, syntax.Expr x)
        { 
            // While we need to handle long sums with asymptotic
            // efficiency, the vast majority of sums are very small: ~95%
            // have only 2 or 3 operands, and ~99% of string literals are
            // never concatenated.

            var adds = make_slice<ref syntax.Operation>(0L, 2L);
            while (true)
            {
                ref syntax.Operation (add, ok) = x._<ref syntax.Operation>();
                if (!ok || add.Op != syntax.Add || add.Y == null)
                {
                    break;
                }
                adds = append(adds, add);
                x = add.X;
            } 

            // nstr is the current rightmost string literal in the
            // summation (if any), and chunks holds its accumulated
            // substrings.
            //
            // Consider the expression x + "a" + "b" + "c" + y. When we
            // reach the string literal "a", we assign nstr to point to
            // its corresponding Node and initialize chunks to {"a"}.
            // Visiting the subsequent string literals "b" and "c", we
            // simply append their values to chunks. Finally, when we
            // reach the non-constant operand y, we'll join chunks to form
            // "abc" and reassign the "a" string literal's value.
            //
            // N.B., we need to be careful about named string constants
            // (indicated by Sym != nil) because 1) we can't modify their
            // value, as doing so would affect other uses of the string
            // constant, and 2) they may have types, which we need to
            // handle correctly. For now, we avoid these problems by
            // treating named string constants the same as non-constant
            // operands.
 

            // nstr is the current rightmost string literal in the
            // summation (if any), and chunks holds its accumulated
            // substrings.
            //
            // Consider the expression x + "a" + "b" + "c" + y. When we
            // reach the string literal "a", we assign nstr to point to
            // its corresponding Node and initialize chunks to {"a"}.
            // Visiting the subsequent string literals "b" and "c", we
            // simply append their values to chunks. Finally, when we
            // reach the non-constant operand y, we'll join chunks to form
            // "abc" and reassign the "a" string literal's value.
            //
            // N.B., we need to be careful about named string constants
            // (indicated by Sym != nil) because 1) we can't modify their
            // value, as doing so would affect other uses of the string
            // constant, and 2) they may have types, which we need to
            // handle correctly. For now, we avoid these problems by
            // treating named string constants the same as non-constant
            // operands.
            ref Node nstr = default;
            var chunks = make_slice<@string>(0L, 1L);

            var n = p.expr(x);
            if (Isconst(n, CTSTR) && n.Sym == null)
            {
                nstr = n;
                chunks = append(chunks, nstr.Val().U._<@string>());
            }
            for (var i = len(adds) - 1L; i >= 0L; i--)
            {
                var add = adds[i];

                var r = p.expr(add.Y);
                if (Isconst(r, CTSTR) && r.Sym == null)
                {
                    if (nstr != null)
                    { 
                        // Collapse r into nstr instead of adding to n.
                        chunks = append(chunks, r.Val().U._<@string>());
                        continue;
                    }
                    nstr = r;
                    chunks = append(chunks, nstr.Val().U._<@string>());
                }
                else
                {
                    if (len(chunks) > 1L)
                    {
                        nstr.SetVal(new Val(U:strings.Join(chunks,"")));
                    }
                    nstr = null;
                    chunks = chunks[..0L];
                }
                n = p.nod(add, OADD, n, r);
            }

            if (len(chunks) > 1L)
            {
                nstr.SetVal(new Val(U:strings.Join(chunks,"")));
            }
            return n;
        }

        private static ref Node typeExpr(this ref noder p, syntax.Expr typ)
        { 
            // TODO(mdempsky): Be stricter? typecheck should handle errors anyway.
            return p.expr(typ);
        }

        private static ref Node typeExprOrNil(this ref noder p, syntax.Expr typ)
        {
            if (typ != null)
            {
                return p.expr(typ);
            }
            return null;
        }

        private static types.ChanDir chanDir(this ref noder _p, syntax.ChanDir dir) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {

            if (dir == 0L) 
                return types.Cboth;
            else if (dir == syntax.SendOnly) 
                return types.Csend;
            else if (dir == syntax.RecvOnly) 
                return types.Crecv;
                        panic("unhandled ChanDir");
        });

        private static ref Node structType(this ref noder p, ref syntax.StructType expr)
        {
            slice<ref Node> l = default;
            foreach (var (i, field) in expr.FieldList)
            {
                p.lineno(field);
                ref Node n = default;
                if (field.Name == null)
                {
                    n = p.embedded(field.Type);
                }
                else
                {
                    n = p.nod(field, ODCLFIELD, p.newname(field.Name), p.typeExpr(field.Type));
                }
                if (i < len(expr.TagList) && expr.TagList[i] != null)
                {
                    n.SetVal(p.basicLit(expr.TagList[i]));
                }
                l = append(l, n);
            }
            p.lineno(expr);
            n = p.nod(expr, OTSTRUCT, null, null);
            n.List.Set(l);
            return n;
        }

        private static ref Node interfaceType(this ref noder p, ref syntax.InterfaceType expr)
        {
            slice<ref Node> l = default;
            foreach (var (_, method) in expr.MethodList)
            {
                p.lineno(method);
                ref Node n = default;
                if (method.Name == null)
                {
                    n = p.nod(method, ODCLFIELD, null, oldname(p.packname(method.Type)));
                }
                else
                {
                    var mname = p.newname(method.Name);
                    var sig = p.typeExpr(method.Type);
                    sig.Left = fakeRecv();
                    n = p.nod(method, ODCLFIELD, mname, sig);
                    ifacedcl(n);
                }
                l = append(l, n);
            }
            n = p.nod(expr, OTINTER, null, null);
            n.List.Set(l);
            return n;
        }

        private static ref types.Sym packname(this ref noder _p, syntax.Expr expr) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            switch (expr.type())
            {
                case ref syntax.Name expr:
                    var name = p.name(expr);
                    {
                        var n = oldname(name);

                        if (n.Name != null && n.Name.Pack != null)
                        {
                            n.Name.Pack.Name.SetUsed(true);
                        }

                    }
                    return name;
                    break;
                case ref syntax.SelectorExpr expr:
                    name = p.name(expr.X._<ref syntax.Name>());
                    ref types.Pkg pkg = default;
                    if (asNode(name.Def) == null || asNode(name.Def).Op != OPACK)
                    {
                        yyerror("%v is not a package", name);
                        pkg = localpkg;
                    }
                    else
                    {
                        asNode(name.Def).Name.SetUsed(true);
                        pkg = asNode(name.Def).Name.Pkg;
                    }
                    return restrictlookup(expr.Sel.Value, pkg);
                    break;
            }
            panic(fmt.Sprintf("unexpected packname: %#v", expr));
        });

        private static ref Node embedded(this ref noder _p, syntax.Expr typ) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            ref syntax.Operation (op, isStar) = typ._<ref syntax.Operation>();
            if (isStar)
            {
                if (op.Op != syntax.Mul || op.Y != null)
                {
                    panic("unexpected Operation");
                }
                typ = op.X;
            }
            var sym = p.packname(typ);
            var n = nod(ODCLFIELD, newname(lookup(sym.Name)), oldname(sym));
            n.SetEmbedded(true);

            if (isStar)
            {
                n.Right = p.nod(op, OIND, n.Right, null);
            }
            return n;
        });

        private static slice<ref Node> stmts(this ref noder p, slice<syntax.Stmt> stmts)
        {
            return p.stmtsFall(stmts, false);
        }

        private static slice<ref Node> stmtsFall(this ref noder p, slice<syntax.Stmt> stmts, bool fallOK)
        {
            slice<ref Node> nodes = default;
            foreach (var (i, stmt) in stmts)
            {
                var s = p.stmtFall(stmt, fallOK && i + 1L == len(stmts));
                if (s == null)
                {
                }
                else if (s.Op == OBLOCK && s.Ninit.Len() == 0L)
                {
                    nodes = append(nodes, s.List.Slice());
                }
                else
                {
                    nodes = append(nodes, s);
                }
            }
            return nodes;
        }

        private static ref Node stmt(this ref noder p, syntax.Stmt stmt)
        {
            return p.stmtFall(stmt, false);
        }

        private static ref Node stmtFall(this ref noder _p, syntax.Stmt stmt, bool fallOK) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            p.lineno(stmt);
            switch (stmt.type())
            {
                case ref syntax.EmptyStmt stmt:
                    return null;
                    break;
                case ref syntax.LabeledStmt stmt:
                    return p.labeledStmt(stmt, fallOK);
                    break;
                case ref syntax.BlockStmt stmt:
                    var l = p.blockStmt(stmt);
                    if (len(l) == 0L)
                    { 
                        // TODO(mdempsky): Line number?
                        return nod(OEMPTY, null, null);
                    }
                    return liststmt(l);
                    break;
                case ref syntax.ExprStmt stmt:
                    return p.wrapname(stmt, p.expr(stmt.X));
                    break;
                case ref syntax.SendStmt stmt:
                    return p.nod(stmt, OSEND, p.expr(stmt.Chan), p.expr(stmt.Value));
                    break;
                case ref syntax.DeclStmt stmt:
                    return liststmt(p.decls(stmt.DeclList));
                    break;
                case ref syntax.AssignStmt stmt:
                    if (stmt.Op != 0L && stmt.Op != syntax.Def)
                    {
                        var n = p.nod(stmt, OASOP, p.expr(stmt.Lhs), p.expr(stmt.Rhs));
                        n.SetImplicit(stmt.Rhs == syntax.ImplicitOne);
                        n.Etype = types.EType(p.binOp(stmt.Op));
                        return n;
                    }
                    n = p.nod(stmt, OAS, null, null); // assume common case

                    var rhs = p.exprList(stmt.Rhs);
                    var lhs = p.assignList(stmt.Lhs, n, stmt.Op == syntax.Def);

                    if (len(lhs) == 1L && len(rhs) == 1L)
                    { 
                        // common case
                        n.Left = lhs[0L];
                        n.Right = rhs[0L];
                    }
                    else
                    {
                        n.Op = OAS2;
                        n.List.Set(lhs);
                        n.Rlist.Set(rhs);
                    }
                    return n;
                    break;
                case ref syntax.BranchStmt stmt:
                    Op op = default;

                    if (stmt.Tok == syntax.Break) 
                        op = OBREAK;
                    else if (stmt.Tok == syntax.Continue) 
                        op = OCONTINUE;
                    else if (stmt.Tok == syntax.Fallthrough) 
                        if (!fallOK)
                        {
                            yyerror("fallthrough statement out of place");
                        }
                        op = OFALL;
                    else if (stmt.Tok == syntax.Goto) 
                        op = OGOTO;
                    else 
                        panic("unhandled BranchStmt");
                                        n = p.nod(stmt, op, null, null);
                    if (stmt.Label != null)
                    {
                        n.Left = p.newname(stmt.Label);
                    }
                    return n;
                    break;
                case ref syntax.CallStmt stmt:
                    op = default;

                    if (stmt.Tok == syntax.Defer) 
                        op = ODEFER;
                    else if (stmt.Tok == syntax.Go) 
                        op = OPROC;
                    else 
                        panic("unhandled CallStmt");
                                        return p.nod(stmt, op, p.expr(stmt.Call), null);
                    break;
                case ref syntax.ReturnStmt stmt:
                    slice<ref Node> results = default;
                    if (stmt.Results != null)
                    {
                        results = p.exprList(stmt.Results);
                    }
                    n = p.nod(stmt, ORETURN, null, null);
                    n.List.Set(results);
                    if (n.List.Len() == 0L && Curfn != null)
                    {
                        foreach (var (_, ln) in Curfn.Func.Dcl)
                        {
                            if (ln.Class() == PPARAM)
                            {
                                continue;
                            }
                            if (ln.Class() != PPARAMOUT)
                            {
                                break;
                            }
                            if (asNode(ln.Sym.Def) != ln)
                            {
                                yyerror("%s is shadowed during return", ln.Sym.Name);
                            }
                        }
                    }
                    return n;
                    break;
                case ref syntax.IfStmt stmt:
                    return p.ifStmt(stmt);
                    break;
                case ref syntax.ForStmt stmt:
                    return p.forStmt(stmt);
                    break;
                case ref syntax.SwitchStmt stmt:
                    return p.switchStmt(stmt);
                    break;
                case ref syntax.SelectStmt stmt:
                    return p.selectStmt(stmt);
                    break;
            }
            panic("unhandled Stmt");
        });

        private static slice<ref Node> assignList(this ref noder p, syntax.Expr expr, ref Node defn, bool colas)
        {
            if (!colas)
            {
                return p.exprList(expr);
            }
            defn.SetColas(true);

            slice<syntax.Expr> exprs = default;
            {
                ref syntax.ListExpr (list, ok) = expr._<ref syntax.ListExpr>();

                if (ok)
                {
                    exprs = list.ElemList;
                }
                else
                {
                    exprs = new slice<syntax.Expr>(new syntax.Expr[] { expr });
                }

            }

            var res = make_slice<ref Node>(len(exprs));
            var seen = make_map<ref types.Sym, bool>(len(exprs));

            var newOrErr = false;
            foreach (var (i, expr) in exprs)
            {
                p.lineno(expr);
                res[i] = nblank;

                ref syntax.Name (name, ok) = expr._<ref syntax.Name>();
                if (!ok)
                {
                    yyerrorpos(expr.Pos(), "non-name %v on left side of :=", p.expr(expr));
                    newOrErr = true;
                    continue;
                }
                var sym = p.name(name);
                if (sym.IsBlank())
                {
                    continue;
                }
                if (seen[sym])
                {
                    yyerrorpos(expr.Pos(), "%v repeated on left side of :=", sym);
                    newOrErr = true;
                    continue;
                }
                seen[sym] = true;

                if (sym.Block == types.Block)
                {
                    res[i] = oldname(sym);
                    continue;
                }
                newOrErr = true;
                var n = newname(sym);
                declare(n, dclcontext);
                n.Name.Defn = defn;
                defn.Ninit.Append(nod(ODCL, n, null));
                res[i] = n;
            }
            if (!newOrErr)
            {
                yyerrorl(defn.Pos, "no new variables on left side of :=");
            }
            return res;
        }

        private static slice<ref Node> blockStmt(this ref noder p, ref syntax.BlockStmt stmt)
        {
            p.openScope(stmt.Pos());
            var nodes = p.stmts(stmt.List);
            p.closeScope(stmt.Rbrace);
            return nodes;
        }

        private static ref Node ifStmt(this ref noder p, ref syntax.IfStmt stmt)
        {
            p.openScope(stmt.Pos());
            var n = p.nod(stmt, OIF, null, null);
            if (stmt.Init != null)
            {
                n.Ninit.Set1(p.stmt(stmt.Init));
            }
            if (stmt.Cond != null)
            {
                n.Left = p.expr(stmt.Cond);
            }
            n.Nbody.Set(p.blockStmt(stmt.Then));
            if (stmt.Else != null)
            {
                var e = p.stmt(stmt.Else);
                if (e.Op == OBLOCK && e.Ninit.Len() == 0L)
                {
                    n.Rlist.Set(e.List.Slice());
                }
                else
                {
                    n.Rlist.Set1(e);
                }
            }
            p.closeAnotherScope();
            return n;
        }

        private static ref Node forStmt(this ref noder _p, ref syntax.ForStmt _stmt) => func(_p, _stmt, (ref noder p, ref syntax.ForStmt stmt, Defer _, Panic panic, Recover __) =>
        {
            p.openScope(stmt.Pos());
            ref Node n = default;
            {
                ref syntax.RangeClause (r, ok) = stmt.Init._<ref syntax.RangeClause>();

                if (ok)
                {
                    if (stmt.Cond != null || stmt.Post != null)
                    {
                        panic("unexpected RangeClause");
                    }
                    n = p.nod(r, ORANGE, null, p.expr(r.X));
                    if (r.Lhs != null)
                    {
                        n.List.Set(p.assignList(r.Lhs, n, r.Def));
                    }
                }
                else
                {
                    n = p.nod(stmt, OFOR, null, null);
                    if (stmt.Init != null)
                    {
                        n.Ninit.Set1(p.stmt(stmt.Init));
                    }
                    if (stmt.Cond != null)
                    {
                        n.Left = p.expr(stmt.Cond);
                    }
                    if (stmt.Post != null)
                    {
                        n.Right = p.stmt(stmt.Post);
                    }
                }

            }
            n.Nbody.Set(p.blockStmt(stmt.Body));
            p.closeAnotherScope();
            return n;
        });

        private static ref Node switchStmt(this ref noder p, ref syntax.SwitchStmt stmt)
        {
            p.openScope(stmt.Pos());
            var n = p.nod(stmt, OSWITCH, null, null);
            if (stmt.Init != null)
            {
                n.Ninit.Set1(p.stmt(stmt.Init));
            }
            if (stmt.Tag != null)
            {
                n.Left = p.expr(stmt.Tag);
            }
            var tswitch = n.Left;
            if (tswitch != null && tswitch.Op != OTYPESW)
            {
                tswitch = null;
            }
            n.List.Set(p.caseClauses(stmt.Body, tswitch, stmt.Rbrace));

            p.closeScope(stmt.Rbrace);
            return n;
        }

        private static slice<ref Node> caseClauses(this ref noder p, slice<ref syntax.CaseClause> clauses, ref Node tswitch, src.Pos rbrace)
        {
            slice<ref Node> nodes = default;
            foreach (var (i, clause) in clauses)
            {
                p.lineno(clause);
                if (i > 0L)
                {
                    p.closeScope(clause.Pos());
                }
                p.openScope(clause.Pos());

                var n = p.nod(clause, OXCASE, null, null);
                if (clause.Cases != null)
                {
                    n.List.Set(p.exprList(clause.Cases));
                }
                if (tswitch != null && tswitch.Left != null)
                {
                    var nn = newname(tswitch.Left.Sym);
                    declare(nn, dclcontext);
                    n.Rlist.Set1(nn); 
                    // keep track of the instances for reporting unused
                    nn.Name.Defn = tswitch;
                } 

                // Trim trailing empty statements. We omit them from
                // the Node AST anyway, and it's easier to identify
                // out-of-place fallthrough statements without them.
                var body = clause.Body;
                while (len(body) > 0L)
                {
                    {
                        ref syntax.EmptyStmt (_, ok) = body[len(body) - 1L]._<ref syntax.EmptyStmt>();

                        if (!ok)
                        {
                            break;
                        }

                    }
                    body = body[..len(body) - 1L];
                }


                n.Nbody.Set(p.stmtsFall(body, true));
                {
                    var l = n.Nbody.Len();

                    if (l > 0L && n.Nbody.Index(l - 1L).Op == OFALL)
                    {
                        if (tswitch != null)
                        {
                            yyerror("cannot fallthrough in type switch");
                        }
                        if (i + 1L == len(clauses))
                        {
                            yyerror("cannot fallthrough final case in switch");
                        }
                    }

                }

                nodes = append(nodes, n);
            }
            if (len(clauses) > 0L)
            {
                p.closeScope(rbrace);
            }
            return nodes;
        }

        private static ref Node selectStmt(this ref noder p, ref syntax.SelectStmt stmt)
        {
            var n = p.nod(stmt, OSELECT, null, null);
            n.List.Set(p.commClauses(stmt.Body, stmt.Rbrace));
            return n;
        }

        private static slice<ref Node> commClauses(this ref noder p, slice<ref syntax.CommClause> clauses, src.Pos rbrace)
        {
            slice<ref Node> nodes = default;
            foreach (var (i, clause) in clauses)
            {
                p.lineno(clause);
                if (i > 0L)
                {
                    p.closeScope(clause.Pos());
                }
                p.openScope(clause.Pos());

                var n = p.nod(clause, OXCASE, null, null);
                if (clause.Comm != null)
                {
                    n.List.Set1(p.stmt(clause.Comm));
                }
                n.Nbody.Set(p.stmts(clause.Body));
                nodes = append(nodes, n);
            }
            if (len(clauses) > 0L)
            {
                p.closeScope(rbrace);
            }
            return nodes;
        }

        private static ref Node labeledStmt(this ref noder p, ref syntax.LabeledStmt label, bool fallOK)
        {
            var lhs = p.nod(label, OLABEL, p.newname(label.Label), null);

            ref Node ls = default;
            if (label.Stmt != null)
            { // TODO(mdempsky): Should always be present.
                ls = p.stmtFall(label.Stmt, fallOK);
            }
            lhs.Name.Defn = ls;
            ref Node l = new slice<ref Node>(new ref Node[] { lhs });
            if (ls != null)
            {
                if (ls.Op == OBLOCK && ls.Ninit.Len() == 0L)
                {
                    l = append(l, ls.List.Slice());
                }
                else
                {
                    l = append(l, ls);
                }
            }
            return liststmt(l);
        }

        private static array<Op> unOps = new array<Op>(InitKeyedValues<Op>((syntax.Recv, ORECV), (syntax.Mul, OIND), (syntax.And, OADDR), (syntax.Not, ONOT), (syntax.Xor, OCOM), (syntax.Add, OPLUS), (syntax.Sub, OMINUS)));

        private static Op unOp(this ref noder _p, syntax.Operator op) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            if (uint64(op) >= uint64(len(unOps)) || unOps[op] == 0L)
            {
                panic("invalid Operator");
            }
            return unOps[op];
        });

        private static array<Op> binOps = new array<Op>(InitKeyedValues<Op>((syntax.OrOr, OOROR), (syntax.AndAnd, OANDAND), (syntax.Eql, OEQ), (syntax.Neq, ONE), (syntax.Lss, OLT), (syntax.Leq, OLE), (syntax.Gtr, OGT), (syntax.Geq, OGE), (syntax.Add, OADD), (syntax.Sub, OSUB), (syntax.Or, OOR), (syntax.Xor, OXOR), (syntax.Mul, OMUL), (syntax.Div, ODIV), (syntax.Rem, OMOD), (syntax.And, OAND), (syntax.AndNot, OANDNOT), (syntax.Shl, OLSH), (syntax.Shr, ORSH)));

        private static Op binOp(this ref noder _p, syntax.Operator op) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {
            if (uint64(op) >= uint64(len(binOps)) || binOps[op] == 0L)
            {
                panic("invalid Operator");
            }
            return binOps[op];
        });

        private static Val basicLit(this ref noder _p, ref syntax.BasicLit _lit) => func(_p, _lit, (ref noder p, ref syntax.BasicLit lit, Defer _, Panic panic, Recover __) =>
        { 
            // TODO: Don't try to convert if we had syntax errors (conversions may fail).
            //       Use dummy values so we can continue to compile. Eventually, use a
            //       form of "unknown" literals that are ignored during type-checking so
            //       we can continue type-checking w/o spurious follow-up errors.
            {
                var s = lit.Value;


                if (lit.Kind == syntax.IntLit) 
                    ptr<object> x = @new<Mpint>();
                    x.SetString(s);
                    return new Val(U:x);
                else if (lit.Kind == syntax.FloatLit) 
                    x = newMpflt();
                    x.SetString(s);
                    return new Val(U:x);
                else if (lit.Kind == syntax.ImagLit) 
                    x = @new<Mpcplx>();
                    x.Imag.SetString(strings.TrimSuffix(s, "i"));
                    return new Val(U:x);
                else if (lit.Kind == syntax.RuneLit) 
                    int r = default;
                    {
                        var u__prev1 = u;

                        var (u, err) = strconv.Unquote(s);

                        if (err == null && len(u) > 0L)
                        { 
                            // Package syntax already reported any errors.
                            // Check for them again though because 0 is a
                            // better fallback value for invalid rune
                            // literals than 0xFFFD.
                            if (len(u) == 1L)
                            {
                                r = rune(u[0L]);
                            }
                            else
                            {
                                r, _ = utf8.DecodeRuneInString(u);
                            }
                        }

                        u = u__prev1;

                    }
                    x = @new<Mpint>();
                    x.SetInt64(int64(r));
                    x.Rune = true;
                    return new Val(U:x);
                else if (lit.Kind == syntax.StringLit) 
                    if (len(s) > 0L && s[0L] == '`')
                    { 
                        // strip carriage returns from raw string
                        s = strings.Replace(s, "\r", "", -1L);
                    } 
                    // Ignore errors because package syntax already reported them.
                    var (u, _) = strconv.Unquote(s);
                    return new Val(U:u);
                else 
                    panic("unhandled BasicLit kind");

            }
        });

        private static ref types.Sym name(this ref noder p, ref syntax.Name name)
        {
            return lookup(name.Value);
        }

        private static ref Node mkname(this ref noder p, ref syntax.Name name)
        { 
            // TODO(mdempsky): Set line number?
            return mkname(p.name(name));
        }

        private static ref Node newname(this ref noder p, ref syntax.Name name)
        { 
            // TODO(mdempsky): Set line number?
            return newname(p.name(name));
        }

        private static ref Node wrapname(this ref noder p, syntax.Node n, ref Node x)
        { 
            // These nodes do not carry line numbers.
            // Introduce a wrapper node to give them the correct line.

            if (x.Op == OTYPE || x.Op == OLITERAL)
            {
                if (x.Sym == null)
                {
                    break;
                }
                fallthrough = true;
            }
            if (fallthrough || x.Op == ONAME || x.Op == ONONAME || x.Op == OPACK)
            {
                x = p.nod(n, OPAREN, x, null);
                x.SetImplicit(true);
                goto __switch_break0;
            }

            __switch_break0:;
            return x;
        }

        private static ref Node nod(this ref noder p, syntax.Node orig, Op op, ref Node left, ref Node right)
        {
            return p.setlineno(orig, nod(op, left, right));
        }

        private static ref Node setlineno(this ref noder p, syntax.Node src_, ref Node dst)
        {
            var pos = src_.Pos();
            if (!pos.IsKnown())
            { 
                // TODO(mdempsky): Shouldn't happen. Fix package syntax.
                return dst;
            }
            dst.Pos = Ctxt.PosTable.XPos(pos);
            return dst;
        }

        private static void lineno(this ref noder p, syntax.Node n)
        {
            if (n == null)
            {
                return;
            }
            var pos = n.Pos();
            if (!pos.IsKnown())
            { 
                // TODO(mdempsky): Shouldn't happen. Fix package syntax.
                return;
            }
            lineno = Ctxt.PosTable.XPos(pos);
        }

        // error is called concurrently if files are parsed concurrently.
        private static void error(this ref noder p, error err)
        {
            p.err.Send(err._<syntax.Error>());
        }

        // pragmas that are allowed in the std lib, but don't have
        // a syntax.Pragma value (see lex.go) associated with them.
        private static map allowedStdPragmas = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"go:cgo_export_static":true,"go:cgo_export_dynamic":true,"go:cgo_import_static":true,"go:cgo_import_dynamic":true,"go:cgo_ldflag":true,"go:cgo_dynamic_linker":true,"go:generate":true,};

        // pragma is called concurrently if files are parsed concurrently.
        private static syntax.Pragma pragma(this ref noder _p, src.Pos pos, @string text) => func(_p, (ref noder p, Defer _, Panic panic, Recover __) =>
        {

            if (strings.HasPrefix(text, "line ")) 
            {
                // line directives are handled by syntax package
                panic("unreachable");
                goto __switch_break1;
            }
            if (strings.HasPrefix(text, "go:linkname "))
            {
                var f = strings.Fields(text);
                if (len(f) != 3L)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:"usage: //go:linkname localname linkname"));
                    break;
                }
                p.linknames = append(p.linknames, new linkname(pos,f[1],f[2]));
                goto __switch_break1;
            }
            if (strings.HasPrefix(text, "go:cgo_import_dynamic ")) 
            {
                // This is permitted for general use because Solaris
                // code relies on it in golang.org/x/sys/unix and others.
                var fields = pragmaFields(text);
                if (len(fields) >= 4L)
                {
                    var lib = strings.Trim(fields[3L], "\"");
                    if (lib != "" && !safeArg(lib) && !isCgoGeneratedFile(pos))
                    {
                        p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("invalid library name %q in cgo_import_dynamic directive",lib)));
                    }
                    p.pragcgobuf += p.pragcgo(pos, text);
                    return pragmaValue("go:cgo_import_dynamic");
                }
                fallthrough = true;
            }
            if (fallthrough || strings.HasPrefix(text, "go:cgo_")) 
            {
                // For security, we disallow //go:cgo_* directives other
                // than cgo_import_dynamic outside cgo-generated files.
                // Exception: they are allowed in the standard library, for runtime and syscall.
                if (!isCgoGeneratedFile(pos) && !compiling_std)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s only allowed in cgo-generated code",text)));
                }
                p.pragcgobuf += p.pragcgo(pos, text);
            }
            // default: 
                var verb = text;
                {
                    var i = strings.Index(text, " ");

                    if (i >= 0L)
                    {
                        verb = verb[..i];
                    }

                }
                var prag = pragmaValue(verb);
                const var runtimePragmas = Systemstack | Nowritebarrier | Nowritebarrierrec | Yeswritebarrierrec;

                if (!compiling_runtime && prag & runtimePragmas != 0L)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s only allowed in runtime",verb)));
                }
                if (prag == 0L && !allowedStdPragmas[verb] && compiling_std)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s is not allowed in the standard library",verb)));
                }
                return prag;

            __switch_break1:;

            return 0L;
        });

        // isCgoGeneratedFile reports whether pos is in a file
        // generated by cgo, which is to say a file with name
        // beginning with "_cgo_". Such files are allowed to
        // contain cgo directives, and for security reasons
        // (primarily misuse of linker flags), other files are not.
        // See golang.org/issue/23672.
        private static bool isCgoGeneratedFile(src.Pos pos)
        {
            return strings.HasPrefix(filepath.Base(filepath.Clean(pos.AbsFilename())), "_cgo_");
        }

        // safeArg reports whether arg is a "safe" command-line argument,
        // meaning that when it appears in a command-line, it probably
        // doesn't have some special meaning other than its own name.
        // This is copied from SafeArg in cmd/go/internal/load/pkg.go.
        private static bool safeArg(@string name)
        {
            if (name == "")
            {
                return false;
            }
            var c = name[0L];
            return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '.' || c == '_' || c == '/' || c >= utf8.RuneSelf;
        }

        private static ref Node mkname(ref types.Sym sym)
        {
            var n = oldname(sym);
            if (n.Name != null && n.Name.Pack != null)
            {
                n.Name.Pack.Name.SetUsed(true);
            }
            return n;
        }

        private static ref Node unparen(ref Node x)
        {
            while (x.Op == OPAREN)
            {
                x = x.Left;
            }

            return x;
        }
    }
}}}}
