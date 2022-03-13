// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package escape -- go2cs converted at 2022 March 13 06:22:33 UTC
// import "cmd/compile/internal/escape" ==> using escape = go.cmd.compile.@internal.escape_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\escape\escape.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using math = math_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using logopt = cmd.compile.@internal.logopt_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


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

// A batch holds escape analysis state that's shared across an entire
// batch of functions being analyzed at once.

using System;
public static partial class escape_package {

private partial struct batch {
    public slice<ptr<location>> allLocs;
    public slice<closure> closures;
    public location heapLoc;
    public location blankLoc;
}

// A closure holds a closure expression and its spill hole (i.e.,
// where the hole representing storing into its closure record).
private partial struct closure {
    public hole k;
    public ptr<ir.ClosureExpr> clo;
}

// An escape holds state specific to a single function being analyzed
// within a batch.
private partial struct escape {
    public ref ptr<batch> ptr<batch> => ref ptr<batch>_ptr;
    public ptr<ir.Func> curfn; // function being analyzed

    public map<ptr<types.Sym>, labelState> labels; // known labels

// loopDepth counts the current loop nesting depth within
// curfn. It increments within each "for" loop and at each
// label with a corresponding backwards "goto" (i.e.,
// unstructured loop).
    public nint loopDepth;
}

// An location represents an abstract location that stores a Go
// variable.
private partial struct location {
    public ir.Node n; // represented variable or expression, if any
    public ptr<ir.Func> curfn; // enclosing function
    public slice<edge> edges; // incoming edges
    public nint loopDepth; // loopDepth at declaration

// resultIndex records the tuple index (starting at 1) for
// PPARAMOUT variables within their function's result type.
// For non-PPARAMOUT variables it's 0.
    public nint resultIndex; // derefs and walkgen are used during walkOne to track the
// minimal dereferences from the walk root.
    public nint derefs; // >= -1
    public uint walkgen; // dst and dstEdgeindex track the next immediate assignment
// destination location during walkone, along with the index
// of the edge pointing back to this location.
    public ptr<location> dst;
    public nint dstEdgeIdx; // queued is used by walkAll to track whether this location is
// in the walk queue.
    public bool queued; // escapes reports whether the represented variable's address
// escapes; that is, whether the variable must be heap
// allocated.
    public bool escapes; // transient reports whether the represented expression's
// address does not outlive the statement; that is, whether
// its storage can be immediately reused.
    public bool transient; // paramEsc records the represented parameter's leak set.
    public leaks paramEsc;
    public bool captured; // has a closure captured this variable?
    public bool reassigned; // has this variable been reassigned?
    public bool addrtaken; // has this variable's address been taken?
}

// An edge represents an assignment edge between two Go variables.
private partial struct edge {
    public ptr<location> src;
    public nint derefs; // >= -1
    public ptr<note> notes;
}

// Fmt is called from node printing to print information about escape analysis results.
public static @string Fmt(ir.Node n) {
    @string text = "";

    if (n.Esc() == ir.EscUnknown) 
        break;
    else if (n.Esc() == ir.EscHeap) 
        text = "esc(h)";
    else if (n.Esc() == ir.EscNone) 
        text = "esc(no)";
    else if (n.Esc() == ir.EscNever) 
        text = "esc(N)";
    else 
        text = fmt.Sprintf("esc(%d)", n.Esc());
        if (n.Op() == ir.ONAME) {
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        {
            ptr<location> (loc, ok) = n.Opt._<ptr<location>>();

            if (ok && loc.loopDepth != 0) {
                if (text != "") {
                    text += " ";
                }
                text += fmt.Sprintf("ld(%d)", loc.loopDepth);
            }

        }
    }
    return text;
}

// Batch performs escape analysis on a minimal batch of
// functions.
public static void Batch(slice<ptr<ir.Func>> fns, bool recursive) {
    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in fns) {
            fn = __fn;
            if (fn.Op() != ir.ODCLFUNC) {
                @base.Fatalf("unexpected node: %v", fn);
            }
        }
        fn = fn__prev1;
    }

    batch b = default;
    b.heapLoc.escapes = true; 

    // Construct data-flow graph from syntax trees.
    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in fns) {
            fn = __fn;
            if (@base.Flag.W > 1) {
                var s = fmt.Sprintf("\nbefore escape %v", fn);
                ir.Dump(s, fn);
            }
            b.initFunc(fn);
        }
        fn = fn__prev1;
    }

    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in fns) {
            fn = __fn;
            if (!fn.IsHiddenClosure()) {
                b.walkFunc(fn);
            }
        }
        fn = fn__prev1;
    }

    foreach (var (_, closure) in b.closures) {
        b.flowClosure(closure.k, closure.clo);
    }    b.closures = null;

    foreach (var (_, loc) in b.allLocs) {
        {
            var why = HeapAllocReason(loc.n);

            if (why != "") {
                b.flow(b.heapHole().addr(loc.n, why), loc);
            }

        }
    }    b.walkAll();
    b.finish(fns);
}

private static ptr<escape> with(this ptr<batch> _addr_b, ptr<ir.Func> _addr_fn) {
    ref batch b = ref _addr_b.val;
    ref ir.Func fn = ref _addr_fn.val;

    return addr(new escape(batch:b,curfn:fn,loopDepth:1,));
}

private static void initFunc(this ptr<batch> _addr_b, ptr<ir.Func> _addr_fn) {
    ref batch b = ref _addr_b.val;
    ref ir.Func fn = ref _addr_fn.val;

    var e = b.with(fn);
    if (fn.Esc() != escFuncUnknown) {
        @base.Fatalf("unexpected node: %v", fn);
    }
    fn.SetEsc(escFuncPlanned);
    if (@base.Flag.LowerM > 3) {
        ir.Dump("escAnalyze", fn);
    }
    foreach (var (_, n) in fn.Dcl) {
        if (n.Op() == ir.ONAME) {
            e.newLoc(n, false);
        }
    }    foreach (var (i, f) in fn.Type().Results().FieldSlice()) {
        e.oldLoc;

        (f.Nname._<ptr<ir.Name>>()).resultIndex = 1 + i;
    }
}

private static void walkFunc(this ptr<batch> _addr_b, ptr<ir.Func> _addr_fn) {
    ref batch b = ref _addr_b.val;
    ref ir.Func fn = ref _addr_fn.val;

    var e = b.with(fn);
    fn.SetEsc(escFuncStarted); 

    // Identify labels that mark the head of an unstructured loop.
    ir.Visit(fn, n => {

        if (n.Op() == ir.OLABEL) 
            ptr<ir.LabelStmt> n = n._<ptr<ir.LabelStmt>>();
            if (e.labels == null) {
                e.labels = make_map<ptr<types.Sym>, labelState>();
            }
            e.labels[n.Label] = nonlooping;
        else if (n.Op() == ir.OGOTO) 
            // If we visited the label before the goto,
            // then this is a looping label.
            n = n._<ptr<ir.BranchStmt>>();
            if (e.labels[n.Label] == nonlooping) {
                e.labels[n.Label] = looping;
            }
            });

    e.block(fn.Body);

    if (len(e.labels) != 0) {
        @base.FatalfAt(fn.Pos(), "leftover labels after walkFunc");
    }
}

private static void flowClosure(this ptr<batch> _addr_b, hole k, ptr<ir.ClosureExpr> _addr_clo) {
    ref batch b = ref _addr_b.val;
    ref ir.ClosureExpr clo = ref _addr_clo.val;

    foreach (var (_, cv) in clo.Func.ClosureVars) {
        var n = cv.Canonical();
        var loc = b.oldLoc(cv);
        if (!loc.captured) {
            @base.FatalfAt(cv.Pos(), "closure variable never captured: %v", cv);
        }
        n.SetByval(!loc.addrtaken && !loc.reassigned && n.Type().Size() <= 128);
        if (!n.Byval()) {
            n.SetAddrtaken(true);
        }
        if (@base.Flag.LowerM > 1) {
            @string how = "ref";
            if (n.Byval()) {
                how = "value";
            }
            @base.WarnfAt(n.Pos(), "%v capturing by %s: %v (addr=%v assign=%v width=%d)", n.Curfn, how, n, loc.addrtaken, loc.reassigned, n.Type().Size());
        }
        var k = k;
        if (!cv.Byval()) {
            k = k.addr(cv, "reference");
        }
        b.flow(k.note(cv, "captured by a closure"), loc);
    }
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
private static void stmt(this ptr<escape> _addr_e, ir.Node n) => func((defer, _, _) => {
    ref escape e = ref _addr_e.val;

    if (n == null) {
        return ;
    }
    var lno = ir.SetPos(n);
    defer(() => {
        @base.Pos = lno;
    }());

    if (@base.Flag.LowerM > 2) {
        fmt.Printf("%v:[%d] %v stmt: %v\n", @base.FmtPos(@base.Pos), e.loopDepth, e.curfn, n);
    }
    e.stmts(n.Init());


    if (n.Op() == ir.ODCLCONST || n.Op() == ir.ODCLTYPE || n.Op() == ir.OFALL || n.Op() == ir.OINLMARK)     else if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE || n.Op() == ir.OGOTO)     else if (n.Op() == ir.OBLOCK) 
        ptr<ir.BlockStmt> n = n._<ptr<ir.BlockStmt>>();
        e.stmts(n.List);
    else if (n.Op() == ir.ODCL) 
        // Record loop depth at declaration.
        n = n._<ptr<ir.Decl>>();
        if (!ir.IsBlank(n.X)) {
            e.dcl(n.X);
        }
    else if (n.Op() == ir.OLABEL) 
        n = n._<ptr<ir.LabelStmt>>();

        if (e.labels[n.Label] == nonlooping) 
            if (@base.Flag.LowerM > 2) {
                fmt.Printf("%v:%v non-looping label\n", @base.FmtPos(@base.Pos), n);
            }
        else if (e.labels[n.Label] == looping) 
            if (@base.Flag.LowerM > 2) {
                fmt.Printf("%v: %v looping label\n", @base.FmtPos(@base.Pos), n);
            }
            e.loopDepth++;
        else 
            @base.Fatalf("label missing tag");
                delete(e.labels, n.Label);
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        e.discard(n.Cond);
        e.block(n.Body);
        e.block(n.Else);
    else if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL) 
        n = n._<ptr<ir.ForStmt>>();
        e.loopDepth++;
        e.discard(n.Cond);
        e.stmt(n.Post);
        e.block(n.Body);
        e.loopDepth--;
    else if (n.Op() == ir.ORANGE) 
        // for Key, Value = range X { Body }
        n = n._<ptr<ir.RangeStmt>>(); 

        // X is evaluated outside the loop.
        var tmp = e.newLoc(null, false);
        e.expr(tmp.asHole(), n.X);

        e.loopDepth++;
        var ks = e.addrs(new slice<ir.Node>(new ir.Node[] { n.Key, n.Value }));
        if (n.X.Type().IsArray()) {
            e.flow(ks[1].note(n, "range"), tmp);
        }
        else
 {
            e.flow(ks[1].deref(n, "range-deref"), tmp);
        }
        e.reassigned(ks, n);

        e.block(n.Body);
        e.loopDepth--;
    else if (n.Op() == ir.OSWITCH) 
        n = n._<ptr<ir.SwitchStmt>>();

        {
            ptr<ir.TypeSwitchGuard> (guard, ok) = n.Tag._<ptr<ir.TypeSwitchGuard>>();

            if (ok) {
                ks = default;
                if (guard.Tag != null) {
                    {
                        var cas__prev1 = cas;

                        foreach (var (_, __cas) in n.Cases) {
                            cas = __cas;
                            var cv = cas.Var;
                            var k = e.dcl(cv); // type switch variables have no ODCL.
                            if (cv.Type().HasPointers()) {
                                ks = append(ks, k.dotType(cv.Type(), cas, "switch case"));
                            }
                        }

                        cas = cas__prev1;
                    }
                }
            else
                e.expr(e.teeHole(ks), n.Tag._<ptr<ir.TypeSwitchGuard>>().X);
            } {
                e.discard(n.Tag);
            }

        }

        {
            var cas__prev1 = cas;

            foreach (var (_, __cas) in n.Cases) {
                cas = __cas;
                e.discards(cas.List);
                e.block(cas.Body);
            }

            cas = cas__prev1;
        }
    else if (n.Op() == ir.OSELECT) 
        n = n._<ptr<ir.SelectStmt>>();
        {
            var cas__prev1 = cas;

            foreach (var (_, __cas) in n.Cases) {
                cas = __cas;
                e.stmt(cas.Comm);
                e.block(cas.Body);
            }

            cas = cas__prev1;
        }
    else if (n.Op() == ir.ORECV) 
        // TODO(mdempsky): Consider e.discard(n.Left).
        n = n._<ptr<ir.UnaryExpr>>();
        e.exprSkipInit(e.discardHole(), n); // already visited n.Ninit
    else if (n.Op() == ir.OSEND) 
        n = n._<ptr<ir.SendStmt>>();
        e.discard(n.Chan);
        e.assignHeap(n.Value, "send", n);
    else if (n.Op() == ir.OAS) 
        n = n._<ptr<ir.AssignStmt>>();
        e.assignList(new slice<ir.Node>(new ir.Node[] { n.X }), new slice<ir.Node>(new ir.Node[] { n.Y }), "assign", n);
    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>(); 
        // TODO(mdempsky): Worry about OLSH/ORSH?
        e.assignList(new slice<ir.Node>(new ir.Node[] { n.X }), new slice<ir.Node>(new ir.Node[] { n.Y }), "assign", n);
    else if (n.Op() == ir.OAS2) 
        n = n._<ptr<ir.AssignListStmt>>();
        e.assignList(n.Lhs, n.Rhs, "assign-pair", n);
    else if (n.Op() == ir.OAS2DOTTYPE) // v, ok = x.(type)
        n = n._<ptr<ir.AssignListStmt>>();
        e.assignList(n.Lhs, n.Rhs, "assign-pair-dot-type", n);
    else if (n.Op() == ir.OAS2MAPR) // v, ok = m[k]
        n = n._<ptr<ir.AssignListStmt>>();
        e.assignList(n.Lhs, n.Rhs, "assign-pair-mapr", n);
    else if (n.Op() == ir.OAS2RECV || n.Op() == ir.OSELRECV2) // v, ok = <-ch
        n = n._<ptr<ir.AssignListStmt>>();
        e.assignList(n.Lhs, n.Rhs, "assign-pair-receive", n);
    else if (n.Op() == ir.OAS2FUNC) 
        n = n._<ptr<ir.AssignListStmt>>();
        e.stmts(n.Rhs[0].Init());
        ks = e.addrs(n.Lhs);
        e.call(ks, n.Rhs[0], null);
        e.reassigned(ks, n);
    else if (n.Op() == ir.ORETURN) 
        n = n._<ptr<ir.ReturnStmt>>();
        var results = e.curfn.Type().Results().FieldSlice();
        var dsts = make_slice<ir.Node>(len(results));
        foreach (var (i, res) in results) {
            dsts[i] = res.Nname._<ptr<ir.Name>>();
        }        e.assignList(dsts, n.Results, "return", n);
    else if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER || n.Op() == ir.OCLOSE || n.Op() == ir.OCOPY || n.Op() == ir.ODELETE || n.Op() == ir.OPANIC || n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN || n.Op() == ir.ORECOVER) 
        e.call(null, n, null);
    else if (n.Op() == ir.OGO || n.Op() == ir.ODEFER) 
        n = n._<ptr<ir.GoDeferStmt>>();
        e.stmts(n.Call.Init());
        e.call(null, n.Call, n);
    else if (n.Op() == ir.OTAILCALL)     else 
        @base.Fatalf("unexpected stmt: %v", n);
    });

private static void stmts(this ptr<escape> _addr_e, ir.Nodes l) {
    ref escape e = ref _addr_e.val;

    foreach (var (_, n) in l) {
        e.stmt(n);
    }
}

// block is like stmts, but preserves loopDepth.
private static void block(this ptr<escape> _addr_e, ir.Nodes l) {
    ref escape e = ref _addr_e.val;

    var old = e.loopDepth;
    e.stmts(l);
    e.loopDepth = old;
}

// expr models evaluating an expression n and flowing the result into
// hole k.
private static void expr(this ptr<escape> _addr_e, hole k, ir.Node n) {
    ref escape e = ref _addr_e.val;

    if (n == null) {
        return ;
    }
    e.stmts(n.Init());
    e.exprSkipInit(k, n);
}

private static void exprSkipInit(this ptr<escape> _addr_e, hole k, ir.Node n) => func((defer, _, _) => {
    ref escape e = ref _addr_e.val;

    if (n == null) {
        return ;
    }
    var lno = ir.SetPos(n);
    defer(() => {
        @base.Pos = lno;
    }());

    var uintptrEscapesHack = k.uintptrEscapesHack;
    k.uintptrEscapesHack = false;

    if (uintptrEscapesHack && n.Op() == ir.OCONVNOP && n._<ptr<ir.ConvExpr>>().X.Type().IsUnsafePtr()) { 
        // nop
    }
    else if (k.derefs >= 0 && !n.Type().HasPointers()) {
        k.dst = _addr_e.blankLoc;
    }

    if (n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.OGETG || n.Op() == ir.OTYPE || n.Op() == ir.OMETHEXPR || n.Op() == ir.OLINKSYMOFFSET)     else if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PFUNC || n.Class == ir.PEXTERN) {
            return ;
        }
        if (n.IsClosureVar() && n.Defn == null) {
            return ; // ".this" from method value wrapper
        }
        e.flow(k, e.oldLoc(n));
    else if (n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OBITNOT || n.Op() == ir.ONOT) 
        n = n._<ptr<ir.UnaryExpr>>();
        e.discard(n.X);
    else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OMUL || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OLSH || n.Op() == ir.ORSH || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE) 
        n = n._<ptr<ir.BinaryExpr>>();
        e.discard(n.X);
        e.discard(n.Y);
    else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
        n = n._<ptr<ir.LogicalExpr>>();
        e.discard(n.X);
        e.discard(n.Y);
    else if (n.Op() == ir.OADDR) 
        n = n._<ptr<ir.AddrExpr>>();
        e.expr(k.addr(n, "address-of"), n.X); // "address-of"
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        e.expr(k.deref(n, "indirection"), n.X); // "indirection"
    else if (n.Op() == ir.ODOT || n.Op() == ir.ODOTMETH || n.Op() == ir.ODOTINTER) 
        n = n._<ptr<ir.SelectorExpr>>();
        e.expr(k.note(n, "dot"), n.X);
    else if (n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        e.expr(k.deref(n, "dot of pointer"), n.X); // "dot of pointer"
    else if (n.Op() == ir.ODOTTYPE || n.Op() == ir.ODOTTYPE2) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        e.expr(k.dotType(n.Type(), n, "dot"), n.X);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        if (n.X.Type().IsArray()) {
            e.expr(k.note(n, "fixed-array-index-of"), n.X);
        }
        else
 { 
            // TODO(mdempsky): Fix why reason text.
            e.expr(k.deref(n, "dot of pointer"), n.X);
        }
        e.discard(n.Index);
    else if (n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        e.discard(n.X);
        e.discard(n.Index);
    else if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICEARR || n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR || n.Op() == ir.OSLICESTR) 
        n = n._<ptr<ir.SliceExpr>>();
        e.expr(k.note(n, "slice"), n.X);
        e.discard(n.Low);
        e.discard(n.High);
        e.discard(n.Max);
    else if (n.Op() == ir.OCONV || n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        if (ir.ShouldCheckPtr(e.curfn, 2) && n.Type().IsUnsafePtr() && n.X.Type().IsPtr()) { 
            // When -d=checkptr=2 is enabled, treat
            // conversions to unsafe.Pointer as an
            // escaping operation. This allows better
            // runtime instrumentation, since we can more
            // easily detect object boundaries on the heap
            // than the stack.
            e.assignHeap(n.X, "conversion to unsafe.Pointer", n);
        }
        else if (n.Type().IsUnsafePtr() && n.X.Type().IsUintptr()) {
            e.unsafeValue(k, n.X);
        }
        else
 {
            e.expr(k, n.X);
        }
    else if (n.Op() == ir.OCONVIFACE) 
        n = n._<ptr<ir.ConvExpr>>();
        if (!n.X.Type().IsInterface() && !types.IsDirectIface(n.X.Type())) {
            k = e.spill(k, n);
        }
        e.expr(k.note(n, "interface-converted"), n.X);
    else if (n.Op() == ir.OSLICE2ARRPTR) 
        // the slice pointer flows directly to the result
        n = n._<ptr<ir.ConvExpr>>();
        e.expr(k, n.X);
    else if (n.Op() == ir.ORECV) 
        n = n._<ptr<ir.UnaryExpr>>();
        e.discard(n.X);
    else if (n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER || n.Op() == ir.OLEN || n.Op() == ir.OCAP || n.Op() == ir.OCOMPLEX || n.Op() == ir.OREAL || n.Op() == ir.OIMAG || n.Op() == ir.OAPPEND || n.Op() == ir.OCOPY || n.Op() == ir.OUNSAFEADD || n.Op() == ir.OUNSAFESLICE) 
        e.call(new slice<hole>(new hole[] { k }), n, null);
    else if (n.Op() == ir.ONEW) 
        n = n._<ptr<ir.UnaryExpr>>();
        e.spill(k, n);
    else if (n.Op() == ir.OMAKESLICE) 
        n = n._<ptr<ir.MakeExpr>>();
        e.spill(k, n);
        e.discard(n.Len);
        e.discard(n.Cap);
    else if (n.Op() == ir.OMAKECHAN) 
        n = n._<ptr<ir.MakeExpr>>();
        e.discard(n.Len);
    else if (n.Op() == ir.OMAKEMAP) 
        n = n._<ptr<ir.MakeExpr>>();
        e.spill(k, n);
        e.discard(n.Len);
    else if (n.Op() == ir.ORECOVER)     else if (n.Op() == ir.OCALLPART) 
        // Flow the receiver argument to both the closure and
        // to the receiver parameter.

        n = n._<ptr<ir.SelectorExpr>>();
        var closureK = e.spill(k, n);

        var m = n.Selection; 

        // We don't know how the method value will be called
        // later, so conservatively assume the result
        // parameters all flow to the heap.
        //
        // TODO(mdempsky): Change ks into a callback, so that
        // we don't have to create this slice?
        slice<hole> ks = default;
        for (var i = m.Type.NumResults(); i > 0; i--) {
            ks = append(ks, e.heapHole());
        }
        ptr<ir.Name> (name, _) = m.Nname._<ptr<ir.Name>>();
        var paramK = e.tagHole(ks, name, m.Type.Recv());

        e.expr(e.teeHole(paramK, closureK), n.X);
    else if (n.Op() == ir.OPTRLIT) 
        n = n._<ptr<ir.AddrExpr>>();
        e.expr(e.spill(k, n), n.X);
    else if (n.Op() == ir.OARRAYLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var elt__prev1 = elt;

            foreach (var (_, __elt) in n.List) {
                elt = __elt;
                if (elt.Op() == ir.OKEY) {
                    elt = elt._<ptr<ir.KeyExpr>>().Value;
                }
                e.expr(k.note(n, "array literal element"), elt);
            }

            elt = elt__prev1;
        }
    else if (n.Op() == ir.OSLICELIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        k = e.spill(k, n);
        k.uintptrEscapesHack = uintptrEscapesHack; // for ...uintptr parameters

        {
            var elt__prev1 = elt;

            foreach (var (_, __elt) in n.List) {
                elt = __elt;
                if (elt.Op() == ir.OKEY) {
                    elt = elt._<ptr<ir.KeyExpr>>().Value;
                }
                e.expr(k.note(n, "slice-literal-element"), elt);
            }

            elt = elt__prev1;
        }
    else if (n.Op() == ir.OSTRUCTLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var elt__prev1 = elt;

            foreach (var (_, __elt) in n.List) {
                elt = __elt;
                e.expr(k.note(n, "struct literal element"), elt._<ptr<ir.StructKeyExpr>>().Value);
            }

            elt = elt__prev1;
        }
    else if (n.Op() == ir.OMAPLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        e.spill(k, n); 

        // Map keys and values are always stored in the heap.
        {
            var elt__prev1 = elt;

            foreach (var (_, __elt) in n.List) {
                elt = __elt;
                ptr<ir.KeyExpr> elt = elt._<ptr<ir.KeyExpr>>();
                e.assignHeap(elt.Key, "map literal key", n);
                e.assignHeap(elt.Value, "map literal value", n);
            }

            elt = elt__prev1;
        }
    else if (n.Op() == ir.OCLOSURE) 
        n = n._<ptr<ir.ClosureExpr>>();
        k = e.spill(k, n);
        e.closures = append(e.closures, new closure(k,n));

        {
            var fn = n.Func;

            if (fn.IsHiddenClosure()) {
                foreach (var (_, cv) in fn.ClosureVars) {
                    {
                        var loc = e.oldLoc(cv);

                        if (!loc.captured) {
                            loc.captured = true; 

                            // Ignore reassignments to the variable in straightline code
                            // preceding the first capture by a closure.
                            if (loc.loopDepth == e.loopDepth) {
                                loc.reassigned = false;
                            }
                        }

                    }
                }
                {
                    ptr<ir.Name> n__prev1 = n;

                    foreach (var (_, __n) in fn.Dcl) {
                        n = __n; 
                        // Add locations for local variables of the
                        // closure, if needed, in case we're not including
                        // the closure func in the batch for escape
                        // analysis (happens for escape analysis called
                        // from reflectdata.methodWrapper)
                        if (n.Op() == ir.ONAME && n.Opt == null) {
                            e.with(fn).newLoc(n, false);
                        }
                    }

                    n = n__prev1;
                }

                e.walkFunc(fn);
            }

        }
    else if (n.Op() == ir.ORUNES2STR || n.Op() == ir.OBYTES2STR || n.Op() == ir.OSTR2RUNES || n.Op() == ir.OSTR2BYTES || n.Op() == ir.ORUNESTR) 
        n = n._<ptr<ir.ConvExpr>>();
        e.spill(k, n);
        e.discard(n.X);
    else if (n.Op() == ir.OADDSTR) 
        n = n._<ptr<ir.AddStringExpr>>();
        e.spill(k, n); 

        // Arguments of OADDSTR never escape;
        // runtime.concatstrings makes sure of that.
        e.discards(n.List);
    else 
        @base.Fatalf("unexpected expr: %s %v", n.Op().String(), n);
    });

// unsafeValue evaluates a uintptr-typed arithmetic expression looking
// for conversions from an unsafe.Pointer.
private static void unsafeValue(this ptr<escape> _addr_e, hole k, ir.Node n) {
    ref escape e = ref _addr_e.val;

    if (n.Type().Kind() != types.TUINTPTR) {
        @base.Fatalf("unexpected type %v for %v", n.Type(), n);
    }
    if (k.addrtaken) {
        @base.Fatalf("unexpected addrtaken");
    }
    e.stmts(n.Init());


    if (n.Op() == ir.OCONV || n.Op() == ir.OCONVNOP) 
        ptr<ir.ConvExpr> n = n._<ptr<ir.ConvExpr>>();
        if (n.X.Type().IsUnsafePtr()) {
            e.expr(k, n.X);
        }
        else
 {
            e.discard(n.X);
        }
    else if (n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        if (ir.IsReflectHeaderDataField(n)) {
            e.expr(k.deref(n, "reflect.Header.Data"), n.X);
        }
        else
 {
            e.discard(n.X);
        }
    else if (n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OBITNOT) 
        n = n._<ptr<ir.UnaryExpr>>();
        e.unsafeValue(k, n.X);
    else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OMUL || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OAND || n.Op() == ir.OANDNOT) 
        n = n._<ptr<ir.BinaryExpr>>();
        e.unsafeValue(k, n.X);
        e.unsafeValue(k, n.Y);
    else if (n.Op() == ir.OLSH || n.Op() == ir.ORSH) 
        n = n._<ptr<ir.BinaryExpr>>();
        e.unsafeValue(k, n.X); 
        // RHS need not be uintptr-typed (#32959) and can't meaningfully
        // flow pointers anyway.
        e.discard(n.Y);
    else 
        e.exprSkipInit(e.discardHole(), n);
    }

// discard evaluates an expression n for side-effects, but discards
// its value.
private static void discard(this ptr<escape> _addr_e, ir.Node n) {
    ref escape e = ref _addr_e.val;

    e.expr(e.discardHole(), n);
}

private static void discards(this ptr<escape> _addr_e, ir.Nodes l) {
    ref escape e = ref _addr_e.val;

    foreach (var (_, n) in l) {
        e.discard(n);
    }
}

// addr evaluates an addressable expression n and returns a hole
// that represents storing into the represented location.
private static hole addr(this ptr<escape> _addr_e, ir.Node n) {
    ref escape e = ref _addr_e.val;

    if (n == null || ir.IsBlank(n)) { 
        // Can happen in select case, range, maybe others.
        return e.discardHole();
    }
    var k = e.heapHole();


    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PEXTERN) {
            break;
        }
        k = e.oldLoc(n).asHole();
    else if (n.Op() == ir.OLINKSYMOFFSET) 
        break;
    else if (n.Op() == ir.ODOT) 
        n = n._<ptr<ir.SelectorExpr>>();
        k = e.addr(n.X);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        e.discard(n.Index);
        if (n.X.Type().IsArray()) {
            k = e.addr(n.X);
        }
        else
 {
            e.discard(n.X);
        }
    else if (n.Op() == ir.ODEREF || n.Op() == ir.ODOTPTR) 
        e.discard(n);
    else if (n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        e.discard(n.X);
        e.assignHeap(n.Index, "key of map put", n);
    else 
        @base.Fatalf("unexpected addr: %v", n);
        return k;
}

private static slice<hole> addrs(this ptr<escape> _addr_e, ir.Nodes l) {
    ref escape e = ref _addr_e.val;

    slice<hole> ks = default;
    foreach (var (_, n) in l) {
        ks = append(ks, e.addr(n));
    }    return ks;
}

// reassigned marks the locations associated with the given holes as
// reassigned, unless the location represents a variable declared and
// assigned exactly once by where.
private static void reassigned(this ptr<escape> _addr_e, slice<hole> ks, ir.Node where) {
    ref escape e = ref _addr_e.val;

    {
        ptr<ir.AssignStmt> (as, ok) = where._<ptr<ir.AssignStmt>>();

        if (ok && @as.Op() == ir.OAS && @as.Y == null) {
            {
                ptr<ir.Name> (dst, ok) = @as.X._<ptr<ir.Name>>();

                if (ok && dst.Op() == ir.ONAME && dst.Defn == null) { 
                    // Zero-value assignment for variable declared without an
                    // explicit initial value. Assume this is its initialization
                    // statement.
                    return ;
                }

            }
        }
    }

    foreach (var (_, k) in ks) {
        var loc = k.dst; 
        // Variables declared by range statements are assigned on every iteration.
        {
            ptr<ir.Name> (n, ok) = loc.n._<ptr<ir.Name>>();

            if (ok && n.Defn == where && where.Op() != ir.ORANGE) {
                continue;
            }

        }
        loc.reassigned = true;
    }
}

// assignList evaluates the assignment dsts... = srcs....
private static void assignList(this ptr<escape> _addr_e, slice<ir.Node> dsts, slice<ir.Node> srcs, @string why, ir.Node where) {
    ref escape e = ref _addr_e.val;

    var ks = e.addrs(dsts);
    foreach (var (i, k) in ks) {
        ir.Node src = default;
        if (i < len(srcs)) {
            src = srcs[i];
        }
        {
            var dst = dsts[i];

            if (dst != null) { 
                // Detect implicit conversion of uintptr to unsafe.Pointer when
                // storing into reflect.{Slice,String}Header.
                if (dst.Op() == ir.ODOTPTR && ir.IsReflectHeaderDataField(dst)) {
                    e.unsafeValue(e.heapHole().note(where, why), src);
                    continue;
                } 

                // Filter out some no-op assignments for escape analysis.
                if (src != null && isSelfAssign(dst, src)) {
                    if (@base.Flag.LowerM != 0) {
                        @base.WarnfAt(where.Pos(), "%v ignoring self-assignment in %v", e.curfn, where);
                    }
                    k = e.discardHole();
                }
            }

        }

        e.expr(k.note(where, why), src);
    }    e.reassigned(ks, where);
}

private static void assignHeap(this ptr<escape> _addr_e, ir.Node src, @string why, ir.Node where) {
    ref escape e = ref _addr_e.val;

    e.expr(e.heapHole().note(where, why), src);
}

// call evaluates a call expressions, including builtin calls. ks
// should contain the holes representing where the function callee's
// results flows; where is the OGO/ODEFER context of the call, if any.
private static void call(this ptr<escape> _addr_e, slice<hole> ks, ir.Node call, ir.Node where) {
    ref escape e = ref _addr_e.val;

    var topLevelDefer = where != null && where.Op() == ir.ODEFER && e.loopDepth == 1;
    if (topLevelDefer) { 
        // force stack allocation of defer record, unless
        // open-coded defers are used (see ssa.go)
        where.SetEsc(ir.EscNever);
    }
    Action<hole, ir.Node> argument = (k, arg) => {
        if (topLevelDefer) { 
            // Top level defers arguments don't escape to
            // heap, but they do need to last until end of
            // function.
            k = e.later(k);
        }
        else if (where != null) {
            k = e.heapHole();
        }
        e.expr(k.note(call, "call parameter"), arg);
    };


    if (call.Op() == ir.OCALLFUNC || call.Op() == ir.OCALLMETH || call.Op() == ir.OCALLINTER) 
        ptr<ir.CallExpr> call = call._<ptr<ir.CallExpr>>();
        typecheck.FixVariadicCall(call); 

        // Pick out the function callee, if statically known.
        ptr<ir.Name> fn;

        if (call.Op() == ir.OCALLFUNC) 
            {
                var v = ir.StaticValue(call.X);


                if (v.Op() == ir.ONAME && v._<ptr<ir.Name>>().Class == ir.PFUNC) 
                    fn = v._<ptr<ir.Name>>();
                else if (v.Op() == ir.OCLOSURE) 
                    fn = v._<ptr<ir.ClosureExpr>>().Func.Nname;

            }
        else if (call.Op() == ir.OCALLMETH) 
            fn = ir.MethodExprName(call.X);
                var fntype = call.X.Type();
        if (fn != null) {
            fntype = fn.Type();
        }
        if (ks != null && fn != null && e.inMutualBatch(fn)) {
            {
                var i__prev1 = i;

                foreach (var (__i, __result) in fn.Type().Results().FieldSlice()) {
                    i = __i;
                    result = __result;
                    e.expr(ks[i], ir.AsNode(result.Nname));
                }

                i = i__prev1;
            }
        }
        {
            var r = fntype.Recv();

            if (r != null) {
                argument(e.tagHole(ks, fn, r), call.X._<ptr<ir.SelectorExpr>>().X);
            }
            else
 { 
                // Evaluate callee function expression.
                argument(e.discardHole(), call.X);
            }

        }

        var args = call.Args;
        {
            var i__prev1 = i;

            foreach (var (__i, __param) in fntype.Params().FieldSlice()) {
                i = __i;
                param = __param;
                argument(e.tagHole(ks, fn, param), args[i]);
            }

            i = i__prev1;
        }
    else if (call.Op() == ir.OAPPEND) 
        call = call._<ptr<ir.CallExpr>>();
        args = call.Args; 

        // Appendee slice may flow directly to the result, if
        // it has enough capacity. Alternatively, a new heap
        // slice might be allocated, and all slice elements
        // might flow to heap.
        var appendeeK = ks[0];
        if (args[0].Type().Elem().HasPointers()) {
            appendeeK = e.teeHole(appendeeK, e.heapHole().deref(call, "appendee slice"));
        }
        argument(appendeeK, args[0]);

        if (call.IsDDD) {
            var appendedK = e.discardHole();
            if (args[1].Type().IsSlice() && args[1].Type().Elem().HasPointers()) {
                appendedK = e.heapHole().deref(call, "appended slice...");
            }
            argument(appendedK, args[1]);
        }
        else
 {
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in args[(int)1..]) {
                    arg = __arg;
                    argument(e.heapHole(), arg);
                }

                arg = arg__prev1;
            }
        }
    else if (call.Op() == ir.OCOPY) 
        call = call._<ptr<ir.BinaryExpr>>();
        argument(e.discardHole(), call.X);

        var copiedK = e.discardHole();
        if (call.Y.Type().IsSlice() && call.Y.Type().Elem().HasPointers()) {
            copiedK = e.heapHole().deref(call, "copied slice");
        }
        argument(copiedK, call.Y);
    else if (call.Op() == ir.OPANIC) 
        call = call._<ptr<ir.UnaryExpr>>();
        argument(e.heapHole(), call.X);
    else if (call.Op() == ir.OCOMPLEX) 
        call = call._<ptr<ir.BinaryExpr>>();
        argument(e.discardHole(), call.X);
        argument(e.discardHole(), call.Y);
    else if (call.Op() == ir.ODELETE || call.Op() == ir.OPRINT || call.Op() == ir.OPRINTN || call.Op() == ir.ORECOVER) 
        call = call._<ptr<ir.CallExpr>>();
        {
            var arg__prev1 = arg;

            foreach (var (_, __arg) in call.Args) {
                arg = __arg;
                argument(e.discardHole(), arg);
            }

            arg = arg__prev1;
        }
    else if (call.Op() == ir.OLEN || call.Op() == ir.OCAP || call.Op() == ir.OREAL || call.Op() == ir.OIMAG || call.Op() == ir.OCLOSE) 
        call = call._<ptr<ir.UnaryExpr>>();
        argument(e.discardHole(), call.X);
    else if (call.Op() == ir.OUNSAFEADD || call.Op() == ir.OUNSAFESLICE) 
        call = call._<ptr<ir.BinaryExpr>>();
        argument(ks[0], call.X);
        argument(e.discardHole(), call.Y);
    else 
        ir.Dump("esc", call);
        @base.Fatalf("unexpected call op: %v", call.Op());
    }

// tagHole returns a hole for evaluating an argument passed to param.
// ks should contain the holes representing where the function
// callee's results flows. fn is the statically-known callee function,
// if any.
private static hole tagHole(this ptr<escape> _addr_e, slice<hole> ks, ptr<ir.Name> _addr_fn, ptr<types.Field> _addr_param) {
    ref escape e = ref _addr_e.val;
    ref ir.Name fn = ref _addr_fn.val;
    ref types.Field param = ref _addr_param.val;
 
    // If this is a dynamic call, we can't rely on param.Note.
    if (fn == null) {
        return e.heapHole();
    }
    if (e.inMutualBatch(fn)) {
        return e.addr(ir.AsNode(param.Nname));
    }
    if (param.Note == UintptrEscapesNote) {
        var k = e.heapHole();
        k.uintptrEscapesHack = true;
        return k;
    }
    slice<hole> tagKs = default;

    var esc = parseLeaks(param.Note);
    {
        var x__prev1 = x;

        var x = esc.Heap();

        if (x >= 0) {
            tagKs = append(tagKs, e.heapHole().shift(x));
        }
        x = x__prev1;

    }

    if (ks != null) {
        for (nint i = 0; i < numEscResults; i++) {
            {
                var x__prev2 = x;

                x = esc.Result(i);

                if (x >= 0) {
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
private static bool inMutualBatch(this ptr<escape> _addr_e, ptr<ir.Name> _addr_fn) {
    ref escape e = ref _addr_e.val;
    ref ir.Name fn = ref _addr_fn.val;

    if (fn.Defn != null && fn.Defn.Esc() < escFuncTagged) {
        if (fn.Defn.Esc() == escFuncUnknown) {
            @base.Fatalf("graph inconsistency: %v", fn);
        }
        return true;
    }
    return false;
}

// An hole represents a context for evaluation a Go
// expression. E.g., when evaluating p in "x = **p", we'd have a hole
// with dst==x and derefs==2.
private partial struct hole {
    public ptr<location> dst;
    public nint derefs; // >= -1
    public ptr<note> notes; // addrtaken indicates whether this context is taking the address of
// the expression, independent of whether the address will actually
// be stored into a variable.
    public bool addrtaken; // uintptrEscapesHack indicates this context is evaluating an
// argument for a //go:uintptrescapes function.
    public bool uintptrEscapesHack;
}

private partial struct note {
    public ptr<note> next;
    public ir.Node where;
    public @string why;
}

private static hole note(this hole k, ir.Node where, @string why) {
    if (where == null || why == "") {
        @base.Fatalf("note: missing where/why");
    }
    if (@base.Flag.LowerM >= 2 || logopt.Enabled()) {
        k.notes = addr(new note(next:k.notes,where:where,why:why,));
    }
    return k;
}

private static hole shift(this hole k, nint delta) {
    k.derefs += delta;
    if (k.derefs < -1) {
        @base.Fatalf("derefs underflow: %v", k.derefs);
    }
    k.addrtaken = delta < 0;
    return k;
}

private static hole deref(this hole k, ir.Node where, @string why) {
    return k.shift(1).note(where, why);
}
private static hole addr(this hole k, ir.Node where, @string why) {
    return k.shift(-1).note(where, why);
}

private static hole dotType(this hole k, ptr<types.Type> _addr_t, ir.Node where, @string why) {
    ref types.Type t = ref _addr_t.val;

    if (!t.IsInterface() && !types.IsDirectIface(t)) {
        k = k.shift(1);
    }
    return k.note(where, why);
}

// teeHole returns a new hole that flows into each hole of ks,
// similar to the Unix tee(1) command.
private static hole teeHole(this ptr<escape> _addr_e, params hole[] ks) {
    ks = ks.Clone();
    ref escape e = ref _addr_e.val;

    if (len(ks) == 0) {
        return e.discardHole();
    }
    if (len(ks) == 1) {
        return ks[0];
    }
    var loc = e.newLoc(null, true);
    foreach (var (_, k) in ks) { 
        // N.B., "p = &q" and "p = &tmp; tmp = q" are not
        // semantically equivalent. To combine holes like "l1
        // = _" and "l2 = &_", we'd need to wire them as "l1 =
        // *ltmp" and "l2 = ltmp" and return "ltmp = &_"
        // instead.
        if (k.derefs < 0) {
            @base.Fatalf("teeHole: negative derefs");
        }
        e.flow(k, loc);
    }    return loc.asHole();
}

private static hole dcl(this ptr<escape> _addr_e, ptr<ir.Name> _addr_n) {
    ref escape e = ref _addr_e.val;
    ref ir.Name n = ref _addr_n.val;

    if (n.Curfn != e.curfn || n.IsClosureVar()) {
        @base.Fatalf("bad declaration of %v", n);
    }
    var loc = e.oldLoc(n);
    loc.loopDepth = e.loopDepth;
    return loc.asHole();
}

// spill allocates a new location associated with expression n, flows
// its address to k, and returns a hole that flows values to it. It's
// intended for use with most expressions that allocate storage.
private static hole spill(this ptr<escape> _addr_e, hole k, ir.Node n) {
    ref escape e = ref _addr_e.val;

    var loc = e.newLoc(n, true);
    e.flow(k.addr(n, "spill"), loc);
    return loc.asHole();
}

// later returns a new hole that flows into k, but some time later.
// Its main effect is to prevent immediate reuse of temporary
// variables introduced during Order.
private static hole later(this ptr<escape> _addr_e, hole k) {
    ref escape e = ref _addr_e.val;

    var loc = e.newLoc(null, false);
    e.flow(k, loc);
    return loc.asHole();
}

private static ptr<location> newLoc(this ptr<escape> _addr_e, ir.Node n, bool transient) {
    ref escape e = ref _addr_e.val;

    if (e.curfn == null) {
        @base.Fatalf("e.curfn isn't set");
    }
    if (n != null && n.Type() != null && n.Type().NotInHeap()) {
        @base.ErrorfAt(n.Pos(), "%v is incomplete (or unallocatable); stack allocation disallowed", n.Type());
    }
    if (n != null && n.Op() == ir.ONAME) {
        n = n._<ptr<ir.Name>>().Canonical();
    }
    ptr<location> loc = addr(new location(n:n,curfn:e.curfn,loopDepth:e.loopDepth,transient:transient,));
    e.allLocs = append(e.allLocs, loc);
    if (n != null) {
        if (n.Op() == ir.ONAME) {
            ptr<ir.Name> n = n._<ptr<ir.Name>>();
            if (n.Curfn != e.curfn) {
                @base.Fatalf("curfn mismatch: %v != %v for %v", n.Curfn, e.curfn, n);
            }
            if (n.Opt != null) {
                @base.Fatalf("%v already has a location", n);
            }
            n.Opt = loc;
        }
    }
    return _addr_loc!;
}

private static ptr<location> oldLoc(this ptr<batch> _addr_b, ptr<ir.Name> _addr_n) {
    ref batch b = ref _addr_b.val;
    ref ir.Name n = ref _addr_n.val;

    if (n.Canonical().Opt == null) {
        @base.Fatalf("%v has no location", n);
    }
    return n.Canonical().Opt._<ptr<location>>();
}

private static hole asHole(this ptr<location> _addr_l) {
    ref location l = ref _addr_l.val;

    return new hole(dst:l);
}

private static void flow(this ptr<batch> _addr_b, hole k, ptr<location> _addr_src) {
    ref batch b = ref _addr_b.val;
    ref location src = ref _addr_src.val;

    if (k.addrtaken) {
        src.addrtaken = true;
    }
    var dst = k.dst;
    if (dst == _addr_b.blankLoc) {
        return ;
    }
    if (dst == src && k.derefs >= 0) { // dst = dst, dst = *dst, ...
        return ;
    }
    if (dst.escapes && k.derefs < 0) { // dst = &src
        if (@base.Flag.LowerM >= 2 || logopt.Enabled()) {
            var pos = @base.FmtPos(src.n.Pos());
            if (@base.Flag.LowerM >= 2) {
                fmt.Printf("%s: %v escapes to heap:\n", pos, src.n);
            }
            var explanation = b.explainFlow(pos, dst, src, k.derefs, k.notes, new slice<ptr<logopt.LoggedOpt>>(new ptr<logopt.LoggedOpt>[] {  }));
            if (logopt.Enabled()) {
                ptr<ir.Func> e_curfn; // TODO(mdempsky): Fix.
                logopt.LogOpt(src.n.Pos(), "escapes", "escape", ir.FuncName(e_curfn), fmt.Sprintf("%v escapes to heap", src.n), explanation);
            }
        }
        src.escapes = true;
        return ;
    }
    dst.edges = append(dst.edges, new edge(src:src,derefs:k.derefs,notes:k.notes));
}

private static hole heapHole(this ptr<batch> _addr_b) {
    ref batch b = ref _addr_b.val;

    return b.heapLoc.asHole();
}
private static hole discardHole(this ptr<batch> _addr_b) {
    ref batch b = ref _addr_b.val;

    return b.blankLoc.asHole();
}

// walkAll computes the minimal dereferences between all pairs of
// locations.
private static void walkAll(this ptr<batch> _addr_b) {
    ref batch b = ref _addr_b.val;
 
    // We use a work queue to keep track of locations that we need
    // to visit, and repeatedly walk until we reach a fixed point.
    //
    // We walk once from each location (including the heap), and
    // then re-enqueue each location on its transition from
    // transient->!transient and !escapes->escapes, which can each
    // happen at most once. So we take Θ(len(e.allLocs)) walks.

    // LIFO queue, has enough room for e.allLocs and e.heapLoc.
    var todo = make_slice<ptr<location>>(0, len(b.allLocs) + 1);
    Action<ptr<location>> enqueue = loc => {
        if (!loc.queued) {
            todo = append(todo, loc);
            loc.queued = true;
        }
    };

    foreach (var (_, loc) in b.allLocs) {
        enqueue(loc);
    }    enqueue(_addr_b.heapLoc);

    uint walkgen = default;
    while (len(todo) > 0) {
        var root = todo[len(todo) - 1];
        todo = todo[..(int)len(todo) - 1];
        root.queued = false;

        walkgen++;
        b.walkOne(root, walkgen, enqueue);
    }
}

// walkOne computes the minimal number of dereferences from root to
// all other locations.
private static void walkOne(this ptr<batch> _addr_b, ptr<location> _addr_root, uint walkgen, Action<ptr<location>> enqueue) {
    ref batch b = ref _addr_b.val;
    ref location root = ref _addr_root.val;
 
    // The data flow graph has negative edges (from addressing
    // operations), so we use the Bellman-Ford algorithm. However,
    // we don't have to worry about infinite negative cycles since
    // we bound intermediate dereference counts to 0.

    root.walkgen = walkgen;
    root.derefs = 0;
    root.dst = null;

    ptr<location> todo = new slice<ptr<location>>(new ptr<location>[] { root }); // LIFO queue
    while (len(todo) > 0) {
        var l = todo[len(todo) - 1];
        todo = todo[..(int)len(todo) - 1];

        var derefs = l.derefs; 

        // If l.derefs < 0, then l's address flows to root.
        var addressOf = derefs < 0;
        if (addressOf) { 
            // For a flow path like "root = &l; l = x",
            // l's address flows to root, but x's does
            // not. We recognize this by lower bounding
            // derefs at 0.
            derefs = 0; 

            // If l's address flows to a non-transient
            // location, then l can't be transiently
            // allocated.
            if (!root.transient && l.transient) {
                l.transient = false;
                enqueue(l);
            }
        }
        if (b.outlives(root, l)) { 
            // l's value flows to root. If l is a function
            // parameter and root is the heap or a
            // corresponding result parameter, then record
            // that value flow for tagging the function
            // later.
            if (l.isName(ir.PPARAM)) {
                if ((logopt.Enabled() || @base.Flag.LowerM >= 2) && !l.escapes) {
                    if (@base.Flag.LowerM >= 2) {
                        fmt.Printf("%s: parameter %v leaks to %s with derefs=%d:\n", @base.FmtPos(l.n.Pos()), l.n, b.explainLoc(root), derefs);
                    }
                    var explanation = b.explainPath(root, l);
                    if (logopt.Enabled()) {
                        ptr<ir.Func> e_curfn; // TODO(mdempsky): Fix.
                        logopt.LogOpt(l.n.Pos(), "leak", "escape", ir.FuncName(e_curfn), fmt.Sprintf("parameter %v leaks to %s with derefs=%d", l.n, b.explainLoc(root), derefs), explanation);
                    }
                }
                l.leakTo(root, derefs);
            } 

            // If l's address flows somewhere that
            // outlives it, then l needs to be heap
            // allocated.
            if (addressOf && !l.escapes) {
                if (logopt.Enabled() || @base.Flag.LowerM >= 2) {
                    if (@base.Flag.LowerM >= 2) {
                        fmt.Printf("%s: %v escapes to heap:\n", @base.FmtPos(l.n.Pos()), l.n);
                    }
                    explanation = b.explainPath(root, l);
                    if (logopt.Enabled()) {
                        e_curfn = ; // TODO(mdempsky): Fix.
                        logopt.LogOpt(l.n.Pos(), "escape", "escape", ir.FuncName(e_curfn), fmt.Sprintf("%v escapes to heap", l.n), explanation);
                    }
                }
                l.escapes = true;
                enqueue(l);
                continue;
            }
        }
        foreach (var (i, edge) in l.edges) {
            if (edge.src.escapes) {
                continue;
            }
            var d = derefs + edge.derefs;
            if (edge.src.walkgen != walkgen || edge.src.derefs > d) {
                edge.src.walkgen = walkgen;
                edge.src.derefs = d;
                edge.src.dst = l;
                edge.src.dstEdgeIdx = i;
                todo = append(todo, edge.src);
            }
        }
    }
}

// explainPath prints an explanation of how src flows to the walk root.
private static slice<ptr<logopt.LoggedOpt>> explainPath(this ptr<batch> _addr_b, ptr<location> _addr_root, ptr<location> _addr_src) {
    ref batch b = ref _addr_b.val;
    ref location root = ref _addr_root.val;
    ref location src = ref _addr_src.val;

    var visited = make_map<ptr<location>, bool>();
    var pos = @base.FmtPos(src.n.Pos());
    slice<ptr<logopt.LoggedOpt>> explanation = default;
    while (true) { 
        // Prevent infinite loop.
        if (visited[src]) {
            if (@base.Flag.LowerM >= 2) {
                fmt.Printf("%s:   warning: truncated explanation due to assignment cycle; see golang.org/issue/35518\n", pos);
            }
            break;
        }
        visited[src] = true;
        var dst = src.dst;
        var edge = _addr_dst.edges[src.dstEdgeIdx];
        if (edge.src != src) {
            @base.Fatalf("path inconsistency: %v != %v", edge.src, src);
        }
        explanation = b.explainFlow(pos, dst, src, edge.derefs, edge.notes, explanation);

        if (dst == root) {
            break;
        }
        src = dst;
    }

    return explanation;
}

private static slice<ptr<logopt.LoggedOpt>> explainFlow(this ptr<batch> _addr_b, @string pos, ptr<location> _addr_dst, ptr<location> _addr_srcloc, nint derefs, ptr<note> _addr_notes, slice<ptr<logopt.LoggedOpt>> explanation) {
    ref batch b = ref _addr_b.val;
    ref location dst = ref _addr_dst.val;
    ref location srcloc = ref _addr_srcloc.val;
    ref note notes = ref _addr_notes.val;

    @string ops = "&";
    if (derefs >= 0) {
        ops = strings.Repeat("*", derefs);
    }
    var print = @base.Flag.LowerM >= 2;

    var flow = fmt.Sprintf("   flow: %s = %s%v:", b.explainLoc(dst), ops, b.explainLoc(srcloc));
    if (print) {
        fmt.Printf("%s:%s\n", pos, flow);
    }
    if (logopt.Enabled()) {
        src.XPos epos = default;
        if (notes != null) {
            epos = notes.where.Pos();
        }
        else if (srcloc != null && srcloc.n != null) {
            epos = srcloc.n.Pos();
        }
        ptr<ir.Func> e_curfn; // TODO(mdempsky): Fix.
        explanation = append(explanation, logopt.NewLoggedOpt(epos, "escflow", "escape", ir.FuncName(e_curfn), flow));
    }
    {
        var note = notes;

        while (note != null) {
            if (print) {
                fmt.Printf("%s:     from %v (%v) at %s\n", pos, note.where, note.why, @base.FmtPos(note.where.Pos()));
            note = note.next;
            }
            if (logopt.Enabled()) {
                e_curfn = ; // TODO(mdempsky): Fix.
                explanation = append(explanation, logopt.NewLoggedOpt(note.where.Pos(), "escflow", "escape", ir.FuncName(e_curfn), fmt.Sprintf("     from %v (%v)", note.where, note.why)));
            }
        }
    }
    return explanation;
}

private static @string explainLoc(this ptr<batch> _addr_b, ptr<location> _addr_l) {
    ref batch b = ref _addr_b.val;
    ref location l = ref _addr_l.val;

    if (l == _addr_b.heapLoc) {
        return "{heap}";
    }
    if (l.n == null) { 
        // TODO(mdempsky): Omit entirely.
        return "{temp}";
    }
    if (l.n.Op() == ir.ONAME) {
        return fmt.Sprintf("%v", l.n);
    }
    return fmt.Sprintf("{storage for %v}", l.n);
}

// outlives reports whether values stored in l may survive beyond
// other's lifetime if stack allocated.
private static bool outlives(this ptr<batch> _addr_b, ptr<location> _addr_l, ptr<location> _addr_other) {
    ref batch b = ref _addr_b.val;
    ref location l = ref _addr_l.val;
    ref location other = ref _addr_other.val;
 
    // The heap outlives everything.
    if (l.escapes) {
        return true;
    }
    if (l.isName(ir.PPARAMOUT)) { 
        // Exception: Directly called closures can return
        // locations allocated outside of them without forcing
        // them to the heap. For example:
        //
        //    var u int  // okay to stack allocate
        //    *(func() *int { return &u }()) = 42
        if (containsClosure(_addr_other.curfn, _addr_l.curfn) && l.curfn.ClosureCalled()) {
            return false;
        }
        return true;
    }
    if (l.curfn == other.curfn && l.loopDepth < other.loopDepth) {
        return true;
    }
    if (containsClosure(_addr_l.curfn, _addr_other.curfn)) {
        return true;
    }
    return false;
}

// containsClosure reports whether c is a closure contained within f.
private static bool containsClosure(ptr<ir.Func> _addr_f, ptr<ir.Func> _addr_c) {
    ref ir.Func f = ref _addr_f.val;
    ref ir.Func c = ref _addr_c.val;
 
    // Common case.
    if (f == c) {
        return false;
    }
    var fn = f.Sym().Name;
    var cn = c.Sym().Name;
    return len(cn) > len(fn) && cn[..(int)len(fn)] == fn && cn[len(fn)] == '.';
}

// leak records that parameter l leaks to sink.
private static void leakTo(this ptr<location> _addr_l, ptr<location> _addr_sink, nint derefs) {
    ref location l = ref _addr_l.val;
    ref location sink = ref _addr_sink.val;
 
    // If sink is a result parameter that doesn't escape (#44614)
    // and we can fit return bits into the escape analysis tag,
    // then record as a result leak.
    if (!sink.escapes && sink.isName(ir.PPARAMOUT) && sink.curfn == l.curfn) {
        var ri = sink.resultIndex - 1;
        if (ri < numEscResults) { 
            // Leak to result parameter.
            l.paramEsc.AddResult(ri, derefs);
            return ;
        }
    }
    l.paramEsc.AddHeap(derefs);
}

private static void finish(this ptr<batch> _addr_b, slice<ptr<ir.Func>> fns) {
    ref batch b = ref _addr_b.val;
 
    // Record parameter tags for package export data.
    foreach (var (_, fn) in fns) {
        fn.SetEsc(escFuncTagged);

        nint narg = 0;
        foreach (var (_, fs) in _addr_types.RecvsParams) {
            foreach (var (_, f) in fs(fn.Type()).Fields().Slice()) {
                narg++;
                f.Note = b.paramTag(fn, narg, f);
            }
        }
    }    foreach (var (_, loc) in b.allLocs) {
        var n = loc.n;
        if (n == null) {
            continue;
        }
        if (n.Op() == ir.ONAME) {
            n = n._<ptr<ir.Name>>();
            n.Opt = null;
        }
        if (loc.escapes) {
            if (n.Op() == ir.ONAME) {
                if (@base.Flag.CompilingRuntime) {
                    @base.ErrorfAt(n.Pos(), "%v escapes to heap, not allowed in runtime", n);
                }
                if (@base.Flag.LowerM != 0) {
                    @base.WarnfAt(n.Pos(), "moved to heap: %v", n);
                }
            }
            else
 {
                if (@base.Flag.LowerM != 0) {
                    @base.WarnfAt(n.Pos(), "%v escapes to heap", n);
                }
                if (logopt.Enabled()) {
                    ptr<ir.Func> e_curfn; // TODO(mdempsky): Fix.
                    logopt.LogOpt(n.Pos(), "escape", "escape", ir.FuncName(e_curfn));
                }
            }
            n.SetEsc(ir.EscHeap);
        }
        else
 {
            if (@base.Flag.LowerM != 0 && n.Op() != ir.ONAME) {
                @base.WarnfAt(n.Pos(), "%v does not escape", n);
            }
            n.SetEsc(ir.EscNone);
            if (loc.transient) {

                if (n.Op() == ir.OCLOSURE) 
                    n = n._<ptr<ir.ClosureExpr>>();
                    n.SetTransient(true);
                else if (n.Op() == ir.OCALLPART) 
                    n = n._<ptr<ir.SelectorExpr>>();
                    n.SetTransient(true);
                else if (n.Op() == ir.OSLICELIT) 
                    n = n._<ptr<ir.CompLitExpr>>();
                    n.SetTransient(true);
                            }
        }
    }
}

private static bool isName(this ptr<location> _addr_l, ir.Class c) {
    ref location l = ref _addr_l.val;

    return l.n != null && l.n.Op() == ir.ONAME && l.n._<ptr<ir.Name>>().Class == c;
}

private static readonly nint numEscResults = 7;

// An leaks represents a set of assignment flows from a parameter
// to the heap or to any of its function's (first numEscResults)
// result parameters.


// An leaks represents a set of assignment flows from a parameter
// to the heap or to any of its function's (first numEscResults)
// result parameters.
private partial struct leaks { // : array<byte>
}

// Empty reports whether l is an empty set (i.e., no assignment flows).
private static bool Empty(this leaks l) {
    return l == new leaks();
}

// Heap returns the minimum deref count of any assignment flow from l
// to the heap. If no such flows exist, Heap returns -1.
private static nint Heap(this leaks l) {
    return l.get(0);
}

// Result returns the minimum deref count of any assignment flow from
// l to its function's i'th result parameter. If no such flows exist,
// Result returns -1.
private static nint Result(this leaks l, nint i) {
    return l.get(1 + i);
}

// AddHeap adds an assignment flow from l to the heap.
private static void AddHeap(this ptr<leaks> _addr_l, nint derefs) {
    ref leaks l = ref _addr_l.val;

    l.add(0, derefs);
}

// AddResult adds an assignment flow from l to its function's i'th
// result parameter.
private static void AddResult(this ptr<leaks> _addr_l, nint i, nint derefs) {
    ref leaks l = ref _addr_l.val;

    l.add(1 + i, derefs);
}

private static void setResult(this ptr<leaks> _addr_l, nint i, nint derefs) {
    ref leaks l = ref _addr_l.val;

    l.set(1 + i, derefs);
}

private static nint get(this leaks l, nint i) {
    return int(l[i]) - 1;
}

private static void add(this ptr<leaks> _addr_l, nint i, nint derefs) {
    ref leaks l = ref _addr_l.val;

    {
        var old = l.get(i);

        if (old < 0 || derefs < old) {
            l.set(i, derefs);
        }
    }
}

private static void set(this ptr<leaks> _addr_l, nint i, nint derefs) {
    ref leaks l = ref _addr_l.val;

    var v = derefs + 1;
    if (v < 0) {
        @base.Fatalf("invalid derefs count: %v", derefs);
    }
    if (v > math.MaxUint8) {
        v = math.MaxUint8;
    }
    l[i] = uint8(v);
}

// Optimize removes result flow paths that are equal in length or
// longer than the shortest heap flow path.
private static void Optimize(this ptr<leaks> _addr_l) {
    ref leaks l = ref _addr_l.val;
 
    // If we have a path to the heap, then there's no use in
    // keeping equal or longer paths elsewhere.
    {
        var x = l.Heap();

        if (x >= 0) {
            for (nint i = 0; i < numEscResults; i++) {
                if (l.Result(i) >= x) {
                    l.setResult(i, -1);
                }
            }
        }
    }
}

private static map leakTagCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<leaks, @string>{};

// Encode converts l into a binary string for export data.
private static @string Encode(this leaks l) {
    if (l.Heap() == 0) { 
        // Space optimization: empty string encodes more
        // efficiently in export data.
        return "";
    }
    {
        var s__prev1 = s;

        var (s, ok) = leakTagCache[l];

        if (ok) {
            return s;
        }
        s = s__prev1;

    }

    var n = len(l);
    while (n > 0 && l[n - 1] == 0) {
        n--;
    }
    @string s = "esc:" + string(l[..(int)n]);
    leakTagCache[l] = s;
    return s;
}

// parseLeaks parses a binary string representing a leaks
private static leaks parseLeaks(@string s) {
    leaks l = default;
    if (!strings.HasPrefix(s, "esc:")) {
        l.AddHeap(0);
        return l;
    }
    copy(l[..], s[(int)4..]);
    return l;
}

public static void Funcs(slice<ir.Node> all) {
    ir.VisitFuncsBottomUp(all, Batch);
}

private static readonly nint escFuncUnknown = 0 + iota;
private static readonly var escFuncPlanned = 0;
private static readonly var escFuncStarted = 1;
private static readonly var escFuncTagged = 2;

// Mark labels that have no backjumps to them as not increasing e.loopdepth.
private partial struct labelState { // : nint
}

private static readonly labelState looping = 1 + iota;
private static readonly var nonlooping = 0;

private static bool isSliceSelfAssign(ir.Node dst, ir.Node src) { 
    // Detect the following special case.
    //
    //    func (b *Buffer) Foo() {
    //        n, m := ...
    //        b.buf = b.buf[n:m]
    //    }
    //
    // This assignment is a no-op for escape analysis,
    // it does not store any new pointers into b that were not already there.
    // However, without this special case b will escape, because we assign to OIND/ODOTPTR.
    // Here we assume that the statement will not contain calls,
    // that is, that order will move any calls to init.
    // Otherwise base ONAME value could change between the moments
    // when we evaluate it for dst and for src.

    // dst is ONAME dereference.
    ir.Node dstX = default;

    if (dst.Op() == ir.ODEREF) 
        ptr<ir.StarExpr> dst = dst._<ptr<ir.StarExpr>>();
        dstX = dst.X;
    else if (dst.Op() == ir.ODOTPTR) 
        dst = dst._<ptr<ir.SelectorExpr>>();
        dstX = dst.X;
    else 
        return false;
        if (dstX.Op() != ir.ONAME) {
        return false;
    }

    if (src.Op() == ir.OSLICE || src.Op() == ir.OSLICE3 || src.Op() == ir.OSLICESTR)     else if (src.Op() == ir.OSLICEARR || src.Op() == ir.OSLICE3ARR) 
        // Since arrays are embedded into containing object,
        // slice of non-pointer array will introduce a new pointer into b that was not already there
        // (pointer to b itself). After such assignment, if b contents escape,
        // b escapes as well. If we ignore such OSLICEARR, we will conclude
        // that b does not escape when b contents do.
        //
        // Pointer to an array is OK since it's not stored inside b directly.
        // For slicing an array (not pointer to array), there is an implicit OADDR.
        // We check that to determine non-pointer array slicing.
        ptr<ir.SliceExpr> src = src._<ptr<ir.SliceExpr>>();
        if (src.X.Op() == ir.OADDR) {
            return false;
        }
    else 
        return false;
    // slice is applied to ONAME dereference.
    ir.Node baseX = default;
    {
        ptr<ir.SliceExpr> base__prev1 = base;

        ptr<ir.SliceExpr> @base = src._<ptr<ir.SliceExpr>>().X;


        if (@base.Op() == ir.ODEREF) 
            @base = base._<ptr<ir.StarExpr>>();
            baseX = @base.X;
        else if (@base.Op() == ir.ODOTPTR) 
            @base = base._<ptr<ir.SelectorExpr>>();
            baseX = @base.X;
        else 
            return false;


        base = base__prev1;
    }
    if (baseX.Op() != ir.ONAME) {
        return false;
    }
    return dstX._<ptr<ir.Name>>() == baseX._<ptr<ir.Name>>();
}

// isSelfAssign reports whether assignment from src to dst can
// be ignored by the escape analysis as it's effectively a self-assignment.
private static bool isSelfAssign(ir.Node dst, ir.Node src) {
    if (isSliceSelfAssign(dst, src)) {
        return true;
    }
    if (dst == null || src == null || dst.Op() != src.Op()) {
        return false;
    }

    if (dst.Op() == ir.ODOT || dst.Op() == ir.ODOTPTR) 
        // Safe trailing accessors that are permitted to differ.
        ptr<ir.SelectorExpr> dst = dst._<ptr<ir.SelectorExpr>>();
        ptr<ir.SelectorExpr> src = src._<ptr<ir.SelectorExpr>>();
        return ir.SameSafeExpr(dst.X, src.X);
    else if (dst.Op() == ir.OINDEX) 
        dst = dst._<ptr<ir.IndexExpr>>();
        src = src._<ptr<ir.IndexExpr>>();
        if (mayAffectMemory(dst.Index) || mayAffectMemory(src.Index)) {
            return false;
        }
        return ir.SameSafeExpr(dst.X, src.X);
    else 
        return false;
    }

// mayAffectMemory reports whether evaluation of n may affect the program's
// memory state. If the expression can't affect memory state, then it can be
// safely ignored by the escape analysis.
private static bool mayAffectMemory(ir.Node n) { 
    // We may want to use a list of "memory safe" ops instead of generally
    // "side-effect free", which would include all calls and other ops that can
    // allocate or change global state. For now, it's safer to start with the latter.
    //
    // We're ignoring things like division by zero, index out of range,
    // and nil pointer dereference here.

    // TODO(rsc): It seems like it should be possible to replace this with
    // an ir.Any looking for any op that's not the ones in the case statement.
    // But that produces changes in the compiled output detected by buildall.

    if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL) 
        return false;
    else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OMUL || n.Op() == ir.OLSH || n.Op() == ir.ORSH || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.ODIV || n.Op() == ir.OMOD) 
        ptr<ir.BinaryExpr> n = n._<ptr<ir.BinaryExpr>>();
        return mayAffectMemory(n.X) || mayAffectMemory(n.Y);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        return mayAffectMemory(n.X) || mayAffectMemory(n.Index);
    else if (n.Op() == ir.OCONVNOP || n.Op() == ir.OCONV) 
        n = n._<ptr<ir.ConvExpr>>();
        return mayAffectMemory(n.X);
    else if (n.Op() == ir.OLEN || n.Op() == ir.OCAP || n.Op() == ir.ONOT || n.Op() == ir.OBITNOT || n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OALIGNOF || n.Op() == ir.OOFFSETOF || n.Op() == ir.OSIZEOF) 
        n = n._<ptr<ir.UnaryExpr>>();
        return mayAffectMemory(n.X);
    else if (n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        return mayAffectMemory(n.X);
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        return mayAffectMemory(n.X);
    else 
        return true;
    }

// HeapAllocReason returns the reason the given Node must be heap
// allocated, or the empty string if it doesn't.
public static @string HeapAllocReason(ir.Node n) {
    if (n == null || n.Type() == null) {
        return "";
    }
    if (n.Op() == ir.ONAME) {
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) {
            return "";
        }
    }
    if (n.Type().Width > ir.MaxStackVarSize) {
        return "too large for stack";
    }
    if ((n.Op() == ir.ONEW || n.Op() == ir.OPTRLIT) && n.Type().Elem().Width > ir.MaxImplicitStackVarSize) {
        return "too large for stack";
    }
    if (n.Op() == ir.OCLOSURE && typecheck.ClosureType(n._<ptr<ir.ClosureExpr>>()).Size() > ir.MaxImplicitStackVarSize) {
        return "too large for stack";
    }
    if (n.Op() == ir.OCALLPART && typecheck.PartialCallType(n._<ptr<ir.SelectorExpr>>()).Size() > ir.MaxImplicitStackVarSize) {
        return "too large for stack";
    }
    if (n.Op() == ir.OMAKESLICE) {
        n = n._<ptr<ir.MakeExpr>>();
        var r = n.Cap;
        if (r == null) {
            r = n.Len;
        }
        if (!ir.IsSmallIntConst(r)) {
            return "non-constant size";
        }
        {
            var t = n.Type();

            if (t.Elem().Width != 0 && ir.Int64Val(r) > ir.MaxImplicitStackVarSize / t.Elem().Width) {
                return "too large for stack";
            }

        }
    }
    return "";
}

// This special tag is applied to uintptr variables
// that we believe may hold unsafe.Pointers for
// calls into assembly functions.
public static readonly @string UnsafeUintptrNote = "unsafe-uintptr";

// This special tag is applied to uintptr parameters of functions
// marked go:uintptrescapes.


// This special tag is applied to uintptr parameters of functions
// marked go:uintptrescapes.
public static readonly @string UintptrEscapesNote = "uintptr-escapes";



private static @string paramTag(this ptr<batch> _addr_b, ptr<ir.Func> _addr_fn, nint narg, ptr<types.Field> _addr_f) {
    ref batch b = ref _addr_b.val;
    ref ir.Func fn = ref _addr_fn.val;
    ref types.Field f = ref _addr_f.val;

    Func<@string> name = () => {
        if (f.Sym != null) {
            return f.Sym.Name;
        }
        return fmt.Sprintf("arg#%d", narg);
    };

    if (len(fn.Body) == 0) { 
        // Assume that uintptr arguments must be held live across the call.
        // This is most important for syscall.Syscall.
        // See golang.org/issue/13372.
        // This really doesn't have much to do with escape analysis per se,
        // but we are reusing the ability to annotate an individual function
        // argument and pass those annotations along to importing code.
        if (f.Type.IsUintptr()) {
            if (@base.Flag.LowerM != 0) {
                @base.WarnfAt(f.Pos, "assuming %v is unsafe uintptr", name());
            }
            return UnsafeUintptrNote;
        }
        if (!f.Type.HasPointers()) { // don't bother tagging for scalars
            return "";
        }
        leaks esc = default; 

        // External functions are assumed unsafe, unless
        // //go:noescape is given before the declaration.
        if (fn.Pragma & ir.Noescape != 0) {
            if (@base.Flag.LowerM != 0 && f.Sym != null) {
                @base.WarnfAt(f.Pos, "%v does not escape", name());
            }
        }
        else
 {
            if (@base.Flag.LowerM != 0 && f.Sym != null) {
                @base.WarnfAt(f.Pos, "leaking param: %v", name());
            }
            esc.AddHeap(0);
        }
        return esc.Encode();
    }
    if (fn.Pragma & ir.UintptrEscapes != 0) {
        if (f.Type.IsUintptr()) {
            if (@base.Flag.LowerM != 0) {
                @base.WarnfAt(f.Pos, "marking %v as escaping uintptr", name());
            }
            return UintptrEscapesNote;
        }
        if (f.IsDDD() && f.Type.Elem().IsUintptr()) { 
            // final argument is ...uintptr.
            if (@base.Flag.LowerM != 0) {
                @base.WarnfAt(f.Pos, "marking %v as escaping ...uintptr", name());
            }
            return UintptrEscapesNote;
        }
    }
    if (!f.Type.HasPointers()) { // don't bother tagging for scalars
        return "";
    }
    if (f.Sym == null || f.Sym.IsBlank()) {
        esc = default;
        return esc.Encode();
    }
    ptr<ir.Name> n = f.Nname._<ptr<ir.Name>>();
    var loc = b.oldLoc(n);
    esc = loc.paramEsc;
    esc.Optimize();

    if (@base.Flag.LowerM != 0 && !loc.escapes) {
        if (esc.Empty()) {
            @base.WarnfAt(f.Pos, "%v does not escape", name());
        }
        {
            var x__prev2 = x;

            var x = esc.Heap();

            if (x >= 0) {
                if (x == 0) {
                    @base.WarnfAt(f.Pos, "leaking param: %v", name());
                }
                else
 { 
                    // TODO(mdempsky): Mention level=x like below?
                    @base.WarnfAt(f.Pos, "leaking param content: %v", name());
                }
            }

            x = x__prev2;

        }
        for (nint i = 0; i < numEscResults; i++) {
            {
                var x__prev2 = x;

                x = esc.Result(i);

                if (x >= 0) {
                    var res = fn.Type().Results().Field(i).Sym;
                    @base.WarnfAt(f.Pos, "leaking param: %v to result %v level=%d", name(), res, x);
                }

                x = x__prev2;

            }
        }
    }
    return esc.Encode();
}

} // end escape_package
