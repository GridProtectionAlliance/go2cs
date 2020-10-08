// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:05 UTC
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
        private static ptr<Node> funcLit(this ptr<noder> _addr_p, ptr<syntax.FuncLit> _addr_expr)
        {
            ref noder p = ref _addr_p.val;
            ref syntax.FuncLit expr = ref _addr_expr.val;

            var xtype = p.typeExpr(expr.Type);
            var ntype = p.typeExpr(expr.Type);

            var xfunc = p.nod(expr, ODCLFUNC, null, null);
            xfunc.Func.SetIsHiddenClosure(Curfn != null);
            xfunc.Func.Nname = newfuncnamel(p.pos(expr), nblank.Sym); // filled in by typecheckclosure
            xfunc.Func.Nname.Name.Param.Ntype = xtype;
            xfunc.Func.Nname.Name.Defn = xfunc;

            var clo = p.nod(expr, OCLOSURE, null, null);
            clo.Func.Ntype = ntype;

            xfunc.Func.Closure = clo;
            clo.Func.Closure = xfunc;

            p.funcBody(xfunc, expr.Body); 

            // closure-specific variables are hanging off the
            // ordinary ones in the symbol table; see oldname.
            // unhook them.
            // make the list of pointers for the closure call.
            foreach (var (_, v) in xfunc.Func.Cvars.Slice())
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

            }            return _addr_clo!;

        }

        private static void typecheckclosure(ptr<Node> _addr_clo, long top)
        {
            ref Node clo = ref _addr_clo.val;

            var xfunc = clo.Func.Closure; 
            // Set current associated iota value, so iota can be used inside
            // function in ConstSpec, see issue #22344
            {
                var x = getIotaValue();

                if (x >= 0L)
                {
                    xfunc.SetIota(x);
                }

            }


            clo.Func.Ntype = typecheck(clo.Func.Ntype, ctxType);
            clo.Type = clo.Func.Ntype.Type;
            clo.Func.Top = top; 

            // Do not typecheck xfunc twice, otherwise, we will end up pushing
            // xfunc to xtop multiple times, causing initLSym called twice.
            // See #30709
            if (xfunc.Typecheck() == 1L)
            {
                return ;
            }

            foreach (var (_, ln) in xfunc.Func.Cvars.Slice())
            {
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
                        n.Name.SetAssigned(false);
                    }

                }

            }
            xfunc.Func.Nname.Sym = closurename(_addr_Curfn);
            disableExport(xfunc.Func.Nname.Sym);
            declare(xfunc.Func.Nname, PFUNC);
            xfunc = typecheck(xfunc, ctxStmt); 

            // Type check the body now, but only if we're inside a function.
            // At top level (in a variable initialization: curfn==nil) we're not
            // ready to type check code yet; we'll check it later, because the
            // underlying closure function we create is added to xtop.
            if (Curfn != null && clo.Type != null)
            {
                var oldfn = Curfn;
                Curfn = xfunc;
                var olddd = decldepth;
                decldepth = 1L;
                typecheckslice(xfunc.Nbody.Slice(), ctxStmt);
                decldepth = olddd;
                Curfn = oldfn;
            }

            xtop = append(xtop, xfunc);

        }

        // globClosgen is like Func.Closgen, but for the global scope.
        private static long globClosgen = default;

        // closurename generates a new unique name for a closure within
        // outerfunc.
        private static ptr<types.Sym> closurename(ptr<Node> _addr_outerfunc)
        {
            ref Node outerfunc = ref _addr_outerfunc.val;

            @string outer = "glob.";
            @string prefix = "func";
            var gen = _addr_globClosgen;

            if (outerfunc != null)
            {
                if (outerfunc.Func.Closure != null)
                {
                    prefix = "";
                }

                outer = outerfunc.funcname(); 

                // There may be multiple functions named "_". In those
                // cases, we can't use their individual Closgens as it
                // would lead to name clashes.
                if (!outerfunc.Func.Nname.isBlank())
                {
                    gen = _addr_outerfunc.Func.Closgen;
                }

            }

            gen.val++;
            return _addr_lookup(fmt.Sprintf("%s.%s%d", outer, prefix, gen.val))!;

        }

        // capturevarscomplete is set to true when the capturevars phase is done.
        private static bool capturevarscomplete = default;

        // capturevars is called in a separate phase after all typechecking is done.
        // It decides whether each variable captured by a closure should be captured
        // by value or by reference.
        // We use value capturing for values <= 128 bytes that are never reassigned
        // after capturing (effectively constant).
        private static void capturevars(ptr<Node> _addr_xfunc)
        {
            ref Node xfunc = ref _addr_xfunc.val;

            var lno = lineno;
            lineno = xfunc.Pos;

            var clo = xfunc.Func.Closure;
            var cvars = xfunc.Func.Cvars.Slice();
            var @out = cvars[..0L];
            foreach (var (_, v) in cvars)
            {
                if (v.Type == null)
                { 
                    // If v.Type is nil, it means v looked like it
                    // was going to be used in the closure, but
                    // isn't. This happens in struct literals like
                    // s{f: x} where we can't distinguish whether
                    // f is a field identifier or expression until
                    // resolving s.
                    continue;

                }

                out = append(out, v); 

                // type check the & of closed variables outside the closure,
                // so that the outer frame also grabs them and knows they escape.
                dowidth(v.Type);

                var outer = v.Name.Param.Outer;
                var outermost = v.Name.Defn; 

                // out parameters will be assigned to implicitly upon return.
                if (outermost.Class() != PPARAMOUT && !outermost.Name.Addrtaken() && !outermost.Name.Assigned() && v.Type.Width <= 128L)
                {
                    v.Name.SetByval(true);
                }
                else
                {
                    outermost.Name.SetAddrtaken(true);
                    outer = nod(OADDR, outer, null);
                }

                if (Debug['m'] > 1L)
                {
                    ptr<types.Sym> name;
                    if (v.Name.Curfn != null && v.Name.Curfn.Func.Nname != null)
                    {
                        name = v.Name.Curfn.Func.Nname.Sym;
                    }

                    @string how = "ref";
                    if (v.Name.Byval())
                    {
                        how = "value";
                    }

                    Warnl(v.Pos, "%v capturing by %s: %v (addr=%v assign=%v width=%d)", name, how, v.Sym, outermost.Name.Addrtaken(), outermost.Name.Assigned(), int32(v.Type.Width));

                }

                outer = typecheck(outer, ctxExpr);
                clo.Func.Enter.Append(outer);

            }
            xfunc.Func.Cvars.Set(out);
            lineno = lno;

        }

        // transformclosure is called in a separate phase after escape analysis.
        // It transform closure bodies to properly reference captured variables.
        private static void transformclosure(ptr<Node> _addr_xfunc)
        {
            ref Node xfunc = ref _addr_xfunc.val;

            var lno = lineno;
            lineno = xfunc.Pos;
            var clo = xfunc.Func.Closure;

            if (clo.Func.Top & ctxCallee != 0L)
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
                slice<ptr<types.Field>> @params = default;
                slice<ptr<Node>> decls = default;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in xfunc.Func.Cvars.Slice())
                    {
                        v = __v;
                        if (!v.Name.Byval())
                        { 
                            // If v of type T is captured by reference,
                            // we introduce function param &v *T
                            // and v remains PAUTOHEAP with &v heapaddr
                            // (accesses will implicitly deref &v).
                            var addr = newname(lookup("&" + v.Sym.Name));
                            addr.Type = types.NewPtr(v.Type);
                            v.Name.Param.Heapaddr = addr;
                            v = addr;

                        }

                        v.SetClass(PPARAM);
                        decls = append(decls, v);

                        var fld = types.NewField();
                        fld.Nname = asTypesNode(v);
                        fld.Type = v.Type;
                        fld.Sym = v.Sym;
                        params = append(params, fld);

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
                slice<ptr<Node>> body = default;
                var offset = int64(Widthptr);
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in xfunc.Func.Cvars.Slice())
                    {
                        v = __v; 
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
                    typecheckslice(body, ctxStmt);
                    xfunc.Func.Enter.Set(body);
                    xfunc.Func.SetNeedctxt(true);
                }

            }

            lineno = lno;

        }

        // hasemptycvars reports whether closure clo has an
        // empty list of captured vars.
        private static bool hasemptycvars(ptr<Node> _addr_clo)
        {
            ref Node clo = ref _addr_clo.val;

            var xfunc = clo.Func.Closure;
            return xfunc.Func.Cvars.Len() == 0L;
        }

        // closuredebugruntimecheck applies boilerplate checks for debug flags
        // and compiling runtime
        private static void closuredebugruntimecheck(ptr<Node> _addr_clo)
        {
            ref Node clo = ref _addr_clo.val;

            if (Debug_closure > 0L)
            {
                var xfunc = clo.Func.Closure;
                if (clo.Esc == EscHeap)
                {
                    Warnl(clo.Pos, "heap closure, captured vars = %v", xfunc.Func.Cvars);
                }
                else
                {
                    Warnl(clo.Pos, "stack closure, captured vars = %v", xfunc.Func.Cvars);
                }

            }

            if (compiling_runtime && clo.Esc == EscHeap)
            {
                yyerrorl(clo.Pos, "heap-allocated closure, not allowed in runtime");
            }

        }

        // closureType returns the struct type used to hold all the information
        // needed in the closure for clo (clo must be a OCLOSURE node).
        // The address of a variable of the returned type can be cast to a func.
        private static ptr<types.Type> closureType(ptr<Node> _addr_clo)
        {
            ref Node clo = ref _addr_clo.val;
 
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
            ptr<Node> fields = new slice<ptr<Node>>(new ptr<Node>[] { namedfield(".F",types.Types[TUINTPTR]) });
            foreach (var (_, v) in clo.Func.Closure.Func.Cvars.Slice())
            {
                var typ = v.Type;
                if (!v.Name.Byval())
                {
                    typ = types.NewPtr(typ);
                }

                fields = append(fields, symfield(v.Sym, typ));

            }
            typ = tostruct(fields);
            typ.SetNoalg(true);
            return _addr_typ!;

        }

        private static ptr<Node> walkclosure(ptr<Node> _addr_clo, ptr<Nodes> _addr_init) => func((_, panic, __) =>
        {
            ref Node clo = ref _addr_clo.val;
            ref Nodes init = ref _addr_init.val;

            var xfunc = clo.Func.Closure; 

            // If no closure vars, don't bother wrapping.
            if (hasemptycvars(_addr_clo))
            {
                if (Debug_closure > 0L)
                {
                    Warnl(clo.Pos, "closure converted to global");
                }

                return _addr_xfunc.Func.Nname!;

            }

            closuredebugruntimecheck(_addr_clo);

            var typ = closureType(_addr_clo);

            var clos = nod(OCOMPLIT, null, typenod(typ));
            clos.Esc = clo.Esc;
            clos.List.Set(append(new slice<ptr<Node>>(new ptr<Node>[] { nod(OCFUNC,xfunc.Func.Nname,nil) }), clo.Func.Enter.Slice()));

            clos = nod(OADDR, clos, null);
            clos.Esc = clo.Esc; 

            // Force type conversion from *struct to the func type.
            clos = convnop(clos, clo.Type); 

            // non-escaping temp to use, if any.
            {
                var x = prealloc[clo];

                if (x != null)
                {
                    if (!types.Identical(typ, x.Type))
                    {
                        panic("closure type does not match order's assigned type");
                    }

                    clos.Left.Right = x;
                    delete(prealloc, clo);

                }

            }


            return _addr_walkexpr(clos, init)!;

        });

        private static void typecheckpartialcall(ptr<Node> _addr_fn, ptr<types.Sym> _addr_sym)
        {
            ref Node fn = ref _addr_fn.val;
            ref types.Sym sym = ref _addr_sym.val;


            if (fn.Op == ODOTINTER || fn.Op == ODOTMETH) 
                break;
            else 
                Fatalf("invalid typecheckpartialcall");
            // Create top-level function.
            var xfunc = makepartialcall(_addr_fn, _addr_fn.Type, _addr_sym);
            fn.Func = xfunc.Func;
            fn.Right = newname(sym);
            fn.Op = OCALLPART;
            fn.Type = xfunc.Type;

        }

        private static ptr<Node> makepartialcall(ptr<Node> _addr_fn, ptr<types.Type> _addr_t0, ptr<types.Sym> _addr_meth)
        {
            ref Node fn = ref _addr_fn.val;
            ref types.Type t0 = ref _addr_t0.val;
            ref types.Sym meth = ref _addr_meth.val;

            var rcvrtype = fn.Left.Type;
            var sym = methodSymSuffix(rcvrtype, meth, "-fm");

            if (sym.Uniq())
            {
                return _addr_asNode(sym.Def)!;
            }

            sym.SetUniq(true);

            var savecurfn = Curfn;
            var saveLineNo = lineno;
            Curfn = null; 

            // Set line number equal to the line number where the method is declared.
            ptr<types.Field> m;
            if (lookdot0(meth, rcvrtype, _addr_m, false) == 1L && m.Pos.IsKnown())
            {
                lineno = m.Pos;
            } 
            // Note: !m.Pos.IsKnown() happens for method expressions where
            // the method is implicitly declared. The Error method of the
            // built-in error type is one such method.  We leave the line
            // number at the use of the method expression in this
            // case. See issue 29389.
            var tfn = nod(OTFUNC, null, null);
            tfn.List.Set(structargs(t0.Params(), true));
            tfn.Rlist.Set(structargs(t0.Results(), false));

            disableExport(sym);
            var xfunc = dclfunc(sym, tfn);
            xfunc.Func.SetDupok(true);
            xfunc.Func.SetNeedctxt(true);

            tfn.Type.SetPkg(t0.Pkg()); 

            // Declare and initialize variable holding receiver.

            var cv = nod(OCLOSUREVAR, null, null);
            cv.Type = rcvrtype;
            cv.Xoffset = Rnd(int64(Widthptr), int64(cv.Type.Align));

            var ptr = newname(lookup(".this"));
            declare(ptr, PAUTO);
            ptr.Name.SetUsed(true);
            slice<ptr<Node>> body = default;
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
            call.List.Set(paramNnames(tfn.Type));
            call.SetIsDDD(tfn.Type.IsVariadic());
            if (t0.NumResults() != 0L)
            {
                var n = nod(ORETURN, null, null);
                n.List.Set1(call);
                call = n;
            }

            body = append(body, call);

            xfunc.Nbody.Set(body);
            funcbody();

            xfunc = typecheck(xfunc, ctxStmt);
            sym.Def = asTypesNode(xfunc);
            xtop = append(xtop, xfunc);
            Curfn = savecurfn;
            lineno = saveLineNo;

            return _addr_xfunc!;

        }

        // partialCallType returns the struct type used to hold all the information
        // needed in the closure for n (n must be a OCALLPART node).
        // The address of a variable of the returned type can be cast to a func.
        private static ptr<types.Type> partialCallType(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var t = tostruct(new slice<ptr<Node>>(new ptr<Node>[] { namedfield("F",types.Types[TUINTPTR]), namedfield("R",n.Left.Type) }));
            t.SetNoalg(true);
            return _addr_t!;
        }

        private static ptr<Node> walkpartialcall(ptr<Node> _addr_n, ptr<Nodes> _addr_init) => func((_, panic, __) =>
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
 
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
                n.Left = walkexpr(n.Left, null);

                var tab = nod(OITAB, n.Left, null);
                tab = typecheck(tab, ctxExpr);

                var c = nod(OCHECKNIL, tab, null);
                c.SetTypecheck(1L);
                init.Append(c);

            }

            var typ = partialCallType(_addr_n);

            var clos = nod(OCOMPLIT, null, typenod(typ));
            clos.Esc = n.Esc;
            clos.List.Set2(nod(OCFUNC, n.Func.Nname, null), n.Left);

            clos = nod(OADDR, clos, null);
            clos.Esc = n.Esc; 

            // Force type conversion from *struct to the func type.
            clos = convnop(clos, n.Type); 

            // non-escaping temp to use, if any.
            {
                var x = prealloc[n];

                if (x != null)
                {
                    if (!types.Identical(typ, x.Type))
                    {
                        panic("partial call type does not match order's assigned type");
                    }

                    clos.Left.Right = x;
                    delete(prealloc, n);

                }

            }


            return _addr_walkexpr(clos, init)!;

        });

        // callpartMethod returns the *types.Field representing the method
        // referenced by method value n.
        private static ptr<types.Field> callpartMethod(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OCALLPART)
            {
                Fatalf("expected OCALLPART, got %v", n);
            } 

            // TODO(mdempsky): Optimize this. If necessary,
            // makepartialcall could save m for us somewhere.
            ptr<types.Field> m;
            if (lookdot0(n.Right.Sym, n.Left.Type, _addr_m, false) != 1L)
            {
                Fatalf("failed to find field for OCALLPART");
            }

            return _addr_m!;

        }
    }
}}}}
