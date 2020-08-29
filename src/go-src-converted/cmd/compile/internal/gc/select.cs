// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:25 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\select.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // select
        private static void typecheckselect(ref Node sel)
        {
            ref Node def = default;
            var lno = setlineno(sel);
            typecheckslice(sel.Ninit.Slice(), Etop);
            foreach (var (_, ncase) in sel.List.Slice())
            {
                if (ncase.Op != OXCASE)
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
                    ncase.List.SetFirst(typecheck(ncase.List.First(), Etop));
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
                        if (n.Rlist.First().Op != ORECV)
                        {
                            yyerrorl(n.Pos, "select assignment must have receive on right hand side");
                            break;
                        }
                        n.Op = OSELRECV2;
                        n.Left = n.List.First();
                        n.List.Set1(n.List.Second());
                        n.Right = n.Rlist.First();
                        n.Rlist.Set(null); 

                        // convert <-c into OSELRECV(N, <-c)
                    else if (n.Op == ORECV) 
                        n = nodl(n.Pos, OSELRECV, null, n);

                        n.SetTypecheck(1L);
                        ncase.Left = n;
                    else if (n.Op == OSEND) 
                        break;
                    else 
                        yyerrorl(n.Pos, "select case must be receive, send or assign recv"); 

                        // convert x = <-c into OSELRECV(x, <-c).
                        // remove implicit conversions; the eventual assignment
                        // will reintroduce them.
                                    }
                typecheckslice(ncase.Nbody.Slice(), Etop);
            }            lineno = lno;
        }

        private static void walkselect(ref Node sel)
        {
            var lno = setlineno(sel);
            if (sel.Nbody.Len() != 0L)
            {
                Fatalf("double walkselect");
            }
            var init = sel.Ninit.Slice();
            sel.Ninit.Set(null);

            init = append(init, walkselectcases(ref sel.List));
            sel.List.Set(null);

            sel.Nbody.Set(init);
            walkstmtlist(sel.Nbody.Slice());

            lineno = lno;
        }

        private static slice<ref Node> walkselectcases(ref Nodes cases)
        {
            var n = cases.Len();
            var sellineno = lineno; 

            // optimization: zero-case select
            if (n == 0L)
            {
                return new slice<ref Node>(new ref Node[] { mkcall("block",nil,nil) });
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
                    ref Node ch = default;

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
                            nblank = typecheck(nblank, Erv | Easgn);
                            n.Left = nblank;
                        }
                        n.Op = OAS2;
                        n.List.Prepend(n.Left);
                        n.Rlist.Set1(n.Right);
                        n.Right = null;
                        n.Left = null;
                        n.SetTypecheck(0L);
                        n = typecheck(n, Etop);
                    else 
                        Fatalf("select %v", n.Op); 

                        // ok already
                    // if ch == nil { block() }; n;
                    var a = nod(OIF, null, null);

                    a.Left = nod(OEQ, ch, nodnil());
                    Nodes ln = default;
                    ln.Set(l);
                    a.Nbody.Set1(mkcall("block", null, ref ln));
                    l = ln.Slice();
                    a = typecheck(a, Etop);
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
                        n.Right = typecheck(n.Right, Erv);
                    else if (n.Op == OSELRECV || n.Op == OSELRECV2) 
                        if (n.Op == OSELRECV2 && n.List.Len() == 0L)
                        {
                            n.Op = OSELRECV;
                        }
                        if (n.Op == OSELRECV2)
                        {
                            n.List.SetFirst(nod(OADDR, n.List.First(), null));
                            n.List.SetFirst(typecheck(n.List.First(), Erv));
                        }
                        if (n.Left == null)
                        {
                            n.Left = nodnil();
                        }
                        else
                        {
                            n.Left = nod(OADDR, n.Left, null);
                            n.Left = typecheck(n.Left, Erv);
                        }
                                    } 

                // optimization: two-case select but one is default: single non-blocking op.

                cas = cas__prev1;
            }

            if (n == 2L && (cases.First().Left == null || cases.Second().Left == null))
            {
                cas = default;
                ref Node dflt = default;
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
                    r.Left = mkcall1(chanfn("selectnbsend", 2L, ch.Type), types.Types[TBOOL], ref r.Ninit, ch, n.Right);
                else if (n.Op == OSELRECV) 
                    // if c != nil && selectnbrecv(&v, c) { body } else { default body }
                    r = nod(OIF, null, null);
                    r.Ninit.Set(cas.Ninit.Slice());
                    ch = n.Right.Left;
                    r.Left = mkcall1(chanfn("selectnbrecv", 2L, ch.Type), types.Types[TBOOL], ref r.Ninit, n.Left, ch);
                else if (n.Op == OSELRECV2) 
                    // if c != nil && selectnbrecv2(&v, c) { body } else { default body }
                    r = nod(OIF, null, null);
                    r.Ninit.Set(cas.Ninit.Slice());
                    ch = n.Right.Left;
                    r.Left = mkcall1(chanfn("selectnbrecv2", 2L, ch.Type), types.Types[TBOOL], ref r.Ninit, n.Left, n.List.First(), ch);
                else 
                    Fatalf("select %v", n.Op);
                                r.Left = typecheck(r.Left, Erv);
                r.Nbody.Set(cas.Nbody.Slice());
                r.Rlist.Set(append(dflt.Ninit.Slice(), dflt.Nbody.Slice()));
                return new slice<ref Node>(new ref Node[] { r, nod(OBREAK,nil,nil) });
            }
            slice<ref Node> init = default; 

            // generate sel-struct
            lineno = sellineno;
            var selv = temp(selecttype(int64(n)));
            r = nod(OAS, selv, null);
            r = typecheck(r, Etop);
            init = append(init, r);
            var var_ = conv(conv(nod(OADDR, selv, null), types.Types[TUNSAFEPTR]), types.NewPtr(types.Types[TUINT8]));
            r = mkcall("newselect", null, null, var_, nodintconst(selv.Type.Width), nodintconst(int64(n)));
            r = typecheck(r, Etop);
            init = append(init, r); 

            // register cases
            {
                var cas__prev1 = cas;

                foreach (var (_, __cas) in cases.Slice())
                {
                    cas = __cas;
                    setlineno(cas);

                    init = append(init, cas.Ninit.Slice());
                    cas.Ninit.Set(null);

                    ref Node x = default;
                    {
                        var n__prev1 = n;

                        n = cas.Left;

                        if (n != null)
                        {
                            init = append(init, n.Ninit.Slice());


                            if (n.Op == OSEND) 
                                // selectsend(sel *byte, hchan *chan any, elem *any)
                                x = mkcall1(chanfn("selectsend", 2L, n.Left.Type), null, null, var_, n.Left, n.Right);
                            else if (n.Op == OSELRECV) 
                                // selectrecv(sel *byte, hchan *chan any, elem *any, received *bool)
                                x = mkcall1(chanfn("selectrecv", 2L, n.Right.Left.Type), null, null, var_, n.Right.Left, n.Left, nodnil());
                            else if (n.Op == OSELRECV2) 
                                // selectrecv(sel *byte, hchan *chan any, elem *any, received *bool)
                                x = mkcall1(chanfn("selectrecv", 2L, n.Right.Left.Type), null, null, var_, n.Right.Left, n.Left, n.List.First());
                            else 
                                Fatalf("select %v", n.Op);
                                                    }
                        else
                        { 
                            // selectdefault(sel *byte)
                            x = mkcall("selectdefault", null, null, var_);
                        }

                        n = n__prev1;

                    }

                    init = append(init, x);
                } 

                // run the select

                cas = cas__prev1;
            }

            lineno = sellineno;
            var chosen = temp(types.Types[TINT]);
            r = nod(OAS, chosen, mkcall("selectgo", types.Types[TINT], null, var_));
            r = typecheck(r, Etop);
            init = append(init, r); 

            // selv is no longer alive after selectgo.
            init = append(init, nod(OVARKILL, selv, null)); 

            // dispatch cases
            {
                var cas__prev1 = cas;

                foreach (var (__i, __cas) in cases.Slice())
                {
                    i = __i;
                    cas = __cas;
                    setlineno(cas);

                    var cond = nod(OEQ, chosen, nodintconst(int64(i)));
                    cond = typecheck(cond, Erv);

                    r = nod(OIF, cond, null);
                    r.Nbody.AppendNodes(ref cas.Nbody);
                    r.Nbody.Append(nod(OBREAK, null, null));
                    init = append(init, r);
                }

                cas = cas__prev1;
            }

            return init;
        }

        // Keep in sync with src/runtime/select.go.
        private static ref types.Type selecttype(long size)
        { 
            // TODO(dvyukov): it's possible to generate Scase only once
            // and then cache; and also cache Select per size.

            var scase = tostruct(new slice<ref Node>(new ref Node[] { namedfield("elem",types.NewPtr(types.Types[TUINT8])), namedfield("chan",types.NewPtr(types.Types[TUINT8])), namedfield("pc",types.Types[TUINTPTR]), namedfield("kind",types.Types[TUINT16]), namedfield("receivedp",types.NewPtr(types.Types[TUINT8])), namedfield("releasetime",types.Types[TUINT64]) }));
            scase.SetNoalg(true);

            var sel = tostruct(new slice<ref Node>(new ref Node[] { namedfield("tcase",types.Types[TUINT16]), namedfield("ncase",types.Types[TUINT16]), namedfield("pollorder",types.NewPtr(types.Types[TUINT8])), namedfield("lockorder",types.NewPtr(types.Types[TUINT8])), namedfield("scase",types.NewArray(scase,size)), namedfield("lockorderarr",types.NewArray(types.Types[TUINT16],size)), namedfield("pollorderarr",types.NewArray(types.Types[TUINT16],size)) }));
            sel.SetNoalg(true);

            return sel;
        }
    }
}}}}
