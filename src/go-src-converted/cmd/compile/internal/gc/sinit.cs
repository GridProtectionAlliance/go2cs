// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:32 UTC
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
        // Static initialization ordering state.
        // These values are stored in two bits in Node.flags.
        public static readonly var InitNotStarted = iota;
        public static readonly var InitDone = 0;
        public static readonly var InitPending = 1;

        public partial struct InitEntry
        {
            public long Xoffset; // struct, array only
            public ptr<Node> Expr; // bytes of run-time computed expressions
        }

        public partial struct InitPlan
        {
            public slice<InitEntry> E;
        }

        private static slice<ref Node> initlist = default;        private static map<ref Node, ref InitPlan> initplans = default;        private static var inittemps = make_map<ref Node, ref Node>();

        // init1 walks the AST starting at n, and accumulates in out
        // the list of definitions needing init code in dependency order.
        private static void init1(ref Node n, ref slice<ref Node> @out)
        {
            if (n == null)
            {
                return;
            }
            init1(n.Left, out);
            init1(n.Right, out);
            foreach (var (_, n1) in n.List.Slice())
            {
                init1(n1, out);
            }
            if (n.isMethodExpression())
            { 
                // Methods called as Type.Method(receiver, ...).
                // Definitions for method expressions are stored in type->nname.
                init1(asNode(n.Type.FuncType().Nname), out);
            }
            if (n.Op != ONAME)
            {
                return;
            }

            if (n.Class() == PEXTERN || n.Class() == PFUNC)             else 
                if (isblank(n) && n.Name.Curfn == null && n.Name.Defn != null && n.Name.Defn.Initorder() == InitNotStarted)
                { 
                    // blank names initialization is part of init() but not
                    // when they are inside a function.
                    break;
                }
                return;
                        if (n.Initorder() == InitDone)
            {
                return;
            }
            if (n.Initorder() == InitPending)
            { 
                // Since mutually recursive sets of functions are allowed,
                // we don't necessarily raise an error if n depends on a node
                // which is already waiting for its dependencies to be visited.
                //
                // initlist contains a cycle of identifiers referring to each other.
                // If this cycle contains a variable, then this variable refers to itself.
                // Conversely, if there exists an initialization cycle involving
                // a variable in the program, the tree walk will reach a cycle
                // involving that variable.
                if (n.Class() != PFUNC)
                {
                    foundinitloop(n, n);
                }
                for (var i = len(initlist) - 1L; i >= 0L; i--)
                {
                    var x = initlist[i];
                    if (x == n)
                    {
                        break;
                    }
                    if (x.Class() != PFUNC)
                    {
                        foundinitloop(n, x);
                    }
                } 

                // The loop involves only functions, ok.
 

                // The loop involves only functions, ok.
                return;
            } 

            // reached a new unvisited node.
            n.SetInitorder(InitPending);
            initlist = append(initlist, n); 

            // make sure that everything n depends on is initialized.
            // n->defn is an assignment to n
            {
                var defn = n.Name.Defn;

                if (defn != null)
                {

                    if (defn.Op == ODCLFUNC) 
                        init2list(defn.Nbody, out);
                    else if (defn.Op == OAS) 
                        if (defn.Left != n)
                        {
                            Dump("defn", defn);
                            Fatalf("init1: bad defn");
                        }
                        if (isblank(defn.Left) && candiscard(defn.Right))
                        {
                            defn.Op = OEMPTY;
                            defn.Left = null;
                            defn.Right = null;
                            break;
                        }
                        init2(defn.Right, out);
                        if (Debug['j'] != 0L)
                        {
                            fmt.Printf("%v\n", n.Sym);
                        }
                        if (isblank(n) || !staticinit(n, out))
                        {
                            if (Debug['%'] != 0L)
                            {
                                Dump("nonstatic", defn);
                            }
                            out.Value = append(out.Value, defn);
                        }
                    else if (defn.Op == OAS2FUNC || defn.Op == OAS2MAPR || defn.Op == OAS2DOTTYPE || defn.Op == OAS2RECV) 
                        if (defn.Initorder() == InitDone)
                        {
                            break;
                        }
                        defn.SetInitorder(InitPending);
                        foreach (var (_, n2) in defn.Rlist.Slice())
                        {
                            init1(n2, out);
                        }
                        if (Debug['%'] != 0L)
                        {
                            Dump("nonstatic", defn);
                        }
                        out.Value = append(out.Value, defn);
                        defn.SetInitorder(InitDone);
                    else 
                        Dump("defn", defn);
                        Fatalf("init1: bad defn");
                                    }

            }

            var last = len(initlist) - 1L;
            if (initlist[last] != n)
            {
                Fatalf("bad initlist %v", initlist);
            }
            initlist[last] = null; // allow GC
            initlist = initlist[..last];

            n.SetInitorder(InitDone);
        }

        // foundinitloop prints an init loop error and exits.
        private static void foundinitloop(ref Node node, ref Node visited)
        { 
            // If there have already been errors printed,
            // those errors probably confused us and
            // there might not be a loop. Let the user
            // fix those first.
            flusherrors();
            if (nerrors > 0L)
            {
                errorexit();
            } 

            // Find the index of node and visited in the initlist.
            long nodeindex = default;            long visitedindex = default;

            while (initlist[nodeindex] != node)
            {
                nodeindex++;
            }

            while (initlist[visitedindex] != visited)
            {
                visitedindex++;
            } 

            // There is a loop involving visited. We know about node and
            // initlist = n1 <- ... <- visited <- ... <- node <- ...
 

            // There is a loop involving visited. We know about node and
            // initlist = n1 <- ... <- visited <- ... <- node <- ...
            fmt.Printf("%v: initialization loop:\n", visited.Line()); 

            // Print visited -> ... -> n1 -> node.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in initlist[visitedindex..])
                {
                    n = __n;
                    fmt.Printf("\t%v %v refers to\n", n.Line(), n.Sym);
                } 

                // Print node -> ... -> visited.

                n = n__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in initlist[nodeindex..visitedindex])
                {
                    n = __n;
                    fmt.Printf("\t%v %v refers to\n", n.Line(), n.Sym);
                }

                n = n__prev1;
            }

            fmt.Printf("\t%v %v\n", visited.Line(), visited.Sym);
            errorexit();
        }

        // recurse over n, doing init1 everywhere.
        private static void init2(ref Node n, ref slice<ref Node> @out)
        {
            if (n == null || n.Initorder() == InitDone)
            {
                return;
            }
            if (n.Op == ONAME && n.Ninit.Len() != 0L)
            {
                Fatalf("name %v with ninit: %+v\n", n.Sym, n);
            }
            init1(n, out);
            init2(n.Left, out);
            init2(n.Right, out);
            init2list(n.Ninit, out);
            init2list(n.List, out);
            init2list(n.Rlist, out);
            init2list(n.Nbody, out);


            if (n.Op == OCLOSURE) 
                init2list(n.Func.Closure.Nbody, out);
            else if (n.Op == ODOTMETH || n.Op == OCALLPART) 
                init2(asNode(n.Type.FuncType().Nname), out);
                    }

        private static void init2list(Nodes l, ref slice<ref Node> @out)
        {
            foreach (var (_, n) in l.Slice())
            {
                init2(n, out);
            }
        }

        private static void initreorder(slice<ref Node> l, ref slice<ref Node> @out)
        {
            foreach (var (_, n) in l)
            {

                if (n.Op == ODCLFUNC || n.Op == ODCLCONST || n.Op == ODCLTYPE) 
                    continue;
                                initreorder(n.Ninit.Slice(), out);
                n.Ninit.Set(null);
                init1(n, out);
            }
        }

        // initfix computes initialization order for a list l of top-level
        // declarations and outputs the corresponding list of statements
        // to include in the init() function body.
        private static slice<ref Node> initfix(slice<ref Node> l)
        {
            slice<ref Node> lout = default;
            initplans = make_map<ref Node, ref InitPlan>();
            var lno = lineno;
            initreorder(l, ref lout);
            lineno = lno;
            initplans = null;
            return lout;
        }

        // compilation of top-level (static) assignments
        // into DATA statements if at all possible.
        private static bool staticinit(ref Node n, ref slice<ref Node> @out)
        {
            if (n.Op != ONAME || n.Class() != PEXTERN || n.Name.Defn == null || n.Name.Defn.Op != OAS)
            {
                Fatalf("staticinit");
            }
            lineno = n.Pos;
            var l = n.Name.Defn.Left;
            var r = n.Name.Defn.Right;
            return staticassign(l, r, out);
        }

        // like staticassign but we are copying an already
        // initialized value r.
        private static bool staticcopy(ref Node l, ref Node r, ref slice<ref Node> @out)
        {
            if (r.Op != ONAME)
            {
                return false;
            }
            if (r.Class() == PFUNC)
            {
                gdata(l, r, Widthptr);
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
            var orig = r;
            r = r.Name.Defn.Right;

            while (r.Op == OCONVNOP && !eqtype(r.Type, l.Type))
            {
                r = r.Left;
            }



            if (r.Op == ONAME) 
                if (staticcopy(l, r, out))
                {
                    return true;
                } 
                // We may have skipped past one or more OCONVNOPs, so
                // use conv to ensure r is assignable to l (#13263).
                out.Value = append(out.Value, nod(OAS, l, conv(r, l.Type)));
                return true;
            else if (r.Op == OLITERAL) 
                if (iszero(r))
                {
                    return true;
                }
                gdata(l, r, int(l.Type.Width));
                return true;
            else if (r.Op == OADDR) 

                if (r.Left.Op == ONAME) 
                    gdata(l, r, int(l.Type.Width));
                    return true;
                            else if (r.Op == OPTRLIT) 

                if (r.Left.Op == OARRAYLIT || r.Left.Op == OSLICELIT || r.Left.Op == OSTRUCTLIT || r.Left.Op == OMAPLIT) 
                    // copy pointer
                    gdata(l, nod(OADDR, inittemps[r], null), int(l.Type.Width));
                    return true;
                            else if (r.Op == OSLICELIT) 
                // copy slice
                var a = inittemps[r];

                var n = l.Value;
                n.Xoffset = l.Xoffset + int64(array_array);
                gdata(ref n, nod(OADDR, a, null), Widthptr);
                n.Xoffset = l.Xoffset + int64(array_nel);
                gdata(ref n, r.Right, Widthptr);
                n.Xoffset = l.Xoffset + int64(array_cap);
                gdata(ref n, r.Right, Widthptr);
                return true;
            else if (r.Op == OARRAYLIT || r.Op == OSTRUCTLIT) 
                var p = initplans[r];

                n = l.Value;
                foreach (var (i) in p.E)
                {
                    var e = ref p.E[i];
                    n.Xoffset = l.Xoffset + e.Xoffset;
                    n.Type = e.Expr.Type;
                    if (e.Expr.Op == OLITERAL)
                    {
                        gdata(ref n, e.Expr, int(n.Type.Width));
                    }
                    else
                    {
                        var ll = nod(OXXX, null, null);
                        ll.Value = n;
                        ll.Orig = ll; // completely separate copy
                        if (!staticassign(ll, e.Expr, out))
                        { 
                            // Requires computation, but we're
                            // copying someone else's computation.
                            var rr = nod(OXXX, null, null);

                            rr.Value = orig.Value;
                            rr.Orig = rr; // completely separate copy
                            rr.Type = ll.Type;
                            rr.Xoffset += e.Xoffset;
                            setlineno(rr);
                            out.Value = append(out.Value, nod(OAS, ll, rr));
                        }
                    }
                }
                return true;
                        return false;
        }

        private static bool staticassign(ref Node l, ref Node r, ref slice<ref Node> @out)
        {
            while (r.Op == OCONVNOP)
            {
                r = r.Left;
            }



            if (r.Op == ONAME)
            {
                return staticcopy(l, r, out);
                goto __switch_break0;
            }
            if (r.Op == OLITERAL)
            {
                if (iszero(r))
                {
                    return true;
                }
                gdata(l, r, int(l.Type.Width));
                return true;
                goto __switch_break0;
            }
            if (r.Op == OADDR)
            {
                Node nam = default;
                if (stataddr(ref nam, r.Left))
                {
                    var n = r.Value;
                    n.Left = ref nam;
                    gdata(l, ref n, int(l.Type.Width));
                    return true;
                }
                fallthrough = true;

            }
            if (fallthrough || r.Op == OPTRLIT)
            {

                if (r.Left.Op == OARRAYLIT || r.Left.Op == OSLICELIT || r.Left.Op == OMAPLIT || r.Left.Op == OSTRUCTLIT) 
                    // Init pointer.
                    var a = staticname(r.Left.Type);

                    inittemps[r] = a;
                    gdata(l, nod(OADDR, a, null), int(l.Type.Width)); 

                    // Init underlying literal.
                    if (!staticassign(a, r.Left, out))
                    {
                        out.Value = append(out.Value, nod(OAS, a, r.Left));
                    }
                    return true;
                //dump("not static ptrlit", r);
                goto __switch_break0;
            }
            if (r.Op == OSTRARRAYBYTE)
            {
                if (l.Class() == PEXTERN && r.Left.Op == OLITERAL)
                {
                    @string sval = r.Left.Val().U._<@string>();
                    slicebytes(l, sval, len(sval));
                    return true;
                }
                goto __switch_break0;
            }
            if (r.Op == OSLICELIT)
            {
                initplan(r); 
                // Init slice.
                var bound = r.Right.Int64();
                var ta = types.NewArray(r.Type.Elem(), bound);
                a = staticname(ta);
                inittemps[r] = a;
                n = l.Value;
                n.Xoffset = l.Xoffset + int64(array_array);
                gdata(ref n, nod(OADDR, a, null), Widthptr);
                n.Xoffset = l.Xoffset + int64(array_nel);
                gdata(ref n, r.Right, Widthptr);
                n.Xoffset = l.Xoffset + int64(array_cap);
                gdata(ref n, r.Right, Widthptr); 

                // Fall through to init underlying array.
                l = a;
                fallthrough = true;

            }
            if (fallthrough || r.Op == OARRAYLIT || r.Op == OSTRUCTLIT)
            {
                initplan(r);

                var p = initplans[r];
                n = l.Value;
                foreach (var (i) in p.E)
                {
                    var e = ref p.E[i];
                    n.Xoffset = l.Xoffset + e.Xoffset;
                    n.Type = e.Expr.Type;
                    if (e.Expr.Op == OLITERAL)
                    {
                        gdata(ref n, e.Expr, int(n.Type.Width));
                    }
                    else
                    {
                        setlineno(e.Expr);
                        a = nod(OXXX, null, null);
                        a.Value = n;
                        a.Orig = a; // completely separate copy
                        if (!staticassign(a, e.Expr, out))
                        {
                            out.Value = append(out.Value, nod(OAS, a, e.Expr));
                        }
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
                    n = l.Value;
                    gdata(ref n, r.Func.Closure.Func.Nname, Widthptr);
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
                ref Node itab = default;
                if (l.Type.IsEmptyInterface())
                {
                    itab = typename(val.Type);
                }
                else
                {
                    itab = itabname(val.Type, l.Type);
                } 

                // Create a copy of l to modify while we emit data.
                n = l.Value; 

                // Emit itab, advance offset.
                gdata(ref n, itab, Widthptr);
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
                    a = nod(OXXX, null, null);
                    a.Value = n;
                    a.Orig = a;
                    if (!staticassign(a, val, out))
                    {
                        out.Value = append(out.Value, nod(OAS, a, val));
                    }
                }
                else
                { 
                    // Construct temp to hold val, write pointer to temp into n.
                    a = staticname(val.Type);
                    inittemps[val] = a;
                    if (!staticassign(a, val, out))
                    {
                        out.Value = append(out.Value, nod(OAS, a, val));
                    }
                    var ptr = nod(OADDR, a, null);
                    n.Type = types.NewPtr(val.Type);
                    gdata(ref n, ptr, Widthptr);
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

        private static readonly initContext inInitFunction = iota;
        private static readonly var inNonInitFunction = 0;

        // from here down is the walk analysis
        // of composite literals.
        // most of the work is to generate
        // data statements for the constant
        // part of the composite literal.

        private static long statuniqgen = default; // name generator for static temps

        // staticname returns a name backed by a static data symbol.
        // Callers should call n.Name.SetReadonly(true) on the
        // returned node for readonly nodes.
        private static ref Node staticname(ref types.Type t)
        { 
            // Don't use lookupN; it interns the resulting string, but these are all unique.
            var n = newname(lookup(fmt.Sprintf("statictmp_%d", statuniqgen)));
            statuniqgen++;
            addvar(n, t, PEXTERN);
            return n;
        }

        private static bool isliteral(ref Node n)
        { 
            // Treat nils as zeros rather than literals.
            return n.Op == OLITERAL && n.Val().Ctype() != CTNIL;
        }

        private static bool isSimpleName(this ref Node n)
        {
            return n.Op == ONAME && n.Addable() && n.Class() != PAUTOHEAP && n.Class() != PEXTERN;
        }

        private static void litas(ref Node l, ref Node r, ref Nodes init)
        {
            var a = nod(OAS, l, r);
            a = typecheck(a, Etop);
            a = walkexpr(a, init);
            init.Append(a);
        }

        // initGenType is a bitmap indicating the types of generation that will occur for a static value.
        private partial struct initGenType // : byte
        {
        }

        private static readonly initGenType initDynamic = 1L << (int)(iota); // contains some dynamic values, for which init code will be generated
        private static readonly var initConst = 0; // contains some constant values, which may be written into data symbols

        // getdyn calculates the initGenType for n.
        // If top is false, getdyn is recursing.
        private static initGenType getdyn(ref Node n, bool top)
        {

            if (n.Op == OSLICELIT) 
                if (!top)
                {
                    return initDynamic;
                }
            else if (n.Op == OARRAYLIT || n.Op == OSTRUCTLIT)             else 
                if (isliteral(n))
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
                                mode |= getdyn(n1, false);
                if (mode == initDynamic | initConst)
                {
                    break;
                }
            }
            return mode;
        }

        // isStaticCompositeLiteral reports whether n is a compile-time constant.
        private static bool isStaticCompositeLiteral(ref Node n)
        {

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
                        if (!isStaticCompositeLiteral(r))
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
                        if (!isStaticCompositeLiteral(r.Left))
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
                return isStaticCompositeLiteral(val);
                        return false;
        }

        // initKind is a kind of static initialization: static, dynamic, or local.
        // Static initialization represents literals and
        // literal components of composite literals.
        // Dynamic initialization represents non-literals and
        // non-literal components of composite literals.
        // LocalCode initializion represents initialization
        // that occurs purely in generated code local to the function of use.
        // Initialization code is sometimes generated in passes,
        // first static then dynamic.
        private partial struct initKind // : byte
        {
        }

        private static readonly initKind initKindStatic = iota + 1L;
        private static readonly var initKindDynamic = 0;
        private static readonly var initKindLocalCode = 1;

        // fixedlit handles struct, array, and slice literals.
        // TODO: expand documentation.
        private static void fixedlit(initContext ctxt, initKind kind, ref Node n, ref Node var_, ref Nodes init)
        {
            Func<ref Node, (ref Node, ref Node)> splitnode = default;

            if (n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                long k = default;
                splitnode = r =>
                {
                    if (r.Op == OKEY)
                    {
                        k = nonnegintconst(r.Left);
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
                    return (nodSym(ODOT, var_, r.Sym), r.Left);
                }
;
            else 
                Fatalf("fixedlit bad op: %v", n.Op);
                        foreach (var (_, r) in n.List.Slice())
            {
                var (a, value) = splitnode(r);


                if (value.Op == OSLICELIT) 
                    if ((kind == initKindStatic && ctxt == inNonInitFunction) || (kind == initKindDynamic && ctxt == inInitFunction))
                    {
                        slicelit(ctxt, value, a, init);
                        continue;
                    }
                else if (value.Op == OARRAYLIT || value.Op == OSTRUCTLIT) 
                    fixedlit(ctxt, kind, value, a, init);
                    continue;
                                var islit = isliteral(value);
                if ((kind == initKindStatic && !islit) || (kind == initKindDynamic && islit))
                {
                    continue;
                } 

                // build list of assignments: var[index] = expr
                setlineno(value);
                a = nod(OAS, a, value);
                a = typecheck(a, Etop);

                if (kind == initKindStatic) 
                    genAsStatic(a);
                else if (kind == initKindDynamic || kind == initKindLocalCode) 
                    a = orderstmtinplace(a);
                    a = walkstmt(a);
                    init.Append(a);
                else 
                    Fatalf("fixedlit: bad kind %d", kind);
                            }
        }

        private static void slicelit(initContext ctxt, ref Node n, ref Node var_, ref Nodes init)
        { 
            // make an array type corresponding the number of elements we have
            var t = types.NewArray(n.Type.Elem(), n.Right.Int64());
            dowidth(t);

            if (ctxt == inNonInitFunction)
            { 
                // put everything into static array
                var vstat = staticname(t);

                fixedlit(ctxt, initKindStatic, n, vstat, init);
                fixedlit(ctxt, initKindDynamic, n, vstat, init); 

                // copy static to slice
                var_ = typecheck(var_, Erv | Easgn);
                Node nam = default;
                if (!stataddr(ref nam, var_) || nam.Class() != PEXTERN)
                {
                    Fatalf("slicelit: %v", var_);
                }
                Node v = default;
                nodconst(ref v, types.Types[TINT], t.NumElem());

                nam.Xoffset += int64(array_array);
                gdata(ref nam, nod(OADDR, vstat, null), Widthptr);
                nam.Xoffset += int64(array_nel) - int64(array_array);
                gdata(ref nam, ref v, Widthptr);
                nam.Xoffset += int64(array_cap) - int64(array_nel);
                gdata(ref nam, ref v, Widthptr);

                return;
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
            vstat = default;

            var mode = getdyn(n, true);
            if (mode & initConst != 0L)
            {
                vstat = staticname(t);
                if (ctxt == inInitFunction)
                {
                    vstat.Name.SetReadonly(true);
                }
                fixedlit(ctxt, initKindStatic, n, vstat, init);
            } 

            // make new auto *array (3 declare)
            var vauto = temp(types.NewPtr(t)); 

            // set auto to point at new temp or heap (3 assign)
            ref Node a = default;
            {
                var x = prealloc[n];

                if (x != null)
                { 
                    // temp allocated during order.go for dddarg
                    x.Type = t;

                    if (vstat == null)
                    {
                        a = nod(OAS, x, null);
                        a = typecheck(a, Etop);
                        init.Append(a); // zero new temp
                    }
                    a = nod(OADDR, x, null);
                }
                else if (n.Esc == EscNone)
                {
                    a = temp(t);
                    if (vstat == null)
                    {
                        a = nod(OAS, temp(t), null);
                        a = typecheck(a, Etop);
                        init.Append(a); // zero new temp
                        a = a.Left;
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
            a = typecheck(a, Etop);
            a = walkexpr(a, init);
            init.Append(a);

            if (vstat != null)
            { 
                // copy static to heap (4)
                a = nod(OIND, vauto, null);

                a = nod(OAS, a, vstat);
                a = typecheck(a, Etop);
                a = walkexpr(a, init);
                init.Append(a);
            } 

            // put dynamics into array (5)
            long index = default;
            foreach (var (_, value) in n.List.Slice())
            {
                if (value.Op == OKEY)
                {
                    index = nonnegintconst(value.Left);
                    value = value.Right;
                }
                a = nod(OINDEX, vauto, nodintconst(index));
                a.SetBounded(true);
                index++; 

                // TODO need to check bounds?


                if (value.Op == OSLICELIT) 
                    break;
                else if (value.Op == OARRAYLIT || value.Op == OSTRUCTLIT) 
                    fixedlit(ctxt, initKindDynamic, value, a, init);
                    continue;
                                if (isliteral(value))
                {
                    continue;
                } 

                // build list of vauto[c] = expr
                setlineno(value);
                a = nod(OAS, a, value);

                a = typecheck(a, Etop);
                a = orderstmtinplace(a);
                a = walkstmt(a);
                init.Append(a);
            } 

            // make slice out of heap (6)
            a = nod(OAS, var_, nod(OSLICE, vauto, null));

            a = typecheck(a, Etop);
            a = orderstmtinplace(a);
            a = walkstmt(a);
            init.Append(a);
        }

        private static void maplit(ref Node n, ref Node m, ref Nodes init)
        { 
            // make the map var
            var a = nod(OMAKE, null, null);
            a.Esc = n.Esc;
            a.List.Set2(typenod(n.Type), nodintconst(int64(n.List.Len())));
            litas(m, a, init); 

            // Split the initializers into static and dynamic.
            slice<ref Node> stat = default;            slice<ref Node> dyn = default;

            {
                var r__prev1 = r;

                foreach (var (_, __r) in n.List.Slice())
                {
                    r = __r;
                    if (r.Op != OKEY)
                    {
                        Fatalf("maplit: rhs not OKEY: %v", r);
                    }
                    if (isStaticCompositeLiteral(r.Left) && isStaticCompositeLiteral(r.Right))
                    {
                        stat = append(stat, r);
                    }
                    else
                    {
                        dyn = append(dyn, r);
                    }
                } 

                // Add static entries.

                r = r__prev1;
            }

            if (len(stat) > 25L)
            { 
                // For a large number of static entries, put them in an array and loop.

                // build types [count]Tindex and [count]Tvalue
                var tk = types.NewArray(n.Type.Key(), int64(len(stat)));
                var tv = types.NewArray(n.Type.Val(), int64(len(stat))); 

                // TODO(josharian): suppress alg generation for these types?
                dowidth(tk);
                dowidth(tv); 

                // make and initialize static arrays
                var vstatk = staticname(tk);
                vstatk.Name.SetReadonly(true);
                var vstatv = staticname(tv);
                vstatv.Name.SetReadonly(true);

                var datak = nod(OARRAYLIT, null, null);
                var datav = nod(OARRAYLIT, null, null);
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in stat)
                    {
                        r = __r;
                        datak.List.Append(r.Left);
                        datav.List.Append(r.Right);
                    }
            else

                    r = r__prev1;
                }

                fixedlit(inInitFunction, initKindStatic, datak, vstatk, init);
                fixedlit(inInitFunction, initKindStatic, datav, vstatv, init); 

                // loop adding structure elements to map
                // for i = 0; i < len(vstatk); i++ {
                //    map[vstatk[i]] = vstatv[i]
                // }
                var i = temp(types.Types[TINT]);
                var rhs = nod(OINDEX, vstatv, i);
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

                loop = typecheck(loop, Etop);
                loop = walkstmt(loop);
                init.Append(loop);
            }            { 
                // For a small number of static entries, just add them directly.
                addMapEntries(m, stat, init);
            } 

            // Add dynamic entries.
            addMapEntries(m, dyn, init);
        }

        private static void addMapEntries(ref Node m, slice<ref Node> dyn, ref Nodes init)
        {
            if (len(dyn) == 0L)
            {
                return;
            }
            var nerr = nerrors; 

            // Build list of var[c] = expr.
            // Use temporaries so that mapassign1 can have addressable key, val.
            // TODO(josharian): avoid map key temporaries for mapfast_* assignments with literal keys.
            var key = temp(m.Type.Key());
            var val = temp(m.Type.Val());

            foreach (var (_, r) in dyn)
            {
                var index = r.Left;
                var value = r.Right;

                setlineno(index);
                var a = nod(OAS, key, index);
                a = typecheck(a, Etop);
                a = walkstmt(a);
                init.Append(a);

                setlineno(value);
                a = nod(OAS, val, value);
                a = typecheck(a, Etop);
                a = walkstmt(a);
                init.Append(a);

                setlineno(val);
                a = nod(OAS, nod(OINDEX, m, key), val);
                a = typecheck(a, Etop);
                a = walkstmt(a);
                init.Append(a);

                if (nerr != nerrors)
                {
                    break;
                }
            }
            a = nod(OVARKILL, key, null);
            a = typecheck(a, Etop);
            init.Append(a);
            a = nod(OVARKILL, val, null);
            a = typecheck(a, Etop);
            init.Append(a);
        }

        private static void anylit(ref Node n, ref Node var_, ref Nodes init)
        {
            var t = n.Type;

            if (n.Op == OPTRLIT) 
                if (!t.IsPtr())
                {
                    Fatalf("anylit: not ptr");
                }
                ref Node r = default;
                if (n.Right != null)
                { 
                    // n.Right is stack temporary used as backing store.
                    init.Append(nod(OAS, n.Right, null)); // zero backing store, just in case (#18410)
                    r = nod(OADDR, n.Right, null);
                    r = typecheck(r, Erv);
                }
                else
                {
                    r = nod(ONEW, null, null);
                    r.SetTypecheck(1L);
                    r.Type = t;
                    r.Esc = n.Esc;
                }
                r = walkexpr(r, init);
                var a = nod(OAS, var_, r);

                a = typecheck(a, Etop);
                init.Append(a);

                var_ = nod(OIND, var_, null);
                var_ = typecheck(var_, Erv | Easgn);
                anylit(n.Left, var_, init);
            else if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT) 
                if (!t.IsStruct() && !t.IsArray())
                {
                    Fatalf("anylit: not struct/array");
                }
                if (var_.isSimpleName() && n.List.Len() > 4L)
                { 
                    // lay out static data
                    var vstat = staticname(t);
                    vstat.Name.SetReadonly(true);

                    var ctxt = inInitFunction;
                    if (n.Op == OARRAYLIT)
                    {
                        ctxt = inNonInitFunction;
                    }
                    fixedlit(ctxt, initKindStatic, n, vstat, init); 

                    // copy static to var
                    a = nod(OAS, var_, vstat);

                    a = typecheck(a, Etop);
                    a = walkexpr(a, init);
                    init.Append(a); 

                    // add expressions to automatic
                    fixedlit(inInitFunction, initKindDynamic, n, var_, init);
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
                    a = typecheck(a, Etop);
                    a = walkexpr(a, init);
                    init.Append(a);
                }
                fixedlit(inInitFunction, initKindLocalCode, n, var_, init);
            else if (n.Op == OSLICELIT) 
                slicelit(inInitFunction, n, var_, init);
            else if (n.Op == OMAPLIT) 
                if (!t.IsMap())
                {
                    Fatalf("anylit: not map");
                }
                maplit(n, var_, init);
            else 
                Fatalf("anylit: not lit, op=%v node=%v", n.Op, n);
                    }

        private static bool oaslit(ref Node n, ref Nodes init)
        {
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
            if (!eqtype(n.Left.Type, n.Right.Type))
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
                anylit(n.Right, n.Left, init);
            else 
                // not a special composite literal assignment
                return false;
                        n.Op = OEMPTY;
            n.Right = null;
            return true;
        }

        private static long getlit(ref Node lit)
        {
            if (smallintconst(lit))
            {
                return int(lit.Int64());
            }
            return -1L;
        }

        // stataddr sets nam to the static address of n and reports whether it succeeded.
        private static bool stataddr(ref Node nam, ref Node n)
        {
            if (n == null)
            {
                return false;
            }

            if (n.Op == ONAME) 
                nam.Value = n.Value;
                return n.Addable();
            else if (n.Op == ODOT) 
                if (!stataddr(nam, n.Left))
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
                if (!stataddr(nam, n.Left))
                {
                    break;
                }
                var l = getlit(n.Right);
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

        private static void initplan(ref Node n)
        {
            if (initplans[n] != null)
            {
                return;
            }
            ptr<InitPlan> p = @new<InitPlan>();
            initplans[n] = p;

            if (n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                long k = default;
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in n.List.Slice())
                    {
                        a = __a;
                        if (a.Op == OKEY)
                        {
                            k = nonnegintconst(a.Left);
                            a = a.Right;
                        }
                        addvalue(p, k * n.Type.Elem().Width, a);
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
                            Fatalf("initplan fixedlit");
                        }
                        addvalue(p, a.Xoffset, a.Left);
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
                        addvalue(p, -1L, a.Right);
                    }

                    a = a__prev1;
                }
            else 
                Fatalf("initplan");
                    }

        private static void addvalue(ref InitPlan p, long xoffset, ref Node n)
        { 
            // special case: zero can be dropped entirely
            if (iszero(n))
            {
                return;
            } 

            // special case: inline struct and array (not slice) literals
            if (isvaluelit(n))
            {
                initplan(n);
                var q = initplans[n];
                foreach (var (_, qe) in q.E)
                { 
                    // qe is a copy; we are not modifying entries in q.E
                    qe.Xoffset += xoffset;
                    p.E = append(p.E, qe);
                }
                return;
            } 

            // add to plan
            p.E = append(p.E, new InitEntry(Xoffset:xoffset,Expr:n));
        }

        private static bool iszero(ref Node n)
        {

            if (n.Op == OLITERAL) 
                switch (n.Val().U.type())
                {
                    case ref NilVal u:
                        return true;
                        break;
                    case @string u:
                        return u == "";
                        break;
                    case bool u:
                        return !u;
                        break;
                    case ref Mpint u:
                        return u.CmpInt64(0L) == 0L;
                        break;
                    case ref Mpflt u:
                        return u.CmpFloat64(0L) == 0L;
                        break;
                    case ref Mpcplx u:
                        return u.Real.CmpFloat64(0L) == 0L && u.Imag.CmpFloat64(0L) == 0L;
                        break;
                    default:
                    {
                        var u = n.Val().U.type();
                        Dump("unexpected literal", n);
                        Fatalf("iszero");
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
                        if (!iszero(n1))
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
                        if (!iszero(n1.Left))
                        {
                            return false;
                        }
                    }

                    n1 = n1__prev1;
                }

                return true;
                        return false;
        }

        private static bool isvaluelit(ref Node n)
        {
            return n.Op == OARRAYLIT || n.Op == OSTRUCTLIT;
        }

        private static void genAsStatic(ref Node @as)
        {
            if (@as.Left.Type == null)
            {
                Fatalf("genAsStatic as.Left not typechecked");
            }
            Node nam = default;
            if (!stataddr(ref nam, @as.Left) || (nam.Class() != PEXTERN && @as.Left != nblank))
            {
                Fatalf("genAsStatic: lhs %v", @as.Left);
            }

            if (@as.Right.Op == OLITERAL)             else if (@as.Right.Op == ONAME && @as.Right.Class() == PFUNC)             else 
                Fatalf("genAsStatic: rhs %v", @as.Right);
                        gdata(ref nam, @as.Right, int(@as.Right.Type.Width));
        }
    }
}}}}
