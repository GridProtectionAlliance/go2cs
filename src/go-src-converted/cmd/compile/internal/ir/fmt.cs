// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:49:06 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\fmt.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using io = go.io_package;
using math = go.math_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using strings = go.strings_package;

using utf8 = go.unicode.utf8_package;

using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ir_package {

    // Op
public static @string OpNames = new slice<@string>(InitKeyedValues<@string>((OADDR, "&"), (OADD, "+"), (OADDSTR, "+"), (OALIGNOF, "unsafe.Alignof"), (OANDAND, "&&"), (OANDNOT, "&^"), (OAND, "&"), (OAPPEND, "append"), (OAS, "="), (OAS2, "="), (OBREAK, "break"), (OCALL, "function call"), (OCAP, "cap"), (OCASE, "case"), (OCLOSE, "close"), (OCOMPLEX, "complex"), (OBITNOT, "^"), (OCONTINUE, "continue"), (OCOPY, "copy"), (ODELETE, "delete"), (ODEFER, "defer"), (ODIV, "/"), (OEQ, "=="), (OFALL, "fallthrough"), (OFOR, "for"), (OFORUNTIL, "foruntil"), (OGE, ">="), (OGOTO, "goto"), (OGT, ">"), (OIF, "if"), (OIMAG, "imag"), (OINLMARK, "inlmark"), (ODEREF, "*"), (OLEN, "len"), (OLE, "<="), (OLSH, "<<"), (OLT, "<"), (OMAKE, "make"), (ONEG, "-"), (OMOD, "%"), (OMUL, "*"), (ONEW, "new"), (ONE, "!="), (ONOT, "!"), (OOFFSETOF, "unsafe.Offsetof"), (OOROR, "||"), (OOR, "|"), (OPANIC, "panic"), (OPLUS, "+"), (OPRINTN, "println"), (OPRINT, "print"), (ORANGE, "range"), (OREAL, "real"), (ORECV, "<-"), (ORECOVER, "recover"), (ORETURN, "return"), (ORSH, ">>"), (OSELECT, "select"), (OSEND, "<-"), (OSIZEOF, "unsafe.Sizeof"), (OSUB, "-"), (OSWITCH, "switch"), (OUNSAFEADD, "unsafe.Add"), (OUNSAFESLICE, "unsafe.Slice"), (OXOR, "^")));

// GoString returns the Go syntax for the Op, or else its name.
public static @string GoString(this Op o) {
    if (int(o) < len(OpNames) && OpNames[o] != "") {
        return OpNames[o];
    }
    return o.String();

}

// Format implements formatting for an Op.
// The valid formats are:
//
//    %v    Go syntax ("+", "<-", "print")
//    %+v    Debug syntax ("ADD", "RECV", "PRINT")
//
public static void Format(this Op o, fmt.State s, int verb) {
    switch (verb) {
        case 'v': 
            if (s.Flag('+')) { 
                // %+v is OMUL instead of "*"
                io.WriteString(s, o.String());
                return ;

            }
            io.WriteString(s, o.GoString());

            break;
        default: 
            fmt.Fprintf(s, "%%!%c(Op=%d)", verb, int(o));
            break;
    }

}

// Node

// FmtNode implements formatting for a Node n.
// Every Node implementation must define a Format method that calls FmtNode.
// The valid formats are:
//
//    %v    Go syntax
//    %L    Go syntax followed by " (type T)" if type is known.
//    %+v    Debug syntax, as in Dump.
//
private static void fmtNode(Node n, fmt.State s, int verb) { 
    // %+v prints Dump.
    // Otherwise we print Go syntax.
    if (s.Flag('+') && verb == 'v') {
        dumpNode(s, n, 1);
        return ;
    }
    if (verb != 'v' && verb != 'S' && verb != 'L') {
        fmt.Fprintf(s, "%%!%c(*Node=%p)", verb, n);
        return ;
    }
    if (n == null) {
        fmt.Fprint(s, "<nil>");
        return ;
    }
    var t = n.Type();
    if (verb == 'L' && t != null) {
        if (t.Kind() == types.TNIL) {
            fmt.Fprint(s, "nil");
        }
        else if (n.Op() == ONAME && n.Name().AutoTemp()) {
            fmt.Fprintf(s, "%v value", t);
        }
        else
 {
            fmt.Fprintf(s, "%v (type %v)", n, t);
        }
        return ;

    }
    if (OpPrec[n.Op()] < 0) {
        stmtFmt(n, s);
        return ;
    }
    exprFmt(n, s, 0);

}

public static nint OpPrec = new slice<nint>(InitKeyedValues<nint>((OALIGNOF, 8), (OAPPEND, 8), (OBYTES2STR, 8), (OARRAYLIT, 8), (OSLICELIT, 8), (ORUNES2STR, 8), (OCALLFUNC, 8), (OCALLINTER, 8), (OCALLMETH, 8), (OCALL, 8), (OCAP, 8), (OCLOSE, 8), (OCOMPLIT, 8), (OCONVIFACE, 8), (OCONVNOP, 8), (OCONV, 8), (OCOPY, 8), (ODELETE, 8), (OGETG, 8), (OLEN, 8), (OLITERAL, 8), (OMAKESLICE, 8), (OMAKESLICECOPY, 8), (OMAKE, 8), (OMAPLIT, 8), (ONAME, 8), (ONEW, 8), (ONIL, 8), (ONONAME, 8), (OOFFSETOF, 8), (OPACK, 8), (OPANIC, 8), (OPAREN, 8), (OPRINTN, 8), (OPRINT, 8), (ORUNESTR, 8), (OSIZEOF, 8), (OSLICE2ARRPTR, 8), (OSTR2BYTES, 8), (OSTR2RUNES, 8), (OSTRUCTLIT, 8), (OTARRAY, 8), (OTSLICE, 8), (OTCHAN, 8), (OTFUNC, 8), (OTINTER, 8), (OTMAP, 8), (OTSTRUCT, 8), (OTYPE, 8), (OUNSAFEADD, 8), (OUNSAFESLICE, 8), (OINDEXMAP, 8), (OINDEX, 8), (OSLICE, 8), (OSLICESTR, 8), (OSLICEARR, 8), (OSLICE3, 8), (OSLICE3ARR, 8), (OSLICEHEADER, 8), (ODOTINTER, 8), (ODOTMETH, 8), (ODOTPTR, 8), (ODOTTYPE2, 8), (ODOTTYPE, 8), (ODOT, 8), (OXDOT, 8), (OCALLPART, 8), (OMETHEXPR, 8), (OPLUS, 7), (ONOT, 7), (OBITNOT, 7), (ONEG, 7), (OADDR, 7), (ODEREF, 7), (ORECV, 7), (OMUL, 6), (ODIV, 6), (OMOD, 6), (OLSH, 6), (ORSH, 6), (OAND, 6), (OANDNOT, 6), (OADD, 5), (OSUB, 5), (OOR, 5), (OXOR, 5), (OEQ, 4), (OLT, 4), (OLE, 4), (OGE, 4), (OGT, 4), (ONE, 4), (OSEND, 3), (OANDAND, 2), (OOROR, 1), (OAS, -1), (OAS2, -1), (OAS2DOTTYPE, -1), (OAS2FUNC, -1), (OAS2MAPR, -1), (OAS2RECV, -1), (OASOP, -1), (OBLOCK, -1), (OBREAK, -1), (OCASE, -1), (OCONTINUE, -1), (ODCL, -1), (ODEFER, -1), (OFALL, -1), (OFOR, -1), (OFORUNTIL, -1), (OGOTO, -1), (OIF, -1), (OLABEL, -1), (OGO, -1), (ORANGE, -1), (ORETURN, -1), (OSELECT, -1), (OSWITCH, -1), (OEND, 0)));

// StmtWithInit reports whether op is a statement with an explicit init list.
public static bool StmtWithInit(Op op) {

    if (op == OIF || op == OFOR || op == OFORUNTIL || op == OSWITCH) 
        return true;
        return false;

}

private static void stmtFmt(Node n, fmt.State s) { 
    // NOTE(rsc): This code used to support the text-based
    // which was more aggressive about printing full Go syntax
    // (for example, an actual loop instead of "for loop").
    // The code is preserved for now in case we want to expand
    // any of those shortenings later. Or maybe we will delete
    // the code. But for now, keep it.
    const var exportFormat = false; 

    // some statements allow for an init, but at most one,
    // but we may have an arbitrary number added, eg by typecheck
    // and inlining. If it doesn't fit the syntax, emit an enclosing
    // block starting with the init statements.

    // if we can just say "for" n->ninit; ... then do so
 

    // some statements allow for an init, but at most one,
    // but we may have an arbitrary number added, eg by typecheck
    // and inlining. If it doesn't fit the syntax, emit an enclosing
    // block starting with the init statements.

    // if we can just say "for" n->ninit; ... then do so
    var simpleinit = len(n.Init()) == 1 && len(n.Init()[0].Init()) == 0 && StmtWithInit(n.Op()); 

    // otherwise, print the inits as separate statements
    var complexinit = len(n.Init()) != 0 && !simpleinit && exportFormat; 

    // but if it was for if/for/switch, put in an extra surrounding block to limit the scope
    var extrablock = complexinit && StmtWithInit(n.Op());

    if (extrablock) {
        fmt.Fprint(s, "{");
    }
    if (complexinit) {
        fmt.Fprintf(s, " %v; ", n.Init());
    }

    if (n.Op() == ODCL) 
        ptr<Decl> n = n._<ptr<Decl>>();
        fmt.Fprintf(s, "var %v %v", n.X.Sym(), n.X.Type()); 

        // Don't export "v = <N>" initializing statements, hope they're always
        // preceded by the DCL which will be re-parsed and typechecked to reproduce
        // the "v = <N>" again.
    else if (n.Op() == OAS) 
        n = n._<ptr<AssignStmt>>();
        if (n.Def && !complexinit) {
            fmt.Fprintf(s, "%v := %v", n.X, n.Y);
        }
        else
 {
            fmt.Fprintf(s, "%v = %v", n.X, n.Y);
        }
    else if (n.Op() == OASOP) 
        n = n._<ptr<AssignOpStmt>>();
        if (n.IncDec) {
            if (n.AsOp == OADD) {
                fmt.Fprintf(s, "%v++", n.X);
            }
            else
 {
                fmt.Fprintf(s, "%v--", n.X);
            }

            break;

        }
        fmt.Fprintf(s, "%v %v= %v", n.X, n.AsOp, n.Y);
    else if (n.Op() == OAS2 || n.Op() == OAS2DOTTYPE || n.Op() == OAS2FUNC || n.Op() == OAS2MAPR || n.Op() == OAS2RECV) 
        n = n._<ptr<AssignListStmt>>();
        if (n.Def && !complexinit) {
            fmt.Fprintf(s, "%.v := %.v", n.Lhs, n.Rhs);
        }
        else
 {
            fmt.Fprintf(s, "%.v = %.v", n.Lhs, n.Rhs);
        }
    else if (n.Op() == OBLOCK) 
        n = n._<ptr<BlockStmt>>();
        if (len(n.List) != 0) {
            fmt.Fprintf(s, "%v", n.List);
        }
    else if (n.Op() == ORETURN) 
        n = n._<ptr<ReturnStmt>>();
        fmt.Fprintf(s, "return %.v", n.Results);
    else if (n.Op() == OTAILCALL) 
        n = n._<ptr<TailCallStmt>>();
        fmt.Fprintf(s, "tailcall %v", n.Target);
    else if (n.Op() == OINLMARK) 
        n = n._<ptr<InlineMarkStmt>>();
        fmt.Fprintf(s, "inlmark %d", n.Index);
    else if (n.Op() == OGO) 
        n = n._<ptr<GoDeferStmt>>();
        fmt.Fprintf(s, "go %v", n.Call);
    else if (n.Op() == ODEFER) 
        n = n._<ptr<GoDeferStmt>>();
        fmt.Fprintf(s, "defer %v", n.Call);
    else if (n.Op() == OIF) 
        n = n._<ptr<IfStmt>>();
        if (simpleinit) {
            fmt.Fprintf(s, "if %v; %v { %v }", n.Init()[0], n.Cond, n.Body);
        }
        else
 {
            fmt.Fprintf(s, "if %v { %v }", n.Cond, n.Body);
        }
        if (len(n.Else) != 0) {
            fmt.Fprintf(s, " else { %v }", n.Else);
        }
    else if (n.Op() == OFOR || n.Op() == OFORUNTIL) 
        n = n._<ptr<ForStmt>>();
        @string opname = "for";
        if (n.Op() == OFORUNTIL) {
            opname = "foruntil";
        }
        if (!exportFormat) { // TODO maybe only if FmtShort, same below
            fmt.Fprintf(s, "%s loop", opname);
            break;

        }
        fmt.Fprint(s, opname);
        if (simpleinit) {
            fmt.Fprintf(s, " %v;", n.Init()[0]);
        }
        else if (n.Post != null) {
            fmt.Fprint(s, " ;");
        }
        if (n.Cond != null) {
            fmt.Fprintf(s, " %v", n.Cond);
        }
        if (n.Post != null) {
            fmt.Fprintf(s, "; %v", n.Post);
        }
        else if (simpleinit) {
            fmt.Fprint(s, ";");
        }
        if (n.Op() == OFORUNTIL && len(n.Late) != 0) {
            fmt.Fprintf(s, "; %v", n.Late);
        }
        fmt.Fprintf(s, " { %v }", n.Body);
    else if (n.Op() == ORANGE) 
        n = n._<ptr<RangeStmt>>();
        if (!exportFormat) {
            fmt.Fprint(s, "for loop");
            break;
        }
        fmt.Fprint(s, "for");
        if (n.Key != null) {
            fmt.Fprintf(s, " %v", n.Key);
            if (n.Value != null) {
                fmt.Fprintf(s, ", %v", n.Value);
            }
            fmt.Fprint(s, " =");
        }
        fmt.Fprintf(s, " range %v { %v }", n.X, n.Body);
    else if (n.Op() == OSELECT) 
        n = n._<ptr<SelectStmt>>();
        if (!exportFormat) {
            fmt.Fprintf(s, "%v statement", n.Op());
            break;
        }
        fmt.Fprintf(s, "select { %v }", n.Cases);
    else if (n.Op() == OSWITCH) 
        n = n._<ptr<SwitchStmt>>();
        if (!exportFormat) {
            fmt.Fprintf(s, "%v statement", n.Op());
            break;
        }
        fmt.Fprintf(s, "switch");
        if (simpleinit) {
            fmt.Fprintf(s, " %v;", n.Init()[0]);
        }
        if (n.Tag != null) {
            fmt.Fprintf(s, " %v ", n.Tag);
        }
        fmt.Fprintf(s, " { %v }", n.Cases);
    else if (n.Op() == OCASE) 
        n = n._<ptr<CaseClause>>();
        if (len(n.List) != 0) {
            fmt.Fprintf(s, "case %.v", n.List);
        }
        else
 {
            fmt.Fprint(s, "default");
        }
        fmt.Fprintf(s, ": %v", n.Body);
    else if (n.Op() == OBREAK || n.Op() == OCONTINUE || n.Op() == OGOTO || n.Op() == OFALL) 
        n = n._<ptr<BranchStmt>>();
        if (n.Label != null) {
            fmt.Fprintf(s, "%v %v", n.Op(), n.Label);
        }
        else
 {
            fmt.Fprintf(s, "%v", n.Op());
        }
    else if (n.Op() == OLABEL) 
        n = n._<ptr<LabelStmt>>();
        fmt.Fprintf(s, "%v: ", n.Label);
        if (extrablock) {
        fmt.Fprint(s, "}");
    }
}

private static void exprFmt(Node n, fmt.State s, nint prec) { 
    // NOTE(rsc): This code used to support the text-based
    // which was more aggressive about printing full Go syntax
    // (for example, an actual loop instead of "for loop").
    // The code is preserved for now in case we want to expand
    // any of those shortenings later. Or maybe we will delete
    // the code. But for now, keep it.
    const var exportFormat = false;



    while (true) {
        if (n == null) {
            fmt.Fprint(s, "<nil>");
            return ;
        }
        {
            var o = Orig(n);

            if (o != n) {
                n = o;
                continue;
            } 

            // Skip implicit operations introduced during typechecking.

        } 

        // Skip implicit operations introduced during typechecking.
        {
            var nn__prev1 = nn;

            var nn = n;


            if (nn.Op() == OADDR) 
                nn = nn._<ptr<AddrExpr>>();
                if (nn.Implicit()) {
                    n = nn.X;
                    continue;
                }
            else if (nn.Op() == ODEREF) 
                nn = nn._<ptr<StarExpr>>();
                if (nn.Implicit()) {
                    n = nn.X;
                    continue;
                }
            else if (nn.Op() == OCONV || nn.Op() == OCONVNOP || nn.Op() == OCONVIFACE) 
                nn = nn._<ptr<ConvExpr>>();
                if (nn.Implicit()) {
                    n = nn.X;
                    continue;
                }


            nn = nn__prev1;
        }

        break;

    }

    var nprec = OpPrec[n.Op()];
    if (n.Op() == OTYPE && n.Type().IsPtr()) {
        nprec = OpPrec[ODEREF];
    }
    if (prec > nprec) {
        fmt.Fprintf(s, "(%v)", n);
        return ;
    }

    if (n.Op() == OPAREN)
    {
        ptr<ParenExpr> n = n._<ptr<ParenExpr>>();
        fmt.Fprintf(s, "(%v)", n.X);
        goto __switch_break1;
    }
    if (n.Op() == ONIL)
    {
        fmt.Fprint(s, "nil");
        goto __switch_break1;
    }
    if (n.Op() == OLITERAL) // this is a bit of a mess
    {
        if (!exportFormat && n.Sym() != null) {
            fmt.Fprint(s, n.Sym());
            return ;
        }
        var needUnparen = false;
        if (n.Type() != null && !n.Type().IsUntyped()) { 
            // Need parens when type begins with what might
            // be misinterpreted as a unary operator: * or <-.
            if (n.Type().IsPtr() || (n.Type().IsChan() && n.Type().ChanDir() == types.Crecv)) {
                fmt.Fprintf(s, "(%v)(", n.Type());
            }
            else
 {
                fmt.Fprintf(s, "%v(", n.Type());
            }

            needUnparen = true;

        }
        if (n.Type() == types.UntypedRune) {
            {
                var (x, ok) = constant.Uint64Val(n.Val());


                if (!ok)
                {
                    fallthrough = true;
                }
                if (fallthrough || x < utf8.RuneSelf)
                {
                    fmt.Fprintf(s, "%q", x);
                    goto __switch_break0;
                }
                if (x < 1 << 16)
                {
                    fmt.Fprintf(s, "'\\u%04x'", x);
                    goto __switch_break0;
                }
                if (x <= utf8.MaxRune)
                {
                    fmt.Fprintf(s, "'\\U%08x'", x);
                    goto __switch_break0;
                }
                // default: 
                    fmt.Fprintf(s, "('\\x00' + %v)", n.Val());

                __switch_break0:;
            }

        }
        else
 {
            fmt.Fprint(s, types.FmtConst(n.Val(), s.Flag('#')));
        }
        if (needUnparen) {
            fmt.Fprintf(s, ")");
        }
        goto __switch_break1;
    }
    if (n.Op() == ODCLFUNC)
    {
        n = n._<ptr<Func>>();
        {
            var sym = n.Sym();

            if (sym != null) {
                fmt.Fprint(s, sym);
                return ;
            }

        }

        fmt.Fprintf(s, "<unnamed Func>");
        goto __switch_break1;
    }
    if (n.Op() == ONAME)
    {
        n = n._<ptr<Name>>(); 
        // Special case: name used as local variable in export.
        // _ becomes ~b%d internally; print as _ for export
        if (!exportFormat && n.Sym() != null && n.Sym().Name[0] == '~' && n.Sym().Name[1] == 'b') {
            fmt.Fprint(s, "_");
            return ;
        }
        fallthrough = true;
    }
    if (fallthrough || n.Op() == OPACK || n.Op() == ONONAME)
    {
        fmt.Fprint(s, n.Sym());
        goto __switch_break1;
    }
    if (n.Op() == OLINKSYMOFFSET)
    {
        n = n._<ptr<LinksymOffsetExpr>>();
        fmt.Fprintf(s, "(%v)(%s@%d)", n.Type(), n.Linksym.Name, n.Offset_);
        goto __switch_break1;
    }
    if (n.Op() == OTYPE)
    {
        if (n.Type() == null && n.Sym() != null) {
            fmt.Fprint(s, n.Sym());
            return ;
        }
        fmt.Fprintf(s, "%v", n.Type());
        goto __switch_break1;
    }
    if (n.Op() == OTSLICE)
    {
        n = n._<ptr<SliceType>>();
        if (n.DDD) {
            fmt.Fprintf(s, "...%v", n.Elem);
        }
        else
 {
            fmt.Fprintf(s, "[]%v", n.Elem); // happens before typecheck
        }
        goto __switch_break1;
    }
    if (n.Op() == OTARRAY)
    {
        n = n._<ptr<ArrayType>>();
        if (n.Len == null) {
            fmt.Fprintf(s, "[...]%v", n.Elem);
        }
        else
 {
            fmt.Fprintf(s, "[%v]%v", n.Len, n.Elem);
        }
        goto __switch_break1;
    }
    if (n.Op() == OTMAP)
    {
        n = n._<ptr<MapType>>();
        fmt.Fprintf(s, "map[%v]%v", n.Key, n.Elem);
        goto __switch_break1;
    }
    if (n.Op() == OTCHAN)
    {
        n = n._<ptr<ChanType>>();

        if (n.Dir == types.Crecv) 
            fmt.Fprintf(s, "<-chan %v", n.Elem);
        else if (n.Dir == types.Csend) 
            fmt.Fprintf(s, "chan<- %v", n.Elem);
        else 
            if (n.Elem != null && n.Elem.Op() == OTCHAN && n.Elem._<ptr<ChanType>>().Dir == types.Crecv) {
                fmt.Fprintf(s, "chan (%v)", n.Elem);
            }
            else
 {
                fmt.Fprintf(s, "chan %v", n.Elem);
            }

                goto __switch_break1;
    }
    if (n.Op() == OTSTRUCT)
    {
        fmt.Fprint(s, "<struct>");
        goto __switch_break1;
    }
    if (n.Op() == OTINTER)
    {
        fmt.Fprint(s, "<inter>");
        goto __switch_break1;
    }
    if (n.Op() == OTFUNC)
    {
        fmt.Fprint(s, "<func>");
        goto __switch_break1;
    }
    if (n.Op() == OCLOSURE)
    {
        n = n._<ptr<ClosureExpr>>();
        if (!exportFormat) {
            fmt.Fprint(s, "func literal");
            return ;
        }
        fmt.Fprintf(s, "%v { %v }", n.Type(), n.Func.Body);
        goto __switch_break1;
    }
    if (n.Op() == OCOMPLIT)
    {
        n = n._<ptr<CompLitExpr>>();
        if (!exportFormat) {
            if (n.Implicit()) {
                fmt.Fprintf(s, "... argument");
                return ;
            }
            if (n.Ntype != null) {
                fmt.Fprintf(s, "%v{%s}", n.Ntype, ellipsisIf(len(n.List) != 0));
                return ;
            }
            fmt.Fprint(s, "composite literal");
            return ;
        }
        fmt.Fprintf(s, "(%v{ %.v })", n.Ntype, n.List);
        goto __switch_break1;
    }
    if (n.Op() == OPTRLIT)
    {
        n = n._<ptr<AddrExpr>>();
        fmt.Fprintf(s, "&%v", n.X);
        goto __switch_break1;
    }
    if (n.Op() == OSTRUCTLIT || n.Op() == OARRAYLIT || n.Op() == OSLICELIT || n.Op() == OMAPLIT)
    {
        n = n._<ptr<CompLitExpr>>();
        if (!exportFormat) {
            fmt.Fprintf(s, "%v{%s}", n.Type(), ellipsisIf(len(n.List) != 0));
            return ;
        }
        fmt.Fprintf(s, "(%v{ %.v })", n.Type(), n.List);
        goto __switch_break1;
    }
    if (n.Op() == OKEY)
    {
        n = n._<ptr<KeyExpr>>();
        if (n.Key != null && n.Value != null) {
            fmt.Fprintf(s, "%v:%v", n.Key, n.Value);
            return ;
        }
        if (n.Key == null && n.Value != null) {
            fmt.Fprintf(s, ":%v", n.Value);
            return ;
        }
        if (n.Key != null && n.Value == null) {
            fmt.Fprintf(s, "%v:", n.Key);
            return ;
        }
        fmt.Fprint(s, ":");
        goto __switch_break1;
    }
    if (n.Op() == OSTRUCTKEY)
    {
        n = n._<ptr<StructKeyExpr>>();
        fmt.Fprintf(s, "%v:%v", n.Field, n.Value);
        goto __switch_break1;
    }
    if (n.Op() == OXDOT || n.Op() == ODOT || n.Op() == ODOTPTR || n.Op() == ODOTINTER || n.Op() == ODOTMETH || n.Op() == OCALLPART || n.Op() == OMETHEXPR)
    {
        n = n._<ptr<SelectorExpr>>();
        exprFmt(n.X, s, nprec);
        if (n.Sel == null) {
            fmt.Fprint(s, ".<nil>");
            return ;
        }
        fmt.Fprintf(s, ".%s", n.Sel.Name);
        goto __switch_break1;
    }
    if (n.Op() == ODOTTYPE || n.Op() == ODOTTYPE2)
    {
        n = n._<ptr<TypeAssertExpr>>();
        exprFmt(n.X, s, nprec);
        if (n.Ntype != null) {
            fmt.Fprintf(s, ".(%v)", n.Ntype);
            return ;
        }
        fmt.Fprintf(s, ".(%v)", n.Type());
        goto __switch_break1;
    }
    if (n.Op() == OINDEX || n.Op() == OINDEXMAP)
    {
        n = n._<ptr<IndexExpr>>();
        exprFmt(n.X, s, nprec);
        fmt.Fprintf(s, "[%v]", n.Index);
        goto __switch_break1;
    }
    if (n.Op() == OSLICE || n.Op() == OSLICESTR || n.Op() == OSLICEARR || n.Op() == OSLICE3 || n.Op() == OSLICE3ARR)
    {
        n = n._<ptr<SliceExpr>>();
        exprFmt(n.X, s, nprec);
        fmt.Fprint(s, "[");
        if (n.Low != null) {
            fmt.Fprint(s, n.Low);
        }
        fmt.Fprint(s, ":");
        if (n.High != null) {
            fmt.Fprint(s, n.High);
        }
        if (n.Op().IsSlice3()) {
            fmt.Fprint(s, ":");
            if (n.Max != null) {
                fmt.Fprint(s, n.Max);
            }
        }
        fmt.Fprint(s, "]");
        goto __switch_break1;
    }
    if (n.Op() == OSLICEHEADER)
    {
        n = n._<ptr<SliceHeaderExpr>>();
        fmt.Fprintf(s, "sliceheader{%v,%v,%v}", n.Ptr, n.Len, n.Cap);
        goto __switch_break1;
    }
    if (n.Op() == OCOMPLEX || n.Op() == OCOPY || n.Op() == OUNSAFEADD || n.Op() == OUNSAFESLICE)
    {
        n = n._<ptr<BinaryExpr>>();
        fmt.Fprintf(s, "%v(%v, %v)", n.Op(), n.X, n.Y);
        goto __switch_break1;
    }
    if (n.Op() == OCONV || n.Op() == OCONVIFACE || n.Op() == OCONVNOP || n.Op() == OBYTES2STR || n.Op() == ORUNES2STR || n.Op() == OSTR2BYTES || n.Op() == OSTR2RUNES || n.Op() == ORUNESTR || n.Op() == OSLICE2ARRPTR)
    {
        n = n._<ptr<ConvExpr>>();
        if (n.Type() == null || n.Type().Sym() == null) {
            fmt.Fprintf(s, "(%v)", n.Type());
        }
        else
 {
            fmt.Fprintf(s, "%v", n.Type());
        }
        fmt.Fprintf(s, "(%v)", n.X);
        goto __switch_break1;
    }
    if (n.Op() == OREAL || n.Op() == OIMAG || n.Op() == OCAP || n.Op() == OCLOSE || n.Op() == OLEN || n.Op() == ONEW || n.Op() == OPANIC || n.Op() == OALIGNOF || n.Op() == OOFFSETOF || n.Op() == OSIZEOF)
    {
        n = n._<ptr<UnaryExpr>>();
        fmt.Fprintf(s, "%v(%v)", n.Op(), n.X);
        goto __switch_break1;
    }
    if (n.Op() == OAPPEND || n.Op() == ODELETE || n.Op() == OMAKE || n.Op() == ORECOVER || n.Op() == OPRINT || n.Op() == OPRINTN)
    {
        n = n._<ptr<CallExpr>>();
        if (n.IsDDD) {
            fmt.Fprintf(s, "%v(%.v...)", n.Op(), n.Args);
            return ;
        }
        fmt.Fprintf(s, "%v(%.v)", n.Op(), n.Args);
        goto __switch_break1;
    }
    if (n.Op() == OCALL || n.Op() == OCALLFUNC || n.Op() == OCALLINTER || n.Op() == OCALLMETH || n.Op() == OGETG)
    {
        n = n._<ptr<CallExpr>>();
        exprFmt(n.X, s, nprec);
        if (n.IsDDD) {
            fmt.Fprintf(s, "(%.v...)", n.Args);
            return ;
        }
        fmt.Fprintf(s, "(%.v)", n.Args);
        goto __switch_break1;
    }
    if (n.Op() == OMAKEMAP || n.Op() == OMAKECHAN || n.Op() == OMAKESLICE)
    {
        n = n._<ptr<MakeExpr>>();
        if (n.Cap != null) {
            fmt.Fprintf(s, "make(%v, %v, %v)", n.Type(), n.Len, n.Cap);
            return ;
        }
        if (n.Len != null && (n.Op() == OMAKESLICE || !n.Len.Type().IsUntyped())) {
            fmt.Fprintf(s, "make(%v, %v)", n.Type(), n.Len);
            return ;
        }
        fmt.Fprintf(s, "make(%v)", n.Type());
        goto __switch_break1;
    }
    if (n.Op() == OMAKESLICECOPY)
    {
        n = n._<ptr<MakeExpr>>();
        fmt.Fprintf(s, "makeslicecopy(%v, %v, %v)", n.Type(), n.Len, n.Cap);
        goto __switch_break1;
    }
    if (n.Op() == OPLUS || n.Op() == ONEG || n.Op() == OBITNOT || n.Op() == ONOT || n.Op() == ORECV) 
    {
        // Unary
        n = n._<ptr<UnaryExpr>>();
        fmt.Fprintf(s, "%v", n.Op());
        if (n.X != null && n.X.Op() == n.Op()) {
            fmt.Fprint(s, " ");
        }
        exprFmt(n.X, s, nprec + 1);
        goto __switch_break1;
    }
    if (n.Op() == OADDR)
    {
        n = n._<ptr<AddrExpr>>();
        fmt.Fprintf(s, "%v", n.Op());
        if (n.X != null && n.X.Op() == n.Op()) {
            fmt.Fprint(s, " ");
        }
        exprFmt(n.X, s, nprec + 1);
        goto __switch_break1;
    }
    if (n.Op() == ODEREF)
    {
        n = n._<ptr<StarExpr>>();
        fmt.Fprintf(s, "%v", n.Op());
        exprFmt(n.X, s, nprec + 1); 

        // Binary
        goto __switch_break1;
    }
    if (n.Op() == OADD || n.Op() == OAND || n.Op() == OANDNOT || n.Op() == ODIV || n.Op() == OEQ || n.Op() == OGE || n.Op() == OGT || n.Op() == OLE || n.Op() == OLT || n.Op() == OLSH || n.Op() == OMOD || n.Op() == OMUL || n.Op() == ONE || n.Op() == OOR || n.Op() == ORSH || n.Op() == OSUB || n.Op() == OXOR)
    {
        n = n._<ptr<BinaryExpr>>();
        exprFmt(n.X, s, nprec);
        fmt.Fprintf(s, " %v ", n.Op());
        exprFmt(n.Y, s, nprec + 1);
        goto __switch_break1;
    }
    if (n.Op() == OANDAND || n.Op() == OOROR)
    {
        n = n._<ptr<LogicalExpr>>();
        exprFmt(n.X, s, nprec);
        fmt.Fprintf(s, " %v ", n.Op());
        exprFmt(n.Y, s, nprec + 1);
        goto __switch_break1;
    }
    if (n.Op() == OSEND)
    {
        n = n._<ptr<SendStmt>>();
        exprFmt(n.Chan, s, nprec);
        fmt.Fprintf(s, " <- ");
        exprFmt(n.Value, s, nprec + 1);
        goto __switch_break1;
    }
    if (n.Op() == OADDSTR)
    {
        n = n._<ptr<AddStringExpr>>();
        foreach (var (i, n1) in n.List) {
            if (i != 0) {
                fmt.Fprint(s, " + ");
            }
            exprFmt(n1, s, nprec);
        }        goto __switch_break1;
    }
    // default: 
        fmt.Fprintf(s, "<node %v>", n.Op());

    __switch_break1:;

}

private static @string ellipsisIf(bool b) {
    if (b) {
        return "...";
    }
    return "";

}

// Nodes

// Format implements formatting for a Nodes.
// The valid formats are:
//
//    %v    Go syntax, semicolon-separated
//    %.v    Go syntax, comma-separated
//    %+v    Debug syntax, as in DumpList.
//
public static void Format(this Nodes l, fmt.State s, int verb) {
    if (s.Flag('+') && verb == 'v') { 
        // %+v is DumpList output
        dumpNodes(s, l, 1);
        return ;

    }
    if (verb != 'v') {
        fmt.Fprintf(s, "%%!%c(Nodes)", verb);
        return ;
    }
    @string sep = "; ";
    {
        var (_, ok) = s.Precision();

        if (ok) { // %.v is expr list
            sep = ", ";

        }
    }


    foreach (var (i, n) in l) {
        fmt.Fprint(s, n);
        if (i + 1 < len(l)) {
            fmt.Fprint(s, sep);
        }
    }
}

// Dump

// Dump prints the message s followed by a debug dump of n.
public static void Dump(@string s, Node n) {
    fmt.Printf("%s [%p]%+v\n", s, n, n);
}

// DumpList prints the message s followed by a debug dump of each node in the list.
public static void DumpList(@string s, Nodes list) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    FDumpList(_addr_buf, s, list);
    os.Stdout.Write(buf.Bytes());
}

// FDumpList prints to w the message s followed by a debug dump of each node in the list.
public static void FDumpList(io.Writer w, @string s, Nodes list) {
    io.WriteString(w, s);
    dumpNodes(w, list, 1);
    io.WriteString(w, "\n");
}

// indent prints indentation to w.
private static void indent(io.Writer w, nint depth) {
    fmt.Fprint(w, "\n");
    for (nint i = 0; i < depth; i++) {
        fmt.Fprint(w, ".   ");
    }
}

// EscFmt is set by the escape analysis code to add escape analysis details to the node print.
public static Func<Node, @string> EscFmt = default;

// dumpNodeHeader prints the debug-format node header line to w.
private static void dumpNodeHeader(io.Writer w, Node n) => func((defer, _, recover) => { 
    // Useful to see which nodes in an AST printout are actually identical
    if (@base.Debug.DumpPtrs != 0) {
        fmt.Fprintf(w, " p(%p)", n);
    }
    if (@base.Debug.DumpPtrs != 0 && n.Name() != null && n.Name().Defn != null) { 
        // Useful to see where Defn is set and what node it points to
        fmt.Fprintf(w, " defn(%p)", n.Name().Defn);

    }
    if (@base.Debug.DumpPtrs != 0 && n.Name() != null && n.Name().Curfn != null) { 
        // Useful to see where Defn is set and what node it points to
        fmt.Fprintf(w, " curfn(%p)", n.Name().Curfn);

    }
    if (@base.Debug.DumpPtrs != 0 && n.Name() != null && n.Name().Outer != null) { 
        // Useful to see where Defn is set and what node it points to
        fmt.Fprintf(w, " outer(%p)", n.Name().Outer);

    }
    if (EscFmt != null) {
        {
            var esc = EscFmt(n);

            if (esc != "") {
                fmt.Fprintf(w, " %s", esc);
            }

        }

    }
    if (n.Typecheck() != 0) {
        fmt.Fprintf(w, " tc(%d)", n.Typecheck());
    }
    var v = reflect.ValueOf(n).Elem();
    var t = v.Type();
    var nf = t.NumField();
    {
        nint i__prev1 = i;

        for (nint i = 0; i < nf; i++) {
            var tf = t.Field(i);
            if (tf.PkgPath != "") { 
                // skip unexported field - Interface will fail
                continue;

            }

            var k = tf.Type.Kind();
            if (reflect.Bool <= k && k <= reflect.Complex128) {
                var name = strings.TrimSuffix(tf.Name, "_");
                var vf = v.Field(i);
                var vfi = vf.Interface();
                if (name == "Offset" && vfi == types.BADWIDTH || name != "Offset" && isZero(vf)) {
                    continue;
                }
                if (vfi == true) {
                    fmt.Fprintf(w, " %s", name);
                }
                else
 {
                    fmt.Fprintf(w, " %s:%+v", name, vf.Interface());
                }

            }

        }

        i = i__prev1;
    } 

    // Print Node-specific booleans by looking for methods.
    // Different v, t from above - want *Struct not Struct, for methods.
    v = reflect.ValueOf(n);
    t = v.Type();
    var nm = t.NumMethod();
    {
        nint i__prev1 = i;

        for (i = 0; i < nm; i++) {
            var tm = t.Method(i);
            if (tm.PkgPath != "") { 
                // skip unexported method - call will fail
                continue;

            }

            var m = v.Method(i);
            var mt = m.Type();
            if (mt.NumIn() == 0 && mt.NumOut() == 1 && mt.Out(0).Kind() == reflect.Bool) { 
                // TODO(rsc): Remove the func/defer/recover wrapping,
                // which is guarding against panics in miniExpr,
                // once we get down to the simpler state in which
                // nodes have no getter methods that aren't allowed to be called.
                () => {
                    defer(() => {
                        recover();
                    }());
                    if (m.Call(null)[0].Bool()) {
                        name = strings.TrimSuffix(tm.Name, "_");
                        fmt.Fprintf(w, " %s", name);
                    }

                }();

            }

        }

        i = i__prev1;
    }

    if (n.Op() == OCLOSURE) {
        ptr<ClosureExpr> n = n._<ptr<ClosureExpr>>();
        {
            var fn = n.Func;

            if (fn != null && fn.Nname.Sym() != null) {
                fmt.Fprintf(w, " fnName(%+v)", fn.Nname.Sym());
            }

        }

    }
    if (n.Type() != null) {
        if (n.Op() == OTYPE) {
            fmt.Fprintf(w, " type");
        }
        fmt.Fprintf(w, " %+v", n.Type());

    }
    if (n.Pos().IsKnown()) {
        @string pfx = "";

        if (n.Pos().IsStmt() == src.PosNotStmt) 
            pfx = "_"; // "-" would be confusing
        else if (n.Pos().IsStmt() == src.PosIsStmt) 
            pfx = "+";
                var pos = @base.Ctxt.PosTable.Pos(n.Pos());
        var file = filepath.Base(pos.Filename());
        fmt.Fprintf(w, " # %s%s:%d", pfx, file, pos.Line());

    }
});

private static void dumpNode(io.Writer w, Node n, nint depth) {
    indent(w, depth);
    if (depth > 40) {
        fmt.Fprint(w, "...");
        return ;
    }
    if (n == null) {
        fmt.Fprint(w, "NilIrNode");
        return ;
    }
    if (len(n.Init()) != 0) {
        fmt.Fprintf(w, "%+v-init", n.Op());
        dumpNodes(w, n.Init(), depth + 1);
        indent(w, depth);
    }

    if (n.Op() == OLITERAL) 
        fmt.Fprintf(w, "%+v-%v", n.Op(), n.Val());
        dumpNodeHeader(w, n);
        return ;
    else if (n.Op() == ONAME || n.Op() == ONONAME) 
        if (n.Sym() != null) {
            fmt.Fprintf(w, "%+v-%+v", n.Op(), n.Sym());
        }
        else
 {
            fmt.Fprintf(w, "%+v", n.Op());
        }
        dumpNodeHeader(w, n);
        if (n.Type() == null && n.Name() != null && n.Name().Ntype != null) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-ntype", n.Op());
            dumpNode(w, n.Name().Ntype, depth + 1);
        }
        return ;
    else if (n.Op() == OASOP) 
        ptr<AssignOpStmt> n = n._<ptr<AssignOpStmt>>();
        fmt.Fprintf(w, "%+v-%+v", n.Op(), n.AsOp);
        dumpNodeHeader(w, n);
    else if (n.Op() == OTYPE) 
        fmt.Fprintf(w, "%+v %+v", n.Op(), n.Sym());
        dumpNodeHeader(w, n);
        if (n.Type() == null && n.Name() != null && n.Name().Ntype != null) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-ntype", n.Op());
            dumpNode(w, n.Name().Ntype, depth + 1);
        }
        return ;
    else if (n.Op() == OCLOSURE) 
        fmt.Fprintf(w, "%+v", n.Op());
        dumpNodeHeader(w, n);
    else if (n.Op() == ODCLFUNC) 
        // Func has many fields we don't want to print.
        // Bypass reflection and just print what we want.
        n = n._<ptr<Func>>();
        fmt.Fprintf(w, "%+v", n.Op());
        dumpNodeHeader(w, n);
        var fn = n;
        if (len(fn.Dcl) > 0) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-Dcl", n.Op());
            foreach (var (_, dcl) in n.Dcl) {
                dumpNode(w, dcl, depth + 1);
            }
        }
        if (len(fn.ClosureVars) > 0) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-ClosureVars", n.Op());
            foreach (var (_, cv) in fn.ClosureVars) {
                dumpNode(w, cv, depth + 1);
            }
        }
        if (len(fn.Enter) > 0) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-Enter", n.Op());
            dumpNodes(w, fn.Enter, depth + 1);
        }
        if (len(fn.Body) > 0) {
            indent(w, depth);
            fmt.Fprintf(w, "%+v-body", n.Op());
            dumpNodes(w, fn.Body, depth + 1);
        }
        return ;
    else 
        fmt.Fprintf(w, "%+v", n.Op());
        dumpNodeHeader(w, n);
        if (n.Sym() != null) {
        fmt.Fprintf(w, " %+v", n.Sym());
    }
    if (n.Type() != null) {
        fmt.Fprintf(w, " %+v", n.Type());
    }
    var v = reflect.ValueOf(n).Elem();
    var t = reflect.TypeOf(n).Elem();
    var nf = t.NumField();
    {
        nint i__prev1 = i;

        for (nint i = 0; i < nf; i++) {
            var tf = t.Field(i);
            var vf = v.Field(i);
            if (tf.PkgPath != "") { 
                // skip unexported field - Interface will fail
                continue;

            }


            if (tf.Type.Kind() == reflect.Interface || tf.Type.Kind() == reflect.Ptr || tf.Type.Kind() == reflect.Slice) 
                if (vf.IsNil()) {
                    continue;
                }
                        var name = strings.TrimSuffix(tf.Name, "_"); 
            // Do not bother with field name header lines for the
            // most common positional arguments: unary, binary expr,
            // index expr, send stmt, go and defer call expression.
            switch (name) {
                case "X": 

                case "Y": 

                case "Index": 

                case "Chan": 

                case "Value": 

                case "Call": 
                    name = "";
                    break;
            }
            switch (vf.Interface().type()) {
                case Node val:
                    if (name != "") {
                        indent(w, depth);
                        fmt.Fprintf(w, "%+v-%s", n.Op(), name);
                    }
                    dumpNode(w, val, depth + 1);
                    break;
                case Nodes val:
                    if (len(val) == 0) {
                        continue;
                    }
                    if (name != "") {
                        indent(w, depth);
                        fmt.Fprintf(w, "%+v-%s", n.Op(), name);
                    }
                    dumpNodes(w, val, depth + 1);
                    break;
                default:
                {
                    var val = vf.Interface().type();
                    if (vf.Kind() == reflect.Slice && vf.Type().Elem().Implements(nodeType)) {
                        if (vf.Len() == 0) {
                            continue;
                        }
                        if (name != "") {
                            indent(w, depth);
                            fmt.Fprintf(w, "%+v-%s", n.Op(), name);
                        }
                        {
                            nint i__prev2 = i;
                            ptr<AssignOpStmt> n__prev2 = n;

                            for (i = 0;
                            n = vf.Len(); i < n; i++) {
                                dumpNode(w, vf.Index(i).Interface()._<Node>(), depth + 1);
                            }


                            i = i__prev2;
                            n = n__prev2;
                        }

                    }

                    break;
                }
            }

        }

        i = i__prev1;
    }

}

private static var nodeType = reflect.TypeOf((Node.val)(null)).Elem();

private static void dumpNodes(io.Writer w, Nodes list, nint depth) {
    if (len(list) == 0) {
        fmt.Fprintf(w, " <nil>");
        return ;
    }
    foreach (var (_, n) in list) {
        dumpNode(w, n, depth);
    }
}

// reflect.IsZero is not available in Go 1.4 (added in Go 1.13), so we use this copy instead.
private static bool isZero(reflect.Value v) {

    if (v.Kind() == reflect.Bool) 
        return !v.Bool();
    else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
        return v.Int() == 0;
    else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
        return v.Uint() == 0;
    else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
        return math.Float64bits(v.Float()) == 0;
    else if (v.Kind() == reflect.Complex64 || v.Kind() == reflect.Complex128) 
        var c = v.Complex();
        return math.Float64bits(real(c)) == 0 && math.Float64bits(imag(c)) == 0;
    else if (v.Kind() == reflect.Array) 
        {
            nint i__prev1 = i;

            for (nint i = 0; i < v.Len(); i++) {
                if (!isZero(v.Index(i))) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (v.Kind() == reflect.Chan || v.Kind() == reflect.Func || v.Kind() == reflect.Interface || v.Kind() == reflect.Map || v.Kind() == reflect.Ptr || v.Kind() == reflect.Slice || v.Kind() == reflect.UnsafePointer) 
        return v.IsNil();
    else if (v.Kind() == reflect.String) 
        return v.Len() == 0;
    else if (v.Kind() == reflect.Struct) 
        {
            nint i__prev1 = i;

            for (i = 0; i < v.NumField(); i++) {
                if (!isZero(v.Field(i))) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else 
        return false;
    
}

} // end ir_package
