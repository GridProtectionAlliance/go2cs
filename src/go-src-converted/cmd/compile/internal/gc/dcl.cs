// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:26:40 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\dcl.go
using bytes = go.bytes_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // Declaration stack & operations
        private static slice<ref Node> externdcl = default;

        private static void testdclstack()
        {
            if (!types.IsDclstackValid())
            {
                if (nerrors != 0L)
                {
                    errorexit();
                }
                Fatalf("mark left on the dclstack");
            }
        }

        // redeclare emits a diagnostic about symbol s being redeclared somewhere.
        private static void redeclare(ref types.Sym s, @string where)
        {
            if (!s.Lastlineno.IsKnown())
            {
                @string tmp = default;
                if (s.Origpkg != null)
                {
                    tmp = s.Origpkg.Path;
                }
                else
                {
                    tmp = s.Pkg.Path;
                }
                var pkgstr = tmp;
                yyerror("%v redeclared %s\n" + "\tprevious declaration during import %q", s, where, pkgstr);
            }
            else
            {
                var line1 = lineno;
                var line2 = s.Lastlineno; 

                // When an import and a declaration collide in separate files,
                // present the import as the "redeclared", because the declaration
                // is visible where the import is, but not vice versa.
                // See issue 4510.
                if (s.Def == null)
                {
                    line2 = line1;
                    line1 = s.Lastlineno;
                }
                yyerrorl(line1, "%v redeclared %s\n" + "\tprevious declaration at %v", s, where, linestr(line2));
            }
        }

        private static long vargen = default;

        // declare individual names - var, typ, const

        private static long declare_typegen = default;

        // declare records that Node n declares symbol n.Sym in the specified
        // declaration context.
        private static void declare(ref Node n, Class ctxt)
        {
            if (ctxt == PDISCARD)
            {
                return;
            }
            if (isblank(n))
            {
                return;
            }
            if (n.Name == null)
            { 
                // named OLITERAL needs Name; most OLITERALs don't.
                n.Name = @new<Name>();
            }
            n.Pos = lineno;
            var s = n.Sym; 

            // kludgy: typecheckok means we're past parsing. Eg genwrapper may declare out of package names later.
            if (!inimport && !typecheckok && s.Pkg != localpkg)
            {
                yyerror("cannot declare name %v", s);
            }
            long gen = 0L;
            if (ctxt == PEXTERN)
            {
                if (s.Name == "init")
                {
                    yyerror("cannot declare init - must be func");
                }
                if (s.Name == "main" && localpkg.Name == "main")
                {
                    yyerror("cannot declare main - must be func");
                }
                externdcl = append(externdcl, n);
            }
            else
            {
                if (Curfn == null && ctxt == PAUTO)
                {
                    Fatalf("automatic outside function");
                }
                if (Curfn != null)
                {
                    Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
                }
                if (n.Op == OTYPE)
                {
                    declare_typegen++;
                    gen = declare_typegen;
                }
                else if (n.Op == ONAME && ctxt == PAUTO && !strings.Contains(s.Name, "·"))
                {
                    vargen++;
                    gen = vargen;
                }
                types.Pushdcl(s);
                n.Name.Curfn = Curfn;
            }
            if (ctxt == PAUTO)
            {
                n.Xoffset = 0L;
            }
            if (s.Block == types.Block)
            { 
                // functype will print errors about duplicate function arguments.
                // Don't repeat the error here.
                if (ctxt != PPARAM && ctxt != PPARAMOUT)
                {
                    redeclare(s, "in this block");
                }
            }
            s.Block = types.Block;
            s.Lastlineno = lineno;
            s.Def = asTypesNode(n);
            n.Name.Vargen = int32(gen);
            n.Name.Funcdepth = funcdepth;
            n.SetClass(ctxt);

            autoexport(n, ctxt);
        }

        private static void addvar(ref Node n, ref types.Type t, Class ctxt)
        {
            if (n == null || n.Sym == null || (n.Op != ONAME && n.Op != ONONAME) || t == null)
            {
                Fatalf("addvar: n=%v t=%v nil", n, t);
            }
            n.Op = ONAME;
            declare(n, ctxt);
            n.Type = t;
        }

        // declare variables from grammar
        // new_name_list (type | [type] = expr_list)
        private static slice<ref Node> variter(slice<ref Node> vl, ref Node t, slice<ref Node> el)
        {
            slice<ref Node> init = default;
            var doexpr = len(el) > 0L;

            if (len(el) == 1L && len(vl) > 1L)
            {
                var e = el[0L];
                var as2 = nod(OAS2, null, null);
                as2.List.Set(vl);
                as2.Rlist.Set1(e);
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in vl)
                    {
                        v = __v;
                        v.Op = ONAME;
                        declare(v, dclcontext);
                        v.Name.Param.Ntype = t;
                        v.Name.Defn = as2;
                        if (funcdepth > 0L)
                        {
                            init = append(init, nod(ODCL, v, null));
                        }
                    }

                    v = v__prev1;
                }

                return append(init, as2);
            }
            {
                var v__prev1 = v;

                foreach (var (_, __v) in vl)
                {
                    v = __v;
                    e = default;
                    if (doexpr)
                    {
                        if (len(el) == 0L)
                        {
                            yyerror("missing expression in var declaration");
                            break;
                        }
                        e = el[0L];
                        el = el[1L..];
                    }
                    v.Op = ONAME;
                    declare(v, dclcontext);
                    v.Name.Param.Ntype = t;

                    if (e != null || funcdepth > 0L || isblank(v))
                    {
                        if (funcdepth > 0L)
                        {
                            init = append(init, nod(ODCL, v, null));
                        }
                        e = nod(OAS, v, e);
                        init = append(init, e);
                        if (e.Right != null)
                        {
                            v.Name.Defn = e;
                        }
                    }
                }

                v = v__prev1;
            }

            if (len(el) != 0L)
            {
                yyerror("extra expression in var declaration");
            }
            return init;
        }

        // newnoname returns a new ONONAME Node associated with symbol s.
        private static ref Node newnoname(ref types.Sym s)
        {
            if (s == null)
            {
                Fatalf("newnoname nil");
            }
            var n = nod(ONONAME, null, null);
            n.Sym = s;
            n.SetAddable(true);
            n.Xoffset = 0L;
            return n;
        }

        // newfuncname generates a new name node for a function or method.
        // TODO(rsc): Use an ODCLFUNC node instead. See comment in CL 7360.
        private static ref Node newfuncname(ref types.Sym s)
        {
            return newfuncnamel(lineno, s);
        }

        // newfuncnamel generates a new name node for a function or method.
        // TODO(rsc): Use an ODCLFUNC node instead. See comment in CL 7360.
        private static ref Node newfuncnamel(src.XPos pos, ref types.Sym s)
        {
            var n = newnamel(pos, s);
            n.Func = @new<Func>();
            n.Func.SetIsHiddenClosure(Curfn != null);
            return n;
        }

        // this generates a new name node for a name
        // being declared.
        private static ref Node dclname(ref types.Sym s)
        {
            var n = newname(s);
            n.Op = ONONAME; // caller will correct it
            return n;
        }

        private static ref Node typenod(ref types.Type t)
        {
            return typenodl(src.NoXPos, t);
        }

        private static ref Node typenodl(src.XPos pos, ref types.Type t)
        { 
            // if we copied another type with *t = *u
            // then t->nod might be out of date, so
            // check t->nod->type too
            if (asNode(t.Nod) == null || asNode(t.Nod).Type != t)
            {
                t.Nod = asTypesNode(nodl(pos, OTYPE, null, null));
                asNode(t.Nod).Type;

                t;
                asNode(t.Nod).Sym;

                t.Sym;
            }
            return asNode(t.Nod);
        }

        private static ref Node anonfield(ref types.Type typ)
        {
            return nod(ODCLFIELD, null, typenod(typ));
        }

        private static ref Node namedfield(@string s, ref types.Type typ)
        {
            return symfield(lookup(s), typ);
        }

        private static ref Node symfield(ref types.Sym s, ref types.Type typ)
        {
            return nod(ODCLFIELD, newname(s), typenod(typ));
        }

        // oldname returns the Node that declares symbol s in the current scope.
        // If no such Node currently exists, an ONONAME Node is returned instead.
        private static ref Node oldname(ref types.Sym s)
        {
            var n = asNode(s.Def);
            if (n == null)
            { 
                // Maybe a top-level declaration will come along later to
                // define s. resolve will check s.Def again once all input
                // source has been processed.
                return newnoname(s);
            }
            if (Curfn != null && n.Op == ONAME && n.Name.Funcdepth > 0L && n.Name.Funcdepth != funcdepth)
            { 
                // Inner func is referring to var in outer func.
                //
                // TODO(rsc): If there is an outer variable x and we
                // are parsing x := 5 inside the closure, until we get to
                // the := it looks like a reference to the outer x so we'll
                // make x a closure variable unnecessarily.
                var c = n.Name.Param.Innermost;
                if (c == null || c.Name.Funcdepth != funcdepth)
                { 
                    // Do not have a closure var for the active closure yet; make one.
                    c = newname(s);
                    c.SetClass(PAUTOHEAP);
                    c.SetIsClosureVar(true);
                    c.SetIsddd(n.Isddd());
                    c.Name.Defn = n;
                    c.SetAddable(false);
                    c.Name.Funcdepth = funcdepth; 

                    // Link into list of active closure variables.
                    // Popped from list in func closurebody.
                    c.Name.Param.Outer = n.Name.Param.Innermost;
                    n.Name.Param.Innermost = c;

                    Curfn.Func.Cvars.Append(c);
                } 

                // return ref to closure var, not original
                return c;
            }
            return n;
        }

        // := declarations
        private static bool colasname(ref Node n)
        {

            if (n.Op == ONAME || n.Op == ONONAME || n.Op == OPACK || n.Op == OTYPE || n.Op == OLITERAL) 
                return n.Sym != null;
                        return false;
        }

        private static void colasdefn(slice<ref Node> left, ref Node defn)
        {
            {
                var n__prev1 = n;

                foreach (var (_, __n) in left)
                {
                    n = __n;
                    if (n.Sym != null)
                    {
                        n.Sym.SetUniq(true);
                    }
                }

                n = n__prev1;
            }

            long nnew = default;            long nerr = default;

            {
                var n__prev1 = n;

                foreach (var (__i, __n) in left)
                {
                    i = __i;
                    n = __n;
                    if (isblank(n))
                    {
                        continue;
                    }
                    if (!colasname(n))
                    {
                        yyerrorl(defn.Pos, "non-name %v on left side of :=", n);
                        nerr++;
                        continue;
                    }
                    if (!n.Sym.Uniq())
                    {
                        yyerrorl(defn.Pos, "%v repeated on left side of :=", n.Sym);
                        n.SetDiag(true);
                        nerr++;
                        continue;
                    }
                    n.Sym.SetUniq(false);
                    if (n.Sym.Block == types.Block)
                    {
                        continue;
                    }
                    nnew++;
                    n = newname(n.Sym);
                    declare(n, dclcontext);
                    n.Name.Defn = defn;
                    defn.Ninit.Append(nod(ODCL, n, null));
                    left[i] = n;
                }

                n = n__prev1;
            }

            if (nnew == 0L && nerr == 0L)
            {
                yyerrorl(defn.Pos, "no new variables on left side of :=");
            }
        }

        // declare the arguments in an
        // interface field declaration.
        private static void ifacedcl(ref Node n)
        {
            if (n.Op != ODCLFIELD || n.Right == null)
            {
                Fatalf("ifacedcl");
            }
            if (isblank(n.Left))
            {
                yyerror("methods must have a unique non-blank name");
            }
        }

        // declare the function proper
        // and declare the arguments.
        // called in extern-declaration context
        // returns in auto-declaration context.
        private static void funchdr(ref Node n)
        { 
            // change the declaration context from extern to auto
            if (funcdepth == 0L && dclcontext != PEXTERN)
            {
                Fatalf("funchdr: dclcontext = %d", dclcontext);
            }
            dclcontext = PAUTO;
            funcstart(n);

            if (n.Func.Nname != null)
            {
                funcargs(n.Func.Nname.Name.Param.Ntype);
            }
            else if (n.Func.Ntype != null)
            {
                funcargs(n.Func.Ntype);
            }
            else
            {
                funcargs2(n.Type);
            }
        }

        private static void funcargs(ref Node nt)
        {
            if (nt.Op != OTFUNC)
            {
                Fatalf("funcargs %v", nt.Op);
            } 

            // re-start the variable generation number
            // we want to use small numbers for the return variables,
            // so let them have the chunk starting at 1.
            vargen = nt.Rlist.Len(); 

            // declare the receiver and in arguments.
            // no n->defn because type checking of func header
            // will not fill in the types until later
            if (nt.Left != null)
            {
                var n = nt.Left;
                if (n.Op != ODCLFIELD)
                {
                    Fatalf("funcargs receiver %v", n.Op);
                }
                if (n.Left != null)
                {
                    n.Left.Op = ONAME;
                    n.Left.Name.Param.Ntype = n.Right;
                    declare(n.Left, PPARAM);
                    if (dclcontext == PAUTO)
                    {
                        vargen++;
                        n.Left.Name.Vargen = int32(vargen);
                    }
                }
            }
            {
                var n__prev1 = n;

                foreach (var (_, __n) in nt.List.Slice())
                {
                    n = __n;
                    if (n.Op != ODCLFIELD)
                    {
                        Fatalf("funcargs in %v", n.Op);
                    }
                    if (n.Left != null)
                    {
                        n.Left.Op = ONAME;
                        n.Left.Name.Param.Ntype = n.Right;
                        declare(n.Left, PPARAM);
                        if (dclcontext == PAUTO)
                        {
                            vargen++;
                            n.Left.Name.Vargen = int32(vargen);
                        }
                    }
                } 

                // declare the out arguments.

                n = n__prev1;
            }

            var gen = nt.List.Len();
            long i = 0L;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in nt.Rlist.Slice())
                {
                    n = __n;
                    if (n.Op != ODCLFIELD)
                    {
                        Fatalf("funcargs out %v", n.Op);
                    }
                    if (n.Left == null)
                    { 
                        // Name so that escape analysis can track it. ~r stands for 'result'.
                        n.Left = newname(lookupN("~r", gen));
                        gen++;
                    } 

                    // TODO: n->left->missing = 1;
                    n.Left.Op = ONAME;

                    if (isblank(n.Left))
                    { 
                        // Give it a name so we can assign to it during return. ~b stands for 'blank'.
                        // The name must be different from ~r above because if you have
                        //    func f() (_ int)
                        //    func g() int
                        // f is allowed to use a plain 'return' with no arguments, while g is not.
                        // So the two cases must be distinguished.
                        // We do not record a pointer to the original node (n->orig).
                        // Having multiple names causes too much confusion in later passes.
                        var nn = n.Left.Value;
                        nn.Orig = ref nn;
                        nn.Sym = lookupN("~b", gen);
                        gen++;
                        n.Left = ref nn;
                    }
                    n.Left.Name.Param.Ntype = n.Right;
                    declare(n.Left, PPARAMOUT);
                    if (dclcontext == PAUTO)
                    {
                        i++;
                        n.Left.Name.Vargen = int32(i);
                    }
                }

                n = n__prev1;
            }

        }

        // Same as funcargs, except run over an already constructed TFUNC.
        // This happens during import, where the hidden_fndcl rule has
        // used functype directly to parse the function's type.
        private static void funcargs2(ref types.Type t)
        {
            if (t.Etype != TFUNC)
            {
                Fatalf("funcargs2 %v", t);
            }
            {
                var ft__prev1 = ft;

                foreach (var (_, __ft) in t.Recvs().Fields().Slice())
                {
                    ft = __ft;
                    if (asNode(ft.Nname) == null || asNode(ft.Nname).Sym == null)
                    {
                        continue;
                    }
                    var n = asNode(ft.Nname); // no need for newname(ft->nname->sym)
                    n.Type = ft.Type;
                    declare(n, PPARAM);
                }

                ft = ft__prev1;
            }

            {
                var ft__prev1 = ft;

                foreach (var (_, __ft) in t.Params().Fields().Slice())
                {
                    ft = __ft;
                    if (asNode(ft.Nname) == null || asNode(ft.Nname).Sym == null)
                    {
                        continue;
                    }
                    n = asNode(ft.Nname);
                    n.Type = ft.Type;
                    declare(n, PPARAM);
                }

                ft = ft__prev1;
            }

            {
                var ft__prev1 = ft;

                foreach (var (_, __ft) in t.Results().Fields().Slice())
                {
                    ft = __ft;
                    if (asNode(ft.Nname) == null || asNode(ft.Nname).Sym == null)
                    {
                        continue;
                    }
                    n = asNode(ft.Nname);
                    n.Type = ft.Type;
                    declare(n, PPARAMOUT);
                }

                ft = ft__prev1;
            }

        }

        private static slice<ref Node> funcstack = default; // stack of previous values of Curfn
        private static int funcdepth = default; // len(funcstack) during parsing, but then forced to be the same later during compilation

        // start the function.
        // called before funcargs; undone at end of funcbody.
        private static void funcstart(ref Node n)
        {
            types.Markdcl();
            funcstack = append(funcstack, Curfn);
            funcdepth++;
            Curfn = n;
        }

        // finish the body.
        // called in auto-declaration context.
        // returns in extern-declaration context.
        private static void funcbody()
        { 
            // change the declaration context from auto to extern
            if (dclcontext != PAUTO)
            {
                Fatalf("funcbody: unexpected dclcontext %d", dclcontext);
            }
            types.Popdcl();
            funcstack = funcstack[..len(funcstack) - 1L];
            Curfn = funcstack[len(funcstack) - 1L];
            funcdepth--;
            if (funcdepth == 0L)
            {
                dclcontext = PEXTERN;
            }
        }

        // structs, functions, and methods.
        // they don't belong here, but where do they belong?
        private static void checkembeddedtype(ref types.Type t)
        {
            if (t == null)
            {
                return;
            }
            if (t.Sym == null && t.IsPtr())
            {
                t = t.Elem();
                if (t.IsInterface())
                {
                    yyerror("embedded type cannot be a pointer to interface");
                }
            }
            if (t.IsPtr() || t.IsUnsafePtr())
            {
                yyerror("embedded type cannot be a pointer");
            }
            else if (t.Etype == TFORW && !t.ForwardType().Embedlineno.IsKnown())
            {
                t.ForwardType().Embedlineno = lineno;
            }
        }

        private static ref types.Field structfield(ref Node n)
        {
            var lno = lineno;
            lineno = n.Pos;

            if (n.Op != ODCLFIELD)
            {
                Fatalf("structfield: oops %v\n", n);
            }
            var f = types.NewField();
            f.SetIsddd(n.Isddd());

            if (n.Right != null)
            {
                n.Right = typecheck(n.Right, Etype);
                n.Type = n.Right.Type;
                if (n.Left != null)
                {
                    n.Left.Type = n.Type;
                }
                if (n.Embedded())
                {
                    checkembeddedtype(n.Type);
                }
            }
            n.Right = null;

            f.Type = n.Type;
            if (f.Type == null)
            {
                f.SetBroke(true);
            }
            switch (n.Val().U.type())
            {
                case @string u:
                    f.Note = u;
                    break;
                case 
                    break;
                default:
                {
                    var u = n.Val().U.type();
                    yyerror("field tag must be a string");
                    break;
                }

            }

            if (n.Left != null && n.Left.Op == ONAME)
            {
                f.Nname = asTypesNode(n.Left);
                if (n.Embedded())
                {
                    f.Embedded = 1L;
                }
                else
                {
                    f.Embedded = 0L;
                }
                f.Sym = asNode(f.Nname).Sym;
            }
            lineno = lno;
            return f;
        }

        // checkdupfields emits errors for duplicately named fields or methods in
        // a list of struct or interface types.
        private static void checkdupfields(@string what, params ptr<types.Type>[] ts)
        {
            ts = ts.Clone();

            var seen = make_map<ref types.Sym, bool>();
            foreach (var (_, t) in ts)
            {
                foreach (var (_, f) in t.Fields().Slice())
                {
                    if (f.Sym == null || f.Sym.IsBlank() || asNode(f.Nname) == null)
                    {
                        continue;
                    }
                    if (seen[f.Sym])
                    {
                        yyerrorl(asNode(f.Nname).Pos, "duplicate %s %s", what, f.Sym.Name);
                        continue;
                    }
                    seen[f.Sym] = true;
                }
            }
        }

        // convert a parsed id/type list into
        // a type for struct/interface/arglist
        private static ref types.Type tostruct(slice<ref Node> l)
        {
            var t = types.New(TSTRUCT);
            tostruct0(t, l);
            return t;
        }

        private static void tostruct0(ref types.Type t, slice<ref Node> l)
        {
            if (t == null || !t.IsStruct())
            {
                Fatalf("struct expected");
            }
            var fields = make_slice<ref types.Field>(len(l));
            foreach (var (i, n) in l)
            {
                var f = structfield(n);
                if (f.Broke())
                {
                    t.SetBroke(true);
                }
                fields[i] = f;
            }
            t.SetFields(fields);

            checkdupfields("field", t);

            if (!t.Broke())
            {
                checkwidth(t);
            }
        }

        private static ref types.Type tofunargs(slice<ref Node> l, types.Funarg funarg)
        {
            var t = types.New(TSTRUCT);
            t.StructType().Funarg = funarg;

            var fields = make_slice<ref types.Field>(len(l));
            foreach (var (i, n) in l)
            {
                var f = structfield(n);
                f.Funarg = funarg; 

                // esc.go needs to find f given a PPARAM to add the tag.
                if (n.Left != null && n.Left.Class() == PPARAM)
                {
                    n.Left.Name.Param.Field = f;
                }
                if (f.Broke())
                {
                    t.SetBroke(true);
                }
                fields[i] = f;
            }
            t.SetFields(fields);
            return t;
        }

        private static ref types.Type tofunargsfield(slice<ref types.Field> fields, types.Funarg funarg)
        {
            var t = types.New(TSTRUCT);
            t.StructType().Funarg = funarg;

            foreach (var (_, f) in fields)
            {
                f.Funarg = funarg; 

                // esc.go needs to find f given a PPARAM to add the tag.
                if (asNode(f.Nname) != null && asNode(f.Nname).Class() == PPARAM)
                {
                    asNode(f.Nname).Name.Param.Field;

                    f;
                }
            }
            t.SetFields(fields);
            return t;
        }

        private static ref types.Field interfacefield(ref Node n)
        {
            var lno = lineno;
            lineno = n.Pos;

            if (n.Op != ODCLFIELD)
            {
                Fatalf("interfacefield: oops %v\n", n);
            }
            if (n.Val().Ctype() != CTxxx)
            {
                yyerror("interface method cannot have annotation");
            } 

            // MethodSpec = MethodName Signature | InterfaceTypeName .
            //
            // If Left != nil, then Left is MethodName and Right is Signature.
            // Otherwise, Right is InterfaceTypeName.
            if (n.Right != null)
            {
                n.Right = typecheck(n.Right, Etype);
                n.Type = n.Right.Type;
                n.Right = null;
            }
            var f = types.NewField();
            if (n.Left != null)
            {
                f.Nname = asTypesNode(n.Left);
                f.Sym = asNode(f.Nname).Sym;
            }
            else
            { 
                // Placeholder ONAME just to hold Pos.
                // TODO(mdempsky): Add Pos directly to Field instead.
                f.Nname = asTypesNode(newname(nblank.Sym));
            }
            f.Type = n.Type;
            if (f.Type == null)
            {
                f.SetBroke(true);
            }
            lineno = lno;
            return f;
        }

        private static ref types.Type tointerface(slice<ref Node> l)
        {
            if (len(l) == 0L)
            {
                return types.Types[TINTER];
            }
            var t = types.New(TINTER);
            tointerface0(t, l);
            return t;
        }

        private static void tointerface0(ref types.Type t, slice<ref Node> l)
        {
            if (t == null || !t.IsInterface())
            {
                Fatalf("interface expected");
            }
            slice<ref types.Field> fields = default;
            foreach (var (_, n) in l)
            {
                var f = interfacefield(n);
                if (f.Broke())
                {
                    t.SetBroke(true);
                }
                fields = append(fields, f);
            }
            t.SetInterface(fields);
        }

        private static ref Node fakeRecv()
        {
            return anonfield(types.FakeRecvType());
        }

        private static ref types.Field fakeRecvField()
        {
            var f = types.NewField();
            f.Type = types.FakeRecvType();
            return f;
        }

        // isifacemethod reports whether (field) m is
        // an interface method. Such methods have the
        // special receiver type types.FakeRecvType().
        private static bool isifacemethod(ref types.Type f)
        {
            return f.Recv().Type == types.FakeRecvType();
        }

        // turn a parsed function declaration into a type
        private static ref types.Type functype(ref Node @this, slice<ref Node> @in, slice<ref Node> @out)
        {
            var t = types.New(TFUNC);
            functype0(t, this, in, out);
            return t;
        }

        private static void functype0(ref types.Type t, ref Node @this, slice<ref Node> @in, slice<ref Node> @out)
        {
            if (t == null || t.Etype != TFUNC)
            {
                Fatalf("function type expected");
            }
            slice<ref Node> rcvr = default;
            if (this != null)
            {
                rcvr = new slice<ref Node>(new ref Node[] { this });
            }
            t.FuncType().Receiver = tofunargs(rcvr, types.FunargRcvr);
            t.FuncType().Results = tofunargs(out, types.FunargResults);
            t.FuncType().Params = tofunargs(in, types.FunargParams);

            checkdupfields("argument", t.Recvs(), t.Results(), t.Params());

            if (t.Recvs().Broke() || t.Results().Broke() || t.Params().Broke())
            {
                t.SetBroke(true);
            }
            t.FuncType().Outnamed = false;
            if (len(out) > 0L && out[0L].Left != null && out[0L].Left.Orig != null)
            {
                var s = out[0L].Left.Orig.Sym;
                if (s != null && (s.Name[0L] != '~' || s.Name[1L] != 'r'))
                { // ~r%d is the name invented for an unnamed result
                    t.FuncType().Outnamed = true;
                }
            }
        }

        private static ref types.Type functypefield(ref types.Field @this, slice<ref types.Field> @in, slice<ref types.Field> @out)
        {
            var t = types.New(TFUNC);
            functypefield0(t, this, in, out);
            return t;
        }

        private static void functypefield0(ref types.Type t, ref types.Field @this, slice<ref types.Field> @in, slice<ref types.Field> @out)
        {
            slice<ref types.Field> rcvr = default;
            if (this != null)
            {
                rcvr = new slice<ref types.Field>(new ref types.Field[] { this });
            }
            t.FuncType().Receiver = tofunargsfield(rcvr, types.FunargRcvr);
            t.FuncType().Results = tofunargsfield(out, types.FunargRcvr);
            t.FuncType().Params = tofunargsfield(in, types.FunargRcvr);

            t.FuncType().Outnamed = false;
            if (len(out) > 0L && asNode(out[0L].Nname) != null && asNode(out[0L].Nname).Orig != null)
            {
                var s = asNode(out[0L].Nname).Orig.Sym;
                if (s != null && (s.Name[0L] != '~' || s.Name[1L] != 'r'))
                { // ~r%d is the name invented for an unnamed result
                    t.FuncType().Outnamed = true;
                }
            }
        }

        private static ref types.Pkg methodsym_toppkg = default;

        private static ref types.Sym methodsym(ref types.Sym nsym, ref types.Type t0, bool iface)
        {
            if (t0 == null)
            {
                Fatalf("methodsym: nil receiver type");
            }
            var t = t0;
            var s = t.Sym;
            if (s == null && t.IsPtr())
            {
                t = t.Elem();
                if (t == null)
                {
                    Fatalf("methodsym: ptrto nil");
                }
                s = t.Sym;
            } 

            // if t0 == *t and t0 has a sym,
            // we want to see *t, not t0, in the method name.
            if (t != t0 && t0.Sym != null)
            {
                t0 = types.NewPtr(t);
            }
            @string suffix = "";
            if (iface)
            {
                dowidth(t0);
                if (t0.Width < int64(Widthptr))
                {
                    suffix = "·i";
                }
            }
            ref types.Pkg spkg = default;
            if (s != null)
            {
                spkg = s.Pkg;
            }
            @string pkgprefix = "";
            if ((spkg == null || nsym.Pkg != spkg) && !exportname(nsym.Name) && nsym.Pkg.Prefix != "\"\"")
            {
                pkgprefix = "." + nsym.Pkg.Prefix;
            }
            @string p = default;
            if (t0.Sym == null && t0.IsPtr())
            {
                p = fmt.Sprintf("(%-S)%s.%s%s", t0, pkgprefix, nsym.Name, suffix);
            }
            else
            {
                p = fmt.Sprintf("%-S%s.%s%s", t0, pkgprefix, nsym.Name, suffix);
            }
            if (spkg == null)
            {
                if (methodsym_toppkg == null)
                {
                    methodsym_toppkg = types.NewPkg("go", "");
                }
                spkg = methodsym_toppkg;
            }
            return spkg.Lookup(p);
        }

        // methodname is a misnomer because this now returns a Sym, rather
        // than an ONAME.
        // TODO(mdempsky): Reconcile with methodsym.
        private static ref types.Sym methodname(ref types.Sym s, ref types.Type recv)
        {
            var star = false;
            if (recv.IsPtr())
            {
                star = true;
                recv = recv.Elem();
            }
            var tsym = recv.Sym;
            if (tsym == null || s.IsBlank())
            {
                return s;
            }
            @string p = default;
            if (star)
            {
                p = fmt.Sprintf("(*%v).%v", tsym.Name, s);
            }
            else
            {
                p = fmt.Sprintf("%v.%v", tsym, s);
            }
            s = tsym.Pkg.Lookup(p);

            return s;
        }

        // Add a method, declared as a function.
        // - msym is the method symbol
        // - t is function type (with receiver)
        // Returns a pointer to the existing or added Field.
        private static ref types.Field addmethod(ref types.Sym msym, ref types.Type t, bool local, bool nointerface)
        {
            if (msym == null)
            {
                Fatalf("no method symbol");
            } 

            // get parent type sym
            var rf = t.Recv(); // ptr to this structure
            if (rf == null)
            {
                yyerror("missing receiver");
                return null;
            }
            var mt = methtype(rf.Type);
            if (mt == null || mt.Sym == null)
            {
                var pa = rf.Type;
                var t = pa;
                if (t != null && t.IsPtr())
                {
                    if (t.Sym != null)
                    {
                        yyerror("invalid receiver type %v (%v is a pointer type)", pa, t);
                        return null;
                    }
                    t = t.Elem();
                }

                if (t == null || t.Broke())                 else if (t.Sym == null) 
                    yyerror("invalid receiver type %v (%v is an unnamed type)", pa, t);
                else if (t.IsPtr()) 
                    yyerror("invalid receiver type %v (%v is a pointer type)", pa, t);
                else if (t.IsInterface()) 
                    yyerror("invalid receiver type %v (%v is an interface type)", pa, t);
                else 
                    // Should have picked off all the reasons above,
                    // but just in case, fall back to generic error.
                    yyerror("invalid receiver type %v (%L / %L)", pa, pa, t);
                                return null;
            }
            if (local && mt.Sym.Pkg != localpkg)
            {
                yyerror("cannot define new methods on non-local type %v", mt);
                return null;
            }
            if (msym.IsBlank())
            {
                return null;
            }
            if (mt.IsStruct())
            {
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in mt.Fields().Slice())
                    {
                        f = __f;
                        if (f.Sym == msym)
                        {
                            yyerror("type %v has both field and method named %v", mt, msym);
                            return null;
                        }
                    }

                    f = f__prev1;
                }

            }
            {
                var f__prev1 = f;

                foreach (var (_, __f) in mt.Methods().Slice())
                {
                    f = __f;
                    if (msym.Name != f.Sym.Name)
                    {
                        continue;
                    } 
                    // eqtype only checks that incoming and result parameters match,
                    // so explicitly check that the receiver parameters match too.
                    if (!eqtype(t, f.Type) || !eqtype(t.Recv().Type, f.Type.Recv().Type))
                    {
                        yyerror("method redeclared: %v.%v\n\t%v\n\t%v", mt, msym, f.Type, t);
                    }
                    return f;
                }

                f = f__prev1;
            }

            var f = types.NewField();
            f.Sym = msym;
            f.Nname = asTypesNode(newname(msym));
            f.Type = t;
            f.SetNointerface(nointerface);

            mt.Methods().Append(f);
            return f;
        }

        private static void funccompile(ref Node n)
        {
            if (n.Type == null)
            {
                if (nerrors == 0L)
                {
                    Fatalf("funccompile missing type");
                }
                return;
            } 

            // assign parameter offsets
            checkwidth(n.Type);

            if (Curfn != null)
            {
                Fatalf("funccompile %v inside %v", n.Func.Nname.Sym, Curfn.Func.Nname.Sym);
            }
            dclcontext = PAUTO;
            funcdepth = n.Func.Depth + 1L;
            compile(n);
            Curfn = null;
            funcdepth = 0L;
            dclcontext = PEXTERN;
        }

        private static @string funcsymname(ref types.Sym s)
        {
            return s.Name + "·f";
        }

        // funcsym returns s·f.
        private static ref types.Sym funcsym(ref types.Sym s)
        { 
            // funcsymsmu here serves to protect not just mutations of funcsyms (below),
            // but also the package lookup of the func sym name,
            // since this function gets called concurrently from the backend.
            // There are no other concurrent package lookups in the backend,
            // except for the types package, which is protected separately.
            // Reusing funcsymsmu to also cover this package lookup
            // avoids a general, broader, expensive package lookup mutex.
            // Note makefuncsym also does package look-up of func sym names,
            // but that it is only called serially, from the front end.
            funcsymsmu.Lock();
            var (sf, existed) = s.Pkg.LookupOK(funcsymname(s)); 
            // Don't export s·f when compiling for dynamic linking.
            // When dynamically linking, the necessary function
            // symbols will be created explicitly with makefuncsym.
            // See the makefuncsym comment for details.
            if (!Ctxt.Flag_dynlink && !existed)
            {
                funcsyms = append(funcsyms, s);
            }
            funcsymsmu.Unlock();
            return sf;
        }

        // makefuncsym ensures that s·f is exported.
        // It is only used with -dynlink.
        // When not compiling for dynamic linking,
        // the funcsyms are created as needed by
        // the packages that use them.
        // Normally we emit the s·f stubs as DUPOK syms,
        // but DUPOK doesn't work across shared library boundaries.
        // So instead, when dynamic linking, we only create
        // the s·f stubs in s's package.
        private static void makefuncsym(ref types.Sym s)
        {
            if (!Ctxt.Flag_dynlink)
            {
                Fatalf("makefuncsym dynlink");
            }
            if (s.IsBlank())
            {
                return;
            }
            if (compiling_runtime && (s.Name == "getg" || s.Name == "getclosureptr" || s.Name == "getcallerpc" || s.Name == "getcallersp"))
            { 
                // runtime.getg(), getclosureptr(), getcallerpc(), and
                // getcallersp() are not real functions and so do not
                // get funcsyms.
                return;
            }
            {
                var (_, existed) = s.Pkg.LookupOK(funcsymname(s));

                if (!existed)
                {
                    funcsyms = append(funcsyms, s);
                }

            }
        }

        private static ref Node dclfunc(ref types.Sym sym, ref Node tfn)
        {
            if (tfn.Op != OTFUNC)
            {
                Fatalf("expected OTFUNC node, got %v", tfn);
            }
            var fn = nod(ODCLFUNC, null, null);
            fn.Func.Nname = newname(sym);
            fn.Func.Nname.Name.Defn = fn;
            fn.Func.Nname.Name.Param.Ntype = tfn;
            declare(fn.Func.Nname, PFUNC);
            funchdr(fn);
            fn.Func.Nname.Name.Param.Ntype = typecheck(fn.Func.Nname.Name.Param.Ntype, Etype);
            return fn;
        }

        private partial struct nowritebarrierrecChecker
        {
            public map<ref Node, slice<nowritebarrierrecCall>> extraCalls; // curfn is the current function during AST walks.
            public ptr<Node> curfn;
        }

        private partial struct nowritebarrierrecCall
        {
            public ptr<Node> target; // ODCLFUNC of caller or callee
            public src.XPos lineno; // line of call
        }

        private partial struct nowritebarrierrecCallSym
        {
            public ptr<obj.LSym> target; // LSym of callee
            public src.XPos lineno; // line of call
        }

        // newNowritebarrierrecChecker creates a nowritebarrierrecChecker. It
        // must be called before transformclosure and walk.
        private static ref nowritebarrierrecChecker newNowritebarrierrecChecker()
        {
            nowritebarrierrecChecker c = ref new nowritebarrierrecChecker(extraCalls:make(map[*Node][]nowritebarrierrecCall),); 

            // Find all systemstack calls and record their targets. In
            // general, flow analysis can't see into systemstack, but it's
            // important to handle it for this check, so we model it
            // directly. This has to happen before transformclosure since
            // it's a lot harder to work out the argument after.
            foreach (var (_, n) in xtop)
            {
                if (n.Op != ODCLFUNC)
                {
                    continue;
                }
                c.curfn = n;
                inspect(n, c.findExtraCalls);
            }
            c.curfn = null;
            return c;
        }

        private static bool findExtraCalls(this ref nowritebarrierrecChecker c, ref Node n)
        {
            if (n.Op != OCALLFUNC)
            {
                return true;
            }
            var fn = n.Left;
            if (fn == null || fn.Op != ONAME || fn.Class() != PFUNC || fn.Name.Defn == null)
            {
                return true;
            }
            if (!isRuntimePkg(fn.Sym.Pkg) || fn.Sym.Name != "systemstack")
            {
                return true;
            }
            ref Node callee = default;
            var arg = n.List.First();

            if (arg.Op == ONAME) 
                callee = arg.Name.Defn;
            else if (arg.Op == OCLOSURE) 
                callee = arg.Func.Closure;
            else 
                Fatalf("expected ONAME or OCLOSURE node, got %+v", arg);
                        if (callee.Op != ODCLFUNC)
            {
                Fatalf("expected ODCLFUNC node, got %+v", callee);
            }
            c.extraCalls[c.curfn] = append(c.extraCalls[c.curfn], new nowritebarrierrecCall(callee,n.Pos));
            return true;
        }

        // recordCall records a call from ODCLFUNC node "from", to function
        // symbol "to" at position pos.
        //
        // This should be done as late as possible during compilation to
        // capture precise call graphs. The target of the call is an LSym
        // because that's all we know after we start SSA.
        //
        // This can be called concurrently for different from Nodes.
        private static void recordCall(this ref nowritebarrierrecChecker c, ref Node from, ref obj.LSym to, src.XPos pos)
        {
            if (from.Op != ODCLFUNC)
            {
                Fatalf("expected ODCLFUNC, got %v", from);
            } 
            // We record this information on the *Func so this is
            // concurrent-safe.
            var fn = from.Func;
            if (fn.nwbrCalls == null)
            {
                fn.nwbrCalls = @new<slice<nowritebarrierrecCallSym>>();
            }
            fn.nwbrCalls.Value = append(fn.nwbrCalls.Value, new nowritebarrierrecCallSym(to,pos));
        }

        private static void check(this ref nowritebarrierrecChecker c)
        { 
            // We walk the call graph as late as possible so we can
            // capture all calls created by lowering, but this means we
            // only get to see the obj.LSyms of calls. symToFunc lets us
            // get back to the ODCLFUNCs.
            var symToFunc = make_map<ref obj.LSym, ref Node>(); 
            // funcs records the back-edges of the BFS call graph walk. It
            // maps from the ODCLFUNC of each function that must not have
            // write barriers to the call that inhibits them. Functions
            // that are directly marked go:nowritebarrierrec are in this
            // map with a zero-valued nowritebarrierrecCall. This also
            // acts as the set of marks for the BFS of the call graph.
            var funcs = make_map<ref Node, nowritebarrierrecCall>(); 
            // q is the queue of ODCLFUNC Nodes to visit in BFS order.
            nodeQueue q = default;

            foreach (var (_, n) in xtop)
            {
                if (n.Op != ODCLFUNC)
                {
                    continue;
                }
                symToFunc[n.Func.lsym] = n; 

                // Make nowritebarrierrec functions BFS roots.
                if (n.Func.Pragma & Nowritebarrierrec != 0L)
                {
                    funcs[n] = new nowritebarrierrecCall();
                    q.pushRight(n);
                } 
                // Check go:nowritebarrier functions.
                if (n.Func.Pragma & Nowritebarrier != 0L && n.Func.WBPos.IsKnown())
                {
                    yyerrorl(n.Func.WBPos, "write barrier prohibited");
                }
            } 

            // Perform a BFS of the call graph from all
            // go:nowritebarrierrec functions.
            Action<ref Node, ref Node, src.XPos> enqueue = (src, target, pos) =>
            {
                if (target.Func.Pragma & Yeswritebarrierrec != 0L)
                { 
                    // Don't flow into this function.
                    return;
                }
                {
                    var (_, ok) = funcs[target];

                    if (ok)
                    { 
                        // Already found a path to target.
                        return;
                    } 

                    // Record the path.

                } 

                // Record the path.
                funcs[target] = new nowritebarrierrecCall(target:src,lineno:pos);
                q.pushRight(target);
            }
;
            while (!q.empty())
            {
                var fn = q.popLeft(); 

                // Check fn.
                if (fn.Func.WBPos.IsKnown())
                {
                    bytes.Buffer err = default;
                    var call = funcs[fn];
                    while (call.target != null)
                    {
                        fmt.Fprintf(ref err, "\n\t%v: called by %v", linestr(call.lineno), call.target.Func.Nname);
                        call = funcs[call.target];
                    }

                    yyerrorl(fn.Func.WBPos, "write barrier prohibited by caller; %v%s", fn.Func.Nname, err.String());
                    continue;
                } 

                // Enqueue fn's calls.
                {
                    var callee__prev2 = callee;

                    foreach (var (_, __callee) in c.extraCalls[fn])
                    {
                        callee = __callee;
                        enqueue(fn, callee.target, callee.lineno);
                    }

                    callee = callee__prev2;
                }

                if (fn.Func.nwbrCalls == null)
                {
                    continue;
                }
                {
                    var callee__prev2 = callee;

                    foreach (var (_, __callee) in fn.Func.nwbrCalls.Value)
                    {
                        callee = __callee;
                        var target = symToFunc[callee.target];
                        if (target != null)
                        {
                            enqueue(fn, target, callee.lineno);
                        }
                    }

                    callee = callee__prev2;
                }

            }

        }
    }
}}}}
