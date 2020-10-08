// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:39 UTC
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
        private static slice<ptr<Node>> externdcl = default;

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

        // redeclare emits a diagnostic about symbol s being redeclared at pos.
        private static void redeclare(src.XPos pos, ptr<types.Sym> _addr_s, @string where)
        {
            ref types.Sym s = ref _addr_s.val;

            if (!s.Lastlineno.IsKnown())
            {
                var pkg = s.Origpkg;
                if (pkg == null)
                {
                    pkg = s.Pkg;
                }

                yyerrorl(pos, "%v redeclared %s\n" + "\tprevious declaration during import %q", s, where, pkg.Path);

            }
            else
            {
                var prevPos = s.Lastlineno; 

                // When an import and a declaration collide in separate files,
                // present the import as the "redeclared", because the declaration
                // is visible where the import is, but not vice versa.
                // See issue 4510.
                if (s.Def == null)
                {
                    pos = prevPos;
                    prevPos = pos;

                }

                yyerrorl(pos, "%v redeclared %s\n" + "\tprevious declaration at %v", s, where, linestr(prevPos));

            }

        }

        private static long vargen = default;

        // declare individual names - var, typ, const

        private static long declare_typegen = default;

        // declare records that Node n declares symbol n.Sym in the specified
        // declaration context.
        private static void declare(ptr<Node> _addr_n, Class ctxt)
        {
            ref Node n = ref _addr_n.val;

            if (n.isBlank())
            {
                return ;
            }

            if (n.Name == null)
            { 
                // named OLITERAL needs Name; most OLITERALs don't.
                n.Name = @new<Name>();

            }

            var s = n.Sym; 

            // kludgy: typecheckok means we're past parsing. Eg genwrapper may declare out of package names later.
            if (!inimport && !typecheckok && s.Pkg != localpkg)
            {
                yyerrorl(n.Pos, "cannot declare name %v", s);
            }

            long gen = 0L;
            if (ctxt == PEXTERN)
            {
                if (s.Name == "init")
                {
                    yyerrorl(n.Pos, "cannot declare init - must be func");
                }

                if (s.Name == "main" && s.Pkg.Name == "main")
                {
                    yyerrorl(n.Pos, "cannot declare main - must be func");
                }

                externdcl = append(externdcl, n);

            }
            else
            {
                if (Curfn == null && ctxt == PAUTO)
                {
                    lineno = n.Pos;
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
                    redeclare(n.Pos, _addr_s, "in this block");
                }

            }

            s.Block = types.Block;
            s.Lastlineno = lineno;
            s.Def = asTypesNode(n);
            n.Name.Vargen = int32(gen);
            n.SetClass(ctxt);
            if (ctxt == PFUNC)
            {
                n.Sym.SetFunc(true);
            }

            autoexport(n, ctxt);

        }

        private static void addvar(ptr<Node> _addr_n, ptr<types.Type> _addr_t, Class ctxt)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (n == null || n.Sym == null || (n.Op != ONAME && n.Op != ONONAME) || t == null)
            {
                Fatalf("addvar: n=%v t=%v nil", n, t);
            }

            n.Op = ONAME;
            declare(_addr_n, ctxt);
            n.Type = t;

        }

        // declare variables from grammar
        // new_name_list (type | [type] = expr_list)
        private static slice<ptr<Node>> variter(slice<ptr<Node>> vl, ptr<Node> _addr_t, slice<ptr<Node>> el)
        {
            ref Node t = ref _addr_t.val;

            slice<ptr<Node>> init = default;
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
                        declare(_addr_v, dclcontext);
                        v.Name.Param.Ntype = t;
                        v.Name.Defn = as2;
                        if (Curfn != null)
                        {
                            init = append(init, nod(ODCL, v, null));
                        }

                    }

                    v = v__prev1;
                }

                return append(init, as2);

            }

            var nel = len(el);
            {
                var v__prev1 = v;

                foreach (var (_, __v) in vl)
                {
                    v = __v;
                    e = ;
                    if (doexpr)
                    {
                        if (len(el) == 0L)
                        {
                            yyerror("assignment mismatch: %d variables but %d values", len(vl), nel);
                            break;
                        }

                        e = el[0L];
                        el = el[1L..];

                    }

                    v.Op = ONAME;
                    declare(_addr_v, dclcontext);
                    v.Name.Param.Ntype = t;

                    if (e != null || Curfn != null || v.isBlank())
                    {
                        if (Curfn != null)
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
                yyerror("assignment mismatch: %d variables but %d values", len(vl), nel);
            }

            return init;

        }

        // newnoname returns a new ONONAME Node associated with symbol s.
        private static ptr<Node> newnoname(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            if (s == null)
            {
                Fatalf("newnoname nil");
            }

            var n = nod(ONONAME, null, null);
            n.Sym = s;
            n.Xoffset = 0L;
            return _addr_n!;

        }

        // newfuncnamel generates a new name node for a function or method.
        // TODO(rsc): Use an ODCLFUNC node instead. See comment in CL 7360.
        private static ptr<Node> newfuncnamel(src.XPos pos, ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            var n = newnamel(pos, s);
            n.Func = @new<Func>();
            n.Func.SetIsHiddenClosure(Curfn != null);
            return _addr_n!;
        }

        // this generates a new name node for a name
        // being declared.
        private static ptr<Node> dclname(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            var n = newname(s);
            n.Op = ONONAME; // caller will correct it
            return _addr_n!;

        }

        private static ptr<Node> typenod(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return _addr_typenodl(src.NoXPos, _addr_t)!;
        }

        private static ptr<Node> typenodl(src.XPos pos, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
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

            return _addr_asNode(t.Nod)!;

        }

        private static ptr<Node> anonfield(ptr<types.Type> _addr_typ)
        {
            ref types.Type typ = ref _addr_typ.val;

            return _addr_symfield(_addr_null, _addr_typ)!;
        }

        private static ptr<Node> namedfield(@string s, ptr<types.Type> _addr_typ)
        {
            ref types.Type typ = ref _addr_typ.val;

            return _addr_symfield(_addr_lookup(s), _addr_typ)!;
        }

        private static ptr<Node> symfield(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_typ)
        {
            ref types.Sym s = ref _addr_s.val;
            ref types.Type typ = ref _addr_typ.val;

            var n = nodSym(ODCLFIELD, null, s);
            n.Type = typ;
            return _addr_n!;
        }

        // oldname returns the Node that declares symbol s in the current scope.
        // If no such Node currently exists, an ONONAME Node is returned instead.
        private static ptr<Node> oldname(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            var n = asNode(s.Def);
            if (n == null)
            { 
                // Maybe a top-level declaration will come along later to
                // define s. resolve will check s.Def again once all input
                // source has been processed.
                return _addr_newnoname(_addr_s)!;

            }

            if (Curfn != null && n.Op == ONAME && n.Name.Curfn != null && n.Name.Curfn != Curfn)
            { 
                // Inner func is referring to var in outer func.
                //
                // TODO(rsc): If there is an outer variable x and we
                // are parsing x := 5 inside the closure, until we get to
                // the := it looks like a reference to the outer x so we'll
                // make x a closure variable unnecessarily.
                var c = n.Name.Param.Innermost;
                if (c == null || c.Name.Curfn != Curfn)
                { 
                    // Do not have a closure var for the active closure yet; make one.
                    c = newname(s);
                    c.SetClass(PAUTOHEAP);
                    c.Name.SetIsClosureVar(true);
                    c.SetIsDDD(n.IsDDD());
                    c.Name.Defn = n; 

                    // Link into list of active closure variables.
                    // Popped from list in func closurebody.
                    c.Name.Param.Outer = n.Name.Param.Innermost;
                    n.Name.Param.Innermost = c;

                    Curfn.Func.Cvars.Append(c);

                } 

                // return ref to closure var, not original
                return _addr_c!;

            }

            return _addr_n!;

        }

        // := declarations
        private static bool colasname(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == ONAME || n.Op == ONONAME || n.Op == OPACK || n.Op == OTYPE || n.Op == OLITERAL) 
                return n.Sym != null;
                        return false;

        }

        private static void colasdefn(slice<ptr<Node>> left, ptr<Node> _addr_defn)
        {
            ref Node defn = ref _addr_defn.val;

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
                    if (n.isBlank())
                    {
                        continue;
                    }

                    if (!colasname(_addr_n))
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
                    declare(_addr_n, dclcontext);
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
        private static void ifacedcl(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != ODCLFIELD || n.Left == null)
            {
                Fatalf("ifacedcl");
            }

            if (n.Sym.IsBlank())
            {
                yyerror("methods must have a unique non-blank name");
            }

        }

        // declare the function proper
        // and declare the arguments.
        // called in extern-declaration context
        // returns in auto-declaration context.
        private static void funchdr(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // change the declaration context from extern to auto
            if (Curfn == null && dclcontext != PEXTERN)
            {
                Fatalf("funchdr: dclcontext = %d", dclcontext);
            }

            dclcontext = PAUTO;
            types.Markdcl();
            funcstack = append(funcstack, Curfn);
            Curfn = n;

            if (n.Func.Nname != null)
            {
                funcargs(_addr_n.Func.Nname.Name.Param.Ntype);
            }
            else if (n.Func.Ntype != null)
            {
                funcargs(_addr_n.Func.Ntype);
            }
            else
            {
                funcargs2(_addr_n.Type);
            }

        }

        private static void funcargs(ptr<Node> _addr_nt)
        {
            ref Node nt = ref _addr_nt.val;

            if (nt.Op != OTFUNC)
            {
                Fatalf("funcargs %v", nt.Op);
            } 

            // re-start the variable generation number
            // we want to use small numbers for the return variables,
            // so let them have the chunk starting at 1.
            //
            // TODO(mdempsky): This is ugly, and only necessary because
            // esc.go uses Vargen to figure out result parameters' index
            // within the result tuple.
            vargen = nt.Rlist.Len(); 

            // declare the receiver and in arguments.
            if (nt.Left != null)
            {
                funcarg(_addr_nt.Left, PPARAM);
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in nt.List.Slice())
                {
                    n = __n;
                    funcarg(_addr_n, PPARAM);
                }

                n = n__prev1;
            }

            var oldvargen = vargen;
            vargen = 0L; 

            // declare the out arguments.
            var gen = nt.List.Len();
            {
                var n__prev1 = n;

                foreach (var (_, __n) in nt.Rlist.Slice())
                {
                    n = __n;
                    if (n.Sym == null)
                    { 
                        // Name so that escape analysis can track it. ~r stands for 'result'.
                        n.Sym = lookupN("~r", gen);
                        gen++;

                    }

                    if (n.Sym.IsBlank())
                    { 
                        // Give it a name so we can assign to it during return. ~b stands for 'blank'.
                        // The name must be different from ~r above because if you have
                        //    func f() (_ int)
                        //    func g() int
                        // f is allowed to use a plain 'return' with no arguments, while g is not.
                        // So the two cases must be distinguished.
                        n.Sym = lookupN("~b", gen);
                        gen++;

                    }

                    funcarg(_addr_n, PPARAMOUT);

                }

                n = n__prev1;
            }

            vargen = oldvargen;

        }

        private static void funcarg(ptr<Node> _addr_n, Class ctxt)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != ODCLFIELD)
            {
                Fatalf("funcarg %v", n.Op);
            }

            if (n.Sym == null)
            {
                return ;
            }

            n.Right = newnamel(n.Pos, n.Sym);
            n.Right.Name.Param.Ntype = n.Left;
            n.Right.SetIsDDD(n.IsDDD());
            declare(_addr_n.Right, ctxt);

            vargen++;
            n.Right.Name.Vargen = int32(vargen);

        }

        // Same as funcargs, except run over an already constructed TFUNC.
        // This happens during import, where the hidden_fndcl rule has
        // used functype directly to parse the function's type.
        private static void funcargs2(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.Etype != TFUNC)
            {
                Fatalf("funcargs2 %v", t);
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in t.Recvs().Fields().Slice())
                {
                    f = __f;
                    funcarg2(_addr_f, PPARAM);
                }

                f = f__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in t.Params().Fields().Slice())
                {
                    f = __f;
                    funcarg2(_addr_f, PPARAM);
                }

                f = f__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in t.Results().Fields().Slice())
                {
                    f = __f;
                    funcarg2(_addr_f, PPARAMOUT);
                }

                f = f__prev1;
            }
        }

        private static void funcarg2(ptr<types.Field> _addr_f, Class ctxt)
        {
            ref types.Field f = ref _addr_f.val;

            if (f.Sym == null)
            {
                return ;
            }

            var n = newnamel(f.Pos, f.Sym);
            f.Nname = asTypesNode(n);
            n.Type = f.Type;
            n.SetIsDDD(f.IsDDD());
            declare(_addr_n, ctxt);

        }

        private static slice<ptr<Node>> funcstack = default; // stack of previous values of Curfn

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
            if (Curfn == null)
            {
                dclcontext = PEXTERN;
            }

        }

        // structs, functions, and methods.
        // they don't belong here, but where do they belong?
        private static void checkembeddedtype(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return ;
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

        private static ptr<types.Field> structfield(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var lno = lineno;
            lineno = n.Pos;

            if (n.Op != ODCLFIELD)
            {
                Fatalf("structfield: oops %v\n", n);
            }

            var f = types.NewField();
            f.Pos = n.Pos;
            f.Sym = n.Sym;

            if (n.Left != null)
            {
                n.Left = typecheck(n.Left, ctxType);
                n.Type = n.Left.Type;
                n.Left = null;
            }

            f.Type = n.Type;
            if (f.Type == null)
            {
                f.SetBroke(true);
            }

            if (n.Embedded())
            {
                checkembeddedtype(_addr_n.Type);
                f.Embedded = 1L;
            }
            else
            {
                f.Embedded = 0L;
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

            lineno = lno;
            return _addr_f!;

        }

        // checkdupfields emits errors for duplicately named fields or methods in
        // a list of struct or interface types.
        private static void checkdupfields(@string what, params slice<ptr<types.Field>>[] fss)
        {
            fss = fss.Clone();

            var seen = make_map<ptr<types.Sym>, bool>();
            foreach (var (_, fs) in fss)
            {
                foreach (var (_, f) in fs)
                {
                    if (f.Sym == null || f.Sym.IsBlank())
                    {
                        continue;
                    }

                    if (seen[f.Sym])
                    {
                        yyerrorl(f.Pos, "duplicate %s %s", what, f.Sym.Name);
                        continue;
                    }

                    seen[f.Sym] = true;

                }

            }

        }

        // convert a parsed id/type list into
        // a type for struct/interface/arglist
        private static ptr<types.Type> tostruct(slice<ptr<Node>> l)
        {
            var t = types.New(TSTRUCT);

            var fields = make_slice<ptr<types.Field>>(len(l));
            foreach (var (i, n) in l)
            {
                var f = structfield(_addr_n);
                if (f.Broke())
                {
                    t.SetBroke(true);
                }

                fields[i] = f;

            }
            t.SetFields(fields);

            checkdupfields("field", t.FieldSlice());

            if (!t.Broke())
            {
                checkwidth(t);
            }

            return _addr_t!;

        }

        private static ptr<types.Type> tofunargs(slice<ptr<Node>> l, types.Funarg funarg)
        {
            var t = types.New(TSTRUCT);
            t.StructType().Funarg = funarg;

            var fields = make_slice<ptr<types.Field>>(len(l));
            foreach (var (i, n) in l)
            {
                var f = structfield(_addr_n);
                f.SetIsDDD(n.IsDDD());
                if (n.Right != null)
                {
                    n.Right.Type = f.Type;
                    f.Nname = asTypesNode(n.Right);
                }

                if (f.Broke())
                {
                    t.SetBroke(true);
                }

                fields[i] = f;

            }
            t.SetFields(fields);
            return _addr_t!;

        }

        private static ptr<types.Type> tofunargsfield(slice<ptr<types.Field>> fields, types.Funarg funarg)
        {
            var t = types.New(TSTRUCT);
            t.StructType().Funarg = funarg;
            t.SetFields(fields);
            return _addr_t!;
        }

        private static ptr<types.Field> interfacefield(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

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
            // If Sym != nil, then Sym is MethodName and Left is Signature.
            // Otherwise, Left is InterfaceTypeName.
            if (n.Left != null)
            {
                n.Left = typecheck(n.Left, ctxType);
                n.Type = n.Left.Type;
                n.Left = null;
            }

            var f = types.NewField();
            f.Pos = n.Pos;
            f.Sym = n.Sym;
            f.Type = n.Type;
            if (f.Type == null)
            {
                f.SetBroke(true);
            }

            lineno = lno;
            return _addr_f!;

        }

        private static ptr<types.Type> tointerface(slice<ptr<Node>> l)
        {
            if (len(l) == 0L)
            {
                return _addr_types.Types[TINTER]!;
            }

            var t = types.New(TINTER);
            slice<ptr<types.Field>> fields = default;
            foreach (var (_, n) in l)
            {
                var f = interfacefield(_addr_n);
                if (f.Broke())
                {
                    t.SetBroke(true);
                }

                fields = append(fields, f);

            }
            t.SetInterface(fields);
            return _addr_t!;

        }

        private static ptr<Node> fakeRecv()
        {
            return _addr_anonfield(_addr_types.FakeRecvType())!;
        }

        private static ptr<types.Field> fakeRecvField()
        {
            var f = types.NewField();
            f.Type = types.FakeRecvType();
            return _addr_f!;
        }

        // isifacemethod reports whether (field) m is
        // an interface method. Such methods have the
        // special receiver type types.FakeRecvType().
        private static bool isifacemethod(ptr<types.Type> _addr_f)
        {
            ref types.Type f = ref _addr_f.val;

            return f.Recv().Type == types.FakeRecvType();
        }

        // turn a parsed function declaration into a type
        private static ptr<types.Type> functype(ptr<Node> _addr_@this, slice<ptr<Node>> @in, slice<ptr<Node>> @out)
        {
            ref Node @this = ref _addr_@this.val;

            var t = types.New(TFUNC);

            slice<ptr<Node>> rcvr = default;
            if (this != null)
            {
                rcvr = new slice<ptr<Node>>(new ptr<Node>[] { this });
            }

            t.FuncType().Receiver = tofunargs(rcvr, types.FunargRcvr);
            t.FuncType().Params = tofunargs(in, types.FunargParams);
            t.FuncType().Results = tofunargs(out, types.FunargResults);

            checkdupfields("argument", t.Recvs().FieldSlice(), t.Params().FieldSlice(), t.Results().FieldSlice());

            if (t.Recvs().Broke() || t.Results().Broke() || t.Params().Broke())
            {
                t.SetBroke(true);
            }

            t.FuncType().Outnamed = t.NumResults() > 0L && origSym(_addr_t.Results().Field(0L).Sym) != null;

            return _addr_t!;

        }

        private static ptr<types.Type> functypefield(ptr<types.Field> _addr_@this, slice<ptr<types.Field>> @in, slice<ptr<types.Field>> @out)
        {
            ref types.Field @this = ref _addr_@this.val;

            var t = types.New(TFUNC);

            slice<ptr<types.Field>> rcvr = default;
            if (this != null)
            {
                rcvr = new slice<ptr<types.Field>>(new ptr<types.Field>[] { this });
            }

            t.FuncType().Receiver = tofunargsfield(rcvr, types.FunargRcvr);
            t.FuncType().Params = tofunargsfield(in, types.FunargParams);
            t.FuncType().Results = tofunargsfield(out, types.FunargResults);

            t.FuncType().Outnamed = t.NumResults() > 0L && origSym(_addr_t.Results().Field(0L).Sym) != null;

            return _addr_t!;

        }

        // origSym returns the original symbol written by the user.
        private static ptr<types.Sym> origSym(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            if (s == null)
            {
                return _addr_null!;
            }

            if (len(s.Name) > 1L && s.Name[0L] == '~')
            {
                switch (s.Name[1L])
                {
                    case 'r': // originally an unnamed result
                        return _addr_null!;
                        break;
                    case 'b': // originally the blank identifier _
                        // TODO(mdempsky): Does s.Pkg matter here?
                        return _addr_nblank.Sym!;
                        break;
                }
                return _addr_s!;

            }

            if (strings.HasPrefix(s.Name, ".anon"))
            { 
                // originally an unnamed or _ name (see subr.go: structargs)
                return _addr_null!;

            }

            return _addr_s!;

        }

        // methodSym returns the method symbol representing a method name
        // associated with a specific receiver type.
        //
        // Method symbols can be used to distinguish the same method appearing
        // in different method sets. For example, T.M and (*T).M have distinct
        // method symbols.
        //
        // The returned symbol will be marked as a function.
        private static ptr<types.Sym> methodSym(ptr<types.Type> _addr_recv, ptr<types.Sym> _addr_msym)
        {
            ref types.Type recv = ref _addr_recv.val;
            ref types.Sym msym = ref _addr_msym.val;

            var sym = methodSymSuffix(_addr_recv, _addr_msym, "");
            sym.SetFunc(true);
            return _addr_sym!;
        }

        // methodSymSuffix is like methodsym, but allows attaching a
        // distinguisher suffix. To avoid collisions, the suffix must not
        // start with a letter, number, or period.
        private static ptr<types.Sym> methodSymSuffix(ptr<types.Type> _addr_recv, ptr<types.Sym> _addr_msym, @string suffix)
        {
            ref types.Type recv = ref _addr_recv.val;
            ref types.Sym msym = ref _addr_msym.val;

            if (msym.IsBlank())
            {
                Fatalf("blank method name");
            }

            var rsym = recv.Sym;
            if (recv.IsPtr())
            {
                if (rsym != null)
                {
                    Fatalf("declared pointer receiver type: %v", recv);
                }

                rsym = recv.Elem().Sym;

            } 

            // Find the package the receiver type appeared in. For
            // anonymous receiver types (i.e., anonymous structs with
            // embedded fields), use the "go" pseudo-package instead.
            var rpkg = gopkg;
            if (rsym != null)
            {
                rpkg = rsym.Pkg;
            }

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            if (recv.IsPtr())
            { 
                // The parentheses aren't really necessary, but
                // they're pretty traditional at this point.
                fmt.Fprintf(_addr_b, "(%-S)", recv);

            }
            else
            {
                fmt.Fprintf(_addr_b, "%-S", recv);
            } 

            // A particular receiver type may have multiple non-exported
            // methods with the same name. To disambiguate them, include a
            // package qualifier for names that came from a different
            // package than the receiver type.
            if (!types.IsExported(msym.Name) && msym.Pkg != rpkg)
            {
                b.WriteString(".");
                b.WriteString(msym.Pkg.Prefix);
            }

            b.WriteString(".");
            b.WriteString(msym.Name);
            b.WriteString(suffix);

            return _addr_rpkg.LookupBytes(b.Bytes())!;

        }

        // Add a method, declared as a function.
        // - msym is the method symbol
        // - t is function type (with receiver)
        // Returns a pointer to the existing or added Field; or nil if there's an error.
        private static ptr<types.Field> addmethod(ptr<types.Sym> _addr_msym, ptr<types.Type> _addr_t, bool local, bool nointerface)
        {
            ref types.Sym msym = ref _addr_msym.val;
            ref types.Type t = ref _addr_t.val;

            if (msym == null)
            {
                Fatalf("no method symbol");
            } 

            // get parent type sym
            var rf = t.Recv(); // ptr to this structure
            if (rf == null)
            {
                yyerror("missing receiver");
                return _addr_null!;
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
                        return _addr_null!;
                    }

                    t = t.Elem();

                }


                if (t == null || t.Broke())                 else if (t.Sym == null) 
                    yyerror("invalid receiver type %v (%v is not a defined type)", pa, t);
                else if (t.IsPtr()) 
                    yyerror("invalid receiver type %v (%v is a pointer type)", pa, t);
                else if (t.IsInterface()) 
                    yyerror("invalid receiver type %v (%v is an interface type)", pa, t);
                else 
                    // Should have picked off all the reasons above,
                    // but just in case, fall back to generic error.
                    yyerror("invalid receiver type %v (%L / %L)", pa, pa, t);
                                return _addr_null!;

            }

            if (local && mt.Sym.Pkg != localpkg)
            {
                yyerror("cannot define new methods on non-local type %v", mt);
                return _addr_null!;
            }

            if (msym.IsBlank())
            {
                return _addr_null!;
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
                            f.SetBroke(true);
                            return _addr_null!;
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
                    // types.Identical only checks that incoming and result parameters match,
                    // so explicitly check that the receiver parameters match too.
                    if (!types.Identical(t, f.Type) || !types.Identical(t.Recv().Type, f.Type.Recv().Type))
                    {
                        yyerror("method redeclared: %v.%v\n\t%v\n\t%v", mt, msym, f.Type, t);
                    }

                    return _addr_f!;

                }

                f = f__prev1;
            }

            var f = types.NewField();
            f.Pos = lineno;
            f.Sym = msym;
            f.Type = t;
            f.SetNointerface(nointerface);

            mt.Methods().Append(f);
            return _addr_f!;

        }

        private static @string funcsymname(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            return s.Name + "·f";
        }

        // funcsym returns s·f.
        private static ptr<types.Sym> funcsym(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;
 
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
            var (sf, existed) = s.Pkg.LookupOK(funcsymname(_addr_s)); 
            // Don't export s·f when compiling for dynamic linking.
            // When dynamically linking, the necessary function
            // symbols will be created explicitly with makefuncsym.
            // See the makefuncsym comment for details.
            if (!Ctxt.Flag_dynlink && !existed)
            {
                funcsyms = append(funcsyms, s);
            }

            funcsymsmu.Unlock();
            return _addr_sf!;

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
        private static void makefuncsym(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            if (!Ctxt.Flag_dynlink)
            {
                Fatalf("makefuncsym dynlink");
            }

            if (s.IsBlank())
            {
                return ;
            }

            if (compiling_runtime && (s.Name == "getg" || s.Name == "getclosureptr" || s.Name == "getcallerpc" || s.Name == "getcallersp"))
            { 
                // runtime.getg(), getclosureptr(), getcallerpc(), and
                // getcallersp() are not real functions and so do not
                // get funcsyms.
                return ;

            }

            {
                var (_, existed) = s.Pkg.LookupOK(funcsymname(_addr_s));

                if (!existed)
                {
                    funcsyms = append(funcsyms, s);
                }

            }

        }

        // disableExport prevents sym from being included in package export
        // data. To be effectual, it must be called before declare.
        private static void disableExport(ptr<types.Sym> _addr_sym)
        {
            ref types.Sym sym = ref _addr_sym.val;

            sym.SetOnExportList(true);
        }

        private static ptr<Node> dclfunc(ptr<types.Sym> _addr_sym, ptr<Node> _addr_tfn)
        {
            ref types.Sym sym = ref _addr_sym.val;
            ref Node tfn = ref _addr_tfn.val;

            if (tfn.Op != OTFUNC)
            {
                Fatalf("expected OTFUNC node, got %v", tfn);
            }

            var fn = nod(ODCLFUNC, null, null);
            fn.Func.Nname = newfuncnamel(lineno, _addr_sym);
            fn.Func.Nname.Name.Defn = fn;
            fn.Func.Nname.Name.Param.Ntype = tfn;
            declare(_addr_fn.Func.Nname, PFUNC);
            funchdr(_addr_fn);
            fn.Func.Nname.Name.Param.Ntype = typecheck(fn.Func.Nname.Name.Param.Ntype, ctxType);
            return _addr_fn!;

        }

        private partial struct nowritebarrierrecChecker
        {
            public map<ptr<Node>, slice<nowritebarrierrecCall>> extraCalls; // curfn is the current function during AST walks.
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
        private static ptr<nowritebarrierrecChecker> newNowritebarrierrecChecker()
        {
            ptr<nowritebarrierrecChecker> c = addr(new nowritebarrierrecChecker(extraCalls:make(map[*Node][]nowritebarrierrecCall),)); 

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
            return _addr_c!;

        }

        private static bool findExtraCalls(this ptr<nowritebarrierrecChecker> _addr_c, ptr<Node> _addr_n)
        {
            ref nowritebarrierrecChecker c = ref _addr_c.val;
            ref Node n = ref _addr_n.val;

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

            ptr<Node> callee;
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
        private static void recordCall(this ptr<nowritebarrierrecChecker> _addr_c, ptr<Node> _addr_from, ptr<obj.LSym> _addr_to, src.XPos pos)
        {
            ref nowritebarrierrecChecker c = ref _addr_c.val;
            ref Node from = ref _addr_from.val;
            ref obj.LSym to = ref _addr_to.val;

            if (from.Op != ODCLFUNC)
            {
                Fatalf("expected ODCLFUNC, got %v", from);
            } 
            // We record this information on the *Func so this is
            // concurrent-safe.
            var fn = from.Func;
            if (fn.nwbrCalls == null)
            {
                fn.nwbrCalls = @new<nowritebarrierrecCallSym>();
            }

            fn.nwbrCalls.val = append(fn.nwbrCalls.val, new nowritebarrierrecCallSym(to,pos));

        }

        private static void check(this ptr<nowritebarrierrecChecker> _addr_c)
        {
            ref nowritebarrierrecChecker c = ref _addr_c.val;
 
            // We walk the call graph as late as possible so we can
            // capture all calls created by lowering, but this means we
            // only get to see the obj.LSyms of calls. symToFunc lets us
            // get back to the ODCLFUNCs.
            var symToFunc = make_map<ptr<obj.LSym>, ptr<Node>>(); 
            // funcs records the back-edges of the BFS call graph walk. It
            // maps from the ODCLFUNC of each function that must not have
            // write barriers to the call that inhibits them. Functions
            // that are directly marked go:nowritebarrierrec are in this
            // map with a zero-valued nowritebarrierrecCall. This also
            // acts as the set of marks for the BFS of the call graph.
            var funcs = make_map<ptr<Node>, nowritebarrierrecCall>(); 
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
            Action<ptr<Node>, ptr<Node>, src.XPos> enqueue = (src, target, pos) =>
            {
                if (target.Func.Pragma & Yeswritebarrierrec != 0L)
                { 
                    // Don't flow into this function.
                    return ;

                }

                {
                    var (_, ok) = funcs[target];

                    if (ok)
                    { 
                        // Already found a path to target.
                        return ;

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
                    ref bytes.Buffer err = ref heap(out ptr<bytes.Buffer> _addr_err);
                    var call = funcs[fn];
                    while (call.target != null)
                    {
                        fmt.Fprintf(_addr_err, "\n\t%v: called by %v", linestr(call.lineno), call.target.Func.Nname);
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

                    foreach (var (_, __callee) in fn.Func.nwbrCalls.val)
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
