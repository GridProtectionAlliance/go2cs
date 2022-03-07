// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Selections.

// package types2 -- go2cs converted at 2022 March 06 23:12:51 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\selection.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;

namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // SelectionKind describes the kind of a selector expression x.f
    // (excluding qualified identifiers).
public partial struct SelectionKind { // : nint
}

public static readonly SelectionKind FieldVal = iota; // x.f is a struct field selector
public static readonly var MethodVal = 0; // x.f is a method selector
public static readonly var MethodExpr = 1; // x.f is a method expression

// A Selection describes a selector expression x.f.
// For the declarations:
//
//    type T struct{ x int; E }
//    type E struct{}
//    func (e E) m() {}
//    var p *T
//
// the following relations exist:
//
//    Selector    Kind          Recv    Obj    Type       Index     Indirect
//
//    p.x         FieldVal      T       x      int        {0}       true
//    p.m         MethodVal     *T      m      func()     {1, 0}    true
//    T.m         MethodExpr    T       m      func(T)    {1, 0}    false
//
public partial struct Selection {
    public SelectionKind kind;
    public Type recv; // type of x
    public Object obj; // object denoted by x.f
    public slice<nint> index; // path from x to x.f
    public bool indirect; // set if there was any pointer indirection on the path
}

// Kind returns the selection kind.
private static SelectionKind Kind(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return s.kind;
}

// Recv returns the type of x in x.f.
private static Type Recv(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return s.recv;
}

// Obj returns the object denoted by x.f; a *Var for
// a field selection, and a *Func in all other cases.
private static Object Obj(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return s.obj;
}

// Type returns the type of x.f, which may be different from the type of f.
// See Selection for more information.
private static Type Type(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;


    if (s.kind == MethodVal) 
        // The type of x.f is a method with its receiver type set
        // to the type of x.
        ptr<Signature> sig = s.obj._<ptr<Func>>().typ._<ptr<Signature>>().val;
        ref var recv = ref heap(sig.recv.val, out ptr<var> _addr_recv);
        recv.typ = s.recv;
        _addr_sig.recv = _addr_recv;
        sig.recv = ref _addr_sig.recv.val;
        return _addr_sig;
    else if (s.kind == MethodExpr) 
        // The type of x.f is a function (without receiver)
        // and an additional first argument with the same type as x.
        // TODO(gri) Similar code is already in call.go - factor!
        // TODO(gri) Compute this eagerly to avoid allocations.
        sig = s.obj._<ptr<Func>>().typ._<ptr<Signature>>().val;
        ref var arg0 = ref heap(sig.recv.val, out ptr<var> _addr_arg0);
        sig.recv = null;
        arg0.typ = s.recv;
        slice<ptr<Var>> @params = default;
        if (sig.@params != null) {
            params = sig.@params.vars;
        }
        sig.@params = NewTuple(append(new slice<ptr<Var>>(new ptr<Var>[] { &arg0 }), params));
        return _addr_sig;
    // In all other cases, the type of x.f is the type of x.
    return s.obj.Type();

}

// Index describes the path from x to f in x.f.
// The last index entry is the field or method index of the type declaring f;
// either:
//
//    1) the list of declared methods of a named type; or
//    2) the list of methods of an interface type; or
//    3) the list of fields of a struct type.
//
// The earlier index entries are the indices of the embedded fields implicitly
// traversed to get from (the type of) x to f, starting at embedding depth 0.
private static slice<nint> Index(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return s.index;
}

// Indirect reports whether any pointer indirection was required to get from
// x to f in x.f.
private static bool Indirect(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return s.indirect;
}

private static @string String(this ptr<Selection> _addr_s) {
    ref Selection s = ref _addr_s.val;

    return SelectionString(_addr_s, null);
}

// SelectionString returns the string form of s.
// The Qualifier controls the printing of
// package-level objects, and may be nil.
//
// Examples:
//    "field (T) f int"
//    "method (T) f(X) Y"
//    "method expr (T) f(X) Y"
//
public static @string SelectionString(ptr<Selection> _addr_s, Qualifier qf) {
    ref Selection s = ref _addr_s.val;

    @string k = default;

    if (s.kind == FieldVal) 
        k = "field ";
    else if (s.kind == MethodVal) 
        k = "method ";
    else if (s.kind == MethodExpr) 
        k = "method expr ";
    else 
        unreachable();
        ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    buf.WriteString(k);
    buf.WriteByte('(');
    WriteType(_addr_buf, s.Recv(), qf);
    fmt.Fprintf(_addr_buf, ") %s", s.obj.Name());
    {
        var T = s.Type();

        if (s.kind == FieldVal) {
            buf.WriteByte(' ');
            WriteType(_addr_buf, T, qf);
        }
        else
 {
            WriteSignature(_addr_buf, T._<ptr<Signature>>(), qf);
        }
    }

    return buf.String();

}

} // end types2_package
