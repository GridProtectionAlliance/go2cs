// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:24 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\swt.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
 
        // expression switch
        private static readonly var switchKindExpr = iota; // switch a {...} or switch 5 {...}
        private static readonly var switchKindTrue = 0; // switch true {...} or switch {...}
        private static readonly var switchKindFalse = 1; // switch false {...}

        private static readonly long binarySearchMin = 4L; // minimum number of cases for binary search
        private static readonly long integerRangeMin = 2L; // minimum size of integer ranges

        // An exprSwitch walks an expression switch.
        private partial struct exprSwitch
        {
            public ptr<Node> exprname; // node for the expression being switched on
            public long kind; // kind of switch statement (switchKind*)
        }

        // A typeSwitch walks a type switch.
        private partial struct typeSwitch
        {
            public ptr<Node> hashname; // node for the hash of the type of the variable being switched on
            public ptr<Node> facename; // node for the concrete type of the variable being switched on
            public ptr<Node> okname; // boolean node used for comma-ok type assertions
        }

        // A caseClause is a single case clause in a switch statement.
        private partial struct caseClause
        {
            public ptr<Node> node; // points at case statement
            public long ordinal; // position in switch
            public uint hash; // hash of a type switch
// isconst indicates whether this case clause is a constant,
// for the purposes of the switch code generation.
// For expression switches, that's generally literals (case 5:, not case x:).
// For type switches, that's concrete types (case time.Time:), not interfaces (case io.Reader:).
            public bool isconst;
        }

        // caseClauses are all the case clauses in a switch statement.
        private partial struct caseClauses
        {
            public slice<caseClause> list; // general cases
            public ptr<Node> defjmp; // OGOTO for default case or OBREAK if no default case present
            public ptr<Node> niljmp; // OGOTO for nil type case in a type switch
        }

        // typecheckswitch typechecks a switch statement.
        private static void typecheckswitch(ref Node n)
        {
            typecheckslice(n.Ninit.Slice(), Etop);

            @string nilonly = default;
            long top = default;
            ref types.Type t = default;

            if (n.Left != null && n.Left.Op == OTYPESW)
            { 
                // type switch
                top = Etype;
                n.Left.Right = typecheck(n.Left.Right, Erv);
                t = n.Left.Right.Type;
                if (t != null && !t.IsInterface())
                {
                    yyerrorl(n.Pos, "cannot type switch on non-interface value %L", n.Left.Right);
                }
            }
            else
            { 
                // expression switch
                top = Erv;
                if (n.Left != null)
                {
                    n.Left = typecheck(n.Left, Erv);
                    n.Left = defaultlit(n.Left, null);
                    t = n.Left.Type;
                }
                else
                {
                    t = types.Types[TBOOL];
                }
                if (t != null)
                {

                    if (!okforeq[t.Etype]) 
                        yyerrorl(n.Pos, "cannot switch on %L", n.Left);
                    else if (t.IsSlice()) 
                        nilonly = "slice";
                    else if (t.IsArray() && !IsComparable(t)) 
                        yyerrorl(n.Pos, "cannot switch on %L", n.Left);
                    else if (t.IsStruct()) 
                        {
                            var f = IncomparableField(t);

                            if (f != null)
                            {
                                yyerrorl(n.Pos, "cannot switch on %L (struct containing %v cannot be compared)", n.Left, f.Type);
                            }

                        }
                    else if (t.Etype == TFUNC) 
                        nilonly = "func";
                    else if (t.IsMap()) 
                        nilonly = "map";
                                    }
            }
            n.Type = t;

            ref Node def = default;            ref Node niltype = default;

            foreach (var (_, ncase) in n.List.Slice())
            {
                if (ncase.List.Len() == 0L)
                { 
                    // default
                    if (def != null)
                    {
                        setlineno(ncase);
                        yyerrorl(ncase.Pos, "multiple defaults in switch (first at %v)", def.Line());
                    }
                    else
                    {
                        def = ncase;
                    }
                }
                else
                {
                    var ls = ncase.List.Slice();
                    foreach (var (i1, n1) in ls)
                    {
                        setlineno(n1);
                        ls[i1] = typecheck(ls[i1], Erv | Etype);
                        n1 = ls[i1];
                        if (n1.Type == null || t == null)
                        {
                            continue;
                        }
                        setlineno(ncase);

                        // expression switch
                        if (top == Erv) 
                            ls[i1] = defaultlit(ls[i1], t);
                            n1 = ls[i1];

                            if (n1.Op == OTYPE) 
                                yyerrorl(ncase.Pos, "type %v is not an expression", n1.Type);
                            else if (n1.Type != null && assignop(n1.Type, t, null) == 0L && assignop(t, n1.Type, null) == 0L) 
                                if (n.Left != null)
                                {
                                    yyerrorl(ncase.Pos, "invalid case %v in switch on %v (mismatched types %v and %v)", n1, n.Left, n1.Type, t);
                                }
                                else
                                {
                                    yyerrorl(ncase.Pos, "invalid case %v in switch (mismatched types %v and bool)", n1, n1.Type);
                                }
                            else if (nilonly != "" && !isnil(n1)) 
                                yyerrorl(ncase.Pos, "invalid case %v in switch (can only compare %s %v to nil)", n1, nilonly, n.Left);
                            else if (t.IsInterface() && !n1.Type.IsInterface() && !IsComparable(n1.Type)) 
                                yyerrorl(ncase.Pos, "invalid case %L in switch (incomparable type)", n1);
                            // type switch
                        else if (top == Etype) 
                            ref types.Field missing = default;                            ref types.Field have = default;

                            long ptr = default;

                            if (n1.Op == OLITERAL && n1.Type.IsKind(TNIL)) 
                                // case nil:
                                if (niltype != null)
                                {
                                    yyerrorl(ncase.Pos, "multiple nil cases in type switch (first at %v)", niltype.Line());
                                }
                                else
                                {
                                    niltype = ncase;
                                }
                            else if (n1.Op != OTYPE && n1.Type != null) // should this be ||?
                                yyerrorl(ncase.Pos, "%L is not a type", n1); 
                                // reset to original type
                                n1 = n.Left.Right;
                                ls[i1] = n1;
                            else if (!n1.Type.IsInterface() && t.IsInterface() && !implements(n1.Type, t, ref missing, ref have, ref ptr)) 
                                if (have != null && !missing.Broke() && !have.Broke())
                                {
                                    yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (wrong type for %v method)\n\thave %v%S\n\twant %v%S", n.Left.Right, n1.Type, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                                }
                                else if (!missing.Broke())
                                {
                                    if (ptr != 0L)
                                    {
                                        yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (%v method has pointer receiver)", n.Left.Right, n1.Type, missing.Sym);
                                    }
                                    else
                                    {
                                        yyerrorl(ncase.Pos, "impossible type switch case: %L cannot have dynamic type %v" + " (missing %v method)", n.Left.Right, n1.Type, missing.Sym);
                                    }
                                }
                                                                        }
                }
                if (n.Type == null || n.Type.IsUntyped())
                { 
                    // if the value we're switching on has no type or is untyped,
                    // we've already printed an error and don't need to continue
                    // typechecking the body
                    return;
                }
                if (top == Etype)
                {
                    var ll = ncase.List;
                    if (ncase.Rlist.Len() != 0L)
                    {
                        var nvar = ncase.Rlist.First();
                        if (ll.Len() == 1L && ll.First().Type != null && !ll.First().Type.IsKind(TNIL))
                        { 
                            // single entry type switch
                            nvar.Type = ll.First().Type;
                        }
                        else
                        { 
                            // multiple entry type switch or default
                            nvar.Type = n.Type;
                        }
                        nvar = typecheck(nvar, Erv | Easgn);
                        ncase.Rlist.SetFirst(nvar);
                    }
                }
                typecheckslice(ncase.Nbody.Slice(), Etop);
            }

            // expression switch
            if (top == Erv) 
                checkDupExprCases(n.Left, n.List.Slice());
                    }

        // walkswitch walks a switch statement.
        private static void walkswitch(ref Node sw)
        { 
            // convert switch {...} to switch true {...}
            if (sw.Left == null)
            {
                sw.Left = nodbool(true);
                sw.Left = typecheck(sw.Left, Erv);
            }
            if (sw.Left.Op == OTYPESW)
            {
                typeSwitch s = default;
                s.walk(sw);
            }
            else
            {
                s = default;
                s.walk(sw);
            }
        }

        // walk generates an AST implementing sw.
        // sw is an expression switch.
        // The AST is generally of the form of a linear
        // search using if..goto, although binary search
        // is used with long runs of constants.
        private static void walk(this ref exprSwitch s, ref Node sw)
        {
            casebody(sw, null);

            var cond = sw.Left;
            sw.Left = null;

            s.kind = switchKindExpr;
            if (Isconst(cond, CTBOOL))
            {
                s.kind = switchKindTrue;
                if (!cond.Val().U._<bool>())
                {
                    s.kind = switchKindFalse;
                }
            }
            cond = walkexpr(cond, ref sw.Ninit);
            var t = sw.Type;
            if (t == null)
            {
                return;
            } 

            // convert the switch into OIF statements
            slice<ref Node> cas = default;
            if (s.kind == switchKindTrue || s.kind == switchKindFalse)
            {
                s.exprname = nodbool(s.kind == switchKindTrue);
            }
            else if (consttype(cond) > 0L)
            { 
                // leave constants to enable dead code elimination (issue 9608)
                s.exprname = cond;
            }
            else
            {
                s.exprname = temp(cond.Type);
                cas = new slice<ref Node>(new ref Node[] { nod(OAS,s.exprname,cond) });
                typecheckslice(cas, Etop);
            } 

            // Enumerate the cases and prepare the default case.
            var clauses = s.genCaseClauses(sw.List.Slice());
            sw.List.Set(null);
            var cc = clauses.list; 

            // handle the cases in order
            while (len(cc) > 0L)
            {
                long run = 1L;
                if (okforcmp[t.Etype] && cc[0L].isconst)
                { 
                    // do binary search on runs of constants
                    while (run < len(cc) && cc[run].isconst)
                    {
                        run++;
                    } 
                    // sort and compile constants
 
                    // sort and compile constants
                    sort.Sort(caseClauseByConstVal(cc[..run]));
                }
                var a = s.walkCases(cc[..run]);
                cas = append(cas, a);
                cc = cc[run..];
            } 

            // handle default case
 

            // handle default case
            if (nerrors == 0L)
            {
                cas = append(cas, clauses.defjmp);
                sw.Nbody.Prepend(cas);
                walkstmtlist(sw.Nbody.Slice());
            }
        }

        // walkCases generates an AST implementing the cases in cc.
        private static ref Node walkCases(this ref exprSwitch s, slice<caseClause> cc)
        {
            if (len(cc) < binarySearchMin)
            { 
                // linear search
                slice<ref Node> cas = default;
                foreach (var (_, c) in cc)
                {
                    var n = c.node;
                    var lno = setlineno(n);

                    var a = nod(OIF, null, null);
                    {
                        var rng__prev2 = rng;

                        var rng = n.List.Slice();

                        if (rng != null)
                        { 
                            // Integer range.
                            // exprname is a temp or a constant,
                            // so it is safe to evaluate twice.
                            // In most cases, this conjunction will be
                            // rewritten by walkinrange into a single comparison.
                            var low = nod(OGE, s.exprname, rng[0L]);
                            var high = nod(OLE, s.exprname, rng[1L]);
                            a.Left = nod(OANDAND, low, high);
                            a.Left = typecheck(a.Left, Erv);
                            a.Left = walkexpr(a.Left, null); // give walk the opportunity to optimize the range check
                        }
                        else if ((s.kind != switchKindTrue && s.kind != switchKindFalse) || assignop(n.Left.Type, s.exprname.Type, null) == OCONVIFACE || assignop(s.exprname.Type, n.Left.Type, null) == OCONVIFACE)
                        {
                            a.Left = nod(OEQ, s.exprname, n.Left); // if name == val
                            a.Left = typecheck(a.Left, Erv);
                        }
                        else if (s.kind == switchKindTrue)
                        {
                            a.Left = n.Left; // if val
                        }
                        else
                        { 
                            // s.kind == switchKindFalse
                            a.Left = nod(ONOT, n.Left, null); // if !val
                            a.Left = typecheck(a.Left, Erv);
                        }

                        rng = rng__prev2;

                    }
                    a.Nbody.Set1(n.Right); // goto l

                    cas = append(cas, a);
                    lineno = lno;
                }
                return liststmt(cas);
            } 

            // find the middle and recur
            var half = len(cc) / 2L;
            a = nod(OIF, null, null);
            n = cc[half - 1L].node;
            ref Node mid = default;
            {
                var rng__prev1 = rng;

                rng = n.List.Slice();

                if (rng != null)
                {
                    mid = rng[1L]; // high end of range
                }
                else
                {
                    mid = n.Left;
                }

                rng = rng__prev1;

            }
            var le = nod(OLE, s.exprname, mid);
            if (Isconst(mid, CTSTR))
            { 
                // Search by length and then by value; see caseClauseByConstVal.
                var lenlt = nod(OLT, nod(OLEN, s.exprname, null), nod(OLEN, mid, null));
                var leneq = nod(OEQ, nod(OLEN, s.exprname, null), nod(OLEN, mid, null));
                a.Left = nod(OOROR, lenlt, nod(OANDAND, leneq, le));
            }
            else
            {
                a.Left = le;
            }
            a.Left = typecheck(a.Left, Erv);
            a.Nbody.Set1(s.walkCases(cc[..half]));
            a.Rlist.Set1(s.walkCases(cc[half..]));
            return a;
        }

        // casebody builds separate lists of statements and cases.
        // It makes labels between cases and statements
        // and deals with fallthrough, break, and unreachable statements.
        private static void casebody(ref Node sw, ref Node typeswvar)
        {
            if (sw.List.Len() == 0L)
            {
                return;
            }
            var lno = setlineno(sw);

            slice<ref Node> cas = default; // cases
            slice<ref Node> stat = default; // statements
            ref Node def = default; // defaults
            var br = nod(OBREAK, null, null);

            {
                var n__prev1 = n;

                foreach (var (_, __n) in sw.List.Slice())
                {
                    n = __n;
                    setlineno(n);
                    if (n.Op != OXCASE)
                    {
                        Fatalf("casebody %v", n.Op);
                    }
                    n.Op = OCASE;
                    var needvar = n.List.Len() != 1L || n.List.First().Op == OLITERAL;

                    var jmp = nod(OGOTO, autolabel(".s"), null);
                    switch (n.List.Len())
                    {
                        case 0L: 
                            // default
                            if (def != null)
                            {
                                yyerrorl(n.Pos, "more than one default case");
                            } 
                            // reuse original default case
                            n.Right = jmp;
                            def = n;
                            break;
                        case 1L: 
                            // one case -- reuse OCASE node
                            n.Left = n.List.First();
                            n.Right = jmp;
                            n.List.Set(null);
                            cas = append(cas, n);
                            break;
                        default: 
                            // Expand multi-valued cases and detect ranges of integer cases.
                            if (typeswvar != null || sw.Left.Type.IsInterface() || !n.List.First().Type.IsInteger() || n.List.Len() < integerRangeMin)
                            { 
                                // Can't use integer ranges. Expand each case into a separate node.
                                foreach (var (_, n1) in n.List.Slice())
                                {
                                    cas = append(cas, nod(OCASE, n1, jmp));
                                }
                                break;
                            } 
                            // Find integer ranges within runs of constants.
                            var s = n.List.Slice();
                            long j = 0L;
                            while (j < len(s))
                            { 
                                // Find a run of constants.
                                long run = default;
                                for (run = j; run < len(s) && Isconst(s[run], CTINT); run++)
                                {
                                }

                                if (run - j >= integerRangeMin)
                                { 
                                    // Search for integer ranges in s[j:run].
                                    // Typechecking is done, so all values are already in an appropriate range.
                                    var search = s[j..run];
                                    sort.Sort(constIntNodesByVal(search));
                                    for (long beg = 0L;
                                    long end = 1L; end <= len(search); end++)
                                    {
                                        if (end < len(search) && search[end].Int64() == search[end - 1L].Int64() + 1L)
                                        {
                                            continue;
                                        }
                                        if (end - beg >= integerRangeMin)
                                        { 
                                            // Record range in List.
                                            var c = nod(OCASE, null, jmp);
                                            c.List.Set2(search[beg], search[end - 1L]);
                                            cas = append(cas, c);
                                        }
                                        else
                                        { 
                                            // Not large enough for range; record separately.
                                            {
                                                var n__prev4 = n;

                                                foreach (var (_, __n) in search[beg..end])
                                                {
                                                    n = __n;
                                                    cas = append(cas, nod(OCASE, n, jmp));
                                                }

                                                n = n__prev4;
                                            }

                                        }
                                        beg = end;
                                    }

                                    j = run;
                                } 
                                // Advance to next constant, adding individual non-constant
                                // or as-yet-unhandled constant cases as we go.
                                while (j < len(s) && (j < run || !Isconst(s[j], CTINT)))
                                {
                                    cas = append(cas, nod(OCASE, s[j], jmp));
                                    j++;
                                }

                            }
                            break;
                    }

                    stat = append(stat, nod(OLABEL, jmp.Left, null));
                    if (typeswvar != null && needvar && n.Rlist.Len() != 0L)
                    {
                        ref Node l = new slice<ref Node>(new ref Node[] { nod(ODCL,n.Rlist.First(),nil), nod(OAS,n.Rlist.First(),typeswvar) });
                        typecheckslice(l, Etop);
                        stat = append(stat, l);
                    }
                    stat = append(stat, n.Nbody.Slice()); 

                    // Search backwards for the index of the fallthrough
                    // statement. Do not assume it'll be in the last
                    // position, since in some cases (e.g. when the statement
                    // list contains autotmp_ variables), one or more OVARKILL
                    // nodes will be at the end of the list.
                    var fallIndex = len(stat) - 1L;
                    while (stat[fallIndex].Op == OVARKILL)
                    {
                        fallIndex--;
                    }

                    var last = stat[fallIndex];
                    if (last.Op != OFALL)
                    {
                        stat = append(stat, br);
                    }
                }

                n = n__prev1;
            }

            stat = append(stat, br);
            if (def != null)
            {
                cas = append(cas, def);
            }
            sw.List.Set(cas);
            sw.Nbody.Set(stat);
            lineno = lno;
        }

        // genCaseClauses generates the caseClauses value for clauses.
        private static caseClauses genCaseClauses(this ref exprSwitch s, slice<ref Node> clauses)
        {
            caseClauses cc = default;
            foreach (var (_, n) in clauses)
            {
                if (n.Left == null && n.List.Len() == 0L)
                { 
                    // default case
                    if (cc.defjmp != null)
                    {
                        Fatalf("duplicate default case not detected during typechecking");
                    }
                    cc.defjmp = n.Right;
                    continue;
                }
                caseClause c = new caseClause(node:n,ordinal:len(cc.list));
                if (n.List.Len() > 0L)
                {
                    c.isconst = true;
                }

                if (consttype(n.Left) == CTFLT || consttype(n.Left) == CTINT || consttype(n.Left) == CTRUNE || consttype(n.Left) == CTSTR) 
                    c.isconst = true;
                                cc.list = append(cc.list, c);
            }
            if (cc.defjmp == null)
            {
                cc.defjmp = nod(OBREAK, null, null);
            }
            return cc;
        }

        // genCaseClauses generates the caseClauses value for clauses.
        private static caseClauses genCaseClauses(this ref typeSwitch s, slice<ref Node> clauses)
        {
            caseClauses cc = default;
            foreach (var (_, n) in clauses)
            {

                if (n.Left == null) 
                    // default case
                    if (cc.defjmp != null)
                    {
                        Fatalf("duplicate default case not detected during typechecking");
                    }
                    cc.defjmp = n.Right;
                    continue;
                else if (n.Left.Op == OLITERAL) 
                    // nil case in type switch
                    if (cc.niljmp != null)
                    {
                        Fatalf("duplicate nil case not detected during typechecking");
                    }
                    cc.niljmp = n.Right;
                    continue;
                // general case
                caseClause c = new caseClause(node:n,ordinal:len(cc.list),isconst:!n.Left.Type.IsInterface(),hash:typehash(n.Left.Type),);
                cc.list = append(cc.list, c);
            }
            if (cc.defjmp == null)
            {
                cc.defjmp = nod(OBREAK, null, null);
            } 

            // diagnose duplicate cases
            s.checkDupCases(cc.list);
            return cc;
        }

        private static void checkDupCases(this ref typeSwitch s, slice<caseClause> cc)
        {
            if (len(cc) < 2L)
            {
                return;
            } 
            // We store seen types in a map keyed by type hash.
            // It is possible, but very unlikely, for multiple distinct types to have the same hash.
            var seen = make_map<uint, slice<ref Node>>(); 
            // To avoid many small allocations of length 1 slices,
            // also set up a single large slice to slice into.
            var nn = make_slice<ref Node>(0L, len(cc));
Outer:
            foreach (var (_, c) in cc)
            {
                var (prev, ok) = seen[c.hash];
                if (!ok)
                { 
                    // First entry for this hash.
                    nn = append(nn, c.node);
                    seen[c.hash] = nn.slice(len(nn) - 1L, len(nn), len(nn));
                    continue;
                }
                foreach (var (_, n) in prev)
                {
                    if (eqtype(n.Left.Type, c.node.Left.Type))
                    {
                        yyerrorl(c.node.Pos, "duplicate case %v in type switch\n\tprevious case at %v", c.node.Left.Type, n.Line()); 
                        // avoid double-reporting errors
                        _continueOuter = true;
                        break;
                    }
                }
                seen[c.hash] = append(seen[c.hash], c.node);
            }
        }

        private static void checkDupExprCases(ref Node exprname, slice<ref Node> clauses)
        { 
            // boolean (naked) switch, nothing to do.
            if (exprname == null)
            {
                return;
            } 
            // The common case is that s's expression is not an interface.
            // In that case, all constant clauses have the same type,
            // so checking for duplicates can be done solely by value.
            if (!exprname.Type.IsInterface())
            {
                var seen = make();
                {
                    var ncase__prev1 = ncase;

                    foreach (var (_, __ncase) in clauses)
                    {
                        ncase = __ncase;
                        {
                            var n__prev2 = n;

                            foreach (var (_, __n) in ncase.List.Slice())
                            {
                                n = __n; 
                                // Can't check for duplicates that aren't constants, per the spec. Issue 15896.
                                // Don't check for duplicate bools. Although the spec allows it,
                                // (1) the compiler hasn't checked it in the past, so compatibility mandates it, and
                                // (2) it would disallow useful things like
                                //       case GOARCH == "arm" && GOARM == "5":
                                //       case GOARCH == "arm":
                                //     which would both evaluate to false for non-ARM compiles.
                                {
                                    var ct__prev2 = ct;

                                    var ct = consttype(n);

                                    if (ct == 0L || ct == CTBOOL)
                                    {
                                        continue;
                                    }

                                    ct = ct__prev2;

                                }

                                var val = n.Val().Interface();
                                var (prev, dup) = seen[val];
                                if (!dup)
                                {
                                    seen[val] = n;
                                    continue;
                                }
                                yyerrorl(ncase.Pos, "duplicate case %s in switch\n\tprevious case at %v", nodeAndVal(n), prev.Line());
                            }

                            n = n__prev2;
                        }

                    }

                    ncase = ncase__prev1;
                }

                return;
            } 
            // s's expression is an interface. This is fairly rare, so keep this simple.
            // Duplicates are only duplicates if they have the same type and the same value.
            private partial struct typeVal
            {
                public @string typ;
            }
            seen = make_map<typeVal, ref Node>();
            {
                var ncase__prev1 = ncase;

                foreach (var (_, __ncase) in clauses)
                {
                    ncase = __ncase;
                    {
                        var n__prev2 = n;

                        foreach (var (_, __n) in ncase.List.Slice())
                        {
                            n = __n;
                            {
                                var ct__prev1 = ct;

                                ct = consttype(n);

                                if (ct == 0L || ct == CTBOOL)
                                {
                                    continue;
                                }

                                ct = ct__prev1;

                            }
                            typeVal tv = new typeVal(typ:n.Type.LongString(),val:n.Val().Interface(),);
                            (prev, dup) = seen[tv];
                            if (!dup)
                            {
                                seen[tv] = n;
                                continue;
                            }
                            yyerrorl(ncase.Pos, "duplicate case %s in switch\n\tprevious case at %v", nodeAndVal(n), prev.Line());
                        }

                        n = n__prev2;
                    }

                }

                ncase = ncase__prev1;
            }

        }

        private static @string nodeAndVal(ref Node n)
        {
            var show = n.String();
            var val = n.Val().Interface();
            {
                var s = fmt.Sprintf("%#v", val);

                if (show != s)
                {
                    show += " (value " + s + ")";
                }

            }
            return show;
        }

        // walk generates an AST that implements sw,
        // where sw is a type switch.
        // The AST is generally of the form of a linear
        // search using if..goto, although binary search
        // is used with long runs of concrete types.
        private static void walk(this ref typeSwitch s, ref Node sw)
        {
            var cond = sw.Left;
            sw.Left = null;

            if (cond == null)
            {
                sw.List.Set(null);
                return;
            }
            if (cond.Right == null)
            {
                yyerrorl(sw.Pos, "type switch must have an assignment");
                return;
            }
            cond.Right = walkexpr(cond.Right, ref sw.Ninit);
            if (!cond.Right.Type.IsInterface())
            {
                yyerrorl(sw.Pos, "type switch must be on an interface");
                return;
            }
            slice<ref Node> cas = default; 

            // predeclare temporary variables and the boolean var
            s.facename = temp(cond.Right.Type);

            var a = nod(OAS, s.facename, cond.Right);
            a = typecheck(a, Etop);
            cas = append(cas, a);

            s.okname = temp(types.Types[TBOOL]);
            s.okname = typecheck(s.okname, Erv);

            s.hashname = temp(types.Types[TUINT32]);
            s.hashname = typecheck(s.hashname, Erv); 

            // set up labels and jumps
            casebody(sw, s.facename);

            var clauses = s.genCaseClauses(sw.List.Slice());
            sw.List.Set(null);
            var def = clauses.defjmp; 

            // For empty interfaces, do:
            //     if e._type == nil {
            //         do nil case if it exists, otherwise default
            //     }
            //     h := e._type.hash
            // Use a similar strategy for non-empty interfaces.

            // Get interface descriptor word.
            // For empty interfaces this will be the type.
            // For non-empty interfaces this will be the itab.
            var itab = nod(OITAB, s.facename, null); 

            // Check for nil first.
            var i = nod(OIF, null, null);
            i.Left = nod(OEQ, itab, nodnil());
            if (clauses.niljmp != null)
            { 
                // Do explicit nil case right here.
                i.Nbody.Set1(clauses.niljmp);
            }
            else
            { 
                // Jump to default case.
                var lbl = autolabel(".s");
                i.Nbody.Set1(nod(OGOTO, lbl, null)); 
                // Wrap default case with label.
                var blk = nod(OBLOCK, null, null);
                blk.List.Set2(nod(OLABEL, lbl, null), def);
                def = blk;
            }
            i.Left = typecheck(i.Left, Erv);
            cas = append(cas, i); 

            // Load hash from type or itab.
            var h = nodSym(ODOTPTR, itab, null);
            h.Type = types.Types[TUINT32];
            h.SetTypecheck(1L);
            if (cond.Right.Type.IsEmptyInterface())
            {
                h.Xoffset = int64(2L * Widthptr); // offset of hash in runtime._type
            }
            else
            {
                h.Xoffset = int64(2L * Widthptr); // offset of hash in runtime.itab
            }
            h.SetBounded(true); // guaranteed not to fault
            a = nod(OAS, s.hashname, h);
            a = typecheck(a, Etop);
            cas = append(cas, a);

            var cc = clauses.list; 

            // insert type equality check into each case block
            foreach (var (_, c) in cc)
            {
                c.node.Right = s.typeone(c.node);
            } 

            // generate list of if statements, binary search for constant sequences
            while (len(cc) > 0L)
            {
                if (!cc[0L].isconst)
                {
                    var n = cc[0L].node;
                    cas = append(cas, n.Right);
                    cc = cc[1L..];
                    continue;
                } 

                // identify run of constants
                long run = default;
                for (run = 1L; run < len(cc) && cc[run].isconst; run++)
                {
                } 

                // sort by hash
 

                // sort by hash
                sort.Sort(caseClauseByType(cc[..run])); 

                // for debugging: linear search
                if (false)
                {
                    {
                        var i__prev2 = i;

                        for (i = 0L; i < run; i++)
                        {
                            n = cc[i].node;
                            cas = append(cas, n.Right);
                        }


                        i = i__prev2;
                    }
                    continue;
                } 

                // combine adjacent cases with the same hash
                long ncase = 0L;
                {
                    var i__prev2 = i;

                    for (i = 0L; i < run; i++)
                    {
                        ncase++;
                        ref Node hash = new slice<ref Node>(new ref Node[] { cc[i].node.Right });
                        for (var j = i + 1L; j < run && cc[i].hash == cc[j].hash; j++)
                        {
                            hash = append(hash, cc[j].node.Right);
                        }

                        cc[i].node.Right = liststmt(hash);
                    } 

                    // binary search among cases to narrow by hash


                    i = i__prev2;
                } 

                // binary search among cases to narrow by hash
                cas = append(cas, s.walkCases(cc[..ncase]));
                cc = cc[ncase..];
            } 

            // handle default case
 

            // handle default case
            if (nerrors == 0L)
            {
                cas = append(cas, def);
                sw.Nbody.Prepend(cas);
                sw.List.Set(null);
                walkstmtlist(sw.Nbody.Slice());
            }
        }

        // typeone generates an AST that jumps to the
        // case body if the variable is of type t.
        private static ref Node typeone(this ref typeSwitch s, ref Node t)
        {
            ref Node name = default;
            Nodes init = default;
            if (t.Rlist.Len() == 0L)
            {
                name = nblank;
                nblank = typecheck(nblank, Erv | Easgn);
            }
            else
            {
                name = t.Rlist.First();
                init.Append(nod(ODCL, name, null));
                var a = nod(OAS, name, null);
                a = typecheck(a, Etop);
                init.Append(a);
            }
            a = nod(OAS2, null, null);
            a.List.Set2(name, s.okname); // name, ok =
            var b = nod(ODOTTYPE, s.facename, null);
            b.Type = t.Left.Type; // interface.(type)
            a.Rlist.Set1(b);
            a = typecheck(a, Etop);
            a = walkexpr(a, ref init);
            init.Append(a);

            var c = nod(OIF, null, null);
            c.Left = s.okname;
            c.Nbody.Set1(t.Right); // if ok { goto l }

            init.Append(c);
            return init.asblock();
        }

        // walkCases generates an AST implementing the cases in cc.
        private static ref Node walkCases(this ref typeSwitch s, slice<caseClause> cc)
        {
            if (len(cc) < binarySearchMin)
            {
                slice<ref Node> cas = default;
                foreach (var (_, c) in cc)
                {
                    var n = c.node;
                    if (!c.isconst)
                    {
                        Fatalf("typeSwitch walkCases");
                    }
                    var a = nod(OIF, null, null);
                    a.Left = nod(OEQ, s.hashname, nodintconst(int64(c.hash)));
                    a.Left = typecheck(a.Left, Erv);
                    a.Nbody.Set1(n.Right);
                    cas = append(cas, a);
                }
                return liststmt(cas);
            } 

            // find the middle and recur
            var half = len(cc) / 2L;
            a = nod(OIF, null, null);
            a.Left = nod(OLE, s.hashname, nodintconst(int64(cc[half - 1L].hash)));
            a.Left = typecheck(a.Left, Erv);
            a.Nbody.Set1(s.walkCases(cc[..half]));
            a.Rlist.Set1(s.walkCases(cc[half..]));
            return a;
        }

        // caseClauseByConstVal sorts clauses by constant value to enable binary search.
        private partial struct caseClauseByConstVal // : slice<caseClause>
        {
        }

        private static long Len(this caseClauseByConstVal x)
        {
            return len(x);
        }
        private static void Swap(this caseClauseByConstVal x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }
        private static bool Less(this caseClauseByConstVal x, long i, long j)
        { 
            // n1 and n2 might be individual constants or integer ranges.
            // We have checked for duplicates already,
            // so ranges can be safely represented by any value in the range.
            var n1 = x[i].node;
            var v1 = default;
            {
                var s__prev1 = s;

                var s = n1.List.Slice();

                if (s != null)
                {
                    v1 = s[0L].Val().U;
                }
                else
                {
                    v1 = n1.Left.Val().U;
                }

                s = s__prev1;

            }

            var n2 = x[j].node;
            var v2 = default;
            {
                var s__prev1 = s;

                s = n2.List.Slice();

                if (s != null)
                {
                    v2 = s[0L].Val().U;
                }
                else
                {
                    v2 = n2.Left.Val().U;
                }

                s = s__prev1;

            }

            switch (v1.type())
            {
                case ref Mpflt v1:
                    return v1.Cmp(v2._<ref Mpflt>()) < 0L;
                    break;
                case ref Mpint v1:
                    return v1.Cmp(v2._<ref Mpint>()) < 0L;
                    break;
                case @string v1:
                    var a = v1;
                    @string b = v2._<@string>();
                    if (len(a) != len(b))
                    {
                        return len(a) < len(b);
                    }
                    return a < b;
                    break;

            }

            Fatalf("caseClauseByConstVal passed bad clauses %v < %v", x[i].node.Left, x[j].node.Left);
            return false;
        }

        private partial struct caseClauseByType // : slice<caseClause>
        {
        }

        private static long Len(this caseClauseByType x)
        {
            return len(x);
        }
        private static void Swap(this caseClauseByType x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }
        private static bool Less(this caseClauseByType x, long i, long j)
        {
            var c1 = x[i];
            var c2 = x[j]; 
            // sort by hash code, then ordinal (for the rare case of hash collisions)
            if (c1.hash != c2.hash)
            {
                return c1.hash < c2.hash;
            }
            return c1.ordinal < c2.ordinal;
        }

        private partial struct constIntNodesByVal // : slice<ref Node>
        {
        }

        private static long Len(this constIntNodesByVal x)
        {
            return len(x);
        }
        private static void Swap(this constIntNodesByVal x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }
        private static bool Less(this constIntNodesByVal x, long i, long j)
        {
            return x[i].Val().U._<ref Mpint>().Cmp(x[j].Val().U._<ref Mpint>()) < 0L;
        }
    }
}}}}
