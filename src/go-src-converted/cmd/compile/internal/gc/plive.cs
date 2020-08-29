// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector liveness bitmap generation.

// The command line flag -live causes this code to print debug information.
// The levels are:
//
//    -live (aka -live=1): print liveness lists as code warnings at safe points
//    -live=2: print an assembly listing with liveness annotations
//
// Each level includes the earlier output as well.

// package gc -- go2cs converted at 2020 August 29 09:28:04 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\plive.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using md5 = go.crypto.md5_package;
using sha1 = go.crypto.sha1_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // TODO(mdempsky): Update to reference OpVar{Def,Kill,Live} instead.

        // VARDEF is an annotation for the liveness analysis, marking a place
        // where a complete initialization (definition) of a variable begins.
        // Since the liveness analysis can see initialization of single-word
        // variables quite easy, gvardef is usually only called for multi-word
        // or 'fat' variables, those satisfying isfat(n->type).
        // However, gvardef is also called when a non-fat variable is initialized
        // via a block move; the only time this happens is when you have
        //    return f()
        // for a function with multiple return values exactly matching the return
        // types of the current function.
        //
        // A 'VARDEF x' annotation in the instruction stream tells the liveness
        // analysis to behave as though the variable x is being initialized at that
        // point in the instruction stream. The VARDEF must appear before the
        // actual (multi-instruction) initialization, and it must also appear after
        // any uses of the previous value, if any. For example, if compiling:
        //
        //    x = x[1:]
        //
        // it is important to generate code like:
        //
        //    base, len, cap = pieces of x[1:]
        //    VARDEF x
        //    x = {base, len, cap}
        //
        // If instead the generated code looked like:
        //
        //    VARDEF x
        //    base, len, cap = pieces of x[1:]
        //    x = {base, len, cap}
        //
        // then the liveness analysis would decide the previous value of x was
        // unnecessary even though it is about to be used by the x[1:] computation.
        // Similarly, if the generated code looked like:
        //
        //    base, len, cap = pieces of x[1:]
        //    x = {base, len, cap}
        //    VARDEF x
        //
        // then the liveness analysis will not preserve the new value of x, because
        // the VARDEF appears to have "overwritten" it.
        //
        // VARDEF is a bit of a kludge to work around the fact that the instruction
        // stream is working on single-word values but the liveness analysis
        // wants to work on individual variables, which might be multi-word
        // aggregates. It might make sense at some point to look into letting
        // the liveness analysis work on single-word values as well, although
        // there are complications around interface values, slices, and strings,
        // all of which cannot be treated as individual words.
        //
        // VARKILL is the opposite of VARDEF: it marks a value as no longer needed,
        // even if its address has been taken. That is, a VARKILL annotation asserts
        // that its argument is certainly dead, for use when the liveness analysis
        // would not otherwise be able to deduce that fact.

        // BlockEffects summarizes the liveness effects on an SSA block.
        public partial struct BlockEffects
        {
            public long lastbitmapindex; // for livenessepilogue

// Computed during livenessprologue using only the content of
// individual blocks:
//
//    uevar: upward exposed variables (used before set in block)
//    varkill: killed variables (set in block)
//    avarinit: addrtaken variables set or used (proof of initialization)
            public bvec uevar;
            public bvec varkill;
            public bvec avarinit; // Computed during livenesssolve using control flow information:
//
//    livein: variables live at block entry
//    liveout: variables live at block exit
//    avarinitany: addrtaken variables possibly initialized at block exit
//        (initialized in block or at exit from any predecessor block)
//    avarinitall: addrtaken variables certainly initialized at block exit
//        (initialized in block or at exit from all predecessor blocks)
            public bvec livein;
            public bvec liveout;
            public bvec avarinitany;
            public bvec avarinitall;
        }

        // A collection of global state used by liveness analysis.
        public partial struct Liveness
        {
            public ptr<Node> fn;
            public ptr<ssa.Func> f;
            public slice<ref Node> vars;
            public map<ref Node, int> idx;
            public long stkptrsize;
            public slice<BlockEffects> be; // stackMapIndex maps from safe points (i.e., CALLs) to their
// index within the stack maps.
            public map<ref ssa.Value, long> stackMapIndex; // An array with a bit vector for each safe point tracking live variables.
            public slice<bvec> livevars;
            public progeffectscache cache;
        }

        private partial struct progeffectscache
        {
            public slice<int> textavarinit;
            public slice<int> retuevar;
            public slice<int> tailuevar;
            public bool initialized;
        }

        // livenessShouldTrack reports whether the liveness analysis
        // should track the variable n.
        // We don't care about variables that have no pointers,
        // nor do we care about non-local variables,
        // nor do we care about empty structs (handled by the pointer check),
        // nor do we care about the fake PAUTOHEAP variables.
        private static bool livenessShouldTrack(ref Node n)
        {
            return n.Op == ONAME && (n.Class() == PAUTO || n.Class() == PPARAM || n.Class() == PPARAMOUT) && types.Haspointers(n.Type);
        }

        // getvariables returns the list of on-stack variables that we need to track
        // and a map for looking up indices by *Node.
        private static (slice<ref Node>, map<ref Node, int>) getvariables(ref Node fn)
        {
            slice<ref Node> vars = default;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;
                    if (livenessShouldTrack(n))
                    {
                        vars = append(vars, n);
                    }
                }

                n = n__prev1;
            }

            var idx = make_map<ref Node, int>(len(vars));
            {
                var n__prev1 = n;

                foreach (var (__i, __n) in vars)
                {
                    i = __i;
                    n = __n;
                    idx[n] = int32(i);
                }

                n = n__prev1;
            }

            return (vars, idx);
        }

        private static void initcache(this ref Liveness lv)
        {
            if (lv.cache.initialized)
            {
                Fatalf("liveness cache initialized twice");
                return;
            }
            lv.cache.initialized = true;

            foreach (var (i, node) in lv.vars)
            {

                if (node.Class() == PPARAM) 
                    // A return instruction with a p.to is a tail return, which brings
                    // the stack pointer back up (if it ever went down) and then jumps
                    // to a new function entirely. That form of instruction must read
                    // all the parameters for correctness, and similarly it must not
                    // read the out arguments - they won't be set until the new
                    // function runs.

                    lv.cache.tailuevar = append(lv.cache.tailuevar, int32(i));

                    if (node.Addrtaken())
                    {
                        lv.cache.textavarinit = append(lv.cache.textavarinit, int32(i));
                    }
                else if (node.Class() == PPARAMOUT) 
                    // If the result had its address taken, it is being tracked
                    // by the avarinit code, which does not use uevar.
                    // If we added it to uevar too, we'd not see any kill
                    // and decide that the variable was live entry, which it is not.
                    // So only use uevar in the non-addrtaken case.
                    // The p.to.type == obj.TYPE_NONE limits the bvset to
                    // non-tail-call return instructions; see note below for details.
                    if (!node.Addrtaken())
                    {
                        lv.cache.retuevar = append(lv.cache.retuevar, int32(i));
                    }
                            }
        }

        // A liveEffect is a set of flags that describe an instruction's
        // liveness effects on a variable.
        //
        // The possible flags are:
        //    uevar - used by the instruction
        //    varkill - killed by the instruction
        //        for variables without address taken, means variable was set
        //        for variables with address taken, means variable was marked dead
        //    avarinit - initialized or referred to by the instruction,
        //        only for variables with address taken but not escaping to heap
        //
        // The avarinit output serves as a signal that the data has been
        // initialized, because any use of a variable must come after its
        // initialization.
        private partial struct liveEffect // : long
        {
        }

        private static readonly liveEffect uevar = 1L << (int)(iota);
        private static readonly var varkill = 0;
        private static readonly var avarinit = 1;

        // valueEffects returns the index of a variable in lv.vars and the
        // liveness effects v has on that variable.
        // If v does not affect any tracked variables, it returns -1, 0.
        private static (int, liveEffect) valueEffects(this ref Liveness lv, ref ssa.Value v)
        {
            var (n, e) = affectedNode(v);
            if (e == 0L || n == null || n.Op != ONAME)
            { // cheapest checks first
                return (-1L, 0L);
            } 

            // AllocFrame has dropped unused variables from
            // lv.fn.Func.Dcl, but they might still be referenced by
            // OpVarFoo pseudo-ops. Ignore them to prevent "lost track of
            // variable" ICEs (issue 19632).

            if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive) 
                if (!n.Name.Used())
                {
                    return (-1L, 0L);
                }
                        liveEffect effect = default;
            if (n.Addrtaken())
            {
                if (v.Op != ssa.OpVarKill)
                {
                    effect |= avarinit;
                }
                if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill)
                {
                    effect |= varkill;
                }
            }
            else
            { 
                // Read is a read, obviously.
                // Addr by itself is also implicitly a read.
                //
                // Addr|Write means that the address is being taken
                // but only so that the instruction can write to the value.
                // It is not a read.

                if (e & ssa.SymRead != 0L || e & (ssa.SymAddr | ssa.SymWrite) == ssa.SymAddr)
                {
                    effect |= uevar;
                }
                if (e & ssa.SymWrite != 0L && (!isfat(n.Type) || v.Op == ssa.OpVarDef))
                {
                    effect |= varkill;
                }
            }
            if (effect == 0L)
            {
                return (-1L, 0L);
            }
            {
                var (pos, ok) = lv.idx[n];

                if (ok)
                {
                    return (pos, effect);
                }

            }
            return (-1L, 0L);
        }

        // affectedNode returns the *Node affected by v
        private static (ref Node, ssa.SymEffect) affectedNode(ref ssa.Value v)
        { 
            // Special cases.

            if (v.Op == ssa.OpLoadReg) 
                var (n, _) = AutoVar(v.Args[0L]);
                return (n, ssa.SymRead);
            else if (v.Op == ssa.OpStoreReg) 
                (n, _) = AutoVar(v);
                return (n, ssa.SymWrite);
            else if (v.Op == ssa.OpVarLive) 
                return (v.Aux._<ref Node>(), ssa.SymRead);
            else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill) 
                return (v.Aux._<ref Node>(), ssa.SymWrite);
            else if (v.Op == ssa.OpKeepAlive) 
                (n, _) = AutoVar(v.Args[0L]);
                return (n, ssa.SymRead);
                        var e = v.Op.SymEffect();
            if (e == 0L)
            {
                return (null, 0L);
            }
            ref Node n = default;
            switch (v.Aux.type())
            {
                case ref obj.LSym a:
                    break;
                case ref Node a:
                    n = a;
                    break;
                default:
                {
                    var a = v.Aux.type();
                    Fatalf("weird aux: %s", v.LongString());
                    break;
                }

            }

            return (n, e);
        }

        // Constructs a new liveness structure used to hold the global state of the
        // liveness computation. The cfg argument is a slice of *BasicBlocks and the
        // vars argument is a slice of *Nodes.
        private static ref Liveness newliveness(ref Node fn, ref ssa.Func f, slice<ref Node> vars, map<ref Node, int> idx, long stkptrsize)
        {
            Liveness lv = ref new Liveness(fn:fn,f:f,vars:vars,idx:idx,stkptrsize:stkptrsize,be:make([]BlockEffects,f.NumBlocks()),);

            var nblocks = int32(len(f.Blocks));
            var nvars = int32(len(vars));
            var bulk = bvbulkalloc(nvars, nblocks * 7L);
            foreach (var (_, b) in f.Blocks)
            {
                var be = lv.blockEffects(b);

                be.uevar = bulk.next();
                be.varkill = bulk.next();
                be.livein = bulk.next();
                be.liveout = bulk.next();
                be.avarinit = bulk.next();
                be.avarinitany = bulk.next();
                be.avarinitall = bulk.next();
            }
            return lv;
        }

        private static ref BlockEffects blockEffects(this ref Liveness lv, ref ssa.Block b)
        {
            return ref lv.be[b.ID];
        }

        // NOTE: The bitmap for a specific type t could be cached in t after
        // the first run and then simply copied into bv at the correct offset
        // on future calls with the same type t.
        private static void onebitwalktype1(ref types.Type t, long off, bvec bv)
        {
            if (t.Align > 0L && off & int64(t.Align - 1L) != 0L)
            {
                Fatalf("onebitwalktype1: invalid initial alignment, %v", t);
            }

            if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR || t.Etype == TBOOL || t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128)             else if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TFUNC || t.Etype == TCHAN || t.Etype == TMAP) 
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                }
                bv.Set(int32(off / int64(Widthptr))); // pointer
            else if (t.Etype == TSTRING) 
                // struct { byte *str; intgo len; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                }
                bv.Set(int32(off / int64(Widthptr))); //pointer in first slot
            else if (t.Etype == TINTER) 
                // struct { Itab *tab;    void *data; }
                // or, when isnilinter(t)==true:
                // struct { Type *type; void *data; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                }
                bv.Set(int32(off / int64(Widthptr))); // pointer in first slot
                bv.Set(int32(off / int64(Widthptr) + 1L)); // pointer in second slot
            else if (t.Etype == TSLICE) 
                // struct { byte *array; uintgo len; uintgo cap; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid TARRAY alignment, %v", t);
                }
                bv.Set(int32(off / int64(Widthptr))); // pointer in first slot (BitsPointer)
            else if (t.Etype == TARRAY) 
                var elt = t.Elem();
                if (elt.Width == 0L)
                { 
                    // Short-circuit for #20739.
                    break;
                }
                for (var i = int64(0L); i < t.NumElem(); i++)
                {
                    onebitwalktype1(elt, off, bv);
                    off += elt.Width;
                }

            else if (t.Etype == TSTRUCT) 
                foreach (var (_, f) in t.Fields().Slice())
                {
                    onebitwalktype1(f.Type, off + f.Offset, bv);
                }
            else 
                Fatalf("onebitwalktype1: unexpected type, %v", t);
                    }

        // localWords returns the number of words of local variables.
        private static int localWords(this ref Liveness lv)
        {
            return int32(lv.stkptrsize / int64(Widthptr));
        }

        // argWords returns the number of words of in and out arguments.
        private static int argWords(this ref Liveness lv)
        {
            return int32(lv.fn.Type.ArgWidth() / int64(Widthptr));
        }

        // Generates live pointer value maps for arguments and local variables. The
        // this argument and the in arguments are always assumed live. The vars
        // argument is a slice of *Nodes.
        private static void pointerMap(this ref Liveness lv, bvec liveout, slice<ref Node> vars, bvec args, bvec locals)
        {
            for (var i = int32(0L); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                i = liveout.Next(i);
                if (i < 0L)
                {
                    break;
                }
                var node = vars[i];

                if (node.Class() == PAUTO) 
                    onebitwalktype1(node.Type, node.Xoffset + lv.stkptrsize, locals);
                else if (node.Class() == PPARAM || node.Class() == PPARAMOUT) 
                    onebitwalktype1(node.Type, node.Xoffset, args);
                            }

        }

        // Returns true for instructions that are safe points that must be annotated
        // with liveness information.
        private static bool issafepoint(ref ssa.Value v)
        {
            return v.Op.IsCall();
        }

        // Initializes the sets for solving the live variables. Visits all the
        // instructions in each basic block to summarizes the information at each basic
        // block
        private static void prologue(this ref Liveness lv)
        {
            lv.initcache();

            foreach (var (_, b) in lv.f.Blocks)
            {
                var be = lv.blockEffects(b); 

                // Walk the block instructions backward and update the block
                // effects with the each prog effects.
                {
                    var j__prev2 = j;

                    for (var j = len(b.Values) - 1L; j >= 0L; j--)
                    {
                        var (pos, e) = lv.valueEffects(b.Values[j]);
                        if (e & varkill != 0L)
                        {
                            be.varkill.Set(pos);
                            be.uevar.Unset(pos);
                        }
                        if (e & uevar != 0L)
                        {
                            be.uevar.Set(pos);
                        }
                    } 

                    // Walk the block instructions forward to update avarinit bits.
                    // avarinit describes the effect at the end of the block, not the beginning.


                    j = j__prev2;
                } 

                // Walk the block instructions forward to update avarinit bits.
                // avarinit describes the effect at the end of the block, not the beginning.
                {
                    var j__prev2 = j;

                    for (j = 0L; j < len(b.Values); j++)
                    {
                        (pos, e) = lv.valueEffects(b.Values[j]);
                        if (e & varkill != 0L)
                        {
                            be.avarinit.Unset(pos);
                        }
                        if (e & avarinit != 0L)
                        {
                            be.avarinit.Set(pos);
                        }
                    }


                    j = j__prev2;
                }
            }
        }

        // Solve the liveness dataflow equations.
        private static void solve(this ref Liveness lv)
        { 
            // These temporary bitvectors exist to avoid successive allocations and
            // frees within the loop.
            var newlivein = bvalloc(int32(len(lv.vars)));
            var newliveout = bvalloc(int32(len(lv.vars)));
            var any = bvalloc(int32(len(lv.vars)));
            var all = bvalloc(int32(len(lv.vars))); 

            // Push avarinitall, avarinitany forward.
            // avarinitall says the addressed var is initialized along all paths reaching the block exit.
            // avarinitany says the addressed var is initialized along some path reaching the block exit.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    var be = lv.blockEffects(b);
                    if (b == lv.f.Entry)
                    {
                        be.avarinitall.Copy(be.avarinit);
                    }
                    else
                    {
                        be.avarinitall.Clear();
                        be.avarinitall.Not();
                    }
                    be.avarinitany.Copy(be.avarinit);
                } 

                // Walk blocks in the general direction of propagation (RPO
                // for avarinit{any,all}, and PO for live{in,out}). This
                // improves convergence.

                b = b__prev1;
            }

            var po = lv.f.Postorder();

            {
                var change__prev1 = change;

                var change = true;

                while (change)
                {
                    change = false;
                    for (var i = len(po) - 1L; i >= 0L; i--)
                    {
                        var b = po[i];
                        be = lv.blockEffects(b);
                        lv.avarinitanyall(b, any, all);

                        any.AndNot(any, be.varkill);
                        all.AndNot(all, be.varkill);
                        any.Or(any, be.avarinit);
                        all.Or(all, be.avarinit);
                        if (!any.Eq(be.avarinitany))
                        {
                            change = true;
                            be.avarinitany.Copy(any);
                        }
                        if (!all.Eq(be.avarinitall))
                        {
                            change = true;
                            be.avarinitall.Copy(all);
                        }
                    }

                } 

                // Iterate through the blocks in reverse round-robin fashion. A work
                // queue might be slightly faster. As is, the number of iterations is
                // so low that it hardly seems to be worth the complexity.


                change = change__prev1;
            } 

            // Iterate through the blocks in reverse round-robin fashion. A work
            // queue might be slightly faster. As is, the number of iterations is
            // so low that it hardly seems to be worth the complexity.

            {
                var change__prev1 = change;

                change = true;

                while (change)
                {
                    change = false;
                    {
                        var b__prev2 = b;

                        foreach (var (_, __b) in po)
                        {
                            b = __b;
                            be = lv.blockEffects(b);

                            newliveout.Clear();

                            if (b.Kind == ssa.BlockRet) 
                                {
                                    var pos__prev3 = pos;

                                    foreach (var (_, __pos) in lv.cache.retuevar)
                                    {
                                        pos = __pos;
                                        newliveout.Set(pos);
                                    }

                                    pos = pos__prev3;
                                }
                            else if (b.Kind == ssa.BlockRetJmp) 
                                {
                                    var pos__prev3 = pos;

                                    foreach (var (_, __pos) in lv.cache.tailuevar)
                                    {
                                        pos = __pos;
                                        newliveout.Set(pos);
                                    }

                                    pos = pos__prev3;
                                }
                            else if (b.Kind == ssa.BlockExit)                             else 
                                // A variable is live on output from this block
                                // if it is live on input to some successor.
                                //
                                // out[b] = \bigcup_{s \in succ[b]} in[s]
                                newliveout.Copy(lv.blockEffects(b.Succs[0L].Block()).livein);
                                foreach (var (_, succ) in b.Succs[1L..])
                                {
                                    newliveout.Or(newliveout, lv.blockEffects(succ.Block()).livein);
                                }
                                                        if (!be.liveout.Eq(newliveout))
                            {
                                change = true;
                                be.liveout.Copy(newliveout);
                            } 

                            // A variable is live on input to this block
                            // if it is live on output from this block and
                            // not set by the code in this block.
                            //
                            // in[b] = uevar[b] \cup (out[b] \setminus varkill[b])
                            newlivein.AndNot(be.liveout, be.varkill);
                            be.livein.Or(newlivein, be.uevar);
                        }

                        b = b__prev2;
                    }

                }


                change = change__prev1;
            }
        }

        // Visits all instructions in a basic block and computes a bit vector of live
        // variables at each safe point locations.
        private static void epilogue(this ref Liveness lv)
        {
            var nvars = int32(len(lv.vars));
            var liveout = bvalloc(nvars);
            var any = bvalloc(nvars);
            var all = bvalloc(nvars);
            var livedefer = bvalloc(nvars); // always-live variables

            // If there is a defer (that could recover), then all output
            // parameters are live all the time.  In addition, any locals
            // that are pointers to heap-allocated output parameters are
            // also always live (post-deferreturn code needs these
            // pointers to copy values back to the stack).
            // TODO: if the output parameter is heap-allocated, then we
            // don't need to keep the stack copy live?
            if (lv.fn.Func.HasDefer())
            {
                {
                    var i__prev1 = i;
                    var n__prev1 = n;

                    foreach (var (__i, __n) in lv.vars)
                    {
                        i = __i;
                        n = __n;
                        if (n.Class() == PPARAMOUT)
                        {
                            if (n.IsOutputParamHeapAddr())
                            { 
                                // Just to be paranoid.  Heap addresses are PAUTOs.
                                Fatalf("variable %v both output param and heap output param", n);
                            }
                            if (n.Name.Param.Heapaddr != null)
                            { 
                                // If this variable moved to the heap, then
                                // its stack copy is not live.
                                continue;
                            } 
                            // Note: zeroing is handled by zeroResults in walk.go.
                            livedefer.Set(int32(i));
                        }
                        if (n.IsOutputParamHeapAddr())
                        {
                            n.Name.SetNeedzero(true);
                            livedefer.Set(int32(i));
                        }
                    }

                    i = i__prev1;
                    n = n__prev1;
                }

            }
            { 
                // Reserve an entry for function entry.
                var live = bvalloc(nvars);
                {
                    var pos__prev1 = pos;

                    foreach (var (_, __pos) in lv.cache.textavarinit)
                    {
                        pos = __pos;
                        live.Set(pos);
                    }

                    pos = pos__prev1;
                }

                lv.livevars = append(lv.livevars, live);
            }
            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    var be = lv.blockEffects(b); 

                    // Compute avarinitany and avarinitall for entry to block.
                    // This duplicates information known during livenesssolve
                    // but avoids storing two more vectors for each block.
                    lv.avarinitanyall(b, any, all); 

                    // Walk forward through the basic block instructions and
                    // allocate liveness maps for those instructions that need them.
                    // Seed the maps with information about the addrtaken variables.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            var (pos, e) = lv.valueEffects(v);
                            if (e & varkill != 0L)
                            {
                                any.Unset(pos);
                                all.Unset(pos);
                            }
                            if (e & avarinit != 0L)
                            {
                                any.Set(pos);
                                all.Set(pos);
                            }
                            if (!issafepoint(v))
                            {
                                continue;
                            } 

                            // Annotate ambiguously live variables so that they can
                            // be zeroed at function entry and at VARKILL points.
                            // liveout is dead here and used as a temporary.
                            liveout.AndNot(any, all);
                            if (!liveout.IsEmpty())
                            {
                                {
                                    var pos__prev3 = pos;

                                    for (var pos = int32(0L); pos < liveout.n; pos++)
                                    {
                                        if (!liveout.Get(pos))
                                        {
                                            continue;
                                        }
                                        all.Set(pos); // silence future warnings in this block
                                        var n = lv.vars[pos];
                                        if (!n.Name.Needzero())
                                        {
                                            n.Name.SetNeedzero(true);
                                            if (debuglive >= 1L)
                                            {
                                                Warnl(v.Pos, "%v: %L is ambiguously live", lv.fn.Func.Nname, n);
                                            }
                                        }
                                    }


                                    pos = pos__prev3;
                                }
                            } 

                            // Live stuff first.
                            live = bvalloc(nvars);
                            live.Copy(any);
                            lv.livevars = append(lv.livevars, live);
                        }

                        v = v__prev2;
                    }

                    be.lastbitmapindex = len(lv.livevars) - 1L;
                }

                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    be = lv.blockEffects(b); 

                    // walk backward, construct maps at each safe point
                    var index = int32(be.lastbitmapindex);
                    if (index < 0L)
                    { 
                        // the first block we encounter should have the ATEXT so
                        // at no point should pos ever be less than zero.
                        Fatalf("livenessepilogue");
                    }
                    liveout.Copy(be.liveout);
                    {
                        var i__prev2 = i;

                        for (var i = len(b.Values) - 1L; i >= 0L; i--)
                        {
                            var v = b.Values[i];

                            if (issafepoint(v))
                            { 
                                // Found an interesting instruction, record the
                                // corresponding liveness information.

                                live = lv.livevars[index];
                                live.Or(live, liveout);
                                live.Or(live, livedefer); // only for non-entry safe points
                                index--;
                            } 

                            // Update liveness information.
                            (pos, e) = lv.valueEffects(v);
                            if (e & varkill != 0L)
                            {
                                liveout.Unset(pos);
                            }
                            if (e & uevar != 0L)
                            {
                                liveout.Set(pos);
                            }
                        }


                        i = i__prev2;
                    }

                    if (b == lv.f.Entry)
                    {
                        if (index != 0L)
                        {
                            Fatalf("bad index for entry point: %v", index);
                        } 

                        // Record live variables.
                        live = lv.livevars[index];
                        live.Or(live, liveout);
                    }
                } 

                // Useful sanity check: on entry to the function,
                // the only things that can possibly be live are the
                // input parameters.

                b = b__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (__j, __n) in lv.vars)
                {
                    j = __j;
                    n = __n;
                    if (n.Class() != PPARAM && lv.livevars[0L].Get(int32(j)))
                    {
                        Fatalf("internal error: %v %L recorded as live on entry", lv.fn.Func.Nname, n);
                    }
                }

                n = n__prev1;
            }

        }

        private static void clobber(this ref Liveness lv)
        { 
            // The clobberdead experiment inserts code to clobber all the dead variables (locals and args)
            // before and after every safepoint. This experiment is useful for debugging the generation
            // of live pointer bitmaps.
            if (objabi.Clobberdead_enabled == 0L)
            {
                return;
            }
            long varSize = default;
            foreach (var (_, n) in lv.vars)
            {
                varSize += n.Type.Size();
            }
            if (len(lv.livevars) > 1000L || varSize > 10000L)
            { 
                // Be careful to avoid doing too much work.
                // Bail if >1000 safepoints or >10000 bytes of variables.
                // Otherwise, giant functions make this experiment generate too much code.
                return;
            }
            {
                var h = os.Getenv("GOCLOBBERDEADHASH");

                if (h != "")
                { 
                    // Clobber only functions where the hash of the function name matches a pattern.
                    // Useful for binary searching for a miscompiled function.
                    @string hstr = "";
                    {
                        var b__prev1 = b;

                        foreach (var (_, __b) in sha1.Sum((slice<byte>)lv.fn.funcname()))
                        {
                            b = __b;
                            hstr += fmt.Sprintf("%08b", b);
                        }

                        b = b__prev1;
                    }

                    if (!strings.HasSuffix(hstr, h))
                    {
                        return;
                    }
                    fmt.Printf("\t\t\tCLOBBERDEAD %s\n", lv.fn.funcname());
                }

            }
            if (lv.f.Name == "forkAndExecInChild")
            { 
                // forkAndExecInChild calls vfork (on linux/amd64, anyway).
                // The code we add here clobbers parts of the stack in the child.
                // When the parent resumes, it is using the same stack frame. But the
                // child has clobbered stack variables that the parent needs. Boom!
                // In particular, the sys argument gets clobbered.
                // Note to self: GOCLOBBERDEADHASH=011100101110
                return;
            }
            slice<ref ssa.Value> oldSched = default;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b; 
                    // Copy block's values to a temporary.
                    oldSched = append(oldSched[..0L], b.Values);
                    b.Values = b.Values[..0L]; 

                    // Clobber all dead variables at entry.
                    if (b == lv.f.Entry)
                    {
                        while (len(oldSched) > 0L && len(oldSched[0L].Args) == 0L)
                        { 
                            // Skip argless ops. We need to skip at least
                            // the lowered ClosurePtr op, because it
                            // really wants to be first. This will also
                            // skip ops like InitMem and SP, which are ok.
                            b.Values = append(b.Values, oldSched[0L]);
                            oldSched = oldSched[1L..];
                        }

                        clobber(lv, b, lv.livevars[0L]);
                    } 

                    // Copy values into schedule, adding clobbering around safepoints.
                    foreach (var (_, v) in oldSched)
                    {
                        if (!issafepoint(v))
                        {
                            b.Values = append(b.Values, v);
                            continue;
                        }
                        var before = true;
                        if (v.Op.IsCall() && v.Aux != null && v.Aux._<ref obj.LSym>() == typedmemmove)
                        { 
                            // Can't put clobber code before the call to typedmemmove.
                            // The variable to-be-copied is marked as dead
                            // at the callsite. That is ok, though, as typedmemmove
                            // is marked as nosplit, and the first thing it does
                            // is to call memmove (also nosplit), after which
                            // the source value is dead.
                            // See issue 16026.
                            before = false;
                        }
                        if (before)
                        {
                            clobber(lv, b, lv.livevars[lv.stackMapIndex[v]]);
                        }
                        b.Values = append(b.Values, v);
                        clobber(lv, b, lv.livevars[lv.stackMapIndex[v]]);
                    }
                }

                b = b__prev1;
            }

        }

        // clobber generates code to clobber all dead variables (those not marked in live).
        // Clobbering instructions are added to the end of b.Values.
        private static void clobber(ref Liveness lv, ref ssa.Block b, bvec live)
        {
            foreach (var (i, n) in lv.vars)
            {
                if (!live.Get(int32(i)))
                {
                    clobberVar(b, n);
                }
            }
        }

        // clobberVar generates code to trash the pointers in v.
        // Clobbering instructions are added to the end of b.Values.
        private static void clobberVar(ref ssa.Block b, ref Node v)
        {
            clobberWalk(b, v, 0L, v.Type);
        }

        // b = block to which we append instructions
        // v = variable
        // offset = offset of (sub-portion of) variable to clobber (in bytes)
        // t = type of sub-portion of v.
        private static void clobberWalk(ref ssa.Block b, ref Node v, long offset, ref types.Type t)
        {
            if (!types.Haspointers(t))
            {
                return;
            }

            if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TFUNC || t.Etype == TCHAN || t.Etype == TMAP) 
                clobberPtr(b, v, offset);
            else if (t.Etype == TSTRING) 
                // struct { byte *str; int len; }
                clobberPtr(b, v, offset);
            else if (t.Etype == TINTER) 
                // struct { Itab *tab; void *data; }
                // or, when isnilinter(t)==true:
                // struct { Type *type; void *data; }
                clobberPtr(b, v, offset);
                clobberPtr(b, v, offset + int64(Widthptr));
            else if (t.Etype == TSLICE) 
                // struct { byte *array; int len; int cap; }
                clobberPtr(b, v, offset);
            else if (t.Etype == TARRAY) 
                for (var i = int64(0L); i < t.NumElem(); i++)
                {
                    clobberWalk(b, v, offset + i * t.Elem().Size(), t.Elem());
                }

            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    clobberWalk(b, v, offset + t1.Offset, t1.Type);
                }
            else 
                Fatalf("clobberWalk: unexpected type, %v", t);
                    }

        // clobberPtr generates a clobber of the pointer at offset offset in v.
        // The clobber instruction is added at the end of b.
        private static void clobberPtr(ref ssa.Block b, ref Node v, long offset)
        {
            b.NewValue0IA(src.NoXPos, ssa.OpClobber, types.TypeVoid, offset, v);
        }

        private static void avarinitanyall(this ref Liveness lv, ref ssa.Block b, bvec any, bvec all)
        {
            if (len(b.Preds) == 0L)
            {
                any.Clear();
                all.Clear();
                foreach (var (_, pos) in lv.cache.textavarinit)
                {
                    any.Set(pos);
                    all.Set(pos);
                }
                return;
            }
            var be = lv.blockEffects(b.Preds[0L].Block());
            any.Copy(be.avarinitany);
            all.Copy(be.avarinitall);

            foreach (var (_, pred) in b.Preds[1L..])
            {
                be = lv.blockEffects(pred.Block());
                any.Or(any, be.avarinitany);
                all.And(all, be.avarinitall);
            }
        }

        // FNV-1 hash function constants.
        public static readonly long H0 = 2166136261L;
        public static readonly long Hp = 16777619L;

        private static uint hashbitmap(uint h, bvec bv)
        {
            var n = int((bv.n + 31L) / 32L);
            for (long i = 0L; i < n; i++)
            {
                var w = bv.b[i];
                h = (h * Hp) ^ (w & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(8L)) & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(16L)) & 0xffUL);
                h = (h * Hp) ^ ((w >> (int)(24L)) & 0xffUL);
            }


            return h;
        }

        // Compact liveness information by coalescing identical per-call-site bitmaps.
        // The merging only happens for a single function, not across the entire binary.
        //
        // There are actually two lists of bitmaps, one list for the local variables and one
        // list for the function arguments. Both lists are indexed by the same PCDATA
        // index, so the corresponding pairs must be considered together when
        // merging duplicates. The argument bitmaps change much less often during
        // function execution than the local variable bitmaps, so it is possible that
        // we could introduce a separate PCDATA index for arguments vs locals and
        // then compact the set of argument bitmaps separately from the set of
        // local variable bitmaps. As of 2014-04-02, doing this to the godoc binary
        // is actually a net loss: we save about 50k of argument bitmaps but the new
        // PCDATA tables cost about 100k. So for now we keep using a single index for
        // both bitmap lists.
        private static void compact(this ref Liveness lv)
        { 
            // Linear probing hash table of bitmaps seen so far.
            // The hash table has 4n entries to keep the linear
            // scan short. An entry of -1 indicates an empty slot.
            var n = len(lv.livevars);

            long tablesize = 4L * n;
            var table = make_slice<long>(tablesize);
            {
                var i__prev1 = i;

                foreach (var (__i) in table)
                {
                    i = __i;
                    table[i] = -1L;
                } 

                // remap[i] = the new index of the old bit vector #i.

                i = i__prev1;
            }

            var remap = make_slice<long>(n);
            {
                var i__prev1 = i;

                foreach (var (__i) in remap)
                {
                    i = __i;
                    remap[i] = -1L;
                }

                i = i__prev1;
            }

            long uniq = 0L; // unique tables found so far

            // Consider bit vectors in turn.
            // If new, assign next number using uniq,
            // record in remap, record in lv.livevars
            // under the new index, and add entry to hash table.
            // If already seen, record earlier index in remap.
Outer: 

            // We've already reordered lv.livevars[0:uniq]. Clear the
            // pointers later in the array so they can be GC'd.
            {
                var i__prev1 = i;

                foreach (var (__i, __live) in lv.livevars)
                {
                    i = __i;
                    live = __live;
                    var h = hashbitmap(H0, live) % uint32(tablesize);

                    while (true)
                    {
                        var j = table[h];
                        if (j < 0L)
                        {
                            break;
                        }
                        var jlive = lv.livevars[j];
                        if (live.Eq(jlive))
                        {
                            remap[i] = j;
                            _continueOuter = true;
                            break;
                        }
                        h++;
                        if (h == uint32(tablesize))
                        {
                            h = 0L;
                        }
                    }


                    table[h] = uniq;
                    remap[i] = uniq;
                    lv.livevars[uniq] = live;
                    uniq++;
                } 

                // We've already reordered lv.livevars[0:uniq]. Clear the
                // pointers later in the array so they can be GC'd.

                i = i__prev1;
            }
            var tail = lv.livevars[uniq..];
            {
                var i__prev1 = i;

                foreach (var (__i) in tail)
                {
                    i = __i; // memclr loop pattern
                    tail[i] = new bvec();
                }

                i = i__prev1;
            }

            lv.livevars = lv.livevars[..uniq]; 

            // Record compacted stack map indexes for each value.
            // These will later become PCDATA instructions.
            lv.showlive(null, lv.livevars[0L]);
            long pos = 1L;
            lv.stackMapIndex = make_map<ref ssa.Value, long>();
            foreach (var (_, b) in lv.f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (issafepoint(v))
                    {
                        lv.showlive(v, lv.livevars[remap[pos]]);
                        lv.stackMapIndex[v] = int(remap[pos]);
                        pos++;
                    }
                }
            }
        }

        private static void showlive(this ref Liveness lv, ref ssa.Value v, bvec live)
        {
            if (debuglive == 0L || lv.fn.funcname() == "init" || strings.HasPrefix(lv.fn.funcname(), "."))
            {
                return;
            }
            if (live.IsEmpty())
            {
                return;
            }
            var pos = lv.fn.Func.Nname.Pos;
            if (v != null)
            {
                pos = v.Pos;
            }
            @string s = "live at ";
            if (v == null)
            {
                s += fmt.Sprintf("entry to %s:", lv.fn.funcname());
            }            {
                ref obj.LSym (sym, ok) = v.Aux._<ref obj.LSym>();


                else if (ok)
                {
                    var fn = sym.Name;
                    {
                        var pos__prev3 = pos;

                        pos = strings.Index(fn, ".");

                        if (pos >= 0L)
                        {
                            fn = fn[pos + 1L..];
                        }

                        pos = pos__prev3;

                    }
                    s += fmt.Sprintf("call to %s:", fn);
                }
                else
                {
                    s += "indirect call:";
                }

            }

            foreach (var (j, n) in lv.vars)
            {
                if (live.Get(int32(j)))
                {
                    s += fmt.Sprintf(" %v", n);
                }
            }
            Warnl(pos, s);
        }

        private static bool printbvec(this ref Liveness lv, bool printed, @string name, bvec live)
        {
            var started = false;
            foreach (var (i, n) in lv.vars)
            {
                if (!live.Get(int32(i)))
                {
                    continue;
                }
                if (!started)
                {
                    if (!printed)
                    {
                        fmt.Printf("\t");
                    }
                    else
                    {
                        fmt.Printf(" ");
                    }
                    started = true;
                    printed = true;
                    fmt.Printf("%s=", name);
                }
                else
                {
                    fmt.Printf(",");
                }
                fmt.Printf("%s", n.Sym.Name);
            }
            return printed;
        }

        // printeffect is like printbvec, but for a single variable.
        private static bool printeffect(this ref Liveness lv, bool printed, @string name, int pos, bool x)
        {
            if (!x)
            {
                return printed;
            }
            if (!printed)
            {
                fmt.Printf("\t");
            }
            else
            {
                fmt.Printf(" ");
            }
            fmt.Printf("%s=%s", name, lv.vars[pos].Sym.Name);
            return true;
        }

        // Prints the computed liveness information and inputs, for debugging.
        // This format synthesizes the information used during the multiple passes
        // into a single presentation.
        private static void printDebug(this ref Liveness lv)
        {
            fmt.Printf("liveness: %s\n", lv.fn.funcname());

            long pcdata = 0L;
            foreach (var (i, b) in lv.f.Blocks)
            {
                if (i > 0L)
                {
                    fmt.Printf("\n");
                } 

                // bb#0 pred=1,2 succ=3,4
                fmt.Printf("bb#%d pred=", b.ID);
                {
                    var j__prev2 = j;

                    foreach (var (__j, __pred) in b.Preds)
                    {
                        j = __j;
                        pred = __pred;
                        if (j > 0L)
                        {
                            fmt.Printf(",");
                        }
                        fmt.Printf("%d", pred.Block().ID);
                    }

                    j = j__prev2;
                }

                fmt.Printf(" succ=");
                {
                    var j__prev2 = j;

                    foreach (var (__j, __succ) in b.Succs)
                    {
                        j = __j;
                        succ = __succ;
                        if (j > 0L)
                        {
                            fmt.Printf(",");
                        }
                        fmt.Printf("%d", succ.Block().ID);
                    }

                    j = j__prev2;
                }

                fmt.Printf("\n");

                var be = lv.blockEffects(b); 

                // initial settings
                var printed = false;
                printed = lv.printbvec(printed, "uevar", be.uevar);
                printed = lv.printbvec(printed, "livein", be.livein);
                if (printed)
                {
                    fmt.Printf("\n");
                } 

                // program listing, with individual effects listed
                if (b == lv.f.Entry)
                {
                    var live = lv.livevars[pcdata];
                    fmt.Printf("(%s) function entry\n", linestr(lv.fn.Func.Nname.Pos));
                    fmt.Printf("\tlive=");
                    printed = false;
                    {
                        var j__prev2 = j;
                        var n__prev2 = n;

                        foreach (var (__j, __n) in lv.vars)
                        {
                            j = __j;
                            n = __n;
                            if (!live.Get(int32(j)))
                            {
                                continue;
                            }
                            if (printed)
                            {
                                fmt.Printf(",");
                            }
                            fmt.Printf("%v", n);
                            printed = true;
                        }

                        j = j__prev2;
                        n = n__prev2;
                    }

                    fmt.Printf("\n");
                }
                foreach (var (_, v) in b.Values)
                {
                    fmt.Printf("(%s) %v\n", linestr(v.Pos), v.LongString());

                    {
                        var pos__prev1 = pos;

                        var (pos, ok) = lv.stackMapIndex[v];

                        if (ok)
                        {
                            pcdata = pos;
                        }

                        pos = pos__prev1;

                    }

                    var (pos, effect) = lv.valueEffects(v);
                    printed = false;
                    printed = lv.printeffect(printed, "uevar", pos, effect & uevar != 0L);
                    printed = lv.printeffect(printed, "varkill", pos, effect & varkill != 0L);
                    printed = lv.printeffect(printed, "avarinit", pos, effect & avarinit != 0L);
                    if (printed)
                    {
                        fmt.Printf("\n");
                    }
                    if (!issafepoint(v))
                    {
                        continue;
                    }
                    live = lv.livevars[pcdata];
                    fmt.Printf("\tlive=");
                    printed = false;
                    {
                        var j__prev3 = j;
                        var n__prev3 = n;

                        foreach (var (__j, __n) in lv.vars)
                        {
                            j = __j;
                            n = __n;
                            if (!live.Get(int32(j)))
                            {
                                continue;
                            }
                            if (printed)
                            {
                                fmt.Printf(",");
                            }
                            fmt.Printf("%v", n);
                            printed = true;
                        }

                        j = j__prev3;
                        n = n__prev3;
                    }

                    fmt.Printf("\n");
                } 

                // bb bitsets
                fmt.Printf("end\n");
                printed = false;
                printed = lv.printbvec(printed, "varkill", be.varkill);
                printed = lv.printbvec(printed, "liveout", be.liveout);
                printed = lv.printbvec(printed, "avarinit", be.avarinit);
                printed = lv.printbvec(printed, "avarinitany", be.avarinitany);
                printed = lv.printbvec(printed, "avarinitall", be.avarinitall);
                if (printed)
                {
                    fmt.Printf("\n");
                }
            }
            fmt.Printf("\n");
        }

        // Dumps a slice of bitmaps to a symbol as a sequence of uint32 values. The
        // first word dumped is the total number of bitmaps. The second word is the
        // length of the bitmaps. All bitmaps are assumed to be of equal length. The
        // remaining bytes are the raw bitmaps.
        private static void emit(this ref Liveness lv, ref obj.LSym argssym, ref obj.LSym livesym)
        {
            var args = bvalloc(lv.argWords());
            var aoff = duint32(argssym, 0L, uint32(len(lv.livevars))); // number of bitmaps
            aoff = duint32(argssym, aoff, uint32(args.n)); // number of bits in each bitmap

            var locals = bvalloc(lv.localWords());
            var loff = duint32(livesym, 0L, uint32(len(lv.livevars))); // number of bitmaps
            loff = duint32(livesym, loff, uint32(locals.n)); // number of bits in each bitmap

            foreach (var (_, live) in lv.livevars)
            {
                args.Clear();
                locals.Clear();

                lv.pointerMap(live, lv.vars, args, locals);

                aoff = dbvec(argssym, aoff, args);
                loff = dbvec(livesym, loff, locals);
            } 

            // Give these LSyms content-addressable names,
            // so that they can be de-duplicated.
            // This provides significant binary size savings.
            // It is safe to rename these LSyms because
            // they are tracked separately from ctxt.hash.
            argssym.Name = fmt.Sprintf("gclocals·%x", md5.Sum(argssym.P));
            livesym.Name = fmt.Sprintf("gclocals·%x", md5.Sum(livesym.P));
        }

        // Entry pointer for liveness analysis. Solves for the liveness of
        // pointer variables in the function and emits a runtime data
        // structure read by the garbage collector.
        // Returns a map from GC safe points to their corresponding stack map index.
        private static map<ref ssa.Value, long> liveness(ref ssafn e, ref ssa.Func f)
        { 
            // Construct the global liveness state.
            var (vars, idx) = getvariables(e.curfn);
            var lv = newliveness(e.curfn, f, vars, idx, e.stkptrsize); 

            // Run the dataflow framework.
            lv.prologue();
            lv.solve();
            lv.epilogue();
            lv.compact();
            lv.clobber();
            if (debuglive >= 2L)
            {
                lv.printDebug();
            } 

            // Emit the live pointer map data structures
            {
                var ls = e.curfn.Func.lsym;

                if (ls != null)
                {
                    lv.emit(ref ls.Func.GCArgs, ref ls.Func.GCLocals);
                }

            }
            return lv.stackMapIndex;
        }
    }
}}}}
