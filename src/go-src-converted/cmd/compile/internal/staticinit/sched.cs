// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package staticinit -- go2cs converted at 2022 March 13 06:25:02 UTC
// import "cmd/compile/internal/staticinit" ==> using staticinit = go.cmd.compile.@internal.staticinit_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\staticinit\sched.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using constant = go.constant_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using reflectdata = cmd.compile.@internal.reflectdata_package;
using staticdata = cmd.compile.@internal.staticdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using src = cmd.@internal.src_package;
using System;

public static partial class staticinit_package {

public partial struct Entry {
    public long Xoffset; // struct, array only
    public ir.Node Expr; // bytes of run-time computed expressions
}

public partial struct Plan {
    public slice<Entry> E;
}

// An Schedule is used to decompose assignment statements into
// static and dynamic initialization parts. Static initializations are
// handled by populating variables' linker symbol data, while dynamic
// initializations are accumulated to be executed in order.
public partial struct Schedule {
    public slice<ir.Node> Out;
    public map<ir.Node, ptr<Plan>> Plans;
    public map<ir.Node, ptr<ir.Name>> Temps;
}

private static void append(this ptr<Schedule> _addr_s, ir.Node n) {
    ref Schedule s = ref _addr_s.val;

    s.Out = append(s.Out, n);
}

// StaticInit adds an initialization statement n to the schedule.
private static void StaticInit(this ptr<Schedule> _addr_s, ir.Node n) {
    ref Schedule s = ref _addr_s.val;

    if (!s.tryStaticInit(n)) {
        if (@base.Flag.Percent != 0) {
            ir.Dump("nonstatic", n);
        }
        s.append(n);
    }
}

// tryStaticInit attempts to statically execute an initialization
// statement and reports whether it succeeded.
private static bool tryStaticInit(this ptr<Schedule> _addr_s, ir.Node nn) => func((defer, _, _) => {
    ref Schedule s = ref _addr_s.val;
 
    // Only worry about simple "l = r" assignments. Multiple
    // variable/expression OAS2 assignments have already been
    // replaced by multiple simple OAS assignments, and the other
    // OAS2* assignments mostly necessitate dynamic execution
    // anyway.
    if (nn.Op() != ir.OAS) {
        return false;
    }
    ptr<ir.AssignStmt> n = nn._<ptr<ir.AssignStmt>>();
    if (ir.IsBlank(n.X) && !AnySideEffects(n.Y)) { 
        // Discard.
        return true;
    }
    var lno = ir.SetPos(n);
    defer(() => {
        @base.Pos = lno;
    }());
    ptr<ir.Name> nam = n.X._<ptr<ir.Name>>();
    return s.StaticAssign(nam, 0, n.Y, nam.Type());
});

// like staticassign but we are copying an already
// initialized value r.
private static bool staticcopy(this ptr<Schedule> _addr_s, ptr<ir.Name> _addr_l, long loff, ptr<ir.Name> _addr_rn, ptr<types.Type> _addr_typ) {
    ref Schedule s = ref _addr_s.val;
    ref ir.Name l = ref _addr_l.val;
    ref ir.Name rn = ref _addr_rn.val;
    ref types.Type typ = ref _addr_typ.val;

    if (rn.Class == ir.PFUNC) { 
        // TODO if roff != 0 { panic }
        staticdata.InitAddr(l, loff, staticdata.FuncLinksym(rn));
        return true;
    }
    if (rn.Class != ir.PEXTERN || rn.Sym().Pkg != types.LocalPkg) {
        return false;
    }
    if (rn.Defn.Op() != ir.OAS) {
        return false;
    }
    if (rn.Type().IsString()) { // perhaps overwritten by cmd/link -X (#34675)
        return false;
    }
    if (rn.Embed != null) {
        return false;
    }
    var orig = rn;
    ptr<ir.AssignStmt> r = rn.Defn._<ptr<ir.AssignStmt>>().Y;
    if (r == null) { 
        // No explicit initialization value. Probably zeroed but perhaps
        // supplied externally and of unknown value.
        return false;
    }
    while (r.Op() == ir.OCONVNOP && !types.Identical(r.Type(), typ)) {
        r = r._<ptr<ir.ConvExpr>>().X;
    }


    if (r.Op() == ir.OMETHEXPR)
    {
        r = r._<ptr<ir.SelectorExpr>>().FuncName();
        fallthrough = true;
    }
    if (fallthrough || r.Op() == ir.ONAME)
    {
        r = r._<ptr<ir.Name>>();
        if (s.staticcopy(l, loff, r, typ)) {
            return true;
        }
        var dst = ir.Node(l);
        if (loff != 0 || !types.Identical(typ, l.Type())) {
            dst = ir.NewNameOffsetExpr(@base.Pos, l, loff, typ);
        }
        s.append(ir.NewAssignStmt(@base.Pos, dst, typecheck.Conv(r, typ)));
        return true;
        goto __switch_break0;
    }
    if (r.Op() == ir.ONIL)
    {
        return true;
        goto __switch_break0;
    }
    if (r.Op() == ir.OLITERAL)
    {
        if (ir.IsZero(r)) {
            return true;
        }
        staticdata.InitConst(l, loff, r, int(typ.Width));
        return true;
        goto __switch_break0;
    }
    if (r.Op() == ir.OADDR)
    {
        r = r._<ptr<ir.AddrExpr>>();
        {
            ptr<ir.Name> (a, ok) = r.X._<ptr<ir.Name>>();

            if (ok && a.Op() == ir.ONAME) {
                staticdata.InitAddr(l, loff, staticdata.GlobalLinksym(a));
                return true;
            }

        }
        goto __switch_break0;
    }
    if (r.Op() == ir.OPTRLIT)
    {
        r = r._<ptr<ir.AddrExpr>>();

        if (r.X.Op() == ir.OARRAYLIT || r.X.Op() == ir.OSLICELIT || r.X.Op() == ir.OSTRUCTLIT || r.X.Op() == ir.OMAPLIT) 
            // copy pointer
            staticdata.InitAddr(l, loff, staticdata.GlobalLinksym(s.Temps[r]));
            return true;
                goto __switch_break0;
    }
    if (r.Op() == ir.OSLICELIT)
    {
        r = r._<ptr<ir.CompLitExpr>>(); 
        // copy slice
        staticdata.InitSlice(l, loff, staticdata.GlobalLinksym(s.Temps[r]), r.Len);
        return true;
        goto __switch_break0;
    }
    if (r.Op() == ir.OARRAYLIT || r.Op() == ir.OSTRUCTLIT)
    {
        r = r._<ptr<ir.CompLitExpr>>();
        var p = s.Plans[r];
        foreach (var (i) in p.E) {
            var e = _addr_p.E[i];
            var typ = e.Expr.Type();
            if (e.Expr.Op() == ir.OLITERAL || e.Expr.Op() == ir.ONIL) {
                staticdata.InitConst(l, loff + e.Xoffset, e.Expr, int(typ.Width));
                continue;
            }
            var x = e.Expr;
            if (x.Op() == ir.OMETHEXPR) {
                x = x._<ptr<ir.SelectorExpr>>().FuncName();
            }
            if (x.Op() == ir.ONAME && s.staticcopy(l, loff + e.Xoffset, x._<ptr<ir.Name>>(), typ)) {
                continue;
            } 
            // Requires computation, but we're
            // copying someone else's computation.
            var ll = ir.NewNameOffsetExpr(@base.Pos, l, loff + e.Xoffset, typ);
            var rr = ir.NewNameOffsetExpr(@base.Pos, orig, e.Xoffset, typ);
            ir.SetPos(rr);
            s.append(ir.NewAssignStmt(@base.Pos, ll, rr));
        }        return true;
        goto __switch_break0;
    }

    __switch_break0:;

    return false;
}

private static bool StaticAssign(this ptr<Schedule> _addr_s, ptr<ir.Name> _addr_l, long loff, ir.Node r, ptr<types.Type> _addr_typ) {
    ref Schedule s = ref _addr_s.val;
    ref ir.Name l = ref _addr_l.val;
    ref types.Type typ = ref _addr_typ.val;

    if (r == null) { 
        // No explicit initialization value. Either zero or supplied
        // externally.
        return true;
    }
    while (r.Op() == ir.OCONVNOP) {
        r = r._<ptr<ir.ConvExpr>>().X;
    }

    Action<src.XPos, ptr<ir.Name>, long, ir.Node> assign = (pos, a, aoff, v) => {
        if (s.StaticAssign(a, aoff, v, v.Type())) {
            return ;
        }
        ir.Node lhs = default;
        if (ir.IsBlank(a)) { 
            // Don't use NameOffsetExpr with blank (#43677).
            lhs = ir.BlankNode;
        }
        else
 {
            lhs = ir.NewNameOffsetExpr(pos, a, aoff, v.Type());
        }
        s.append(ir.NewAssignStmt(pos, lhs, v));
    };


    if (r.Op() == ir.ONAME)
    {
        ptr<ir.Name> r = r._<ptr<ir.Name>>();
        return s.staticcopy(l, loff, r, typ);
        goto __switch_break1;
    }
    if (r.Op() == ir.OMETHEXPR)
    {
        r = r._<ptr<ir.SelectorExpr>>();
        return s.staticcopy(l, loff, r.FuncName(), typ);
        goto __switch_break1;
    }
    if (r.Op() == ir.ONIL)
    {
        return true;
        goto __switch_break1;
    }
    if (r.Op() == ir.OLITERAL)
    {
        if (ir.IsZero(r)) {
            return true;
        }
        staticdata.InitConst(l, loff, r, int(typ.Width));
        return true;
        goto __switch_break1;
    }
    if (r.Op() == ir.OADDR)
    {
        r = r._<ptr<ir.AddrExpr>>();
        {
            var (name, offset, ok) = StaticLoc(r.X);

            if (ok && name.Class == ir.PEXTERN) {
                staticdata.InitAddrOffset(l, loff, name.Linksym(), offset);
                return true;
            }

        }
        fallthrough = true;

    }
    if (fallthrough || r.Op() == ir.OPTRLIT)
    {
        r = r._<ptr<ir.AddrExpr>>();

        if (r.X.Op() == ir.OARRAYLIT || r.X.Op() == ir.OSLICELIT || r.X.Op() == ir.OMAPLIT || r.X.Op() == ir.OSTRUCTLIT) 
            // Init pointer.
            var a = StaticName(_addr_r.X.Type());

            s.Temps[r] = a;
            staticdata.InitAddr(l, loff, a.Linksym()); 

            // Init underlying literal.
            assign(@base.Pos, a, 0, r.X);
            return true;
        //dump("not static ptrlit", r);
        goto __switch_break1;
    }
    if (r.Op() == ir.OSTR2BYTES)
    {
        r = r._<ptr<ir.ConvExpr>>();
        if (l.Class == ir.PEXTERN && r.X.Op() == ir.OLITERAL) {
            var sval = ir.StringVal(r.X);
            staticdata.InitSliceBytes(l, loff, sval);
            return true;
        }
        goto __switch_break1;
    }
    if (r.Op() == ir.OSLICELIT)
    {
        r = r._<ptr<ir.CompLitExpr>>();
        s.initplan(r); 
        // Init slice.
        var ta = types.NewArray(r.Type().Elem(), r.Len);
        ta.SetNoalg(true);
        a = StaticName(_addr_ta);
        s.Temps[r] = a;
        staticdata.InitSlice(l, loff, a.Linksym(), r.Len); 
        // Fall through to init underlying array.
        l = a;
        loff = 0;
        fallthrough = true;

    }
    if (fallthrough || r.Op() == ir.OARRAYLIT || r.Op() == ir.OSTRUCTLIT)
    {
        r = r._<ptr<ir.CompLitExpr>>();
        s.initplan(r);

        var p = s.Plans[r];
        foreach (var (i) in p.E) {
            var e = _addr_p.E[i];
            if (e.Expr.Op() == ir.OLITERAL || e.Expr.Op() == ir.ONIL) {
                staticdata.InitConst(l, loff + e.Xoffset, e.Expr, int(e.Expr.Type().Width));
                continue;
            }
            ir.SetPos(e.Expr);
            assign(@base.Pos, l, loff + e.Xoffset, e.Expr);
        }        return true;
        goto __switch_break1;
    }
    if (r.Op() == ir.OMAPLIT)
    {
        break;
        goto __switch_break1;
    }
    if (r.Op() == ir.OCLOSURE)
    {
        r = r._<ptr<ir.ClosureExpr>>();
        if (ir.IsTrivialClosure(r)) {
            if (@base.Debug.Closure > 0) {
                @base.WarnfAt(r.Pos(), "closure converted to global");
            } 
            // Closures with no captured variables are globals,
            // so the assignment can be done at link time.
            // TODO if roff != 0 { panic }
            staticdata.InitAddr(l, loff, staticdata.FuncLinksym(r.Func.Nname));
            return true;
        }
        ir.ClosureDebugRuntimeCheck(r);
        goto __switch_break1;
    }
    if (r.Op() == ir.OCONVIFACE) 
    {
        // This logic is mirrored in isStaticCompositeLiteral.
        // If you change something here, change it there, and vice versa.

        // Determine the underlying concrete type and value we are converting from.
        r = r._<ptr<ir.ConvExpr>>();
        var val = ir.Node(r);
        while (val.Op() == ir.OCONVIFACE) {
            val = val._<ptr<ir.ConvExpr>>().X;
        }

        if (val.Type().IsInterface()) { 
            // val is an interface type.
            // If val is nil, we can statically initialize l;
            // both words are zero and so there no work to do, so report success.
            // If val is non-nil, we have no concrete type to record,
            // and we won't be able to statically initialize its value, so report failure.
            return val.Op() == ir.ONIL;
        }
        reflectdata.MarkTypeUsedInInterface(val.Type(), l.Linksym());

        ptr<ir.AddrExpr> itab;
        if (typ.IsEmptyInterface()) {
            itab = reflectdata.TypePtr(val.Type());
        }
        else
 {
            itab = reflectdata.ITabAddr(val.Type(), typ);
        }
        staticdata.InitAddr(l, loff, itab.X._<ptr<ir.LinksymOffsetExpr>>().Linksym); 

        // Emit data.
        if (types.IsDirectIface(val.Type())) {
            if (val.Op() == ir.ONIL) { 
                // Nil is zero, nothing to do.
                return true;
            } 
            // Copy val directly into n.
            ir.SetPos(val);
            assign(@base.Pos, l, loff + int64(types.PtrSize), val);
        }
        else
 { 
            // Construct temp to hold val, write pointer to temp into n.
            a = StaticName(_addr_val.Type());
            s.Temps[val] = a;
            assign(@base.Pos, a, 0, val);
            staticdata.InitAddr(l, loff + int64(types.PtrSize), a.Linksym());
        }
        return true;
        goto __switch_break1;
    }

    __switch_break1:; 

    //dump("not static", r);
    return false;
}

private static void initplan(this ptr<Schedule> _addr_s, ir.Node n) {
    ref Schedule s = ref _addr_s.val;

    if (s.Plans[n] != null) {
        return ;
    }
    ptr<Plan> p = @new<Plan>();
    s.Plans[n] = p;

    if (n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT) 
        ptr<ir.CompLitExpr> n = n._<ptr<ir.CompLitExpr>>();
        long k = default;
        {
            var a__prev1 = a;

            foreach (var (_, __a) in n.List) {
                a = __a;
                if (a.Op() == ir.OKEY) {
                    ptr<ir.KeyExpr> kv = a._<ptr<ir.KeyExpr>>();
                    k = typecheck.IndexConst(kv.Key);
                    if (k < 0) {
                        @base.Fatalf("initplan arraylit: invalid index %v", kv.Key);
                    }
                    a = kv.Value;
                }
                s.addvalue(p, k * n.Type().Elem().Width, a);
                k++;
            }

            a = a__prev1;
        }
    else if (n.Op() == ir.OSTRUCTLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var a__prev1 = a;

            foreach (var (_, __a) in n.List) {
                a = __a;
                if (a.Op() != ir.OSTRUCTKEY) {
                    @base.Fatalf("initplan structlit");
                }
                ptr<ir.StructKeyExpr> a = a._<ptr<ir.StructKeyExpr>>();
                if (a.Field.IsBlank()) {
                    continue;
                }
                s.addvalue(p, a.Offset, a.Value);
            }

            a = a__prev1;
        }
    else if (n.Op() == ir.OMAPLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        {
            var a__prev1 = a;

            foreach (var (_, __a) in n.List) {
                a = __a;
                if (a.Op() != ir.OKEY) {
                    @base.Fatalf("initplan maplit");
                }
                a = a._<ptr<ir.KeyExpr>>();
                s.addvalue(p, -1, a.Value);
            }

            a = a__prev1;
        }
    else 
        @base.Fatalf("initplan");
    }

private static void addvalue(this ptr<Schedule> _addr_s, ptr<Plan> _addr_p, long xoffset, ir.Node n) {
    ref Schedule s = ref _addr_s.val;
    ref Plan p = ref _addr_p.val;
 
    // special case: zero can be dropped entirely
    if (ir.IsZero(n)) {
        return ;
    }
    if (isvaluelit(n)) {
        s.initplan(n);
        var q = s.Plans[n];
        foreach (var (_, qe) in q.E) { 
            // qe is a copy; we are not modifying entries in q.E
            qe.Xoffset += xoffset;
            p.E = append(p.E, qe);
        }        return ;
    }
    p.E = append(p.E, new Entry(Xoffset:xoffset,Expr:n));
}

// from here down is the walk analysis
// of composite literals.
// most of the work is to generate
// data statements for the constant
// part of the composite literal.

private static nint statuniqgen = default; // name generator for static temps

// StaticName returns a name backed by a (writable) static data symbol.
// Use readonlystaticname for read-only node.
public static ptr<ir.Name> StaticName(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 
    // Don't use LookupNum; it interns the resulting string, but these are all unique.
    var n = typecheck.NewName(typecheck.Lookup(fmt.Sprintf("%s%d", obj.StaticNamePref, statuniqgen)));
    statuniqgen++;
    typecheck.Declare(n, ir.PEXTERN);
    n.SetType(t);
    return _addr_n!;
}

// StaticLoc returns the static address of n, if n has one, or else nil.
public static (ptr<ir.Name>, long, bool) StaticLoc(ir.Node n) {
    ptr<ir.Name> name = default!;
    long offset = default;
    bool ok = default;

    if (n == null) {
        return (_addr_null!, 0, false);
    }

    if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        return (_addr_n!, 0, true);
    else if (n.Op() == ir.OMETHEXPR) 
        n = n._<ptr<ir.SelectorExpr>>();
        return _addr_StaticLoc(n.FuncName())!;
    else if (n.Op() == ir.ODOT) 
        n = n._<ptr<ir.SelectorExpr>>();
        name, offset, ok = StaticLoc(n.X);

        if (!ok) {
            break;
        }
        offset += n.Offset();
        return (_addr_name!, offset, true);
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        if (n.X.Type().IsSlice()) {
            break;
        }
        name, offset, ok = StaticLoc(n.X);

        if (!ok) {
            break;
        }
        var l = getlit(n.Index);
        if (l < 0) {
            break;
        }
        if (n.Type().Width != 0 && types.MaxWidth / n.Type().Width <= int64(l)) {
            break;
        }
        offset += int64(l) * n.Type().Width;
        return (_addr_name!, offset, true);
        return (_addr_null!, 0, false);
}

// AnySideEffects reports whether n contains any operations that could have observable side effects.
public static bool AnySideEffects(ir.Node n) {
    return ir.Any(n, n => {

        // Assume side effects unless we know otherwise.
        if (n.Op() == ir.ONAME || n.Op() == ir.ONONAME || n.Op() == ir.OTYPE || n.Op() == ir.OPACK || n.Op() == ir.OLITERAL || n.Op() == ir.ONIL || n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OADDSTR || n.Op() == ir.OADDR || n.Op() == ir.OANDAND || n.Op() == ir.OBYTES2STR || n.Op() == ir.ORUNES2STR || n.Op() == ir.OSTR2BYTES || n.Op() == ir.OSTR2RUNES || n.Op() == ir.OCAP || n.Op() == ir.OCOMPLIT || n.Op() == ir.OMAPLIT || n.Op() == ir.OSTRUCTLIT || n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT || n.Op() == ir.OPTRLIT || n.Op() == ir.OCONV || n.Op() == ir.OCONVIFACE || n.Op() == ir.OCONVNOP || n.Op() == ir.ODOT || n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE || n.Op() == ir.OKEY || n.Op() == ir.OSTRUCTKEY || n.Op() == ir.OLEN || n.Op() == ir.OMUL || n.Op() == ir.OLSH || n.Op() == ir.ORSH || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.ONEW || n.Op() == ir.ONOT || n.Op() == ir.OBITNOT || n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OOROR || n.Op() == ir.OPAREN || n.Op() == ir.ORUNESTR || n.Op() == ir.OREAL || n.Op() == ir.OIMAG || n.Op() == ir.OCOMPLEX) 
            return false; 

            // Only possible side effect is division by zero.
        else if (n.Op() == ir.ODIV || n.Op() == ir.OMOD) 
            ptr<ir.BinaryExpr> n = n._<ptr<ir.BinaryExpr>>();
            if (n.Y.Op() != ir.OLITERAL || constant.Sign(n.Y.Val()) == 0) {
                return true;
            } 

            // Only possible side effect is panic on invalid size,
            // but many makechan and makemap use size zero, which is definitely OK.
        else if (n.Op() == ir.OMAKECHAN || n.Op() == ir.OMAKEMAP) 
            n = n._<ptr<ir.MakeExpr>>();
            if (!ir.IsConst(n.Len, constant.Int) || constant.Sign(n.Len.Val()) != 0) {
                return true;
            } 

            // Only possible side effect is panic on invalid size.
            // TODO(rsc): Merge with previous case (probably breaks toolstash -cmp).
        else if (n.Op() == ir.OMAKESLICE || n.Op() == ir.OMAKESLICECOPY) 
            return true;
        else 
            return true; 

            // No side effects here (arguments are checked separately).
                return false;
    });
}

private static nint getlit(ir.Node lit) {
    if (ir.IsSmallIntConst(lit)) {
        return int(ir.Int64Val(lit));
    }
    return -1;
}

private static bool isvaluelit(ir.Node n) {
    return n.Op() == ir.OARRAYLIT || n.Op() == ir.OSTRUCTLIT;
}

} // end staticinit_package
