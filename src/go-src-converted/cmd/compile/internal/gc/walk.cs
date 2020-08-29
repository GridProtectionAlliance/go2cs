// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:30:09 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\walk.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
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
        private static readonly long tmpstringbufsize = 32L;



        private static void walk(ref Node fn)
        {
            Curfn = fn;

            if (Debug['W'] != 0L)
            {
                var s = fmt.Sprintf("\nbefore %v", Curfn.Func.Nname.Sym);
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
                        ln = typecheck(ln, Erv | Easgn);
                        fn.Func.Dcl[i] = ln;
                    }
                } 

                // Propagate the used flag for typeswitch variables up to the NONAME in it's definition.

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
                            yyerrorl(defn.Left.Pos, "%v declared and not used", ln.Sym);
                            defn.Left.Name.SetUsed(true); // suppress repeats
                        }
                        else
                        {
                            yyerrorl(ln.Pos, "%v declared and not used", ln.Sym);
                        }

                    }
                }

                ln = ln__prev1;
            }

            lineno = lno;
            if (nerrors != 0L)
            {
                return;
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

        private static void walkstmtlist(slice<ref Node> s)
        {
            foreach (var (i) in s)
            {
                s[i] = walkstmt(s[i]);
            }
        }

        private static bool samelist(slice<ref Node> a, slice<ref Node> b)
        {
            if (len(a) != len(b))
            {
                return false;
            }
            foreach (var (i, n) in a)
            {
                if (n != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool paramoutheap(ref Node fn)
        {
            foreach (var (_, ln) in fn.Func.Dcl)
            {

                if (ln.Class() == PPARAMOUT) 
                    if (ln.isParamStackCopy() || ln.Addrtaken())
                    {
                        return true;
                    }
                else if (ln.Class() == PAUTO) 
                    // stop early - parameters are over
                    return false;
                            }
            return false;
        }

        // adds "adjust" to all the argument locations for the call n.
        // n must be a defer or go node that has already been walked.
        private static void adjustargs(ref Node n, long adjust)
        {
            var callfunc = n.Left;
            foreach (var (_, arg) in callfunc.List.Slice())
            {
                if (arg.Op != OAS)
                {
                    Fatalf("call arg not assignment");
                }
                var lhs = arg.Left;
                if (lhs.Op == ONAME)
                { 
                    // This is a temporary introduced by reorder1.
                    // The real store to the stack appears later in the arg list.
                    continue;
                }
                if (lhs.Op != OINDREGSP)
                {
                    Fatalf("call argument store does not use OINDREGSP");
                } 

                // can't really check this in machine-indep code.
                //if(lhs->val.u.reg != D_SP)
                //      Fatalf("call arg assign not indreg(SP)")
                lhs.Xoffset += int64(adjust);
            }
        }

        // The result of walkstmt MUST be assigned back to n, e.g.
        //     n.Left = walkstmt(n.Left)
        private static ref Node walkstmt(ref Node n)
        {
            if (n == null)
            {
                return n;
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
                var init = n.Ninit;
                n.Ninit.Set(null);
                n = walkexpr(n, ref init);
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

                n.Left = walkexpr(n.Left, ref init);
                n = mkcall1(chanfn("chanrecv1", 2L, n.Left.Type), null, ref init, n.Left, nodnil());
                n = walkexpr(n, ref init);

                n = addinit(n, init.Slice());
                goto __switch_break0;
            }
            if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == OFALL || n.Op == OGOTO || n.Op == OLABEL || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OCHECKNIL || n.Op == OVARKILL || n.Op == OVARLIVE)
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
                        yyerror("%v escapes to heap, not allowed in runtime.", v);
                    }
                    if (prealloc[v] == null)
                    {
                        prealloc[v] = callnew(v.Type);
                    }
                    var nn = nod(OAS, v.Name.Param.Heapaddr, prealloc[v]);
                    nn.SetColas(true);
                    nn = typecheck(nn, Etop);
                    return walkstmt(nn);
                }
                goto __switch_break0;
            }
            if (n.Op == OBLOCK)
            {
                walkstmtlist(n.List.Slice());
                goto __switch_break0;
            }
            if (n.Op == OXCASE)
            {
                yyerror("case statement out of place");
                n.Op = OCASE;
                fallthrough = true;

            }
            if (fallthrough || n.Op == OCASE)
            {
                n.Right = walkstmt(n.Right);
                goto __switch_break0;
            }
            if (n.Op == ODEFER)
            {
                Curfn.Func.SetHasDefer(true);

                if (n.Left.Op == OPRINT || n.Left.Op == OPRINTN) 
                    n.Left = walkprintfunc(n.Left, ref n.Ninit);
                else if (n.Left.Op == OCOPY) 
                    n.Left = copyany(n.Left, ref n.Ninit, true);
                else 
                    n.Left = walkexpr(n.Left, ref n.Ninit);
                // make room for size & fn arguments.
                adjustargs(n, 2L * Widthptr);
                goto __switch_break0;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL)
            {
                if (n.Left != null)
                {
                    walkstmtlist(n.Left.Ninit.Slice());
                    init = n.Left.Ninit;
                    n.Left.Ninit.Set(null);
                    n.Left = walkexpr(n.Left, ref init);
                    n.Left = addinit(n.Left, init.Slice());
                }
                n.Right = walkstmt(n.Right);
                walkstmtlist(n.Nbody.Slice());
                goto __switch_break0;
            }
            if (n.Op == OIF)
            {
                n.Left = walkexpr(n.Left, ref n.Ninit);
                walkstmtlist(n.Nbody.Slice());
                walkstmtlist(n.Rlist.Slice());
                goto __switch_break0;
            }
            if (n.Op == OPROC)
            {

                if (n.Left.Op == OPRINT || n.Left.Op == OPRINTN) 
                    n.Left = walkprintfunc(n.Left, ref n.Ninit);
                else if (n.Left.Op == OCOPY) 
                    n.Left = copyany(n.Left, ref n.Ninit, true);
                else 
                    n.Left = walkexpr(n.Left, ref n.Ninit);
                // make room for size & fn arguments.
                adjustargs(n, 2L * Widthptr);
                goto __switch_break0;
            }
            if (n.Op == ORETURN)
            {
                walkexprlist(n.List.Slice(), ref n.Ninit);
                if (n.List.Len() == 0L)
                {
                    break;
                }
                if ((Curfn.Type.FuncType().Outnamed && n.List.Len() > 1L) || paramoutheap(Curfn))
                { 
                    // assign to the function out parameters,
                    // so that reorder3 can fix up conflicts
                    slice<ref Node> rl = default;

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
                                ln = walkexpr(typecheck(nod(OIND, ln.Name.Param.Heapaddr, null), Erv), null);
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

                    }

                    if (samelist(rl, n.List.Slice()))
                    { 
                        // special return in disguise
                        n.List.Set(null);

                        break;
                    } 

                    // move function calls out, to make reorder3's job easier.
                    walkexprlistsafe(n.List.Slice(), ref n.Ninit);

                    var ll = ascompatee(n.Op, rl, n.List.Slice(), ref n.Ninit);
                    n.List.Set(reorder3(ll));
                    break;
                }
                ll = ascompatte(null, false, Curfn.Type.Results(), n.List.Slice(), 1L, ref n.Ninit);
                n.List.Set(ll);
                goto __switch_break0;
            }
            if (n.Op == ORETJMP)
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
            return n;
        }

        private static bool isSmallMakeSlice(ref Node n)
        {
            if (n.Op != OMAKESLICE)
            {
                return false;
            }
            var l = n.Left;
            var r = n.Right;
            if (r == null)
            {
                r = l;
            }
            var t = n.Type;

            return smallintconst(l) && smallintconst(r) && (t.Elem().Width == 0L || r.Int64() < (1L << (int)(16L)) / t.Elem().Width);
        }

        // walk the whole tree of the body of an
        // expression or simple statement.
        // the types expressions are calculated.
        // compile-time constants are evaluated.
        // complex side effects like statements are appended to init
        private static void walkexprlist(slice<ref Node> s, ref Nodes init)
        {
            foreach (var (i) in s)
            {
                s[i] = walkexpr(s[i], init);
            }
        }

        private static void walkexprlistsafe(slice<ref Node> s, ref Nodes init)
        {
            foreach (var (i, n) in s)
            {
                s[i] = safeexpr(n, init);
                s[i] = walkexpr(s[i], init);
            }
        }

        private static void walkexprlistcheap(slice<ref Node> s, ref Nodes init)
        {
            foreach (var (i, n) in s)
            {
                s[i] = cheapexpr(n, init);
                s[i] = walkexpr(s[i], init);
            }
        }

        // Build name of function for interface conversion.
        // Not all names are possible
        // (e.g., we'll never generate convE2E or convE2I or convI2E).
        private static @string convFuncName(ref types.Type _from, ref types.Type _to) => func(_from, _to, (ref types.Type from, ref types.Type to, Defer _, Panic panic, Recover __) =>
        {
            var tkind = to.Tie();
            switch (from.Tie())
            {
                case 'I': 
                    switch (tkind)
                    {
                        case 'I': 
                            return "convI2I";
                            break;
                    }
                    break;
                case 'T': 
                    switch (tkind)
                    {
                        case 'E': 

                            if (from.Size() == 2L && from.Align == 2L) 
                                return "convT2E16";
                            else if (from.Size() == 4L && from.Align == 4L && !types.Haspointers(from)) 
                                return "convT2E32";
                            else if (from.Size() == 8L && from.Align == types.Types[TUINT64].Align && !types.Haspointers(from)) 
                                return "convT2E64";
                            else if (from.IsString()) 
                                return "convT2Estring";
                            else if (from.IsSlice()) 
                                return "convT2Eslice";
                            else if (!types.Haspointers(from)) 
                                return "convT2Enoptr";
                                                return "convT2E";
                            break;
                        case 'I': 

                            if (from.Size() == 2L && from.Align == 2L) 
                                return "convT2I16";
                            else if (from.Size() == 4L && from.Align == 4L && !types.Haspointers(from)) 
                                return "convT2I32";
                            else if (from.Size() == 8L && from.Align == types.Types[TUINT64].Align && !types.Haspointers(from)) 
                                return "convT2I64";
                            else if (from.IsString()) 
                                return "convT2Istring";
                            else if (from.IsSlice()) 
                                return "convT2Islice";
                            else if (!types.Haspointers(from)) 
                                return "convT2Inoptr";
                                                return "convT2I";
                            break;
                    }
                    break;
            }
            Fatalf("unknown conv func %c2%c", from.Tie(), to.Tie());
            panic("unreachable");
        });

        // The result of walkexpr MUST be assigned back to n, e.g.
        //     n.Left = walkexpr(n.Left, init)
        private static ref Node walkexpr(ref Node n, ref Nodes init)
        {
            if (n == null)
            {
                return n;
            } 

            // Eagerly checkwidth all expressions for the back end.
            if (n.Type != null && !n.Type.WidthCalculated())
            {

                if (n.Type.Etype == TBLANK || n.Type.Etype == TNIL || n.Type.Etype == TIDEAL)                 else 
                    checkwidth(n.Type);
                            }
            if (init == ref n.Ninit)
            { 
                // not okay to use n->ninit when walking n,
                // because we might replace n with some other node
                // and would lose the init list.
                Fatalf("walkexpr init == &n->ninit");
            }
            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(ref n.Ninit);
            }
            var lno = setlineno(n);

            if (Debug['w'] > 1L)
            {
                Dump("walk-before", n);
            }
            if (n.Typecheck() != 1L)
            {
                Fatalf("missed typecheck: %+v", n);
            }
            if (n.Op == ONAME && n.Class() == PAUTOHEAP)
            {
                var nn = nod(OIND, n.Name.Param.Heapaddr, null);
                nn = typecheck(nn, Erv);
                nn = walkexpr(nn, init);
                nn.Left.SetNonNil(true);
                return nn;
            }
opswitch: 

            // Expressions that are constant at run time but not
            // considered const by the language spec are not turned into
            // constants until walk. For example, if n is y%1 == 0, the
            // walk of y%1 may have replaced it by 0.
            // Check whether n with its updated args is itself now a constant.

            if (n.Op == ONONAME || n.Op == OINDREGSP || n.Op == OEMPTY || n.Op == OGETG)             else if (n.Op == OTYPE || n.Op == ONAME || n.Op == OLITERAL)             else if (n.Op == ONOT || n.Op == OMINUS || n.Op == OPLUS || n.Op == OCOM || n.Op == OREAL || n.Op == OIMAG || n.Op == ODOTMETH || n.Op == ODOTINTER || n.Op == OIND || n.Op == OSPTR || n.Op == OITAB || n.Op == OIDATA || n.Op == OADDR) 
                n.Left = walkexpr(n.Left, init);
            else if (n.Op == OEFACE || n.Op == OAND || n.Op == OSUB || n.Op == OMUL || n.Op == OLT || n.Op == OLE || n.Op == OGE || n.Op == OGT || n.Op == OADD || n.Op == OOR || n.Op == OXOR) 
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init);
            else if (n.Op == ODOT) 
                usefield(n);
                n.Left = walkexpr(n.Left, init);
            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                n.Left = walkexpr(n.Left, init); 
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
            else if (n.Op == ODOTPTR) 
                usefield(n);
                if (n.Op == ODOTPTR && n.Left.Type.Elem().Width == 0L)
                { 
                    // No actual copy will be generated, so emit an explicit nil check.
                    n.Left = cheapexpr(n.Left, init);

                    checknil(n.Left, init);
                }
                n.Left = walkexpr(n.Left, init);
            else if (n.Op == OLEN || n.Op == OCAP) 
                n.Left = walkexpr(n.Left, init); 

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
                    nodconst(n, n.Type, t.NumElem());
                    n.SetTypecheck(1L);
                }
            else if (n.Op == OLSH || n.Op == ORSH) 
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init);
                t = n.Left.Type;
                n.SetBounded(bounded(n.Right, 8L * t.Width));
                if (Debug['m'] != 0L && n.Etype != 0L && !Isconst(n.Right, CTINT))
                {
                    Warn("shift bounds check elided");
                }
            else if (n.Op == OCOMPLEX) 
                // Use results from call expression as arguments for complex.
                if (n.Left == null && n.Right == null)
                {
                    n.Left = n.List.First();
                    n.Right = n.List.Second();
                }
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init);
            else if (n.Op == OEQ || n.Op == ONE) 
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init); 

                // Disable safemode while compiling this code: the code we
                // generate internally can refer to unsafe.Pointer.
                // In this case it can happen if we need to generate an ==
                // for a struct containing a reflect.Value, which itself has
                // an unexported field of type unsafe.Pointer.
                var old_safemode = safemode;
                safemode = false;
                n = walkcompare(n, init);
                safemode = old_safemode;
            else if (n.Op == OANDAND || n.Op == OOROR) 
                n.Left = walkexpr(n.Left, init); 

                // cannot put side effects from n.Right on init,
                // because they cannot run before n.Left is checked.
                // save elsewhere and store on the eventual n.Right.
                Nodes ll = default;

                n.Right = walkexpr(n.Right, ref ll);
                n.Right = addinit(n.Right, ll.Slice());
                n = walkinrange(n, init);
            else if (n.Op == OPRINT || n.Op == OPRINTN) 
                walkexprlist(n.List.Slice(), init);
                n = walkprint(n, init);
            else if (n.Op == OPANIC) 
                n = mkcall("gopanic", null, init, n.Left);
            else if (n.Op == ORECOVER) 
                n = mkcall("gorecover", n.Type, init, nod(OADDR, nodfp, null));
            else if (n.Op == OCLOSUREVAR || n.Op == OCFUNC) 
                n.SetAddable(true);
            else if (n.Op == OCALLINTER) 
                usemethod(n);
                t = n.Left.Type;
                if (n.List.Len() != 0L && n.List.First().Op == OAS)
                {
                    break;
                }
                n.Left = walkexpr(n.Left, init);
                walkexprlist(n.List.Slice(), init);
                ll = ascompatte(n, n.Isddd(), t.Params(), n.List.Slice(), 0L, init);
                n.List.Set(reorder1(ll));
            else if (n.Op == OCALLFUNC) 
                if (n.Left.Op == OCLOSURE)
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
                t = n.Left.Type;
                if (n.List.Len() != 0L && n.List.First().Op == OAS)
                {
                    break;
                }
                n.Left = walkexpr(n.Left, init);
                walkexprlist(n.List.Slice(), init);

                ll = ascompatte(n, n.Isddd(), t.Params(), n.List.Slice(), 0L, init);
                n.List.Set(reorder1(ll));
            else if (n.Op == OCALLMETH) 
                t = n.Left.Type;
                if (n.List.Len() != 0L && n.List.First().Op == OAS)
                {
                    break;
                }
                n.Left = walkexpr(n.Left, init);
                walkexprlist(n.List.Slice(), init);
                ll = ascompatte(n, false, t.Recvs(), new slice<ref Node>(new ref Node[] { n.Left.Left }), 0L, init);
                var lr = ascompatte(n, n.Isddd(), t.Params(), n.List.Slice(), 0L, init);
                ll = append(ll, lr);
                n.Left.Left = null;
                updateHasCall(n.Left);
                n.List.Set(reorder1(ll));
            else if (n.Op == OAS) 
                init.AppendNodes(ref n.Ninit);

                n.Left = walkexpr(n.Left, init);
                n.Left = safeexpr(n.Left, init);

                if (oaslit(n, init))
                {
                    break;
                }
                if (n.Right == null)
                { 
                    // TODO(austin): Check all "implicit zeroing"
                    break;
                }
                if (!instrumenting && iszero(n.Right))
                {
                    break;
                }

                if (n.Right.Op == ORECV) 
                    // x = <-c; n.Left is x, n.Right.Left is c.
                    // orderstmt made sure x is addressable.
                    n.Right.Left = walkexpr(n.Right.Left, init);

                    var n1 = nod(OADDR, n.Left, null);
                    var r = n.Right.Left; // the channel
                    n = mkcall1(chanfn("chanrecv1", 2L, r.Type), null, init, r, n1);
                    n = walkexpr(n, init);
                    _breakopswitch = true;

                    break;
                else if (n.Right.Op == OAPPEND) 
                    // x = append(...)
                    r = n.Right;
                    if (r.Type.Elem().NotInHeap())
                    {
                        yyerror("%v is go:notinheap; heap allocation disallowed", r.Type.Elem());
                    }
                    if (r.Isddd())
                    {
                        r = appendslice(r, init); // also works for append(slice, string).
                    }
                    else
                    {
                        r = walkappend(r, init, n);
                    }
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
                    n.Right = walkexpr(n.Right, init);
                                if (n.Left != null && n.Right != null)
                {
                    n = convas(n, init);
                }
            else if (n.Op == OAS2) 
                init.AppendNodes(ref n.Ninit);
                walkexprlistsafe(n.List.Slice(), init);
                walkexprlistsafe(n.Rlist.Slice(), init);
                ll = ascompatee(OAS, n.List.Slice(), n.Rlist.Slice(), init);
                ll = reorder3(ll);
                n = liststmt(ll); 

                // a,b,... = fn()
            else if (n.Op == OAS2FUNC) 
                init.AppendNodes(ref n.Ninit);

                r = n.Rlist.First();
                walkexprlistsafe(n.List.Slice(), init);
                r = walkexpr(r, init);

                if (isIntrinsicCall(r))
                {
                    n.Rlist.Set1(r);
                    break;
                }
                init.Append(r);

                ll = ascompatet(n.List, r.Type);
                n = liststmt(ll); 

                // x, y = <-c
                // orderstmt made sure x is addressable.
            else if (n.Op == OAS2RECV) 
                init.AppendNodes(ref n.Ninit);

                r = n.Rlist.First();
                walkexprlistsafe(n.List.Slice(), init);
                r.Left = walkexpr(r.Left, init);
                n1 = default;
                if (isblank(n.List.First()))
                {
                    n1 = nodnil();
                }
                else
                {
                    n1 = nod(OADDR, n.List.First(), null);
                }
                n1.Etype = 1L; // addr does not escape
                var fn = chanfn("chanrecv2", 2L, r.Left.Type);
                var ok = n.List.Second();
                var call = mkcall1(fn, ok.Type, init, r.Left, n1);
                n = nod(OAS, ok, call);
                n = typecheck(n, Etop); 

                // a,b = m[i]
            else if (n.Op == OAS2MAPR) 
                init.AppendNodes(ref n.Ninit);

                r = n.Rlist.First();
                walkexprlistsafe(n.List.Slice(), init);
                r.Left = walkexpr(r.Left, init);
                r.Right = walkexpr(r.Right, init);
                t = r.Left.Type;

                var fast = mapfast(t);
                ref Node key = default;
                if (fast != mapslow)
                { 
                    // fast versions take key by value
                    key = r.Right;
                }
                else
                { 
                    // standard version takes key by reference
                    // orderexpr made sure key is addressable.
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

                    var w = t.Val().Width;

                    if (w <= 1024L)
                    { // 1024 must match ../../../../runtime/hashmap.go:maxZero
                        fn = mapfn(mapaccess2[fast], t);
                        r = mkcall1(fn, fn.Type.Results(), init, typename(t), r.Left, key);
                    }
                    else
                    {
                        fn = mapfn("mapaccess2_fat", t);
                        var z = zeroaddr(w);
                        r = mkcall1(fn, fn.Type.Results(), init, typename(t), r.Left, key, z);
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

                    if (!isblank(ok) && ok.Type.IsBoolean())
                    {
                        r.Type.Field(1L).Type;

                        ok.Type;
                    }

                    ok = ok__prev1;

                }
                n.Rlist.Set1(r);
                n.Op = OAS2FUNC; 

                // don't generate a = *var if a is _
                if (!isblank(a))
                {
                    var var_ = temp(types.NewPtr(t.Val()));
                    var_.SetTypecheck(1L);
                    var_.SetNonNil(true); // mapaccess always returns a non-nil pointer
                    n.List.SetFirst(var_);
                    n = walkexpr(n, init);
                    init.Append(n);
                    n = nod(OAS, a, nod(OIND, var_, null));
                }
                n = typecheck(n, Etop);
                n = walkexpr(n, init);
            else if (n.Op == ODELETE) 
                init.AppendNodes(ref n.Ninit);
                var map_ = n.List.First();
                key = n.List.Second();
                map_ = walkexpr(map_, init);
                key = walkexpr(key, init);

                t = map_.Type;
                fast = mapfast(t);
                if (fast == mapslow)
                { 
                    // orderstmt made sure key is addressable.
                    key = nod(OADDR, key, null);
                }
                n = mkcall1(mapfndel(mapdelete[fast], t), null, init, typename(t), map_, key);
            else if (n.Op == OAS2DOTTYPE) 
                walkexprlistsafe(n.List.Slice(), init);
                n.Rlist.SetFirst(walkexpr(n.Rlist.First(), init));
            else if (n.Op == OCONVIFACE) 
                n.Left = walkexpr(n.Left, init); 

                // Optimize convT2E or convT2I as a two-word copy when T is pointer-shaped.
                if (isdirectiface(n.Left.Type))
                {
                    t = default;
                    if (n.Type.IsEmptyInterface())
                    {
                        t = typename(n.Left.Type);
                    }
                    else
                    {
                        t = itabname(n.Left.Type, n.Type);
                    }
                    var l = nod(OEFACE, t, n.Left);
                    l.Type = n.Type;
                    l.SetTypecheck(n.Typecheck());
                    n = l;
                    break;
                }
                if (staticbytes == null)
                {
                    staticbytes = newname(Runtimepkg.Lookup("staticbytes"));
                    staticbytes.SetClass(PEXTERN);
                    staticbytes.Type = types.NewArray(types.Types[TUINT8], 256L);
                    zerobase = newname(Runtimepkg.Lookup("zerobase"));
                    zerobase.SetClass(PEXTERN);
                    zerobase.Type = types.Types[TUINTPTR];
                } 

                // Optimize convT2{E,I} for many cases in which T is not pointer-shaped,
                // by using an existing addressable value identical to n.Left
                // or creating one on the stack.
                ref Node value = default;

                if (n.Left.Type.Size() == 0L) 
                    // n.Left is zero-sized. Use zerobase.
                    cheapexpr(n.Left, init); // Evaluate n.Left for side-effects. See issue 19246.
                    value = zerobase;
                else if (n.Left.Type.IsBoolean() || (n.Left.Type.Size() == 1L && n.Left.Type.IsInteger())) 
                    // n.Left is a bool/byte. Use staticbytes[n.Left].
                    n.Left = cheapexpr(n.Left, init);
                    value = nod(OINDEX, staticbytes, byteindex(n.Left));
                    value.SetBounded(true);
                else if (n.Left.Class() == PEXTERN && n.Left.Name != null && n.Left.Name.Readonly()) 
                    // n.Left is a readonly global; use it directly.
                    value = n.Left;
                else if (!n.Left.Type.IsInterface() && n.Esc == EscNone && n.Left.Type.Width <= 1024L) 
                    // n.Left does not escape. Use a stack temporary initialized to n.Left.
                    value = temp(n.Left.Type);
                    init.Append(typecheck(nod(OAS, value, n.Left), Etop));
                                if (value != null)
                { 
                    // Value is identical to n.Left.
                    // Construct the interface directly: {type/itab, &value}.
                    t = default;
                    if (n.Type.IsEmptyInterface())
                    {
                        t = typename(n.Left.Type);
                    }
                    else
                    {
                        t = itabname(n.Left.Type, n.Type);
                    }
                    l = nod(OEFACE, t, typecheck(nod(OADDR, value, null), Erv));
                    l.Type = n.Type;
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
                if (n.Type.IsEmptyInterface() && n.Left.Type.IsInterface() && !n.Left.Type.IsEmptyInterface())
                { 
                    // Evaluate the input interface.
                    var c = temp(n.Left.Type);
                    init.Append(nod(OAS, c, n.Left)); 

                    // Get the itab out of the interface.
                    var tmp = temp(types.NewPtr(types.Types[TUINT8]));
                    init.Append(nod(OAS, tmp, typecheck(nod(OITAB, c, null), Erv))); 

                    // Get the type out of the itab.
                    var nif = nod(OIF, typecheck(nod(ONE, tmp, nodnil()), Erv), null);
                    nif.Nbody.Set1(nod(OAS, tmp, itabType(tmp)));
                    init.Append(nif); 

                    // Build the result.
                    var e = nod(OEFACE, tmp, ifaceData(c, types.NewPtr(types.Types[TUINT8])));
                    e.Type = n.Type; // assign type manually, typecheck doesn't understand OEFACE.
                    e.SetTypecheck(1L);
                    n = e;
                    break;
                }
                ll = default;
                if (n.Type.IsEmptyInterface())
                {
                    if (!n.Left.Type.IsInterface())
                    {
                        ll = append(ll, typename(n.Left.Type));
                    }
                }
                else
                {
                    if (n.Left.Type.IsInterface())
                    {
                        ll = append(ll, typename(n.Type));
                    }
                    else
                    {
                        ll = append(ll, itabname(n.Left.Type, n.Type));
                    }
                }
                if (n.Left.Type.IsInterface())
                {
                    ll = append(ll, n.Left);
                }
                else
                { 
                    // regular types are passed by reference to avoid C vararg calls
                    // orderexpr arranged for n.Left to be a temporary for all
                    // the conversions it could see. comparison of an interface
                    // with a non-interface, especially in a switch on interface value
                    // with non-interface cases, is not visible to orderstmt, so we
                    // have to fall back on allocating a temp here.
                    if (islvalue(n.Left))
                    {
                        ll = append(ll, nod(OADDR, n.Left, null));
                    }
                    else
                    {
                        ll = append(ll, nod(OADDR, copyexpr(n.Left, n.Left.Type, init), null));
                    }
                    dowidth(n.Left.Type);
                }
                fn = syslook(convFuncName(n.Left.Type, n.Type));
                fn = substArgTypes(fn, n.Left.Type, n.Type);
                dowidth(fn.Type);
                n = nod(OCALL, fn, null);
                n.List.Set(ll);
                n = typecheck(n, Erv);
                n = walkexpr(n, init);
            else if (n.Op == OCONV || n.Op == OCONVNOP) 
                if (thearch.SoftFloat)
                { 
                    // For the soft-float case, ssa.go handles these conversions.
                    goto oconv_walkexpr;
                }

                if (thearch.LinkArch.Family == sys.ARM || thearch.LinkArch.Family == sys.MIPS) 
                    if (n.Left.Type.IsFloat())
                    {

                        if (n.Type.Etype == TINT64) 
                            n = mkcall("float64toint64", n.Type, init, conv(n.Left, types.Types[TFLOAT64]));
                            _breakopswitch = true;
                            break;
                        else if (n.Type.Etype == TUINT64) 
                            n = mkcall("float64touint64", n.Type, init, conv(n.Left, types.Types[TFLOAT64]));
                            _breakopswitch = true;
                            break;
                                            }
                    if (n.Type.IsFloat())
                    {

                        if (n.Left.Type.Etype == TINT64) 
                            n = conv(mkcall("int64tofloat64", types.Types[TFLOAT64], init, conv(n.Left, types.Types[TINT64])), n.Type);
                            _breakopswitch = true;
                            break;
                        else if (n.Left.Type.Etype == TUINT64) 
                            n = conv(mkcall("uint64tofloat64", types.Types[TFLOAT64], init, conv(n.Left, types.Types[TUINT64])), n.Type);
                            _breakopswitch = true;
                            break;
                                            }
                else if (thearch.LinkArch.Family == sys.I386) 
                    if (n.Left.Type.IsFloat())
                    {

                        if (n.Type.Etype == TINT64) 
                            n = mkcall("float64toint64", n.Type, init, conv(n.Left, types.Types[TFLOAT64]));
                            _breakopswitch = true;
                            break;
                        else if (n.Type.Etype == TUINT64) 
                            n = mkcall("float64touint64", n.Type, init, conv(n.Left, types.Types[TFLOAT64]));
                            _breakopswitch = true;
                            break;
                        else if (n.Type.Etype == TUINT32 || n.Type.Etype == TUINT || n.Type.Etype == TUINTPTR) 
                            n = mkcall("float64touint32", n.Type, init, conv(n.Left, types.Types[TFLOAT64]));
                            _breakopswitch = true;
                            break;
                                            }
                    if (n.Type.IsFloat())
                    {

                        if (n.Left.Type.Etype == TINT64) 
                            n = conv(mkcall("int64tofloat64", types.Types[TFLOAT64], init, conv(n.Left, types.Types[TINT64])), n.Type);
                            _breakopswitch = true;
                            break;
                        else if (n.Left.Type.Etype == TUINT64) 
                            n = conv(mkcall("uint64tofloat64", types.Types[TFLOAT64], init, conv(n.Left, types.Types[TUINT64])), n.Type);
                            _breakopswitch = true;
                            break;
                        else if (n.Left.Type.Etype == TUINT32 || n.Left.Type.Etype == TUINT || n.Left.Type.Etype == TUINTPTR) 
                            n = conv(mkcall("uint32tofloat64", types.Types[TFLOAT64], init, conv(n.Left, types.Types[TUINT32])), n.Type);
                            _breakopswitch = true;
                            break;
                                            }
                oconv_walkexpr:

                n.Left = walkexpr(n.Left, init);
            else if (n.Op == OANDNOT) 
                n.Left = walkexpr(n.Left, init);
                n.Op = OAND;
                n.Right = nod(OCOM, n.Right, null);
                n.Right = typecheck(n.Right, Erv);
                n.Right = walkexpr(n.Right, init);
            else if (n.Op == ODIV || n.Op == OMOD) 
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init); 

                // rewrite complex div into function call.
                var et = n.Left.Type.Etype;

                if (isComplex[et] && n.Op == ODIV)
                {
                    t = n.Type;
                    n = mkcall("complex128div", types.Types[TCOMPLEX128], init, conv(n.Left, types.Types[TCOMPLEX128]), conv(n.Right, types.Types[TCOMPLEX128]));
                    n = conv(n, t);
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
                    n = mkcall(fn, n.Type, init, conv(n.Left, types.Types[et]), conv(n.Right, types.Types[et]));
                }
            else if (n.Op == OINDEX) 
                n.Left = walkexpr(n.Left, init); 

                // save the original node for bounds checking elision.
                // If it was a ODIV/OMOD walk might rewrite it.
                r = n.Right;

                n.Right = walkexpr(n.Right, init); 

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
                    n.SetBounded(bounded(r, t.NumElem()));
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
                    n.SetBounded(bounded(r, int64(len(n.Left.Val().U._<@string>()))));
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
                    if (n.Right.Val().U._<ref Mpint>().CmpInt64(0L) < 0L || n.Right.Val().U._<ref Mpint>().Cmp(maxintval[TINT]) > 0L)
                    {
                        yyerror("index out of bounds");
                    }
                }
            else if (n.Op == OINDEXMAP) 
                // Replace m[k] with *map{access1,assign}(maptype, m, &k)
                n.Left = walkexpr(n.Left, init);
                n.Right = walkexpr(n.Right, init);
                map_ = n.Left;
                key = n.Right;
                t = map_.Type;
                if (n.Etype == 1L)
                { 
                    // This m[k] expression is on the left-hand side of an assignment.
                    fast = mapfast(t);
                    if (fast == mapslow)
                    { 
                        // standard version takes key by reference.
                        // orderexpr made sure key is addressable.
                        key = nod(OADDR, key, null);
                    }
                    n = mkcall1(mapfn(mapassign[fast], t), null, init, typename(t), map_, key);
                }
                else
                { 
                    // m[k] is not the target of an assignment.
                    fast = mapfast(t);
                    if (fast == mapslow)
                    { 
                        // standard version takes key by reference.
                        // orderexpr made sure key is addressable.
                        key = nod(OADDR, key, null);
                    }
                    {
                        var w__prev2 = w;

                        w = t.Val().Width;

                        if (w <= 1024L)
                        { // 1024 must match ../../../../runtime/hashmap.go:maxZero
                            n = mkcall1(mapfn(mapaccess1[fast], t), types.NewPtr(t.Val()), init, typename(t), map_, key);
                        }
                        else
                        {
                            z = zeroaddr(w);
                            n = mkcall1(mapfn("mapaccess1_fat", t), types.NewPtr(t.Val()), init, typename(t), map_, key, z);
                        }

                        w = w__prev2;

                    }
                }
                n.Type = types.NewPtr(t.Val());
                n.SetNonNil(true); // mapaccess1* and mapassign always return non-nil pointers.
                n = nod(OIND, n, null);
                n.Type = t.Val();
                n.SetTypecheck(1L);
            else if (n.Op == ORECV) 
                Fatalf("walkexpr ORECV"); // should see inside OAS only
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR || n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                n.Left = walkexpr(n.Left, init);
                var (low, high, max) = n.SliceBounds();
                low = walkexpr(low, init);
                if (low != null && iszero(low))
                { 
                    // Reduce x[0:j] to x[:j] and x[0:j:k] to x[:j:k].
                    low = null;
                }
                high = walkexpr(high, init);
                max = walkexpr(max, init);
                n.SetSliceBounds(low, high, max);
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
                        n = reduceSlice(n);
                    }
                }
                else
                {
                    n = reduceSlice(n);
                }
            else if (n.Op == ONEW) 
                if (n.Esc == EscNone)
                {
                    if (n.Type.Elem().Width >= 1L << (int)(16L))
                    {
                        Fatalf("large ONEW with EscNone: %v", n);
                    }
                    r = temp(n.Type.Elem());
                    r = nod(OAS, r, null); // zero temp
                    r = typecheck(r, Etop);
                    init.Append(r);
                    r = nod(OADDR, r.Left, null);
                    r = typecheck(r, Erv);
                    n = r;
                }
                else
                {
                    n = callnew(n.Type.Elem());
                }
            else if (n.Op == OCMPSTR) 
                // s + "badgerbadgerbadger" == "badgerbadgerbadger"
                if ((Op(n.Etype) == OEQ || Op(n.Etype) == ONE) && Isconst(n.Right, CTSTR) && n.Left.Op == OADDSTR && n.Left.List.Len() == 2L && Isconst(n.Left.List.Second(), CTSTR) && strlit(n.Right) == strlit(n.Left.List.Second()))
                { 
                    // TODO(marvin): Fix Node.EType type union.
                    r = nod(Op(n.Etype), nod(OLEN, n.Left.List.First(), null), nodintconst(0L));
                    r = typecheck(r, Erv);
                    r = walkexpr(r, init);
                    r.Type = n.Type;
                    n = r;
                    break;
                } 

                // Rewrite comparisons to short constant strings as length+byte-wise comparisons.
                ref Node cs = default;                ref Node ncs = default; // const string, non-const string
 // const string, non-const string

                if (Isconst(n.Left, CTSTR) && Isconst(n.Right, CTSTR))                 else if (Isconst(n.Left, CTSTR)) 
                    cs = n.Left;
                    ncs = n.Right;
                else if (Isconst(n.Right, CTSTR)) 
                    cs = n.Right;
                    ncs = n.Left;
                                if (cs != null)
                {
                    var cmp = Op(n.Etype); 
                    // maxRewriteLen was chosen empirically.
                    // It is the value that minimizes cmd/go file size
                    // across most architectures.
                    // See the commit description for CL 26758 for details.
                    long maxRewriteLen = 6L; 
                    // Some architectures can load unaligned byte sequence as 1 word.
                    // So we can cover longer strings with the same amount of code.
                    var canCombineLoads = false;
                    var combine64bit = false; 
                    // TODO: does this improve performance on any other architectures?

                    if (thearch.LinkArch.Family == sys.AMD64) 
                        // Larger compare require longer instructions, so keep this reasonably low.
                        // Data from CL 26758 shows that longer strings are rare.
                        // If we really want we can do 16 byte SSE comparisons in the future.
                        maxRewriteLen = 16L;
                        canCombineLoads = true;
                        combine64bit = true;
                    else if (thearch.LinkArch.Family == sys.I386) 
                        maxRewriteLen = 8L;
                        canCombineLoads = true;
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
                        @string s__prev2 = s;

                        @string s = cs.Val().U._<@string>();

                        if (len(s) <= maxRewriteLen)
                        {
                            if (len(s) > 0L)
                            {
                                ncs = safeexpr(ncs, init);
                            } 
                            // TODO(marvin): Fix Node.EType type union.
                            r = nod(cmp, nod(OLEN, ncs, null), nodintconst(int64(len(s))));
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
                                    ref types.Type convType = default;

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
                                    ncsubstr = conv(ncsubstr, convType);
                                    var csubstr = int64(s[i]); 
                                    // Calculate large constant from bytes as sequence of shifts and ors.
                                    // Like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                                    // ssa will combine this into a single large load.
                                    for (long offset = 1L; offset < step; offset++)
                                    {
                                        var b = nod(OINDEX, ncs, nodintconst(int64(i + offset)));
                                        b = conv(b, convType);
                                        b = nod(OLSH, b, nodintconst(int64(8L * offset)));
                                        ncsubstr = nod(OOR, ncsubstr, b);
                                        csubstr = csubstr | int64(s[i + offset]) << (int)(uint8(8L * offset));
                                    }

                                    var csubstrPart = nodintconst(csubstr); 
                                    // Compare "step" bytes as once
                                    r = nod(and, r, nod(cmp, csubstrPart, ncsubstr));
                                    remains -= step;
                                    i += step;
                                }

                            }
                            r = typecheck(r, Erv);
                            r = walkexpr(r, init);
                            r.Type = n.Type;
                            n = r;
                            break;
                        }

                        s = s__prev2;

                    }
                }
                r = default; 
                // TODO(marvin): Fix Node.EType type union.
                if (Op(n.Etype) == OEQ || Op(n.Etype) == ONE)
                { 
                    // prepare for rewrite below
                    n.Left = cheapexpr(n.Left, init);
                    n.Right = cheapexpr(n.Right, init);

                    var lstr = conv(n.Left, types.Types[TSTRING]);
                    var rstr = conv(n.Right, types.Types[TSTRING]);
                    var lptr = nod(OSPTR, lstr, null);
                    var rptr = nod(OSPTR, rstr, null);
                    var llen = conv(nod(OLEN, lstr, null), types.Types[TUINTPTR]);
                    var rlen = conv(nod(OLEN, rstr, null), types.Types[TUINTPTR]);

                    fn = syslook("memequal");
                    fn = substArgTypes(fn, types.Types[TUINT8], types.Types[TUINT8]);
                    r = mkcall1(fn, types.Types[TBOOL], init, lptr, rptr, llen); 

                    // quick check of len before full compare for == or !=.
                    // memequal then tests equality up to length len.
                    // TODO(marvin): Fix Node.EType type union.
                    if (Op(n.Etype) == OEQ)
                    { 
                        // len(left) == len(right) && memequal(left, right, len)
                        r = nod(OANDAND, nod(OEQ, llen, rlen), r);
                    }
                    else
                    { 
                        // len(left) != len(right) || !memequal(left, right, len)
                        r = nod(ONOT, r, null);
                        r = nod(OOROR, nod(ONE, llen, rlen), r);
                    }
                    r = typecheck(r, Erv);
                    r = walkexpr(r, null);
                }
                else
                { 
                    // sys_cmpstring(s1, s2) :: 0
                    r = mkcall("cmpstring", types.Types[TINT], init, conv(n.Left, types.Types[TSTRING]), conv(n.Right, types.Types[TSTRING])); 
                    // TODO(marvin): Fix Node.EType type union.
                    r = nod(Op(n.Etype), r, nodintconst(0L));
                }
                r = typecheck(r, Erv);
                if (!n.Type.IsBoolean())
                {
                    Fatalf("cmp %v", n.Type);
                }
                r.Type = n.Type;
                n = r;
            else if (n.Op == OADDSTR) 
                n = addstr(n, init);
            else if (n.Op == OAPPEND) 
                // order should make sure we only see OAS(node, OAPPEND), which we handle above.
                Fatalf("append outside assignment");
            else if (n.Op == OCOPY) 
                n = copyany(n, init, instrumenting && !compiling_runtime); 

                // cannot use chanfn - closechan takes any, not chan any
            else if (n.Op == OCLOSE) 
                fn = syslook("closechan");

                fn = substArgTypes(fn, n.Left.Type);
                n = mkcall1(fn, null, init, n.Left);
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
                n = mkcall1(chanfn(fnname, 1L, n.Type), n.Type, init, typename(n.Type), conv(size, argtype));
            else if (n.Op == OMAKEMAP) 
                t = n.Type;
                var hmapType = hmap(t);
                var hint = n.Left; 

                // var h *hmap
                ref Node h = default;
                if (n.Esc == EscNone)
                { 
                    // Allocate hmap on stack.

                    // var hv hmap
                    var hv = temp(hmapType);
                    var zero = nod(OAS, hv, null);
                    zero = typecheck(zero, Etop);
                    init.Append(zero); 
                    // h = &hv
                    h = nod(OADDR, hv, null); 

                    // Allocate one bucket pointed to by hmap.buckets on stack if hint
                    // is not larger than BUCKETSIZE. In case hint is larger than
                    // BUCKETSIZE runtime.makemap will allocate the buckets on the heap.
                    // Maximum key and value size is 128 bytes, larger objects
                    // are stored with an indirection. So max bucket size is 2048+eps.
                    if (!Isconst(hint, CTINT) || !(hint.Val().U._<ref Mpint>().CmpInt64(BUCKETSIZE) > 0L))
                    { 
                        // var bv bmap
                        var bv = temp(bmap(t));

                        zero = nod(OAS, bv, null);
                        zero = typecheck(zero, Etop);
                        init.Append(zero); 

                        // b = &bv
                        b = nod(OADDR, bv, null); 

                        // h.buckets = b
                        var bsym = hmapType.Field(5L).Sym; // hmap.buckets see reflect.go:hmap
                        var na = nod(OAS, nodSym(ODOT, h, bsym), b);
                        na = typecheck(na, Etop);
                        init.Append(na);
                    }
                }
                if (Isconst(hint, CTINT) && hint.Val().U._<ref Mpint>().CmpInt64(BUCKETSIZE) <= 0L)
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
                        var rand = mkcall("fastrand", types.Types[TUINT32], init);
                        var hashsym = hmapType.Field(4L).Sym; // hmap.hash0 see reflect.go:hmap
                        a = nod(OAS, nodSym(ODOT, h, hashsym), rand);
                        a = typecheck(a, Etop);
                        a = walkexpr(a, init);
                        init.Append(a);
                        n = nod(OCONVNOP, h, null);
                        n.Type = t;
                        n = typecheck(n, Erv);
                    }
                    else
                    { 
                        // Call runtime.makehmap to allocate an
                        // hmap on the heap and initialize hmap's hash0 field.
                        fn = syslook("makemap_small");
                        fn = substArgTypes(fn, t.Key(), t.Val());
                        n = mkcall1(fn, n.Type, init);
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
                    // runtime.makemap to intialize hmap and allocate the
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
                    fn = substArgTypes(fn, hmapType, t.Key(), t.Val());
                    n = mkcall1(fn, n.Type, init, typename(n.Type), conv(hint, argtype), h);
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
                    if (!isSmallMakeSlice(n))
                    {
                        Fatalf("non-small OMAKESLICE with EscNone: %v", n);
                    } 
                    // var arr [r]T
                    // n = arr[:l]
                    t = types.NewArray(t.Elem(), nonnegintconst(r)); // [r]T
                    var_ = temp(t);
                    a = nod(OAS, var_, null); // zero temp
                    a = typecheck(a, Etop);
                    init.Append(a);
                    r = nod(OSLICE, var_, null); // arr[:l]
                    r.SetSliceBounds(null, l, null);
                    r = conv(r, n.Type); // in case n.Type is named.
                    r = typecheck(r, Erv);
                    r = walkexpr(r, init);
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
                    fn = syslook(fnname);
                    fn = substArgTypes(fn, t.Elem()); // any-1
                    n = mkcall1(fn, t, init, typename(t.Elem()), conv(len, argtype), conv(cap, argtype));
                }
            else if (n.Op == ORUNESTR) 
                a = nodnil();
                if (n.Esc == EscNone)
                {
                    t = types.NewArray(types.Types[TUINT8], 4L);
                    var_ = temp(t);
                    a = nod(OADDR, var_, null);
                } 

                // intstring(*[4]byte, rune)
                n = mkcall("intstring", n.Type, init, a, conv(n.Left, types.Types[TINT64]));
            else if (n.Op == OARRAYBYTESTR) 
                a = nodnil();
                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for string on stack.
                    t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);

                    a = nod(OADDR, temp(t), null);
                } 

                // slicebytetostring(*[32]byte, []byte) string;
                n = mkcall("slicebytetostring", n.Type, init, a, n.Left); 

                // slicebytetostringtmp([]byte) string;
            else if (n.Op == OARRAYBYTESTRTMP) 
                n.Left = walkexpr(n.Left, init);

                if (!instrumenting)
                { 
                    // Let the backend handle OARRAYBYTESTRTMP directly
                    // to avoid a function call to slicebytetostringtmp.
                    break;
                }
                n = mkcall("slicebytetostringtmp", n.Type, init, n.Left); 

                // slicerunetostring(*[32]byte, []rune) string;
            else if (n.Op == OARRAYRUNESTR) 
                a = nodnil();

                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for string on stack.
                    t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);

                    a = nod(OADDR, temp(t), null);
                }
                n = mkcall("slicerunetostring", n.Type, init, a, n.Left); 

                // stringtoslicebyte(*32[byte], string) []byte;
            else if (n.Op == OSTRARRAYBYTE) 
                a = nodnil();

                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for slice on stack.
                    t = types.NewArray(types.Types[TUINT8], tmpstringbufsize);

                    a = nod(OADDR, temp(t), null);
                }
                n = mkcall("stringtoslicebyte", n.Type, init, a, conv(n.Left, types.Types[TSTRING]));
            else if (n.Op == OSTRARRAYBYTETMP) 
                // []byte(string) conversion that creates a slice
                // referring to the actual string bytes.
                // This conversion is handled later by the backend and
                // is only for use by internal compiler optimizations
                // that know that the slice won't be mutated.
                // The only such case today is:
                // for i, c := range []byte(string)
                n.Left = walkexpr(n.Left, init); 

                // stringtoslicerune(*[32]rune, string) []rune
            else if (n.Op == OSTRARRAYRUNE) 
                a = nodnil();

                if (n.Esc == EscNone)
                { 
                    // Create temporary buffer for slice on stack.
                    t = types.NewArray(types.Types[TINT32], tmpstringbufsize);

                    a = nod(OADDR, temp(t), null);
                }
                n = mkcall("stringtoslicerune", n.Type, init, a, n.Left); 

                // ifaceeq(i1 any-1, i2 any-2) (ret bool);
            else if (n.Op == OCMPIFACE) 
                if (!eqtype(n.Left.Type, n.Right.Type))
                {
                    Fatalf("ifaceeq %v %v %v", n.Op, n.Left.Type, n.Right.Type);
                }
                fn = default;
                if (n.Left.Type.IsEmptyInterface())
                {
                    fn = syslook("efaceeq");
                }
                else
                {
                    fn = syslook("ifaceeq");
                }
                n.Right = cheapexpr(n.Right, init);
                n.Left = cheapexpr(n.Left, init);
                var lt = nod(OITAB, n.Left, null);
                var rt = nod(OITAB, n.Right, null);
                var ld = nod(OIDATA, n.Left, null);
                var rd = nod(OIDATA, n.Right, null);
                ld.Type = types.Types[TUNSAFEPTR];
                rd.Type = types.Types[TUNSAFEPTR];
                ld.SetTypecheck(1L);
                rd.SetTypecheck(1L);
                call = mkcall1(fn, n.Type, init, lt, ld, rd); 

                // Check itable/type before full compare.
                // Note: short-circuited because order matters.
                // TODO(marvin): Fix Node.EType type union.
                cmp = default;
                if (Op(n.Etype) == OEQ)
                {
                    cmp = nod(OANDAND, nod(OEQ, lt, rt), call);
                }
                else
                {
                    cmp = nod(OOROR, nod(ONE, lt, rt), nod(ONOT, call, null));
                }
                cmp = typecheck(cmp, Erv);
                cmp = walkexpr(cmp, init);
                cmp.Type = n.Type;
                n = cmp;
            else if (n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OMAPLIT || n.Op == OSTRUCTLIT || n.Op == OPTRLIT) 
                if (isStaticCompositeLiteral(n) && !canSSAType(n.Type))
                { 
                    // n can be directly represented in the read-only data section.
                    // Make direct reference to the static data. See issue 12841.
                    var vstat = staticname(n.Type);
                    vstat.Name.SetReadonly(true);
                    fixedlit(inInitFunction, initKindStatic, n, vstat, init);
                    n = vstat;
                    n = typecheck(n, Erv);
                    break;
                }
                var_ = temp(n.Type);
                anylit(n, var_, init);
                n = var_;
            else if (n.Op == OSEND) 
                n1 = n.Right;
                n1 = assignconv(n1, n.Left.Type.Elem(), "chan send");
                n1 = walkexpr(n1, init);
                n1 = nod(OADDR, n1, null);
                n = mkcall1(chanfn("chansend1", 2L, n.Left.Type), null, init, n.Left, n1);
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
                n = typecheck(n, Erv); 
                // Emit string symbol now to avoid emitting
                // any concurrently during the backend.
                {
                    @string s__prev2 = s;
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
                Dump("walk", n);
            }
            lineno = lno;
            return n;
        }

        // TODO(josharian): combine this with its caller and simplify
        private static ref Node reduceSlice(ref Node n)
        {
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
                return n.Left;
            }
            return n;
        }

        private static ref Node ascompatee1(ref Node l, ref Node r, ref Nodes init)
        { 
            // convas will turn map assigns into function calls,
            // making it impossible for reorder3 to work.
            var n = nod(OAS, l, r);

            if (l.Op == OINDEXMAP)
            {
                return n;
            }
            return convas(n, init);
        }

        private static slice<ref Node> ascompatee(Op op, slice<ref Node> nl, slice<ref Node> nr, ref Nodes init)
        { 
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
            slice<ref Node> nn = default;
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
                nn = append(nn, ascompatee1(nl[i], nr[i], init));
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

        // l is an lv and rt is the type of an rv
        // return 1 if this implies a function call
        // evaluating the lv or a function call
        // in the conversion of the types
        private static bool fncall(ref Node l, ref types.Type rt)
        {
            if (l.HasCall() || l.Op == OINDEXMAP)
            {
                return true;
            }
            if (eqtype(l.Type, rt))
            {
                return false;
            }
            return true;
        }

        // check assign type list to
        // an expression list. called in
        //    expr-list = func()
        private static slice<ref Node> ascompatet(Nodes nl, ref types.Type nr)
        {
            if (nl.Len() != nr.NumFields())
            {
                Fatalf("ascompatet: assignment count mismatch: %d = %d", nl.Len(), nr.NumFields());
            }
            Nodes nn = default;            Nodes mm = default;

            foreach (var (i, l) in nl.Slice())
            {
                if (isblank(l))
                {
                    continue;
                }
                var r = nr.Field(i); 

                // any lv that causes a fn call must be
                // deferred until all the return arguments
                // have been pulled from the output arguments
                if (fncall(l, r.Type))
                {
                    var tmp = temp(r.Type);
                    tmp = typecheck(tmp, Erv);
                    var a = nod(OAS, l, tmp);
                    a = convas(a, ref mm);
                    mm.Append(a);
                    l = tmp;
                }
                a = nod(OAS, l, nodarg(r, 0L));
                a = convas(a, ref nn);
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

        // nodarg returns a Node for the function argument denoted by t,
        // which is either the entire function argument or result struct (t is a  struct *types.Type)
        // or a specific argument (t is a *types.Field within a struct *types.Type).
        //
        // If fp is 0, the node is for use by a caller invoking the given
        // function, preparing the arguments before the call
        // or retrieving the results after the call.
        // In this case, the node will correspond to an outgoing argument
        // slot like 8(SP).
        //
        // If fp is 1, the node is for use by the function itself
        // (the callee), to retrieve its arguments or write its results.
        // In this case the node will be an ONAME with an appropriate
        // type and offset.
        private static ref Node nodarg(object t, long fp)
        {
            ref Node n = default;

            types.Funarg funarg = default;
            switch (t.type())
            {
                case ref types.Type t:
                    if (!t.IsFuncArgStruct())
                    {
                        Fatalf("nodarg: bad type %v", t);
                    }
                    funarg = t.StructType().Funarg; 

                    // Build fake variable name for whole arg struct.
                    n = newname(lookup(".args"));
                    n.Type = t;
                    var first = t.Field(0L);
                    if (first == null)
                    {
                        Fatalf("nodarg: bad struct");
                    }
                    if (first.Offset == BADWIDTH)
                    {
                        Fatalf("nodarg: offset not computed for %v", t);
                    }
                    n.Xoffset = first.Offset;
                    break;
                case ref types.Field t:
                    funarg = t.Funarg;
                    if (fp == 1L)
                    { 
                        // NOTE(rsc): This should be using t.Nname directly,
                        // except in the case where t.Nname.Sym is the blank symbol and
                        // so the assignment would be discarded during code generation.
                        // In that case we need to make a new node, and there is no harm
                        // in optimization passes to doing so. But otherwise we should
                        // definitely be using the actual declaration and not a newly built node.
                        // The extra Fatalf checks here are verifying that this is the case,
                        // without changing the actual logic (at time of writing, it's getting
                        // toward time for the Go 1.7 beta).
                        // At some quieter time (assuming we've never seen these Fatalfs happen)
                        // we could change this code to use "expect" directly.
                        var expect = asNode(t.Nname);
                        if (expect.isParamHeapCopy())
                        {
                            expect = expect.Name.Param.Stackcopy;
                        }
                        {
                            ref Node n__prev1 = n;

                            foreach (var (_, __n) in Curfn.Func.Dcl)
                            {
                                n = __n;
                                if ((n.Class() == PPARAM || n.Class() == PPARAMOUT) && !t.Sym.IsBlank() && n.Sym == t.Sym)
                                {
                                    if (n != expect)
                                    {
                                        Fatalf("nodarg: unexpected node: %v (%p %v) vs %v (%p %v)", n, n, n.Op, asNode(t.Nname), asNode(t.Nname), asNode(t.Nname).Op);
                                    }
                                    return n;
                                }
                            }

                            n = n__prev1;
                        }

                        if (!expect.Sym.IsBlank())
                        {
                            Fatalf("nodarg: did not find node in dcl list: %v", expect);
                        }
                    } 

                    // Build fake name for individual variable.
                    // This is safe because if there was a real declared name
                    // we'd have used it above.
                    n = newname(lookup("__"));
                    n.Type = t.Type;
                    if (t.Offset == BADWIDTH)
                    {
                        Fatalf("nodarg: offset not computed for %v", t);
                    }
                    n.Xoffset = t.Offset;
                    n.Orig = asNode(t.Nname);
                    break;
                default:
                {
                    var t = t.type();
                    Fatalf("bad nodarg %T(%v)", t, t);
                    break;
                } 

                // Rewrite argument named _ to __,
                // or else the assignment to _ will be
                // discarded during code generation.
            } 

            // Rewrite argument named _ to __,
            // or else the assignment to _ will be
            // discarded during code generation.
            if (isblank(n))
            {
                n.Sym = lookup("__");
            }
            switch (fp)
            {
                case 0L: // preparing arguments for call
                    n.Op = OINDREGSP;
                    n.Xoffset += Ctxt.FixedFrameSize();
                    break;
                case 1L: // reading arguments inside call
                    n.SetClass(PPARAM);
                    if (funarg == types.FunargResults)
                    {
                        n.SetClass(PPARAMOUT);
                    }
                    break;
                default: 
                    Fatalf("bad fp");
                    break;
            }

            n.SetTypecheck(1L);
            n.SetAddrtaken(true); // keep optimizers at bay
            return n;
        }

        // package all the arguments that match a ... T parameter into a []T.
        private static ref Node mkdotargslice(ref types.Type typ, slice<ref Node> args, ref Nodes init, ref Node ddd)
        {
            var esc = uint16(EscUnknown);
            if (ddd != null)
            {
                esc = ddd.Esc;
            }
            if (len(args) == 0L)
            {
                var n = nodnil();
                n.Type = typ;
                return n;
            }
            n = nod(OCOMPLIT, null, typenod(typ));
            if (ddd != null && prealloc[ddd] != null)
            {
                prealloc[n] = prealloc[ddd]; // temporary to use
            }
            n.List.Set(args);
            n.Esc = esc;
            n = typecheck(n, Erv);
            if (n.Type == null)
            {
                Fatalf("mkdotargslice: typecheck failed");
            }
            n = walkexpr(n, init);
            return n;
        }

        // check assign expression list to
        // a type list. called in
        //    return expr-list
        //    func(expr-list)
        private static slice<ref Node> ascompatte(ref Node call, bool isddd, ref types.Type lhs, slice<ref Node> rhs, long fp, ref Nodes init)
        { 
            // f(g()) where g has multiple return values
            if (len(rhs) == 1L && rhs[0L].Type.IsFuncArgStruct())
            { 
                // optimization - can do block copy
                if (eqtypenoname(rhs[0L].Type, lhs))
                {
                    var nl = nodarg(lhs, fp);
                    var nr = nod(OCONVNOP, rhs[0L], null);
                    nr.Type = nl.Type;
                    var n = convas(nod(OAS, nl, nr), init);
                    n.SetTypecheck(1L);
                    return new slice<ref Node>(new ref Node[] { n });
                } 

                // conversions involved.
                // copy into temporaries.
                slice<ref Node> tmps = default;
                {
                    var nr__prev1 = nr;

                    foreach (var (_, __nr) in rhs[0L].Type.FieldSlice())
                    {
                        nr = __nr;
                        tmps = append(tmps, temp(nr.Type));
                    }

                    nr = nr__prev1;
                }

                var a = nod(OAS2, null, null);
                a.List.Set(tmps);
                a.Rlist.Set(rhs);
                a = typecheck(a, Etop);
                a = walkstmt(a);
                init.Append(a);

                rhs = tmps;
            } 

            // For each parameter (LHS), assign its corresponding argument (RHS).
            // If there's a ... parameter (which is only valid as the final
            // parameter) and this is not a ... call expression,
            // then assign the remaining arguments as a slice.
            slice<ref Node> nn = default;
            {
                var nl__prev1 = nl;

                foreach (var (__i, __nl) in lhs.FieldSlice())
                {
                    i = __i;
                    nl = __nl;
                    nr = default;
                    if (nl.Isddd() && !isddd)
                    {
                        nr = mkdotargslice(nl.Type, rhs[i..], init, call.Right);
                    }
                    else
                    {
                        nr = rhs[i];
                    }
                    a = nod(OAS, nodarg(nl, fp), nr);
                    a = convas(a, init);
                    a.SetTypecheck(1L);
                    nn = append(nn, a);
                }

                nl = nl__prev1;
            }

            return nn;
        }

        // generate code for print
        private static ref Node walkprint(ref Node nn, ref Nodes init)
        { 
            // Hoist all the argument evaluation up before the lock.
            walkexprlistcheap(nn.List.Slice(), init); 

            // For println, add " " between elements and "\n" at the end.
            if (nn.Op == OPRINTN)
            {
                var s = nn.List.Slice();
                var t = make_slice<ref Node>(0L, len(s) * 2L);
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
            t = make_slice<ref Node>(0L, len(s));
            {
                var i__prev1 = i;

                long i = 0L;

                while (i < len(s))
                {
                    slice<@string> strs = default;
                    while (i < len(s) && Isconst(s[i], CTSTR))
                    {
                        strs = append(strs, s[i].Val().U._<@string>());
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

            ref Node calls = new slice<ref Node>(new ref Node[] { mkcall("printlock",nil,init) });
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
                    ref Node on = default;

                    if (n.Type.Etype == TINTER) 
                        if (n.Type.IsEmptyInterface())
                        {
                            on = syslook("printeface");
                        }
                        else
                        {
                            on = syslook("printiface");
                        }
                        on = substArgTypes(on, n.Type); // any-1
                    else if (n.Type.Etype == TPTR32 || n.Type.Etype == TPTR64 || n.Type.Etype == TCHAN || n.Type.Etype == TMAP || n.Type.Etype == TFUNC || n.Type.Etype == TUNSAFEPTR) 
                        on = syslook("printpointer");
                        on = substArgTypes(on, n.Type); // any-1
                    else if (n.Type.Etype == TSLICE) 
                        on = syslook("printslice");
                        on = substArgTypes(on, n.Type); // any-1
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
                            cs = n.Val().U._<@string>();
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
                            if (!eqtype(t, n.Type))
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

            calls = append(calls, mkcall("printunlock", null, init));

            typecheckslice(calls, Etop);
            walkexprlist(calls, init);

            r = nod(OEMPTY, null, null);
            r = typecheck(r, Etop);
            r = walkexpr(r, init);
            r.Ninit.Set(calls);
            return r;
        }

        private static ref Node callnew(ref types.Type t)
        {
            if (t.NotInHeap())
            {
                yyerror("%v is go:notinheap; heap allocation disallowed", t);
            }
            dowidth(t);
            var fn = syslook("newobject");
            fn = substArgTypes(fn, t);
            var v = mkcall1(fn, types.NewPtr(t), null, typename(t));
            v.SetNonNil(true);
            return v;
        }

        private static bool iscallret(ref Node n)
        {
            if (n == null)
            {
                return false;
            }
            n = outervalue(n);
            return n.Op == OINDREGSP;
        }

        // isReflectHeaderDataField reports whether l is an expression p.Data
        // where p has type reflect.SliceHeader or reflect.StringHeader.
        private static bool isReflectHeaderDataField(ref Node l)
        {
            if (l.Type != types.Types[TUINTPTR])
            {
                return false;
            }
            ref types.Sym tsym = default;

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

        private static ref Node convas(ref Node _n, ref Nodes _init) => func(_n, _init, (ref Node n, ref Nodes init, Defer defer, Panic _, Recover __) =>
        {
            if (n.Op != OAS)
            {
                Fatalf("convas: not OAS %v", n.Op);
            }
            defer(updateHasCall(n));

            n.SetTypecheck(1L);

            if (n.Left == null || n.Right == null)
            {
                return n;
            }
            var lt = n.Left.Type;
            var rt = n.Right.Type;
            if (lt == null || rt == null)
            {
                return n;
            }
            if (isblank(n.Left))
            {
                n.Right = defaultlit(n.Right, null);
                return n;
            }
            if (!eqtype(lt, rt))
            {
                n.Right = assignconv(n.Right, lt, "assignment");
                n.Right = walkexpr(n.Right, init);
            }
            dowidth(n.Right.Type);

            return n;
        });

        // from ascompat[te]
        // evaluating actual function arguments.
        //    f(a,b)
        // if there is exactly one function expr,
        // then it is done first. otherwise must
        // make temp variables
        private static slice<ref Node> reorder1(slice<ref Node> all)
        {
            if (len(all) == 1L)
            {
                return all;
            }
            long funcCalls = 0L;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in all)
                {
                    n = __n;
                    updateHasCall(n);
                    if (n.HasCall())
                    {
                        funcCalls++;
                    }
                }

                n = n__prev1;
            }

            if (funcCalls == 0L)
            {
                return all;
            }
            slice<ref Node> g = default; // fncalls assigned to tempnames
            ref Node f = default; // last fncall assigned to stack
            slice<ref Node> r = default; // non fncalls and tempnames assigned to stack
            long d = 0L;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in all)
                {
                    n = __n;
                    if (!n.HasCall())
                    {
                        r = append(r, n);
                        continue;
                    }
                    d++;
                    if (d == funcCalls)
                    {
                        f = n;
                        continue;
                    } 

                    // make assignment of fncall to tempname
                    var a = temp(n.Right.Type);

                    a = nod(OAS, a, n.Right);
                    g = append(g, a); 

                    // put normal arg assignment on list
                    // with fncall replaced by tempname
                    n.Right = a.Left;

                    r = append(r, n);
                }

                n = n__prev1;
            }

            if (f != null)
            {
                g = append(g, f);
            }
            return append(g, r);
        }

        // from ascompat[ee]
        //    a,b = c,d
        // simultaneous assignment. there cannot
        // be later use of an earlier lvalue.
        //
        // function calls have been removed.
        private static slice<ref Node> reorder3(slice<ref Node> all)
        { 
            // If a needed expression may be affected by an
            // earlier assignment, make an early copy of that
            // expression and use the copy instead.
            slice<ref Node> early = default;

            Nodes mapinit = default;
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
                        l.Right = reorder3save(l.Right, all, i, ref early);
                        l = l.Left;
                        continue;
                    }
                    break;
                }



                if (l.Op == ONAME) 
                    break;
                else if (l.Op == OINDEX || l.Op == OINDEXMAP) 
                    l.Left = reorder3save(l.Left, all, i, ref early);
                    l.Right = reorder3save(l.Right, all, i, ref early);
                    if (l.Op == OINDEXMAP)
                    {
                        all[i] = convas(all[i], ref mapinit);
                    }
                else if (l.Op == OIND || l.Op == ODOTPTR) 
                    l.Left = reorder3save(l.Left, all, i, ref early);
                else 
                    Fatalf("reorder3 unexpected lvalue %#v", l.Op);
                // Save expression on right side.
                all[i].Right = reorder3save(all[i].Right, all, i, ref early);
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
        private static ref Node reorder3save(ref Node n, slice<ref Node> all, long i, ref slice<ref Node> early)
        {
            if (!aliased(n, all, i))
            {
                return n;
            }
            var q = temp(n.Type);
            q = nod(OAS, q, n);
            q = typecheck(q, Etop);
            early.Value = append(early.Value, q);
            return q.Left;
        }

        // what's the outer value that a write to n affects?
        // outer value means containing struct or array.
        private static ref Node outervalue(ref Node n)
        {
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
                                return n;
            }

        }

        // Is it possible that the computation of n might be
        // affected by writes in as up to but not including the ith element?
        private static bool aliased(ref Node n, slice<ref Node> all, long i)
        {
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
                var a = outervalue(an.Left);

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
                    if (n.Addrtaken())
                    {
                        varwrite = true;
                        continue;
                    }
                    if (vmatch2(a, n))
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
            if (varexpr(n))
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
        private static bool varexpr(ref Node n)
        {
            if (n == null)
            {
                return true;
            }

            if (n.Op == OLITERAL) 
                return true;
            else if (n.Op == ONAME) 

                if (n.Class() == PAUTO || n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    if (!n.Addrtaken())
                    {
                        return true;
                    }
                                return false;
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OMUL || n.Op == ODIV || n.Op == OMOD || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == OPLUS || n.Op == OMINUS || n.Op == OCOM || n.Op == OPAREN || n.Op == OANDAND || n.Op == OOROR || n.Op == OCONV || n.Op == OCONVNOP || n.Op == OCONVIFACE || n.Op == ODOTTYPE) 
                return varexpr(n.Left) && varexpr(n.Right);
            else if (n.Op == ODOT) // but not ODOTPTR
                // Should have been handled in aliased.
                Fatalf("varexpr unexpected ODOT");
            // Be conservative.
            return false;
        }

        // is the name l mentioned in r?
        private static bool vmatch2(ref Node l, ref Node r)
        {
            if (r == null)
            {
                return false;
            }

            // match each right given left
            if (r.Op == ONAME) 
                return l == r;
            else if (r.Op == OLITERAL) 
                return false;
                        if (vmatch2(l, r.Left))
            {
                return true;
            }
            if (vmatch2(l, r.Right))
            {
                return true;
            }
            foreach (var (_, n) in r.List.Slice())
            {
                if (vmatch2(l, n))
                {
                    return true;
                }
            }
            return false;
        }

        // is any name mentioned in l also mentioned in r?
        // called by sinit.go
        private static bool vmatch1(ref Node l, ref Node r)
        { 
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
                                return vmatch2(l, r);
            else if (l.Op == OLITERAL) 
                return false;
                        if (vmatch1(l.Left, r))
            {
                return true;
            }
            if (vmatch1(l.Right, r))
            {
                return true;
            }
            foreach (var (_, n) in l.List.Slice())
            {
                if (vmatch1(n, r))
                {
                    return true;
                }
            }
            return false;
        }

        // paramstoheap returns code to allocate memory for heap-escaped parameters
        // and to copy non-result parameters' values from the stack.
        private static slice<ref Node> paramstoheap(ref types.Type @params)
        {
            slice<ref Node> nn = default;
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
                        nn = append(nn, walkstmt(nod(ODCL, v, null)));
                        if (stackcopy.Class() == PPARAM)
                        {
                            nn = append(nn, walkstmt(typecheck(nod(OAS, v, stackcopy), Etop)));
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
            var lno = lineno;
            lineno = Curfn.Pos;
            foreach (var (_, f) in Curfn.Type.Results().Fields().Slice())
            {
                {
                    var v = asNode(f.Nname);

                    if (v != null && v.Name.Param.Heapaddr != null)
                    { 
                        // The local which points to the return value is the
                        // thing that needs zeroing. This is already handled
                        // by a Needzero annotation in plive.go:livenessepilogue.
                        continue;
                    } 
                    // Zero the stack location containing f.

                } 
                // Zero the stack location containing f.
                Curfn.Func.Enter.Append(nod(OAS, nodarg(f, 1L), null));
            }
            lineno = lno;
        }

        // returnsfromheap returns code to copy values for heap-escaped parameters
        // back to the stack.
        private static slice<ref Node> returnsfromheap(ref types.Type @params)
        {
            slice<ref Node> nn = default;
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
                        nn = append(nn, walkstmt(typecheck(nod(OAS, stackcopy, v), Etop)));
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
            var nn = paramstoheap(Curfn.Type.Recvs());
            nn = append(nn, paramstoheap(Curfn.Type.Params()));
            nn = append(nn, paramstoheap(Curfn.Type.Results()));
            Curfn.Func.Enter.Append(nn);
            lineno = Curfn.Func.Endlineno;
            Curfn.Func.Exit.Append(returnsfromheap(Curfn.Type.Results()));
            lineno = lno;
        }

        private static ref Node vmkcall(ref Node fn, ref types.Type t, ref Nodes init, slice<ref Node> va)
        {
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
                r = typecheck(r, Erv | Efnstruct);
            }
            else
            {
                r = typecheck(r, Etop);
            }
            r = walkexpr(r, init);
            r.Type = t;
            return r;
        }

        private static ref Node mkcall(@string name, ref types.Type t, ref Nodes init, params ptr<Node>[] args)
        {
            args = args.Clone();

            return vmkcall(syslook(name), t, init, args);
        }

        private static ref Node mkcall1(ref Node fn, ref types.Type t, ref Nodes init, params ptr<Node>[] args)
        {
            args = args.Clone();

            return vmkcall(fn, t, init, args);
        }

        private static ref Node conv(ref Node n, ref types.Type t)
        {
            if (eqtype(n.Type, t))
            {
                return n;
            }
            n = nod(OCONV, n, null);
            n.Type = t;
            n = typecheck(n, Erv);
            return n;
        }

        // byteindex converts n, which is byte-sized, to a uint8.
        // We cannot use conv, because we allow converting bool to uint8 here,
        // which is forbidden in user code.
        private static ref Node byteindex(ref Node n)
        {
            if (eqtype(n.Type, types.Types[TUINT8]))
            {
                return n;
            }
            n = nod(OCONV, n, null);
            n.Type = types.Types[TUINT8];
            n.SetTypecheck(1L);
            return n;
        }

        private static ref Node chanfn(@string name, long n, ref types.Type t)
        {
            if (!t.IsChan())
            {
                Fatalf("chanfn %v", t);
            }
            var fn = syslook(name);
            switch (n)
            {
                case 1L: 
                    fn = substArgTypes(fn, t.Elem());
                    break;
                case 2L: 
                    fn = substArgTypes(fn, t.Elem(), t.Elem());
                    break;
                default: 
                    Fatalf("chanfn %d", n);
                    break;
            }
            return fn;
        }

        private static ref Node mapfn(@string name, ref types.Type t)
        {
            if (!t.IsMap())
            {
                Fatalf("mapfn %v", t);
            }
            var fn = syslook(name);
            fn = substArgTypes(fn, t.Key(), t.Val(), t.Key(), t.Val());
            return fn;
        }

        private static ref Node mapfndel(@string name, ref types.Type t)
        {
            if (!t.IsMap())
            {
                Fatalf("mapfn %v", t);
            }
            var fn = syslook(name);
            fn = substArgTypes(fn, t.Key(), t.Val(), t.Key());
            return fn;
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

        private static long mapfast(ref types.Type t)
        { 
            // Check ../../runtime/hashmap.go:maxValueSize before changing.
            if (t.Val().Width > 128L)
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

        private static ref Node writebarrierfn(@string name, ref types.Type l, ref types.Type r)
        {
            var fn = syslook(name);
            fn = substArgTypes(fn, l, r);
            return fn;
        }

        private static ref Node addstr(ref Node n, ref Nodes init)
        { 
            // orderexpr rewrote OADDSTR to have a list of strings.
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
                        sz += int64(len(n1.Val().U._<@string>()));
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
            ref Node args = new slice<ref Node>(new ref Node[] { buf });
            foreach (var (_, n2) in n.List.Slice())
            {
                args = append(args, conv(n2, types.Types[TSTRING]));
            }
            @string fn = default;
            if (c <= 5L)
            { 
                // small numbers of strings use direct runtime helpers.
                // note: orderexpr knows this cutoff too.
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
                args = new slice<ref Node>(new ref Node[] { buf, slice });
                slice.Esc = EscNone;
            }
            var cat = syslook(fn);
            var r = nod(OCALL, cat, null);
            r.List.Set(args);
            r = typecheck(r, Erv);
            r = walkexpr(r, init);
            r.Type = n.Type;

            return r;
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
        private static ref Node appendslice(ref Node n, ref Nodes init)
        {
            walkexprlistsafe(n.List.Slice(), init); 

            // walkexprlistsafe will leave OINDEX (s[n]) alone if both s
            // and n are name or literal, but those may index the slice we're
            // modifying here. Fix explicitly.
            var ls = n.List.Slice();
            foreach (var (i1, n1) in ls)
            {
                ls[i1] = cheapexpr(n1, init);
            }
            var l1 = n.List.First();
            var l2 = n.List.Second();

            slice<ref Node> l = default; 

            // var s []T
            var s = temp(l1.Type);
            l = append(l, nod(OAS, s, l1)); // s = l1

            // n := len(s) + len(l2)
            var nn = temp(types.Types[TINT]);
            l = append(l, nod(OAS, nn, nod(OADD, nod(OLEN, s, null), nod(OLEN, l2, null)))); 

            // if uint(n) > uint(cap(s))
            var nif = nod(OIF, null, null);
            nif.Left = nod(OGT, nod(OCONV, nn, null), nod(OCONV, nod(OCAP, s, null), null));
            nif.Left.Left.Type = types.Types[TUINT];
            nif.Left.Right.Type = types.Types[TUINT]; 

            // instantiate growslice(Type*, []any, int) []any
            var fn = syslook("growslice");
            fn = substArgTypes(fn, s.Type.Elem(), s.Type.Elem()); 

            // s = growslice(T, s, n)
            nif.Nbody.Set1(nod(OAS, s, mkcall1(fn, s.Type, ref nif.Ninit, typename(s.Type.Elem()), s, nn)));
            l = append(l, nif); 

            // s = s[:n]
            var nt = nod(OSLICE, s, null);
            nt.SetSliceBounds(null, nn, null);
            nt.Etype = 1L;
            l = append(l, nod(OAS, s, nt));

            if (l1.Type.Elem().HasHeapPointer())
            { 
                // copy(s[len(l1):], l2)
                var nptr1 = nod(OSLICE, s, null);
                nptr1.SetSliceBounds(nod(OLEN, l1, null), null, null);
                nptr1.Etype = 1L;
                var nptr2 = l2;
                Curfn.Func.setWBPos(n.Pos);
                fn = syslook("typedslicecopy");
                fn = substArgTypes(fn, l1.Type, l2.Type);
                Nodes ln = default;
                ln.Set(l);
                nt = mkcall1(fn, types.Types[TINT], ref ln, typename(l1.Type.Elem()), nptr1, nptr2);
                l = append(ln.Slice(), nt);
            }
            else if (instrumenting && !compiling_runtime)
            { 
                // rely on runtime to instrument copy.
                // copy(s[len(l1):], l2)
                nptr1 = nod(OSLICE, s, null);
                nptr1.SetSliceBounds(nod(OLEN, l1, null), null, null);
                nptr1.Etype = 1L;
                nptr2 = l2;

                ln = default;
                ln.Set(l);
                nt = default;
                if (l2.Type.IsString())
                {
                    fn = syslook("slicestringcopy");
                    fn = substArgTypes(fn, l1.Type, l2.Type);
                    nt = mkcall1(fn, types.Types[TINT], ref ln, nptr1, nptr2);
                }
                else
                {
                    fn = syslook("slicecopy");
                    fn = substArgTypes(fn, l1.Type, l2.Type);
                    nt = mkcall1(fn, types.Types[TINT], ref ln, nptr1, nptr2, nodintconst(s.Type.Elem().Width));
                }
                l = append(ln.Slice(), nt);
            }
            else
            { 
                // memmove(&s[len(l1)], &l2[0], len(l2)*sizeof(T))
                nptr1 = nod(OINDEX, s, nod(OLEN, l1, null));
                nptr1.SetBounded(true);

                nptr1 = nod(OADDR, nptr1, null);

                nptr2 = nod(OSPTR, l2, null);

                fn = syslook("memmove");
                fn = substArgTypes(fn, s.Type.Elem(), s.Type.Elem());

                ln = default;
                ln.Set(l);
                var nwid = cheapexpr(conv(nod(OLEN, l2, null), types.Types[TUINTPTR]), ref ln);

                nwid = nod(OMUL, nwid, nodintconst(s.Type.Elem().Width));
                nt = mkcall1(fn, null, ref ln, nptr1, nptr2, nwid);
                l = append(ln.Slice(), nt);
            }
            typecheckslice(l, Etop);
            walkstmtlist(l);
            init.Append(l);
            return s;
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
        private static ref Node walkappend(ref Node n, ref Nodes init, ref Node dst)
        {
            if (!samesafeexpr(dst, n.List.First()))
            {
                n.List.SetFirst(safeexpr(n.List.First(), init));
                n.List.SetFirst(walkexpr(n.List.First(), init));
            }
            walkexprlistsafe(n.List.Slice()[1L..], init); 

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
                    ls[i] = cheapexpr(n, init);
                }

                i = i__prev1;
                n = n__prev1;
            }

            var nsrc = n.List.First();

            var argc = n.List.Len() - 1L;
            if (argc < 1L)
            {
                return nsrc;
            } 

            // General case, with no function calls left as arguments.
            // Leave for gen, except that instrumentation requires old form.
            if (!instrumenting || compiling_runtime)
            {
                return n;
            }
            slice<ref Node> l = default;

            var ns = temp(nsrc.Type);
            l = append(l, nod(OAS, ns, nsrc)); // s = src

            var na = nodintconst(int64(argc)); // const argc
            var nx = nod(OIF, null, null); // if cap(s) - len(s) < argc
            nx.Left = nod(OLT, nod(OSUB, nod(OCAP, ns, null), nod(OLEN, ns, null)), na);

            var fn = syslook("growslice"); //   growslice(<type>, old []T, mincap int) (ret []T)
            fn = substArgTypes(fn, ns.Type.Elem(), ns.Type.Elem());

            nx.Nbody.Set1(nod(OAS, ns, mkcall1(fn, ns.Type, ref nx.Ninit, typename(ns.Type.Elem()), ns, nod(OADD, nod(OLEN, ns, null), na))));

            l = append(l, nx);

            var nn = temp(types.Types[TINT]);
            l = append(l, nod(OAS, nn, nod(OLEN, ns, null))); // n = len(s)

            nx = nod(OSLICE, ns, null); // ...s[:n+argc]
            nx.SetSliceBounds(null, nod(OADD, nn, na), null);
            nx.Etype = 1L;
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

            typecheckslice(l, Etop);
            walkstmtlist(l);
            init.Append(l);
            return ns;
        }

        // Lower copy(a, b) to a memmove call or a runtime call.
        //
        // init {
        //   n := len(a)
        //   if n > len(b) { n = len(b) }
        //   memmove(a.ptr, b.ptr, n*sizeof(elem(a)))
        // }
        // n;
        //
        // Also works if b is a string.
        //
        private static ref Node copyany(ref Node n, ref Nodes init, bool runtimecall)
        {
            if (n.Left.Type.Elem().HasHeapPointer())
            {
                Curfn.Func.setWBPos(n.Pos);
                var fn = writebarrierfn("typedslicecopy", n.Left.Type, n.Right.Type);
                return mkcall1(fn, n.Type, init, typename(n.Left.Type.Elem()), n.Left, n.Right);
            }
            if (runtimecall)
            {
                if (n.Right.Type.IsString())
                {
                    fn = syslook("slicestringcopy");
                    fn = substArgTypes(fn, n.Left.Type, n.Right.Type);
                    return mkcall1(fn, n.Type, init, n.Left, n.Right);
                }
                fn = syslook("slicecopy");
                fn = substArgTypes(fn, n.Left.Type, n.Right.Type);
                return mkcall1(fn, n.Type, init, n.Left, n.Right, nodintconst(n.Left.Type.Elem().Width));
            }
            n.Left = walkexpr(n.Left, init);
            n.Right = walkexpr(n.Right, init);
            var nl = temp(n.Left.Type);
            var nr = temp(n.Right.Type);
            slice<ref Node> l = default;
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

            // Call memmove.
            fn = syslook("memmove");

            fn = substArgTypes(fn, nl.Type.Elem(), nl.Type.Elem());
            var nwid = temp(types.Types[TUINTPTR]);
            l = append(l, nod(OAS, nwid, conv(nlen, types.Types[TUINTPTR])));
            nwid = nod(OMUL, nwid, nodintconst(nl.Type.Elem().Width));
            l = append(l, mkcall1(fn, null, init, nto, nfrm, nwid));

            typecheckslice(l, Etop);
            walkstmtlist(l);
            init.Append(l);
            return nlen;
        }

        private static (ref Node, bool) eqfor(ref types.Type t)
        { 
            // Should only arrive here with large memory or
            // a struct/array containing a non-memory field/element.
            // Small memory is handled inline, and single non-memory
            // is handled during type check (OCMPSTR etc).
            {
                var (a, _) = algtype1(t);


                if (a == AMEM) 
                    var n = syslook("memequal");
                    n = substArgTypes(n, t, t);
                    return (n, true);
                else if (a == ASPECIAL) 
                    var sym = typesymprefix(".eq", t);
                    n = newname(sym);
                    n.SetClass(PFUNC);
                    var ntype = nod(OTFUNC, null, null);
                    ntype.List.Append(anonfield(types.NewPtr(t)));
                    ntype.List.Append(anonfield(types.NewPtr(t)));
                    ntype.Rlist.Append(anonfield(types.Types[TBOOL]));
                    ntype = typecheck(ntype, Etype);
                    n.Type = ntype.Type;
                    return (n, false);

            }
            Fatalf("eqfor %v", t);
            return (null, false);
        }

        // The result of walkcompare MUST be assigned back to n, e.g.
        //     n.Left = walkcompare(n.Left, init)
        private static ref Node walkcompare(ref Node n, ref Nodes init)
        { 
            // Given interface value l and concrete value r, rewrite
            //   l == r
            // into types-equal && data-equal.
            // This is efficient, avoids allocations, and avoids runtime calls.
            ref Node l = default;            ref Node r = default;

            if (n.Left.Type.IsInterface() && !n.Right.Type.IsInterface())
            {
                l = n.Left;
                r = n.Right;
            }
            else if (!n.Left.Type.IsInterface() && n.Right.Type.IsInterface())
            {
                l = n.Right;
                r = n.Left;
            }
            if (l != null)
            { 
                // Handle both == and !=.
                var eq = n.Op;
                Op andor = default;
                if (eq == OEQ)
                {
                    andor = OANDAND;
                }
                else
                {
                    andor = OOROR;
                } 
                // Check for types equal.
                // For empty interface, this is:
                //   l.tab == type(r)
                // For non-empty interface, this is:
                //   l.tab != nil && l.tab._type == type(r)
                ref Node eqtype = default;
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
                var eqdata = nod(eq, ifaceData(l, r.Type), r); 
                // Put it all together.
                var expr = nod(andor, eqtype, eqdata);
                n = finishcompare(n, expr, init);
                return n;
            } 

            // Must be comparison of array or struct.
            // Otherwise back end handles it.
            // While we're here, decide whether to
            // inline or call an eq alg.
            var t = n.Left.Type;
            bool inline = default;

            var maxcmpsize = int64(4L);
            var unalignedLoad = false;

            if (thearch.LinkArch.Family == sys.AMD64 || thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.S390X) 
                // Keep this low enough, to generate less code than function call.
                maxcmpsize = 16L;
                unalignedLoad = true;
            else if (thearch.LinkArch.Family == sys.I386) 
                maxcmpsize = 8L;
                unalignedLoad = true;
            
            if (t.Etype == TARRAY) 
                // We can compare several elements at once with 2/4/8 byte integer compares
                inline = t.NumElem() <= 1L || (issimple[t.Elem().Etype] && (t.NumElem() <= 4L || t.Elem().Width * t.NumElem() <= maxcmpsize));
            else if (t.Etype == TSTRUCT) 
                inline = t.NumFields() <= 4L;
            else 
                return n;
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
                if (isvaluelit(cmpl))
                {
                    var var_ = temp(cmpl.Type);
                    anylit(cmpl, var_, init);
                    cmpl = var_;
                }
                if (isvaluelit(cmpr))
                {
                    var_ = temp(cmpr.Type);
                    anylit(cmpr, var_, init);
                    cmpr = var_;
                }
                if (!islvalue(cmpl) || !islvalue(cmpr))
                {
                    Fatalf("arguments of comparison must be lvalues - %v %v", cmpl, cmpr);
                } 

                // eq algs take pointers
                var pl = temp(types.NewPtr(t));
                var al = nod(OAS, pl, nod(OADDR, cmpl, null));
                al.Right.Etype = 1L; // addr does not escape
                al = typecheck(al, Etop);
                init.Append(al);

                var pr = temp(types.NewPtr(t));
                var ar = nod(OAS, pr, nod(OADDR, cmpr, null));
                ar.Right.Etype = 1L; // addr does not escape
                ar = typecheck(ar, Etop);
                init.Append(ar);

                var (fn, needsize) = eqfor(t);
                var call = nod(OCALL, fn, null);
                call.List.Append(pl);
                call.List.Append(pr);
                if (needsize)
                {
                    call.List.Append(nodintconst(t.Width));
                }
                var res = call;
                if (n.Op != OEQ)
                {
                    res = nod(ONOT, res, null);
                }
                n = finishcompare(n, res, init);
                return n;
            } 

            // inline: build boolean expression comparing element by element
            andor = OANDAND;
            if (n.Op == ONE)
            {
                andor = OOROR;
            }
            expr = default;
            Action<ref Node, ref Node> compare = (el, er) =>
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
                        ref types.Type convType = default;

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
                            compare(nod(OINDEX, cmpl, nodintconst(int64(i))), nod(OINDEX, cmpr, nodintconst(int64(i))));
                            i++;
                            remains -= t.Elem().Width;
                        }
                        else
                        {
                            var elemType = t.Elem().ToUnsigned();
                            var cmplw = nod(OINDEX, cmpl, nodintconst(int64(i)));
                            cmplw = conv(cmplw, elemType); // convert to unsigned
                            cmplw = conv(cmplw, convType); // widen
                            var cmprw = nod(OINDEX, cmpr, nodintconst(int64(i)));
                            cmprw = conv(cmprw, elemType);
                            cmprw = conv(cmprw, convType); 
                            // For code like this:  uint32(s[0]) | uint32(s[1])<<8 | uint32(s[2])<<16 ...
                            // ssa will generate a single large load.
                            for (var offset = int64(1L); offset < step; offset++)
                            {
                                var lb = nod(OINDEX, cmpl, nodintconst(int64(i + offset)));
                                lb = conv(lb, elemType);
                                lb = conv(lb, convType);
                                lb = nod(OLSH, lb, nodintconst(int64(8L * t.Elem().Width * offset)));
                                cmplw = nod(OOR, cmplw, lb);
                                var rb = nod(OINDEX, cmpr, nodintconst(int64(i + offset)));
                                rb = conv(rb, elemType);
                                rb = conv(rb, convType);
                                rb = nod(OLSH, rb, nodintconst(int64(8L * t.Elem().Width * offset)));
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
            }
            n = finishcompare(n, expr, init);
            return n;
        }

        // The result of finishcompare MUST be assigned back to n, e.g.
        //     n.Left = finishcompare(n.Left, x, r, init)
        private static ref Node finishcompare(ref Node n, ref Node r, ref Nodes init)
        { 
            // Use nn here to avoid passing r to typecheck.
            var nn = r;
            nn = typecheck(nn, Erv);
            nn = walkexpr(nn, init);
            r = nn;
            if (r.Type != n.Type)
            {
                r = nod(OCONVNOP, r, null);
                r.Type = n.Type;
                r.SetTypecheck(1L);
                nn = r;
            }
            return nn;
        }

        // isIntOrdering reports whether n is a <, ≤, >, or ≥ ordering between integers.
        private static bool isIntOrdering(this ref Node n)
        {

            if (n.Op == OLE || n.Op == OLT || n.Op == OGE || n.Op == OGT)             else 
                return false;
                        return n.Left.Type.IsInteger() && n.Right.Type.IsInteger();
        }

        // walkinrange optimizes integer-in-range checks, such as 4 <= x && x < 10.
        // n must be an OANDAND or OOROR node.
        // The result of walkinrange MUST be assigned back to n, e.g.
        //     n.Left = walkinrange(n.Left)
        private static ref Node walkinrange(ref Node n, ref Nodes init)
        { 
            // We are looking for something equivalent to a opl b OP b opr c, where:
            // * a, b, and c have integer type
            // * b is side-effect-free
            // * opl and opr are each < or ≤
            // * OP is &&
            var l = n.Left;
            var r = n.Right;
            if (!l.isIntOrdering() || !r.isIntOrdering())
            {
                return n;
            } 

            // Find b, if it exists, and rename appropriately.
            // Input is: l.Left l.Op l.Right ANDAND/OROR r.Left r.Op r.Right
            // Output is: a opl b(==x) ANDAND/OROR b(==x) opr c
            var a = l.Left;
            var opl = l.Op;
            var b = l.Right;
            var x = r.Left;
            var opr = r.Op;
            var c = r.Right;
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                if (samesafeexpr(b, x))
                {
                    break;
                }
                if (i == 3L)
                { 
                    // Tried all permutations and couldn't find an appropriate b == x.
                    return n;
                }
                if (i & 1L == 0L)
                {
                    a = b;
                    opl = brrev(opl);
                    b = a;
                }
                else
                {
                    x = c;
                    opr = brrev(opr);
                    c = x;
                }
            } 

            // If n.Op is ||, apply de Morgan.
            // Negate the internal ops now; we'll negate the top level op at the end.
            // Henceforth assume &&.
 

            // If n.Op is ||, apply de Morgan.
            // Negate the internal ops now; we'll negate the top level op at the end.
            // Henceforth assume &&.
            var negateResult = n.Op == OOROR;
            if (negateResult)
            {
                opl = brcom(opl);
                opr = brcom(opr);
            }
            Func<Op, long> cmpdir = o =>
            {

                if (o == OLE || o == OLT) 
                    return -1L;
                else if (o == OGE || o == OGT) 
                    return +1L;
                                Fatalf("walkinrange cmpdir %v", o);
                return 0L;
            }
;
            if (cmpdir(opl) != cmpdir(opr))
            { 
                // Not a range check; something like b < a && b < c.
                return n;
            }

            if (opl == OGE || opl == OGT) 
                // We have something like a > b && b ≥ c.
                // Switch and reverse ops and rename constants,
                // to make it look like a ≤ b && b < c.
                a = c;
                c = a;
                opl = brrev(opr);
                opr = brrev(opl);
            // We must ensure that c-a is non-negative.
            // For now, require a and c to be constants.
            // In the future, we could also support a == 0 and c == len/cap(...).
            // Unfortunately, by this point, most len/cap expressions have been
            // stored into temporary variables.
            if (!Isconst(a, CTINT) || !Isconst(c, CTINT))
            {
                return n;
            }
            if (opl == OLT)
            { 
                // We have a < b && ...
                // We need a ≤ b && ... to safely use unsigned comparison tricks.
                // If a is not the maximum constant for b's type,
                // we can increment a and switch to ≤.
                if (a.Int64() >= maxintval[b.Type.Etype].Int64())
                {
                    return n;
                }
                a = nodintconst(a.Int64() + 1L);
                opl = OLE;
            }
            var bound = c.Int64() - a.Int64();
            if (bound < 0L)
            { 
                // Bad news. Something like 5 <= x && x < 3.
                // Rare in practice, and we still need to generate side-effects,
                // so just leave it alone.
                return n;
            } 

            // We have a ≤ b && b < c (or a ≤ b && b ≤ c).
            // This is equivalent to (a-a) ≤ (b-a) && (b-a) < (c-a),
            // which is equivalent to 0 ≤ (b-a) && (b-a) < (c-a),
            // which is equivalent to uint(b-a) < uint(c-a).
            var ut = b.Type.ToUnsigned();
            var lhs = conv(nod(OSUB, b, a), ut);
            var rhs = nodintconst(bound);
            if (negateResult)
            { 
                // Negate top level.
                opr = brcom(opr);
            }
            var cmp = nod(opr, lhs, rhs);
            cmp.Pos = n.Pos;
            cmp = addinit(cmp, l.Ninit.Slice());
            cmp = addinit(cmp, r.Ninit.Slice()); 
            // Typecheck the AST rooted at cmp...
            cmp = typecheck(cmp, Erv); 
            // ...but then reset cmp's type to match n's type.
            cmp.Type = n.Type;
            cmp = walkexpr(cmp, init);
            return cmp;
        }

        // return 1 if integer n must be in range [0, max), 0 otherwise
        private static bool bounded(ref Node n, long max)
        {
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
        private static void usemethod(ref Node n)
        {
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
                    return;
                }

                n = n__prev1;

            }
            {
                var n__prev1 = n;

                n = t.NumResults();

                if (n != 1L && n != 2L)
                {
                    return;
                }

                n = n__prev1;

            }
            var p0 = t.Params().Field(0L);
            var res0 = t.Results().Field(0L);
            ref types.Field res1 = default;
            if (t.NumResults() == 2L)
            {
                res1 = t.Results().Field(1L);
            }
            if (res1 == null)
            {
                if (p0.Type.Etype != TINT)
                {
                    return;
                }
            }
            else
            {
                if (!p0.Type.IsString())
                {
                    return;
                }
                if (!res1.Type.IsBoolean())
                {
                    return;
                }
            } 

            // Note: Don't rely on res0.Type.String() since its formatting depends on multiple factors
            //       (including global variables such as numImports - was issue #19028).
            {
                var s = res0.Type.Sym;

                if (s != null && s.Name == "Method" && s.Pkg != null && s.Pkg.Path == "reflect")
                {
                    Curfn.Func.SetReflectMethod(true);
                }

            }
        }

        private static void usefield(ref Node n)
        {
            if (objabi.Fieldtrack_enabled == 0L)
            {
                return;
            }

            if (n.Op == ODOT || n.Op == ODOTPTR) 
                break;
            else 
                Fatalf("usefield %v", n.Op);
                        if (n.Sym == null)
            { 
                // No field name.  This DOTPTR was built by the compiler for access
                // to runtime data structures.  Ignore.
                return;
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
                return;
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
            if (!exportname(field.Sym.Name))
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
                if (!candiscard(n))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool candiscard(ref Node n)
        {
            if (n == null)
            {
                return true;
            }

            if (n.Op == ONAME || n.Op == ONONAME || n.Op == OTYPE || n.Op == OPACK || n.Op == OLITERAL || n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OADDSTR || n.Op == OADDR || n.Op == OANDAND || n.Op == OARRAYBYTESTR || n.Op == OARRAYRUNESTR || n.Op == OSTRARRAYBYTE || n.Op == OSTRARRAYRUNE || n.Op == OCAP || n.Op == OCMPIFACE || n.Op == OCMPSTR || n.Op == OCOMPLIT || n.Op == OMAPLIT || n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OPTRLIT || n.Op == OCONV || n.Op == OCONVIFACE || n.Op == OCONVNOP || n.Op == ODOT || n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE || n.Op == OKEY || n.Op == OSTRUCTKEY || n.Op == OLEN || n.Op == OMUL || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == ONEW || n.Op == ONOT || n.Op == OCOM || n.Op == OPLUS || n.Op == OMINUS || n.Op == OOROR || n.Op == OPAREN || n.Op == ORUNESTR || n.Op == OREAL || n.Op == OIMAG || n.Op == OCOMPLEX) 
                break; 

                // Discardable as long as we know it's not division by zero.
            else if (n.Op == ODIV || n.Op == OMOD) 
                if (Isconst(n.Right, CTINT) && n.Right.Val().U._<ref Mpint>().CmpInt64(0L) != 0L)
                {
                    break;
                }
                if (Isconst(n.Right, CTFLT) && n.Right.Val().U._<ref Mpflt>().CmpFloat64(0L) != 0L)
                {
                    break;
                }
                return false; 

                // Discardable as long as we know it won't fail because of a bad size.
            else if (n.Op == OMAKECHAN || n.Op == OMAKEMAP) 
                if (Isconst(n.Left, CTINT) && n.Left.Val().U._<ref Mpint>().CmpInt64(0L) == 0L)
                {
                    break;
                }
                return false; 

                // Difficult to tell what sizes are okay.
            else if (n.Op == OMAKESLICE) 
                return false;
            else 
                return false; 

                // Discardable as long as the subpieces are.
                        if (!candiscard(n.Left) || !candiscard(n.Right) || !candiscardlist(n.Ninit) || !candiscardlist(n.Nbody) || !candiscardlist(n.List) || !candiscardlist(n.Rlist))
            {
                return false;
            }
            return true;
        }

        // rewrite
        //    print(x, y, z)
        // into
        //    func(a1, a2, a3) {
        //        print(a1, a2, a3)
        //    }(x, y, z)
        // and same for println.

        private static long walkprintfunc_prgen = default;

        // The result of walkprintfunc MUST be assigned back to n, e.g.
        //     n.Left = walkprintfunc(n.Left, init)
        private static ref Node walkprintfunc(ref Node n, ref Nodes init)
        {
            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(ref n.Ninit);
            }
            var t = nod(OTFUNC, null, null);
            slice<ref Node> printargs = default;
            foreach (var (i, n1) in n.List.Slice())
            {
                var buf = fmt.Sprintf("a%d", i);
                var a = namedfield(buf, n1.Type);
                t.List.Append(a);
                printargs = append(printargs, a.Left);
            }
            var oldfn = Curfn;
            Curfn = null;

            walkprintfunc_prgen++;
            var sym = lookupN("print·%d", walkprintfunc_prgen);
            var fn = dclfunc(sym, t);

            a = nod(n.Op, null, null);
            a.List.Set(printargs);
            a = typecheck(a, Etop);
            a = walkstmt(a);

            fn.Nbody.Set1(a);

            funcbody();

            fn = typecheck(fn, Etop);
            typecheckslice(fn.Nbody.Slice(), Etop);
            xtop = append(xtop, fn);
            Curfn = oldfn;

            a = nod(OCALL, null, null);
            a.Left = fn.Func.Nname;
            a.List.Set(n.List.Slice());
            a = typecheck(a, Etop);
            a = walkexpr(a, init);
            return a;
        }

        // substArgTypes substitutes the given list of types for
        // successive occurrences of the "any" placeholder in the
        // type syntax expression n.Type.
        // The result of substArgTypes MUST be assigned back to old, e.g.
        //     n.Left = substArgTypes(n.Left, t1, t2)
        private static ref Node substArgTypes(ref Node old, params ptr<types.Type>[] types_)
        {
            types_ = types_.Clone();

            var n = old.Value; // make shallow copy

            foreach (var (_, t) in types_)
            {
                dowidth(t);
            }
            n.Type = types.SubstAny(n.Type, ref types_);
            if (len(types_) > 0L)
            {
                Fatalf("substArgTypes: too many argument types");
            }
            return ref n;
        }
    }
}}}}
