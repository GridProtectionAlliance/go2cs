// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// “Abstract” syntax representation.

// package ir -- go2cs converted at 2022 March 06 22:49:09 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\node.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using sort = go.sort_package;

using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ir_package {

    // A Node is the abstract interface to an IR node.
public partial interface Node {
    bool Format(fmt.State s, int verb); // Source position.
    bool Pos();
    bool SetPos(src.XPos x); // For making copies. For Copy and SepCopy.
    bool copy();
    bool doChildren(Func<Node, bool> _p0);
    bool editChildren(Func<Node, Node> _p0); // Abstract graph structure, for generic traversals.
    bool Op();
    bool Init(); // Fields specific to certain Ops only.
    bool Type();
    bool SetType(ptr<types.Type> t);
    bool Name();
    bool Sym();
    bool Val();
    bool SetVal(constant.Value v); // Storage for analysis passes.
    bool Esc();
    bool SetEsc(ushort x);
    bool Diag();
    bool SetDiag(bool x); // Typecheck values:
//  0 means the node is not typechecked
//  1 means the node is completely typechecked
//  2 means typechecking of the node is in progress
//  3 means the node has its type from types2, but may need transformation
    bool Typecheck();
    bool SetTypecheck(byte x);
    bool NonNil();
    bool MarkNonNil();
}

// Line returns n's position as a string. If n has been inlined,
// it uses the outermost position where n has been inlined.
public static @string Line(Node n) {
    return @base.FmtPos(n.Pos());
}

public static bool IsSynthetic(Node n) {
    var name = n.Sym().Name;
    return name[0] == '.' || name[0] == '~';
}

// IsAutoTmp indicates if n was created by the compiler as a temporary,
// based on the setting of the .AutoTemp flag in n's Name.
public static bool IsAutoTmp(Node n) {
    if (n == null || n.Op() != ONAME) {
        return false;
    }
    return n.Name().AutoTemp();

}

// MayBeShared reports whether n may occur in multiple places in the AST.
// Extra care must be taken when mutating such a node.
public static bool MayBeShared(Node n) {

    if (n.Op() == ONAME || n.Op() == OLITERAL || n.Op() == ONIL || n.Op() == OTYPE) 
        return true;
        return false;

}

public partial interface InitNode {
    ptr<Nodes> PtrInit();
    ptr<Nodes> SetInit(Nodes x);
}

public static Nodes TakeInit(Node n) {
    var init = n.Init();
    if (len(init) != 0) {
        n._<InitNode>().SetInit(null);
    }
    return init;

}

//go:generate stringer -type=Op -trimprefix=O node.go

public partial struct Op { // : byte
}

// Node ops.
public static readonly Op OXXX = iota; 

// names
public static readonly var ONAME = 0; // var or func name
// Unnamed arg or return value: f(int, string) (int, error) { etc }
// Also used for a qualified package identifier that hasn't been resolved yet.
public static readonly var ONONAME = 1;
public static readonly var OTYPE = 2; // type name
public static readonly var OPACK = 3; // import
public static readonly var OLITERAL = 4; // literal
public static readonly var ONIL = 5; // nil

// expressions
public static readonly var OADD = 6; // X + Y
public static readonly var OSUB = 7; // X - Y
public static readonly var OOR = 8; // X | Y
public static readonly var OXOR = 9; // X ^ Y
public static readonly var OADDSTR = 10; // +{List} (string addition, list elements are strings)
public static readonly var OADDR = 11; // &X
public static readonly var OANDAND = 12; // X && Y
public static readonly var OAPPEND = 13; // append(Args); after walk, X may contain elem type descriptor
public static readonly var OBYTES2STR = 14; // Type(X) (Type is string, X is a []byte)
public static readonly var OBYTES2STRTMP = 15; // Type(X) (Type is string, X is a []byte, ephemeral)
public static readonly var ORUNES2STR = 16; // Type(X) (Type is string, X is a []rune)
public static readonly var OSTR2BYTES = 17; // Type(X) (Type is []byte, X is a string)
public static readonly var OSTR2BYTESTMP = 18; // Type(X) (Type is []byte, X is a string, ephemeral)
public static readonly var OSTR2RUNES = 19; // Type(X) (Type is []rune, X is a string)
public static readonly var OSLICE2ARRPTR = 20; // Type(X) (Type is *[N]T, X is a []T)
// X = Y or (if Def=true) X := Y
// If Def, then Init includes a DCL node for X.
public static readonly var OAS = 21; 
// Lhs = Rhs (x, y, z = a, b, c) or (if Def=true) Lhs := Rhs
// If Def, then Init includes DCL nodes for Lhs
public static readonly var OAS2 = 22;
public static readonly var OAS2DOTTYPE = 23; // Lhs = Rhs (x, ok = I.(int))
public static readonly var OAS2FUNC = 24; // Lhs = Rhs (x, y = f())
public static readonly var OAS2MAPR = 25; // Lhs = Rhs (x, ok = m["foo"])
public static readonly var OAS2RECV = 26; // Lhs = Rhs (x, ok = <-c)
public static readonly var OASOP = 27; // X AsOp= Y (x += y)
public static readonly var OCALL = 28; // X(Args) (function call, method call or type conversion)

// OCALLFUNC, OCALLMETH, and OCALLINTER have the same structure.
// Prior to walk, they are: X(Args), where Args is all regular arguments.
// After walk, if any argument whose evaluation might requires temporary variable,
// that temporary variable will be pushed to Init, Args will contains an updated
// set of arguments. KeepAlive is all OVARLIVE nodes that are attached to OCALLxxx.
public static readonly var OCALLFUNC = 29; // X(Args) (function call f(args))
public static readonly var OCALLMETH = 30; // X(Args) (direct method call x.Method(args))
public static readonly var OCALLINTER = 31; // X(Args) (interface method call x.Method(args))
public static readonly var OCALLPART = 32; // X.Sel (method expression x.Method, not called)
public static readonly var OCAP = 33; // cap(X)
public static readonly var OCLOSE = 34; // close(X)
public static readonly var OCLOSURE = 35; // func Type { Func.Closure.Body } (func literal)
public static readonly var OCOMPLIT = 36; // Type{List} (composite literal, not yet lowered to specific form)
public static readonly var OMAPLIT = 37; // Type{List} (composite literal, Type is map)
public static readonly var OSTRUCTLIT = 38; // Type{List} (composite literal, Type is struct)
public static readonly var OARRAYLIT = 39; // Type{List} (composite literal, Type is array)
public static readonly var OSLICELIT = 40; // Type{List} (composite literal, Type is slice), Len is slice length.
public static readonly var OPTRLIT = 41; // &X (X is composite literal)
public static readonly var OCONV = 42; // Type(X) (type conversion)
public static readonly var OCONVIFACE = 43; // Type(X) (type conversion, to interface)
public static readonly var OCONVNOP = 44; // Type(X) (type conversion, no effect)
public static readonly var OCOPY = 45; // copy(X, Y)
public static readonly var ODCL = 46; // var X (declares X of type X.Type)

// Used during parsing but don't last.
public static readonly var ODCLFUNC = 47; // func f() or func (r) f()
public static readonly var ODCLCONST = 48; // const pi = 3.14
public static readonly var ODCLTYPE = 49; // type Int int or type Int = int

public static readonly var ODELETE = 50; // delete(Args)
public static readonly var ODOT = 51; // X.Sel (X is of struct type)
public static readonly var ODOTPTR = 52; // X.Sel (X is of pointer to struct type)
public static readonly var ODOTMETH = 53; // X.Sel (X is non-interface, Sel is method name)
public static readonly var ODOTINTER = 54; // X.Sel (X is interface, Sel is method name)
public static readonly var OXDOT = 55; // X.Sel (before rewrite to one of the preceding)
public static readonly var ODOTTYPE = 56; // X.Ntype or X.Type (.Ntype during parsing, .Type once resolved); after walk, Itab contains address of interface type descriptor and Itab.X contains address of concrete type descriptor
public static readonly var ODOTTYPE2 = 57; // X.Ntype or X.Type (.Ntype during parsing, .Type once resolved; on rhs of OAS2DOTTYPE); after walk, Itab contains address of interface type descriptor
public static readonly var OEQ = 58; // X == Y
public static readonly var ONE = 59; // X != Y
public static readonly var OLT = 60; // X < Y
public static readonly var OLE = 61; // X <= Y
public static readonly var OGE = 62; // X >= Y
public static readonly var OGT = 63; // X > Y
public static readonly var ODEREF = 64; // *X
public static readonly var OINDEX = 65; // X[Index] (index of array or slice)
public static readonly var OINDEXMAP = 66; // X[Index] (index of map)
public static readonly var OKEY = 67; // Key:Value (key:value in struct/array/map literal)
public static readonly var OSTRUCTKEY = 68; // Field:Value (key:value in struct literal, after type checking)
public static readonly var OLEN = 69; // len(X)
public static readonly var OMAKE = 70; // make(Args) (before type checking converts to one of the following)
public static readonly var OMAKECHAN = 71; // make(Type[, Len]) (type is chan)
public static readonly var OMAKEMAP = 72; // make(Type[, Len]) (type is map)
public static readonly var OMAKESLICE = 73; // make(Type[, Len[, Cap]]) (type is slice)
public static readonly var OMAKESLICECOPY = 74; // makeslicecopy(Type, Len, Cap) (type is slice; Len is length and Cap is the copied from slice)
// OMAKESLICECOPY is created by the order pass and corresponds to:
//  s = make(Type, Len); copy(s, Cap)
//
// Bounded can be set on the node when Len == len(Cap) is known at compile time.
//
// This node is created so the walk pass can optimize this pattern which would
// otherwise be hard to detect after the order pass.
public static readonly var OMUL = 75; // X * Y
public static readonly var ODIV = 76; // X / Y
public static readonly var OMOD = 77; // X % Y
public static readonly var OLSH = 78; // X << Y
public static readonly var ORSH = 79; // X >> Y
public static readonly var OAND = 80; // X & Y
public static readonly var OANDNOT = 81; // X &^ Y
public static readonly var ONEW = 82; // new(X); corresponds to calls to new in source code
public static readonly var ONOT = 83; // !X
public static readonly var OBITNOT = 84; // ^X
public static readonly var OPLUS = 85; // +X
public static readonly var ONEG = 86; // -X
public static readonly var OOROR = 87; // X || Y
public static readonly var OPANIC = 88; // panic(X)
public static readonly var OPRINT = 89; // print(List)
public static readonly var OPRINTN = 90; // println(List)
public static readonly var OPAREN = 91; // (X)
public static readonly var OSEND = 92; // Chan <- Value
public static readonly var OSLICE = 93; // X[Low : High] (X is untypechecked or slice)
public static readonly var OSLICEARR = 94; // X[Low : High] (X is pointer to array)
public static readonly var OSLICESTR = 95; // X[Low : High] (X is string)
public static readonly var OSLICE3 = 96; // X[Low : High : Max] (X is untypedchecked or slice)
public static readonly var OSLICE3ARR = 97; // X[Low : High : Max] (X is pointer to array)
public static readonly var OSLICEHEADER = 98; // sliceheader{Ptr, Len, Cap} (Ptr is unsafe.Pointer, Len is length, Cap is capacity)
public static readonly var ORECOVER = 99; // recover()
public static readonly var ORECV = 100; // <-X
public static readonly var ORUNESTR = 101; // Type(X) (Type is string, X is rune)
public static readonly var OSELRECV2 = 102; // like OAS2: Lhs = Rhs where len(Lhs)=2, len(Rhs)=1, Rhs[0].Op = ORECV (appears as .Var of OCASE)
public static readonly var OIOTA = 103; // iota
public static readonly var OREAL = 104; // real(X)
public static readonly var OIMAG = 105; // imag(X)
public static readonly var OCOMPLEX = 106; // complex(X, Y)
public static readonly var OALIGNOF = 107; // unsafe.Alignof(X)
public static readonly var OOFFSETOF = 108; // unsafe.Offsetof(X)
public static readonly var OSIZEOF = 109; // unsafe.Sizeof(X)
public static readonly var OUNSAFEADD = 110; // unsafe.Add(X, Y)
public static readonly var OUNSAFESLICE = 111; // unsafe.Slice(X, Y)
public static readonly var OMETHEXPR = 112; // method expression

// statements
public static readonly var OBLOCK = 113; // { List } (block of code)
public static readonly var OBREAK = 114; // break [Label]
// OCASE:  case List: Body (List==nil means default)
//   For OTYPESW, List is a OTYPE node for the specified type (or OLITERAL
//   for nil), and, if a type-switch variable is specified, Rlist is an
//   ONAME for the version of the type-switch variable with the specified
//   type.
public static readonly var OCASE = 115;
public static readonly var OCONTINUE = 116; // continue [Label]
public static readonly var ODEFER = 117; // defer Call
public static readonly var OFALL = 118; // fallthrough
public static readonly var OFOR = 119; // for Init; Cond; Post { Body }
// OFORUNTIL is like OFOR, but the test (Cond) is applied after the body:
//     Init
//     top: { Body }   // Execute the body at least once
//     cont: Post
//     if Cond {        // And then test the loop condition
//         List     // Before looping to top, execute List
//         goto top
//     }
// OFORUNTIL is created by walk. There's no way to write this in Go code.
public static readonly var OFORUNTIL = 120;
public static readonly var OGOTO = 121; // goto Label
public static readonly var OIF = 122; // if Init; Cond { Then } else { Else }
public static readonly var OLABEL = 123; // Label:
public static readonly var OGO = 124; // go Call
public static readonly var ORANGE = 125; // for Key, Value = range X { Body }
public static readonly var ORETURN = 126; // return Results
public static readonly var OSELECT = 127; // select { Cases }
public static readonly var OSWITCH = 128; // switch Init; Expr { Cases }
// OTYPESW:  X := Y.(type) (appears as .Tag of OSWITCH)
//   X is nil if there is no type-switch variable
public static readonly var OTYPESW = 129;
public static readonly var OFUNCINST = 130; // instantiation of a generic function

// types
public static readonly var OTCHAN = 131; // chan int
public static readonly var OTMAP = 132; // map[string]int
public static readonly var OTSTRUCT = 133; // struct{}
public static readonly var OTINTER = 134; // interface{}
// OTFUNC: func() - Recv is receiver field, Params is list of param fields, Results is
// list of result fields.
public static readonly var OTFUNC = 135;
public static readonly var OTARRAY = 136; // [8]int or [...]int
public static readonly var OTSLICE = 137; // []int

// misc
// intermediate representation of an inlined call.  Uses Init (assignments
// for the captured variables, parameters, retvars, & INLMARK op),
// Body (body of the inlined function), and ReturnVars (list of
// return values)
public static readonly var OINLCALL = 138; // intermediary representation of an inlined call.
public static readonly var OEFACE = 139; // itable and data words of an empty-interface value.
public static readonly var OITAB = 140; // itable word of an interface value.
public static readonly var OIDATA = 141; // data word of an interface value in X
public static readonly var OSPTR = 142; // base pointer of a slice or string.
public static readonly var OCFUNC = 143; // reference to c function pointer (not go func value)
public static readonly var OCHECKNIL = 144; // emit code to ensure pointer/interface not nil
public static readonly var OVARDEF = 145; // variable is about to be fully initialized
public static readonly var OVARKILL = 146; // variable is dead
public static readonly var OVARLIVE = 147; // variable is alive
public static readonly var ORESULT = 148; // result of a function call; Xoffset is stack offset
public static readonly var OINLMARK = 149; // start of an inlined body, with file/line of caller. Xoffset is an index into the inline tree.
public static readonly var OLINKSYMOFFSET = 150; // offset within a name

// arch-specific opcodes
public static readonly var OTAILCALL = 151; // tail call to another function
public static readonly var OGETG = 152; // runtime.getg() (read g pointer)

public static readonly var OEND = 153;


// Nodes is a pointer to a slice of *Node.
// For fields that are not used in most nodes, this is used instead of
// a slice to save space.
public partial struct Nodes { // : slice<Node>
}

// Append appends entries to Nodes.
private static void Append(this ptr<Nodes> _addr_n, params Node[] a) {
    a = a.Clone();
    ref Nodes n = ref _addr_n.val;

    if (len(a) == 0) {
        return ;
    }
    n.val = append(n.val, a);

}

// Prepend prepends entries to Nodes.
// If a slice is passed in, this will take ownership of it.
private static void Prepend(this ptr<Nodes> _addr_n, params Node[] a) {
    a = a.Clone();
    ref Nodes n = ref _addr_n.val;

    if (len(a) == 0) {
        return ;
    }
    n.val = append(a, n.val);

}

// Take clears n, returning its former contents.
private static slice<Node> Take(this ptr<Nodes> _addr_n) {
    ref Nodes n = ref _addr_n.val;

    var ret = n.val;
    n.val = null;
    return ret;
}

// Copy returns a copy of the content of the slice.
public static Nodes Copy(this Nodes n) {
    if (n == null) {
        return null;
    }
    var c = make(Nodes, len(n));
    copy(c, n);
    return c;

}

// NameQueue is a FIFO queue of *Name. The zero value of NameQueue is
// a ready-to-use empty queue.
public partial struct NameQueue {
    public slice<ptr<Name>> ring;
    public nint head;
    public nint tail;
}

// Empty reports whether q contains no Names.
private static bool Empty(this ptr<NameQueue> _addr_q) {
    ref NameQueue q = ref _addr_q.val;

    return q.head == q.tail;
}

// PushRight appends n to the right of the queue.
private static void PushRight(this ptr<NameQueue> _addr_q, ptr<Name> _addr_n) {
    ref NameQueue q = ref _addr_q.val;
    ref Name n = ref _addr_n.val;

    if (len(q.ring) == 0) {
        q.ring = make_slice<ptr<Name>>(16);
    }
    else if (q.head + len(q.ring) == q.tail) { 
        // Grow the ring.
        var nring = make_slice<ptr<Name>>(len(q.ring) * 2); 
        // Copy the old elements.
        var part = q.ring[(int)q.head % len(q.ring)..];
        if (q.tail - q.head <= len(part)) {
            part = part[..(int)q.tail - q.head];
            copy(nring, part);
        }
        else
 {
            var pos = copy(nring, part);
            copy(nring[(int)pos..], q.ring[..(int)q.tail % len(q.ring)]);
        }
        (q.ring, q.head, q.tail) = (nring, 0, q.tail - q.head);
    }
    q.ring[q.tail % len(q.ring)] = n;
    q.tail++;

}

// PopLeft pops a Name from the left of the queue. It panics if q is
// empty.
private static ptr<Name> PopLeft(this ptr<NameQueue> _addr_q) => func((_, panic, _) => {
    ref NameQueue q = ref _addr_q.val;

    if (q.Empty()) {
        panic("dequeue empty");
    }
    var n = q.ring[q.head % len(q.ring)];
    q.head++;
    return _addr_n!;

});

// NameSet is a set of Names.
public static bool Has(this NameSet s, ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    var (_, isPresent) = s[n];
    return isPresent;
}

// Add adds n to s.
private static void Add(this ptr<NameSet> _addr_s, ptr<Name> _addr_n) {
    ref NameSet s = ref _addr_s.val;
    ref Name n = ref _addr_n.val;

    if (s == null.val) {
        s.val = make();
    }
    (s.val)[n] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

}

// Sorted returns s sorted according to less.
public static slice<ptr<Name>> Sorted(this NameSet s, Func<ptr<Name>, ptr<Name>, bool> less) {
    slice<ptr<Name>> res = default;
    foreach (var (n) in s) {
        res = append(res, n);
    }    sort.Slice(res, (i, j) => less(res[i], res[j]));
    return res;
}

public partial struct PragmaFlag { // : short
}

 
// Func pragmas.
public static readonly PragmaFlag Nointerface = 1 << (int)(iota);
public static readonly var Noescape = 0; // func parameters don't escape
public static readonly var Norace = 1; // func must not have race detector annotations
public static readonly var Nosplit = 2; // func should not execute on separate stack
public static readonly var Noinline = 3; // func should not be inlined
public static readonly var NoCheckPtr = 4; // func should not be instrumented by checkptr
public static readonly var CgoUnsafeArgs = 5; // treat a pointer to one arg as a pointer to them all
public static readonly var UintptrEscapes = 6; // pointers converted to uintptr escape

// Runtime-only func pragmas.
// See ../../../../runtime/README.md for detailed descriptions.
public static readonly var Systemstack = 7; // func must run on system stack
public static readonly var Nowritebarrier = 8; // emit compiler error instead of write barrier
public static readonly var Nowritebarrierrec = 9; // error on write barrier in this or recursive callees
public static readonly var Yeswritebarrierrec = 10; // cancels Nowritebarrierrec in this function and callees

// Runtime and cgo type pragmas
public static readonly var NotInHeap = 11; // values of this type must not be heap allocated

// Go command pragmas
public static readonly var GoBuildPragma = 12;

public static readonly var RegisterParams = 13; // TODO(register args) remove after register abi is working


public static Node AsNode(types.Object n) {
    if (n == null) {
        return null;
    }
    return n._<Node>();

}

public static Node BlankNode = default!;

public static bool IsConst(Node n, constant.Kind ct) {
    return ConstType(n) == ct;
}

// IsNil reports whether n represents the universal untyped zero value "nil".
public static bool IsNil(Node n) { 
    // Check n.Orig because constant propagation may produce typed nil constants,
    // which don't exist in the Go spec.
    return n != null && Orig(n).Op() == ONIL;

}

public static bool IsBlank(Node n) {
    if (n == null) {
        return false;
    }
    return n.Sym().IsBlank();

}

// IsMethod reports whether n is a method.
// n must be a function or a method.
public static bool IsMethod(Node n) {
    return n.Type().Recv() != null;
}

public static bool HasNamedResults(ptr<Func> _addr_fn) {
    ref Func fn = ref _addr_fn.val;

    var typ = fn.Type();
    return typ.NumResults() > 0 && types.OrigSym(typ.Results().Field(0).Sym) != null;
}

// HasUniquePos reports whether n has a unique position that can be
// used for reporting error messages.
//
// It's primarily used to distinguish references to named objects,
// whose Pos will point back to their declaration position rather than
// their usage position.
public static bool HasUniquePos(Node n) {

    if (n.Op() == ONAME || n.Op() == OPACK) 
        return false;
    else if (n.Op() == OLITERAL || n.Op() == ONIL || n.Op() == OTYPE) 
        if (n.Sym() != null) {
            return false;
        }
        if (!n.Pos().IsKnown()) {
        if (@base.Flag.K != 0) {
            @base.Warn("setlineno: unknown position (line 0)");
        }
        return false;

    }
    return true;

}

public static src.XPos SetPos(Node n) {
    var lno = @base.Pos;
    if (n != null && HasUniquePos(n)) {
        @base.Pos = n.Pos();
    }
    return lno;

}

// The result of InitExpr MUST be assigned back to n, e.g.
//     n.X = InitExpr(init, n.X)
public static Node InitExpr(slice<Node> init, Node expr) {
    if (len(init) == 0) {
        return expr;
    }
    InitNode (n, ok) = InitNode.As(expr._<InitNode>())!;
    if (!ok || MayBeShared(n)) { 
        // Introduce OCONVNOP to hold init list.
        n = NewConvExpr(@base.Pos, OCONVNOP, null, expr);
        n.SetType(expr.Type());
        n.SetTypecheck(1);

    }
    n.PtrInit().Prepend(init);
    return n;

}

// what's the outer value that a write to n affects?
// outer value means containing struct or array.
public static Node OuterValue(Node n) {
    while (true) {
        {
            var nn__prev1 = nn;

            var nn = n;


            if (nn.Op() == OXDOT) 
                @base.Fatalf("OXDOT in walk");
            else if (nn.Op() == ODOT) 
                nn = nn._<ptr<SelectorExpr>>();
                n = nn.X;
                continue;
            else if (nn.Op() == OPAREN) 
                nn = nn._<ptr<ParenExpr>>();
                n = nn.X;
                continue;
            else if (nn.Op() == OCONVNOP) 
                nn = nn._<ptr<ConvExpr>>();
                n = nn.X;
                continue;
            else if (nn.Op() == OINDEX) 
                nn = nn._<ptr<IndexExpr>>();
                if (nn.X.Type() == null) {
                    @base.Fatalf("OuterValue needs type for %v", nn.X);
                }
                if (nn.X.Type().IsArray()) {
                    n = nn.X;
                    continue;
                }


            nn = nn__prev1;
        }

        return n;

    }

}

public static readonly var EscUnknown = iota;
public static readonly var EscNone = 0; // Does not escape to heap, result, or parameters.
public static readonly var EscHeap = 1; // Reachable from the heap
public static readonly var EscNever = 2; // By construction will not escape.

} // end ir_package
