// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// “Abstract” syntax representation.

// package gc -- go2cs converted at 2020 October 09 05:43:23 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\syntax.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sort = go.sort_package;
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
            public byte aux;
        }

        private static void ResetAux(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            n.aux = 0L;
        }

        private static Op SubOp(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OASOP || n.Op == ONAME)             else 
                Fatalf("unexpected op: %v", n.Op);
                        return Op(n.aux);

        }

        private static void SetSubOp(this ptr<Node> _addr_n, Op op)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OASOP || n.Op == ONAME)             else 
                Fatalf("unexpected op: %v", n.Op);
                        n.aux = uint8(op);

        }

        private static bool IndexMapLValue(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OINDEXMAP)
            {
                Fatalf("unexpected op: %v", n.Op);
            }

            return n.aux != 0L;

        }

        private static void SetIndexMapLValue(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OINDEXMAP)
            {
                Fatalf("unexpected op: %v", n.Op);
            }

            if (b)
            {
                n.aux = 1L;
            }
            else
            {
                n.aux = 0L;
            }

        }

        private static types.ChanDir TChanDir(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OTCHAN)
            {
                Fatalf("unexpected op: %v", n.Op);
            }

            return types.ChanDir(n.aux);

        }

        private static void SetTChanDir(this ptr<Node> _addr_n, types.ChanDir dir)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OTCHAN)
            {
                Fatalf("unexpected op: %v", n.Op);
            }

            n.aux = uint8(dir);

        }

        private static bool IsSynthetic(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var name = n.Sym.Name;
            return name[0L] == '.' || name[0L] == '~';
        }

        // IsAutoTmp indicates if n was created by the compiler as a temporary,
        // based on the setting of the .AutoTemp flag in n's Name.
        private static bool IsAutoTmp(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || n.Op != ONAME)
            {
                return false;
            }

            return n.Name.AutoTemp();

        }

        private static readonly var nodeClass = iota;
        private static readonly long _ = (long)1L << (int)(iota); // PPARAM, PAUTO, PEXTERN, etc; three bits; first in the list because frequently accessed
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
        private static readonly var nodeNoInline = 19; // used internally by inliner to indicate that a function call should not be inlined; set for OCALLFUNC and OCALLMETH only
        private static readonly var _ = 20;
        private static readonly var nodeImplicit = 21; // implicit OADDR or ODEREF; ++/-- statement represented as OASOP; or ANDNOT lowered to OAND
        private static readonly var _ = 22;
        private static readonly var nodeIsDDD = 23; // is the argument variadic
        private static readonly var _ = 24;
        private static readonly var nodeDiag = 25; // already printed error about this
        private static readonly var _ = 26;
        private static readonly var nodeColas = 27; // OAS resulting from :=
        private static readonly var _ = 28;
        private static readonly var nodeNonNil = 29; // guaranteed to be non-nil
        private static readonly var _ = 30;
        private static readonly var nodeTransient = 31; // storage can be reused immediately after this statement
        private static readonly var _ = 32;
        private static readonly var nodeBounded = 33; // bounds check unnecessary
        private static readonly var _ = 34;
        private static readonly var nodeHasCall = 35; // expression contains a function call
        private static readonly var _ = 36;
        private static readonly var nodeLikely = 37; // if statement condition likely
        private static readonly var _ = 38;
        private static readonly var nodeHasVal = 39; // node.E contains a Val
        private static readonly var _ = 40;
        private static readonly var nodeHasOpt = 41; // node.E contains an Opt
        private static readonly var _ = 42;
        private static readonly var nodeEmbedded = 43; // ODCLFIELD embedded type

        private static Class Class(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return Class(n.flags.get3(nodeClass));
        }
        private static byte Walkdef(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags.get2(nodeWalkdef);
        }
        private static byte Typecheck(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags.get2(nodeTypecheck);
        }
        private static byte Initorder(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags.get2(nodeInitorder);
        }

        private static bool HasBreak(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeHasBreak != 0L;
        }
        private static bool NoInline(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeNoInline != 0L;
        }
        private static bool Implicit(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeImplicit != 0L;
        }
        private static bool IsDDD(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeIsDDD != 0L;
        }
        private static bool Diag(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeDiag != 0L;
        }
        private static bool Colas(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeColas != 0L;
        }
        private static bool NonNil(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeNonNil != 0L;
        }
        private static bool Transient(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeTransient != 0L;
        }
        private static bool Bounded(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeBounded != 0L;
        }
        private static bool HasCall(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeHasCall != 0L;
        }
        private static bool Likely(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeLikely != 0L;
        }
        private static bool HasVal(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeHasVal != 0L;
        }
        private static bool HasOpt(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeHasOpt != 0L;
        }
        private static bool Embedded(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.flags & nodeEmbedded != 0L;
        }

        private static void SetClass(this ptr<Node> _addr_n, Class b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set3(nodeClass, uint8(b));
        }
        private static void SetWalkdef(this ptr<Node> _addr_n, byte b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set2(nodeWalkdef, b);
        }
        private static void SetTypecheck(this ptr<Node> _addr_n, byte b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set2(nodeTypecheck, b);
        }
        private static void SetInitorder(this ptr<Node> _addr_n, byte b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set2(nodeInitorder, b);
        }

        private static void SetHasBreak(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeHasBreak, b);
        }
        private static void SetNoInline(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeNoInline, b);
        }
        private static void SetImplicit(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeImplicit, b);
        }
        private static void SetIsDDD(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeIsDDD, b);
        }
        private static void SetDiag(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeDiag, b);
        }
        private static void SetColas(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeColas, b);
        }
        private static void SetTransient(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeTransient, b);
        }
        private static void SetHasCall(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeHasCall, b);
        }
        private static void SetLikely(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeLikely, b);
        }
        private static void SetHasVal(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeHasVal, b);
        }
        private static void SetHasOpt(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeHasOpt, b);
        }
        private static void SetEmbedded(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;

            n.flags.set(nodeEmbedded, b);
        }

        // MarkNonNil marks a pointer n as being guaranteed non-nil,
        // on all code paths, at all times.
        // During conversion to SSA, non-nil pointers won't have nil checks
        // inserted before dereferencing. See state.exprPtr.
        private static void MarkNonNil(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!n.Type.IsPtr() && !n.Type.IsUnsafePtr())
            {
                Fatalf("MarkNonNil(%v), type %v", n, n.Type);
            }

            n.flags.set(nodeNonNil, true);

        }

        // SetBounded indicates whether operation n does not need safety checks.
        // When n is an index or slice operation, n does not need bounds checks.
        // When n is a dereferencing operation, n does not need nil checks.
        // When n is a makeslice+copy operation, n does not need length and cap checks.
        private static void SetBounded(this ptr<Node> _addr_n, bool b)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OINDEX || n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR || n.Op == OSLICESTR)             else if (n.Op == ODOTPTR || n.Op == ODEREF)             else if (n.Op == OMAKESLICECOPY)             else 
                Fatalf("SetBounded(%v)", n);
                        n.flags.set(nodeBounded, b);

        }

        // MarkReadonly indicates that n is an ONAME with readonly contents.
        private static void MarkReadonly(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != ONAME)
            {
                Fatalf("Node.MarkReadonly %v", n.Op);
            }

            n.Name.SetReadonly(true); 
            // Mark the linksym as readonly immediately
            // so that the SSA backend can use this information.
            // It will be overridden later during dumpglobls.
            n.Sym.Linksym().Type = objabi.SRODATA;

        }

        // Val returns the Val for the node.
        private static Val Val(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!n.HasVal())
            {
                return new Val();
            }

            return new Val(n.E);

        }

        // SetVal sets the Val for the node, which must not have been used with SetOpt.
        private static void SetVal(this ptr<Node> _addr_n, Val v)
        {
            ref Node n = ref _addr_n.val;

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
        private static void Opt(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!n.HasOpt())
            {
                return null;
            }

            return n.E;

        }

        // SetOpt sets the optimizer data for the node, which must not have been used with SetVal.
        // SetOpt(nil) is ignored for Vals to simplify call sites that are clearing Opts.
        private static void SetOpt(this ptr<Node> _addr_n, object x)
        {
            ref Node n = ref _addr_n.val;

            if (x == null && n.HasVal())
            {
                return ;
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

        private static long Iota(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Xoffset;
        }

        private static void SetIota(this ptr<Node> _addr_n, long x)
        {
            ref Node n = ref _addr_n.val;

            n.Xoffset = x;
        }

        // mayBeShared reports whether n may occur in multiple places in the AST.
        // Extra care must be taken when mutating such a node.
        private static bool mayBeShared(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == ONAME || n.Op == OLITERAL || n.Op == OTYPE) 
                return true;
                        return false;

        }

        // isMethodExpression reports whether n represents a method expression T.M.
        private static bool isMethodExpression(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == ONAME && n.Left != null && n.Left.Op == OTYPE && n.Right != null && n.Right.Op == ONAME;
        }

        // funcname returns the name (without the package) of the function n.
        private static @string funcname(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || n.Func == null || n.Func.Nname == null)
            {
                return "<nil>";
            }

            return n.Func.Nname.Sym.Name;

        }

        // pkgFuncName returns the name of the function referenced by n, with package prepended.
        // This differs from the compiler's internal convention where local functions lack a package
        // because the ultimate consumer of this is a human looking at an IDE; package is only empty
        // if the compilation package is actually the empty string.
        private static @string pkgFuncName(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            ptr<types.Sym> s;
            if (n == null)
            {
                return "<nil>";
            }

            if (n.Op == ONAME)
            {
                s = n.Sym;
            }
            else
            {
                if (n.Func == null || n.Func.Nname == null)
                {
                    return "<nil>";
                }

                s = n.Func.Nname.Sym;

            }

            var pkg = s.Pkg;

            var p = myimportpath;
            if (pkg != null && pkg.Path != "")
            {
                p = pkg.Path;
            }

            if (p == "")
            {
                return s.Name;
            }

            return p + "." + s.Name;

        }

        // The compiler needs *Node to be assignable to cmd/compile/internal/ssa.Sym.
        private static void CanBeAnSSASym(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

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
            public bitset16 flags;
        }

        private static readonly long nameCaptured = (long)1L << (int)(iota); // is the variable captured by a closure
        private static readonly var nameReadonly = 0;
        private static readonly var nameByval = 1; // is the variable captured by value or by reference
        private static readonly var nameNeedzero = 2; // if it contains pointers, needs to be zeroed on function entry
        private static readonly var nameKeepalive = 3; // mark value live across unknown assembly call
        private static readonly var nameAutoTemp = 4; // is the variable a temporary (implies no dwarf info. reset if escapes to heap)
        private static readonly var nameUsed = 5; // for variable declared and not used error
        private static readonly var nameIsClosureVar = 6; // PAUTOHEAP closure pseudo-variable; original at n.Name.Defn
        private static readonly var nameIsOutputParamHeapAddr = 7; // pointer to a result parameter's heap copy
        private static readonly var nameAssigned = 8; // is the variable ever assigned to
        private static readonly var nameAddrtaken = 9; // address taken, even if not moved to heap
        private static readonly var nameInlFormal = 10; // PAUTO created by inliner, derived from callee formal
        private static readonly var nameInlLocal = 11; // PAUTO created by inliner, derived from callee local
        private static readonly var nameOpenDeferSlot = 12; // if temporary var storing info for open-coded defers
        private static readonly var nameLibfuzzerExtraCounter = 13; // if PEXTERN should be assigned to __libfuzzer_extra_counters section

        private static bool Captured(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameCaptured != 0L;
        }
        private static bool Readonly(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameReadonly != 0L;
        }
        private static bool Byval(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameByval != 0L;
        }
        private static bool Needzero(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameNeedzero != 0L;
        }
        private static bool Keepalive(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameKeepalive != 0L;
        }
        private static bool AutoTemp(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameAutoTemp != 0L;
        }
        private static bool Used(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameUsed != 0L;
        }
        private static bool IsClosureVar(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameIsClosureVar != 0L;
        }
        private static bool IsOutputParamHeapAddr(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameIsOutputParamHeapAddr != 0L;
        }
        private static bool Assigned(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameAssigned != 0L;
        }
        private static bool Addrtaken(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameAddrtaken != 0L;
        }
        private static bool InlFormal(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameInlFormal != 0L;
        }
        private static bool InlLocal(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameInlLocal != 0L;
        }
        private static bool OpenDeferSlot(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameOpenDeferSlot != 0L;
        }
        private static bool LibfuzzerExtraCounter(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.flags & nameLibfuzzerExtraCounter != 0L;
        }

        private static void SetCaptured(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameCaptured, b);
        }
        private static void SetReadonly(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameReadonly, b);
        }
        private static void SetByval(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameByval, b);
        }
        private static void SetNeedzero(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameNeedzero, b);
        }
        private static void SetKeepalive(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameKeepalive, b);
        }
        private static void SetAutoTemp(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameAutoTemp, b);
        }
        private static void SetUsed(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameUsed, b);
        }
        private static void SetIsClosureVar(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameIsClosureVar, b);
        }
        private static void SetIsOutputParamHeapAddr(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameIsOutputParamHeapAddr, b);
        }
        private static void SetAssigned(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameAssigned, b);
        }
        private static void SetAddrtaken(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameAddrtaken, b);
        }
        private static void SetInlFormal(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameInlFormal, b);
        }
        private static void SetInlLocal(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameInlLocal, b);
        }
        private static void SetOpenDeferSlot(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameOpenDeferSlot, b);
        }
        private static void SetLibfuzzerExtraCounter(this ptr<Name> _addr_n, bool b)
        {
            ref Name n = ref _addr_n.val;

            n.flags.set(nameLibfuzzerExtraCounter, b);
        }

        public partial struct Param
        {
            public ptr<Node> Ntype;
            public ptr<Node> Heapaddr; // temp holding heap address of param

// ONAME PAUTOHEAP
            public ptr<Node> Stackcopy; // the PPARAM/PPARAMOUT on-stack slot (moved func params only)

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
            public PragmaFlag Pragma;
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
            public slice<ptr<Node>> Dcl; // autodcl for this func/closure

// Parents records the parent scope of each scope within a
// function. The root scope (0) has no parent, so the i'th
// scope's parent is stored at Parents[i-1].
            public slice<ScopeID> Parents; // Marks records scope boundary changes.
            public slice<Mark> Marks; // Closgen tracks how many closures have been generated within
// this function. Used by closurename for creating unique
// function names.
            public long Closgen;
            public ptr<ssa.FuncDebug> DebugInfo;
            public ptr<Node> Ntype; // signature
            public long Top; // top context (ctxCallee, etc)
            public ptr<Node> Closure; // OCLOSURE <-> ODCLFUNC
            public ptr<Node> Nname;
            public ptr<obj.LSym> lsym;
            public ptr<Inline> Inl;
            public int Label; // largest auto-generated label in this function

            public src.XPos Endlineno;
            public src.XPos WBPos; // position of first write barrier; see SetWBPos

            public PragmaFlag Pragma; // go:xxx function annotations

            public bitset16 flags;
            public long numDefers; // number of defer calls in the function
            public long numReturns; // number of explicit returns in the function

// nwbrCalls records the LSyms of functions called by this
// function for go:nowritebarrierrec analysis. Only filled in
// if nowritebarrierrecCheck != nil.
            public ptr<slice<nowritebarrierrecCallSym>> nwbrCalls;
        }

        // An Inline holds fields used for function bodies that can be inlined.
        public partial struct Inline
        {
            public int Cost; // heuristic cost of inlining this function

// Copies of Func.Dcl and Nbody for use during inlining.
            public slice<ptr<Node>> Dcl;
            public slice<ptr<Node>> Body;
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

        private static readonly long funcDupok = (long)1L << (int)(iota); // duplicate definitions ok
        private static readonly var funcWrapper = 0; // is method wrapper
        private static readonly var funcNeedctxt = 1; // function uses context register (has closure variables)
        private static readonly var funcReflectMethod = 2; // function calls reflect.Type.Method or MethodByName
        private static readonly var funcIsHiddenClosure = 3;
        private static readonly var funcHasDefer = 4; // contains a defer statement
        private static readonly var funcNilCheckDisabled = 5; // disable nil checks when compiling this function
        private static readonly var funcInlinabilityChecked = 6; // inliner has already determined whether the function is inlinable
        private static readonly var funcExportInline = 7; // include inline body in export data
        private static readonly var funcInstrumentBody = 8; // add race/msan instrumentation during SSA construction
        private static readonly var funcOpenCodedDeferDisallowed = 9; // can't do open-coded defers

        private static bool Dupok(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcDupok != 0L;
        }
        private static bool Wrapper(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcWrapper != 0L;
        }
        private static bool Needctxt(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcNeedctxt != 0L;
        }
        private static bool ReflectMethod(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcReflectMethod != 0L;
        }
        private static bool IsHiddenClosure(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcIsHiddenClosure != 0L;
        }
        private static bool HasDefer(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcHasDefer != 0L;
        }
        private static bool NilCheckDisabled(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcNilCheckDisabled != 0L;
        }
        private static bool InlinabilityChecked(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcInlinabilityChecked != 0L;
        }
        private static bool ExportInline(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcExportInline != 0L;
        }
        private static bool InstrumentBody(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcInstrumentBody != 0L;
        }
        private static bool OpenCodedDeferDisallowed(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.flags & funcOpenCodedDeferDisallowed != 0L;
        }

        private static void SetDupok(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcDupok, b);
        }
        private static void SetWrapper(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcWrapper, b);
        }
        private static void SetNeedctxt(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcNeedctxt, b);
        }
        private static void SetReflectMethod(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcReflectMethod, b);
        }
        private static void SetIsHiddenClosure(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcIsHiddenClosure, b);
        }
        private static void SetHasDefer(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcHasDefer, b);
        }
        private static void SetNilCheckDisabled(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcNilCheckDisabled, b);
        }
        private static void SetInlinabilityChecked(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcInlinabilityChecked, b);
        }
        private static void SetExportInline(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcExportInline, b);
        }
        private static void SetInstrumentBody(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcInstrumentBody, b);
        }
        private static void SetOpenCodedDeferDisallowed(this ptr<Func> _addr_f, bool b)
        {
            ref Func f = ref _addr_f.val;

            f.flags.set(funcOpenCodedDeferDisallowed, b);
        }

        private static void setWBPos(this ptr<Func> _addr_f, src.XPos pos)
        {
            ref Func f = ref _addr_f.val;

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
        public static readonly Op OXXX = (Op)iota; 

        // names
        public static readonly var ONAME = 0; // var or func name
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
        public static readonly var OBYTES2STR = 13; // Type(Left) (Type is string, Left is a []byte)
        public static readonly var OBYTES2STRTMP = 14; // Type(Left) (Type is string, Left is a []byte, ephemeral)
        public static readonly var ORUNES2STR = 15; // Type(Left) (Type is string, Left is a []rune)
        public static readonly var OSTR2BYTES = 16; // Type(Left) (Type is []byte, Left is a string)
        public static readonly var OSTR2BYTESTMP = 17; // Type(Left) (Type is []byte, Left is a string, ephemeral)
        public static readonly var OSTR2RUNES = 18; // Type(Left) (Type is []rune, Left is a string)
        public static readonly var OAS = 19; // Left = Right or (if Colas=true) Left := Right
        public static readonly var OAS2 = 20; // List = Rlist (x, y, z = a, b, c)
        public static readonly var OAS2DOTTYPE = 21; // List = Right (x, ok = I.(int))
        public static readonly var OAS2FUNC = 22; // List = Right (x, y = f())
        public static readonly var OAS2MAPR = 23; // List = Right (x, ok = m["foo"])
        public static readonly var OAS2RECV = 24; // List = Right (x, ok = <-c)
        public static readonly var OASOP = 25; // Left Etype= Right (x += y)
        public static readonly var OCALL = 26; // Left(List) (function call, method call or type conversion)

        // OCALLFUNC, OCALLMETH, and OCALLINTER have the same structure.
        // Prior to walk, they are: Left(List), where List is all regular arguments.
        // After walk, List is a series of assignments to temporaries,
        // and Rlist is an updated set of arguments.
        // TODO(josharian/khr): Use Ninit instead of List for the assignments to temporaries. See CL 114797.
        public static readonly var OCALLFUNC = 27; // Left(List/Rlist) (function call f(args))
        public static readonly var OCALLMETH = 28; // Left(List/Rlist) (direct method call x.Method(args))
        public static readonly var OCALLINTER = 29; // Left(List/Rlist) (interface method call x.Method(args))
        public static readonly var OCALLPART = 30; // Left.Right (method expression x.Method, not called)
        public static readonly var OCAP = 31; // cap(Left)
        public static readonly var OCLOSE = 32; // close(Left)
        public static readonly var OCLOSURE = 33; // func Type { Body } (func literal)
        public static readonly var OCOMPLIT = 34; // Right{List} (composite literal, not yet lowered to specific form)
        public static readonly var OMAPLIT = 35; // Type{List} (composite literal, Type is map)
        public static readonly var OSTRUCTLIT = 36; // Type{List} (composite literal, Type is struct)
        public static readonly var OARRAYLIT = 37; // Type{List} (composite literal, Type is array)
        public static readonly var OSLICELIT = 38; // Type{List} (composite literal, Type is slice) Right.Int64() = slice length.
        public static readonly var OPTRLIT = 39; // &Left (left is composite literal)
        public static readonly var OCONV = 40; // Type(Left) (type conversion)
        public static readonly var OCONVIFACE = 41; // Type(Left) (type conversion, to interface)
        public static readonly var OCONVNOP = 42; // Type(Left) (type conversion, no effect)
        public static readonly var OCOPY = 43; // copy(Left, Right)
        public static readonly var ODCL = 44; // var Left (declares Left of type Left.Type)

        // Used during parsing but don't last.
        public static readonly var ODCLFUNC = 45; // func f() or func (r) f()
        public static readonly var ODCLFIELD = 46; // struct field, interface field, or func/method argument/return value.
        public static readonly var ODCLCONST = 47; // const pi = 3.14
        public static readonly var ODCLTYPE = 48; // type Int int or type Int = int

        public static readonly var ODELETE = 49; // delete(Left, Right)
        public static readonly var ODOT = 50; // Left.Sym (Left is of struct type)
        public static readonly var ODOTPTR = 51; // Left.Sym (Left is of pointer to struct type)
        public static readonly var ODOTMETH = 52; // Left.Sym (Left is non-interface, Right is method name)
        public static readonly var ODOTINTER = 53; // Left.Sym (Left is interface, Right is method name)
        public static readonly var OXDOT = 54; // Left.Sym (before rewrite to one of the preceding)
        public static readonly var ODOTTYPE = 55; // Left.Right or Left.Type (.Right during parsing, .Type once resolved); after walk, .Right contains address of interface type descriptor and .Right.Right contains address of concrete type descriptor
        public static readonly var ODOTTYPE2 = 56; // Left.Right or Left.Type (.Right during parsing, .Type once resolved; on rhs of OAS2DOTTYPE); after walk, .Right contains address of interface type descriptor
        public static readonly var OEQ = 57; // Left == Right
        public static readonly var ONE = 58; // Left != Right
        public static readonly var OLT = 59; // Left < Right
        public static readonly var OLE = 60; // Left <= Right
        public static readonly var OGE = 61; // Left >= Right
        public static readonly var OGT = 62; // Left > Right
        public static readonly var ODEREF = 63; // *Left
        public static readonly var OINDEX = 64; // Left[Right] (index of array or slice)
        public static readonly var OINDEXMAP = 65; // Left[Right] (index of map)
        public static readonly var OKEY = 66; // Left:Right (key:value in struct/array/map literal)
        public static readonly var OSTRUCTKEY = 67; // Sym:Left (key:value in struct literal, after type checking)
        public static readonly var OLEN = 68; // len(Left)
        public static readonly var OMAKE = 69; // make(List) (before type checking converts to one of the following)
        public static readonly var OMAKECHAN = 70; // make(Type, Left) (type is chan)
        public static readonly var OMAKEMAP = 71; // make(Type, Left) (type is map)
        public static readonly var OMAKESLICE = 72; // make(Type, Left, Right) (type is slice)
        public static readonly var OMAKESLICECOPY = 73; // makeslicecopy(Type, Left, Right) (type is slice; Left is length and Right is the copied from slice)
        // OMAKESLICECOPY is created by the order pass and corresponds to:
        //  s = make(Type, Left); copy(s, Right)
        //
        // Bounded can be set on the node when Left == len(Right) is known at compile time.
        //
        // This node is created so the walk pass can optimize this pattern which would
        // otherwise be hard to detect after the order pass.
        public static readonly var OMUL = 74; // Left * Right
        public static readonly var ODIV = 75; // Left / Right
        public static readonly var OMOD = 76; // Left % Right
        public static readonly var OLSH = 77; // Left << Right
        public static readonly var ORSH = 78; // Left >> Right
        public static readonly var OAND = 79; // Left & Right
        public static readonly var OANDNOT = 80; // Left &^ Right
        public static readonly var ONEW = 81; // new(Left); corresponds to calls to new in source code
        public static readonly var ONEWOBJ = 82; // runtime.newobject(n.Type); introduced by walk; Left is type descriptor
        public static readonly var ONOT = 83; // !Left
        public static readonly var OBITNOT = 84; // ^Left
        public static readonly var OPLUS = 85; // +Left
        public static readonly var ONEG = 86; // -Left
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
        public static readonly var OSLICEHEADER = 98; // sliceheader{Left, List[0], List[1]} (Left is unsafe.Pointer, List[0] is length, List[1] is capacity)
        public static readonly var ORECOVER = 99; // recover()
        public static readonly var ORECV = 100; // <-Left
        public static readonly var ORUNESTR = 101; // Type(Left) (Type is string, Left is rune)
        public static readonly var OSELRECV = 102; // Left = <-Right.Left: (appears as .Left of OCASE; Right.Op == ORECV)
        public static readonly var OSELRECV2 = 103; // List = <-Right.Left: (appears as .Left of OCASE; count(List) == 2, Right.Op == ORECV)
        public static readonly var OIOTA = 104; // iota
        public static readonly var OREAL = 105; // real(Left)
        public static readonly var OIMAG = 106; // imag(Left)
        public static readonly var OCOMPLEX = 107; // complex(Left, Right) or complex(List[0]) where List[0] is a 2-result function call
        public static readonly var OALIGNOF = 108; // unsafe.Alignof(Left)
        public static readonly var OOFFSETOF = 109; // unsafe.Offsetof(Left)
        public static readonly var OSIZEOF = 110; // unsafe.Sizeof(Left)

        // statements
        public static readonly var OBLOCK = 111; // { List } (block of code)
        public static readonly var OBREAK = 112; // break [Sym]
        public static readonly var OCASE = 113; // case List: Nbody (List==nil means default)
        public static readonly var OCONTINUE = 114; // continue [Sym]
        public static readonly var ODEFER = 115; // defer Left (Left must be call)
        public static readonly var OEMPTY = 116; // no-op (empty statement)
        public static readonly var OFALL = 117; // fallthrough
        public static readonly var OFOR = 118; // for Ninit; Left; Right { Nbody }
        // OFORUNTIL is like OFOR, but the test (Left) is applied after the body:
        //     Ninit
        //     top: { Nbody }   // Execute the body at least once
        //     cont: Right
        //     if Left {        // And then test the loop condition
        //         List     // Before looping to top, execute List
        //         goto top
        //     }
        // OFORUNTIL is created by walk. There's no way to write this in Go code.
        public static readonly var OFORUNTIL = 119;
        public static readonly var OGOTO = 120; // goto Sym
        public static readonly var OIF = 121; // if Ninit; Left { Nbody } else { Rlist }
        public static readonly var OLABEL = 122; // Sym:
        public static readonly var OGO = 123; // go Left (Left must be call)
        public static readonly var ORANGE = 124; // for List = range Right { Nbody }
        public static readonly var ORETURN = 125; // return List
        public static readonly var OSELECT = 126; // select { List } (List is list of OCASE)
        public static readonly var OSWITCH = 127; // switch Ninit; Left { List } (List is a list of OCASE)
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
        public static readonly var OINLCALL = 136; // intermediary representation of an inlined call.
        public static readonly var OEFACE = 137; // itable and data words of an empty-interface value.
        public static readonly var OITAB = 138; // itable word of an interface value.
        public static readonly var OIDATA = 139; // data word of an interface value in Left
        public static readonly var OSPTR = 140; // base pointer of a slice or string.
        public static readonly var OCLOSUREVAR = 141; // variable reference at beginning of closure function
        public static readonly var OCFUNC = 142; // reference to c function pointer (not go func value)
        public static readonly var OCHECKNIL = 143; // emit code to ensure pointer/interface not nil
        public static readonly var OVARDEF = 144; // variable is about to be fully initialized
        public static readonly var OVARKILL = 145; // variable is dead
        public static readonly var OVARLIVE = 146; // variable is alive
        public static readonly var ORESULT = 147; // result of a function call; Xoffset is stack offset
        public static readonly var OINLMARK = 148; // start of an inlined body, with file/line of caller. Xoffset is an index into the inline tree.

        // arch-specific opcodes
        public static readonly var ORETJMP = 149; // return to other function
        public static readonly var OGETG = 150; // runtime.getg() (read g pointer)

        public static readonly var OEND = 151;


        // Nodes is a pointer to a slice of *Node.
        // For fields that are not used in most nodes, this is used instead of
        // a slice to save space.
        public partial struct Nodes
        {
            public ptr<slice<ptr<Node>>> slice;
        }

        // asNodes returns a slice of *Node as a Nodes value.
        private static Nodes asNodes(slice<ptr<Node>> s)
        {
            return new Nodes(&s);
        }

        // Slice returns the entries in Nodes as a slice.
        // Changes to the slice entries (as in s[i] = n) will be reflected in
        // the Nodes.
        public static slice<ptr<Node>> Slice(this Nodes n)
        {
            if (n.slice == null)
            {
                return null;
            }

            return n.slice.val;

        }

        // Len returns the number of entries in Nodes.
        public static long Len(this Nodes n)
        {
            if (n.slice == null)
            {
                return 0L;
            }

            return len(n.slice.val);

        }

        // Index returns the i'th element of Nodes.
        // It panics if n does not have at least i+1 elements.
        public static ptr<Node> Index(this Nodes n, long i)
        {
            return _addr_(n.slice.val)[i]!;
        }

        // First returns the first element of Nodes (same as n.Index(0)).
        // It panics if n has no elements.
        public static ptr<Node> First(this Nodes n)
        {
            return _addr_(n.slice.val)[0L]!;
        }

        // Second returns the second element of Nodes (same as n.Index(1)).
        // It panics if n has fewer than two elements.
        public static ptr<Node> Second(this Nodes n)
        {
            return _addr_(n.slice.val)[1L]!;
        }

        // Set sets n to a slice.
        // This takes ownership of the slice.
        private static void Set(this ptr<Nodes> _addr_n, slice<ptr<Node>> s)
        {
            ref Nodes n = ref _addr_n.val;

            if (len(s) == 0L)
            {
                n.slice = null;
            }
            else
            { 
                // Copy s and take address of t rather than s to avoid
                // allocation in the case where len(s) == 0 (which is
                // over 3x more common, dynamically, for make.bash).
                ref var t = ref heap(s, out ptr<var> _addr_t);
                _addr_n.slice = _addr_t;
                n.slice = ref _addr_n.slice.val;

            }

        }

        // Set1 sets n to a slice containing a single node.
        private static void Set1(this ptr<Nodes> _addr_n, ptr<Node> _addr_n1)
        {
            ref Nodes n = ref _addr_n.val;
            ref Node n1 = ref _addr_n1.val;

            n.slice = addr(new slice<ptr<Node>>(new ptr<Node>[] { n1 }));
        }

        // Set2 sets n to a slice containing two nodes.
        private static void Set2(this ptr<Nodes> _addr_n, ptr<Node> _addr_n1, ptr<Node> _addr_n2)
        {
            ref Nodes n = ref _addr_n.val;
            ref Node n1 = ref _addr_n1.val;
            ref Node n2 = ref _addr_n2.val;

            n.slice = addr(new slice<ptr<Node>>(new ptr<Node>[] { n1, n2 }));
        }

        // Set3 sets n to a slice containing three nodes.
        private static void Set3(this ptr<Nodes> _addr_n, ptr<Node> _addr_n1, ptr<Node> _addr_n2, ptr<Node> _addr_n3)
        {
            ref Nodes n = ref _addr_n.val;
            ref Node n1 = ref _addr_n1.val;
            ref Node n2 = ref _addr_n2.val;
            ref Node n3 = ref _addr_n3.val;

            n.slice = addr(new slice<ptr<Node>>(new ptr<Node>[] { n1, n2, n3 }));
        }

        // MoveNodes sets n to the contents of n2, then clears n2.
        private static void MoveNodes(this ptr<Nodes> _addr_n, ptr<Nodes> _addr_n2)
        {
            ref Nodes n = ref _addr_n.val;
            ref Nodes n2 = ref _addr_n2.val;

            n.slice = n2.slice;
            n2.slice = null;
        }

        // SetIndex sets the i'th element of Nodes to node.
        // It panics if n does not have at least i+1 elements.
        public static void SetIndex(this Nodes n, long i, ptr<Node> _addr_node)
        {
            ref Node node = ref _addr_node.val;

            (n.slice.val)[i] = node;
        }

        // SetFirst sets the first element of Nodes to node.
        // It panics if n does not have at least one elements.
        public static void SetFirst(this Nodes n, ptr<Node> _addr_node)
        {
            ref Node node = ref _addr_node.val;

            (n.slice.val)[0L] = node;
        }

        // SetSecond sets the second element of Nodes to node.
        // It panics if n does not have at least two elements.
        public static void SetSecond(this Nodes n, ptr<Node> _addr_node)
        {
            ref Node node = ref _addr_node.val;

            (n.slice.val)[1L] = node;
        }

        // Addr returns the address of the i'th element of Nodes.
        // It panics if n does not have at least i+1 elements.
        public static ptr<ptr<Node>> Addr(this Nodes n, long i)
        {
            return _addr__addr_(n.slice.val)[i]!;
        }

        // Append appends entries to Nodes.
        private static void Append(this ptr<Nodes> _addr_n, params ptr<ptr<Node>>[] _addr_a)
        {
            a = a.Clone();
            ref Nodes n = ref _addr_n.val;
            ref Node a = ref _addr_a.val;

            if (len(a) == 0L)
            {
                return ;
            }

            if (n.slice == null)
            {
                ref var s = ref heap(make_slice<ptr<Node>>(len(a)), out ptr<var> _addr_s);
                copy(s, a);
                _addr_n.slice = _addr_s;
                n.slice = ref _addr_n.slice.val;
                return ;

            }

            n.slice.val = append(n.slice.val, a);

        }

        // Prepend prepends entries to Nodes.
        // If a slice is passed in, this will take ownership of it.
        private static void Prepend(this ptr<Nodes> _addr_n, params ptr<ptr<Node>>[] _addr_a)
        {
            a = a.Clone();
            ref Nodes n = ref _addr_n.val;
            ref Node a = ref _addr_a.val;

            if (len(a) == 0L)
            {
                return ;
            }

            if (n.slice == null)
            {
                n.slice = _addr_a;
            }
            else
            {
                n.slice.val = append(a, n.slice.val);
            }

        }

        // AppendNodes appends the contents of *n2 to n, then clears n2.
        private static void AppendNodes(this ptr<Nodes> _addr_n, ptr<Nodes> _addr_n2)
        {
            ref Nodes n = ref _addr_n.val;
            ref Nodes n2 = ref _addr_n2.val;


            if (n2.slice == null)             else if (n.slice == null) 
                n.slice = n2.slice;
            else 
                n.slice.val = append(n.slice.val, n2.slice.val);
                        n2.slice = null;

        }

        // inspect invokes f on each node in an AST in depth-first order.
        // If f(n) returns false, inspect skips visiting n's children.
        private static bool inspect(ptr<Node> _addr_n, Func<ptr<Node>, bool> f)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || !f(n))
            {
                return ;
            }

            inspectList(n.Ninit, f);
            inspect(_addr_n.Left, f);
            inspect(_addr_n.Right, f);
            inspectList(n.List, f);
            inspectList(n.Nbody, f);
            inspectList(n.Rlist, f);

        }

        private static bool inspectList(Nodes l, Func<ptr<Node>, bool> f)
        {
            foreach (var (_, n) in l.Slice())
            {
                inspect(_addr_n, f);
            }

        }

        // nodeQueue is a FIFO queue of *Node. The zero value of nodeQueue is
        // a ready-to-use empty queue.
        private partial struct nodeQueue
        {
            public slice<ptr<Node>> ring;
            public long head;
            public long tail;
        }

        // empty reports whether q contains no Nodes.
        private static bool empty(this ptr<nodeQueue> _addr_q)
        {
            ref nodeQueue q = ref _addr_q.val;

            return q.head == q.tail;
        }

        // pushRight appends n to the right of the queue.
        private static void pushRight(this ptr<nodeQueue> _addr_q, ptr<Node> _addr_n)
        {
            ref nodeQueue q = ref _addr_q.val;
            ref Node n = ref _addr_n.val;

            if (len(q.ring) == 0L)
            {
                q.ring = make_slice<ptr<Node>>(16L);
            }
            else if (q.head + len(q.ring) == q.tail)
            { 
                // Grow the ring.
                var nring = make_slice<ptr<Node>>(len(q.ring) * 2L); 
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
        private static ptr<Node> popLeft(this ptr<nodeQueue> _addr_q) => func((_, panic, __) =>
        {
            ref nodeQueue q = ref _addr_q.val;

            if (q.empty())
            {
                panic("dequeue empty");
            }

            var n = q.ring[q.head % len(q.ring)];
            q.head++;
            return _addr_n!;

        });

        // NodeSet is a set of Nodes.
        public static bool Has(this NodeSet s, ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var (_, isPresent) = s[n];
            return isPresent;
        }

        // Add adds n to s.
        private static void Add(this ptr<NodeSet> _addr_s, ptr<Node> _addr_n)
        {
            ref NodeSet s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (s == null.val)
            {
                s.val = make();
            }

            (s.val)[n] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

        }

        // Sorted returns s sorted according to less.
        public static slice<ptr<Node>> Sorted(this NodeSet s, Func<ptr<Node>, ptr<Node>, bool> less)
        {
            slice<ptr<Node>> res = default;
            foreach (var (n) in s)
            {
                res = append(res, n);
            }
            sort.Slice(res, (i, j) => less(res[i], res[j]));
            return res;

        }
    }
}}}}
