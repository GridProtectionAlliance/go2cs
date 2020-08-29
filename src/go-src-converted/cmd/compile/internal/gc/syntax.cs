// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// “Abstract” syntax representation.

// package gc -- go2cs converted at 2020 August 29 09:29:27 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\syntax.go
using ssa = go.cmd.compile.@internal.ssa_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // A Node is a single node in the syntax tree.
        // Actually the syntax tree is a syntax DAG, because there is only one
        // node with Op=ONAME for a given instance of a variable x.
        // The same is true for Op=OTYPE and Op=OLITERAL. See Node.mayBeShared.
        public partial struct Node
        {
            public ptr<Node> Left;
            public ptr<Node> Right;
            public Nodes Ninit;
            public Nodes Nbody;
            public Nodes List;
            public Nodes Rlist; // most nodes
            public ptr<types.Type> Type;
            public ptr<Node> Orig; // original form, for printing, and tracking copies of ONAMEs

// func
            public ptr<Func> Func; // ONAME, OTYPE, OPACK, OLABEL, some OLITERAL
            public ptr<Name> Name;
            public ptr<types.Sym> Sym; // various
            public long Xoffset;
            public src.XPos Pos;
            public bitset32 flags;
            public ushort Esc; // EscXXX

            public Op Op;
            public types.EType Etype; // op for OASOP, etype for OTYPE, exclam for export, 6g saved reg, ChanDir for OTCHAN, for OINDEXMAP 1=LHS,0=RHS
        }

        // IsAutoTmp indicates if n was created by the compiler as a temporary,
        // based on the setting of the .AutoTemp flag in n's Name.
        private static bool IsAutoTmp(this ref Node n)
        {
            if (n == null || n.Op != ONAME)
            {
                return false;
            }
            return n.Name.AutoTemp();
        }

        private static readonly var nodeClass = iota;
        private static readonly long _ = 1L << (int)(iota); // PPARAM, PAUTO, PEXTERN, etc; three bits; first in the list because frequently accessed
        private static readonly var _ = 0;
        private static readonly var _ = 1; // second nodeClass bit
        private static readonly var _ = 2;
        private static readonly var _ = 3; // third nodeClass bit
        private static readonly var nodeWalkdef = 4;
        private static readonly var _ = 5; // tracks state during typecheckdef; 2 == loop detected; two bits
        private static readonly var _ = 6;
        private static readonly var _ = 7; // second nodeWalkdef bit
        private static readonly var nodeTypecheck = 8;
        private static readonly var _ = 9; // tracks state during typechecking; 2 == loop detected; two bits
        private static readonly var _ = 10;
        private static readonly var _ = 11; // second nodeTypecheck bit
        private static readonly var nodeInitorder = 12;
        private static readonly var _ = 13; // tracks state during init1; two bits
        private static readonly var _ = 14;
        private static readonly var _ = 15; // second nodeInitorder bit
        private static readonly var _ = 16;
        private static readonly var nodeHasBreak = 17;
        private static readonly var _ = 18;
        private static readonly var nodeIsClosureVar = 19;
        private static readonly var _ = 20;
        private static readonly var nodeIsOutputParamHeapAddr = 21;
        private static readonly var _ = 22;
        private static readonly var nodeNoInline = 23; // used internally by inliner to indicate that a function call should not be inlined; set for OCALLFUNC and OCALLMETH only
        private static readonly var _ = 24;
        private static readonly var nodeAssigned = 25; // is the variable ever assigned to
        private static readonly var _ = 26;
        private static readonly var nodeAddrtaken = 27; // address taken, even if not moved to heap
        private static readonly var _ = 28;
        private static readonly var nodeImplicit = 29;
        private static readonly var _ = 30;
        private static readonly var nodeIsddd = 31; // is the argument variadic
        private static readonly var _ = 32;
        private static readonly var nodeDiag = 33; // already printed error about this
        private static readonly var _ = 34;
        private static readonly var nodeColas = 35; // OAS resulting from :=
        private static readonly var _ = 36;
        private static readonly var nodeNonNil = 37; // guaranteed to be non-nil
        private static readonly var _ = 38;
        private static readonly var nodeNoescape = 39; // func arguments do not escape; TODO(rsc): move Noescape to Func struct (see CL 7360)
        private static readonly var _ = 40;
        private static readonly var nodeBounded = 41; // bounds check unnecessary
        private static readonly var _ = 42;
        private static readonly var nodeAddable = 43; // addressable
        private static readonly var _ = 44;
        private static readonly var nodeHasCall = 45; // expression contains a function call
        private static readonly var _ = 46;
        private static readonly var nodeLikely = 47; // if statement condition likely
        private static readonly var _ = 48;
        private static readonly var nodeHasVal = 49; // node.E contains a Val
        private static readonly var _ = 50;
        private static readonly var nodeHasOpt = 51; // node.E contains an Opt
        private static readonly var _ = 52;
        private static readonly var nodeEmbedded = 53; // ODCLFIELD embedded type
        private static readonly var _ = 54;
        private static readonly var nodeInlFormal = 55; // OPAUTO created by inliner, derived from callee formal
        private static readonly var _ = 56;
        private static readonly var nodeInlLocal = 57; // OPAUTO created by inliner, derived from callee local

        private static Class Class(this ref Node n)
        {
            return Class(n.flags.get3(nodeClass));
        }
        private static byte Walkdef(this ref Node n)
        {
            return n.flags.get2(nodeWalkdef);
        }
        private static byte Typecheck(this ref Node n)
        {
            return n.flags.get2(nodeTypecheck);
        }
        private static byte Initorder(this ref Node n)
        {
            return n.flags.get2(nodeInitorder);
        }

        private static bool HasBreak(this ref Node n)
        {
            return n.flags & nodeHasBreak != 0L;
        }
        private static bool IsClosureVar(this ref Node n)
        {
            return n.flags & nodeIsClosureVar != 0L;
        }
        private static bool NoInline(this ref Node n)
        {
            return n.flags & nodeNoInline != 0L;
        }
        private static bool IsOutputParamHeapAddr(this ref Node n)
        {
            return n.flags & nodeIsOutputParamHeapAddr != 0L;
        }
        private static bool Assigned(this ref Node n)
        {
            return n.flags & nodeAssigned != 0L;
        }
        private static bool Addrtaken(this ref Node n)
        {
            return n.flags & nodeAddrtaken != 0L;
        }
        private static bool Implicit(this ref Node n)
        {
            return n.flags & nodeImplicit != 0L;
        }
        private static bool Isddd(this ref Node n)
        {
            return n.flags & nodeIsddd != 0L;
        }
        private static bool Diag(this ref Node n)
        {
            return n.flags & nodeDiag != 0L;
        }
        private static bool Colas(this ref Node n)
        {
            return n.flags & nodeColas != 0L;
        }
        private static bool NonNil(this ref Node n)
        {
            return n.flags & nodeNonNil != 0L;
        }
        private static bool Noescape(this ref Node n)
        {
            return n.flags & nodeNoescape != 0L;
        }
        private static bool Bounded(this ref Node n)
        {
            return n.flags & nodeBounded != 0L;
        }
        private static bool Addable(this ref Node n)
        {
            return n.flags & nodeAddable != 0L;
        }
        private static bool HasCall(this ref Node n)
        {
            return n.flags & nodeHasCall != 0L;
        }
        private static bool Likely(this ref Node n)
        {
            return n.flags & nodeLikely != 0L;
        }
        private static bool HasVal(this ref Node n)
        {
            return n.flags & nodeHasVal != 0L;
        }
        private static bool HasOpt(this ref Node n)
        {
            return n.flags & nodeHasOpt != 0L;
        }
        private static bool Embedded(this ref Node n)
        {
            return n.flags & nodeEmbedded != 0L;
        }
        private static bool InlFormal(this ref Node n)
        {
            return n.flags & nodeInlFormal != 0L;
        }
        private static bool InlLocal(this ref Node n)
        {
            return n.flags & nodeInlLocal != 0L;
        }

        private static void SetClass(this ref Node n, Class b)
        {
            n.flags.set3(nodeClass, uint8(b));

        }
        private static void SetWalkdef(this ref Node n, byte b)
        {
            n.flags.set2(nodeWalkdef, b);

        }
        private static void SetTypecheck(this ref Node n, byte b)
        {
            n.flags.set2(nodeTypecheck, b);

        }
        private static void SetInitorder(this ref Node n, byte b)
        {
            n.flags.set2(nodeInitorder, b);

        }

        private static void SetHasBreak(this ref Node n, bool b)
        {
            n.flags.set(nodeHasBreak, b);

        }
        private static void SetIsClosureVar(this ref Node n, bool b)
        {
            n.flags.set(nodeIsClosureVar, b);

        }
        private static void SetNoInline(this ref Node n, bool b)
        {
            n.flags.set(nodeNoInline, b);

        }
        private static void SetIsOutputParamHeapAddr(this ref Node n, bool b)
        {
            n.flags.set(nodeIsOutputParamHeapAddr, b);

        }
        private static void SetAssigned(this ref Node n, bool b)
        {
            n.flags.set(nodeAssigned, b);

        }
        private static void SetAddrtaken(this ref Node n, bool b)
        {
            n.flags.set(nodeAddrtaken, b);

        }
        private static void SetImplicit(this ref Node n, bool b)
        {
            n.flags.set(nodeImplicit, b);

        }
        private static void SetIsddd(this ref Node n, bool b)
        {
            n.flags.set(nodeIsddd, b);

        }
        private static void SetDiag(this ref Node n, bool b)
        {
            n.flags.set(nodeDiag, b);

        }
        private static void SetColas(this ref Node n, bool b)
        {
            n.flags.set(nodeColas, b);

        }
        private static void SetNonNil(this ref Node n, bool b)
        {
            n.flags.set(nodeNonNil, b);

        }
        private static void SetNoescape(this ref Node n, bool b)
        {
            n.flags.set(nodeNoescape, b);

        }
        private static void SetBounded(this ref Node n, bool b)
        {
            n.flags.set(nodeBounded, b);

        }
        private static void SetAddable(this ref Node n, bool b)
        {
            n.flags.set(nodeAddable, b);

        }
        private static void SetHasCall(this ref Node n, bool b)
        {
            n.flags.set(nodeHasCall, b);

        }
        private static void SetLikely(this ref Node n, bool b)
        {
            n.flags.set(nodeLikely, b);

        }
        private static void SetHasVal(this ref Node n, bool b)
        {
            n.flags.set(nodeHasVal, b);

        }
        private static void SetHasOpt(this ref Node n, bool b)
        {
            n.flags.set(nodeHasOpt, b);

        }
        private static void SetEmbedded(this ref Node n, bool b)
        {
            n.flags.set(nodeEmbedded, b);

        }
        private static void SetInlFormal(this ref Node n, bool b)
        {
            n.flags.set(nodeInlFormal, b);

        }
        private static void SetInlLocal(this ref Node n, bool b)
        {
            n.flags.set(nodeInlLocal, b);

        }

        // Val returns the Val for the node.
        private static Val Val(this ref Node n)
        {
            if (!n.HasVal())
            {
                return new Val();
            }
            return new Val(n.E);
        }

        // SetVal sets the Val for the node, which must not have been used with SetOpt.
        private static void SetVal(this ref Node n, Val v)
        {
            if (n.HasOpt())
            {
                Debug['h'] = 1L;
                Dump("have Opt", n);
                Fatalf("have Opt");
            }
            n.SetHasVal(true);
            n.E = v.U;
        }

        // Opt returns the optimizer data for the node.
        private static void Opt(this ref Node n)
        {
            if (!n.HasOpt())
            {
                return null;
            }
            return n.E;
        }

        // SetOpt sets the optimizer data for the node, which must not have been used with SetVal.
        // SetOpt(nil) is ignored for Vals to simplify call sites that are clearing Opts.
        private static void SetOpt(this ref Node n, object x)
        {
            if (x == null && n.HasVal())
            {
                return;
            }
            if (n.HasVal())
            {
                Debug['h'] = 1L;
                Dump("have Val", n);
                Fatalf("have Val");
            }
            n.SetHasOpt(true);
            n.E = x;
        }

        private static long Iota(this ref Node n)
        {
            return n.Xoffset;
        }

        private static void SetIota(this ref Node n, long x)
        {
            n.Xoffset = x;
        }

        // mayBeShared reports whether n may occur in multiple places in the AST.
        // Extra care must be taken when mutating such a node.
        private static bool mayBeShared(this ref Node n)
        {

            if (n.Op == ONAME || n.Op == OLITERAL || n.Op == OTYPE) 
                return true;
                        return false;
        }

        // isMethodExpression reports whether n represents a method expression T.M.
        private static bool isMethodExpression(this ref Node n)
        {
            return n.Op == ONAME && n.Left != null && n.Left.Op == OTYPE && n.Right != null && n.Right.Op == ONAME;
        }

        // funcname returns the name of the function n.
        private static @string funcname(this ref Node n)
        {
            if (n == null || n.Func == null || n.Func.Nname == null)
            {
                return "<nil>";
            }
            return n.Func.Nname.Sym.Name;
        }

        // Name holds Node fields used only by named nodes (ONAME, OTYPE, OPACK, OLABEL, some OLITERAL).
        public partial struct Name
        {
            public ptr<Node> Pack; // real package for import . names
            public ptr<types.Pkg> Pkg; // pkg for OPACK nodes
            public ptr<Node> Defn; // initializing assignment
            public ptr<Node> Curfn; // function for local variables
            public ptr<Param> Param; // additional fields for ONAME, OTYPE
            public int Decldepth; // declaration loop depth, increased for every loop or label
            public int Vargen; // unique name for ONAME within a function.  Function outputs are numbered starting at one.
            public int Funcdepth;
            public bool used; // for variable declared and not used error
            public bitset8 flags;
        }

        private static readonly long nameCaptured = 1L << (int)(iota); // is the variable captured by a closure
        private static readonly var nameReadonly = 0;
        private static readonly var nameByval = 1; // is the variable captured by value or by reference
        private static readonly var nameNeedzero = 2; // if it contains pointers, needs to be zeroed on function entry
        private static readonly var nameKeepalive = 3; // mark value live across unknown assembly call
        private static readonly var nameAutoTemp = 4; // is the variable a temporary (implies no dwarf info. reset if escapes to heap)

        private static bool Captured(this ref Name n)
        {
            return n.flags & nameCaptured != 0L;
        }
        private static bool Readonly(this ref Name n)
        {
            return n.flags & nameReadonly != 0L;
        }
        private static bool Byval(this ref Name n)
        {
            return n.flags & nameByval != 0L;
        }
        private static bool Needzero(this ref Name n)
        {
            return n.flags & nameNeedzero != 0L;
        }
        private static bool Keepalive(this ref Name n)
        {
            return n.flags & nameKeepalive != 0L;
        }
        private static bool AutoTemp(this ref Name n)
        {
            return n.flags & nameAutoTemp != 0L;
        }
        private static bool Used(this ref Name n)
        {
            return n.used;
        }

        private static void SetCaptured(this ref Name n, bool b)
        {
            n.flags.set(nameCaptured, b);

        }
        private static void SetReadonly(this ref Name n, bool b)
        {
            n.flags.set(nameReadonly, b);

        }
        private static void SetByval(this ref Name n, bool b)
        {
            n.flags.set(nameByval, b);

        }
        private static void SetNeedzero(this ref Name n, bool b)
        {
            n.flags.set(nameNeedzero, b);

        }
        private static void SetKeepalive(this ref Name n, bool b)
        {
            n.flags.set(nameKeepalive, b);

        }
        private static void SetAutoTemp(this ref Name n, bool b)
        {
            n.flags.set(nameAutoTemp, b);

        }
        private static void SetUsed(this ref Name n, bool b)
        {
            n.used = b;

        }

        public partial struct Param
        {
            public ptr<Node> Ntype;
            public ptr<Node> Heapaddr; // temp holding heap address of param

// ONAME PAUTOHEAP
            public ptr<Node> Stackcopy; // the PPARAM/PPARAMOUT on-stack slot (moved func params only)

// ONAME PPARAM
            public ptr<types.Field> Field; // TFIELD in arg struct

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
// We leave xN.Innermost set so that we can still get to the original
// variable quickly. Not shown here, but once we're
// done parsing a function and no longer need xN.Outer for the
// lexical x reference links as described above, closurebody
// recomputes xN.Outer as the semantic x reference link tree,
// even filling in x in intermediate closures that might not
// have mentioned it along the way to inner closures that did.
// See closurebody for details.
//
// During the eventual compilation, then, for closure variables we have:
//
//     xN.Defn = original variable
//     xN.Outer = variable captured in next outward scope
//                to make closure where xN appears
//
// Because of the sharding of pieces of the node, x.Defn means x.Name.Defn
// and x.Innermost/Outer means x.Name.Param.Innermost/Outer.
            public ptr<Node> Innermost;
            public ptr<Node> Outer; // OTYPE
//
// TODO: Should Func pragmas also be stored on the Name?
            public syntax.Pragma Pragma;
            public bool Alias; // node is alias for Ntype (only used when type-checking ODCLTYPE)
        }

        // Functions
        //
        // A simple function declaration is represented as an ODCLFUNC node f
        // and an ONAME node n. They're linked to one another through
        // f.Func.Nname == n and n.Name.Defn == f. When functions are
        // referenced by name in an expression, the function's ONAME node is
        // used directly.
        //
        // Function names have n.Class() == PFUNC. This distinguishes them
        // from variables of function type.
        //
        // Confusingly, n.Func and f.Func both exist, but commonly point to
        // different Funcs. (Exception: an OCALLPART's Func does point to its
        // ODCLFUNC's Func.)
        //
        // A method declaration is represented like functions, except n.Sym
        // will be the qualified method name (e.g., "T.m") and
        // f.Func.Shortname is the bare method name (e.g., "m").
        //
        // Method expressions are represented as ONAME/PFUNC nodes like
        // function names, but their Left and Right fields still point to the
        // type and method, respectively. They can be distinguished from
        // normal functions with isMethodExpression. Also, unlike function
        // name nodes, method expression nodes exist for each method
        // expression. The declaration ONAME can be accessed with
        // x.Type.Nname(), where x is the method expression ONAME node.
        //
        // Method values are represented by ODOTMETH/ODOTINTER when called
        // immediately, and OCALLPART otherwise. They are like method
        // expressions, except that for ODOTMETH/ODOTINTER the method name is
        // stored in Sym instead of Right.
        //
        // Closures are represented by OCLOSURE node c. They link back and
        // forth with the ODCLFUNC via Func.Closure; that is, c.Func.Closure
        // == f and f.Func.Closure == c.
        //
        // Function bodies are stored in f.Nbody, and inline function bodies
        // are stored in n.Func.Inl. Pragmas are stored in f.Func.Pragma.
        //
        // Imported functions skip the ODCLFUNC, so n.Name.Defn is nil. They
        // also use Dcl instead of Inldcl.

        // Func holds Node fields used only with function-like nodes.
        public partial struct Func
        {
            public ptr<types.Sym> Shortname;
            public Nodes Enter; // for example, allocate and initialize memory for escaping parameters
            public Nodes Exit;
            public Nodes Cvars; // closure params
            public slice<ref Node> Dcl; // autodcl for this func/closure
            public Nodes Inldcl; // copy of dcl for use in inlining

// Parents records the parent scope of each scope within a
// function. The root scope (0) has no parent, so the i'th
// scope's parent is stored at Parents[i-1].
            public slice<ScopeID> Parents; // Marks records scope boundary changes.
            public slice<Mark> Marks;
            public long Closgen;
            public ptr<Node> Outerfunc; // outer function (for closure)
            public ptr<ssa.FuncDebug> DebugInfo;
            public ptr<Node> Ntype; // signature
            public long Top; // top context (Ecall, Eproc, etc)
            public ptr<Node> Closure; // OCLOSURE <-> ODCLFUNC
            public ptr<Node> Nname;
            public ptr<obj.LSym> lsym;
            public Nodes Inl; // copy of the body for use in inlining
            public int InlCost;
            public int Depth;
            public int Label; // largest auto-generated label in this function

            public src.XPos Endlineno;
            public src.XPos WBPos; // position of first write barrier; see SetWBPos

            public syntax.Pragma Pragma; // go:xxx function annotations

            public bitset16 flags; // nwbrCalls records the LSyms of functions called by this
// function for go:nowritebarrierrec analysis. Only filled in
// if nowritebarrierrecCheck != nil.
            public ptr<slice<nowritebarrierrecCallSym>> nwbrCalls;
        }

        // A Mark represents a scope boundary.
        public partial struct Mark
        {
            public src.XPos Pos; // Scope identifies the innermost scope to the right of Pos.
            public ScopeID Scope;
        }

        // A ScopeID represents a lexical scope within a function.
        public partial struct ScopeID // : int
        {
        }

        private static readonly long funcDupok = 1L << (int)(iota); // duplicate definitions ok
        private static readonly var funcWrapper = 0; // is method wrapper
        private static readonly var funcNeedctxt = 1; // function uses context register (has closure variables)
        private static readonly var funcReflectMethod = 2; // function calls reflect.Type.Method or MethodByName
        private static readonly var funcIsHiddenClosure = 3;
        private static readonly var funcNoFramePointer = 4; // Must not use a frame pointer for this function
        private static readonly var funcHasDefer = 5; // contains a defer statement
        private static readonly var funcNilCheckDisabled = 6; // disable nil checks when compiling this function
        private static readonly var funcInlinabilityChecked = 7; // inliner has already determined whether the function is inlinable
        private static readonly var funcExportInline = 8; // include inline body in export data

        private static bool Dupok(this ref Func f)
        {
            return f.flags & funcDupok != 0L;
        }
        private static bool Wrapper(this ref Func f)
        {
            return f.flags & funcWrapper != 0L;
        }
        private static bool Needctxt(this ref Func f)
        {
            return f.flags & funcNeedctxt != 0L;
        }
        private static bool ReflectMethod(this ref Func f)
        {
            return f.flags & funcReflectMethod != 0L;
        }
        private static bool IsHiddenClosure(this ref Func f)
        {
            return f.flags & funcIsHiddenClosure != 0L;
        }
        private static bool NoFramePointer(this ref Func f)
        {
            return f.flags & funcNoFramePointer != 0L;
        }
        private static bool HasDefer(this ref Func f)
        {
            return f.flags & funcHasDefer != 0L;
        }
        private static bool NilCheckDisabled(this ref Func f)
        {
            return f.flags & funcNilCheckDisabled != 0L;
        }
        private static bool InlinabilityChecked(this ref Func f)
        {
            return f.flags & funcInlinabilityChecked != 0L;
        }
        private static bool ExportInline(this ref Func f)
        {
            return f.flags & funcExportInline != 0L;
        }

        private static void SetDupok(this ref Func f, bool b)
        {
            f.flags.set(funcDupok, b);

        }
        private static void SetWrapper(this ref Func f, bool b)
        {
            f.flags.set(funcWrapper, b);

        }
        private static void SetNeedctxt(this ref Func f, bool b)
        {
            f.flags.set(funcNeedctxt, b);

        }
        private static void SetReflectMethod(this ref Func f, bool b)
        {
            f.flags.set(funcReflectMethod, b);

        }
        private static void SetIsHiddenClosure(this ref Func f, bool b)
        {
            f.flags.set(funcIsHiddenClosure, b);

        }
        private static void SetNoFramePointer(this ref Func f, bool b)
        {
            f.flags.set(funcNoFramePointer, b);

        }
        private static void SetHasDefer(this ref Func f, bool b)
        {
            f.flags.set(funcHasDefer, b);

        }
        private static void SetNilCheckDisabled(this ref Func f, bool b)
        {
            f.flags.set(funcNilCheckDisabled, b);

        }
        private static void SetInlinabilityChecked(this ref Func f, bool b)
        {
            f.flags.set(funcInlinabilityChecked, b);

        }
        private static void SetExportInline(this ref Func f, bool b)
        {
            f.flags.set(funcExportInline, b);

        }

        private static void setWBPos(this ref Func f, src.XPos pos)
        {
            if (Debug_wb != 0L)
            {
                Warnl(pos, "write barrier");
            }
            if (!f.WBPos.IsKnown())
            {
                f.WBPos = pos;
            }
        }

        //go:generate stringer -type=Op -trimprefix=O

        public partial struct Op // : byte
        {
        }

        // Node ops.
        public static readonly Op OXXX = iota; 

        // names
        public static readonly var ONAME = 0; // var, const or func name
        public static readonly var ONONAME = 1; // unnamed arg or return value: f(int, string) (int, error) { etc }
        public static readonly var OTYPE = 2; // type name
        public static readonly var OPACK = 3; // import
        public static readonly var OLITERAL = 4; // literal

        // expressions
        public static readonly var OADD = 5; // Left + Right
        public static readonly var OSUB = 6; // Left - Right
        public static readonly var OOR = 7; // Left | Right
        public static readonly var OXOR = 8; // Left ^ Right
        public static readonly var OADDSTR = 9; // +{List} (string addition, list elements are strings)
        public static readonly var OADDR = 10; // &Left
        public static readonly var OANDAND = 11; // Left && Right
        public static readonly var OAPPEND = 12; // append(List); after walk, Left may contain elem type descriptor
        public static readonly var OARRAYBYTESTR = 13; // Type(Left) (Type is string, Left is a []byte)
        public static readonly var OARRAYBYTESTRTMP = 14; // Type(Left) (Type is string, Left is a []byte, ephemeral)
        public static readonly var OARRAYRUNESTR = 15; // Type(Left) (Type is string, Left is a []rune)
        public static readonly var OSTRARRAYBYTE = 16; // Type(Left) (Type is []byte, Left is a string)
        public static readonly var OSTRARRAYBYTETMP = 17; // Type(Left) (Type is []byte, Left is a string, ephemeral)
        public static readonly var OSTRARRAYRUNE = 18; // Type(Left) (Type is []rune, Left is a string)
        public static readonly var OAS = 19; // Left = Right or (if Colas=true) Left := Right
        public static readonly var OAS2 = 20; // List = Rlist (x, y, z = a, b, c)
        public static readonly var OAS2FUNC = 21; // List = Rlist (x, y = f())
        public static readonly var OAS2RECV = 22; // List = Rlist (x, ok = <-c)
        public static readonly var OAS2MAPR = 23; // List = Rlist (x, ok = m["foo"])
        public static readonly var OAS2DOTTYPE = 24; // List = Rlist (x, ok = I.(int))
        public static readonly var OASOP = 25; // Left Etype= Right (x += y)
        public static readonly var OCALL = 26; // Left(List) (function call, method call or type conversion)
        public static readonly var OCALLFUNC = 27; // Left(List) (function call f(args))
        public static readonly var OCALLMETH = 28; // Left(List) (direct method call x.Method(args))
        public static readonly var OCALLINTER = 29; // Left(List) (interface method call x.Method(args))
        public static readonly var OCALLPART = 30; // Left.Right (method expression x.Method, not called)
        public static readonly var OCAP = 31; // cap(Left)
        public static readonly var OCLOSE = 32; // close(Left)
        public static readonly var OCLOSURE = 33; // func Type { Body } (func literal)
        public static readonly var OCMPIFACE = 34; // Left Etype Right (interface comparison, x == y or x != y)
        public static readonly var OCMPSTR = 35; // Left Etype Right (string comparison, x == y, x < y, etc)
        public static readonly var OCOMPLIT = 36; // Right{List} (composite literal, not yet lowered to specific form)
        public static readonly var OMAPLIT = 37; // Type{List} (composite literal, Type is map)
        public static readonly var OSTRUCTLIT = 38; // Type{List} (composite literal, Type is struct)
        public static readonly var OARRAYLIT = 39; // Type{List} (composite literal, Type is array)
        public static readonly var OSLICELIT = 40; // Type{List} (composite literal, Type is slice)
        public static readonly var OPTRLIT = 41; // &Left (left is composite literal)
        public static readonly var OCONV = 42; // Type(Left) (type conversion)
        public static readonly var OCONVIFACE = 43; // Type(Left) (type conversion, to interface)
        public static readonly var OCONVNOP = 44; // Type(Left) (type conversion, no effect)
        public static readonly var OCOPY = 45; // copy(Left, Right)
        public static readonly var ODCL = 46; // var Left (declares Left of type Left.Type)

        // Used during parsing but don't last.
        public static readonly var ODCLFUNC = 47; // func f() or func (r) f()
        public static readonly var ODCLFIELD = 48; // struct field, interface field, or func/method argument/return value.
        public static readonly var ODCLCONST = 49; // const pi = 3.14
        public static readonly var ODCLTYPE = 50; // type Int int or type Int = int

        public static readonly var ODELETE = 51; // delete(Left, Right)
        public static readonly var ODOT = 52; // Left.Sym (Left is of struct type)
        public static readonly var ODOTPTR = 53; // Left.Sym (Left is of pointer to struct type)
        public static readonly var ODOTMETH = 54; // Left.Sym (Left is non-interface, Right is method name)
        public static readonly var ODOTINTER = 55; // Left.Sym (Left is interface, Right is method name)
        public static readonly var OXDOT = 56; // Left.Sym (before rewrite to one of the preceding)
        public static readonly var ODOTTYPE = 57; // Left.Right or Left.Type (.Right during parsing, .Type once resolved); after walk, .Right contains address of interface type descriptor and .Right.Right contains address of concrete type descriptor
        public static readonly var ODOTTYPE2 = 58; // Left.Right or Left.Type (.Right during parsing, .Type once resolved; on rhs of OAS2DOTTYPE); after walk, .Right contains address of interface type descriptor
        public static readonly var OEQ = 59; // Left == Right
        public static readonly var ONE = 60; // Left != Right
        public static readonly var OLT = 61; // Left < Right
        public static readonly var OLE = 62; // Left <= Right
        public static readonly var OGE = 63; // Left >= Right
        public static readonly var OGT = 64; // Left > Right
        public static readonly var OIND = 65; // *Left
        public static readonly var OINDEX = 66; // Left[Right] (index of array or slice)
        public static readonly var OINDEXMAP = 67; // Left[Right] (index of map)
        public static readonly var OKEY = 68; // Left:Right (key:value in struct/array/map literal)
        public static readonly var OSTRUCTKEY = 69; // Sym:Left (key:value in struct literal, after type checking)
        public static readonly var OLEN = 70; // len(Left)
        public static readonly var OMAKE = 71; // make(List) (before type checking converts to one of the following)
        public static readonly var OMAKECHAN = 72; // make(Type, Left) (type is chan)
        public static readonly var OMAKEMAP = 73; // make(Type, Left) (type is map)
        public static readonly var OMAKESLICE = 74; // make(Type, Left, Right) (type is slice)
        public static readonly var OMUL = 75; // Left * Right
        public static readonly var ODIV = 76; // Left / Right
        public static readonly var OMOD = 77; // Left % Right
        public static readonly var OLSH = 78; // Left << Right
        public static readonly var ORSH = 79; // Left >> Right
        public static readonly var OAND = 80; // Left & Right
        public static readonly var OANDNOT = 81; // Left &^ Right
        public static readonly var ONEW = 82; // new(Left)
        public static readonly var ONOT = 83; // !Left
        public static readonly var OCOM = 84; // ^Left
        public static readonly var OPLUS = 85; // +Left
        public static readonly var OMINUS = 86; // -Left
        public static readonly var OOROR = 87; // Left || Right
        public static readonly var OPANIC = 88; // panic(Left)
        public static readonly var OPRINT = 89; // print(List)
        public static readonly var OPRINTN = 90; // println(List)
        public static readonly var OPAREN = 91; // (Left)
        public static readonly var OSEND = 92; // Left <- Right
        public static readonly var OSLICE = 93; // Left[List[0] : List[1]] (Left is untypechecked or slice)
        public static readonly var OSLICEARR = 94; // Left[List[0] : List[1]] (Left is array)
        public static readonly var OSLICESTR = 95; // Left[List[0] : List[1]] (Left is string)
        public static readonly var OSLICE3 = 96; // Left[List[0] : List[1] : List[2]] (Left is untypedchecked or slice)
        public static readonly var OSLICE3ARR = 97; // Left[List[0] : List[1] : List[2]] (Left is array)
        public static readonly var ORECOVER = 98; // recover()
        public static readonly var ORECV = 99; // <-Left
        public static readonly var ORUNESTR = 100; // Type(Left) (Type is string, Left is rune)
        public static readonly var OSELRECV = 101; // Left = <-Right.Left: (appears as .Left of OCASE; Right.Op == ORECV)
        public static readonly var OSELRECV2 = 102; // List = <-Right.Left: (apperas as .Left of OCASE; count(List) == 2, Right.Op == ORECV)
        public static readonly var OIOTA = 103; // iota
        public static readonly var OREAL = 104; // real(Left)
        public static readonly var OIMAG = 105; // imag(Left)
        public static readonly var OCOMPLEX = 106; // complex(Left, Right)
        public static readonly var OALIGNOF = 107; // unsafe.Alignof(Left)
        public static readonly var OOFFSETOF = 108; // unsafe.Offsetof(Left)
        public static readonly var OSIZEOF = 109; // unsafe.Sizeof(Left)

        // statements
        public static readonly var OBLOCK = 110; // { List } (block of code)
        public static readonly var OBREAK = 111; // break
        public static readonly var OCASE = 112; // case Left or List[0]..List[1]: Nbody (select case after processing; Left==nil and List==nil means default)
        public static readonly var OXCASE = 113; // case List: Nbody (select case before processing; List==nil means default)
        public static readonly var OCONTINUE = 114; // continue
        public static readonly var ODEFER = 115; // defer Left (Left must be call)
        public static readonly var OEMPTY = 116; // no-op (empty statement)
        public static readonly var OFALL = 117; // fallthrough
        public static readonly var OFOR = 118; // for Ninit; Left; Right { Nbody }
        public static readonly var OFORUNTIL = 119; // for Ninit; Left; Right { Nbody } ; test applied after executing body, not before
        public static readonly var OGOTO = 120; // goto Left
        public static readonly var OIF = 121; // if Ninit; Left { Nbody } else { Rlist }
        public static readonly var OLABEL = 122; // Left:
        public static readonly var OPROC = 123; // go Left (Left must be call)
        public static readonly var ORANGE = 124; // for List = range Right { Nbody }
        public static readonly var ORETURN = 125; // return List
        public static readonly var OSELECT = 126; // select { List } (List is list of OXCASE or OCASE)
        public static readonly var OSWITCH = 127; // switch Ninit; Left { List } (List is a list of OXCASE or OCASE)
        public static readonly var OTYPESW = 128; // Left = Right.(type) (appears as .Left of OSWITCH)

        // types
        public static readonly var OTCHAN = 129; // chan int
        public static readonly var OTMAP = 130; // map[string]int
        public static readonly var OTSTRUCT = 131; // struct{}
        public static readonly var OTINTER = 132; // interface{}
        public static readonly var OTFUNC = 133; // func()
        public static readonly var OTARRAY = 134; // []int, [8]int, [N]int or [...]int

        // misc
        public static readonly var ODDD = 135; // func f(args ...int) or f(l...) or var a = [...]int{0, 1, 2}.
        public static readonly var ODDDARG = 136; // func f(args ...int), introduced by escape analysis.
        public static readonly var OINLCALL = 137; // intermediary representation of an inlined call.
        public static readonly var OEFACE = 138; // itable and data words of an empty-interface value.
        public static readonly var OITAB = 139; // itable word of an interface value.
        public static readonly var OIDATA = 140; // data word of an interface value in Left
        public static readonly var OSPTR = 141; // base pointer of a slice or string.
        public static readonly var OCLOSUREVAR = 142; // variable reference at beginning of closure function
        public static readonly var OCFUNC = 143; // reference to c function pointer (not go func value)
        public static readonly var OCHECKNIL = 144; // emit code to ensure pointer/interface not nil
        public static readonly var OVARKILL = 145; // variable is dead
        public static readonly var OVARLIVE = 146; // variable is alive
        public static readonly var OINDREGSP = 147; // offset plus indirect of REGSP, such as 8(SP).

        // arch-specific opcodes
        public static readonly var ORETJMP = 148; // return to other function
        public static readonly var OGETG = 149; // runtime.getg() (read g pointer)

        public static readonly var OEND = 150;

        // Nodes is a pointer to a slice of *Node.
        // For fields that are not used in most nodes, this is used instead of
        // a slice to save space.
        public partial struct Nodes
        {
            public ref slice<ref Node> slice;
        }

        // Slice returns the entries in Nodes as a slice.
        // Changes to the slice entries (as in s[i] = n) will be reflected in
        // the Nodes.
        public static slice<ref Node> Slice(this Nodes n)
        {
            if (n.slice == null)
            {
                return null;
            }
            return n.slice.Value;
        }

        // Len returns the number of entries in Nodes.
        public static long Len(this Nodes n)
        {
            if (n.slice == null)
            {
                return 0L;
            }
            return len(n.slice.Value);
        }

        // Index returns the i'th element of Nodes.
        // It panics if n does not have at least i+1 elements.
        public static ref Node Index(this Nodes n, long i)
        {
            return (n.slice.Value)[i];
        }

        // First returns the first element of Nodes (same as n.Index(0)).
        // It panics if n has no elements.
        public static ref Node First(this Nodes n)
        {
            return (n.slice.Value)[0L];
        }

        // Second returns the second element of Nodes (same as n.Index(1)).
        // It panics if n has fewer than two elements.
        public static ref Node Second(this Nodes n)
        {
            return (n.slice.Value)[1L];
        }

        // Set sets n to a slice.
        // This takes ownership of the slice.
        private static void Set(this ref Nodes n, slice<ref Node> s)
        {
            if (len(s) == 0L)
            {
                n.slice = null;
            }
            else
            { 
                // Copy s and take address of t rather than s to avoid
                // allocation in the case where len(s) == 0 (which is
                // over 3x more common, dynamically, for make.bash).
                var t = s;
                n.slice = ref t;
            }
        }

        // Set1 sets n to a slice containing a single node.
        private static void Set1(this ref Nodes n, ref Node n1)
        {
            n.slice = ref new slice<ref Node>(new ref Node[] { n1 });
        }

        // Set2 sets n to a slice containing two nodes.
        private static void Set2(this ref Nodes n, ref Node n1, ref Node n2)
        {
            n.slice = ref new slice<ref Node>(new ref Node[] { n1, n2 });
        }

        // Set3 sets n to a slice containing three nodes.
        private static void Set3(this ref Nodes n, ref Node n1, ref Node n2, ref Node n3)
        {
            n.slice = ref new slice<ref Node>(new ref Node[] { n1, n2, n3 });
        }

        // MoveNodes sets n to the contents of n2, then clears n2.
        private static void MoveNodes(this ref Nodes n, ref Nodes n2)
        {
            n.slice = n2.slice;
            n2.slice = null;
        }

        // SetIndex sets the i'th element of Nodes to node.
        // It panics if n does not have at least i+1 elements.
        public static void SetIndex(this Nodes n, long i, ref Node node)
        {
            (n.slice.Value)[i] = node;
        }

        // SetFirst sets the first element of Nodes to node.
        // It panics if n does not have at least one elements.
        public static void SetFirst(this Nodes n, ref Node node)
        {
            (n.slice.Value)[0L] = node;
        }

        // SetSecond sets the second element of Nodes to node.
        // It panics if n does not have at least two elements.
        public static void SetSecond(this Nodes n, ref Node node)
        {
            (n.slice.Value)[1L] = node;
        }

        // Addr returns the address of the i'th element of Nodes.
        // It panics if n does not have at least i+1 elements.
        public static ptr<ptr<Node>> Addr(this Nodes n, long i)
        {
            return ref (n.slice.Value)[i];
        }

        // Append appends entries to Nodes.
        private static void Append(this ref Nodes n, params ptr<Node>[] a)
        {
            if (len(a) == 0L)
            {
                return;
            }
            if (n.slice == null)
            {
                var s = make_slice<ref Node>(len(a));
                copy(s, a);
                n.slice = ref s;
                return;
            }
            n.slice.Value = append(n.slice.Value, a);
        }

        // Prepend prepends entries to Nodes.
        // If a slice is passed in, this will take ownership of it.
        private static void Prepend(this ref Nodes n, params ptr<Node>[] a)
        {
            if (len(a) == 0L)
            {
                return;
            }
            if (n.slice == null)
            {
                n.slice = ref a;
            }
            else
            {
                n.slice.Value = append(a, n.slice.Value);
            }
        }

        // AppendNodes appends the contents of *n2 to n, then clears n2.
        private static void AppendNodes(this ref Nodes n, ref Nodes n2)
        {

            if (n2.slice == null)             else if (n.slice == null) 
                n.slice = n2.slice;
            else 
                n.slice.Value = append(n.slice.Value, n2.slice.Value);
                        n2.slice = null;
        }

        // inspect invokes f on each node in an AST in depth-first order.
        // If f(n) returns false, inspect skips visiting n's children.
        private static bool inspect(ref Node n, Func<ref Node, bool> f)
        {
            if (n == null || !f(n))
            {
                return;
            }
            inspectList(n.Ninit, f);
            inspect(n.Left, f);
            inspect(n.Right, f);
            inspectList(n.List, f);
            inspectList(n.Nbody, f);
            inspectList(n.Rlist, f);
        }

        private static bool inspectList(Nodes l, Func<ref Node, bool> f)
        {
            foreach (var (_, n) in l.Slice())
            {
                inspect(n, f);
            }
        }

        // nodeQueue is a FIFO queue of *Node. The zero value of nodeQueue is
        // a ready-to-use empty queue.
        private partial struct nodeQueue
        {
            public slice<ref Node> ring;
            public long head;
            public long tail;
        }

        // empty returns true if q contains no Nodes.
        private static bool empty(this ref nodeQueue q)
        {
            return q.head == q.tail;
        }

        // pushRight appends n to the right of the queue.
        private static void pushRight(this ref nodeQueue q, ref Node n)
        {
            if (len(q.ring) == 0L)
            {
                q.ring = make_slice<ref Node>(16L);
            }
            else if (q.head + len(q.ring) == q.tail)
            { 
                // Grow the ring.
                var nring = make_slice<ref Node>(len(q.ring) * 2L); 
                // Copy the old elements.
                var part = q.ring[q.head % len(q.ring)..];
                if (q.tail - q.head <= len(part))
                {
                    part = part[..q.tail - q.head];
                    copy(nring, part);
                }
                else
                {
                    var pos = copy(nring, part);
                    copy(nring[pos..], q.ring[..q.tail % len(q.ring)]);
                }
                q.ring = nring;
                q.head = 0L;
                q.tail = q.tail - q.head;
            }
            q.ring[q.tail % len(q.ring)] = n;
            q.tail++;
        }

        // popLeft pops a node from the left of the queue. It panics if q is
        // empty.
        private static ref Node popLeft(this ref nodeQueue _q) => func(_q, (ref nodeQueue q, Defer _, Panic panic, Recover __) =>
        {
            if (q.empty())
            {
                panic("dequeue empty");
            }
            var n = q.ring[q.head % len(q.ring)];
            q.head++;
            return n;
        });
    }
}}}}
