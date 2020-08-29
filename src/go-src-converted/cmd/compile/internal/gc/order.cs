// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:49 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\order.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
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
        // Rewrite tree to use separate statements to enforce
        // order of evaluation. Makes walk easier, because it
        // can (after this runs) reorder at will within an expression.
        //
        // Rewrite x op= y into x = x op y.
        //
        // Introduce temporaries as needed by runtime routines.
        // For example, the map runtime routines take the map key
        // by reference, so make sure all map keys are addressable
        // by copying them to temporaries as needed.
        // The same is true for channel operations.
        //
        // Arrange that map index expressions only appear in direct
        // assignments x = m[k] or m[k] = x, never in larger expressions.
        //
        // Arrange that receive expressions only appear in direct assignments
        // x = <-c or as standalone statements <-c, never in larger expressions.

        // TODO(rsc): The temporary introduction during multiple assignments
        // should be moved into this file, so that the temporaries can be cleaned
        // and so that conversions implicit in the OAS2FUNC and OAS2RECV
        // nodes can be made explicit and then have their temporaries cleaned.

        // TODO(rsc): Goto and multilevel break/continue can jump over
        // inserted VARKILL annotations. Work out a way to handle these.
        // The current implementation is safe, in that it will execute correctly.
        // But it won't reuse temporaries as aggressively as it might, and
        // it can result in unnecessary zeroing of those variables in the function
        // prologue.

        // Order holds state during the ordering process.
        public partial struct Order
        {
            public slice<ref Node> @out; // list of generated statements
            public slice<ref Node> temp; // stack of temporary variables
        }

        // Order rewrites fn->nbody to apply the ordering constraints
        // described in the comment at the top of the file.
        private static void order(ref Node fn)
        {
            if (Debug['W'] > 1L)
            {
                var s = fmt.Sprintf("\nbefore order %v", fn.Func.Nname.Sym);
                dumplist(s, fn.Nbody);
            }
            orderblockNodes(ref fn.Nbody);
        }

        // Ordertemp allocates a new temporary with the given type,
        // pushes it onto the temp stack, and returns it.
        // If clear is true, ordertemp emits code to zero the temporary.
        private static ref Node ordertemp(ref types.Type t, ref Order order, bool clear)
        {
            var var_ = temp(t);
            if (clear)
            {
                var a = nod(OAS, var_, null);
                a = typecheck(a, Etop);
                order.@out = append(order.@out, a);
            }
            order.temp = append(order.temp, var_);
            return var_;
        }

        // Ordercopyexpr behaves like ordertemp but also emits
        // code to initialize the temporary to the value n.
        //
        // The clear argument is provided for use when the evaluation
        // of tmp = n turns into a function call that is passed a pointer
        // to the temporary as the output space. If the call blocks before
        // tmp has been written, the garbage collector will still treat the
        // temporary as live, so we must zero it before entering that call.
        // Today, this only happens for channel receive operations.
        // (The other candidate would be map access, but map access
        // returns a pointer to the result data instead of taking a pointer
        // to be filled in.)
        private static ref Node ordercopyexpr(ref Node n, ref types.Type t, ref Order order, long clear)
        {
            var var_ = ordertemp(t, order, clear != 0L);
            var a = nod(OAS, var_, n);
            a = typecheck(a, Etop);
            order.@out = append(order.@out, a);
            return var_;
        }

        // Ordercheapexpr returns a cheap version of n.
        // The definition of cheap is that n is a variable or constant.
        // If not, ordercheapexpr allocates a new tmp, emits tmp = n,
        // and then returns tmp.
        private static ref Node ordercheapexpr(ref Node n, ref Order order)
        {
            if (n == null)
            {
                return null;
            }

            if (n.Op == ONAME || n.Op == OLITERAL) 
                return n;
            else if (n.Op == OLEN || n.Op == OCAP) 
                var l = ordercheapexpr(n.Left, order);
                if (l == n.Left)
                {
                    return n;
                }
                var a = n.Value;
                a.Orig = ref a;
                a.Left = l;
                return typecheck(ref a, Erv);
                        return ordercopyexpr(n, n.Type, order, 0L);
        }

        // Ordersafeexpr returns a safe version of n.
        // The definition of safe is that n can appear multiple times
        // without violating the semantics of the original program,
        // and that assigning to the safe version has the same effect
        // as assigning to the original n.
        //
        // The intended use is to apply to x when rewriting x += y into x = x + y.
        private static ref Node ordersafeexpr(ref Node n, ref Order order)
        {

            if (n.Op == ONAME || n.Op == OLITERAL) 
                return n;
            else if (n.Op == ODOT || n.Op == OLEN || n.Op == OCAP) 
                var l = ordersafeexpr(n.Left, order);
                if (l == n.Left)
                {
                    return n;
                }
                var a = n.Value;
                a.Orig = ref a;
                a.Left = l;
                return typecheck(ref a, Erv);
            else if (n.Op == ODOTPTR || n.Op == OIND) 
                l = ordercheapexpr(n.Left, order);
                if (l == n.Left)
                {
                    return n;
                }
                a = n.Value;
                a.Orig = ref a;
                a.Left = l;
                return typecheck(ref a, Erv);
            else if (n.Op == OINDEX || n.Op == OINDEXMAP) 
                l = default;
                if (n.Left.Type.IsArray())
                {
                    l = ordersafeexpr(n.Left, order);
                }
                else
                {
                    l = ordercheapexpr(n.Left, order);
                }
                var r = ordercheapexpr(n.Right, order);
                if (l == n.Left && r == n.Right)
                {
                    return n;
                }
                a = n.Value;
                a.Orig = ref a;
                a.Left = l;
                a.Right = r;
                return typecheck(ref a, Erv);
            else 
                Fatalf("ordersafeexpr %v", n.Op);
                return null; // not reached
                    }

        // Isaddrokay reports whether it is okay to pass n's address to runtime routines.
        // Taking the address of a variable makes the liveness and optimization analyses
        // lose track of where the variable's lifetime ends. To avoid hurting the analyses
        // of ordinary stack variables, those are not 'isaddrokay'. Temporaries are okay,
        // because we emit explicit VARKILL instructions marking the end of those
        // temporaries' lifetimes.
        private static bool isaddrokay(ref Node n)
        {
            return islvalue(n) && (n.Op != ONAME || n.Class() == PEXTERN || n.IsAutoTmp());
        }

        // Orderaddrtemp ensures that n is okay to pass by address to runtime routines.
        // If the original argument n is not okay, orderaddrtemp creates a tmp, emits
        // tmp = n, and then returns tmp.
        // The result of orderaddrtemp MUST be assigned back to n, e.g.
        //     n.Left = orderaddrtemp(n.Left, order)
        private static ref Node orderaddrtemp(ref Node n, ref Order order)
        {
            if (consttype(n) > 0L)
            { 
                // TODO: expand this to all static composite literal nodes?
                n = defaultlit(n, null);
                dowidth(n.Type);
                var vstat = staticname(n.Type);
                vstat.Name.SetReadonly(true);
                slice<ref Node> @out = default;
                staticassign(vstat, n, ref out);
                if (out != null)
                {
                    Fatalf("staticassign of const generated code: %+v", n);
                }
                vstat = typecheck(vstat, Erv);
                return vstat;
            }
            if (isaddrokay(n))
            {
                return n;
            }
            return ordercopyexpr(n, n.Type, order, 0L);
        }

        // ordermapkeytemp prepares n to be a key in a map runtime call and returns n.
        // It should only be used for map runtime calls which have *_fast* versions.
        private static ref Node ordermapkeytemp(ref types.Type t, ref Node n, ref Order order)
        { 
            // Most map calls need to take the address of the key.
            // Exception: map*_fast* calls. See golang.org/issue/19015.
            if (mapfast(t) == mapslow)
            {
                return orderaddrtemp(n, order);
            }
            return n;
        }

        private partial struct ordermarker // : long
        {
        }

        // Marktemp returns the top of the temporary variable stack.
        private static ordermarker marktemp(ref Order order)
        {
            return ordermarker(len(order.temp));
        }

        // Poptemp pops temporaries off the stack until reaching the mark,
        // which must have been returned by marktemp.
        private static void poptemp(ordermarker mark, ref Order order)
        {
            order.temp = order.temp[..mark];
        }

        // Cleantempnopop emits to *out VARKILL instructions for each temporary
        // above the mark on the temporary stack, but it does not pop them
        // from the stack.
        private static void cleantempnopop(ordermarker mark, ref Order order, ref slice<ref Node> @out)
        {
            for (var i = len(order.temp) - 1L; i >= int(mark); i--)
            {
                var n = order.temp[i];
                if (n.Name.Keepalive())
                {
                    n.Name.SetKeepalive(false);
                    n.SetAddrtaken(true); // ensure SSA keeps the n variable
                    var kill = nod(OVARLIVE, n, null);
                    kill = typecheck(kill, Etop);
                    out.Value = append(out.Value, kill);
                }
                kill = nod(OVARKILL, n, null);
                kill = typecheck(kill, Etop);
                out.Value = append(out.Value, kill);
            }

        }

        // Cleantemp emits VARKILL instructions for each temporary above the
        // mark on the temporary stack and removes them from the stack.
        private static void cleantemp(ordermarker top, ref Order order)
        {
            cleantempnopop(top, order, ref order.@out);
            poptemp(top, order);
        }

        // Orderstmtlist orders each of the statements in the list.
        private static void orderstmtlist(Nodes l, ref Order order)
        {
            foreach (var (_, n) in l.Slice())
            {
                orderstmt(n, order);
            }
        }

        // Orderblock orders the block of statements l onto a new list,
        // and returns the ordered list.
        private static slice<ref Node> orderblock(Nodes l)
        {
            Order order = default;
            var mark = marktemp(ref order);
            orderstmtlist(l, ref order);
            cleantemp(mark, ref order);
            return order.@out;
        }

        // OrderblockNodes orders the block of statements in n into a new slice,
        // and then replaces the old slice in n with the new slice.
        private static void orderblockNodes(ref Nodes n)
        {
            Order order = default;
            var mark = marktemp(ref order);
            orderstmtlist(n.Value, ref order);
            cleantemp(mark, ref order);
            n.Set(order.@out);
        }

        // Orderexprinplace orders the side effects in *np and
        // leaves them as the init list of the final *np.
        // The result of orderexprinplace MUST be assigned back to n, e.g.
        //     n.Left = orderexprinplace(n.Left, outer)
        private static ref Node orderexprinplace(ref Node n, ref Order outer)
        {
            Order order = default;
            n = orderexpr(n, ref order, null);
            n = addinit(n, order.@out); 

            // insert new temporaries from order
            // at head of outer list.
            outer.temp = append(outer.temp, order.temp);
            return n;
        }

        // Orderstmtinplace orders the side effects of the single statement *np
        // and replaces it with the resulting statement list.
        // The result of orderstmtinplace MUST be assigned back to n, e.g.
        //     n.Left = orderstmtinplace(n.Left)
        private static ref Node orderstmtinplace(ref Node n)
        {
            Order order = default;
            var mark = marktemp(ref order);
            orderstmt(n, ref order);
            cleantemp(mark, ref order);
            return liststmt(order.@out);
        }

        // Orderinit moves n's init list to order->out.
        private static void orderinit(ref Node n, ref Order order)
        {
            if (n.mayBeShared())
            { 
                // For concurrency safety, don't mutate potentially shared nodes.
                // First, ensure that no work is required here.
                if (n.Ninit.Len() > 0L)
                {
                    Fatalf("orderinit shared node with ninit");
                }
                return;
            }
            orderstmtlist(n.Ninit, order);
            n.Ninit.Set(null);
        }

        // Ismulticall reports whether the list l is f() for a multi-value function.
        // Such an f() could appear as the lone argument to a multi-arg function.
        private static bool ismulticall(Nodes l)
        { 
            // one arg only
            if (l.Len() != 1L)
            {
                return false;
            }
            var n = l.First(); 

            // must be call

            if (n.Op == OCALLFUNC || n.Op == OCALLMETH || n.Op == OCALLINTER) 
                break;
            else 
                return false;
            // call must return multiple values
            return n.Left.Type.NumResults() > 1L;
        }

        // Copyret emits t1, t2, ... = n, where n is a function call,
        // and then returns the list t1, t2, ....
        private static slice<ref Node> copyret(ref Node n, ref Order order)
        {
            if (!n.Type.IsFuncArgStruct())
            {
                Fatalf("copyret %v %d", n.Type, n.Left.Type.NumResults());
            }
            slice<ref Node> l1 = default;
            slice<ref Node> l2 = default;
            foreach (var (_, t) in n.Type.Fields().Slice())
            {
                var tmp = temp(t.Type);
                l1 = append(l1, tmp);
                l2 = append(l2, tmp);
            }
            var @as = nod(OAS2, null, null);
            @as.List.Set(l1);
            @as.Rlist.Set1(n);
            as = typecheck(as, Etop);
            orderstmt(as, order);

            return l2;
        }

        // Ordercallargs orders the list of call arguments *l.
        private static void ordercallargs(ref Nodes l, ref Order order)
        {
            if (ismulticall(l.Value))
            { 
                // return f() where f() is multiple values.
                l.Set(copyret(l.First(), order));
            }
            else
            {
                orderexprlist(l.Value, order);
            }
        }

        // Ordercall orders the call expression n.
        // n->op is OCALLMETH/OCALLFUNC/OCALLINTER or a builtin like OCOPY.
        private static void ordercall(ref Node n, ref Order order)
        {
            n.Left = orderexpr(n.Left, order, null);
            n.Right = orderexpr(n.Right, order, null); // ODDDARG temp
            ordercallargs(ref n.List, order);

            if (n.Op == OCALLFUNC)
            {
                Action<long> keepAlive = i =>
                { 
                    // If the argument is really a pointer being converted to uintptr,
                    // arrange for the pointer to be kept alive until the call returns,
                    // by copying it into a temp and marking that temp
                    // still alive when we pop the temp stack.
                    var xp = n.List.Addr(i);
                    while (ref xp == OCONVNOP && !ref xp.IsUnsafePtr())
                    {
                        xp = ref ref xp;
                    }

                    var x = xp.Value;
                    if (x.Type.IsUnsafePtr())
                    {
                        x = ordercopyexpr(x, x.Type, order, 0L);
                        x.Name.SetKeepalive(true);
                        xp.Value = x;
                    }
                }
;

                foreach (var (i, t) in n.Left.Type.Params().FieldSlice())
                { 
                    // Check for "unsafe-uintptr" tag provided by escape analysis.
                    if (t.Isddd() && !n.Isddd())
                    {
                        if (t.Note == uintptrEscapesTag)
                        {
                            while (i < n.List.Len())
                            {
                                keepAlive(i);
                                i++;
                            }

                        }
                    else
                    }                    {
                        if (t.Note == unsafeUintptrTag || t.Note == uintptrEscapesTag)
                        {
                            keepAlive(i);
                        }
                    }
                }
            }
        }

        // Ordermapassign appends n to order->out, introducing temporaries
        // to make sure that all map assignments have the form m[k] = x.
        // (Note: orderexpr has already been called on n, so we know k is addressable.)
        //
        // If n is the multiple assignment form ..., m[k], ... = ..., x, ..., the rewrite is
        //    t1 = m
        //    t2 = k
        //    ...., t3, ... = ..., x, ...
        //    t1[t2] = t3
        //
        // The temporaries t1, t2 are needed in case the ... being assigned
        // contain m or k. They are usually unnecessary, but in the unnecessary
        // cases they are also typically registerizable, so not much harm done.
        // And this only applies to the multiple-assignment form.
        // We could do a more precise analysis if needed, like in walk.go.
        private static void ordermapassign(ref Node n, ref Order order)
        {

            if (n.Op == OAS) 
                if (n.Left.Op == OINDEXMAP)
                { 
                    // Make sure we evaluate the RHS before starting the map insert.
                    // We need to make sure the RHS won't panic.  See issue 22881.
                    n.Right = ordercheapexpr(n.Right, order);
                }
                order.@out = append(order.@out, n);
            else if (n.Op == OAS2 || n.Op == OAS2DOTTYPE || n.Op == OAS2MAPR || n.Op == OAS2FUNC) 
                slice<ref Node> post = default;
                foreach (var (i, m) in n.List.Slice())
                {

                    if (m.Op == OINDEXMAP)
                    {
                        if (!m.Left.IsAutoTmp())
                        {
                            m.Left = ordercopyexpr(m.Left, m.Left.Type, order, 0L);
                        }
                        if (!m.Right.IsAutoTmp())
                        {
                            m.Right = ordercopyexpr(m.Right, m.Right.Type, order, 0L);
                        }
                        fallthrough = true;
                    }
                    if (fallthrough || instrumenting && n.Op == OAS2FUNC && !isblank(m))
                    {
                        var t = ordertemp(m.Type, order, false);
                        n.List.SetIndex(i, t);
                        var a = nod(OAS, m, t);
                        a = typecheck(a, Etop);
                        post = append(post, a);
                        goto __switch_break0;
                    }

                    __switch_break0:;
                }
                order.@out = append(order.@out, n);
                order.@out = append(order.@out, post);
            else 
                Fatalf("ordermapassign %v", n.Op);
                    }

        // Orderstmt orders the statement n, appending to order->out.
        // Temporaries created during the statement are cleaned
        // up using VARKILL instructions as possible.
        private static void orderstmt(ref Node n, ref Order order)
        {
            if (n == null)
            {
                return;
            }
            var lno = setlineno(n);

            orderinit(n, order);


            if (n.Op == OVARKILL || n.Op == OVARLIVE) 
                order.@out = append(order.@out, n);
            else if (n.Op == OAS) 
                var t = marktemp(order);
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, n.Left);
                ordermapassign(n, order);
                cleantemp(t, order);
            else if (n.Op == OAS2 || n.Op == OCLOSE || n.Op == OCOPY || n.Op == OPRINT || n.Op == OPRINTN || n.Op == ORECOVER || n.Op == ORECV) 
                t = marktemp(order);
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                orderexprlist(n.List, order);
                orderexprlist(n.Rlist, order);

                if (n.Op == OAS2) 
                    ordermapassign(n, order);
                else 
                    order.@out = append(order.@out, n);
                                cleantemp(t, order);
            else if (n.Op == OASOP) 
                // Special: rewrite l op= r into l = l op r.
                // This simplifies quite a few operations;
                // most important is that it lets us separate
                // out map read from map write when l is
                // a map index expression.
                t = marktemp(order);
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);

                n.Left = ordersafeexpr(n.Left, order);
                var tmp1 = treecopy(n.Left, src.NoXPos);
                if (tmp1.Op == OINDEXMAP)
                {
                    tmp1.Etype = 0L; // now an rvalue not an lvalue
                }
                tmp1 = ordercopyexpr(tmp1, n.Left.Type, order, 0L); 
                // TODO(marvin): Fix Node.EType type union.
                n.Right = nod(Op(n.Etype), tmp1, n.Right);
                n.Right = typecheck(n.Right, Erv);
                n.Right = orderexpr(n.Right, order, null);
                n.Etype = 0L;
                n.Op = OAS;
                ordermapassign(n, order);
                cleantemp(t, order); 

                // Special: make sure key is addressable if needed,
                // and make sure OINDEXMAP is not copied out.
            else if (n.Op == OAS2MAPR) 
                t = marktemp(order);

                orderexprlist(n.List, order);
                var r = n.Rlist.First();
                r.Left = orderexpr(r.Left, order, null);
                r.Right = orderexpr(r.Right, order, null); 

                // See case OINDEXMAP below.
                if (r.Right.Op == OARRAYBYTESTR)
                {
                    r.Right.Op = OARRAYBYTESTRTMP;
                }
                r.Right = ordermapkeytemp(r.Left.Type, r.Right, order);
                orderokas2(n, order);
                cleantemp(t, order); 

                // Special: avoid copy of func call n->rlist->n.
            else if (n.Op == OAS2FUNC) 
                t = marktemp(order);

                orderexprlist(n.List, order);
                ordercall(n.Rlist.First(), order);
                orderas2(n, order);
                cleantemp(t, order); 

                // Special: use temporary variables to hold result,
                // so that assertI2Tetc can take address of temporary.
                // No temporary for blank assignment.
            else if (n.Op == OAS2DOTTYPE) 
                t = marktemp(order);

                orderexprlist(n.List, order);
                n.Rlist.First().Left = orderexpr(n.Rlist.First().Left, order, null); // i in i.(T)
                orderokas2(n, order);
                cleantemp(t, order); 

                // Special: use temporary variables to hold result,
                // so that chanrecv can take address of temporary.
            else if (n.Op == OAS2RECV) 
                t = marktemp(order);

                orderexprlist(n.List, order);
                n.Rlist.First().Left = orderexpr(n.Rlist.First().Left, order, null); // arg to recv
                var ch = n.Rlist.First().Left.Type;
                tmp1 = ordertemp(ch.Elem(), order, types.Haspointers(ch.Elem()));
                var tmp2 = ordertemp(types.Types[TBOOL], order, false);
                order.@out = append(order.@out, n);
                r = nod(OAS, n.List.First(), tmp1);
                r = typecheck(r, Etop);
                ordermapassign(r, order);
                r = okas(n.List.Second(), tmp2);
                r = typecheck(r, Etop);
                ordermapassign(r, order);
                n.List.Set2(tmp1, tmp2);
                cleantemp(t, order); 

                // Special: does not save n onto out.
            else if (n.Op == OBLOCK || n.Op == OEMPTY) 
                orderstmtlist(n.List, order); 

                // Special: n->left is not an expression; save as is.
            else if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == ODCL || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OFALL || n.Op == OGOTO || n.Op == OLABEL || n.Op == ORETJMP) 
                order.@out = append(order.@out, n); 

                // Special: handle call arguments.
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH) 
                t = marktemp(order);

                ordercall(n, order);
                order.@out = append(order.@out, n);
                cleantemp(t, order); 

                // Special: order arguments to inner call but not call itself.
            else if (n.Op == ODEFER || n.Op == OPROC) 
                t = marktemp(order);


                // Delete will take the address of the key.
                // Copy key into new temp and do not clean it
                // (it persists beyond the statement).
                if (n.Left.Op == ODELETE) 
                    orderexprlist(n.Left.List, order);

                    if (mapfast(n.Left.List.First().Type) == mapslow)
                    {
                        var t1 = marktemp(order);
                        var np = n.Left.List.Addr(1L); // map key
                        np.Value = ordercopyexpr(np.Value, ref np, order, 0L);
                        poptemp(t1, order);
                    }
                else 
                    ordercall(n.Left, order);
                                order.@out = append(order.@out, n);
                cleantemp(t, order);
            else if (n.Op == ODELETE) 
                t = marktemp(order);
                n.List.SetFirst(orderexpr(n.List.First(), order, null));
                n.List.SetSecond(orderexpr(n.List.Second(), order, null));
                n.List.SetSecond(ordermapkeytemp(n.List.First().Type, n.List.Second(), order));
                order.@out = append(order.@out, n);
                cleantemp(t, order); 

                // Clean temporaries from condition evaluation at
                // beginning of loop body and after for statement.
            else if (n.Op == OFOR) 
                t = marktemp(order);

                n.Left = orderexprinplace(n.Left, order);
                slice<ref Node> l = default;
                cleantempnopop(t, order, ref l);
                n.Nbody.Prepend(l);
                orderblockNodes(ref n.Nbody);
                n.Right = orderstmtinplace(n.Right);
                order.@out = append(order.@out, n);
                cleantemp(t, order); 

                // Clean temporaries from condition at
                // beginning of both branches.
            else if (n.Op == OIF) 
                t = marktemp(order);

                n.Left = orderexprinplace(n.Left, order);
                l = default;
                cleantempnopop(t, order, ref l);
                n.Nbody.Prepend(l);
                l = null;
                cleantempnopop(t, order, ref l);
                n.Rlist.Prepend(l);
                poptemp(t, order);
                orderblockNodes(ref n.Nbody);
                n.Rlist.Set(orderblock(n.Rlist));
                order.@out = append(order.@out, n); 

                // Special: argument will be converted to interface using convT2E
                // so make sure it is an addressable temporary.
            else if (n.Op == OPANIC) 
                t = marktemp(order);

                n.Left = orderexpr(n.Left, order, null);
                if (!n.Left.Type.IsInterface())
                {
                    n.Left = orderaddrtemp(n.Left, order);
                }
                order.@out = append(order.@out, n);
                cleantemp(t, order);
            else if (n.Op == ORANGE) 
                // n.Right is the expression being ranged over.
                // order it, and then make a copy if we need one.
                // We almost always do, to ensure that we don't
                // see any value changes made during the loop.
                // Usually the copy is cheap (e.g., array pointer,
                // chan, slice, string are all tiny).
                // The exception is ranging over an array value
                // (not a slice, not a pointer to array),
                // which must make a copy to avoid seeing updates made during
                // the range body. Ranging over an array value is uncommon though.

                // Mark []byte(str) range expression to reuse string backing storage.
                // It is safe because the storage cannot be mutated.
                if (n.Right.Op == OSTRARRAYBYTE)
                {
                    n.Right.Op = OSTRARRAYBYTETMP;
                }
                t = marktemp(order);
                n.Right = orderexpr(n.Right, order, null);

                if (n.Type.Etype == TARRAY || n.Type.Etype == TSLICE)
                {
                    if (n.List.Len() < 2L || isblank(n.List.Second()))
                    { 
                        // for i := range x will only use x once, to compute len(x).
                        // No need to copy it.
                        break;
                    }
                    fallthrough = true;

                }
                if (fallthrough || n.Type.Etype == TCHAN || n.Type.Etype == TSTRING) 
                {
                    // chan, string, slice, array ranges use value multiple times.
                    // make copy.
                    r = n.Right;

                    if (r.Type.IsString() && r.Type != types.Types[TSTRING])
                    {
                        r = nod(OCONV, r, null);
                        r.Type = types.Types[TSTRING];
                        r = typecheck(r, Erv);
                    }
                    n.Right = ordercopyexpr(r, r.Type, order, 0L);
                    goto __switch_break1;
                }
                if (n.Type.Etype == TMAP) 
                {
                    // copy the map value in case it is a map literal.
                    // TODO(rsc): Make tmp = literal expressions reuse tmp.
                    // For maps tmp is just one word so it hardly matters.
                    r = n.Right;
                    n.Right = ordercopyexpr(r, r.Type, order, 0L); 

                    // prealloc[n] is the temp for the iterator.
                    // hiter contains pointers and needs to be zeroed.
                    prealloc[n] = ordertemp(hiter(n.Type), order, true);
                    goto __switch_break1;
                }
                // default: 
                    Fatalf("orderstmt range %v", n.Type);

                __switch_break1:;
                {
                    var i__prev1 = i;

                    foreach (var (__i, __n1) in n.List.Slice())
                    {
                        i = __i;
                        n1 = __n1;
                        n.List.SetIndex(i, orderexprinplace(n1, order));
                    }

                    i = i__prev1;
                }

                orderblockNodes(ref n.Nbody);
                order.@out = append(order.@out, n);
                cleantemp(t, order);
            else if (n.Op == ORETURN) 
                ordercallargs(ref n.List, order);
                order.@out = append(order.@out, n); 

                // Special: clean case temporaries in each block entry.
                // Select must enter one of its blocks, so there is no
                // need for a cleaning at the end.
                // Doubly special: evaluation order for select is stricter
                // than ordinary expressions. Even something like p.c
                // has to be hoisted into a temporary, so that it cannot be
                // reordered after the channel evaluation for a different
                // case (if p were nil, then the timing of the fault would
                // give this away).
            else if (n.Op == OSELECT) 
                t = marktemp(order);

                foreach (var (_, n2) in n.List.Slice())
                {
                    if (n2.Op != OXCASE)
                    {
                        Fatalf("order select case %v", n2.Op);
                    }
                    r = n2.Left;
                    setlineno(n2); 

                    // Append any new body prologue to ninit.
                    // The next loop will insert ninit into nbody.
                    if (n2.Ninit.Len() != 0L)
                    {
                        Fatalf("order select ninit");
                    }
                    if (r != null)
                    {

                        if (r.Op == OSELRECV || r.Op == OSELRECV2) 
                            if (r.Colas())
                            {
                                long i = 0L;
                                if (r.Ninit.Len() != 0L && r.Ninit.First().Op == ODCL && r.Ninit.First().Left == r.Left)
                                {
                                    i++;
                                }
                                if (i < r.Ninit.Len() && r.Ninit.Index(i).Op == ODCL && r.List.Len() != 0L && r.Ninit.Index(i).Left == r.List.First())
                                {
                                    i++;
                                }
                                if (i >= r.Ninit.Len())
                                {
                                    r.Ninit.Set(null);
                                }
                            }
                            if (r.Ninit.Len() != 0L)
                            {
                                dumplist("ninit", r.Ninit);
                                Fatalf("ninit on select recv");
                            } 

                            // case x = <-c
                            // case x, ok = <-c
                            // r->left is x, r->ntest is ok, r->right is ORECV, r->right->left is c.
                            // r->left == N means 'case <-c'.
                            // c is always evaluated; x and ok are only evaluated when assigned.
                            r.Right.Left = orderexpr(r.Right.Left, order, null);

                            if (r.Right.Left.Op != ONAME)
                            {
                                r.Right.Left = ordercopyexpr(r.Right.Left, r.Right.Left.Type, order, 0L);
                            } 

                            // Introduce temporary for receive and move actual copy into case body.
                            // avoids problems with target being addressed, as usual.
                            // NOTE: If we wanted to be clever, we could arrange for just one
                            // temporary per distinct type, sharing the temp among all receives
                            // with that temp. Similarly one ok bool could be shared among all
                            // the x,ok receives. Not worth doing until there's a clear need.
                            if (r.Left != null && isblank(r.Left))
                            {
                                r.Left = null;
                            }
                            if (r.Left != null)
                            { 
                                // use channel element type for temporary to avoid conversions,
                                // such as in case interfacevalue = <-intchan.
                                // the conversion happens in the OAS instead.
                                tmp1 = r.Left;

                                if (r.Colas())
                                {
                                    tmp2 = nod(ODCL, tmp1, null);
                                    tmp2 = typecheck(tmp2, Etop);
                                    n2.Ninit.Append(tmp2);
                                }
                                r.Left = ordertemp(r.Right.Left.Type.Elem(), order, types.Haspointers(r.Right.Left.Type.Elem()));
                                tmp2 = nod(OAS, tmp1, r.Left);
                                tmp2 = typecheck(tmp2, Etop);
                                n2.Ninit.Append(tmp2);
                            }
                            if (r.List.Len() != 0L && isblank(r.List.First()))
                            {
                                r.List.Set(null);
                            }
                            if (r.List.Len() != 0L)
                            {
                                tmp1 = r.List.First();
                                if (r.Colas())
                                {
                                    tmp2 = nod(ODCL, tmp1, null);
                                    tmp2 = typecheck(tmp2, Etop);
                                    n2.Ninit.Append(tmp2);
                                }
                                r.List.Set1(ordertemp(types.Types[TBOOL], order, false));
                                tmp2 = okas(tmp1, r.List.First());
                                tmp2 = typecheck(tmp2, Etop);
                                n2.Ninit.Append(tmp2);
                            }
                            n2.Ninit.Set(orderblock(n2.Ninit));
                        else if (r.Op == OSEND) 
                            if (r.Ninit.Len() != 0L)
                            {
                                dumplist("ninit", r.Ninit);
                                Fatalf("ninit on select send");
                            } 

                            // case c <- x
                            // r->left is c, r->right is x, both are always evaluated.
                            r.Left = orderexpr(r.Left, order, null);

                            if (!r.Left.IsAutoTmp())
                            {
                                r.Left = ordercopyexpr(r.Left, r.Left.Type, order, 0L);
                            }
                            r.Right = orderexpr(r.Right, order, null);
                            if (!r.Right.IsAutoTmp())
                            {
                                r.Right = ordercopyexpr(r.Right, r.Right.Type, order, 0L);
                            }
                        else 
                            Dump("select case", r);
                            Fatalf("unknown op in select %v", r.Op); 

                            // If this is case x := <-ch or case x, y := <-ch, the case has
                            // the ODCL nodes to declare x and y. We want to delay that
                            // declaration (and possible allocation) until inside the case body.
                            // Delete the ODCL nodes here and recreate them inside the body below.
                                            }
                    orderblockNodes(ref n2.Nbody);
                } 
                // Now that we have accumulated all the temporaries, clean them.
                // Also insert any ninit queued during the previous loop.
                // (The temporary cleaning must follow that ninit work.)
                foreach (var (_, n3) in n.List.Slice())
                {
                    var s = n3.Ninit.Slice();
                    cleantempnopop(t, order, ref s);
                    n3.Nbody.Prepend(s);
                    n3.Ninit.Set(null);
                }
                order.@out = append(order.@out, n);
                poptemp(t, order); 

                // Special: value being sent is passed as a pointer; make it addressable.
            else if (n.Op == OSEND) 
                t = marktemp(order);

                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                if (instrumenting)
                { 
                    // Force copying to the stack so that (chan T)(nil) <- x
                    // is still instrumented as a read of x.
                    n.Right = ordercopyexpr(n.Right, n.Right.Type, order, 0L);
                }
                else
                {
                    n.Right = orderaddrtemp(n.Right, order);
                }
                order.@out = append(order.@out, n);
                cleantemp(t, order); 

                // TODO(rsc): Clean temporaries more aggressively.
                // Note that because walkswitch will rewrite some of the
                // switch into a binary search, this is not as easy as it looks.
                // (If we ran that code here we could invoke orderstmt on
                // the if-else chain instead.)
                // For now just clean all the temporaries at the end.
                // In practice that's fine.
            else if (n.Op == OSWITCH) 
                t = marktemp(order);

                n.Left = orderexpr(n.Left, order, null);
                foreach (var (_, n4) in n.List.Slice())
                {
                    if (n4.Op != OXCASE)
                    {
                        Fatalf("order switch case %v", n4.Op);
                    }
                    orderexprlistinplace(n4.List, order);
                    orderblockNodes(ref n4.Nbody);
                }
                order.@out = append(order.@out, n);
                cleantemp(t, order);
            else 
                Fatalf("orderstmt %v", n.Op);
                        lineno = lno;
        }

        // Orderexprlist orders the expression list l into order.
        private static void orderexprlist(Nodes l, ref Order order)
        {
            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = orderexpr(s[i], order, null);
            }
        }

        // Orderexprlist orders the expression list l but saves
        // the side effects on the individual expression ninit lists.
        private static void orderexprlistinplace(Nodes l, ref Order order)
        {
            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = orderexprinplace(s[i], order);
            }
        }

        // prealloc[x] records the allocation to use for x.
        private static map prealloc = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref Node>{};

        // Orderexpr orders a single expression, appending side
        // effects to order->out as needed.
        // If this is part of an assignment lhs = *np, lhs is given.
        // Otherwise lhs == nil. (When lhs != nil it may be possible
        // to avoid copying the result of the expression to a temporary.)
        // The result of orderexpr MUST be assigned back to n, e.g.
        //     n.Left = orderexpr(n.Left, order, lhs)
        private static ref Node orderexpr(ref Node n, ref Order order, ref Node lhs)
        {
            if (n == null)
            {
                return n;
            }
            var lno = setlineno(n);
            orderinit(n, order);


            if (n.Op == OADDSTR) 
                orderexprlist(n.List, order);

                if (n.List.Len() > 5L)
                {
                    var t = types.NewArray(types.Types[TSTRING], int64(n.List.Len()));
                    prealloc[n] = ordertemp(t, order, false);
                } 

                // Mark string(byteSlice) arguments to reuse byteSlice backing
                // buffer during conversion. String concatenation does not
                // memorize the strings for later use, so it is safe.
                // However, we can do it only if there is at least one non-empty string literal.
                // Otherwise if all other arguments are empty strings,
                // concatstrings will return the reference to the temp string
                // to the caller.
                var hasbyte = false;

                var haslit = false;
                foreach (var (_, n1) in n.List.Slice())
                {
                    hasbyte = hasbyte || n1.Op == OARRAYBYTESTR;
                    haslit = haslit || n1.Op == OLITERAL && len(n1.Val().U._<@string>()) != 0L;
                }
                if (haslit && hasbyte)
                {
                    foreach (var (_, n2) in n.List.Slice())
                    {
                        if (n2.Op == OARRAYBYTESTR)
                        {
                            n2.Op = OARRAYBYTESTRTMP;
                        }
                    }
                }
            else if (n.Op == OCMPSTR) 
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null); 

                // Mark string(byteSlice) arguments to reuse byteSlice backing
                // buffer during conversion. String comparison does not
                // memorize the strings for later use, so it is safe.
                if (n.Left.Op == OARRAYBYTESTR)
                {
                    n.Left.Op = OARRAYBYTESTRTMP;
                }
                if (n.Right.Op == OARRAYBYTESTR)
                {
                    n.Right.Op = OARRAYBYTESTRTMP;
                } 

                // key must be addressable
            else if (n.Op == OINDEXMAP) 
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                var needCopy = false;

                if (n.Etype == 0L && instrumenting)
                { 
                    // Race detector needs the copy so it can
                    // call treecopy on the result.
                    needCopy = true;
                } 

                // For x = m[string(k)] where k is []byte, the allocation of
                // backing bytes for the string can be avoided by reusing
                // the []byte backing array. This is a special case that it
                // would be nice to handle more generally, but because
                // there are no []byte-keyed maps, this specific case comes
                // up in important cases in practice. See issue 3512.
                // Nothing can change the []byte we are not copying before
                // the map index, because the map access is going to
                // be forced to happen immediately following this
                // conversion (by the ordercopyexpr a few lines below).
                if (n.Etype == 0L && n.Right.Op == OARRAYBYTESTR)
                {
                    n.Right.Op = OARRAYBYTESTRTMP;
                    needCopy = true;
                }
                n.Right = ordermapkeytemp(n.Left.Type, n.Right, order);
                if (needCopy)
                {
                    n = ordercopyexpr(n, n.Type, order, 0L);
                } 

                // concrete type (not interface) argument must be addressable
                // temporary to pass to runtime.
            else if (n.Op == OCONVIFACE) 
                n.Left = orderexpr(n.Left, order, null);

                if (!n.Left.Type.IsInterface())
                {
                    n.Left = orderaddrtemp(n.Left, order);
                }
            else if (n.Op == OCONVNOP) 
                if (n.Type.IsKind(TUNSAFEPTR) && n.Left.Type.IsKind(TUINTPTR) && (n.Left.Op == OCALLFUNC || n.Left.Op == OCALLINTER || n.Left.Op == OCALLMETH))
                { 
                    // When reordering unsafe.Pointer(f()) into a separate
                    // statement, the conversion and function call must stay
                    // together. See golang.org/issue/15329.
                    orderinit(n.Left, order);
                    ordercall(n.Left, order);
                    if (lhs == null || lhs.Op != ONAME || instrumenting)
                    {
                        n = ordercopyexpr(n, n.Type, order, 0L);
                    }
                }
                else
                {
                    n.Left = orderexpr(n.Left, order, null);
                }
            else if (n.Op == OANDAND || n.Op == OOROR) 
                var mark = marktemp(order);
                n.Left = orderexpr(n.Left, order, null); 

                // Clean temporaries from first branch at beginning of second.
                // Leave them on the stack so that they can be killed in the outer
                // context in case the short circuit is taken.
                slice<ref Node> s = default;

                cleantempnopop(mark, order, ref s);
                n.Right = addinit(n.Right, s);
                n.Right = orderexprinplace(n.Right, order);
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH || n.Op == OCAP || n.Op == OCOMPLEX || n.Op == OCOPY || n.Op == OIMAG || n.Op == OLEN || n.Op == OMAKECHAN || n.Op == OMAKEMAP || n.Op == OMAKESLICE || n.Op == ONEW || n.Op == OREAL || n.Op == ORECOVER || n.Op == OSTRARRAYBYTE || n.Op == OSTRARRAYBYTETMP || n.Op == OSTRARRAYRUNE) 
                ordercall(n, order);
                if (lhs == null || lhs.Op != ONAME || instrumenting)
                {
                    n = ordercopyexpr(n, n.Type, order, 0L);
                }
            else if (n.Op == OAPPEND) 
                ordercallargs(ref n.List, order);
                if (lhs == null || lhs.Op != ONAME && !samesafeexpr(lhs, n.List.First()))
                {
                    n = ordercopyexpr(n, n.Type, order, 0L);
                }
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR || n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                n.Left = orderexpr(n.Left, order, null);
                var (low, high, max) = n.SliceBounds();
                low = orderexpr(low, order, null);
                low = ordercheapexpr(low, order);
                high = orderexpr(high, order, null);
                high = ordercheapexpr(high, order);
                max = orderexpr(max, order, null);
                max = ordercheapexpr(max, order);
                n.SetSliceBounds(low, high, max);
                if (lhs == null || lhs.Op != ONAME && !samesafeexpr(lhs, n.Left))
                {
                    n = ordercopyexpr(n, n.Type, order, 0L);
                }
            else if (n.Op == OCLOSURE) 
                if (n.Noescape() && n.Func.Cvars.Len() > 0L)
                {
                    prealloc[n] = ordertemp(types.Types[TUINT8], order, false); // walk will fill in correct type
                }
            else if (n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OCALLPART) 
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                orderexprlist(n.List, order);
                orderexprlist(n.Rlist, order);
                if (n.Noescape())
                {
                    prealloc[n] = ordertemp(types.Types[TUINT8], order, false); // walk will fill in correct type
                }
            else if (n.Op == ODDDARG) 
                if (n.Noescape())
                { 
                    // The ddd argument does not live beyond the call it is created for.
                    // Allocate a temporary that will be cleaned up when this statement
                    // completes. We could be more aggressive and try to arrange for it
                    // to be cleaned up when the call completes.
                    prealloc[n] = ordertemp(n.Type.Elem(), order, false);
                }
            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                n.Left = orderexpr(n.Left, order, null); 
                // TODO(rsc): The isfat is for consistency with componentgen and walkexpr.
                // It needs to be removed in all three places.
                // That would allow inlining x.(struct{*int}) the same as x.(*int).
                if (!isdirectiface(n.Type) || isfat(n.Type) || instrumenting)
                {
                    n = ordercopyexpr(n, n.Type, order, 1L);
                }
            else if (n.Op == ORECV) 
                n.Left = orderexpr(n.Left, order, null);
                n = ordercopyexpr(n, n.Type, order, 1L);
            else if (n.Op == OEQ || n.Op == ONE) 
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                t = n.Left.Type;
                if (t.IsStruct() || t.IsArray())
                { 
                    // for complex comparisons, we need both args to be
                    // addressable so we can pass them to the runtime.
                    n.Left = orderaddrtemp(n.Left, order);
                    n.Right = orderaddrtemp(n.Right, order);
                }
            else 
                n.Left = orderexpr(n.Left, order, null);
                n.Right = orderexpr(n.Right, order, null);
                orderexprlist(n.List, order);
                orderexprlist(n.Rlist, order); 

                // Addition of strings turns into a function call.
                // Allocate a temporary to hold the strings.
                // Fewer than 5 strings use direct runtime helpers.
                        lineno = lno;
            return n;
        }

        // okas creates and returns an assignment of val to ok,
        // including an explicit conversion if necessary.
        private static ref Node okas(ref Node ok, ref Node val)
        {
            if (!isblank(ok))
            {
                val = conv(val, ok.Type);
            }
            return nod(OAS, ok, val);
        }

        // orderas2 orders OAS2XXXX nodes. It creates temporaries to ensure left-to-right assignment.
        // The caller should order the right-hand side of the assignment before calling orderas2.
        // It rewrites,
        //     a, b, a = ...
        // as
        //    tmp1, tmp2, tmp3 = ...
        //     a, b, a = tmp1, tmp2, tmp3
        // This is necessary to ensure left to right assignment order.
        private static void orderas2(ref Node n, ref Order order)
        {
            ref Node tmplist = new slice<ref Node>(new ref Node[] {  });
            ref Node left = new slice<ref Node>(new ref Node[] {  });
            {
                var l__prev1 = l;

                foreach (var (_, __l) in n.List.Slice())
                {
                    l = __l;
                    if (!isblank(l))
                    {
                        var tmp = ordertemp(l.Type, order, types.Haspointers(l.Type));
                        tmplist = append(tmplist, tmp);
                        left = append(left, l);
                    }
                }

                l = l__prev1;
            }

            order.@out = append(order.@out, n);

            var @as = nod(OAS2, null, null);
            @as.List.Set(left);
            @as.Rlist.Set(tmplist);
            as = typecheck(as, Etop);
            orderstmt(as, order);

            long ti = 0L;
            {
                var l__prev1 = l;

                foreach (var (__ni, __l) in n.List.Slice())
                {
                    ni = __ni;
                    l = __l;
                    if (!isblank(l))
                    {
                        n.List.SetIndex(ni, tmplist[ti]);
                        ti++;
                    }
                }

                l = l__prev1;
            }

        }

        // orderokas2 orders OAS2 with ok.
        // Just like orderas2(), this also adds temporaries to ensure left-to-right assignment.
        private static void orderokas2(ref Node n, ref Order order)
        {
            ref Node tmp1 = default;            ref Node tmp2 = default;

            if (!isblank(n.List.First()))
            {
                var typ = n.Rlist.First().Type;
                tmp1 = ordertemp(typ, order, types.Haspointers(typ));
            }
            if (!isblank(n.List.Second()))
            {
                tmp2 = ordertemp(types.Types[TBOOL], order, false);
            }
            order.@out = append(order.@out, n);

            if (tmp1 != null)
            {
                var r = nod(OAS, n.List.First(), tmp1);
                r = typecheck(r, Etop);
                ordermapassign(r, order);
                n.List.SetFirst(tmp1);
            }
            if (tmp2 != null)
            {
                r = okas(n.List.Second(), tmp2);
                r = typecheck(r, Etop);
                ordermapassign(r, order);
                n.List.SetSecond(tmp2);
            }
        }
    }
}}}}
