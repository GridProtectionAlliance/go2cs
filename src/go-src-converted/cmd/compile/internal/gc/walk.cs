// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:43:50 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\walk.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using binary = go.encoding.binary_package;
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
        // The constant is known to runtime.
        private static readonly long tmpstringbufsize = (long)32L;

        private static readonly long zeroValSize = (long)1024L; // must match value of runtime/map.go:maxZero

 // must match value of runtime/map.go:maxZero

        private static void walk(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            Curfn = fn;

            if (Debug['W'] != 0L)
            {
                var s = fmt.Sprintf("\nbefore walk %v", Curfn.Func.Nname.Sym);
                dumplist(s, Curfn.Nbody);
            }

            var lno = lineno; 

            // Final typecheck for any unused variables.
            {
                var ln__prev1 = ln;

                foreach (var (__i, __ln) in fn.Func.Dcl)
                {
                    i = __i;
                    ln = __ln;
                    if (ln.Op == ONAME && (ln.Class() == PAUTO || ln.Class() == PAUTOHEAP))
                    {
                        ln = typecheck(ln, ctxExpr | ctxAssign);
                        fn.Func.Dcl[i] = ln;
                    }

                } 

                // Propagate the used flag for typeswitch variables up to the NONAME in its definition.

                ln = ln__prev1;
            }

            {
                var ln__prev1 = ln;

                foreach (var (_, __ln) in fn.Func.Dcl)
                {
                    ln = __ln;
                    if (ln.Op == ONAME && (ln.Class() == PAUTO || ln.Class() == PAUTOHEAP) && ln.Name.Defn != null && ln.Name.Defn.Op == OTYPESW && ln.Name.Used())
                    {
                        ln.Name.Defn.Left.Name.SetUsed(true);
                    }

                }

                ln = ln__prev1;
            }

            {
                var ln__prev1 = ln;

                foreach (var (_, __ln) in fn.Func.Dcl)
                {
                    ln = __ln;
                    if (ln.Op != ONAME || (ln.Class() != PAUTO && ln.Class() != PAUTOHEAP) || ln.Sym.Name[0L] == '&' || ln.Name.Used())
                    {
                        continue;
                    }

                    {
                        var defn = ln.Name.Defn;

                        if (defn != null && defn.Op == OTYPESW)
                        {
                            if (defn.Left.Name.Used())
                            {
                                continue;
                            }

                            yyerrorl(defn.Left.Pos, "%v declared but not used", ln.Sym);
                            defn.Left.Name.SetUsed(true); // suppress repeats
                        }
                        else
                        {
                            yyerrorl(ln.Pos, "%v declared but not used", ln.Sym);
                        }

                    }

                }

                ln = ln__prev1;
            }

            lineno = lno;
            if (nerrors != 0L)
            {
                return ;
            }

            walkstmtlist(Curfn.Nbody.Slice());
            if (Debug['W'] != 0L)
            {
                s = fmt.Sprintf("after walk %v", Curfn.Func.Nname.Sym);
                dumplist(s, Curfn.Nbody);
            }

            zeroResults();
            heapmoves();
            if (Debug['W'] != 0L && Curfn.Func.Enter.Len() > 0L)
            {
                s = fmt.Sprintf("enter %v", Curfn.Func.Nname.Sym);
                dumplist(s, Curfn.Func.Enter);
            }

        }

        private static void walkstmtlist(slice<ptr<Node>> s)
        {
            foreach (var (i) in s)
            {
                s[i] = walkstmt(_addr_s[i]);
            }

        }

        private static bool paramoutheap(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            foreach (var (_, ln) in fn.Func.Dcl)
            {

                if (ln.Class() == PPARAMOUT) 
                    if (ln.isParamStackCopy() || ln.Name.Addrtaken())
                    {
                        return true;
                    }

                else if (ln.Class() == PAUTO) 
                    // stop early - parameters are over
                    return false;
                
            }
            return false;

        }

        // The result of walkstmt MUST be assigned back to n, e.g.
        //     n.Left = walkstmt(n.Left)
        private static ptr<Node> walkstmt(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_n!;
            }

            setlineno(n);

            walkstmtlist(n.Ninit.Slice());


            if (n.Op == OAS || n.Op == OASOP || n.Op == OAS2 || n.Op == OAS2DOTTYPE || n.Op == OAS2RECV || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OCLOSE || n.Op == OCOPY || n.Op == OCALLMETH || n.Op == OCALLINTER || n.Op == OCALL || n.Op == OCALLFUNC || n.Op == ODELETE || n.Op == OSEND || n.Op == OPRINT || n.Op == OPRINTN || n.Op == OPANIC || n.Op == OEMPTY || n.Op == ORECOVER || n.Op == OGETG)
            {
                if (n.Typecheck() == 0L)
                {
                    Fatalf("missing typecheck: %+v", n);
                }

                var wascopy = n.Op == OCOPY;
                ref var init = ref heap(n.Ninit, out ptr<var> _addr_init);
                n.Ninit.Set(null);
                n = walkexpr(_addr_n, _addr_init);
                n = addinit(n, init.Slice());
                if (wascopy && n.Op == OCONVNOP)
                {
                    n.Op = OEMPTY; // don't leave plain values as statements.
                } 

                // special case for a receive where we throw away
                // the value received.
                goto __switch_break0;
            }
            if (n.Op == ORECV)
            {
                if (n.Typecheck() == 0L)
                {
                    Fatalf("missing typecheck: %+v", n);
                }

                init = n.Ninit;
                n.Ninit.Set(null);

                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n = mkcall1(_addr_chanfn("chanrecv1", 2L, _addr_n.Left.Type), _addr_null, _addr_init, _addr_n.Left, nodnil());
                n = walkexpr(_addr_n, _addr_init);

                n = addinit(n, init.Slice());
                goto __switch_break0;
            }
            if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == OFALL || n.Op == OGOTO || n.Op == OLABEL || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OCHECKNIL || n.Op == OVARDEF || n.Op == OVARKILL || n.Op == OVARLIVE)
            {
                break;
                goto __switch_break0;
            }
            if (n.Op == ODCL)
            {
                var v = n.Left;
                if (v.Class() == PAUTOHEAP)
                {
                    if (compiling_runtime)
                    {
                        yyerror("%v escapes to heap, not allowed in runtime", v);
                    }

                    if (prealloc[v] == null)
                    {
                        prealloc[v] = callnew(_addr_v.Type);
                    }

                    var nn = nod(OAS, v.Name.Param.Heapaddr, prealloc[v]);
                    nn.SetColas(true);
                    nn = typecheck(nn, ctxStmt);
                    return _addr_walkstmt(_addr_nn)!;

                }

                goto __switch_break0;
            }
            if (n.Op == OBLOCK)
            {
                walkstmtlist(n.List.Slice());
                goto __switch_break0;
            }
            if (n.Op == OCASE)
            {
                yyerror("case statement out of place");
                goto __switch_break0;
            }
            if (n.Op == ODEFER)
            {
                Curfn.Func.SetHasDefer(true);
                Curfn.Func.numDefers++;
                if (Curfn.Func.numDefers > maxOpenDefers)
                { 
                    // Don't allow open-coded defers if there are more than
                    // 8 defers in the function, since we use a single
                    // byte to record active defers.
                    Curfn.Func.SetOpenCodedDeferDisallowed(true);

                }

                if (n.Esc != EscNever)
                { 
                    // If n.Esc is not EscNever, then this defer occurs in a loop,
                    // so open-coded defers cannot be used in this function.
                    Curfn.Func.SetOpenCodedDeferDisallowed(true);

                }

                fallthrough = true;
            }
            if (fallthrough || n.Op == OGO)
            {

                if (n.Left.Op == OPRINT || n.Left.Op == OPRINTN) 
                    n.Left = wrapCall(_addr_n.Left, _addr_n.Ninit);
                else if (n.Left.Op == ODELETE) 
                    if (mapfast(_addr_n.Left.List.First().Type) == mapslow)
                    {
                        n.Left = wrapCall(_addr_n.Left, _addr_n.Ninit);
                    }
                    else
                    {
                        n.Left = walkexpr(_addr_n.Left, _addr_n.Ninit);
                    }

                else if (n.Left.Op == OCOPY) 
                    n.Left = copyany(_addr_n.Left, _addr_n.Ninit, true);
                else 
                    n.Left = walkexpr(_addr_n.Left, _addr_n.Ninit);
                                goto __switch_break0;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL)
            {
                if (n.Left != null)
                {
                    walkstmtlist(n.Left.Ninit.Slice());
                    init = n.Left.Ninit;
                    n.Left.Ninit.Set(null);
                    n.Left = walkexpr(_addr_n.Left, _addr_init);
                    n.Left = addinit(n.Left, init.Slice());
                }

                n.Right = walkstmt(_addr_n.Right);
                if (n.Op == OFORUNTIL)
                {
                    walkstmtlist(n.List.Slice());
                }

                walkstmtlist(n.Nbody.Slice());
                goto __switch_break0;
            }
            if (n.Op == OIF)
            {
                n.Left = walkexpr(_addr_n.Left, _addr_n.Ninit);
                walkstmtlist(n.Nbody.Slice());
                walkstmtlist(n.Rlist.Slice());
                goto __switch_break0;
            }
            if (n.Op == ORETURN)
            {
                Curfn.Func.numReturns++;
                if (n.List.Len() == 0L)
                {
                    break;
                }

                if ((Curfn.Type.FuncType().Outnamed && n.List.Len() > 1L) || paramoutheap(_addr_Curfn))
                { 
                    // assign to the function out parameters,
                    // so that reorder3 can fix up conflicts
                    slice<ptr<Node>> rl = default;

                    foreach (var (_, ln) in Curfn.Func.Dcl)
                    {
                        var cl = ln.Class();
                        if (cl == PAUTO || cl == PAUTOHEAP)
                        {
                            break;
                        }

                        if (cl == PPARAMOUT)
                        {
                            if (ln.isParamStackCopy())
                            {
                                ln = walkexpr(_addr_typecheck(nod(ODEREF, ln.Name.Param.Heapaddr, null), ctxExpr), _addr_null);
                            }

                            rl = append(rl, ln);

                        }

                    }
                    {
                        var got = n.List.Len();
                        var want = len(rl);

                        if (got != want)
                        { 
                            // order should have rewritten multi-value function calls
                            // with explicit OAS2FUNC nodes.
                            Fatalf("expected %v return arguments, have %v", want, got);

                        } 

                        // move function calls out, to make reorder3's job easier.

                    } 

                    // move function calls out, to make reorder3's job easier.
                    walkexprlistsafe(n.List.Slice(), _addr_n.Ninit);

                    var ll = ascompatee(n.Op, rl, n.List.Slice(), _addr_n.Ninit);
                    n.List.Set(reorder3(ll));
                    break;

                }

                walkexprlist(n.List.Slice(), _addr_n.Ninit); 

                // For each return parameter (lhs), assign the corresponding result (rhs).
                var lhs = Curfn.Type.Results();
                var rhs = n.List.Slice();
                var res = make_slice<ptr<Node>>(lhs.NumFields());
                foreach (var (i, nl) in lhs.FieldSlice())
                {
                    var nname = asNode(nl.Nname);
                    if (nname.isParamHeapCopy())
                    {
                        nname = nname.Name.Param.Stackcopy;
                    }

                    var a = nod(OAS, nname, rhs[i]);
                    res[i] = convas(_addr_a, _addr_n.Ninit);

                }
                n.List.Set(res);
                goto __switch_break0;
            }
            if (n.Op == ORETJMP)
            {
                break;
                goto __switch_break0;
            }
            if (n.Op == OINLMARK)
            {
                break;
                goto __switch_break0;
            }
            if (n.Op == OSELECT)
            {
                walkselect(n);
                goto __switch_break0;
            }
            if (n.Op == OSWITCH)
            {
                walkswitch(n);
                goto __switch_break0;
            }
            if (n.Op == ORANGE)
            {
                n = walkrange(n);
                goto __switch_break0;
            }
            // default: 
                if (n.Op == ONAME)
                {
                    yyerror("%v is not a top level statement", n.Sym);
                }
                else
                {
                    yyerror("%v is not a top level statement", n.Op);
                }

                Dump("nottop", n);

            __switch_break0:;

            if (n.Op == ONAME)
            {
                Fatalf("walkstmt ended up with name: %+v", n);
            }

            return _addr_n!;

        }

        private static bool isSmallMakeSlice(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OMAKESLICE)
            {
                return false;
            }

            var r = n.Right;
            if (r == null)
            {
                r = n.Left;
            }

            var t = n.Type;

            return smallintconst(r) && (t.Elem().Width == 0L || r.Int64() < maxImplicitStackVarSize / t.Elem().Width);

        }

        // walk the whole tree of the body of an
        // expression or simple statement.
        // the types expressions are calculated.
        // compile-time constants are evaluated.
        // complex side effects like statements are appended to init
        private static void walkexprlist(slice<ptr<Node>> s, ptr<Nodes> _addr_init)
        {
            ref Nodes init = ref _addr_init.val;

            foreach (var (i) in s)
            {
                s[i] = walkexpr(_addr_s[i], _addr_init);
            }

        }

        private static void walkexprlistsafe(slice<ptr<Node>> s, ptr<Nodes> _addr_init)
        {
            ref Nodes init = ref _addr_init.val;

            foreach (var (i, n) in s)
            {
                s[i] = safeexpr(n, init);
                s[i] = walkexpr(_addr_s[i], _addr_init);
            }

        }

        private static void walkexprlistcheap(slice<ptr<Node>> s, ptr<Nodes> _addr_init)
        {
            ref Nodes init = ref _addr_init.val;

            foreach (var (i, n) in s)
            {
                s[i] = cheapexpr(n, init);
                s[i] = walkexpr(_addr_s[i], _addr_init);
            }

        }

        // convFuncName builds the runtime function name for interface conversion.
        // It also reports whether the function expects the data by address.
        // Not all names are possible. For example, we never generate convE2E or convE2I.
        private static (@string, bool) convFuncName(ptr<types.Type> _addr_from, ptr<types.Type> _addr_to) => func((_, panic, __) =>
        {
            @string fnname = default;
            bool needsaddr = default;
            ref types.Type from = ref _addr_from.val;
            ref types.Type to = ref _addr_to.val;

            var tkind = to.Tie();
            switch (from.Tie())
            {
                case 'I': 
                    if (tkind == 'I')
                    {
                        return ("convI2I", false);
                    }

                    break;
                case 'T': 

                    if (from.Size() == 2L && from.Align == 2L) 
                        return ("convT16", false);
                    else if (from.Size() == 4L && from.Align == 4L && !types.Haspointers(from)) 
                        return ("convT32", false);
                    else if (from.Size() == 8L && from.Align == types.Types[TUINT64].Align && !types.Haspointers(from)) 
                        return ("convT64", false);
                                    {
                        var sc = from.SoleComponent();

                        if (sc != null)
                        {

                            if (sc.IsString()) 
                                return ("convTstring", false);
                            else if (sc.IsSlice()) 
                                return ("convTslice", false);

                        }

                    }


                    switch (tkind)
                    {
                        case 'E': 
                            if (!types.Haspointers(from))
                            {
                                return ("convT2Enoptr", true);
                            }

                            return ("convT2E", true);
                            break;
                        case 'I': 
                            if (!types.Haspointers(from))
                            {
                                return ("convT2Inoptr", true);
                            }

                            return ("convT2I", true);
                            break;
                    }
                    break;
            }
            Fatalf("unknown conv func %c2%c", from.Tie(), to.Tie());
            panic("unreachable");

        });

        // The result of walkexpr MUST be assigned back to n, e.g.
        //     n.Left = walkexpr(n.Left, init)
        private static ptr<Node> walkexpr(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n == null)
            {
                return _addr_n!;
            } 

            // Eagerly checkwidth all expressions for the back end.
            if (n.Type != null && !n.Type.WidthCalculated())
            {

                if (n.Type.Etype == TBLANK || n.Type.Etype == TNIL || n.Type.Etype == TIDEAL)                 else 
                    checkwidth(n.Type);
                
            }

            if (init == _addr_n.Ninit)
            { 
                // not okay to use n->ninit when walking n,
                // because we might replace n with some other node
                // and would lose the init list.
                Fatalf("walkexpr init == &n->ninit");

            }

            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(_addr_n.Ninit);
            }

            var lno = setlineno(n);

            if (Debug['w'] > 1L)
            {
                Dump("before walk expr", n);
            }

            if (n.Typecheck() != 1L)
            {
                Fatalf("missed typecheck: %+v", n);
            }

            if (n.Type.IsUntyped())
            {
                Fatalf("expression has untyped type: %+v", n);
            }

            if (n.Op == ONAME && n.Class() == PAUTOHEAP)
            {
                var nn = nod(ODEREF, n.Name.Param.Heapaddr, null);
                nn = typecheck(nn, ctxExpr);
                nn = walkexpr(_addr_nn, _addr_init);
                nn.Left.MarkNonNil();
                return _addr_nn!;
            }

opswitch: 

            // Expressions that are constant at run time but not
            // considered const by the language spec are not turned into
            // constants until walk. For example, if n is y%1 == 0, the
            // walk of y%1 may have replaced it by 0.
            // Check whether n with its updated args is itself now a constant.

            if (n.Op == ONONAME || n.Op == OEMPTY || n.Op == OGETG || n.Op == ONEWOBJ)             else if (n.Op == OTYPE || n.Op == ONAME || n.Op == OLITERAL)             else if (n.Op == ONOT || n.Op == ONEG || n.Op == OPLUS || n.Op == OBITNOT || n.Op == OREAL || n.Op == OIMAG || n.Op == ODOTMETH || n.Op == ODOTINTER || n.Op == ODEREF || n.Op == OSPTR || n.Op == OITAB || n.Op == OIDATA || n.Op == OADDR) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
            else if (n.Op == OEFACE || n.Op == OAND || n.Op == OSUB || n.Op == OMUL || n.Op == OADD || n.Op == OOR || n.Op == OXOR || n.Op == OLSH || n.Op == ORSH) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Right = walkexpr(_addr_n.Right, _addr_init);
            else if (n.Op == ODOT || n.Op == ODOTPTR) 
                usefield(_addr_n);
                n.Left = walkexpr(_addr_n.Left, _addr_init);
            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                n.Left = walkexpr(_addr_n.Left, _addr_init); 
                // Set up interface type addresses for back end.
                n.Right = typename(n.Type);
                if (n.Op == ODOTTYPE)
                {
                    n.Right.Right = typename(n.Left.Type);
                }

                if (!n.Type.IsInterface() && !n.Left.Type.IsEmptyInterface())
                {
                    n.List.Set1(itabname(n.Type, n.Left.Type));
                }

            else if (n.Op == OLEN || n.Op == OCAP) 
                if (isRuneCount(_addr_n))
                { 
                    // Replace len([]rune(string)) with runtime.countrunes(string).
                    n = mkcall("countrunes", _addr_n.Type, _addr_init, _addr_conv(_addr_n.Left.Left, _addr_types.Types[TSTRING]));
                    break;

                }

                n.Left = walkexpr(_addr_n.Left, _addr_init); 

                // replace len(*[10]int) with 10.
                // delayed until now to preserve side effects.
                var t = n.Left.Type;

                if (t.IsPtr())
                {
                    t = t.Elem();
                }

                if (t.IsArray())
                {
                    safeexpr(n.Left, init);
                    setintconst(n, t.NumElem());
                    n.SetTypecheck(1L);
                }

            else if (n.Op == OCOMPLEX) 
                // Use results from call expression as arguments for complex.
                if (n.Left == null && n.Right == null)
                {
                    n.Left = n.List.First();
                    n.Right = n.List.Second();
                }

                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Right = walkexpr(_addr_n.Right, _addr_init);
            else if (n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE) 
                n = walkcompare(_addr_n, _addr_init);
            else if (n.Op == OANDAND || n.Op == OOROR) 
                n.Left = walkexpr(_addr_n.Left, _addr_init); 

                // cannot put side effects from n.Right on init,
                // because they cannot run before n.Left is checked.
                // save elsewhere and store on the eventual n.Right.
                ref Nodes ll = ref heap(out ptr<Nodes> _addr_ll);

                n.Right = walkexpr(_addr_n.Right, _addr_ll);
                n.Right = addinit(n.Right, ll.Slice());
            else if (n.Op == OPRINT || n.Op == OPRINTN) 
                n = walkprint(_addr_n, _addr_init);
            else if (n.Op == OPANIC) 
                n = mkcall("gopanic", _addr_null, _addr_init, _addr_n.Left);
            else if (n.Op == ORECOVER) 
                n = mkcall("gorecover", _addr_n.Type, _addr_init, _addr_nod(OADDR, nodfp, null));
            else if (n.Op == OCLOSUREVAR || n.Op == OCFUNC)             else if (n.Op == OCALLINTER || n.Op == OCALLFUNC || n.Op == OCALLMETH) 
                if (n.Op == OCALLINTER)
                {
                    usemethod(_addr_n);
                }

                if (n.Op == OCALLFUNC && n.Left.Op == OCLOSURE)
                { 
                    // Transform direct call of a closure to call of a normal function.
                    // transformclosure already did all preparation work.

                    // Prepend captured variables to argument list.
                    n.List.Prepend(n.Left.Func.Enter.Slice());

                    n.Left.Func.Enter.Set(null); 

                    // Replace OCLOSURE with ONAME/PFUNC.
                    n.Left = n.Left.Func.Closure.Func.Nname; 

                    // Update type of OCALLFUNC node.
                    // Output arguments had not changed, but their offsets could.
                    if (n.Left.Type.NumResults() == 1L)
                    {
                        n.Type = n.Left.Type.Results().Field(0L).Type;
                    }
                    else
                    {
                        n.Type = n.Left.Type.Results();
                    }

                }

                walkCall(_addr_n, _addr_init);
            else if (n.Op == OAS || n.Op == OASOP) 
                init.AppendNodes(_addr_n.Ninit); 

                // Recognize m[k] = append(m[k], ...) so we can reuse
                // the mapassign call.
                var mapAppend = n.Left.Op == OINDEXMAP && n.Right.Op == OAPPEND;
                if (mapAppend && !samesafeexpr(n.Left, n.Right.List.First()))
                {
                    Fatalf("not same expressions: %v != %v", n.Left, n.Right.List.First());
                }

                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Left = safeexpr(n.Left, init);

                if (mapAppend)
                {
                    n.Right.List.SetFirst(n.Left);
                }

                if (n.Op == OASOP)
                { 
                    // Rewrite x op= y into x = x op y.
                    n.Right = nod(n.SubOp(), n.Left, n.Right);
                    n.Right = typecheck(n.Right, ctxExpr);

                    n.Op = OAS;
                    n.ResetAux();

                }

                if (oaslit(n, init))
                {
                    break;
                }

                if (n.Right == null)
                { 
                    // TODO(austin): Check all "implicit zeroing"
                    break;

                }

                if (!instrumenting && isZero(n.Right))
                {
                    break;
                }


                if (n.Right.Op == ORECV) 
                    // x = <-c; n.Left is x, n.Right.Left is c.
                    // order.stmt made sure x is addressable.
                    n.Right.Left = walkexpr(_addr_n.Right.Left, _addr_init);

                    var n1 = nod(OADDR, n.Left, null);
                    var r = n.Right.Left; // the channel
                    n = mkcall1(_addr_chanfn("chanrecv1", 2L, _addr_r.Type), _addr_null, _addr_init, _addr_r, n1);
                    n = walkexpr(_addr_n, _addr_init);
                    _breakopswitch = true;

                    break;
                else if (n.Right.Op == OAPPEND) 
                    // x = append(...)
                    r = n.Right;
                    if (r.Type.Elem().NotInHeap())
                    {
                        yyerror("%v is go:notinheap; heap allocation disallowed", r.Type.Elem());
                    }


                    if (isAppendOfMake(_addr_r)) 
                        // x = append(y, make([]T, y)...)
                        r = extendslice(_addr_r, _addr_init);
                    else if (r.IsDDD()) 
                        r = appendslice(_addr_r, _addr_init); // also works for append(slice, string).
                    else 
                        r = walkappend(_addr_r, _addr_init, _addr_n);
                                        n.Right = r;
                    if (r.Op == OAPPEND)
                    { 
                        // Left in place for back end.
                        // Do not add a new write barrier.
                        // Set up address of type for back end.
                        r.Left = typename(r.Type.Elem());
                        _breakopswitch = true;
                        break;
                    } 
                    // Otherwise, lowered for race detector.
                    // Treat as ordinary assignment.
                else 
                    n.Right = walkexpr(_addr_n.Right, _addr_init);
                                if (n.Left != null && n.Right != null)
                {
                    n = convas(_addr_n, _addr_init);
                }

            else if (n.Op == OAS2) 
                init.AppendNodes(_addr_n.Ninit);
                walkexprlistsafe(n.List.Slice(), _addr_init);
                walkexprlistsafe(n.Rlist.Slice(), _addr_init);
                ll = ascompatee(OAS, n.List.Slice(), n.Rlist.Slice(), _addr_init);
                ll = reorder3(ll);
                n = liststmt(ll); 

                // a,b,... = fn()
            else if (n.Op == OAS2FUNC) 
                init.AppendNodes(_addr_n.Ninit);

                r = n.Right;
                walkexprlistsafe(n.List.Slice(), _addr_init);
                r = walkexpr(_addr_r, _addr_init);

                if (isIntrinsicCall(r))
                {
                    n.Right = r;
                    break;
                }

                init.Append(r);

                ll = ascompatet(n.List, _addr_r.Type);
                n = liststmt(ll); 

                // x, y = <-c
                // order.stmt made sure x is addressable or blank.
            else if (n.Op == OAS2RECV) 
                init.AppendNodes(_addr_n.Ninit);

                r = n.Right;
                walkexprlistsafe(n.List.Slice(), _addr_init);
                r.Left = walkexpr(_addr_r.Left, _addr_init);
                n1 = ;
                if (n.List.First().isBlank())
                {
                    n1 = nodnil();
                }
                else
                {
                    n1 = nod(OADDR, n.List.First(), null);
                }

                var fn = chanfn("chanrecv2", 2L, _addr_r.Left.Type);
                var ok = n.List.Second();
                var call = mkcall1(_addr_fn, _addr_types.Types[TBOOL], _addr_init, _addr_r.Left, n1);
                n = nod(OAS, ok, call);
                n = typecheck(n, ctxStmt); 

                // a,b = m[i]
            else if (n.Op == OAS2MAPR) 
                init.AppendNodes(_addr_n.Ninit);

                r = n.Right;
                walkexprlistsafe(n.List.Slice(), _addr_init);
                r.Left = walkexpr(_addr_r.Left, _addr_init);
                r.Right = walkexpr(_addr_r.Right, _addr_init);
                t = r.Left.Type;

                var fast = mapfast(_addr_t);
                ptr<Node> key;
                if (fast != mapslow)
                { 
                    // fast versions take key by value
                    key = r.Right;

                }
                else
                { 
                    // standard version takes key by reference
                    // order.expr made sure key is addressable.
                    key = nod(OADDR, r.Right, null);

                } 

                // from:
                //   a,b = m[i]
                // to:
                //   var,b = mapaccess2*(t, m, i)
                //   a = *var
                var a = n.List.First();

                {
                    var w__prev1 = w;

                    var w = t.Elem().Width;

                    if (w <= zeroValSize)
                    {
                        fn = mapfn(mapaccess2[fast], _addr_t);
                        r = mkcall1(_addr_fn, _addr_fn.Type.Results(), _addr_init, _addr_typename(t), r.Left, key);
                    }
                    else
                    {
                        fn = mapfn("mapaccess2_fat", _addr_t);
                        var z = zeroaddr(w);
                        r = mkcall1(_addr_fn, _addr_fn.Type.Results(), _addr_init, _addr_typename(t), r.Left, key, z);
                    } 

                    // mapaccess2* returns a typed bool, but due to spec changes,
                    // the boolean result of i.(T) is now untyped so we make it the
                    // same type as the variable on the lhs.

                    w = w__prev1;

                } 

                // mapaccess2* returns a typed bool, but due to spec changes,
                // the boolean result of i.(T) is now untyped so we make it the
                // same type as the variable on the lhs.
                {
                    var ok__prev1 = ok;

                    ok = n.List.Second();

                    if (!ok.isBlank() && ok.Type.IsBoolean())
                    {
                        r.Type.Field(1L).Type;

                        ok.Type;

                    }

                    ok = ok__prev1;

                }

                n.Right = r;
                n.Op = OAS2FUNC; 

                // don't generate a = *var if a is _
                if (!a.isBlank())
                {
                    var var_ = temp(types.NewPtr(t.Elem()));
                    var_.SetTypecheck(1L);
                    var_.MarkNonNil(); // mapaccess always returns a non-nil pointer
                    n.List.SetFirst(var_);
                    n = walkexpr(_addr_n, _addr_init);
                    init.Append(n);
                    n = nod(OAS, a, nod(ODEREF, var_, null));

                }

                n = typecheck(n, ctxStmt);
                n = walkexpr(_addr_n, _addr_init);
            else if (n.Op == ODELETE) 
                init.AppendNodes(_addr_n.Ninit);
                var map_ = n.List.First();
                key = n.List.Second();
                map_ = walkexpr(_addr_map_, _addr_init);
                key = walkexpr(key, _addr_init);

                t = map_.Type;
                fast = mapfast(_addr_t);
                if (fast == mapslow)
                { 
                    // order.stmt made sure key is addressable.
                    key = nod(OADDR, key, null);

                }

                n = mkcall1(_addr_mapfndel(mapdelete[fast], _addr_t), _addr_null, _addr_init, _addr_typename(t), map_, key);
            else if (n.Op == OAS2DOTTYPE) 
                walkexprlistsafe(n.List.Slice(), _addr_init);
                n.Right = walkexpr(_addr_n.Right, _addr_init);
            else if (n.Op == OCONVIFACE) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);

                var fromType = n.Left.Type;
                var toType = n.Type; 

                // typeword generates the type word of the interface value.
                Func<ptr<Node>> typeword = () =>
                {
                    if (toType.IsEmptyInterface())
                    {
                        return _addr_typename(fromType)!;
                    }

                    return _addr_itabname(fromType, toType)!;

                } 

                // Optimize convT2E or convT2I as a two-word copy when T is pointer-shaped.
; 

                // Optimize convT2E or convT2I as a two-word copy when T is pointer-shaped.
                if (isdirectiface(fromType))
                {
                    var l = nod(OEFACE, typeword(), n.Left);
                    l.Type = toType;
                    l.SetTypecheck(n.Typecheck());
                    n = l;
                    break;
                }

                if (staticuint64s == null)
                {
                    staticuint64s = newname(Runtimepkg.Lookup("staticuint64s"));
                    staticuint64s.SetClass(PEXTERN); 
                    // The actual type is [256]uint64, but we use [256*8]uint8 so we can address
                    // individual bytes.
                    staticuint64s.Type = types.NewArray(types.Types[TUINT8], 256L * 8L);
                    zerobase = newname(Runtimepkg.Lookup("zerobase"));
                    zerobase.SetClass(PEXTERN);
                    zerobase.Type = types.Types[TUINTPTR];

                } 

                // Optimize convT2{E,I} for many cases in which T is not pointer-shaped,
                // by using an existing addressable value identical to n.Left
                // or creating one on the stack.
                ptr<Node> value;

                if (fromType.Size() == 0L) 
                    // n.Left is zero-sized. Use zerobase.
                    cheapexpr(n.Left, init); // Evaluate n.Left for side-effects. See issue 19246.
                    value = zerobase;
                else if (fromType.IsBoolean() || (fromType.Size() == 1L && fromType.IsInteger())) 
                    // n.Left is a bool/byte. Use staticuint64s[n.Left * 8] on little-endian
                    // and staticuint64s[n.Left * 8 + 7] on big-endian.
                    n.Left = cheapexpr(n.Left, init); 
                    // byteindex widens n.Left so that the multiplication doesn't overflow.
                    var index = nod(OLSH, byteindex(_addr_n.Left), nodintconst(3L));
                    if (thearch.LinkArch.ByteOrder == binary.BigEndian)
                    {
                        index = nod(OADD, index, nodintconst(7L));
                    }

                    value = nod(OINDEX, staticuint64s, index);
                    value.SetBounded(true);
                else if (n.Left.Class() == PEXTERN && n.Left.Name != null && n.Left.Name.Readonly()) 
                    // n.Left is a readonly global; use it directly.
                    value = n.Left;
                else if (!fromType.IsInterface() && n.Esc == EscNone && fromType.Width <= 1024L) 
                    // n.Left does not escape. Use a stack temporary initialized to n.Left.
                    value = temp(fromType);
                    init.Append(typecheck(nod(OAS, value, n.Left), ctxStmt));
                                if (value != null)
                { 
                    // Value is identical to n.Left.
                    // Construct the interface directly: {type/itab, &value}.
                    l = nod(OEFACE, typeword(), typecheck(nod(OADDR, value, null), ctxExpr));
                    l.Type = toType;
                    l.SetTypecheck(n.Typecheck());
                    n = l;
                    break;

                } 

                // Implement interface to empty interface conversion.
                // tmp = i.itab
                // if tmp != nil {
                //    tmp = tmp.type
                // }
                // e = iface{tmp, i.data}
                if (toType.IsEmptyInterface() && fromType.IsInterface() && !fromType.IsEmptyInterface())
                { 
                    // Evaluate the input interface.
                    var c = temp(fromType);
                    init.Append(nod(OAS, c, n.Left)); 

                    // Get the itab out of the interface.
                    var tmp = temp(types.NewPtr(types.Types[TUINT8]));
                    init.Append(nod(OAS, tmp, typecheck(nod(OITAB, c, null), ctxExpr))); 

                    // Get the type out of the itab.
                    var nif = nod(OIF, typecheck(nod(ONE, tmp, nodnil()), ctxExpr), null);
                    nif.Nbody.Set1(nod(OAS, tmp, itabType(tmp)));
                    init.Append(nif); 

                    // Build the result.
                    var e = nod(OEFACE, tmp, ifaceData(n.Pos, c, types.NewPtr(types.Types[TUINT8])));
                    e.Type = toType; // assign type manually, typecheck doesn't understand OEFACE.
                    e.SetTypecheck(1L);
                    n = e;
                    break;

                }

                var (fnname, needsaddr) = convFuncName(_addr_fromType, _addr_toType);

                if (!needsaddr && !fromType.IsInterface())
                { 
                    // Use a specialized conversion routine that only returns a data pointer.
                    // ptr = convT2X(val)
                    // e = iface{typ/tab, ptr}
                    fn = syslook(fnname);
                    dowidth(fromType);
                    fn = substArgTypes(_addr_fn, _addr_fromType);
                    dowidth(fn.Type);
                    call = nod(OCALL, fn, null);
                    call.List.Set1(n.Left);
                    call = typecheck(call, ctxExpr);
                    call = walkexpr(_addr_call, _addr_init);
                    call = safeexpr(call, init);
                    e = nod(OEFACE, typeword(), call);
                    e.Type = toType;
                    e.SetTypecheck(1L);
                    n = e;
                    break;

                }

                ptr<Node> tab;
                if (fromType.IsInterface())
                { 
                    // convI2I
                    tab = typename(toType);

                }
                else
                { 
                    // convT2x
                    tab = typeword();

                }

                var v = n.Left;
                if (needsaddr)
                { 
                    // Types of large or unknown size are passed by reference.
                    // Orderexpr arranged for n.Left to be a temporary for all
                    // the conversions it could see. Comparison of an interface
                    // with a non-interface, especially in a switch on interface value
                    // with non-interface cases, is not visible to order.stmt, so we
                    // have to fall back on allocating a temp here.
                    if (!islvalue(v))
                    {
                        v = copyexpr(v, v.Type, init);
                    }

                    v = nod(OADDR, v, null);

                }

                dowidth(fromType);
                fn = syslook(fnname);
                fn = substArgTypes(_addr_fn, _addr_fromType, toType);
                dowidth(fn.Type);
                n = nod(OCALL, fn, null);
                n.List.Set2(tab, v);
                n = typecheck(n, ctxExpr);
                n = walkexpr(_addr_n, _addr_init);
            else if (n.Op == OCONV || n.Op == OCONVNOP) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                if (n.Op == OCONVNOP && checkPtr(_addr_Curfn, 1L))
                {
                    if (n.Type.IsPtr() && n.Left.Type.Etype == TUNSAFEPTR)
                    { // unsafe.Pointer to *T
                        n = walkCheckPtrAlignment(_addr_n, _addr_init, _addr_null);
                        break;

                    }

                    if (n.Type.Etype == TUNSAFEPTR && n.Left.Type.Etype == TUINTPTR)
                    { // uintptr to unsafe.Pointer
                        n = walkCheckPtrArithmetic(_addr_n, _addr_init);
                        break;

                    }

                }

                var (param, result) = rtconvfn(_addr_n.Left.Type, _addr_n.Type);
                if (param == Txxx)
                {
                    break;
                }

                fn = basicnames[param] + "to" + basicnames[result];
                n = conv(_addr_mkcall(fn, _addr_types.Types[result], _addr_init, _addr_conv(_addr_n.Left, _addr_types.Types[param])), _addr_n.Type);
            else if (n.Op == OANDNOT) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Op = OAND;
                n.SetImplicit(true); // for walkCheckPtrArithmetic
                n.Right = nod(OBITNOT, n.Right, null);
                n.Right = typecheck(n.Right, ctxExpr);
                n.Right = walkexpr(_addr_n.Right, _addr_init);
            else if (n.Op == ODIV || n.Op == OMOD) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Right = walkexpr(_addr_n.Right, _addr_init); 

                // rewrite complex div into function call.
                var et = n.Left.Type.Etype;

                if (isComplex[et] && n.Op == ODIV)
                {
                    t = n.Type;
                    n = mkcall("complex128div", _addr_types.Types[TCOMPLEX128], _addr_init, _addr_conv(_addr_n.Left, _addr_types.Types[TCOMPLEX128]), conv(_addr_n.Right, _addr_types.Types[TCOMPLEX128]));
                    n = conv(_addr_n, _addr_t);
                    break;
                } 

                // Nothing to do for float divisions.
                if (isFloat[et])
                {
                    break;
                } 

                // rewrite 64-bit div and mod on 32-bit architectures.
                // TODO: Remove this code once we can introduce
                // runtime calls late in SSA processing.
                if (Widthreg < 8L && (et == TINT64 || et == TUINT64))
                {
                    if (n.Right.Op == OLITERAL)
                    { 
                        // Leave div/mod by constant powers of 2.
                        // The SSA backend will handle those.

                        if (et == TINT64) 
                            c = n.Right.Int64();
                            if (c < 0L)
                            {
                                c = -c;
                            }

                            if (c != 0L && c & (c - 1L) == 0L)
                            {
                                _breakopswitch = true;
                                break;
                            }

                        else if (et == TUINT64) 
                            c = uint64(n.Right.Int64());
                            if (c != 0L && c & (c - 1L) == 0L)
                            {
                                _breakopswitch = true;
                                break;
                            }

                                            }

                    fn = default;
                    if (et == TINT64)
                    {
                        fn = "int64";
                    }
                    else
                    {
                        fn = "uint64";
                    }

                    if (n.Op == ODIV)
                    {
                        fn += "div";
                    }
                    else
                    {
                        fn += "mod";
                    }

                    n = mkcall(fn, _addr_n.Type, _addr_init, _addr_conv(_addr_n.Left, _addr_types.Types[et]), conv(_addr_n.Right, _addr_types.Types[et]));

                }

            else if (n.Op == OINDEX) 
                n.Left = walkexpr(_addr_n.Left, _addr_init); 

                // save the original node for bounds checking elision.
                // If it was a ODIV/OMOD walk might rewrite it.
                r = n.Right;

                n.Right = walkexpr(_addr_n.Right, _addr_init); 

                // if range of type cannot exceed static array bound,
                // disable bounds check.
                if (n.Bounded())
                {
                    break;
                }

                t = n.Left.Type;
                if (t != null && t.IsPtr())
                {
                    t = t.Elem();
                }

                if (t.IsArray())
                {
                    n.SetBounded(bounded(_addr_r, t.NumElem()));
                    if (Debug['m'] != 0L && n.Bounded() && !Isconst(n.Right, CTINT))
                    {
                        Warn("index bounds check elided");
                    }

                    if (smallintconst(n.Right) && !n.Bounded())
                    {
                        yyerror("index out of bounds");
                    }

                }
                else if (Isconst(n.Left, CTSTR))
                {
                    n.SetBounded(bounded(_addr_r, int64(len(strlit(n.Left)))));
                    if (Debug['m'] != 0L && n.Bounded() && !Isconst(n.Right, CTINT))
                    {
                        Warn("index bounds check elided");
                    }

                    if (smallintconst(n.Right) && !n.Bounded())
                    {
                        yyerror("index out of bounds");
                    }

                }

                if (Isconst(n.Right, CTINT))
                {
                    if (n.Right.Val().U._<ptr<Mpint>>().CmpInt64(0L) < 0L || n.Right.Val().U._<ptr<Mpint>>().Cmp(maxintval[TINT]) > 0L)
                    {
                        yyerror("index out of bounds");
                    }

                }

            else if (n.Op == OINDEXMAP) 
                // Replace m[k] with *map{access1,assign}(maptype, m, &k)
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.Right = walkexpr(_addr_n.Right, _addr_init);
                map_ = n.Left;
                key = n.Right;
                t = map_.Type;
                if (n.IndexMapLValue())
                { 
                    // This m[k] expression is on the left-hand side of an assignment.
                    fast = mapfast(_addr_t);
                    if (fast == mapslow)
                    { 
                        // standard version takes key by reference.
                        // order.expr made sure key is addressable.
                        key = nod(OADDR, key, null);

                    }

                    n = mkcall1(_addr_mapfn(mapassign[fast], _addr_t), _addr_null, _addr_init, _addr_typename(t), map_, key);

                }
                else
                { 
                    // m[k] is not the target of an assignment.
                    fast = mapfast(_addr_t);
                    if (fast == mapslow)
                    { 
                        // standard version takes key by reference.
                        // order.expr made sure key is addressable.
                        key = nod(OADDR, key, null);

                    }

                    {
                        var w__prev2 = w;

                        w = t.Elem().Width;

                        if (w <= zeroValSize)
                        {
                            n = mkcall1(_addr_mapfn(mapaccess1[fast], _addr_t), _addr_types.NewPtr(t.Elem()), _addr_init, _addr_typename(t), map_, key);
                        }
                        else
                        {
                            z = zeroaddr(w);
                            n = mkcall1(_addr_mapfn("mapaccess1_fat", _addr_t), _addr_types.NewPtr(t.Elem()), _addr_init, _addr_typename(t), map_, key, z);
                        }

                        w = w__prev2;

                    }

                }

                n.Type = types.NewPtr(t.Elem());
                n.MarkNonNil(); // mapaccess1* and mapassign always return non-nil pointers.
                n = nod(ODEREF, n, null);
                n.Type = t.Elem();
                n.SetTypecheck(1L);
            else if (n.Op == ORECV) 
                Fatalf("walkexpr ORECV"); // should see inside OAS only
            else if (n.Op == OSLICEHEADER) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                n.List.SetFirst(walkexpr(_addr_n.List.First(), _addr_init));
                n.List.SetSecond(walkexpr(_addr_n.List.Second(), _addr_init));
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR || n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                var checkSlice = checkPtr(_addr_Curfn, 1L) && n.Op == OSLICE3ARR && n.Left.Op == OCONVNOP && n.Left.Left.Type.Etype == TUNSAFEPTR;
                if (checkSlice)
                {
                    n.Left.Left = walkexpr(_addr_n.Left.Left, _addr_init);
                }
                else
                {
                    n.Left = walkexpr(_addr_n.Left, _addr_init);
                }

                var (low, high, max) = n.SliceBounds();
                low = walkexpr(_addr_low, _addr_init);
                if (low != null && isZero(low))
                { 
                    // Reduce x[0:j] to x[:j] and x[0:j:k] to x[:j:k].
                    low = null;

                }

                high = walkexpr(_addr_high, _addr_init);
                max = walkexpr(_addr_max, _addr_init);
                n.SetSliceBounds(low, high, max);
                if (checkSlice)
                {
                    n.Left = walkCheckPtrAlignment(_addr_n.Left, _addr_init, _addr_max);
                }

                if (n.Op.IsSlice3())
                {
                    if (max != null && max.Op == OCAP && samesafeexpr(n.Left, max.Left))
                    { 
                        // Reduce x[i:j:cap(x)] to x[i:j].
                        if (n.Op == OSLICE3)
                        {
                            n.Op = OSLICE;
                        }
                        else
                        {
                            n.Op = OSLICEARR;
                        }

                        n = reduceSlice(_addr_n);

                    }

                }
                else
                {
                    n = reduceSlice(_addr_n);
                }

            else if (n.Op == ONEW) 
                if (n.Esc == EscNone)
                {
                    if (n.Type.Elem().Width >= maxImplicitStackVarSize)
                    {
                        Fatalf("large ONEW with EscNone: %v", n);
                    }

                    r = temp(n.Type.Elem());
                    r = nod(OAS, r, null); // zero temp
                    r = typecheck(r, ctxStmt);
                    init.Append(r);
                    r = nod(OADDR, r.Left, null);
                    r = typecheck(r, ctxExpr);
                    n = r;

                }
                else
                {
                    n = callnew(_addr_n.Type.Elem());
                }

            else if (n.Op == OADDSTR) 
                n = addstr(_addr_n, _addr_init);
            else if (n.Op == OAPPEND) 
                // order should make sure we only see OAS(node, OAPPEND), which we handle above.
                Fatalf("append outside assignment");
            else if (n.Op == OCOPY) 
                n = copyany(_addr_n, _addr_init, instrumenting && !compiling_runtime); 

                // cannot use chanfn - closechan takes any, not chan any
            else if (n.Op == OCLOSE) 
                fn = syslook("closechan");

                fn = substArgTypes(_addr_fn, _addr_n.Left.Type);
                n = mkcall1(_addr_fn, _addr_null, _addr_init, _addr_n.Left);
            else if (n.Op == OMAKECHAN) 
                // When size fits into int, use makechan instead of
                // makechan64, which is faster and shorter on 32 bit platforms.
                var size = n.Left;
                @string fnname = "makechan64";
                var argtype = types.Types[TINT64]; 

                // Type checking guarantees that TIDEAL size is positive and fits in an int.
                // The case of size overflow when converting TUINT or TUINTPTR to TINT
                // will be handled by the negative range checks in makechan during runtime.
                if (size.Type.IsKind(TIDEAL) || maxintval[size.Type.Etype].Cmp(maxintval[TUINT]) <= 0L)
                {
                    fnname = "makechan";
                    argtype = types.Types[TINT];
                }

                n = mkcall1(_addr_chanfn(fnname, 1L, _addr_n.Type), _addr_n.Type, _addr_init, _addr_typename(n.Type), conv(_addr_size, _addr_argtype));
            else if (n.Op == OMAKEMAP) 
                t = n.Type;
                var hmapType = hmap(t);
                var hint = n.Left; 

                // var h *hmap
                ptr<Node> h;
                if (n.Esc == EscNone)
                { 
                    // Allocate hmap on stack.

                    // var hv hmap
                    var hv = temp(hmapType);
                    var zero = nod(OAS, hv, null);
                    zero = typecheck(zero, ctxStmt);
                    init.Append(zero); 
                    // h = &hv
                    h = nod(OADDR, hv, null); 

                    // Allocate one bucket pointed to by hmap.buckets on stack if hint
                    // is not larger than BUCKETSIZE. In case hint is larger than
                    // BUCKETSIZE runtime.makemap will allocate the buckets on the heap.
                    // Maximum key and elem size is 128 bytes, larger objects
                    // are stored with an indirection. So max bucket size is 2048+eps.
                    if (!Isconst(hint, CTINT) || hint.Val().U._<ptr<Mpint>>().CmpInt64(BUCKETSIZE) <= 0L)
                    {
                        // In case hint is larger than BUCKETSIZE runtime.makemap
                        // will allocate the buckets on the heap, see #20184
                        //
                        // if hint <= BUCKETSIZE {
                        //     var bv bmap
                        //     b = &bv
                        //     h.buckets = b
                        // }

                        nif = nod(OIF, nod(OLE, hint, nodintconst(BUCKETSIZE)), null);
                        nif.SetLikely(true); 

                        // var bv bmap
                        var bv = temp(bmap(t));
                        zero = nod(OAS, bv, null);
                        nif.Nbody.Append(zero); 

                        // b = &bv
                        var b = nod(OADDR, bv, null); 

                        // h.buckets = b
                        var bsym = hmapType.Field(5L).Sym; // hmap.buckets see reflect.go:hmap
                        var na = nod(OAS, nodSym(ODOT, h, bsym), b);
                        nif.Nbody.Append(na);

                        nif = typecheck(nif, ctxStmt);
                        nif = walkstmt(_addr_nif);
                        init.Append(nif);

                    }

                }

                if (Isconst(hint, CTINT) && hint.Val().U._<ptr<Mpint>>().CmpInt64(BUCKETSIZE) <= 0L)
                { 
                    // Handling make(map[any]any) and
                    // make(map[any]any, hint) where hint <= BUCKETSIZE
                    // special allows for faster map initialization and
                    // improves binary size by using calls with fewer arguments.
                    // For hint <= BUCKETSIZE overLoadFactor(hint, 0) is false
                    // and no buckets will be allocated by makemap. Therefore,
                    // no buckets need to be allocated in this code path.
                    if (n.Esc == EscNone)
                    { 
                        // Only need to initialize h.hash0 since
                        // hmap h has been allocated on the stack already.
                        // h.hash0 = fastrand()
                        var rand = mkcall("fastrand", _addr_types.Types[TUINT32], _addr_init);
                        var hashsym = hmapType.Field(4L).Sym; // hmap.hash0 see reflect.go:hmap
                        a = nod(OAS, nodSym(ODOT, h, hashsym), rand);
                        a = typecheck(a, ctxStmt);
                        a = walkexpr(_addr_a, _addr_init);
                        init.Append(a);
                        n = convnop(h, _addr_t);

                    }
                    else
                    { 
                        // Call runtime.makehmap to allocate an
                        // hmap on the heap and initialize hmap's hash0 field.
                        fn = syslook("makemap_small");
                        fn = substArgTypes(_addr_fn, _addr_t.Key(), t.Elem());
                        n = mkcall1(_addr_fn, _addr_n.Type, _addr_init);

                    }

                }
                else
                {
                    if (n.Esc != EscNone)
                    {
                        h = nodnil();
                    } 
                    // Map initialization with a variable or large hint is
                    // more complicated. We therefore generate a call to
                    // runtime.makemap to initialize hmap and allocate the
                    // map buckets.

                    // When hint fits into int, use makemap instead of
                    // makemap64, which is faster and shorter on 32 bit platforms.
                    fnname = "makemap64";
                    argtype = types.Types[TINT64]; 

                    // Type checking guarantees that TIDEAL hint is positive and fits in an int.
                    // See checkmake call in TMAP case of OMAKE case in OpSwitch in typecheck1 function.
                    // The case of hint overflow when converting TUINT or TUINTPTR to TINT
                    // will be handled by the negative range checks in makemap during runtime.
                    if (hint.Type.IsKind(TIDEAL) || maxintval[hint.Type.Etype].Cmp(maxintval[TUINT]) <= 0L)
                    {
                        fnname = "makemap";
                        argtype = types.Types[TINT];
                    }

                    fn = syslook(fnname);
                    fn = substArgTypes(_addr_fn, _addr_hmapType, t.Key(), t.Elem());
                    n = mkcall1(_addr_fn, _addr_n.Type, _addr_init, _addr_typename(n.Type), conv(_addr_hint, _addr_argtype), h);

                }

            else if (n.Op == OMAKESLICE) 
                l = n.Left;
                r = n.Right;
                if (r == null)
                {
                    r = safeexpr(l, init);
                    l = r;
                }

                t = n.Type;
                if (n.Esc == EscNone)
                {
                    if (!isSmallMakeSlice(_addr_n))
                    {
                        Fatalf("non-small OMAKESLICE with EscNone: %v", n);
                    } 
                    // var arr [r]T
                    // n = arr[:l]
                    var i = indexconst(r);
                    if (i < 0L)
                    {
                        Fatalf("walkexpr: invalid index %v", r);
                    } 

                    // cap is constrained to [0,2^31) or [0,2^63) depending on whether
                    // we're in 32-bit or 64-bit systems. So it's safe to do:
                    //
                    // if uint64(len) > cap {
                    //     if len < 0 { panicmakeslicelen() }
                    //     panicmakeslicecap()
                    // }
                    nif = nod(OIF, nod(OGT, conv(_addr_l, _addr_types.Types[TUINT64]), nodintconst(i)), null);
                    var niflen = nod(OIF, nod(OLT, l, nodintconst(0L)), null);
                    niflen.Nbody.Set1(mkcall("panicmakeslicelen", _addr_null, _addr_init));
                    nif.Nbody.Append(niflen, mkcall("panicmakeslicecap", _addr_null, _addr_init));
                    nif = typecheck(nif, ctxStmt);
                    init.Append(nif);

                    t = types.NewArray(t.Elem(), i); // [r]T
                    var_ = temp(t);
                    a = nod(OAS, var_, null); // zero temp
                    a = typecheck(a, ctxStmt);
                    init.Append(a);
                    r = nod(OSLICE, var_, null); // arr[:l]
                    r.SetSliceBounds(null, l, null);
                    r = conv(_addr_r, _addr_n.Type); // in case n.Type is named.
                    r = typecheck(r, ctxExpr);
                    r = walkexpr(_addr_r, _addr_init);
                    n = r;

                }
                else
                { 
                    // n escapes; set up a call to makeslice.
                    // When len and cap can fit into int, use makeslice instead of
                    // makeslice64, which is faster and shorter on 32 bit platforms.

                    if (t.Elem().NotInHeap())
                    {
                        yyerror("%v is go:notinheap; heap allocation disallowed", t.Elem());
                    }

                    var len = l;
                    var cap = r;

                    fnname = "makeslice64";
                    argtype = types.Types[TINT64]; 

                    // Type checking guarantees that TIDEAL len/cap are positive and fit in an int.
                    // The case of len or cap overflow when converting TUINT or TUINTPTR to TINT
                    // will be handled by the negative range checks in makeslice during runtime.
                    if ((len.Type.IsKind(TIDEAL) || maxintval[len.Type.Etype].Cmp(maxintval[TUINT]) <= 0L) && (cap.Type.IsKind(TIDEAL) || maxintval[cap.Type.Etype].Cmp(maxintval[TUINT]) <= 0L))
                    {
                        fnname = "makeslice";
                        argtype = types.Types[TINT];
                    }

                    var m = nod(OSLICEHEADER, null, null);
                    m.Type = t;

                    fn = syslook(fnname);
                    m.Left = mkcall1(_addr_fn, _addr_types.Types[TUNSAFEPTR], _addr_init, _addr_typename(t.Elem()), conv(_addr_len, _addr_argtype), conv(_addr_cap, _addr_argtype));
                    m.Left.MarkNonNil();
                    m.List.Set2(conv(_addr_len, _addr_types.Types[TINT]), conv(_addr_cap, _addr_types.Types[TINT]));

                    m = typecheck(m, ctxExpr);
                    m = walkexpr(_addr_m, _addr_init);
                    n = m;

                }

            else if (n.Op == OMAKESLICECOPY) 
                if (n.Esc == EscNone)
                {
                    Fatalf("OMAKESLICECOPY with EscNone: %v", n);
                }

                t = n.Type;
                if (t.Elem().NotInHeap())
                {
                    Fatalf("%v is go:notinheap; heap allocation disallowed", t.Elem());
                }

                var length = conv(_addr_n.Left, _addr_types.Types[TINT]);
                var copylen = nod(OLEN, n.Right, null);
                var copyptr = nod(OSPTR, n.Right, null);

                if (!types.Haspointers(t.Elem()) && n.Bounded())
                { 
                    // When len(to)==len(from) and elements have no pointers:
                    // replace make+copy with runtime.mallocgc+runtime.memmove.

                    // We do not check for overflow of len(to)*elem.Width here
                    // since len(from) is an existing checked slice capacity
                    // with same elem.Width for the from slice.
                    size = nod(OMUL, conv(_addr_length, _addr_types.Types[TUINTPTR]), conv(_addr_nodintconst(t.Elem().Width), _addr_types.Types[TUINTPTR])); 

                    // instantiate mallocgc(size uintptr, typ *byte, needszero bool) unsafe.Pointer
                    fn = syslook("mallocgc");
                    var sh = nod(OSLICEHEADER, null, null);
                    sh.Left = mkcall1(_addr_fn, _addr_types.Types[TUNSAFEPTR], _addr_init, _addr_size, nodnil(), nodbool(false));
                    sh.Left.MarkNonNil();
                    sh.List.Set2(length, length);
                    sh.Type = t;

                    var s = temp(t);
                    r = typecheck(nod(OAS, s, sh), ctxStmt);
                    r = walkexpr(_addr_r, _addr_init);
                    init.Append(r); 

                    // instantiate memmove(to *any, frm *any, size uintptr)
                    fn = syslook("memmove");
                    fn = substArgTypes(_addr_fn, _addr_t.Elem(), t.Elem());
                    var ncopy = mkcall1(_addr_fn, _addr_null, _addr_init, _addr_nod(OSPTR, s, null), copyptr, size);
                    ncopy = typecheck(ncopy, ctxStmt);
                    ncopy = walkexpr(_addr_ncopy, _addr_init);
                    init.Append(ncopy);

                    n = s;

                }
                else
                { // Replace make+copy with runtime.makeslicecopy.
                    // instantiate makeslicecopy(typ *byte, tolen int, fromlen int, from unsafe.Pointer) unsafe.Pointer
                    fn = syslook("makeslicecopy");
                    s = nod(OSLICEHEADER, null, null);
                    s.Left = mkcall1(_addr_fn, _addr_types.Types[TUNSAFEPTR], _addr_init, _addr_typename(t.Elem()), length, copylen, conv(_addr_copyptr, _addr_types.Types[TUNSAFEPTR]));
                    s.Left.MarkNonNil();
                    s.List.Set2(length, length);
                    s.Type = t;
                    n = typecheck(s, ctxExpr);
                    n = walkexpr(_addr_n, _addr_init);

                }

            else if (n.Op == ORUNESTR) 
                a = nodnil();
                if (n.Esc == EscNone)
                {
                    t = types.NewArray(types.Types[TUINT8], 4L);
                    a = nod(OADDR, temp(t), null);
                } 
                // intstring(*[4]byte, rune)
                n = mkcall("intstring", _addr_n.Type, _addr_init, _addr_a, conv(_addr_n.Left, _addr_types.Types[TINT64]));
            else if (n.Op == OBYTES2STR || n.Op == ORUNES2STR) 
                a = nodnil();
                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for string on stack.
                    t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);
                    a = nod(OADDR, temp(t), null);

                }

                if (n.Op == ORUNES2STR)
                { 
                    // slicerunetostring(*[32]byte, []rune) string
                    n = mkcall("slicerunetostring", _addr_n.Type, _addr_init, _addr_a, n.Left);

                }
                else
                { 
                    // slicebytetostring(*[32]byte, ptr *byte, n int) string
                    n.Left = cheapexpr(n.Left, init);
                    var (ptr, len) = n.Left.slicePtrLen();
                    n = mkcall("slicebytetostring", _addr_n.Type, _addr_init, _addr_a, ptr, len);

                }

            else if (n.Op == OBYTES2STRTMP) 
                n.Left = walkexpr(_addr_n.Left, _addr_init);
                if (!instrumenting)
                { 
                    // Let the backend handle OBYTES2STRTMP directly
                    // to avoid a function call to slicebytetostringtmp.
                    break;

                } 
                // slicebytetostringtmp(ptr *byte, n int) string
                n.Left = cheapexpr(n.Left, init);
                (ptr, len) = n.Left.slicePtrLen();
                n = mkcall("slicebytetostringtmp", _addr_n.Type, _addr_init, _addr_ptr, len);
            else if (n.Op == OSTR2BYTES) 
                s = n.Left;
                if (Isconst(s, CTSTR))
                {
                    var sc = strlit(s); 

                    // Allocate a [n]byte of the right size.
                    t = types.NewArray(types.Types[TUINT8], int64(len(sc)));
                    a = ;
                    if (n.Esc == EscNone && len(sc) <= int(maxImplicitStackVarSize))
                    {
                        a = nod(OADDR, temp(t), null);
                    }
                    else
                    {
                        a = callnew(_addr_t);
                    }

                    var p = temp(t.PtrTo()); // *[n]byte
                    init.Append(typecheck(nod(OAS, p, a), ctxStmt)); 

                    // Copy from the static string data to the [n]byte.
                    if (len(sc) > 0L)
                    {
                        var @as = nod(OAS, nod(ODEREF, p, null), nod(ODEREF, convnop(_addr_nod(OSPTR, s, null), _addr_t.PtrTo()), null));
                        as = typecheck(as, ctxStmt);
                        as = walkstmt(_addr_as);
                        init.Append(as);
                    } 

                    // Slice the [n]byte to a []byte.
                    n.Op = OSLICEARR;
                    n.Left = p;
                    n = walkexpr(_addr_n, _addr_init);
                    break;

                }

                a = nodnil();
                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for slice on stack.
                    t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);
                    a = nod(OADDR, temp(t), null);

                } 
                // stringtoslicebyte(*32[byte], string) []byte
                n = mkcall("stringtoslicebyte", _addr_n.Type, _addr_init, _addr_a, conv(_addr_s, _addr_types.Types[TSTRING]));
            else if (n.Op == OSTR2BYTESTMP) 
                // []byte(string) conversion that creates a slice
                // referring to the actual string bytes.
                // This conversion is handled later by the backend and
                // is only for use by internal compiler optimizations
                // that know that the slice won't be mutated.
                // The only such case today is:
                // for i, c := range []byte(string)
                n.Left = walkexpr(_addr_n.Left, _addr_init);
            else if (n.Op == OSTR2RUNES) 
                a = nodnil();
                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for slice on stack.
                    t = types.NewArray(types.Types[TINT32], tmpstringbufsize);
                    a = nod(OADDR, temp(t), null);

                } 
                // stringtoslicerune(*[32]rune, string) []rune
                n = mkcall("stringtoslicerune", _addr_n.Type, _addr_init, _addr_a, conv(_addr_n.Left, _addr_types.Types[TSTRING]));
            else if (n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OMAPLIT || n.Op == OSTRUCTLIT || n.Op == OPTRLIT) 
                if (isStaticCompositeLiteral(n) && !canSSAType(n.Type))
                { 
                    // n can be directly represented in the read-only data section.
                    // Make direct reference to the static data. See issue 12841.
                    var vstat = staticname(n.Type);
                    vstat.MarkReadonly();
                    fixedlit(inInitFunction, initKindStatic, n, vstat, init);
                    n = vstat;
                    n = typecheck(n, ctxExpr);
                    break;

                }

                var_ = temp(n.Type);
                anylit(n, var_, init);
                n = var_;
            else if (n.Op == OSEND) 
                n1 = n.Right;
                n1 = assignconv(n1, n.Left.Type.Elem(), "chan send");
                n1 = walkexpr(_addr_n1, _addr_init);
                n1 = nod(OADDR, n1, null);
                n = mkcall1(_addr_chanfn("chansend1", 2L, _addr_n.Left.Type), _addr_null, _addr_init, _addr_n.Left, n1);
            else if (n.Op == OCLOSURE) 
                n = walkclosure(n, init);
            else if (n.Op == OCALLPART) 
                n = walkpartialcall(n, init);
            else 
                Dump("walk", n);
                Fatalf("walkexpr: switch 1 unknown op %+S", n);
            // Expressions that are constant at run time but not
            // considered const by the language spec are not turned into
            // constants until walk. For example, if n is y%1 == 0, the
            // walk of y%1 may have replaced it by 0.
            // Check whether n with its updated args is itself now a constant.
            t = n.Type;
            evconst(n);
            if (n.Type != t)
            {
                Fatalf("evconst changed Type: %v had type %v, now %v", n, t, n.Type);
            }

            if (n.Op == OLITERAL)
            {
                n = typecheck(n, ctxExpr); 
                // Emit string symbol now to avoid emitting
                // any concurrently during the backend.
                {
                    var s__prev2 = s;
                    var ok__prev2 = ok;

                    @string (s, ok) = n.Val().U._<@string>();

                    if (ok)
                    {
                        _ = stringsym(n.Pos, s);
                    }

                    s = s__prev2;
                    ok = ok__prev2;

                }

            }

            updateHasCall(n);

            if (Debug['w'] != 0L && n != null)
            {
                Dump("after walk expr", n);
            }

            lineno = lno;
            return _addr_n!;

        }

        // rtconvfn returns the parameter and result types that will be used by a
        // runtime function to convert from type src to type dst. The runtime function
        // name can be derived from the names of the returned types.
        //
        // If no such function is necessary, it returns (Txxx, Txxx).
        private static (types.EType, types.EType) rtconvfn(ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst)
        {
            types.EType param = default;
            types.EType result = default;
            ref types.Type src = ref _addr_src.val;
            ref types.Type dst = ref _addr_dst.val;

            if (thearch.SoftFloat)
            {
                return (Txxx, Txxx);
            }


            if (thearch.LinkArch.Family == sys.ARM || thearch.LinkArch.Family == sys.MIPS) 
                if (src.IsFloat())
                {

                    if (dst.Etype == TINT64 || dst.Etype == TUINT64) 
                        return (TFLOAT64, dst.Etype);
                    
                }

                if (dst.IsFloat())
                {

                    if (src.Etype == TINT64 || src.Etype == TUINT64) 
                        return (src.Etype, TFLOAT64);
                    
                }

            else if (thearch.LinkArch.Family == sys.I386) 
                if (src.IsFloat())
                {

                    if (dst.Etype == TINT64 || dst.Etype == TUINT64) 
                        return (TFLOAT64, dst.Etype);
                    else if (dst.Etype == TUINT32 || dst.Etype == TUINT || dst.Etype == TUINTPTR) 
                        return (TFLOAT64, TUINT32);
                    
                }

                if (dst.IsFloat())
                {

                    if (src.Etype == TINT64 || src.Etype == TUINT64) 
                        return (src.Etype, TFLOAT64);
                    else if (src.Etype == TUINT32 || src.Etype == TUINT || src.Etype == TUINTPTR) 
                        return (TUINT32, TFLOAT64);
                    
                }

                        return (Txxx, Txxx);

        }

        // TODO(josharian): combine this with its caller and simplify
        private static ptr<Node> reduceSlice(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var (low, high, max) = n.SliceBounds();
            if (high != null && high.Op == OLEN && samesafeexpr(n.Left, high.Left))
            { 
                // Reduce x[i:len(x)] to x[i:].
                high = null;

            }

            n.SetSliceBounds(low, high, max);
            if ((n.Op == OSLICE || n.Op == OSLICESTR) && low == null && high == null)
            { 
                // Reduce x[:] to x.
                if (Debug_slice > 0L)
                {
                    Warn("slice: omit slice operation");
                }

                return _addr_n.Left!;

            }

            return _addr_n!;

        }

        private static ptr<Node> ascompatee1(ptr<Node> _addr_l, ptr<Node> _addr_r, ptr<Nodes> _addr_init)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;
            ref Nodes init = ref _addr_init.val;
 
            // convas will turn map assigns into function calls,
            // making it impossible for reorder3 to work.
            var n = nod(OAS, l, r);

            if (l.Op == OINDEXMAP)
            {
                return _addr_n!;
            }

            return _addr_convas(_addr_n, _addr_init)!;

        }

        private static slice<ptr<Node>> ascompatee(Op op, slice<ptr<Node>> nl, slice<ptr<Node>> nr, ptr<Nodes> _addr_init)
        {
            ref Nodes init = ref _addr_init.val;
 
            // check assign expression list to
            // an expression list. called in
            //    expr-list = expr-list

            // ensure order of evaluation for function calls
            {
                var i__prev1 = i;

                foreach (var (__i) in nl)
                {
                    i = __i;
                    nl[i] = safeexpr(nl[i], init);
                }

                i = i__prev1;
            }

            foreach (var (i1) in nr)
            {
                nr[i1] = safeexpr(nr[i1], init);
            }
            slice<ptr<Node>> nn = default;
            long i = 0L;
            while (i < len(nl))
            {
                if (i >= len(nr))
                {
                    break;
                i++;
                } 
                // Do not generate 'x = x' during return. See issue 4014.
                if (op == ORETURN && samesafeexpr(nl[i], nr[i]))
                {
                    continue;
                }

                nn = append(nn, ascompatee1(_addr_nl[i], _addr_nr[i], _addr_init));

            } 

            // cannot happen: caller checked that lists had same length
 

            // cannot happen: caller checked that lists had same length
            if (i < len(nl) || i < len(nr))
            {
                Nodes nln = default;                Nodes nrn = default;

                nln.Set(nl);
                nrn.Set(nr);
                Fatalf("error in shape across %+v %v %+v / %d %d [%s]", nln, op, nrn, len(nl), len(nr), Curfn.funcname());
            }

            return nn;

        }

        // fncall reports whether assigning an rvalue of type rt to an lvalue l might involve a function call.
        private static bool fncall(ptr<Node> _addr_l, ptr<types.Type> _addr_rt)
        {
            ref Node l = ref _addr_l.val;
            ref types.Type rt = ref _addr_rt.val;

            if (l.HasCall() || l.Op == OINDEXMAP)
            {
                return true;
            }

            if (types.Identical(l.Type, rt))
            {
                return false;
            } 
            // There might be a conversion required, which might involve a runtime call.
            return true;

        }

        // check assign type list to
        // an expression list. called in
        //    expr-list = func()
        private static slice<ptr<Node>> ascompatet(Nodes nl, ptr<types.Type> _addr_nr)
        {
            ref types.Type nr = ref _addr_nr.val;

            if (nl.Len() != nr.NumFields())
            {
                Fatalf("ascompatet: assignment count mismatch: %d = %d", nl.Len(), nr.NumFields());
            }

            ref Nodes nn = ref heap(out ptr<Nodes> _addr_nn);            ref Nodes mm = ref heap(out ptr<Nodes> _addr_mm);

            foreach (var (i, l) in nl.Slice())
            {
                if (l.isBlank())
                {
                    continue;
                }

                var r = nr.Field(i); 

                // Any assignment to an lvalue that might cause a function call must be
                // deferred until all the returned values have been read.
                if (fncall(_addr_l, _addr_r.Type))
                {
                    var tmp = temp(r.Type);
                    tmp = typecheck(tmp, ctxExpr);
                    var a = nod(OAS, l, tmp);
                    a = convas(_addr_a, _addr_mm);
                    mm.Append(a);
                    l = tmp;
                }

                var res = nod(ORESULT, null, null);
                res.Xoffset = Ctxt.FixedFrameSize() + r.Offset;
                res.Type = r.Type;
                res.SetTypecheck(1L);

                a = nod(OAS, l, res);
                a = convas(_addr_a, _addr_nn);
                updateHasCall(a);
                if (a.HasCall())
                {
                    Dump("ascompatet ucount", a);
                    Fatalf("ascompatet: too many function calls evaluating parameters");
                }

                nn.Append(a);

            }
            return append(nn.Slice(), mm.Slice());

        }

        // package all the arguments that match a ... T parameter into a []T.
        private static ptr<Node> mkdotargslice(ptr<types.Type> _addr_typ, slice<ptr<Node>> args)
        {
            ref types.Type typ = ref _addr_typ.val;

            ptr<Node> n;
            if (len(args) == 0L)
            {
                n = nodnil();
                n.Type = typ;
            }
            else
            {
                n = nod(OCOMPLIT, null, typenod(typ));
                n.List.Append(args);
                n.SetImplicit(true);
            }

            n = typecheck(n, ctxExpr);
            if (n.Type == null)
            {
                Fatalf("mkdotargslice: typecheck failed");
            }

            return _addr_n!;

        }

        // fixVariadicCall rewrites calls to variadic functions to use an
        // explicit ... argument if one is not already present.
        private static void fixVariadicCall(ptr<Node> _addr_call)
        {
            ref Node call = ref _addr_call.val;

            var fntype = call.Left.Type;
            if (!fntype.IsVariadic() || call.IsDDD())
            {
                return ;
            }

            var vi = fntype.NumParams() - 1L;
            var vt = fntype.Params().Field(vi).Type;

            var args = call.List.Slice();
            var extra = args[vi..];
            var slice = mkdotargslice(_addr_vt, extra);
            foreach (var (i) in extra)
            {
                extra[i] = null; // allow GC
            }
            call.List.Set(append(args[..vi], slice));
            call.SetIsDDD(true);

        }

        private static void walkCall(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Rlist.Len() != 0L)
            {
                return ; // already walked
            }

            var @params = n.Left.Type.Params();
            var args = n.List.Slice();

            n.Left = walkexpr(_addr_n.Left, _addr_init);
            walkexprlist(args, _addr_init); 

            // If this is a method call, add the receiver at the beginning of the args.
            if (n.Op == OCALLMETH)
            {
                var withRecv = make_slice<ptr<Node>>(len(args) + 1L);
                withRecv[0L] = n.Left.Left;
                n.Left.Left = null;
                copy(withRecv[1L..], args);
                args = withRecv;
            } 

            // For any argument whose evaluation might require a function call,
            // store that argument into a temporary variable,
            // to prevent that calls from clobbering arguments already on the stack.
            // When instrumenting, all arguments might require function calls.
            slice<ptr<Node>> tempAssigns = default;
            foreach (var (i, arg) in args)
            {
                updateHasCall(arg); 
                // Determine param type.
                ptr<types.Type> t;
                if (n.Op == OCALLMETH)
                {
                    if (i == 0L)
                    {
                        t = n.Left.Type.Recv().Type;
                    }
                    else
                    {
                        t = @params.Field(i - 1L).Type;
                    }

                }
                else
                {
                    t = @params.Field(i).Type;
                }

                if (instrumenting || fncall(_addr_arg, t))
                { 
                    // make assignment of fncall to tempAt
                    var tmp = temp(t);
                    var a = nod(OAS, tmp, arg);
                    a = convas(_addr_a, _addr_init);
                    tempAssigns = append(tempAssigns, a); 
                    // replace arg with temp
                    args[i] = tmp;

                }

            }
            n.List.Set(tempAssigns);
            n.Rlist.Set(args);

        }

        // generate code for print
        private static ptr<Node> walkprint(ptr<Node> _addr_nn, ptr<Nodes> _addr_init)
        {
            ref Node nn = ref _addr_nn.val;
            ref Nodes init = ref _addr_init.val;
 
            // Hoist all the argument evaluation up before the lock.
            walkexprlistcheap(nn.List.Slice(), _addr_init); 

            // For println, add " " between elements and "\n" at the end.
            if (nn.Op == OPRINTN)
            {
                var s = nn.List.Slice();
                var t = make_slice<ptr<Node>>(0L, len(s) * 2L);
                {
                    var i__prev1 = i;
                    var n__prev1 = n;

                    foreach (var (__i, __n) in s)
                    {
                        i = __i;
                        n = __n;
                        if (i != 0L)
                        {
                            t = append(t, nodstr(" "));
                        }

                        t = append(t, n);

                    }

                    i = i__prev1;
                    n = n__prev1;
                }

                t = append(t, nodstr("\n"));
                nn.List.Set(t);

            } 

            // Collapse runs of constant strings.
            s = nn.List.Slice();
            t = make_slice<ptr<Node>>(0L, len(s));
            {
                var i__prev1 = i;

                long i = 0L;

                while (i < len(s))
                {
                    slice<@string> strs = default;
                    while (i < len(s) && Isconst(s[i], CTSTR))
                    {
                        strs = append(strs, strlit(s[i]));
                        i++;
                    }

                    if (len(strs) > 0L)
                    {
                        t = append(t, nodstr(strings.Join(strs, "")));
                    }

                    if (i < len(s))
                    {
                        t = append(t, s[i]);
                        i++;
                    }

                }


                i = i__prev1;
            }
            nn.List.Set(t);

            ptr<Node> calls = new slice<ptr<Node>>(new ptr<Node>[] { mkcall("printlock",nil,init) });
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in nn.List.Slice())
                {
                    i = __i;
                    n = __n;
                    if (n.Op == OLITERAL)
                    {

                        if (n.Val().Ctype() == CTRUNE) 
                            n = defaultlit(n, types.Runetype);
                        else if (n.Val().Ctype() == CTINT) 
                            n = defaultlit(n, types.Types[TINT64]);
                        else if (n.Val().Ctype() == CTFLT) 
                            n = defaultlit(n, types.Types[TFLOAT64]);
                        
                    }

                    if (n.Op != OLITERAL && n.Type != null && n.Type.Etype == TIDEAL)
                    {
                        n = defaultlit(n, types.Types[TINT64]);
                    }

                    n = defaultlit(n, null);
                    nn.List.SetIndex(i, n);
                    if (n.Type == null || n.Type.Etype == TFORW)
                    {
                        continue;
                    }

                    ptr<Node> on;

                    if (n.Type.Etype == TINTER) 
                        if (n.Type.IsEmptyInterface())
                        {
                            on = syslook("printeface");
                        }
                        else
                        {
                            on = syslook("printiface");
                        }

                        on = substArgTypes(on, _addr_n.Type); // any-1
                    else if (n.Type.Etype == TPTR || n.Type.Etype == TCHAN || n.Type.Etype == TMAP || n.Type.Etype == TFUNC || n.Type.Etype == TUNSAFEPTR) 
                        on = syslook("printpointer");
                        on = substArgTypes(on, _addr_n.Type); // any-1
                    else if (n.Type.Etype == TSLICE) 
                        on = syslook("printslice");
                        on = substArgTypes(on, _addr_n.Type); // any-1
                    else if (n.Type.Etype == TUINT || n.Type.Etype == TUINT8 || n.Type.Etype == TUINT16 || n.Type.Etype == TUINT32 || n.Type.Etype == TUINT64 || n.Type.Etype == TUINTPTR) 
                        if (isRuntimePkg(n.Type.Sym.Pkg) && n.Type.Sym.Name == "hex")
                        {
                            on = syslook("printhex");
                        }
                        else
                        {
                            on = syslook("printuint");
                        }

                    else if (n.Type.Etype == TINT || n.Type.Etype == TINT8 || n.Type.Etype == TINT16 || n.Type.Etype == TINT32 || n.Type.Etype == TINT64) 
                        on = syslook("printint");
                    else if (n.Type.Etype == TFLOAT32 || n.Type.Etype == TFLOAT64) 
                        on = syslook("printfloat");
                    else if (n.Type.Etype == TCOMPLEX64 || n.Type.Etype == TCOMPLEX128) 
                        on = syslook("printcomplex");
                    else if (n.Type.Etype == TBOOL) 
                        on = syslook("printbool");
                    else if (n.Type.Etype == TSTRING) 
                        @string cs = "";
                        if (Isconst(n, CTSTR))
                        {
                            cs = strlit(n);
                        }

                        switch (cs)
                        {
                            case " ": 
                                on = syslook("printsp");
                                break;
                            case "\n": 
                                on = syslook("printnl");
                                break;
                            default: 
                                on = syslook("printstring");
                                break;
                        }
                    else 
                        badtype(OPRINT, n.Type, null);
                        continue;
                                        var r = nod(OCALL, on, null);
                    {
                        var @params = on.Type.Params().FieldSlice();

                        if (len(params) > 0L)
                        {
                            t = params[0L].Type;
                            if (!types.Identical(t, n.Type))
                            {
                                n = nod(OCONV, n, null);
                                n.Type = t;
                            }

                            r.List.Append(n);

                        }

                    }

                    calls = append(calls, r);

                }

                i = i__prev1;
                n = n__prev1;
            }

            calls = append(calls, mkcall("printunlock", _addr_null, _addr_init));

            typecheckslice(calls, ctxStmt);
            walkexprlist(calls, _addr_init);

            r = nod(OEMPTY, null, null);
            r = typecheck(r, ctxStmt);
            r = walkexpr(_addr_r, _addr_init);
            r.Ninit.Set(calls);
            return _addr_r!;

        }

        private static ptr<Node> callnew(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.NotInHeap())
            {
                yyerror("%v is go:notinheap; heap allocation disallowed", t);
            }

            dowidth(t);
            var n = nod(ONEWOBJ, typename(t), null);
            n.Type = types.NewPtr(t);
            n.SetTypecheck(1L);
            n.MarkNonNil();
            return _addr_n!;

        }

        // isReflectHeaderDataField reports whether l is an expression p.Data
        // where p has type reflect.SliceHeader or reflect.StringHeader.
        private static bool isReflectHeaderDataField(ptr<Node> _addr_l)
        {
            ref Node l = ref _addr_l.val;

            if (l.Type != types.Types[TUINTPTR])
            {
                return false;
            }

            ptr<types.Sym> tsym;

            if (l.Op == ODOT) 
                tsym = l.Left.Type.Sym;
            else if (l.Op == ODOTPTR) 
                tsym = l.Left.Type.Elem().Sym;
            else 
                return false;
                        if (tsym == null || l.Sym.Name != "Data" || tsym.Pkg.Path != "reflect")
            {
                return false;
            }

            return tsym.Name == "SliceHeader" || tsym.Name == "StringHeader";

        }

        private static ptr<Node> convas(ptr<Node> _addr_n, ptr<Nodes> _addr_init) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Op != OAS)
            {
                Fatalf("convas: not OAS %v", n.Op);
            }

            defer(updateHasCall(n));

            n.SetTypecheck(1L);

            if (n.Left == null || n.Right == null)
            {
                return _addr_n!;
            }

            var lt = n.Left.Type;
            var rt = n.Right.Type;
            if (lt == null || rt == null)
            {
                return _addr_n!;
            }

            if (n.Left.isBlank())
            {
                n.Right = defaultlit(n.Right, null);
                return _addr_n!;
            }

            if (!types.Identical(lt, rt))
            {
                n.Right = assignconv(n.Right, lt, "assignment");
                n.Right = walkexpr(_addr_n.Right, _addr_init);
            }

            dowidth(n.Right.Type);

            return _addr_n!;

        });

        // from ascompat[ee]
        //    a,b = c,d
        // simultaneous assignment. there cannot
        // be later use of an earlier lvalue.
        //
        // function calls have been removed.
        private static slice<ptr<Node>> reorder3(slice<ptr<Node>> all)
        { 
            // If a needed expression may be affected by an
            // earlier assignment, make an early copy of that
            // expression and use the copy instead.
            ref slice<ptr<Node>> early = ref heap(out ptr<slice<ptr<Node>>> _addr_early);

            ref Nodes mapinit = ref heap(out ptr<Nodes> _addr_mapinit);
            foreach (var (i, n) in all)
            {
                var l = n.Left; 

                // Save subexpressions needed on left side.
                // Drill through non-dereferences.
                while (true)
                {
                    if (l.Op == ODOT || l.Op == OPAREN)
                    {
                        l = l.Left;
                        continue;
                    }

                    if (l.Op == OINDEX && l.Left.Type.IsArray())
                    {
                        l.Right = reorder3save(_addr_l.Right, all, i, _addr_early);
                        l = l.Left;
                        continue;
                    }

                    break;

                }



                if (l.Op == ONAME) 
                    break;
                else if (l.Op == OINDEX || l.Op == OINDEXMAP) 
                    l.Left = reorder3save(_addr_l.Left, all, i, _addr_early);
                    l.Right = reorder3save(_addr_l.Right, all, i, _addr_early);
                    if (l.Op == OINDEXMAP)
                    {
                        all[i] = convas(_addr_all[i], _addr_mapinit);
                    }

                else if (l.Op == ODEREF || l.Op == ODOTPTR) 
                    l.Left = reorder3save(_addr_l.Left, all, i, _addr_early);
                else 
                    Fatalf("reorder3 unexpected lvalue %#v", l.Op);
                // Save expression on right side.
                all[i].Right = reorder3save(_addr_all[i].Right, all, i, _addr_early);

            }
            early = append(mapinit.Slice(), early);
            return append(early, all);

        }

        // if the evaluation of *np would be affected by the
        // assignments in all up to but not including the ith assignment,
        // copy into a temporary during *early and
        // replace *np with that temp.
        // The result of reorder3save MUST be assigned back to n, e.g.
        //     n.Left = reorder3save(n.Left, all, i, early)
        private static ptr<Node> reorder3save(ptr<Node> _addr_n, slice<ptr<Node>> all, long i, ptr<slice<ptr<Node>>> _addr_early)
        {
            ref Node n = ref _addr_n.val;
            ref slice<ptr<Node>> early = ref _addr_early.val;

            if (!aliased(_addr_n, all, i))
            {
                return _addr_n!;
            }

            var q = temp(n.Type);
            q = nod(OAS, q, n);
            q = typecheck(q, ctxStmt);
            early.val = append(early.val, q);
            return _addr_q.Left!;

        }

        // what's the outer value that a write to n affects?
        // outer value means containing struct or array.
        private static ptr<Node> outervalue(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            while (true)
            {

                if (n.Op == OXDOT) 
                    Fatalf("OXDOT in walk");
                else if (n.Op == ODOT || n.Op == OPAREN || n.Op == OCONVNOP) 
                    n = n.Left;
                    continue;
                else if (n.Op == OINDEX) 
                    if (n.Left.Type != null && n.Left.Type.IsArray())
                    {
                        n = n.Left;
                        continue;
                    }

                                return _addr_n!;

            }


        }

        // Is it possible that the computation of n might be
        // affected by writes in as up to but not including the ith element?
        private static bool aliased(ptr<Node> _addr_n, slice<ptr<Node>> all, long i)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return false;
            } 

            // Treat all fields of a struct as referring to the whole struct.
            // We could do better but we would have to keep track of the fields.
            while (n.Op == ODOT)
            {
                n = n.Left;
            } 

            // Look for obvious aliasing: a variable being assigned
            // during the all list and appearing in n.
            // Also record whether there are any writes to main memory.
            // Also record whether there are any writes to variables
            // whose addresses have been taken.
 

            // Look for obvious aliasing: a variable being assigned
            // during the all list and appearing in n.
            // Also record whether there are any writes to main memory.
            // Also record whether there are any writes to variables
            // whose addresses have been taken.
            var memwrite = false;
            var varwrite = false;
            foreach (var (_, an) in all[..i])
            {
                var a = outervalue(_addr_an.Left);

                while (a.Op == ODOT)
                {
                    a = a.Left;
                }


                if (a.Op != ONAME)
                {
                    memwrite = true;
                    continue;
                }


                if (n.Class() == PAUTO || n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    if (n.Name.Addrtaken())
                    {
                        varwrite = true;
                        continue;
                    }

                    if (vmatch2(_addr_a, _addr_n))
                    { 
                        // Direct hit.
                        return true;

                    }

                else 
                    varwrite = true;
                    continue;
                
            } 

            // The variables being written do not appear in n.
            // However, n might refer to computed addresses
            // that are being written.

            // If no computed addresses are affected by the writes, no aliasing.
            if (!memwrite && !varwrite)
            {
                return false;
            } 

            // If n does not refer to computed addresses
            // (that is, if n only refers to variables whose addresses
            // have not been taken), no aliasing.
            if (varexpr(_addr_n))
            {
                return false;
            } 

            // Otherwise, both the writes and n refer to computed memory addresses.
            // Assume that they might conflict.
            return true;

        }

        // does the evaluation of n only refer to variables
        // whose addresses have not been taken?
        // (and no other memory)
        private static bool varexpr(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return true;
            }


            if (n.Op == OLITERAL) 
                return true;
            else if (n.Op == ONAME) 

                if (n.Class() == PAUTO || n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    if (!n.Name.Addrtaken())
                    {
                        return true;
                    }

                                return false;
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OMUL || n.Op == ODIV || n.Op == OMOD || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == OPLUS || n.Op == ONEG || n.Op == OBITNOT || n.Op == OPAREN || n.Op == OANDAND || n.Op == OOROR || n.Op == OCONV || n.Op == OCONVNOP || n.Op == OCONVIFACE || n.Op == ODOTTYPE) 
                return varexpr(_addr_n.Left) && varexpr(_addr_n.Right);
            else if (n.Op == ODOT) // but not ODOTPTR
                // Should have been handled in aliased.
                Fatalf("varexpr unexpected ODOT");
            // Be conservative.
            return false;

        }

        // is the name l mentioned in r?
        private static bool vmatch2(ptr<Node> _addr_l, ptr<Node> _addr_r)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;

            if (r == null)
            {
                return false;
            }


            // match each right given left
            if (r.Op == ONAME) 
                return l == r;
            else if (r.Op == OLITERAL) 
                return false;
                        if (vmatch2(_addr_l, _addr_r.Left))
            {
                return true;
            }

            if (vmatch2(_addr_l, _addr_r.Right))
            {
                return true;
            }

            foreach (var (_, n) in r.List.Slice())
            {
                if (vmatch2(_addr_l, _addr_n))
                {
                    return true;
                }

            }
            return false;

        }

        // is any name mentioned in l also mentioned in r?
        // called by sinit.go
        private static bool vmatch1(ptr<Node> _addr_l, ptr<Node> _addr_r)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;
 
            // isolate all left sides
            if (l == null || r == null)
            {
                return false;
            }


            if (l.Op == ONAME) 

                if (l.Class() == PPARAM || l.Class() == PAUTO) 
                    break;
                else 
                    // assignment to non-stack variable must be
                    // delayed if right has function calls.
                    if (r.HasCall())
                    {
                        return true;
                    }

                                return vmatch2(_addr_l, _addr_r);
            else if (l.Op == OLITERAL) 
                return false;
                        if (vmatch1(_addr_l.Left, _addr_r))
            {
                return true;
            }

            if (vmatch1(_addr_l.Right, _addr_r))
            {
                return true;
            }

            foreach (var (_, n) in l.List.Slice())
            {
                if (vmatch1(_addr_n, _addr_r))
                {
                    return true;
                }

            }
            return false;

        }

        // paramstoheap returns code to allocate memory for heap-escaped parameters
        // and to copy non-result parameters' values from the stack.
        private static slice<ptr<Node>> paramstoheap(ptr<types.Type> _addr_@params)
        {
            ref types.Type @params = ref _addr_@params.val;

            slice<ptr<Node>> nn = default;
            foreach (var (_, t) in @params.Fields().Slice())
            {
                var v = asNode(t.Nname);
                if (v != null && v.Sym != null && strings.HasPrefix(v.Sym.Name, "~r"))
                { // unnamed result
                    v = null;

                }

                if (v == null)
                {
                    continue;
                }

                {
                    var stackcopy = v.Name.Param.Stackcopy;

                    if (stackcopy != null)
                    {
                        nn = append(nn, walkstmt(_addr_nod(ODCL, v, null)));
                        if (stackcopy.Class() == PPARAM)
                        {
                            nn = append(nn, walkstmt(_addr_typecheck(nod(OAS, v, stackcopy), ctxStmt)));
                        }

                    }

                }

            }
            return nn;

        }

        // zeroResults zeros the return values at the start of the function.
        // We need to do this very early in the function.  Defer might stop a
        // panic and show the return values as they exist at the time of
        // panic.  For precise stacks, the garbage collector assumes results
        // are always live, so we need to zero them before any allocations,
        // even allocations to move params/results to the heap.
        // The generated code is added to Curfn's Enter list.
        private static void zeroResults()
        {
            foreach (var (_, f) in Curfn.Type.Results().Fields().Slice())
            {
                var v = asNode(f.Nname);
                if (v != null && v.Name.Param.Heapaddr != null)
                { 
                    // The local which points to the return value is the
                    // thing that needs zeroing. This is already handled
                    // by a Needzero annotation in plive.go:livenessepilogue.
                    continue;

                }

                if (v.isParamHeapCopy())
                { 
                    // TODO(josharian/khr): Investigate whether we can switch to "continue" here,
                    // and document more in either case.
                    // In the review of CL 114797, Keith wrote (roughly):
                    // I don't think the zeroing below matters.
                    // The stack return value will never be marked as live anywhere in the function.
                    // It is not written to until deferreturn returns.
                    v = v.Name.Param.Stackcopy;

                } 
                // Zero the stack location containing f.
                Curfn.Func.Enter.Append(nodl(Curfn.Pos, OAS, v, null));

            }

        }

        // returnsfromheap returns code to copy values for heap-escaped parameters
        // back to the stack.
        private static slice<ptr<Node>> returnsfromheap(ptr<types.Type> _addr_@params)
        {
            ref types.Type @params = ref _addr_@params.val;

            slice<ptr<Node>> nn = default;
            foreach (var (_, t) in @params.Fields().Slice())
            {
                var v = asNode(t.Nname);
                if (v == null)
                {
                    continue;
                }

                {
                    var stackcopy = v.Name.Param.Stackcopy;

                    if (stackcopy != null && stackcopy.Class() == PPARAMOUT)
                    {
                        nn = append(nn, walkstmt(_addr_typecheck(nod(OAS, stackcopy, v), ctxStmt)));
                    }

                }

            }
            return nn;

        }

        // heapmoves generates code to handle migrating heap-escaped parameters
        // between the stack and the heap. The generated code is added to Curfn's
        // Enter and Exit lists.
        private static void heapmoves()
        {
            var lno = lineno;
            lineno = Curfn.Pos;
            var nn = paramstoheap(_addr_Curfn.Type.Recvs());
            nn = append(nn, paramstoheap(_addr_Curfn.Type.Params()));
            nn = append(nn, paramstoheap(_addr_Curfn.Type.Results()));
            Curfn.Func.Enter.Append(nn);
            lineno = Curfn.Func.Endlineno;
            Curfn.Func.Exit.Append(returnsfromheap(_addr_Curfn.Type.Results()));
            lineno = lno;
        }

        private static ptr<Node> vmkcall(ptr<Node> _addr_fn, ptr<types.Type> _addr_t, ptr<Nodes> _addr_init, slice<ptr<Node>> va)
        {
            ref Node fn = ref _addr_fn.val;
            ref types.Type t = ref _addr_t.val;
            ref Nodes init = ref _addr_init.val;

            if (fn.Type == null || fn.Type.Etype != TFUNC)
            {
                Fatalf("mkcall %v %v", fn, fn.Type);
            }

            var n = fn.Type.NumParams();
            if (n != len(va))
            {
                Fatalf("vmkcall %v needs %v args got %v", fn, n, len(va));
            }

            var r = nod(OCALL, fn, null);
            r.List.Set(va);
            if (fn.Type.NumResults() > 0L)
            {
                r = typecheck(r, ctxExpr | ctxMultiOK);
            }
            else
            {
                r = typecheck(r, ctxStmt);
            }

            r = walkexpr(_addr_r, _addr_init);
            r.Type = t;
            return _addr_r!;

        }

        private static ptr<Node> mkcall(@string name, ptr<types.Type> _addr_t, ptr<Nodes> _addr_init, params ptr<ptr<Node>>[] _addr_args)
        {
            args = args.Clone();
            ref types.Type t = ref _addr_t.val;
            ref Nodes init = ref _addr_init.val;
            ref Node args = ref _addr_args.val;

            return _addr_vmkcall(_addr_syslook(name), _addr_t, _addr_init, args)!;
        }

        private static ptr<Node> mkcall1(ptr<Node> _addr_fn, ptr<types.Type> _addr_t, ptr<Nodes> _addr_init, params ptr<ptr<Node>>[] _addr_args)
        {
            args = args.Clone();
            ref Node fn = ref _addr_fn.val;
            ref types.Type t = ref _addr_t.val;
            ref Nodes init = ref _addr_init.val;
            ref Node args = ref _addr_args.val;

            return _addr_vmkcall(_addr_fn, _addr_t, _addr_init, args)!;
        }

        private static ptr<Node> conv(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (types.Identical(n.Type, t))
            {
                return _addr_n!;
            }

            n = nod(OCONV, n, null);
            n.Type = t;
            n = typecheck(n, ctxExpr);
            return _addr_n!;

        }

        // convnop converts node n to type t using the OCONVNOP op
        // and typechecks the result with ctxExpr.
        private static ptr<Node> convnop(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (types.Identical(n.Type, t))
            {
                return _addr_n!;
            }

            n = nod(OCONVNOP, n, null);
            n.Type = t;
            n = typecheck(n, ctxExpr);
            return _addr_n!;

        }

        // byteindex converts n, which is byte-sized, to an int used to index into an array.
        // We cannot use conv, because we allow converting bool to int here,
        // which is forbidden in user code.
        private static ptr<Node> byteindex(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // We cannot convert from bool to int directly.
            // While converting from int8 to int is possible, it would yield
            // the wrong result for negative values.
            // Reinterpreting the value as an unsigned byte solves both cases.
            if (!types.Identical(n.Type, types.Types[TUINT8]))
            {
                n = nod(OCONV, n, null);
                n.Type = types.Types[TUINT8];
                n.SetTypecheck(1L);
            }

            n = nod(OCONV, n, null);
            n.Type = types.Types[TINT];
            n.SetTypecheck(1L);
            return _addr_n!;

        }

        private static ptr<Node> chanfn(@string name, long n, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (!t.IsChan())
            {
                Fatalf("chanfn %v", t);
            }

            var fn = syslook(name);
            switch (n)
            {
                case 1L: 
                    fn = substArgTypes(_addr_fn, _addr_t.Elem());
                    break;
                case 2L: 
                    fn = substArgTypes(_addr_fn, _addr_t.Elem(), t.Elem());
                    break;
                default: 
                    Fatalf("chanfn %d", n);
                    break;
            }
            return _addr_fn!;

        }

        private static ptr<Node> mapfn(@string name, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (!t.IsMap())
            {
                Fatalf("mapfn %v", t);
            }

            var fn = syslook(name);
            fn = substArgTypes(_addr_fn, _addr_t.Key(), t.Elem(), t.Key(), t.Elem());
            return _addr_fn!;

        }

        private static ptr<Node> mapfndel(@string name, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (!t.IsMap())
            {
                Fatalf("mapfn %v", t);
            }

            var fn = syslook(name);
            fn = substArgTypes(_addr_fn, _addr_t.Key(), t.Elem(), t.Key());
            return _addr_fn!;

        }

        private static readonly var mapslow = iota;
        private static readonly var mapfast32 = 0;
        private static readonly var mapfast32ptr = 1;
        private static readonly var mapfast64 = 2;
        private static readonly var mapfast64ptr = 3;
        private static readonly var mapfaststr = 4;
        private static readonly var nmapfast = 5;


        private partial struct mapnames // : array<@string>
        {
        }

        private static mapnames mkmapnames(@string @base, @string ptr)
        {
            return new mapnames(base,base+"_fast32",base+"_fast32"+ptr,base+"_fast64",base+"_fast64"+ptr,base+"_faststr");
        }

        private static var mapaccess1 = mkmapnames("mapaccess1", "");
        private static var mapaccess2 = mkmapnames("mapaccess2", "");
        private static var mapassign = mkmapnames("mapassign", "ptr");
        private static var mapdelete = mkmapnames("mapdelete", "");

        private static long mapfast(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
            // Check runtime/map.go:maxElemSize before changing.
            if (t.Elem().Width > 128L)
            {
                return mapslow;
            }


            if (algtype(t.Key()) == AMEM32) 
                if (!t.Key().HasHeapPointer())
                {
                    return mapfast32;
                }

                if (Widthptr == 4L)
                {
                    return mapfast32ptr;
                }

                Fatalf("small pointer %v", t.Key());
            else if (algtype(t.Key()) == AMEM64) 
                if (!t.Key().HasHeapPointer())
                {
                    return mapfast64;
                }

                if (Widthptr == 8L)
                {
                    return mapfast64ptr;
                } 
                // Two-word object, at least one of which is a pointer.
                // Use the slow path.
            else if (algtype(t.Key()) == ASTRING) 
                return mapfaststr;
                        return mapslow;

        }

        private static ptr<Node> writebarrierfn(@string name, ptr<types.Type> _addr_l, ptr<types.Type> _addr_r)
        {
            ref types.Type l = ref _addr_l.val;
            ref types.Type r = ref _addr_r.val;

            var fn = syslook(name);
            fn = substArgTypes(_addr_fn, _addr_l, r);
            return _addr_fn!;
        }

        private static ptr<Node> addstr(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
 
            // order.expr rewrote OADDSTR to have a list of strings.
            var c = n.List.Len();

            if (c < 2L)
            {
                Fatalf("addstr count %d too small", c);
            }

            var buf = nodnil();
            if (n.Esc == EscNone)
            {
                var sz = int64(0L);
                foreach (var (_, n1) in n.List.Slice())
                {
                    if (n1.Op == OLITERAL)
                    {
                        sz += int64(len(strlit(n1)));
                    }

                } 

                // Don't allocate the buffer if the result won't fit.
                if (sz < tmpstringbufsize)
                { 
                    // Create temporary buffer for result string on stack.
                    var t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);
                    buf = nod(OADDR, temp(t), null);

                }

            } 

            // build list of string arguments
            ptr<Node> args = new slice<ptr<Node>>(new ptr<Node>[] { buf });
            foreach (var (_, n2) in n.List.Slice())
            {
                args = append(args, conv(_addr_n2, _addr_types.Types[TSTRING]));
            }
            @string fn = default;
            if (c <= 5L)
            { 
                // small numbers of strings use direct runtime helpers.
                // note: order.expr knows this cutoff too.
                fn = fmt.Sprintf("concatstring%d", c);

            }
            else
            { 
                // large numbers of strings are passed to the runtime as a slice.
                fn = "concatstrings";

                t = types.NewSlice(types.Types[TSTRING]);
                var slice = nod(OCOMPLIT, null, typenod(t));
                if (prealloc[n] != null)
                {
                    prealloc[slice] = prealloc[n];
                }

                slice.List.Set(args[1L..]); // skip buf arg
                args = new slice<ptr<Node>>(new ptr<Node>[] { buf, slice });
                slice.Esc = EscNone;

            }

            var cat = syslook(fn);
            var r = nod(OCALL, cat, null);
            r.List.Set(args);
            r = typecheck(r, ctxExpr);
            r = walkexpr(_addr_r, _addr_init);
            r.Type = n.Type;

            return _addr_r!;

        }

        private static void walkAppendArgs(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            walkexprlistsafe(n.List.Slice(), _addr_init); 

            // walkexprlistsafe will leave OINDEX (s[n]) alone if both s
            // and n are name or literal, but those may index the slice we're
            // modifying here. Fix explicitly.
            var ls = n.List.Slice();
            foreach (var (i1, n1) in ls)
            {
                ls[i1] = cheapexpr(n1, init);
            }

        }

        // expand append(l1, l2...) to
        //   init {
        //     s := l1
        //     n := len(s) + len(l2)
        //     // Compare as uint so growslice can panic on overflow.
        //     if uint(n) > uint(cap(s)) {
        //       s = growslice(s, n)
        //     }
        //     s = s[:n]
        //     memmove(&s[len(l1)], &l2[0], len(l2)*sizeof(T))
        //   }
        //   s
        //
        // l2 is allowed to be a string.
        private static ptr<Node> appendslice(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            walkAppendArgs(_addr_n, _addr_init);

            var l1 = n.List.First();
            var l2 = n.List.Second();
            l2 = cheapexpr(l2, init);
            n.List.SetSecond(l2);

            ref Nodes nodes = ref heap(out ptr<Nodes> _addr_nodes); 

            // var s []T
            var s = temp(l1.Type);
            nodes.Append(nod(OAS, s, l1)); // s = l1

            var elemtype = s.Type.Elem(); 

            // n := len(s) + len(l2)
            var nn = temp(types.Types[TINT]);
            nodes.Append(nod(OAS, nn, nod(OADD, nod(OLEN, s, null), nod(OLEN, l2, null)))); 

            // if uint(n) > uint(cap(s))
            var nif = nod(OIF, null, null);
            var nuint = conv(_addr_nn, _addr_types.Types[TUINT]);
            var scapuint = conv(_addr_nod(OCAP, s, null), _addr_types.Types[TUINT]);
            nif.Left = nod(OGT, nuint, scapuint); 

            // instantiate growslice(typ *type, []any, int) []any
            var fn = syslook("growslice");
            fn = substArgTypes(_addr_fn, _addr_elemtype, elemtype); 

            // s = growslice(T, s, n)
            nif.Nbody.Set1(nod(OAS, s, mkcall1(_addr_fn, _addr_s.Type, _addr_nif.Ninit, _addr_typename(elemtype), s, nn)));
            nodes.Append(nif); 

            // s = s[:n]
            var nt = nod(OSLICE, s, null);
            nt.SetSliceBounds(null, nn, null);
            nt.SetBounded(true);
            nodes.Append(nod(OAS, s, nt));

            ptr<Node> ncopy;
            if (elemtype.HasHeapPointer())
            { 
                // copy(s[len(l1):], l2)
                var nptr1 = nod(OSLICE, s, null);
                nptr1.Type = s.Type;
                nptr1.SetSliceBounds(nod(OLEN, l1, null), null, null);
                nptr1 = cheapexpr(nptr1, _addr_nodes);

                var nptr2 = l2;

                Curfn.Func.setWBPos(n.Pos); 

                // instantiate typedslicecopy(typ *type, dstPtr *any, dstLen int, srcPtr *any, srcLen int) int
                fn = syslook("typedslicecopy");
                fn = substArgTypes(_addr_fn, _addr_l1.Type.Elem(), l2.Type.Elem());
                var (ptr1, len1) = nptr1.slicePtrLen();
                var (ptr2, len2) = nptr2.slicePtrLen();
                ncopy = mkcall1(_addr_fn, _addr_types.Types[TINT], _addr_nodes, _addr_typename(elemtype), ptr1, len1, ptr2, len2);


            }
            else if (instrumenting && !compiling_runtime)
            { 
                // rely on runtime to instrument copy.
                // copy(s[len(l1):], l2)
                nptr1 = nod(OSLICE, s, null);
                nptr1.Type = s.Type;
                nptr1.SetSliceBounds(nod(OLEN, l1, null), null, null);
                nptr1 = cheapexpr(nptr1, _addr_nodes);

                nptr2 = l2;

                if (l2.Type.IsString())
                { 
                    // instantiate func slicestringcopy(toPtr *byte, toLen int, fr string) int
                    fn = syslook("slicestringcopy");
                    var (ptr, len) = nptr1.slicePtrLen();
                    var str = nod(OCONVNOP, nptr2, null);
                    str.Type = types.Types[TSTRING];
                    ncopy = mkcall1(_addr_fn, _addr_types.Types[TINT], _addr_nodes, _addr_ptr, len, str);

                }
                else
                { 
                    // instantiate func slicecopy(to any, fr any, wid uintptr) int
                    fn = syslook("slicecopy");
                    fn = substArgTypes(_addr_fn, _addr_l1.Type.Elem(), l2.Type.Elem());
                    (ptr1, len1) = nptr1.slicePtrLen();
                    (ptr2, len2) = nptr2.slicePtrLen();
                    ncopy = mkcall1(_addr_fn, _addr_types.Types[TINT], _addr_nodes, _addr_ptr1, len1, ptr2, len2, nodintconst(elemtype.Width));

                }

            }
            else
            { 
                // memmove(&s[len(l1)], &l2[0], len(l2)*sizeof(T))
                nptr1 = nod(OINDEX, s, nod(OLEN, l1, null));
                nptr1.SetBounded(true);
                nptr1 = nod(OADDR, nptr1, null);

                nptr2 = nod(OSPTR, l2, null);

                var nwid = cheapexpr(conv(_addr_nod(OLEN, l2, null), _addr_types.Types[TUINTPTR]), _addr_nodes);
                nwid = nod(OMUL, nwid, nodintconst(elemtype.Width)); 

                // instantiate func memmove(to *any, frm *any, length uintptr)
                fn = syslook("memmove");
                fn = substArgTypes(_addr_fn, _addr_elemtype, elemtype);
                ncopy = mkcall1(_addr_fn, _addr_null, _addr_nodes, _addr_nptr1, nptr2, nwid);

            }

            var ln = append(nodes.Slice(), ncopy);

            typecheckslice(ln, ctxStmt);
            walkstmtlist(ln);
            init.Append(ln);
            return _addr_s!;

        }

        // isAppendOfMake reports whether n is of the form append(x , make([]T, y)...).
        // isAppendOfMake assumes n has already been typechecked.
        private static bool isAppendOfMake(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (Debug['N'] != 0L || instrumenting)
            {
                return false;
            }

            if (n.Typecheck() == 0L)
            {
                Fatalf("missing typecheck: %+v", n);
            }

            if (n.Op != OAPPEND || !n.IsDDD() || n.List.Len() != 2L)
            {
                return false;
            }

            var second = n.List.Second();
            if (second.Op != OMAKESLICE || second.Right != null)
            {
                return false;
            } 

            // y must be either an integer constant or the largest possible positive value
            // of variable y needs to fit into an uint.

            // typecheck made sure that constant arguments to make are not negative and fit into an int.

            // The care of overflow of the len argument to make will be handled by an explicit check of int(len) < 0 during runtime.
            var y = second.Left;
            if (!Isconst(y, CTINT) && maxintval[y.Type.Etype].Cmp(maxintval[TUINT]) > 0L)
            {
                return false;
            }

            return true;

        }

        // extendslice rewrites append(l1, make([]T, l2)...) to
        //   init {
        //     if l2 >= 0 { // Empty if block here for more meaningful node.SetLikely(true)
        //     } else {
        //       panicmakeslicelen()
        //     }
        //     s := l1
        //     n := len(s) + l2
        //     // Compare n and s as uint so growslice can panic on overflow of len(s) + l2.
        //     // cap is a positive int and n can become negative when len(s) + l2
        //     // overflows int. Interpreting n when negative as uint makes it larger
        //     // than cap(s). growslice will check the int n arg and panic if n is
        //     // negative. This prevents the overflow from being undetected.
        //     if uint(n) > uint(cap(s)) {
        //       s = growslice(T, s, n)
        //     }
        //     s = s[:n]
        //     lptr := &l1[0]
        //     sptr := &s[0]
        //     if lptr == sptr || !hasPointers(T) {
        //       // growslice did not clear the whole underlying array (or did not get called)
        //       hp := &s[len(l1)]
        //       hn := l2 * sizeof(T)
        //       memclr(hp, hn)
        //     }
        //   }
        //   s
        private static ptr<Node> extendslice(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
 
            // isAppendOfMake made sure all possible positive values of l2 fit into an uint.
            // The case of l2 overflow when converting from e.g. uint to int is handled by an explicit
            // check of l2 < 0 at runtime which is generated below.
            var l2 = conv(_addr_n.List.Second().Left, _addr_types.Types[TINT]);
            l2 = typecheck(l2, ctxExpr);
            n.List.SetSecond(l2); // walkAppendArgs expects l2 in n.List.Second().

            walkAppendArgs(_addr_n, _addr_init);

            var l1 = n.List.First();
            l2 = n.List.Second(); // re-read l2, as it may have been updated by walkAppendArgs

            slice<ptr<Node>> nodes = default; 

            // if l2 >= 0 (likely happens), do nothing
            var nifneg = nod(OIF, nod(OGE, l2, nodintconst(0L)), null);
            nifneg.SetLikely(true); 

            // else panicmakeslicelen()
            nifneg.Rlist.Set1(mkcall("panicmakeslicelen", _addr_null, _addr_init));
            nodes = append(nodes, nifneg); 

            // s := l1
            var s = temp(l1.Type);
            nodes = append(nodes, nod(OAS, s, l1));

            var elemtype = s.Type.Elem(); 

            // n := len(s) + l2
            var nn = temp(types.Types[TINT]);
            nodes = append(nodes, nod(OAS, nn, nod(OADD, nod(OLEN, s, null), l2))); 

            // if uint(n) > uint(cap(s))
            var nuint = conv(_addr_nn, _addr_types.Types[TUINT]);
            var capuint = conv(_addr_nod(OCAP, s, null), _addr_types.Types[TUINT]);
            var nif = nod(OIF, nod(OGT, nuint, capuint), null); 

            // instantiate growslice(typ *type, old []any, newcap int) []any
            var fn = syslook("growslice");
            fn = substArgTypes(_addr_fn, _addr_elemtype, elemtype); 

            // s = growslice(T, s, n)
            nif.Nbody.Set1(nod(OAS, s, mkcall1(_addr_fn, _addr_s.Type, _addr_nif.Ninit, _addr_typename(elemtype), s, nn)));
            nodes = append(nodes, nif); 

            // s = s[:n]
            var nt = nod(OSLICE, s, null);
            nt.SetSliceBounds(null, nn, null);
            nt.SetBounded(true);
            nodes = append(nodes, nod(OAS, s, nt)); 

            // lptr := &l1[0]
            var l1ptr = temp(l1.Type.Elem().PtrTo());
            var tmp = nod(OSPTR, l1, null);
            nodes = append(nodes, nod(OAS, l1ptr, tmp)); 

            // sptr := &s[0]
            var sptr = temp(elemtype.PtrTo());
            tmp = nod(OSPTR, s, null);
            nodes = append(nodes, nod(OAS, sptr, tmp)); 

            // hp := &s[len(l1)]
            var hp = nod(OINDEX, s, nod(OLEN, l1, null));
            hp.SetBounded(true);
            hp = nod(OADDR, hp, null);
            hp = convnop(_addr_hp, _addr_types.Types[TUNSAFEPTR]); 

            // hn := l2 * sizeof(elem(s))
            var hn = nod(OMUL, l2, nodintconst(elemtype.Width));
            hn = conv(_addr_hn, _addr_types.Types[TUINTPTR]);

            @string clrname = "memclrNoHeapPointers";
            var hasPointers = types.Haspointers(elemtype);
            if (hasPointers)
            {
                clrname = "memclrHasPointers";
                Curfn.Func.setWBPos(n.Pos);
            }

            ref Nodes clr = ref heap(out ptr<Nodes> _addr_clr);
            var clrfn = mkcall(clrname, _addr_null, _addr_clr, _addr_hp, hn);
            clr.Append(clrfn);

            if (hasPointers)
            { 
                // if l1ptr == sptr
                var nifclr = nod(OIF, nod(OEQ, l1ptr, sptr), null);
                nifclr.Nbody = clr;
                nodes = append(nodes, nifclr);

            }
            else
            {
                nodes = append(nodes, clr.Slice());
            }

            typecheckslice(nodes, ctxStmt);
            walkstmtlist(nodes);
            init.Append(nodes);
            return _addr_s!;

        }

        // Rewrite append(src, x, y, z) so that any side effects in
        // x, y, z (including runtime panics) are evaluated in
        // initialization statements before the append.
        // For normal code generation, stop there and leave the
        // rest to cgen_append.
        //
        // For race detector, expand append(src, a [, b]* ) to
        //
        //   init {
        //     s := src
        //     const argc = len(args) - 1
        //     if cap(s) - len(s) < argc {
        //        s = growslice(s, len(s)+argc)
        //     }
        //     n := len(s)
        //     s = s[:n+argc]
        //     s[n] = a
        //     s[n+1] = b
        //     ...
        //   }
        //   s
        private static ptr<Node> walkappend(ptr<Node> _addr_n, ptr<Nodes> _addr_init, ptr<Node> _addr_dst)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
            ref Node dst = ref _addr_dst.val;

            if (!samesafeexpr(dst, n.List.First()))
            {
                n.List.SetFirst(safeexpr(n.List.First(), init));
                n.List.SetFirst(walkexpr(_addr_n.List.First(), _addr_init));
            }

            walkexprlistsafe(n.List.Slice()[1L..], _addr_init);

            var nsrc = n.List.First(); 

            // walkexprlistsafe will leave OINDEX (s[n]) alone if both s
            // and n are name or literal, but those may index the slice we're
            // modifying here. Fix explicitly.
            // Using cheapexpr also makes sure that the evaluation
            // of all arguments (and especially any panics) happen
            // before we begin to modify the slice in a visible way.
            var ls = n.List.Slice()[1L..];
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in ls)
                {
                    i = __i;
                    n = __n;
                    n = cheapexpr(n, init);
                    if (!types.Identical(n.Type, nsrc.Type.Elem()))
                    {
                        n = assignconv(n, nsrc.Type.Elem(), "append");
                        n = walkexpr(_addr_n, _addr_init);
                    }

                    ls[i] = n;

                }

                i = i__prev1;
                n = n__prev1;
            }

            var argc = n.List.Len() - 1L;
            if (argc < 1L)
            {
                return _addr_nsrc!;
            } 

            // General case, with no function calls left as arguments.
            // Leave for gen, except that instrumentation requires old form.
            if (!instrumenting || compiling_runtime)
            {
                return _addr_n!;
            }

            slice<ptr<Node>> l = default;

            var ns = temp(nsrc.Type);
            l = append(l, nod(OAS, ns, nsrc)); // s = src

            var na = nodintconst(int64(argc)); // const argc
            var nx = nod(OIF, null, null); // if cap(s) - len(s) < argc
            nx.Left = nod(OLT, nod(OSUB, nod(OCAP, ns, null), nod(OLEN, ns, null)), na);

            var fn = syslook("growslice"); //   growslice(<type>, old []T, mincap int) (ret []T)
            fn = substArgTypes(_addr_fn, _addr_ns.Type.Elem(), ns.Type.Elem());

            nx.Nbody.Set1(nod(OAS, ns, mkcall1(_addr_fn, _addr_ns.Type, _addr_nx.Ninit, _addr_typename(ns.Type.Elem()), ns, nod(OADD, nod(OLEN, ns, null), na))));

            l = append(l, nx);

            var nn = temp(types.Types[TINT]);
            l = append(l, nod(OAS, nn, nod(OLEN, ns, null))); // n = len(s)

            nx = nod(OSLICE, ns, null); // ...s[:n+argc]
            nx.SetSliceBounds(null, nod(OADD, nn, na), null);
            nx.SetBounded(true);
            l = append(l, nod(OAS, ns, nx)); // s = s[:n+argc]

            ls = n.List.Slice()[1L..];
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in ls)
                {
                    i = __i;
                    n = __n;
                    nx = nod(OINDEX, ns, nn); // s[n] ...
                    nx.SetBounded(true);
                    l = append(l, nod(OAS, nx, n)); // s[n] = arg
                    if (i + 1L < len(ls))
                    {
                        l = append(l, nod(OAS, nn, nod(OADD, nn, nodintconst(1L)))); // n = n + 1
                    }

                }

                i = i__prev1;
                n = n__prev1;
            }

            typecheckslice(l, ctxStmt);
            walkstmtlist(l);
            init.Append(l);
            return _addr_ns!;

        }

        // Lower copy(a, b) to a memmove call or a runtime call.
        //
        // init {
        //   n := len(a)
        //   if n > len(b) { n = len(b) }
        //   if a.ptr != b.ptr { memmove(a.ptr, b.ptr, n*sizeof(elem(a))) }
        // }
        // n;
        //
        // Also works if b is a string.
        //
        private static ptr<Node> copyany(ptr<Node> _addr_n, ptr<Nodes> _addr_init, bool runtimecall)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Left.Type.Elem().HasHeapPointer())
            {
                Curfn.Func.setWBPos(n.Pos);
                var fn = writebarrierfn("typedslicecopy", _addr_n.Left.Type.Elem(), _addr_n.Right.Type.Elem());
                n.Left = cheapexpr(n.Left, init);
                var (ptrL, lenL) = n.Left.slicePtrLen();
                n.Right = cheapexpr(n.Right, init);
                var (ptrR, lenR) = n.Right.slicePtrLen();
                return _addr_mkcall1(_addr_fn, _addr_n.Type, _addr_init, _addr_typename(n.Left.Type.Elem()), ptrL, lenL, ptrR, lenR)!;
            }

            if (runtimecall)
            {
                if (n.Right.Type.IsString())
                {
                    fn = syslook("slicestringcopy");
                    n.Left = cheapexpr(n.Left, init);
                    var (ptr, len) = n.Left.slicePtrLen();
                    var str = nod(OCONVNOP, n.Right, null);
                    str.Type = types.Types[TSTRING];
                    return _addr_mkcall1(_addr_fn, _addr_n.Type, _addr_init, _addr_ptr, len, str)!;
                }

                fn = syslook("slicecopy");
                fn = substArgTypes(_addr_fn, _addr_n.Left.Type.Elem(), n.Right.Type.Elem());
                n.Left = cheapexpr(n.Left, init);
                (ptrL, lenL) = n.Left.slicePtrLen();
                n.Right = cheapexpr(n.Right, init);
                (ptrR, lenR) = n.Right.slicePtrLen();
                return _addr_mkcall1(_addr_fn, _addr_n.Type, _addr_init, _addr_ptrL, lenL, ptrR, lenR, nodintconst(n.Left.Type.Elem().Width))!;

            }

            n.Left = walkexpr(_addr_n.Left, _addr_init);
            n.Right = walkexpr(_addr_n.Right, _addr_init);
            var nl = temp(n.Left.Type);
            var nr = temp(n.Right.Type);
            slice<ptr<Node>> l = default;
            l = append(l, nod(OAS, nl, n.Left));
            l = append(l, nod(OAS, nr, n.Right));

            var nfrm = nod(OSPTR, nr, null);
            var nto = nod(OSPTR, nl, null);

            var nlen = temp(types.Types[TINT]); 

            // n = len(to)
            l = append(l, nod(OAS, nlen, nod(OLEN, nl, null))); 

            // if n > len(frm) { n = len(frm) }
            var nif = nod(OIF, null, null);

            nif.Left = nod(OGT, nlen, nod(OLEN, nr, null));
            nif.Nbody.Append(nod(OAS, nlen, nod(OLEN, nr, null)));
            l = append(l, nif); 

            // if to.ptr != frm.ptr { memmove( ... ) }
            var ne = nod(OIF, nod(ONE, nto, nfrm), null);
            ne.SetLikely(true);
            l = append(l, ne);

            fn = syslook("memmove");
            fn = substArgTypes(_addr_fn, _addr_nl.Type.Elem(), nl.Type.Elem());
            var nwid = temp(types.Types[TUINTPTR]);
            var setwid = nod(OAS, nwid, conv(_addr_nlen, _addr_types.Types[TUINTPTR]));
            ne.Nbody.Append(setwid);
            nwid = nod(OMUL, nwid, nodintconst(nl.Type.Elem().Width));
            var call = mkcall1(_addr_fn, _addr_null, _addr_init, _addr_nto, nfrm, nwid);
            ne.Nbody.Append(call);

            typecheckslice(l, ctxStmt);
            walkstmtlist(l);
            init.Append(l);
            return _addr_nlen!;

        }

        private static (ptr<Node>, bool) eqfor(ptr<types.Type> _addr_t)
        {
            ptr<Node> n = default!;
            bool needsize = default;
            ref types.Type t = ref _addr_t.val;
 
            // Should only arrive here with large memory or
            // a struct/array containing a non-memory field/element.
            // Small memory is handled inline, and single non-memory
            // is handled by walkcompare.
            {
                var (a, _) = algtype1(t);


                if (a == AMEM) 
                    var n = syslook("memequal");
                    n = substArgTypes(_addr_n, _addr_t, t);
                    return (_addr_n!, true);
                else if (a == ASPECIAL) 
                    var sym = typesymprefix(".eq", t);
                    n = newname(sym);
                    n.SetClass(PFUNC);
                    n.Sym.SetFunc(true);
                    n.Type = functype(null, new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.NewPtr(t)), anonfield(types.NewPtr(t)) }), new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.Types[TBOOL]) }));
                    return (_addr_n!, false);

            }
            Fatalf("eqfor %v", t);
            return (_addr_null!, false);

        }

        // The result of walkcompare MUST be assigned back to n, e.g.
        //     n.Left = walkcompare(n.Left, init)
        private static ptr<Node> walkcompare(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Left.Type.IsInterface() && n.Right.Type.IsInterface() && n.Left.Op != OLITERAL && n.Right.Op != OLITERAL)
            {
                return _addr_walkcompareInterface(_addr_n, _addr_init)!;
            }

            if (n.Left.Type.IsString() && n.Right.Type.IsString())
            {
                return _addr_walkcompareString(_addr_n, _addr_init)!;
            }

            n.Left = walkexpr(_addr_n.Left, _addr_init);
            n.Right = walkexpr(_addr_n.Right, _addr_init); 

            // Given mixed interface/concrete comparison,
            // rewrite into types-equal && data-equal.
            // This is efficient, avoids allocations, and avoids runtime calls.
            if (n.Left.Type.IsInterface() != n.Right.Type.IsInterface())
            { 
                // Preserve side-effects in case of short-circuiting; see #32187.
                var l = cheapexpr(n.Left, init);
                var r = cheapexpr(n.Right, init); 
                // Swap so that l is the interface value and r is the concrete value.
                if (n.Right.Type.IsInterface())
                {
                    l = r;
                    r = l;

                } 

                // Handle both == and !=.
                var eq = n.Op;
                var andor = OOROR;
                if (eq == OEQ)
                {
                    andor = OANDAND;
                } 
                // Check for types equal.
                // For empty interface, this is:
                //   l.tab == type(r)
                // For non-empty interface, this is:
                //   l.tab != nil && l.tab._type == type(r)
                ptr<Node> eqtype;
                var tab = nod(OITAB, l, null);
                var rtyp = typename(r.Type);
                if (l.Type.IsEmptyInterface())
                {
                    tab.Type = types.NewPtr(types.Types[TUINT8]);
                    tab.SetTypecheck(1L);
                    eqtype = nod(eq, tab, rtyp);
                }
                else
                {
                    var nonnil = nod(brcom(eq), nodnil(), tab);
                    var match = nod(eq, itabType(tab), rtyp);
                    eqtype = nod(andor, nonnil, match);
                } 
                // Check for data equal.
                var eqdata = nod(eq, ifaceData(n.Pos, l, r.Type), r); 
                // Put it all together.
                var expr = nod(andor, eqtype, eqdata);
                n = finishcompare(_addr_n, _addr_expr, _addr_init);
                return _addr_n!;

            } 

            // Must be comparison of array or struct.
            // Otherwise back end handles it.
            // While we're here, decide whether to
            // inline or call an eq alg.
            var t = n.Left.Type;
            bool inline = default;

            var maxcmpsize = int64(4L);
            var unalignedLoad = canMergeLoads();
            if (unalignedLoad)
            { 
                // Keep this low enough to generate less code than a function call.
                maxcmpsize = 2L * int64(thearch.LinkArch.RegSize);

            }


            if (t.Etype == TARRAY) 
                // We can compare several elements at once with 2/4/8 byte integer compares
                inline = t.NumElem() <= 1L || (issimple[t.Elem().Etype] && (t.NumElem() <= 4L || t.Elem().Width * t.NumElem() <= maxcmpsize));
            else if (t.Etype == TSTRUCT) 
                inline = t.NumComponents(types.IgnoreBlankFields) <= 4L;
            else 
                if (Debug_libfuzzer != 0L && t.IsInteger())
                {
                    n.Left = cheapexpr(n.Left, init);
                    n.Right = cheapexpr(n.Right, init); 

                    // If exactly one comparison operand is
                    // constant, invoke the constcmp functions
                    // instead, and arrange for the constant
                    // operand to be the first argument.
                    l = n.Left;
                    r = n.Right;
                    if (r.Op == OLITERAL)
                    {
                        l = r;
                        r = l;

                    }

                    var constcmp = l.Op == OLITERAL && r.Op != OLITERAL;

                    @string fn = default;
                    ptr<types.Type> paramType;
                    switch (t.Size())
                    {
                        case 1L: 
                            fn = "libfuzzerTraceCmp1";
                            if (constcmp)
                            {
                                fn = "libfuzzerTraceConstCmp1";
                            }

                            paramType = types.Types[TUINT8];
                            break;
                        case 2L: 
                            fn = "libfuzzerTraceCmp2";
                            if (constcmp)
                            {
                                fn = "libfuzzerTraceConstCmp2";
                            }

                            paramType = types.Types[TUINT16];
                            break;
                        case 4L: 
                            fn = "libfuzzerTraceCmp4";
                            if (constcmp)
                            {
                                fn = "libfuzzerTraceConstCmp4";
                            }

                            paramType = types.Types[TUINT32];
                            break;
                        case 8L: 
                            fn = "libfuzzerTraceCmp8";
                            if (constcmp)
                            {
                                fn = "libfuzzerTraceConstCmp8";
                            }

                            paramType = types.Types[TUINT64];
                            break;
                        default: 
                            Fatalf("unexpected integer size %d for %v", t.Size(), t);
                            break;
                    }
                    init.Append(mkcall(fn, _addr_null, _addr_init, _addr_tracecmpArg(_addr_l, paramType, _addr_init), tracecmpArg(_addr_r, paramType, _addr_init)));

                }

                return _addr_n!;
                        var cmpl = n.Left;
            while (cmpl != null && cmpl.Op == OCONVNOP)
            {
                cmpl = cmpl.Left;
            }

            var cmpr = n.Right;
            while (cmpr != null && cmpr.Op == OCONVNOP)
            {
                cmpr = cmpr.Left;
            } 

            // Chose not to inline. Call equality function directly.
 

            // Chose not to inline. Call equality function directly.
            if (!inline)
            { 
                // eq algs take pointers; cmpl and cmpr must be addressable
                if (!islvalue(cmpl) || !islvalue(cmpr))
                {
                    Fatalf("arguments of comparison must be lvalues - %v %v", cmpl, cmpr);
                }

                var (fn, needsize) = eqfor(_addr_t);
                var call = nod(OCALL, fn, null);
                call.List.Append(nod(OADDR, cmpl, null));
                call.List.Append(nod(OADDR, cmpr, null));
                if (needsize)
                {
                    call.List.Append(nodintconst(t.Width));
                }

                var res = call;
                if (n.Op != OEQ)
                {
                    res = nod(ONOT, res, null);
                }

                n = finishcompare(_addr_n, _addr_res, _addr_init);
                return _addr_n!;

            } 

            // inline: build boolean expression comparing element by element
            andor = OANDAND;
            if (n.Op == ONE)
            {
                andor = OOROR;
            }

            expr = ;
            Action<ptr<Node>, ptr<Node>> compare = (el, er) =>
            {
                var a = nod(n.Op, el, er);
                if (expr == null)
                {
                    expr = a;
                }
                else
                {
                    expr = nod(andor, expr, a);
                }

            }
;
            cmpl = safeexpr(cmpl, init);
            cmpr = safeexpr(cmpr, init);
            if (t.IsStruct())
            {
                foreach (var (_, f) in t.Fields().Slice())
                {
                    var sym = f.Sym;
                    if (sym.IsBlank())
                    {
                        continue;
                    }

                    compare(nodSym(OXDOT, cmpl, sym), nodSym(OXDOT, cmpr, sym));

                }
            else
            }            {
                var step = int64(1L);
                var remains = t.NumElem() * t.Elem().Width;
                var combine64bit = unalignedLoad && Widthreg == 8L && t.Elem().Width <= 4L && t.Elem().IsInteger();
                var combine32bit = unalignedLoad && t.Elem().Width <= 2L && t.Elem().IsInteger();
                var combine16bit = unalignedLoad && t.Elem().Width == 1L && t.Elem().IsInteger();
                {
                    var i = int64(0L);

                    while (remains > 0L)
                    {
                        ptr<types.Type> convType;

                        if (remains >= 8L && combine64bit) 
                            convType = types.Types[TINT64];
                            step = 8L / t.Elem().Width;
                        else if (remains >= 4L && combine32bit) 
                            convType = types.Types[TUINT32];
                            step = 4L / t.Elem().Width;
                        else if (remains >= 2L && combine16bit) 
                            convType = types.Types[TUINT16];
                            step = 2L / t.Elem().Width;
                        else 
                            step = 1L;
                                                if (step == 1L)
                        {
                            compare(nod(OINDEX, cmpl, nodintconst(i)), nod(OINDEX, cmpr, nodintconst(i)));
                            i++;
                            remains -= t.Elem().Width;
                        }
                        else
                        {
                            var elemType = t.Elem().ToUnsigned();
                            var cmplw = nod(OINDEX, cmpl, nodintconst(i));
                            cmplw = conv(_addr_cmplw, _addr_elemType); // convert to unsigned
                            cmplw = conv(_addr_cmplw, convType); // widen
                            var cmprw = nod(OINDEX, cmpr, nodintconst(i));
                            cmprw = conv(_addr_cmprw, _addr_elemType);
                            cmprw = conv(_addr_cmprw, convType); 
                            // For code like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                            // ssa will generate a single large load.
                            for (var offset = int64(1L); offset < step; offset++)
                            {
                                var lb = nod(OINDEX, cmpl, nodintconst(i + offset));
                                lb = conv(_addr_lb, _addr_elemType);
                                lb = conv(_addr_lb, convType);
                                lb = nod(OLSH, lb, nodintconst(8L * t.Elem().Width * offset));
                                cmplw = nod(OOR, cmplw, lb);
                                var rb = nod(OINDEX, cmpr, nodintconst(i + offset));
                                rb = conv(_addr_rb, _addr_elemType);
                                rb = conv(_addr_rb, convType);
                                rb = nod(OLSH, rb, nodintconst(8L * t.Elem().Width * offset));
                                cmprw = nod(OOR, cmprw, rb);
                            }

                            compare(cmplw, cmprw);
                            i += step;
                            remains -= step * t.Elem().Width;

                        }

                    }

                }

            }

            if (expr == null)
            {
                expr = nodbool(n.Op == OEQ); 
                // We still need to use cmpl and cmpr, in case they contain
                // an expression which might panic. See issue 23837.
                t = temp(cmpl.Type);
                var a1 = nod(OAS, t, cmpl);
                a1 = typecheck(a1, ctxStmt);
                var a2 = nod(OAS, t, cmpr);
                a2 = typecheck(a2, ctxStmt);
                init.Append(a1, a2);

            }

            n = finishcompare(_addr_n, _addr_expr, _addr_init);
            return _addr_n!;

        }

        private static ptr<Node> tracecmpArg(ptr<Node> _addr_n, ptr<types.Type> _addr_t, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;
            ref Nodes init = ref _addr_init.val;
 
            // Ugly hack to avoid "constant -1 overflows uintptr" errors, etc.
            if (n.Op == OLITERAL && n.Type.IsSigned() && n.Int64() < 0L)
            {
                n = copyexpr(n, n.Type, init);
            }

            return _addr_conv(_addr_n, _addr_t)!;

        }

        private static ptr<Node> walkcompareInterface(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            n.Right = cheapexpr(n.Right, init);
            n.Left = cheapexpr(n.Left, init);
            var (eqtab, eqdata) = eqinterface(n.Left, n.Right);
            ptr<Node> cmp;
            if (n.Op == OEQ)
            {
                cmp = nod(OANDAND, eqtab, eqdata);
            }
            else
            {
                eqtab.Op = ONE;
                cmp = nod(OOROR, eqtab, nod(ONOT, eqdata, null));
            }

            return _addr_finishcompare(_addr_n, cmp, _addr_init)!;

        }

        private static ptr<Node> walkcompareString(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
 
            // Rewrite comparisons to short constant strings as length+byte-wise comparisons.
            ptr<Node> cs;            ptr<Node> ncs; // const string, non-const string
 // const string, non-const string

            if (Isconst(n.Left, CTSTR) && Isconst(n.Right, CTSTR))             else if (Isconst(n.Left, CTSTR)) 
                cs = n.Left;
                ncs = n.Right;
            else if (Isconst(n.Right, CTSTR)) 
                cs = n.Right;
                ncs = n.Left;
                        if (cs != null)
            {
                var cmp = n.Op; 
                // Our comparison below assumes that the non-constant string
                // is on the left hand side, so rewrite "" cmp x to x cmp "".
                // See issue 24817.
                if (Isconst(n.Left, CTSTR))
                {
                    cmp = brrev(cmp);
                } 

                // maxRewriteLen was chosen empirically.
                // It is the value that minimizes cmd/go file size
                // across most architectures.
                // See the commit description for CL 26758 for details.
                long maxRewriteLen = 6L; 
                // Some architectures can load unaligned byte sequence as 1 word.
                // So we can cover longer strings with the same amount of code.
                var canCombineLoads = canMergeLoads();
                var combine64bit = false;
                if (canCombineLoads)
                { 
                    // Keep this low enough to generate less code than a function call.
                    maxRewriteLen = 2L * thearch.LinkArch.RegSize;
                    combine64bit = thearch.LinkArch.RegSize >= 8L;

                }

                Op and = default;

                if (cmp == OEQ) 
                    and = OANDAND;
                else if (cmp == ONE) 
                    and = OOROR;
                else 
                    // Don't do byte-wise comparisons for <, <=, etc.
                    // They're fairly complicated.
                    // Length-only checks are ok, though.
                    maxRewriteLen = 0L;
                                {
                    var s = strlit(cs);

                    if (len(s) <= maxRewriteLen)
                    {
                        if (len(s) > 0L)
                        {
                            ncs = safeexpr(ncs, init);
                        }

                        var r = nod(cmp, nod(OLEN, ncs, null), nodintconst(int64(len(s))));
                        var remains = len(s);
                        {
                            long i = 0L;

                            while (remains > 0L)
                            {
                                if (remains == 1L || !canCombineLoads)
                                {
                                    var cb = nodintconst(int64(s[i]));
                                    var ncb = nod(OINDEX, ncs, nodintconst(int64(i)));
                                    r = nod(and, r, nod(cmp, ncb, cb));
                                    remains--;
                                    i++;
                                    continue;
                                }

                                long step = default;
                                ptr<types.Type> convType;

                                if (remains >= 8L && combine64bit) 
                                    convType = types.Types[TINT64];
                                    step = 8L;
                                else if (remains >= 4L) 
                                    convType = types.Types[TUINT32];
                                    step = 4L;
                                else if (remains >= 2L) 
                                    convType = types.Types[TUINT16];
                                    step = 2L;
                                                                var ncsubstr = nod(OINDEX, ncs, nodintconst(int64(i)));
                                ncsubstr = conv(_addr_ncsubstr, convType);
                                var csubstr = int64(s[i]); 
                                // Calculate large constant from bytes as sequence of shifts and ors.
                                // Like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                                // ssa will combine this into a single large load.
                                for (long offset = 1L; offset < step; offset++)
                                {
                                    var b = nod(OINDEX, ncs, nodintconst(int64(i + offset)));
                                    b = conv(_addr_b, convType);
                                    b = nod(OLSH, b, nodintconst(int64(8L * offset)));
                                    ncsubstr = nod(OOR, ncsubstr, b);
                                    csubstr |= int64(s[i + offset]) << (int)(uint8(8L * offset));
                                }

                                var csubstrPart = nodintconst(csubstr); 
                                // Compare "step" bytes as once
                                r = nod(and, r, nod(cmp, csubstrPart, ncsubstr));
                                remains -= step;
                                i += step;

                            }

                        }
                        return _addr_finishcompare(_addr_n, _addr_r, _addr_init)!;

                    }

                }

            }

            r = ;
            if (n.Op == OEQ || n.Op == ONE)
            { 
                // prepare for rewrite below
                n.Left = cheapexpr(n.Left, init);
                n.Right = cheapexpr(n.Right, init);
                var (eqlen, eqmem) = eqstring(n.Left, n.Right); 
                // quick check of len before full compare for == or !=.
                // memequal then tests equality up to length len.
                if (n.Op == OEQ)
                { 
                    // len(left) == len(right) && memequal(left, right, len)
                    r = nod(OANDAND, eqlen, eqmem);

                }
                else
                { 
                    // len(left) != len(right) || !memequal(left, right, len)
                    eqlen.Op = ONE;
                    r = nod(OOROR, eqlen, nod(ONOT, eqmem, null));

                }

            }
            else
            { 
                // sys_cmpstring(s1, s2) :: 0
                r = mkcall("cmpstring", _addr_types.Types[TINT], _addr_init, _addr_conv(_addr_n.Left, _addr_types.Types[TSTRING]), conv(_addr_n.Right, _addr_types.Types[TSTRING]));
                r = nod(n.Op, r, nodintconst(0L));

            }

            return _addr_finishcompare(_addr_n, _addr_r, _addr_init)!;

        }

        // The result of finishcompare MUST be assigned back to n, e.g.
        //     n.Left = finishcompare(n.Left, x, r, init)
        private static ptr<Node> finishcompare(ptr<Node> _addr_n, ptr<Node> _addr_r, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Node r = ref _addr_r.val;
            ref Nodes init = ref _addr_init.val;

            r = typecheck(r, ctxExpr);
            r = conv(_addr_r, _addr_n.Type);
            r = walkexpr(_addr_r, _addr_init);
            return _addr_r!;
        }

        // return 1 if integer n must be in range [0, max), 0 otherwise
        private static bool bounded(ptr<Node> _addr_n, long max)
        {
            ref Node n = ref _addr_n.val;

            if (n.Type == null || !n.Type.IsInteger())
            {
                return false;
            }

            var sign = n.Type.IsSigned();
            var bits = int32(8L * n.Type.Width);

            if (smallintconst(n))
            {
                var v = n.Int64();
                return 0L <= v && v < max;
            }


            if (n.Op == OAND) 
                v = int64(-1L);
                if (smallintconst(n.Left))
                {
                    v = n.Left.Int64();
                }
                else if (smallintconst(n.Right))
                {
                    v = n.Right.Int64();
                }

                if (0L <= v && v < max)
                {
                    return true;
                }

            else if (n.Op == OMOD) 
                if (!sign && smallintconst(n.Right))
                {
                    v = n.Right.Int64();
                    if (0L <= v && v <= max)
                    {
                        return true;
                    }

                }

            else if (n.Op == ODIV) 
                if (!sign && smallintconst(n.Right))
                {
                    v = n.Right.Int64();
                    while (bits > 0L && v >= 2L)
                    {
                        bits--;
                        v >>= 1L;
                    }


                }

            else if (n.Op == ORSH) 
                if (!sign && smallintconst(n.Right))
                {
                    v = n.Right.Int64();
                    if (v > int64(bits))
                    {
                        return true;
                    }

                    bits -= int32(v);

                }

                        if (!sign && bits <= 62L && 1L << (int)(uint(bits)) <= max)
            {
                return true;
            }

            return false;

        }

        // usemethod checks interface method calls for uses of reflect.Type.Method.
        private static void usemethod(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var t = n.Left.Type; 

            // Looking for either of:
            //    Method(int) reflect.Method
            //    MethodByName(string) (reflect.Method, bool)
            //
            // TODO(crawshaw): improve precision of match by working out
            //                 how to check the method name.
            {
                var n__prev1 = n;

                var n = t.NumParams();

                if (n != 1L)
                {
                    return ;
                }

                n = n__prev1;

            }

            {
                var n__prev1 = n;

                n = t.NumResults();

                if (n != 1L && n != 2L)
                {
                    return ;
                }

                n = n__prev1;

            }

            var p0 = t.Params().Field(0L);
            var res0 = t.Results().Field(0L);
            ptr<types.Field> res1;
            if (t.NumResults() == 2L)
            {
                res1 = t.Results().Field(1L);
            }

            if (res1 == null)
            {
                if (p0.Type.Etype != TINT)
                {
                    return ;
                }

            }
            else
            {
                if (!p0.Type.IsString())
                {
                    return ;
                }

                if (!res1.Type.IsBoolean())
                {
                    return ;
                }

            } 

            // Note: Don't rely on res0.Type.String() since its formatting depends on multiple factors
            //       (including global variables such as numImports - was issue #19028).
            // Also need to check for reflect package itself (see Issue #38515).
            {
                var s = res0.Type.Sym;

                if (s != null && s.Name == "Method" && isReflectPkg(s.Pkg))
                {
                    Curfn.Func.SetReflectMethod(true);
                }

            }

        }

        private static void usefield(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (objabi.Fieldtrack_enabled == 0L)
            {
                return ;
            }


            if (n.Op == ODOT || n.Op == ODOTPTR) 
                break;
            else 
                Fatalf("usefield %v", n.Op);
                        if (n.Sym == null)
            { 
                // No field name.  This DOTPTR was built by the compiler for access
                // to runtime data structures.  Ignore.
                return ;

            }

            var t = n.Left.Type;
            if (t.IsPtr())
            {
                t = t.Elem();
            }

            var field = dotField[new typeSymKey(t.Orig,n.Sym)];
            if (field == null)
            {
                Fatalf("usefield %v %v without paramfld", n.Left.Type, n.Sym);
            }

            if (!strings.Contains(field.Note, "go:\"track\""))
            {
                return ;
            }

            var outer = n.Left.Type;
            if (outer.IsPtr())
            {
                outer = outer.Elem();
            }

            if (outer.Sym == null)
            {
                yyerror("tracked field must be in named struct type");
            }

            if (!types.IsExported(field.Sym.Name))
            {
                yyerror("tracked field must be exported (upper case)");
            }

            var sym = tracksym(outer, field);
            if (Curfn.Func.FieldTrack == null)
            {
                Curfn.Func.FieldTrack = make();
            }

            Curfn.Func.FieldTrack[sym] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

        }

        private static bool candiscardlist(Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                if (!candiscard(_addr_n))
                {
                    return false;
                }

            }
            return true;

        }

        private static bool candiscard(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return true;
            }


            if (n.Op == ONAME || n.Op == ONONAME || n.Op == OTYPE || n.Op == OPACK || n.Op == OLITERAL || n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OADDSTR || n.Op == OADDR || n.Op == OANDAND || n.Op == OBYTES2STR || n.Op == ORUNES2STR || n.Op == OSTR2BYTES || n.Op == OSTR2RUNES || n.Op == OCAP || n.Op == OCOMPLIT || n.Op == OMAPLIT || n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OPTRLIT || n.Op == OCONV || n.Op == OCONVIFACE || n.Op == OCONVNOP || n.Op == ODOT || n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE || n.Op == OKEY || n.Op == OSTRUCTKEY || n.Op == OLEN || n.Op == OMUL || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == ONEW || n.Op == ONOT || n.Op == OBITNOT || n.Op == OPLUS || n.Op == ONEG || n.Op == OOROR || n.Op == OPAREN || n.Op == ORUNESTR || n.Op == OREAL || n.Op == OIMAG || n.Op == OCOMPLEX) 
                break; 

                // Discardable as long as we know it's not division by zero.
            else if (n.Op == ODIV || n.Op == OMOD) 
                if (Isconst(n.Right, CTINT) && n.Right.Val().U._<ptr<Mpint>>().CmpInt64(0L) != 0L)
                {
                    break;
                }

                if (Isconst(n.Right, CTFLT) && n.Right.Val().U._<ptr<Mpflt>>().CmpFloat64(0L) != 0L)
                {
                    break;
                }

                return false; 

                // Discardable as long as we know it won't fail because of a bad size.
            else if (n.Op == OMAKECHAN || n.Op == OMAKEMAP) 
                if (Isconst(n.Left, CTINT) && n.Left.Val().U._<ptr<Mpint>>().CmpInt64(0L) == 0L)
                {
                    break;
                }

                return false; 

                // Difficult to tell what sizes are okay.
            else if (n.Op == OMAKESLICE) 
                return false;
            else if (n.Op == OMAKESLICECOPY) 
                return false;
            else 
                return false; 

                // Discardable as long as the subpieces are.
                        if (!candiscard(_addr_n.Left) || !candiscard(_addr_n.Right) || !candiscardlist(n.Ninit) || !candiscardlist(n.Nbody) || !candiscardlist(n.List) || !candiscardlist(n.Rlist))
            {
                return false;
            }

            return true;

        }

        // Rewrite
        //    go builtin(x, y, z)
        // into
        //    go func(a1, a2, a3) {
        //        builtin(a1, a2, a3)
        //    }(x, y, z)
        // for print, println, and delete.

        private static long wrapCall_prgen = default;

        // The result of wrapCall MUST be assigned back to n, e.g.
        //     n.Left = wrapCall(n.Left, init)
        private static ptr<Node> wrapCall(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(_addr_n.Ninit);
            }

            var t = nod(OTFUNC, null, null);
            foreach (var (i, arg) in n.List.Slice())
            {
                var s = lookupN("a", i);
                t.List.Append(symfield(s, arg.Type));
            }
            wrapCall_prgen++;
            var sym = lookupN("wrap·", wrapCall_prgen);
            var fn = dclfunc(sym, t);

            var a = nod(n.Op, null, null);
            a.List.Set(paramNnames(t.Type));
            a = typecheck(a, ctxStmt);
            fn.Nbody.Set1(a);

            funcbody();

            fn = typecheck(fn, ctxStmt);
            typecheckslice(fn.Nbody.Slice(), ctxStmt);
            xtop = append(xtop, fn);

            a = nod(OCALL, null, null);
            a.Left = fn.Func.Nname;
            a.List.Set(n.List.Slice());
            a = typecheck(a, ctxStmt);
            a = walkexpr(_addr_a, _addr_init);
            return _addr_a!;

        }

        // substArgTypes substitutes the given list of types for
        // successive occurrences of the "any" placeholder in the
        // type syntax expression n.Type.
        // The result of substArgTypes MUST be assigned back to old, e.g.
        //     n.Left = substArgTypes(n.Left, t1, t2)
        private static ptr<Node> substArgTypes(ptr<Node> _addr_old, params ptr<ptr<types.Type>>[] _addr_types_)
        {
            types_ = types_.Clone();
            ref Node old = ref _addr_old.val;
            ref types.Type types_ = ref _addr_types_.val;

            var n = old.copy();

            foreach (var (_, t) in types_)
            {
                dowidth(t);
            }
            n.Type = types.SubstAny(n.Type, _addr_types_);
            if (len(types_) > 0L)
            {
                Fatalf("substArgTypes: too many argument types");
            }

            return _addr_n!;

        }

        // canMergeLoads reports whether the backend optimization passes for
        // the current architecture can combine adjacent loads into a single
        // larger, possibly unaligned, load. Note that currently the
        // optimizations must be able to handle little endian byte order.
        private static bool canMergeLoads()
        {

            if (thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.AMD64 || thearch.LinkArch.Family == sys.I386 || thearch.LinkArch.Family == sys.S390X) 
                return true;
            else if (thearch.LinkArch.Family == sys.PPC64) 
                // Load combining only supported on ppc64le.
                return thearch.LinkArch.ByteOrder == binary.LittleEndian;
                        return false;

        }

        // isRuneCount reports whether n is of the form len([]rune(string)).
        // These are optimized into a call to runtime.countrunes.
        private static bool isRuneCount(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return Debug['N'] == 0L && !instrumenting && n.Op == OLEN && n.Left.Op == OSTR2RUNES;
        }

        private static ptr<Node> walkCheckPtrAlignment(ptr<Node> _addr_n, ptr<Nodes> _addr_init, ptr<Node> _addr_count)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
            ref Node count = ref _addr_count.val;

            if (!n.Type.IsPtr())
            {
                Fatalf("expected pointer type: %v", n.Type);
            }

            var elem = n.Type.Elem();
            if (count != null)
            {
                if (!elem.IsArray())
                {
                    Fatalf("expected array type: %v", elem);
                }

                elem = elem.Elem();

            }

            var size = elem.Size();
            if (elem.Alignment() == 1L && (size == 0L || size == 1L && count == null))
            {
                return _addr_n!;
            }

            if (count == null)
            {
                count = nodintconst(1L);
            }

            n.Left = cheapexpr(n.Left, init);
            init.Append(mkcall("checkptrAlignment", _addr_null, _addr_init, _addr_convnop(_addr_n.Left, _addr_types.Types[TUNSAFEPTR]), typename(elem), conv(_addr_count, _addr_types.Types[TUINTPTR])));
            return _addr_n!;

        }

        private static byte walkCheckPtrArithmeticMarker = default;

        private static ptr<Node> walkCheckPtrArithmetic(ptr<Node> _addr_n, ptr<Nodes> _addr_init) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;
 
            // Calling cheapexpr(n, init) below leads to a recursive call
            // to walkexpr, which leads us back here again. Use n.Opt to
            // prevent infinite loops.
            {
                var opt = n.Opt();

                if (opt == _addr_walkCheckPtrArithmeticMarker)
                {
                    return _addr_n!;
                }
                else if (opt != null)
                { 
                    // We use n.Opt() here because today it's not used for OCONVNOP. If that changes,
                    // there's no guarantee that temporarily replacing it is safe, so just hard fail here.
                    Fatalf("unexpected Opt: %v", opt);

                }


            }

            n.SetOpt(_addr_walkCheckPtrArithmeticMarker);
            defer(n.SetOpt(null)); 

            // TODO(mdempsky): Make stricter. We only need to exempt
            // reflect.Value.Pointer and reflect.Value.UnsafeAddr.

            if (n.Left.Op == OCALLFUNC || n.Left.Op == OCALLMETH || n.Left.Op == OCALLINTER) 
                return _addr_n!;
                        if (n.Left.Op == ODOTPTR && isReflectHeaderDataField(_addr_n.Left))
            {
                return _addr_n!;
            } 

            // Find original unsafe.Pointer operands involved in this
            // arithmetic expression.
            //
            // "It is valid both to add and to subtract offsets from a
            // pointer in this way. It is also valid to use &^ to round
            // pointers, usually for alignment."
            slice<ptr<Node>> originals = default;
            Action<ptr<Node>> walk = default;
            walk = n =>
            {

                if (n.Op == OADD) 
                    walk(_addr_n.Left);
                    walk(_addr_n.Right);
                else if (n.Op == OSUB) 
                    walk(_addr_n.Left);
                else if (n.Op == OAND) 
                    if (n.Implicit())
                    { // was OANDNOT
                        walk(_addr_n.Left);

                    }

                else if (n.Op == OCONVNOP) 
                    if (n.Left.Type.Etype == TUNSAFEPTR)
                    {
                        n.Left = cheapexpr(n.Left, init);
                        originals = append(originals, convnop(_addr_n.Left, _addr_types.Types[TUNSAFEPTR]));
                    }

                            }
;
            walk(_addr_n.Left);

            n = cheapexpr(n, init);

            var slice = mkdotargslice(_addr_types.NewSlice(types.Types[TUNSAFEPTR]), originals);
            slice.Esc = EscNone;

            init.Append(mkcall("checkptrArithmetic", _addr_null, _addr_init, _addr_convnop(_addr_n, _addr_types.Types[TUNSAFEPTR]), slice)); 
            // TODO(khr): Mark backing store of slice as dead. This will allow us to reuse
            // the backing store for multiple calls to checkptrArithmetic.

            return _addr_n!;

        });

        // checkPtr reports whether pointer checking should be enabled for
        // function fn at a given level. See debugHelpFooter for defined
        // levels.
        private static bool checkPtr(ptr<Node> _addr_fn, long level)
        {
            ref Node fn = ref _addr_fn.val;

            return Debug_checkptr >= level && fn.Func.Pragma & NoCheckPtr == 0L;
        }
    }
}}}}
