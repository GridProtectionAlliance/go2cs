// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:49:01 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\expr.go
using bytes = go.bytes_package;
using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using System.ComponentModel;
using System;


namespace go.cmd.compile.@internal;

public static partial class ir_package {

    // An Expr is a Node that can appear as an expression.
public partial interface Expr {
    void isExpr();
}

// A miniExpr is a miniNode with extra fields common to expressions.
// TODO(rsc): Once we are sure about the contents, compact the bools
// into a bit field and leave extra bits available for implementations
// embedding miniExpr. Right now there are ~60 unused bits sitting here.
private partial struct miniExpr {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<types.Type> typ;
    public Nodes init; // TODO(rsc): Don't require every Node to have an init
    public bitset8 flags;
}

private static readonly nint miniExprNonNil = 1 << (int)(iota);
private static readonly var miniExprTransient = 0;
private static readonly var miniExprBounded = 1;
private static readonly var miniExprImplicit = 2; // for use by implementations; not supported by every Expr
private static readonly var miniExprCheckPtr = 3;


private static void isExpr(this ptr<miniExpr> _addr__p0) {
    ref miniExpr _p0 = ref _addr__p0.val;

}

private static ptr<types.Type> Type(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return _addr_n.typ!;
}
private static void SetType(this ptr<miniExpr> _addr_n, ptr<types.Type> _addr_x) {
    ref miniExpr n = ref _addr_n.val;
    ref types.Type x = ref _addr_x.val;

    n.typ = x;
}
private static bool NonNil(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return n.flags & miniExprNonNil != 0;
}
private static void MarkNonNil(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    n.flags |= miniExprNonNil;
}
private static bool Transient(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return n.flags & miniExprTransient != 0;
}
private static void SetTransient(this ptr<miniExpr> _addr_n, bool b) {
    ref miniExpr n = ref _addr_n.val;

    n.flags.set(miniExprTransient, b);
}
private static bool Bounded(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return n.flags & miniExprBounded != 0;
}
private static void SetBounded(this ptr<miniExpr> _addr_n, bool b) {
    ref miniExpr n = ref _addr_n.val;

    n.flags.set(miniExprBounded, b);
}
private static Nodes Init(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return n.init;
}
private static ptr<Nodes> PtrInit(this ptr<miniExpr> _addr_n) {
    ref miniExpr n = ref _addr_n.val;

    return _addr__addr_n.init!;
}
private static void SetInit(this ptr<miniExpr> _addr_n, Nodes x) {
    ref miniExpr n = ref _addr_n.val;

    n.init = x;
}

// An AddStringExpr is a string concatenation Expr[0] + Exprs[1] + ... + Expr[len(Expr)-1].
public partial struct AddStringExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Nodes List;
    public ptr<Name> Prealloc;
}

public static ptr<AddStringExpr> NewAddStringExpr(src.XPos pos, slice<Node> list) {
    ptr<AddStringExpr> n = addr(new AddStringExpr());
    n.pos = pos;
    n.op = OADDSTR;
    n.List = list;
    return _addr_n!;
}

// An AddrExpr is an address-of expression &X.
// It may end up being a normal address-of or an allocation of a composite literal.
public partial struct AddrExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public ptr<Name> Prealloc; // preallocated storage if any
}

public static ptr<AddrExpr> NewAddrExpr(src.XPos pos, Node x) {
    ptr<AddrExpr> n = addr(new AddrExpr(X:x));
    n.op = OADDR;
    n.pos = pos;
    return _addr_n!;
}

private static bool Implicit(this ptr<AddrExpr> _addr_n) {
    ref AddrExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<AddrExpr> _addr_n, bool b) {
    ref AddrExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}

private static void SetOp(this ptr<AddrExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref AddrExpr n = ref _addr_n.val;


    if (op == OADDR || op == OPTRLIT) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A BasicLit is a literal of basic type.
public partial struct BasicLit {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public constant.Value val;
}

public static Node NewBasicLit(src.XPos pos, constant.Value val) {
    ptr<BasicLit> n = addr(new BasicLit(val:val));
    n.op = OLITERAL;
    n.pos = pos;
    {
        var k = val.Kind();

        if (k != constant.Unknown) {
            n.SetType(idealType(k));
        }
    }

    return n;

}

private static constant.Value Val(this ptr<BasicLit> _addr_n) {
    ref BasicLit n = ref _addr_n.val;

    return n.val;
}
private static void SetVal(this ptr<BasicLit> _addr_n, constant.Value val) {
    ref BasicLit n = ref _addr_n.val;

    n.val = val;
}

// A BinaryExpr is a binary expression X Op Y,
// or Op(X, Y) for builtin functions that do not become calls.
public partial struct BinaryExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public Node Y;
}

public static ptr<BinaryExpr> NewBinaryExpr(src.XPos pos, Op op, Node x, Node y) {
    ptr<BinaryExpr> n = addr(new BinaryExpr(X:x,Y:y));
    n.pos = pos;
    n.SetOp(op);
    return _addr_n!;
}

private static void SetOp(this ptr<BinaryExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref BinaryExpr n = ref _addr_n.val;


    if (op == OADD || op == OADDSTR || op == OAND || op == OANDNOT || op == ODIV || op == OEQ || op == OGE || op == OGT || op == OLE || op == OLSH || op == OLT || op == OMOD || op == OMUL || op == ONE || op == OOR || op == ORSH || op == OSUB || op == OXOR || op == OCOPY || op == OCOMPLEX || op == OUNSAFEADD || op == OUNSAFESLICE || op == OEFACE) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A CallUse records how the result of the call is used:
public partial struct CallUse { // : byte
}

private static readonly CallUse _ = iota;

public static readonly var CallUseExpr = 0; // single expression result is used
public static readonly var CallUseList = 1; // list of results are used
public static readonly var CallUseStmt = 2; // results not used - call is a statement

// A CallExpr is a function call X(Args).
public partial struct CallExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ref origNode origNode => ref origNode_val;
    public Node X;
    public Nodes Args;
    public slice<ptr<Name>> KeepAlive; // vars to be kept alive until call returns
    public bool IsDDD;
    public CallUse Use;
    public bool NoInline;
    public bool PreserveClosure; // disable directClosureCall for this call
}

public static ptr<CallExpr> NewCallExpr(src.XPos pos, Op op, Node fun, slice<Node> args) {
    ptr<CallExpr> n = addr(new CallExpr(X:fun));
    n.pos = pos;
    n.orig = n;
    n.SetOp(op);
    n.Args = args;
    return _addr_n!;
}

private static void isStmt(this ptr<CallExpr> _addr__p0) {
    ref CallExpr _p0 = ref _addr__p0.val;

}

private static void SetOp(this ptr<CallExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref CallExpr n = ref _addr_n.val;


    if (op == OCALL || op == OCALLFUNC || op == OCALLINTER || op == OCALLMETH || op == OAPPEND || op == ODELETE || op == OGETG || op == OMAKE || op == OPRINT || op == OPRINTN || op == ORECOVER) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A ClosureExpr is a function literal expression.
public partial struct ClosureExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    [Description("mknode:\"-\"")]
    public ptr<Func> Func;
    public ptr<Name> Prealloc;
}

public static ptr<ClosureExpr> NewClosureExpr(src.XPos pos, ptr<Func> _addr_fn) {
    ref Func fn = ref _addr_fn.val;

    ptr<ClosureExpr> n = addr(new ClosureExpr(Func:fn));
    n.op = OCLOSURE;
    n.pos = pos;
    return _addr_n!;
}

// A CompLitExpr is a composite literal Type{Vals}.
// Before type-checking, the type is Ntype.
public partial struct CompLitExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ref origNode origNode => ref origNode_val;
    public Ntype Ntype;
    public Nodes List; // initialized values
    public ptr<Name> Prealloc;
    public long Len; // backing array length for OSLICELIT
}

public static ptr<CompLitExpr> NewCompLitExpr(src.XPos pos, Op op, Ntype typ, slice<Node> list) {
    ptr<CompLitExpr> n = addr(new CompLitExpr(Ntype:typ));
    n.pos = pos;
    n.SetOp(op);
    n.List = list;
    n.orig = n;
    return _addr_n!;
}

private static bool Implicit(this ptr<CompLitExpr> _addr_n) {
    ref CompLitExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<CompLitExpr> _addr_n, bool b) {
    ref CompLitExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}

private static void SetOp(this ptr<CompLitExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref CompLitExpr n = ref _addr_n.val;


    if (op == OARRAYLIT || op == OCOMPLIT || op == OMAPLIT || op == OSTRUCTLIT || op == OSLICELIT) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

public partial struct ConstExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ref origNode origNode => ref origNode_val;
    public constant.Value val;
}

public static Node NewConstExpr(constant.Value val, Node orig) {
    ptr<ConstExpr> n = addr(new ConstExpr(val:val));
    n.op = OLITERAL;
    n.pos = orig.Pos();
    n.orig = orig;
    n.SetType(orig.Type());
    n.SetTypecheck(orig.Typecheck());
    n.SetDiag(orig.Diag());
    return n;
}

private static ptr<types.Sym> Sym(this ptr<ConstExpr> _addr_n) {
    ref ConstExpr n = ref _addr_n.val;

    return _addr_n.orig.Sym()!;
}
private static constant.Value Val(this ptr<ConstExpr> _addr_n) {
    ref ConstExpr n = ref _addr_n.val;

    return n.val;
}

// A ConvExpr is a conversion Type(X).
// It may end up being a value or a type.
public partial struct ConvExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
}

public static ptr<ConvExpr> NewConvExpr(src.XPos pos, Op op, ptr<types.Type> _addr_typ, Node x) {
    ref types.Type typ = ref _addr_typ.val;

    ptr<ConvExpr> n = addr(new ConvExpr(X:x));
    n.pos = pos;
    n.typ = typ;
    n.SetOp(op);
    return _addr_n!;
}

private static bool Implicit(this ptr<ConvExpr> _addr_n) {
    ref ConvExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<ConvExpr> _addr_n, bool b) {
    ref ConvExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}
private static bool CheckPtr(this ptr<ConvExpr> _addr_n) {
    ref ConvExpr n = ref _addr_n.val;

    return n.flags & miniExprCheckPtr != 0;
}
private static void SetCheckPtr(this ptr<ConvExpr> _addr_n, bool b) {
    ref ConvExpr n = ref _addr_n.val;

    n.flags.set(miniExprCheckPtr, b);
}

private static void SetOp(this ptr<ConvExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref ConvExpr n = ref _addr_n.val;


    if (op == OCONV || op == OCONVIFACE || op == OCONVNOP || op == OBYTES2STR || op == OBYTES2STRTMP || op == ORUNES2STR || op == OSTR2BYTES || op == OSTR2BYTESTMP || op == OSTR2RUNES || op == ORUNESTR || op == OSLICE2ARRPTR) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// An IndexExpr is an index expression X[Y].
public partial struct IndexExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public Node Index;
    public bool Assigned;
}

public static ptr<IndexExpr> NewIndexExpr(src.XPos pos, Node x, Node index) {
    ptr<IndexExpr> n = addr(new IndexExpr(X:x,Index:index));
    n.pos = pos;
    n.op = OINDEX;
    return _addr_n!;
}

private static void SetOp(this ptr<IndexExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref IndexExpr n = ref _addr_n.val;


    if (op == OINDEX || op == OINDEXMAP) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A KeyExpr is a Key: Value composite literal key.
public partial struct KeyExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node Key;
    public Node Value;
}

public static ptr<KeyExpr> NewKeyExpr(src.XPos pos, Node key, Node value) {
    ptr<KeyExpr> n = addr(new KeyExpr(Key:key,Value:value));
    n.pos = pos;
    n.op = OKEY;
    return _addr_n!;
}

// A StructKeyExpr is an Field: Value composite literal key.
public partial struct StructKeyExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ptr<types.Sym> Field;
    public Node Value;
    public long Offset;
}

public static ptr<StructKeyExpr> NewStructKeyExpr(src.XPos pos, ptr<types.Sym> _addr_field, Node value) {
    ref types.Sym field = ref _addr_field.val;

    ptr<StructKeyExpr> n = addr(new StructKeyExpr(Field:field,Value:value));
    n.pos = pos;
    n.op = OSTRUCTKEY;
    n.Offset = types.BADWIDTH;
    return _addr_n!;
}

private static ptr<types.Sym> Sym(this ptr<StructKeyExpr> _addr_n) {
    ref StructKeyExpr n = ref _addr_n.val;

    return _addr_n.Field!;
}

// An InlinedCallExpr is an inlined function call.
public partial struct InlinedCallExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Nodes Body;
    public Nodes ReturnVars;
}

public static ptr<InlinedCallExpr> NewInlinedCallExpr(src.XPos pos, slice<Node> body, slice<Node> retvars) {
    ptr<InlinedCallExpr> n = addr(new InlinedCallExpr());
    n.pos = pos;
    n.op = OINLCALL;
    n.Body = body;
    n.ReturnVars = retvars;
    return _addr_n!;
}

// A LogicalExpr is a expression X Op Y where Op is && or ||.
// It is separate from BinaryExpr to make room for statements
// that must be executed before Y but after X.
public partial struct LogicalExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public Node Y;
}

public static ptr<LogicalExpr> NewLogicalExpr(src.XPos pos, Op op, Node x, Node y) {
    ptr<LogicalExpr> n = addr(new LogicalExpr(X:x,Y:y));
    n.pos = pos;
    n.SetOp(op);
    return _addr_n!;
}

private static void SetOp(this ptr<LogicalExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref LogicalExpr n = ref _addr_n.val;


    if (op == OANDAND || op == OOROR) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A MakeExpr is a make expression: make(Type[, Len[, Cap]]).
// Op is OMAKECHAN, OMAKEMAP, OMAKESLICE, or OMAKESLICECOPY,
// but *not* OMAKE (that's a pre-typechecking CallExpr).
public partial struct MakeExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node Len;
    public Node Cap;
}

public static ptr<MakeExpr> NewMakeExpr(src.XPos pos, Op op, Node len, Node cap) {
    ptr<MakeExpr> n = addr(new MakeExpr(Len:len,Cap:cap));
    n.pos = pos;
    n.SetOp(op);
    return _addr_n!;
}

private static void SetOp(this ptr<MakeExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref MakeExpr n = ref _addr_n.val;


    if (op == OMAKECHAN || op == OMAKEMAP || op == OMAKESLICE || op == OMAKESLICECOPY) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A NilExpr represents the predefined untyped constant nil.
// (It may be copied and assigned a type, though.)
public partial struct NilExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ptr<types.Sym> Sym_; // TODO: Remove
}

public static ptr<NilExpr> NewNilExpr(src.XPos pos) {
    ptr<NilExpr> n = addr(new NilExpr());
    n.pos = pos;
    n.op = ONIL;
    return _addr_n!;
}

private static ptr<types.Sym> Sym(this ptr<NilExpr> _addr_n) {
    ref NilExpr n = ref _addr_n.val;

    return _addr_n.Sym_!;
}
private static void SetSym(this ptr<NilExpr> _addr_n, ptr<types.Sym> _addr_x) {
    ref NilExpr n = ref _addr_n.val;
    ref types.Sym x = ref _addr_x.val;

    n.Sym_ = x;
}

// A ParenExpr is a parenthesized expression (X).
// It may end up being a value or a type.
public partial struct ParenExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
}

public static ptr<ParenExpr> NewParenExpr(src.XPos pos, Node x) {
    ptr<ParenExpr> n = addr(new ParenExpr(X:x));
    n.op = OPAREN;
    n.pos = pos;
    return _addr_n!;
}

private static bool Implicit(this ptr<ParenExpr> _addr_n) {
    ref ParenExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<ParenExpr> _addr_n, bool b) {
    ref ParenExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}

private static void CanBeNtype(this ptr<ParenExpr> _addr__p0) {
    ref ParenExpr _p0 = ref _addr__p0.val;

}

// SetOTYPE changes n to be an OTYPE node returning t,
// like all the type nodes in type.go.
private static void SetOTYPE(this ptr<ParenExpr> _addr_n, ptr<types.Type> _addr_t) {
    ref ParenExpr n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.op = OTYPE;
    n.typ = t;
    t.SetNod(n);
}

// A ResultExpr represents a direct access to a result.
public partial struct ResultExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public long Index; // index of the result expr.
}

public static ptr<ResultExpr> NewResultExpr(src.XPos pos, ptr<types.Type> _addr_typ, long index) {
    ref types.Type typ = ref _addr_typ.val;

    ptr<ResultExpr> n = addr(new ResultExpr(Index:index));
    n.pos = pos;
    n.op = ORESULT;
    n.typ = typ;
    return _addr_n!;
}

// A LinksymOffsetExpr refers to an offset within a global variable.
// It is like a SelectorExpr but without the field name.
public partial struct LinksymOffsetExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ptr<obj.LSym> Linksym;
    public long Offset_;
}

public static ptr<LinksymOffsetExpr> NewLinksymOffsetExpr(src.XPos pos, ptr<obj.LSym> _addr_lsym, long offset, ptr<types.Type> _addr_typ) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Type typ = ref _addr_typ.val;

    ptr<LinksymOffsetExpr> n = addr(new LinksymOffsetExpr(Linksym:lsym,Offset_:offset));
    n.typ = typ;
    n.op = OLINKSYMOFFSET;
    return _addr_n!;
}

// NewLinksymExpr is NewLinksymOffsetExpr, but with offset fixed at 0.
public static ptr<LinksymOffsetExpr> NewLinksymExpr(src.XPos pos, ptr<obj.LSym> _addr_lsym, ptr<types.Type> _addr_typ) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Type typ = ref _addr_typ.val;

    return _addr_NewLinksymOffsetExpr(pos, _addr_lsym, 0, _addr_typ)!;
}

// NewNameOffsetExpr is NewLinksymOffsetExpr, but taking a *Name
// representing a global variable instead of an *obj.LSym directly.
public static ptr<LinksymOffsetExpr> NewNameOffsetExpr(src.XPos pos, ptr<Name> _addr_name, long offset, ptr<types.Type> _addr_typ) {
    ref Name name = ref _addr_name.val;
    ref types.Type typ = ref _addr_typ.val;

    if (name == null || IsBlank(name) || !(name.Op() == ONAME && name.Class == PEXTERN)) {
        @base.FatalfAt(pos, "cannot take offset of nil, blank name or non-global variable: %v", name);
    }
    return _addr_NewLinksymOffsetExpr(pos, _addr_name.Linksym(), offset, _addr_typ)!;

}

// A SelectorExpr is a selector expression X.Sel.
public partial struct SelectorExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public ptr<types.Sym> Sel;
    public ptr<types.Field> Selection;
    public ptr<Name> Prealloc; // preallocated storage for OCALLPART, if any
}

public static ptr<SelectorExpr> NewSelectorExpr(src.XPos pos, Op op, Node x, ptr<types.Sym> _addr_sel) {
    ref types.Sym sel = ref _addr_sel.val;

    ptr<SelectorExpr> n = addr(new SelectorExpr(X:x,Sel:sel));
    n.pos = pos;
    n.SetOp(op);
    return _addr_n!;
}

private static void SetOp(this ptr<SelectorExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref SelectorExpr n = ref _addr_n.val;


    if (op == OXDOT || op == ODOT || op == ODOTPTR || op == ODOTMETH || op == ODOTINTER || op == OCALLPART || op == OMETHEXPR) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

private static ptr<types.Sym> Sym(this ptr<SelectorExpr> _addr_n) {
    ref SelectorExpr n = ref _addr_n.val;

    return _addr_n.Sel!;
}
private static bool Implicit(this ptr<SelectorExpr> _addr_n) {
    ref SelectorExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<SelectorExpr> _addr_n, bool b) {
    ref SelectorExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}
private static long Offset(this ptr<SelectorExpr> _addr_n) {
    ref SelectorExpr n = ref _addr_n.val;

    return n.Selection.Offset;
}

private static ptr<Name> FuncName(this ptr<SelectorExpr> _addr_n) => func((_, panic, _) => {
    ref SelectorExpr n = ref _addr_n.val;

    if (n.Op() != OMETHEXPR) {
        panic(n.no("FuncName"));
    }
    var fn = NewNameAt(n.Selection.Pos, MethodSym(_addr_n.X.Type(), _addr_n.Sel));
    fn.Class = PFUNC;
    fn.SetType(n.Type());
    if (n.Selection.Nname != null) { 
        // TODO(austin): Nname is nil for interface method
        // expressions (I.M), so we can't attach a Func to
        // those here. reflectdata.methodWrapper generates the
        // Func.
        fn.Func = n.Selection.Nname._<ptr<Name>>().Func;

    }
    return _addr_fn!;

});

// Before type-checking, bytes.Buffer is a SelectorExpr.
// After type-checking it becomes a Name.
private static void CanBeNtype(this ptr<SelectorExpr> _addr__p0) {
    ref SelectorExpr _p0 = ref _addr__p0.val;

}

// A SliceExpr is a slice expression X[Low:High] or X[Low:High:Max].
public partial struct SliceExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public Node Low;
    public Node High;
    public Node Max;
}

public static ptr<SliceExpr> NewSliceExpr(src.XPos pos, Op op, Node x, Node low, Node high, Node max) {
    ptr<SliceExpr> n = addr(new SliceExpr(X:x,Low:low,High:high,Max:max));
    n.pos = pos;
    n.op = op;
    return _addr_n!;
}

private static void SetOp(this ptr<SliceExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref SliceExpr n = ref _addr_n.val;


    if (op == OSLICE || op == OSLICEARR || op == OSLICESTR || op == OSLICE3 || op == OSLICE3ARR) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// IsSlice3 reports whether o is a slice3 op (OSLICE3, OSLICE3ARR).
// o must be a slicing op.
public static bool IsSlice3(this Op o) {

    if (o == OSLICE || o == OSLICEARR || o == OSLICESTR) 
        return false;
    else if (o == OSLICE3 || o == OSLICE3ARR) 
        return true;
        @base.Fatalf("IsSlice3 op %v", o);
    return false;

}

// A SliceHeader expression constructs a slice header from its parts.
public partial struct SliceHeaderExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node Ptr;
    public Node Len;
    public Node Cap;
}

public static ptr<SliceHeaderExpr> NewSliceHeaderExpr(src.XPos pos, ptr<types.Type> _addr_typ, Node ptr, Node len, Node cap) {
    ref types.Type typ = ref _addr_typ.val;

    ptr<SliceHeaderExpr> n = addr(new SliceHeaderExpr(Ptr:ptr,Len:len,Cap:cap));
    n.pos = pos;
    n.op = OSLICEHEADER;
    n.typ = typ;
    return _addr_n!;
}

// A StarExpr is a dereference expression *X.
// It may end up being a value or a type.
public partial struct StarExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
}

public static ptr<StarExpr> NewStarExpr(src.XPos pos, Node x) {
    ptr<StarExpr> n = addr(new StarExpr(X:x));
    n.op = ODEREF;
    n.pos = pos;
    return _addr_n!;
}

private static bool Implicit(this ptr<StarExpr> _addr_n) {
    ref StarExpr n = ref _addr_n.val;

    return n.flags & miniExprImplicit != 0;
}
private static void SetImplicit(this ptr<StarExpr> _addr_n, bool b) {
    ref StarExpr n = ref _addr_n.val;

    n.flags.set(miniExprImplicit, b);
}

private static void CanBeNtype(this ptr<StarExpr> _addr__p0) {
    ref StarExpr _p0 = ref _addr__p0.val;

}

// SetOTYPE changes n to be an OTYPE node returning t,
// like all the type nodes in type.go.
private static void SetOTYPE(this ptr<StarExpr> _addr_n, ptr<types.Type> _addr_t) {
    ref StarExpr n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.op = OTYPE;
    n.X = null;
    n.typ = t;
    t.SetNod(n);
}

// A TypeAssertionExpr is a selector expression X.(Type).
// Before type-checking, the type is Ntype.
public partial struct TypeAssertExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public Ntype Ntype; // Runtime type information provided by walkDotType for
// assertions from non-empty interface to concrete type.
    [Description("mknode:\"-\"")]
    public ptr<AddrExpr> Itab; // *runtime.itab for Type implementing X's type
}

public static ptr<TypeAssertExpr> NewTypeAssertExpr(src.XPos pos, Node x, Ntype typ) {
    ptr<TypeAssertExpr> n = addr(new TypeAssertExpr(X:x,Ntype:typ));
    n.pos = pos;
    n.op = ODOTTYPE;
    return _addr_n!;
}

private static void SetOp(this ptr<TypeAssertExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref TypeAssertExpr n = ref _addr_n.val;


    if (op == ODOTTYPE || op == ODOTTYPE2) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// A UnaryExpr is a unary expression Op X,
// or Op(X) for a builtin function that does not end up being a call.
public partial struct UnaryExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
}

public static ptr<UnaryExpr> NewUnaryExpr(src.XPos pos, Op op, Node x) {
    ptr<UnaryExpr> n = addr(new UnaryExpr(X:x));
    n.pos = pos;
    n.SetOp(op);
    return _addr_n!;
}

private static void SetOp(this ptr<UnaryExpr> _addr_n, Op op) => func((_, panic, _) => {
    ref UnaryExpr n = ref _addr_n.val;


    if (op == OBITNOT || op == ONEG || op == ONOT || op == OPLUS || op == ORECV || op == OALIGNOF || op == OCAP || op == OCLOSE || op == OIMAG || op == OLEN || op == ONEW || op == OOFFSETOF || op == OPANIC || op == OREAL || op == OSIZEOF || op == OCHECKNIL || op == OCFUNC || op == OIDATA || op == OITAB || op == OSPTR || op == OVARDEF || op == OVARKILL || op == OVARLIVE) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    
});

// An InstExpr is a generic function or type instantiation.
public partial struct InstExpr {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Node X;
    public slice<Node> Targs;
}

public static ptr<InstExpr> NewInstExpr(src.XPos pos, Op op, Node x, slice<Node> targs) {
    ptr<InstExpr> n = addr(new InstExpr(X:x,Targs:targs));
    n.pos = pos;
    n.op = op;
    return _addr_n!;
}

public static bool IsZero(Node n) {

    if (n.Op() == ONIL) 
        return true;
    else if (n.Op() == OLITERAL) 
        {
            var u = n.Val();


            if (u.Kind() == constant.String) 
                return constant.StringVal(u) == "";
            else if (u.Kind() == constant.Bool) 
                return !constant.BoolVal(u);
            else 
                return constant.Sign(u) == 0;

        }
    else if (n.Op() == OARRAYLIT) 
        ptr<CompLitExpr> n = n._<ptr<CompLitExpr>>();
        {
            var n1__prev1 = n1;

            foreach (var (_, __n1) in n.List) {
                n1 = __n1;
                if (n1.Op() == OKEY) {
                    n1 = n1._<ptr<KeyExpr>>().Value;
                }
                if (!IsZero(n1)) {
                    return false;
                }
            }

            n1 = n1__prev1;
        }

        return true;
    else if (n.Op() == OSTRUCTLIT) 
        n = n._<ptr<CompLitExpr>>();
        {
            var n1__prev1 = n1;

            foreach (var (_, __n1) in n.List) {
                n1 = __n1;
                ptr<StructKeyExpr> n1 = n1._<ptr<StructKeyExpr>>();
                if (!IsZero(n1.Value)) {
                    return false;
                }
            }

            n1 = n1__prev1;
        }

        return true;
        return false;

}

// lvalue etc
public static bool IsAddressable(Node n) {

    if (n.Op() == OINDEX)
    {
        ptr<IndexExpr> n = n._<ptr<IndexExpr>>();
        if (n.X.Type() != null && n.X.Type().IsArray()) {
            return IsAddressable(n.X);
        }
        if (n.X.Type() != null && n.X.Type().IsString()) {
            return false;
        }
        fallthrough = true;
    }
    if (fallthrough || n.Op() == ODEREF || n.Op() == ODOTPTR)
    {
        return true;
        goto __switch_break0;
    }
    if (n.Op() == ODOT)
    {
        n = n._<ptr<SelectorExpr>>();
        return IsAddressable(n.X);
        goto __switch_break0;
    }
    if (n.Op() == ONAME)
    {
        n = n._<ptr<Name>>();
        if (n.Class == PFUNC) {
            return false;
        }
        return true;
        goto __switch_break0;
    }
    if (n.Op() == OLINKSYMOFFSET)
    {
        return true;
        goto __switch_break0;
    }

    __switch_break0:;

    return false;

}

public static Node StaticValue(Node n) {
    while (true) {
        if (n.Op() == OCONVNOP) {
            n = n._<ptr<ConvExpr>>().X;
            continue;
        }
        var n1 = staticValue1(n);
        if (n1 == null) {
            return n;
        }
        n = n1;

    }

}

// staticValue1 implements a simple SSA-like optimization. If n is a local variable
// that is initialized and never reassigned, staticValue1 returns the initializer
// expression. Otherwise, it returns nil.
private static Node staticValue1(Node nn) {
    if (nn.Op() != ONAME) {
        return null;
    }
    ptr<Name> n = nn._<ptr<Name>>();
    if (n.Class != PAUTO) {
        return null;
    }
    var defn = n.Defn;
    if (defn == null) {
        return null;
    }
    Node rhs = default;
FindRHS:

    if (defn.Op() == OAS) 
        defn = defn._<ptr<AssignStmt>>();
        rhs = defn.Y;
    else if (defn.Op() == OAS2) 
        defn = defn._<ptr<AssignListStmt>>();
        foreach (var (i, lhs) in defn.Lhs) {
            if (lhs == n) {
                rhs = defn.Rhs[i];
                _breakFindRHS = true;
                break;
            }

        }        @base.Fatalf("%v missing from LHS of %v", n, defn);
    else 
        return null;
        if (rhs == null) {
        @base.Fatalf("RHS is nil: %v", defn);
    }
    if (reassigned(n)) {
        return null;
    }
    return rhs;

}

// reassigned takes an ONAME node, walks the function in which it is defined, and returns a boolean
// indicating whether the name has any assignments other than its declaration.
// The second return value is the first such assignment encountered in the walk, if any. It is mostly
// useful for -m output documenting the reason for inhibited optimizations.
// NB: global variables are always considered to be re-assigned.
// TODO: handle initial declaration not including an assignment and followed by a single assignment?
private static bool reassigned(ptr<Name> _addr_name) {
    ref Name name = ref _addr_name.val;

    if (name.Op() != ONAME) {
        @base.Fatalf("reassigned %v", name);
    }
    if (name.Curfn == null) {
        return true;
    }
    Func<Node, bool> isName = x => {
        ptr<Name> (n, ok) = x._<ptr<Name>>();
        return ok && n.Canonical() == name;
    };

    Func<Node, bool> @do = default;
    do = n => {

        if (n.Op() == OAS) 
            ptr<AssignStmt> n = n._<ptr<AssignStmt>>();
            if (isName(n.X) && n != name.Defn) {
                return true;
            }
        else if (n.Op() == OAS2 || n.Op() == OAS2FUNC || n.Op() == OAS2MAPR || n.Op() == OAS2DOTTYPE || n.Op() == OAS2RECV || n.Op() == OSELRECV2) 
            n = n._<ptr<AssignListStmt>>();
            foreach (var (_, p) in n.Lhs) {
                if (isName(p) && n != name.Defn) {
                    return true;
                }
            }
        else if (n.Op() == OADDR) 
            n = n._<ptr<AddrExpr>>();
            if (isName(OuterValue(n.X))) {
                return true;
            }
        else if (n.Op() == OCLOSURE) 
            n = n._<ptr<ClosureExpr>>();
            if (Any(n.Func, do)) {
                return true;
            }
                return false;

    };
    return Any(name.Curfn, do);

}

// IsIntrinsicCall reports whether the compiler back end will treat the call as an intrinsic operation.
public static Func<ptr<CallExpr>, bool> IsIntrinsicCall = _p0 => false;

// SameSafeExpr checks whether it is safe to reuse one of l and r
// instead of computing both. SameSafeExpr assumes that l and r are
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
// currently OK, since the only place SameSafeExpr gets used on an
// lvalue expression is for OSLICE and OAPPEND optimizations, and it
// is correct in those settings.
public static bool SameSafeExpr(Node l, Node r) {
    if (l.Op() != r.Op() || !types.Identical(l.Type(), r.Type())) {
        return false;
    }

    if (l.Op() == ONAME) 
        return l == r;
    else if (l.Op() == ODOT || l.Op() == ODOTPTR) 
        ptr<SelectorExpr> l = l._<ptr<SelectorExpr>>();
        ptr<SelectorExpr> r = r._<ptr<SelectorExpr>>();
        return l.Sel != null && r.Sel != null && l.Sel == r.Sel && SameSafeExpr(l.X, r.X);
    else if (l.Op() == ODEREF) 
        l = l._<ptr<StarExpr>>();
        r = r._<ptr<StarExpr>>();
        return SameSafeExpr(l.X, r.X);
    else if (l.Op() == ONOT || l.Op() == OBITNOT || l.Op() == OPLUS || l.Op() == ONEG) 
        l = l._<ptr<UnaryExpr>>();
        r = r._<ptr<UnaryExpr>>();
        return SameSafeExpr(l.X, r.X);
    else if (l.Op() == OCONVNOP) 
        l = l._<ptr<ConvExpr>>();
        r = r._<ptr<ConvExpr>>();
        return SameSafeExpr(l.X, r.X);
    else if (l.Op() == OCONV) 
        l = l._<ptr<ConvExpr>>();
        r = r._<ptr<ConvExpr>>(); 
        // Some conversions can't be reused, such as []byte(str).
        // Allow only numeric-ish types. This is a bit conservative.
        return types.IsSimple[l.Type().Kind()] && SameSafeExpr(l.X, r.X);
    else if (l.Op() == OINDEX || l.Op() == OINDEXMAP) 
        l = l._<ptr<IndexExpr>>();
        r = r._<ptr<IndexExpr>>();
        return SameSafeExpr(l.X, r.X) && SameSafeExpr(l.Index, r.Index);
    else if (l.Op() == OADD || l.Op() == OSUB || l.Op() == OOR || l.Op() == OXOR || l.Op() == OMUL || l.Op() == OLSH || l.Op() == ORSH || l.Op() == OAND || l.Op() == OANDNOT || l.Op() == ODIV || l.Op() == OMOD) 
        l = l._<ptr<BinaryExpr>>();
        r = r._<ptr<BinaryExpr>>();
        return SameSafeExpr(l.X, r.X) && SameSafeExpr(l.Y, r.Y);
    else if (l.Op() == OLITERAL) 
        return constant.Compare(l.Val(), token.EQL, r.Val());
    else if (l.Op() == ONIL) 
        return true;
        return false;

}

// ShouldCheckPtr reports whether pointer checking should be enabled for
// function fn at a given level. See debugHelpFooter for defined
// levels.
public static bool ShouldCheckPtr(ptr<Func> _addr_fn, nint level) {
    ref Func fn = ref _addr_fn.val;

    return @base.Debug.Checkptr >= level && fn.Pragma & NoCheckPtr == 0;
}

// IsReflectHeaderDataField reports whether l is an expression p.Data
// where p has type reflect.SliceHeader or reflect.StringHeader.
public static bool IsReflectHeaderDataField(Node l) {
    if (l.Type() != types.Types[types.TUINTPTR]) {
        return false;
    }
    ptr<types.Sym> tsym;

    if (l.Op() == ODOT) 
        ptr<SelectorExpr> l = l._<ptr<SelectorExpr>>();
        tsym = l.X.Type().Sym();
    else if (l.Op() == ODOTPTR) 
        l = l._<ptr<SelectorExpr>>();
        tsym = l.X.Type().Elem().Sym();
    else 
        return false;
        if (tsym == null || l.Sym().Name != "Data" || tsym.Pkg.Path != "reflect") {
        return false;
    }
    return tsym.Name == "SliceHeader" || tsym.Name == "StringHeader";

}

public static slice<Node> ParamNames(ptr<types.Type> _addr_ft) {
    ref types.Type ft = ref _addr_ft.val;

    var args = make_slice<Node>(ft.NumParams());
    foreach (var (i, f) in ft.Params().FieldSlice()) {
        args[i] = AsNode(f.Nname);
    }    return args;
}

// MethodSym returns the method symbol representing a method name
// associated with a specific receiver type.
//
// Method symbols can be used to distinguish the same method appearing
// in different method sets. For example, T.M and (*T).M have distinct
// method symbols.
//
// The returned symbol will be marked as a function.
public static ptr<types.Sym> MethodSym(ptr<types.Type> _addr_recv, ptr<types.Sym> _addr_msym) {
    ref types.Type recv = ref _addr_recv.val;
    ref types.Sym msym = ref _addr_msym.val;

    var sym = MethodSymSuffix(_addr_recv, _addr_msym, "");
    sym.SetFunc(true);
    return _addr_sym!;
}

// MethodSymSuffix is like methodsym, but allows attaching a
// distinguisher suffix. To avoid collisions, the suffix must not
// start with a letter, number, or period.
public static ptr<types.Sym> MethodSymSuffix(ptr<types.Type> _addr_recv, ptr<types.Sym> _addr_msym, @string suffix) {
    ref types.Type recv = ref _addr_recv.val;
    ref types.Sym msym = ref _addr_msym.val;

    if (msym.IsBlank()) {
        @base.Fatalf("blank method name");
    }
    var rsym = recv.Sym();
    if (recv.IsPtr()) {
        if (rsym != null) {
            @base.Fatalf("declared pointer receiver type: %v", recv);
        }
        rsym = recv.Elem().Sym();

    }
    var rpkg = Pkgs.Go;
    if (rsym != null) {
        rpkg = rsym.Pkg;
    }
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    if (recv.IsPtr()) { 
        // The parentheses aren't really necessary, but
        // they're pretty traditional at this point.
        fmt.Fprintf(_addr_b, "(%-S)", recv);

    }
    else
 {
        fmt.Fprintf(_addr_b, "%-S", recv);
    }
    if (!types.IsExported(msym.Name) && msym.Pkg != rpkg) {
        b.WriteString(".");
        b.WriteString(msym.Pkg.Prefix);
    }
    b.WriteString(".");
    b.WriteString(msym.Name);
    b.WriteString(suffix);

    return _addr_rpkg.LookupBytes(b.Bytes())!;

}

// MethodExprName returns the ONAME representing the method
// referenced by expression n, which must be a method selector,
// method expression, or method value.
public static ptr<Name> MethodExprName(Node n) {
    ptr<Name> (name, _) = MethodExprFunc(n).Nname._<ptr<Name>>();
    return _addr_name!;
}

// MethodExprFunc is like MethodExprName, but returns the types.Field instead.
public static ptr<types.Field> MethodExprFunc(Node n) => func((_, panic, _) => {

    if (n.Op() == ODOTMETH || n.Op() == OMETHEXPR || n.Op() == OCALLPART) 
        return n._<ptr<SelectorExpr>>().Selection;
        @base.Fatalf("unexpected node: %v (%v)", n, n.Op());
    panic("unreachable");

});

} // end ir_package
