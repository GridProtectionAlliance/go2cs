// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:17 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\esc.go
using types = go.cmd.compile.@internal.types_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static void escapes(slice<ptr<Node>> all)
        {
            visitBottomUp(all, escapeFuncs);
        }

        public static readonly long EscFuncUnknown = (long)0L + iota;
        public static readonly var EscFuncPlanned = 0;
        public static readonly var EscFuncStarted = 1;
        public static readonly var EscFuncTagged = 2;


        private static sbyte min8(sbyte a, sbyte b)
        {
            if (a < b)
            {
                return a;
            }

            return b;

        }

        private static sbyte max8(sbyte a, sbyte b)
        {
            if (a > b)
            {
                return a;
            }

            return b;

        }

        public static readonly var EscUnknown = iota;
        public static readonly var EscNone = 0; // Does not escape to heap, result, or parameters.
        public static readonly var EscHeap = 1; // Reachable from the heap
        public static readonly var EscNever = 2; // By construction will not escape.

        // funcSym returns fn.Func.Nname.Sym if no nils are encountered along the way.
        private static ptr<types.Sym> funcSym(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn == null || fn.Func.Nname == null)
            {
                return _addr_null!;
            }

            return _addr_fn.Func.Nname.Sym!;

        }

        // Mark labels that have no backjumps to them as not increasing e.loopdepth.
        // Walk hasn't generated (goto|label).Left.Sym.Label yet, so we'll cheat
        // and set it to one of the following two. Then in esc we'll clear it again.
        private static Node looping = default;        private static Node nonlooping = default;

        private static bool isSliceSelfAssign(ptr<Node> _addr_dst, ptr<Node> _addr_src)
        {
            ref Node dst = ref _addr_dst.val;
            ref Node src = ref _addr_src.val;
 
            // Detect the following special case.
            //
            //    func (b *Buffer) Foo() {
            //        n, m := ...
            //        b.buf = b.buf[n:m]
            //    }
            //
            // This assignment is a no-op for escape analysis,
            // it does not store any new pointers into b that were not already there.
            // However, without this special case b will escape, because we assign to OIND/ODOTPTR.
            // Here we assume that the statement will not contain calls,
            // that is, that order will move any calls to init.
            // Otherwise base ONAME value could change between the moments
            // when we evaluate it for dst and for src.

            // dst is ONAME dereference.
            if (dst.Op != ODEREF && dst.Op != ODOTPTR || dst.Left.Op != ONAME)
            {
                return false;
            } 
            // src is a slice operation.

            if (src.Op == OSLICE || src.Op == OSLICE3 || src.Op == OSLICESTR)             else if (src.Op == OSLICEARR || src.Op == OSLICE3ARR) 
                // Since arrays are embedded into containing object,
                // slice of non-pointer array will introduce a new pointer into b that was not already there
                // (pointer to b itself). After such assignment, if b contents escape,
                // b escapes as well. If we ignore such OSLICEARR, we will conclude
                // that b does not escape when b contents do.
                //
                // Pointer to an array is OK since it's not stored inside b directly.
                // For slicing an array (not pointer to array), there is an implicit OADDR.
                // We check that to determine non-pointer array slicing.
                if (src.Left.Op == OADDR)
                {
                    return false;
                }

            else 
                return false;
            // slice is applied to ONAME dereference.
            if (src.Left.Op != ODEREF && src.Left.Op != ODOTPTR || src.Left.Left.Op != ONAME)
            {
                return false;
            } 
            // dst and src reference the same base ONAME.
            return dst.Left == src.Left.Left;

        }

        // isSelfAssign reports whether assignment from src to dst can
        // be ignored by the escape analysis as it's effectively a self-assignment.
        private static bool isSelfAssign(ptr<Node> _addr_dst, ptr<Node> _addr_src)
        {
            ref Node dst = ref _addr_dst.val;
            ref Node src = ref _addr_src.val;

            if (isSliceSelfAssign(_addr_dst, _addr_src))
            {
                return true;
            } 

            // Detect trivial assignments that assign back to the same object.
            //
            // It covers these cases:
            //    val.x = val.y
            //    val.x[i] = val.y[j]
            //    val.x1.x2 = val.x1.y2
            //    ... etc
            //
            // These assignments do not change assigned object lifetime.
            if (dst == null || src == null || dst.Op != src.Op)
            {
                return false;
            }


            if (dst.Op == ODOT || dst.Op == ODOTPTR)             else if (dst.Op == OINDEX) 
                if (mayAffectMemory(_addr_dst.Right) || mayAffectMemory(_addr_src.Right))
                {
                    return false;
                }

            else 
                return false;
            // The expression prefix must be both "safe" and identical.
            return samesafeexpr(dst.Left, src.Left);

        }

        // mayAffectMemory reports whether evaluation of n may affect the program's
        // memory state. If the expression can't affect memory state, then it can be
        // safely ignored by the escape analysis.
        private static bool mayAffectMemory(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // We may want to use a list of "memory safe" ops instead of generally
            // "side-effect free", which would include all calls and other ops that can
            // allocate or change global state. For now, it's safer to start with the latter.
            //
            // We're ignoring things like division by zero, index out of range,
            // and nil pointer dereference here.

            if (n.Op == ONAME || n.Op == OCLOSUREVAR || n.Op == OLITERAL) 
                return false; 

                // Left+Right group.
            else if (n.Op == OINDEX || n.Op == OADD || n.Op == OSUB || n.Op == OOR || n.Op == OXOR || n.Op == OMUL || n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == ODIV || n.Op == OMOD) 
                return mayAffectMemory(_addr_n.Left) || mayAffectMemory(_addr_n.Right); 

                // Left group.
            else if (n.Op == ODOT || n.Op == ODOTPTR || n.Op == ODEREF || n.Op == OCONVNOP || n.Op == OCONV || n.Op == OLEN || n.Op == OCAP || n.Op == ONOT || n.Op == OBITNOT || n.Op == OPLUS || n.Op == ONEG || n.Op == OALIGNOF || n.Op == OOFFSETOF || n.Op == OSIZEOF) 
                return mayAffectMemory(_addr_n.Left);
            else 
                return true;
            
        }

        private static bool mustHeapAlloc(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Type == null)
            {
                return false;
            } 

            // Parameters are always passed via the stack.
            if (n.Op == ONAME && (n.Class() == PPARAM || n.Class() == PPARAMOUT))
            {
                return false;
            }

            if (n.Type.Width > maxStackVarSize)
            {
                return true;
            }

            if ((n.Op == ONEW || n.Op == OPTRLIT) && n.Type.Elem().Width >= maxImplicitStackVarSize)
            {
                return true;
            }

            if (n.Op == OMAKESLICE && !isSmallMakeSlice(n))
            {
                return true;
            }

            return false;

        }

        // addrescapes tags node n as having had its address taken
        // by "increasing" the "value" of n.Esc to EscHeap.
        // Storage is allocated as necessary to allow the address
        // to be taken.
        private static void addrescapes(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == ODEREF || n.Op == ODOTPTR)             else if (n.Op == ONAME) 
                if (n == nodfp)
                {
                    break;
                } 

                // if this is a tmpname (PAUTO), it was tagged by tmpname as not escaping.
                // on PPARAM it means something different.
                if (n.Class() == PAUTO && n.Esc == EscNever)
                {
                    break;
                } 

                // If a closure reference escapes, mark the outer variable as escaping.
                if (n.Name.IsClosureVar())
                {
                    addrescapes(_addr_n.Name.Defn);
                    break;
                }

                if (n.Class() != PPARAM && n.Class() != PPARAMOUT && n.Class() != PAUTO)
                {
                    break;
                } 

                // This is a plain parameter or local variable that needs to move to the heap,
                // but possibly for the function outside the one we're compiling.
                // That is, if we have:
                //
                //    func f(x int) {
                //        func() {
                //            global = &x
                //        }
                //    }
                //
                // then we're analyzing the inner closure but we need to move x to the
                // heap in f, not in the inner closure. Flip over to f before calling moveToHeap.
                var oldfn = Curfn;
                Curfn = n.Name.Curfn;
                if (Curfn.Func.Closure != null && Curfn.Op == OCLOSURE)
                {
                    Curfn = Curfn.Func.Closure;
                }

                var ln = lineno;
                lineno = Curfn.Pos;
                moveToHeap(_addr_n);
                Curfn = oldfn;
                lineno = ln; 

                // ODOTPTR has already been introduced,
                // so these are the non-pointer ODOT and OINDEX.
                // In &x[0], if x is a slice, then x does not
                // escape--the pointer inside x does, but that
                // is always a heap pointer anyway.
            else if (n.Op == ODOT || n.Op == OINDEX || n.Op == OPAREN || n.Op == OCONVNOP) 
                if (!n.Left.Type.IsSlice())
                {
                    addrescapes(_addr_n.Left);
                }

            else             
        }

        // moveToHeap records the parameter or local variable n as moved to the heap.
        private static void moveToHeap(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (Debug['r'] != 0L)
            {
                Dump("MOVE", n);
            }

            if (compiling_runtime)
            {
                yyerror("%v escapes to heap, not allowed in runtime", n);
            }

            if (n.Class() == PAUTOHEAP)
            {
                Dump("n", n);
                Fatalf("double move to heap");
            } 

            // Allocate a local stack variable to hold the pointer to the heap copy.
            // temp will add it to the function declaration list automatically.
            var heapaddr = temp(types.NewPtr(n.Type));
            heapaddr.Sym = lookup("&" + n.Sym.Name);
            heapaddr.Orig.Sym = heapaddr.Sym;
            heapaddr.Pos = n.Pos; 

            // Unset AutoTemp to persist the &foo variable name through SSA to
            // liveness analysis.
            // TODO(mdempsky/drchase): Cleaner solution?
            heapaddr.Name.SetAutoTemp(false); 

            // Parameters have a local stack copy used at function start/end
            // in addition to the copy in the heap that may live longer than
            // the function.
            if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
            {
                if (n.Xoffset == BADWIDTH)
                {
                    Fatalf("addrescapes before param assignment");
                } 

                // We rewrite n below to be a heap variable (indirection of heapaddr).
                // Preserve a copy so we can still write code referring to the original,
                // and substitute that copy into the function declaration list
                // so that analyses of the local (on-stack) variables use it.
                var stackcopy = newname(n.Sym);
                stackcopy.Type = n.Type;
                stackcopy.Xoffset = n.Xoffset;
                stackcopy.SetClass(n.Class());
                stackcopy.Name.Param.Heapaddr = heapaddr;
                if (n.Class() == PPARAMOUT)
                { 
                    // Make sure the pointer to the heap copy is kept live throughout the function.
                    // The function could panic at any point, and then a defer could recover.
                    // Thus, we need the pointer to the heap copy always available so the
                    // post-deferreturn code can copy the return value back to the stack.
                    // See issue 16095.
                    heapaddr.Name.SetIsOutputParamHeapAddr(true);

                }

                n.Name.Param.Stackcopy = stackcopy; 

                // Substitute the stackcopy into the function variable list so that
                // liveness and other analyses use the underlying stack slot
                // and not the now-pseudo-variable n.
                var found = false;
                foreach (var (i, d) in Curfn.Func.Dcl)
                {
                    if (d == n)
                    {
                        Curfn.Func.Dcl[i] = stackcopy;
                        found = true;
                        break;
                    } 
                    // Parameters are before locals, so can stop early.
                    // This limits the search even in functions with many local variables.
                    if (d.Class() == PAUTO)
                    {
                        break;
                    }

                }
                if (!found)
                {
                    Fatalf("cannot find %v in local variable list", n);
                }

                Curfn.Func.Dcl = append(Curfn.Func.Dcl, n);

            } 

            // Modify n in place so that uses of n now mean indirection of the heapaddr.
            n.SetClass(PAUTOHEAP);
            n.Xoffset = 0L;
            n.Name.Param.Heapaddr = heapaddr;
            n.Esc = EscHeap;
            if (Debug['m'] != 0L)
            {
                Warnl(n.Pos, "moved to heap: %v", n);
            }

        }

        // This special tag is applied to uintptr variables
        // that we believe may hold unsafe.Pointers for
        // calls into assembly functions.
        private static readonly @string unsafeUintptrTag = (@string)"unsafe-uintptr";

        // This special tag is applied to uintptr parameters of functions
        // marked go:uintptrescapes.


        // This special tag is applied to uintptr parameters of functions
        // marked go:uintptrescapes.
        private static readonly @string uintptrEscapesTag = (@string)"uintptr-escapes";



        private static @string paramTag(this ptr<Escape> _addr_e, ptr<Node> _addr_fn, long narg, ptr<types.Field> _addr_f)
        {
            ref Escape e = ref _addr_e.val;
            ref Node fn = ref _addr_fn.val;
            ref types.Field f = ref _addr_f.val;

            Func<@string> name = () =>
            {
                if (f.Sym != null)
                {
                    return f.Sym.Name;
                }

                return fmt.Sprintf("arg#%d", narg);

            }
;

            if (fn.Nbody.Len() == 0L)
            { 
                // Assume that uintptr arguments must be held live across the call.
                // This is most important for syscall.Syscall.
                // See golang.org/issue/13372.
                // This really doesn't have much to do with escape analysis per se,
                // but we are reusing the ability to annotate an individual function
                // argument and pass those annotations along to importing code.
                if (f.Type.Etype == TUINTPTR)
                {
                    if (Debug['m'] != 0L)
                    {
                        Warnl(f.Pos, "assuming %v is unsafe uintptr", name());
                    }

                    return unsafeUintptrTag;

                }

                if (!types.Haspointers(f.Type))
                { // don't bother tagging for scalars
                    return "";

                }

                EscLeaks esc = default; 

                // External functions are assumed unsafe, unless
                // //go:noescape is given before the declaration.
                if (fn.Func.Pragma & Noescape != 0L)
                {
                    if (Debug['m'] != 0L && f.Sym != null)
                    {
                        Warnl(f.Pos, "%v does not escape", name());
                    }

                }
                else
                {
                    if (Debug['m'] != 0L && f.Sym != null)
                    {
                        Warnl(f.Pos, "leaking param: %v", name());
                    }

                    esc.AddHeap(0L);

                }

                return esc.Encode();

            }

            if (fn.Func.Pragma & UintptrEscapes != 0L)
            {
                if (f.Type.Etype == TUINTPTR)
                {
                    if (Debug['m'] != 0L)
                    {
                        Warnl(f.Pos, "marking %v as escaping uintptr", name());
                    }

                    return uintptrEscapesTag;

                }

                if (f.IsDDD() && f.Type.Elem().Etype == TUINTPTR)
                { 
                    // final argument is ...uintptr.
                    if (Debug['m'] != 0L)
                    {
                        Warnl(f.Pos, "marking %v as escaping ...uintptr", name());
                    }

                    return uintptrEscapesTag;

                }

            }

            if (!types.Haspointers(f.Type))
            { // don't bother tagging for scalars
                return "";

            } 

            // Unnamed parameters are unused and therefore do not escape.
            if (f.Sym == null || f.Sym.IsBlank())
            {
                esc = default;
                return esc.Encode();
            }

            var n = asNode(f.Nname);
            var loc = e.oldLoc(n);
            esc = loc.paramEsc;
            esc.Optimize();

            if (Debug['m'] != 0L && !loc.escapes)
            {
                if (esc.Empty())
                {
                    Warnl(f.Pos, "%v does not escape", name());
                }

                {
                    var x__prev2 = x;

                    var x = esc.Heap();

                    if (x >= 0L)
                    {
                        if (x == 0L)
                        {
                            Warnl(f.Pos, "leaking param: %v", name());
                        }
                        else
                        { 
                            // TODO(mdempsky): Mention level=x like below?
                            Warnl(f.Pos, "leaking param content: %v", name());

                        }

                    }

                    x = x__prev2;

                }

                for (long i = 0L; i < numEscResults; i++)
                {
                    {
                        var x__prev2 = x;

                        x = esc.Result(i);

                        if (x >= 0L)
                        {
                            var res = fn.Type.Results().Field(i).Sym;
                            Warnl(f.Pos, "leaking param: %v to result %v level=%d", name(), res, x);
                        }

                        x = x__prev2;

                    }

                }


            }

            return esc.Encode();

        }
    }
}}}}
