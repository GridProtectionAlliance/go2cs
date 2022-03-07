// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:49:07 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\func.go
using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ir_package {

    // A Func corresponds to a single function in a Go program
    // (and vice versa: each function is denoted by exactly one *Func).
    //
    // There are multiple nodes that represent a Func in the IR.
    //
    // The ONAME node (Func.Nname) is used for plain references to it.
    // The ODCLFUNC node (the Func itself) is used for its declaration code.
    // The OCLOSURE node (Func.OClosure) is used for a reference to a
    // function literal.
    //
    // An imported function will have an ONAME node which points to a Func
    // with an empty body.
    // A declared function or method has an ODCLFUNC (the Func itself) and an ONAME.
    // A function literal is represented directly by an OCLOSURE, but it also
    // has an ODCLFUNC (and a matching ONAME) representing the compiled
    // underlying form of the closure, which accesses the captured variables
    // using a special data structure passed in a register.
    //
    // A method declaration is represented like functions, except f.Sym
    // will be the qualified method name (e.g., "T.m") and
    // f.Func.Shortname is the bare method name (e.g., "m").
    //
    // A method expression (T.M) is represented as an OMETHEXPR node,
    // in which n.Left and n.Right point to the type and method, respectively.
    // Each distinct mention of a method expression in the source code
    // constructs a fresh node.
    //
    // A method value (t.M) is represented by ODOTMETH/ODOTINTER
    // when it is called directly and by OCALLPART otherwise.
    // These are like method expressions, except that for ODOTMETH/ODOTINTER,
    // the method name is stored in Sym instead of Right.
    // Each OCALLPART ends up being implemented as a new
    // function, a bit like a closure, with its own ODCLFUNC.
    // The OCALLPART uses n.Func to record the linkage to
    // the generated ODCLFUNC, but there is no
    // pointer from the Func back to the OCALLPART.
public partial struct Func {
    public ref miniNode miniNode => ref miniNode_val;
    public Nodes Body;
    public long Iota;
    public ptr<Name> Nname; // ONAME node
    public ptr<ClosureExpr> OClosure; // OCLOSURE node

    public ptr<types.Sym> Shortname; // Extra entry code for the function. For example, allocate and initialize
// memory for escaping parameters.
    public Nodes Enter;
    public Nodes Exit; // ONAME nodes for all params/locals for this func/closure, does NOT
// include closurevars until transforming closures during walk.
// Names must be listed PPARAMs, PPARAMOUTs, then PAUTOs,
// with PPARAMs and PPARAMOUTs in order corresponding to the function signature.
// However, as anonymous or blank PPARAMs are not actually declared,
// they are omitted from Dcl.
// Anonymous and blank PPARAMOUTs are declared as ~rNN and ~bNN Names, respectively.
    public slice<ptr<Name>> Dcl; // ClosureVars lists the free variables that are used within a
// function literal, but formally declared in an enclosing
// function. The variables in this slice are the closure function's
// own copy of the variables, which are used within its function
// body. They will also each have IsClosureVar set, and will have
// Byval set if they're captured by value.
    public slice<ptr<Name>> ClosureVars; // Enclosed functions that need to be compiled.
// Populated during walk.
    public slice<ptr<Func>> Closures; // Parents records the parent scope of each scope within a
// function. The root scope (0) has no parent, so the i'th
// scope's parent is stored at Parents[i-1].
    public slice<ScopeID> Parents; // Marks records scope boundary changes.
    public slice<Mark> Marks;
    public ptr<obj.LSym> LSym; // Linker object in this function's native ABI (Func.ABI)

    public ptr<Inline> Inl; // Closgen tracks how many closures have been generated within
// this function. Used by closurename for creating unique
// function names.
    public int Closgen;
    public int Label; // largest auto-generated label in this function

    public src.XPos Endlineno;
    public src.XPos WBPos; // position of first write barrier; see SetWBPos

    public PragmaFlag Pragma; // go:xxx function annotations

    public bitset16 flags; // ABI is a function's "definition" ABI. This is the ABI that
// this function's generated code is expecting to be called by.
//
// For most functions, this will be obj.ABIInternal. It may be
// a different ABI for functions defined in assembly or ABI wrappers.
//
// This is included in the export data and tracked across packages.
    public obj.ABI ABI; // ABIRefs is the set of ABIs by which this function is referenced.
// For ABIs other than this function's definition ABI, the
// compiler generates ABI wrapper functions. This is only tracked
// within a package.
    public obj.ABISet ABIRefs;
    public int NumDefers; // number of defer calls in the function
    public int NumReturns; // number of explicit returns in the function

// nwbrCalls records the LSyms of functions called by this
// function for go:nowritebarrierrec analysis. Only filled in
// if nowritebarrierrecCheck != nil.
    public ptr<slice<SymAndPos>> NWBRCalls;
}

public static ptr<Func> NewFunc(src.XPos pos) {
    ptr<Func> f = @new<Func>();
    f.pos = pos;
    f.op = ODCLFUNC;
    f.Iota = -1; 
    // Most functions are ABIInternal. The importer or symabis
    // pass may override this.
    f.ABI = obj.ABIInternal;
    return _addr_f!;

}

private static void isStmt(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

}

private static Node copy(this ptr<Func> _addr_n) => func((_, panic, _) => {
    ref Func n = ref _addr_n.val;

    panic(n.no("copy"));
});
private static bool doChildren(this ptr<Func> _addr_n, Func<Node, bool> @do) {
    ref Func n = ref _addr_n.val;

    return doNodes(n.Body, do);
}
private static Node editChildren(this ptr<Func> _addr_n, Func<Node, Node> edit) {
    ref Func n = ref _addr_n.val;

    editNodes(n.Body, edit);
}

private static ptr<types.Type> Type(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return _addr_f.Nname.Type()!;
}
private static ptr<types.Sym> Sym(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return _addr_f.Nname.Sym()!;
}
private static ptr<obj.LSym> Linksym(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return _addr_f.Nname.Linksym()!;
}
private static ptr<obj.LSym> LinksymABI(this ptr<Func> _addr_f, obj.ABI abi) {
    ref Func f = ref _addr_f.val;

    return _addr_f.Nname.LinksymABI(abi)!;
}

// An Inline holds fields used for function bodies that can be inlined.
public partial struct Inline {
    public int Cost; // heuristic cost of inlining this function

// Copies of Func.Dcl and Func.Body for use during inlining. Copies are
// needed because the function's dcl/body may be changed by later compiler
// transformations. These fields are also populated when a function from
// another package is imported.
    public slice<ptr<Name>> Dcl;
    public slice<Node> Body;
}

// A Mark represents a scope boundary.
public partial struct Mark {
    public src.XPos Pos; // Scope identifies the innermost scope to the right of Pos.
    public ScopeID Scope;
}

// A ScopeID represents a lexical scope within a function.
public partial struct ScopeID { // : int
}

private static readonly nint funcDupok = 1 << (int)(iota); // duplicate definitions ok
private static readonly var funcWrapper = 0; // hide frame from users (elide in tracebacks, don't count as a frame for recover())
private static readonly var funcABIWrapper = 1; // is an ABI wrapper (also set flagWrapper)
private static readonly var funcNeedctxt = 2; // function uses context register (has closure variables)
private static readonly var funcReflectMethod = 3; // function calls reflect.Type.Method or MethodByName
// true if closure inside a function; false if a simple function or a
// closure in a global variable initialization
private static readonly var funcIsHiddenClosure = 4;
private static readonly var funcHasDefer = 5; // contains a defer statement
private static readonly var funcNilCheckDisabled = 6; // disable nil checks when compiling this function
private static readonly var funcInlinabilityChecked = 7; // inliner has already determined whether the function is inlinable
private static readonly var funcExportInline = 8; // include inline body in export data
private static readonly var funcInstrumentBody = 9; // add race/msan instrumentation during SSA construction
private static readonly var funcOpenCodedDeferDisallowed = 10; // can't do open-coded defers
private static readonly var funcClosureCalled = 11; // closure is only immediately called

public partial struct SymAndPos {
    public ptr<obj.LSym> Sym; // LSym of callee
    public src.XPos Pos; // line of call
}

private static bool Dupok(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcDupok != 0;
}
private static bool Wrapper(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcWrapper != 0;
}
private static bool ABIWrapper(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcABIWrapper != 0;
}
private static bool Needctxt(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcNeedctxt != 0;
}
private static bool ReflectMethod(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcReflectMethod != 0;
}
private static bool IsHiddenClosure(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcIsHiddenClosure != 0;
}
private static bool HasDefer(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcHasDefer != 0;
}
private static bool NilCheckDisabled(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcNilCheckDisabled != 0;
}
private static bool InlinabilityChecked(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcInlinabilityChecked != 0;
}
private static bool ExportInline(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcExportInline != 0;
}
private static bool InstrumentBody(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcInstrumentBody != 0;
}
private static bool OpenCodedDeferDisallowed(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcOpenCodedDeferDisallowed != 0;
}
private static bool ClosureCalled(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.flags & funcClosureCalled != 0;
}

private static void SetDupok(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcDupok, b);
}
private static void SetWrapper(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcWrapper, b);
}
private static void SetABIWrapper(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcABIWrapper, b);
}
private static void SetNeedctxt(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcNeedctxt, b);
}
private static void SetReflectMethod(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcReflectMethod, b);
}
private static void SetIsHiddenClosure(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcIsHiddenClosure, b);
}
private static void SetHasDefer(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcHasDefer, b);
}
private static void SetNilCheckDisabled(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcNilCheckDisabled, b);
}
private static void SetInlinabilityChecked(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcInlinabilityChecked, b);
}
private static void SetExportInline(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcExportInline, b);
}
private static void SetInstrumentBody(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcInstrumentBody, b);
}
private static void SetOpenCodedDeferDisallowed(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcOpenCodedDeferDisallowed, b);
}
private static void SetClosureCalled(this ptr<Func> _addr_f, bool b) {
    ref Func f = ref _addr_f.val;

    f.flags.set(funcClosureCalled, b);
}

private static void SetWBPos(this ptr<Func> _addr_f, src.XPos pos) {
    ref Func f = ref _addr_f.val;

    if (@base.Debug.WB != 0) {
        @base.WarnfAt(pos, "write barrier");
    }
    if (!f.WBPos.IsKnown()) {
        f.WBPos = pos;
    }
}

// FuncName returns the name (without the package) of the function n.
public static @string FuncName(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f == null || f.Nname == null) {
        return "<nil>";
    }
    return f.Sym().Name;

}

// PkgFuncName returns the name of the function referenced by n, with package prepended.
// This differs from the compiler's internal convention where local functions lack a package
// because the ultimate consumer of this is a human looking at an IDE; package is only empty
// if the compilation package is actually the empty string.
public static @string PkgFuncName(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f == null || f.Nname == null) {
        return "<nil>";
    }
    var s = f.Sym();
    var pkg = s.Pkg;

    var p = @base.Ctxt.Pkgpath;
    if (pkg != null && pkg.Path != "") {
        p = pkg.Path;
    }
    if (p == "") {
        return s.Name;
    }
    return p + "." + s.Name;

}

public static ptr<Func> CurFunc;

public static @string FuncSymName(ptr<types.Sym> _addr_s) {
    ref types.Sym s = ref _addr_s.val;

    return s.Name + "·f";
}

// MarkFunc marks a node as a function.
public static void MarkFunc(ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    if (n.Op() != ONAME || n.Class != Pxxx) {
        @base.Fatalf("expected ONAME/Pxxx node, got %v", n);
    }
    n.Class = PFUNC;
    n.Sym().SetFunc(true);

}

// ClosureDebugRuntimeCheck applies boilerplate checks for debug flags
// and compiling runtime
public static void ClosureDebugRuntimeCheck(ptr<ClosureExpr> _addr_clo) {
    ref ClosureExpr clo = ref _addr_clo.val;

    if (@base.Debug.Closure > 0) {
        if (clo.Esc() == EscHeap) {
            @base.WarnfAt(clo.Pos(), "heap closure, captured vars = %v", clo.Func.ClosureVars);
        }
        else
 {
            @base.WarnfAt(clo.Pos(), "stack closure, captured vars = %v", clo.Func.ClosureVars);
        }
    }
    if (@base.Flag.CompilingRuntime && clo.Esc() == EscHeap) {
        @base.ErrorfAt(clo.Pos(), "heap-allocated closure, not allowed in runtime");
    }
}

// IsTrivialClosure reports whether closure clo has an
// empty list of captured vars.
public static bool IsTrivialClosure(ptr<ClosureExpr> _addr_clo) {
    ref ClosureExpr clo = ref _addr_clo.val;

    return len(clo.Func.ClosureVars) == 0;
}

} // end ir_package
