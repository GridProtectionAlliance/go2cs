// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:26:00 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\closure.go
using syntax = go.cmd.compile.@internal.syntax_package;
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ref Node funcLit(this ref noder p, ref syntax.FuncLit expr)
        {
            var ntype = p.typeExpr(expr.Type);

            var n = p.nod(expr, OCLOSURE, null, null);
            n.Func.SetIsHiddenClosure(Curfn != null);
            n.Func.Ntype = ntype;
            n.Func.Depth = funcdepth;
            n.Func.Outerfunc = Curfn;

            var old = p.funchdr(n); 

            // steal ntype's argument names and
            // leave a fresh copy in their place.
            // references to these variables need to
            // refer to the variables in the external
            // function declared below; see walkclosure.
            n.List.Set(ntype.List.Slice());
            n.Rlist.Set(ntype.Rlist.Slice());

            ntype.List.Set(null);
            ntype.Rlist.Set(null);
            foreach (var (_, n1) in n.List.Slice())
            {
                var name = n1.Left;
                if (name != null)
                {
                    name = newname(name.Sym);
                }
                var a = nod(ODCLFIELD, name, n1.Right);
                a.SetIsddd(n1.Isddd());
                if (name != null)
                {
                    name.SetIsddd(a.Isddd());
                }
                ntype.List.Append(a);
            }            foreach (var (_, n2) in n.Rlist.Slice())
            {
                name = n2.Left;
                if (name != null)
                {
                    name = newname(name.Sym);
                }
                ntype.Rlist.Append(nod(ODCLFIELD, name, n2.Right));
            }            var body = p.stmts(expr.Body.List);

            lineno = Ctxt.PosTable.XPos(expr.Body.Rbrace);
            if (len(body) == 0L)
            {
                body = new slice<ref Node>(new ref Node[] { nod(OEMPTY,nil,nil) });
            }
            n.Nbody.Set(body);
            n.Func.Endlineno = lineno;
            p.funcbody(old); 

            // closure-specific variables are hanging off the
            // ordinary ones in the symbol table; see oldname.
            // unhook them.
            // make the list of pointers for the closure call.
            foreach (var (_, v) in n.Func.Cvars.Slice())
            { 
                // Unlink from v1; see comment in syntax.go type Param for these fields.
                var v1 = v.Name.Defn;
                v1.Name.Param.Innermost = v.Name.Param.Outer; 

                // If the closure usage of v is not dense,
                // we need to make it dense; now that we're out
                // of the function in which v appeared,
                // look up v.Sym in the enclosing function
                // and keep it around for use in the compiled code.
                //
                // That is, suppose we just finished parsing the innermost
                // closure f4 in this code:
                //
                //    func f() {
                //        v := 1
                //        func() { // f2
                //            use(v)
                //            func() { // f3
                //                func() { // f4
                //                    use(v)
                //                }()
                //            }()
                //        }()
                //    }
                //
                // At this point v.Outer is f2's v; there is no f3's v.
                // To construct the closure f4 from within f3,
                // we need to use f3's v and in this case we need to create f3's v.
                // We are now in the context of f3, so calling oldname(v.Sym)
                // obtains f3's v, creating it if necessary (as it is in the example).
                //
                // capturevars will decide whether to use v directly or &v.
                v.Name.Param.Outer = oldname(v.Sym);
            }            return n;
        }

        private static void typecheckclosure(ref Node func_, long top)
        {
            {
                var ln__prev1 = ln;

                foreach (var (_, __ln) in func_.Func.Cvars.Slice())
                {
                    ln = __ln;
                    var n = ln.Name.Defn;
                    if (!n.Name.Captured())
                    {
                        n.Name.SetCaptured(true);
                        if (n.Name.Decldepth == 0L)
                        {
                            Fatalf("typecheckclosure: var %S does not have decldepth assigned", n);
                        } 

                        // Ignore assignments to the variable in straightline code
                        // preceding the first capturing by a closure.
                        if (n.Name.Decldepth == decldepth)
                        {
                            n.SetAssigned(false);
                        }
                    }
                }

                ln = ln__prev1;
            }

            {
                var ln__prev1 = ln;

                foreach (var (_, __ln) in func_.Func.Dcl)
                {
                    ln = __ln;
                    if (ln.Op == ONAME && (ln.Class() == PPARAM || ln.Class() == PPARAMOUT))
                    {
                        ln.Name.Decldepth = 1L;
                    }
                }

                ln = ln__prev1;
            }

            var oldfn = Curfn;
            func_.Func.Ntype = typecheck(func_.Func.Ntype, Etype);
            func_.Type = func_.Func.Ntype.Type;
            func_.Func.Top = top; 

            // Type check the body now, but only if we're inside a function.
            // At top level (in a variable initialization: curfn==nil) we're not
            // ready to type check code yet; we'll check it later, because the
            // underlying closure function we create is added to xtop.
            if (Curfn != null && func_.Type != null)
            {
                Curfn = func_;
                var olddd = decldepth;
                decldepth = 1L;
                typecheckslice(func_.Nbody.Slice(), Etop);
                decldepth = olddd;
                Curfn = oldfn;
            } 

            // Create top-level function
            xtop = append(xtop, makeclosure(func_));
        }

        // closurename returns name for OCLOSURE n.
        // It is not as simple as it ought to be, because we typecheck nested closures
        // starting from the innermost one. So when we check the inner closure,
        // we don't yet have name for the outer closure. This function uses recursion
        // to generate names all the way up if necessary.

        private static long closurename_closgen = default;

        private static ref types.Sym closurename(ref Node n)
        {
            if (n.Sym != null)
            {
                return n.Sym;
            }
            long gen = 0L;
            @string outer = "";
            @string prefix = "";

            if (n.Func.Outerfunc == null) 
                // Global closure.
                outer = "glob.";

                prefix = "func";
                closurename_closgen++;
                gen = closurename_closgen;
            else if (n.Func.Outerfunc.Op == ODCLFUNC) 
                // The outermost closure inside of a named function.
                outer = n.Func.Outerfunc.funcname();

                prefix = "func"; 

                // Yes, functions can be named _.
                // Can't use function closgen in such case,
                // because it would lead to name clashes.
                if (!isblank(n.Func.Outerfunc.Func.Nname))
                {
                    n.Func.Outerfunc.Func.Closgen++;
                    gen = n.Func.Outerfunc.Func.Closgen;
                }
                else
                {
                    closurename_closgen++;
                    gen = closurename_closgen;
                }
            else if (n.Func.Outerfunc.Op == OCLOSURE) 
                // Nested closure, recurse.
                outer = closurename(n.Func.Outerfunc).Name;

                prefix = "";
                n.Func.Outerfunc.Func.Closgen++;
                gen = n.Func.Outerfunc.Func.Closgen;
            else 
                Fatalf("closurename called for %S", n);
                        n.Sym = lookup(fmt.Sprintf("%s.%s%d", outer, prefix, gen));
            return n.Sym;
        }

        private static ref Node makeclosure(ref Node func_)
        { 
            // wrap body in external function
            // that begins by reading closure parameters.
            var xtype = nod(OTFUNC, null, null);

            xtype.List.Set(func_.List.Slice());
            xtype.Rlist.Set(func_.Rlist.Slice()); 

            // create the function
            var xfunc = nod(ODCLFUNC, null, null);
            xfunc.Func.SetIsHiddenClosure(Curfn != null);

            xfunc.Func.Nname = newfuncname(closurename(func_));
            xfunc.Func.Nname.Sym.SetExported(true); // disable export
            xfunc.Func.Nname.Name.Param.Ntype = xtype;
            xfunc.Func.Nname.Name.Defn = xfunc;
            declare(xfunc.Func.Nname, PFUNC);
            xfunc.Func.Nname.Name.Funcdepth = func_.Func.Depth;
            xfunc.Func.Depth = func_.Func.Depth;
            xfunc.Func.Endlineno = func_.Func.Endlineno;
            if (Ctxt.Flag_dynlink)
            {
                makefuncsym(xfunc.Func.Nname.Sym);
            }
            xfunc.Nbody.Set(func_.Nbody.Slice());
            xfunc.Func.Dcl = append(func_.Func.Dcl, xfunc.Func.Dcl);
            xfunc.Func.Parents = func_.Func.Parents;
            xfunc.Func.Marks = func_.Func.Marks;
            func_.Func.Dcl = null;
            func_.Func.Parents = null;
            func_.Func.Marks = null;
            if (xfunc.Nbody.Len() == 0L)
            {
                Fatalf("empty body - won't generate any code");
            }
            xfunc = typecheck(xfunc, Etop);

            xfunc.Func.Closure = func_;
            func_.Func.Closure = xfunc;

            func_.Nbody.Set(null);
            func_.List.Set(null);
            func_.Rlist.Set(null);

            return xfunc;
        }

        // capturevarscomplete is set to true when the capturevars phase is done.
        private static bool capturevarscomplete = default;

        // capturevars is called in a separate phase after all typechecking is done.
        // It decides whether each variable captured by a closure should be captured
        // by value or by reference.
        // We use value capturing for values <= 128 bytes that are never reassigned
        // after capturing (effectively constant).
        private static void capturevars(ref Node xfunc)
        {
            var lno = lineno;
            lineno = xfunc.Pos;

            var func_ = xfunc.Func.Closure;
            func_.Func.Enter.Set(null);
            foreach (var (_, v) in func_.Func.Cvars.Slice())
            {
                if (v.Type == null)
                { 
                    // if v->type is nil, it means v looked like it was
                    // going to be used in the closure but wasn't.
                    // this happens because when parsing a, b, c := f()
                    // the a, b, c gets parsed as references to older
                    // a, b, c before the parser figures out this is a
                    // declaration.
                    v.Op = OXXX;

                    continue;
                } 

                // type check the & of closed variables outside the closure,
                // so that the outer frame also grabs them and knows they escape.
                dowidth(v.Type);

                var outer = v.Name.Param.Outer;
                var outermost = v.Name.Defn; 

                // out parameters will be assigned to implicitly upon return.
                if (outer.Class() != PPARAMOUT && !outermost.Addrtaken() && !outermost.Assigned() && v.Type.Width <= 128L)
                {
                    v.Name.SetByval(true);
                }
                else
                {
                    outermost.SetAddrtaken(true);
                    outer = nod(OADDR, outer, null);
                }
                if (Debug['m'] > 1L)
                {
                    ref types.Sym name = default;
                    if (v.Name.Curfn != null && v.Name.Curfn.Func.Nname != null)
                    {
                        name = v.Name.Curfn.Func.Nname.Sym;
                    }
                    @string how = "ref";
                    if (v.Name.Byval())
                    {
                        how = "value";
                    }
                    Warnl(v.Pos, "%v capturing by %s: %v (addr=%v assign=%v width=%d)", name, how, v.Sym, outermost.Addrtaken(), outermost.Assigned(), int32(v.Type.Width));
                }
                outer = typecheck(outer, Erv);
                func_.Func.Enter.Append(outer);
            }
            lineno = lno;
        }

        // transformclosure is called in a separate phase after escape analysis.
        // It transform closure bodies to properly reference captured variables.
        private static void transformclosure(ref Node xfunc)
        {
            var lno = lineno;
            lineno = xfunc.Pos;
            var func_ = xfunc.Func.Closure;

            if (func_.Func.Top & Ecall != 0L)
            { 
                // If the closure is directly called, we transform it to a plain function call
                // with variables passed as args. This avoids allocation of a closure object.
                // Here we do only a part of the transformation. Walk of OCALLFUNC(OCLOSURE)
                // will complete the transformation later.
                // For illustration, the following closure:
                //    func(a int) {
                //        println(byval)
                //        byref++
                //    }(42)
                // becomes:
                //    func(byval int, &byref *int, a int) {
                //        println(byval)
                //        (*&byref)++
                //    }(byval, &byref, 42)

                // f is ONAME of the actual function.
                var f = xfunc.Func.Nname; 

                // We are going to insert captured variables before input args.
                slice<ref types.Field> @params = default;
                slice<ref Node> decls = default;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in func_.Func.Cvars.Slice())
                    {
                        v = __v;
                        if (v.Op == OXXX)
                        {
                            continue;
                        }
                        var fld = types.NewField();
                        fld.Funarg = types.FunargParams;
                        if (v.Name.Byval())
                        { 
                            // If v is captured by value, we merely downgrade it to PPARAM.
                            v.SetClass(PPARAM);
                            fld.Nname = asTypesNode(v);
                        }
                        else
                        { 
                            // If v of type T is captured by reference,
                            // we introduce function param &v *T
                            // and v remains PAUTOHEAP with &v heapaddr
                            // (accesses will implicitly deref &v).
                            var addr = newname(lookup("&" + v.Sym.Name));
                            addr.Type = types.NewPtr(v.Type);
                            addr.SetClass(PPARAM);
                            v.Name.Param.Heapaddr = addr;
                            fld.Nname = asTypesNode(addr);
                        }
                        fld.Type = asNode(fld.Nname).Type;
                        fld.Sym = asNode(fld.Nname).Sym;

                        params = append(params, fld);
                        decls = append(decls, asNode(fld.Nname));
                    }
            else


                    v = v__prev1;
                }

                if (len(params) > 0L)
                { 
                    // Prepend params and decls.
                    f.Type.Params().SetFields(append(params, f.Type.Params().FieldSlice()));
                    xfunc.Func.Dcl = append(decls, xfunc.Func.Dcl);
                }
                dowidth(f.Type);
                xfunc.Type = f.Type; // update type of ODCLFUNC
            }            { 
                // The closure is not called, so it is going to stay as closure.
                slice<ref Node> body = default;
                var offset = int64(Widthptr);
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in func_.Func.Cvars.Slice())
                    {
                        v = __v;
                        if (v.Op == OXXX)
                        {
                            continue;
                        } 

                        // cv refers to the field inside of closure OSTRUCTLIT.
                        var cv = nod(OCLOSUREVAR, null, null);

                        cv.Type = v.Type;
                        if (!v.Name.Byval())
                        {
                            cv.Type = types.NewPtr(v.Type);
                        }
                        offset = Rnd(offset, int64(cv.Type.Align));
                        cv.Xoffset = offset;
                        offset += cv.Type.Width;

                        if (v.Name.Byval() && v.Type.Width <= int64(2L * Widthptr))
                        { 
                            // If it is a small variable captured by value, downgrade it to PAUTO.
                            v.SetClass(PAUTO);
                            xfunc.Func.Dcl = append(xfunc.Func.Dcl, v);
                            body = append(body, nod(OAS, v, cv));
                        }
                        else
                        { 
                            // Declare variable holding addresses taken from closure
                            // and initialize in entry prologue.
                            addr = newname(lookup("&" + v.Sym.Name));
                            addr.Type = types.NewPtr(v.Type);
                            addr.SetClass(PAUTO);
                            addr.Name.SetUsed(true);
                            addr.Name.Curfn = xfunc;
                            xfunc.Func.Dcl = append(xfunc.Func.Dcl, addr);
                            v.Name.Param.Heapaddr = addr;
                            if (v.Name.Byval())
                            {
                                cv = nod(OADDR, cv, null);
                            }
                            body = append(body, nod(OAS, addr, cv));
                        }
                    }

                    v = v__prev1;
                }

                if (len(body) > 0L)
                {
                    typecheckslice(body, Etop);
                    walkstmtlist(body);
                    xfunc.Func.Enter.Set(body);
                    xfunc.Func.SetNeedctxt(true);
                }
            }
            lineno = lno;
        }

        // hasemptycvars returns true iff closure func_ has an
        // empty list of captured vars. OXXX nodes don't count.
        private static bool hasemptycvars(ref Node func_)
        {
            foreach (var (_, v) in func_.Func.Cvars.Slice())
            {
                if (v.Op == OXXX)
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        // closuredebugruntimecheck applies boilerplate checks for debug flags
        // and compiling runtime
        private static void closuredebugruntimecheck(ref Node r)
        {
            if (Debug_closure > 0L)
            {
                if (r.Esc == EscHeap)
                {
                    Warnl(r.Pos, "heap closure, captured vars = %v", r.Func.Cvars);
                }
                else
                {
                    Warnl(r.Pos, "stack closure, captured vars = %v", r.Func.Cvars);
                }
            }
            if (compiling_runtime && r.Esc == EscHeap)
            {
                yyerrorl(r.Pos, "heap-allocated closure, not allowed in runtime.");
            }
        }

        private static ref Node walkclosure(ref Node func_, ref Nodes init)
        { 
            // If no closure vars, don't bother wrapping.
            if (hasemptycvars(func_))
            {
                if (Debug_closure > 0L)
                {
                    Warnl(func_.Pos, "closure converted to global");
                }
                return func_.Func.Closure.Func.Nname;
            }
            closuredebugruntimecheck(func_); 

            // Create closure in the form of a composite literal.
            // supposing the closure captures an int i and a string s
            // and has one float64 argument and no results,
            // the generated code looks like:
            //
            //    clos = &struct{.F uintptr; i *int; s *string}{func.1, &i, &s}
            //
            // The use of the struct provides type information to the garbage
            // collector so that it can walk the closure. We could use (in this case)
            // [3]unsafe.Pointer instead, but that would leave the gc in the dark.
            // The information appears in the binary in the form of type descriptors;
            // the struct is unnamed so that closures in multiple packages with the
            // same struct type can share the descriptor.

            ref Node fields = new slice<ref Node>(new ref Node[] { namedfield(".F",types.Types[TUINTPTR]) });
            foreach (var (_, v) in func_.Func.Cvars.Slice())
            {
                if (v.Op == OXXX)
                {
                    continue;
                }
                var typ = v.Type;
                if (!v.Name.Byval())
                {
                    typ = types.NewPtr(typ);
                }
                fields = append(fields, symfield(v.Sym, typ));
            }
            typ = tostruct(fields);
            typ.SetNoalg(true);

            var clos = nod(OCOMPLIT, null, nod(OIND, typenod(typ), null));
            clos.Esc = func_.Esc;
            clos.Right.SetImplicit(true);
            clos.List.Set(append(new slice<ref Node>(new ref Node[] { nod(OCFUNC,func_.Func.Closure.Func.Nname,nil) }), func_.Func.Enter.Slice())); 

            // Force type conversion from *struct to the func type.
            clos = nod(OCONVNOP, clos, null);
            clos.Type = func_.Type;

            clos = typecheck(clos, Erv); 

            // typecheck will insert a PTRLIT node under CONVNOP,
            // tag it with escape analysis result.
            clos.Left.Esc = func_.Esc; 

            // non-escaping temp to use, if any.
            // orderexpr did not compute the type; fill it in now.
            {
                var x = prealloc[func_];

                if (x != null)
                {
                    x.Type = clos.Left.Left.Type;
                    x.Orig.Type = x.Type;
                    clos.Left.Right = x;
                    delete(prealloc, func_);
                }

            }

            return walkexpr(clos, init);
        }

        private static void typecheckpartialcall(ref Node fn, ref types.Sym sym)
        {

            if (fn.Op == ODOTINTER || fn.Op == ODOTMETH) 
                break;
            else 
                Fatalf("invalid typecheckpartialcall");
            // Create top-level function.
            var xfunc = makepartialcall(fn, fn.Type, sym);
            fn.Func = xfunc.Func;
            fn.Right = newname(sym);
            fn.Op = OCALLPART;
            fn.Type = xfunc.Type;
        }

        private static ref types.Pkg makepartialcall_gopkg = default;

        private static ref Node makepartialcall(ref Node fn, ref types.Type t0, ref types.Sym meth)
        {
            @string p = default;

            var rcvrtype = fn.Left.Type;
            if (exportname(meth.Name))
            {
                p = fmt.Sprintf("(%-S).%s-fm", rcvrtype, meth.Name);
            }
            else
            {
                p = fmt.Sprintf("(%-S).(%-v)-fm", rcvrtype, meth);
            }
            var basetype = rcvrtype;
            if (rcvrtype.IsPtr())
            {
                basetype = basetype.Elem();
            }
            if (!basetype.IsInterface() && basetype.Sym == null)
            {
                Fatalf("missing base type for %v", rcvrtype);
            }
            ref types.Pkg spkg = default;
            if (basetype.Sym != null)
            {
                spkg = basetype.Sym.Pkg;
            }
            if (spkg == null)
            {
                if (makepartialcall_gopkg == null)
                {
                    makepartialcall_gopkg = types.NewPkg("go", "");
                }
                spkg = makepartialcall_gopkg;
            }
            var sym = spkg.Lookup(p);

            if (sym.Uniq())
            {
                return asNode(sym.Def);
            }
            sym.SetUniq(true);

            var savecurfn = Curfn;
            Curfn = null;

            var xtype = nod(OTFUNC, null, null);
            slice<ref Node> l = default;
            slice<ref Node> callargs = default;
            var ddd = false;
            var xfunc = nod(ODCLFUNC, null, null);
            Curfn = xfunc;
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in t0.Params().Fields().Slice())
                {
                    i = __i;
                    t = __t;
                    var n = newname(lookupN("a", i));
                    n.SetClass(PPARAM);
                    xfunc.Func.Dcl = append(xfunc.Func.Dcl, n);
                    callargs = append(callargs, n);
                    var fld = nod(ODCLFIELD, n, typenod(t.Type));
                    if (t.Isddd())
                    {
                        fld.SetIsddd(true);
                        ddd = true;
                    }
                    l = append(l, fld);
                }

                i = i__prev1;
                t = t__prev1;
            }

            xtype.List.Set(l);
            l = null;
            slice<ref Node> retargs = default;
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in t0.Results().Fields().Slice())
                {
                    i = __i;
                    t = __t;
                    n = newname(lookupN("r", i));
                    n.SetClass(PPARAMOUT);
                    xfunc.Func.Dcl = append(xfunc.Func.Dcl, n);
                    retargs = append(retargs, n);
                    l = append(l, nod(ODCLFIELD, n, typenod(t.Type)));
                }

                i = i__prev1;
                t = t__prev1;
            }

            xtype.Rlist.Set(l);

            xfunc.Func.SetDupok(true);
            xfunc.Func.Nname = newfuncname(sym);
            xfunc.Func.Nname.Sym.SetExported(true); // disable export
            xfunc.Func.Nname.Name.Param.Ntype = xtype;
            xfunc.Func.Nname.Name.Defn = xfunc;
            declare(xfunc.Func.Nname, PFUNC); 

            // Declare and initialize variable holding receiver.

            xfunc.Func.SetNeedctxt(true);
            var cv = nod(OCLOSUREVAR, null, null);
            cv.Xoffset = int64(Widthptr);
            cv.Type = rcvrtype;
            if (int(cv.Type.Align) > Widthptr)
            {
                cv.Xoffset = int64(cv.Type.Align);
            }
            var ptr = newname(lookup("rcvr"));
            ptr.SetClass(PAUTO);
            ptr.Name.SetUsed(true);
            ptr.Name.Curfn = xfunc;
            xfunc.Func.Dcl = append(xfunc.Func.Dcl, ptr);
            slice<ref Node> body = default;
            if (rcvrtype.IsPtr() || rcvrtype.IsInterface())
            {
                ptr.Type = rcvrtype;
                body = append(body, nod(OAS, ptr, cv));
            }
            else
            {
                ptr.Type = types.NewPtr(rcvrtype);
                body = append(body, nod(OAS, ptr, nod(OADDR, cv, null)));
            }
            var call = nod(OCALL, nodSym(OXDOT, ptr, meth), null);
            call.List.Set(callargs);
            call.SetIsddd(ddd);
            if (t0.NumResults() == 0L)
            {
                body = append(body, call);
            }
            else
            {
                n = nod(OAS2, null, null);
                n.List.Set(retargs);
                n.Rlist.Set1(call);
                body = append(body, n);
                n = nod(ORETURN, null, null);
                body = append(body, n);
            }
            xfunc.Nbody.Set(body);

            xfunc = typecheck(xfunc, Etop);
            sym.Def = asTypesNode(xfunc);
            xtop = append(xtop, xfunc);
            Curfn = savecurfn;

            return xfunc;
        }

        private static ref Node walkpartialcall(ref Node n, ref Nodes init)
        { 
            // Create closure in the form of a composite literal.
            // For x.M with receiver (x) type T, the generated code looks like:
            //
            //    clos = &struct{F uintptr; R T}{M.T·f, x}
            //
            // Like walkclosure above.

            if (n.Left.Type.IsInterface())
            { 
                // Trigger panic for method on nil interface now.
                // Otherwise it happens in the wrapper and is confusing.
                n.Left = cheapexpr(n.Left, init);

                checknil(n.Left, init);
            }
            var typ = tostruct(new slice<ref Node>(new ref Node[] { namedfield("F",types.Types[TUINTPTR]), namedfield("R",n.Left.Type) }));
            typ.SetNoalg(true);

            var clos = nod(OCOMPLIT, null, nod(OIND, typenod(typ), null));
            clos.Esc = n.Esc;
            clos.Right.SetImplicit(true);
            clos.List.Set1(nod(OCFUNC, n.Func.Nname, null));
            clos.List.Append(n.Left); 

            // Force type conversion from *struct to the func type.
            clos = nod(OCONVNOP, clos, null);
            clos.Type = n.Type;

            clos = typecheck(clos, Erv); 

            // typecheck will insert a PTRLIT node under CONVNOP,
            // tag it with escape analysis result.
            clos.Left.Esc = n.Esc; 

            // non-escaping temp to use, if any.
            // orderexpr did not compute the type; fill it in now.
            {
                var x = prealloc[n];

                if (x != null)
                {
                    x.Type = clos.Left.Left.Type;
                    x.Orig.Type = x.Type;
                    clos.Left.Right = x;
                    delete(prealloc, n);
                }

            }

            return walkexpr(clos, init);
        }
    }
}}}}
