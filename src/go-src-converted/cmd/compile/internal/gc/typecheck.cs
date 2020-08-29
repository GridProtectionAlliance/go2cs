// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:47 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\typecheck.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using fmt = go.fmt_package;
using math = go.math_package;
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
        public static readonly long Etop = 1L << (int)(iota); // evaluated at statement level
        public static readonly var Erv = 0; // evaluated in value context
        public static readonly var Etype = 1; // evaluated in type context
        public static readonly var Ecall = 2; // call-only expressions are ok
        public static readonly var Efnstruct = 3; // multivalue function returns are ok
        public static readonly var Easgn = 4; // assigning to expression
        public static readonly var Ecomplit = 5; // type in composite literal

        // type checks the whole tree of an expression.
        // calculates expression types.
        // evaluates compile time constants.
        // marks variables that escape the local frame.
        // rewrites n.Op to be more specific in some cases.

        private static slice<ref Node> typecheckdefstack = default;

        // resolve ONONAME to definition, if any.
        private static ref Node resolve(ref Node n)
        {
            if (n != null && n.Op == ONONAME && n.Sym != null)
            {
                var r = asNode(n.Sym.Def);
                if (r != null)
                {
                    if (r.Op != OIOTA)
                    {
                        n = r;
                    }
                    else if (len(typecheckdefstack) > 0L)
                    {
                        var x = typecheckdefstack[len(typecheckdefstack) - 1L];
                        if (x.Op == OLITERAL)
                        {
                            n = nodintconst(x.Iota());
                        }
                    }
                }
            }
            return n;
        }

        private static void typecheckslice(slice<ref Node> l, long top)
        {
            foreach (var (i) in l)
            {
                l[i] = typecheck(l[i], top);
            }
        }

        private static @string _typekind = new slice<@string>(InitKeyedValues<@string>((TINT, "int"), (TUINT, "uint"), (TINT8, "int8"), (TUINT8, "uint8"), (TINT16, "int16"), (TUINT16, "uint16"), (TINT32, "int32"), (TUINT32, "uint32"), (TINT64, "int64"), (TUINT64, "uint64"), (TUINTPTR, "uintptr"), (TCOMPLEX64, "complex64"), (TCOMPLEX128, "complex128"), (TFLOAT32, "float32"), (TFLOAT64, "float64"), (TBOOL, "bool"), (TSTRING, "string"), (TPTR32, "pointer"), (TPTR64, "pointer"), (TUNSAFEPTR, "unsafe.Pointer"), (TSTRUCT, "struct"), (TINTER, "interface"), (TCHAN, "chan"), (TMAP, "map"), (TARRAY, "array"), (TSLICE, "slice"), (TFUNC, "func"), (TNIL, "nil"), (TIDEAL, "untyped number")));

        private static @string typekind(ref types.Type t)
        {
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

        // sprint_depchain prints a dependency chain of nodes into trace.
        // It is used by typecheck in the case of OLITERAL nodes
        // to print constant definition loops.
        private static void sprint_depchain(ref @string trace, slice<ref Node> stack, ref Node cur, ref Node first)
        {
            for (var i = len(stack) - 1L; i >= 0L; i--)
            {
                {
                    var n = stack[i];

                    if (n.Op == cur.Op)
                    {
                        if (n != first)
                        {
                            sprint_depchain(trace, stack[..i], n, first);
                        }
                        trace.Value += fmt.Sprintf("\n\t%v: %v uses %v", n.Line(), n, cur);
                        return;
                    }

                }
            }

        }

        private static slice<ref Node> typecheck_tcstack = default;

        // typecheck type checks node n.
        // The result of typecheck MUST be assigned back to n, e.g.
        //     n.Left = typecheck(n.Left, top)
        private static ref Node typecheck(ref Node n, long top)
        { 
            // cannot type check until all the source has been parsed
            if (!typecheckok)
            {
                Fatalf("early typecheck");
            }
            if (n == null)
            {
                return null;
            }
            var lno = setlineno(n); 

            // Skip over parens.
            while (n.Op == OPAREN)
            {
                n = n.Left;
            } 

            // Resolve definition of name and value of iota lazily.
 

            // Resolve definition of name and value of iota lazily.
            n = resolve(n); 

            // Skip typecheck if already done.
            // But re-typecheck ONAME/OTYPE/OLITERAL/OPACK node in case context has changed.
            if (n.Typecheck() == 1L)
            {

                if (n.Op == ONAME || n.Op == OTYPE || n.Op == OLITERAL || n.Op == OPACK) 
                    break;
                else 
                    lineno = lno;
                    return n;
                            }
            if (n.Typecheck() == 2L)
            { 
                // Typechecking loop. Trying printing a meaningful message,
                // otherwise a stack trace of typechecking.

                // We can already diagnose variables used as types.
                if (n.Op == ONAME) 
                    if (top & (Erv | Etype) == Etype)
                    {
                        yyerror("%v is not a type", n);
                    }
                else if (n.Op == OTYPE) 
                    if (top & Etype == Etype)
                    {
                        @string trace = default;
                        sprint_depchain(ref trace, typecheck_tcstack, n, n);
                        yyerrorl(n.Pos, "invalid recursive type alias %v%s", n, trace);
                    }
                else if (n.Op == OLITERAL) 
                    if (top & (Erv | Etype) == Etype)
                    {
                        yyerror("%v is not a type", n);
                        break;
                    }
                    trace = default;
                    sprint_depchain(ref trace, typecheck_tcstack, n, n);
                    yyerrorl(n.Pos, "constant definition loop%s", trace);
                                if (nsavederrors + nerrors == 0L)
                {
                    trace = default;
                    for (var i = len(typecheck_tcstack) - 1L; i >= 0L; i--)
                    {
                        var x = typecheck_tcstack[i];
                        trace += fmt.Sprintf("\n\t%v %v", x.Line(), x);
                    }

                    yyerror("typechecking loop involving %v%s", n, trace);
                }
                lineno = lno;
                return n;
            }
            n.SetTypecheck(2L);

            typecheck_tcstack = append(typecheck_tcstack, n);
            n = typecheck1(n, top);

            n.SetTypecheck(1L);

            var last = len(typecheck_tcstack) - 1L;
            typecheck_tcstack[last] = null;
            typecheck_tcstack = typecheck_tcstack[..last];

            lineno = lno;
            return n;
        }

        // does n contain a call or receive operation?
        private static bool callrecv(ref Node n)
        {
            if (n == null)
            {
                return false;
            }

            if (n.Op == OCALL || n.Op == OCALLMETH || n.Op == OCALLINTER || n.Op == OCALLFUNC || n.Op == ORECV || n.Op == OCAP || n.Op == OLEN || n.Op == OCOPY || n.Op == ONEW || n.Op == OAPPEND || n.Op == ODELETE) 
                return true;
                        return callrecv(n.Left) || callrecv(n.Right) || callrecvlist(n.Ninit) || callrecvlist(n.Nbody) || callrecvlist(n.List) || callrecvlist(n.Rlist);
        }

        private static bool callrecvlist(Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                if (callrecv(n))
                {
                    return true;
                }
            }
            return false;
        }

        // indexlit implements typechecking of untyped values as
        // array/slice indexes. It is almost equivalent to defaultlit
        // but also accepts untyped numeric values representable as
        // value of type int (see also checkmake for comparison).
        // The result of indexlit MUST be assigned back to n, e.g.
        //     n.Left = indexlit(n.Left)
        private static ref Node indexlit(ref Node n)
        {
            if (n != null && n.Type != null && n.Type.Etype == TIDEAL)
            {
                return defaultlit(n, types.Types[TINT]);
            }
            return n;
        }

        // The result of typecheck1 MUST be assigned back to n, e.g.
        //     n.Left = typecheck1(n.Left, top)
        private static ref Node typecheck1(ref Node n, long top)
        {

            if (n.Op == OXDOT || n.Op == ODOT || n.Op == ODOTPTR || n.Op == ODOTMETH || n.Op == ODOTINTER || n.Op == ORETJMP)             else 
                if (n.Sym != null)
                {
                    if (n.Op == ONAME && n.Etype != 0L && top & Ecall == 0L)
                    {
                        yyerror("use of builtin %v not in function call", n.Sym);
                        n.Type = null;
                        return n;
                    }
                    typecheckdef(n);
                    if (n.Op == ONONAME)
                    {
                        n.Type = null;
                        return n;
                    }
                }
                        long ok = 0L;

            // until typecheck is complete, do nothing.
            if (n.Op == OLITERAL) 
                ok |= Erv;

                if (n.Type == null && n.Val().Ctype() == CTSTR)
                {
                    n.Type = types.Idealstring;
                }
            else if (n.Op == ONONAME) 
                ok |= Erv;
            else if (n.Op == ONAME) 
                if (n.Name.Decldepth == 0L)
                {
                    n.Name.Decldepth = decldepth;
                }
                if (n.Etype != 0L)
                {
                    ok |= Ecall;
                    break;
                }
                if (top & Easgn == 0L)
                { 
                    // not a write to the variable
                    if (isblank(n))
                    {
                        yyerror("cannot use _ as value");
                        n.Type = null;
                        return n;
                    }
                    n.Name.SetUsed(true);
                }
                ok |= Erv;
            else if (n.Op == OPACK) 
                yyerror("use of package %v without selector", n.Sym);
                n.Type = null;
                return n;
            else if (n.Op == ODDD) 
                break; 

                // types (OIND is with exprs)
            else if (n.Op == OTYPE) 
                ok |= Etype;

                if (n.Type == null)
                {
                    return n;
                }
            else if (n.Op == OTARRAY) 
                ok |= Etype;
                var r = typecheck(n.Right, Etype);
                if (r.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                ref types.Type t = default;
                if (n.Left == null)
                {
                    t = types.NewSlice(r.Type);
                }
                else if (n.Left.Op == ODDD)
                {
                    if (top & Ecomplit == 0L)
                    {
                        if (!n.Diag())
                        {
                            n.SetDiag(true);
                            yyerror("use of [...] array outside of array literal");
                        }
                        n.Type = null;
                        return n;
                    }
                    t = types.NewDDDArray(r.Type);
                }
                else
                {
                    n.Left = indexlit(typecheck(n.Left, Erv));
                    var l = n.Left;
                    if (consttype(l) != CTINT)
                    {

                        if (l.Type == null)                         else if (l.Type.IsInteger() && l.Op != OLITERAL) 
                            yyerror("non-constant array bound %v", l);
                        else 
                            yyerror("invalid array bound %v", l);
                                                n.Type = null;
                        return n;
                    }
                    var v = l.Val();
                    if (doesoverflow(v, types.Types[TINT]))
                    {
                        yyerror("array bound is too large");
                        n.Type = null;
                        return n;
                    }
                    ref Mpint bound = v.U._<ref Mpint>().Int64();
                    if (bound < 0L)
                    {
                        yyerror("array bound must be non-negative");
                        n.Type = null;
                        return n;
                    }
                    t = types.NewArray(r.Type, bound);
                }
                n.Op = OTYPE;
                n.Type = t;
                n.Left = null;
                n.Right = null;
                if (!t.IsDDDArray())
                {
                    checkwidth(t);
                }
            else if (n.Op == OTMAP) 
                ok |= Etype;
                n.Left = typecheck(n.Left, Etype);
                n.Right = typecheck(n.Right, Etype);
                l = n.Left;
                r = n.Right;
                if (l.Type == null || r.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                if (l.Type.NotInHeap())
                {
                    yyerror("go:notinheap map key not allowed");
                }
                if (r.Type.NotInHeap())
                {
                    yyerror("go:notinheap map value not allowed");
                }
                n.Op = OTYPE;
                n.Type = types.NewMap(l.Type, r.Type);
                mapqueue = append(mapqueue, n); // check map keys when all types are settled
                n.Left = null;
                n.Right = null;
            else if (n.Op == OTCHAN) 
                ok |= Etype;
                n.Left = typecheck(n.Left, Etype);
                l = n.Left;
                if (l.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                if (l.Type.NotInHeap())
                {
                    yyerror("chan of go:notinheap type not allowed");
                }
                t = types.NewChan(l.Type, types.ChanDir(n.Etype)); // TODO(marvin): Fix Node.EType type union.
                n.Op = OTYPE;
                n.Type = t;
                n.Left = null;
                n.Etype = 0L;
            else if (n.Op == OTSTRUCT) 
                ok |= Etype;
                n.Op = OTYPE;
                n.Type = tostruct(n.List.Slice());
                if (n.Type == null || n.Type.Broke())
                {
                    n.Type = null;
                    return n;
                }
                n.List.Set(null);
            else if (n.Op == OTINTER) 
                ok |= Etype;
                n.Op = OTYPE;
                n.Type = tointerface(n.List.Slice());
                if (n.Type == null)
                {
                    return n;
                }
            else if (n.Op == OTFUNC) 
                ok |= Etype;
                n.Op = OTYPE;
                n.Type = functype(n.Left, n.List.Slice(), n.Rlist.Slice());
                if (n.Type == null)
                {
                    return n;
                }
                n.Left = null;
                n.List.Set(null);
                n.Rlist.Set(null); 

                // type or expr
            else if (n.Op == OIND) 
                n.Left = typecheck(n.Left, Erv | Etype | top & Ecomplit);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (l.Op == OTYPE)
                {
                    ok |= Etype;
                    n.Op = OTYPE;
                    n.Type = types.NewPtr(l.Type); 
                    // Ensure l.Type gets dowidth'd for the backend. Issue 20174.
                    // Don't checkwidth [...] arrays, though, since they
                    // will be replaced by concrete-sized arrays. Issue 20333.
                    if (!l.Type.IsDDDArray())
                    {
                        checkwidth(l.Type);
                    }
                    n.Left = null;
                    break;
                }
                if (!t.IsPtr())
                {
                    if (top & (Erv | Etop) != 0L)
                    {
                        yyerror("invalid indirect of %L", n.Left);
                        n.Type = null;
                        return n;
                    }
                    break;
                }
                ok |= Erv;
                n.Type = t.Elem(); 

                // arithmetic exprs
            else if (n.Op == OASOP || n.Op == OADD || n.Op == OAND || n.Op == OANDAND || n.Op == OANDNOT || n.Op == ODIV || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLT || n.Op == OLSH || n.Op == ORSH || n.Op == OMOD || n.Op == OMUL || n.Op == ONE || n.Op == OOR || n.Op == OOROR || n.Op == OSUB || n.Op == OXOR) 
                l = default;
                Op op = default;
                r = default;
                if (n.Op == OASOP)
                {
                    ok |= Etop;
                    n.Left = typecheck(n.Left, Erv);
                    n.Right = typecheck(n.Right, Erv);
                    l = n.Left;
                    r = n.Right;
                    checkassign(n, n.Left);
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return n;
                    }
                    if (n.Implicit() && !okforarith[l.Type.Etype])
                    {
                        yyerror("invalid operation: %v (non-numeric type %v)", n, l.Type);
                        n.Type = null;
                        return n;
                    } 
                    // TODO(marvin): Fix Node.EType type union.
                    op = Op(n.Etype);
                }
                else
                {
                    ok |= Erv;
                    n.Left = typecheck(n.Left, Erv);
                    n.Right = typecheck(n.Right, Erv);
                    l = n.Left;
                    r = n.Right;
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return n;
                    }
                    op = n.Op;
                }
                if (op == OLSH || op == ORSH)
                {
                    r = defaultlit(r, types.Types[TUINT]);
                    n.Right = r;
                    t = r.Type;
                    if (!t.IsInteger() || t.IsSigned())
                    {
                        yyerror("invalid operation: %v (shift count type %v, must be unsigned integer)", n, r.Type);
                        n.Type = null;
                        return n;
                    }
                    t = l.Type;
                    if (t != null && t.Etype != TIDEAL && !t.IsInteger())
                    {
                        yyerror("invalid operation: %v (shift of type %v)", n, t);
                        n.Type = null;
                        return n;
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
                    return n;
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
                if (iscmp[n.Op] && t.Etype != TIDEAL && !eqtype(l.Type, r.Type))
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
                                yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(l.Type));
                                n.Type = null;
                                return n;
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
                                yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(r.Type));
                                n.Type = null;
                                return n;
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
                if (t.Etype != TIDEAL && !eqtype(l.Type, r.Type))
                {
                    l, r = defaultlit2(l, r, true);
                    if (r.Type.IsInterface() == l.Type.IsInterface() || aop == 0L)
                    {
                        yyerror("invalid operation: %v (mismatched types %v and %v)", n, l.Type, r.Type);
                        n.Type = null;
                        return n;
                    }
                }
                if (!okfor[op][et])
                {
                    yyerror("invalid operation: %v (operator %v not defined on %s)", n, op, typekind(t));
                    n.Type = null;
                    return n;
                } 

                // okfor allows any array == array, map == map, func == func.
                // restrict to slice/map/func == nil and nil == slice/map/func.
                if (l.Type.IsArray() && !IsComparable(l.Type))
                {
                    yyerror("invalid operation: %v (%v cannot be compared)", n, l.Type);
                    n.Type = null;
                    return n;
                }
                if (l.Type.IsSlice() && !isnil(l) && !isnil(r))
                {
                    yyerror("invalid operation: %v (slice can only be compared to nil)", n);
                    n.Type = null;
                    return n;
                }
                if (l.Type.IsMap() && !isnil(l) && !isnil(r))
                {
                    yyerror("invalid operation: %v (map can only be compared to nil)", n);
                    n.Type = null;
                    return n;
                }
                if (l.Type.Etype == TFUNC && !isnil(l) && !isnil(r))
                {
                    yyerror("invalid operation: %v (func can only be compared to nil)", n);
                    n.Type = null;
                    return n;
                }
                if (l.Type.IsStruct())
                {
                    {
                        var f = IncomparableField(l.Type);

                        if (f != null)
                        {
                            yyerror("invalid operation: %v (struct containing %v cannot be compared)", n, f.Type);
                            n.Type = null;
                            return n;
                        }

                    }
                }
                t = l.Type;
                if (iscmp[n.Op])
                {
                    evconst(n);
                    t = types.Idealbool;
                    if (n.Op != OLITERAL)
                    {
                        l, r = defaultlit2(l, r, true);
                        n.Left = l;
                        n.Right = r;
                    }
                }
                if (et == TSTRING)
                {
                    if (iscmp[n.Op])
                    { 
                        // TODO(marvin): Fix Node.EType type union.
                        n.Etype = types.EType(n.Op);
                        n.Op = OCMPSTR;
                    }
                    else if (n.Op == OADD)
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
                            n.List.AppendNodes(ref r.List);
                        }
                        else
                        {
                            n.List.Append(r);
                        }
                        n.Left = null;
                        n.Right = null;
                    }
                }
                if (et == TINTER)
                {
                    if (l.Op == OLITERAL && l.Val().Ctype() == CTNIL)
                    { 
                        // swap for back end
                        n.Left = r;

                        n.Right = l;
                    }
                    else if (r.Op == OLITERAL && r.Val().Ctype() == CTNIL)
                    {
                    }
                    else if (r.Type.IsInterface() == l.Type.IsInterface())
                    { 
                        // TODO(marvin): Fix Node.EType type union.
                        n.Etype = types.EType(n.Op);
                        n.Op = OCMPIFACE;
                    }
                }
                if ((op == ODIV || op == OMOD) && Isconst(r, CTINT))
                {
                    if (r.Val().U._<ref Mpint>().CmpInt64(0L) == 0L)
                    {
                        yyerror("division by zero");
                        n.Type = null;
                        return n;
                    }
                }
                n.Type = t;
            else if (n.Op == OCOM || n.Op == OMINUS || n.Op == ONOT || n.Op == OPLUS) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (!okfor[n.Op][t.Etype])
                {
                    yyerror("invalid operation: %v %v", n.Op, t);
                    n.Type = null;
                    return n;
                }
                n.Type = t; 

                // exprs
            else if (n.Op == OADDR) 
                ok |= Erv;

                n.Left = typecheck(n.Left, Erv);
                if (n.Left.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                checklvalue(n.Left, "take the address of");
                r = outervalue(n.Left);
                l = default;
                l = n.Left;

                while (l != r)
                {
                    l.SetAddrtaken(true);
                    if (l.IsClosureVar() && !capturevarscomplete)
                    { 
                        // Mark the original variable as Addrtaken so that capturevars
                        // knows not to pass it by value.
                        // But if the capturevars phase is complete, don't touch it,
                        // in case l.Name's containing function has not yet been compiled.
                        l.Name.Defn.SetAddrtaken(true);
                    l = l.Left;
                    }
                }


                if (l.Orig != l && l.Op == ONAME)
                {
                    Fatalf("found non-orig name node %v", l);
                }
                l.SetAddrtaken(true);
                if (l.IsClosureVar() && !capturevarscomplete)
                { 
                    // See comments above about closure variables.
                    l.Name.Defn.SetAddrtaken(true);
                }
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                n.Type = types.NewPtr(t);
            else if (n.Op == OCOMPLIT) 
                ok |= Erv;
                n = typecheckcomplit(n);
                if (n.Type == null)
                {
                    return n;
                }
            else if (n.Op == OXDOT || n.Op == ODOT) 
                if (n.Op == OXDOT)
                {
                    n = adddot(n);
                    n.Op = ODOT;
                    if (n.Left == null)
                    {
                        n.Type = null;
                        return n;
                    }
                }
                n.Left = typecheck(n.Left, Erv | Etype);

                n.Left = defaultlit(n.Left, null);

                t = n.Left.Type;
                if (t == null)
                {
                    adderrorname(n);
                    n.Type = null;
                    return n;
                }
                var s = n.Sym;

                if (n.Left.Op == OTYPE)
                {
                    if (!looktypedot(n, t, 0L))
                    {
                        if (looktypedot(n, t, 1L))
                        {
                            yyerror("%v undefined (cannot refer to unexported method %v)", n, n.Sym);
                        }
                        else
                        {
                            yyerror("%v undefined (type %v has no method %v)", n, t, n.Sym);
                        }
                        n.Type = null;
                        return n;
                    }
                    if (n.Type.Etype != TFUNC || !n.IsMethod())
                    {
                        yyerror("type %v has no method %S", n.Left.Type, n.Sym);
                        n.Type = null;
                        return n;
                    }
                    n.Op = ONAME;
                    if (n.Name == null)
                    {
                        n.Name = @new<Name>();
                    }
                    n.Right = newname(n.Sym);
                    n.Type = methodfunc(n.Type, n.Left.Type);
                    n.Xoffset = 0L;
                    n.SetClass(PFUNC);
                    ok = Erv;
                    break;
                }
                if (t.IsPtr() && !t.Elem().IsInterface())
                {
                    t = t.Elem();
                    if (t == null)
                    {
                        n.Type = null;
                        return n;
                    }
                    n.Op = ODOTPTR;
                    checkwidth(t);
                }
                if (n.Sym.IsBlank())
                {
                    yyerror("cannot refer to blank field or method");
                    n.Type = null;
                    return n;
                }
                if (lookdot(n, t, 0L) == null)
                { 
                    // Legitimate field or method lookup failed, try to explain the error

                    if (t.IsEmptyInterface()) 
                        yyerror("%v undefined (type %v is interface with no methods)", n, n.Left.Type);
                    else if (t.IsPtr() && t.Elem().IsInterface()) 
                        // Pointer to interface is almost always a mistake.
                        yyerror("%v undefined (type %v is pointer to interface, not interface)", n, n.Left.Type);
                    else if (lookdot(n, t, 1L) != null) 
                        // Field or method matches by name, but it is not exported.
                        yyerror("%v undefined (cannot refer to unexported field or method %v)", n, n.Sym);
                    else 
                        {
                            var mt = lookdot(n, t, 2L);

                            if (mt != null)
                            { // Case-insensitive lookup.
                                yyerror("%v undefined (type %v has no field or method %v, but does have %v)", n, n.Left.Type, n.Sym, mt.Sym);
                            }
                            else
                            {
                                yyerror("%v undefined (type %v has no field or method %v)", n, n.Left.Type, n.Sym);
                            }

                        }
                                        n.Type = null;
                    return n;
                }

                if (n.Op == ODOTINTER || n.Op == ODOTMETH) 
                    if (top & Ecall != 0L)
                    {
                        ok |= Ecall;
                    }
                    else
                    {
                        typecheckpartialcall(n, s);
                        ok |= Erv;
                    }
                else 
                    ok |= Erv;
                            else if (n.Op == ODOTTYPE) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (!t.IsInterface())
                {
                    yyerror("invalid type assertion: %v (non-interface type %v on left)", n, t);
                    n.Type = null;
                    return n;
                }
                if (n.Right != null)
                {
                    n.Right = typecheck(n.Right, Etype);
                    n.Type = n.Right.Type;
                    n.Right = null;
                    if (n.Type == null)
                    {
                        return n;
                    }
                }
                if (n.Type != null && !n.Type.IsInterface())
                {
                    ref types.Field missing = default;                    ref types.Field have = default;

                    long ptr = default;
                    if (!implements(n.Type, t, ref missing, ref have, ref ptr))
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
                        return n;
                    }
                }
            else if (n.Op == OINDEX) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                n.Left = implicitstar(n.Left);
                l = n.Left;
                n.Right = typecheck(n.Right, Erv);
                r = n.Right;
                t = l.Type;
                if (t == null || r.Type == null)
                {
                    n.Type = null;
                    return n;
                }

                if (t.Etype == TSTRING || t.Etype == TARRAY || t.Etype == TSLICE) 
                    n.Right = indexlit(n.Right);
                    if (t.IsString())
                    {
                        n.Type = types.Bytetype;
                    }
                    else
                    {
                        n.Type = t.Elem();
                    }
                    @string why = "string";
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
                        else if (Isconst(n.Left, CTSTR) && x >= int64(len(n.Left.Val().U._<@string>())))
                        {
                            yyerror("invalid string index %v (out of bounds for %d-byte string)", n.Right, len(n.Left.Val().U._<@string>()));
                        }
                        else if (n.Right.Val().U._<ref Mpint>().Cmp(maxintval[TINT]) > 0L)
                        {
                            yyerror("invalid %s index %v (index too large)", why, n.Right);
                        }
                    }
                else if (t.Etype == TMAP) 
                    n.Etype = 0L;
                    n.Right = defaultlit(n.Right, t.Key());
                    if (n.Right.Type != null)
                    {
                        n.Right = assignconv(n.Right, t.Key(), "map index");
                    }
                    n.Type = t.Val();
                    n.Op = OINDEXMAP;
                else 
                    yyerror("invalid operation: %v (type %v does not support indexing)", n, t);
                    n.Type = null;
                    return n;
                            else if (n.Op == ORECV) 
                ok |= Etop | Erv;
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (receive from non-chan type %v)", n, t);
                    n.Type = null;
                    return n;
                }
                if (!t.ChanDir().CanRecv())
                {
                    yyerror("invalid operation: %v (receive from send-only type %v)", n, t);
                    n.Type = null;
                    return n;
                }
                n.Type = t.Elem();
            else if (n.Op == OSEND) 
                ok |= Etop;
                n.Left = typecheck(n.Left, Erv);
                n.Right = typecheck(n.Right, Erv);
                n.Left = defaultlit(n.Left, null);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (send to non-chan type %v)", n, t);
                    n.Type = null;
                    return n;
                }
                if (!t.ChanDir().CanSend())
                {
                    yyerror("invalid operation: %v (send to receive-only type %v)", n, t);
                    n.Type = null;
                    return n;
                }
                n.Right = defaultlit(n.Right, t.Elem());
                r = n.Right;
                if (r.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                n.Right = assignconv(r, t.Elem(), "send"); 

                // TODO: more aggressive
                n.Etype = 0L;

                n.Type = null;
            else if (n.Op == OSLICE || n.Op == OSLICE3) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                var (low, high, max) = n.SliceBounds();
                var hasmax = n.Op.IsSlice3();
                low = typecheck(low, Erv);
                high = typecheck(high, Erv);
                max = typecheck(max, Erv);
                n.Left = defaultlit(n.Left, null);
                low = indexlit(low);
                high = indexlit(high);
                max = indexlit(max);
                n.SetSliceBounds(low, high, max);
                l = n.Left;
                if (l.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                if (l.Type.IsArray())
                {
                    if (!islvalue(n.Left))
                    {
                        yyerror("invalid operation %v (slice of unaddressable value)", n);
                        n.Type = null;
                        return n;
                    }
                    n.Left = nod(OADDR, n.Left, null);
                    n.Left.SetImplicit(true);
                    n.Left = typecheck(n.Left, Erv);
                    l = n.Left;
                }
                t = l.Type;
                ref types.Type tp = default;
                if (t.IsString())
                {
                    if (hasmax)
                    {
                        yyerror("invalid operation %v (3-index slice of string)", n);
                        n.Type = null;
                        return n;
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
                    return n;
                }
                if (low != null && !checksliceindex(l, low, tp))
                {
                    n.Type = null;
                    return n;
                }
                if (high != null && !checksliceindex(l, high, tp))
                {
                    n.Type = null;
                    return n;
                }
                if (max != null && !checksliceindex(l, max, tp))
                {
                    n.Type = null;
                    return n;
                }
                if (!checksliceconst(low, high) || !checksliceconst(low, max) || !checksliceconst(high, max))
                {
                    n.Type = null;
                    return n;
                } 

                // call and call like
            else if (n.Op == OCALL) 
                n.Left = typecheck(n.Left, Erv | Etype | Ecall);
                if (n.Left.Diag())
                {
                    n.SetDiag(true);
                }
                l = n.Left;

                if (l.Op == ONAME && l.Etype != 0L)
                { 
                    // TODO(marvin): Fix Node.EType type union.
                    if (n.Isddd() && Op(l.Etype) != OAPPEND)
                    {
                        yyerror("invalid use of ... with builtin %v", l);
                    } 

                    // builtin: OLEN, OCAP, etc.
                    // TODO(marvin): Fix Node.EType type union.
                    n.Op = Op(l.Etype);
                    n.Left = n.Right;
                    n.Right = null;
                    n = typecheck1(n, top);
                    return n;
                }
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                if (l.Op == OTYPE)
                {
                    if (n.Isddd() || l.Type.IsDDDArray())
                    {
                        if (!l.Type.Broke())
                        {
                            yyerror("invalid use of ... in type conversion to %v", l.Type);
                        }
                        n.SetDiag(true);
                    } 

                    // pick off before type-checking arguments
                    ok |= Erv; 

                    // turn CALL(type, arg) into CONV(arg) w/ type
                    n.Left = null;

                    n.Op = OCONV;
                    n.Type = l.Type;
                    if (!onearg(n, "conversion to %v", l.Type))
                    {
                        n.Type = null;
                        return n;
                    }
                    n = typecheck1(n, top);
                    return n;
                }
                if (n.List.Len() == 1L && !n.Isddd())
                {
                    n.List.SetFirst(typecheck(n.List.First(), Erv | Efnstruct));
                }
                else
                {
                    typecheckslice(n.List.Slice(), Erv);
                }
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
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

                    if (l.Left == null || !eqtype(l.Left.Type, tp))
                    {
                        Fatalf("method receiver");
                    }
                else 
                    n.Op = OCALLFUNC;
                    if (t.Etype != TFUNC)
                    {
                        yyerror("cannot call non-function %v (type %v)", l, t);
                        n.Type = null;
                        return n;
                    }
                                typecheckaste(OCALL, n.Left, n.Isddd(), t.Params(), n.List, () => fmt.Sprintf("argument to %v", n.Left));
                ok |= Etop;
                if (t.NumResults() == 0L)
                {
                    break;
                }
                ok |= Erv;
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
                if (top & (Efnstruct | Etop) == 0L)
                {
                    yyerror("multiple-value %v() in single-value context", l);
                    break;
                }
                n.Type = l.Type.Results();
            else if (n.Op == OALIGNOF || n.Op == OOFFSETOF || n.Op == OSIZEOF) 
                ok |= Erv;
                if (!onearg(n, "%v", n.Op))
                {
                    n.Type = null;
                    return n;
                } 

                // any side effects disappear; ignore init
                r = default;
                nodconst(ref r, types.Types[TUINTPTR], evalunsafe(n));
                r.Orig = n;
                n = ref r;
            else if (n.Op == OCAP || n.Op == OLEN) 
                ok |= Erv;
                if (!onearg(n, "%v", n.Op))
                {
                    n.Type = null;
                    return n;
                }
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                n.Left = implicitstar(n.Left);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
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
                    return n;
                } 

                // result might be constant
                long res = -1L; // valid if >= 0

                if (t.Etype == TSTRING) 
                    if (Isconst(l, CTSTR))
                    {
                        res = int64(len(l.Val().U._<@string>()));
                    }
                else if (t.Etype == TARRAY) 
                    if (!callrecv(l))
                    {
                        res = t.NumElem();
                    }
                                if (res >= 0L)
                {
                    r = default;
                    nodconst(ref r, types.Types[TINT], res);
                    r.Orig = n;
                    n = ref r;
                }
                n.Type = types.Types[TINT];
            else if (n.Op == OREAL || n.Op == OIMAG) 
                ok |= Erv;
                if (!onearg(n, "%v", n.Op))
                {
                    n.Type = null;
                    return n;
                }
                n.Left = typecheck(n.Left, Erv);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (t.Etype != TIDEAL && !t.IsComplex())
                {
                    yyerror("invalid argument %L for %v", l, n.Op);
                    n.Type = null;
                    return n;
                } 

                // if the argument is a constant, the result is a constant
                // (any untyped numeric constant can be represented as a
                // complex number)
                if (l.Op == OLITERAL)
                {
                    ref Mpflt re = default;                    ref Mpflt im = default;


                    if (consttype(l) == CTINT || consttype(l) == CTRUNE) 
                        re = newMpflt();
                        re.SetInt(l.Val().U._<ref Mpint>()); 
                        // im = 0
                    else if (consttype(l) == CTFLT) 
                        re = l.Val().U._<ref Mpflt>(); 
                        // im = 0
                    else if (consttype(l) == CTCPLX) 
                        re = ref l.Val().U._<ref Mpcplx>().Real;
                        im = ref l.Val().U._<ref Mpcplx>().Imag;
                    else 
                        yyerror("invalid argument %L for %v", l, n.Op);
                        n.Type = null;
                        return n;
                                        if (n.Op == OIMAG)
                    {
                        if (im == null)
                        {
                            im = newMpflt();
                        }
                        re = im;
                    }
                    var orig = n;
                    n = nodfltconst(re);
                    n.Orig = orig;
                } 

                // determine result type
                et = t.Etype;

                if (et == TIDEAL)                 else if (et == TCOMPLEX64) 
                    et = TFLOAT32;
                else if (et == TCOMPLEX128) 
                    et = TFLOAT64;
                else 
                    Fatalf("unexpected Etype: %v\n", et);
                                n.Type = types.Types[et];
            else if (n.Op == OCOMPLEX) 
                ok |= Erv;
                r = default;
                l = default;
                if (n.List.Len() == 1L)
                {
                    typecheckslice(n.List.Slice(), Efnstruct);
                    if (n.List.First().Op != OCALLFUNC && n.List.First().Op != OCALLMETH)
                    {
                        yyerror("invalid operation: complex expects two arguments");
                        n.Type = null;
                        return n;
                    }
                    t = n.List.First().Left.Type;
                    if (!t.IsKind(TFUNC))
                    { 
                        // Bail. This error will be reported elsewhere.
                        return n;
                    }
                    if (t.NumResults() != 2L)
                    {
                        yyerror("invalid operation: complex expects two arguments, %v returns %d results", n.List.First(), t.NumResults());
                        n.Type = null;
                        return n;
                    }
                    t = n.List.First().Type;
                    l = asNode(t.Field(0L).Nname);
                    r = asNode(t.Field(1L).Nname);
                }
                else
                {
                    if (!twoarg(n))
                    {
                        n.Type = null;
                        return n;
                    }
                    n.Left = typecheck(n.Left, Erv);
                    n.Right = typecheck(n.Right, Erv);
                    l = n.Left;
                    r = n.Right;
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return n;
                    }
                    l, r = defaultlit2(l, r, false);
                    if (l.Type == null || r.Type == null)
                    {
                        n.Type = null;
                        return n;
                    }
                    n.Left = l;
                    n.Right = r;
                }
                if (!eqtype(l.Type, r.Type))
                {
                    yyerror("invalid operation: %v (mismatched types %v and %v)", n, l.Type, r.Type);
                    n.Type = null;
                    return n;
                }
                t = default;

                if (l.Type.Etype == TIDEAL) 
                    t = types.Types[TIDEAL];
                else if (l.Type.Etype == TFLOAT32) 
                    t = types.Types[TCOMPLEX64];
                else if (l.Type.Etype == TFLOAT64) 
                    t = types.Types[TCOMPLEX128];
                else 
                    yyerror("invalid operation: %v (arguments have type %v, expected floating-point)", n, l.Type);
                    n.Type = null;
                    return n;
                                if (l.Op == OLITERAL && r.Op == OLITERAL)
                { 
                    // make it a complex literal
                    r = nodcplxlit(l.Val(), r.Val());

                    r.Orig = n;
                    n = r;
                }
                n.Type = t;
            else if (n.Op == OCLOSE) 
                if (!onearg(n, "%v", n.Op))
                {
                    n.Type = null;
                    return n;
                }
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, null);
                l = n.Left;
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (!t.IsChan())
                {
                    yyerror("invalid operation: %v (non-chan type %v)", n, t);
                    n.Type = null;
                    return n;
                }
                if (!t.ChanDir().CanSend())
                {
                    yyerror("invalid operation: %v (cannot close receive-only channel)", n);
                    n.Type = null;
                    return n;
                }
                ok |= Etop;
            else if (n.Op == ODELETE) 
                var args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing arguments to delete");
                    n.Type = null;
                    return n;
                }
                if (args.Len() == 1L)
                {
                    yyerror("missing second (key) argument to delete");
                    n.Type = null;
                    return n;
                }
                if (args.Len() != 2L)
                {
                    yyerror("too many arguments to delete");
                    n.Type = null;
                    return n;
                }
                ok |= Etop;
                typecheckslice(args.Slice(), Erv);
                l = args.First();
                r = args.Second();
                if (l.Type != null && !l.Type.IsMap())
                {
                    yyerror("first argument to delete must be map; have %L", l.Type);
                    n.Type = null;
                    return n;
                }
                args.SetSecond(assignconv(r, l.Type.Key(), "delete"));
            else if (n.Op == OAPPEND) 
                ok |= Erv;
                args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing arguments to append");
                    n.Type = null;
                    return n;
                }
                if (args.Len() == 1L && !n.Isddd())
                {
                    args.SetFirst(typecheck(args.First(), Erv | Efnstruct));
                }
                else
                {
                    typecheckslice(args.Slice(), Erv);
                }
                t = args.First().Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                } 

                // Unpack multiple-return result before type-checking.
                ref types.Type funarg = default;
                if (t.IsFuncArgStruct())
                {
                    funarg = t;
                    t = t.Field(0L).Type;
                }
                n.Type = t;
                if (!t.IsSlice())
                {
                    if (Isconst(args.First(), CTNIL))
                    {
                        yyerror("first argument to append must be typed slice; have untyped nil");
                        n.Type = null;
                        return n;
                    }
                    yyerror("first argument to append must be slice; have %L", t);
                    n.Type = null;
                    return n;
                }
                if (n.Isddd())
                {
                    if (args.Len() == 1L)
                    {
                        yyerror("cannot use ... on first argument to append");
                        n.Type = null;
                        return n;
                    }
                    if (args.Len() != 2L)
                    {
                        yyerror("too many arguments to append");
                        n.Type = null;
                        return n;
                    }
                    if (t.Elem().IsKind(TUINT8) && args.Second().Type.IsString())
                    {
                        args.SetSecond(defaultlit(args.Second(), types.Types[TSTRING]));
                        break;
                    }
                    args.SetSecond(assignconv(args.Second(), t.Orig, "append"));
                    break;
                }
                if (funarg != null)
                {
                    {
                        ref types.Type t__prev1 = t;

                        foreach (var (_, __t) in funarg.FieldSlice()[1L..])
                        {
                            t = __t;
                            if (assignop(t.Type, n.Type.Elem(), null) == 0L)
                            {
                                yyerror("cannot append %v value to []%v", t.Type, n.Type.Elem());
                            }
                        }
                else

                        t = t__prev1;
                    }

                }                {
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

                }
            else if (n.Op == OCOPY) 
                ok |= Etop | Erv;
                args = n.List;
                if (args.Len() < 2L)
                {
                    yyerror("missing arguments to copy");
                    n.Type = null;
                    return n;
                }
                if (args.Len() > 2L)
                {
                    yyerror("too many arguments to copy");
                    n.Type = null;
                    return n;
                }
                n.Left = args.First();
                n.Right = args.Second();
                n.List.Set(null);
                n.Type = types.Types[TINT];
                n.Left = typecheck(n.Left, Erv);
                n.Right = typecheck(n.Right, Erv);
                if (n.Left.Type == null || n.Right.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                n.Left = defaultlit(n.Left, null);
                n.Right = defaultlit(n.Right, null);
                if (n.Left.Type == null || n.Right.Type == null)
                {
                    n.Type = null;
                    return n;
                } 

                // copy([]byte, string)
                if (n.Left.Type.IsSlice() && n.Right.Type.IsString())
                {
                    if (eqtype(n.Left.Type.Elem(), types.Bytetype))
                    {
                        break;
                    }
                    yyerror("arguments to copy have different element types: %L and string", n.Left.Type);
                    n.Type = null;
                    return n;
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
                    return n;
                }
                if (!eqtype(n.Left.Type.Elem(), n.Right.Type.Elem()))
                {
                    yyerror("arguments to copy have different element types: %L and %L", n.Left.Type, n.Right.Type);
                    n.Type = null;
                    return n;
                }
            else if (n.Op == OCONV) 
                ok |= Erv;
                saveorignode(n);
                checkwidth(n.Type); // ensure width is calculated for backend
                n.Left = typecheck(n.Left, Erv);
                n.Left = convlit1(n.Left, n.Type, true, noReuse);
                t = n.Left.Type;
                if (t == null || n.Type == null)
                {
                    n.Type = null;
                    return n;
                }
                why = default;
                n.Op = convertop(t, n.Type, ref why);
                if (n.Op == 0L)
                {
                    if (!n.Diag() && !n.Type.Broke() && !n.Left.Diag())
                    {
                        yyerror("cannot convert %L to type %v%s", n.Left, n.Type, why);
                        n.SetDiag(true);
                    }
                    n.Op = OCONV;
                }

                if (n.Op == OCONVNOP) 
                    if (n.Left.Op == OLITERAL)
                    {
                        r = nod(OXXX, null, null);
                        n.Op = OCONV;
                        n.Orig = r;
                        r.Value = n.Value;
                        n.Op = OLITERAL;
                        n.SetVal(n.Left.Val());
                    }
                    else if (t.Etype == n.Type.Etype)
                    {

                        if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128) 
                            // Floating point casts imply rounding and
                            // so the conversion must be kept.
                            n.Op = OCONV;
                                            } 

                    // do not use stringtoarraylit.
                    // generated code and compiler memory footprint is better without it.
                else if (n.Op == OSTRARRAYBYTE) 
                    break;
                else if (n.Op == OSTRARRAYRUNE) 
                    if (n.Left.Op == OLITERAL)
                    {
                        n = stringtoarraylit(n);
                    }
                            else if (n.Op == OMAKE) 
                ok |= Erv;
                args = n.List.Slice();
                if (len(args) == 0L)
                {
                    yyerror("missing argument to make");
                    n.Type = null;
                    return n;
                }
                n.List.Set(null);
                l = args[0L];
                l = typecheck(l, Etype);
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                long i = 1L;

                if (t.Etype == TSLICE) 
                    if (i >= len(args))
                    {
                        yyerror("missing len argument to make(%v)", t);
                        n.Type = null;
                        return n;
                    }
                    l = args[i];
                    i++;
                    l = typecheck(l, Erv);
                    r = default;
                    if (i < len(args))
                    {
                        r = args[i];
                        i++;
                        r = typecheck(r, Erv);
                    }
                    if (l.Type == null || (r != null && r.Type == null))
                    {
                        n.Type = null;
                        return n;
                    }
                    if (!checkmake(t, "len", l) || r != null && !checkmake(t, "cap", r))
                    {
                        n.Type = null;
                        return n;
                    }
                    if (Isconst(l, CTINT) && r != null && Isconst(r, CTINT) && l.Val().U._<ref Mpint>().Cmp(r.Val().U._<ref Mpint>()) > 0L)
                    {
                        yyerror("len larger than cap in make(%v)", t);
                        n.Type = null;
                        return n;
                    }
                    n.Left = l;
                    n.Right = r;
                    n.Op = OMAKESLICE;
                else if (t.Etype == TMAP) 
                    if (i < len(args))
                    {
                        l = args[i];
                        i++;
                        l = typecheck(l, Erv);
                        l = defaultlit(l, types.Types[TINT]);
                        if (l.Type == null)
                        {
                            n.Type = null;
                            return n;
                        }
                        if (!checkmake(t, "size", l))
                        {
                            n.Type = null;
                            return n;
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
                        l = typecheck(l, Erv);
                        l = defaultlit(l, types.Types[TINT]);
                        if (l.Type == null)
                        {
                            n.Type = null;
                            return n;
                        }
                        if (!checkmake(t, "buffer", l))
                        {
                            n.Type = null;
                            return n;
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
                    return n;
                                if (i < len(args))
                {
                    yyerror("too many arguments to make(%v)", t);
                    n.Op = OMAKE;
                    n.Type = null;
                    return n;
                }
                n.Type = t;
            else if (n.Op == ONEW) 
                ok |= Erv;
                args = n.List;
                if (args.Len() == 0L)
                {
                    yyerror("missing argument to new");
                    n.Type = null;
                    return n;
                }
                l = args.First();
                l = typecheck(l, Etype);
                t = l.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
                }
                if (args.Len() > 1L)
                {
                    yyerror("too many arguments to new(%v)", t);
                    n.Type = null;
                    return n;
                }
                n.Left = l;
                n.Type = types.NewPtr(t);
            else if (n.Op == OPRINT || n.Op == OPRINTN) 
                ok |= Etop;
                typecheckslice(n.List.Slice(), Erv);
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
                ok |= Etop;
                if (!onearg(n, "panic"))
                {
                    n.Type = null;
                    return n;
                }
                n.Left = typecheck(n.Left, Erv);
                n.Left = defaultlit(n.Left, types.Types[TINTER]);
                if (n.Left.Type == null)
                {
                    n.Type = null;
                    return n;
                }
            else if (n.Op == ORECOVER) 
                ok |= Erv | Etop;
                if (n.List.Len() != 0L)
                {
                    yyerror("too many arguments to recover");
                    n.Type = null;
                    return n;
                }
                n.Type = types.Types[TINTER];
            else if (n.Op == OCLOSURE) 
                ok |= Erv;
                typecheckclosure(n, top);
                if (n.Type == null)
                {
                    return n;
                }
            else if (n.Op == OITAB) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
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
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                t = n.Left.Type;
                if (t == null)
                {
                    n.Type = null;
                    return n;
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
                ok |= Erv;
            else if (n.Op == OCFUNC) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv);
                n.Type = types.Types[TUINTPTR];
            else if (n.Op == OCONVNOP) 
                ok |= Erv;
                n.Left = typecheck(n.Left, Erv); 

                // statements
            else if (n.Op == OAS) 
                ok |= Etop;

                typecheckas(n); 

                // Code that creates temps does not bother to set defn, so do it here.
                if (n.Left.Op == ONAME && n.Left.IsAutoTmp())
                {
                    n.Left.Name.Defn = n;
                }
            else if (n.Op == OAS2) 
                ok |= Etop;
                typecheckas2(n);
            else if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == ODCL || n.Op == OEMPTY || n.Op == OGOTO || n.Op == OFALL || n.Op == OVARKILL || n.Op == OVARLIVE) 
                ok |= Etop;
            else if (n.Op == OLABEL) 
                ok |= Etop;
                decldepth++;
                if (n.Left.Sym.IsBlank())
                { 
                    // Empty identifier is valid but useless.
                    // Eliminate now to simplify life later.
                    // See issues 7538, 11589, 11593.
                    n.Op = OEMPTY;
                    n.Left = null;
                }
            else if (n.Op == ODEFER) 
                ok |= Etop;
                n.Left = typecheck(n.Left, Etop | Erv);
                if (!n.Left.Diag())
                {
                    checkdefergo(n);
                }
            else if (n.Op == OPROC) 
                ok |= Etop;
                n.Left = typecheck(n.Left, Etop | Erv);
                checkdefergo(n);
            else if (n.Op == OFOR || n.Op == OFORUNTIL) 
                ok |= Etop;
                typecheckslice(n.Ninit.Slice(), Etop);
                decldepth++;
                n.Left = typecheck(n.Left, Erv);
                if (n.Left != null)
                {
                    t = n.Left.Type;
                    if (t != null && !t.IsBoolean())
                    {
                        yyerror("non-bool %L used as for condition", n.Left);
                    }
                }
                n.Right = typecheck(n.Right, Etop);
                typecheckslice(n.Nbody.Slice(), Etop);
                decldepth--;
            else if (n.Op == OIF) 
                ok |= Etop;
                typecheckslice(n.Ninit.Slice(), Etop);
                n.Left = typecheck(n.Left, Erv);
                if (n.Left != null)
                {
                    t = n.Left.Type;
                    if (t != null && !t.IsBoolean())
                    {
                        yyerror("non-bool %L used as if condition", n.Left);
                    }
                }
                typecheckslice(n.Nbody.Slice(), Etop);
                typecheckslice(n.Rlist.Slice(), Etop);
            else if (n.Op == ORETURN) 
                ok |= Etop;
                if (n.List.Len() == 1L)
                {
                    typecheckslice(n.List.Slice(), Erv | Efnstruct);
                }
                else
                {
                    typecheckslice(n.List.Slice(), Erv);
                }
                if (Curfn == null)
                {
                    yyerror("return outside function");
                    n.Type = null;
                    return n;
                }
                if (Curfn.Type.FuncType().Outnamed && n.List.Len() == 0L)
                {
                    break;
                }
                typecheckaste(ORETURN, null, false, Curfn.Type.Results(), n.List, () => "return argument");
            else if (n.Op == ORETJMP) 
                ok |= Etop;
            else if (n.Op == OSELECT) 
                ok |= Etop;
                typecheckselect(n);
            else if (n.Op == OSWITCH) 
                ok |= Etop;
                typecheckswitch(n);
            else if (n.Op == ORANGE) 
                ok |= Etop;
                typecheckrange(n);
            else if (n.Op == OTYPESW) 
                yyerror("use of .(type) outside type switch");
                n.Type = null;
                return n;
            else if (n.Op == OXCASE) 
                ok |= Etop;
                typecheckslice(n.List.Slice(), Erv);
                typecheckslice(n.Nbody.Slice(), Etop);
            else if (n.Op == ODCLFUNC) 
                ok |= Etop;
                typecheckfunc(n);
            else if (n.Op == ODCLCONST) 
                ok |= Etop;
                n.Left = typecheck(n.Left, Erv);
            else if (n.Op == ODCLTYPE) 
                ok |= Etop;
                n.Left = typecheck(n.Left, Etype);
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
            if (safemode && !inimport && !compiling_wrappers && t != null && t.Etype == TUNSAFEPTR)
            {
                yyerror("cannot use unsafe.Pointer");
            }
            evconst(n);
            if (n.Op == OTYPE && top & Etype == 0L)
            {
                if (!n.Type.Broke())
                {
                    yyerror("type %v is not an expression", n.Type);
                }
                n.Type = null;
                return n;
            }
            if (top & (Erv | Etype) == Etype && n.Op != OTYPE)
            {
                yyerror("%v is not a type", n);
                n.Type = null;
                return n;
            } 

            // TODO(rsc): simplify
            if ((top & (Ecall | Erv | Etype) != 0L) && top & Etop == 0L && ok & (Erv | Etype | Ecall) == 0L)
            {
                yyerror("%v used as value", n);
                n.Type = null;
                return n;
            }
            if ((top & Etop != 0L) && top & (Ecall | Erv | Etype) == 0L && ok & Etop == 0L)
            {
                if (!n.Diag())
                {
                    yyerror("%v evaluated but not used", n);
                    n.SetDiag(true);
                }
                n.Type = null;
                return n;
            }
            return n;
        }

        private static bool checksliceindex(ref Node l, ref Node r, ref types.Type tp)
        {
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
                else if (Isconst(l, CTSTR) && r.Int64() > int64(len(l.Val().U._<@string>())))
                {
                    yyerror("invalid slice index %v (out of bounds for %d-byte string)", r, len(l.Val().U._<@string>()));
                    return false;
                }
                else if (r.Val().U._<ref Mpint>().Cmp(maxintval[TINT]) > 0L)
                {
                    yyerror("invalid slice index %v (index too large)", r);
                    return false;
                }
            }
            return true;
        }

        private static bool checksliceconst(ref Node lo, ref Node hi)
        {
            if (lo != null && hi != null && lo.Op == OLITERAL && hi.Op == OLITERAL && lo.Val().U._<ref Mpint>().Cmp(hi.Val().U._<ref Mpint>()) > 0L)
            {
                yyerror("invalid slice index: %v > %v", lo, hi);
                return false;
            }
            return true;
        }

        private static void checkdefergo(ref Node n)
        {
            @string what = "defer";
            if (n.Op == OPROC)
            {
                what = "go";
            }

            // ok
            if (n.Left.Op == OCALLINTER || n.Left.Op == OCALLMETH || n.Left.Op == OCALLFUNC || n.Left.Op == OCLOSE || n.Left.Op == OCOPY || n.Left.Op == ODELETE || n.Left.Op == OPANIC || n.Left.Op == OPRINT || n.Left.Op == OPRINTN || n.Left.Op == ORECOVER) 
                return;
            else if (n.Left.Op == OAPPEND || n.Left.Op == OCAP || n.Left.Op == OCOMPLEX || n.Left.Op == OIMAG || n.Left.Op == OLEN || n.Left.Op == OMAKE || n.Left.Op == OMAKESLICE || n.Left.Op == OMAKECHAN || n.Left.Op == OMAKEMAP || n.Left.Op == ONEW || n.Left.Op == OREAL || n.Left.Op == OLITERAL) // conversion or unsafe.Alignof, Offsetof, Sizeof
                if (n.Left.Orig != null && n.Left.Orig.Op == OCONV)
                {
                    break;
                }
                yyerror("%s discards result of %v", what, n.Left);
                return;
            // type is broken or missing, most likely a method call on a broken type
            // we will warn about the broken type elsewhere. no need to emit a potentially confusing error
            if (n.Left.Type == null || n.Left.Type.Broke())
            {
                return;
            }
            if (!n.Diag())
            { 
                // The syntax made sure it was a call, so this must be
                // a conversion.
                n.SetDiag(true);
                yyerror("%s requires function call, not conversion", what);
            }
        }

        // The result of implicitstar MUST be assigned back to n, e.g.
        //     n.Left = implicitstar(n.Left)
        private static ref Node implicitstar(ref Node n)
        { 
            // insert implicit * if needed for fixed array
            var t = n.Type;
            if (t == null || !t.IsPtr())
            {
                return n;
            }
            t = t.Elem();
            if (t == null)
            {
                return n;
            }
            if (!t.IsArray())
            {
                return n;
            }
            n = nod(OIND, n, null);
            n.SetImplicit(true);
            n = typecheck(n, Erv);
            return n;
        }

        private static bool onearg(ref Node n, @string f, params object[] args)
        {
            args = args.Clone();

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

        private static bool twoarg(ref Node n)
        {
            if (n.Left != null)
            {
                return true;
            }
            if (n.List.Len() == 0L)
            {
                yyerror("missing argument to %v - %v", n.Op, n);
                return false;
            }
            n.Left = n.List.First();
            if (n.List.Len() == 1L)
            {
                yyerror("missing argument to %v - %v", n.Op, n);
                n.List.Set(null);
                return false;
            }
            if (n.List.Len() > 2L)
            {
                yyerror("too many arguments to %v - %v", n.Op, n);
                n.List.Set(null);
                return false;
            }
            n.Right = n.List.Second();
            n.List.Set(null);
            return true;
        }

        private static ref types.Field lookdot1(ref Node errnode, ref types.Sym s, ref types.Type t, ref types.Fields fs, long dostrcmp)
        {
            ref types.Field r = default;
            foreach (var (_, f) in fs.Slice())
            {
                if (dostrcmp != 0L && f.Sym.Name == s.Name)
                {
                    return f;
                }
                if (dostrcmp == 2L && strings.EqualFold(f.Sym.Name, s.Name))
                {
                    return f;
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
            return r;
        }

        private static bool looktypedot(ref Node n, ref types.Type t, long dostrcmp)
        {
            var s = n.Sym;

            if (t.IsInterface())
            {
                var f1 = lookdot1(n, s, t, t.Fields(), dostrcmp);
                if (f1 == null)
                {
                    return false;
                }
                n.Sym = methodsym(n.Sym, t, false);
                n.Xoffset = f1.Offset;
                n.Type = f1.Type;
                n.Op = ODOTINTER;
                return true;
            } 

            // Find the base type: methtype will fail if t
            // is not of the form T or *T.
            var mt = methtype(t);
            if (mt == null)
            {
                return false;
            }
            expandmeth(mt);
            var f2 = lookdot1(n, s, mt, mt.AllMethods(), dostrcmp);
            if (f2 == null)
            {
                return false;
            } 

            // disallow T.m if m requires *T receiver
            if (f2.Type.Recv().Type.IsPtr() && !t.IsPtr() && f2.Embedded != 2L && !isifacemethod(f2.Type))
            {
                yyerror("invalid method expression %v (needs pointer receiver: (*%v).%S)", n, t, f2.Sym);
                return false;
            }
            n.Sym = methodsym(n.Sym, t, false);
            n.Xoffset = f2.Offset;
            n.Type = f2.Type;
            n.Op = ODOTMETH;
            return true;
        }

        private static ref types.Type derefall(ref types.Type t)
        {
            while (t != null && t.Etype == types.Tptr)
            {
                t = t.Elem();
            }

            return t;
        }

        private partial struct typeSymKey
        {
            public ptr<types.Type> t;
            public ptr<types.Sym> s;
        }

        // dotField maps (*types.Type, *types.Sym) pairs to the corresponding struct field (*types.Type with Etype==TFIELD).
        // It is a cache for use during usefield in walk.go, only enabled when field tracking.
        private static map dotField = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<typeSymKey, ref types.Field>{};

        private static ref types.Field lookdot(ref Node n, ref types.Type t, long dostrcmp)
        {
            var s = n.Sym;

            dowidth(t);
            ref types.Field f1 = default;
            if (t.IsStruct() || t.IsInterface())
            {
                f1 = lookdot1(n, s, t, t.Fields(), dostrcmp);
            }
            ref types.Field f2 = default;
            if (n.Left.Type == t || n.Left.Type.Sym == null)
            {
                var mt = methtype(t);
                if (mt != null)
                {
                    f2 = lookdot1(n, s, mt, mt.Methods(), dostrcmp);
                }
            }
            if (f1 != null)
            {
                if (dostrcmp > 1L)
                { 
                    // Already in the process of diagnosing an error.
                    return f1;
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
                        n.Left = nod(OIND, n.Left, null); // implicitstar
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(n.Left, Erv);
                    }
                    n.Op = ODOTINTER;
                }
                return f1;
            }
            if (f2 != null)
            {
                if (dostrcmp > 1L)
                { 
                    // Already in the process of diagnosing an error.
                    return f2;
                }
                var tt = n.Left.Type;
                dowidth(tt);
                var rcvr = f2.Type.Recv().Type;
                if (!eqtype(rcvr, tt))
                {
                    if (rcvr.Etype == types.Tptr && eqtype(rcvr.Elem(), tt))
                    {
                        checklvalue(n.Left, "call pointer method on");
                        n.Left = nod(OADDR, n.Left, null);
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(n.Left, Etype | Erv);
                    }
                    else if (tt.Etype == types.Tptr && rcvr.Etype != types.Tptr && eqtype(tt.Elem(), rcvr))
                    {
                        n.Left = nod(OIND, n.Left, null);
                        n.Left.SetImplicit(true);
                        n.Left = typecheck(n.Left, Etype | Erv);
                    }
                    else if (tt.Etype == types.Tptr && tt.Elem().Etype == types.Tptr && eqtype(derefall(tt), derefall(rcvr)))
                    {
                        yyerror("calling method %v with receiver %L requires explicit dereference", n.Sym, n.Left);
                        while (tt.Etype == types.Tptr)
                        { 
                            // Stop one level early for method with pointer receiver.
                            if (rcvr.Etype == types.Tptr && tt.Elem().Etype != types.Tptr)
                            {
                                break;
                            }
                            n.Left = nod(OIND, n.Left, null);
                            n.Left.SetImplicit(true);
                            n.Left = typecheck(n.Left, Etype | Erv);
                            tt = tt.Elem();
                        }
                    else

                    }                    {
                        Fatalf("method mismatch: %v for %v", rcvr, tt);
                    }
                }
                var pll = n;
                var ll = n.Left;
                while (ll.Left != null && (ll.Op == ODOT || ll.Op == ODOTPTR || ll.Op == OIND))
                {
                    pll = ll;
                    ll = ll.Left;
                }

                if (pll.Implicit() && ll.Type.IsPtr() && ll.Type.Sym != null && asNode(ll.Type.Sym.Def) != null && asNode(ll.Type.Sym.Def).Op == OTYPE)
                { 
                    // It is invalid to automatically dereference a named pointer type when selecting a method.
                    // Make n.Left == ll to clarify error message.
                    n.Left = ll;
                    return null;
                }
                n.Sym = methodsym(n.Sym, n.Left.Type, false);
                n.Xoffset = f2.Offset;
                n.Type = f2.Type;

                n.Op = ODOTMETH;

                return f2;
            }
            return null;
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

        private static bool hasddd(ref types.Type t)
        {
            foreach (var (_, tl) in t.Fields().Slice())
            {
                if (tl.Isddd())
                {
                    return true;
                }
            }
            return false;
        }

        // typecheck assignment: type list = expression list
        private static @string typecheckaste(Op op, ref Node _call, bool isddd, ref types.Type _tstruct, Nodes nl, Func<@string> desc) => func(_call, _tstruct, (ref Node call, ref types.Type tstruct, Defer defer, Panic _, Recover __) =>
        {
            ref types.Type t = default;
            long n1 = default;
            long n2 = default;
            long i = default;

            var lno = lineno;
            defer(() =>
            {
                lineno = lno;

            }());

            if (tstruct.Broke())
            {
                return;
            }
            ref Node n = default;
            if (nl.Len() == 1L)
            {
                n = nl.First();
                if (n.Type != null && n.Type.IsFuncArgStruct())
                {
                    if (!hasddd(tstruct))
                    {
                        n1 = tstruct.NumFields();
                        n2 = n.Type.NumFields();
                        if (n2 > n1)
                        {
                            goto toomany;
                        }
                        if (n2 < n1)
                        {
                            goto notenough;
                        }
                    }
                    var lfs = tstruct.FieldSlice();
                    var rfs = n.Type.FieldSlice();
                    @string why = default;
                    {
                        long i__prev1 = i;
                        var tl__prev1 = tl;

                        foreach (var (__i, __tl) in lfs)
                        {
                            i = __i;
                            tl = __tl;
                            if (tl.Isddd())
                            {
                                {
                                    var tn__prev2 = tn;

                                    foreach (var (_, __tn) in rfs[i..])
                                    {
                                        tn = __tn;
                                        if (assignop(tn.Type, tl.Type.Elem(), ref why) == 0L)
                                        {
                                            if (call != null)
                                            {
                                                yyerror("cannot use %v as type %v in argument to %v%s", tn.Type, tl.Type.Elem(), call, why);
                                            }
                                            else
                                            {
                                                yyerror("cannot use %v as type %v in %s%s", tn.Type, tl.Type.Elem(), desc(), why);
                                            }
                                        }
                                    }

                                    tn = tn__prev2;
                                }

                                return;
                            }
                            if (i >= len(rfs))
                            {
                                goto notenough;
                            }
                            var tn = rfs[i];
                            if (assignop(tn.Type, tl.Type, ref why) == 0L)
                            {
                                if (call != null)
                                {
                                    yyerror("cannot use %v as type %v in argument to %v%s", tn.Type, tl.Type, call, why);
                                }
                                else
                                {
                                    yyerror("cannot use %v as type %v in %s%s", tn.Type, tl.Type, desc(), why);
                                }
                            }
                        }

                        i = i__prev1;
                        tl = tl__prev1;
                    }

                    if (len(rfs) > len(lfs))
                    {
                        goto toomany;
                    }
                    return;
                }
            }
            n1 = tstruct.NumFields();
            n2 = nl.Len();
            if (!hasddd(tstruct))
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
            {
                var tl__prev1 = tl;

                foreach (var (_, __tl) in tstruct.Fields().Slice())
                {
                    tl = __tl;
                    t = tl.Type;
                    if (tl.Isddd())
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
                            return;
                        }
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

                        return;
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

                tl = tl__prev1;
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
            return;

notenough:
            if (n == null || !n.Diag())
            {
                var details = errorDetails(nl, tstruct, isddd);
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
            return;

toomany:
            details = errorDetails(nl, tstruct, isddd);
            if (call != null)
            {
                yyerror("too many arguments in call to %v%s", call, details);
            }
            else
            {
                yyerror("too many arguments to %v%s", op, details);
            }
        });

        private static @string errorDetails(Nodes nl, ref types.Type tstruct, bool isddd)
        { 
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
        private static @string sigrepr(ref types.Type t)
        {

            if (t == types.Types[TIDEAL]) 
                // "untyped number" is not commonly used
                // outside of the compiler, so let's use "number".
                return "number";
            else if (t == types.Idealstring) 
                return "string";
            else if (t == types.Idealbool) 
                return "bool";
            else 
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
            if (nl.Len() == 1L && nl.First().Type != null && nl.First().Type.IsFuncArgStruct())
            {
                foreach (var (_, f) in nl.First().Type.Fields().Slice())
                {
                    typeStrings = append(typeStrings, sigrepr(f.Type));
                }
            else
            }            {
                foreach (var (_, n) in nl.Slice())
                {
                    typeStrings = append(typeStrings, sigrepr(n.Type));
                }
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
                return;
            }
            hash[name] = true;
        }

        private static void keydup(ref Node n, map<uint, slice<ref Node>> hash)
        {
            var orign = n;
            if (n.Op == OCONVIFACE)
            {
                n = n.Left;
            }
            evconst(n);
            if (n.Op != OLITERAL)
            {
                return; // we don't check variables
            }
            const long PRIME1 = 3L;



            uint h = default;
            switch (n.Val().U.type())
            {
                case ref Mpint v:
                    h = uint32(v.Int64());
                    break;
                case ref Mpflt v:
                    var x = math.Float64bits(v.Float64());
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < 8L; i++)
                        {
                            h = h * PRIME1 + uint32(x & 0xFFUL);
                            x >>= 8L;
                        }


                        i = i__prev1;
                    }
                    break;
                case @string v:
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < len(v); i++)
                        {
                            h = h * PRIME1 + uint32(v[i]);
                        }


                        i = i__prev1;
                    }
                    break;
                default:
                {
                    var v = n.Val().U.type();
                    h = 23L;
                    break;
                }

            }

            Node cmp = default;
            foreach (var (_, a) in hash[h])
            {
                cmp.Op = OEQ;
                cmp.Left = n;
                if (a.Op == OCONVIFACE && orign.Op == OCONVIFACE)
                {
                    a = a.Left;
                }
                if (!eqtype(a.Type, n.Type))
                {
                    continue;
                }
                cmp.Right = a;
                evconst(ref cmp);
                if (cmp.Op != OLITERAL)
                { 
                    // Sometimes evconst fails. See issue 12536.
                    continue;
                }
                if (cmp.Val().U._<bool>())
                {
                    yyerror("duplicate key %v in map literal", n);
                    return;
                }
            }
            hash[h] = append(hash[h], orign);
        }

        // iscomptype reports whether type t is a composite literal type
        // or a pointer to one.
        private static bool iscomptype(ref types.Type t)
        {
            if (t.IsPtr())
            {
                t = t.Elem();
            }

            if (t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TSTRUCT || t.Etype == TMAP) 
                return true;
            else 
                return false;
                    }

        private static void pushtype(ref Node n, ref types.Type t)
        {
            if (n == null || n.Op != OCOMPLIT || !iscomptype(t))
            {
                return;
            }
            if (n.Right == null)
            {
                n.Right = typenod(t);
                n.SetImplicit(true); // don't print
                n.Right.SetImplicit(true); // * is okay
            }
            else if (Debug['s'] != 0L)
            {
                n.Right = typecheck(n.Right, Etype);
                if (n.Right.Type != null && eqtype(n.Right.Type, t))
                {
                    fmt.Printf("%v: redundant type: %v\n", n.Line(), t);
                }
            }
        }

        // The result of typecheckcomplit MUST be assigned back to n, e.g.
        //     n.Left = typecheckcomplit(n.Left)
        private static ref Node typecheckcomplit(ref Node _n) => func(_n, (ref Node n, Defer defer, Panic _, Recover __) =>
        {
            var lno = lineno;
            defer(() =>
            {
                lineno = lno;
            }());

            if (n.Right == null)
            {
                yyerrorl(n.Pos, "missing type in composite literal");
                n.Type = null;
                return n;
            } 

            // Save original node (including n.Right)
            var norig = nod(n.Op, null, null);

            norig.Value = n.Value;

            setlineno(n.Right);
            n.Right = typecheck(n.Right, Etype | Ecomplit);
            var l = n.Right; // sic
            var t = l.Type;
            if (t == null)
            {
                n.Type = null;
                return n;
            }
            var nerr = nerrors;
            n.Type = t;

            if (t.IsPtr())
            { 
                // For better or worse, we don't allow pointers as the composite literal type,
                // except when using the &T syntax, which sets implicit on the OIND.
                if (!n.Right.Implicit())
                {
                    yyerror("invalid pointer type %v for composite literal (use &%v instead)", t, t.Elem());
                    n.Type = null;
                    return n;
                } 

                // Also, the underlying type must be a struct, map, slice, or array.
                if (!iscomptype(t))
                {
                    yyerror("invalid pointer type %v for composite literal", t);
                    n.Type = null;
                    return n;
                }
                t = t.Elem();
            }

            if (t.Etype == TARRAY || t.Etype == TSLICE) 
                // If there are key/value pairs, create a map to keep seen
                // keys so we can check for duplicate indices.
                map<long, bool> indices = default;
                {
                    var n1__prev1 = n1;

                    foreach (var (_, __n1) in n.List.Slice())
                    {
                        n1 = __n1;
                        if (n1.Op == OKEY)
                        {
                            indices = make_map<long, bool>();
                            break;
                        }
                    }

                    n1 = n1__prev1;
                }

                long length = default;                long i = default;

                var checkBounds = t.IsArray() && !t.IsDDDArray();
                var nl = n.List.Slice();
                {
                    var l__prev1 = l;

                    foreach (var (__i2, __l) in nl)
                    {
                        i2 = __i2;
                        l = __l;
                        setlineno(l);
                        var vp = ref nl[i2];
                        if (l.Op == OKEY)
                        {
                            l.Left = typecheck(l.Left, Erv);
                            evconst(l.Left);
                            i = nonnegintconst(l.Left);
                            if (i < 0L && !l.Left.Diag())
                            {
                                yyerror("index must be non-negative integer constant");
                                l.Left.SetDiag(true);
                                i = -(1L << (int)(30L)); // stay negative for a while
                            }
                            vp = ref l.Right;
                        }
                        if (i >= 0L && indices != null)
                        {
                            if (indices[i])
                            {
                                yyerror("duplicate index in array literal: %d", i);
                            }
                            else
                            {
                                indices[i] = true;
                            }
                        }
                        var r = vp.Value;
                        pushtype(r, t.Elem());
                        r = typecheck(r, Erv);
                        r = defaultlit(r, t.Elem());
                        vp.Value = assignconv(r, t.Elem(), "array or slice literal");

                        i++;
                        if (i > length)
                        {
                            length = i;
                            if (checkBounds && length > t.NumElem())
                            {
                                setlineno(l);
                                yyerror("array index %d out of bounds [0:%d]", length - 1L, t.NumElem());
                                checkBounds = false;
                            }
                        }
                    }

                    l = l__prev1;
                }

                if (t.IsDDDArray())
                {
                    t.SetNumElem(length);
                }
                if (t.IsSlice())
                {
                    n.Right = nodintconst(length);
                    n.Op = OSLICELIT;
                }
                else
                {
                    n.Op = OARRAYLIT;
                }
            else if (t.Etype == TMAP) 
                var hash = make_map<uint, slice<ref Node>>();
                {
                    var l__prev1 = l;

                    foreach (var (__i3, __l) in n.List.Slice())
                    {
                        i3 = __i3;
                        l = __l;
                        setlineno(l);
                        if (l.Op != OKEY)
                        {
                            n.List.SetIndex(i3, typecheck(l, Erv));
                            yyerror("missing key in map literal");
                            continue;
                        }
                        r = l.Left;
                        pushtype(r, t.Key());
                        r = typecheck(r, Erv);
                        r = defaultlit(r, t.Key());
                        l.Left = assignconv(r, t.Key(), "map key");
                        if (l.Left.Op != OCONV)
                        {
                            keydup(l.Left, hash);
                        }
                        r = l.Right;
                        pushtype(r, t.Val());
                        r = typecheck(r, Erv);
                        r = defaultlit(r, t.Val());
                        l.Right = assignconv(r, t.Val(), "map value");
                    }

                    l = l__prev1;
                }

                n.Op = OMAPLIT;
            else if (t.Etype == TSTRUCT) 
                // Need valid field offsets for Xoffset below.
                dowidth(t);

                var errored = false;
                if (n.List.Len() != 0L && nokeys(n.List))
                { 
                    // simple list of variables
                    var ls = n.List.Slice();
                    {
                        long i__prev1 = i;
                        var n1__prev1 = n1;

                        foreach (var (__i, __n1) in ls)
                        {
                            i = __i;
                            n1 = __n1;
                            setlineno(n1);
                            n1 = typecheck(n1, Erv);
                            ls[i] = n1;
                            if (i >= t.NumFields())
                            {
                                if (!errored)
                                {
                                    yyerror("too many values in struct initializer");
                                    errored = true;
                                }
                                continue;
                            }
                            var f = t.Field(i);
                            var s = f.Sym;
                            if (s != null && !exportname(s.Name) && s.Pkg != localpkg)
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
                        n1 = n1__prev1;
                    }

                    if (len(ls) < t.NumFields())
                    {
                        yyerror("too few values in struct initializer");
                    }
                }                {
                    hash = make_map<@string, bool>(); 

                    // keyed list
                    ls = n.List.Slice();
                    {
                        long i__prev1 = i;
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
                                    l.Left = typecheck(l.Left, Erv);
                                    continue;
                                } 

                                // Sym might have resolved to name in other top-level
                                // package, because of import dot. Redirect to correct sym
                                // before we do the lookup.
                                s = key.Sym;
                                if (s.Pkg != localpkg && exportname(s.Name))
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
                                ls[i] = typecheck(ls[i], Erv);
                                continue;
                            }
                            f = lookdot1(null, l.Sym, t, t.Fields(), 0L);
                            if (f == null)
                            {
                                {
                                    var ci = lookdot1(null, l.Sym, t, t.Fields(), 2L);

                                    if (ci != null)
                                    { // Case-insensitive lookup.
                                        yyerror("unknown field '%v' in struct literal of type %v (but does have %v)", l.Sym, t, ci.Sym);
                                    }
                                    else
                                    {
                                        yyerror("unknown field '%v' in struct literal of type %v", l.Sym, t);
                                    }

                                }
                                continue;
                            }
                            fielddup(f.Sym.Name, hash);
                            l.Xoffset = f.Offset; 

                            // No pushtype allowed here. Tried and rejected.
                            l.Left = typecheck(l.Left, Erv);
                            l.Left = assignconv(l.Left, f.Type, "field value");
                        }

                        i = i__prev1;
                        l = l__prev1;
                    }

                }
                n.Op = OSTRUCTLIT;
            else 
                yyerror("invalid type for composite literal: %v", t);
                n.Type = null;
                        if (nerr != nerrors)
            {
                return n;
            }
            n.Orig = norig;
            if (n.Type.IsPtr())
            {
                n = nod(OPTRLIT, n, null);
                n.SetTypecheck(1L);
                n.Type = n.Left.Type;
                n.Left.Type = t;
                n.Left.SetTypecheck(1L);
            }
            n.Orig = norig;
            return n;
        });

        // lvalue etc
        private static bool islvalue(ref Node n)
        {

            if (n.Op == OINDEX)
            {
                if (n.Left.Type != null && n.Left.Type.IsArray())
                {
                    return islvalue(n.Left);
                }
                if (n.Left.Type != null && n.Left.Type.IsString())
                {
                    return false;
                }
                fallthrough = true;
            }
            if (fallthrough || n.Op == OIND || n.Op == ODOTPTR || n.Op == OCLOSUREVAR)
            {
                return true;
                goto __switch_break0;
            }
            if (n.Op == ODOT)
            {
                return islvalue(n.Left);
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

        private static void checklvalue(ref Node n, @string verb)
        {
            if (!islvalue(n))
            {
                yyerror("cannot %s %v", verb, n);
            }
        }

        private static void checkassign(ref Node stmt, ref Node n)
        { 
            // Variables declared in ORANGE are assigned on every iteration.
            if (n.Name == null || n.Name.Defn != stmt || stmt.Op == ORANGE)
            {
                var r = outervalue(n);
                ref Node l = default;
                l = n;

                while (l != r)
                {
                    l.SetAssigned(true);
                    if (l.IsClosureVar())
                    {
                        l.Name.Defn.SetAssigned(true);
                    l = l.Left;
                    }
                }


                l.SetAssigned(true);
                if (l.IsClosureVar())
                {
                    l.Name.Defn.SetAssigned(true);
                }
            }
            if (islvalue(n))
            {
                return;
            }
            if (n.Op == OINDEXMAP)
            {
                n.Etype = 1L;
                return;
            } 

            // have already complained about n being invalid
            if (n.Type == null)
            {
                return;
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

        private static void checkassignlist(ref Node stmt, Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                checkassign(stmt, n);
            }
        }

        // Check whether l and r are the same side effect-free expression,
        // so that it is safe to reuse one instead of computing both.
        private static bool samesafeexpr(ref Node l, ref Node r)
        {
            if (l.Op != r.Op || !eqtype(l.Type, r.Type))
            {
                return false;
            }

            if (l.Op == ONAME || l.Op == OCLOSUREVAR) 
                return l == r;
            else if (l.Op == ODOT || l.Op == ODOTPTR) 
                return l.Sym != null && r.Sym != null && l.Sym == r.Sym && samesafeexpr(l.Left, r.Left);
            else if (l.Op == OIND || l.Op == OCONVNOP) 
                return samesafeexpr(l.Left, r.Left);
            else if (l.Op == OCONV) 
                // Some conversions can't be reused, such as []byte(str).
                // Allow only numeric-ish types. This is a bit conservative.
                return issimple[l.Type.Etype] && samesafeexpr(l.Left, r.Left);
            else if (l.Op == OINDEX) 
                return samesafeexpr(l.Left, r.Left) && samesafeexpr(l.Right, r.Right);
            else if (l.Op == OLITERAL) 
                return eqval(l.Val(), r.Val());
                        return false;
        }

        // type check assignment.
        // if this assignment is the definition of a var on the left side,
        // fill in the var's type.
        private static void typecheckas(ref Node n)
        { 
            // delicate little dance.
            // the definition of n may refer to this assignment
            // as its definition, in which case it will call typecheckas.
            // in that case, do not call typecheck back, or it will cycle.
            // if the variable has a type (ntype) then typechecking
            // will not look at defn, so it is okay (and desirable,
            // so that the conversion below happens).
            n.Left = resolve(n.Left);

            if (n.Left.Name == null || n.Left.Name.Defn != n || n.Left.Name.Param.Ntype != null)
            {
                n.Left = typecheck(n.Left, Erv | Easgn);
            }
            n.Right = typecheck(n.Right, Erv);
            checkassign(n, n.Left);
            if (n.Right != null && n.Right.Type != null)
            {
                if (n.Left.Type != null)
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
                n.Left = typecheck(n.Left, Erv | Easgn);
            }
            if (!isblank(n.Left))
            {
                checkwidth(n.Left.Type); // ensure width is calculated for backend
            }
        }

        private static void checkassignto(ref types.Type src, ref Node dst)
        {
            @string why = default;

            if (assignop(src, dst.Type, ref why) == 0L)
            {
                yyerror("cannot assign %v to %L in multiple assignment%s", src, dst, why);
                return;
            }
        }

        private static void typecheckas2(ref Node n)
        {
            var ls = n.List.Slice();
            {
                var i1__prev1 = i1;
                var n1__prev1 = n1;

                foreach (var (__i1, __n1) in ls)
                {
                    i1 = __i1;
                    n1 = __n1; 
                    // delicate little dance.
                    n1 = resolve(n1);
                    ls[i1] = n1;

                    if (n1.Name == null || n1.Name.Defn != n || n1.Name.Param.Ntype != null)
                    {
                        ls[i1] = typecheck(ls[i1], Erv | Easgn);
                    }
                }

                i1 = i1__prev1;
                n1 = n1__prev1;
            }

            var cl = n.List.Len();
            var cr = n.Rlist.Len();
            if (cl > 1L && cr == 1L)
            {
                n.Rlist.SetFirst(typecheck(n.Rlist.First(), Erv | Efnstruct));
            }
            else
            {
                typecheckslice(n.Rlist.Slice(), Erv);
            }
            checkassignlist(n, n.List);

            ref Node l = default;
            ref Node r = default;
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
                    {
                        ref Node l__prev1 = l;

                        foreach (var (__i, __l) in n.List.Slice())
                        {
                            i = __i;
                            l = __l;
                            var f = r.Type.Field(i);
                            if (f.Type != null && l.Type != null)
                            {
                                checkassignto(f.Type, l);
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
                                        if (l.Type != null)
                    {
                        checkassignto(r.Type, l);
                    }
                    if (l.Name != null && l.Name.Defn == n)
                    {
                        l.Type = r.Type;
                    }
                    l = n.List.Second();
                    if (l.Type != null && !l.Type.IsBoolean())
                    {
                        checkassignto(types.Types[TBOOL], l);
                    }
                    if (l.Name != null && l.Name.Defn == n && l.Name.Param.Ntype == null)
                    {
                        l.Type = types.Types[TBOOL];
                    }
                    goto @out;
                            }
mismatch: 

            // second half of dance
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
                        ls[i1] = typecheck(ls[i1], Erv | Easgn);
                    }
                }

                i1 = i1__prev1;
                n1 = n1__prev1;
            }

        }

        // type check function definition
        private static void typecheckfunc(ref Node n)
        {
            foreach (var (_, ln) in n.Func.Dcl)
            {
                if (ln.Op == ONAME && (ln.Class() == PPARAM || ln.Class() == PPARAMOUT))
                {
                    ln.Name.Decldepth = 1L;
                }
            }
            n.Func.Nname = typecheck(n.Func.Nname, Erv | Easgn);
            var t = n.Func.Nname.Type;
            if (t == null)
            {
                return;
            }
            n.Type = t;
            t.FuncType().Nname = asTypesNode(n.Func.Nname);
            var rcvr = t.Recv();
            if (rcvr != null && n.Func.Shortname != null)
            {
                n.Func.Nname.Sym = methodname(n.Func.Shortname, rcvr.Type);
                declare(n.Func.Nname, PFUNC);

                addmethod(n.Func.Shortname, t, true, n.Func.Pragma & Nointerface != 0L);
            }
            if (Ctxt.Flag_dynlink && !inimport && n.Func.Nname != null)
            {
                makefuncsym(n.Func.Nname.Sym);
            }
        }

        // The result of stringtoarraylit MUST be assigned back to n, e.g.
        //     n.Left = stringtoarraylit(n.Left)
        private static ref Node stringtoarraylit(ref Node n)
        {
            if (n.Left.Op != OLITERAL || n.Left.Val().Ctype() != CTSTR)
            {
                Fatalf("stringtoarraylit %v", n);
            }
            @string s = n.Left.Val().U._<@string>();
            slice<ref Node> l = default;
            if (n.Type.Elem().Etype == TUINT8)
            { 
                // []byte
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(s); i++)
                    {
                        l = append(l, nod(OKEY, nodintconst(int64(i)), nodintconst(int64(s[0L]))));
                    }
            else


                    i = i__prev1;
                }
            }            { 
                // []rune
                i = 0L;
                foreach (var (_, r) in s)
                {
                    l = append(l, nod(OKEY, nodintconst(int64(i)), nodintconst(int64(r))));
                    i++;
                }
            }
            var nn = nod(OCOMPLIT, null, typenod(n.Type));
            nn.List.Set(l);
            nn = typecheck(nn, Erv);
            return nn;
        }

        private static slice<ref Node> mapqueue = default;

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

        private static void copytype(ref Node n, ref types.Type t)
        {
            if (t.Etype == TFORW)
            { 
                // This type isn't computed yet; when it is, update n.
                t.ForwardType().Copyto = append(t.ForwardType().Copyto, asTypesNode(n));
                return;
            }
            var embedlineno = n.Type.ForwardType().Embedlineno;
            var l = n.Type.ForwardType().Copyto;

            var ptrBase = n.Type.PtrBase;
            var sliceOf = n.Type.SliceOf; 

            // TODO(mdempsky): Fix Type rekinding.
            n.Type.Value = t.Value;

            t = n.Type;
            t.Sym = n.Sym;
            if (n.Name != null)
            {
                t.Vargen = n.Name.Vargen;
            } 

            // spec: "The declared type does not inherit any methods bound
            // to the existing type, but the method set of an interface
            // type [...] remains unchanged."
            if (!t.IsInterface())
            {
                t.Methods().Value = new types.Fields();
                t.AllMethods().Value = new types.Fields();
            }
            t.Nod = asTypesNode(n);
            t.SetDeferwidth(false);
            t.PtrBase = ptrBase;
            t.SliceOf = sliceOf; 

            // Propagate go:notinheap pragma from the Name to the Type.
            if (n.Name != null && n.Name.Param != null && n.Name.Param.Pragma & NotInHeap != 0L)
            {
                t.SetNotInHeap(true);
            } 

            // Update nodes waiting on this type.
            foreach (var (_, n) in l)
            {
                copytype(asNode(n), t);
            } 

            // Double-check use of type as embedded type.
            var lno = lineno;

            if (embedlineno.IsKnown())
            {
                lineno = embedlineno;
                if (t.IsPtr() || t.IsUnsafePtr())
                {
                    yyerror("embedded type cannot be a pointer");
                }
            }
            lineno = lno;
        }

        private static void typecheckdeftype(ref Node n)
        {
            var lno = lineno;
            setlineno(n);
            n.Type.Sym = n.Sym;
            n.SetTypecheck(1L);
            n.Name.Param.Ntype = typecheck(n.Name.Param.Ntype, Etype);
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
                copytype(n, t);
            }
            lineno = lno;
        }

        private static void typecheckdef(ref Node n)
        {
            var lno = lineno;
            setlineno(n);

            if (n.Op == ONONAME)
            {
                if (!n.Diag())
                {
                    n.SetDiag(true);
                    if (n.Pos.IsKnown())
                    {
                        lineno = n.Pos;
                    } 

                    // Note: adderrorname looks for this string and
                    // adds context about the outer expression
                    yyerror("undefined: %v", n.Sym);
                }
                return;
            }
            if (n.Walkdef() == 1L)
            {
                return;
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

            if (n.Op == OGOTO || n.Op == OLABEL || n.Op == OPACK)             else if (n.Op == OLITERAL) 
                if (n.Name.Param.Ntype != null)
                {
                    n.Name.Param.Ntype = typecheck(n.Name.Param.Ntype, Etype);
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
                    lineno = n.Pos;
                    Dump("typecheckdef nil defn", n);
                    yyerror("xxx");
                }
                e = typecheck(e, Erv);
                if (Isconst(e, CTNIL))
                {
                    yyerror("const initializer cannot be nil");
                    goto ret;
                }
                if (e.Type != null && e.Op != OLITERAL || !isgoconst(e))
                {
                    if (!e.Diag())
                    {
                        yyerror("const initializer %v is not a constant", e);
                        e.SetDiag(true);
                    }
                    goto ret;
                }
                var t = n.Type;
                if (t != null)
                {
                    if (!okforconst[t.Etype])
                    {
                        yyerror("invalid constant type %v", t);
                        goto ret;
                    }
                    if (!e.Type.IsUntyped() && !eqtype(t, e.Type))
                    {
                        yyerror("cannot use %L as type %v in const initializer", e, t);
                        goto ret;
                    }
                    e = convlit(e, t);
                }
                n.SetVal(e.Val());
                n.Type = e.Type;
            else if (n.Op == ONAME) 
                if (n.Name.Param.Ntype != null)
                {
                    n.Name.Param.Ntype = typecheck(n.Name.Param.Ntype, Etype);
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
                    if (n.Etype != 0L)
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
                    n.Name.Defn = typecheck(n.Name.Defn, Erv);
                    n.Type = n.Name.Defn.Type;
                    break;
                }
                n.Name.Defn = typecheck(n.Name.Defn, Etop); // fills in n.Type
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
                            p.Ntype = typecheck(p.Ntype, Etype);
                            n.Type = p.Ntype.Type;
                            if (n.Type == null)
                            {
                                n.SetDiag(true);
                                goto ret;
                            }
                            n.Sym.Def = asTypesNode(p.Ntype);
                        }
                        break;
                    } 

                    // regular type declaration

                } 

                // regular type declaration
                if (Curfn != null)
                {
                    defercheckwidth();
                }
                n.SetWalkdef(1L);
                n.Type = types.New(TFORW);
                n.Type.Nod = asTypesNode(n);
                n.Type.Sym = n.Sym; // TODO(gri) this also happens in typecheckdeftype(n) - where should it happen?
                var nerrors0 = nerrors;
                typecheckdeftype(n);
                if (n.Type.Etype == TFORW && nerrors > nerrors0)
                { 
                    // Something went wrong during type-checking,
                    // but it was reported. Silence future errors.
                    n.Type.SetBroke(true);
                }
                if (Curfn != null)
                {
                    resumecheckwidth();
                }
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
        }

        private static bool checkmake(ref types.Type t, @string arg, ref Node n)
        {
            if (!n.Type.IsInteger() && n.Type.Etype != TIDEAL)
            {
                yyerror("non-integer %s argument in make(%v) - %v", arg, t, n.Type);
                return false;
            } 

            // Do range checks for constants before defaultlit
            // to avoid redundant "constant NNN overflows int" errors.

            if (consttype(n) == CTINT || consttype(n) == CTRUNE || consttype(n) == CTFLT || consttype(n) == CTCPLX) 
                n.SetVal(toint(n.Val()));
                if (n.Val().U._<ref Mpint>().CmpInt64(0L) < 0L)
                {
                    yyerror("negative %s argument in make(%v)", arg, t);
                    return false;
                }
                if (n.Val().U._<ref Mpint>().Cmp(maxintval[TINT]) > 0L)
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

        private static void markbreak(ref Node n, ref Node @implicit)
        {
            if (n == null)
            {
                return;
            }

            if (n.Op == OBREAK)
            {
                if (n.Left == null)
                {
                    if (implicit != null)
                    {
                        @implicit.SetHasBreak(true);
                    }
                }
                else
                {
                    var lab = asNode(n.Left.Sym.Label);
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
                markbreak(n.Left, implicit);
                markbreak(n.Right, implicit);
                markbreaklist(n.Ninit, implicit);
                markbreaklist(n.Nbody, implicit);
                markbreaklist(n.List, implicit);
                markbreaklist(n.Rlist, implicit);

            __switch_break1:;
        }

        private static void markbreaklist(Nodes l, ref Node @implicit)
        {
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
                        n.Left.Sym.Label = asTypesNode(n.Name.Defn);
                        markbreak(n.Name.Defn, n.Name.Defn);
                        n.Left.Sym.Label = null;
                        i++;
                        continue;
                                    }
                markbreak(n, implicit);
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
        private static bool isterminating(this ref Node n)
        {

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
        private static void checkreturn(ref Node fn)
        {
            if (fn.Type.NumResults() != 0L && fn.Nbody.Len() != 0L)
            {
                markbreaklist(fn.Nbody, null);
                if (!fn.Nbody.isterminating())
                {
                    yyerrorl(fn.Func.Endlineno, "missing return at end of function");
                }
            }
        }

        private static void deadcode(ref Node fn)
        {
            deadcodeslice(fn.Nbody);
        }

        private static void deadcodeslice(Nodes nn)
        {
            foreach (var (_, n) in nn.Slice())
            {
                if (n == null)
                {
                    continue;
                }
                if (n.Op == OIF && Isconst(n.Left, CTBOOL))
                {
                    if (n.Left.Bool())
                    {
                        n.Rlist = new Nodes();
                    }
                    else
                    {
                        n.Nbody = new Nodes();
                    }
                }
                deadcodeslice(n.Ninit);
                deadcodeslice(n.Nbody);
                deadcodeslice(n.List);
                deadcodeslice(n.Rlist);
            }
        }
    }
}}}}
