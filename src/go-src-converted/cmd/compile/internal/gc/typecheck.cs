// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:43:35 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\typecheck.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
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
        // To enable tracing support (-t flag), set enableTrace to true.
        private static readonly var enableTrace = false;



        private static bool trace = default;
        private static slice<byte> traceIndent = default;
        private static bool skipDowidthForTracing = default;

        private static Action<ptr<ptr<Node>>> tracePrint(@string title, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            var indent = traceIndent; 

            // guard against nil
            @string pos = default;            @string op = default;

            byte tc = default;
            if (n != null)
            {
                pos = linestr(n.Pos);
                op = n.Op.String();
                tc = n.Typecheck();
            }

            skipDowidthForTracing = true;
            defer(() =>
            {
                skipDowidthForTracing = false;
            }());
            fmt.Printf("%s: %s%s %p %s %v tc=%d\n", pos, indent, title, n, op, n, tc);
            traceIndent = append(traceIndent, ". ");

            return np =>
            {
                traceIndent = traceIndent[..len(traceIndent) - 2L]; 

                // if we have a result, use that
                if (np != null)
                {
                    n = np.val;
                } 

                // guard against nil
                // use outer pos, op so we don't get empty pos/op if n == nil (nicer output)
                tc = default;
                ptr<types.Type> typ;
                if (n != null)
                {
                    pos = linestr(n.Pos);
                    op = n.Op.String();
                    tc = n.Typecheck();
                    typ = n.Type;
                }

                skipDowidthForTracing = true;
                defer(() =>
                {
                    skipDowidthForTracing = false;
                }());
                fmt.Printf("%s: %s=> %p %s %v tc=%d type=%#L\n", pos, indent, n, op, n, tc, typ);

            };

        });

        private static readonly long ctxStmt = (long)1L << (int)(iota); // evaluated at statement level
        private static readonly var ctxExpr = 0; // evaluated in value context
        private static readonly var ctxType = 1; // evaluated in type context
        private static readonly var ctxCallee = 2; // call-only expressions are ok
        private static readonly var ctxMultiOK = 3; // multivalue function returns are ok
        private static readonly var ctxAssign = 4; // assigning to expression

        // type checks the whole tree of an expression.
        // calculates expression types.
        // evaluates compile time constants.
        // marks variables that escape the local frame.
        // rewrites n.Op to be more specific in some cases.

        private static slice<ptr<Node>> typecheckdefstack = default;

        // resolve ONONAME to definition, if any.
        private static ptr<Node> resolve(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ptr<Node> res = default!;
            ref Node n = ref _addr_n.val;

            if (n == null || n.Op != ONONAME)
            {
                return _addr_n!;
            } 

            // only trace if there's work to do
            if (enableTrace && trace)
            {
                defer(tracePrint("resolve", _addr_n)(_addr_res));
            }

            if (n.Sym.Pkg != localpkg)
            {
                if (inimport)
                {
                    Fatalf("recursive inimport");
                }

                inimport = true;
                expandDecl(n);
                inimport = false;
                return _addr_n!;

            }

            var r = asNode(n.Sym.Def);
            if (r == null)
            {
                return _addr_n!;
            }

            if (r.Op == OIOTA)
            {
                {
                    var x = getIotaValue();

                    if (x >= 0L)
                    {
                        return _addr_nodintconst(x)!;
                    }

                }

                return _addr_n!;

            }

            return _addr_r!;

        });

        private static void typecheckslice(slice<ptr<Node>> l, long top)
        {
            foreach (var (i) in l)
            {
                l[i] = typecheck(_addr_l[i], top);
            }

        }

        private static @string _typekind = new slice<@string>(InitKeyedValues<@string>((TINT, "int"), (TUINT, "uint"), (TINT8, "int8"), (TUINT8, "uint8"), (TINT16, "int16"), (TUINT16, "uint16"), (TINT32, "int32"), (TUINT32, "uint32"), (TINT64, "int64"), (TUINT64, "uint64"), (TUINTPTR, "uintptr"), (TCOMPLEX64, "complex64"), (TCOMPLEX128, "complex128"), (TFLOAT32, "float32"), (TFLOAT64, "float64"), (TBOOL, "bool"), (TSTRING, "string"), (TPTR, "pointer"), (TUNSAFEPTR, "unsafe.Pointer"), (TSTRUCT, "struct"), (TINTER, "interface"), (TCHAN, "chan"), (TMAP, "map"), (TARRAY, "array"), (TSLICE, "slice"), (TFUNC, "func"), (TNIL, "nil"), (TIDEAL, "untyped number")));

        private static @string typekind(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsSlice())
            {
                return "slice";
            }

            var et = t.Etype;
            if (int(et) < len(_typekind))
            {
                var s = _typekind[et];
                if (s != "")
                {
                    return s;
                }

            }

            return fmt.Sprintf("etype=%d", et);

        }

        private static slice<ptr<Node>> cycleFor(ptr<Node> _addr_start)
        {
            ref Node start = ref _addr_start.val;
 
            // Find the start node in typecheck_tcstack.
            // We know that it must exist because each time we mark
            // a node with n.SetTypecheck(2) we push it on the stack,
            // and each time we mark a node with n.SetTypecheck(2) we
            // pop it from the stack. We hit a cycle when we encounter
            // a node marked 2 in which case is must be on the stack.
            var i = len(typecheck_tcstack) - 1L;
            while (i > 0L && typecheck_tcstack[i] != start)
            {
                i--;
            } 

            // collect all nodes with same Op
 

            // collect all nodes with same Op
            slice<ptr<Node>> cycle = default;
            foreach (var (_, n) in typecheck_tcstack[i..])
            {
                if (n.Op == start.Op)
                {
                    cycle = append(cycle, n);
                }

            }
            return cycle;

        }

        private static @string cycleTrace(slice<ptr<Node>> cycle)
        {
            @string s = default;
            foreach (var (i, n) in cycle)
            {
                s += fmt.Sprintf("\n\t%v: %v uses %v", n.Line(), n, cycle[(i + 1L) % len(cycle)]);
            }
            return s;

        }

        private static slice<ptr<Node>> typecheck_tcstack = default;

        // typecheck type checks node n.
        // The result of typecheck MUST be assigned back to n, e.g.
        //     n.Left = typecheck(n.Left, top)
        private static ptr<Node> typecheck(ptr<Node> _addr_n, long top) => func((defer, _, __) =>
        {
            ptr<Node> res = default!;
            ref Node n = ref _addr_n.val;
 
            // cannot type check until all the source has been parsed
            if (!typecheckok)
            {
                Fatalf("early typecheck");
            }

            if (n == null)
            {
                return _addr_null!;
            } 

            // only trace if there's work to do
            if (enableTrace && trace)
            {
                defer(tracePrint("typecheck", _addr_n)(_addr_res));
            }

            var lno = setlineno(n); 

            // Skip over parens.
            while (n.Op == OPAREN)
            {
                n = n.Left;
            } 

            // Resolve definition of name and value of iota lazily.
 

            // Resolve definition of name and value of iota lazily.
            n = resolve(_addr_n); 

            // Skip typecheck if already done.
            // But re-typecheck ONAME/OTYPE/OLITERAL/OPACK node in case context has changed.
            if (n.Typecheck() == 1L)
            {

                if (n.Op == ONAME || n.Op == OTYPE || n.Op == OLITERAL || n.Op == OPACK) 
                    break;
                else 
                    lineno = lno;
                    return _addr_n!;
                
            }

            if (n.Typecheck() == 2L)
            { 
                // Typechecking loop. Trying printing a meaningful message,
                // otherwise a stack trace of typechecking.

                // We can already diagnose variables used as types.
                if (n.Op == ONAME) 
                    if (top & (ctxExpr | ctxType) == ctxType)
                    {
                        yyerror("%v is not a type", n);
                    }

                else if (n.Op == OTYPE) 
                    // Only report a type cycle if we are expecting a type.
                    // Otherwise let other code report an error.
                    if (top & ctxType == ctxType)
                    { 
                        // A cycle containing only alias types is an error
                        // since it would expand indefinitely when aliases
                        // are substituted.
                        var cycle = cycleFor(_addr_n);
                        foreach (var (_, n1) in cycle)
                        {
                            if (n1.Name != null && !n1.Name.Param.Alias)
                            { 
                                // Cycle is ok. But if n is an alias type and doesn't
                                // have a type yet, we have a recursive type declaration
                                // with aliases that we can't handle properly yet.
                                // Report an error rather than crashing later.
                                if (n.Name != null && n.Name.Param.Alias && n.Type == null)
                                {
                                    lineno = n.Pos;
                                    Fatalf("cannot handle alias type declaration (issue #25838): %v", n);
                                }

                                lineno = lno;
                                return _addr_n!;

                            }

                        }
                        yyerrorl(n.Pos, "invalid recursive type alias %v%s", n, cycleTrace(cycle));

                    }

                else if (n.Op == OLITERAL) 
                    if (top & (ctxExpr | ctxType) == ctxType)
                    {
                        yyerror("%v is not a type", n);
                        break;
                    }

                    yyerrorl(n.Pos, "constant definition loop%s", cycleTrace(cycleFor(_addr_n)));
                                if (nsavederrors + nerrors == 0L)
                {
                    @string trace = default;
                    for (var i = len(typecheck_tcstack) - 1L; i >= 0L; i--)
                    {
                        var x = typecheck_tcstack[i];
                        trace += fmt.Sprintf("\n\t%v %v", x.Line(), x);
                    }

                    yyerror("typechecking loop involving %v%s", n, trace);

                }

                lineno = lno;
                return _addr_n!;

            }

            n.SetTypecheck(2L);

            typecheck_tcstack = append(typecheck_tcstack, n);
            n = typecheck1(_addr_n, top);

            n.SetTypecheck(1L);

            var last = len(typecheck_tcstack) - 1L;
            typecheck_tcstack[last] = null;
            typecheck_tcstack = typecheck_tcstack[..last];

            lineno = lno;
            return _addr_n!;

        });

        // indexlit implements typechecking of untyped values as
        // array/slice indexes. It is almost equivalent to defaultlit
        // but also accepts untyped numeric values representable as
        // value of type int (see also checkmake for comparison).
        // The result of indexlit MUST be assigned back to n, e.g.
        //     n.Left = indexlit(n.Left)
        private static ptr<Node> indexlit(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n != null && n.Type != null && n.Type.Etype == TIDEAL)
            {
                return _addr_defaultlit(n, types.Types[TINT])!;
            }

            return _addr_n!;

        }

        // The result of typecheck1 MUST be assigned back to n, e.g.
        //     n.Left = typecheck1(n.Left, top)
        private static ptr<Node> typecheck1(ptr<Node> _addr_n, long top) => func((defer, _, __) =>
        {
            ptr<Node> res = default!;
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheck1", _addr_n)(_addr_res));
            }


            if (n.Op == OLITERAL || n.Op == ONAME || n.Op == ONONAME || n.Op == OTYPE) 
                if (n.Sym == null)
                {
                    break;
                }

                if (n.Op == ONAME && n.SubOp() != 0L && top & ctxCallee == 0L)
                {
                    yyerror("use of builtin %v not in function call", n.Sym);
                    n.Type = null;
                    return _addr_n!;
                }

                typecheckdef(_addr_n);
                if (n.Op == ONONAME)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                        long ok = 0L;

            // until typecheck is complete, do nothing.
            if (n.Op == OLITERAL) 
                ok |= ctxExpr;

                if (n.Type == null && n.Val().Ctype() == CTSTR)
                {
                    n.Type = types.Idealstring;
                }

            else if (n.Op == ONONAME) 
                ok |= ctxExpr;
            else if (n.Op == ONAME) 
                if (n.Name.Decldepth == 0L)
                {
                    n.Name.Decldepth = decldepth;
                }

                if (n.SubOp() != 0L)
                {
                    ok |= ctxCallee;
                    break;
                }

                if (top & ctxAssign == 0L)
                { 
                    // not a write to the variable
                    if (n.isBlank())
                    {
                        yyerror("cannot use _ as value");
                        n.Type = null;
                        return _addr_n!;
                    }

                    n.Name.SetUsed(true);

                }

                ok |= ctxExpr;
            else if (n.Op == OPACK) 
                yyerror("use of package %v without selector", n.Sym);
                n.Type = null;
                return _addr_n!;
            else if (n.Op == ODDD) 
                break; 

                // types (ODEREF is with exprs)
            else if (n.Op == OTYPE) 
                ok |= ctxType;

                if (n.Type == null)
                {
                    return _addr_n!;
                }

            else if (n.Op == OTARRAY) 
                ok |= ctxType;
                var r = typecheck(_addr_n.Right, ctxType);
                if (r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                ptr<types.Type> t;
                if (n.Left == null)
                {
                    t = types.NewSlice(r.Type);
                }
                else if (n.Left.Op == ODDD)
                {
                    if (!n.Diag())
                    {
                        n.SetDiag(true);
                        yyerror("use of [...] array outside of array literal");
                    }

                    n.Type = null;
                    return _addr_n!;

                }
                else
                {
                    n.Left = indexlit(_addr_typecheck(_addr_n.Left, ctxExpr));
                    var l = n.Left;
                    if (consttype(l) != CTINT)
                    {

                        if (l.Type == null)                         else if (l.Type.IsInteger() && l.Op != OLITERAL) 
                            yyerror("non-constant array bound %v", l);
                        else 
                            yyerror("invalid array bound %v", l);
                                                n.Type = null;
                        return _addr_n!;

                    }

                    var v = l.Val();
                    if (doesoverflow(v, types.Types[TINT]))
                    {
                        yyerror("array bound is too large");
                        n.Type = null;
                        return _addr_n!;
                    }

                    ptr<Mpint> bound = v.U._<ptr<Mpint>>().Int64();
                    if (bound < 0L)
                    {
                        yyerror("array bound must be non-negative");
                        n.Type = null;
                        return _addr_n!;
                    }

                    t = types.NewArray(r.Type, bound);

                }

                setTypeNode(_addr_n, t);
                n.Left = null;
                n.Right = null;
                checkwidth(t);
            else if (n.Op == OTMAP) 
                ok |= ctxType;
                n.Left = typecheck(_addr_n.Left, ctxType);
                n.Right = typecheck(_addr_n.Right, ctxType);
                l = n.Left;
                r = n.Right;
                if (l.Type == null || r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.NotInHeap())
                {
                    yyerror("go:notinheap map key not allowed");
                }

                if (r.Type.NotInHeap())
                {
                    yyerror("go:notinheap map value not allowed");
                }

                setTypeNode(_addr_n, _addr_types.NewMap(l.Type, r.Type));
                mapqueue = append(mapqueue, n); // check map keys when all types are settled
                n.Left = null;
                n.Right = null;
            else if (n.Op == OTCHAN) 
                ok |= ctxType;
                n.Left = typecheck(_addr_n.Left, ctxType);
                l = n.Left;
                if (l.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.NotInHeap())
                {
                    yyerror("chan of go:notinheap type not allowed");
                }

                setTypeNode(_addr_n, _addr_types.NewChan(l.Type, n.TChanDir()));
                n.Left = null;
                n.ResetAux();
            else if (n.Op == OTSTRUCT) 
                ok |= ctxType;
                setTypeNode(_addr_n, _addr_tostruct(n.List.Slice()));
                n.List.Set(null);
            else if (n.Op == OTINTER) 
                ok |= ctxType;
                setTypeNode(_addr_n, _addr_tointerface(n.List.Slice()));
            else if (n.Op == OTFUNC) 
                ok |= ctxType;
                setTypeNode(_addr_n, _addr_functype(n.Left, n.List.Slice(), n.Rlist.Slice()));
                n.Left = null;
                n.List.Set(null);
                n.Rlist.Set(null); 

                // type or expr
            else if (n.Op == ODEREF) 
                n.Left = typecheck(_addr_n.Left, ctxExpr | ctxType);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Op == OTYPE)
                {
                    ok |= ctxType;
                    setTypeNode(_addr_n, _addr_types.NewPtr(l.Type));
                    n.Left = null; 
                    // Ensure l.Type gets dowidth'd for the backend. Issue 20174.
                    checkwidth(l.Type);
                    break;

                }

                if (!t.IsPtr())
                {
                    if (top & (ctxExpr | ctxStmt) != 0L)
                    {
                        yyerror("invalid indirect of %L", n.Left);
                        n.Type = null;
                        return _addr_n!;
                    }

                    break;

                }

                ok |= ctxExpr;
                n.Type = t.Elem(); 

                // arithmetic exprs
            else if (n.Op == OASOP || n.Op == OADD || n.Op == OAND || n.Op == OANDAND || n.Op == OANDNOT || n.Op == ODIV || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLT || n.Op == OLSH || n.Op == ORSH || n.Op == OMOD || n.Op == OMUL || n.Op == ONE || n.Op == OOR || n.Op == OOROR || n.Op == OSUB || n.Op == OXOR) 
                l = ;
                Op op = default;
                r = ;
                if (n.Op == OASOP)
                {
                    ok |= ctxStmt;
                    n.Left = typecheck(_addr_n.Left, ctxExpr);
                    n.Right = typecheck(_addr_n.Right, ctxExpr);
                    l = n.Left;
                    r = n.Right;
                    checkassign(_addr_n, _addr_n.Left);
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (n.Implicit() && !okforarith[l.Type.Etype])
                    {
                        yyerror("invalid operation: %v (non-numeric type %v)", n, l.Type);
                        n.Type = null;
                        return _addr_n!;
                    } 
                    // TODO(marvin): Fix Node.EType type union.
                    op = n.SubOp();

                }
                else
                {
                    ok |= ctxExpr;
                    n.Left = typecheck(_addr_n.Left, ctxExpr);
                    n.Right = typecheck(_addr_n.Right, ctxExpr);
                    l = n.Left;
                    r = n.Right;
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    op = n.Op;

                }

                if (op == OLSH || op == ORSH)
                {
                    r = defaultlit(r, types.Types[TUINT]);
                    n.Right = r;
                    t = r.Type;
                    if (!t.IsInteger())
                    {
                        yyerror("invalid operation: %v (shift count type %v, must be integer)", n, r.Type);
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (t.IsSigned() && !langSupported(1L, 13L, curpkg()))
                    {
                        yyerrorv("go1.13", "invalid operation: %v (signed shift count type %v)", n, r.Type);
                        n.Type = null;
                        return _addr_n!;
                    }

                    t = l.Type;
                    if (t != null && t.Etype != TIDEAL && !t.IsInteger())
                    {
                        yyerror("invalid operation: %v (shift of type %v)", n, t);
                        n.Type = null;
                        return _addr_n!;
                    } 

                    // no defaultlit for left
                    // the outer context gives the type
                    n.Type = l.Type;

                    break;

                } 

                // ideal mixed with non-ideal
                l, r = defaultlit2(l, r, false);

                n.Left = l;
                n.Right = r;
                if (l.Type == null || r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                t = l.Type;
                if (t.Etype == TIDEAL)
                {
                    t = r.Type;
                }

                var et = t.Etype;
                if (et == TIDEAL)
                {
                    et = TINT;
                }

                var aop = OXXX;
                if (iscmp[n.Op] && t.Etype != TIDEAL && !types.Identical(l.Type, r.Type))
                { 
                    // comparison is okay as long as one side is
                    // assignable to the other.  convert so they have
                    // the same type.
                    //
                    // the only conversion that isn't a no-op is concrete == interface.
                    // in that case, check comparability of the concrete type.
                    // The conversion allocates, so only do it if the concrete type is huge.
                    var converted = false;
                    if (r.Type.Etype != TBLANK)
                    {
                        aop = assignop(l.Type, r.Type, null);
                        if (aop != 0L)
                        {
                            if (r.Type.IsInterface() && !l.Type.IsInterface() && !IsComparable(l.Type))
                            {
                                yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(_addr_l.Type));
                                n.Type = null;
                                return _addr_n!;
                            }

                            dowidth(l.Type);
                            if (r.Type.IsInterface() == l.Type.IsInterface() || l.Type.Width >= 1L << (int)(16L))
                            {
                                l = nod(aop, l, null);
                                l.Type = r.Type;
                                l.SetTypecheck(1L);
                                n.Left = l;
                            }

                            t = r.Type;
                            converted = true;

                        }

                    }

                    if (!converted && l.Type.Etype != TBLANK)
                    {
                        aop = assignop(r.Type, l.Type, null);
                        if (aop != 0L)
                        {
                            if (l.Type.IsInterface() && !r.Type.IsInterface() && !IsComparable(r.Type))
                            {
                                yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(_addr_r.Type));
                                n.Type = null;
                                return _addr_n!;
                            }

                            dowidth(r.Type);
                            if (r.Type.IsInterface() == l.Type.IsInterface() || r.Type.Width >= 1L << (int)(16L))
                            {
                                r = nod(aop, r, null);
                                r.Type = l.Type;
                                r.SetTypecheck(1L);
                                n.Right = r;
                            }

                            t = l.Type;

                        }

                    }

                    et = t.Etype;

                }

                if (t.Etype != TIDEAL && !types.Identical(l.Type, r.Type))
                {
                    l, r = defaultlit2(l, r, true);
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (l.Type.IsInterface() == r.Type.IsInterface() || aop == 0L)
                    {
                        yyerror("invalid operation: %v (mismatched types %v and %v)", n, l.Type, r.Type);
                        n.Type = null;
                        return _addr_n!;
                    }

                }

                if (!okfor[op][et])
                {
                    yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(t));
                    n.Type = null;
                    return _addr_n!;
                } 

                // okfor allows any array == array, map == map, func == func.
                // restrict to slice/map/func == nil and nil == slice/map/func.
                if (l.Type.IsArray() && !IsComparable(l.Type))
                {
                    yyerror("invalid operation: %v (%v cannot be compared)", n, l.Type);
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.IsSlice() && !l.isNil() && !r.isNil())
                {
                    yyerror("invalid operation: %v (slice can only be compared to nil)", n);
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.IsMap() && !l.isNil() && !r.isNil())
                {
                    yyerror("invalid operation: %v (map can only be compared to nil)", n);
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.Etype == TFUNC && !l.isNil() && !r.isNil())
                {
                    yyerror("invalid operation: %v (func can only be compared to nil)", n);
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.IsStruct())
                {
                    {
                        var f = IncomparableField(l.Type);

                        if (f != null)
                        {
                            yyerror("invalid operation: %v (struct containing %v cannot be compared)", n, f.Type);
                            n.Type = null;
                            return _addr_n!;
                        }

                    }

                }

                t = l.Type;
                if (iscmp[n.Op])
                { 
                    // TIDEAL includes complex constant, but only OEQ and ONE are defined for complex,
                    // so check that the n.op is available for complex  here before doing evconst.
                    if (!okfor[n.Op][TCOMPLEX128] && (Isconst(l, CTCPLX) || Isconst(r, CTCPLX)))
                    {
                        yyerror("invalid operation: %v (operator %v not defined on untyped complex)", n, n.Op);
                        n.Type = null;
                        return _addr_n!;
                    }

                    evconst(n);
                    t = types.Idealbool;
                    if (n.Op != OLITERAL)
                    {
                        l, r = defaultlit2(l, r, true);
                        n.Left = l;
                        n.Right = r;
                    }

                }

                if (et == TSTRING && n.Op == OADD)
                { 
                    // create OADDSTR node with list of strings in x + y + z + (w + v) + ...
                    n.Op = OADDSTR;

                    if (l.Op == OADDSTR)
                    {
                        n.List.Set(l.List.Slice());
                    }
                    else
                    {
                        n.List.Set1(l);
                    }

                    if (r.Op == OADDSTR)
                    {
                        n.List.AppendNodes(_addr_r.List);
                    }
                    else
                    {
                        n.List.Append(r);
                    }

                    n.Left = null;
                    n.Right = null;

                }

                if ((op == ODIV || op == OMOD) && Isconst(r, CTINT))
                {
                    if (r.Val().U._<ptr<Mpint>>().CmpInt64(0L) == 0L)
                    {
                        yyerror("division by zero");
                        n.Type = null;
                        return _addr_n!;
                    }

                }

                n.Type = t;
            else if (n.Op == OBITNOT || n.Op == ONEG || n.Op == ONOT || n.Op == OPLUS) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!okfor[n.Op][t.Etype])
                {
                    yyerror("invalid operation: %v %v", n.Op, t);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t; 

                // exprs
            else if (n.Op == OADDR) 
                ok |= ctxExpr;

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                if (n.Left.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }


                if (n.Left.Op == OARRAYLIT || n.Left.Op == OMAPLIT || n.Left.Op == OSLICELIT || n.Left.Op == OSTRUCTLIT) 
                    n.Op = OPTRLIT;
                else 
                    checklvalue(_addr_n.Left, "take the address of");
                    r = outervalue(n.Left);
                    if (r.Op == ONAME)
                    {
                        if (r.Orig != r)
                        {
                            Fatalf("found non-orig name node %v", r); // TODO(mdempsky): What does this mean?
                        }

                        r.Name.SetAddrtaken(true);
                        if (r.Name.IsClosureVar() && !capturevarscomplete)
                        { 
                            // Mark the original variable as Addrtaken so that capturevars
                            // knows not to pass it by value.
                            // But if the capturevars phase is complete, don't touch it,
                            // in case l.Name's containing function has not yet been compiled.
                            r.Name.Defn.Name.SetAddrtaken(true);

                        }

                    }

                    n.Left = defaultlit(n.Left, null);
                    if (n.Left.Type == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                                n.Type = types.NewPtr(n.Left.Type);
            else if (n.Op == OCOMPLIT) 
                ok |= ctxExpr;
                n = typecheckcomplit(_addr_n);
                if (n.Type == null)
                {
                    return _addr_n!;
                }

            else if (n.Op == OXDOT || n.Op == ODOT) 
                if (n.Op == OXDOT)
                {
                    n = adddot(n);
                    n.Op = ODOT;
                    if (n.Left == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                }

                n.Left = typecheck(_addr_n.Left, ctxExpr | ctxType);

                n.Left = defaultlit(n.Left, null);

                t = n.Left.Type;
                if (t == null)
                {
                    adderrorname(n);
                    n.Type = null;
                    return _addr_n!;
                }

                var s = n.Sym;

                if (n.Left.Op == OTYPE)
                {
                    n = typecheckMethodExpr(_addr_n);
                    if (n.Type == null)
                    {
                        return _addr_n!;
                    }

                    ok = ctxExpr;
                    break;

                }

                if (t.IsPtr() && !t.Elem().IsInterface())
                {
                    t = t.Elem();
                    if (t == null)
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    n.Op = ODOTPTR;
                    checkwidth(t);

                }

                if (n.Sym.IsBlank())
                {
                    yyerror("cannot refer to blank field or method");
                    n.Type = null;
                    return _addr_n!;
                }

                if (lookdot(_addr_n, t, 0L) == null)
                { 
                    // Legitimate field or method lookup failed, try to explain the error

                    if (t.IsEmptyInterface()) 
                        yyerror("%v undefined (type %v is interface with no methods)", n, n.Left.Type);
                    else if (t.IsPtr() && t.Elem().IsInterface()) 
                        // Pointer to interface is almost always a mistake.
                        yyerror("%v undefined (type %v is pointer to interface, not interface)", n, n.Left.Type);
                    else if (lookdot(_addr_n, t, 1L) != null) 
                        // Field or method matches by name, but it is not exported.
                        yyerror("%v undefined (cannot refer to unexported field or method %v)", n, n.Sym);
                    else 
                        {
                            var mt = lookdot(_addr_n, t, 2L);

                            if (mt != null && visible(_addr_mt.Sym))
                            { // Case-insensitive lookup.
                                yyerror("%v undefined (type %v has no field or method %v, but does have %v)", n, n.Left.Type, n.Sym, mt.Sym);

                            }
                            else
                            {
                                yyerror("%v undefined (type %v has no field or method %v)", n, n.Left.Type, n.Sym);
                            }

                        }

                                        n.Type = null;
                    return _addr_n!;

                }


                if (n.Op == ODOTINTER || n.Op == ODOTMETH) 
                    if (top & ctxCallee != 0L)
                    {
                        ok |= ctxCallee;
                    }
                    else
                    {
                        typecheckpartialcall(n, s);
                        ok |= ctxExpr;
                    }

                else 
                    ok |= ctxExpr;
                            else if (n.Op == ODOTTYPE) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsInterface())
                {
                    yyerror("invalid type assertion: %v (non-interface type %v on left)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                if (n.Right != null)
                {
                    n.Right = typecheck(_addr_n.Right, ctxType);
                    n.Type = n.Right.Type;
                    n.Right = null;
                    if (n.Type == null)
                    {
                        return _addr_n!;
                    }

                }

                if (n.Type != null && !n.Type.IsInterface())
                {
                    ptr<types.Field> missing;                    ptr<types.Field> have;

                    ref long ptr = ref heap(out ptr<long> _addr_ptr);
                    if (!implements(n.Type, t, _addr_missing, _addr_have, _addr_ptr))
                    {
                        if (have != null && have.Sym == missing.Sym)
                        {
                            yyerror("impossible type assertion:\n\t%v does not implement %v (wrong type for %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", n.Type, t, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                        }
                        else if (ptr != 0L)
                        {
                            yyerror("impossible type assertion:\n\t%v does not implement %v (%v method has pointer receiver)", n.Type, t, missing.Sym);
                        }
                        else if (have != null)
                        {
                            yyerror("impossible type assertion:\n\t%v does not implement %v (missing %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", n.Type, t, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                        }
                        else
                        {
                            yyerror("impossible type assertion:\n\t%v does not implement %v (missing %v method)", n.Type, t, missing.Sym);
                        }

                        n.Type = null;
                        return _addr_n!;

                    }

                }

            else if (n.Op == OINDEX) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                n.Left = implicitstar(_addr_n.Left);
                l = n.Left;
                n.Right = typecheck(_addr_n.Right, ctxExpr);
                r = n.Right;
                t = l.Type;
                if (t == null || r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }


                if (t.Etype == TSTRING || t.Etype == TARRAY || t.Etype == TSLICE) 
                    n.Right = indexlit(_addr_n.Right);
                    if (t.IsString())
                    {
                        n.Type = types.Bytetype;
                    }
                    else
                    {
                        n.Type = t.Elem();
                    }

                    ref @string why = ref heap("string", out ptr<@string> _addr_why);
                    if (t.IsArray())
                    {
                        why = "array";
                    }
                    else if (t.IsSlice())
                    {
                        why = "slice";
                    }

                    if (n.Right.Type != null && !n.Right.Type.IsInteger())
                    {
                        yyerror("non-integer %s index %v", why, n.Right);
                        break;
                    }

                    if (!n.Bounded() && Isconst(n.Right, CTINT))
                    {
                        var x = n.Right.Int64();
                        if (x < 0L)
                        {
                            yyerror("invalid %s index %v (index must be non-negative)", why, n.Right);
                        }
                        else if (t.IsArray() && x >= t.NumElem())
                        {
                            yyerror("invalid array index %v (out of bounds for %d-element array)", n.Right, t.NumElem());
                        }
                        else if (Isconst(n.Left, CTSTR) && x >= int64(len(strlit(n.Left))))
                        {
                            yyerror("invalid string index %v (out of bounds for %d-byte string)", n.Right, len(strlit(n.Left)));
                        }
                        else if (n.Right.Val().U._<ptr<Mpint>>().Cmp(maxintval[TINT]) > 0L)
                        {
                            yyerror("invalid %s index %v (index too large)", why, n.Right);
                        }

                    }

                else if (t.Etype == TMAP) 
                    n.Right = assignconv(n.Right, t.Key(), "map index");
                    n.Type = t.Elem();
                    n.Op = OINDEXMAP;
                    n.ResetAux();
                else 
                    yyerror("invalid operation: %v (type %v does not support indexing)", n, t);
                    n.Type = null;
                    return _addr_n!;
                            else if (n.Op == ORECV) 
                ok |= ctxStmt | ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (receive from non-chan type %v)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.ChanDir().CanRecv())
                {
                    yyerror("invalid operation: %v (receive from send-only type %v)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t.Elem();
            else if (n.Op == OSEND) 
                ok |= ctxStmt;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Right = typecheck(_addr_n.Right, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (send to non-chan type %v)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.ChanDir().CanSend())
                {
                    yyerror("invalid operation: %v (send to receive-only type %v)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Right = assignconv(n.Right, t.Elem(), "send");
                if (n.Right.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = null;
            else if (n.Op == OSLICEHEADER) 
                // Errors here are Fatalf instead of yyerror because only the compiler
                // can construct an OSLICEHEADER node.
                // Components used in OSLICEHEADER that are supplied by parsed source code
                // have already been typechecked in e.g. OMAKESLICE earlier.
                ok |= ctxExpr;

                t = n.Type;
                if (t == null)
                {
                    Fatalf("no type specified for OSLICEHEADER");
                }

                if (!t.IsSlice())
                {
                    Fatalf("invalid type %v for OSLICEHEADER", n.Type);
                }

                if (n.Left == null || n.Left.Type == null || !n.Left.Type.IsUnsafePtr())
                {
                    Fatalf("need unsafe.Pointer for OSLICEHEADER");
                }

                {
                    var x__prev1 = x;

                    x = n.List.Len();

                    if (x != 2L)
                    {
                        Fatalf("expected 2 params (len, cap) for OSLICEHEADER, got %d", x);
                    }

                    x = x__prev1;

                }


                n.Left = typecheck(_addr_n.Left, ctxExpr);
                l = typecheck(_addr_n.List.First(), ctxExpr);
                var c = typecheck(_addr_n.List.Second(), ctxExpr);
                l = defaultlit(l, types.Types[TINT]);
                c = defaultlit(c, types.Types[TINT]);

                if (Isconst(l, CTINT) && l.Int64() < 0L)
                {
                    Fatalf("len for OSLICEHEADER must be non-negative");
                }

                if (Isconst(c, CTINT) && c.Int64() < 0L)
                {
                    Fatalf("cap for OSLICEHEADER must be non-negative");
                }

                if (Isconst(l, CTINT) && Isconst(c, CTINT) && l.Val().U._<ptr<Mpint>>().Cmp(c.Val().U._<ptr<Mpint>>()) > 0L)
                {
                    Fatalf("len larger than cap for OSLICEHEADER");
                }

                n.List.SetFirst(l);
                n.List.SetSecond(c);
            else if (n.Op == OMAKESLICECOPY) 
                // Errors here are Fatalf instead of yyerror because only the compiler
                // can construct an OMAKESLICECOPY node.
                // Components used in OMAKESCLICECOPY that are supplied by parsed source code
                // have already been typechecked in OMAKE and OCOPY earlier.
                ok |= ctxExpr;

                t = n.Type;

                if (t == null)
                {
                    Fatalf("no type specified for OMAKESLICECOPY");
                }

                if (!t.IsSlice())
                {
                    Fatalf("invalid type %v for OMAKESLICECOPY", n.Type);
                }

                if (n.Left == null)
                {
                    Fatalf("missing len argument for OMAKESLICECOPY");
                }

                if (n.Right == null)
                {
                    Fatalf("missing slice argument to copy for OMAKESLICECOPY");
                }

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Right = typecheck(_addr_n.Right, ctxExpr);

                n.Left = defaultlit(n.Left, types.Types[TINT]);

                if (!n.Left.Type.IsInteger() && n.Type.Etype != TIDEAL)
                {
                    yyerror("non-integer len argument in OMAKESLICECOPY");
                }

                if (Isconst(n.Left, CTINT))
                {
                    if (n.Left.Val().U._<ptr<Mpint>>().Cmp(maxintval[TINT]) > 0L)
                    {
                        Fatalf("len for OMAKESLICECOPY too large");
                    }

                    if (n.Left.Int64() < 0L)
                    {
                        Fatalf("len for OMAKESLICECOPY must be non-negative");
                    }

                }

            else if (n.Op == OSLICE || n.Op == OSLICE3) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                var (low, high, max) = n.SliceBounds();
                var hasmax = n.Op.IsSlice3();
                low = typecheck(_addr_low, ctxExpr);
                high = typecheck(_addr_high, ctxExpr);
                max = typecheck(_addr_max, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                low = indexlit(_addr_low);
                high = indexlit(_addr_high);
                max = indexlit(_addr_max);
                n.SetSliceBounds(low, high, max);
                l = n.Left;
                if (l.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (l.Type.IsArray())
                {
                    if (!islvalue(_addr_n.Left))
                    {
                        yyerror("invalid operation %v (slice of unaddressable value)", n);
                        n.Type = null;
                        return _addr_n!;
                    }

                    n.Left = nod(OADDR, n.Left, null);
                    n.Left.SetImplicit(true);
                    n.Left = typecheck(_addr_n.Left, ctxExpr);
                    l = n.Left;

                }

                t = l.Type;
                ptr<types.Type> tp;
                if (t.IsString())
                {
                    if (hasmax)
                    {
                        yyerror("invalid operation %v (3-index slice of string)", n);
                        n.Type = null;
                        return _addr_n!;
                    }

                    n.Type = t;
                    n.Op = OSLICESTR;

                }
                else if (t.IsPtr() && t.Elem().IsArray())
                {
                    tp = t.Elem();
                    n.Type = types.NewSlice(tp.Elem());
                    dowidth(n.Type);
                    if (hasmax)
                    {
                        n.Op = OSLICE3ARR;
                    }
                    else
                    {
                        n.Op = OSLICEARR;
                    }

                }
                else if (t.IsSlice())
                {
                    n.Type = t;
                }
                else
                {
                    yyerror("cannot slice %v (type %v)", l, t);
                    n.Type = null;
                    return _addr_n!;
                }

                if (low != null && !checksliceindex(_addr_l, _addr_low, tp))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (high != null && !checksliceindex(_addr_l, _addr_high, tp))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (max != null && !checksliceindex(_addr_l, _addr_max, tp))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!checksliceconst(_addr_low, _addr_high) || !checksliceconst(_addr_low, _addr_max) || !checksliceconst(_addr_high, _addr_max))
                {
                    n.Type = null;
                    return _addr_n!;
                } 

                // call and call like
            else if (n.Op == OCALL) 
                typecheckslice(n.Ninit.Slice(), ctxStmt); // imported rewritten f(g()) calls (#30907)
                n.Left = typecheck(_addr_n.Left, ctxExpr | ctxType | ctxCallee);
                if (n.Left.Diag())
                {
                    n.SetDiag(true);
                }

                l = n.Left;

                if (l.Op == ONAME && l.SubOp() != 0L)
                {
                    if (n.IsDDD() && l.SubOp() != OAPPEND)
                    {
                        yyerror("invalid use of ... with builtin %v", l);
                    } 

                    // builtin: OLEN, OCAP, etc.
                    n.Op = l.SubOp();
                    n.Left = n.Right;
                    n.Right = null;
                    n = typecheck1(_addr_n, top);
                    return _addr_n!;

                }

                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                if (l.Op == OTYPE)
                {
                    if (n.IsDDD())
                    {
                        if (!l.Type.Broke())
                        {
                            yyerror("invalid use of ... in type conversion to %v", l.Type);
                        }

                        n.SetDiag(true);

                    } 

                    // pick off before type-checking arguments
                    ok |= ctxExpr; 

                    // turn CALL(type, arg) into CONV(arg) w/ type
                    n.Left = null;

                    n.Op = OCONV;
                    n.Type = l.Type;
                    if (!onearg(_addr_n, "conversion to %v", l.Type))
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    n = typecheck1(_addr_n, top);
                    return _addr_n!;

                }

                typecheckargs(_addr_n);
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                checkwidth(t);


                if (l.Op == ODOTINTER) 
                    n.Op = OCALLINTER;
                else if (l.Op == ODOTMETH) 
                    n.Op = OCALLMETH; 

                    // typecheckaste was used here but there wasn't enough
                    // information further down the call chain to know if we
                    // were testing a method receiver for unexported fields.
                    // It isn't necessary, so just do a sanity check.
                    tp = t.Recv().Type;

                    if (l.Left == null || !types.Identical(l.Left.Type, tp))
                    {
                        Fatalf("method receiver");
                    }

                else 
                    n.Op = OCALLFUNC;
                    if (t.Etype != TFUNC)
                    {
                        var name = l.String();
                        if (isBuiltinFuncName(name) && l.Name.Defn != null)
                        { 
                            // be more specific when the function
                            // name matches a predeclared function
                            yyerror("cannot call non-function %s (type %v), declared at %s", name, t, linestr(l.Name.Defn.Pos));

                        }
                        else
                        {
                            yyerror("cannot call non-function %s (type %v)", name, t);
                        }

                        n.Type = null;
                        return _addr_n!;

                    }

                                typecheckaste(OCALL, _addr_n.Left, n.IsDDD(), _addr_t.Params(), n.List, () => _addr_fmt.Sprintf("argument to %v", n.Left)!);
                ok |= ctxStmt;
                if (t.NumResults() == 0L)
                {
                    break;
                }

                ok |= ctxExpr;
                if (t.NumResults() == 1L)
                {
                    n.Type = l.Type.Results().Field(0L).Type;

                    if (n.Op == OCALLFUNC && n.Left.Op == ONAME && isRuntimePkg(n.Left.Sym.Pkg) && n.Left.Sym.Name == "getg")
                    { 
                        // Emit code for runtime.getg() directly instead of calling function.
                        // Most such rewrites (for example the similar one for math.Sqrt) should be done in walk,
                        // so that the ordering pass can make sure to preserve the semantics of the original code
                        // (in particular, the exact time of the function call) by introducing temporaries.
                        // In this case, we know getg() always returns the same result within a given function
                        // and we want to avoid the temporaries, so we do the rewrite earlier than is typical.
                        n.Op = OGETG;

                    }

                    break;

                } 

                // multiple return
                if (top & (ctxMultiOK | ctxStmt) == 0L)
                {
                    yyerror("multiple-value %v() in single-value context", l);
                    break;
                }

                n.Type = l.Type.Results();
            else if (n.Op == OALIGNOF || n.Op == OOFFSETOF || n.Op == OSIZEOF) 
                ok |= ctxExpr;
                if (!onearg(_addr_n, "%v", n.Op))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = types.Types[TUINTPTR];
            else if (n.Op == OCAP || n.Op == OLEN) 
                ok |= ctxExpr;
                if (!onearg(_addr_n, "%v", n.Op))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                n.Left = implicitstar(_addr_n.Left);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                ok = default;
                if (n.Op == OLEN)
                {
                    ok = okforlen[t.Etype];
                }
                else
                {
                    ok = okforcap[t.Etype];
                }

                if (!ok)
                {
                    yyerror("invalid argument %L for %v", l, n.Op);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = types.Types[TINT];
            else if (n.Op == OREAL || n.Op == OIMAG) 
                ok |= ctxExpr;
                if (!onearg(_addr_n, "%v", n.Op))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                } 

                // Determine result type.

                if (t.Etype == TIDEAL) 
                    n.Type = types.Idealfloat;
                else if (t.Etype == TCOMPLEX64) 
                    n.Type = types.Types[TFLOAT32];
                else if (t.Etype == TCOMPLEX128) 
                    n.Type = types.Types[TFLOAT64];
                else 
                    yyerror("invalid argument %L for %v", l, n.Op);
                    n.Type = null;
                    return _addr_n!;
                            else if (n.Op == OCOMPLEX) 
                ok |= ctxExpr;
                typecheckargs(_addr_n);
                if (!twoarg(_addr_n))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                l = n.Left;
                r = n.Right;
                if (l.Type == null || r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                l, r = defaultlit2(l, r, false);
                if (l.Type == null || r.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = l;
                n.Right = r;

                if (!types.Identical(l.Type, r.Type))
                {
                    yyerror("invalid operation: %v (mismatched types %v and %v)", n, l.Type, r.Type);
                    n.Type = null;
                    return _addr_n!;
                }

                t = ;

                if (l.Type.Etype == TIDEAL) 
                    t = types.Idealcomplex;
                else if (l.Type.Etype == TFLOAT32) 
                    t = types.Types[TCOMPLEX64];
                else if (l.Type.Etype == TFLOAT64) 
                    t = types.Types[TCOMPLEX128];
                else 
                    yyerror("invalid operation: %v (arguments have type %v, expected floating-point)", n, l.Type);
                    n.Type = null;
                    return _addr_n!;
                                n.Type = t;
            else if (n.Op == OCLOSE) 
                if (!onearg(_addr_n, "%v", n.Op))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (non-chan type %v)", n, t);
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.ChanDir().CanSend())
                {
                    yyerror("invalid operation: %v (cannot close receive-only channel)", n);
                    n.Type = null;
                    return _addr_n!;
                }

                ok |= ctxStmt;
            else if (n.Op == ODELETE) 
                ok |= ctxStmt;
                typecheckargs(_addr_n);
                var args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing arguments to delete");
                    n.Type = null;
                    return _addr_n!;
                }

                if (args.Len() == 1L)
                {
                    yyerror("missing second (key) argument to delete");
                    n.Type = null;
                    return _addr_n!;
                }

                if (args.Len() != 2L)
                {
                    yyerror("too many arguments to delete");
                    n.Type = null;
                    return _addr_n!;
                }

                l = args.First();
                r = args.Second();
                if (l.Type != null && !l.Type.IsMap())
                {
                    yyerror("first argument to delete must be map; have %L", l.Type);
                    n.Type = null;
                    return _addr_n!;
                }

                args.SetSecond(assignconv(r, l.Type.Key(), "delete"));
            else if (n.Op == OAPPEND) 
                ok |= ctxExpr;
                typecheckargs(_addr_n);
                args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing arguments to append");
                    n.Type = null;
                    return _addr_n!;
                }

                t = args.First().Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t;
                if (!t.IsSlice())
                {
                    if (Isconst(args.First(), CTNIL))
                    {
                        yyerror("first argument to append must be typed slice; have untyped nil");
                        n.Type = null;
                        return _addr_n!;
                    }

                    yyerror("first argument to append must be slice; have %L", t);
                    n.Type = null;
                    return _addr_n!;

                }

                if (n.IsDDD())
                {
                    if (args.Len() == 1L)
                    {
                        yyerror("cannot use ... on first argument to append");
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (args.Len() != 2L)
                    {
                        yyerror("too many arguments to append");
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (t.Elem().IsKind(TUINT8) && args.Second().Type.IsString())
                    {
                        args.SetSecond(defaultlit(args.Second(), types.Types[TSTRING]));
                        break;
                    }

                    args.SetSecond(assignconv(args.Second(), t.Orig, "append"));
                    break;

                }

                var @as = args.Slice()[1L..];
                {
                    var i__prev1 = i;

                    foreach (var (__i, __n) in as)
                    {
                        i = __i;
                        n = __n;
                        if (n.Type == null)
                        {
                            continue;
                        }

                        as[i] = assignconv(n, t.Elem(), "append");
                        checkwidth(as[i].Type); // ensure width is calculated for backend
                    }

                    i = i__prev1;
                }
            else if (n.Op == OCOPY) 
                ok |= ctxStmt | ctxExpr;
                typecheckargs(_addr_n);
                if (!twoarg(_addr_n))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = types.Types[TINT];
                if (n.Left.Type == null || n.Right.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = defaultlit(n.Left, null);
                n.Right = defaultlit(n.Right, null);
                if (n.Left.Type == null || n.Right.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                } 

                // copy([]byte, string)
                if (n.Left.Type.IsSlice() && n.Right.Type.IsString())
                {
                    if (types.Identical(n.Left.Type.Elem(), types.Bytetype))
                    {
                        break;
                    }

                    yyerror("arguments to copy have different element types: %L and string", n.Left.Type);
                    n.Type = null;
                    return _addr_n!;

                }

                if (!n.Left.Type.IsSlice() || !n.Right.Type.IsSlice())
                {
                    if (!n.Left.Type.IsSlice() && !n.Right.Type.IsSlice())
                    {
                        yyerror("arguments to copy must be slices; have %L, %L", n.Left.Type, n.Right.Type);
                    }
                    else if (!n.Left.Type.IsSlice())
                    {
                        yyerror("first argument to copy should be slice; have %L", n.Left.Type);
                    }
                    else
                    {
                        yyerror("second argument to copy should be slice or string; have %L", n.Right.Type);
                    }

                    n.Type = null;
                    return _addr_n!;

                }

                if (!types.Identical(n.Left.Type.Elem(), n.Right.Type.Elem()))
                {
                    yyerror("arguments to copy have different element types: %L and %L", n.Left.Type, n.Right.Type);
                    n.Type = null;
                    return _addr_n!;
                }

            else if (n.Op == OCONV) 
                ok |= ctxExpr;
                checkwidth(n.Type); // ensure width is calculated for backend
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = convlit1(n.Left, n.Type, true, null);
                t = n.Left.Type;
                if (t == null || n.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                why = default;
                n.Op = convertop(n.Left.Op == OLITERAL, t, n.Type, _addr_why);
                if (n.Op == 0L)
                {
                    if (!n.Diag() && !n.Type.Broke() && !n.Left.Diag())
                    {
                        yyerror("cannot convert %L to type %v%s", n.Left, n.Type, why);
                        n.SetDiag(true);
                    }

                    n.Op = OCONV;
                    n.Type = null;
                    return _addr_n!;

                }


                if (n.Op == OCONVNOP) 
                    if (t.Etype == n.Type.Etype)
                    {

                        if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128) 
                            // Floating point casts imply rounding and
                            // so the conversion must be kept.
                            n.Op = OCONV;
                        
                    } 

                    // do not convert to []byte literal. See CL 125796.
                    // generated code and compiler memory footprint is better without it.
                else if (n.Op == OSTR2BYTES) 
                    break;
                else if (n.Op == OSTR2RUNES) 
                    if (n.Left.Op == OLITERAL)
                    {
                        n = stringtoruneslit(_addr_n);
                    }

                            else if (n.Op == OMAKE) 
                ok |= ctxExpr;
                args = n.List.Slice();
                if (len(args) == 0L)
                {
                    yyerror("missing argument to make");
                    n.Type = null;
                    return _addr_n!;
                }

                n.List.Set(null);
                l = args[0L];
                l = typecheck(_addr_l, ctxType);
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                long i = 1L;

                if (t.Etype == TSLICE) 
                    if (i >= len(args))
                    {
                        yyerror("missing len argument to make(%v)", t);
                        n.Type = null;
                        return _addr_n!;
                    }

                    l = args[i];
                    i++;
                    l = typecheck(_addr_l, ctxExpr);
                    r = ;
                    if (i < len(args))
                    {
                        r = args[i];
                        i++;
                        r = typecheck(_addr_r, ctxExpr);
                    }

                    if (l.Type == null || (r != null && r.Type == null))
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (!checkmake(t, "len", _addr_l) || r != null && !checkmake(t, "cap", _addr_r))
                    {
                        n.Type = null;
                        return _addr_n!;
                    }

                    if (Isconst(l, CTINT) && r != null && Isconst(r, CTINT) && l.Val().U._<ptr<Mpint>>().Cmp(r.Val().U._<ptr<Mpint>>()) > 0L)
                    {
                        yyerror("len larger than cap in make(%v)", t);
                        n.Type = null;
                        return _addr_n!;
                    }

                    n.Left = l;
                    n.Right = r;
                    n.Op = OMAKESLICE;
                else if (t.Etype == TMAP) 
                    if (i < len(args))
                    {
                        l = args[i];
                        i++;
                        l = typecheck(_addr_l, ctxExpr);
                        l = defaultlit(l, types.Types[TINT]);
                        if (l.Type == null)
                        {
                            n.Type = null;
                            return _addr_n!;
                        }

                        if (!checkmake(t, "size", _addr_l))
                        {
                            n.Type = null;
                            return _addr_n!;
                        }

                        n.Left = l;

                    }
                    else
                    {
                        n.Left = nodintconst(0L);
                    }

                    n.Op = OMAKEMAP;
                else if (t.Etype == TCHAN) 
                    l = null;
                    if (i < len(args))
                    {
                        l = args[i];
                        i++;
                        l = typecheck(_addr_l, ctxExpr);
                        l = defaultlit(l, types.Types[TINT]);
                        if (l.Type == null)
                        {
                            n.Type = null;
                            return _addr_n!;
                        }

                        if (!checkmake(t, "buffer", _addr_l))
                        {
                            n.Type = null;
                            return _addr_n!;
                        }

                        n.Left = l;

                    }
                    else
                    {
                        n.Left = nodintconst(0L);
                    }

                    n.Op = OMAKECHAN;
                else 
                    yyerror("cannot make type %v", t);
                    n.Type = null;
                    return _addr_n!;
                                if (i < len(args))
                {
                    yyerror("too many arguments to make(%v)", t);
                    n.Op = OMAKE;
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t;
            else if (n.Op == ONEW) 
                ok |= ctxExpr;
                args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing argument to new");
                    n.Type = null;
                    return _addr_n!;
                }

                l = args.First();
                l = typecheck(_addr_l, ctxType);
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (args.Len() > 1L)
                {
                    yyerror("too many arguments to new(%v)", t);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = l;
                n.Type = types.NewPtr(t);
            else if (n.Op == OPRINT || n.Op == OPRINTN) 
                ok |= ctxStmt;
                typecheckargs(_addr_n);
                var ls = n.List.Slice();
                foreach (var (i1, n1) in ls)
                { 
                    // Special case for print: int constant is int64, not int.
                    if (Isconst(n1, CTINT))
                    {
                        ls[i1] = defaultlit(ls[i1], types.Types[TINT64]);
                    }
                    else
                    {
                        ls[i1] = defaultlit(ls[i1], null);
                    }

                }
            else if (n.Op == OPANIC) 
                ok |= ctxStmt;
                if (!onearg(_addr_n, "panic"))
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, types.Types[TINTER]);
                if (n.Left.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

            else if (n.Op == ORECOVER) 
                ok |= ctxExpr | ctxStmt;
                if (n.List.Len() != 0L)
                {
                    yyerror("too many arguments to recover");
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = types.Types[TINTER];
            else if (n.Op == OCLOSURE) 
                ok |= ctxExpr;
                typecheckclosure(n, top);
                if (n.Type == null)
                {
                    return _addr_n!;
                }

            else if (n.Op == OITAB) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsInterface())
                {
                    Fatalf("OITAB of %v", t);
                }

                n.Type = types.NewPtr(types.Types[TUINTPTR]);
            else if (n.Op == OIDATA) 
                // Whoever creates the OIDATA node must know a priori the concrete type at that moment,
                // usually by just having checked the OITAB.
                Fatalf("cannot typecheck interface data %v", n);
            else if (n.Op == OSPTR) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.IsSlice() && !t.IsString())
                {
                    Fatalf("OSPTR of %v", t);
                }

                if (t.IsString())
                {
                    n.Type = types.NewPtr(types.Types[TUINT8]);
                }
                else
                {
                    n.Type = types.NewPtr(t.Elem());
                }

            else if (n.Op == OCLOSUREVAR) 
                ok |= ctxExpr;
            else if (n.Op == OCFUNC) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Type = types.Types[TUINTPTR];
            else if (n.Op == OCONVNOP) 
                ok |= ctxExpr;
                n.Left = typecheck(_addr_n.Left, ctxExpr); 

                // statements
            else if (n.Op == OAS) 
                ok |= ctxStmt;

                typecheckas(_addr_n); 

                // Code that creates temps does not bother to set defn, so do it here.
                if (n.Left.Op == ONAME && n.Left.IsAutoTmp())
                {
                    n.Left.Name.Defn = n;
                }

            else if (n.Op == OAS2) 
                ok |= ctxStmt;
                typecheckas2(_addr_n);
            else if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == ODCL || n.Op == OEMPTY || n.Op == OGOTO || n.Op == OFALL || n.Op == OVARKILL || n.Op == OVARLIVE) 
                ok |= ctxStmt;
            else if (n.Op == OLABEL) 
                ok |= ctxStmt;
                decldepth++;
                if (n.Sym.IsBlank())
                { 
                    // Empty identifier is valid but useless.
                    // Eliminate now to simplify life later.
                    // See issues 7538, 11589, 11593.
                    n.Op = OEMPTY;
                    n.Left = null;

                }

            else if (n.Op == ODEFER) 
                ok |= ctxStmt;
                n.Left = typecheck(_addr_n.Left, ctxStmt | ctxExpr);
                if (!n.Left.Diag())
                {
                    checkdefergo(_addr_n);
                }

            else if (n.Op == OGO) 
                ok |= ctxStmt;
                n.Left = typecheck(_addr_n.Left, ctxStmt | ctxExpr);
                checkdefergo(_addr_n);
            else if (n.Op == OFOR || n.Op == OFORUNTIL) 
                ok |= ctxStmt;
                typecheckslice(n.Ninit.Slice(), ctxStmt);
                decldepth++;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                if (n.Left != null)
                {
                    t = n.Left.Type;
                    if (t != null && !t.IsBoolean())
                    {
                        yyerror("non-bool %L used as for condition", n.Left);
                    }

                }

                n.Right = typecheck(_addr_n.Right, ctxStmt);
                if (n.Op == OFORUNTIL)
                {
                    typecheckslice(n.List.Slice(), ctxStmt);
                }

                typecheckslice(n.Nbody.Slice(), ctxStmt);
                decldepth--;
            else if (n.Op == OIF) 
                ok |= ctxStmt;
                typecheckslice(n.Ninit.Slice(), ctxStmt);
                n.Left = typecheck(_addr_n.Left, ctxExpr);
                n.Left = defaultlit(n.Left, null);
                if (n.Left != null)
                {
                    t = n.Left.Type;
                    if (t != null && !t.IsBoolean())
                    {
                        yyerror("non-bool %L used as if condition", n.Left);
                    }

                }

                typecheckslice(n.Nbody.Slice(), ctxStmt);
                typecheckslice(n.Rlist.Slice(), ctxStmt);
            else if (n.Op == ORETURN) 
                ok |= ctxStmt;
                typecheckargs(_addr_n);
                if (Curfn == null)
                {
                    yyerror("return outside function");
                    n.Type = null;
                    return _addr_n!;
                }

                if (Curfn.Type.FuncType().Outnamed && n.List.Len() == 0L)
                {
                    break;
                }

                typecheckaste(ORETURN, _addr_null, false, _addr_Curfn.Type.Results(), n.List, () => _addr_"return argument"!);
            else if (n.Op == ORETJMP) 
                ok |= ctxStmt;
            else if (n.Op == OSELECT) 
                ok |= ctxStmt;
                typecheckselect(n);
            else if (n.Op == OSWITCH) 
                ok |= ctxStmt;
                typecheckswitch(n);
            else if (n.Op == ORANGE) 
                ok |= ctxStmt;
                typecheckrange(n);
            else if (n.Op == OTYPESW) 
                yyerror("use of .(type) outside type switch");
                n.Type = null;
                return _addr_n!;
            else if (n.Op == OCASE) 
                ok |= ctxStmt;
                typecheckslice(n.List.Slice(), ctxExpr);
                typecheckslice(n.Nbody.Slice(), ctxStmt);
            else if (n.Op == ODCLFUNC) 
                ok |= ctxStmt;
                typecheckfunc(_addr_n);
            else if (n.Op == ODCLCONST) 
                ok |= ctxStmt;
                n.Left = typecheck(_addr_n.Left, ctxExpr);
            else if (n.Op == ODCLTYPE) 
                ok |= ctxStmt;
                n.Left = typecheck(_addr_n.Left, ctxType);
                checkwidth(n.Left.Type);
                if (n.Left.Type != null && n.Left.Type.NotInHeap() && n.Left.Name.Param.Pragma & NotInHeap == 0L)
                { 
                    // The type contains go:notinheap types, so it
                    // must be marked as such (alternatively, we
                    // could silently propagate go:notinheap).
                    yyerror("type %v must be go:notinheap", n.Left.Type);

                }

            else 
                Dump("typecheck", n);

                Fatalf("typecheck %v", n.Op); 

                // names
                        t = n.Type;
            if (t != null && !t.IsFuncArgStruct() && n.Op != OTYPE)
            {

                if (t.Etype == TFUNC || t.Etype == TANY || t.Etype == TFORW || t.Etype == TIDEAL || t.Etype == TNIL || t.Etype == TBLANK) 
                    break;
                else 
                    checkwidth(t);
                
            }

            evconst(n);
            if (n.Op == OTYPE && top & ctxType == 0L)
            {
                if (!n.Type.Broke())
                {
                    yyerror("type %v is not an expression", n.Type);
                }

                n.Type = null;
                return _addr_n!;

            }

            if (top & (ctxExpr | ctxType) == ctxType && n.Op != OTYPE)
            {
                yyerror("%v is not a type", n);
                n.Type = null;
                return _addr_n!;
            } 

            // TODO(rsc): simplify
            if ((top & (ctxCallee | ctxExpr | ctxType) != 0L) && top & ctxStmt == 0L && ok & (ctxExpr | ctxType | ctxCallee) == 0L)
            {
                yyerror("%v used as value", n);
                n.Type = null;
                return _addr_n!;
            }

            if ((top & ctxStmt != 0L) && top & (ctxCallee | ctxExpr | ctxType) == 0L && ok & ctxStmt == 0L)
            {
                if (!n.Diag())
                {
                    yyerror("%v evaluated but not used", n);
                    n.SetDiag(true);
                }

                n.Type = null;
                return _addr_n!;

            }

            return _addr_n!;

        });

        private static void typecheckargs(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.List.Len() != 1L || n.IsDDD())
            {
                typecheckslice(n.List.Slice(), ctxExpr);
                return ;
            }

            typecheckslice(n.List.Slice(), ctxExpr | ctxMultiOK);
            var t = n.List.First().Type;
            if (t == null || !t.IsFuncArgStruct())
            {
                return ;
            } 

            // Rewrite f(g()) into t1, t2, ... = g(); f(t1, t2, ...).

            // Save n as n.Orig for fmt.go.
            if (n.Orig == n)
            {
                n.Orig = n.sepcopy();
            }

            var @as = nod(OAS2, null, null);
            @as.Rlist.AppendNodes(_addr_n.List); 

            // If we're outside of function context, then this call will
            // be executed during the generated init function. However,
            // init.go hasn't yet created it. Instead, associate the
            // temporary variables with dummyInitFn for now, and init.go
            // will reassociate them later when it's appropriate.
            var @static = Curfn == null;
            if (static)
            {
                Curfn = dummyInitFn;
            }

            foreach (var (_, f) in t.FieldSlice())
            {
                t = temp(f.Type);
                @as.Ninit.Append(nod(ODCL, t, null));
                @as.List.Append(t);
                n.List.Append(t);
            }
            if (static)
            {
                Curfn = null;
            }

            as = typecheck(_addr_as, ctxStmt);
            n.Ninit.Append(as);

        }

        private static bool checksliceindex(ptr<Node> _addr_l, ptr<Node> _addr_r, ptr<types.Type> _addr_tp)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;
            ref types.Type tp = ref _addr_tp.val;

            var t = r.Type;
            if (t == null)
            {
                return false;
            }

            if (!t.IsInteger())
            {
                yyerror("invalid slice index %v (type %v)", r, t);
                return false;
            }

            if (r.Op == OLITERAL)
            {
                if (r.Int64() < 0L)
                {
                    yyerror("invalid slice index %v (index must be non-negative)", r);
                    return false;
                }
                else if (tp != null && tp.NumElem() >= 0L && r.Int64() > tp.NumElem())
                {
                    yyerror("invalid slice index %v (out of bounds for %d-element array)", r, tp.NumElem());
                    return false;
                }
                else if (Isconst(l, CTSTR) && r.Int64() > int64(len(strlit(l))))
                {
                    yyerror("invalid slice index %v (out of bounds for %d-byte string)", r, len(strlit(l)));
                    return false;
                }
                else if (r.Val().U._<ptr<Mpint>>().Cmp(maxintval[TINT]) > 0L)
                {
                    yyerror("invalid slice index %v (index too large)", r);
                    return false;
                }

            }

            return true;

        }

        private static bool checksliceconst(ptr<Node> _addr_lo, ptr<Node> _addr_hi)
        {
            ref Node lo = ref _addr_lo.val;
            ref Node hi = ref _addr_hi.val;

            if (lo != null && hi != null && lo.Op == OLITERAL && hi.Op == OLITERAL && lo.Val().U._<ptr<Mpint>>().Cmp(hi.Val().U._<ptr<Mpint>>()) > 0L)
            {
                yyerror("invalid slice index: %v > %v", lo, hi);
                return false;
            }

            return true;

        }

        private static void checkdefergo(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            @string what = "defer";
            if (n.Op == OGO)
            {
                what = "go";
            }


            // ok
            if (n.Left.Op == OCALLINTER || n.Left.Op == OCALLMETH || n.Left.Op == OCALLFUNC || n.Left.Op == OCLOSE || n.Left.Op == OCOPY || n.Left.Op == ODELETE || n.Left.Op == OPANIC || n.Left.Op == OPRINT || n.Left.Op == OPRINTN || n.Left.Op == ORECOVER) 
                return ;
            else if (n.Left.Op == OAPPEND || n.Left.Op == OCAP || n.Left.Op == OCOMPLEX || n.Left.Op == OIMAG || n.Left.Op == OLEN || n.Left.Op == OMAKE || n.Left.Op == OMAKESLICE || n.Left.Op == OMAKECHAN || n.Left.Op == OMAKEMAP || n.Left.Op == ONEW || n.Left.Op == OREAL || n.Left.Op == OLITERAL) // conversion or unsafe.Alignof, Offsetof, Sizeof
                if (n.Left.Orig != null && n.Left.Orig.Op == OCONV)
                {
                    break;
                }

                yyerrorl(n.Pos, "%s discards result of %v", what, n.Left);
                return ;
            // type is broken or missing, most likely a method call on a broken type
            // we will warn about the broken type elsewhere. no need to emit a potentially confusing error
            if (n.Left.Type == null || n.Left.Type.Broke())
            {
                return ;
            }

            if (!n.Diag())
            { 
                // The syntax made sure it was a call, so this must be
                // a conversion.
                n.SetDiag(true);
                yyerrorl(n.Pos, "%s requires function call, not conversion", what);

            }

        }

        // The result of implicitstar MUST be assigned back to n, e.g.
        //     n.Left = implicitstar(n.Left)
        private static ptr<Node> implicitstar(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // insert implicit * if needed for fixed array
            var t = n.Type;
            if (t == null || !t.IsPtr())
            {
                return _addr_n!;
            }

            t = t.Elem();
            if (t == null)
            {
                return _addr_n!;
            }

            if (!t.IsArray())
            {
                return _addr_n!;
            }

            n = nod(ODEREF, n, null);
            n.SetImplicit(true);
            n = typecheck(_addr_n, ctxExpr);
            return _addr_n!;

        }

        private static bool onearg(ptr<Node> _addr_n, @string f, params object[] args)
        {
            args = args.Clone();
            ref Node n = ref _addr_n.val;

            if (n.Left != null)
            {
                return true;
            }

            if (n.List.Len() == 0L)
            {
                var p = fmt.Sprintf(f, args);
                yyerror("missing argument to %s: %v", p, n);
                return false;
            }

            if (n.List.Len() > 1L)
            {
                p = fmt.Sprintf(f, args);
                yyerror("too many arguments to %s: %v", p, n);
                n.Left = n.List.First();
                n.List.Set(null);
                return false;
            }

            n.Left = n.List.First();
            n.List.Set(null);
            return true;

        }

        private static bool twoarg(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Left != null)
            {
                return true;
            }

            if (n.List.Len() != 2L)
            {
                if (n.List.Len() < 2L)
                {
                    yyerror("not enough arguments in call to %v", n);
                }
                else
                {
                    yyerror("too many arguments in call to %v", n);
                }

                return false;

            }

            n.Left = n.List.First();
            n.Right = n.List.Second();
            n.List.Set(null);
            return true;

        }

        private static ptr<types.Field> lookdot1(ptr<Node> _addr_errnode, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<types.Fields> _addr_fs, long dostrcmp)
        {
            ref Node errnode = ref _addr_errnode.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref types.Fields fs = ref _addr_fs.val;

            ptr<types.Field> r;
            foreach (var (_, f) in fs.Slice())
            {
                if (dostrcmp != 0L && f.Sym.Name == s.Name)
                {
                    return _addr_f!;
                }

                if (dostrcmp == 2L && strings.EqualFold(f.Sym.Name, s.Name))
                {
                    return _addr_f!;
                }

                if (f.Sym != s)
                {
                    continue;
                }

                if (r != null)
                {
                    if (errnode != null)
                    {
                        yyerror("ambiguous selector %v", errnode);
                    }
                    else if (t.IsPtr())
                    {
                        yyerror("ambiguous selector (%v).%v", t, s);
                    }
                    else
                    {
                        yyerror("ambiguous selector %v.%v", t, s);
                    }

                    break;

                }

                r = f;

            }
            return _addr_r!;

        }

        // typecheckMethodExpr checks selector expressions (ODOT) where the
        // base expression is a type expression (OTYPE).
        private static ptr<Node> typecheckMethodExpr(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ptr<Node> res = default!;
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckMethodExpr", _addr_n)(_addr_res));
            }

            var t = n.Left.Type; 

            // Compute the method set for t.
            ptr<types.Fields> ms;
            if (t.IsInterface())
            {
                ms = t.Fields();
            }
            else
            {
                var mt = methtype(t);
                if (mt == null)
                {
                    yyerror("%v undefined (type %v has no method %v)", n, t, n.Sym);
                    n.Type = null;
                    return _addr_n!;
                }

                expandmeth(mt);
                ms = mt.AllMethods(); 

                // The method expression T.m requires a wrapper when T
                // is different from m's declared receiver type. We
                // normally generate these wrappers while writing out
                // runtime type descriptors, which is always done for
                // types declared at package scope. However, we need
                // to make sure to generate wrappers for anonymous
                // receiver types too.
                if (mt.Sym == null)
                {
                    addsignat(t);
                }

            }

            var s = n.Sym;
            var m = lookdot1(_addr_n, _addr_s, _addr_t, ms, 0L);
            if (m == null)
            {
                if (lookdot1(_addr_n, _addr_s, _addr_t, ms, 1L) != null)
                {
                    yyerror("%v undefined (cannot refer to unexported method %v)", n, s);
                }                {
                    var (_, ambig) = dotpath(s, t, null, false);


                    else if (ambig)
                    {
                        yyerror("%v undefined (ambiguous selector)", n); // method or field
                    }
                    else
                    {
                        yyerror("%v undefined (type %v has no method %v)", n, t, s);
                    }

                }

                n.Type = null;
                return _addr_n!;

            }

            if (!isMethodApplicable(_addr_t, _addr_m))
            {
                yyerror("invalid method expression %v (needs pointer receiver: (*%v).%S)", n, t, s);
                n.Type = null;
                return _addr_n!;
            }

            n.Op = ONAME;
            if (n.Name == null)
            {
                n.Name = @new<Name>();
            }

            n.Right = newname(n.Sym);
            n.Sym = methodSym(t, n.Sym);
            n.Type = methodfunc(m.Type, n.Left.Type);
            n.Xoffset = 0L;
            n.SetClass(PFUNC); 
            // methodSym already marked n.Sym as a function.

            // Issue 25065. Make sure that we emit the symbol for a local method.
            if (Ctxt.Flag_dynlink && !inimport && (t.Sym == null || t.Sym.Pkg == localpkg))
            {
                makefuncsym(n.Sym);
            }

            return _addr_n!;

        });

        // isMethodApplicable reports whether method m can be called on a
        // value of type t. This is necessary because we compute a single
        // method set for both T and *T, but some *T methods are not
        // applicable to T receivers.
        private static bool isMethodApplicable(ptr<types.Type> _addr_t, ptr<types.Field> _addr_m)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Field m = ref _addr_m.val;

            return t.IsPtr() || !m.Type.Recv().Type.IsPtr() || isifacemethod(m.Type) || m.Embedded == 2L;
        }

        private static ptr<types.Type> derefall(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            while (t != null && t.IsPtr())
            {
                t = t.Elem();
            }

            return _addr_t!;

        }

        private partial struct typeSymKey
        {
            public ptr<types.Type> t;
            public ptr<types.Sym> s;
        }

        // dotField maps (*types.Type, *types.Sym) pairs to the corresponding struct field (*types.Type with Etype==TFIELD).
        // It is a cache for use during usefield in walk.go, only enabled when field tracking.
        private static map dotField = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<typeSymKey, ptr<types.Field>>{};

        private static ptr<types.Field> lookdot(ptr<Node> _addr_n, ptr<types.Type> _addr_t, long dostrcmp)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            var s = n.Sym;

            dowidth(t);
            ptr<types.Field> f1;
            if (t.IsStruct() || t.IsInterface())
            {
                f1 = lookdot1(_addr_n, _addr_s, _addr_t, _addr_t.Fields(), dostrcmp);
            }

            ptr<types.Field> f2;
            if (n.Left.Type == t || n.Left.Type.Sym == null)
            {
                var mt = methtype(t);
                if (mt != null)
                {
                    f2 = lookdot1(_addr_n, _addr_s, _addr_mt, _addr_mt.Methods(), dostrcmp);
                }

            }

            if (f1 != null)
            {
                if (dostrcmp > 1L || f1.Broke())
                { 
                    // Already in the process of diagnosing an error.
                    return _addr_f1!;

                }

                if (f2 != null)
                {
                    yyerror("%v is both field and method", n.Sym);
                }

                if (f1.Offset == BADWIDTH)
                {
                    Fatalf("lookdot badwidth %v %p", f1, f1);
                }

                n.Xoffset = f1.Offset;
                n.Type = f1.Type;
                if (objabi.Fieldtrack_enabled > 0L)
                {
                    dotField[new typeSymKey(t.Orig,s)] = f1;
                }

                if (t.IsInterface())
                {
                    if (n.Left.Type.IsPtr())
                    {
                        n.Left = nod(ODEREF, n.Left, null); // implicitstar
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(_addr_n.Left, ctxExpr);

                    }

                    n.Op = ODOTINTER;

                }

                return _addr_f1!;

            }

            if (f2 != null)
            {
                if (dostrcmp > 1L)
                { 
                    // Already in the process of diagnosing an error.
                    return _addr_f2!;

                }

                var tt = n.Left.Type;
                dowidth(tt);
                var rcvr = f2.Type.Recv().Type;
                if (!types.Identical(rcvr, tt))
                {
                    if (rcvr.IsPtr() && types.Identical(rcvr.Elem(), tt))
                    {
                        checklvalue(_addr_n.Left, "call pointer method on");
                        n.Left = nod(OADDR, n.Left, null);
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(_addr_n.Left, ctxType | ctxExpr);
                    }
                    else if (tt.IsPtr() && !rcvr.IsPtr() && types.Identical(tt.Elem(), rcvr))
                    {
                        n.Left = nod(ODEREF, n.Left, null);
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(_addr_n.Left, ctxType | ctxExpr);
                    }
                    else if (tt.IsPtr() && tt.Elem().IsPtr() && types.Identical(derefall(_addr_tt), derefall(_addr_rcvr)))
                    {
                        yyerror("calling method %v with receiver %L requires explicit dereference", n.Sym, n.Left);
                        while (tt.IsPtr())
                        { 
                            // Stop one level early for method with pointer receiver.
                            if (rcvr.IsPtr() && !tt.Elem().IsPtr())
                            {
                                break;
                            }

                            n.Left = nod(ODEREF, n.Left, null);
                            n.Left.SetImplicit(true);
                            n.Left = typecheck(_addr_n.Left, ctxType | ctxExpr);
                            tt = tt.Elem();

                        }
                    else


                    }                    {
                        Fatalf("method mismatch: %v for %v", rcvr, tt);
                    }

                }

                var pll = n;
                var ll = n.Left;
                while (ll.Left != null && (ll.Op == ODOT || ll.Op == ODOTPTR || ll.Op == ODEREF))
                {
                    pll = ll;
                    ll = ll.Left;
                }

                if (pll.Implicit() && ll.Type.IsPtr() && ll.Type.Sym != null && asNode(ll.Type.Sym.Def) != null && asNode(ll.Type.Sym.Def).Op == OTYPE)
                { 
                    // It is invalid to automatically dereference a named pointer type when selecting a method.
                    // Make n.Left == ll to clarify error message.
                    n.Left = ll;
                    return _addr_null!;

                }

                n.Sym = methodSym(n.Left.Type, f2.Sym);
                n.Xoffset = f2.Offset;
                n.Type = f2.Type;
                n.Op = ODOTMETH;

                return _addr_f2!;

            }

            return _addr_null!;

        }

        private static bool nokeys(Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                if (n.Op == OKEY || n.Op == OSTRUCTKEY)
                {
                    return false;
                }

            }
            return true;

        }

        private static bool hasddd(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            foreach (var (_, tl) in t.Fields().Slice())
            {
                if (tl.IsDDD())
                {
                    return true;
                }

            }
            return false;

        }

        // typecheck assignment: type list = expression list
        private static @string typecheckaste(Op op, ptr<Node> _addr_call, bool isddd, ptr<types.Type> _addr_tstruct, Nodes nl, Func<@string> desc) => func((defer, _, __) =>
        {
            ref Node call = ref _addr_call.val;
            ref types.Type tstruct = ref _addr_tstruct.val;

            ptr<types.Type> t;
            long i = default;

            var lno = lineno;
            defer(() =>
            {
                lineno = lno;
            }());

            if (tstruct.Broke())
            {
                return ;
            }

            ptr<Node> n;
            if (nl.Len() == 1L)
            {
                n = nl.First();
            }

            var n1 = tstruct.NumFields();
            var n2 = nl.Len();
            if (!hasddd(_addr_tstruct))
            {
                if (n2 > n1)
                {
                    goto toomany;
                }

                if (n2 < n1)
                {
                    goto notenough;
                }

            }
            else
            {
                if (!isddd)
                {
                    if (n2 < n1 - 1L)
                    {
                        goto notenough;
                    }

                }
                else
                {
                    if (n2 > n1)
                    {
                        goto toomany;
                    }

                    if (n2 < n1)
                    {
                        goto notenough;
                    }

                }

            }

            i = 0L;
            foreach (var (_, tl) in tstruct.Fields().Slice())
            {
                t = tl.Type;
                if (tl.IsDDD())
                {
                    if (isddd)
                    {
                        if (i >= nl.Len())
                        {
                            goto notenough;
                        }

                        if (nl.Len() - i > 1L)
                        {
                            goto toomany;
                        }

                        n = nl.Index(i);
                        setlineno(n);
                        if (n.Type != null)
                        {
                            nl.SetIndex(i, assignconvfn(n, t, desc));
                        }

                        return ;

                    } 

                    // TODO(mdempsky): Make into ... call with implicit slice.
                    while (i < nl.Len())
                    {
                        n = nl.Index(i);
                        setlineno(n);
                        if (n.Type != null)
                        {
                            nl.SetIndex(i, assignconvfn(n, t.Elem(), desc));
                        i++;
                        }

                    }

                    return ;

                }

                if (i >= nl.Len())
                {
                    goto notenough;
                }

                n = nl.Index(i);
                setlineno(n);
                if (n.Type != null)
                {
                    nl.SetIndex(i, assignconvfn(n, t, desc));
                }

                i++;

            }
            if (i < nl.Len())
            {
                goto toomany;
            }

            if (isddd)
            {
                if (call != null)
                {
                    yyerror("invalid use of ... in call to %v", call);
                }
                else
                {
                    yyerror("invalid use of ... in %v", op);
                }

            }

            return ;

notenough:
            if (n == null || !n.Diag())
            {
                var details = errorDetails(nl, _addr_tstruct, isddd);
                if (call != null)
                { 
                    // call is the expression being called, not the overall call.
                    // Method expressions have the form T.M, and the compiler has
                    // rewritten those to ONAME nodes but left T in Left.
                    if (call.isMethodExpression())
                    {
                        yyerror("not enough arguments in call to method expression %v%s", call, details);
                    }
                    else
                    {
                        yyerror("not enough arguments in call to %v%s", call, details);
                    }

                }
                else
                {
                    yyerror("not enough arguments to %v%s", op, details);
                }

                if (n != null)
                {
                    n.SetDiag(true);
                }

            }

            return ;

toomany:
            details = errorDetails(nl, _addr_tstruct, isddd);
            if (call != null)
            {
                yyerror("too many arguments in call to %v%s", call, details);
            }
            else
            {
                yyerror("too many arguments to %v%s", op, details);
            }

        });

        private static @string errorDetails(Nodes nl, ptr<types.Type> _addr_tstruct, bool isddd)
        {
            ref types.Type tstruct = ref _addr_tstruct.val;
 
            // If we don't know any type at a call site, let's suppress any return
            // message signatures. See Issue https://golang.org/issues/19012.
            if (tstruct == null)
            {
                return "";
            } 
            // If any node has an unknown type, suppress it as well
            foreach (var (_, n) in nl.Slice())
            {
                if (n.Type == null)
                {
                    return "";
                }

            }
            return fmt.Sprintf("\n\thave %s\n\twant %v", nl.retsigerr(isddd), tstruct);

        }

        // sigrepr is a type's representation to the outside world,
        // in string representations of return signatures
        // e.g in error messages about wrong arguments to return.
        private static @string sigrepr(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t == types.Idealstring) 
                return "string";
            else if (t == types.Idealbool) 
                return "bool";
                        if (t.Etype == TIDEAL)
            { 
                // "untyped number" is not commonly used
                // outside of the compiler, so let's use "number".
                // TODO(mdempsky): Revisit this.
                return "number";

            }

            return t.String();

        }

        // retsigerr returns the signature of the types
        // at the respective return call site of a function.
        public static @string retsigerr(this Nodes nl, bool isddd)
        {
            if (nl.Len() < 1L)
            {
                return "()";
            }

            slice<@string> typeStrings = default;
            foreach (var (_, n) in nl.Slice())
            {
                typeStrings = append(typeStrings, sigrepr(_addr_n.Type));
            }
            @string ddd = "";
            if (isddd)
            {
                ddd = "...";
            }

            return fmt.Sprintf("(%s%s)", strings.Join(typeStrings, ", "), ddd);

        }

        // type check composite
        private static void fielddup(@string name, map<@string, bool> hash)
        {
            if (hash[name])
            {
                yyerror("duplicate field name in struct literal: %s", name);
                return ;
            }

            hash[name] = true;

        }

        // iscomptype reports whether type t is a composite literal type.
        private static bool iscomptype(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TSTRUCT || t.Etype == TMAP) 
                return true;
            else 
                return false;
            
        }

        // pushtype adds elided type information for composite literals if
        // appropriate, and returns the resulting expression.
        private static ptr<Node> pushtype(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (n == null || n.Op != OCOMPLIT || n.Right != null)
            {
                return _addr_n!;
            }


            if (iscomptype(_addr_t)) 
                // For T, return T{...}.
                n.Right = typenod(t);
            else if (t.IsPtr() && iscomptype(_addr_t.Elem())) 
                // For *T, return &T{...}.
                n.Right = typenod(t.Elem());

                n = nodl(n.Pos, OADDR, n, null);
                n.SetImplicit(true);
                        return _addr_n!;

        }

        // The result of typecheckcomplit MUST be assigned back to n, e.g.
        //     n.Left = typecheckcomplit(n.Left)
        private static ptr<Node> typecheckcomplit(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ptr<Node> res = default!;
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckcomplit", _addr_n)(_addr_res));
            }

            var lno = lineno;
            defer(() =>
            {
                lineno = lno;
            }());

            if (n.Right == null)
            {
                yyerrorl(n.Pos, "missing type in composite literal");
                n.Type = null;
                return _addr_n!;
            } 

            // Save original node (including n.Right)
            n.Orig = n.copy();

            setlineno(n.Right); 

            // Need to handle [...]T arrays specially.
            if (n.Right.Op == OTARRAY && n.Right.Left != null && n.Right.Left.Op == ODDD)
            {
                n.Right.Right = typecheck(_addr_n.Right.Right, ctxType);
                if (n.Right.Right.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                var elemType = n.Right.Right.Type;

                var length = typecheckarraylit(_addr_elemType, -1L, n.List.Slice(), "array literal");

                n.Op = OARRAYLIT;
                n.Type = types.NewArray(elemType, length);
                n.Right = null;
                return _addr_n!;

            }

            n.Right = typecheck(_addr_n.Right, ctxType);
            var t = n.Right.Type;
            if (t == null)
            {
                n.Type = null;
                return _addr_n!;
            }

            n.Type = t;


            if (t.Etype == TARRAY) 
                typecheckarraylit(_addr_t.Elem(), t.NumElem(), n.List.Slice(), "array literal");
                n.Op = OARRAYLIT;
                n.Right = null;
            else if (t.Etype == TSLICE) 
                length = typecheckarraylit(_addr_t.Elem(), -1L, n.List.Slice(), "slice literal");
                n.Op = OSLICELIT;
                n.Right = nodintconst(length);
            else if (t.Etype == TMAP) 
                constSet cs = default;
                {
                    var l__prev1 = l;

                    foreach (var (__i3, __l) in n.List.Slice())
                    {
                        i3 = __i3;
                        l = __l;
                        setlineno(l);
                        if (l.Op != OKEY)
                        {
                            n.List.SetIndex(i3, typecheck(_addr_l, ctxExpr));
                            yyerror("missing key in map literal");
                            continue;
                        }

                        var r = l.Left;
                        r = pushtype(_addr_r, _addr_t.Key());
                        r = typecheck(_addr_r, ctxExpr);
                        l.Left = assignconv(r, t.Key(), "map key");
                        cs.add(lineno, l.Left, "key", "map literal");

                        r = l.Right;
                        r = pushtype(_addr_r, _addr_t.Elem());
                        r = typecheck(_addr_r, ctxExpr);
                        l.Right = assignconv(r, t.Elem(), "map value");

                    }

                    l = l__prev1;
                }

                n.Op = OMAPLIT;
                n.Right = null;
            else if (t.Etype == TSTRUCT) 
                // Need valid field offsets for Xoffset below.
                dowidth(t);

                var errored = false;
                if (n.List.Len() != 0L && nokeys(n.List))
                { 
                    // simple list of variables
                    var ls = n.List.Slice();
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __n1) in ls)
                        {
                            i = __i;
                            n1 = __n1;
                            setlineno(n1);
                            n1 = typecheck(_addr_n1, ctxExpr);
                            ls[i] = n1;
                            if (i >= t.NumFields())
                            {
                                if (!errored)
                                {
                                    yyerror("too many values in %v", n);
                                    errored = true;
                                }

                                continue;

                            }

                            ref var f = ref heap(t.Field(i), out ptr<var> _addr_f);
                            var s = f.Sym;
                            if (s != null && !types.IsExported(s.Name) && s.Pkg != localpkg)
                            {
                                yyerror("implicit assignment of unexported field '%s' in %v literal", s.Name, t);
                            } 
                            // No pushtype allowed here. Must name fields for that.
                            n1 = assignconv(n1, f.Type, "field value");
                            n1 = nodSym(OSTRUCTKEY, n1, f.Sym);
                            n1.Xoffset = f.Offset;
                            ls[i] = n1;

                        }
                else

                        i = i__prev1;
                    }

                    if (len(ls) < t.NumFields())
                    {
                        yyerror("too few values in %v", n);
                    }

                }                {
                    var hash = make_map<@string, bool>(); 

                    // keyed list
                    ls = n.List.Slice();
                    {
                        var i__prev1 = i;
                        var l__prev1 = l;

                        foreach (var (__i, __l) in ls)
                        {
                            i = __i;
                            l = __l;
                            setlineno(l);

                            if (l.Op == OKEY)
                            {
                                var key = l.Left;

                                l.Op = OSTRUCTKEY;
                                l.Left = l.Right;
                                l.Right = null; 

                                // An OXDOT uses the Sym field to hold
                                // the field to the right of the dot,
                                // so s will be non-nil, but an OXDOT
                                // is never a valid struct literal key.
                                if (key.Sym == null || key.Op == OXDOT || key.Sym.IsBlank())
                                {
                                    yyerror("invalid field name %v in struct initializer", key);
                                    l.Left = typecheck(_addr_l.Left, ctxExpr);
                                    continue;
                                } 

                                // Sym might have resolved to name in other top-level
                                // package, because of import dot. Redirect to correct sym
                                // before we do the lookup.
                                s = key.Sym;
                                if (s.Pkg != localpkg && types.IsExported(s.Name))
                                {
                                    var s1 = lookup(s.Name);
                                    if (s1.Origpkg == s.Pkg)
                                    {
                                        s = s1;
                                    }

                                }

                                l.Sym = s;

                            }

                            if (l.Op != OSTRUCTKEY)
                            {
                                if (!errored)
                                {
                                    yyerror("mixture of field:value and value initializers");
                                    errored = true;
                                }

                                ls[i] = typecheck(_addr_ls[i], ctxExpr);
                                continue;

                            }

                            f = lookdot1(_addr_null, _addr_l.Sym, _addr_t, _addr_t.Fields(), 0L);
                            if (f == null)
                            {
                                {
                                    var ci = lookdot1(_addr_null, _addr_l.Sym, _addr_t, _addr_t.Fields(), 2L);

                                    if (ci != null)
                                    { // Case-insensitive lookup.
                                        if (visible(_addr_ci.Sym))
                                        {
                                            yyerror("unknown field '%v' in struct literal of type %v (but does have %v)", l.Sym, t, ci.Sym);
                                        }
                                        else if (nonexported(_addr_l.Sym) && l.Sym.Name == ci.Sym.Name)
                                        { // Ensure exactness before the suggestion.
                                            yyerror("cannot refer to unexported field '%v' in struct literal of type %v", l.Sym, t);

                                        }
                                        else
                                        {
                                            yyerror("unknown field '%v' in struct literal of type %v", l.Sym, t);
                                        }

                                        continue;

                                    }

                                }

                                f = ;
                                var (p, _) = dotpath(l.Sym, t, _addr_f, true);
                                if (p == null || f.IsMethod())
                                {
                                    yyerror("unknown field '%v' in struct literal of type %v", l.Sym, t);
                                    continue;
                                } 
                                // dotpath returns the parent embedded types in reverse order.
                                slice<@string> ep = default;
                                for (var ei = len(p) - 1L; ei >= 0L; ei--)
                                {
                                    ep = append(ep, p[ei].field.Sym.Name);
                                }

                                ep = append(ep, l.Sym.Name);
                                yyerror("cannot use promoted field %v in struct literal of type %v", strings.Join(ep, "."), t);
                                continue;

                            }

                            fielddup(f.Sym.Name, hash);
                            l.Xoffset = f.Offset; 

                            // No pushtype allowed here. Tried and rejected.
                            l.Left = typecheck(_addr_l.Left, ctxExpr);
                            l.Left = assignconv(l.Left, f.Type, "field value");

                        }

                        i = i__prev1;
                        l = l__prev1;
                    }
                }

                n.Op = OSTRUCTLIT;
                n.Right = null;
            else 
                yyerror("invalid composite literal type %v", t);
                n.Type = null;
                        return _addr_n!;

        });

        // typecheckarraylit type-checks a sequence of slice/array literal elements.
        private static long typecheckarraylit(ptr<types.Type> _addr_elemType, long bound, slice<ptr<Node>> elts, @string ctx)
        {
            ref types.Type elemType = ref _addr_elemType.val;
 
            // If there are key/value pairs, create a map to keep seen
            // keys so we can check for duplicate indices.
            map<long, bool> indices = default;
            {
                var elt__prev1 = elt;

                foreach (var (_, __elt) in elts)
                {
                    elt = __elt;
                    if (elt.Op == OKEY)
                    {
                        indices = make_map<long, bool>();
                        break;
                    }

                }

                elt = elt__prev1;
            }

            long key = default;            long length = default;

            {
                var elt__prev1 = elt;

                foreach (var (__i, __elt) in elts)
                {
                    i = __i;
                    elt = __elt;
                    setlineno(elt);
                    var vp = _addr_elts[i];
                    if (elt.Op == OKEY)
                    {
                        elt.Left = typecheck(_addr_elt.Left, ctxExpr);
                        key = indexconst(elt.Left);
                        if (key < 0L)
                        {
                            if (!elt.Left.Diag())
                            {
                                if (key == -2L)
                                {
                                    yyerror("index too large");
                                }
                                else
                                {
                                    yyerror("index must be non-negative integer constant");
                                }

                                elt.Left.SetDiag(true);

                            }

                            key = -(1L << (int)(30L)); // stay negative for a while
                        }

                        vp = _addr_elt.Right;

                    }

                    var r = vp.val;
                    r = pushtype(_addr_r, _addr_elemType);
                    r = typecheck(_addr_r, ctxExpr);
                    vp.val = assignconv(r, elemType, ctx);

                    if (key >= 0L)
                    {
                        if (indices != null)
                        {
                            if (indices[key])
                            {
                                yyerror("duplicate index in %s: %d", ctx, key);
                            }
                            else
                            {
                                indices[key] = true;
                            }

                        }

                        if (bound >= 0L && key >= bound)
                        {
                            yyerror("array index %d out of bounds [0:%d]", key, bound);
                            bound = -1L;
                        }

                    }

                    key++;
                    if (key > length)
                    {
                        length = key;
                    }

                }

                elt = elt__prev1;
            }

            return length;

        }

        // visible reports whether sym is exported or locally defined.
        private static bool visible(ptr<types.Sym> _addr_sym)
        {
            ref types.Sym sym = ref _addr_sym.val;

            return sym != null && (types.IsExported(sym.Name) || sym.Pkg == localpkg);
        }

        // nonexported reports whether sym is an unexported field.
        private static bool nonexported(ptr<types.Sym> _addr_sym)
        {
            ref types.Sym sym = ref _addr_sym.val;

            return sym != null && !types.IsExported(sym.Name);
        }

        // lvalue etc
        private static bool islvalue(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OINDEX)
            {
                if (n.Left.Type != null && n.Left.Type.IsArray())
                {
                    return islvalue(_addr_n.Left);
                }

                if (n.Left.Type != null && n.Left.Type.IsString())
                {
                    return false;
                }

                fallthrough = true;
            }
            if (fallthrough || n.Op == ODEREF || n.Op == ODOTPTR || n.Op == OCLOSUREVAR)
            {
                return true;
                goto __switch_break0;
            }
            if (n.Op == ODOT)
            {
                return islvalue(_addr_n.Left);
                goto __switch_break0;
            }
            if (n.Op == ONAME)
            {
                if (n.Class() == PFUNC)
                {
                    return false;
                }

                return true;
                goto __switch_break0;
            }

            __switch_break0:;

            return false;

        }

        private static void checklvalue(ptr<Node> _addr_n, @string verb)
        {
            ref Node n = ref _addr_n.val;

            if (!islvalue(_addr_n))
            {
                yyerror("cannot %s %v", verb, n);
            }

        }

        private static void checkassign(ptr<Node> _addr_stmt, ptr<Node> _addr_n)
        {
            ref Node stmt = ref _addr_stmt.val;
            ref Node n = ref _addr_n.val;
 
            // Variables declared in ORANGE are assigned on every iteration.
            if (n.Name == null || n.Name.Defn != stmt || stmt.Op == ORANGE)
            {
                var r = outervalue(n);
                if (r.Op == ONAME)
                {
                    r.Name.SetAssigned(true);
                    if (r.Name.IsClosureVar())
                    {
                        r.Name.Defn.Name.SetAssigned(true);
                    }

                }

            }

            if (islvalue(_addr_n))
            {
                return ;
            }

            if (n.Op == OINDEXMAP)
            {
                n.SetIndexMapLValue(true);
                return ;
            } 

            // have already complained about n being invalid
            if (n.Type == null)
            {
                return ;
            }

            if (n.Op == ODOT && n.Left.Op == OINDEXMAP)
            {
                yyerror("cannot assign to struct field %v in map", n);
            }
            else
            {
                yyerror("cannot assign to %v", n);
            }

            n.Type = null;

        }

        private static void checkassignlist(ptr<Node> _addr_stmt, Nodes l)
        {
            ref Node stmt = ref _addr_stmt.val;

            foreach (var (_, n) in l.Slice())
            {
                checkassign(_addr_stmt, _addr_n);
            }

        }

        // samesafeexpr checks whether it is safe to reuse one of l and r
        // instead of computing both. samesafeexpr assumes that l and r are
        // used in the same statement or expression. In order for it to be
        // safe to reuse l or r, they must:
        // * be the same expression
        // * not have side-effects (no function calls, no channel ops);
        //   however, panics are ok
        // * not cause inappropriate aliasing; e.g. two string to []byte
        //   conversions, must result in two distinct slices
        //
        // The handling of OINDEXMAP is subtle. OINDEXMAP can occur both
        // as an lvalue (map assignment) and an rvalue (map access). This is
        // currently OK, since the only place samesafeexpr gets used on an
        // lvalue expression is for OSLICE and OAPPEND optimizations, and it
        // is correct in those settings.
        private static bool samesafeexpr(ptr<Node> _addr_l, ptr<Node> _addr_r)
        {
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;

            if (l.Op != r.Op || !types.Identical(l.Type, r.Type))
            {
                return false;
            }


            if (l.Op == ONAME || l.Op == OCLOSUREVAR) 
                return l == r;
            else if (l.Op == ODOT || l.Op == ODOTPTR) 
                return l.Sym != null && r.Sym != null && l.Sym == r.Sym && samesafeexpr(_addr_l.Left, _addr_r.Left);
            else if (l.Op == ODEREF || l.Op == OCONVNOP || l.Op == ONOT || l.Op == OBITNOT || l.Op == OPLUS || l.Op == ONEG) 
                return samesafeexpr(_addr_l.Left, _addr_r.Left);
            else if (l.Op == OCONV) 
                // Some conversions can't be reused, such as []byte(str).
                // Allow only numeric-ish types. This is a bit conservative.
                return issimple[l.Type.Etype] && samesafeexpr(_addr_l.Left, _addr_r.Left);
            else if (l.Op == OINDEX || l.Op == OINDEXMAP || l.Op == OADD || l.Op == OSUB || l.Op == OOR || l.Op == OXOR || l.Op == OMUL || l.Op == OLSH || l.Op == ORSH || l.Op == OAND || l.Op == OANDNOT || l.Op == ODIV || l.Op == OMOD) 
                return samesafeexpr(_addr_l.Left, _addr_r.Left) && samesafeexpr(_addr_l.Right, _addr_r.Right);
            else if (l.Op == OLITERAL) 
                return eqval(l.Val(), r.Val());
                        return false;

        }

        // type check assignment.
        // if this assignment is the definition of a var on the left side,
        // fill in the var's type.
        private static void typecheckas(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckas", _addr_n)(null));
            } 

            // delicate little dance.
            // the definition of n may refer to this assignment
            // as its definition, in which case it will call typecheckas.
            // in that case, do not call typecheck back, or it will cycle.
            // if the variable has a type (ntype) then typechecking
            // will not look at defn, so it is okay (and desirable,
            // so that the conversion below happens).
            n.Left = resolve(_addr_n.Left);

            if (n.Left.Name == null || n.Left.Name.Defn != n || n.Left.Name.Param.Ntype != null)
            {
                n.Left = typecheck(_addr_n.Left, ctxExpr | ctxAssign);
            } 

            // Use ctxMultiOK so we can emit an "N variables but M values" error
            // to be consistent with typecheckas2 (#26616).
            n.Right = typecheck(_addr_n.Right, ctxExpr | ctxMultiOK);
            checkassign(_addr_n, _addr_n.Left);
            if (n.Right != null && n.Right.Type != null)
            {
                if (n.Right.Type.IsFuncArgStruct())
                {
                    yyerror("assignment mismatch: 1 variable but %v returns %d values", n.Right.Left, n.Right.Type.NumFields()); 
                    // Multi-value RHS isn't actually valid for OAS; nil out
                    // to indicate failed typechecking.
                    n.Right.Type = null;

                }
                else if (n.Left.Type != null)
                {
                    n.Right = assignconv(n.Right, n.Left.Type, "assignment");
                }

            }

            if (n.Left.Name != null && n.Left.Name.Defn == n && n.Left.Name.Param.Ntype == null)
            {
                n.Right = defaultlit(n.Right, null);
                n.Left.Type = n.Right.Type;
            } 

            // second half of dance.
            // now that right is done, typecheck the left
            // just to get it over with.  see dance above.
            n.SetTypecheck(1L);

            if (n.Left.Typecheck() == 0L)
            {
                n.Left = typecheck(_addr_n.Left, ctxExpr | ctxAssign);
            }

            if (!n.Left.isBlank())
            {
                checkwidth(n.Left.Type); // ensure width is calculated for backend
            }

        });

        private static void checkassignto(ptr<types.Type> _addr_src, ptr<Node> _addr_dst)
        {
            ref types.Type src = ref _addr_src.val;
            ref Node dst = ref _addr_dst.val;

            ref @string why = ref heap(out ptr<@string> _addr_why);

            if (assignop(src, dst.Type, _addr_why) == 0L)
            {
                yyerror("cannot assign %v to %L in multiple assignment%s", src, dst, why);
                return ;
            }

        }

        private static void typecheckas2(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckas2", _addr_n)(null));
            }

            var ls = n.List.Slice();
            {
                var i1__prev1 = i1;
                var n1__prev1 = n1;

                foreach (var (__i1, __n1) in ls)
                {
                    i1 = __i1;
                    n1 = __n1; 
                    // delicate little dance.
                    n1 = resolve(_addr_n1);
                    ls[i1] = n1;

                    if (n1.Name == null || n1.Name.Defn != n || n1.Name.Param.Ntype != null)
                    {
                        ls[i1] = typecheck(_addr_ls[i1], ctxExpr | ctxAssign);
                    }

                }

                i1 = i1__prev1;
                n1 = n1__prev1;
            }

            var cl = n.List.Len();
            var cr = n.Rlist.Len();
            if (cl > 1L && cr == 1L)
            {
                n.Rlist.SetFirst(typecheck(_addr_n.Rlist.First(), ctxExpr | ctxMultiOK));
            }
            else
            {
                typecheckslice(n.Rlist.Slice(), ctxExpr);
            }

            checkassignlist(_addr_n, n.List);

            ptr<Node> l;
            ptr<Node> r;
            if (cl == cr)
            { 
                // easy
                ls = n.List.Slice();
                var rs = n.Rlist.Slice();
                foreach (var (il, nl) in ls)
                {
                    var nr = rs[il];
                    if (nl.Type != null && nr.Type != null)
                    {
                        rs[il] = assignconv(nr, nl.Type, "assignment");
                    }

                    if (nl.Name != null && nl.Name.Defn == n && nl.Name.Param.Ntype == null)
                    {
                        rs[il] = defaultlit(rs[il], null);
                        nl.Type = rs[il].Type;
                    }

                }
                goto @out;

            }

            l = n.List.First();
            r = n.Rlist.First(); 

            // x,y,z = f()
            if (cr == 1L)
            {
                if (r.Type == null)
                {
                    goto @out;
                }


                if (r.Op == OCALLMETH || r.Op == OCALLINTER || r.Op == OCALLFUNC) 
                    if (!r.Type.IsFuncArgStruct())
                    {
                        break;
                    }

                    cr = r.Type.NumFields();
                    if (cr != cl)
                    {
                        goto mismatch;
                    }

                    n.Op = OAS2FUNC;
                    n.Right = r;
                    n.Rlist.Set(null);
                    {
                        ptr<Node> l__prev1 = l;

                        foreach (var (__i, __l) in n.List.Slice())
                        {
                            i = __i;
                            l = __l;
                            var f = r.Type.Field(i);
                            if (f.Type != null && l.Type != null)
                            {
                                checkassignto(_addr_f.Type, l);
                            }

                            if (l.Name != null && l.Name.Defn == n && l.Name.Param.Ntype == null)
                            {
                                l.Type = f.Type;
                            }

                        }

                        l = l__prev1;
                    }

                    goto @out;
                
            } 

            // x, ok = y
            if (cl == 2L && cr == 1L)
            {
                if (r.Type == null)
                {
                    goto @out;
                }


                if (r.Op == OINDEXMAP || r.Op == ORECV || r.Op == ODOTTYPE) 

                    if (r.Op == OINDEXMAP) 
                        n.Op = OAS2MAPR;
                    else if (r.Op == ORECV) 
                        n.Op = OAS2RECV;
                    else if (r.Op == ODOTTYPE) 
                        n.Op = OAS2DOTTYPE;
                        r.Op = ODOTTYPE2;
                                        n.Right = r;
                    n.Rlist.Set(null);
                    if (l.Type != null)
                    {
                        checkassignto(_addr_r.Type, l);
                    }

                    if (l.Name != null && l.Name.Defn == n)
                    {
                        l.Type = r.Type;
                    }

                    l = n.List.Second();
                    if (l.Type != null && !l.Type.IsBoolean())
                    {
                        checkassignto(_addr_types.Types[TBOOL], l);
                    }

                    if (l.Name != null && l.Name.Defn == n && l.Name.Param.Ntype == null)
                    {
                        l.Type = types.Types[TBOOL];
                    }

                    goto @out;
                
            }

mismatch: 

            // second half of dance

            if (r.Op == OCALLFUNC || r.Op == OCALLMETH || r.Op == OCALLINTER) 
                yyerror("assignment mismatch: %d variables but %v returns %d values", cl, r.Left, cr);
            else 
                yyerror("assignment mismatch: %d variables but %d values", cl, cr);
            // second half of dance
@out:
            n.SetTypecheck(1L);
            ls = n.List.Slice();
            {
                var i1__prev1 = i1;
                var n1__prev1 = n1;

                foreach (var (__i1, __n1) in ls)
                {
                    i1 = __i1;
                    n1 = __n1;
                    if (n1.Typecheck() == 0L)
                    {
                        ls[i1] = typecheck(_addr_ls[i1], ctxExpr | ctxAssign);
                    }

                }

                i1 = i1__prev1;
                n1 = n1__prev1;
            }
        });

        // type check function definition
        private static void typecheckfunc(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckfunc", _addr_n)(null));
            }

            foreach (var (_, ln) in n.Func.Dcl)
            {
                if (ln.Op == ONAME && (ln.Class() == PPARAM || ln.Class() == PPARAMOUT))
                {
                    ln.Name.Decldepth = 1L;
                }

            }
            n.Func.Nname = typecheck(_addr_n.Func.Nname, ctxExpr | ctxAssign);
            var t = n.Func.Nname.Type;
            if (t == null)
            {
                return ;
            }

            n.Type = t;
            t.FuncType().Nname = asTypesNode(n.Func.Nname);
            var rcvr = t.Recv();
            if (rcvr != null && n.Func.Shortname != null)
            {
                var m = addmethod(n.Func.Shortname, t, true, n.Func.Pragma & Nointerface != 0L);
                if (m == null)
                {
                    return ;
                }

                n.Func.Nname.Sym = methodSym(rcvr.Type, n.Func.Shortname);
                declare(n.Func.Nname, PFUNC);

            }

            if (Ctxt.Flag_dynlink && !inimport && n.Func.Nname != null)
            {
                makefuncsym(n.Func.Nname.Sym);
            }

        });

        // The result of stringtoruneslit MUST be assigned back to n, e.g.
        //     n.Left = stringtoruneslit(n.Left)
        private static ptr<Node> stringtoruneslit(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Left.Op != OLITERAL || n.Left.Val().Ctype() != CTSTR)
            {
                Fatalf("stringtoarraylit %v", n);
            }

            slice<ptr<Node>> l = default;
            var s = strlit(n.Left);
            long i = 0L;
            foreach (var (_, r) in s)
            {
                l = append(l, nod(OKEY, nodintconst(int64(i)), nodintconst(int64(r))));
                i++;
            }
            var nn = nod(OCOMPLIT, null, typenod(n.Type));
            nn.List.Set(l);
            nn = typecheck(_addr_nn, ctxExpr);
            return _addr_nn!;

        }

        private static slice<ptr<Node>> mapqueue = default;

        private static void checkMapKeys()
        {
            foreach (var (_, n) in mapqueue)
            {
                var k = n.Type.MapType().Key;
                if (!k.Broke() && !IsComparable(k))
                {
                    yyerrorl(n.Pos, "invalid map key type %v", k);
                }

            }
            mapqueue = null;

        }

        private static void setUnderlying(ptr<types.Type> _addr_t, ptr<types.Type> _addr_underlying)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Type underlying = ref _addr_underlying.val;

            if (underlying.Etype == TFORW)
            { 
                // This type isn't computed yet; when it is, update n.
                underlying.ForwardType().Copyto = append(underlying.ForwardType().Copyto, t);
                return ;

            }

            var n = asNode(t.Nod);
            var ft = t.ForwardType();
            var cache = t.Cache; 

            // TODO(mdempsky): Fix Type rekinding.
            t = underlying; 

            // Restore unnecessarily clobbered attributes.
            t.Nod = asTypesNode(n);
            t.Sym = n.Sym;
            if (n.Name != null)
            {
                t.Vargen = n.Name.Vargen;
            }

            t.Cache = cache;
            t.SetDeferwidth(false); 

            // spec: "The declared type does not inherit any methods bound
            // to the existing type, but the method set of an interface
            // type [...] remains unchanged."
            if (!t.IsInterface())
            {
                t.Methods().val = new types.Fields();
                t.AllMethods().val = new types.Fields();
            } 

            // Propagate go:notinheap pragma from the Name to the Type.
            if (n.Name != null && n.Name.Param != null && n.Name.Param.Pragma & NotInHeap != 0L)
            {
                t.SetNotInHeap(true);
            } 

            // Update types waiting on this type.
            foreach (var (_, w) in ft.Copyto)
            {
                setUnderlying(_addr_w, _addr_t);
            } 

            // Double-check use of type as embedded type.
            if (ft.Embedlineno.IsKnown())
            {
                if (t.IsPtr() || t.IsUnsafePtr())
                {
                    yyerrorl(ft.Embedlineno, "embedded type cannot be a pointer");
                }

            }

        }

        private static void typecheckdeftype(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckdeftype", _addr_n)(null));
            }

            n.SetTypecheck(1L);
            n.Name.Param.Ntype = typecheck(_addr_n.Name.Param.Ntype, ctxType);
            var t = n.Name.Param.Ntype.Type;
            if (t == null)
            {
                n.SetDiag(true);
                n.Type = null;
            }
            else if (n.Type == null)
            {
                n.SetDiag(true);
            }
            else
            { 
                // copy new type and clear fields
                // that don't come along.
                setUnderlying(_addr_n.Type, _addr_t);

            }

        });

        private static void typecheckdef(ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref Node n = ref _addr_n.val;

            if (enableTrace && trace)
            {
                defer(tracePrint("typecheckdef", _addr_n)(null));
            }

            var lno = setlineno(n);

            if (n.Op == ONONAME)
            {
                if (!n.Diag())
                {
                    n.SetDiag(true); 

                    // Note: adderrorname looks for this string and
                    // adds context about the outer expression
                    yyerrorl(lineno, "undefined: %v", n.Sym);

                }

                lineno = lno;
                return ;

            }

            if (n.Walkdef() == 1L)
            {
                lineno = lno;
                return ;
            }

            typecheckdefstack = append(typecheckdefstack, n);
            if (n.Walkdef() == 2L)
            {
                flusherrors();
                fmt.Printf("typecheckdef loop:");
                for (var i = len(typecheckdefstack) - 1L; i >= 0L; i--)
                {
                    var n = typecheckdefstack[i];
                    fmt.Printf(" %v", n.Sym);
                }

                fmt.Printf("\n");
                Fatalf("typecheckdef loop");

            }

            n.SetWalkdef(2L);

            if (n.Type != null || n.Sym == null)
            { // builtin or no name
                goto ret;

            }


            if (n.Op == OLITERAL) 
                if (n.Name.Param.Ntype != null)
                {
                    n.Name.Param.Ntype = typecheck(_addr_n.Name.Param.Ntype, ctxType);
                    n.Type = n.Name.Param.Ntype.Type;
                    n.Name.Param.Ntype = null;
                    if (n.Type == null)
                    {
                        n.SetDiag(true);
                        goto ret;
                    }

                }

                var e = n.Name.Defn;
                n.Name.Defn = null;
                if (e == null)
                {
                    Dump("typecheckdef nil defn", n);
                    yyerrorl(n.Pos, "xxx");
                }

                e = typecheck(_addr_e, ctxExpr);
                if (e.Type == null)
                {
                    goto ret;
                }

                if (!e.isGoConst())
                {
                    if (!e.Diag())
                    {
                        if (Isconst(e, CTNIL))
                        {
                            yyerrorl(n.Pos, "const initializer cannot be nil");
                        }
                        else
                        {
                            yyerrorl(n.Pos, "const initializer %v is not a constant", e);
                        }

                        e.SetDiag(true);

                    }

                    goto ret;

                }

                var t = n.Type;
                if (t != null)
                {
                    if (!okforconst[t.Etype])
                    {
                        yyerrorl(n.Pos, "invalid constant type %v", t);
                        goto ret;
                    }

                    if (!e.Type.IsUntyped() && !types.Identical(t, e.Type))
                    {
                        yyerrorl(n.Pos, "cannot use %L as type %v in const initializer", e, t);
                        goto ret;
                    }

                    e = convlit(e, t);

                }

                n.SetVal(e.Val());
                n.Type = e.Type;
            else if (n.Op == ONAME) 
                if (n.Name.Param.Ntype != null)
                {
                    n.Name.Param.Ntype = typecheck(_addr_n.Name.Param.Ntype, ctxType);
                    n.Type = n.Name.Param.Ntype.Type;
                    if (n.Type == null)
                    {
                        n.SetDiag(true);
                        goto ret;
                    }

                }

                if (n.Type != null)
                {
                    break;
                }

                if (n.Name.Defn == null)
                {
                    if (n.SubOp() != 0L)
                    { // like OPRINTN
                        break;

                    }

                    if (nsavederrors + nerrors > 0L)
                    { 
                        // Can have undefined variables in x := foo
                        // that make x have an n.name.Defn == nil.
                        // If there are other errors anyway, don't
                        // bother adding to the noise.
                        break;

                    }

                    Fatalf("var without type, init: %v", n.Sym);

                }

                if (n.Name.Defn.Op == ONAME)
                {
                    n.Name.Defn = typecheck(_addr_n.Name.Defn, ctxExpr);
                    n.Type = n.Name.Defn.Type;
                    break;
                }

                n.Name.Defn = typecheck(_addr_n.Name.Defn, ctxStmt); // fills in n.Type
            else if (n.Op == OTYPE) 
                {
                    var p = n.Name.Param;

                    if (p.Alias)
                    { 
                        // Type alias declaration: Simply use the rhs type - no need
                        // to create a new type.
                        // If we have a syntax error, p.Ntype may be nil.
                        if (p.Ntype != null)
                        {
                            p.Ntype = typecheck(_addr_p.Ntype, ctxType);
                            n.Type = p.Ntype.Type;
                            if (n.Type == null)
                            {
                                n.SetDiag(true);
                                goto ret;
                            } 
                            // For package-level type aliases, set n.Sym.Def so we can identify
                            // it as a type alias during export. See also #31959.
                            if (n.Name.Curfn == null)
                            {
                                n.Sym.Def = asTypesNode(p.Ntype);
                            }

                        }

                        break;

                    } 

                    // regular type declaration

                } 

                // regular type declaration
                defercheckwidth();
                n.SetWalkdef(1L);
                setTypeNode(_addr_n, _addr_types.New(TFORW));
                n.Type.Sym = n.Sym;
                var nerrors0 = nerrors;
                typecheckdeftype(_addr_n);
                if (n.Type.Etype == TFORW && nerrors > nerrors0)
                { 
                    // Something went wrong during type-checking,
                    // but it was reported. Silence future errors.
                    n.Type.SetBroke(true);

                }

                resumecheckwidth();
            else 
                Fatalf("typecheckdef %v", n.Op);
            ret:
            if (n.Op != OLITERAL && n.Type != null && n.Type.IsUntyped())
            {
                Fatalf("got %v for %v", n.Type, n);
            }

            var last = len(typecheckdefstack) - 1L;
            if (typecheckdefstack[last] != n)
            {
                Fatalf("typecheckdefstack mismatch");
            }

            typecheckdefstack[last] = null;
            typecheckdefstack = typecheckdefstack[..last];

            lineno = lno;
            n.SetWalkdef(1L);

        });

        private static bool checkmake(ptr<types.Type> _addr_t, @string arg, ptr<Node> _addr_n)
        {
            ref types.Type t = ref _addr_t.val;
            ref Node n = ref _addr_n.val;

            if (!n.Type.IsInteger() && n.Type.Etype != TIDEAL)
            {
                yyerror("non-integer %s argument in make(%v) - %v", arg, t, n.Type);
                return false;
            } 

            // Do range checks for constants before defaultlit
            // to avoid redundant "constant NNN overflows int" errors.

            if (consttype(n) == CTINT || consttype(n) == CTRUNE || consttype(n) == CTFLT || consttype(n) == CTCPLX) 
                n.SetVal(toint(n.Val()));
                if (n.Val().U._<ptr<Mpint>>().CmpInt64(0L) < 0L)
                {
                    yyerror("negative %s argument in make(%v)", arg, t);
                    return false;
                }

                if (n.Val().U._<ptr<Mpint>>().Cmp(maxintval[TINT]) > 0L)
                {
                    yyerror("%s argument too large in make(%v)", arg, t);
                    return false;
                }

            // defaultlit is necessary for non-constants too: n might be 1.1<<k.
            // TODO(gri) The length argument requirements for (array/slice) make
            // are the same as for index expressions. Factor the code better;
            // for instance, indexlit might be called here and incorporate some
            // of the bounds checks done for make.
            n = defaultlit(n, types.Types[TINT]);

            return true;

        }

        private static void markbreak(ptr<Node> _addr_n, ptr<Node> _addr_@implicit)
        {
            ref Node n = ref _addr_n.val;
            ref Node @implicit = ref _addr_@implicit.val;

            if (n == null)
            {
                return ;
            }


            if (n.Op == OBREAK)
            {
                if (n.Sym == null)
                {
                    if (implicit != null)
                    {
                        @implicit.SetHasBreak(true);
                    }

                }
                else
                {
                    var lab = asNode(n.Sym.Label);
                    if (lab != null)
                    {
                        lab.SetHasBreak(true);
                    }

                }

                goto __switch_break1;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL || n.Op == OSWITCH || n.Op == OTYPESW || n.Op == OSELECT || n.Op == ORANGE)
            {
                implicit = n;
            }
            // default: 
                markbreak(_addr_n.Left, _addr_implicit);
                markbreak(_addr_n.Right, _addr_implicit);
                markbreaklist(n.Ninit, _addr_implicit);
                markbreaklist(n.Nbody, _addr_implicit);
                markbreaklist(n.List, _addr_implicit);
                markbreaklist(n.Rlist, _addr_implicit);

            __switch_break1:;

        }

        private static void markbreaklist(Nodes l, ptr<Node> _addr_@implicit)
        {
            ref Node @implicit = ref _addr_@implicit.val;

            var s = l.Slice();
            for (long i = 0L; i < len(s); i++)
            {
                var n = s[i];
                if (n == null)
                {
                    continue;
                }

                if (n.Op == OLABEL && i + 1L < len(s) && n.Name.Defn == s[i + 1L])
                {

                    if (n.Name.Defn.Op == OFOR || n.Name.Defn.Op == OFORUNTIL || n.Name.Defn.Op == OSWITCH || n.Name.Defn.Op == OTYPESW || n.Name.Defn.Op == OSELECT || n.Name.Defn.Op == ORANGE) 
                        n.Sym.Label = asTypesNode(n.Name.Defn);
                        markbreak(_addr_n.Name.Defn, _addr_n.Name.Defn);
                        n.Sym.Label = null;
                        i++;
                        continue;
                    
                }

                markbreak(_addr_n, _addr_implicit);

            }


        }

        // isterminating reports whether the Nodes list ends with a terminating statement.
        public static bool isterminating(this Nodes l)
        {
            var s = l.Slice();
            var c = len(s);
            if (c == 0L)
            {
                return false;
            }

            return s[c - 1L].isterminating();

        }

        // Isterminating reports whether the node n, the last one in a
        // statement list, is a terminating statement.
        private static bool isterminating(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            // NOTE: OLABEL is treated as a separate statement,
            // not a separate prefix, so skipping to the last statement
            // in the block handles the labeled statement case by
            // skipping over the label. No case OLABEL here.

            if (n.Op == OBLOCK) 
                return n.List.isterminating();
            else if (n.Op == OGOTO || n.Op == ORETURN || n.Op == ORETJMP || n.Op == OPANIC || n.Op == OFALL) 
                return true;
            else if (n.Op == OFOR || n.Op == OFORUNTIL) 
                if (n.Left != null)
                {
                    return false;
                }

                if (n.HasBreak())
                {
                    return false;
                }

                return true;
            else if (n.Op == OIF) 
                return n.Nbody.isterminating() && n.Rlist.isterminating();
            else if (n.Op == OSWITCH || n.Op == OTYPESW || n.Op == OSELECT) 
                if (n.HasBreak())
                {
                    return false;
                }

                var def = false;
                foreach (var (_, n1) in n.List.Slice())
                {
                    if (!n1.Nbody.isterminating())
                    {
                        return false;
                    }

                    if (n1.List.Len() == 0L)
                    { // default
                        def = true;

                    }

                }
                if (n.Op != OSELECT && !def)
                {
                    return false;
                }

                return true;
                        return false;

        }

        // checkreturn makes sure that fn terminates appropriately.
        private static void checkreturn(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Type.NumResults() != 0L && fn.Nbody.Len() != 0L)
            {
                markbreaklist(fn.Nbody, _addr_null);
                if (!fn.Nbody.isterminating())
                {
                    yyerrorl(fn.Func.Endlineno, "missing return at end of function");
                }

            }

        }

        private static void deadcode(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            deadcodeslice(fn.Nbody);
            deadcodefn(_addr_fn);
        }

        private static void deadcodefn(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Nbody.Len() == 0L)
            {
                return ;
            }

            foreach (var (_, n) in fn.Nbody.Slice())
            {
                if (n.Ninit.Len() > 0L)
                {
                    return ;
                }


                if (n.Op == OIF) 
                    if (!Isconst(n.Left, CTBOOL) || n.Nbody.Len() > 0L || n.Rlist.Len() > 0L)
                    {
                        return ;
                    }

                else if (n.Op == OFOR) 
                    if (!Isconst(n.Left, CTBOOL) || n.Left.Bool())
                    {
                        return ;
                    }

                else 
                    return ;
                
            }
            fn.Nbody.Set(new slice<ptr<Node>>(new ptr<Node>[] { nod(OEMPTY,nil,nil) }));

        }

        private static void deadcodeslice(Nodes nn)
        {
            long lastLabel = -1L;
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in nn.Slice())
                {
                    i = __i;
                    n = __n;
                    if (n != null && n.Op == OLABEL)
                    {
                        lastLabel = i;
                    }

                }

                i = i__prev1;
                n = n__prev1;
            }

            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in nn.Slice())
                {
                    i = __i;
                    n = __n; 
                    // Cut is set to true when all nodes after i'th position
                    // should be removed.
                    // In other words, it marks whole slice "tail" as dead.
                    var cut = false;
                    if (n == null)
                    {
                        continue;
                    }

                    if (n.Op == OIF)
                    {
                        n.Left = deadcodeexpr(_addr_n.Left);
                        if (Isconst(n.Left, CTBOOL))
                        {
                            Nodes body = default;
                            if (n.Left.Bool())
                            {
                                n.Rlist = new Nodes();
                                body = n.Nbody;
                            }
                            else
                            {
                                n.Nbody = new Nodes();
                                body = n.Rlist;
                            } 
                            // If "then" or "else" branch ends with panic or return statement,
                            // it is safe to remove all statements after this node.
                            // isterminating is not used to avoid goto-related complications.
                            // We must be careful not to deadcode-remove labels, as they
                            // might be the target of a goto. See issue 28616.
                            {
                                Nodes body__prev3 = body;

                                body = body.Slice();

                                if (len(body) != 0L)
                                {

                                    if (body[(len(body) - 1L)].Op == ORETURN || body[(len(body) - 1L)].Op == ORETJMP || body[(len(body) - 1L)].Op == OPANIC) 
                                        if (i > lastLabel)
                                        {
                                            cut = true;
                                        }

                                                                    }

                                body = body__prev3;

                            }

                        }

                    }

                    deadcodeslice(n.Ninit);
                    deadcodeslice(n.Nbody);
                    deadcodeslice(n.List);
                    deadcodeslice(n.Rlist);
                    if (cut)
                    {
                        nn.slice.val = nn.Slice()[..i + 1L];
                        break;
                    }

                }

                i = i__prev1;
                n = n__prev1;
            }
        }

        private static ptr<Node> deadcodeexpr(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // Perform dead-code elimination on short-circuited boolean
            // expressions involving constants with the intent of
            // producing a constant 'if' condition.

            if (n.Op == OANDAND) 
                n.Left = deadcodeexpr(_addr_n.Left);
                n.Right = deadcodeexpr(_addr_n.Right);
                if (Isconst(n.Left, CTBOOL))
                {
                    if (n.Left.Bool())
                    {
                        return _addr_n.Right!; // true && x => x
                    }
                    else
                    {
                        return _addr_n.Left!; // false && x => false
                    }

                }

            else if (n.Op == OOROR) 
                n.Left = deadcodeexpr(_addr_n.Left);
                n.Right = deadcodeexpr(_addr_n.Right);
                if (Isconst(n.Left, CTBOOL))
                {
                    if (n.Left.Bool())
                    {
                        return _addr_n.Left!; // true || x => true
                    }
                    else
                    {
                        return _addr_n.Right!; // false || x => x
                    }

                }

                        return _addr_n!;

        }

        // setTypeNode sets n to an OTYPE node representing t.
        private static void setTypeNode(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            n.Op = OTYPE;
            n.Type = t;
            n.Type.Nod = asTypesNode(n);
        }

        // getIotaValue returns the current value for "iota",
        // or -1 if not within a ConstSpec.
        private static long getIotaValue()
        {
            {
                var i = len(typecheckdefstack);

                if (i > 0L)
                {
                    {
                        var x = typecheckdefstack[i - 1L];

                        if (x.Op == OLITERAL)
                        {
                            return x.Iota();
                        }

                    }

                }

            }


            if (Curfn != null && Curfn.Iota() >= 0L)
            {
                return Curfn.Iota();
            }

            return -1L;

        }

        // curpkg returns the current package, based on Curfn.
        private static ptr<types.Pkg> curpkg()
        {
            var fn = Curfn;
            if (fn == null)
            { 
                // Initialization expressions for package-scope variables.
                return _addr_localpkg!;

            } 

            // TODO(mdempsky): Standardize on either ODCLFUNC or ONAME for
            // Curfn, rather than mixing them.
            if (fn.Op == ODCLFUNC)
            {
                fn = fn.Func.Nname;
            }

            return _addr_fnpkg(fn)!;

        }
    }
}}}}
