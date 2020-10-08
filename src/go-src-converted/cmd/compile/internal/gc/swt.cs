// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:31:17 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\swt.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // typecheckswitch typechecks a switch statement.
        private static void typecheckswitch(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            typecheckslice(n.Ninit.Slice(), ctxStmt);
            if (n.Left != null && n.Left.Op == OTYPESW)
            {
                typecheckTypeSwitch(_addr_n);
            }
            else
            {
                typecheckExprSwitch(_addr_n);
            }
        }

        private static void typecheckTypeSwitch(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            n.Left.Right = typecheck(n.Left.Right, ctxExpr);
            var t = n.Left.Right.Type;
            if (t != null && !t.IsInterface())
            {
                yyerrorl(n.Pos, "cannot type switch on non-interface value %L", n.Left.Right);
                t = null;
            } 

            // We don't actually declare the type switch's guarded
            // declaration itself. So if there are no cases, we won't
            // notice that it went unused.
            {
                var v = n.Left.Left;

                if (v != null && !v.isBlank() && n.List.Len() == 0L)
                {
                    yyerrorl(v.Pos, "%v declared but not used", v.Sym);
                }

            }


            ptr<Node> defCase;            ptr<Node> nilCase;

            typeSet ts = default;
            foreach (var (_, ncase) in n.List.Slice())
            {
                var ls = ncase.List.Slice();
                if (len(ls) == 0L)
                { // default:
                    if (defCase != null)
                    {
                        yyerrorl(ncase.Pos, "multiple defaults in switch (first at %v)", defCase.Line());
                    }
                    else
                    {
                        defCase = ncase;
                    }

                }

                foreach (var (i) in ls)
                {
                    ls[i] = typecheck(ls[i], ctxExpr | ctxType);
                    var n1 = ls[i];
                    if (t == null || n1.Type == null)
                    {
                        continue;
                    }

                    ptr<types.Field> missing;                    ptr<types.Field> have;

                    ref long ptr = ref heap(out ptr<long> _addr_ptr);

                    if (n1.isNil()) // case nil:
                        if (nilCase != null)
                        {
                            yyerrorl(ncase.Pos, "multiple nil cases in type switch (first at %v)", nilCase.Line());
                        }
                        else
                        {
                            nilCase = ncase;
                        }

                    else if (n1.Op != OTYPE) 
                        yyerrorl(ncase.Pos, "%L is not a type", n1);
                    else if (!n1.Type.IsInterface() && !implements(n1.Type, t, _addr_missing, _addr_have, _addr_ptr) && !missing.Broke()) 
                        if (have != null && !have.Broke())
                        {
                            yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (wrong type for %v method)\n\thave %v%S\n\twant %v%S", n.Left.Right, n1.Type, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                        }
                        else if (ptr != 0L)
                        {
                            yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (%v method has pointer receiver)", n.Left.Right, n1.Type, missing.Sym);
                        }
                        else
                        {
                            yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (missing %v method)", n.Left.Right, n1.Type, missing.Sym);
                        }

                                        if (n1.Op == OTYPE)
                    {
                        ts.add(ncase.Pos, n1.Type);
                    }

                }
                if (ncase.Rlist.Len() != 0L)
                { 
                    // Assign the clause variable's type.
                    var vt = t;
                    if (len(ls) == 1L)
                    {
                        if (ls[0L].Op == OTYPE)
                        {
                            vt = ls[0L].Type;
                        }
                        else if (ls[0L].Op != OLITERAL)
                        { // TODO(mdempsky): Should be !ls[0].isNil()
                            // Invalid single-type case;
                            // mark variable as broken.
                            vt = null;

                        }

                    } 

                    // TODO(mdempsky): It should be possible to
                    // still typecheck the case body.
                    if (vt == null)
                    {
                        continue;
                    }

                    var nvar = ncase.Rlist.First();
                    nvar.Type = vt;
                    nvar = typecheck(nvar, ctxExpr | ctxAssign);
                    ncase.Rlist.SetFirst(nvar);

                }

                typecheckslice(ncase.Nbody.Slice(), ctxStmt);

            }

        }

        private partial struct typeSet
        {
            public map<@string, slice<typeSetEntry>> m;
        }

        private partial struct typeSetEntry
        {
            public src.XPos pos;
            public ptr<types.Type> typ;
        }

        private static void add(this ptr<typeSet> _addr_s, src.XPos pos, ptr<types.Type> _addr_typ)
        {
            ref typeSet s = ref _addr_s.val;
            ref types.Type typ = ref _addr_typ.val;

            if (s.m == null)
            {
                s.m = make_map<@string, slice<typeSetEntry>>();
            } 

            // LongString does not uniquely identify types, so we need to
            // disambiguate collisions with types.Identical.
            // TODO(mdempsky): Add a method that *is* unique.
            var ls = typ.LongString();
            var prevs = s.m[ls];
            foreach (var (_, prev) in prevs)
            {
                if (types.Identical(typ, prev.typ))
                {
                    yyerrorl(pos, "duplicate case %v in type switch\n\tprevious case at %s", typ, linestr(prev.pos));
                    return ;
                }

            }
            s.m[ls] = append(prevs, new typeSetEntry(pos,typ));

        }

        private static void typecheckExprSwitch(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var t = types.Types[TBOOL];
            if (n.Left != null)
            {
                n.Left = typecheck(n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                t = n.Left.Type;
            }

            @string nilonly = default;
            if (t != null)
            {

                if (t.IsMap()) 
                    nilonly = "map";
                else if (t.Etype == TFUNC) 
                    nilonly = "func";
                else if (t.IsSlice()) 
                    nilonly = "slice";
                else if (!IsComparable(t)) 
                    if (t.IsStruct())
                    {
                        yyerrorl(n.Pos, "cannot switch on %L (struct containing %v cannot be compared)", n.Left, IncomparableField(t).Type);
                    }
                    else
                    {
                        yyerrorl(n.Pos, "cannot switch on %L", n.Left);
                    }

                    t = null;
                
            }

            ptr<Node> defCase;
            constSet cs = default;
            foreach (var (_, ncase) in n.List.Slice())
            {
                var ls = ncase.List.Slice();
                if (len(ls) == 0L)
                { // default:
                    if (defCase != null)
                    {
                        yyerrorl(ncase.Pos, "multiple defaults in switch (first at %v)", defCase.Line());
                    }
                    else
                    {
                        defCase = ncase;
                    }

                }

                foreach (var (i) in ls)
                {
                    setlineno(ncase);
                    ls[i] = typecheck(ls[i], ctxExpr);
                    ls[i] = defaultlit(ls[i], t);
                    var n1 = ls[i];
                    if (t == null || n1.Type == null)
                    {
                        continue;
                    }


                    if (nilonly != "" && !n1.isNil()) 
                        yyerrorl(ncase.Pos, "invalid case %v in switch (can only compare %s %v to nil)", n1, nilonly, n.Left);
                    else if (t.IsInterface() && !n1.Type.IsInterface() && !IsComparable(n1.Type)) 
                        yyerrorl(ncase.Pos, "invalid case %L in switch (incomparable type)", n1);
                    else if (assignop(n1.Type, t, null) == 0L && assignop(t, n1.Type, null) == 0L) 
                        if (n.Left != null)
                        {
                            yyerrorl(ncase.Pos, "invalid case %v in switch on %v (mismatched types %v and %v)", n1, n.Left, n1.Type, t);
                        }
                        else
                        {
                            yyerrorl(ncase.Pos, "invalid case %v in switch (mismatched types %v and bool)", n1, n1.Type);
                        }

                    // Don't check for duplicate bools. Although the spec allows it,
                    // (1) the compiler hasn't checked it in the past, so compatibility mandates it, and
                    // (2) it would disallow useful things like
                    //       case GOARCH == "arm" && GOARM == "5":
                    //       case GOARCH == "arm":
                    //     which would both evaluate to false for non-ARM compiles.
                    if (!n1.Type.IsBoolean())
                    {
                        cs.add(ncase.Pos, n1, "case", "switch");
                    }

                }
                typecheckslice(ncase.Nbody.Slice(), ctxStmt);

            }

        }

        // walkswitch walks a switch statement.
        private static void walkswitch(ptr<Node> _addr_sw)
        {
            ref Node sw = ref _addr_sw.val;
 
            // Guard against double walk, see #25776.
            if (sw.List.Len() == 0L && sw.Nbody.Len() > 0L)
            {
                return ; // Was fatal, but eliminating every possible source of double-walking is hard
            }

            if (sw.Left != null && sw.Left.Op == OTYPESW)
            {
                walkTypeSwitch(_addr_sw);
            }
            else
            {
                walkExprSwitch(_addr_sw);
            }

        }

        // walkExprSwitch generates an AST implementing sw.  sw is an
        // expression switch.
        private static void walkExprSwitch(ptr<Node> _addr_sw)
        {
            ref Node sw = ref _addr_sw.val;

            var lno = setlineno(sw);

            var cond = sw.Left;
            sw.Left = null; 

            // convert switch {...} to switch true {...}
            if (cond == null)
            {
                cond = nodbool(true);
                cond = typecheck(cond, ctxExpr);
                cond = defaultlit(cond, null);
            } 

            // Given "switch string(byteslice)",
            // with all cases being side-effect free,
            // use a zero-cost alias of the byte slice.
            // Do this before calling walkexpr on cond,
            // because walkexpr will lower the string
            // conversion into a runtime call.
            // See issue 24937 for more discussion.
            if (cond.Op == OBYTES2STR && allCaseExprsAreSideEffectFree(_addr_sw))
            {
                cond.Op = OBYTES2STRTMP;
            }

            cond = walkexpr(cond, _addr_sw.Ninit);
            if (cond.Op != OLITERAL)
            {
                cond = copyexpr(cond, cond.Type, _addr_sw.Nbody);
            }

            lineno = lno;

            exprSwitch s = new exprSwitch(exprname:cond,);

            ptr<Node> defaultGoto;
            ref Nodes body = ref heap(out ptr<Nodes> _addr_body);
            foreach (var (_, ncase) in sw.List.Slice())
            {
                var label = autolabel(".s");
                var jmp = npos(ncase.Pos, nodSym(OGOTO, null, label)); 

                // Process case dispatch.
                if (ncase.List.Len() == 0L)
                {
                    if (defaultGoto != null)
                    {
                        Fatalf("duplicate default case not detected during typechecking");
                    }

                    defaultGoto = jmp;

                }

                foreach (var (_, n1) in ncase.List.Slice())
                {
                    s.Add(ncase.Pos, n1, jmp);
                } 

                // Process body.
                body.Append(npos(ncase.Pos, nodSym(OLABEL, null, label)));
                body.Append(ncase.Nbody.Slice());
                {
                    var (fall, pos) = hasFall(ncase.Nbody.Slice());

                    if (!fall)
                    {
                        var br = nod(OBREAK, null, null);
                        br.Pos = pos;
                        body.Append(br);
                    }

                }

            }
            sw.List.Set(null);

            if (defaultGoto == null)
            {
                br = nod(OBREAK, null, null);
                br.Pos = br.Pos.WithNotStmt();
                defaultGoto = br;
            }

            s.Emit(_addr_sw.Nbody);
            sw.Nbody.Append(defaultGoto);
            sw.Nbody.AppendNodes(_addr_body);
            walkstmtlist(sw.Nbody.Slice());

        }

        // An exprSwitch walks an expression switch.
        private partial struct exprSwitch
        {
            public ptr<Node> exprname; // value being switched on

            public Nodes done;
            public slice<exprClause> clauses;
        }

        private partial struct exprClause
        {
            public src.XPos pos;
            public ptr<Node> lo;
            public ptr<Node> hi;
            public ptr<Node> jmp;
        }

        private static void Add(this ptr<exprSwitch> _addr_s, src.XPos pos, ptr<Node> _addr_expr, ptr<Node> _addr_jmp)
        {
            ref exprSwitch s = ref _addr_s.val;
            ref Node expr = ref _addr_expr.val;
            ref Node jmp = ref _addr_jmp.val;

            exprClause c = new exprClause(pos:pos,lo:expr,hi:expr,jmp:jmp);
            if (okforcmp[s.exprname.Type.Etype] && expr.Op == OLITERAL)
            {
                s.clauses = append(s.clauses, c);
                return ;
            }

            s.flush();
            s.clauses = append(s.clauses, c);
            s.flush();

        }

        private static void Emit(this ptr<exprSwitch> _addr_s, ptr<Nodes> _addr_@out)
        {
            ref exprSwitch s = ref _addr_s.val;
            ref Nodes @out = ref _addr_@out.val;

            s.flush();
            @out.AppendNodes(_addr_s.done);
        }

        private static void flush(this ptr<exprSwitch> _addr_s)
        {
            ref exprSwitch s = ref _addr_s.val;

            var cc = s.clauses;
            s.clauses = null;
            if (len(cc) == 0L)
            {
                return ;
            } 

            // Caution: If len(cc) == 1, then cc[0] might not an OLITERAL.
            // The code below is structured to implicitly handle this case
            // (e.g., sort.Slice doesn't need to invoke the less function
            // when there's only a single slice element).
            if (s.exprname.Type.IsString() && len(cc) >= 2L)
            { 
                // Sort strings by length and then by value. It is
                // much cheaper to compare lengths than values, and
                // all we need here is consistency. We respect this
                // sorting below.
                sort.Slice(cc, (i, j) =>
                {
                    var si = strlit(cc[i].lo);
                    var sj = strlit(cc[j].lo);
                    if (len(si) != len(sj))
                    {
                        return len(si) < len(sj);
                    }

                    return si < sj;

                }); 

                // runLen returns the string length associated with a
                // particular run of exprClauses.
                Func<slice<exprClause>, long> runLen = run => int64(len(strlit(run[0L].lo)));
                } 

                // Collapse runs of consecutive strings with the same length.; 

                // Collapse runs of consecutive strings with the same length.
                slice<slice<exprClause>> runs = default;
                long start = 0L;
                for (long i = 1L; i < len(cc); i++)
                {
                    if (runLen(cc[start..]) != runLen(cc[i..]))
                    {
                        runs = append(runs, cc[start..i]);
                        start = i;
                    }

                }

                runs = append(runs, cc[start..]); 

                // Perform two-level binary search.
                var nlen = nod(OLEN, s.exprname, null);
                binarySearch(len(runs), _addr_s.done, i =>
                {
                    return nod(OLE, nlen, nodintconst(runLen(runs[i - 1L])));
                }, (i, nif) =>
                {
                    var run = runs[i];
                    nif.Left = nod(OEQ, nlen, nodintconst(runLen(run)));
                    s.search(run, _addr_nif.Nbody);
                });
                return ;

            }

            sort.Slice(cc, (i, j) =>
            {
                return compareOp(cc[i].lo.Val(), OLT, cc[j].lo.Val());
            }); 

            // Merge consecutive integer cases.
            if (s.exprname.Type.IsInteger())
            {
                var merged = cc[..1L];
                foreach (var (_, c) in cc[1L..])
                {
                    var last = _addr_merged[len(merged) - 1L];
                    if (last.jmp == c.jmp && last.hi.Int64() + 1L == c.lo.Int64())
                    {
                        last.hi = c.lo;
                    }
                    else
                    {
                        merged = append(merged, c);
                    }

                }
                cc = merged;

            }

            s.search(cc, _addr_s.done);

        }

        private static void search(this ptr<exprSwitch> _addr_s, slice<exprClause> cc, ptr<Nodes> _addr_@out)
        {
            ref exprSwitch s = ref _addr_s.val;
            ref Nodes @out = ref _addr_@out.val;

            binarySearch(len(cc), _addr_out, i =>
            {
                return nod(OLE, s.exprname, cc[i - 1L].hi);
            }, (i, nif) =>
            {
                var c = _addr_cc[i];
                nif.Left = c.test(s.exprname);
                nif.Nbody.Set1(c.jmp);
            });

        }

        private static ptr<Node> test(this ptr<exprClause> _addr_c, ptr<Node> _addr_exprname)
        {
            ref exprClause c = ref _addr_c.val;
            ref Node exprname = ref _addr_exprname.val;
 
            // Integer range.
            if (c.hi != c.lo)
            {
                var low = nodl(c.pos, OGE, exprname, c.lo);
                var high = nodl(c.pos, OLE, exprname, c.hi);
                return _addr_nodl(c.pos, OANDAND, low, high)!;
            } 

            // Optimize "switch true { ...}" and "switch false { ... }".
            if (Isconst(exprname, CTBOOL) && !c.lo.Type.IsInterface())
            {
                if (exprname.Val().U._<bool>())
                {
                    return _addr_c.lo!;
                }
                else
                {
                    return _addr_nodl(c.pos, ONOT, c.lo, null)!;
                }

            }

            return _addr_nodl(c.pos, OEQ, exprname, c.lo)!;

        }

        private static bool allCaseExprsAreSideEffectFree(ptr<Node> _addr_sw)
        {
            ref Node sw = ref _addr_sw.val;
 
            // In theory, we could be more aggressive, allowing any
            // side-effect-free expressions in cases, but it's a bit
            // tricky because some of that information is unavailable due
            // to the introduction of temporaries during order.
            // Restricting to constants is simple and probably powerful
            // enough.

            foreach (var (_, ncase) in sw.List.Slice())
            {
                if (ncase.Op != OCASE)
                {
                    Fatalf("switch string(byteslice) bad op: %v", ncase.Op);
                }

                foreach (var (_, v) in ncase.List.Slice())
                {
                    if (v.Op != OLITERAL)
                    {
                        return false;
                    }

                }

            }
            return true;

        }

        // hasFall reports whether stmts ends with a "fallthrough" statement.
        private static (bool, src.XPos) hasFall(slice<ptr<Node>> stmts)
        {
            bool _p0 = default;
            src.XPos _p0 = default;
 
            // Search backwards for the index of the fallthrough
            // statement. Do not assume it'll be in the last
            // position, since in some cases (e.g. when the statement
            // list contains autotmp_ variables), one or more OVARKILL
            // nodes will be at the end of the list.

            var i = len(stmts) - 1L;
            while (i >= 0L && stmts[i].Op == OVARKILL)
            {
                i--;
            }

            if (i < 0L)
            {
                return (false, src.NoXPos);
            }

            return (stmts[i].Op == OFALL, stmts[i].Pos);

        }

        // walkTypeSwitch generates an AST that implements sw, where sw is a
        // type switch.
        private static void walkTypeSwitch(ptr<Node> _addr_sw)
        {
            ref Node sw = ref _addr_sw.val;

            typeSwitch s = default;
            s.facename = sw.Left.Right;
            sw.Left = null;

            s.facename = walkexpr(s.facename, _addr_sw.Ninit);
            s.facename = copyexpr(s.facename, s.facename.Type, _addr_sw.Nbody);
            s.okname = temp(types.Types[TBOOL]); 

            // Get interface descriptor word.
            // For empty interfaces this will be the type.
            // For non-empty interfaces this will be the itab.
            var itab = nod(OITAB, s.facename, null); 

            // For empty interfaces, do:
            //     if e._type == nil {
            //         do nil case if it exists, otherwise default
            //     }
            //     h := e._type.hash
            // Use a similar strategy for non-empty interfaces.
            var ifNil = nod(OIF, null, null);
            ifNil.Left = nod(OEQ, itab, nodnil());
            lineno = lineno.WithNotStmt(); // disable statement marks after the first check.
            ifNil.Left = typecheck(ifNil.Left, ctxExpr);
            ifNil.Left = defaultlit(ifNil.Left, null); 
            // ifNil.Nbody assigned at end.
            sw.Nbody.Append(ifNil); 

            // Load hash from type or itab.
            var dotHash = nodSym(ODOTPTR, itab, null);
            dotHash.Type = types.Types[TUINT32];
            dotHash.SetTypecheck(1L);
            if (s.facename.Type.IsEmptyInterface())
            {
                dotHash.Xoffset = int64(2L * Widthptr); // offset of hash in runtime._type
            }
            else
            {
                dotHash.Xoffset = int64(2L * Widthptr); // offset of hash in runtime.itab
            }

            dotHash.SetBounded(true); // guaranteed not to fault
            s.hashname = copyexpr(dotHash, dotHash.Type, _addr_sw.Nbody);

            var br = nod(OBREAK, null, null);
            ptr<Node> defaultGoto;            ptr<Node> nilGoto;

            ref Nodes body = ref heap(out ptr<Nodes> _addr_body);
            foreach (var (_, ncase) in sw.List.Slice())
            {
                ptr<Node> caseVar;
                if (ncase.Rlist.Len() != 0L)
                {
                    caseVar = ncase.Rlist.First();
                } 

                // For single-type cases with an interface type,
                // we initialize the case variable as part of the type assertion.
                // In other cases, we initialize it in the body.
                ptr<types.Type> singleType;
                if (ncase.List.Len() == 1L && ncase.List.First().Op == OTYPE)
                {
                    singleType = ncase.List.First().Type;
                }

                var caseVarInitialized = false;

                var label = autolabel(".s");
                var jmp = npos(ncase.Pos, nodSym(OGOTO, null, label));

                if (ncase.List.Len() == 0L)
                { // default:
                    if (defaultGoto != null)
                    {
                        Fatalf("duplicate default case not detected during typechecking");
                    }

                    defaultGoto = jmp;

                }

                foreach (var (_, n1) in ncase.List.Slice())
                {
                    if (n1.isNil())
                    { // case nil:
                        if (nilGoto != null)
                        {
                            Fatalf("duplicate nil case not detected during typechecking");
                        }

                        nilGoto = jmp;
                        continue;

                    }

                    if (singleType != null && singleType.IsInterface())
                    {
                        s.Add(ncase.Pos, n1.Type, caseVar, jmp);
                        caseVarInitialized = true;
                    }
                    else
                    {
                        s.Add(ncase.Pos, n1.Type, null, jmp);
                    }

                }
                body.Append(npos(ncase.Pos, nodSym(OLABEL, null, label)));
                if (caseVar != null && !caseVarInitialized)
                {
                    var val = s.facename;
                    if (singleType != null)
                    { 
                        // We have a single concrete type. Extract the data.
                        if (singleType.IsInterface())
                        {
                            Fatalf("singleType interface should have been handled in Add");
                        }

                        val = ifaceData(ncase.Pos, s.facename, singleType);

                    }

                    ptr<Node> l = new slice<ptr<Node>>(new ptr<Node>[] { nodl(ncase.Pos,ODCL,caseVar,nil), nodl(ncase.Pos,OAS,caseVar,val) });
                    typecheckslice(l, ctxStmt);
                    body.Append(l);

                }

                body.Append(ncase.Nbody.Slice());
                body.Append(br);

            }
            sw.List.Set(null);

            if (defaultGoto == null)
            {
                defaultGoto = br;
            }

            if (nilGoto == null)
            {
                nilGoto = addr(defaultGoto);
            }

            ifNil.Nbody.Set1(nilGoto);

            s.Emit(_addr_sw.Nbody);
            sw.Nbody.Append(defaultGoto);
            sw.Nbody.AppendNodes(_addr_body);

            walkstmtlist(sw.Nbody.Slice());

        }

        // A typeSwitch walks a type switch.
        private partial struct typeSwitch
        {
            public ptr<Node> facename; // value being type-switched on
            public ptr<Node> hashname; // type hash of the value being type-switched on
            public ptr<Node> okname; // boolean used for comma-ok type assertions

            public Nodes done;
            public slice<typeClause> clauses;
        }

        private partial struct typeClause
        {
            public uint hash;
            public Nodes body;
        }

        private static void Add(this ptr<typeSwitch> _addr_s, src.XPos pos, ptr<types.Type> _addr_typ, ptr<Node> _addr_caseVar, ptr<Node> _addr_jmp)
        {
            ref typeSwitch s = ref _addr_s.val;
            ref types.Type typ = ref _addr_typ.val;
            ref Node caseVar = ref _addr_caseVar.val;
            ref Node jmp = ref _addr_jmp.val;

            ref Nodes body = ref heap(out ptr<Nodes> _addr_body);
            if (caseVar != null)
            {
                ptr<Node> l = new slice<ptr<Node>>(new ptr<Node>[] { nodl(pos,ODCL,caseVar,nil), nodl(pos,OAS,caseVar,nil) });
                typecheckslice(l, ctxStmt);
                body.Append(l);
            }
            else
            {
                caseVar = nblank;
            } 

            // cv, ok = iface.(type)
            var @as = nodl(pos, OAS2, null, null);
            @as.List.Set2(caseVar, s.okname); // cv, ok =
            var dot = nodl(pos, ODOTTYPE, s.facename, null);
            dot.Type = typ; // iface.(type)
            @as.Rlist.Set1(dot);
            as = typecheck(as, ctxStmt);
            as = walkexpr(as, _addr_body);
            body.Append(as); 

            // if ok { goto label }
            var nif = nodl(pos, OIF, null, null);
            nif.Left = s.okname;
            nif.Nbody.Set1(jmp);
            body.Append(nif);

            if (!typ.IsInterface())
            {
                s.clauses = append(s.clauses, new typeClause(hash:typehash(typ),body:body,));
                return ;
            }

            s.flush();
            s.done.AppendNodes(_addr_body);

        }

        private static void Emit(this ptr<typeSwitch> _addr_s, ptr<Nodes> _addr_@out)
        {
            ref typeSwitch s = ref _addr_s.val;
            ref Nodes @out = ref _addr_@out.val;

            s.flush();
            @out.AppendNodes(_addr_s.done);
        }

        private static void flush(this ptr<typeSwitch> _addr_s)
        {
            ref typeSwitch s = ref _addr_s.val;

            var cc = s.clauses;
            s.clauses = null;
            if (len(cc) == 0L)
            {
                return ;
            }

            sort.Slice(cc, (i, j) => cc[i].hash < cc[j].hash); 

            // Combine adjacent cases with the same hash.
            var merged = cc[..1L];
            {
                var c__prev1 = c;

                foreach (var (_, __c) in cc[1L..])
                {
                    c = __c;
                    var last = _addr_merged[len(merged) - 1L];
                    if (last.hash == c.hash)
                    {
                        last.body.AppendNodes(_addr_c.body);
                    }
                    else
                    {
                        merged = append(merged, c);
                    }

                }

                c = c__prev1;
            }

            cc = merged;

            binarySearch(len(cc), _addr_s.done, i =>
            {
                return nod(OLE, s.hashname, nodintconst(int64(cc[i - 1L].hash)));
            }, (i, nif) =>
            { 
                // TODO(mdempsky): Omit hash equality check if
                // there's only one type.
                var c = cc[i];
                nif.Left = nod(OEQ, s.hashname, nodintconst(int64(c.hash)));
                nif.Nbody.AppendNodes(_addr_c.body);

            });

        }

        // binarySearch constructs a binary search tree for handling n cases,
        // and appends it to out. It's used for efficiently implementing
        // switch statements.
        //
        // less(i) should return a boolean expression. If it evaluates true,
        // then cases before i will be tested; otherwise, cases i and later.
        //
        // base(i, nif) should setup nif (an OIF node) to test case i. In
        // particular, it should set nif.Left and nif.Nbody.
        private static void binarySearch(long n, ptr<Nodes> _addr_@out, Func<long, ptr<Node>> less, Action<long, ptr<Node>> @base)
        {
            ref Nodes @out = ref _addr_@out.val;

            const long binarySearchMin = (long)4L; // minimum number of cases for binary search

 // minimum number of cases for binary search

            Action<long, long, ptr<Nodes>> @do = default;
            do = (lo, hi, @out) =>
            {
                var n = hi - lo;
                if (n < binarySearchMin)
                {
                    for (var i = lo; i < hi; i++)
                    {
                        var nif = nod(OIF, null, null);
                        base(i, nif);
                        lineno = lineno.WithNotStmt();
                        nif.Left = typecheck(nif.Left, ctxExpr);
                        nif.Left = defaultlit(nif.Left, null);
                        @out.Append(nif);
                        out = _addr_nif.Rlist;
                    }

                    return ;

                }

                var half = lo + n / 2L;
                nif = nod(OIF, null, null);
                nif.Left = less(half);
                lineno = lineno.WithNotStmt();
                nif.Left = typecheck(nif.Left, ctxExpr);
                nif.Left = defaultlit(nif.Left, null);
                do(lo, half, _addr_nif.Nbody);
                do(half, hi, _addr_nif.Rlist);
                @out.Append(nif);

            }
;

            do(0L, n, out);

        }
    }
}}}}
