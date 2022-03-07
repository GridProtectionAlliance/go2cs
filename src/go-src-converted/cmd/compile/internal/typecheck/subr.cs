// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:44 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\subr.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

public static ir.Node AssignConv(ir.Node n, ptr<types.Type> _addr_t, @string context) {
    ref types.Type t = ref _addr_t.val;

    return assignconvfn(n, _addr_t, () => context);
}

// DotImportRefs maps idents introduced by importDot back to the
// ir.PkgName they were dot-imported through.
public static map<ptr<ir.Ident>, ptr<ir.PkgName>> DotImportRefs = default;

// LookupNum looks up the symbol starting with prefix and ending with
// the decimal n. If prefix is too long, LookupNum panics.
public static ptr<types.Sym> LookupNum(@string prefix, nint n) {
    array<byte> buf = new array<byte>(20); // plenty long enough for all current users
    copy(buf[..], prefix);
    var b = strconv.AppendInt(buf[..(int)len(prefix)], int64(n), 10);
    return _addr_types.LocalPkg.LookupBytes(b)!;

}

// Given funarg struct list, return list of fn args.
public static slice<ptr<ir.Field>> NewFuncParams(ptr<types.Type> _addr_tl, bool mustname) {
    ref types.Type tl = ref _addr_tl.val;

    slice<ptr<ir.Field>> args = default;
    nint gen = 0;
    foreach (var (_, t) in tl.Fields().Slice()) {
        var s = t.Sym;
        if (mustname && (s == null || s.Name == "_")) { 
            // invent a name so that we can refer to it in the trampoline
            s = LookupNum(".anon", gen);
            gen++;

        }
        else if (s != null && s.Pkg != types.LocalPkg) { 
            // TODO(mdempsky): Preserve original position, name, and package.
            s = Lookup(s.Name);

        }
        var a = ir.NewField(@base.Pos, s, null, t.Type);
        a.Pos = t.Pos;
        a.IsDDD = t.IsDDD();
        args = append(args, a);

    }    return args;

}

// newname returns a new ONAME Node associated with symbol s.
public static ptr<ir.Name> NewName(ptr<types.Sym> _addr_s) {
    ref types.Sym s = ref _addr_s.val;

    var n = ir.NewNameAt(@base.Pos, s);
    n.Curfn = ir.CurFunc;
    return _addr_n!;
}

// NodAddr returns a node representing &n at base.Pos.
public static ptr<ir.AddrExpr> NodAddr(ir.Node n) {
    return _addr_NodAddrAt(@base.Pos, n)!;
}

// nodAddrPos returns a node representing &n at position pos.
public static ptr<ir.AddrExpr> NodAddrAt(src.XPos pos, ir.Node n) {
    n = markAddrOf(n);
    return _addr_ir.NewAddrExpr(pos, n)!;
}

private static ir.Node markAddrOf(ir.Node n) {
    if (IncrementalAddrtaken) { 
        // We can only do incremental addrtaken computation when it is ok
        // to typecheck the argument of the OADDR. That's only safe after the
        // main typecheck has completed.
        // The argument to OADDR needs to be typechecked because &x[i] takes
        // the address of x if x is an array, but not if x is a slice.
        // Note: OuterValue doesn't work correctly until n is typechecked.
        n = typecheck(n, ctxExpr);
        {
            var x = ir.OuterValue(n);

            if (x.Op() == ir.ONAME) {
                x.Name().SetAddrtaken(true);
            }

        }

    }
    else
 { 
        // Remember that we built an OADDR without computing the Addrtaken bit for
        // its argument. We'll do that later in bulk using computeAddrtaken.
        DirtyAddrtaken = true;

    }
    return n;

}

// If IncrementalAddrtaken is false, we do not compute Addrtaken for an OADDR Node
// when it is built. The Addrtaken bits are set in bulk by computeAddrtaken.
// If IncrementalAddrtaken is true, then when an OADDR Node is built the Addrtaken
// field of its argument is updated immediately.
public static var IncrementalAddrtaken = false;

// If DirtyAddrtaken is true, then there are OADDR whose corresponding arguments
// have not yet been marked as Addrtaken.
public static var DirtyAddrtaken = false;

public static void ComputeAddrtaken(slice<ir.Node> top) {
    foreach (var (_, n) in top) {
        Action<ir.Node> doVisit = default;
        doVisit = n => {
            if (n.Op() == ir.OADDR) {
                {
                    var x = ir.OuterValue(n._<ptr<ir.AddrExpr>>().X);

                    if (x.Op() == ir.ONAME) {
                        x.Name().SetAddrtaken(true);
                        if (x.Name().IsClosureVar()) { 
                            // Mark the original variable as Addrtaken so that capturevars
                            // knows not to pass it by value.
                            x.Name().Defn.Name().SetAddrtaken(true);

                        }

                    }

                }

            }

            if (n.Op() == ir.OCLOSURE) {
                ir.VisitList(n._<ptr<ir.ClosureExpr>>().Func.Body, doVisit);
            }

        };
        ir.Visit(n, doVisit);

    }
}

public static ir.Node NodNil() {
    var n = ir.NewNilExpr(@base.Pos);
    n.SetType(types.Types[types.TNIL]);
    return n;
}

// AddImplicitDots finds missing fields in obj.field that
// will give the shortest unique addressing and
// modifies the tree with missing field names.
public static ptr<ir.SelectorExpr> AddImplicitDots(ptr<ir.SelectorExpr> _addr_n) {
    ref ir.SelectorExpr n = ref _addr_n.val;

    n.X = typecheck(n.X, ctxType | ctxExpr);
    if (n.X.Diag()) {
        n.SetDiag(true);
    }
    var t = n.X.Type();
    if (t == null) {
        return _addr_n!;
    }
    if (n.X.Op() == ir.OTYPE) {
        return _addr_n!;
    }
    var s = n.Sel;
    if (s == null) {
        return _addr_n!;
    }
    {
        var (path, ambig) = dotpath(_addr_s, _addr_t, _addr_null, false);


        if (path != null) 
            // rebuild elided dots
            for (var c = len(path) - 1; c >= 0; c--) {
                var dot = ir.NewSelectorExpr(@base.Pos, ir.ODOT, n.X, path[c].field.Sym);
                dot.SetImplicit(true);
                dot.SetType(path[c].field.Type);
                n.X = dot;
            }
        else if (ambig) 
            @base.Errorf("ambiguous selector %v", n);
            n.X = null;

    }

    return _addr_n!;

}

public static void CalcMethods(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t == null || t.AllMethods().Len() != 0) {
        return ;
    }
    {
        var f__prev1 = f;

        foreach (var (_, __f) in t.Methods().Slice()) {
            f = __f;
            f.Sym.SetUniq(true);
        }
        f = f__prev1;
    }

    slist = slist[..(int)0];
    expand1(_addr_t, true); 

    // check each method to be uniquely reachable
    slice<ptr<types.Field>> ms = default;
    foreach (var (i, sl) in slist) {
        slist[i].field = null;
        sl.field.Sym.SetUniq(false);

        ptr<types.Field> f;
        var (path, _) = dotpath(_addr_sl.field.Sym, _addr_t, _addr_f, false);
        if (path == null) {
            continue;
        }
        if (!f.IsMethod()) {
            continue;
        }
        f = f.Copy();
        f.Embedded = 1; // needs a trampoline
        foreach (var (_, d) in path) {
            if (d.field.Type.IsPtr()) {
                f.Embedded = 2;
                break;
            }
        }        ms = append(ms, f);

    }    {
        var f__prev1 = f;

        foreach (var (_, __f) in t.Methods().Slice()) {
            f = __f;
            f.Sym.SetUniq(false);
        }
        f = f__prev1;
    }

    ms = append(ms, t.Methods().Slice());
    sort.Sort(types.MethodsByName(ms));
    t.SetAllMethods(ms);

}

// adddot1 returns the number of fields or methods named s at depth d in Type t.
// If exactly one exists, it will be returned in *save (if save is not nil),
// and dotlist will contain the path of embedded fields traversed to find it,
// in reverse order. If none exist, more will indicate whether t contains any
// embedded fields at depth d, so callers can decide whether to retry at
// a greater depth.
private static (nint, bool) adddot1(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, nint d, ptr<ptr<types.Field>> _addr_save, bool ignorecase) => func((defer, _, _) => {
    nint c = default;
    bool more = default;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ptr<types.Field> save = ref _addr_save.val;

    if (t.Recur()) {
        return ;
    }
    t.SetRecur(true);
    defer(t.SetRecur(false));

    ptr<types.Type> u;
    d--;
    if (d < 0) { 
        // We've reached our target depth. If t has any fields/methods
        // named s, then we're done. Otherwise, we still need to check
        // below for embedded fields.
        c = lookdot0(_addr_s, _addr_t, _addr_save, ignorecase);
        if (c != 0) {
            return (c, false);
        }
    }
    u = t;
    if (u.IsPtr()) {
        u = u.Elem();
    }
    if (!u.IsStruct() && !u.IsInterface()) {
        return (c, false);
    }
    ptr<types.Fields> fields;
    if (u.IsStruct()) {
        fields = u.Fields();
    }
    else
 {
        fields = u.AllMethods();
    }
    foreach (var (_, f) in fields.Slice()) {
        if (f.Embedded == 0 || f.Sym == null) {
            continue;
        }
        if (d < 0) { 
            // Found an embedded field at target depth.
            return (c, true);

        }
        var (a, more1) = adddot1(_addr_s, _addr_f.Type, d, _addr_save, ignorecase);
        if (a != 0 && c == 0) {
            dotlist[d].field = f;
        }
        c += a;
        if (more1) {
            more = true;
        }
    }    return (c, more);

});

// dotlist is used by adddot1 to record the path of embedded fields
// used to access a target field or method.
// Must be non-nil so that dotpath returns a non-nil slice even if d is zero.
private static var dotlist = make_slice<dlist>(10);

// Convert node n for assignment to type t.
private static ir.Node assignconvfn(ir.Node n, ptr<types.Type> _addr_t, Func<@string> context) {
    ref types.Type t = ref _addr_t.val;

    if (n == null || n.Type() == null || n.Type().Broke()) {
        return n;
    }
    if (t.Kind() == types.TBLANK && n.Type().Kind() == types.TNIL) {
        @base.Errorf("use of untyped nil");
    }
    n = convlit1(n, t, false, context);
    if (n.Type() == null) {
        return n;
    }
    if (t.Kind() == types.TBLANK) {
        return n;
    }
    if (n.Type() == types.UntypedBool && !t.IsBoolean()) {
        if (n.Op() == ir.ONAME || n.Op() == ir.OLITERAL) {
            var r = ir.NewConvExpr(@base.Pos, ir.OCONVNOP, null, n);
            r.SetType(types.Types[types.TBOOL]);
            r.SetTypecheck(1);
            r.SetImplicit(true);
            n = r;
        }
    }
    if (types.Identical(n.Type(), t)) {
        return n;
    }
    var (op, why) = Assignop(_addr_n.Type(), _addr_t);
    if (op == ir.OXXX) {
        @base.Errorf("cannot use %L as type %v in %s%s", n, t, context(), why);
        op = ir.OCONV;
    }
    r = ir.NewConvExpr(@base.Pos, op, t, n);
    r.SetTypecheck(1);
    r.SetImplicit(true);
    return r;

}

// Is type src assignment compatible to type dst?
// If so, return op code to use in conversion.
// If not, return OXXX. In this case, the string return parameter may
// hold a reason why. In all other cases, it'll be the empty string.
public static (ir.Op, @string) Assignop(ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst) {
    ir.Op _p0 = default;
    @string _p0 = default;
    ref types.Type src = ref _addr_src.val;
    ref types.Type dst = ref _addr_dst.val;

    if (src == dst) {
        return (ir.OCONVNOP, "");
    }
    if (src == null || dst == null || src.Kind() == types.TFORW || dst.Kind() == types.TFORW || src.Underlying() == null || dst.Underlying() == null) {
        return (ir.OXXX, "");
    }
    if (types.Identical(src, dst)) {
        return (ir.OCONVNOP, "");
    }
    if (types.Identical(src.Underlying(), dst.Underlying())) {
        if (src.IsEmptyInterface()) { 
            // Conversion between two empty interfaces
            // requires no code.
            return (ir.OCONVNOP, "");

        }
        if ((src.Sym() == null || dst.Sym() == null) && !src.IsInterface()) { 
            // Conversion between two types, at least one unnamed,
            // needs no conversion. The exception is nonempty interfaces
            // which need to have their itab updated.
            return (ir.OCONVNOP, "");

        }
    }
    if (dst.IsInterface() && src.Kind() != types.TNIL) {
        ptr<types.Field> missing;        ptr<types.Field> have;

        ref nint ptr = ref heap(out ptr<nint> _addr_ptr);
        if (implements(_addr_src, _addr_dst, _addr_missing, _addr_have, _addr_ptr)) { 
            // Call NeedITab/ITabAddr so that (src, dst)
            // gets added to itabs early, which allows
            // us to de-virtualize calls through this
            // type/interface pair later. See CompileITabs in reflect.go
            if (types.IsDirectIface(src) && !dst.IsEmptyInterface()) {
                NeedITab(src, dst);
            }

            return (ir.OCONVIFACE, "");

        }
        if (have != null && have.Sym == missing.Sym && (have.Type.Broke() || missing.Type.Broke())) {
            return (ir.OCONVIFACE, "");
        }
        @string why = default;
        if (isptrto(_addr_src, types.TINTER)) {
            why = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", src);
        }
        else if (have != null && have.Sym == missing.Sym && have.Nointerface()) {
            why = fmt.Sprintf(":\n\t%v does not implement %v (%v method is marked 'nointerface')", src, dst, missing.Sym);
        }
        else if (have != null && have.Sym == missing.Sym) {
            why = fmt.Sprintf(":\n\t%v does not implement %v (wrong type for %v method)\n" + "\t\thave %v%S\n\t\twant %v%S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
        }
        else if (ptr != 0) {
            why = fmt.Sprintf(":\n\t%v does not implement %v (%v method has pointer receiver)", src, dst, missing.Sym);
        }
        else if (have != null) {
            why = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)\n" + "\t\thave %v%S\n\t\twant %v%S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
        }
        else
 {
            why = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)", src, dst, missing.Sym);
        }
        return (ir.OXXX, why);

    }
    if (isptrto(_addr_dst, types.TINTER)) {
        why = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", dst);
        return (ir.OXXX, why);
    }
    if (src.IsInterface() && dst.Kind() != types.TBLANK) {
        missing = ;        have = ;

        ptr = default;
        why = default;
        if (implements(_addr_dst, _addr_src, _addr_missing, _addr_have, _addr_ptr)) {
            why = ": need type assertion";
        }
        return (ir.OXXX, why);

    }
    if (src.IsChan() && src.ChanDir() == types.Cboth && dst.IsChan()) {
        if (types.Identical(src.Elem(), dst.Elem()) && (src.Sym() == null || dst.Sym() == null)) {
            return (ir.OCONVNOP, "");
        }
    }
    if (src.Kind() == types.TNIL) {

        if (dst.Kind() == types.TPTR || dst.Kind() == types.TFUNC || dst.Kind() == types.TMAP || dst.Kind() == types.TCHAN || dst.Kind() == types.TINTER || dst.Kind() == types.TSLICE) 
            return (ir.OCONVNOP, "");
        
    }
    if (dst.Kind() == types.TBLANK) {
        return (ir.OCONVNOP, "");
    }
    return (ir.OXXX, "");

}

// Can we convert a value of type src to a value of type dst?
// If so, return op code to use in conversion (maybe OCONVNOP).
// If not, return OXXX. In this case, the string return parameter may
// hold a reason why. In all other cases, it'll be the empty string.
// srcConstant indicates whether the value of type src is a constant.
public static (ir.Op, @string) Convertop(bool srcConstant, ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst) {
    ir.Op _p0 = default;
    @string _p0 = default;
    ref types.Type src = ref _addr_src.val;
    ref types.Type dst = ref _addr_dst.val;

    if (src == dst) {
        return (ir.OCONVNOP, "");
    }
    if (src == null || dst == null) {
        return (ir.OXXX, "");
    }
    if (src.IsPtr() && dst.IsPtr() && dst.Elem().NotInHeap() && !src.Elem().NotInHeap()) {
        var why = fmt.Sprintf(":\n\t%v is incomplete (or unallocatable), but %v is not", dst.Elem(), src.Elem());
        return (ir.OXXX, why);
    }
    if (src.IsString() && dst.IsSlice() && dst.Elem().NotInHeap() && (dst.Elem().Kind() == types.ByteType.Kind() || dst.Elem().Kind() == types.RuneType.Kind())) {
        why = fmt.Sprintf(":\n\t%v is incomplete (or unallocatable)", dst.Elem());
        return (ir.OXXX, why);
    }
    var (op, why) = Assignop(_addr_src, _addr_dst);
    if (op != ir.OXXX) {
        return (op, why);
    }
    if (src.IsInterface() || dst.IsInterface()) {
        return (ir.OXXX, why);
    }
    if (types.IdenticalIgnoreTags(src.Underlying(), dst.Underlying())) {
        return (ir.OCONVNOP, "");
    }
    if (src.IsPtr() && dst.IsPtr() && src.Sym() == null && dst.Sym() == null) {
        if (types.IdenticalIgnoreTags(src.Elem().Underlying(), dst.Elem().Underlying())) {
            return (ir.OCONVNOP, "");
        }
    }
    if ((src.IsInteger() || src.IsFloat()) && (dst.IsInteger() || dst.IsFloat())) {
        if (types.SimType[src.Kind()] == types.SimType[dst.Kind()]) {
            return (ir.OCONVNOP, "");
        }
        return (ir.OCONV, "");

    }
    if (src.IsComplex() && dst.IsComplex()) {
        if (types.SimType[src.Kind()] == types.SimType[dst.Kind()]) {
            return (ir.OCONVNOP, "");
        }
        return (ir.OCONV, "");

    }
    if (srcConstant && (src.IsInteger() || src.IsFloat() || src.IsComplex()) && (dst.IsInteger() || dst.IsFloat() || dst.IsComplex())) {
        return (ir.OCONV, "");
    }
    if (src.IsInteger() && dst.IsString()) {
        return (ir.ORUNESTR, "");
    }
    if (src.IsSlice() && dst.IsString()) {
        if (src.Elem().Kind() == types.ByteType.Kind()) {
            return (ir.OBYTES2STR, "");
        }
        if (src.Elem().Kind() == types.RuneType.Kind()) {
            return (ir.ORUNES2STR, "");
        }
    }
    if (src.IsString() && dst.IsSlice()) {
        if (dst.Elem().Kind() == types.ByteType.Kind()) {
            return (ir.OSTR2BYTES, "");
        }
        if (dst.Elem().Kind() == types.RuneType.Kind()) {
            return (ir.OSTR2RUNES, "");
        }
    }
    if ((src.IsPtr() || src.IsUintptr()) && dst.IsUnsafePtr()) {
        return (ir.OCONVNOP, "");
    }
    if (src.IsUnsafePtr() && (dst.IsPtr() || dst.IsUintptr())) {
        return (ir.OCONVNOP, "");
    }
    if (src.Kind() == types.TMAP && dst.IsPtr() && src.MapType().Hmap == dst.Elem()) {
        return (ir.OCONVNOP, "");
    }
    if (src.IsSlice() && dst.IsPtr() && dst.Elem().IsArray() && types.Identical(src.Elem(), dst.Elem().Elem())) {
        if (!types.AllowsGoVersion(curpkg(), 1, 17)) {
            return (ir.OXXX, ":\n\tconversion of slices to array pointers only supported as of -lang=go1.17");
        }
        return (ir.OSLICE2ARRPTR, "");

    }
    return (ir.OXXX, "");

}

// Code to resolve elided DOTs in embedded types.

// A dlist stores a pointer to a TFIELD Type embedded within
// a TSTRUCT or TINTER Type.
private partial struct dlist {
    public ptr<types.Field> field;
}

// dotpath computes the unique shortest explicit selector path to fully qualify
// a selection expression x.f, where x is of type t and f is the symbol s.
// If no such path exists, dotpath returns nil.
// If there are multiple shortest paths to the same depth, ambig is true.
private static (slice<dlist>, bool) dotpath(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<ptr<types.Field>> _addr_save, bool ignorecase) {
    slice<dlist> path = default;
    bool ambig = default;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ptr<types.Field> save = ref _addr_save.val;
 
    // The embedding of types within structs imposes a tree structure onto
    // types: structs parent the types they embed, and types parent their
    // fields or methods. Our goal here is to find the shortest path to
    // a field or method named s in the subtree rooted at t. To accomplish
    // that, we iteratively perform depth-first searches of increasing depth
    // until we either find the named field/method or exhaust the tree.
    for (nint d = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; d++) {
        if (d > len(dotlist)) {
            dotlist = append(dotlist, new dlist());
        }
        {
            var (c, more) = adddot1(_addr_s, _addr_t, d, _addr_save, ignorecase);

            if (c == 1) {
                return (dotlist[..(int)d], false);
            }
            else if (c > 1) {
                return (null, true);
            }
            else if (!more) {
                return (null, false);
            }


        }

    }

}

private static void expand0(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var u = t;
    if (u.IsPtr()) {
        u = u.Elem();
    }
    if (u.IsInterface()) {
        {
            var f__prev1 = f;

            foreach (var (_, __f) in u.AllMethods().Slice()) {
                f = __f;
                if (f.Sym.Uniq()) {
                    continue;
                }
                f.Sym.SetUniq(true);
                slist = append(slist, new symlink(field:f));
            }

            f = f__prev1;
        }

        return ;

    }
    u = types.ReceiverBaseType(t);
    if (u != null) {
        {
            var f__prev1 = f;

            foreach (var (_, __f) in u.Methods().Slice()) {
                f = __f;
                if (f.Sym.Uniq()) {
                    continue;
                }
                f.Sym.SetUniq(true);
                slist = append(slist, new symlink(field:f));
            }

            f = f__prev1;
        }
    }
}

private static void expand1(ptr<types.Type> _addr_t, bool top) {
    ref types.Type t = ref _addr_t.val;

    if (t.Recur()) {
        return ;
    }
    t.SetRecur(true);

    if (!top) {
        expand0(_addr_t);
    }
    var u = t;
    if (u.IsPtr()) {
        u = u.Elem();
    }
    if (u.IsStruct() || u.IsInterface()) {
        ptr<types.Fields> fields;
        if (u.IsStruct()) {
            fields = u.Fields();
        }
        else
 {
            fields = u.AllMethods();
        }
        foreach (var (_, f) in fields.Slice()) {
            if (f.Embedded == 0) {
                continue;
            }
            if (f.Sym == null) {
                continue;
            }
            expand1(_addr_f.Type, false);
        }
    }
    t.SetRecur(false);

}

private static (ptr<types.Field>, bool) ifacelookdot(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, bool ignorecase) {
    ptr<types.Field> m = default!;
    bool followptr = default;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    if (t == null) {
        return (_addr_null!, false);
    }
    var (path, ambig) = dotpath(_addr_s, _addr_t, _addr_m, ignorecase);
    if (path == null) {
        if (ambig) {
            @base.Errorf("%v.%v is ambiguous", t, s);
        }
        return (_addr_null!, false);

    }
    foreach (var (_, d) in path) {
        if (d.field.Type.IsPtr()) {
            followptr = true;
            break;
        }
    }    if (!m.IsMethod()) {
        @base.Errorf("%v.%v is a field, not a method", t, s);
        return (_addr_null!, followptr);
    }
    return (_addr_m!, followptr);

}

private static bool implements(ptr<types.Type> _addr_t, ptr<types.Type> _addr_iface, ptr<ptr<types.Field>> _addr_m, ptr<ptr<types.Field>> _addr_samename, ptr<nint> _addr_ptr) {
    ref types.Type t = ref _addr_t.val;
    ref types.Type iface = ref _addr_iface.val;
    ref ptr<types.Field> m = ref _addr_m.val;
    ref ptr<types.Field> samename = ref _addr_samename.val;
    ref nint ptr = ref _addr_ptr.val;

    var t0 = t;
    if (t == null) {
        return false;
    }
    if (t.IsInterface()) {
        nint i = 0;
        var tms = t.AllMethods().Slice();
        {
            var im__prev1 = im;

            foreach (var (_, __im) in iface.AllMethods().Slice()) {
                im = __im;
                while (i < len(tms) && tms[i].Sym != im.Sym) {
                    i++;
                }

                if (i == len(tms)) {
                    m.val = im;
                    samename.val = null;
                    ptr = 0;
                    return false;
                }
                var tm = tms[i];
                if (!types.Identical(tm.Type, im.Type)) {
                    m.val = im;
                    samename.val = tm;
                    ptr = 0;
                    return false;
                }
            }

            im = im__prev1;
        }

        return true;

    }
    t = types.ReceiverBaseType(t);
    tms = default;
    if (t != null) {
        CalcMethods(_addr_t);
        tms = t.AllMethods().Slice();
    }
    i = 0;
    {
        var im__prev1 = im;

        foreach (var (_, __im) in iface.AllMethods().Slice()) {
            im = __im;
            if (im.Broke()) {
                continue;
            }
            while (i < len(tms) && tms[i].Sym != im.Sym) {
                i++;
            }

            if (i == len(tms)) {
                m.val = im;
                samename.val, _ = ifacelookdot(_addr_im.Sym, _addr_t, true);
                ptr = 0;
                return false;
            }
            tm = tms[i];
            if (tm.Nointerface() || !types.Identical(tm.Type, im.Type)) {
                m.val = im;
                samename.val = tm;
                ptr = 0;
                return false;
            }
            var followptr = tm.Embedded == 2; 

            // if pointer receiver in method,
            // the method does not exist for value types.
            var rcvr = tm.Type.Recv().Type;
            if (rcvr.IsPtr() && !t0.IsPtr() && !followptr && !types.IsInterfaceMethod(tm.Type)) {
                if (false && @base.Flag.LowerR != 0) {
                    @base.Errorf("interface pointer mismatch");
                }
                m.val = im;
                samename.val = null;
                ptr = 1;
                return false;
            }

        }
        im = im__prev1;
    }

    return true;

}

private static bool isptrto(ptr<types.Type> _addr_t, types.Kind et) {
    ref types.Type t = ref _addr_t.val;

    if (t == null) {
        return false;
    }
    if (!t.IsPtr()) {
        return false;
    }
    t = t.Elem();
    if (t == null) {
        return false;
    }
    if (t.Kind() != et) {
        return false;
    }
    return true;

}

// lookdot0 returns the number of fields or methods named s associated
// with Type t. If exactly one exists, it will be returned in *save
// (if save is not nil).
private static nint lookdot0(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<ptr<types.Field>> _addr_save, bool ignorecase) {
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ptr<types.Field> save = ref _addr_save.val;

    var u = t;
    if (u.IsPtr()) {
        u = u.Elem();
    }
    nint c = 0;
    if (u.IsStruct() || u.IsInterface()) {
        ptr<types.Fields> fields;
        if (u.IsStruct()) {
            fields = u.Fields();
        }
        else
 {
            fields = u.AllMethods();
        }
        {
            var f__prev1 = f;

            foreach (var (_, __f) in fields.Slice()) {
                f = __f;
                if (f.Sym == s || (ignorecase && f.IsMethod() && strings.EqualFold(f.Sym.Name, s.Name))) {
                    if (save != null) {
                        save.val = f;
                    }
                    c++;
                }
            }

            f = f__prev1;
        }
    }
    u = t;
    if (t.Sym() != null && t.IsPtr() && !t.Elem().IsPtr()) { 
        // If t is a defined pointer type, then x.m is shorthand for (*x).m.
        u = t.Elem();

    }
    u = types.ReceiverBaseType(u);
    if (u != null) {
        {
            var f__prev1 = f;

            foreach (var (_, __f) in u.Methods().Slice()) {
                f = __f;
                if (f.Embedded == 0 && (f.Sym == s || (ignorecase && strings.EqualFold(f.Sym.Name, s.Name)))) {
                    if (save != null) {
                        save.val = f;
                    }
                    c++;
                }
            }

            f = f__prev1;
        }
    }
    return c;

}

private static slice<symlink> slist = default;

// Code to help generate trampoline functions for methods on embedded
// types. These are approx the same as the corresponding AddImplicitDots
// routines except that they expect to be called with unique tasks and
// they return the actual methods.

private partial struct symlink {
    public ptr<types.Field> field;
}

} // end typecheck_package
