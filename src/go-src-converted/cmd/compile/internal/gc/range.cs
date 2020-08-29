// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:12 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\range.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // range
        private static void typecheckrange(ref Node n)
        { 
            // Typechecking order is important here:
            // 0. first typecheck range expression (slice/map/chan),
            //    it is evaluated only once and so logically it is not part of the loop.
            // 1. typcheck produced values,
            //    this part can declare new vars and so it must be typechecked before body,
            //    because body can contain a closure that captures the vars.
            // 2. decldepth++ to denote loop body.
            // 3. typecheck body.
            // 4. decldepth--.
            typecheckrangeExpr(n); 

            // second half of dance, the first half being typecheckrangeExpr
            n.SetTypecheck(1L);
            var ls = n.List.Slice();
            foreach (var (i1, n1) in ls)
            {
                if (n1.Typecheck() == 0L)
                {
                    ls[i1] = typecheck(ls[i1], Erv | Easgn);
                }
            }            decldepth++;
            typecheckslice(n.Nbody.Slice(), Etop);
            decldepth--;
        }

        private static void typecheckrangeExpr(ref Node n)
        {
            n.Right = typecheck(n.Right, Erv);

            var t = n.Right.Type;
            if (t == null)
            {
                return;
            } 
            // delicate little dance.  see typecheckas2
            var ls = n.List.Slice();
            foreach (var (i1, n1) in ls)
            {
                if (n1.Name == null || n1.Name.Defn != n)
                {
                    ls[i1] = typecheck(ls[i1], Erv | Easgn);
                }
            }
            if (t.IsPtr() && t.Elem().IsArray())
            {
                t = t.Elem();
            }
            n.Type = t;

            ref types.Type t1 = default;            ref types.Type t2 = default;

            var toomany = false;

            if (t.Etype == TARRAY || t.Etype == TSLICE) 
                t1 = types.Types[TINT];
                t2 = t.Elem();
            else if (t.Etype == TMAP) 
                t1 = t.Key();
                t2 = t.Val();
            else if (t.Etype == TCHAN) 
                if (!t.ChanDir().CanRecv())
                {
                    yyerrorl(n.Pos, "invalid operation: range %v (receive from send-only type %v)", n.Right, n.Right.Type);
                    return;
                }
                t1 = t.Elem();
                t2 = null;
                if (n.List.Len() == 2L)
                {
                    toomany = true;
                }
            else if (t.Etype == TSTRING) 
                t1 = types.Types[TINT];
                t2 = types.Runetype;
            else 
                yyerrorl(n.Pos, "cannot range over %L", n.Right);
                return;
                        if (n.List.Len() > 2L || toomany)
            {
                yyerrorl(n.Pos, "too many variables in range");
            }
            ref Node v1 = default;            ref Node v2 = default;

            if (n.List.Len() != 0L)
            {
                v1 = n.List.First();
            }
            if (n.List.Len() > 1L)
            {
                v2 = n.List.Second();
            } 

            // this is not only a optimization but also a requirement in the spec.
            // "if the second iteration variable is the blank identifier, the range
            // clause is equivalent to the same clause with only the first variable
            // present."
            if (isblank(v2))
            {
                if (v1 != null)
                {
                    n.List.Set1(v1);
                }
                v2 = null;
            }
            @string why = default;
            if (v1 != null)
            {
                if (v1.Name != null && v1.Name.Defn == n)
                {
                    v1.Type = t1;
                }
                else if (v1.Type != null && assignop(t1, v1.Type, ref why) == 0L)
                {
                    yyerrorl(n.Pos, "cannot assign type %v to %L in range%s", t1, v1, why);
                }
                checkassign(n, v1);
            }
            if (v2 != null)
            {
                if (v2.Name != null && v2.Name.Defn == n)
                {
                    v2.Type = t2;
                }
                else if (v2.Type != null && assignop(t2, v2.Type, ref why) == 0L)
                {
                    yyerrorl(n.Pos, "cannot assign type %v to %L in range%s", t2, v2, why);
                }
                checkassign(n, v2);
            }
        }

        private static bool cheapComputableIndex(long width)
        {

            // MIPS does not have R+R addressing
            // Arm64 may lack ability to generate this code in our assembler,
            // but the architecture supports it.
            if (thearch.LinkArch.Family == sys.PPC64 || thearch.LinkArch.Family == sys.S390X) 
                return width == 1L;
            else if (thearch.LinkArch.Family == sys.AMD64 || thearch.LinkArch.Family == sys.I386 || thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.ARM) 
                switch (width)
                {
                    case 1L: 

                    case 2L: 

                    case 4L: 

                    case 8L: 
                        return true;
                        break;
                }
                        return false;
        }

        // walkrange transforms various forms of ORANGE into
        // simpler forms.  The result must be assigned back to n.
        // Node n may also be modified in place, and may also be
        // the returned node.
        private static ref Node walkrange(ref Node n)
        { 
            // variable name conventions:
            //    ohv1, hv1, hv2: hidden (old) val 1, 2
            //    ha, hit: hidden aggregate, iterator
            //    hn, hp: hidden len, pointer
            //    hb: hidden bool
            //    a, v1, v2: not hidden aggregate, val 1, 2

            var t = n.Type;

            var a = n.Right;
            var lno = setlineno(a);
            n.Right = null;

            ref Node v1 = default;            ref Node v2 = default;

            var l = n.List.Len();
            if (l > 0L)
            {
                v1 = n.List.First();
            }
            if (l > 1L)
            {
                v2 = n.List.Second();
            }
            if (isblank(v2))
            {
                v2 = null;
            }
            if (isblank(v1) && v2 == null)
            {
                v1 = null;
            }
            if (v1 == null && v2 != null)
            {
                Fatalf("walkrange: v2 != nil while v1 == nil");
            } 

            // n.List has no meaning anymore, clear it
            // to avoid erroneous processing by racewalk.
            n.List.Set(null);

            ref Node ifGuard = default;

            var translatedLoopOp = OFOR;

            slice<ref Node> body = default;
            slice<ref Node> init = default;

            if (t.Etype == TARRAY || t.Etype == TSLICE) 
                if (memclrrange(n, v1, v2, a))
                {
                    lineno = lno;
                    return n;
                } 

                // orderstmt arranged for a copy of the array/slice variable if needed.
                var ha = a;

                var hv1 = temp(types.Types[TINT]);
                var hn = temp(types.Types[TINT]);

                init = append(init, nod(OAS, hv1, null));
                init = append(init, nod(OAS, hn, nod(OLEN, ha, null)));

                n.Left = nod(OLT, hv1, hn);
                n.Right = nod(OAS, hv1, nod(OADD, hv1, nodintconst(1L))); 

                // for range ha { body }
                if (v1 == null)
                {
                    break;
                } 

                // for v1 := range ha { body }
                if (v2 == null)
                {
                    body = new slice<ref Node>(new ref Node[] { nod(OAS,v1,hv1) });
                    break;
                } 

                // for v1, v2 := range ha { body }
                if (cheapComputableIndex(n.Type.Elem().Width))
                { 
                    // v1, v2 = hv1, ha[hv1]
                    var tmp = nod(OINDEX, ha, hv1);
                    tmp.SetBounded(true); 
                    // Use OAS2 to correctly handle assignments
                    // of the form "v1, a[v1] := range".
                    a = nod(OAS2, null, null);
                    a.List.Set2(v1, v2);
                    a.Rlist.Set2(hv1, tmp);
                    body = new slice<ref Node>(new ref Node[] { a });
                    break;
                }
                if (objabi.Preemptibleloops_enabled != 0L)
                { 
                    // Doing this transformation makes a bounds check removal less trivial; see #20711
                    // TODO enhance the preemption check insertion so that this transformation is not necessary.
                    ifGuard = nod(OIF, null, null);
                    ifGuard.Left = nod(OLT, hv1, hn);
                    translatedLoopOp = OFORUNTIL;
                }
                var hp = temp(types.NewPtr(n.Type.Elem()));
                tmp = nod(OINDEX, ha, nodintconst(0L));
                tmp.SetBounded(true);
                init = append(init, nod(OAS, hp, nod(OADDR, tmp, null))); 

                // Use OAS2 to correctly handle assignments
                // of the form "v1, a[v1] := range".
                a = nod(OAS2, null, null);
                a.List.Set2(v1, v2);
                a.Rlist.Set2(hv1, nod(OIND, hp, null));
                body = append(body, a); 

                // Advance pointer as part of increment.
                // We used to advance the pointer before executing the loop body,
                // but doing so would make the pointer point past the end of the
                // array during the final iteration, possibly causing another unrelated
                // piece of memory not to be garbage collected until the loop finished.
                // Advancing during the increment ensures that the pointer p only points
                // pass the end of the array during the final "p++; i++; if(i >= len(x)) break;",
                // after which p is dead, so it cannot confuse the collector.
                tmp = nod(OADD, hp, nodintconst(t.Elem().Width));

                tmp.Type = hp.Type;
                tmp.SetTypecheck(1L);
                tmp.Right.Type = types.Types[types.Tptr];
                tmp.Right.SetTypecheck(1L);
                a = nod(OAS, hp, tmp);
                a = typecheck(a, Etop);
                n.Right.Ninit.Set1(a);
            else if (t.Etype == TMAP) 
                // orderstmt allocated the iterator for us.
                // we only use a once, so no copy needed.
                ha = a;

                var hit = prealloc[n];
                var th = hit.Type;
                n.Left = null;
                var keysym = th.Field(0L).Sym; // depends on layout of iterator struct.  See reflect.go:hiter
                var valsym = th.Field(1L).Sym; // ditto

                var fn = syslook("mapiterinit");

                fn = substArgTypes(fn, t.Key(), t.Val(), th);
                init = append(init, mkcall1(fn, null, null, typename(t), ha, nod(OADDR, hit, null)));
                n.Left = nod(ONE, nodSym(ODOT, hit, keysym), nodnil());

                fn = syslook("mapiternext");
                fn = substArgTypes(fn, th);
                n.Right = mkcall1(fn, null, null, nod(OADDR, hit, null));

                var key = nodSym(ODOT, hit, keysym);
                key = nod(OIND, key, null);
                if (v1 == null)
                {
                    body = null;
                }
                else if (v2 == null)
                {
                    body = new slice<ref Node>(new ref Node[] { nod(OAS,v1,key) });
                }
                else
                {
                    var val = nodSym(ODOT, hit, valsym);
                    val = nod(OIND, val, null);
                    a = nod(OAS2, null, null);
                    a.List.Set2(v1, v2);
                    a.Rlist.Set2(key, val);
                    body = new slice<ref Node>(new ref Node[] { a });
                }
            else if (t.Etype == TCHAN) 
                // orderstmt arranged for a copy of the channel variable.
                ha = a;

                n.Left = null;

                hv1 = temp(t.Elem());
                hv1.SetTypecheck(1L);
                if (types.Haspointers(t.Elem()))
                {
                    init = append(init, nod(OAS, hv1, null));
                }
                var hb = temp(types.Types[TBOOL]);

                n.Left = nod(ONE, hb, nodbool(false));
                a = nod(OAS2RECV, null, null);
                a.SetTypecheck(1L);
                a.List.Set2(hv1, hb);
                a.Rlist.Set1(nod(ORECV, ha, null));
                n.Left.Ninit.Set1(a);
                if (v1 == null)
                {
                    body = null;
                }
                else
                {
                    body = new slice<ref Node>(new ref Node[] { nod(OAS,v1,hv1) });
                } 
                // Zero hv1. This prevents hv1 from being the sole, inaccessible
                // reference to an otherwise GC-able value during the next channel receive.
                // See issue 15281.
                body = append(body, nod(OAS, hv1, null));
            else if (t.Etype == TSTRING) 
                // Transform string range statements like "for v1, v2 = range a" into
                //
                // ha := a
                // for hv1 := 0; hv1 < len(ha); {
                //   hv1t := hv1
                //   hv2 := rune(ha[hv1])
                //   if hv2 < utf8.RuneSelf {
                //      hv1++
                //   } else {
                //      hv2, hv1 = decoderune(ha, hv1)
                //   }
                //   v1, v2 = hv1t, hv2
                //   // original body
                // }

                // orderstmt arranged for a copy of the string variable.
                ha = a;

                hv1 = temp(types.Types[TINT]);
                var hv1t = temp(types.Types[TINT]);
                var hv2 = temp(types.Runetype); 

                // hv1 := 0
                init = append(init, nod(OAS, hv1, null)); 

                // hv1 < len(ha)
                n.Left = nod(OLT, hv1, nod(OLEN, ha, null));

                if (v1 != null)
                { 
                    // hv1t = hv1
                    body = append(body, nod(OAS, hv1t, hv1));
                } 

                // hv2 := rune(ha[hv1])
                var nind = nod(OINDEX, ha, hv1);
                nind.SetBounded(true);
                body = append(body, nod(OAS, hv2, conv(nind, types.Runetype))); 

                // if hv2 < utf8.RuneSelf
                var nif = nod(OIF, null, null);
                nif.Left = nod(OLT, hv2, nodintconst(utf8.RuneSelf)); 

                // hv1++
                nif.Nbody.Set1(nod(OAS, hv1, nod(OADD, hv1, nodintconst(1L)))); 

                // } else {
                var eif = nod(OAS2, null, null);
                nif.Rlist.Set1(eif); 

                // hv2, hv1 = decoderune(ha, hv1)
                eif.List.Set2(hv2, hv1);
                fn = syslook("decoderune");
                eif.Rlist.Set1(mkcall1(fn, fn.Type.Results(), null, ha, hv1));

                body = append(body, nif);

                if (v1 != null)
                {
                    if (v2 != null)
                    { 
                        // v1, v2 = hv1t, hv2
                        a = nod(OAS2, null, null);
                        a.List.Set2(v1, v2);
                        a.Rlist.Set2(hv1t, hv2);
                        body = append(body, a);
                    }
                    else
                    { 
                        // v1 = hv1t
                        body = append(body, nod(OAS, v1, hv1t));
                    }
                }
            else 
                Fatalf("walkrange");
                        n.Op = translatedLoopOp;
            typecheckslice(init, Etop);

            if (ifGuard != null)
            {
                ifGuard.Ninit.Append(init);
                typecheckslice(ifGuard.Left.Ninit.Slice(), Etop);
                ifGuard.Left = typecheck(ifGuard.Left, Erv);
            }
            else
            {
                n.Ninit.Append(init);
            }
            typecheckslice(n.Left.Ninit.Slice(), Etop);

            n.Left = typecheck(n.Left, Erv);
            n.Right = typecheck(n.Right, Etop);
            typecheckslice(body, Etop);
            n.Nbody.Prepend(body);

            if (ifGuard != null)
            {
                ifGuard.Nbody.Set1(n);
                n = ifGuard;
            }
            n = walkstmt(n);

            lineno = lno;
            return n;
        }

        // Lower n into runtime·memclr if possible, for
        // fast zeroing of slices and arrays (issue 5373).
        // Look for instances of
        //
        // for i := range a {
        //     a[i] = zero
        // }
        //
        // in which the evaluation of a is side-effect-free.
        //
        // Parameters are as in walkrange: "for v1, v2 = range a".
        private static bool memclrrange(ref Node n, ref Node v1, ref Node v2, ref Node a)
        {
            if (Debug['N'] != 0L || instrumenting)
            {
                return false;
            }
            if (v1 == null || v2 != null)
            {
                return false;
            }
            if (n.Nbody.Len() == 0L || n.Nbody.First() == null || n.Nbody.Len() > 1L)
            {
                return false;
            }
            var stmt = n.Nbody.First(); // only stmt in body
            if (stmt.Op != OAS || stmt.Left.Op != OINDEX)
            {
                return false;
            }
            if (!samesafeexpr(stmt.Left.Left, a) || !samesafeexpr(stmt.Left.Right, v1))
            {
                return false;
            }
            var elemsize = n.Type.Elem().Width;
            if (elemsize <= 0L || !iszero(stmt.Right))
            {
                return false;
            } 

            // Convert to
            // if len(a) != 0 {
            //     hp = &a[0]
            //     hn = len(a)*sizeof(elem(a))
            //     memclr{NoHeap,Has}Pointers(hp, hn)
            //     i = len(a) - 1
            // }
            n.Op = OIF;

            n.Nbody.Set(null);
            n.Left = nod(ONE, nod(OLEN, a, null), nodintconst(0L)); 

            // hp = &a[0]
            var hp = temp(types.Types[TUNSAFEPTR]);

            var tmp = nod(OINDEX, a, nodintconst(0L));
            tmp.SetBounded(true);
            tmp = nod(OADDR, tmp, null);
            tmp = nod(OCONVNOP, tmp, null);
            tmp.Type = types.Types[TUNSAFEPTR];
            n.Nbody.Append(nod(OAS, hp, tmp)); 

            // hn = len(a) * sizeof(elem(a))
            var hn = temp(types.Types[TUINTPTR]);

            tmp = nod(OLEN, a, null);
            tmp = nod(OMUL, tmp, nodintconst(elemsize));
            tmp = conv(tmp, types.Types[TUINTPTR]);
            n.Nbody.Append(nod(OAS, hn, tmp));

            ref Node fn = default;
            if (types.Haspointers(a.Type.Elem()))
            { 
                // memclrHasPointers(hp, hn)
                fn = mkcall("memclrHasPointers", null, null, hp, hn);
            }
            else
            { 
                // memclrNoHeapPointers(hp, hn)
                fn = mkcall("memclrNoHeapPointers", null, null, hp, hn);
            }
            n.Nbody.Append(fn); 

            // i = len(a) - 1
            v1 = nod(OAS, v1, nod(OSUB, nod(OLEN, a, null), nodintconst(1L)));

            n.Nbody.Append(v1);

            n.Left = typecheck(n.Left, Erv);
            typecheckslice(n.Nbody.Slice(), Etop);
            n = walkstmt(n);
            return true;
        }
    }
}}}}
