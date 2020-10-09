// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:22 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\escape.go
using logopt = go.cmd.compile.@internal.logopt_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using math = go.math_package;
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
        // Escape analysis.
        //
        // Here we analyze functions to determine which Go variables
        // (including implicit allocations such as calls to "new" or "make",
        // composite literals, etc.) can be allocated on the stack. The two
        // key invariants we have to ensure are: (1) pointers to stack objects
        // cannot be stored in the heap, and (2) pointers to a stack object
        // cannot outlive that object (e.g., because the declaring function
        // returned and destroyed the object's stack frame, or its space is
        // reused across loop iterations for logically distinct variables).
        //
        // We implement this with a static data-flow analysis of the AST.
        // First, we construct a directed weighted graph where vertices
        // (termed "locations") represent variables allocated by statements
        // and expressions, and edges represent assignments between variables
        // (with weights representing addressing/dereference counts).
        //
        // Next we walk the graph looking for assignment paths that might
        // violate the invariants stated above. If a variable v's address is
        // stored in the heap or elsewhere that may outlive it, then v is
        // marked as requiring heap allocation.
        //
        // To support interprocedural analysis, we also record data-flow from
        // each function's parameters to the heap and to its result
        // parameters. This information is summarized as "parameter tags",
        // which are used at static call sites to improve escape analysis of
        // function arguments.

        // Constructing the location graph.
        //
        // Every allocating statement (e.g., variable declaration) or
        // expression (e.g., "new" or "make") is first mapped to a unique
        // "location."
        //
        // We also model every Go assignment as a directed edges between
        // locations. The number of dereference operations minus the number of
        // addressing operations is recorded as the edge's weight (termed
        // "derefs"). For example:
        //
        //     p = &q    // -1
        //     p = q     //  0
        //     p = *q    //  1
        //     p = **q   //  2
        //
        //     p = **&**&q  // 2
        //
        // Note that the & operator can only be applied to addressable
        // expressions, and the expression &x itself is not addressable, so
        // derefs cannot go below -1.
        //
        // Every Go language construct is lowered into this representation,
        // generally without sensitivity to flow, path, or context; and
        // without distinguishing elements within a compound variable. For
        // example:
        //
        //     var x struct { f, g *int }
        //     var u []*int
        //
        //     x.f = u[0]
        //
        // is modeled simply as
        //
        //     x = *u
        //
        // That is, we don't distinguish x.f from x.g, or u[0] from u[1],
        // u[2], etc. However, we do record the implicit dereference involved
        // in indexing a slice.
        public partial struct Escape
        {
            public slice<ptr<EscLocation>> allLocs;
            public ptr<Node> curfn; // loopDepth counts the current loop nesting depth within
// curfn. It increments within each "for" loop and at each
// label with a corresponding backwards "goto" (i.e.,
// unstructured loop).
            public long loopDepth;
            public EscLocation heapLoc;
            public EscLocation blankLoc;
        }

        // An EscLocation represents an abstract location that stores a Go
        // variable.
        public partial struct EscLocation
        {
            public ptr<Node> n; // represented variable or expression, if any
            public ptr<Node> curfn; // enclosing function
            public slice<EscEdge> edges; // incoming edges
            public long loopDepth; // loopDepth at declaration

// derefs and walkgen are used during walkOne to track the
// minimal dereferences from the walk root.
            public long derefs; // >= -1
            public uint walkgen; // dst and dstEdgeindex track the next immediate assignment
// destination location during walkone, along with the index
// of the edge pointing back to this location.
            public ptr<EscLocation> dst;
            public long dstEdgeIdx; // queued is used by walkAll to track whether this location is
// in the walk queue.
            public bool queued; // escapes reports whether the represented variable's address
// escapes; that is, whether the variable must be heap
// allocated.
            public bool escapes; // transient reports whether the represented expression's
// address does not outlive the statement; that is, whether
// its storage can be immediately reused.
            public bool transient; // paramEsc records the represented parameter's leak set.
            public EscLeaks paramEsc;
        }

        // An EscEdge represents an assignment edge between two Go variables.
        public partial struct EscEdge
        {
            public ptr<EscLocation> src;
            public long derefs; // >= -1
            public ptr<EscNote> notes;
        }

        // escapeFuncs performs escape analysis on a minimal batch of
        // functions.
        private static void escapeFuncs(slice<ptr<Node>> fns, bool recursive)
        {
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in fns)
                {
                    fn = __fn;
                    if (fn.Op != ODCLFUNC)
                    {
                        Fatalf("unexpected node: %v", fn);
                    }

                }

                fn = fn__prev1;
            }

            Escape e = default;
            e.heapLoc.escapes = true; 

            // Construct data-flow graph from syntax trees.
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in fns)
                {
                    fn = __fn;
                    e.initFunc(fn);
                }

                fn = fn__prev1;
            }

            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in fns)
                {
                    fn = __fn;
                    e.walkFunc(fn);
                }

                fn = fn__prev1;
            }

            e.curfn = null;

            e.walkAll();
            e.finish(fns);

        }

        private static void initFunc(this ptr<Escape> _addr_e, ptr<Node> _addr_fn)
        {
            ref Escape e = ref _addr_e.val;
            ref Node fn = ref _addr_fn.val;

            if (fn.Op != ODCLFUNC || fn.Esc != EscFuncUnknown)
            {
                Fatalf("unexpected node: %v", fn);
            }

            fn.Esc = EscFuncPlanned;
            if (Debug['m'] > 3L)
            {
                Dump("escAnalyze", fn);
            }

            e.curfn = fn;
            e.loopDepth = 1L; 

            // Allocate locations for local variables.
            foreach (var (_, dcl) in fn.Func.Dcl)
            {
                if (dcl.Op == ONAME)
                {
                    e.newLoc(dcl, false);
                }

            }

        }

        private static void walkFunc(this ptr<Escape> _addr_e, ptr<Node> _addr_fn)
        {
            ref Escape e = ref _addr_e.val;
            ref Node fn = ref _addr_fn.val;

            fn.Esc = EscFuncStarted; 

            // Identify labels that mark the head of an unstructured loop.
            inspectList(fn.Nbody, n =>
            {

                if (n.Op == OLABEL) 
                    n.Sym.Label = asTypesNode(_addr_nonlooping);
                else if (n.Op == OGOTO) 
                    // If we visited the label before the goto,
                    // then this is a looping label.
                    if (n.Sym.Label == asTypesNode(_addr_nonlooping))
                    {
                        n.Sym.Label = asTypesNode(_addr_looping);
                    }

                                return true;

            });

            e.curfn = fn;
            e.loopDepth = 1L;
            e.block(fn.Nbody);

        }

        // Below we implement the methods for walking the AST and recording
        // data flow edges. Note that because a sub-expression might have
        // side-effects, it's important to always visit the entire AST.
        //
        // For example, write either:
        //
        //     if x {
        //         e.discard(n.Left)
        //     } else {
        //         e.value(k, n.Left)
        //     }
        //
        // or
        //
        //     if x {
        //         k = e.discardHole()
        //     }
        //     e.value(k, n.Left)
        //
        // Do NOT write:
        //
        //    // BAD: possibly loses side-effects within n.Left
        //    if !x {
        //        e.value(k, n.Left)
        //    }

        // stmt evaluates a single Go statement.
        private static void stmt(this ptr<Escape> _addr_e, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            var lno = setlineno(n);
            defer(() =>
            {
                lineno = lno;
            }());

            if (Debug['m'] > 2L)
            {
                fmt.Printf("%v:[%d] %v stmt: %v\n", linestr(lineno), e.loopDepth, funcSym(e.curfn), n);
            }

            e.stmts(n.Ninit);


            if (n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OEMPTY || n.Op == OFALL || n.Op == OINLMARK)             else if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == OGOTO)             else if (n.Op == OBLOCK) 
                e.stmts(n.List);
            else if (n.Op == ODCL) 
                // Record loop depth at declaration.
                if (!n.Left.isBlank())
                {
                    e.dcl(n.Left);
                }

            else if (n.Op == OLABEL) 

                if (asNode(n.Sym.Label) == _addr_nonlooping) 
                    if (Debug['m'] > 2L)
                    {
                        fmt.Printf("%v:%v non-looping label\n", linestr(lineno), n);
                    }

                else if (asNode(n.Sym.Label) == _addr_looping) 
                    if (Debug['m'] > 2L)
                    {
                        fmt.Printf("%v: %v looping label\n", linestr(lineno), n);
                    }

                    e.loopDepth++;
                else 
                    Fatalf("label missing tag");
                                n.Sym.Label = null;
            else if (n.Op == OIF) 
                e.discard(n.Left);
                e.block(n.Nbody);
                e.block(n.Rlist);
            else if (n.Op == OFOR || n.Op == OFORUNTIL) 
                e.loopDepth++;
                e.discard(n.Left);
                e.stmt(n.Right);
                e.block(n.Nbody);
                e.loopDepth--;
            else if (n.Op == ORANGE) 
                // for List = range Right { Nbody }
                e.loopDepth++;
                var ks = e.addrs(n.List);
                e.block(n.Nbody);
                e.loopDepth--; 

                // Right is evaluated outside the loop.
                var k = e.discardHole();
                if (len(ks) >= 2L)
                {
                    if (n.Right.Type.IsArray())
                    {
                        k = ks[1L].note(n, "range");
                    }
                    else
                    {
                        k = ks[1L].deref(n, "range-deref");
                    }

                }

                e.expr(e.later(k), n.Right);
            else if (n.Op == OSWITCH) 
                var typesw = n.Left != null && n.Left.Op == OTYPESW;

                ks = default;
                {
                    var cas__prev1 = cas;

                    foreach (var (_, __cas) in n.List.Slice())
                    {
                        cas = __cas; // cases
                        if (typesw && n.Left.Left != null)
                        {
                            var cv = cas.Rlist.First();
                            k = e.dcl(cv); // type switch variables have no ODCL.
                            if (types.Haspointers(cv.Type))
                            {
                                ks = append(ks, k.dotType(cv.Type, cas, "switch case"));
                            }

                        }

                        e.discards(cas.List);
                        e.block(cas.Nbody);

                    }

                    cas = cas__prev1;
                }

                if (typesw)
                {
                    e.expr(e.teeHole(ks), n.Left.Right);
                }
                else
                {
                    e.discard(n.Left);
                }

            else if (n.Op == OSELECT) 
                {
                    var cas__prev1 = cas;

                    foreach (var (_, __cas) in n.List.Slice())
                    {
                        cas = __cas;
                        e.stmt(cas.Left);
                        e.block(cas.Nbody);
                    }

                    cas = cas__prev1;
                }
            else if (n.Op == OSELRECV) 
                e.assign(n.Left, n.Right, "selrecv", n);
            else if (n.Op == OSELRECV2) 
                e.assign(n.Left, n.Right, "selrecv", n);
                e.assign(n.List.First(), null, "selrecv", n);
            else if (n.Op == ORECV) 
                // TODO(mdempsky): Consider e.discard(n.Left).
                e.exprSkipInit(e.discardHole(), n); // already visited n.Ninit
            else if (n.Op == OSEND) 
                e.discard(n.Left);
                e.assignHeap(n.Right, "send", n);
            else if (n.Op == OAS || n.Op == OASOP) 
                e.assign(n.Left, n.Right, "assign", n);
            else if (n.Op == OAS2) 
                {
                    var i__prev1 = i;

                    foreach (var (__i, __nl) in n.List.Slice())
                    {
                        i = __i;
                        nl = __nl;
                        e.assign(nl, n.Rlist.Index(i), "assign-pair", n);
                    }

                    i = i__prev1;
                }
            else if (n.Op == OAS2DOTTYPE) // v, ok = x.(type)
                e.assign(n.List.First(), n.Right, "assign-pair-dot-type", n);
                e.assign(n.List.Second(), null, "assign-pair-dot-type", n);
            else if (n.Op == OAS2MAPR) // v, ok = m[k]
                e.assign(n.List.First(), n.Right, "assign-pair-mapr", n);
                e.assign(n.List.Second(), null, "assign-pair-mapr", n);
            else if (n.Op == OAS2RECV) // v, ok = <-ch
                e.assign(n.List.First(), n.Right, "assign-pair-receive", n);
                e.assign(n.List.Second(), null, "assign-pair-receive", n);
            else if (n.Op == OAS2FUNC) 
                e.stmts(n.Right.Ninit);
                e.call(e.addrs(n.List), n.Right, null);
            else if (n.Op == ORETURN) 
                var results = e.curfn.Type.Results().FieldSlice();
                {
                    var i__prev1 = i;

                    foreach (var (__i, __v) in n.List.Slice())
                    {
                        i = __i;
                        v = __v;
                        e.assign(asNode(results[i].Nname), v, "return", n);
                    }

                    i = i__prev1;
                }
            else if (n.Op == OCALLFUNC || n.Op == OCALLMETH || n.Op == OCALLINTER || n.Op == OCLOSE || n.Op == OCOPY || n.Op == ODELETE || n.Op == OPANIC || n.Op == OPRINT || n.Op == OPRINTN || n.Op == ORECOVER) 
                e.call(null, n, null);
            else if (n.Op == OGO || n.Op == ODEFER) 
                e.stmts(n.Left.Ninit);
                e.call(null, n.Left, n);
            else if (n.Op == ORETJMP)             else 
                Fatalf("unexpected stmt: %v", n);
            
        });

        private static void stmts(this ptr<Escape> _addr_e, Nodes l)
        {
            ref Escape e = ref _addr_e.val;

            foreach (var (_, n) in l.Slice())
            {
                e.stmt(n);
            }

        }

        // block is like stmts, but preserves loopDepth.
        private static void block(this ptr<Escape> _addr_e, Nodes l)
        {
            ref Escape e = ref _addr_e.val;

            var old = e.loopDepth;
            e.stmts(l);
            e.loopDepth = old;
        }

        // expr models evaluating an expression n and flowing the result into
        // hole k.
        private static void expr(this ptr<Escape> _addr_e, EscHole k, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            e.stmts(n.Ninit);
            e.exprSkipInit(k, n);

        }

        private static void exprSkipInit(this ptr<Escape> _addr_e, EscHole k, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            var lno = setlineno(n);
            defer(() =>
            {
                lineno = lno;
            }());

            var uintptrEscapesHack = k.uintptrEscapesHack;
            k.uintptrEscapesHack = false;

            if (uintptrEscapesHack && n.Op == OCONVNOP && n.Left.Type.IsUnsafePtr())
            { 
                // nop
            }
            else if (k.derefs >= 0L && !types.Haspointers(n.Type))
            {
                k = e.discardHole();
            }


            if (n.Op == OLITERAL || n.Op == OGETG || n.Op == OCLOSUREVAR || n.Op == OTYPE)             else if (n.Op == ONAME) 
                if (n.Class() == PFUNC || n.Class() == PEXTERN)
                {
                    return ;
                }

                e.flow(k, e.oldLoc(n));
            else if (n.Op == OPLUS || n.Op == ONEG || n.Op == OBITNOT || n.Op == ONOT) 
                e.discard(n.Left);
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OMUL || n.Op == ODIV || n.Op == OMOD || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE || n.Op == OANDAND || n.Op == OOROR) 
                e.discard(n.Left);
                e.discard(n.Right);
            else if (n.Op == OADDR) 
                e.expr(k.addr(n, "address-of"), n.Left); // "address-of"
            else if (n.Op == ODEREF) 
                e.expr(k.deref(n, "indirection"), n.Left); // "indirection"
            else if (n.Op == ODOT || n.Op == ODOTMETH || n.Op == ODOTINTER) 
                e.expr(k.note(n, "dot"), n.Left);
            else if (n.Op == ODOTPTR) 
                e.expr(k.deref(n, "dot of pointer"), n.Left); // "dot of pointer"
            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                e.expr(k.dotType(n.Type, n, "dot"), n.Left);
            else if (n.Op == OINDEX) 
                if (n.Left.Type.IsArray())
                {
                    e.expr(k.note(n, "fixed-array-index-of"), n.Left);
                }
                else
                { 
                    // TODO(mdempsky): Fix why reason text.
                    e.expr(k.deref(n, "dot of pointer"), n.Left);

                }

                e.discard(n.Right);
            else if (n.Op == OINDEXMAP) 
                e.discard(n.Left);
                e.discard(n.Right);
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR || n.Op == OSLICESTR) 
                e.expr(k.note(n, "slice"), n.Left);
                var (low, high, max) = n.SliceBounds();
                e.discard(low);
                e.discard(high);
                e.discard(max);
            else if (n.Op == OCONV || n.Op == OCONVNOP) 
                if (checkPtr(e.curfn, 2L) && n.Type.Etype == TUNSAFEPTR && n.Left.Type.IsPtr())
                { 
                    // When -d=checkptr=2 is enabled, treat
                    // conversions to unsafe.Pointer as an
                    // escaping operation. This allows better
                    // runtime instrumentation, since we can more
                    // easily detect object boundaries on the heap
                    // than the stack.
                    e.assignHeap(n.Left, "conversion to unsafe.Pointer", n);

                }
                else if (n.Type.Etype == TUNSAFEPTR && n.Left.Type.Etype == TUINTPTR)
                {
                    e.unsafeValue(k, n.Left);
                }
                else
                {
                    e.expr(k, n.Left);
                }

            else if (n.Op == OCONVIFACE) 
                if (!n.Left.Type.IsInterface() && !isdirectiface(n.Left.Type))
                {
                    k = e.spill(k, n);
                }

                e.expr(k.note(n, "interface-converted"), n.Left);
            else if (n.Op == ORECV) 
                e.discard(n.Left);
            else if (n.Op == OCALLMETH || n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OLEN || n.Op == OCAP || n.Op == OCOMPLEX || n.Op == OREAL || n.Op == OIMAG || n.Op == OAPPEND || n.Op == OCOPY) 
                e.call(new slice<EscHole>(new EscHole[] { k }), n, null);
            else if (n.Op == ONEW) 
                e.spill(k, n);
            else if (n.Op == OMAKESLICE) 
                e.spill(k, n);
                e.discard(n.Left);
                e.discard(n.Right);
            else if (n.Op == OMAKECHAN) 
                e.discard(n.Left);
            else if (n.Op == OMAKEMAP) 
                e.spill(k, n);
                e.discard(n.Left);
            else if (n.Op == ORECOVER)             else if (n.Op == OCALLPART) 
                // Flow the receiver argument to both the closure and
                // to the receiver parameter.

                var closureK = e.spill(k, n);

                var m = callpartMethod(n); 

                // We don't know how the method value will be called
                // later, so conservatively assume the result
                // parameters all flow to the heap.
                //
                // TODO(mdempsky): Change ks into a callback, so that
                // we don't have to create this dummy slice?
                slice<EscHole> ks = default;
                for (var i = m.Type.NumResults(); i > 0L; i--)
                {
                    ks = append(ks, e.heapHole());
                }

                var paramK = e.tagHole(ks, asNode(m.Type.Nname()), m.Type.Recv());

                e.expr(e.teeHole(paramK, closureK), n.Left);
            else if (n.Op == OPTRLIT) 
                e.expr(e.spill(k, n), n.Left);
            else if (n.Op == OARRAYLIT) 
                {
                    var elt__prev1 = elt;

                    foreach (var (_, __elt) in n.List.Slice())
                    {
                        elt = __elt;
                        if (elt.Op == OKEY)
                        {
                            elt = elt.Right;
                        }

                        e.expr(k.note(n, "array literal element"), elt);

                    }

                    elt = elt__prev1;
                }
            else if (n.Op == OSLICELIT) 
                k = e.spill(k, n);
                k.uintptrEscapesHack = uintptrEscapesHack; // for ...uintptr parameters

                {
                    var elt__prev1 = elt;

                    foreach (var (_, __elt) in n.List.Slice())
                    {
                        elt = __elt;
                        if (elt.Op == OKEY)
                        {
                            elt = elt.Right;
                        }

                        e.expr(k.note(n, "slice-literal-element"), elt);

                    }

                    elt = elt__prev1;
                }
            else if (n.Op == OSTRUCTLIT) 
                {
                    var elt__prev1 = elt;

                    foreach (var (_, __elt) in n.List.Slice())
                    {
                        elt = __elt;
                        e.expr(k.note(n, "struct literal element"), elt.Left);
                    }

                    elt = elt__prev1;
                }
            else if (n.Op == OMAPLIT) 
                e.spill(k, n); 

                // Map keys and values are always stored in the heap.
                {
                    var elt__prev1 = elt;

                    foreach (var (_, __elt) in n.List.Slice())
                    {
                        elt = __elt;
                        e.assignHeap(elt.Left, "map literal key", n);
                        e.assignHeap(elt.Right, "map literal value", n);
                    }

                    elt = elt__prev1;
                }
            else if (n.Op == OCLOSURE) 
                k = e.spill(k, n); 

                // Link addresses of captured variables to closure.
                foreach (var (_, v) in n.Func.Closure.Func.Cvars.Slice())
                {
                    if (v.Op == OXXX)
                    { // unnamed out argument; see dcl.go:/^funcargs
                        continue;

                    }

                    var k = k;
                    if (!v.Name.Byval())
                    {
                        k = k.addr(v, "reference");
                    }

                    e.expr(k.note(n, "captured by a closure"), v.Name.Defn);

                }
            else if (n.Op == ORUNES2STR || n.Op == OBYTES2STR || n.Op == OSTR2RUNES || n.Op == OSTR2BYTES || n.Op == ORUNESTR) 
                e.spill(k, n);
                e.discard(n.Left);
            else if (n.Op == OADDSTR) 
                e.spill(k, n); 

                // Arguments of OADDSTR never escape;
                // runtime.concatstrings makes sure of that.
                e.discards(n.List);
            else 
                Fatalf("unexpected expr: %v", n);
            
        });

        // unsafeValue evaluates a uintptr-typed arithmetic expression looking
        // for conversions from an unsafe.Pointer.
        private static void unsafeValue(this ptr<Escape> _addr_e, EscHole k, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (n.Type.Etype != TUINTPTR)
            {
                Fatalf("unexpected type %v for %v", n.Type, n);
            }

            e.stmts(n.Ninit);


            if (n.Op == OCONV || n.Op == OCONVNOP) 
                if (n.Left.Type.Etype == TUNSAFEPTR)
                {
                    e.expr(k, n.Left);
                }
                else
                {
                    e.discard(n.Left);
                }

            else if (n.Op == ODOTPTR) 
                if (isReflectHeaderDataField(n))
                {
                    e.expr(k.deref(n, "reflect.Header.Data"), n.Left);
                }
                else
                {
                    e.discard(n.Left);
                }

            else if (n.Op == OPLUS || n.Op == ONEG || n.Op == OBITNOT) 
                e.unsafeValue(k, n.Left);
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OMUL || n.Op == ODIV || n.Op == OMOD || n.Op == OAND || n.Op == OANDNOT) 
                e.unsafeValue(k, n.Left);
                e.unsafeValue(k, n.Right);
            else if (n.Op == OLSH || n.Op == ORSH) 
                e.unsafeValue(k, n.Left); 
                // RHS need not be uintptr-typed (#32959) and can't meaningfully
                // flow pointers anyway.
                e.discard(n.Right);
            else 
                e.exprSkipInit(e.discardHole(), n);
            
        }

        // discard evaluates an expression n for side-effects, but discards
        // its value.
        private static void discard(this ptr<Escape> _addr_e, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            e.expr(e.discardHole(), n);
        }

        private static void discards(this ptr<Escape> _addr_e, Nodes l)
        {
            ref Escape e = ref _addr_e.val;

            foreach (var (_, n) in l.Slice())
            {
                e.discard(n);
            }

        }

        // addr evaluates an addressable expression n and returns an EscHole
        // that represents storing into the represented location.
        private static EscHole addr(this ptr<Escape> _addr_e, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (n == null || n.isBlank())
            { 
                // Can happen at least in OSELRECV.
                // TODO(mdempsky): Anywhere else?
                return e.discardHole();

            }

            var k = e.heapHole();


            if (n.Op == ONAME) 
                if (n.Class() == PEXTERN)
                {
                    break;
                }

                k = e.oldLoc(n).asHole();
            else if (n.Op == ODOT) 
                k = e.addr(n.Left);
            else if (n.Op == OINDEX) 
                e.discard(n.Right);
                if (n.Left.Type.IsArray())
                {
                    k = e.addr(n.Left);
                }
                else
                {
                    e.discard(n.Left);
                }

            else if (n.Op == ODEREF || n.Op == ODOTPTR) 
                e.discard(n);
            else if (n.Op == OINDEXMAP) 
                e.discard(n.Left);
                e.assignHeap(n.Right, "key of map put", n);
            else 
                Fatalf("unexpected addr: %v", n);
                        if (!types.Haspointers(n.Type))
            {
                k = e.discardHole();
            }

            return k;

        }

        private static slice<EscHole> addrs(this ptr<Escape> _addr_e, Nodes l)
        {
            ref Escape e = ref _addr_e.val;

            slice<EscHole> ks = default;
            foreach (var (_, n) in l.Slice())
            {
                ks = append(ks, e.addr(n));
            }
            return ks;

        }

        // assign evaluates the assignment dst = src.
        private static void assign(this ptr<Escape> _addr_e, ptr<Node> _addr_dst, ptr<Node> _addr_src, @string why, ptr<Node> _addr_where)
        {
            ref Escape e = ref _addr_e.val;
            ref Node dst = ref _addr_dst.val;
            ref Node src = ref _addr_src.val;
            ref Node where = ref _addr_where.val;
 
            // Filter out some no-op assignments for escape analysis.
            var ignore = dst != null && src != null && isSelfAssign(dst, src);
            if (ignore && Debug['m'] != 0L)
            {
                Warnl(where.Pos, "%v ignoring self-assignment in %S", funcSym(e.curfn), where);
            }

            var k = e.addr(dst);
            if (dst != null && dst.Op == ODOTPTR && isReflectHeaderDataField(dst))
            {
                e.unsafeValue(e.heapHole().note(where, why), src);
            }
            else
            {
                if (ignore)
                {
                    k = e.discardHole();
                }

                e.expr(k.note(where, why), src);

            }

        }

        private static void assignHeap(this ptr<Escape> _addr_e, ptr<Node> _addr_src, @string why, ptr<Node> _addr_where)
        {
            ref Escape e = ref _addr_e.val;
            ref Node src = ref _addr_src.val;
            ref Node where = ref _addr_where.val;

            e.expr(e.heapHole().note(where, why), src);
        }

        // call evaluates a call expressions, including builtin calls. ks
        // should contain the holes representing where the function callee's
        // results flows; where is the OGO/ODEFER context of the call, if any.
        private static void call(this ptr<Escape> _addr_e, slice<EscHole> ks, ptr<Node> _addr_call, ptr<Node> _addr_where)
        {
            ref Escape e = ref _addr_e.val;
            ref Node call = ref _addr_call.val;
            ref Node where = ref _addr_where.val;

            var topLevelDefer = where != null && where.Op == ODEFER && e.loopDepth == 1L;
            if (topLevelDefer)
            { 
                // force stack allocation of defer record, unless
                // open-coded defers are used (see ssa.go)
                where.Esc = EscNever;

            }

            Action<EscHole, ptr<Node>> argument = (k, arg) =>
            {
                if (topLevelDefer)
                { 
                    // Top level defers arguments don't escape to
                    // heap, but they do need to last until end of
                    // function.
                    k = e.later(k);

                }
                else if (where != null)
                {
                    k = e.heapHole();
                }

                e.expr(k.note(call, "call parameter"), arg);

            }
;


            if (call.Op == OCALLFUNC || call.Op == OCALLMETH || call.Op == OCALLINTER) 
                fixVariadicCall(call); 

                // Pick out the function callee, if statically known.
                ptr<Node> fn;

                if (call.Op == OCALLFUNC) 
                    if (call.Left.Op == ONAME && call.Left.Class() == PFUNC)
                    {
                        fn = call.Left;
                    }
                    else if (call.Left.Op == OCLOSURE)
                    {
                        fn = call.Left.Func.Closure.Func.Nname;
                    }

                else if (call.Op == OCALLMETH) 
                    fn = asNode(call.Left.Type.FuncType().Nname);
                                var fntype = call.Left.Type;
                if (fn != null)
                {
                    fntype = fn.Type;
                }

                if (ks != null && fn != null && e.inMutualBatch(fn))
                {
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __result) in fn.Type.Results().FieldSlice())
                        {
                            i = __i;
                            result = __result;
                            e.expr(ks[i], asNode(result.Nname));
                        }

                        i = i__prev1;
                    }
                }

                {
                    var r = fntype.Recv();

                    if (r != null)
                    {
                        argument(e.tagHole(ks, fn, r), call.Left.Left);
                    }
                    else
                    { 
                        // Evaluate callee function expression.
                        argument(e.discardHole(), call.Left);

                    }

                }


                var args = call.List.Slice();
                {
                    var i__prev1 = i;

                    foreach (var (__i, __param) in fntype.Params().FieldSlice())
                    {
                        i = __i;
                        param = __param;
                        argument(e.tagHole(ks, fn, param), args[i]);
                    }

                    i = i__prev1;
                }
            else if (call.Op == OAPPEND) 
                args = call.List.Slice(); 

                // Appendee slice may flow directly to the result, if
                // it has enough capacity. Alternatively, a new heap
                // slice might be allocated, and all slice elements
                // might flow to heap.
                var appendeeK = ks[0L];
                if (types.Haspointers(args[0L].Type.Elem()))
                {
                    appendeeK = e.teeHole(appendeeK, e.heapHole().deref(call, "appendee slice"));
                }

                argument(appendeeK, args[0L]);

                if (call.IsDDD())
                {
                    var appendedK = e.discardHole();
                    if (args[1L].Type.IsSlice() && types.Haspointers(args[1L].Type.Elem()))
                    {
                        appendedK = e.heapHole().deref(call, "appended slice...");
                    }

                    argument(appendedK, args[1L]);

                }
                else
                {
                    {
                        var arg__prev1 = arg;

                        foreach (var (_, __arg) in args[1L..])
                        {
                            arg = __arg;
                            argument(e.heapHole(), arg);
                        }

                        arg = arg__prev1;
                    }
                }

            else if (call.Op == OCOPY) 
                argument(e.discardHole(), call.Left);

                var copiedK = e.discardHole();
                if (call.Right.Type.IsSlice() && types.Haspointers(call.Right.Type.Elem()))
                {
                    copiedK = e.heapHole().deref(call, "copied slice");
                }

                argument(copiedK, call.Right);
            else if (call.Op == OPANIC) 
                argument(e.heapHole(), call.Left);
            else if (call.Op == OCOMPLEX) 
                argument(e.discardHole(), call.Left);
                argument(e.discardHole(), call.Right);
            else if (call.Op == ODELETE || call.Op == OPRINT || call.Op == OPRINTN || call.Op == ORECOVER) 
                {
                    var arg__prev1 = arg;

                    foreach (var (_, __arg) in call.List.Slice())
                    {
                        arg = __arg;
                        argument(e.discardHole(), arg);
                    }

                    arg = arg__prev1;
                }
            else if (call.Op == OLEN || call.Op == OCAP || call.Op == OREAL || call.Op == OIMAG || call.Op == OCLOSE) 
                argument(e.discardHole(), call.Left);
            else 
                Fatalf("unexpected call op: %v", call.Op);
            
        }

        // tagHole returns a hole for evaluating an argument passed to param.
        // ks should contain the holes representing where the function
        // callee's results flows. fn is the statically-known callee function,
        // if any.
        private static EscHole tagHole(this ptr<Escape> _addr_e, slice<EscHole> ks, ptr<Node> _addr_fn, ptr<types.Field> _addr_param)
        {
            ref Escape e = ref _addr_e.val;
            ref Node fn = ref _addr_fn.val;
            ref types.Field param = ref _addr_param.val;
 
            // If this is a dynamic call, we can't rely on param.Note.
            if (fn == null)
            {
                return e.heapHole();
            }

            if (e.inMutualBatch(fn))
            {
                return e.addr(asNode(param.Nname));
            } 

            // Call to previously tagged function.
            if (param.Note == uintptrEscapesTag)
            {
                var k = e.heapHole();
                k.uintptrEscapesHack = true;
                return k;
            }

            slice<EscHole> tagKs = default;

            var esc = ParseLeaks(param.Note);
            {
                var x__prev1 = x;

                var x = esc.Heap();

                if (x >= 0L)
                {
                    tagKs = append(tagKs, e.heapHole().shift(x));
                }

                x = x__prev1;

            }


            if (ks != null)
            {
                for (long i = 0L; i < numEscResults; i++)
                {
                    {
                        var x__prev2 = x;

                        x = esc.Result(i);

                        if (x >= 0L)
                        {
                            tagKs = append(tagKs, ks[i].shift(x));
                        }

                        x = x__prev2;

                    }

                }


            }

            return e.teeHole(tagKs);

        }

        // inMutualBatch reports whether function fn is in the batch of
        // mutually recursive functions being analyzed. When this is true,
        // fn has not yet been analyzed, so its parameters and results
        // should be incorporated directly into the flow graph instead of
        // relying on its escape analysis tagging.
        private static bool inMutualBatch(this ptr<Escape> _addr_e, ptr<Node> _addr_fn)
        {
            ref Escape e = ref _addr_e.val;
            ref Node fn = ref _addr_fn.val;

            if (fn.Name.Defn != null && fn.Name.Defn.Esc < EscFuncTagged)
            {
                if (fn.Name.Defn.Esc == EscFuncUnknown)
                {
                    Fatalf("graph inconsistency");
                }

                return true;

            }

            return false;

        }

        // An EscHole represents a context for evaluation a Go
        // expression. E.g., when evaluating p in "x = **p", we'd have a hole
        // with dst==x and derefs==2.
        public partial struct EscHole
        {
            public ptr<EscLocation> dst;
            public long derefs; // >= -1
            public ptr<EscNote> notes; // uintptrEscapesHack indicates this context is evaluating an
// argument for a //go:uintptrescapes function.
            public bool uintptrEscapesHack;
        }

        public partial struct EscNote
        {
            public ptr<EscNote> next;
            public ptr<Node> where;
            public @string why;
        }

        public static EscHole note(this EscHole k, ptr<Node> _addr_where, @string why)
        {
            ref Node where = ref _addr_where.val;

            if (where == null || why == "")
            {
                Fatalf("note: missing where/why");
            }

            if (Debug['m'] >= 2L || logopt.Enabled())
            {
                k.notes = addr(new EscNote(next:k.notes,where:where,why:why,));
            }

            return k;

        }

        public static EscHole shift(this EscHole k, long delta)
        {
            k.derefs += delta;
            if (k.derefs < -1L)
            {
                Fatalf("derefs underflow: %v", k.derefs);
            }

            return k;

        }

        public static EscHole deref(this EscHole k, ptr<Node> _addr_where, @string why)
        {
            ref Node where = ref _addr_where.val;

            return k.shift(1L).note(where, why);
        }
        public static EscHole addr(this EscHole k, ptr<Node> _addr_where, @string why)
        {
            ref Node where = ref _addr_where.val;

            return k.shift(-1L).note(where, why);
        }

        public static EscHole dotType(this EscHole k, ptr<types.Type> _addr_t, ptr<Node> _addr_where, @string why)
        {
            ref types.Type t = ref _addr_t.val;
            ref Node where = ref _addr_where.val;

            if (!t.IsInterface() && !isdirectiface(t))
            {
                k = k.shift(1L);
            }

            return k.note(where, why);

        }

        // teeHole returns a new hole that flows into each hole of ks,
        // similar to the Unix tee(1) command.
        private static EscHole teeHole(this ptr<Escape> _addr_e, params EscHole[] ks)
        {
            ks = ks.Clone();
            ref Escape e = ref _addr_e.val;

            if (len(ks) == 0L)
            {
                return e.discardHole();
            }

            if (len(ks) == 1L)
            {
                return ks[0L];
            } 
            // TODO(mdempsky): Optimize if there's only one non-discard hole?

            // Given holes "l1 = _", "l2 = **_", "l3 = *_", ..., create a
            // new temporary location ltmp, wire it into place, and return
            // a hole for "ltmp = _".
            var loc = e.newLoc(null, true);
            foreach (var (_, k) in ks)
            { 
                // N.B., "p = &q" and "p = &tmp; tmp = q" are not
                // semantically equivalent. To combine holes like "l1
                // = _" and "l2 = &_", we'd need to wire them as "l1 =
                // *ltmp" and "l2 = ltmp" and return "ltmp = &_"
                // instead.
                if (k.derefs < 0L)
                {
                    Fatalf("teeHole: negative derefs");
                }

                e.flow(k, loc);

            }
            return loc.asHole();

        }

        private static EscHole dcl(this ptr<Escape> _addr_e, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            var loc = e.oldLoc(n);
            loc.loopDepth = e.loopDepth;
            return loc.asHole();
        }

        // spill allocates a new location associated with expression n, flows
        // its address to k, and returns a hole that flows values to it. It's
        // intended for use with most expressions that allocate storage.
        private static EscHole spill(this ptr<Escape> _addr_e, EscHole k, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            var loc = e.newLoc(n, true);
            e.flow(k.addr(n, "spill"), loc);
            return loc.asHole();
        }

        // later returns a new hole that flows into k, but some time later.
        // Its main effect is to prevent immediate reuse of temporary
        // variables introduced during Order.
        private static EscHole later(this ptr<Escape> _addr_e, EscHole k)
        {
            ref Escape e = ref _addr_e.val;

            var loc = e.newLoc(null, false);
            e.flow(k, loc);
            return loc.asHole();
        }

        // canonicalNode returns the canonical *Node that n logically
        // represents.
        private static ptr<Node> canonicalNode(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n != null && n.Op == ONAME && n.Name.IsClosureVar())
            {
                n = n.Name.Defn;
                if (n.Name.IsClosureVar())
                {
                    Fatalf("still closure var");
                }

            }

            return _addr_n!;

        }

        private static ptr<EscLocation> newLoc(this ptr<Escape> _addr_e, ptr<Node> _addr_n, bool transient)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            if (e.curfn == null)
            {
                Fatalf("e.curfn isn't set");
            }

            n = canonicalNode(_addr_n);
            ptr<EscLocation> loc = addr(new EscLocation(n:n,curfn:e.curfn,loopDepth:e.loopDepth,transient:transient,));
            e.allLocs = append(e.allLocs, loc);
            if (n != null)
            {
                if (n.Op == ONAME && n.Name.Curfn != e.curfn)
                {
                    Fatalf("curfn mismatch: %v != %v", n.Name.Curfn, e.curfn);
                }

                if (n.HasOpt())
                {
                    Fatalf("%v already has a location", n);
                }

                n.SetOpt(loc);

                if (mustHeapAlloc(n))
                {
                    @string why = "too large for stack";
                    if (n.Op == OMAKESLICE && (!Isconst(n.Left, CTINT) || !Isconst(n.Right, CTINT)))
                    {
                        why = "non-constant size";
                    }

                    e.flow(e.heapHole().addr(n, why), loc);

                }

            }

            return _addr_loc!;

        }

        private static ptr<EscLocation> oldLoc(this ptr<Escape> _addr_e, ptr<Node> _addr_n)
        {
            ref Escape e = ref _addr_e.val;
            ref Node n = ref _addr_n.val;

            n = canonicalNode(_addr_n);
            return n.Opt()._<ptr<EscLocation>>();
        }

        private static EscHole asHole(this ptr<EscLocation> _addr_l)
        {
            ref EscLocation l = ref _addr_l.val;

            return new EscHole(dst:l);
        }

        private static void flow(this ptr<Escape> _addr_e, EscHole k, ptr<EscLocation> _addr_src)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation src = ref _addr_src.val;

            var dst = k.dst;
            if (dst == _addr_e.blankLoc)
            {
                return ;
            }

            if (dst == src && k.derefs >= 0L)
            { // dst = dst, dst = *dst, ...
                return ;

            }

            if (dst.escapes && k.derefs < 0L)
            { // dst = &src
                if (Debug['m'] >= 2L || logopt.Enabled())
                {
                    var pos = linestr(src.n.Pos);
                    if (Debug['m'] >= 2L)
                    {
                        fmt.Printf("%s: %v escapes to heap:\n", pos, src.n);
                    }

                    var explanation = e.explainFlow(pos, dst, src, k.derefs, k.notes, new slice<ptr<logopt.LoggedOpt>>(new ptr<logopt.LoggedOpt>[] {  }));
                    if (logopt.Enabled())
                    {
                        logopt.LogOpt(src.n.Pos, "escapes", "escape", e.curfn.funcname(), fmt.Sprintf("%v escapes to heap", src.n), explanation);
                    }

                }

                src.escapes = true;
                return ;

            } 

            // TODO(mdempsky): Deduplicate edges?
            dst.edges = append(dst.edges, new EscEdge(src:src,derefs:k.derefs,notes:k.notes));

        }

        private static EscHole heapHole(this ptr<Escape> _addr_e)
        {
            ref Escape e = ref _addr_e.val;

            return e.heapLoc.asHole();
        }
        private static EscHole discardHole(this ptr<Escape> _addr_e)
        {
            ref Escape e = ref _addr_e.val;

            return e.blankLoc.asHole();
        }

        // walkAll computes the minimal dereferences between all pairs of
        // locations.
        private static void walkAll(this ptr<Escape> _addr_e)
        {
            ref Escape e = ref _addr_e.val;
 
            // We use a work queue to keep track of locations that we need
            // to visit, and repeatedly walk until we reach a fixed point.
            //
            // We walk once from each location (including the heap), and
            // then re-enqueue each location on its transition from
            // transient->!transient and !escapes->escapes, which can each
            // happen at most once. So we take Θ(len(e.allLocs)) walks.

            // LIFO queue, has enough room for e.allLocs and e.heapLoc.
            var todo = make_slice<ptr<EscLocation>>(0L, len(e.allLocs) + 1L);
            Action<ptr<EscLocation>> enqueue = loc =>
            {
                if (!loc.queued)
                {
                    todo = append(todo, loc);
                    loc.queued = true;
                }

            }
;

            foreach (var (_, loc) in e.allLocs)
            {
                enqueue(loc);
            }
            enqueue(_addr_e.heapLoc);

            uint walkgen = default;
            while (len(todo) > 0L)
            {
                var root = todo[len(todo) - 1L];
                todo = todo[..len(todo) - 1L];
                root.queued = false;

                walkgen++;
                e.walkOne(root, walkgen, enqueue);
            }


        }

        // walkOne computes the minimal number of dereferences from root to
        // all other locations.
        private static void walkOne(this ptr<Escape> _addr_e, ptr<EscLocation> _addr_root, uint walkgen, Action<ptr<EscLocation>> enqueue)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation root = ref _addr_root.val;
 
            // The data flow graph has negative edges (from addressing
            // operations), so we use the Bellman-Ford algorithm. However,
            // we don't have to worry about infinite negative cycles since
            // we bound intermediate dereference counts to 0.

            root.walkgen = walkgen;
            root.derefs = 0L;
            root.dst = null;

            ptr<EscLocation> todo = new slice<ptr<EscLocation>>(new ptr<EscLocation>[] { root }); // LIFO queue
            while (len(todo) > 0L)
            {
                var l = todo[len(todo) - 1L];
                todo = todo[..len(todo) - 1L];

                var @base = l.derefs; 

                // If l.derefs < 0, then l's address flows to root.
                var addressOf = base < 0L;
                if (addressOf)
                { 
                    // For a flow path like "root = &l; l = x",
                    // l's address flows to root, but x's does
                    // not. We recognize this by lower bounding
                    // base at 0.
                    base = 0L; 

                    // If l's address flows to a non-transient
                    // location, then l can't be transiently
                    // allocated.
                    if (!root.transient && l.transient)
                    {
                        l.transient = false;
                        enqueue(l);
                    }

                }

                if (e.outlives(root, l))
                { 
                    // l's value flows to root. If l is a function
                    // parameter and root is the heap or a
                    // corresponding result parameter, then record
                    // that value flow for tagging the function
                    // later.
                    if (l.isName(PPARAM))
                    {
                        if ((logopt.Enabled() || Debug['m'] >= 2L) && !l.escapes)
                        {
                            if (Debug['m'] >= 2L)
                            {
                                fmt.Printf("%s: parameter %v leaks to %s with derefs=%d:\n", linestr(l.n.Pos), l.n, e.explainLoc(root), base);
                            }

                            var explanation = e.explainPath(root, l);
                            if (logopt.Enabled())
                            {
                                logopt.LogOpt(l.n.Pos, "leak", "escape", e.curfn.funcname(), fmt.Sprintf("parameter %v leaks to %s with derefs=%d", l.n, e.explainLoc(root), base), explanation);
                            }

                        }

                        l.leakTo(root, base);

                    } 

                    // If l's address flows somewhere that
                    // outlives it, then l needs to be heap
                    // allocated.
                    if (addressOf && !l.escapes)
                    {
                        if (logopt.Enabled() || Debug['m'] >= 2L)
                        {
                            if (Debug['m'] >= 2L)
                            {
                                fmt.Printf("%s: %v escapes to heap:\n", linestr(l.n.Pos), l.n);
                            }

                            explanation = e.explainPath(root, l);
                            if (logopt.Enabled())
                            {
                                logopt.LogOpt(l.n.Pos, "escape", "escape", e.curfn.funcname(), fmt.Sprintf("%v escapes to heap", l.n), explanation);
                            }

                        }

                        l.escapes = true;
                        enqueue(l);
                        continue;

                    }

                }

                foreach (var (i, edge) in l.edges)
                {
                    if (edge.src.escapes)
                    {
                        continue;
                    }

                    var derefs = base + edge.derefs;
                    if (edge.src.walkgen != walkgen || edge.src.derefs > derefs)
                    {
                        edge.src.walkgen = walkgen;
                        edge.src.derefs = derefs;
                        edge.src.dst = l;
                        edge.src.dstEdgeIdx = i;
                        todo = append(todo, edge.src);
                    }

                }

            }


        }

        // explainPath prints an explanation of how src flows to the walk root.
        private static slice<ptr<logopt.LoggedOpt>> explainPath(this ptr<Escape> _addr_e, ptr<EscLocation> _addr_root, ptr<EscLocation> _addr_src)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation root = ref _addr_root.val;
            ref EscLocation src = ref _addr_src.val;

            var visited = make_map<ptr<EscLocation>, bool>();
            var pos = linestr(src.n.Pos);
            slice<ptr<logopt.LoggedOpt>> explanation = default;
            while (true)
            { 
                // Prevent infinite loop.
                if (visited[src])
                {
                    if (Debug['m'] >= 2L)
                    {
                        fmt.Printf("%s:   warning: truncated explanation due to assignment cycle; see golang.org/issue/35518\n", pos);
                    }

                    break;

                }

                visited[src] = true;
                var dst = src.dst;
                var edge = _addr_dst.edges[src.dstEdgeIdx];
                if (edge.src != src)
                {
                    Fatalf("path inconsistency: %v != %v", edge.src, src);
                }

                explanation = e.explainFlow(pos, dst, src, edge.derefs, edge.notes, explanation);

                if (dst == root)
                {
                    break;
                }

                src = dst;

            }


            return explanation;

        }

        private static slice<ptr<logopt.LoggedOpt>> explainFlow(this ptr<Escape> _addr_e, @string pos, ptr<EscLocation> _addr_dst, ptr<EscLocation> _addr_srcloc, long derefs, ptr<EscNote> _addr_notes, slice<ptr<logopt.LoggedOpt>> explanation)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation dst = ref _addr_dst.val;
            ref EscLocation srcloc = ref _addr_srcloc.val;
            ref EscNote notes = ref _addr_notes.val;

            @string ops = "&";
            if (derefs >= 0L)
            {
                ops = strings.Repeat("*", derefs);
            }

            var print = Debug['m'] >= 2L;

            var flow = fmt.Sprintf("   flow: %s = %s%v:", e.explainLoc(dst), ops, e.explainLoc(srcloc));
            if (print)
            {
                fmt.Printf("%s:%s\n", pos, flow);
            }

            if (logopt.Enabled())
            {
                src.XPos epos = default;
                if (notes != null)
                {
                    epos = notes.where.Pos;
                }
                else if (srcloc != null && srcloc.n != null)
                {
                    epos = srcloc.n.Pos;
                }

                explanation = append(explanation, logopt.NewLoggedOpt(epos, "escflow", "escape", e.curfn.funcname(), flow));

            }

            {
                var note = notes;

                while (note != null)
                {
                    if (print)
                    {
                        fmt.Printf("%s:     from %v (%v) at %s\n", pos, note.where, note.why, linestr(note.where.Pos));
                    note = note.next;
                    }

                    if (logopt.Enabled())
                    {
                        explanation = append(explanation, logopt.NewLoggedOpt(note.where.Pos, "escflow", "escape", e.curfn.funcname(), fmt.Sprintf("     from %v (%v)", note.where, note.why)));
                    }

                }

            }
            return explanation;

        }

        private static @string explainLoc(this ptr<Escape> _addr_e, ptr<EscLocation> _addr_l)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation l = ref _addr_l.val;

            if (l == _addr_e.heapLoc)
            {
                return "{heap}";
            }

            if (l.n == null)
            { 
                // TODO(mdempsky): Omit entirely.
                return "{temp}";

            }

            if (l.n.Op == ONAME)
            {
                return fmt.Sprintf("%v", l.n);
            }

            return fmt.Sprintf("{storage for %v}", l.n);

        }

        // outlives reports whether values stored in l may survive beyond
        // other's lifetime if stack allocated.
        private static bool outlives(this ptr<Escape> _addr_e, ptr<EscLocation> _addr_l, ptr<EscLocation> _addr_other)
        {
            ref Escape e = ref _addr_e.val;
            ref EscLocation l = ref _addr_l.val;
            ref EscLocation other = ref _addr_other.val;
 
            // The heap outlives everything.
            if (l.escapes)
            {
                return true;
            } 

            // We don't know what callers do with returned values, so
            // pessimistically we need to assume they flow to the heap and
            // outlive everything too.
            if (l.isName(PPARAMOUT))
            { 
                // Exception: Directly called closures can return
                // locations allocated outside of them without forcing
                // them to the heap. For example:
                //
                //    var u int  // okay to stack allocate
                //    *(func() *int { return &u }()) = 42
                if (containsClosure(_addr_other.curfn, _addr_l.curfn) && l.curfn.Func.Closure.Func.Top & ctxCallee != 0L)
                {
                    return false;
                }

                return true;

            } 

            // If l and other are within the same function, then l
            // outlives other if it was declared outside other's loop
            // scope. For example:
            //
            //    var l *int
            //    for {
            //        l = new(int)
            //    }
            if (l.curfn == other.curfn && l.loopDepth < other.loopDepth)
            {
                return true;
            } 

            // If other is declared within a child closure of where l is
            // declared, then l outlives it. For example:
            //
            //    var l *int
            //    func() {
            //        l = new(int)
            //    }
            if (containsClosure(_addr_l.curfn, _addr_other.curfn))
            {
                return true;
            }

            return false;

        }

        // containsClosure reports whether c is a closure contained within f.
        private static bool containsClosure(ptr<Node> _addr_f, ptr<Node> _addr_c)
        {
            ref Node f = ref _addr_f.val;
            ref Node c = ref _addr_c.val;

            if (f.Op != ODCLFUNC || c.Op != ODCLFUNC)
            {
                Fatalf("bad containsClosure: %v, %v", f, c);
            } 

            // Common case.
            if (f == c)
            {
                return false;
            } 

            // Closures within function Foo are named like "Foo.funcN..."
            // TODO(mdempsky): Better way to recognize this.
            var fn = f.Func.Nname.Sym.Name;
            var cn = c.Func.Nname.Sym.Name;
            return len(cn) > len(fn) && cn[..len(fn)] == fn && cn[len(fn)] == '.';

        }

        // leak records that parameter l leaks to sink.
        private static void leakTo(this ptr<EscLocation> _addr_l, ptr<EscLocation> _addr_sink, long derefs)
        {
            ref EscLocation l = ref _addr_l.val;
            ref EscLocation sink = ref _addr_sink.val;
 
            // If sink is a result parameter and we can fit return bits
            // into the escape analysis tag, then record a return leak.
            if (sink.isName(PPARAMOUT) && sink.curfn == l.curfn)
            { 
                // TODO(mdempsky): Eliminate dependency on Vargen here.
                var ri = int(sink.n.Name.Vargen) - 1L;
                if (ri < numEscResults)
                { 
                    // Leak to result parameter.
                    l.paramEsc.AddResult(ri, derefs);
                    return ;

                }

            } 

            // Otherwise, record as heap leak.
            l.paramEsc.AddHeap(derefs);

        }

        private static void finish(this ptr<Escape> _addr_e, slice<ptr<Node>> fns)
        {
            ref Escape e = ref _addr_e.val;
 
            // Record parameter tags for package export data.
            foreach (var (_, fn) in fns)
            {
                fn.Esc = EscFuncTagged;

                long narg = 0L;
                foreach (var (_, fs) in _addr_types.RecvsParams)
                {
                    foreach (var (_, f) in fs(fn.Type).Fields().Slice())
                    {
                        narg++;
                        f.Note = e.paramTag(fn, narg, f);
                    }

                }

            }
            foreach (var (_, loc) in e.allLocs)
            {
                var n = loc.n;
                if (n == null)
                {
                    continue;
                }

                n.SetOpt(null); 

                // Update n.Esc based on escape analysis results.

                if (loc.escapes)
                {
                    if (n.Op != ONAME)
                    {
                        if (Debug['m'] != 0L)
                        {
                            Warnl(n.Pos, "%S escapes to heap", n);
                        }

                        if (logopt.Enabled())
                        {
                            logopt.LogOpt(n.Pos, "escape", "escape", e.curfn.funcname());
                        }

                    }

                    n.Esc = EscHeap;
                    addrescapes(n);

                }
                else
                {
                    if (Debug['m'] != 0L && n.Op != ONAME)
                    {
                        Warnl(n.Pos, "%S does not escape", n);
                    }

                    n.Esc = EscNone;
                    if (loc.transient)
                    {
                        n.SetTransient(true);
                    }

                }

            }

        }

        private static bool isName(this ptr<EscLocation> _addr_l, Class c)
        {
            ref EscLocation l = ref _addr_l.val;

            return l.n != null && l.n.Op == ONAME && l.n.Class() == c;
        }

        private static readonly long numEscResults = (long)7L;

        // An EscLeaks represents a set of assignment flows from a parameter
        // to the heap or to any of its function's (first numEscResults)
        // result parameters.


        // An EscLeaks represents a set of assignment flows from a parameter
        // to the heap or to any of its function's (first numEscResults)
        // result parameters.
        public partial struct EscLeaks // : array<byte>
        {
        }

        // Empty reports whether l is an empty set (i.e., no assignment flows).
        public static bool Empty(this EscLeaks l)
        {
            return l == new EscLeaks();
        }

        // Heap returns the minimum deref count of any assignment flow from l
        // to the heap. If no such flows exist, Heap returns -1.
        public static long Heap(this EscLeaks l)
        {
            return l.get(0L);
        }

        // Result returns the minimum deref count of any assignment flow from
        // l to its function's i'th result parameter. If no such flows exist,
        // Result returns -1.
        public static long Result(this EscLeaks l, long i)
        {
            return l.get(1L + i);
        }

        // AddHeap adds an assignment flow from l to the heap.
        private static void AddHeap(this ptr<EscLeaks> _addr_l, long derefs)
        {
            ref EscLeaks l = ref _addr_l.val;

            l.add(0L, derefs);
        }

        // AddResult adds an assignment flow from l to its function's i'th
        // result parameter.
        private static void AddResult(this ptr<EscLeaks> _addr_l, long i, long derefs)
        {
            ref EscLeaks l = ref _addr_l.val;

            l.add(1L + i, derefs);
        }

        private static void setResult(this ptr<EscLeaks> _addr_l, long i, long derefs)
        {
            ref EscLeaks l = ref _addr_l.val;

            l.set(1L + i, derefs);
        }

        public static long get(this EscLeaks l, long i)
        {
            return int(l[i]) - 1L;
        }

        private static void add(this ptr<EscLeaks> _addr_l, long i, long derefs)
        {
            ref EscLeaks l = ref _addr_l.val;

            {
                var old = l.get(i);

                if (old < 0L || derefs < old)
                {
                    l.set(i, derefs);
                }

            }

        }

        private static void set(this ptr<EscLeaks> _addr_l, long i, long derefs)
        {
            ref EscLeaks l = ref _addr_l.val;

            var v = derefs + 1L;
            if (v < 0L)
            {
                Fatalf("invalid derefs count: %v", derefs);
            }

            if (v > math.MaxUint8)
            {
                v = math.MaxUint8;
            }

            l[i] = uint8(v);

        }

        // Optimize removes result flow paths that are equal in length or
        // longer than the shortest heap flow path.
        private static void Optimize(this ptr<EscLeaks> _addr_l)
        {
            ref EscLeaks l = ref _addr_l.val;
 
            // If we have a path to the heap, then there's no use in
            // keeping equal or longer paths elsewhere.
            {
                var x = l.Heap();

                if (x >= 0L)
                {
                    for (long i = 0L; i < numEscResults; i++)
                    {
                        if (l.Result(i) >= x)
                        {
                            l.setResult(i, -1L);
                        }

                    }


                }

            }

        }

        private static map leakTagCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<EscLeaks, @string>{};

        // Encode converts l into a binary string for export data.
        public static @string Encode(this EscLeaks l)
        {
            if (l.Heap() == 0L)
            { 
                // Space optimization: empty string encodes more
                // efficiently in export data.
                return "";

            }

            {
                var s__prev1 = s;

                var (s, ok) = leakTagCache[l];

                if (ok)
                {
                    return s;
                }

                s = s__prev1;

            }


            var n = len(l);
            while (n > 0L && l[n - 1L] == 0L)
            {
                n--;
            }

            @string s = "esc:" + string(l[..n]);
            leakTagCache[l] = s;
            return s;

        }

        // ParseLeaks parses a binary string representing an EscLeaks.
        public static EscLeaks ParseLeaks(@string s)
        {
            EscLeaks l = default;
            if (!strings.HasPrefix(s, "esc:"))
            {
                l.AddHeap(0L);
                return l;
            }

            copy(l[..], s[4L..]);
            return l;

        }
    }
}}}}
