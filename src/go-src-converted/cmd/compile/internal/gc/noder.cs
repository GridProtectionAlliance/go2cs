// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:02 UTC
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
using obj = go.cmd.@internal.obj_package;
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
        // parseFiles concurrently parses files into *syntax.File structures.
        // Each declaration in every *syntax.File is converted to a syntax tree
        // and its root represented by *Node is appended to xtop.
        // Returns the total count of parsed lines.
        private static ulong parseFiles(slice<@string> filenames) => func((defer, _, __) =>
        {
            var noders = make_slice<ptr<noder>>(0L, len(filenames)); 
            // Limit the number of simultaneously open files.
            var sem = make_channel<object>(runtime.GOMAXPROCS(0L) + 10L);

            foreach (var (_, filename) in filenames)
            {
                ptr<noder> p = addr(new noder(basemap:make(map[*syntax.PosBase]*src.PosBase),err:make(chansyntax.Error),));
                noders = append(noders, p);

                go_(() => filename =>
                {
                    sem.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                    defer(() =>
                    {
                        sem.Receive();
                    }());
                    defer(close(p.err));
                    var @base = syntax.NewFileBase(filename);

                    var (f, err) = os.Open(filename);
                    if (err != null)
                    {
                        p.error(new syntax.Error(Msg:err.Error()));
                        return ;
                    }
                    defer(f.Close());

                    p.file, _ = syntax.Parse(base, f, p.error, p.pragma, syntax.CheckBranches); // errors are tracked via p.error
                }(filename));

            }            ulong lines = default;
            {
                ptr<noder> p__prev1 = p;

                foreach (var (_, __p) in noders)
                {
                    p = __p;
                    foreach (var (e) in p.err)
                    {
                        p.yyerrorpos(e.Pos, "%s", e.Msg);
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

            localpkg.Height = myheight;

            return lines;

        });

        // makeSrcPosBase translates from a *syntax.PosBase to a *src.PosBase.
        private static ptr<src.PosBase> makeSrcPosBase(this ptr<noder> _addr_p, ptr<syntax.PosBase> _addr_b0)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.PosBase b0 = ref _addr_b0.val;
 
            // fast path: most likely PosBase hasn't changed
            if (p.basecache.last == b0)
            {
                return _addr_p.basecache.@base!;
            }

            var (b1, ok) = p.basemap[b0];
            if (!ok)
            {
                var fn = b0.Filename();
                if (b0.IsFileBase())
                {
                    b1 = src.NewFileBase(fn, absFilename(fn));
                }
                else
                { 
                    // line directive base
                    var p0 = b0.Pos();
                    var p1 = src.MakePos(p.makeSrcPosBase(p0.Base()), p0.Line(), p0.Col());
                    b1 = src.NewLinePragmaBase(p1, fn, fileh(fn), b0.Line(), b0.Col());

                }

                p.basemap[b0] = b1;

            } 

            // update cache
            p.basecache.last = b0;
            p.basecache.@base = b1;

            return _addr_b1!;

        }

        private static src.XPos makeXPos(this ptr<noder> _addr_p, syntax.Pos pos)
        {
            src.XPos _ = default;
            ref noder p = ref _addr_p.val;

            return Ctxt.PosTable.XPos(src.MakePos(p.makeSrcPosBase(pos.Base()), pos.Line(), pos.Col()));
        }

        private static void yyerrorpos(this ptr<noder> _addr_p, syntax.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref noder p = ref _addr_p.val;

            yyerrorl(p.makeXPos(pos), format, args);
        }

        private static @string pathPrefix = default;

        // TODO(gri) Can we eliminate fileh in favor of absFilename?
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
            public map<ptr<syntax.PosBase>, ptr<src.PosBase>> basemap;
            public ptr<syntax.File> file;
            public slice<linkname> linknames;
            public slice<slice<@string>> pragcgobuf;
            public channel<syntax.Error> err;
            public ScopeID scope; // scopeVars is a stack tracking the number of variables declared in the
// current function at the moment each open scope was opened.
            public slice<long> scopeVars;
            public syntax.Pos lastCloseScopePos;
        }

        private static void funcBody(this ptr<noder> _addr_p, ptr<Node> _addr_fn, ptr<syntax.BlockStmt> _addr_block)
        {
            ref noder p = ref _addr_p.val;
            ref Node fn = ref _addr_fn.val;
            ref syntax.BlockStmt block = ref _addr_block.val;

            var oldScope = p.scope;
            p.scope = 0L;
            funchdr(fn);

            if (block != null)
            {
                var body = p.stmts(block.List);
                if (body == null)
                {
                    body = new slice<ptr<Node>>(new ptr<Node>[] { nod(OEMPTY,nil,nil) });
                }

                fn.Nbody.Set(body);

                lineno = p.makeXPos(block.Rbrace);
                fn.Func.Endlineno = lineno;

            }

            funcbody();
            p.scope = oldScope;

        }

        private static void openScope(this ptr<noder> _addr_p, syntax.Pos pos)
        {
            ref noder p = ref _addr_p.val;

            types.Markdcl();

            if (trackScopes)
            {
                Curfn.Func.Parents = append(Curfn.Func.Parents, p.scope);
                p.scopeVars = append(p.scopeVars, len(Curfn.Func.Dcl));
                p.scope = ScopeID(len(Curfn.Func.Parents));

                p.markScope(pos);
            }

        }

        private static void closeScope(this ptr<noder> _addr_p, syntax.Pos pos)
        {
            ref noder p = ref _addr_p.val;

            p.lastCloseScopePos = pos;
            types.Popdcl();

            if (trackScopes)
            {
                var scopeVars = p.scopeVars[len(p.scopeVars) - 1L];
                p.scopeVars = p.scopeVars[..len(p.scopeVars) - 1L];
                if (scopeVars == len(Curfn.Func.Dcl))
                { 
                    // no variables were declared in this scope, so we can retract it.

                    if (int(p.scope) != len(Curfn.Func.Parents))
                    {
                        Fatalf("scope tracking inconsistency, no variables declared but scopes were not retracted");
                    }

                    p.scope = Curfn.Func.Parents[p.scope - 1L];
                    Curfn.Func.Parents = Curfn.Func.Parents[..len(Curfn.Func.Parents) - 1L];

                    var nmarks = len(Curfn.Func.Marks);
                    Curfn.Func.Marks[nmarks - 1L].Scope = p.scope;
                    var prevScope = ScopeID(0L);
                    if (nmarks >= 2L)
                    {
                        prevScope = Curfn.Func.Marks[nmarks - 2L].Scope;
                    }

                    if (Curfn.Func.Marks[nmarks - 1L].Scope == prevScope)
                    {
                        Curfn.Func.Marks = Curfn.Func.Marks[..nmarks - 1L];
                    }

                    return ;

                }

                p.scope = Curfn.Func.Parents[p.scope - 1L];

                p.markScope(pos);

            }

        }

        private static void markScope(this ptr<noder> _addr_p, syntax.Pos pos)
        {
            ref noder p = ref _addr_p.val;

            var xpos = p.makeXPos(pos);
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
        private static void closeAnotherScope(this ptr<noder> _addr_p)
        {
            ref noder p = ref _addr_p.val;

            p.closeScope(p.lastCloseScopePos);
        }

        // linkname records a //go:linkname directive.
        private partial struct linkname
        {
            public syntax.Pos pos;
            public @string local;
            public @string remote;
        }

        private static void node(this ptr<noder> _addr_p)
        {
            ref noder p = ref _addr_p.val;

            types.Block = 1L;
            imported_unsafe = false;

            p.setlineno(p.file.PkgName);
            mkpackage(p.file.PkgName.Value);

            {
                ptr<Pragma> (pragma, ok) = p.file.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    p.checkUnused(pragma);
                }

            }


            xtop = append(xtop, p.decls(p.file.DeclList));

            foreach (var (_, n) in p.linknames)
            {
                if (!imported_unsafe)
                {
                    p.yyerrorpos(n.pos, "//go:linkname only allowed in Go files that import \"unsafe\"");
                    continue;
                }

                var s = lookup(n.local);
                if (n.remote != "")
                {
                    s.Linkname = n.remote;
                }
                else
                { 
                    // Use the default object symbol name if the
                    // user didn't provide one.
                    if (myimportpath == "")
                    {
                        p.yyerrorpos(n.pos, "//go:linkname requires linkname argument or -p compiler flag");
                    }
                    else
                    {
                        s.Linkname = objabi.PathToPrefix(myimportpath) + "." + n.local;
                    }

                }

            } 

            // The linker expects an ABI0 wrapper for all cgo-exported
            // functions.
            foreach (var (_, prag) in p.pragcgobuf)
            {
                switch (prag[0L])
                {
                    case "cgo_export_static": 

                    case "cgo_export_dynamic": 
                        if (symabiRefs == null)
                        {
                            symabiRefs = make_map<@string, obj.ABI>();
                        }

                        symabiRefs[prag[1L]] = obj.ABI0;
                        break;
                }

            }
            pragcgobuf = append(pragcgobuf, p.pragcgobuf);
            lineno = src.NoXPos;
            clearImports();

        }

        private static slice<ptr<Node>> decls(this ptr<noder> _addr_p, slice<syntax.Decl> decls) => func((_, panic, __) =>
        {
            slice<ptr<Node>> l = default;
            ref noder p = ref _addr_p.val;

            ref constState cs = ref heap(out ptr<constState> _addr_cs);

            {
                var decl__prev1 = decl;

                foreach (var (_, __decl) in decls)
                {
                    decl = __decl;
                    p.setlineno(decl);
                    switch (decl.type())
                    {
                        case ptr<syntax.ImportDecl> decl:
                            p.importDecl(decl);
                            break;
                        case ptr<syntax.VarDecl> decl:
                            l = append(l, p.varDecl(decl));
                            break;
                        case ptr<syntax.ConstDecl> decl:
                            l = append(l, p.constDecl(decl, _addr_cs));
                            break;
                        case ptr<syntax.TypeDecl> decl:
                            l = append(l, p.typeDecl(decl));
                            break;
                        case ptr<syntax.FuncDecl> decl:
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

            return ;

        });

        private static void importDecl(this ptr<noder> _addr_p, ptr<syntax.ImportDecl> _addr_imp)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.ImportDecl imp = ref _addr_imp.val;

            if (imp.Path.Bad)
            {
                return ; // avoid follow-on errors if there was a syntax error
            }

            {
                ptr<Pragma> (pragma, ok) = imp.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    p.checkUnused(pragma);
                }

            }


            ref var val = ref heap(p.basicLit(imp.Path), out ptr<var> _addr_val);
            var ipkg = importfile(_addr_val);

            if (ipkg == null)
            {
                if (nerrors == 0L)
                {
                    Fatalf("phase error in import");
                }

                return ;

            }

            ipkg.Direct = true;

            ptr<types.Sym> my;
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
                    return ;
                    break;
                case "init": 
                    yyerrorl(pack.Pos, "cannot import package as init - init must be a func");
                    return ;
                    break;
                case "_": 
                    return ;
                    break;
            }
            if (my.Def != null)
            {
                redeclare(pack.Pos, my, "as imported package name");
            }

            my.Def = asTypesNode(pack);
            my.Lastlineno = pack.Pos;
            my.Block = 1L; // at top level
        }

        private static slice<ptr<Node>> varDecl(this ptr<noder> _addr_p, ptr<syntax.VarDecl> _addr_decl)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.VarDecl decl = ref _addr_decl.val;

            var names = p.declNames(decl.NameList);
            var typ = p.typeExprOrNil(decl.Type);

            slice<ptr<Node>> exprs = default;
            if (decl.Values != null)
            {
                exprs = p.exprList(decl.Values);
            }

            {
                ptr<Pragma> (pragma, ok) = decl.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    p.checkUnused(pragma);
                }

            }


            p.setlineno(decl);
            return variter(names, typ, exprs);

        }

        // constState tracks state between constant specifiers within a
        // declaration group. This state is kept separate from noder so nested
        // constant declarations are handled correctly (e.g., issue 15550).
        private partial struct constState
        {
            public ptr<syntax.Group> group;
            public ptr<Node> typ;
            public slice<ptr<Node>> values;
            public long iota;
        }

        private static slice<ptr<Node>> constDecl(this ptr<noder> _addr_p, ptr<syntax.ConstDecl> _addr_decl, ptr<constState> _addr_cs)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.ConstDecl decl = ref _addr_decl.val;
            ref constState cs = ref _addr_cs.val;

            if (decl.Group == null || decl.Group != cs.group)
            {
                cs = new constState(group:decl.Group,);
            }

            {
                ptr<Pragma> (pragma, ok) = decl.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    p.checkUnused(pragma);
                }

            }


            var names = p.declNames(decl.NameList);
            var typ = p.typeExprOrNil(decl.Type);

            slice<ptr<Node>> values = default;
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

            var nn = make_slice<ptr<Node>>(0L, len(names));
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

        private static ptr<Node> typeDecl(this ptr<noder> _addr_p, ptr<syntax.TypeDecl> _addr_decl)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.TypeDecl decl = ref _addr_decl.val;

            var n = p.declName(decl.Name);
            n.Op = OTYPE;
            declare(n, dclcontext); 

            // decl.Type may be nil but in that case we got a syntax error during parsing
            var typ = p.typeExprOrNil(decl.Type);

            var param = n.Name.Param;
            param.Ntype = typ;
            param.Alias = decl.Alias;
            {
                ptr<Pragma> (pragma, ok) = decl.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    if (!decl.Alias)
                    {
                        param.Pragma = pragma.Flag & TypePragmas;
                        pragma.Flag &= TypePragmas;
                    }

                    p.checkUnused(pragma);

                }

            }


            var nod = p.nod(decl, ODCLTYPE, n, null);
            if (param.Alias && !langSupported(1L, 9L, localpkg))
            {
                yyerrorl(nod.Pos, "type aliases only supported as of -lang=go1.9");
            }

            return _addr_nod!;

        }

        private static slice<ptr<Node>> declNames(this ptr<noder> _addr_p, slice<ptr<syntax.Name>> names)
        {
            ref noder p = ref _addr_p.val;

            var nodes = make_slice<ptr<Node>>(0L, len(names));
            foreach (var (_, name) in names)
            {
                nodes = append(nodes, p.declName(name));
            }
            return nodes;

        }

        private static ptr<Node> declName(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Name name = ref _addr_name.val;

            var n = dclname(p.name(name));
            n.Pos = p.pos(name);
            return _addr_n!;
        }

        private static ptr<Node> funcDecl(this ptr<noder> _addr_p, ptr<syntax.FuncDecl> _addr_fun)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.FuncDecl fun = ref _addr_fun.val;

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

            f.Func.Nname = newfuncnamel(p.pos(fun.Name), name);
            f.Func.Nname.Name.Defn = f;
            f.Func.Nname.Name.Param.Ntype = t;

            {
                ptr<Pragma> (pragma, ok) = fun.Pragma._<ptr<Pragma>>();

                if (ok)
                {
                    f.Func.Pragma = pragma.Flag & FuncPragmas;
                    if (pragma.Flag & Systemstack != 0L && pragma.Flag & Nosplit != 0L)
                    {
                        yyerrorl(f.Pos, "go:nosplit and go:systemstack cannot be combined");
                    }

                    pragma.Flag &= FuncPragmas;
                    p.checkUnused(pragma);

                }

            }


            if (fun.Recv == null)
            {
                declare(f.Func.Nname, PFUNC);
            }

            p.funcBody(f, fun.Body);

            if (fun.Body != null)
            {
                if (f.Func.Pragma & Noescape != 0L)
                {
                    yyerrorl(f.Pos, "can only use //go:noescape with external func implementations");
                }

            }
            else
            {
                if (pure_go || strings.HasPrefix(f.funcname(), "init."))
                { 
                    // Linknamed functions are allowed to have no body. Hopefully
                    // the linkname target has a body. See issue 23311.
                    var isLinknamed = false;
                    foreach (var (_, n) in p.linknames)
                    {
                        if (f.funcname() == n.local)
                        {
                            isLinknamed = true;
                            break;
                        }

                    }
                    if (!isLinknamed)
                    {
                        yyerrorl(f.Pos, "missing function body");
                    }

                }

            }

            return _addr_f!;

        }

        private static ptr<Node> signature(this ptr<noder> _addr_p, ptr<syntax.Field> _addr_recv, ptr<syntax.FuncType> _addr_typ)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Field recv = ref _addr_recv.val;
            ref syntax.FuncType typ = ref _addr_typ.val;

            var n = p.nod(typ, OTFUNC, null, null);
            if (recv != null)
            {
                n.Left = p.param(recv, false, false);
            }

            n.List.Set(p.@params(typ.ParamList, true));
            n.Rlist.Set(p.@params(typ.ResultList, false));
            return _addr_n!;

        }

        private static slice<ptr<Node>> @params(this ptr<noder> _addr_p, slice<ptr<syntax.Field>> @params, bool dddOk)
        {
            ref noder p = ref _addr_p.val;

            var nodes = make_slice<ptr<Node>>(0L, len(params));
            foreach (var (i, param) in params)
            {
                p.setlineno(param);
                nodes = append(nodes, p.param(param, dddOk, i + 1L == len(params)));
            }
            return nodes;

        }

        private static ptr<Node> param(this ptr<noder> _addr_p, ptr<syntax.Field> _addr_param, bool dddOk, bool final)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Field param = ref _addr_param.val;

            ptr<types.Sym> name;
            if (param.Name != null)
            {
                name = p.name(param.Name);
            }

            var typ = p.typeExpr(param.Type);
            var n = p.nodSym(param, ODCLFIELD, typ, name); 

            // rewrite ...T parameter
            if (typ.Op == ODDD)
            {
                if (!dddOk)
                { 
                    // We mark these as syntax errors to get automatic elimination
                    // of multiple such errors per line (see yyerrorl in subr.go).
                    yyerror("syntax error: cannot use ... in receiver or result parameter list");

                }
                else if (!final)
                {
                    if (param.Name == null)
                    {
                        yyerror("syntax error: cannot use ... with non-final parameter");
                    }
                    else
                    {
                        p.yyerrorpos(param.Name.Pos(), "syntax error: cannot use ... with non-final parameter %s", param.Name.Value);
                    }

                }

                typ.Op = OTARRAY;
                typ.Right = typ.Left;
                typ.Left = null;
                n.SetIsDDD(true);
                if (n.Left != null)
                {
                    n.Left.SetIsDDD(true);
                }

            }

            return _addr_n!;

        }

        private static slice<ptr<Node>> exprList(this ptr<noder> _addr_p, syntax.Expr expr)
        {
            ref noder p = ref _addr_p.val;

            {
                ptr<syntax.ListExpr> (list, ok) = expr._<ptr<syntax.ListExpr>>();

                if (ok)
                {
                    return p.exprs(list.ElemList);
                }

            }

            return new slice<ptr<Node>>(new ptr<Node>[] { p.expr(expr) });

        }

        private static slice<ptr<Node>> exprs(this ptr<noder> _addr_p, slice<syntax.Expr> exprs)
        {
            ref noder p = ref _addr_p.val;

            var nodes = make_slice<ptr<Node>>(0L, len(exprs));
            foreach (var (_, expr) in exprs)
            {
                nodes = append(nodes, p.expr(expr));
            }
            return nodes;

        }

        private static ptr<Node> expr(this ptr<noder> _addr_p, syntax.Expr expr) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            p.setlineno(expr);
            switch (expr.type())
            {
                case ptr<syntax.BadExpr> expr:
                    return _addr_null!;
                    break;
                case ptr<syntax.Name> expr:
                    return _addr_p.mkname(expr)!;
                    break;
                case ptr<syntax.BasicLit> expr:
                    var n = nodlit(p.basicLit(expr));
                    n.SetDiag(expr.Bad); // avoid follow-on errors if there was a syntax error
                    return _addr_n!;
                    break;
                case ptr<syntax.CompositeLit> expr:
                    n = p.nod(expr, OCOMPLIT, null, null);
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
                    lineno = p.makeXPos(expr.Rbrace);
                    return _addr_n!;
                    break;
                case ptr<syntax.KeyValueExpr> expr:
                    return _addr_p.nod(expr.Key, OKEY, p.expr(expr.Key), p.wrapname(expr.Value, p.expr(expr.Value)))!;
                    break;
                case ptr<syntax.FuncLit> expr:
                    return _addr_p.funcLit(expr)!;
                    break;
                case ptr<syntax.ParenExpr> expr:
                    return _addr_p.nod(expr, OPAREN, p.expr(expr.X), null)!;
                    break;
                case ptr<syntax.SelectorExpr> expr:
                    var obj = p.expr(expr.X);
                    if (obj.Op == OPACK)
                    {
                        obj.Name.SetUsed(true);
                        return _addr_oldname(restrictlookup(expr.Sel.Value, obj.Name.Pkg))!;
                    }

                    n = nodSym(OXDOT, obj, p.name(expr.Sel));
                    n.Pos = p.pos(expr); // lineno may have been changed by p.expr(expr.X)
                    return _addr_n!;
                    break;
                case ptr<syntax.IndexExpr> expr:
                    return _addr_p.nod(expr, OINDEX, p.expr(expr.X), p.expr(expr.Index))!;
                    break;
                case ptr<syntax.SliceExpr> expr:
                    var op = OSLICE;
                    if (expr.Full)
                    {
                        op = OSLICE3;
                    }

                    n = p.nod(expr, op, p.expr(expr.X), null);
                    array<ptr<Node>> index = new array<ptr<Node>>(3L);
                    {
                        var i__prev1 = i;
                        var x__prev1 = x;

                        foreach (var (__i, __x) in _addr_expr.Index)
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
                    return _addr_n!;
                    break;
                case ptr<syntax.AssertExpr> expr:
                    return _addr_p.nod(expr, ODOTTYPE, p.expr(expr.X), p.typeExpr(expr.Type))!;
                    break;
                case ptr<syntax.Operation> expr:
                    if (expr.Op == syntax.Add && expr.Y != null)
                    {
                        return _addr_p.sum(expr)!;
                    }

                    var x = p.expr(expr.X);
                    if (expr.Y == null)
                    {
                        return _addr_p.nod(expr, p.unOp(expr.Op), x, null)!;
                    }

                    return _addr_p.nod(expr, p.binOp(expr.Op), x, p.expr(expr.Y))!;
                    break;
                case ptr<syntax.CallExpr> expr:
                    n = p.nod(expr, OCALL, p.expr(expr.Fun), null);
                    n.List.Set(p.exprs(expr.ArgList));
                    n.SetIsDDD(expr.HasDots);
                    return _addr_n!;
                    break;
                case ptr<syntax.ArrayType> expr:
                    ptr<Node> len;
                    if (expr.Len != null)
                    {
                        len = p.expr(expr.Len);
                    }
                    else
                    {
                        len = p.nod(expr, ODDD, null, null);
                    }

                    return _addr_p.nod(expr, OTARRAY, len, p.typeExpr(expr.Elem))!;
                    break;
                case ptr<syntax.SliceType> expr:
                    return _addr_p.nod(expr, OTARRAY, null, p.typeExpr(expr.Elem))!;
                    break;
                case ptr<syntax.DotsType> expr:
                    return _addr_p.nod(expr, ODDD, p.typeExpr(expr.Elem), null)!;
                    break;
                case ptr<syntax.StructType> expr:
                    return _addr_p.structType(expr)!;
                    break;
                case ptr<syntax.InterfaceType> expr:
                    return _addr_p.interfaceType(expr)!;
                    break;
                case ptr<syntax.FuncType> expr:
                    return _addr_p.signature(null, expr)!;
                    break;
                case ptr<syntax.MapType> expr:
                    return _addr_p.nod(expr, OTMAP, p.typeExpr(expr.Key), p.typeExpr(expr.Value))!;
                    break;
                case ptr<syntax.ChanType> expr:
                    n = p.nod(expr, OTCHAN, p.typeExpr(expr.Elem), null);
                    n.SetTChanDir(p.chanDir(expr.Dir));
                    return _addr_n!;
                    break;
                case ptr<syntax.TypeSwitchGuard> expr:
                    n = p.nod(expr, OTYPESW, null, p.expr(expr.X));
                    if (expr.Lhs != null)
                    {
                        n.Left = p.declName(expr.Lhs);
                        if (n.Left.isBlank())
                        {
                            yyerror("invalid variable name %v in type switch", n.Left);
                        }

                    }

                    return _addr_n!;
                    break;
            }
            panic("unhandled Expr");

        });

        // sum efficiently handles very large summation expressions (such as
        // in issue #16394). In particular, it avoids left recursion and
        // collapses string literals.
        private static ptr<Node> sum(this ptr<noder> _addr_p, syntax.Expr x)
        {
            ref noder p = ref _addr_p.val;
 
            // While we need to handle long sums with asymptotic
            // efficiency, the vast majority of sums are very small: ~95%
            // have only 2 or 3 operands, and ~99% of string literals are
            // never concatenated.

            var adds = make_slice<ptr<syntax.Operation>>(0L, 2L);
            while (true)
            {
                ptr<syntax.Operation> (add, ok) = x._<ptr<syntax.Operation>>();
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
            ptr<Node> nstr;
            var chunks = make_slice<@string>(0L, 1L);

            var n = p.expr(x);
            if (Isconst(n, CTSTR) && n.Sym == null)
            {
                nstr = n;
                chunks = append(chunks, strlit(nstr));
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
                        chunks = append(chunks, strlit(r));
                        continue;

                    }

                    nstr = r;
                    chunks = append(chunks, strlit(nstr));

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

            return _addr_n!;

        }

        private static ptr<Node> typeExpr(this ptr<noder> _addr_p, syntax.Expr typ)
        {
            ref noder p = ref _addr_p.val;
 
            // TODO(mdempsky): Be stricter? typecheck should handle errors anyway.
            return _addr_p.expr(typ)!;

        }

        private static ptr<Node> typeExprOrNil(this ptr<noder> _addr_p, syntax.Expr typ)
        {
            ref noder p = ref _addr_p.val;

            if (typ != null)
            {
                return _addr_p.expr(typ)!;
            }

            return _addr_null!;

        }

        private static types.ChanDir chanDir(this ptr<noder> _addr_p, syntax.ChanDir dir) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;


            if (dir == 0L) 
                return types.Cboth;
            else if (dir == syntax.SendOnly) 
                return types.Csend;
            else if (dir == syntax.RecvOnly) 
                return types.Crecv;
                        panic("unhandled ChanDir");

        });

        private static ptr<Node> structType(this ptr<noder> _addr_p, ptr<syntax.StructType> _addr_expr)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.StructType expr = ref _addr_expr.val;

            var l = make_slice<ptr<Node>>(0L, len(expr.FieldList));
            foreach (var (i, field) in expr.FieldList)
            {
                p.setlineno(field);
                ptr<Node> n;
                if (field.Name == null)
                {
                    n = p.embedded(field.Type);
                }
                else
                {
                    n = p.nodSym(field, ODCLFIELD, p.typeExpr(field.Type), p.name(field.Name));
                }

                if (i < len(expr.TagList) && expr.TagList[i] != null)
                {
                    n.SetVal(p.basicLit(expr.TagList[i]));
                }

                l = append(l, n);

            }
            p.setlineno(expr);
            n = p.nod(expr, OTSTRUCT, null, null);
            n.List.Set(l);
            return _addr_n!;

        }

        private static ptr<Node> interfaceType(this ptr<noder> _addr_p, ptr<syntax.InterfaceType> _addr_expr)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.InterfaceType expr = ref _addr_expr.val;

            var l = make_slice<ptr<Node>>(0L, len(expr.MethodList));
            foreach (var (_, method) in expr.MethodList)
            {
                p.setlineno(method);
                ptr<Node> n;
                if (method.Name == null)
                {
                    n = p.nodSym(method, ODCLFIELD, oldname(p.packname(method.Type)), null);
                }
                else
                {
                    var mname = p.name(method.Name);
                    var sig = p.typeExpr(method.Type);
                    sig.Left = fakeRecv();
                    n = p.nodSym(method, ODCLFIELD, sig, mname);
                    ifacedcl(n);
                }

                l = append(l, n);

            }
            n = p.nod(expr, OTINTER, null, null);
            n.List.Set(l);
            return _addr_n!;

        }

        private static ptr<types.Sym> packname(this ptr<noder> _addr_p, syntax.Expr expr) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            switch (expr.type())
            {
                case ptr<syntax.Name> expr:
                    var name = p.name(expr);
                    {
                        var n = oldname(name);

                        if (n.Name != null && n.Name.Pack != null)
                        {
                            n.Name.Pack.Name.SetUsed(true);
                        }

                    }

                    return _addr_name!;
                    break;
                case ptr<syntax.SelectorExpr> expr:
                    name = p.name(expr.X._<ptr<syntax.Name>>());
                    var def = asNode(name.Def);
                    if (def == null)
                    {
                        yyerror("undefined: %v", name);
                        return _addr_name!;
                    }

                    ptr<types.Pkg> pkg;
                    if (def.Op != OPACK)
                    {
                        yyerror("%v is not a package", name);
                        pkg = localpkg;
                    }
                    else
                    {
                        def.Name.SetUsed(true);
                        pkg = def.Name.Pkg;
                    }

                    return _addr_restrictlookup(expr.Sel.Value, pkg)!;
                    break;
            }
            panic(fmt.Sprintf("unexpected packname: %#v", expr));

        });

        private static ptr<Node> embedded(this ptr<noder> _addr_p, syntax.Expr typ) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            ptr<syntax.Operation> (op, isStar) = typ._<ptr<syntax.Operation>>();
            if (isStar)
            {
                if (op.Op != syntax.Mul || op.Y != null)
                {
                    panic("unexpected Operation");
                }

                typ = op.X;

            }

            var sym = p.packname(typ);
            var n = p.nodSym(typ, ODCLFIELD, oldname(sym), lookup(sym.Name));
            n.SetEmbedded(true);

            if (isStar)
            {
                n.Left = p.nod(op, ODEREF, n.Left, null);
            }

            return _addr_n!;

        });

        private static slice<ptr<Node>> stmts(this ptr<noder> _addr_p, slice<syntax.Stmt> stmts)
        {
            ref noder p = ref _addr_p.val;

            return p.stmtsFall(stmts, false);
        }

        private static slice<ptr<Node>> stmtsFall(this ptr<noder> _addr_p, slice<syntax.Stmt> stmts, bool fallOK)
        {
            ref noder p = ref _addr_p.val;

            slice<ptr<Node>> nodes = default;
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

        private static ptr<Node> stmt(this ptr<noder> _addr_p, syntax.Stmt stmt)
        {
            ref noder p = ref _addr_p.val;

            return _addr_p.stmtFall(stmt, false)!;
        }

        private static ptr<Node> stmtFall(this ptr<noder> _addr_p, syntax.Stmt stmt, bool fallOK) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            p.setlineno(stmt);
            switch (stmt.type())
            {
                case ptr<syntax.EmptyStmt> stmt:
                    return _addr_null!;
                    break;
                case ptr<syntax.LabeledStmt> stmt:
                    return _addr_p.labeledStmt(stmt, fallOK)!;
                    break;
                case ptr<syntax.BlockStmt> stmt:
                    var l = p.blockStmt(stmt);
                    if (len(l) == 0L)
                    { 
                        // TODO(mdempsky): Line number?
                        return _addr_nod(OEMPTY, null, null)!;

                    }

                    return _addr_liststmt(l)!;
                    break;
                case ptr<syntax.ExprStmt> stmt:
                    return _addr_p.wrapname(stmt, p.expr(stmt.X))!;
                    break;
                case ptr<syntax.SendStmt> stmt:
                    return _addr_p.nod(stmt, OSEND, p.expr(stmt.Chan), p.expr(stmt.Value))!;
                    break;
                case ptr<syntax.DeclStmt> stmt:
                    return _addr_liststmt(p.decls(stmt.DeclList))!;
                    break;
                case ptr<syntax.AssignStmt> stmt:
                    if (stmt.Op != 0L && stmt.Op != syntax.Def)
                    {
                        var n = p.nod(stmt, OASOP, p.expr(stmt.Lhs), p.expr(stmt.Rhs));
                        n.SetImplicit(stmt.Rhs == syntax.ImplicitOne);
                        n.SetSubOp(p.binOp(stmt.Op));
                        return _addr_n!;
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

                    return _addr_n!;
                    break;
                case ptr<syntax.BranchStmt> stmt:
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
                        n.Sym = p.name(stmt.Label);
                    }

                    return _addr_n!;
                    break;
                case ptr<syntax.CallStmt> stmt:
                    op = default;

                    if (stmt.Tok == syntax.Defer) 
                        op = ODEFER;
                    else if (stmt.Tok == syntax.Go) 
                        op = OGO;
                    else 
                        panic("unhandled CallStmt");
                                        return _addr_p.nod(stmt, op, p.expr(stmt.Call), null)!;
                    break;
                case ptr<syntax.ReturnStmt> stmt:
                    slice<ptr<Node>> results = default;
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

                    return _addr_n!;
                    break;
                case ptr<syntax.IfStmt> stmt:
                    return _addr_p.ifStmt(stmt)!;
                    break;
                case ptr<syntax.ForStmt> stmt:
                    return _addr_p.forStmt(stmt)!;
                    break;
                case ptr<syntax.SwitchStmt> stmt:
                    return _addr_p.switchStmt(stmt)!;
                    break;
                case ptr<syntax.SelectStmt> stmt:
                    return _addr_p.selectStmt(stmt)!;
                    break;
            }
            panic("unhandled Stmt");

        });

        private static slice<ptr<Node>> assignList(this ptr<noder> _addr_p, syntax.Expr expr, ptr<Node> _addr_defn, bool colas)
        {
            ref noder p = ref _addr_p.val;
            ref Node defn = ref _addr_defn.val;

            if (!colas)
            {
                return p.exprList(expr);
            }

            defn.SetColas(true);

            slice<syntax.Expr> exprs = default;
            {
                ptr<syntax.ListExpr> (list, ok) = expr._<ptr<syntax.ListExpr>>();

                if (ok)
                {
                    exprs = list.ElemList;
                }
                else
                {
                    exprs = new slice<syntax.Expr>(new syntax.Expr[] { expr });
                }

            }


            var res = make_slice<ptr<Node>>(len(exprs));
            var seen = make_map<ptr<types.Sym>, bool>(len(exprs));

            var newOrErr = false;
            foreach (var (i, expr) in exprs)
            {
                p.setlineno(expr);
                res[i] = nblank;

                ptr<syntax.Name> (name, ok) = expr._<ptr<syntax.Name>>();
                if (!ok)
                {
                    p.yyerrorpos(expr.Pos(), "non-name %v on left side of :=", p.expr(expr));
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
                    p.yyerrorpos(expr.Pos(), "%v repeated on left side of :=", sym);
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

        private static slice<ptr<Node>> blockStmt(this ptr<noder> _addr_p, ptr<syntax.BlockStmt> _addr_stmt)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.BlockStmt stmt = ref _addr_stmt.val;

            p.openScope(stmt.Pos());
            var nodes = p.stmts(stmt.List);
            p.closeScope(stmt.Rbrace);
            return nodes;
        }

        private static ptr<Node> ifStmt(this ptr<noder> _addr_p, ptr<syntax.IfStmt> _addr_stmt)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.IfStmt stmt = ref _addr_stmt.val;

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
            return _addr_n!;

        }

        private static ptr<Node> forStmt(this ptr<noder> _addr_p, ptr<syntax.ForStmt> _addr_stmt) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;
            ref syntax.ForStmt stmt = ref _addr_stmt.val;

            p.openScope(stmt.Pos());
            ptr<Node> n;
            {
                ptr<syntax.RangeClause> (r, ok) = stmt.Init._<ptr<syntax.RangeClause>>();

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
            return _addr_n!;

        });

        private static ptr<Node> switchStmt(this ptr<noder> _addr_p, ptr<syntax.SwitchStmt> _addr_stmt)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.SwitchStmt stmt = ref _addr_stmt.val;

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
            return _addr_n!;

        }

        private static slice<ptr<Node>> caseClauses(this ptr<noder> _addr_p, slice<ptr<syntax.CaseClause>> clauses, ptr<Node> _addr_tswitch, syntax.Pos rbrace)
        {
            ref noder p = ref _addr_p.val;
            ref Node tswitch = ref _addr_tswitch.val;

            var nodes = make_slice<ptr<Node>>(0L, len(clauses));
            foreach (var (i, clause) in clauses)
            {
                p.setlineno(clause);
                if (i > 0L)
                {
                    p.closeScope(clause.Pos());
                }

                p.openScope(clause.Pos());

                var n = p.nod(clause, OCASE, null, null);
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
                        ptr<syntax.EmptyStmt> (_, ok) = body[len(body) - 1L]._<ptr<syntax.EmptyStmt>>();

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

        private static ptr<Node> selectStmt(this ptr<noder> _addr_p, ptr<syntax.SelectStmt> _addr_stmt)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.SelectStmt stmt = ref _addr_stmt.val;

            var n = p.nod(stmt, OSELECT, null, null);
            n.List.Set(p.commClauses(stmt.Body, stmt.Rbrace));
            return _addr_n!;
        }

        private static slice<ptr<Node>> commClauses(this ptr<noder> _addr_p, slice<ptr<syntax.CommClause>> clauses, syntax.Pos rbrace)
        {
            ref noder p = ref _addr_p.val;

            var nodes = make_slice<ptr<Node>>(0L, len(clauses));
            foreach (var (i, clause) in clauses)
            {
                p.setlineno(clause);
                if (i > 0L)
                {
                    p.closeScope(clause.Pos());
                }

                p.openScope(clause.Pos());

                var n = p.nod(clause, OCASE, null, null);
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

        private static ptr<Node> labeledStmt(this ptr<noder> _addr_p, ptr<syntax.LabeledStmt> _addr_label, bool fallOK)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.LabeledStmt label = ref _addr_label.val;

            var lhs = p.nodSym(label, OLABEL, null, p.name(label.Label));

            ptr<Node> ls;
            if (label.Stmt != null)
            { // TODO(mdempsky): Should always be present.
                ls = p.stmtFall(label.Stmt, fallOK);

            }

            lhs.Name.Defn = ls;
            ptr<Node> l = new slice<ptr<Node>>(new ptr<Node>[] { lhs });
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

            return _addr_liststmt(l)!;

        }

        private static array<Op> unOps = new array<Op>(InitKeyedValues<Op>((syntax.Recv, ORECV), (syntax.Mul, ODEREF), (syntax.And, OADDR), (syntax.Not, ONOT), (syntax.Xor, OBITNOT), (syntax.Add, OPLUS), (syntax.Sub, ONEG)));

        private static Op unOp(this ptr<noder> _addr_p, syntax.Operator op) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            if (uint64(op) >= uint64(len(unOps)) || unOps[op] == 0L)
            {
                panic("invalid Operator");
            }

            return unOps[op];

        });

        private static array<Op> binOps = new array<Op>(InitKeyedValues<Op>((syntax.OrOr, OOROR), (syntax.AndAnd, OANDAND), (syntax.Eql, OEQ), (syntax.Neq, ONE), (syntax.Lss, OLT), (syntax.Leq, OLE), (syntax.Gtr, OGT), (syntax.Geq, OGE), (syntax.Add, OADD), (syntax.Sub, OSUB), (syntax.Or, OOR), (syntax.Xor, OXOR), (syntax.Mul, OMUL), (syntax.Div, ODIV), (syntax.Rem, OMOD), (syntax.And, OAND), (syntax.AndNot, OANDNOT), (syntax.Shl, OLSH), (syntax.Shr, ORSH)));

        private static Op binOp(this ptr<noder> _addr_p, syntax.Operator op) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            if (uint64(op) >= uint64(len(binOps)) || binOps[op] == 0L)
            {
                panic("invalid Operator");
            }

            return binOps[op];

        });

        // checkLangCompat reports an error if the representation of a numeric
        // literal is not compatible with the current language version.
        private static void checkLangCompat(ptr<syntax.BasicLit> _addr_lit)
        {
            ref syntax.BasicLit lit = ref _addr_lit.val;

            var s = lit.Value;
            if (len(s) <= 2L || langSupported(1L, 13L, localpkg))
            {
                return ;
            } 
            // len(s) > 2
            if (strings.Contains(s, "_"))
            {
                yyerrorv("go1.13", "underscores in numeric literals");
                return ;
            }

            if (s[0L] != '0')
            {
                return ;
            }

            var @base = s[1L];
            if (base == 'b' || base == 'B')
            {
                yyerrorv("go1.13", "binary literals");
                return ;
            }

            if (base == 'o' || base == 'O')
            {
                yyerrorv("go1.13", "0o/0O-style octal literals");
                return ;
            }

            if (lit.Kind != syntax.IntLit && (base == 'x' || base == 'X'))
            {
                yyerrorv("go1.13", "hexadecimal floating-point literals");
            }

        }

        private static Val basicLit(this ptr<noder> _addr_p, ptr<syntax.BasicLit> _addr_lit) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;
            ref syntax.BasicLit lit = ref _addr_lit.val;
 
            // We don't use the errors of the conversion routines to determine
            // if a literal string is valid because the conversion routines may
            // accept a wider syntax than the language permits. Rely on lit.Bad
            // instead.
            {
                var s = lit.Value;


                if (lit.Kind == syntax.IntLit) 
                    checkLangCompat(_addr_lit);
                    ptr<object> x = @new<Mpint>();
                    if (!lit.Bad)
                    {
                        x.SetString(s);
                    }

                    return new Val(U:x);
                else if (lit.Kind == syntax.FloatLit) 
                    checkLangCompat(_addr_lit);
                    x = newMpflt();
                    if (!lit.Bad)
                    {
                        x.SetString(s);
                    }

                    return new Val(U:x);
                else if (lit.Kind == syntax.ImagLit) 
                    checkLangCompat(_addr_lit);
                    x = newMpcmplx();
                    if (!lit.Bad)
                    {
                        x.Imag.SetString(strings.TrimSuffix(s, "i"));
                    }

                    return new Val(U:x);
                else if (lit.Kind == syntax.RuneLit) 
                    x = @new<Mpint>();
                    x.Rune = true;
                    if (!lit.Bad)
                    {
                        var (u, _) = strconv.Unquote(s);
                        int r = default;
                        if (len(u) == 1L)
                        {
                            r = rune(u[0L]);
                        }
                        else
                        {
                            r, _ = utf8.DecodeRuneInString(u);
                        }

                        x.SetInt64(int64(r));

                    }

                    return new Val(U:x);
                else if (lit.Kind == syntax.StringLit) 
                    x = default;
                    if (!lit.Bad)
                    {
                        if (len(s) > 0L && s[0L] == '`')
                        { 
                            // strip carriage returns from raw string
                            s = strings.Replace(s, "\r", "", -1L);

                        }

                        x, _ = strconv.Unquote(s);

                    }

                    return new Val(U:x);
                else 
                    panic("unhandled BasicLit kind");

            }

        });

        private static ptr<types.Sym> name(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Name name = ref _addr_name.val;

            return _addr_lookup(name.Value)!;
        }

        private static ptr<Node> mkname(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Name name = ref _addr_name.val;
 
            // TODO(mdempsky): Set line number?
            return _addr_mkname(_addr_p.name(name))!;

        }

        private static ptr<Node> newname(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.Name name = ref _addr_name.val;
 
            // TODO(mdempsky): Set line number?
            return _addr_newname(p.name(name))!;

        }

        private static ptr<Node> wrapname(this ptr<noder> _addr_p, syntax.Node n, ptr<Node> _addr_x)
        {
            ref noder p = ref _addr_p.val;
            ref Node x = ref _addr_x.val;
 
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
            return _addr_x!;

        }

        private static ptr<Node> nod(this ptr<noder> _addr_p, syntax.Node orig, Op op, ptr<Node> _addr_left, ptr<Node> _addr_right)
        {
            ref noder p = ref _addr_p.val;
            ref Node left = ref _addr_left.val;
            ref Node right = ref _addr_right.val;

            return _addr_nodl(p.pos(orig), op, left, right)!;
        }

        private static ptr<Node> nodSym(this ptr<noder> _addr_p, syntax.Node orig, Op op, ptr<Node> _addr_left, ptr<types.Sym> _addr_sym)
        {
            ref noder p = ref _addr_p.val;
            ref Node left = ref _addr_left.val;
            ref types.Sym sym = ref _addr_sym.val;

            var n = nodSym(op, left, sym);
            n.Pos = p.pos(orig);
            return _addr_n!;
        }

        private static src.XPos pos(this ptr<noder> _addr_p, syntax.Node n)
        {
            ref noder p = ref _addr_p.val;
 
            // TODO(gri): orig.Pos() should always be known - fix package syntax
            var xpos = lineno;
            {
                var pos = n.Pos();

                if (pos.IsKnown())
                {
                    xpos = p.makeXPos(pos);
                }

            }

            return xpos;

        }

        private static void setlineno(this ptr<noder> _addr_p, syntax.Node n)
        {
            ref noder p = ref _addr_p.val;

            if (n != null)
            {
                lineno = p.pos(n);
            }

        }

        // error is called concurrently if files are parsed concurrently.
        private static void error(this ptr<noder> _addr_p, error err)
        {
            ref noder p = ref _addr_p.val;

            p.err.Send(err._<syntax.Error>());
        }

        // pragmas that are allowed in the std lib, but don't have
        // a syntax.Pragma value (see lex.go) associated with them.
        private static map allowedStdPragmas = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"go:cgo_export_static":true,"go:cgo_export_dynamic":true,"go:cgo_import_static":true,"go:cgo_import_dynamic":true,"go:cgo_ldflag":true,"go:cgo_dynamic_linker":true,"go:generate":true,};

        // *Pragma is the value stored in a syntax.Pragma during parsing.
        public partial struct Pragma
        {
            public PragmaFlag Flag; // collected bits
            public slice<PragmaPos> Pos; // position of each individual flag
        }

        public partial struct PragmaPos
        {
            public PragmaFlag Flag;
            public syntax.Pos Pos;
        }

        private static void checkUnused(this ptr<noder> _addr_p, ptr<Pragma> _addr_pragma)
        {
            ref noder p = ref _addr_p.val;
            ref Pragma pragma = ref _addr_pragma.val;

            foreach (var (_, pos) in pragma.Pos)
            {
                if (pos.Flag & pragma.Flag != 0L)
                {
                    p.yyerrorpos(pos.Pos, "misplaced compiler directive");
                }

            }

        }

        private static void checkUnusedDuringParse(this ptr<noder> _addr_p, ptr<Pragma> _addr_pragma)
        {
            ref noder p = ref _addr_p.val;
            ref Pragma pragma = ref _addr_pragma.val;

            foreach (var (_, pos) in pragma.Pos)
            {
                if (pos.Flag & pragma.Flag != 0L)
                {
                    p.error(new syntax.Error(Pos:pos.Pos,Msg:"misplaced compiler directive"));
                }

            }

        }

        // pragma is called concurrently if files are parsed concurrently.
        private static syntax.Pragma pragma(this ptr<noder> _addr_p, syntax.Pos pos, bool blankLine, @string text, syntax.Pragma old) => func((_, panic, __) =>
        {
            ref noder p = ref _addr_p.val;

            ptr<Pragma> (pragma, _) = old._<ptr<Pragma>>();
            if (pragma == null)
            {
                pragma = @new<Pragma>();
            }

            if (text == "")
            { 
                // unused pragma; only called with old != nil.
                p.checkUnusedDuringParse(pragma);
                return null;

            }

            if (strings.HasPrefix(text, "line "))
            { 
                // line directives are handled by syntax package
                panic("unreachable");

            }

            if (!blankLine)
            { 
                // directive must be on line by itself
                p.error(new syntax.Error(Pos:pos,Msg:"misplaced compiler directive"));
                return pragma;

            }


            if (strings.HasPrefix(text, "go:linkname "))
            {
                var f = strings.Fields(text);
                if (!(2L <= len(f) && len(f) <= 3L))
                {
                    p.error(new syntax.Error(Pos:pos,Msg:"usage: //go:linkname localname [linkname]"));
                    break;
                } 
                // The second argument is optional. If omitted, we use
                // the default object symbol name for this and
                // linkname only serves to mark this symbol as
                // something that may be referenced via the object
                // symbol name from another package.
                @string target = default;
                if (len(f) == 3L)
                {
                    target = f[2L];
                }

                p.linknames = append(p.linknames, new linkname(pos,f[1],target));
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

                    p.pragcgo(pos, text);
                    pragma.Flag |= pragmaFlag("go:cgo_import_dynamic");
                    break;

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

                p.pragcgo(pos, text);
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

                var flag = pragmaFlag(verb);
                const var runtimePragmas = Systemstack | Nowritebarrier | Nowritebarrierrec | Yeswritebarrierrec;

                if (!compiling_runtime && flag & runtimePragmas != 0L)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s only allowed in runtime",verb)));
                }

                if (flag == 0L && !allowedStdPragmas[verb] && compiling_std)
                {
                    p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s is not allowed in the standard library",verb)));
                }

                pragma.Flag |= flag;
                pragma.Pos = append(pragma.Pos, new PragmaPos(flag,pos));

            __switch_break1:;

            return pragma;

        });

        // isCgoGeneratedFile reports whether pos is in a file
        // generated by cgo, which is to say a file with name
        // beginning with "_cgo_". Such files are allowed to
        // contain cgo directives, and for security reasons
        // (primarily misuse of linker flags), other files are not.
        // See golang.org/issue/23672.
        private static bool isCgoGeneratedFile(syntax.Pos pos)
        {
            return strings.HasPrefix(filepath.Base(filepath.Clean(fileh(pos.Base().Filename()))), "_cgo_");
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

        private static ptr<Node> mkname(ptr<types.Sym> _addr_sym)
        {
            ref types.Sym sym = ref _addr_sym.val;

            var n = oldname(sym);
            if (n.Name != null && n.Name.Pack != null)
            {
                n.Name.Pack.Name.SetUsed(true);
            }

            return _addr_n!;

        }

        private static ptr<Node> unparen(ptr<Node> _addr_x)
        {
            ref Node x = ref _addr_x.val;

            while (x.Op == OPAREN)
            {
                x = x.Left;
            }

            return _addr_x!;

        }
    }
}}}}
