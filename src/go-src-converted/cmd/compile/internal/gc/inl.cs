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
//      1: 80-nodes leaf functions, oneliners, lazy typechecking (default)
//      2: (unassigned)
//      3: allow variadic functions
//      4: allow non-leaf functions
//
// At some point this may get another default and become switch-offable with -N.
//
// The -d typcheckinl flag enables early typechecking of all imported bodies,
// which is useful to flush out bugs.
//
// The debug['m'] flag enables diagnostic output.  a single -m is useful for verifying
// which calls get inlined or not, more is for debugging, and may go away at any point.
//
// TODO:
//   - inline functions with ... args

// package gc -- go2cs converted at 2020 August 29 09:27:17 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\inl.go
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
        // Get the function's package. For ordinary functions it's on the ->sym, but for imported methods
        // the ->sym can be re-used in the local package, so peel it off the receiver's type.
        private static ref types.Pkg fnpkg(ref Node fn)
        {
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
                return rcvr.Sym.Pkg;
            }
            return fn.Sym.Pkg;
        }

        // Lazy typechecking of imported bodies. For local functions, caninl will set ->typecheck
        // because they're a copy of an already checked body.
        private static void typecheckinl(ref Node fn)
        {
            var lno = setlineno(fn); 

            // typecheckinl is only for imported functions;
            // their bodies may refer to unsafe as long as the package
            // was marked safe during import (which was checked then).
            // the ->inl of a local function has been typechecked before caninl copied it.
            var pkg = fnpkg(fn);

            if (pkg == localpkg || pkg == null)
            {
                return; // typecheckinl on local function
            }
            if (Debug['m'] > 2L || Debug_export != 0L)
            {
                fmt.Printf("typecheck import [%v] %L { %#v }\n", fn.Sym, fn, fn.Func.Inl);
            }
            var save_safemode = safemode;
            safemode = false;

            var savefn = Curfn;
            Curfn = fn;
            typecheckslice(fn.Func.Inl.Slice(), Etop);
            Curfn = savefn;

            safemode = save_safemode;

            lineno = lno;
        }

        // Caninl determines whether fn is inlineable.
        // If so, caninl saves fn->nbody in fn->inl and substitutes it with a copy.
        // fn and ->nbody will already have been typechecked.
        private static void caninl(ref Node _fn) => func(_fn, (ref Node fn, Defer defer, Panic _, Recover __) =>
        {
            if (fn.Op != ODCLFUNC)
            {
                Fatalf("caninl %v", fn);
            }
            if (fn.Func.Nname == null)
            {
                Fatalf("caninl no nname %+v", fn);
            }
            @string reason = default; // reason, if any, that the function was not inlined
            if (Debug['m'] > 1L)
            {
                defer(() =>
                {
                    if (reason != "")
                    {
                        fmt.Printf("%v: cannot inline %v: %s\n", fn.Line(), fn.Func.Nname, reason);
                    }
                }());
            } 

            // If marked "go:noinline", don't inline
            if (fn.Func.Pragma & Noinline != 0L)
            {
                reason = "marked go:noinline";
                return;
            } 

            // If marked "go:cgo_unsafe_args", don't inline, since the
            // function makes assumptions about its argument frame layout.
            if (fn.Func.Pragma & CgoUnsafeArgs != 0L)
            {
                reason = "marked go:cgo_unsafe_args";
                return;
            } 

            // The nowritebarrierrec checker currently works at function
            // granularity, so inlining yeswritebarrierrec functions can
            // confuse it (#22342). As a workaround, disallow inlining
            // them for now.
            if (fn.Func.Pragma & Yeswritebarrierrec != 0L)
            {
                reason = "marked go:yeswritebarrierrec";
                return;
            } 

            // If fn has no body (is defined outside of Go), cannot inline it.
            if (fn.Nbody.Len() == 0L)
            {
                reason = "no function body";
                return;
            }
            if (fn.Typecheck() == 0L)
            {
                Fatalf("caninl on non-typechecked function %v", fn);
            } 

            // can't handle ... args yet
            if (Debug['l'] < 3L)
            {
                var f = fn.Type.Params().Fields();
                {
                    var len = f.Len();

                    if (len > 0L)
                    {
                        {
                            var t = f.Index(len - 1L);

                            if (t.Isddd())
                            {
                                reason = "has ... args";
                                return;
                            }

                        }
                    }

                }
            } 

            // Runtime package must not be instrumented.
            // Instrument skips runtime package. However, some runtime code can be
            // inlined into other packages and instrumented there. To avoid this,
            // we disable inlining of runtime functions when instrumenting.
            // The example that we observed is inlining of LockOSThread,
            // which lead to false race reports on m contents.
            if (instrumenting && myimportpath == "runtime")
            {
                reason = "instrumenting and is runtime function";
                return;
            }
            var n = fn.Func.Nname;
            if (n.Func.InlinabilityChecked())
            {
                return;
            }
            defer(n.Func.SetInlinabilityChecked(true));

            const long maxBudget = 80L;

            hairyVisitor visitor = new hairyVisitor(budget:maxBudget);
            if (visitor.visitList(fn.Nbody))
            {
                reason = visitor.reason;
                return;
            }
            if (visitor.budget < 0L)
            {
                reason = fmt.Sprintf("function too complex: cost %d exceeds budget %d", maxBudget - visitor.budget, maxBudget);
                return;
            }
            var savefn = Curfn;
            Curfn = fn;

            n.Func.Inl.Set(fn.Nbody.Slice());
            fn.Nbody.Set(inlcopylist(n.Func.Inl.Slice()));
            var inldcl = inlcopylist(n.Name.Defn.Func.Dcl);
            n.Func.Inldcl.Set(inldcl);
            n.Func.InlCost = maxBudget - visitor.budget; 

            // hack, TODO, check for better way to link method nodes back to the thing with the ->inl
            // this is so export can find the body of a method
            fn.Type.FuncType().Nname = asTypesNode(n);

            if (Debug['m'] > 1L)
            {
                fmt.Printf("%v: can inline %#v as: %#v { %#v }\n", fn.Line(), n, fn.Type, n.Func.Inl);
            }
            else if (Debug['m'] != 0L)
            {
                fmt.Printf("%v: can inline %v\n", fn.Line(), n);
            }
            Curfn = savefn;
        });

        // inlFlood marks n's inline body for export and recursively ensures
        // all called functions are marked too.
        private static void inlFlood(ref Node n)
        {
            if (n == null)
            {
                return;
            }
            if (n.Op != ONAME || n.Class() != PFUNC)
            {
                Fatalf("inlFlood: unexpected %v, %v, %v", n, n.Op, n.Class());
            }
            if (n.Func == null)
            { 
                // TODO(mdempsky): Should init have a Func too?
                if (n.Sym.Name == "init")
                {
                    return;
                }
                Fatalf("inlFlood: missing Func on %v", n);
            }
            if (n.Func.Inl.Len() == 0L)
            {
                return;
            }
            if (n.Func.ExportInline())
            {
                return;
            }
            n.Func.SetExportInline(true);

            typecheckinl(n); 

            // Recursively flood any functions called by this one.
            inspectList(n.Func.Inl, n =>
            {

                if (n.Op == OCALLFUNC || n.Op == OCALLMETH) 
                    inlFlood(asNode(n.Left.Type.Nname()));
                                return true;
            });
        }

        // hairyVisitor visits a function body to determine its inlining
        // hairiness and whether or not it can be inlined.
        private partial struct hairyVisitor
        {
            public int budget;
            public @string reason;
        }

        // Look for anything we want to punt on.
        private static bool visitList(this ref hairyVisitor v, Nodes ll)
        {
            foreach (var (_, n) in ll.Slice())
            {
                if (v.visit(n))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool visit(this ref hairyVisitor v, ref Node n)
        {
            if (n == null)
            {
                return false;
            }

            // Call is okay if inlinable and we have the budget for the body.
            if (n.Op == OCALLFUNC) 
                if (isIntrinsicCall(n))
                {
                    v.budget--;
                    break;
                } 
                // Functions that call runtime.getcaller{pc,sp} can not be inlined
                // because getcaller{pc,sp} expect a pointer to the caller's first argument.
                if (n.Left.Op == ONAME && n.Left.Class() == PFUNC && isRuntimePkg(n.Left.Sym.Pkg))
                {
                    var fn = n.Left.Sym.Name;
                    if (fn == "getcallerpc" || fn == "getcallersp")
                    {
                        v.reason = "call to " + fn;
                        return true;
                    }
                }
                {
                    var fn__prev1 = fn;

                    fn = n.Left.Func;

                    if (fn != null && fn.Inl.Len() != 0L)
                    {
                        v.budget -= fn.InlCost;
                        break;
                    }

                    fn = fn__prev1;

                }
                if (n.Left.isMethodExpression())
                {
                    {
                        var d = asNode(n.Left.Sym.Def);

                        if (d != null && d.Func.Inl.Len() != 0L)
                        {
                            v.budget -= d.Func.InlCost;
                            break;
                        }

                    }
                } 
                // TODO(mdempsky): Budget for OCLOSURE calls if we
                // ever allow that. See #15561 and #23093.
                if (Debug['l'] < 4L)
                {
                    v.reason = "non-leaf function";
                    return true;
                } 

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
                {
                    var inlfn = asNode(t.FuncType().Nname).Func;

                    if (inlfn.Inl.Len() != 0L)
                    {
                        v.budget -= inlfn.InlCost;
                        break;
                    }

                }
                if (Debug['l'] < 4L)
                {
                    v.reason = "non-leaf method";
                    return true;
                } 

                // Things that are too hairy, irrespective of the budget
            else if (n.Op == OCALL || n.Op == OCALLINTER || n.Op == OPANIC) 
                if (Debug['l'] < 4L)
                {
                    v.reason = "non-leaf op " + n.Op.String();
                    return true;
                }
            else if (n.Op == ORECOVER) 
                // recover matches the argument frame pointer to find
                // the right panic value, so it needs an argument frame.
                v.reason = "call to recover";
                return true;
            else if (n.Op == OCLOSURE || n.Op == OCALLPART || n.Op == ORANGE || n.Op == OFOR || n.Op == OFORUNTIL || n.Op == OSELECT || n.Op == OTYPESW || n.Op == OPROC || n.Op == ODEFER || n.Op == ODCLTYPE || n.Op == OBREAK || n.Op == ORETJMP) 
                v.reason = "unhandled op " + n.Op.String();
                return true;
            else if (n.Op == ODCLCONST || n.Op == OEMPTY || n.Op == OFALL || n.Op == OLABEL) 
                // These nodes don't produce code; omit from inlining budget.
                return false;
                        v.budget--; 
            // TODO(mdempsky/josharian): Hacks to appease toolstash; remove.
            // See issue 17566 and CL 31674 for discussion.

            if (n.Op == OSTRUCTKEY) 
                v.budget--;
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR) 
                v.budget--;
            else if (n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                v.budget -= 2L;
            // When debugging, don't stop early, to get full cost of inlining this function
            if (v.budget < 0L && Debug['m'] < 2L)
            {
                return true;
            }
            return v.visit(n.Left) || v.visit(n.Right) || v.visitList(n.List) || v.visitList(n.Rlist) || v.visitList(n.Ninit) || v.visitList(n.Nbody);
        }

        // Inlcopy and inlcopylist recursively copy the body of a function.
        // Any name-like node of non-local class is marked for re-export by adding it to
        // the exportlist.
        private static slice<ref Node> inlcopylist(slice<ref Node> ll)
        {
            var s = make_slice<ref Node>(0L, len(ll));
            foreach (var (_, n) in ll)
            {
                s = append(s, inlcopy(n));
            }
            return s;
        }

        private static ref Node inlcopy(ref Node n)
        {
            if (n == null)
            {
                return null;
            }

            if (n.Op == ONAME || n.Op == OTYPE || n.Op == OLITERAL) 
                return n;
                        var m = n.Value;
            if (m.Func != null)
            {
                m.Func.Inl.Set(null);
            }
            m.Left = inlcopy(n.Left);
            m.Right = inlcopy(n.Right);
            m.List.Set(inlcopylist(n.List.Slice()));
            m.Rlist.Set(inlcopylist(n.Rlist.Slice()));
            m.Ninit.Set(inlcopylist(n.Ninit.Slice()));
            m.Nbody.Set(inlcopylist(n.Nbody.Slice()));

            return ref m;
        }

        // Inlcalls/nodelist/node walks fn's statements and expressions and substitutes any
        // calls made to inlineable functions. This is the external entry point.
        private static void inlcalls(ref Node fn)
        {
            var savefn = Curfn;
            Curfn = fn;
            fn = inlnode(fn);
            if (fn != Curfn)
            {
                Fatalf("inlnode replaced curfn");
            }
            Curfn = savefn;
        }

        // Turn an OINLCALL into a statement.
        private static void inlconv2stmt(ref Node n)
        {
            n.Op = OBLOCK; 

            // n->ninit stays
            n.List.Set(n.Nbody.Slice());

            n.Nbody.Set(null);
            n.Rlist.Set(null);
        }

        // Turn an OINLCALL into a single valued expression.
        // The result of inlconv2expr MUST be assigned back to n, e.g.
        //     n.Left = inlconv2expr(n.Left)
        private static ref Node inlconv2expr(ref Node n)
        {
            var r = n.Rlist.First();
            return addinit(r, append(n.Ninit.Slice(), n.Nbody.Slice()));
        }

        // Turn the rlist (with the return values) of the OINLCALL in
        // n into an expression list lumping the ninit and body
        // containing the inlined statements on the first list element so
        // order will be preserved Used in return, oas2func and call
        // statements.
        private static slice<ref Node> inlconv2list(ref Node n)
        {
            if (n.Op != OINLCALL || n.Rlist.Len() == 0L)
            {
                Fatalf("inlconv2list %+v\n", n);
            }
            var s = n.Rlist.Slice();
            s[0L] = addinit(s[0L], append(n.Ninit.Slice(), n.Nbody.Slice()));
            return s;
        }

        private static void inlnodelist(Nodes l)
        {
            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = inlnode(s[i]);
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
        private static ref Node inlnode(ref Node n)
        {
            if (n == null)
            {
                return n;
            }

            // inhibit inlining of their argument
            if (n.Op == ODEFER || n.Op == OPROC) 

                if (n.Left.Op == OCALLFUNC || n.Left.Op == OCALLMETH) 
                    n.Left.SetNoInline(true);
                                return n; 

                // TODO do them here (or earlier),
                // so escape analysis can avoid more heapmoves.
            else if (n.Op == OCLOSURE) 
                return n;
                        var lno = setlineno(n);

            inlnodelist(n.Ninit);
            {
                var n1__prev1 = n1;

                foreach (var (_, __n1) in n.Ninit.Slice())
                {
                    n1 = __n1;
                    if (n1.Op == OINLCALL)
                    {
                        inlconv2stmt(n1);
                    }
                }

                n1 = n1__prev1;
            }

            n.Left = inlnode(n.Left);
            if (n.Left != null && n.Left.Op == OINLCALL)
            {
                n.Left = inlconv2expr(n.Left);
            }
            n.Right = inlnode(n.Right);
            if (n.Right != null && n.Right.Op == OINLCALL)
            {
                if (n.Op == OFOR || n.Op == OFORUNTIL)
                {
                    inlconv2stmt(n.Right);
                }
                else
                {
                    n.Right = inlconv2expr(n.Right);
                }
            }
            inlnodelist(n.List);

            if (n.Op == OBLOCK)
            {
                foreach (var (_, n2) in n.List.Slice())
                {
                    if (n2.Op == OINLCALL)
                    {
                        inlconv2stmt(n2);
                    }
                }
                goto __switch_break0;
            }
            if (n.Op == ORETURN || n.Op == OCALLFUNC || n.Op == OCALLMETH || n.Op == OCALLINTER || n.Op == OAPPEND || n.Op == OCOMPLEX) 
            {
                // if we just replaced arg in f(arg()) or return arg with an inlined call
                // and arg returns multiple values, glue as list
                if (n.List.Len() == 1L && n.List.First().Op == OINLCALL && n.List.First().Rlist.Len() > 1L)
                {
                    n.List.Set(inlconv2list(n.List.First()));
                    break;
                }
            }
            // default: 
                var s = n.List.Slice();
                {
                    var i1__prev1 = i1;
                    var n1__prev1 = n1;

                    foreach (var (__i1, __n1) in s)
                    {
                        i1 = __i1;
                        n1 = __n1;
                        if (n1 != null && n1.Op == OINLCALL)
                        {
                            s[i1] = inlconv2expr(s[i1]);
                        }
                    }

                    i1 = i1__prev1;
                    n1 = n1__prev1;
                }

            __switch_break0:;

            inlnodelist(n.Rlist);
            if (n.Op == OAS2FUNC && n.Rlist.First().Op == OINLCALL)
            {
                n.Rlist.Set(inlconv2list(n.Rlist.First()));
                n.Op = OAS2;
                n.SetTypecheck(0L);
                n = typecheck(n, Etop);
            }
            else
            {
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
                                inlconv2stmt(n1);
                            }
                            else
                            {
                                s[i1] = inlconv2expr(s[i1]);
                            }
                        }
                    }

                    i1 = i1__prev1;
                    n1 = n1__prev1;
                }

            }
            inlnodelist(n.Nbody);
            foreach (var (_, n) in n.Nbody.Slice())
            {
                if (n.Op == OINLCALL)
                {
                    inlconv2stmt(n);
                }
            } 

            // with all the branches out of the way, it is now time to
            // transmogrify this node itself unless inhibited by the
            // switch at the top of this function.

            if (n.Op == OCALLFUNC || n.Op == OCALLMETH) 
                if (n.NoInline())
                {
                    return n;
                }
            
            if (n.Op == OCALLFUNC) 
                if (Debug['m'] > 3L)
                {
                    fmt.Printf("%v:call to func %+v\n", n.Line(), n.Left);
                }
                if (n.Left.Func != null && n.Left.Func.Inl.Len() != 0L && !isIntrinsicCall(n))
                { // normal case
                    n = mkinlcall(n, n.Left, n.Isddd());
                }
                else if (n.Left.isMethodExpression() && asNode(n.Left.Sym.Def) != null)
                {
                    n = mkinlcall(n, asNode(n.Left.Sym.Def), n.Isddd());
                }
                else if (n.Left.Op == OCLOSURE)
                {
                    {
                        var f__prev4 = f;

                        var f = inlinableClosure(n.Left);

                        if (f != null)
                        {
                            n = mkinlcall(n, f, n.Isddd());
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

                                f = inlinableClosure(d.Right);

                                if (f != null)
                                { 
                                    // NB: this check is necessary to prevent indirect re-assignment of the variable
                                    // having the address taken after the invocation or only used for reads is actually fine
                                    // but we have no easy way to distinguish the safe cases
                                    if (d.Left.Addrtaken())
                                    {
                                        if (Debug['m'] > 1L)
                                        {
                                            fmt.Printf("%v: cannot inline escaping closure variable %v\n", n.Line(), n.Left);
                                        }
                                        break;
                                    } 

                                    // ensure the variable is never re-assigned
                                    {
                                        var (unsafe, a) = reassigned(n.Left);

                                        if (unsafe)
                                        {
                                            if (Debug['m'] > 1L)
                                            {
                                                if (a != null)
                                                {
                                                    fmt.Printf("%v: cannot inline re-assigned closure variable at %v: %v\n", n.Line(), a.Line(), a);
                                                }
                                                else
                                                {
                                                    fmt.Printf("%v: cannot inline global closure variable %v\n", n.Line(), n.Left);
                                                }
                                            }
                                            break;
                                        }

                                    }
                                    n = mkinlcall(n, f, n.Isddd());
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
                n = mkinlcall(n, asNode(n.Left.Type.FuncType().Nname), n.Isddd());
                        lineno = lno;
            return n;
        }

        // inlinableClosure takes an OCLOSURE node and follows linkage to the matching ONAME with
        // the inlinable body. Returns nil if the function is not inlinable.
        private static ref Node inlinableClosure(ref Node n)
        {
            var c = n.Func.Closure;
            caninl(c);
            var f = c.Func.Nname;
            if (f == null || f.Func.Inl.Len() == 0L)
            {
                return null;
            }
            return f;
        }

        // reassigned takes an ONAME node, walks the function in which it is defined, and returns a boolean
        // indicating whether the name has any assignments other than its declaration.
        // The second return value is the first such assignment encountered in the walk, if any. It is mostly
        // useful for -m output documenting the reason for inhibited optimizations.
        // NB: global variables are always considered to be re-assigned.
        // TODO: handle initial declaration not including an assignment and followed by a single assignment?
        private static (bool, ref Node) reassigned(ref Node n)
        {
            if (n.Op != ONAME)
            {
                Fatalf("reassigned %v", n);
            } 
            // no way to reliably check for no-reassignment of globals, assume it can be
            if (n.Name.Curfn == null)
            {
                return (true, null);
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
            return (a != null, a);
        }

        private partial struct reassignVisitor
        {
            public ptr<Node> name;
        }

        private static ref Node visit(this ref reassignVisitor v, ref Node n)
        {
            if (n == null)
            {
                return null;
            }

            if (n.Op == OAS) 
                if (n.Left == v.name && n != v.name.Name.Defn)
                {
                    return n;
                }
                return null;
            else if (n.Op == OAS2 || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2DOTTYPE) 
                foreach (var (_, p) in n.List.Slice())
                {
                    if (p == v.name && n != v.name.Name.Defn)
                    {
                        return n;
                    }
                }
                return null;
                        {
                var a__prev1 = a;

                var a = v.visit(n.Left);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            {
                var a__prev1 = a;

                a = v.visit(n.Right);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            {
                var a__prev1 = a;

                a = v.visitList(n.List);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            {
                var a__prev1 = a;

                a = v.visitList(n.Rlist);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            {
                var a__prev1 = a;

                a = v.visitList(n.Ninit);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            {
                var a__prev1 = a;

                a = v.visitList(n.Nbody);

                if (a != null)
                {
                    return a;
                }

                a = a__prev1;

            }
            return null;
        }

        private static ref Node visitList(this ref reassignVisitor v, Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                {
                    var a = v.visit(n);

                    if (a != null)
                    {
                        return a;
                    }

                }
            }
            return null;
        }

        // The result of mkinlcall MUST be assigned back to n, e.g.
        //     n.Left = mkinlcall(n.Left, fn, isddd)
        private static ref Node mkinlcall(ref Node n, ref Node fn, bool isddd)
        {
            var save_safemode = safemode; 

            // imported functions may refer to unsafe as long as the
            // package was marked safe during import (already checked).
            var pkg = fnpkg(fn);

            if (pkg != localpkg && pkg != null)
            {
                safemode = false;
            }
            n = mkinlcall1(n, fn, isddd);
            safemode = save_safemode;
            return n;
        }

        private static ref Node tinlvar(ref types.Field t, map<ref Node, ref Node> inlvars)
        {
            if (asNode(t.Nname) != null && !isblank(asNode(t.Nname)))
            {
                var inlvar = inlvars[asNode(t.Nname)];
                if (inlvar == null)
                {
                    Fatalf("missing inlvar for %v\n", asNode(t.Nname));
                }
                return inlvar;
            }
            return typecheck(nblank, Erv | Easgn);
        }

        private static long inlgen = default;

        // If n is a call, and fn is a function with an inlinable body,
        // return an OINLCALL.
        // On return ninit has the parameter assignments, the nbody is the
        // inlined function body and list, rlist contain the input, output
        // parameters.
        // The result of mkinlcall1 MUST be assigned back to n, e.g.
        //     n.Left = mkinlcall1(n.Left, fn, isddd)
        private static ref Node mkinlcall1(ref Node n, ref Node fn, bool isddd)
        {
            if (fn.Func.Inl.Len() == 0L)
            { 
                // No inlinable body.
                return n;
            }
            if (fn == Curfn || fn.Name.Defn == Curfn)
            { 
                // Can't recursively inline a function into itself.
                return n;
            }
            if (Debug_typecheckinl == 0L)
            {
                typecheckinl(fn);
            } 

            // We have a function node, and it has an inlineable body.
            if (Debug['m'] > 1L)
            {
                fmt.Printf("%v: inlining call to %v %#v { %#v }\n", n.Line(), fn.Sym, fn.Type, fn.Func.Inl);
            }
            else if (Debug['m'] != 0L)
            {
                fmt.Printf("%v: inlining call to %v\n", n.Line(), fn);
            }
            if (Debug['m'] > 2L)
            {
                fmt.Printf("%v: Before inlining: %+v\n", n.Line(), n);
            }
            var ninit = n.Ninit; 

            // Make temp names to use instead of the originals.
            var inlvars = make_map<ref Node, ref Node>(); 

            // record formals/locals for later post-processing
            slice<ref Node> inlfvars = default; 

            // Find declarations corresponding to inlineable body.
            slice<ref Node> dcl = default;
            if (fn.Name.Defn != null)
            {
                dcl = fn.Func.Inldcl.Slice(); // local function

                // handle captured variables when inlining closures
                {
                    var c = fn.Name.Defn.Func.Closure;

                    if (c != null)
                    {
                        {
                            var v__prev1 = v;

                            foreach (var (_, __v) in c.Func.Cvars.Slice())
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
                                    var iv = typecheck(inlvar(v), Erv);
                                    ninit.Append(nod(ODCL, iv, null));
                                    ninit.Append(typecheck(nod(OAS, iv, o), Etop));
                                    inlvars[v] = iv;
                                }
                                else
                                {
                                    var addr = newname(lookup("&" + v.Sym.Name));
                                    addr.Type = types.NewPtr(v.Type);
                                    var ia = typecheck(inlvar(addr), Erv);
                                    ninit.Append(nod(ODCL, ia, null));
                                    ninit.Append(typecheck(nod(OAS, ia, nod(OADDR, o, null)), Etop));
                                    inlvars[addr] = ia; 

                                    // When capturing by reference, all occurrence of the captured var
                                    // must be substituted with dereference of the temporary address
                                    inlvars[v] = typecheck(nod(OIND, ia, null), Erv);
                                }
                            }

                            v = v__prev1;
                        }

                    }
            else

                }
            }            {
                dcl = fn.Func.Dcl; // imported function
            }
            foreach (var (_, ln) in dcl)
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
                inlvars[ln] = typecheck(inlvar(ln), Erv);
                if (ln.Class() == PPARAM || ln.Name.Param.Stackcopy != null && ln.Name.Param.Stackcopy.Class() == PPARAM)
                {
                    ninit.Append(nod(ODCL, inlvars[ln], null));
                }
                if (genDwarfInline > 0L)
                {
                    var inlf = inlvars[ln];
                    if (ln.Class() == PPARAM)
                    {
                        inlf.SetInlFormal(true);
                    }
                    else
                    {
                        inlf.SetInlLocal(true);
                    }
                    inlf.Pos = ln.Pos;
                    inlfvars = append(inlfvars, inlf);
                }
            } 

            // temporaries for return values.
            slice<ref Node> retvars = default;
            foreach (var (i, t) in fn.Type.Results().Fields().Slice())
            {
                ref Node m = default;
                src.XPos mpos = default;
                if (t != null && asNode(t.Nname) != null && !isblank(asNode(t.Nname)))
                {
                    mpos = asNode(t.Nname).Pos;
                    m = inlvar(asNode(t.Nname));
                    m = typecheck(m, Erv);
                    inlvars[asNode(t.Nname)] = m;
                }
                else
                { 
                    // anonymous return values, synthesize names for use in assignment that replaces return
                    m = retvar(t, i);
                }
                if (genDwarfInline > 0L)
                { 
                    // Don't update the src.Pos on a return variable if it
                    // was manufactured by the inliner (e.g. "~R2"); such vars
                    // were not part of the original callee.
                    if (!strings.HasPrefix(m.Sym.Name, "~R"))
                    {
                        m.SetInlFormal(true);
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
            ref Node vas = default;

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
                    var ras = nod(OAS, tinlvar(rcv, inlvars), n.Left.Left);
                    ras = typecheck(ras, Etop);
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
                    @as.List.Append(tinlvar(rcv, inlvars));
                }
            }
            foreach (var (_, param) in fn.Type.Params().Fields().Slice())
            { 
                // For ordinary parameters or variadic parameters in
                // dotted calls, just add the variable to the
                // assignment list, and we're done.
                if (!param.Isddd() || isddd)
                {
                    @as.List.Append(tinlvar(param, inlvars));
                    continue;
                } 

                // Otherwise, we need to collect the remaining values
                // to pass as a slice.
                var numvals = n.List.Len();
                if (numvals == 1L && n.List.First().Type.IsFuncArgStruct())
                {
                    numvals = n.List.First().Type.NumFields();
                }
                var x = @as.List.Len();
                while (@as.List.Len() < numvals)
                {
                    @as.List.Append(argvar(param.Type, @as.List.Len()));
                }

                var varargs = @as.List.Slice()[x..];

                vas = nod(OAS, tinlvar(param, inlvars), null);
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
                as = typecheck(as, Etop);
                ninit.Append(as);
            }
            if (vas != null)
            {
                vas = typecheck(vas, Etop);
                ninit.Append(vas);
            } 

            // Zero the return parameters.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in retvars)
                {
                    n = __n;
                    ras = nod(OAS, n, null);
                    ras = typecheck(ras, Etop);
                    ninit.Append(ras);
                }

                n = n__prev1;
            }

            var retlabel = autolabel(".i");
            retlabel.Etype = 1L; // flag 'safe' for escape analysis (no backjumps)

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

            if (genDwarfInline > 0L)
            {
                if (!fn.Sym.Linksym().WasInlined())
                {
                    Ctxt.DwFixups.SetPrecursorFunc(fn.Sym.Linksym(), fn);
                    fn.Sym.Linksym().Set(obj.AttrWasInlined, true);
                }
            }
            inlsubst subst = new inlsubst(retlabel:retlabel,retvars:retvars,inlvars:inlvars,bases:make(map[*src.PosBase]*src.PosBase),newInlIndex:newIndex,);

            var body = subst.list(fn.Func.Inl);

            var lab = nod(OLABEL, retlabel, null);
            body = append(body, lab);

            typecheckslice(body, Etop);

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
            inlnodelist(call.Nbody);
            {
                var n__prev1 = n;

                foreach (var (_, __n) in call.Nbody.Slice())
                {
                    n = __n;
                    if (n.Op == OINLCALL)
                    {
                        inlconv2stmt(n);
                    }
                }

                n = n__prev1;
            }

            if (Debug['m'] > 2L)
            {
                fmt.Printf("%v: After inlining %+v\n\n", call.Line(), call);
            }
            return call;
        }

        // Every time we expand a function we generate a new set of tmpnames,
        // PAUTO's in the calling functions, and link them off of the
        // PPARAM's, PAUTOS and PPARAMOUTs of the called function.
        private static ref Node inlvar(ref Node var_)
        {
            if (Debug['m'] > 3L)
            {
                fmt.Printf("inlvar %+v\n", var_);
            }
            var n = newname(var_.Sym);
            n.Type = var_.Type;
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            n.SetAddrtaken(var_.Addrtaken());

            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return n;
        }

        // Synthesize a variable to store the inlined function's results in.
        private static ref Node retvar(ref types.Field t, long i)
        {
            var n = newname(lookupN("~R", i));
            n.Type = t.Type;
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return n;
        }

        // Synthesize a variable to store the inlined function's arguments
        // when they come from a multiple return call.
        private static ref Node argvar(ref types.Type t, long i)
        {
            var n = newname(lookupN("~arg", i));
            n.Type = t.Elem();
            n.SetClass(PAUTO);
            n.Name.SetUsed(true);
            n.Name.Curfn = Curfn; // the calling function, not the called one
            Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);
            return n;
        }

        // The inlsubst type implements the actual inlining of a single
        // function call.
        private partial struct inlsubst
        {
            public ptr<Node> retlabel; // Temporary result variables.
            public slice<ref Node> retvars;
            public map<ref Node, ref Node> inlvars; // bases maps from original PosBase to PosBase with an extra
// inlined call frame.
            public map<ref src.PosBase, ref src.PosBase> bases; // newInlIndex is the index of the inlined call frame to
// insert for inlined nodes.
            public long newInlIndex;
        }

        // list inlines a list of nodes.
        private static slice<ref Node> list(this ref inlsubst subst, Nodes ll)
        {
            var s = make_slice<ref Node>(0L, ll.Len());
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
        private static ref Node node(this ref inlsubst subst, ref Node n)
        {
            if (n == null)
            {
                return null;
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
                        return inlvar;
                    }

                }

                if (Debug['m'] > 2L)
                {
                    fmt.Printf("not substituting name %+v\n", n);
                }
                return n;
            else if (n.Op == OLITERAL || n.Op == OTYPE) 
                // If n is a named constant or type, we can continue
                // using it in the inline copy. Otherwise, make a copy
                // so we can update the line number.
                if (n.Sym != null)
                {
                    return n;
                } 

                // Since we don't handle bodies with closures, this return is guaranteed to belong to the current inlined function.

                //        dump("Return before substitution", n);
            else if (n.Op == ORETURN) 
                var m = nod(OGOTO, subst.retlabel, null);
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
                    as = typecheck(as, Etop);
                    m.Ninit.Append(as);
                }
                typecheckslice(m.Ninit.Slice(), Etop);
                m = typecheck(m, Etop); 

                //        dump("Return after substitution", m);
                return m;
            else if (n.Op == OGOTO || n.Op == OLABEL) 
                m = nod(OXXX, null, null);
                m.Value = n.Value;
                m.Pos = subst.updatedPos(m.Pos);
                m.Ninit.Set(null);
                var p = fmt.Sprintf("%s·%d", n.Left.Sym.Name, inlgen);
                m.Left = newname(lookup(p));

                return m;
                        m = nod(OXXX, null, null);
            m.Value = n.Value;
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

            return m;
        }

        private static src.XPos updatedPos(this ref inlsubst subst, src.XPos xpos)
        {
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
    }
}}}}
