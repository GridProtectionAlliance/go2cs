// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:11 UTC
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
        // Rewrite m[k] op= r into m[k] = m[k] op r if op is / or %.
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
            public slice<ptr<Node>> @out; // list of generated statements
            public slice<ptr<Node>> temp; // stack of temporary variables
            public map<@string, slice<ptr<Node>>> free; // free list of unused temporaries, by type.LongString().
        }

        // Order rewrites fn.Nbody to apply the ordering constraints
        // described in the comment at the top of the file.
        private static void order(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (Debug['W'] > 1L)
            {
                var s = fmt.Sprintf("\nbefore order %v", fn.Func.Nname.Sym);
                dumplist(s, fn.Nbody);
            }

            orderBlock(_addr_fn.Nbody, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<ptr<Node>>>{});

        }

        // newTemp allocates a new temporary with the given type,
        // pushes it onto the temp stack, and returns it.
        // If clear is true, newTemp emits code to zero the temporary.
        private static ptr<Node> newTemp(this ptr<Order> _addr_o, ptr<types.Type> _addr_t, bool clear)
        {
            ref Order o = ref _addr_o.val;
            ref types.Type t = ref _addr_t.val;

            ptr<Node> v; 
            // Note: LongString is close to the type equality we want,
            // but not exactly. We still need to double-check with types.Identical.
            var key = t.LongString();
            var a = o.free[key];
            foreach (var (i, n) in a)
            {
                if (types.Identical(t, n.Type))
                {
                    v = a[i];
                    a[i] = a[len(a) - 1L];
                    a = a[..len(a) - 1L];
                    o.free[key] = a;
                    break;
                }

            }
            if (v == null)
            {
                v = temp(t);
            }

            if (clear)
            {
                a = nod(OAS, v, null);
                a = typecheck(a, ctxStmt);
                o.@out = append(o.@out, a);
            }

            o.temp = append(o.temp, v);
            return _addr_v!;

        }

        // copyExpr behaves like newTemp but also emits
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
        private static ptr<Node> copyExpr(this ptr<Order> _addr_o, ptr<Node> _addr_n, ptr<types.Type> _addr_t, bool clear)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            var v = o.newTemp(t, clear);
            var a = nod(OAS, v, n);
            a = typecheck(a, ctxStmt);
            o.@out = append(o.@out, a);
            return _addr_v!;
        }

        // cheapExpr returns a cheap version of n.
        // The definition of cheap is that n is a variable or constant.
        // If not, cheapExpr allocates a new tmp, emits tmp = n,
        // and then returns tmp.
        private static ptr<Node> cheapExpr(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_null!;
            }


            if (n.Op == ONAME || n.Op == OLITERAL) 
                return _addr_n!;
            else if (n.Op == OLEN || n.Op == OCAP) 
                var l = o.cheapExpr(n.Left);
                if (l == n.Left)
                {
                    return _addr_n!;
                }

                var a = n.sepcopy();
                a.Left = l;
                return _addr_typecheck(a, ctxExpr)!;
                        return _addr_o.copyExpr(n, n.Type, false)!;

        }

        // safeExpr returns a safe version of n.
        // The definition of safe is that n can appear multiple times
        // without violating the semantics of the original program,
        // and that assigning to the safe version has the same effect
        // as assigning to the original n.
        //
        // The intended use is to apply to x when rewriting x += y into x = x + y.
        private static ptr<Node> safeExpr(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;


            if (n.Op == ONAME || n.Op == OLITERAL) 
                return _addr_n!;
            else if (n.Op == ODOT || n.Op == OLEN || n.Op == OCAP) 
                var l = o.safeExpr(n.Left);
                if (l == n.Left)
                {
                    return _addr_n!;
                }

                var a = n.sepcopy();
                a.Left = l;
                return _addr_typecheck(a, ctxExpr)!;
            else if (n.Op == ODOTPTR || n.Op == ODEREF) 
                l = o.cheapExpr(n.Left);
                if (l == n.Left)
                {
                    return _addr_n!;
                }

                a = n.sepcopy();
                a.Left = l;
                return _addr_typecheck(a, ctxExpr)!;
            else if (n.Op == OINDEX || n.Op == OINDEXMAP) 
                l = ;
                if (n.Left.Type.IsArray())
                {
                    l = o.safeExpr(n.Left);
                }
                else
                {
                    l = o.cheapExpr(n.Left);
                }

                var r = o.cheapExpr(n.Right);
                if (l == n.Left && r == n.Right)
                {
                    return _addr_n!;
                }

                a = n.sepcopy();
                a.Left = l;
                a.Right = r;
                return _addr_typecheck(a, ctxExpr)!;
            else 
                Fatalf("order.safeExpr %v", n.Op);
                return _addr_null!; // not reached
                    }

        // isaddrokay reports whether it is okay to pass n's address to runtime routines.
        // Taking the address of a variable makes the liveness and optimization analyses
        // lose track of where the variable's lifetime ends. To avoid hurting the analyses
        // of ordinary stack variables, those are not 'isaddrokay'. Temporaries are okay,
        // because we emit explicit VARKILL instructions marking the end of those
        // temporaries' lifetimes.
        private static bool isaddrokay(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return islvalue(n) && (n.Op != ONAME || n.Class() == PEXTERN || n.IsAutoTmp());
        }

        // addrTemp ensures that n is okay to pass by address to runtime routines.
        // If the original argument n is not okay, addrTemp creates a tmp, emits
        // tmp = n, and then returns tmp.
        // The result of addrTemp MUST be assigned back to n, e.g.
        //     n.Left = o.addrTemp(n.Left)
        private static ptr<Node> addrTemp(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (consttype(n) != CTxxx)
            { 
                // TODO: expand this to all static composite literal nodes?
                n = defaultlit(n, null);
                dowidth(n.Type);
                var vstat = staticname(n.Type);
                vstat.MarkReadonly();
                InitSchedule s = default;
                s.staticassign(vstat, n);
                if (s.@out != null)
                {
                    Fatalf("staticassign of const generated code: %+v", n);
                }

                vstat = typecheck(vstat, ctxExpr);
                return _addr_vstat!;

            }

            if (isaddrokay(_addr_n))
            {
                return _addr_n!;
            }

            return _addr_o.copyExpr(n, n.Type, false)!;

        }

        // mapKeyTemp prepares n to be a key in a map runtime call and returns n.
        // It should only be used for map runtime calls which have *_fast* versions.
        private static ptr<Node> mapKeyTemp(this ptr<Order> _addr_o, ptr<types.Type> _addr_t, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref types.Type t = ref _addr_t.val;
            ref Node n = ref _addr_n.val;
 
            // Most map calls need to take the address of the key.
            // Exception: map*_fast* calls. See golang.org/issue/19015.
            if (mapfast(t) == mapslow)
            {
                return _addr_o.addrTemp(n)!;
            }

            return _addr_n!;

        }

        // mapKeyReplaceStrConv replaces OBYTES2STR by OBYTES2STRTMP
        // in n to avoid string allocations for keys in map lookups.
        // Returns a bool that signals if a modification was made.
        //
        // For:
        //  x = m[string(k)]
        //  x = m[T1{... Tn{..., string(k), ...}]
        // where k is []byte, T1 to Tn is a nesting of struct and array literals,
        // the allocation of backing bytes for the string can be avoided
        // by reusing the []byte backing array. These are special cases
        // for avoiding allocations when converting byte slices to strings.
        // It would be nice to handle these generally, but because
        // []byte keys are not allowed in maps, the use of string(k)
        // comes up in important cases in practice. See issue 3512.
        private static bool mapKeyReplaceStrConv(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            bool replaced = default;

            if (n.Op == OBYTES2STR) 
                n.Op = OBYTES2STRTMP;
                replaced = true;
            else if (n.Op == OSTRUCTLIT) 
                {
                    var elem__prev1 = elem;

                    foreach (var (_, __elem) in n.List.Slice())
                    {
                        elem = __elem;
                        if (mapKeyReplaceStrConv(_addr_elem.Left))
                        {
                            replaced = true;
                        }

                    }

                    elem = elem__prev1;
                }
            else if (n.Op == OARRAYLIT) 
                {
                    var elem__prev1 = elem;

                    foreach (var (_, __elem) in n.List.Slice())
                    {
                        elem = __elem;
                        if (elem.Op == OKEY)
                        {
                            elem = elem.Right;
                        }

                        if (mapKeyReplaceStrConv(_addr_elem))
                        {
                            replaced = true;
                        }

                    }

                    elem = elem__prev1;
                }
                        return replaced;

        }

        private partial struct ordermarker // : long
        {
        }

        // markTemp returns the top of the temporary variable stack.
        private static ordermarker markTemp(this ptr<Order> _addr_o)
        {
            ref Order o = ref _addr_o.val;

            return ordermarker(len(o.temp));
        }

        // popTemp pops temporaries off the stack until reaching the mark,
        // which must have been returned by markTemp.
        private static void popTemp(this ptr<Order> _addr_o, ordermarker mark)
        {
            ref Order o = ref _addr_o.val;

            foreach (var (_, n) in o.temp[mark..])
            {
                var key = n.Type.LongString();
                o.free[key] = append(o.free[key], n);
            }
            o.temp = o.temp[..mark];

        }

        // cleanTempNoPop emits VARKILL and if needed VARLIVE instructions
        // to *out for each temporary above the mark on the temporary stack.
        // It does not pop the temporaries from the stack.
        private static slice<ptr<Node>> cleanTempNoPop(this ptr<Order> _addr_o, ordermarker mark)
        {
            ref Order o = ref _addr_o.val;

            slice<ptr<Node>> @out = default;
            for (var i = len(o.temp) - 1L; i >= int(mark); i--)
            {
                var n = o.temp[i];
                if (n.Name.Keepalive())
                {
                    n.Name.SetKeepalive(false);
                    n.Name.SetAddrtaken(true); // ensure SSA keeps the n variable
                    var live = nod(OVARLIVE, n, null);
                    live = typecheck(live, ctxStmt);
                    out = append(out, live);

                }

                var kill = nod(OVARKILL, n, null);
                kill = typecheck(kill, ctxStmt);
                out = append(out, kill);

            }

            return out;

        }

        // cleanTemp emits VARKILL instructions for each temporary above the
        // mark on the temporary stack and removes them from the stack.
        private static void cleanTemp(this ptr<Order> _addr_o, ordermarker top)
        {
            ref Order o = ref _addr_o.val;

            o.@out = append(o.@out, o.cleanTempNoPop(top));
            o.popTemp(top);
        }

        // stmtList orders each of the statements in the list.
        private static void stmtList(this ptr<Order> _addr_o, Nodes l)
        {
            ref Order o = ref _addr_o.val;

            var s = l.Slice();
            foreach (var (i) in s)
            {
                orderMakeSliceCopy(s[i..]);
                o.stmt(s[i]);
            }

        }

        // orderMakeSliceCopy matches the pattern:
        //  m = OMAKESLICE([]T, x); OCOPY(m, s)
        // and rewrites it to:
        //  m = OMAKESLICECOPY([]T, x, s); nil
        private static void orderMakeSliceCopy(slice<ptr<Node>> s)
        {
            const var go115makeslicecopy = true;

            if (!go115makeslicecopy)
            {
                return ;
            }

            if (Debug['N'] != 0L || instrumenting)
            {
                return ;
            }

            if (len(s) < 2L)
            {
                return ;
            }

            var asn = s[0L];
            var copyn = s[1L];

            if (asn == null || asn.Op != OAS)
            {
                return ;
            }

            if (asn.Left.Op != ONAME)
            {
                return ;
            }

            if (asn.Left.isBlank())
            {
                return ;
            }

            var maken = asn.Right;
            if (maken == null || maken.Op != OMAKESLICE)
            {
                return ;
            }

            if (maken.Esc == EscNone)
            {
                return ;
            }

            if (maken.Left == null || maken.Right != null)
            {
                return ;
            }

            if (copyn.Op != OCOPY)
            {
                return ;
            }

            if (copyn.Left.Op != ONAME)
            {
                return ;
            }

            if (asn.Left.Sym != copyn.Left.Sym)
            {
                return ;
            }

            if (copyn.Right.Op != ONAME)
            {
                return ;
            }

            if (copyn.Left.Sym == copyn.Right.Sym)
            {
                return ;
            }

            maken.Op = OMAKESLICECOPY;
            maken.Right = copyn.Right; 
            // Set bounded when m = OMAKESLICE([]T, len(s)); OCOPY(m, s)
            maken.SetBounded(maken.Left.Op == OLEN && samesafeexpr(maken.Left.Left, copyn.Right));

            maken = typecheck(maken, ctxExpr);

            s[1L] = null; // remove separate copy call

            return ;

        }

        // edge inserts coverage instrumentation for libfuzzer.
        private static void edge(this ptr<Order> _addr_o)
        {
            ref Order o = ref _addr_o.val;

            if (Debug_libfuzzer == 0L)
            {
                return ;
            } 

            // Create a new uint8 counter to be allocated in section
            // __libfuzzer_extra_counters.
            var counter = staticname(types.Types[TUINT8]);
            counter.Name.SetLibfuzzerExtraCounter(true); 

            // counter += 1
            var incr = nod(OASOP, counter, nodintconst(1L));
            incr.SetSubOp(OADD);
            incr = typecheck(incr, ctxStmt);

            o.@out = append(o.@out, incr);

        }

        // orderBlock orders the block of statements in n into a new slice,
        // and then replaces the old slice in n with the new slice.
        // free is a map that can be used to obtain temporary variables by type.
        private static void orderBlock(ptr<Nodes> _addr_n, map<@string, slice<ptr<Node>>> free)
        {
            ref Nodes n = ref _addr_n.val;

            Order order = default;
            order.free = free;
            var mark = order.markTemp();
            order.edge();
            order.stmtList(n);
            order.cleanTemp(mark);
            n.Set(order.@out);
        }

        // exprInPlace orders the side effects in *np and
        // leaves them as the init list of the final *np.
        // The result of exprInPlace MUST be assigned back to n, e.g.
        //     n.Left = o.exprInPlace(n.Left)
        private static ptr<Node> exprInPlace(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            Order order = default;
            order.free = o.free;
            n = order.expr(n, null);
            n = addinit(n, order.@out); 

            // insert new temporaries from order
            // at head of outer list.
            o.temp = append(o.temp, order.temp);
            return _addr_n!;

        }

        // orderStmtInPlace orders the side effects of the single statement *np
        // and replaces it with the resulting statement list.
        // The result of orderStmtInPlace MUST be assigned back to n, e.g.
        //     n.Left = orderStmtInPlace(n.Left)
        // free is a map that can be used to obtain temporary variables by type.
        private static ptr<Node> orderStmtInPlace(ptr<Node> _addr_n, map<@string, slice<ptr<Node>>> free)
        {
            ref Node n = ref _addr_n.val;

            Order order = default;
            order.free = free;
            var mark = order.markTemp();
            order.stmt(n);
            order.cleanTemp(mark);
            return _addr_liststmt(order.@out)!;
        }

        // init moves n's init list to o.out.
        private static void init(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (n.mayBeShared())
            { 
                // For concurrency safety, don't mutate potentially shared nodes.
                // First, ensure that no work is required here.
                if (n.Ninit.Len() > 0L)
                {
                    Fatalf("order.init shared node with ninit");
                }

                return ;

            }

            o.stmtList(n.Ninit);
            n.Ninit.Set(null);

        }

        // call orders the call expression n.
        // n.Op is OCALLMETH/OCALLFUNC/OCALLINTER or a builtin like OCOPY.
        private static void call(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (n.Ninit.Len() > 0L)
            { 
                // Caller should have already called o.init(n).
                Fatalf("%v with unexpected ninit", n.Op);

            } 

            // Builtin functions.
            if (n.Op != OCALLFUNC && n.Op != OCALLMETH && n.Op != OCALLINTER)
            {
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                o.exprList(n.List);
                return ;
            }

            fixVariadicCall(n);
            n.Left = o.expr(n.Left, null);
            o.exprList(n.List);

            if (n.Op == OCALLINTER)
            {
                return ;
            }

            Action<ptr<Node>> keepAlive = arg =>
            { 
                // If the argument is really a pointer being converted to uintptr,
                // arrange for the pointer to be kept alive until the call returns,
                // by copying it into a temp and marking that temp
                // still alive when we pop the temp stack.
                if (arg.Op == OCONVNOP && arg.Left.Type.IsUnsafePtr())
                {
                    var x = o.copyExpr(arg.Left, arg.Left.Type, false);
                    x.Name.SetKeepalive(true);
                    arg.Left = x;
                }

            } 

            // Check for "unsafe-uintptr" tag provided by escape analysis.
; 

            // Check for "unsafe-uintptr" tag provided by escape analysis.
            foreach (var (i, param) in n.Left.Type.Params().FieldSlice())
            {
                if (param.Note == unsafeUintptrTag || param.Note == uintptrEscapesTag)
                {
                    {
                        var arg = n.List.Index(i);

                        if (arg.Op == OSLICELIT)
                        {
                            foreach (var (_, elt) in arg.List.Slice())
                            {
                                keepAlive(elt);
                            }
                        else
                        }                        {
                            keepAlive(arg);
                        }

                    }

                }

            }

        }

        // mapAssign appends n to o.out, introducing temporaries
        // to make sure that all map assignments have the form m[k] = x.
        // (Note: expr has already been called on n, so we know k is addressable.)
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
        private static void mapAssign(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;


            if (n.Op == OAS || n.Op == OASOP) 
                if (n.Left.Op == OINDEXMAP)
                { 
                    // Make sure we evaluate the RHS before starting the map insert.
                    // We need to make sure the RHS won't panic.  See issue 22881.
                    if (n.Right.Op == OAPPEND)
                    {
                        var s = n.Right.List.Slice()[1L..];
                        {
                            var i__prev1 = i;

                            foreach (var (__i, __n) in s)
                            {
                                i = __i;
                                n = __n;
                                s[i] = o.cheapExpr(n);
                            }
                    else

                            i = i__prev1;
                        }
                    }                    {
                        n.Right = o.cheapExpr(n.Right);
                    }

                }

                o.@out = append(o.@out, n);
            else if (n.Op == OAS2 || n.Op == OAS2DOTTYPE || n.Op == OAS2MAPR || n.Op == OAS2FUNC) 
                slice<ptr<Node>> post = default;
                {
                    var i__prev1 = i;

                    foreach (var (__i, __m) in n.List.Slice())
                    {
                        i = __i;
                        m = __m;

                        if (m.Op == OINDEXMAP)
                        {
                            if (!m.Left.IsAutoTmp())
                            {
                                m.Left = o.copyExpr(m.Left, m.Left.Type, false);
                            }

                            if (!m.Right.IsAutoTmp())
                            {
                                m.Right = o.copyExpr(m.Right, m.Right.Type, false);
                            }

                            fallthrough = true;
                        }
                        if (fallthrough || instrumenting && n.Op == OAS2FUNC && !m.isBlank())
                        {
                            var t = o.newTemp(m.Type, false);
                            n.List.SetIndex(i, t);
                            var a = nod(OAS, m, t);
                            a = typecheck(a, ctxStmt);
                            post = append(post, a);
                            goto __switch_break0;
                        }

                        __switch_break0:;

                    }

                    i = i__prev1;
                }

                o.@out = append(o.@out, n);
                o.@out = append(o.@out, post);
            else 
                Fatalf("order.mapAssign %v", n.Op);
            
        }

        // stmt orders the statement n, appending to o.out.
        // Temporaries created during the statement are cleaned
        // up using VARKILL instructions as possible.
        private static void stmt(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            var lno = setlineno(n);
            o.init(n);


            if (n.Op == OVARKILL || n.Op == OVARLIVE || n.Op == OINLMARK) 
                o.@out = append(o.@out, n);
            else if (n.Op == OAS) 
                var t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, n.Left);
                o.mapAssign(n);
                o.cleanTemp(t);
            else if (n.Op == OASOP) 
                t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);

                if (instrumenting || n.Left.Op == OINDEXMAP && (n.SubOp() == ODIV || n.SubOp() == OMOD))
                { 
                    // Rewrite m[k] op= r into m[k] = m[k] op r so
                    // that we can ensure that if op panics
                    // because r is zero, the panic happens before
                    // the map assignment.

                    n.Left = o.safeExpr(n.Left);

                    var l = treecopy(n.Left, src.NoXPos);
                    if (l.Op == OINDEXMAP)
                    {
                        l.SetIndexMapLValue(false);
                    }

                    l = o.copyExpr(l, n.Left.Type, false);
                    n.Right = nod(n.SubOp(), l, n.Right);
                    n.Right = typecheck(n.Right, ctxExpr);
                    n.Right = o.expr(n.Right, null);

                    n.Op = OAS;
                    n.ResetAux();

                }

                o.mapAssign(n);
                o.cleanTemp(t);
            else if (n.Op == OAS2) 
                t = o.markTemp();
                o.exprList(n.List);
                o.exprList(n.Rlist);
                o.mapAssign(n);
                o.cleanTemp(t); 

                // Special: avoid copy of func call n.Right
            else if (n.Op == OAS2FUNC) 
                t = o.markTemp();
                o.exprList(n.List);
                o.init(n.Right);
                o.call(n.Right);
                o.as2(n);
                o.cleanTemp(t); 

                // Special: use temporary variables to hold result,
                // so that runtime can take address of temporary.
                // No temporary for blank assignment.
                //
                // OAS2MAPR: make sure key is addressable if needed,
                //           and make sure OINDEXMAP is not copied out.
            else if (n.Op == OAS2DOTTYPE || n.Op == OAS2RECV || n.Op == OAS2MAPR) 
                t = o.markTemp();
                o.exprList(n.List);

                {
                    var r__prev2 = r;

                    var r = n.Right;


                    if (r.Op == ODOTTYPE2 || r.Op == ORECV) 
                        r.Left = o.expr(r.Left, null);
                    else if (r.Op == OINDEXMAP) 
                        r.Left = o.expr(r.Left, null);
                        r.Right = o.expr(r.Right, null); 
                        // See similar conversion for OINDEXMAP below.
                        _ = mapKeyReplaceStrConv(_addr_r.Right);
                        r.Right = o.mapKeyTemp(r.Left.Type, r.Right);
                    else 
                        Fatalf("order.stmt: %v", r.Op);


                    r = r__prev2;
                }

                o.okAs2(n);
                o.cleanTemp(t); 

                // Special: does not save n onto out.
            else if (n.Op == OBLOCK || n.Op == OEMPTY) 
                o.stmtList(n.List); 

                // Special: n->left is not an expression; save as is.
            else if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == ODCL || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OFALL || n.Op == OGOTO || n.Op == OLABEL || n.Op == ORETJMP) 
                o.@out = append(o.@out, n); 

                // Special: handle call arguments.
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH) 
                t = o.markTemp();
                o.call(n);
                o.@out = append(o.@out, n);
                o.cleanTemp(t);
            else if (n.Op == OCLOSE || n.Op == OCOPY || n.Op == OPRINT || n.Op == OPRINTN || n.Op == ORECOVER || n.Op == ORECV) 
                t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                o.exprList(n.List);
                o.exprList(n.Rlist);
                o.@out = append(o.@out, n);
                o.cleanTemp(t); 

                // Special: order arguments to inner call but not call itself.
            else if (n.Op == ODEFER || n.Op == OGO) 
                t = o.markTemp();
                o.init(n.Left);
                o.call(n.Left);
                o.@out = append(o.@out, n);
                o.cleanTemp(t);
            else if (n.Op == ODELETE) 
                t = o.markTemp();
                n.List.SetFirst(o.expr(n.List.First(), null));
                n.List.SetSecond(o.expr(n.List.Second(), null));
                n.List.SetSecond(o.mapKeyTemp(n.List.First().Type, n.List.Second()));
                o.@out = append(o.@out, n);
                o.cleanTemp(t); 

                // Clean temporaries from condition evaluation at
                // beginning of loop body and after for statement.
            else if (n.Op == OFOR) 
                t = o.markTemp();
                n.Left = o.exprInPlace(n.Left);
                n.Nbody.Prepend(o.cleanTempNoPop(t));
                orderBlock(_addr_n.Nbody, o.free);
                n.Right = orderStmtInPlace(_addr_n.Right, o.free);
                o.@out = append(o.@out, n);
                o.cleanTemp(t); 

                // Clean temporaries from condition at
                // beginning of both branches.
            else if (n.Op == OIF) 
                t = o.markTemp();
                n.Left = o.exprInPlace(n.Left);
                n.Nbody.Prepend(o.cleanTempNoPop(t));
                n.Rlist.Prepend(o.cleanTempNoPop(t));
                o.popTemp(t);
                orderBlock(_addr_n.Nbody, o.free);
                orderBlock(_addr_n.Rlist, o.free);
                o.@out = append(o.@out, n); 

                // Special: argument will be converted to interface using convT2E
                // so make sure it is an addressable temporary.
            else if (n.Op == OPANIC) 
                t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                if (!n.Left.Type.IsInterface())
                {
                    n.Left = o.addrTemp(n.Left);
                }

                o.@out = append(o.@out, n);
                o.cleanTemp(t);
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
                if (n.Right.Op == OSTR2BYTES)
                {
                    n.Right.Op = OSTR2BYTESTMP;
                }

                t = o.markTemp();
                n.Right = o.expr(n.Right, null);

                var orderBody = true;

                if (n.Type.Etype == TARRAY || n.Type.Etype == TSLICE)
                {
                    if (n.List.Len() < 2L || n.List.Second().isBlank())
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
                        r = typecheck(r, ctxExpr);
                    }

                    n.Right = o.copyExpr(r, r.Type, false);
                    goto __switch_break1;
                }
                if (n.Type.Etype == TMAP)
                {
                    if (isMapClear(n))
                    { 
                        // Preserve the body of the map clear pattern so it can
                        // be detected during walk. The loop body will not be used
                        // when optimizing away the range loop to a runtime call.
                        orderBody = false;
                        break;

                    } 

                    // copy the map value in case it is a map literal.
                    // TODO(rsc): Make tmp = literal expressions reuse tmp.
                    // For maps tmp is just one word so it hardly matters.
                    r = n.Right;
                    n.Right = o.copyExpr(r, r.Type, false); 

                    // prealloc[n] is the temp for the iterator.
                    // hiter contains pointers and needs to be zeroed.
                    prealloc[n] = o.newTemp(hiter(n.Type), true);
                    goto __switch_break1;
                }
                // default: 
                    Fatalf("order.stmt range %v", n.Type);

                __switch_break1:;
                o.exprListInPlace(n.List);
                if (orderBody)
                {
                    orderBlock(_addr_n.Nbody, o.free);
                }

                o.@out = append(o.@out, n);
                o.cleanTemp(t);
            else if (n.Op == ORETURN) 
                o.exprList(n.List);
                o.@out = append(o.@out, n); 

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
                t = o.markTemp();

                foreach (var (_, n2) in n.List.Slice())
                {
                    if (n2.Op != OCASE)
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

                    if (r == null)
                    {
                        continue;
                    }


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
                        r.Right.Left = o.expr(r.Right.Left, null);

                        if (r.Right.Left.Op != ONAME)
                        {
                            r.Right.Left = o.copyExpr(r.Right.Left, r.Right.Left.Type, false);
                        } 

                        // Introduce temporary for receive and move actual copy into case body.
                        // avoids problems with target being addressed, as usual.
                        // NOTE: If we wanted to be clever, we could arrange for just one
                        // temporary per distinct type, sharing the temp among all receives
                        // with that temp. Similarly one ok bool could be shared among all
                        // the x,ok receives. Not worth doing until there's a clear need.
                        if (r.Left != null && r.Left.isBlank())
                        {
                            r.Left = null;
                        }

                        if (r.Left != null)
                        { 
                            // use channel element type for temporary to avoid conversions,
                            // such as in case interfacevalue = <-intchan.
                            // the conversion happens in the OAS instead.
                            var tmp1 = r.Left;

                            if (r.Colas())
                            {
                                var tmp2 = nod(ODCL, tmp1, null);
                                tmp2 = typecheck(tmp2, ctxStmt);
                                n2.Ninit.Append(tmp2);
                            }

                            r.Left = o.newTemp(r.Right.Left.Type.Elem(), types.Haspointers(r.Right.Left.Type.Elem()));
                            tmp2 = nod(OAS, tmp1, r.Left);
                            tmp2 = typecheck(tmp2, ctxStmt);
                            n2.Ninit.Append(tmp2);

                        }

                        if (r.List.Len() != 0L && r.List.First().isBlank())
                        {
                            r.List.Set(null);
                        }

                        if (r.List.Len() != 0L)
                        {
                            tmp1 = r.List.First();
                            if (r.Colas())
                            {
                                tmp2 = nod(ODCL, tmp1, null);
                                tmp2 = typecheck(tmp2, ctxStmt);
                                n2.Ninit.Append(tmp2);
                            }

                            r.List.Set1(o.newTemp(types.Types[TBOOL], false));
                            tmp2 = okas(_addr_tmp1, _addr_r.List.First());
                            tmp2 = typecheck(tmp2, ctxStmt);
                            n2.Ninit.Append(tmp2);

                        }

                        orderBlock(_addr_n2.Ninit, o.free);
                    else if (r.Op == OSEND) 
                        if (r.Ninit.Len() != 0L)
                        {
                            dumplist("ninit", r.Ninit);
                            Fatalf("ninit on select send");
                        } 

                        // case c <- x
                        // r->left is c, r->right is x, both are always evaluated.
                        r.Left = o.expr(r.Left, null);

                        if (!r.Left.IsAutoTmp())
                        {
                            r.Left = o.copyExpr(r.Left, r.Left.Type, false);
                        }

                        r.Right = o.expr(r.Right, null);
                        if (!r.Right.IsAutoTmp())
                        {
                            r.Right = o.copyExpr(r.Right, r.Right.Type, false);
                        }

                    else 
                        Dump("select case", r);
                        Fatalf("unknown op in select %v", r.Op); 

                        // If this is case x := <-ch or case x, y := <-ch, the case has
                        // the ODCL nodes to declare x and y. We want to delay that
                        // declaration (and possible allocation) until inside the case body.
                        // Delete the ODCL nodes here and recreate them inside the body below.
                                    } 
                // Now that we have accumulated all the temporaries, clean them.
                // Also insert any ninit queued during the previous loop.
                // (The temporary cleaning must follow that ninit work.)
                foreach (var (_, n3) in n.List.Slice())
                {
                    orderBlock(_addr_n3.Nbody, o.free);
                    n3.Nbody.Prepend(o.cleanTempNoPop(t)); 

                    // TODO(mdempsky): Is this actually necessary?
                    // walkselect appears to walk Ninit.
                    n3.Nbody.Prepend(n3.Ninit.Slice());
                    n3.Ninit.Set(null);

                }
                o.@out = append(o.@out, n);
                o.popTemp(t); 

                // Special: value being sent is passed as a pointer; make it addressable.
            else if (n.Op == OSEND) 
                t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                if (instrumenting)
                { 
                    // Force copying to the stack so that (chan T)(nil) <- x
                    // is still instrumented as a read of x.
                    n.Right = o.copyExpr(n.Right, n.Right.Type, false);

                }
                else
                {
                    n.Right = o.addrTemp(n.Right);
                }

                o.@out = append(o.@out, n);
                o.cleanTemp(t); 

                // TODO(rsc): Clean temporaries more aggressively.
                // Note that because walkswitch will rewrite some of the
                // switch into a binary search, this is not as easy as it looks.
                // (If we ran that code here we could invoke order.stmt on
                // the if-else chain instead.)
                // For now just clean all the temporaries at the end.
                // In practice that's fine.
            else if (n.Op == OSWITCH) 
                if (Debug_libfuzzer != 0L && !hasDefaultCase(_addr_n))
                { 
                    // Add empty "default:" case for instrumentation.
                    n.List.Append(nod(OCASE, null, null));

                }

                t = o.markTemp();
                n.Left = o.expr(n.Left, null);
                foreach (var (_, ncas) in n.List.Slice())
                {
                    if (ncas.Op != OCASE)
                    {
                        Fatalf("order switch case %v", ncas.Op);
                    }

                    o.exprListInPlace(ncas.List);
                    orderBlock(_addr_ncas.Nbody, o.free);

                }
                o.@out = append(o.@out, n);
                o.cleanTemp(t);
            else 
                Fatalf("order.stmt %v", n.Op);
                        lineno = lno;

        }

        private static bool hasDefaultCase(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            foreach (var (_, ncas) in n.List.Slice())
            {
                if (ncas.Op != OCASE)
                {
                    Fatalf("expected case, found %v", ncas.Op);
                }

                if (ncas.List.Len() == 0L)
                {
                    return true;
                }

            }
            return false;

        }

        // exprList orders the expression list l into o.
        private static void exprList(this ptr<Order> _addr_o, Nodes l)
        {
            ref Order o = ref _addr_o.val;

            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = o.expr(s[i], null);
            }

        }

        // exprListInPlace orders the expression list l but saves
        // the side effects on the individual expression ninit lists.
        private static void exprListInPlace(this ptr<Order> _addr_o, Nodes l)
        {
            ref Order o = ref _addr_o.val;

            var s = l.Slice();
            foreach (var (i) in s)
            {
                s[i] = o.exprInPlace(s[i]);
            }

        }

        // prealloc[x] records the allocation to use for x.
        private static map prealloc = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<Node>>{};

        // expr orders a single expression, appending side
        // effects to o.out as needed.
        // If this is part of an assignment lhs = *np, lhs is given.
        // Otherwise lhs == nil. (When lhs != nil it may be possible
        // to avoid copying the result of the expression to a temporary.)
        // The result of expr MUST be assigned back to n, e.g.
        //     n.Left = o.expr(n.Left, lhs)
        private static ptr<Node> expr(this ptr<Order> _addr_o, ptr<Node> _addr_n, ptr<Node> _addr_lhs)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;
            ref Node lhs = ref _addr_lhs.val;

            if (n == null)
            {
                return _addr_n!;
            }

            var lno = setlineno(n);
            o.init(n);


            if (n.Op == OADDSTR) 
                o.exprList(n.List);

                if (n.List.Len() > 5L)
                {
                    var t = types.NewArray(types.Types[TSTRING], int64(n.List.Len()));
                    prealloc[n] = o.newTemp(t, false);
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
                    hasbyte = hasbyte || n1.Op == OBYTES2STR;
                    haslit = haslit || n1.Op == OLITERAL && len(strlit(n1)) != 0L;
                }
                if (haslit && hasbyte)
                {
                    foreach (var (_, n2) in n.List.Slice())
                    {
                        if (n2.Op == OBYTES2STR)
                        {
                            n2.Op = OBYTES2STRTMP;
                        }

                    }

                }

            else if (n.Op == OINDEXMAP) 
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                var needCopy = false;

                if (!n.IndexMapLValue())
                { 
                    // Enforce that any []byte slices we are not copying
                    // can not be changed before the map index by forcing
                    // the map index to happen immediately following the
                    // conversions. See copyExpr a few lines below.
                    needCopy = mapKeyReplaceStrConv(_addr_n.Right);

                    if (instrumenting)
                    { 
                        // Race detector needs the copy so it can
                        // call treecopy on the result.
                        needCopy = true;

                    }

                } 

                // key must be addressable
                n.Right = o.mapKeyTemp(n.Left.Type, n.Right);
                if (needCopy)
                {
                    n = o.copyExpr(n, n.Type, false);
                } 

                // concrete type (not interface) argument might need an addressable
                // temporary to pass to the runtime conversion routine.
            else if (n.Op == OCONVIFACE) 
                n.Left = o.expr(n.Left, null);
                if (n.Left.Type.IsInterface())
                {
                    break;
                }

                {
                    var (_, needsaddr) = convFuncName(n.Left.Type, n.Type);

                    if (needsaddr || isStaticCompositeLiteral(n.Left))
                    { 
                        // Need a temp if we need to pass the address to the conversion function.
                        // We also process static composite literal node here, making a named static global
                        // whose address we can put directly in an interface (see OCONVIFACE case in walk).
                        n.Left = o.addrTemp(n.Left);

                    }

                }


            else if (n.Op == OCONVNOP) 
                if (n.Type.IsKind(TUNSAFEPTR) && n.Left.Type.IsKind(TUINTPTR) && (n.Left.Op == OCALLFUNC || n.Left.Op == OCALLINTER || n.Left.Op == OCALLMETH))
                { 
                    // When reordering unsafe.Pointer(f()) into a separate
                    // statement, the conversion and function call must stay
                    // together. See golang.org/issue/15329.
                    o.init(n.Left);
                    o.call(n.Left);
                    if (lhs == null || lhs.Op != ONAME || instrumenting)
                    {
                        n = o.copyExpr(n, n.Type, false);
                    }

                }
                else
                {
                    n.Left = o.expr(n.Left, null);
                }

            else if (n.Op == OANDAND || n.Op == OOROR) 
                // ... = LHS && RHS
                //
                // var r bool
                // r = LHS
                // if r {       // or !r, for OROR
                //     r = RHS
                // }
                // ... = r

                var r = o.newTemp(n.Type, false); 

                // Evaluate left-hand side.
                var lhs = o.expr(n.Left, null);
                o.@out = append(o.@out, typecheck(nod(OAS, r, lhs), ctxStmt)); 

                // Evaluate right-hand side, save generated code.
                var saveout = o.@out;
                o.@out = null;
                t = o.markTemp();
                o.edge();
                var rhs = o.expr(n.Right, null);
                o.@out = append(o.@out, typecheck(nod(OAS, r, rhs), ctxStmt));
                o.cleanTemp(t);
                var gen = o.@out;
                o.@out = saveout; 

                // If left-hand side doesn't cause a short-circuit, issue right-hand side.
                var nif = nod(OIF, r, null);
                if (n.Op == OANDAND)
                {
                    nif.Nbody.Set(gen);
                }
                else
                {
                    nif.Rlist.Set(gen);
                }

                o.@out = append(o.@out, nif);
                n = r;
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH || n.Op == OCAP || n.Op == OCOMPLEX || n.Op == OCOPY || n.Op == OIMAG || n.Op == OLEN || n.Op == OMAKECHAN || n.Op == OMAKEMAP || n.Op == OMAKESLICE || n.Op == OMAKESLICECOPY || n.Op == ONEW || n.Op == OREAL || n.Op == ORECOVER || n.Op == OSTR2BYTES || n.Op == OSTR2BYTESTMP || n.Op == OSTR2RUNES) 

                if (isRuneCount(n))
                { 
                    // len([]rune(s)) is rewritten to runtime.countrunes(s) later.
                    n.Left.Left = o.expr(n.Left.Left, null);

                }
                else
                {
                    o.call(n);
                }

                if (lhs == null || lhs.Op != ONAME || instrumenting)
                {
                    n = o.copyExpr(n, n.Type, false);
                }

            else if (n.Op == OAPPEND) 
                // Check for append(x, make([]T, y)...) .
                if (isAppendOfMake(n))
                {
                    n.List.SetFirst(o.expr(n.List.First(), null)); // order x
                    n.List.Second().Left = o.expr(n.List.Second().Left, null); // order y
                }
                else
                {
                    o.exprList(n.List);
                }

                if (lhs == null || lhs.Op != ONAME && !samesafeexpr(lhs, n.List.First()))
                {
                    n = o.copyExpr(n, n.Type, false);
                }

            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR || n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                n.Left = o.expr(n.Left, null);
                var (low, high, max) = n.SliceBounds();
                low = o.expr(low, null);
                low = o.cheapExpr(low);
                high = o.expr(high, null);
                high = o.cheapExpr(high);
                max = o.expr(max, null);
                max = o.cheapExpr(max);
                n.SetSliceBounds(low, high, max);
                if (lhs == null || lhs.Op != ONAME && !samesafeexpr(lhs, n.Left))
                {
                    n = o.copyExpr(n, n.Type, false);
                }

            else if (n.Op == OCLOSURE) 
                if (n.Transient() && n.Func.Closure.Func.Cvars.Len() > 0L)
                {
                    prealloc[n] = o.newTemp(closureType(n), false);
                }

            else if (n.Op == OSLICELIT || n.Op == OCALLPART) 
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                o.exprList(n.List);
                o.exprList(n.Rlist);
                if (n.Transient())
                {
                    t = ;

                    if (n.Op == OSLICELIT) 
                        t = types.NewArray(n.Type.Elem(), n.Right.Int64());
                    else if (n.Op == OCALLPART) 
                        t = partialCallType(n);
                                        prealloc[n] = o.newTemp(t, false);

                }

            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                n.Left = o.expr(n.Left, null);
                if (!isdirectiface(n.Type) || instrumenting)
                {
                    n = o.copyExpr(n, n.Type, true);
                }

            else if (n.Op == ORECV) 
                n.Left = o.expr(n.Left, null);
                n = o.copyExpr(n, n.Type, true);
            else if (n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE) 
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);

                t = n.Left.Type;

                if (t.IsString()) 
                    // Mark string(byteSlice) arguments to reuse byteSlice backing
                    // buffer during conversion. String comparison does not
                    // memorize the strings for later use, so it is safe.
                    if (n.Left.Op == OBYTES2STR)
                    {
                        n.Left.Op = OBYTES2STRTMP;
                    }

                    if (n.Right.Op == OBYTES2STR)
                    {
                        n.Right.Op = OBYTES2STRTMP;
                    }

                else if (t.IsStruct() || t.IsArray()) 
                    // for complex comparisons, we need both args to be
                    // addressable so we can pass them to the runtime.
                    n.Left = o.addrTemp(n.Left);
                    n.Right = o.addrTemp(n.Right);
                            else if (n.Op == OMAPLIT) 
                // Order map by converting:
                //   map[int]int{
                //     a(): b(),
                //     c(): d(),
                //     e(): f(),
                //   }
                // to
                //   m := map[int]int{}
                //   m[a()] = b()
                //   m[c()] = d()
                //   m[e()] = f()
                // Then order the result.
                // Without this special case, order would otherwise compute all
                // the keys and values before storing any of them to the map.
                // See issue 26552.
                var entries = n.List.Slice();
                var statics = entries[..0L];
                slice<ptr<Node>> dynamics = default;
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in entries)
                    {
                        r = __r;
                        if (r.Op != OKEY)
                        {
                            Fatalf("OMAPLIT entry not OKEY: %v\n", r);
                        }

                        if (!isStaticCompositeLiteral(r.Left) || !isStaticCompositeLiteral(r.Right))
                        {
                            dynamics = append(dynamics, r);
                            continue;
                        } 

                        // Recursively ordering some static entries can change them to dynamic;
                        // e.g., OCONVIFACE nodes. See #31777.
                        r = o.expr(r, null);
                        if (!isStaticCompositeLiteral(r.Left) || !isStaticCompositeLiteral(r.Right))
                        {
                            dynamics = append(dynamics, r);
                            continue;
                        }

                        statics = append(statics, r);

                    }

                    r = r__prev1;
                }

                n.List.Set(statics);

                if (len(dynamics) == 0L)
                {
                    break;
                } 

                // Emit the creation of the map (with all its static entries).
                var m = o.newTemp(n.Type, false);
                var @as = nod(OAS, m, n);
                typecheck(as, ctxStmt);
                o.stmt(as);
                n = m; 

                // Emit eval+insert of dynamic entries, one at a time.
                {
                    var r__prev1 = r;

                    foreach (var (_, __r) in dynamics)
                    {
                        r = __r;
                        @as = nod(OAS, nod(OINDEX, n, r.Left), r.Right);
                        typecheck(as, ctxStmt); // Note: this converts the OINDEX to an OINDEXMAP
                        o.stmt(as);

                    }

                    r = r__prev1;
                }
            else 
                n.Left = o.expr(n.Left, null);
                n.Right = o.expr(n.Right, null);
                o.exprList(n.List);
                o.exprList(n.Rlist); 

                // Addition of strings turns into a function call.
                // Allocate a temporary to hold the strings.
                // Fewer than 5 strings use direct runtime helpers.
                        lineno = lno;
            return _addr_n!;

        }

        // okas creates and returns an assignment of val to ok,
        // including an explicit conversion if necessary.
        private static ptr<Node> okas(ptr<Node> _addr_ok, ptr<Node> _addr_val)
        {
            ref Node ok = ref _addr_ok.val;
            ref Node val = ref _addr_val.val;

            if (!ok.isBlank())
            {
                val = conv(val, ok.Type);
            }

            return _addr_nod(OAS, ok, val)!;

        }

        // as2 orders OAS2XXXX nodes. It creates temporaries to ensure left-to-right assignment.
        // The caller should order the right-hand side of the assignment before calling order.as2.
        // It rewrites,
        //     a, b, a = ...
        // as
        //    tmp1, tmp2, tmp3 = ...
        //     a, b, a = tmp1, tmp2, tmp3
        // This is necessary to ensure left to right assignment order.
        private static void as2(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            ptr<Node> tmplist = new slice<ptr<Node>>(new ptr<Node>[] {  });
            ptr<Node> left = new slice<ptr<Node>>(new ptr<Node>[] {  });
            foreach (var (ni, l) in n.List.Slice())
            {
                if (!l.isBlank())
                {
                    var tmp = o.newTemp(l.Type, types.Haspointers(l.Type));
                    n.List.SetIndex(ni, tmp);
                    tmplist = append(tmplist, tmp);
                    left = append(left, l);
                }

            }
            o.@out = append(o.@out, n);

            var @as = nod(OAS2, null, null);
            @as.List.Set(left);
            @as.Rlist.Set(tmplist);
            as = typecheck(as, ctxStmt);
            o.stmt(as);

        }

        // okAs2 orders OAS2XXX with ok.
        // Just like as2, this also adds temporaries to ensure left-to-right assignment.
        private static void okAs2(this ptr<Order> _addr_o, ptr<Node> _addr_n)
        {
            ref Order o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            ptr<Node> tmp1;            ptr<Node> tmp2;

            if (!n.List.First().isBlank())
            {
                var typ = n.Right.Type;
                tmp1 = o.newTemp(typ, types.Haspointers(typ));
            }

            if (!n.List.Second().isBlank())
            {
                tmp2 = o.newTemp(types.Types[TBOOL], false);
            }

            o.@out = append(o.@out, n);

            if (tmp1 != null)
            {
                var r = nod(OAS, n.List.First(), tmp1);
                r = typecheck(r, ctxStmt);
                o.mapAssign(r);
                n.List.SetFirst(tmp1);
            }

            if (tmp2 != null)
            {
                r = okas(_addr_n.List.Second(), tmp2);
                r = typecheck(r, ctxStmt);
                o.mapAssign(r);
                n.List.SetSecond(tmp2);
            }

        }
    }
}}}}
