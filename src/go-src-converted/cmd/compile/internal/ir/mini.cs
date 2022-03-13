// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run -mod=mod mknode.go

// package ir -- go2cs converted at 2022 March 13 06:00:29 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\mini.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using constant = go.constant_package;


// A miniNode is a minimal node implementation,
// meant to be embedded as the first field in a larger node implementation,
// at a cost of 8 bytes.
//
// A miniNode is NOT a valid Node by itself: the embedding struct
// must at the least provide:
//
//    func (n *MyNode) String() string { return fmt.Sprint(n) }
//    func (n *MyNode) rawCopy() Node { c := *n; return &c }
//    func (n *MyNode) Format(s fmt.State, verb rune) { FmtNode(n, s, verb) }
//
// The embedding struct should also fill in n.op in its constructor,
// for more useful panic messages when invalid methods are called,
// instead of implementing Op itself.
//

public static partial class ir_package {

private partial struct miniNode {
    public src.XPos pos; // uint32
    public Op op; // uint8
    public bitset8 bits;
    public ushort esc;
}

// posOr returns pos if known, or else n.pos.
// For use in DeepCopy.
private static src.XPos posOr(this ptr<miniNode> _addr_n, src.XPos pos) {
    ref miniNode n = ref _addr_n.val;

    if (pos.IsKnown()) {
        return pos;
    }
    return n.pos;
}

// op can be read, but not written.
// An embedding implementation can provide a SetOp if desired.
// (The panicking SetOp is with the other panics below.)
private static Op Op(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.op;
}
private static src.XPos Pos(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.pos;
}
private static void SetPos(this ptr<miniNode> _addr_n, src.XPos x) {
    ref miniNode n = ref _addr_n.val;

    n.pos = x;
}
private static ushort Esc(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.esc;
}
private static void SetEsc(this ptr<miniNode> _addr_n, ushort x) {
    ref miniNode n = ref _addr_n.val;

    n.esc = x;
}

private static readonly nint miniWalkdefShift = 0; // TODO(mdempsky): Move to Name.flags.
private static readonly nint miniTypecheckShift = 2;
private static readonly nint miniDiag = 1 << 4;
private static readonly nint miniWalked = 1 << 5; // to prevent/catch re-walking

private static byte Typecheck(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.bits.get2(miniTypecheckShift);
}
private static void SetTypecheck(this ptr<miniNode> _addr_n, byte x) => func((_, panic, _) => {
    ref miniNode n = ref _addr_n.val;

    if (x > 3) {
        panic(fmt.Sprintf("cannot SetTypecheck %d", x));
    }
    n.bits.set2(miniTypecheckShift, x);
});

private static bool Diag(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.bits & miniDiag != 0;
}
private static void SetDiag(this ptr<miniNode> _addr_n, bool x) {
    ref miniNode n = ref _addr_n.val;

    n.bits.set(miniDiag, x);
}

private static bool Walked(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return n.bits & miniWalked != 0;
}
private static void SetWalked(this ptr<miniNode> _addr_n, bool x) {
    ref miniNode n = ref _addr_n.val;

    n.bits.set(miniWalked, x);
}

// Empty, immutable graph structure.

private static Nodes Init(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return new Nodes();
}

// Additional functionality unavailable.

private static @string no(this ptr<miniNode> _addr_n, @string name) {
    ref miniNode n = ref _addr_n.val;

    return "cannot " + name + " on " + n.op.String();
}

private static ptr<types.Type> Type(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return _addr_null!;
}
private static void SetType(this ptr<miniNode> _addr_n, ptr<types.Type> _addr__p0) => func((_, panic, _) => {
    ref miniNode n = ref _addr_n.val;
    ref types.Type _p0 = ref _addr__p0.val;

    panic(n.no("SetType"));
});
private static ptr<Name> Name(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return _addr_null!;
}
private static ptr<types.Sym> Sym(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return _addr_null!;
}
private static constant.Value Val(this ptr<miniNode> _addr_n) => func((_, panic, _) => {
    ref miniNode n = ref _addr_n.val;

    panic(n.no("Val"));
});
private static void SetVal(this ptr<miniNode> _addr_n, constant.Value v) => func((_, panic, _) => {
    ref miniNode n = ref _addr_n.val;

    panic(n.no("SetVal"));
});
private static bool NonNil(this ptr<miniNode> _addr_n) {
    ref miniNode n = ref _addr_n.val;

    return false;
}
private static void MarkNonNil(this ptr<miniNode> _addr_n) => func((_, panic, _) => {
    ref miniNode n = ref _addr_n.val;

    panic(n.no("MarkNonNil"));
});

} // end ir_package
