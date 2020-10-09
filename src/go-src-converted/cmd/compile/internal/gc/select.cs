// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:33 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\select.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // select
        private static void typecheckselect(ptr<Node> _addr_sel)
        {
            ref Node sel = ref _addr_sel.val;

            ptr<Node> def;
            var lno = setlineno(sel);
            typecheckslice(sel.Ninit.Slice(), ctxStmt);
            foreach (var (_, ncase) in sel.List.Slice())
            {
                if (ncase.Op != OCASE)
                {
                    setlineno(ncase);
                    Fatalf("typecheckselect %v", ncase.Op);
                }
                if (ncase.List.Len() == 0L)
                { 
                    // default
                    if (def != null)
                    {
                        yyerrorl(ncase.Pos, "multiple defaults in select (first at %v)", def.Line());
                    }
                    else
                    {
                        def = ncase;
                    }
                }
                else if (ncase.List.Len() > 1L)
                {
                    yyerrorl(ncase.Pos, "select cases cannot be lists");
                }
                else
                {
                    ncase.List.SetFirst(typecheck(ncase.List.First(), ctxStmt));
                    var n = ncase.List.First();
                    ncase.Left = n;
                    ncase.List.Set(null);

                    if (n.Op == OAS) 
                        if ((n.Right.Op == OCONVNOP || n.Right.Op == OCONVIFACE) && n.Right.Implicit())
                        {
                            n.Right = n.Right.Left;
                        }
                        if (n.Right.Op != ORECV)
                        {
                            yyerrorl(n.Pos, "select assignment must have receive on right hand side");
                            break;
                        }
                        n.Op = OSELRECV; 

                        // convert x, ok = <-c into OSELRECV2(x, <-c) with ntest=ok
                    else if (n.Op == OAS2RECV) 
                        if (n.Right.Op != ORECV)
                        {
                            yyerrorl(n.Pos, "select assignment must have receive on right hand side");
                            break;
                        }
                        n.Op = OSELRECV2;
                        n.Left = n.List.First();
                        n.List.Set1(n.List.Second()); 

                        // convert <-c into OSELRECV(N, <-c)
                    else if (n.Op == ORECV) 
                        n = nodl(n.Pos, OSELRECV, null, n);

                        n.SetTypecheck(1L);
                        ncase.Left = n;
                    else if (n.Op == OSEND) 
                        break;
                    else 
                        var pos = n.Pos;
                        if (n.Op == ONAME)
                        { 
                            // We don't have the right position for ONAME nodes (see #15459 and
                            // others). Using ncase.Pos for now as it will provide the correct
                            // line number (assuming the expression follows the "case" keyword
                            // on the same line). This matches the approach before 1.10.
                            pos = ncase.Pos;

                        }
                        yyerrorl(pos, "select case must be receive, send or assign recv"); 

                        // convert x = <-c into OSELRECV(x, <-c).
                        // remove implicit conversions; the eventual assignment
                        // will reintroduce them.
                                    }
                typecheckslice(ncase.Nbody.Slice(), ctxStmt);

            }            lineno = lno;

        }

        private static void walkselect(ptr<Node> _addr_sel)
        {
            ref Node sel = ref _addr_sel.val;

            var lno = setlineno(sel);
            if (sel.Nbody.Len() != 0L)
            {
                Fatalf("double walkselect");
            }

            var init = sel.Ninit.Slice();
            sel.Ninit.Set(null);

            init = append(init, walkselectcases(_addr_sel.List));
            sel.List.Set(null);

            sel.Nbody.Set(init);
            walkstmtlist(sel.Nbody.Slice());

            lineno = lno;

        }

        private static slice<ptr<Node>> walkselectcases(ptr<Nodes> _addr_cases)
        {
            ref Nodes cases = ref _addr_cases.val;

            var n = cases.Len();
            var sellineno = lineno; 

            // optimization: zero-case select
            if (n == 0L)
            {
                return new slice<ptr<Node>>(new ptr<Node>[] { mkcall("block",nil,nil) });
            } 

            // optimization: one-case select: single op.
            // TODO(rsc): Reenable optimization once order.go can handle it.
            // golang.org/issue/7672.
            if (n == 1L)
            {
                var cas = cases.First();
                setlineno(cas);
                var l = cas.Ninit.Slice();
                if (cas.Left != null)
                { // not default:
                    n = cas.Left;
                    l = append(l, n.Ninit.Slice());
                    n.Ninit.Set(null);
                    ptr<Node> ch;

                    if (n.Op == OSEND) 
                        ch = n.Left;
                    else if (n.Op == OSELRECV || n.Op == OSELRECV2) 
                        ch = n.Right.Left;
                        if (n.Op == OSELRECV || n.List.Len() == 0L)
                        {
                            if (n.Left == null)
                            {
                                n = n.Right;
                            }
                            else
                            {
                                n.Op = OAS;
                            }

                            break;

                        }

                        if (n.Left == null)
                        {
                            nblank = typecheck(nblank, ctxExpr | ctxAssign);
                            n.Left = nblank;
                        }

                        n.Op = OAS2;
                        n.List.Prepend(n.Left);
                        n.Rlist.Set1(n.Right);
                        n.Right = null;
                        n.Left = null;
                        n.SetTypecheck(0L);
                        n = typecheck(n, ctxStmt);
                    else 
                        Fatalf("select %v", n.Op); 

                        // ok already
                    // if ch == nil { block() }; n;
                    var a = nod(OIF, null, null);

                    a.Left = nod(OEQ, ch, nodnil());
                    ref Nodes ln = ref heap(out ptr<Nodes> _addr_ln);
                    ln.Set(l);
                    a.Nbody.Set1(mkcall("block", null, _addr_ln));
                    l = ln.Slice();
                    a = typecheck(a, ctxStmt);
                    l = append(l, a, n);

                }

                l = append(l, cas.Nbody.Slice());
                l = append(l, nod(OBREAK, null, null));
                return l;

            } 

            // convert case value arguments to addresses.
            // this rewrite is used by both the general code and the next optimization.
            {
                var cas__prev1 = cas;

                foreach (var (_, __cas) in cases.Slice())
                {
                    cas = __cas;
                    setlineno(cas);
                    n = cas.Left;
                    if (n == null)
                    {
                        continue;
                    }


                    if (n.Op == OSEND) 
                        n.Right = nod(OADDR, n.Right, null);
                        n.Right = typecheck(n.Right, ctxExpr);
                    else if (n.Op == OSELRECV || n.Op == OSELRECV2) 
                        if (n.Op == OSELRECV2 && n.List.Len() == 0L)
                        {
                            n.Op = OSELRECV;
                        }

                        if (n.Left != null)
                        {
                            n.Left = nod(OADDR, n.Left, null);
                            n.Left = typecheck(n.Left, ctxExpr);
                        }

                                    } 

                // optimization: two-case select but one is default: single non-blocking op.

                cas = cas__prev1;
            }

            if (n == 2L && (cases.First().Left == null || cases.Second().Left == null))
            {
                cas = ;
                ptr<Node> dflt;
                if (cases.First().Left == null)
                {
                    cas = cases.Second();
                    dflt = cases.First();
                }
                else
                {
                    dflt = cases.Second();
                    cas = cases.First();
                }

                n = cas.Left;
                setlineno(n);
                var r = nod(OIF, null, null);
                r.Ninit.Set(cas.Ninit.Slice());

                if (n.Op == OSEND) 
                    // if selectnbsend(c, v) { body } else { default body }
                    ch = n.Left;
                    r.Left = mkcall1(chanfn("selectnbsend", 2L, ch.Type), types.Types[TBOOL], _addr_r.Ninit, ch, n.Right);
                else if (n.Op == OSELRECV) 
                    // if selectnbrecv(&v, c) { body } else { default body }
                    r = nod(OIF, null, null);
                    r.Ninit.Set(cas.Ninit.Slice());
                    ch = n.Right.Left;
                    var elem = n.Left;
                    if (elem == null)
                    {
                        elem = nodnil();
                    }

                    r.Left = mkcall1(chanfn("selectnbrecv", 2L, ch.Type), types.Types[TBOOL], _addr_r.Ninit, elem, ch);
                else if (n.Op == OSELRECV2) 
                    // if selectnbrecv2(&v, &received, c) { body } else { default body }
                    r = nod(OIF, null, null);
                    r.Ninit.Set(cas.Ninit.Slice());
                    ch = n.Right.Left;
                    elem = n.Left;
                    if (elem == null)
                    {
                        elem = nodnil();
                    }

                    var receivedp = nod(OADDR, n.List.First(), null);
                    receivedp = typecheck(receivedp, ctxExpr);
                    r.Left = mkcall1(chanfn("selectnbrecv2", 2L, ch.Type), types.Types[TBOOL], _addr_r.Ninit, elem, receivedp, ch);
                else 
                    Fatalf("select %v", n.Op);
                                r.Left = typecheck(r.Left, ctxExpr);
                r.Nbody.Set(cas.Nbody.Slice());
                r.Rlist.Set(append(dflt.Ninit.Slice(), dflt.Nbody.Slice()));
                return new slice<ptr<Node>>(new ptr<Node>[] { r, nod(OBREAK,nil,nil) });

            }

            slice<ptr<Node>> init = default; 

            // generate sel-struct
            lineno = sellineno;
            var selv = temp(types.NewArray(scasetype(), int64(n)));
            r = nod(OAS, selv, null);
            r = typecheck(r, ctxStmt);
            init = append(init, r);

            var order = temp(types.NewArray(types.Types[TUINT16], 2L * int64(n)));
            r = nod(OAS, order, null);
            r = typecheck(r, ctxStmt);
            init = append(init, r); 

            // register cases
            {
                var i__prev1 = i;
                var cas__prev1 = cas;

                foreach (var (__i, __cas) in cases.Slice())
                {
                    i = __i;
                    cas = __cas;
                    setlineno(cas);

                    init = append(init, cas.Ninit.Slice());
                    cas.Ninit.Set(null); 

                    // Keep in sync with runtime/select.go.
                    const var caseNil = iota;
                    const var caseRecv = 0;
                    const var caseSend = 1;
                    const var caseDefault = 2;


                    ptr<Node> c;                    elem = ;

                    long kind = caseDefault;

                    {
                        var n__prev1 = n;

                        n = cas.Left;

                        if (n != null)
                        {
                            init = append(init, n.Ninit.Slice());


                            if (n.Op == OSEND) 
                                kind = caseSend;
                                c = n.Left;
                                elem = n.Right;
                            else if (n.Op == OSELRECV || n.Op == OSELRECV2) 
                                kind = caseRecv;
                                c = n.Right.Left;
                                elem = n.Left;
                            else 
                                Fatalf("select %v", n.Op);
                            
                        }

                        n = n__prev1;

                    }


                    Action<@string, ptr<Node>> setField = (f, val) =>
                    {
                        r = nod(OAS, nodSym(ODOT, nod(OINDEX, selv, nodintconst(int64(i))), lookup(f)), val);
                        r = typecheck(r, ctxStmt);
                        init = append(init, r);
                    }
;

                    setField("kind", nodintconst(kind));
                    if (c != null)
                    {
                        c = convnop(c, types.Types[TUNSAFEPTR]);
                        setField("c", c);
                    }

                    if (elem != null)
                    {
                        elem = convnop(elem, types.Types[TUNSAFEPTR]);
                        setField("elem", elem);
                    } 

                    // TODO(mdempsky): There should be a cleaner way to
                    // handle this.
                    if (instrumenting)
                    {
                        r = mkcall("selectsetpc", null, null, bytePtrToIndex(_addr_selv, int64(i)));
                        init = append(init, r);
                    }

                } 

                // run the select

                i = i__prev1;
                cas = cas__prev1;
            }

            lineno = sellineno;
            var chosen = temp(types.Types[TINT]);
            var recvOK = temp(types.Types[TBOOL]);
            r = nod(OAS2, null, null);
            r.List.Set2(chosen, recvOK);
            var fn = syslook("selectgo");
            r.Rlist.Set1(mkcall1(fn, fn.Type.Results(), null, bytePtrToIndex(_addr_selv, 0L), bytePtrToIndex(_addr_order, 0L), nodintconst(int64(n))));
            r = typecheck(r, ctxStmt);
            init = append(init, r); 

            // selv and order are no longer alive after selectgo.
            init = append(init, nod(OVARKILL, selv, null));
            init = append(init, nod(OVARKILL, order, null)); 

            // dispatch cases
            {
                var i__prev1 = i;
                var cas__prev1 = cas;

                foreach (var (__i, __cas) in cases.Slice())
                {
                    i = __i;
                    cas = __cas;
                    setlineno(cas);

                    var cond = nod(OEQ, chosen, nodintconst(int64(i)));
                    cond = typecheck(cond, ctxExpr);
                    cond = defaultlit(cond, null);

                    r = nod(OIF, cond, null);

                    {
                        var n__prev1 = n;

                        n = cas.Left;

                        if (n != null && n.Op == OSELRECV2)
                        {
                            var x = nod(OAS, n.List.First(), recvOK);
                            x = typecheck(x, ctxStmt);
                            r.Nbody.Append(x);
                        }

                        n = n__prev1;

                    }


                    r.Nbody.AppendNodes(_addr_cas.Nbody);
                    r.Nbody.Append(nod(OBREAK, null, null));
                    init = append(init, r);

                }

                i = i__prev1;
                cas = cas__prev1;
            }

            return init;

        }

        // bytePtrToIndex returns a Node representing "(*byte)(&n[i])".
        private static ptr<Node> bytePtrToIndex(ptr<Node> _addr_n, long i)
        {
            ref Node n = ref _addr_n.val;

            var s = nod(OADDR, nod(OINDEX, n, nodintconst(i)), null);
            var t = types.NewPtr(types.Types[TUINT8]);
            return _addr_convnop(s, t)!;
        }

        private static ptr<types.Type> scase;

        // Keep in sync with src/runtime/select.go.
        private static ptr<types.Type> scasetype()
        {
            if (scase == null)
            {
                scase = tostruct(new slice<ptr<Node>>(new ptr<Node>[] { namedfield("c",types.Types[TUNSAFEPTR]), namedfield("elem",types.Types[TUNSAFEPTR]), namedfield("kind",types.Types[TUINT16]), namedfield("pc",types.Types[TUINTPTR]), namedfield("releasetime",types.Types[TINT64]) }));
                scase.SetNoalg(true);
            }

            return _addr_scase!;

        }
    }
}}}}
