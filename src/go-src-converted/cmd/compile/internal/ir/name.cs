// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:30 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\name.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;

using constant = go.constant_package;


// An Ident is an identifier, possibly qualified.

using System;
public static partial class ir_package {

public partial struct Ident {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public ptr<types.Sym> sym;
}

public static ptr<Ident> NewIdent(src.XPos pos, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    ptr<Ident> n = @new<Ident>();
    n.op = ONONAME;
    n.pos = pos;
    n.sym = sym;
    return _addr_n!;
}

private static ptr<types.Sym> Sym(this ptr<Ident> _addr_n) {
    ref Ident n = ref _addr_n.val;

    return _addr_n.sym!;
}

private static void CanBeNtype(this ptr<Ident> _addr__p0) {
    ref Ident _p0 = ref _addr__p0.val;

}

// Name holds Node fields used only by named nodes (ONAME, OTYPE, some OLITERAL).
public partial struct Name {
    public ref miniExpr miniExpr => ref miniExpr_val;
    public Op BuiltinOp; // uint8
    public Class Class; // uint8
    public PragmaFlag pragma; // int16
    public bitset16 flags;
    public ptr<types.Sym> sym;
    public ptr<Func> Func; // TODO(austin): nil for I.M, eqFor, hashfor, and hashmem
    public long Offset_;
    public constant.Value val;
    public ptr<slice<Embed>> Embed; // list of embedded files, for ONAME var

    public ptr<PkgName> PkgName; // real package for import . names
// For a local variable (not param) or extern, the initializing assignment (OAS or OAS2).
// For a closure var, the ONAME node of the outer captured variable.
// For the case-local variables of a type switch, the type switch guard (OTYPESW).
// For the name of a function, points to corresponding Func node.
    public Node Defn; // The function, method, or closure in which local variable or param is declared.
    public ptr<Func> Curfn;
    public Ntype Ntype;
    public ptr<Name> Heapaddr; // temp holding heap address of param

// ONAME closure linkage
// Consider:
//
//    func f() {
//        x := 1 // x1
//        func() {
//            use(x) // x2
//            func() {
//                use(x) // x3
//                --- parser is here ---
//            }()
//        }()
//    }
//
// There is an original declaration of x and then a chain of mentions of x
// leading into the current function. Each time x is mentioned in a new closure,
// we create a variable representing x for use in that specific closure,
// since the way you get to x is different in each closure.
//
// Let's number the specific variables as shown in the code:
// x1 is the original x, x2 is when mentioned in the closure,
// and x3 is when mentioned in the closure in the closure.
//
// We keep these linked (assume N > 1):
//
//   - x1.Defn = original declaration statement for x (like most variables)
//   - x1.Innermost = current innermost closure x (in this case x3), or nil for none
//   - x1.IsClosureVar() = false
//
//   - xN.Defn = x1, N > 1
//   - xN.IsClosureVar() = true, N > 1
//   - x2.Outer = nil
//   - xN.Outer = x(N-1), N > 2
//
//
// When we look up x in the symbol table, we always get x1.
// Then we can use x1.Innermost (if not nil) to get the x
// for the innermost known closure function,
// but the first reference in a closure will find either no x1.Innermost
// or an x1.Innermost with .Funcdepth < Funcdepth.
// In that case, a new xN must be created, linked in with:
//
//     xN.Defn = x1
//     xN.Outer = x1.Innermost
//     x1.Innermost = xN
//
// When we finish the function, we'll process its closure variables
// and find xN and pop it off the list using:
//
//     x1 := xN.Defn
//     x1.Innermost = xN.Outer
//
// We leave x1.Innermost set so that we can still get to the original
// variable quickly. Not shown here, but once we're
// done parsing a function and no longer need xN.Outer for the
// lexical x reference links as described above, funcLit
// recomputes xN.Outer as the semantic x reference link tree,
// even filling in x in intermediate closures that might not
// have mentioned it along the way to inner closures that did.
// See funcLit for details.
//
// During the eventual compilation, then, for closure variables we have:
//
//     xN.Defn = original variable
//     xN.Outer = variable captured in next outward scope
//                to make closure where xN appears
//
// Because of the sharding of pieces of the node, x.Defn means x.Name.Defn
// and x.Innermost/Outer means x.Name.Param.Innermost/Outer.
    public ptr<Name> Innermost;
    public ptr<Name> Outer;
}

private static void isExpr(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

}

private static Node copy(this ptr<Name> _addr_n) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    panic(n.no("copy"));
});
private static bool doChildren(this ptr<Name> _addr_n, Func<Node, bool> @do) {
    ref Name n = ref _addr_n.val;

    return false;
}
private static Node editChildren(this ptr<Name> _addr_n, Func<Node, Node> edit) {
    ref Name n = ref _addr_n.val;

}

// TypeDefn returns the type definition for a named OTYPE.
// That is, given "type T Defn", it returns Defn.
// It is used by package types.
private static ptr<types.Type> TypeDefn(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return _addr_n.Ntype.Type()!;
}

// RecordFrameOffset records the frame offset for the name.
// It is used by package types when laying out function arguments.
private static void RecordFrameOffset(this ptr<Name> _addr_n, long offset) {
    ref Name n = ref _addr_n.val;

    n.SetFrameOffset(offset);
}

// NewNameAt returns a new ONAME Node associated with symbol s at position pos.
// The caller is responsible for setting Curfn.
public static ptr<Name> NewNameAt(src.XPos pos, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    if (sym == null) {
        @base.Fatalf("NewNameAt nil");
    }
    return _addr_newNameAt(pos, ONAME, _addr_sym)!;
}

// NewIota returns a new OIOTA Node.
public static ptr<Name> NewIota(src.XPos pos, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    if (sym == null) {
        @base.Fatalf("NewIota nil");
    }
    return _addr_newNameAt(pos, OIOTA, _addr_sym)!;
}

// NewDeclNameAt returns a new Name associated with symbol s at position pos.
// The caller is responsible for setting Curfn.
public static ptr<Name> NewDeclNameAt(src.XPos pos, Op op, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    if (sym == null) {
        @base.Fatalf("NewDeclNameAt nil");
    }

    if (op == ONAME || op == OTYPE || op == OLITERAL)     else 
        @base.Fatalf("NewDeclNameAt op %v", op);
        return _addr_newNameAt(pos, op, _addr_sym)!;
}

// NewConstAt returns a new OLITERAL Node associated with symbol s at position pos.
public static ptr<Name> NewConstAt(src.XPos pos, ptr<types.Sym> _addr_sym, ptr<types.Type> _addr_typ, constant.Value val) {
    ref types.Sym sym = ref _addr_sym.val;
    ref types.Type typ = ref _addr_typ.val;

    if (sym == null) {
        @base.Fatalf("NewConstAt nil");
    }
    var n = newNameAt(pos, OLITERAL, _addr_sym);
    n.SetType(typ);
    n.SetVal(val);
    return _addr_n!;
}

// newNameAt is like NewNameAt but allows sym == nil.
private static ptr<Name> newNameAt(src.XPos pos, Op op, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    ptr<Name> n = @new<Name>();
    n.op = op;
    n.pos = pos;
    n.sym = sym;
    return _addr_n!;
}

private static ptr<Name> Name(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return _addr_n!;
}
private static ptr<types.Sym> Sym(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return _addr_n.sym!;
}
private static void SetSym(this ptr<Name> _addr_n, ptr<types.Sym> _addr_x) {
    ref Name n = ref _addr_n.val;
    ref types.Sym x = ref _addr_x.val;

    n.sym = x;
}
private static Op SubOp(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.BuiltinOp;
}
private static void SetSubOp(this ptr<Name> _addr_n, Op x) {
    ref Name n = ref _addr_n.val;

    n.BuiltinOp = x;
}
private static void SetFunc(this ptr<Name> _addr_n, ptr<Func> _addr_x) {
    ref Name n = ref _addr_n.val;
    ref Func x = ref _addr_x.val;

    n.Func = x;
}
private static long Offset(this ptr<Name> _addr_n) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    panic("Name.Offset");
});
private static void SetOffset(this ptr<Name> _addr_n, long x) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    if (x != 0) {
        panic("Name.SetOffset");
    }
});
private static long FrameOffset(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.Offset_;
}
private static void SetFrameOffset(this ptr<Name> _addr_n, long x) {
    ref Name n = ref _addr_n.val;

    n.Offset_ = x;
}
private static long Iota(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.Offset_;
}
private static void SetIota(this ptr<Name> _addr_n, long x) {
    ref Name n = ref _addr_n.val;

    n.Offset_ = x;
}
private static byte Walkdef(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.bits.get2(miniWalkdefShift);
}
private static void SetWalkdef(this ptr<Name> _addr_n, byte x) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    if (x > 3) {
        panic(fmt.Sprintf("cannot SetWalkdef %d", x));
    }
    n.bits.set2(miniWalkdefShift, x);
});

private static ptr<obj.LSym> Linksym(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return _addr_n.sym.Linksym()!;
}
private static ptr<obj.LSym> LinksymABI(this ptr<Name> _addr_n, obj.ABI abi) {
    ref Name n = ref _addr_n.val;

    return _addr_n.sym.LinksymABI(abi)!;
}

private static void CanBeNtype(this ptr<Name> _addr__p0) {
    ref Name _p0 = ref _addr__p0.val;

}
private static void CanBeAnSSASym(this ptr<Name> _addr__p0) {
    ref Name _p0 = ref _addr__p0.val;

}
private static void CanBeAnSSAAux(this ptr<Name> _addr__p0) {
    ref Name _p0 = ref _addr__p0.val;

}

// Pragma returns the PragmaFlag for p, which must be for an OTYPE.
private static PragmaFlag Pragma(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.pragma;
}

// SetPragma sets the PragmaFlag for p, which must be for an OTYPE.
private static void SetPragma(this ptr<Name> _addr_n, PragmaFlag flag) {
    ref Name n = ref _addr_n.val;

    n.pragma = flag;
}

// Alias reports whether p, which must be for an OTYPE, is a type alias.
private static bool Alias(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameAlias != 0;
}

// SetAlias sets whether p, which must be for an OTYPE, is a type alias.
private static void SetAlias(this ptr<Name> _addr_n, bool alias) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameAlias, alias);
}

private static readonly nint nameReadonly = 1 << (int)(iota);
private static readonly var nameByval = 0; // is the variable captured by value or by reference
private static readonly var nameNeedzero = 1; // if it contains pointers, needs to be zeroed on function entry
private static readonly var nameAutoTemp = 2; // is the variable a temporary (implies no dwarf info. reset if escapes to heap)
private static readonly var nameUsed = 3; // for variable declared and not used error
private static readonly var nameIsClosureVar = 4; // PAUTOHEAP closure pseudo-variable; original (if any) at n.Defn
private static readonly var nameIsOutputParamHeapAddr = 5; // pointer to a result parameter's heap copy
private static readonly var nameIsOutputParamInRegisters = 6; // output parameter in registers spills as an auto
private static readonly var nameAddrtaken = 7; // address taken, even if not moved to heap
private static readonly var nameInlFormal = 8; // PAUTO created by inliner, derived from callee formal
private static readonly var nameInlLocal = 9; // PAUTO created by inliner, derived from callee local
private static readonly var nameOpenDeferSlot = 10; // if temporary var storing info for open-coded defers
private static readonly var nameLibfuzzerExtraCounter = 11; // if PEXTERN should be assigned to __libfuzzer_extra_counters section
private static readonly var nameAlias = 12; // is type name an alias

private static bool Readonly(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameReadonly != 0;
}
private static bool Needzero(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameNeedzero != 0;
}
private static bool AutoTemp(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameAutoTemp != 0;
}
private static bool Used(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameUsed != 0;
}
private static bool IsClosureVar(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameIsClosureVar != 0;
}
private static bool IsOutputParamHeapAddr(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameIsOutputParamHeapAddr != 0;
}
private static bool IsOutputParamInRegisters(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameIsOutputParamInRegisters != 0;
}
private static bool Addrtaken(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameAddrtaken != 0;
}
private static bool InlFormal(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameInlFormal != 0;
}
private static bool InlLocal(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameInlLocal != 0;
}
private static bool OpenDeferSlot(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameOpenDeferSlot != 0;
}
private static bool LibfuzzerExtraCounter(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.flags & nameLibfuzzerExtraCounter != 0;
}

private static void setReadonly(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameReadonly, b);
}
private static void SetNeedzero(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameNeedzero, b);
}
private static void SetAutoTemp(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameAutoTemp, b);
}
private static void SetUsed(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameUsed, b);
}
private static void SetIsClosureVar(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameIsClosureVar, b);
}
private static void SetIsOutputParamHeapAddr(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameIsOutputParamHeapAddr, b);
}
private static void SetIsOutputParamInRegisters(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameIsOutputParamInRegisters, b);
}
private static void SetAddrtaken(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameAddrtaken, b);
}
private static void SetInlFormal(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameInlFormal, b);
}
private static void SetInlLocal(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameInlLocal, b);
}
private static void SetOpenDeferSlot(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameOpenDeferSlot, b);
}
private static void SetLibfuzzerExtraCounter(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    n.flags.set(nameLibfuzzerExtraCounter, b);
}

// OnStack reports whether variable n may reside on the stack.
private static bool OnStack(this ptr<Name> _addr_n) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    if (n.Op() == ONAME) {

        if (n.Class == PPARAM || n.Class == PPARAMOUT || n.Class == PAUTO) 
            return n.Esc() != EscHeap;
        else if (n.Class == PEXTERN || n.Class == PAUTOHEAP) 
            return false;
            }
    panic(fmt.Sprintf("%v: not a variable: %v", @base.FmtPos(n.Pos()), n));
});

// MarkReadonly indicates that n is an ONAME with readonly contents.
private static void MarkReadonly(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    if (n.Op() != ONAME) {
        @base.Fatalf("Node.MarkReadonly %v", n.Op());
    }
    n.setReadonly(true); 
    // Mark the linksym as readonly immediately
    // so that the SSA backend can use this information.
    // It will be overridden later during dumpglobls.
    n.Linksym().Type = objabi.SRODATA;
}

// Val returns the constant.Value for the node.
private static constant.Value Val(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    if (n.val == null) {
        return constant.MakeUnknown();
    }
    return n.val;
}

// SetVal sets the constant.Value for the node.
private static void SetVal(this ptr<Name> _addr_n, constant.Value v) => func((_, panic, _) => {
    ref Name n = ref _addr_n.val;

    if (n.op != OLITERAL) {
        panic(n.no("SetVal"));
    }
    AssertValidTypeForConst(n.Type(), v);
    n.val = v;
});

// Canonical returns the logical declaration that n represents. If n
// is a closure variable, then Canonical returns the original Name as
// it appears in the function that immediately contains the
// declaration. Otherwise, Canonical simply returns n itself.
private static ptr<Name> Canonical(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    if (n.IsClosureVar() && n.Defn != null) {
        n = n.Defn._<ptr<Name>>();
    }
    return _addr_n!;
}

private static void SetByval(this ptr<Name> _addr_n, bool b) {
    ref Name n = ref _addr_n.val;

    if (n.Canonical() != n) {
        @base.Fatalf("SetByval called on non-canonical variable: %v", n);
    }
    n.flags.set(nameByval, b);
}

private static bool Byval(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;
 
    // We require byval to be set on the canonical variable, but we
    // allow it to be accessed from any instance.
    return n.Canonical().flags & nameByval != 0;
}

// CaptureName returns a Name suitable for referring to n from within function
// fn or from the package block if fn is nil. If n is a free variable declared
// within a function that encloses fn, then CaptureName returns a closure
// variable that refers to n and adds it to fn.ClosureVars. Otherwise, it simply
// returns n.
public static ptr<Name> CaptureName(src.XPos pos, ptr<Func> _addr_fn, ptr<Name> _addr_n) {
    ref Func fn = ref _addr_fn.val;
    ref Name n = ref _addr_n.val;

    if (n.IsClosureVar()) {
        @base.FatalfAt(pos, "misuse of CaptureName on closure variable: %v", n);
    }
    if (n.Op() != ONAME || n.Curfn == null || n.Curfn == fn) {
        return _addr_n!; // okay to use directly
    }
    if (fn == null) {
        @base.FatalfAt(pos, "package-block reference to %v, declared in %v", n, n.Curfn);
    }
    var c = n.Innermost;
    if (c != null && c.Curfn == fn) {
        return _addr_c!;
    }
    c = NewNameAt(pos, _addr_n.Sym());
    c.Curfn = fn;
    c.Class = PAUTOHEAP;
    c.SetIsClosureVar(true);
    c.Defn = n; 

    // Link into list of active closure variables.
    // Popped from list in FinishCaptureNames.
    c.Outer = n.Innermost;
    n.Innermost = c;
    fn.ClosureVars = append(fn.ClosureVars, c);

    return _addr_c!;
}

// FinishCaptureNames handles any work leftover from calling CaptureName
// earlier. outerfn should be the function that immediately encloses fn.
public static void FinishCaptureNames(src.XPos pos, ptr<Func> _addr_outerfn, ptr<Func> _addr_fn) {
    ref Func outerfn = ref _addr_outerfn.val;
    ref Func fn = ref _addr_fn.val;
 
    // closure-specific variables are hanging off the
    // ordinary ones; see CaptureName above.
    // unhook them.
    // make the list of pointers for the closure call.
    foreach (var (_, cv) in fn.ClosureVars) { 
        // Unlink from n; see comment above on type Name for these fields.
        ptr<Name> n = cv.Defn._<ptr<Name>>();
        n.Innermost = cv.Outer; 

        // If the closure usage of n is not dense, we need to make it
        // dense by recapturing n within the enclosing function.
        //
        // That is, suppose we just finished parsing the innermost
        // closure f4 in this code:
        //
        //    func f() {
        //        n := 1
        //        func() { // f2
        //            use(n)
        //            func() { // f3
        //                func() { // f4
        //                    use(n)
        //                }()
        //            }()
        //        }()
        //    }
        //
        // At this point cv.Outer is f2's n; there is no n for f3. To
        // construct the closure f4 from within f3, we need to use f3's
        // n and in this case we need to create f3's n with CaptureName.
        //
        // We'll decide later in walk whether to use v directly or &v.
        cv.Outer = CaptureName(pos, _addr_outerfn, n);
    }
}

// SameSource reports whether two nodes refer to the same source
// element.
//
// It exists to help incrementally migrate the compiler towards
// allowing the introduction of IdentExpr (#42990). Once we have
// IdentExpr, it will no longer be safe to directly compare Node
// values to tell if they refer to the same Name. Instead, code will
// need to explicitly get references to the underlying Name object(s),
// and compare those instead.
//
// It will still be safe to compare Nodes directly for checking if two
// nodes are syntactically the same. The SameSource function exists to
// indicate code that intentionally compares Nodes for syntactic
// equality as opposed to code that has yet to be updated in
// preparation for IdentExpr.
public static bool SameSource(Node n1, Node n2) {
    return n1 == n2;
}

// Uses reports whether expression x is a (direct) use of the given
// variable.
public static bool Uses(Node x, ptr<Name> _addr_v) {
    ref Name v = ref _addr_v.val;

    if (v == null || v.Op() != ONAME) {
        @base.Fatalf("RefersTo bad Name: %v", v);
    }
    return x.Op() == ONAME && x.Name() == v;
}

// DeclaredBy reports whether expression x refers (directly) to a
// variable that was declared by the given statement.
public static bool DeclaredBy(Node x, Node stmt) {
    if (stmt == null) {
        @base.Fatalf("DeclaredBy nil");
    }
    return x.Op() == ONAME && SameSource(x.Name().Defn, stmt);
}

// The Class of a variable/function describes the "storage class"
// of a variable or function. During parsing, storage classes are
// called declaration contexts.
public partial struct Class { // : byte
}

//go:generate stringer -type=Class name.go
public static readonly Class Pxxx = iota; // no class; used during ssa conversion to indicate pseudo-variables
public static readonly var PEXTERN = 0; // global variables
public static readonly var PAUTO = 1; // local variables
public static readonly var PAUTOHEAP = 2; // local variables or parameters moved to heap
public static readonly var PPARAM = 3; // input arguments
public static readonly var PPARAMOUT = 4; // output results
public static readonly var PTYPEPARAM = 5; // type params
public static readonly _ PFUNC = uint((1 << 3) - iota); // static assert for iota <= (1 << 3)

public partial struct Embed {
    public src.XPos Pos;
    public slice<@string> Patterns;
}

// A Pack is an identifier referring to an imported package.
public partial struct PkgName {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<types.Sym> sym;
    public ptr<types.Pkg> Pkg;
    public bool Used;
}

private static ptr<types.Sym> Sym(this ptr<PkgName> _addr_p) {
    ref PkgName p = ref _addr_p.val;

    return _addr_p.sym!;
}

private static void CanBeNtype(this ptr<PkgName> _addr__p0) {
    ref PkgName _p0 = ref _addr__p0.val;

}

public static ptr<PkgName> NewPkgName(src.XPos pos, ptr<types.Sym> _addr_sym, ptr<types.Pkg> _addr_pkg) {
    ref types.Sym sym = ref _addr_sym.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    ptr<PkgName> p = addr(new PkgName(sym:sym,Pkg:pkg));
    p.op = OPACK;
    p.pos = pos;
    return _addr_p!;
}

} // end ir_package
