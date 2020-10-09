// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:37 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\sinit.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        public partial struct InitEntry
        {
            public long Xoffset; // struct, array only
            public ptr<Node> Expr; // bytes of run-time computed expressions
        }

        public partial struct InitPlan
        {
            public slice<InitEntry> E;
        }

        // An InitSchedule is used to decompose assignment statements into
        // static and dynamic initialization parts. Static initializations are
        // handled by populating variables' linker symbol data, while dynamic
        // initializations are accumulated to be executed in order.
        public partial struct InitSchedule
        {
            public slice<ptr<Node>> @out;
            public map<ptr<Node>, ptr<InitPlan>> initplans;
            public map<ptr<Node>, ptr<Node>> inittemps;
        }

        private static void append(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_n)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            s.@out = append(s.@out, n);
        }

        // staticInit adds an initialization statement n to the schedule.
        private static void staticInit(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_n)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (!s.tryStaticInit(n))
            {
                if (Debug['%'] != 0L)
                {
                    Dump("nonstatic", n);
                }

                s.append(n);

            }

        }

        // tryStaticInit attempts to statically execute an initialization
        // statement and reports whether it succeeded.
        private static bool tryStaticInit(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
 
            // Only worry about simple "l = r" assignments. Multiple
            // variable/expression OAS2 assignments have already been
            // replaced by multiple simple OAS assignments, and the other
            // OAS2* assignments mostly necessitate dynamic execution
            // anyway.
            if (n.Op != OAS)
            {
                return false;
            }

            if (n.Left.isBlank() && candiscard(n.Right))
            {
                return true;
            }

            var lno = setlineno(n);
            defer(() =>
            {
                lineno = lno;
            }());
            return s.staticassign(n.Left, n.Right);

        });

        // like staticassign but we are copying an already
        // initialized value r.
        private static bool staticcopy(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_l, ptr<Node> _addr_r)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;

            if (r.Op != ONAME)
            {
                return false;
            }

            if (r.Class() == PFUNC)
            {
                pfuncsym(l, r);
                return true;
            }

            if (r.Class() != PEXTERN || r.Sym.Pkg != localpkg)
            {
                return false;
            }

            if (r.Name.Defn == null)
            { // probably zeroed but perhaps supplied externally and of unknown value
                return false;

            }

            if (r.Name.Defn.Op != OAS)
            {
                return false;
            }

            if (r.Type.IsString())
            { // perhaps overwritten by cmd/link -X (#34675)
                return false;

            }

            var orig = r;
            r = r.Name.Defn.Right;

            while (r.Op == OCONVNOP && !types.Identical(r.Type, l.Type))
            {
                r = r.Left;
            }



            if (r.Op == ONAME) 
                if (s.staticcopy(l, r))
                {
                    return true;
                } 
                // We may have skipped past one or more OCONVNOPs, so
                // use conv to ensure r is assignable to l (#13263).
                s.append(nod(OAS, l, conv(r, l.Type)));
                return true;
            else if (r.Op == OLITERAL) 
                if (isZero(_addr_r))
                {
                    return true;
                }

                litsym(l, r, int(l.Type.Width));
                return true;
            else if (r.Op == OADDR) 
                {
                    var a__prev1 = a;

                    var a = r.Left;

                    if (a.Op == ONAME)
                    {
                        addrsym(l, a);
                        return true;
                    }

                    a = a__prev1;

                }


            else if (r.Op == OPTRLIT) 

                if (r.Left.Op == OARRAYLIT || r.Left.Op == OSLICELIT || r.Left.Op == OSTRUCTLIT || r.Left.Op == OMAPLIT) 
                    // copy pointer
                    addrsym(l, s.inittemps[r]);
                    return true;
                            else if (r.Op == OSLICELIT) 
                // copy slice
                a = s.inittemps[r];
                slicesym(l, a, r.Right.Int64());
                return true;
            else if (r.Op == OARRAYLIT || r.Op == OSTRUCTLIT) 
                var p = s.initplans[r];

                var n = l.copy();
                foreach (var (i) in p.E)
                {
                    var e = _addr_p.E[i];
                    n.Xoffset = l.Xoffset + e.Xoffset;
                    n.Type = e.Expr.Type;
                    if (e.Expr.Op == OLITERAL)
                    {
                        litsym(n, e.Expr, int(n.Type.Width));
                        continue;
                    }

                    var ll = n.sepcopy();
                    if (s.staticcopy(ll, e.Expr))
                    {
                        continue;
                    } 
                    // Requires computation, but we're
                    // copying someone else's computation.
                    var rr = orig.sepcopy();
                    rr.Type = ll.Type;
                    rr.Xoffset += e.Xoffset;
                    setlineno(rr);
                    s.append(nod(OAS, ll, rr));

                }
                return true;
                        return false;

        }

        private static bool staticassign(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_l, ptr<Node> _addr_r)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;

            while (r.Op == OCONVNOP)
            {
                r = r.Left;
            }



            if (r.Op == ONAME)
            {
                return s.staticcopy(l, r);
                goto __switch_break0;
            }
            if (r.Op == OLITERAL)
            {
                if (isZero(_addr_r))
                {
                    return true;
                }

                litsym(l, r, int(l.Type.Width));
                return true;
                goto __switch_break0;
            }
            if (r.Op == OADDR)
            {
                ref Node nam = ref heap(out ptr<Node> _addr_nam);
                if (stataddr(_addr_nam, _addr_r.Left))
                {
                    addrsym(l, _addr_nam);
                    return true;
                }

                fallthrough = true;

            }
            if (fallthrough || r.Op == OPTRLIT)
            {

                if (r.Left.Op == OARRAYLIT || r.Left.Op == OSLICELIT || r.Left.Op == OMAPLIT || r.Left.Op == OSTRUCTLIT) 
                    // Init pointer.
                    var a = staticname(_addr_r.Left.Type);

                    s.inittemps[r] = a;
                    addrsym(l, a); 

                    // Init underlying literal.
                    if (!s.staticassign(a, r.Left))
                    {
                        s.append(nod(OAS, a, r.Left));
                    }

                    return true;
                //dump("not static ptrlit", r);
                goto __switch_break0;
            }
            if (r.Op == OSTR2BYTES)
            {
                if (l.Class() == PEXTERN && r.Left.Op == OLITERAL)
                {
                    var sval = strlit(r.Left);
                    slicebytes(l, sval);
                    return true;
                }

                goto __switch_break0;
            }
            if (r.Op == OSLICELIT)
            {
                s.initplan(r); 
                // Init slice.
                var bound = r.Right.Int64();
                var ta = types.NewArray(r.Type.Elem(), bound);
                ta.SetNoalg(true);
                a = staticname(_addr_ta);
                s.inittemps[r] = a;
                slicesym(l, a, bound); 
                // Fall through to init underlying array.
                l = a;
                fallthrough = true;

            }
            if (fallthrough || r.Op == OARRAYLIT || r.Op == OSTRUCTLIT)
            {
                s.initplan(r);

                var p = s.initplans[r];
                var n = l.copy();
                foreach (var (i) in p.E)
                {
                    var e = _addr_p.E[i];
                    n.Xoffset = l.Xoffset + e.Xoffset;
                    n.Type = e.Expr.Type;
                    if (e.Expr.Op == OLITERAL)
                    {
                        litsym(n, e.Expr, int(n.Type.Width));
                        continue;
                    }

                    setlineno(e.Expr);
                    a = n.sepcopy();
                    if (!s.staticassign(a, e.Expr))
                    {
                        s.append(nod(OAS, a, e.Expr));
                    }

                }
                return true;
                goto __switch_break0;
            }
            if (r.Op == OMAPLIT)
            {
                break;
                goto __switch_break0;
            }
            if (r.Op == OCLOSURE)
            {
                if (hasemptycvars(r))
                {
                    if (Debug_closure > 0L)
                    {
                        Warnl(r.Pos, "closure converted to global");
                    } 
                    // Closures with no captured variables are globals,
                    // so the assignment can be done at link time.
                    pfuncsym(l, r.Func.Closure.Func.Nname);
                    return true;

                }

                closuredebugruntimecheck(r);
                goto __switch_break0;
            }
            if (r.Op == OCONVIFACE) 
            {
                // This logic is mirrored in isStaticCompositeLiteral.
                // If you change something here, change it there, and vice versa.

                // Determine the underlying concrete type and value we are converting from.
                var val = r;
                while (val.Op == OCONVIFACE)
                {
                    val = val.Left;
                }

                if (val.Type.IsInterface())
                { 
                    // val is an interface type.
                    // If val is nil, we can statically initialize l;
                    // both words are zero and so there no work to do, so report success.
                    // If val is non-nil, we have no concrete type to record,
                    // and we won't be able to statically initialize its value, so report failure.
                    return Isconst(val, CTNIL);

                }

                ptr<Node> itab;
                if (l.Type.IsEmptyInterface())
                {
                    itab = typename(val.Type);
                }
                else
                {
                    itab = itabname(val.Type, l.Type);
                } 

                // Create a copy of l to modify while we emit data.
                n = l.copy(); 

                // Emit itab, advance offset.
                addrsym(n, itab.Left); // itab is an OADDR node
                n.Xoffset += int64(Widthptr); 

                // Emit data.
                if (isdirectiface(val.Type))
                {
                    if (Isconst(val, CTNIL))
                    { 
                        // Nil is zero, nothing to do.
                        return true;

                    } 
                    // Copy val directly into n.
                    n.Type = val.Type;
                    setlineno(val);
                    a = n.sepcopy();
                    if (!s.staticassign(a, val))
                    {
                        s.append(nod(OAS, a, val));
                    }

                }
                else
                { 
                    // Construct temp to hold val, write pointer to temp into n.
                    a = staticname(_addr_val.Type);
                    s.inittemps[val] = a;
                    if (!s.staticassign(a, val))
                    {
                        s.append(nod(OAS, a, val));
                    }

                    addrsym(n, a);

                }

                return true;
                goto __switch_break0;
            }

            __switch_break0:; 

            //dump("not static", r);
            return false;

        }

        // initContext is the context in which static data is populated.
        // It is either in an init function or in any other function.
        // Static data populated in an init function will be written either
        // zero times (as a readonly, static data symbol) or
        // one time (during init function execution).
        // Either way, there is no opportunity for races or further modification,
        // so the data can be written to a (possibly readonly) data symbol.
        // Static data populated in any other function needs to be local to
        // that function to allow multiple instances of that function
        // to execute concurrently without clobbering each others' data.
        private partial struct initContext // : byte
        {
        }

        private static readonly initContext inInitFunction = (initContext)iota;
        private static readonly var inNonInitFunction = 0;


        private static @string String(this initContext c)
        {
            if (c == inInitFunction)
            {
                return "inInitFunction";
            }

            return "inNonInitFunction";

        }

        // from here down is the walk analysis
        // of composite literals.
        // most of the work is to generate
        // data statements for the constant
        // part of the composite literal.

        private static long statuniqgen = default; // name generator for static temps

        // staticname returns a name backed by a static data symbol.
        // Callers should call n.MarkReadonly on the
        // returned node for readonly nodes.
        private static ptr<Node> staticname(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
            // Don't use lookupN; it interns the resulting string, but these are all unique.
            var n = newname(lookup(fmt.Sprintf(".stmp_%d", statuniqgen)));
            statuniqgen++;
            addvar(n, t, PEXTERN);
            return _addr_n!;

        }

        private static bool isLiteral(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // Treat nils as zeros rather than literals.
            return n.Op == OLITERAL && n.Val().Ctype() != CTNIL;

        }

        private static bool isSimpleName(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == ONAME && n.Class() != PAUTOHEAP && n.Class() != PEXTERN;
        }

        private static void litas(ptr<Node> _addr_l, ptr<Node> _addr_r, ptr<Nodes> _addr_init)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;
            ref Nodes init = ref _addr_init.val;

            var a = nod(OAS, l, r);
            a = typecheck(a, ctxStmt);
            a = walkexpr(a, init);
            init.Append(a);
        }

        // initGenType is a bitmap indicating the types of generation that will occur for a static value.
        private partial struct initGenType // : byte
        {
        }

        private static readonly initGenType initDynamic = (initGenType)1L << (int)(iota); // contains some dynamic values, for which init code will be generated
        private static readonly var initConst = 0; // contains some constant values, which may be written into data symbols

        // getdyn calculates the initGenType for n.
        // If top is false, getdyn is recursing.
        private static initGenType getdyn(ptr<Node> _addr_n, bool top)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OSLICELIT) 
                if (!top)
                {
                    return initDynamic;
                }

                if (n.Right.Int64() / 4L > int64(n.List.Len()))
                { 
                    // <25% of entries have explicit values.
                    // Very rough estimation, it takes 4 bytes of instructions
                    // to initialize 1 byte of result. So don't use a static
                    // initializer if the dynamic initialization code would be
                    // smaller than the static value.
                    // See issue 23780.
                    return initDynamic;

                }

            else if (n.Op == OARRAYLIT || n.Op == OSTRUCTLIT)             else 
                if (isLiteral(_addr_n))
                {
                    return initConst;
                }

                return initDynamic;
                        initGenType mode = default;
            foreach (var (_, n1) in n.List.Slice())
            {

                if (n1.Op == OKEY) 
                    n1 = n1.Right;
                else if (n1.Op == OSTRUCTKEY) 
                    n1 = n1.Left;
                                mode |= getdyn(_addr_n1, false);
                if (mode == initDynamic | initConst)
                {
                    break;
                }

            }
            return mode;

        }

        // isStaticCompositeLiteral reports whether n is a compile-time constant.
        private static bool isStaticCompositeLiteral(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OSLICELIT) 
                return false;
            else if (n.Op == OARRAYLIT) 
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in n.List.Slice())
                    {
                        r = __r;
                        if (r.Op == OKEY)
                        {
                            r = r.Right;
                        }

                        if (!isStaticCompositeLiteral(_addr_r))
                        {
                            return false;
                        }

                    }

                    r = r__prev1;
                }

                return true;
            else if (n.Op == OSTRUCTLIT) 
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in n.List.Slice())
                    {
                        r = __r;
                        if (r.Op != OSTRUCTKEY)
                        {
                            Fatalf("isStaticCompositeLiteral: rhs not OSTRUCTKEY: %v", r);
                        }

                        if (!isStaticCompositeLiteral(_addr_r.Left))
                        {
                            return false;
                        }

                    }

                    r = r__prev1;
                }

                return true;
            else if (n.Op == OLITERAL) 
                return true;
            else if (n.Op == OCONVIFACE) 
                // See staticassign's OCONVIFACE case for comments.
                var val = n;
                while (val.Op == OCONVIFACE)
                {
                    val = val.Left;
                }

                if (val.Type.IsInterface())
                {
                    return Isconst(val, CTNIL);
                }

                if (isdirectiface(val.Type) && Isconst(val, CTNIL))
                {
                    return true;
                }

                return isStaticCompositeLiteral(_addr_val);
                        return false;

        }

        // initKind is a kind of static initialization: static, dynamic, or local.
        // Static initialization represents literals and
        // literal components of composite literals.
        // Dynamic initialization represents non-literals and
        // non-literal components of composite literals.
        // LocalCode initialization represents initialization
        // that occurs purely in generated code local to the function of use.
        // Initialization code is sometimes generated in passes,
        // first static then dynamic.
        private partial struct initKind // : byte
        {
        }

        private static readonly initKind initKindStatic = (initKind)iota + 1L;
        private static readonly var initKindDynamic = 0;
        private static readonly var initKindLocalCode = 1;


        // fixedlit handles struct, array, and slice literals.
        // TODO: expand documentation.
        private static void fixedlit(initContext ctxt, initKind kind, ptr<Node> _addr_n, ptr<Node> _addr_var_, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Node var_ = ref _addr_var_.val;
            ref Nodes init = ref _addr_init.val;

            Func<ptr<Node>, (ptr<Node>, ptr<Node>)> splitnode = default;

            if (n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                long k = default;
                splitnode = r =>
                {
                    if (r.Op == OKEY)
                    {
                        k = indexconst(r.Left);
                        if (k < 0L)
                        {
                            Fatalf("fixedlit: invalid index %v", r.Left);
                        }

                        r = r.Right;

                    }

                    var a = nod(OINDEX, var_, nodintconst(k));
                    k++;
                    return (a, r);

                }
;
            else if (n.Op == OSTRUCTLIT) 
                splitnode = r =>
                {
                    if (r.Op != OSTRUCTKEY)
                    {
                        Fatalf("fixedlit: rhs not OSTRUCTKEY: %v", r);
                    }

                    if (r.Sym.IsBlank())
                    {
                        return (nblank, r.Left);
                    }

                    setlineno(r);
                    return (nodSym(ODOT, var_, r.Sym), r.Left);

                }
;
            else 
                Fatalf("fixedlit bad op: %v", n.Op);
                        foreach (var (_, r) in n.List.Slice())
            {
                var (a, value) = splitnode(r);
                if (a == nblank && candiscard(value))
                {
                    continue;
                }


                if (value.Op == OSLICELIT) 
                    if ((kind == initKindStatic && ctxt == inNonInitFunction) || (kind == initKindDynamic && ctxt == inInitFunction))
                    {
                        slicelit(ctxt, _addr_value, _addr_a, _addr_init);
                        continue;
                    }

                else if (value.Op == OARRAYLIT || value.Op == OSTRUCTLIT) 
                    fixedlit(ctxt, kind, _addr_value, _addr_a, _addr_init);
                    continue;
                                var islit = isLiteral(_addr_value);
                if ((kind == initKindStatic && !islit) || (kind == initKindDynamic && islit))
                {
                    continue;
                } 

                // build list of assignments: var[index] = expr
                setlineno(a);
                a = nod(OAS, a, value);
                a = typecheck(a, ctxStmt);

                if (kind == initKindStatic) 
                    genAsStatic(_addr_a);
                else if (kind == initKindDynamic || kind == initKindLocalCode) 
                    a = orderStmtInPlace(a, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<Node>>>{});
                    a = walkstmt(a);
                    init.Append(a);
                else 
                    Fatalf("fixedlit: bad kind %d", kind);
                
            }

        }

        private static bool isSmallSliceLit(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OSLICELIT)
            {
                return false;
            }

            var r = n.Right;

            return smallintconst(r) && (n.Type.Elem().Width == 0L || r.Int64() <= smallArrayBytes / n.Type.Elem().Width);

        }

        private static void slicelit(initContext ctxt, ptr<Node> _addr_n, ptr<Node> _addr_var_, ptr<Nodes> _addr_init) => func((_, panic, __) =>
        {
            ref Node n = ref _addr_n.val;
            ref Node var_ = ref _addr_var_.val;
            ref Nodes init = ref _addr_init.val;
 
            // make an array type corresponding the number of elements we have
            var t = types.NewArray(n.Type.Elem(), n.Right.Int64());
            dowidth(t);

            if (ctxt == inNonInitFunction)
            { 
                // put everything into static array
                var vstat = staticname(_addr_t);

                fixedlit(ctxt, initKindStatic, _addr_n, _addr_vstat, _addr_init);
                fixedlit(ctxt, initKindDynamic, _addr_n, _addr_vstat, _addr_init); 

                // copy static to slice
                var_ = typecheck(var_, ctxExpr | ctxAssign);
                ref Node nam = ref heap(out ptr<Node> _addr_nam);
                if (!stataddr(_addr_nam, _addr_var_) || nam.Class() != PEXTERN)
                {
                    Fatalf("slicelit: %v", var_);
                }

                slicesym(_addr_nam, vstat, t.NumElem());
                return ;

            } 

            // recipe for var = []t{...}
            // 1. make a static array
            //    var vstat [...]t
            // 2. assign (data statements) the constant part
            //    vstat = constpart{}
            // 3. make an auto pointer to array and allocate heap to it
            //    var vauto *[...]t = new([...]t)
            // 4. copy the static array to the auto array
            //    *vauto = vstat
            // 5. for each dynamic part assign to the array
            //    vauto[i] = dynamic part
            // 6. assign slice of allocated heap to var
            //    var = vauto[:]
            //
            // an optimization is done if there is no constant part
            //    3. var vauto *[...]t = new([...]t)
            //    5. vauto[i] = dynamic part
            //    6. var = vauto[:]

            // if the literal contains constants,
            // make static initialized array (1),(2)
            vstat = ;

            var mode = getdyn(_addr_n, true);
            if (mode & initConst != 0L && !isSmallSliceLit(_addr_n))
            {
                vstat = staticname(_addr_t);
                if (ctxt == inInitFunction)
                {
                    vstat.MarkReadonly();
                }

                fixedlit(ctxt, initKindStatic, _addr_n, _addr_vstat, _addr_init);

            } 

            // make new auto *array (3 declare)
            var vauto = temp(types.NewPtr(t)); 

            // set auto to point at new temp or heap (3 assign)
            ptr<Node> a;
            {
                var x = prealloc[n];

                if (x != null)
                { 
                    // temp allocated during order.go for dddarg
                    if (!types.Identical(t, x.Type))
                    {
                        panic("dotdotdot base type does not match order's assigned type");
                    }

                    if (vstat == null)
                    {
                        a = nod(OAS, x, null);
                        a = typecheck(a, ctxStmt);
                        init.Append(a); // zero new temp
                    }
                    else
                    { 
                        // Declare that we're about to initialize all of x.
                        // (Which happens at the *vauto = vstat below.)
                        init.Append(nod(OVARDEF, x, null));

                    }

                    a = nod(OADDR, x, null);

                }
                else if (n.Esc == EscNone)
                {
                    a = temp(t);
                    if (vstat == null)
                    {
                        a = nod(OAS, temp(t), null);
                        a = typecheck(a, ctxStmt);
                        init.Append(a); // zero new temp
                        a = a.Left;

                    }
                    else
                    {
                        init.Append(nod(OVARDEF, a, null));
                    }

                    a = nod(OADDR, a, null);

                }
                else
                {
                    a = nod(ONEW, null, null);
                    a.List.Set1(typenod(t));
                }


            }


            a = nod(OAS, vauto, a);
            a = typecheck(a, ctxStmt);
            a = walkexpr(a, init);
            init.Append(a);

            if (vstat != null)
            { 
                // copy static to heap (4)
                a = nod(ODEREF, vauto, null);

                a = nod(OAS, a, vstat);
                a = typecheck(a, ctxStmt);
                a = walkexpr(a, init);
                init.Append(a);

            } 

            // put dynamics into array (5)
            long index = default;
            foreach (var (_, value) in n.List.Slice())
            {
                if (value.Op == OKEY)
                {
                    index = indexconst(value.Left);
                    if (index < 0L)
                    {
                        Fatalf("slicelit: invalid index %v", value.Left);
                    }

                    value = value.Right;

                }

                a = nod(OINDEX, vauto, nodintconst(index));
                a.SetBounded(true);
                index++; 

                // TODO need to check bounds?


                if (value.Op == OSLICELIT) 
                    break;
                else if (value.Op == OARRAYLIT || value.Op == OSTRUCTLIT) 
                    var k = initKindDynamic;
                    if (vstat == null)
                    { 
                        // Generate both static and dynamic initializations.
                        // See issue #31987.
                        k = initKindLocalCode;

                    }

                    fixedlit(ctxt, k, _addr_value, a, _addr_init);
                    continue;
                                if (vstat != null && isLiteral(_addr_value))
                { // already set by copy from static value
                    continue;

                } 

                // build list of vauto[c] = expr
                setlineno(value);
                a = nod(OAS, a, value);

                a = typecheck(a, ctxStmt);
                a = orderStmtInPlace(a, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<Node>>>{});
                a = walkstmt(a);
                init.Append(a);

            } 

            // make slice out of heap (6)
            a = nod(OAS, var_, nod(OSLICE, vauto, null));

            a = typecheck(a, ctxStmt);
            a = orderStmtInPlace(a, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<Node>>>{});
            a = walkstmt(a);
            init.Append(a);

        });

        private static void maplit(ptr<Node> _addr_n, ptr<Node> _addr_m, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Node m = ref _addr_m.val;
            ref Nodes init = ref _addr_init.val;
 
            // make the map var
            var a = nod(OMAKE, null, null);
            a.Esc = n.Esc;
            a.List.Set2(typenod(n.Type), nodintconst(int64(n.List.Len())));
            litas(_addr_m, _addr_a, _addr_init);

            var entries = n.List.Slice(); 

            // The order pass already removed any dynamic (runtime-computed) entries.
            // All remaining entries are static. Double-check that.
            {
                var r__prev1 = r;

                foreach (var (_, __r) in entries)
                {
                    r = __r;
                    if (!isStaticCompositeLiteral(_addr_r.Left) || !isStaticCompositeLiteral(_addr_r.Right))
                    {
                        Fatalf("maplit: entry is not a literal: %v", r);
                    }

                }

                r = r__prev1;
            }

            if (len(entries) > 25L)
            { 
                // For a large number of entries, put them in an array and loop.

                // build types [count]Tindex and [count]Tvalue
                var tk = types.NewArray(n.Type.Key(), int64(len(entries)));
                var te = types.NewArray(n.Type.Elem(), int64(len(entries)));

                tk.SetNoalg(true);
                te.SetNoalg(true);

                dowidth(tk);
                dowidth(te); 

                // make and initialize static arrays
                var vstatk = staticname(_addr_tk);
                vstatk.MarkReadonly();
                var vstate = staticname(_addr_te);
                vstate.MarkReadonly();

                var datak = nod(OARRAYLIT, null, null);
                var datae = nod(OARRAYLIT, null, null);
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in entries)
                    {
                        r = __r;
                        datak.List.Append(r.Left);
                        datae.List.Append(r.Right);
                    }

                    r = r__prev1;
                }

                fixedlit(inInitFunction, initKindStatic, _addr_datak, _addr_vstatk, _addr_init);
                fixedlit(inInitFunction, initKindStatic, _addr_datae, _addr_vstate, _addr_init); 

                // loop adding structure elements to map
                // for i = 0; i < len(vstatk); i++ {
                //    map[vstatk[i]] = vstate[i]
                // }
                var i = temp(types.Types[TINT]);
                var rhs = nod(OINDEX, vstate, i);
                rhs.SetBounded(true);

                var kidx = nod(OINDEX, vstatk, i);
                kidx.SetBounded(true);
                var lhs = nod(OINDEX, m, kidx);

                var zero = nod(OAS, i, nodintconst(0L));
                var cond = nod(OLT, i, nodintconst(tk.NumElem()));
                var incr = nod(OAS, i, nod(OADD, i, nodintconst(1L)));
                var body = nod(OAS, lhs, rhs);

                var loop = nod(OFOR, cond, incr);
                loop.Nbody.Set1(body);
                loop.Ninit.Set1(zero);

                loop = typecheck(loop, ctxStmt);
                loop = walkstmt(loop);
                init.Append(loop);
                return ;

            } 
            // For a small number of entries, just add them directly.

            // Build list of var[c] = expr.
            // Use temporaries so that mapassign1 can have addressable key, elem.
            // TODO(josharian): avoid map key temporaries for mapfast_* assignments with literal keys.
            var tmpkey = temp(m.Type.Key());
            var tmpelem = temp(m.Type.Elem());

            {
                var r__prev1 = r;

                foreach (var (_, __r) in entries)
                {
                    r = __r;
                    var index = r.Left;
                    var elem = r.Right;

                    setlineno(index);
                    a = nod(OAS, tmpkey, index);
                    a = typecheck(a, ctxStmt);
                    a = walkstmt(a);
                    init.Append(a);

                    setlineno(elem);
                    a = nod(OAS, tmpelem, elem);
                    a = typecheck(a, ctxStmt);
                    a = walkstmt(a);
                    init.Append(a);

                    setlineno(tmpelem);
                    a = nod(OAS, nod(OINDEX, m, tmpkey), tmpelem);
                    a = typecheck(a, ctxStmt);
                    a = walkstmt(a);
                    init.Append(a);

                }

                r = r__prev1;
            }

            a = nod(OVARKILL, tmpkey, null);
            a = typecheck(a, ctxStmt);
            init.Append(a);
            a = nod(OVARKILL, tmpelem, null);
            a = typecheck(a, ctxStmt);
            init.Append(a);

        }

        private static void anylit(ptr<Node> _addr_n, ptr<Node> _addr_var_, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Node var_ = ref _addr_var_.val;
            ref Nodes init = ref _addr_init.val;

            var t = n.Type;

            if (n.Op == ONAME) 
                var a = nod(OAS, var_, n);
                a = typecheck(a, ctxStmt);
                init.Append(a);
            else if (n.Op == OPTRLIT) 
                if (!t.IsPtr())
                {
                    Fatalf("anylit: not ptr");
                }

                ptr<Node> r;
                if (n.Right != null)
                { 
                    // n.Right is stack temporary used as backing store.
                    init.Append(nod(OAS, n.Right, null)); // zero backing store, just in case (#18410)
                    r = nod(OADDR, n.Right, null);
                    r = typecheck(r, ctxExpr);

                }
                else
                {
                    r = nod(ONEW, null, null);
                    r.SetTypecheck(1L);
                    r.Type = t;
                    r.Esc = n.Esc;
                }

                r = walkexpr(r, init);
                a = nod(OAS, var_, r);

                a = typecheck(a, ctxStmt);
                init.Append(a);

                var_ = nod(ODEREF, var_, null);
                var_ = typecheck(var_, ctxExpr | ctxAssign);
                anylit(_addr_n.Left, _addr_var_, _addr_init);
            else if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT) 
                if (!t.IsStruct() && !t.IsArray())
                {
                    Fatalf("anylit: not struct/array");
                }

                if (var_.isSimpleName() && n.List.Len() > 4L)
                { 
                    // lay out static data
                    var vstat = staticname(_addr_t);
                    vstat.MarkReadonly();

                    var ctxt = inInitFunction;
                    if (n.Op == OARRAYLIT)
                    {
                        ctxt = inNonInitFunction;
                    }

                    fixedlit(ctxt, initKindStatic, _addr_n, _addr_vstat, _addr_init); 

                    // copy static to var
                    a = nod(OAS, var_, vstat);

                    a = typecheck(a, ctxStmt);
                    a = walkexpr(a, init);
                    init.Append(a); 

                    // add expressions to automatic
                    fixedlit(inInitFunction, initKindDynamic, _addr_n, _addr_var_, _addr_init);
                    break;

                }

                long components = default;
                if (n.Op == OARRAYLIT)
                {
                    components = t.NumElem();
                }
                else
                {
                    components = int64(t.NumFields());
                } 
                // initialization of an array or struct with unspecified components (missing fields or arrays)
                if (var_.isSimpleName() || int64(n.List.Len()) < components)
                {
                    a = nod(OAS, var_, null);
                    a = typecheck(a, ctxStmt);
                    a = walkexpr(a, init);
                    init.Append(a);
                }

                fixedlit(inInitFunction, initKindLocalCode, _addr_n, _addr_var_, _addr_init);
            else if (n.Op == OSLICELIT) 
                slicelit(inInitFunction, _addr_n, _addr_var_, _addr_init);
            else if (n.Op == OMAPLIT) 
                if (!t.IsMap())
                {
                    Fatalf("anylit: not map");
                }

                maplit(_addr_n, _addr_var_, _addr_init);
            else 
                Fatalf("anylit: not lit, op=%v node=%v", n.Op, n);
            
        }

        private static bool oaslit(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Left == null || n.Right == null)
            { 
                // not a special composite literal assignment
                return false;

            }

            if (n.Left.Type == null || n.Right.Type == null)
            { 
                // not a special composite literal assignment
                return false;

            }

            if (!n.Left.isSimpleName())
            { 
                // not a special composite literal assignment
                return false;

            }

            if (!types.Identical(n.Left.Type, n.Right.Type))
            { 
                // not a special composite literal assignment
                return false;

            }


            if (n.Right.Op == OSTRUCTLIT || n.Right.Op == OARRAYLIT || n.Right.Op == OSLICELIT || n.Right.Op == OMAPLIT) 
                if (vmatch1(n.Left, n.Right))
                { 
                    // not a special composite literal assignment
                    return false;

                }

                anylit(_addr_n.Right, _addr_n.Left, _addr_init);
            else 
                // not a special composite literal assignment
                return false;
                        n.Op = OEMPTY;
            n.Right = null;
            return true;

        }

        private static long getlit(ptr<Node> _addr_lit)
        {
            ref Node lit = ref _addr_lit.val;

            if (smallintconst(lit))
            {
                return int(lit.Int64());
            }

            return -1L;

        }

        // stataddr sets nam to the static address of n and reports whether it succeeded.
        private static bool stataddr(ptr<Node> _addr_nam, ptr<Node> _addr_n)
        {
            ref Node nam = ref _addr_nam.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return false;
            }


            if (n.Op == ONAME) 
                nam = n;
                return true;
            else if (n.Op == ODOT) 
                if (!stataddr(_addr_nam, _addr_n.Left))
                {
                    break;
                }

                nam.Xoffset += n.Xoffset;
                nam.Type = n.Type;
                return true;
            else if (n.Op == OINDEX) 
                if (n.Left.Type.IsSlice())
                {
                    break;
                }

                if (!stataddr(_addr_nam, _addr_n.Left))
                {
                    break;
                }

                var l = getlit(_addr_n.Right);
                if (l < 0L)
                {
                    break;
                } 

                // Check for overflow.
                if (n.Type.Width != 0L && thearch.MAXWIDTH / n.Type.Width <= int64(l))
                {
                    break;
                }

                nam.Xoffset += int64(l) * n.Type.Width;
                nam.Type = n.Type;
                return true;
                        return false;

        }

        private static void initplan(this ptr<InitSchedule> _addr_s, ptr<Node> _addr_n)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (s.initplans[n] != null)
            {
                return ;
            }

            ptr<InitPlan> p = @new<InitPlan>();
            s.initplans[n] = p;

            if (n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                long k = default;
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in n.List.Slice())
                    {
                        a = __a;
                        if (a.Op == OKEY)
                        {
                            k = indexconst(a.Left);
                            if (k < 0L)
                            {
                                Fatalf("initplan arraylit: invalid index %v", a.Left);
                            }

                            a = a.Right;

                        }

                        s.addvalue(p, k * n.Type.Elem().Width, a);
                        k++;

                    }

                    a = a__prev1;
                }
            else if (n.Op == OSTRUCTLIT) 
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in n.List.Slice())
                    {
                        a = __a;
                        if (a.Op != OSTRUCTKEY)
                        {
                            Fatalf("initplan structlit");
                        }

                        if (a.Sym.IsBlank())
                        {
                            continue;
                        }

                        s.addvalue(p, a.Xoffset, a.Left);

                    }

                    a = a__prev1;
                }
            else if (n.Op == OMAPLIT) 
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in n.List.Slice())
                    {
                        a = __a;
                        if (a.Op != OKEY)
                        {
                            Fatalf("initplan maplit");
                        }

                        s.addvalue(p, -1L, a.Right);

                    }

                    a = a__prev1;
                }
            else 
                Fatalf("initplan");
            
        }

        private static void addvalue(this ptr<InitSchedule> _addr_s, ptr<InitPlan> _addr_p, long xoffset, ptr<Node> _addr_n)
        {
            ref InitSchedule s = ref _addr_s.val;
            ref InitPlan p = ref _addr_p.val;
            ref Node n = ref _addr_n.val;
 
            // special case: zero can be dropped entirely
            if (isZero(_addr_n))
            {
                return ;
            } 

            // special case: inline struct and array (not slice) literals
            if (isvaluelit(_addr_n))
            {
                s.initplan(n);
                var q = s.initplans[n];
                foreach (var (_, qe) in q.E)
                { 
                    // qe is a copy; we are not modifying entries in q.E
                    qe.Xoffset += xoffset;
                    p.E = append(p.E, qe);

                }
                return ;

            } 

            // add to plan
            p.E = append(p.E, new InitEntry(Xoffset:xoffset,Expr:n));

        }

        private static bool isZero(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OLITERAL) 
                switch (n.Val().U.type())
                {
                    case ptr<NilVal> u:
                        return true;
                        break;
                    case @string u:
                        return u == "";
                        break;
                    case bool u:
                        return !u;
                        break;
                    case ptr<Mpint> u:
                        return u.CmpInt64(0L) == 0L;
                        break;
                    case ptr<Mpflt> u:
                        return u.CmpFloat64(0L) == 0L;
                        break;
                    case ptr<Mpcplx> u:
                        return u.Real.CmpFloat64(0L) == 0L && u.Imag.CmpFloat64(0L) == 0L;
                        break;
                    default:
                    {
                        var u = n.Val().U.type();
                        Dump("unexpected literal", n);
                        Fatalf("isZero");
                        break;
                    }

                }
            else if (n.Op == OARRAYLIT) 
                {
                    var n1__prev1 = n1;

                    foreach (var (_, __n1) in n.List.Slice())
                    {
                        n1 = __n1;
                        if (n1.Op == OKEY)
                        {
                            n1 = n1.Right;
                        }

                        if (!isZero(_addr_n1))
                        {
                            return false;
                        }

                    }

                    n1 = n1__prev1;
                }

                return true;
            else if (n.Op == OSTRUCTLIT) 
                {
                    var n1__prev1 = n1;

                    foreach (var (_, __n1) in n.List.Slice())
                    {
                        n1 = __n1;
                        if (!isZero(_addr_n1.Left))
                        {
                            return false;
                        }

                    }

                    n1 = n1__prev1;
                }

                return true;
                        return false;

        }

        private static bool isvaluelit(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == OARRAYLIT || n.Op == OSTRUCTLIT;
        }

        private static void genAsStatic(ptr<Node> _addr_@as)
        {
            ref Node @as = ref _addr_@as.val;

            if (@as.Left.Type == null)
            {
                Fatalf("genAsStatic as.Left not typechecked");
            }

            ref Node nam = ref heap(out ptr<Node> _addr_nam);
            if (!stataddr(_addr_nam, _addr_@as.Left) || (nam.Class() != PEXTERN && @as.Left != nblank))
            {
                Fatalf("genAsStatic: lhs %v", @as.Left);
            }


            if (@as.Right.Op == OLITERAL) 
                litsym(_addr_nam, @as.Right, int(@as.Right.Type.Width));
            else if (@as.Right.Op == ONAME && @as.Right.Class() == PFUNC) 
                pfuncsym(_addr_nam, @as.Right);
            else 
                Fatalf("genAsStatic: rhs %v", @as.Right);
            
        }
    }
}}}}
