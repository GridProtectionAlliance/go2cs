// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// The inlining facility makes 2 passes: first caninl determines which
// functions are suitable for inlining, and for those that are it
// saves a copy of the body. Then inlcalls walks each function body to
// expand calls to inlinable functions.
//
// The debug['l'] flag controls the aggressiveness. Note that main() swaps level 0 and 1,
// making 1 the default and -l disable. Additional levels (beyond -l) may be buggy and
// are not supported.
//      0: disabled
//      1: 80-nodes leaf functions, oneliners, panic, lazy typechecking (default)
//      2: (unassigned)
//      3: (unassigned)
//      4: allow non-leaf functions
//
// At some point this may get another default and become switch-offable with -N.
//
// The -d typcheckinl flag enables early typechecking of all imported bodies,
// which is useful to flush out bugs.
//
// The debug['m'] flag enables diagnostic output.  a single -m is useful for verifying
// which calls get inlined or not, more is for debugging, and may go away at any point.

// package gc -- go2cs converted at 2020 October 08 04:29:19 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\inl.go
using logopt = go.cmd.compile.@internal.logopt_package;
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
        // Inlining budget parameters, gathered in one place
        private static readonly long inlineMaxBudget = (long)80L;
        private static readonly long inlineExtraAppendCost = (long)0L; 
        // default is to inline if there's at most one call. -l=4 overrides this by using 1 instead.
        private static readonly long inlineExtraCallCost = (long)57L; // 57 was benchmarked to provided most benefit with no bad surprises; see https://github.com/golang/go/issues/19348#issuecomment-439370742
        private static readonly long inlineExtraPanicCost = (long)1L; // do not penalize inlining panics.
        private static readonly var inlineExtraThrowCost = (var)inlineMaxBudget; // with current (2018-05/1.11) code, inlining runtime.throw does not help.

        private static readonly long inlineBigFunctionNodes = (long)5000L; // Functions with this many nodes are considered "big".
        private static readonly long inlineBigFunctionMaxCost = (long)20L; // Max cost of inlinee when inlining into a "big" function.

        // Get the function's package. For ordinary functions it's on the ->sym, but for imported methods
        // the ->sym can be re-used in the local package, so peel it off the receiver's type.
        private static ptr<types.Pkg> fnpkg(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.IsMethod())
            { 
                // method
                var rcvr = fn.Type.Recv().Type;

                if (rcvr.IsPtr())
                {
                    rcvr = rcvr.Elem();
                }

                if (rcvr.Sym == null)
                {
                    Fatalf("receiver with no sym: [%v] %L  (%v)", fn.Sym, fn, rcvr);
                }

                return _addr_rcvr.Sym.Pkg!;

            } 

            // non-method
            return _addr_fn.Sym.Pkg!;

        }

        // Lazy typechecking of imported bodies. For local functions, caninl will set ->typecheck
        // because they're a copy of an already checked body.
        private static void typecheckinl(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            var lno = setlineno(fn);

            expandInline(fn); 

            // typecheckinl is only for imported functions;
            // their bodies may refer to unsafe as long as the package
            // was marked safe during import (which was checked then).
            // the ->inl of a local function has been typechecked before caninl copied it.
            var pkg = fnpkg(_addr_fn);

            if (pkg == localpkg || pkg == null)
            {
                return ; // typecheckinl on local function
            }

            if (Debug['m'] > 2L || Debug_export != 0L)
            {
                fmt.Printf("typecheck import [%v] %L { %#v }\n", fn.Sym, fn, asNodes(fn.Func.Inl.Body));
            }

            var savefn = Curfn;
            Curfn = fn;
            typecheckslice(fn.Func.Inl.Body, ctxStmt);
            Curfn = savefn; 

            // During typechecking, declarations are added to
            // Curfn.Func.Dcl. Move them to Inl.Dcl for consistency with
            // how local functions behave. (Append because typecheckinl
            // may be called multiple times.)
            fn.Func.Inl.Dcl = append(fn.Func.Inl.Dcl, fn.Func.Dcl);
            fn.Func.Dcl = null;

            lineno = lno;

        }

        // Caninl determines whether fn is inlineable.
        // If so, caninl saves fn->nbody in fn->inl and substitutes it with a copy.
        // fn and ->nbody will already have been typechecked.
        private static void caninl(ptr<Node> _addr_fn) => func((defer, _, __) =>
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Op != ODCLFUNC)
            {
                Fatalf("caninl %v", fn);
            }

            if (fn.Func.Nname == null)
            {
                Fatalf("caninl no nname %+v", fn);
            }

            @string reason = default; // reason, if any, that the function was not inlined
            if (Debug['m'] > 1L || logopt.Enabled())
            {
                defer(() =>
                {
                    if (reason != "")
                    {
                        if (Debug['m'] > 1L)
                        {
                            fmt.Printf("%v: cannot inline %v: %s\n", fn.Line(), fn.Func.Nname, reason);
                        }

                        if (logopt.Enabled())
                        {
                            logopt.LogOpt(fn.Pos, "cannotInlineFunction", "inline", fn.funcname(), reason);
                        }

                    }

                }());

            } 

            // If marked "go:noinline", don't inline
            if (fn.Func.Pragma & Noinline != 0L)
            {
                reason = "marked go:noinline";
                return ;
            } 

            // If marked "go:norace" and -race compilation, don't inline.
            if (flag_race && fn.Func.Pragma & Norace != 0L)
            {
                reason = "marked go:norace with -race compilation";
                return ;
            } 

            // If marked "go:nocheckptr" and -d checkptr compilation, don't inline.
            if (Debug_checkptr != 0L && fn.Func.Pragma & NoCheckPtr != 0L)
            {
                reason = "marked go:nocheckptr";
                return ;
            } 

            // If marked "go:cgo_unsafe_args", don't inline, since the
            // function makes assumptions about its argument frame layout.
            if (fn.Func.Pragma & CgoUnsafeArgs != 0L)
            {
                reason = "marked go:cgo_unsafe_args";
                return ;
            } 

            // If marked as "go:uintptrescapes", don't inline, since the
            // escape information is lost during inlining.
            if (fn.Func.Pragma & UintptrEscapes != 0L)
            {
                reason = "marked as having an escaping uintptr argument";
                return ;
            } 

            // The nowritebarrierrec checker currently works at function
            // granularity, so inlining yeswritebarrierrec functions can
            // confuse it (#22342). As a workaround, disallow inlining
            // them for now.
            if (fn.Func.Pragma & Yeswritebarrierrec != 0L)
            {
                reason = "marked go:yeswritebarrierrec";
                return ;
            } 

            // If fn has no body (is defined outside of Go), cannot inline it.
            if (fn.Nbody.Len() == 0L)
            {
                reason = "no function body";
                return ;
            }

            if (fn.Typecheck() == 0L)
            {
                Fatalf("caninl on non-typechecked function %v", fn);
            }

            var n = fn.Func.Nname;
            if (n.Func.InlinabilityChecked())
            {
                return ;
            }

            defer(n.Func.SetInlinabilityChecked(true));

            var cc = int32(inlineExtraCallCost);
            if (Debug['l'] == 4L)
            {
                cc = 1L; // this appears to yield better performance than 0.
            } 

            // At this point in the game the function we're looking at may
            // have "stale" autos, vars that still appear in the Dcl list, but
            // which no longer have any uses in the function body (due to
            // elimination by deadcode). We'd like to exclude these dead vars
            // when creating the "Inline.Dcl" field below; to accomplish this,
            // the hairyVisitor below builds up a map of used/referenced
            // locals, and we use this map to produce a pruned Inline.Dcl
            // list. See issue 25249 for more context.
            ref hairyVisitor visitor = ref heap(new hairyVisitor(budget:inlineMaxBudget,extraCallCost:cc,usedLocals:make(map[*Node]bool),), out ptr<hairyVisitor> _addr_visitor);
            if (visitor.visitList(fn.Nbody))
            {
                reason = visitor.reason;
                return ;
            }

            if (visitor.budget < 0L)
            {
                reason = fmt.Sprintf("function too complex: cost %d exceeds budget %d", inlineMaxBudget - visitor.budget, inlineMaxBudget);
                return ;
            }

            n.Func.Inl = addr(new Inline(Cost:inlineMaxBudget-visitor.budget,Dcl:inlcopylist(pruneUnusedAutos(n.Name.Defn.Func.Dcl,&visitor)),Body:inlcopylist(fn.Nbody.Slice()),)); 

            // hack, TODO, check for better way to link method nodes back to the thing with the ->inl
            // this is so export can find the body of a method
            fn.Type.FuncType().Nname = asTypesNode(n);

            if (Debug['m'] > 1L)
            {
                fmt.Printf("%v: can inline %#v with cost %d as: %#v { %#v }\n", fn.Line(), n, inlineMaxBudget - visitor.budget, fn.Type, asNodes(n.Func.Inl.Body));
            }
            else if (Debug['m'] != 0L)
            {
                fmt.Printf("%v: can inline %v\n", fn.Line(), n);
            }

            if (logopt.Enabled())
            {
                logopt.LogOpt(fn.Pos, "canInlineFunction", "inline", fn.funcname(), fmt.Sprintf("cost: %d", inlineMaxBudget - visitor.budget));
            }

        });

        // inlFlood marks n's inline body for export and recursively ensures
        // all called functions are marked too.
        private static void inlFlood(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            if (n.Op != ONAME || n.Class() != PFUNC)
            {
                Fatalf("inlFlood: unexpected %v, %v, %v", n, n.Op, n.Class());
            }

            if (n.Func == null)
            {
                Fatalf("inlFlood: missing Func on %v", n);
            }

            if (n.Func.Inl == null)
            {
                return ;
            }

            if (n.Func.ExportInline())
            {
                return ;
            }

            n.Func.SetExportInline(true);

            typecheckinl(_addr_n);

            inspectList(asNodes(n.Func.Inl.Body), n =>
            {

                if (n.Op == ONAME) 
                    // Mark any referenced global variables or
                    // functions for reexport. Skip methods,
                    // because they're reexported alongside their
                    // receiver type.
                    if (n.Class() == PEXTERN || n.Class() == PFUNC && !n.isMethodExpression())
                    {
                        exportsym(n);
                    }

                else if (n.Op == OCALLFUNC || n.Op == OCALLMETH) 
                    // Recursively flood any functions called by
                    // this one.
                    inlFlood(_addr_asNode(n.Left.Type.Nname()));
                                return true;

            });

        }

        // hairyVisitor visits a function body to determine its inlining
        // hairiness and whether or not it can be inlined.
        private partial struct hairyVisitor
        {
            public int budget;
            public @string reason;
            public int extraCallCost;
            public map<ptr<Node>, bool> usedLocals;
        }

        // Look for anything we want to punt on.
        private static bool visitList(this ptr<hairyVisitor> _addr_v, Nodes ll)
        {
            ref hairyVisitor v = ref _addr_v.val;

            foreach (var (_, n) in ll.Slice())
            {
                if (v.visit(n))
                {
                    return true;
                }

            }
            return false;

        }

        private static bool visit(this ptr<hairyVisitor> _addr_v, ptr<Node> _addr_n)
        {
            ref hairyVisitor v = ref _addr_v.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return false;
            }


            // Call is okay if inlinable and we have the budget for the body.
            if (n.Op == OCALLFUNC) 
                // Functions that call runtime.getcaller{pc,sp} can not be inlined
                // because getcaller{pc,sp} expect a pointer to the caller's first argument.
                //
                // runtime.throw is a "cheap call" like panic in normal code.
                if (n.Left.Op == ONAME && n.Left.Class() == PFUNC && isRuntimePkg(n.Left.Sym.Pkg))
                {
                    var fn = n.Left.Sym.Name;
                    if (fn == "getcallerpc" || fn == "getcallersp")
                    {
                        v.reason = "call to " + fn;
                        return true;
                    }

                    if (fn == "throw")
                    {
                        v.budget -= inlineExtraThrowCost;
                        break;
                    }

                }

                if (isIntrinsicCall(n))
                { 
                    // Treat like any other node.
                    break;

                }

                {
                    var fn__prev1 = fn;

                    fn = n.Left.Func;

                    if (fn != null && fn.Inl != null)
                    {
                        v.budget -= fn.Inl.Cost;
                        break;
                    }

                    fn = fn__prev1;

                }

                if (n.Left.isMethodExpression())
                {
                    {
                        var d = asNode(n.Left.Sym.Def);

                        if (d != null && d.Func.Inl != null)
                        {
                            v.budget -= d.Func.Inl.Cost;
                            break;
                        }

                    }

                } 
                // TODO(mdempsky): Budget for OCLOSURE calls if we
                // ever allow that. See #15561 and #23093.

                // Call cost for non-leaf inlining.
                v.budget -= v.extraCallCost; 

                // Call is okay if inlinable and we have the budget for the body.
            else if (n.Op == OCALLMETH) 
                var t = n.Left.Type;
                if (t == null)
                {
                    Fatalf("no function type for [%p] %+v\n", n.Left, n.Left);
                }

                if (t.Nname() == null)
                {
                    Fatalf("no function definition for [%p] %+v\n", t, t);
                }

                if (isRuntimePkg(n.Left.Sym.Pkg))
                {
                    fn = n.Left.Sym.Name;
                    if (fn == "heapBits.nextArena")
                    { 
                        // Special case: explicitly allow
                        // mid-stack inlining of
                        // runtime.heapBits.next even though
                        // it calls slow-path
                        // runtime.heapBits.nextArena.
                        break;

                    }

                }

                {
                    var inlfn = asNode(t.FuncType().Nname).Func;

                    if (inlfn.Inl != null)
                    {
                        v.budget -= inlfn.Inl.Cost;
                        break;
                    } 
                    // Call cost for non-leaf inlining.

                } 
                // Call cost for non-leaf inlining.
                v.budget -= v.extraCallCost; 

                // Things that are too hairy, irrespective of the budget
            else if (n.Op == OCALL || n.Op == OCALLINTER) 
                // Call cost for non-leaf inlining.
                v.budget -= v.extraCallCost;
            else if (n.Op == OPANIC) 
                v.budget -= inlineExtraPanicCost;
            else if (n.Op == ORECOVER) 
                // recover matches the argument frame pointer to find
                // the right panic value, so it needs an argument frame.
                v.reason = "call to recover";
                return true;
            else if (n.Op == OCLOSURE || n.Op == OCALLPART || n.Op == ORANGE || n.Op == OFOR || n.Op == OFORUNTIL || n.Op == OSELECT || n.Op == OTYPESW || n.Op == OGO || n.Op == ODEFER || n.Op == ODCLTYPE || n.Op == OBREAK || n.Op == ORETJMP) 
                v.reason = "unhandled op " + n.Op.String();
                return true;
            else if (n.Op == OAPPEND) 
                v.budget -= inlineExtraAppendCost;
            else if (n.Op == ODCLCONST || n.Op == OEMPTY || n.Op == OFALL || n.Op == OLABEL) 
                // These nodes don't produce code; omit from inlining budget.
                return false;
            else if (n.Op == OIF) 
                if (Isconst(n.Left, CTBOOL))
                { 
                    // This if and the condition cost nothing.
                    return v.visitList(n.Ninit) || v.visitList(n.Nbody) || v.visitList(n.Rlist);

                }

            else if (n.Op == ONAME) 
                if (n.Class() == PAUTO)
                {
                    v.usedLocals[n] = true;
                }

                        v.budget--; 

            // When debugging, don't stop early, to get full cost of inlining this function
            if (v.budget < 0L && Debug['m'] < 2L && !logopt.Enabled())
            {
                return true;
            }

            return v.visit(n.Left) || v.visit(n.Right) || v.visitList(n.List) || v.visitList(n.Rlist) || v.visitList(n.Ninit) || v.visitList(n.Nbody);

        }

        // Inlcopy and inlcopylist recursively copy the body of a function.
        // Any name-like node of non-local class is marked for re-export by adding it to
        // the exportlist.
        private static slice<ptr<Node>> inlcopylist(slice<ptr<Node>> ll)
        {
            var s = make_slice<ptr<Node>>(0L, len(ll));
            foreach (var (_, n) in ll)
            {
                s = append(s, inlcopy(_addr_n));
            }
            return s;

        }

        private static ptr<Node> inlcopy(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_null!;
            }


            if (n.Op == ONAME || n.Op == OTYPE || n.Op == OLITERAL) 
                return _addr_n!;
                        var m = n.copy();
            if (m.Func != null)
            {
                Fatalf("unexpected Func: %v", m);
            }

            m.Left = inlcopy(_addr_n.Left);
            m.Right = inlcopy(_addr_n.Right);
            m.List.Set(inlcopylist(n.List.Slice()));
            m.Rlist.Set(inlcopylist(n.Rlist.Slice()));
            m.Ninit.Set(inlcopylist(n.Ninit.Slice()));
            m.Nbody.Set(inlcopylist(n.Nbody.Slice()));

            return _addr_m!;

        }

        private static long countNodes(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return 0L;
            }

            long cnt = 1L;
            cnt += countNodes(_addr_n.Left);
            cnt += countNodes(_addr_n.Right);
            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.Ninit.Slice())
                {
                    n1 = __n1;
                    cnt += countNodes(_addr_n1);
                }

                n1 = n1__prev1;
            }

            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.Nbody.Slice())
                {
                    n1 = __n1;
                    cnt += countNodes(_addr_n1);
                }

                n1 = n1__prev1;
            }

            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.List.Slice())
                {
                    n1 = __n1;
                    cnt += countNodes(_addr_n1);
                }

                n1 = n1__prev1;
            }

            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.Rlist.Slice())
                {
                    n1 = __n1;
                    cnt += countNodes(_addr_n1);
                }

                n1 = n1__prev1;
            }

            return cnt;

        }

        // Inlcalls/nodelist/node walks fn's statements and expressions and substitutes any
        // calls made to inlineable functions. This is the external entry point.
        private static void inlcalls(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            var savefn = Curfn;
            Curfn = fn;
            var maxCost = int32(inlineMaxBudget);
            if (countNodes(_addr_fn) >= inlineBigFunctionNodes)
            {
                maxCost = inlineBigFunctionMaxCost;
            } 
            // Map to keep track of functions that have been inlined at a particular
            // call site, in order to stop inlining when we reach the beginning of a
            // recursion cycle again. We don't inline immediately recursive functions,
            // but allow inlining if there is a recursion cycle of many functions.
            // Most likely, the inlining will stop before we even hit the beginning of
            // the cycle again, but the map catches the unusual case.
            var inlMap = make_map<ptr<Node>, bool>();
            fn = inlnode(_addr_fn, maxCost, inlMap);
            if (fn != Curfn)
            {
                Fatalf("inlnode replaced curfn");
            }

            Curfn = savefn;

        }

        // Turn an OINLCALL into a statement.
        private static void inlconv2stmt(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            n.Op = OBLOCK; 

            // n->ninit stays
            n.List.Set(n.Nbody.Slice());

            n.Nbody.Set(null);
            n.Rlist.Set(null);

        }

        // Turn an OINLCALL into a single valued expression.
        // The result of inlconv2expr MUST be assigned back to n, e.g.
        //     n.Left = inlconv2expr(n.Left)
        private static ptr<Node> inlconv2expr(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var r = n.Rlist.First();
            return _addr_addinit(r, append(n.Ninit.Slice(), n.Nbody.Slice()))!;
        }

        // Turn the rlist (with the return values) of the OINLCALL in
        // n into an expression list lumping the ninit and body
        // containing the inlined statements on the first list element so
        // order will be preserved Used in return, oas2func and call
        // statements.
        private static slice<ptr<Node>> inlconv2list(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OINLCALL || n.Rlist.Len() == 0L)
            {
                Fatalf("inlconv2list %+v\n", n);
            }

            var s = n.Rlist.Slice();
            s[0L] = addinit(s[0L], append(n.Ninit.Slice(), n.Nbody.Slice()));
            return s;

        }

        private static void inlnodelist(Nodes l, int maxCost, map<ptr<Node>, bool> inlMap)
        {
            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = inlnode(_addr_s[i], maxCost, inlMap);
            }

        }

        // inlnode recurses over the tree to find inlineable calls, which will
        // be turned into OINLCALLs by mkinlcall. When the recursion comes
        // back up will examine left, right, list, rlist, ninit, ntest, nincr,
        // nbody and nelse and use one of the 4 inlconv/glue functions above
        // to turn the OINLCALL into an expression, a statement, or patch it
        // in to this nodes list or rlist as appropriate.
        // NOTE it makes no sense to pass the glue functions down the
        // recursion to the level where the OINLCALL gets created because they
        // have to edit /this/ n, so you'd have to push that one down as well,
        // but then you may as well do it here.  so this is cleaner and
        // shorter and less complicated.
        // The result of inlnode MUST be assigned back to n, e.g.
        //     n.Left = inlnode(n.Left)
        private static ptr<Node> inlnode(ptr<Node> _addr_n, int maxCost, map<ptr<Node>, bool> inlMap)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_n!;
            }


            // inhibit inlining of their argument
            if (n.Op == ODEFER || n.Op == OGO) 

                if (n.Left.Op == OCALLFUNC || n.Left.Op == OCALLMETH) 
                    n.Left.SetNoInline(true);
                                return _addr_n!; 

                // TODO do them here (or earlier),
                // so escape analysis can avoid more heapmoves.
            else if (n.Op == OCLOSURE) 
                return _addr_n!;
            else if (n.Op == OCALLMETH) 
                // Prevent inlining some reflect.Value methods when using checkptr,
                // even when package reflect was compiled without it (#35073).
                {
                    var s__prev1 = s;

                    var s = n.Left.Sym;

                    if (Debug_checkptr != 0L && isReflectPkg(s.Pkg) && (s.Name == "Value.UnsafeAddr" || s.Name == "Value.Pointer"))
                    {
                        return _addr_n!;
                    }

                    s = s__prev1;

                }

                        var lno = setlineno(n);

            inlnodelist(n.Ninit, maxCost, inlMap);
            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.Ninit.Slice())
                {
                    n1 = __n1;
                    if (n1.Op == OINLCALL)
                    {
                        inlconv2stmt(_addr_n1);
                    }

                }

                n1 = n1__prev1;
            }

            n.Left = inlnode(_addr_n.Left, maxCost, inlMap);
            if (n.Left != null && n.Left.Op == OINLCALL)
            {
                n.Left = inlconv2expr(_addr_n.Left);
            }

            n.Right = inlnode(_addr_n.Right, maxCost, inlMap);
            if (n.Right != null && n.Right.Op == OINLCALL)
            {
                if (n.Op == OFOR || n.Op == OFORUNTIL)
                {
                    inlconv2stmt(_addr_n.Right);
                }
                else if (n.Op == OAS2FUNC)
                {
                    n.Rlist.Set(inlconv2list(_addr_n.Right));
                    n.Right = null;
                    n.Op = OAS2;
                    n.SetTypecheck(0L);
                    n = typecheck(n, ctxStmt);
                }
                else
                {
                    n.Right = inlconv2expr(_addr_n.Right);
                }

            }

            inlnodelist(n.List, maxCost, inlMap);
            if (n.Op == OBLOCK)
            {
                foreach (var (_, n2) in n.List.Slice())
                {
                    if (n2.Op == OINLCALL)
                    {
                        inlconv2stmt(_addr_n2);
                    }

                }
            else
            }            {
                s = n.List.Slice();
                {
                    var i1__prev1 = i1;
                    var n1__prev1 = n1;

                    foreach (var (__i1, __n1) in s)
                    {
                        i1 = __i1;
                        n1 = __n1;
                        if (n1 != null && n1.Op == OINLCALL)
                        {
                            s[i1] = inlconv2expr(_addr_s[i1]);
                        }

                    }

                    i1 = i1__prev1;
                    n1 = n1__prev1;
                }
            }

            inlnodelist(n.Rlist, maxCost, inlMap);
            s = n.Rlist.Slice();
            {
                var i1__prev1 = i1;
                var n1__prev1 = n1;

                foreach (var (__i1, __n1) in s)
                {
                    i1 = __i1;
                    n1 = __n1;
                    if (n1.Op == OINLCALL)
                    {
                        if (n.Op == OIF)
                        {
                            inlconv2stmt(_addr_n1);
                        }
                        else
                        {
                            s[i1] = inlconv2expr(_addr_s[i1]);
                        }

                    }

                }

                i1 = i1__prev1;
                n1 = n1__prev1;
            }

            inlnodelist(n.Nbody, maxCost, inlMap);
            foreach (var (_, n) in n.Nbody.Slice())
            {
                if (n.Op == OINLCALL)
                {
                    inlconv2stmt(_addr_n);
                }

            } 

            // with all the branches out of the way, it is now time to
            // transmogrify this node itself unless inhibited by the
            // switch at the top of this function.

            if (n.Op == OCALLFUNC || n.Op == OCALLMETH) 
                if (n.NoInline())
                {
                    return _addr_n!;
                }

            
            if (n.Op == OCALLFUNC) 
                if (Debug['m'] > 3L)
                {
                    fmt.Printf("%v:call to func %+v\n", n.Line(), n.Left);
                }

                if (n.Left.Func != null && n.Left.Func.Inl != null && !isIntrinsicCall(n))
                { // normal case
                    n = mkinlcall(_addr_n, _addr_n.Left, maxCost, inlMap);

                }
                else if (n.Left.isMethodExpression() && asNode(n.Left.Sym.Def) != null)
                {
                    n = mkinlcall(_addr_n, _addr_asNode(n.Left.Sym.Def), maxCost, inlMap);
                }
                else if (n.Left.Op == OCLOSURE)
                {
                    {
                        var f__prev4 = f;

                        var f = inlinableClosure(_addr_n.Left);

                        if (f != null)
                        {
                            n = mkinlcall(_addr_n, _addr_f, maxCost, inlMap);
                        }

                        f = f__prev4;

                    }

                }
                else if (n.Left.Op == ONAME && n.Left.Name != null && n.Left.Name.Defn != null)
                {
                    {
                        var d = n.Left.Name.Defn;

                        if (d.Op == OAS && d.Right.Op == OCLOSURE)
                        {
                            {
                                var f__prev6 = f;

                                f = inlinableClosure(_addr_d.Right);

                                if (f != null)
                                { 
                                    // NB: this check is necessary to prevent indirect re-assignment of the variable
                                    // having the address taken after the invocation or only used for reads is actually fine
                                    // but we have no easy way to distinguish the safe cases
                                    if (d.Left.Name.Addrtaken())
                                    {
                                        if (Debug['m'] > 1L)
                                        {
                                            fmt.Printf("%v: cannot inline escaping closure variable %v\n", n.Line(), n.Left);
                                        }

                                        if (logopt.Enabled())
                                        {
                                            logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", Curfn.funcname(), fmt.Sprintf("%v cannot be inlined (escaping closure variable)", n.Left));
                                        }

                                        break;

                                    } 

                                    // ensure the variable is never re-assigned
                                    {
                                        var (unsafe, a) = reassigned(_addr_n.Left);

                                        if (unsafe)
                                        {
                                            if (Debug['m'] > 1L)
                                            {
                                                if (a != null)
                                                {
                                                    fmt.Printf("%v: cannot inline re-assigned closure variable at %v: %v\n", n.Line(), a.Line(), a);
                                                    if (logopt.Enabled())
                                                    {
                                                        logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", Curfn.funcname(), fmt.Sprintf("%v cannot be inlined (re-assigned closure variable)", a));
                                                    }

                                                }
                                                else
                                                {
                                                    fmt.Printf("%v: cannot inline global closure variable %v\n", n.Line(), n.Left);
                                                    if (logopt.Enabled())
                                                    {
                                                        logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", Curfn.funcname(), fmt.Sprintf("%v cannot be inlined (global closure variable)", n.Left));
                                                    }

                                                }

                                            }

                                            break;

                                        }

                                    }

                                    n = mkinlcall(_addr_n, _addr_f, maxCost, inlMap);

                                }

                                f = f__prev6;

                            }

                        }

                    }

                }

            else if (n.Op == OCALLMETH) 
                if (Debug['m'] > 3L)
                {
                    fmt.Printf("%v:call to meth %L\n", n.Line(), n.Left.Right);
                } 

                // typecheck should have resolved ODOTMETH->type, whose nname points to the actual function.
                if (n.Left.Type == null)
                {
                    Fatalf("no function type for [%p] %+v\n", n.Left, n.Left);
                }

                if (n.Left.Type.Nname() == null)
                {
                    Fatalf("no function definition for [%p] %+v\n", n.Left.Type, n.Left.Type);
                }

                n = mkinlcall(_addr_n, _addr_asNode(n.Left.Type.FuncType().Nname), maxCost, inlMap);
                        lineno = lno;
            return _addr_n!;

        }

        // inlinableClosure takes an OCLOSURE node and follows linkage to the matching ONAME with
        // the inlinable body. Returns nil if the function is not inlinable.
        private static ptr<Node> inlinableClosure(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var c = n.Func.Closure;
            caninl(_addr_c);
            var f = c.Func.Nname;
            if (f == null || f.Func.Inl == null)
            {
                return _addr_null!;
            }

            return _addr_f!;

        }

        // reassigned takes an ONAME node, walks the function in which it is defined, and returns a boolean
        // indicating whether the name has any assignments other than its declaration.
        // The second return value is the first such assignment encountered in the walk, if any. It is mostly
        // useful for -m output documenting the reason for inhibited optimizations.
        // NB: global variables are always considered to be re-assigned.
        // TODO: handle initial declaration not including an assignment and followed by a single assignment?
        private static (bool, ptr<Node>) reassigned(ptr<Node> _addr_n)
        {
            bool _p0 = default;
            ptr<Node> _p0 = default!;
            ref Node n = ref _addr_n.val;

            if (n.Op != ONAME)
            {
                Fatalf("reassigned %v", n);
            } 
            // no way to reliably check for no-reassignment of globals, assume it can be
            if (n.Name.Curfn == null)
            {
                return (true, _addr_null!);
            }

            var f = n.Name.Curfn; 
            // There just might be a good reason for this although this can be pretty surprising:
            // local variables inside a closure have Curfn pointing to the OCLOSURE node instead
            // of the corresponding ODCLFUNC.
            // We need to walk the function body to check for reassignments so we follow the
            // linkage to the ODCLFUNC node as that is where body is held.
            if (f.Op == OCLOSURE)
            {
                f = f.Func.Closure;
            }

            reassignVisitor v = new reassignVisitor(name:n);
            var a = v.visitList(f.Nbody);
            return (a != null, _addr_a!);

        }

        private partial struct reassignVisitor
        {
            public ptr<Node> name;
        }

        private static ptr<Node> visit(this ptr<reassignVisitor> _addr_v, ptr<Node> _addr_n)
        {
            ref reassignVisitor v = ref _addr_v.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_null!;
            }


            if (n.Op == OAS) 
                if (n.Left == v.name && n != v.name.Name.Defn)
                {
                    return _addr_n!;
                }

                return _addr_null!;
            else if (n.Op == OAS2 || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2DOTTYPE) 
                foreach (var (_, p) in n.List.Slice())
                {
                    if (p == v.name && n != v.name.Name.Defn)
                    {
                        return _addr_n!;
                    }

                }
                return _addr_null!;
                        {
                var a__prev1 = a;

                var a = v.visit(n.Left);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            {
                var a__prev1 = a;

                a = v.visit(n.Right);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            {
                var a__prev1 = a;

                a = v.visitList(n.List);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            {
                var a__prev1 = a;

                a = v.visitList(n.Rlist);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            {
                var a__prev1 = a;

                a = v.visitList(n.Ninit);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            {
                var a__prev1 = a;

                a = v.visitList(n.Nbody);

                if (a != null)
                {
                    return _addr_a!;
                }

                a = a__prev1;

            }

            return _addr_null!;

        }

        private static ptr<Node> visitList(this ptr<reassignVisitor> _addr_v, Nodes l)
        {
            ref reassignVisitor v = ref _addr_v.val;

            foreach (var (_, n) in l.Slice())
            {
                {
                    var a = v.visit(n);

                    if (a != null)
                    {
                        return _addr_a!;
                    }

                }

            }
            return _addr_null!;

        }

        private static ptr<Node> tinlvar(ptr<types.Field> _addr_t, map<ptr<Node>, ptr<Node>> inlvars)
        {
            ref types.Field t = ref _addr_t.val;

            {
                var n = asNode(t.Nname);

                if (n != null && !n.isBlank())
                {
                    var inlvar = inlvars[n];
                    if (inlvar == null)
                    {
                        Fatalf("missing inlvar for %v\n", n);
                    }

                    return _addr_inlvar!;

                }

            }


            return _addr_typecheck(nblank, ctxExpr | ctxAssign)!;

        }

        private static long inlgen = default;

        // If n is a call, and fn is a function with an inlinable body,
        // return an OINLCALL.
        // On return ninit has the parameter assignments, the nbody is the
        // inlined function body and list, rlist contain the input, output
        // parameters.
        // The result of mkinlcall MUST be assigned back to n, e.g.
        //     n.Left = mkinlcall(n.Left, fn, isddd)
        private static ptr<Node> mkinlcall(ptr<Node> _addr_n, ptr<Node> _addr_fn, int maxCost, map<ptr<Node>, bool> inlMap) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;
            ref Node fn = ref _addr_fn.val;

            if (fn.Func.Inl == null)
            {
                if (logopt.Enabled())
                {
                    logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", Curfn.funcname(), fmt.Sprintf("%s cannot be inlined", fn.pkgFuncName()));
                }

                return _addr_n!;

            }

            if (fn.Func.Inl.Cost > maxCost)
            { 
                // The inlined function body is too big. Typically we use this check to restrict
                // inlining into very big functions.  See issue 26546 and 17566.
                if (logopt.Enabled())
                {
                    logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", Curfn.funcname(), fmt.Sprintf("cost %d of %s exceeds max large caller cost %d", fn.Func.Inl.Cost, fn.pkgFuncName(), maxCost));
                }

                return _addr_n!;

            }

            if (fn == Curfn || fn.Name.Defn == Curfn)
            { 
                // Can't recursively inline a function into itself.
                if (logopt.Enabled())
                {
                    logopt.LogOpt(n.Pos, "cannotInlineCall", "inline", fmt.Sprintf("recursive call to %s", Curfn.funcname()));
                }

                return _addr_n!;

            }

            if (instrumenting && isRuntimePkg(fn.Sym.Pkg))
            { 
                // Runtime package must not be instrumented.
                // Instrument skips runtime package. However, some runtime code can be
                // inlined into other packages and instrumented there. To avoid this,
                // we disable inlining of runtime functions when instrumenting.
                // The example that we observed is inlining of LockOSThread,
                // which lead to false race reports on m contents.
                return _addr_n!;

            }

            if (inlMap[fn])
            {
                if (Debug['m'] > 1L)
                {
                    fmt.Printf("%v: cannot inline %v into %v: repeated recursive cycle\n", n.Line(), fn, Curfn.funcname());
                }

                return _addr_n!;

            }

            inlMap[fn] = true;
            defer(() =>
            {
                inlMap[fn] = false;
            }());
            if (Debug_typecheckinl == 0L)
            {
                typecheckinl(_addr_fn);
            } 

            // We have a function node, and it has an inlineable body.
            if (Debug['m'] > 1L)
            {
                fmt.Printf("%v: inlining call to %v %#v { %#v }\n", n.Line(), fn.Sym, fn.Type, asNodes(fn.Func.Inl.Body));
            }
            else if (Debug['m'] != 0L)
            {
                fmt.Printf("%v: inlining call to %v\n", n.Line(), fn);
            }

            if (Debug['m'] > 2L)
            {
                fmt.Printf("%v: Before inlining: %+v\n", n.Line(), n);
            }

            if (ssaDump != "" && ssaDump == Curfn.funcname())
            {
                ssaDumpInlined = append(ssaDumpInlined, fn);
            }

            var ninit = n.Ninit; 

            // Make temp names to use instead of the originals.
            var inlvars = make_map<ptr<Node>, ptr<Node>>(); 

            // record formals/locals for later post-processing
            slice<ptr<Node>> inlfvars = default; 

            // Handle captured variables when inlining closures.
            if (fn.Name.Defn != null)
            {
                {
                    var c = fn.Name.Defn.Func.Closure;

                    if (c != null)
                    {
                        {
                            var v__prev1 = v;

                            foreach (var (_, __v) in c.Func.Closure.Func.Cvars.Slice())
                            {
                                v = __v;
                                if (v.Op == OXXX)
                                {
                                    continue;
                                }

                                var o = v.Name.Param.Outer; 
                                // make sure the outer param matches the inlining location
                                // NB: if we enabled inlining of functions containing OCLOSURE or refined
                                // the reassigned check via some sort of copy propagation this would most
                                // likely need to be changed to a loop to walk up to the correct Param
                                if (o == null || (o.Name.Curfn != Curfn && o.Name.Curfn.Func.Closure != Curfn))
                                {
                                    Fatalf("%v: unresolvable capture %v %v\n", n.Line(), fn, v);
                                }

                                if (v.Name.Byval())
                                {
                                    var iv = typecheck(inlvar(_addr_v), ctxExpr);
                                    ninit.Append(nod(ODCL, iv, null));
                                    ninit.Append(typecheck(nod(OAS, iv, o), ctxStmt));
                                    inlvars[v] = iv;
                                }
                                else
                                {
                                    var addr = newname(lookup("&" + v.Sym.Name));
                                    addr.Type = types.NewPtr(v.Type);
                                    var ia = typecheck(inlvar(_addr_addr), ctxExpr);
                                    ninit.Append(nod(ODCL, ia, null));
                                    ninit.Append(typecheck(nod(OAS, ia, nod(OADDR, o, null)), ctxStmt));
                                    inlvars[addr] = ia; 

                                    // When capturing by reference, all occurrence of the captured var
                                    // must be substituted with dereference of the temporary address
                                    inlvars[v] = typecheck(nod(ODEREF, ia, null), ctxExpr);

                                }

                            }

                            v = v__prev1;
                        }
                    }

                }

            }

            foreach (var (_, ln) in fn.Func.Inl.Dcl)
            {
                if (ln.Op != ONAME)
                {
                    continue;
                }

                if (ln.Class() == PPARAMOUT)
                { // return values handled below.
                    continue;

                }

                if (ln.isParamStackCopy())
                { // ignore the on-stack copy of a parameter that moved to the heap
                    continue;

                }

                inlvars[ln] = typecheck(inlvar(_addr_ln), ctxExpr);
                if (ln.Class() == PPARAM || ln.Name.Param.Stackcopy != null && ln.Name.Param.Stackcopy.Class() == PPARAM)
                {
                    ninit.Append(nod(ODCL, inlvars[ln], null));
                }

                if (genDwarfInline > 0L)
                {
                    var inlf = inlvars[ln];
                    if (ln.Class() == PPARAM)
                    {
                        inlf.Name.SetInlFormal(true);
                    }
                    else
                    {
                        inlf.Name.SetInlLocal(true);
                    }

                    inlf.Pos = ln.Pos;
                    inlfvars = append(inlfvars, inlf);

                }

            } 

            // temporaries for return values.
            slice<ptr<Node>> retvars = default;
            foreach (var (i, t) in fn.Type.Results().Fields().Slice())
            {
                ptr<Node> m;
                var mpos = t.Pos;
                {
                    var n__prev1 = n;

                    var n = asNode(t.Nname);

                    if (n != null && !n.isBlank())
                    {
                        m = inlvar(_addr_n);
                        m = typecheck(m, ctxExpr);
                        inlvars[n] = m;
                    }
                    else
                    { 
                        // anonymous return values, synthesize names for use in assignment that replaces return
                        m = retvar(_addr_t, i);

                    }

                    n = n__prev1;

                }


                if (genDwarfInline > 0L)
                { 
                    // Don't update the src.Pos on a return variable if it
                    // was manufactured by the inliner (e.g. "~R2"); such vars
                    // were not part of the original callee.
                    if (!strings.HasPrefix(m.Sym.Name, "~R"))
                    {
                        m.Name.SetInlFormal(true);
                        m.Pos = mpos;
                        inlfvars = append(inlfvars, m);
                    }

                }

                ninit.Append(nod(ODCL, m, null));
                retvars = append(retvars, m);

            } 

            // Assign arguments to the parameters' temp names.
            var @as = nod(OAS2, null, null);
            @as.Rlist.Set(n.List.Slice()); 

            // For non-dotted calls to variadic functions, we assign the
            // variadic parameter's temp name separately.
            ptr<Node> vas;

            if (fn.IsMethod())
            {
                var rcv = fn.Type.Recv();

                if (n.Left.Op == ODOTMETH)
                { 
                    // For x.M(...), assign x directly to the
                    // receiver parameter.
                    if (n.Left.Left == null)
                    {
                        Fatalf("method call without receiver: %+v", n);
                    }

                    var ras = nod(OAS, tinlvar(_addr_rcv, inlvars), n.Left.Left);
                    ras = typecheck(ras, ctxStmt);
                    ninit.Append(ras);

                }
                else
                { 
                    // For T.M(...), add the receiver parameter to
                    // as.List, so it's assigned by the normal
                    // arguments.
                    if (@as.Rlist.Len() == 0L)
                    {
                        Fatalf("non-method call to method without first arg: %+v", n);
                    }

                    @as.List.Append(tinlvar(_addr_rcv, inlvars));

                }

            }

            foreach (var (_, param) in fn.Type.Params().Fields().Slice())
            { 
                // For ordinary parameters or variadic parameters in
                // dotted calls, just add the variable to the
                // assignment list, and we're done.
                if (!param.IsDDD() || n.IsDDD())
                {
                    @as.List.Append(tinlvar(_addr_param, inlvars));
                    continue;
                } 

                // Otherwise, we need to collect the remaining values
                // to pass as a slice.
                var numvals = n.List.Len();

                var x = @as.List.Len();
                while (@as.List.Len() < numvals)
                {
                    @as.List.Append(argvar(_addr_param.Type, @as.List.Len()));
                }

                var varargs = @as.List.Slice()[x..];

                vas = nod(OAS, tinlvar(_addr_param, inlvars), null);
                if (len(varargs) == 0L)
                {
                    vas.Right = nodnil();
                    vas.Right.Type = param.Type;
                }
                else
                {
                    vas.Right = nod(OCOMPLIT, null, typenod(param.Type));
                    vas.Right.List.Set(varargs);
                }

            }
            if (@as.Rlist.Len() != 0L)
            {
                as = typecheck(as, ctxStmt);
                ninit.Append(as);
            }

            if (vas != null)
            {
                vas = typecheck(vas, ctxStmt);
                ninit.Append(vas);
            } 

            // Zero the return parameters.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in retvars)
                {
                    n = __n;
                    ras = nod(OAS, n, null);
                    ras = typecheck(ras, ctxStmt);
                    ninit.Append(ras);
                }

                n = n__prev1;
            }

            var retlabel = autolabel(".i");

            inlgen++;

            long parent = -1L;
            {
                var b = Ctxt.PosTable.Pos(n.Pos).Base();

                if (b != null)
                {
                    parent = b.InliningIndex();
                }

            }

            var newIndex = Ctxt.InlTree.Add(parent, n.Pos, fn.Sym.Linksym()); 

            // Add an inline mark just before the inlined body.
            // This mark is inline in the code so that it's a reasonable spot
            // to put a breakpoint. Not sure if that's really necessary or not
            // (in which case it could go at the end of the function instead).
            // Note issue 28603.
            var inlMark = nod(OINLMARK, null, null);
            inlMark.Pos = n.Pos.WithIsStmt();
            inlMark.Xoffset = int64(newIndex);
            ninit.Append(inlMark);

            if (genDwarfInline > 0L)
            {
                if (!fn.Sym.Linksym().WasInlined())
                {
                    Ctxt.DwFixups.SetPrecursorFunc(fn.Sym.Linksym(), fn);
                    fn.Sym.Linksym().Set(obj.AttrWasInlined, true);
                }

            }

            inlsubst subst = new inlsubst(retlabel:retlabel,retvars:retvars,inlvars:inlvars,bases:make(map[*src.PosBase]*src.PosBase),newInlIndex:newIndex,);

            var body = subst.list(asNodes(fn.Func.Inl.Body));

            var lab = nodSym(OLABEL, null, retlabel);
            body = append(body, lab);

            typecheckslice(body, ctxStmt);

            if (genDwarfInline > 0L)
            {
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in inlfvars)
                    {
                        v = __v;
                        v.Pos = subst.updatedPos(v.Pos);
                    }

                    v = v__prev1;
                }
            } 

            //dumplist("ninit post", ninit);
            var call = nod(OINLCALL, null, null);
            call.Ninit.Set(ninit.Slice());
            call.Nbody.Set(body);
            call.Rlist.Set(retvars);
            call.Type = n.Type;
            call.SetTypecheck(1L); 

            // transitive inlining
            // might be nice to do this before exporting the body,
            // but can't emit the body with inlining expanded.
            // instead we emit the things that the body needs
            // and each use must redo the inlining.
            // luckily these are small.
            inlnodelist(call.Nbody, maxCost, inlMap);
            {
                var n__prev1 = n;

                foreach (var (_, __n) in call.Nbody.Slice())
                {
                    n = __n;
                    if (n.Op == OINLCALL)
                    {
                        inlconv2stmt(_addr_n);
                    }

                }

                n = n__prev1;
            }

            if (Debug['m'] > 2L)
            {
                fmt.Printf("%v: After inlining %+v\n\n", call.Line(), call);
            }

            return _addr_call!;

        });

        // Every time we expand a function we generate a new set of tmpnames,
        // PAUTO's in the calling functions, and link them off of the
        // PPARAM's, PAUTOS and PPARAMOUTs of the called function.
        private static ptr<Node> inlvar(ptr<Node> _addr_var_)
        {
            ref Node var_ = ref _addr_var_.val;

            if (Debug['m'] > 3L)
            {
                fmt.Printf("inlvar %+v\n", var_);
            }

            var n = newname(var_.Sym);
            n.Type = var_.Type;
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            n.Name.SetAddrtaken(var_.Name.Addrtaken());

            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return _addr_n!;

        }

        // Synthesize a variable to store the inlined function's results in.
        private static ptr<Node> retvar(ptr<types.Field> _addr_t, long i)
        {
            ref types.Field t = ref _addr_t.val;

            var n = newname(lookupN("~R", i));
            n.Type = t.Type;
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return _addr_n!;

        }

        // Synthesize a variable to store the inlined function's arguments
        // when they come from a multiple return call.
        private static ptr<Node> argvar(ptr<types.Type> _addr_t, long i)
        {
            ref types.Type t = ref _addr_t.val;

            var n = newname(lookupN("~arg", i));
            n.Type = t.Elem();
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return _addr_n!;

        }

        // The inlsubst type implements the actual inlining of a single
        // function call.
        private partial struct inlsubst
        {
            public ptr<types.Sym> retlabel; // Temporary result variables.
            public slice<ptr<Node>> retvars;
            public map<ptr<Node>, ptr<Node>> inlvars; // bases maps from original PosBase to PosBase with an extra
// inlined call frame.
            public map<ptr<src.PosBase>, ptr<src.PosBase>> bases; // newInlIndex is the index of the inlined call frame to
// insert for inlined nodes.
            public long newInlIndex;
        }

        // list inlines a list of nodes.
        private static slice<ptr<Node>> list(this ptr<inlsubst> _addr_subst, Nodes ll)
        {
            ref inlsubst subst = ref _addr_subst.val;

            var s = make_slice<ptr<Node>>(0L, ll.Len());
            foreach (var (_, n) in ll.Slice())
            {
                s = append(s, subst.node(n));
            }
            return s;

        }

        // node recursively copies a node from the saved pristine body of the
        // inlined function, substituting references to input/output
        // parameters with ones to the tmpnames, and substituting returns with
        // assignments to the output.
        private static ptr<Node> node(this ptr<inlsubst> _addr_subst, ptr<Node> _addr_n)
        {
            ref inlsubst subst = ref _addr_subst.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_null!;
            }


            if (n.Op == ONAME) 
                {
                    var inlvar = subst.inlvars[n];

                    if (inlvar != null)
                    { // These will be set during inlnode
                        if (Debug['m'] > 2L)
                        {
                            fmt.Printf("substituting name %+v  ->  %+v\n", n, inlvar);
                        }

                        return _addr_inlvar!;

                    }

                }


                if (Debug['m'] > 2L)
                {
                    fmt.Printf("not substituting name %+v\n", n);
                }

                return _addr_n!;
            else if (n.Op == OLITERAL || n.Op == OTYPE) 
                // If n is a named constant or type, we can continue
                // using it in the inline copy. Otherwise, make a copy
                // so we can update the line number.
                if (n.Sym != null)
                {
                    return _addr_n!;
                } 

                // Since we don't handle bodies with closures, this return is guaranteed to belong to the current inlined function.

                //        dump("Return before substitution", n);
            else if (n.Op == ORETURN) 
                var m = nodSym(OGOTO, null, subst.retlabel);
                m.Ninit.Set(subst.list(n.Ninit));

                if (len(subst.retvars) != 0L && n.List.Len() != 0L)
                {
                    var @as = nod(OAS2, null, null); 

                    // Make a shallow copy of retvars.
                    // Otherwise OINLCALL.Rlist will be the same list,
                    // and later walk and typecheck may clobber it.
                    foreach (var (_, n) in subst.retvars)
                    {
                        @as.List.Append(n);
                    }
                    @as.Rlist.Set(subst.list(n.List));
                    as = typecheck(as, ctxStmt);
                    m.Ninit.Append(as);

                }

                typecheckslice(m.Ninit.Slice(), ctxStmt);
                m = typecheck(m, ctxStmt); 

                //        dump("Return after substitution", m);
                return _addr_m!;
            else if (n.Op == OGOTO || n.Op == OLABEL) 
                m = n.copy();
                m.Pos = subst.updatedPos(m.Pos);
                m.Ninit.Set(null);
                var p = fmt.Sprintf("%s·%d", n.Sym.Name, inlgen);
                m.Sym = lookup(p);

                return _addr_m!;
                        m = n.copy();
            m.Pos = subst.updatedPos(m.Pos);
            m.Ninit.Set(null);

            if (n.Op == OCLOSURE)
            {
                Fatalf("cannot inline function containing closure: %+v", n);
            }

            m.Left = subst.node(n.Left);
            m.Right = subst.node(n.Right);
            m.List.Set(subst.list(n.List));
            m.Rlist.Set(subst.list(n.Rlist));
            m.Ninit.Set(append(m.Ninit.Slice(), subst.list(n.Ninit)));
            m.Nbody.Set(subst.list(n.Nbody));

            return _addr_m!;

        }

        private static src.XPos updatedPos(this ptr<inlsubst> _addr_subst, src.XPos xpos)
        {
            ref inlsubst subst = ref _addr_subst.val;

            var pos = Ctxt.PosTable.Pos(xpos);
            var oldbase = pos.Base(); // can be nil
            var newbase = subst.bases[oldbase];
            if (newbase == null)
            {
                newbase = src.NewInliningBase(oldbase, subst.newInlIndex);
                subst.bases[oldbase] = newbase;
            }

            pos.SetBase(newbase);
            return Ctxt.PosTable.XPos(pos);

        }

        private static slice<ptr<Node>> pruneUnusedAutos(slice<ptr<Node>> ll, ptr<hairyVisitor> _addr_vis)
        {
            ref hairyVisitor vis = ref _addr_vis.val;

            var s = make_slice<ptr<Node>>(0L, len(ll));
            foreach (var (_, n) in ll)
            {
                if (n.Class() == PAUTO)
                {
                    {
                        var (_, found) = vis.usedLocals[n];

                        if (!found)
                        {
                            continue;
                        }

                    }

                }

                s = append(s, n);

            }
            return s;

        }
    }
}}}}
