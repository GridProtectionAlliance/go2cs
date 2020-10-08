// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:27 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\func.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using sha1 = go.crypto.sha1_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private partial interface writeSyncer : io.Writer
        {
            error Sync();
        }

        // A Func represents a Go func declaration (or function literal) and its body.
        // This package compiles each Func independently.
        // Funcs are single-use; a new Func must be created for every compiled function.
        public partial struct Func
        {
            public ptr<Config> Config; // architecture information
            public ptr<Cache> Cache; // re-usable cache
            public Frontend fe; // frontend state associated with this Func, callbacks into compiler frontend
            public ptr<pass> pass; // current pass information (name, options, etc.)
            public @string Name; // e.g. NewFunc or (*Func).NumBlocks (no package prefix)
            public ptr<types.Type> Type; // type signature of the function.
            public slice<ptr<Block>> Blocks; // unordered set of all basic blocks (note: not indexable by ID)
            public ptr<Block> Entry; // the entry basic block

// If we are using open-coded defers, this is the first call to a deferred
// function in the final defer exit sequence that we generated. This call
// should be after all defer statements, and will have all args, etc. of
// all defer calls as live. The liveness info of this call will be used
// for the deferreturn/ret segment generated for functions with open-coded
// defers.
            public ptr<Value> LastDeferExit;
            public idAlloc bid; // block ID allocator
            public idAlloc vid; // value ID allocator

// Given an environment variable used for debug hash match,
// what file (if any) receives the yes/no logging?
            public map<@string, writeSyncer> logfiles;
            public ptr<HTMLWriter> HTMLWriter; // html writer, for debugging
            public bool DebugTest; // default true unless $GOSSAHASH != ""; as a debugging aid, make new code conditional on this and use GOSSAHASH to binary search for failing cases
            public bool PrintOrHtmlSSA; // true if GOSSAFUNC matches, true even if fe.Log() (spew phase results to stdout) is false.
            public map<@string, long> ruleMatches; // number of times countRule was called during compilation for any given string

            public bool scheduled; // Values in Blocks are in final order
            public bool laidout; // Blocks are ordered
            public bool NoSplit; // true if function is marked as nosplit.  Used by schedule check pass.

// when register allocation is done, maps value ids to locations
            public slice<Location> RegAlloc; // map from LocalSlot to set of Values that we want to store in that slot.
            public map<LocalSlot, slice<ptr<Value>>> NamedValues; // Names is a copy of NamedValues.Keys. We keep a separate list
// of keys to make iteration order deterministic.
            public slice<LocalSlot> Names; // WBLoads is a list of Blocks that branch on the write
// barrier flag. Safe-points are disabled from the OpLoad that
// reads the write-barrier flag until the control flow rejoins
// below the two successors of this block.
            public slice<ptr<Block>> WBLoads;
            public ptr<Value> freeValues; // free Values linked by argstorage[0].  All other fields except ID are 0/nil.
            public ptr<Block> freeBlocks; // free Blocks linked by succstorage[0].b.  All other fields except ID are 0/nil.

            public slice<ptr<Block>> cachedPostorder; // cached postorder traversal
            public slice<ptr<Block>> cachedIdom; // cached immediate dominators
            public SparseTree cachedSdom; // cached dominator tree
            public ptr<loopnest> cachedLoopnest; // cached loop nest information
            public ptr<xposmap> cachedLineStarts; // cached map/set of xpos to integers

            public auxmap auxmap; // map from aux values to opaque ids used by CSE
            public map<long, slice<ptr<Value>>> constants; // constants cache, keyed by constant value; users must check value's Op and Type
        }

        // NewFunc returns a new, empty function object.
        // Caller must set f.Config and f.Cache before using f.
        public static ptr<Func> NewFunc(Frontend fe)
        {
            return addr(new Func(fe:fe,NamedValues:make(map[LocalSlot][]*Value)));
        }

        // NumBlocks returns an integer larger than the id of any Block in the Func.
        private static long NumBlocks(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.bid.num();
        }

        // NumValues returns an integer larger than the id of any Value in the Func.
        private static long NumValues(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.vid.num();
        }

        // newSparseSet returns a sparse set that can store at least up to n integers.
        private static ptr<sparseSet> newSparseSet(this ptr<Func> _addr_f, long n)
        {
            ref Func f = ref _addr_f.val;

            foreach (var (i, scr) in f.Cache.scrSparseSet)
            {
                if (scr != null && scr.cap() >= n)
                {
                    f.Cache.scrSparseSet[i] = null;
                    scr.clear();
                    return _addr_scr!;
                }

            }
            return _addr_newSparseSet(n)!;

        }

        // retSparseSet returns a sparse set to the config's cache of sparse
        // sets to be reused by f.newSparseSet.
        private static void retSparseSet(this ptr<Func> _addr_f, ptr<sparseSet> _addr_ss)
        {
            ref Func f = ref _addr_f.val;
            ref sparseSet ss = ref _addr_ss.val;

            foreach (var (i, scr) in f.Cache.scrSparseSet)
            {
                if (scr == null)
                {
                    f.Cache.scrSparseSet[i] = ss;
                    return ;
                }

            }
            f.Cache.scrSparseSet = append(f.Cache.scrSparseSet, ss);

        }

        // newSparseMap returns a sparse map that can store at least up to n integers.
        private static ptr<sparseMap> newSparseMap(this ptr<Func> _addr_f, long n)
        {
            ref Func f = ref _addr_f.val;

            foreach (var (i, scr) in f.Cache.scrSparseMap)
            {
                if (scr != null && scr.cap() >= n)
                {
                    f.Cache.scrSparseMap[i] = null;
                    scr.clear();
                    return _addr_scr!;
                }

            }
            return _addr_newSparseMap(n)!;

        }

        // retSparseMap returns a sparse map to the config's cache of sparse
        // sets to be reused by f.newSparseMap.
        private static void retSparseMap(this ptr<Func> _addr_f, ptr<sparseMap> _addr_ss)
        {
            ref Func f = ref _addr_f.val;
            ref sparseMap ss = ref _addr_ss.val;

            foreach (var (i, scr) in f.Cache.scrSparseMap)
            {
                if (scr == null)
                {
                    f.Cache.scrSparseMap[i] = ss;
                    return ;
                }

            }
            f.Cache.scrSparseMap = append(f.Cache.scrSparseMap, ss);

        }

        // newPoset returns a new poset from the internal cache
        private static ptr<poset> newPoset(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (len(f.Cache.scrPoset) > 0L)
            {
                var po = f.Cache.scrPoset[len(f.Cache.scrPoset) - 1L];
                f.Cache.scrPoset = f.Cache.scrPoset[..len(f.Cache.scrPoset) - 1L];
                return _addr_po!;
            }

            return _addr_newPoset()!;

        }

        // retPoset returns a poset to the internal cache
        private static void retPoset(this ptr<Func> _addr_f, ptr<poset> _addr_po)
        {
            ref Func f = ref _addr_f.val;
            ref poset po = ref _addr_po.val;

            f.Cache.scrPoset = append(f.Cache.scrPoset, po);
        }

        // newDeadcodeLive returns a slice for the
        // deadcode pass to use to indicate which values are live.
        private static slice<bool> newDeadcodeLive(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var r = f.Cache.deadcode.live;
            f.Cache.deadcode.live = null;
            return r;
        }

        // retDeadcodeLive returns a deadcode live value slice for re-use.
        private static void retDeadcodeLive(this ptr<Func> _addr_f, slice<bool> live)
        {
            ref Func f = ref _addr_f.val;

            f.Cache.deadcode.live = live;
        }

        // newDeadcodeLiveOrderStmts returns a slice for the
        // deadcode pass to use to indicate which values
        // need special treatment for statement boundaries.
        private static slice<ptr<Value>> newDeadcodeLiveOrderStmts(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var r = f.Cache.deadcode.liveOrderStmts;
            f.Cache.deadcode.liveOrderStmts = null;
            return r;
        }

        // retDeadcodeLiveOrderStmts returns a deadcode liveOrderStmts slice for re-use.
        private static void retDeadcodeLiveOrderStmts(this ptr<Func> _addr_f, slice<ptr<Value>> liveOrderStmts)
        {
            ref Func f = ref _addr_f.val;

            f.Cache.deadcode.liveOrderStmts = liveOrderStmts;
        }

        // newValue allocates a new Value with the given fields and places it at the end of b.Values.
        private static ptr<Value> newValue(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, ptr<Block> _addr_b, src.XPos pos)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;
            ref Block b = ref _addr_b.val;

            ptr<Value> v;
            if (f.freeValues != null)
            {
                v = f.freeValues;
                f.freeValues = v.argstorage[0L];
                v.argstorage[0L] = null;
            }
            else
            {
                var ID = f.vid.get();
                if (int(ID) < len(f.Cache.values))
                {
                    v = _addr_f.Cache.values[ID];
                    v.ID = ID;
                }
                else
                {
                    v = addr(new Value(ID:ID));
                }

            }

            v.Op = op;
            v.Type = t;
            v.Block = b;
            if (notStmtBoundary(op))
            {
                pos = pos.WithNotStmt();
            }

            v.Pos = pos;
            b.Values = append(b.Values, v);
            return _addr_v!;

        }

        // newValueNoBlock allocates a new Value with the given fields.
        // The returned value is not placed in any block.  Once the caller
        // decides on a block b, it must set b.Block and append
        // the returned value to b.Values.
        private static ptr<Value> newValueNoBlock(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, src.XPos pos)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            ptr<Value> v;
            if (f.freeValues != null)
            {
                v = f.freeValues;
                f.freeValues = v.argstorage[0L];
                v.argstorage[0L] = null;
            }
            else
            {
                var ID = f.vid.get();
                if (int(ID) < len(f.Cache.values))
                {
                    v = _addr_f.Cache.values[ID];
                    v.ID = ID;
                }
                else
                {
                    v = addr(new Value(ID:ID));
                }

            }

            v.Op = op;
            v.Type = t;
            v.Block = null; // caller must fix this.
            if (notStmtBoundary(op))
            {
                pos = pos.WithNotStmt();
            }

            v.Pos = pos;
            return _addr_v!;

        }

        // logPassStat writes a string key and int value as a warning in a
        // tab-separated format easily handled by spreadsheets or awk.
        // file names, lines, and function names are included to provide enough (?)
        // context to allow item-by-item comparisons across runs.
        // For example:
        // awk 'BEGIN {FS="\t"} $3~/TIME/{sum+=$4} END{print "t(ns)=",sum}' t.log
        private static void LogStat(this ptr<Func> _addr_f, @string key, params object[] args)
        {
            args = args.Clone();
            ref Func f = ref _addr_f.val;

            @string value = "";
            foreach (var (_, a) in args)
            {
                value += fmt.Sprintf("\t%v", a);
            }
            @string n = "missing_pass";
            if (f.pass != null)
            {
                n = strings.Replace(f.pass.name, " ", "_", -1L);
            }

            f.Warnl(f.Entry.Pos, "\t%s\t%s%s\t%s", n, key, value, f.Name);

        }

        // freeValue frees a value. It must no longer be referenced or have any args.
        private static void freeValue(this ptr<Func> _addr_f, ptr<Value> _addr_v)
        {
            ref Func f = ref _addr_f.val;
            ref Value v = ref _addr_v.val;

            if (v.Block == null)
            {
                f.Fatalf("trying to free an already freed value");
            }

            if (v.Uses != 0L)
            {
                f.Fatalf("value %s still has %d uses", v, v.Uses);
            }

            if (len(v.Args) != 0L)
            {
                f.Fatalf("value %s still has %d args", v, len(v.Args));
            } 
            // Clear everything but ID (which we reuse).
            var id = v.ID; 

            // Values with zero arguments and OpOffPtr values might be cached, so remove them there.
            var nArgs = opcodeTable[v.Op].argLen;
            if (nArgs == 0L || v.Op == OpOffPtr)
            {
                var vv = f.constants[v.AuxInt];
                foreach (var (i, cv) in vv)
                {
                    if (v == cv)
                    {
                        vv[i] = vv[len(vv) - 1L];
                        vv[len(vv) - 1L] = null;
                        f.constants[v.AuxInt] = vv[0L..len(vv) - 1L];
                        break;
                    }

                }

            }

            v = new Value();
            v.ID = id;
            v.argstorage[0L] = f.freeValues;
            f.freeValues = v;

        }

        // newBlock allocates a new Block of the given kind and places it at the end of f.Blocks.
        private static ptr<Block> NewBlock(this ptr<Func> _addr_f, BlockKind kind)
        {
            ref Func f = ref _addr_f.val;

            ptr<Block> b;
            if (f.freeBlocks != null)
            {
                b = f.freeBlocks;
                f.freeBlocks = b.succstorage[0L].b;
                b.succstorage[0L].b = null;
            }
            else
            {
                var ID = f.bid.get();
                if (int(ID) < len(f.Cache.blocks))
                {
                    b = _addr_f.Cache.blocks[ID];
                    b.ID = ID;
                }
                else
                {
                    b = addr(new Block(ID:ID));
                }

            }

            b.Kind = kind;
            b.Func = f;
            b.Preds = b.predstorage[..0L];
            b.Succs = b.succstorage[..0L];
            b.Values = b.valstorage[..0L];
            f.Blocks = append(f.Blocks, b);
            f.invalidateCFG();
            return _addr_b!;

        }

        private static void freeBlock(this ptr<Func> _addr_f, ptr<Block> _addr_b)
        {
            ref Func f = ref _addr_f.val;
            ref Block b = ref _addr_b.val;

            if (b.Func == null)
            {
                f.Fatalf("trying to free an already freed block");
            } 
            // Clear everything but ID (which we reuse).
            var id = b.ID;
            b = new Block();
            b.ID = id;
            b.succstorage[0L].b = f.freeBlocks;
            f.freeBlocks = b;

        }

        // NewValue0 returns a new value in the block with no arguments and zero aux values.
        private static ptr<Value> NewValue0(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..0L];
            return _addr_v!;
        }

        // NewValue returns a new value in the block with no arguments and an auxint value.
        private static ptr<Value> NewValue0I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..0L];
            return _addr_v!;
        }

        // NewValue returns a new value in the block with no arguments and an aux value.
        private static ptr<Value> NewValue0A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, object aux)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;

            {
                long (_, ok) = aux._<long>();

                if (ok)
                { 
                    // Disallow int64 aux values. They should be in the auxint field instead.
                    // Maybe we want to allow this at some point, but for now we disallow it
                    // to prevent errors like using NewValue1A instead of NewValue1I.
                    b.Fatalf("aux field has int64 type op=%s type=%s aux=%v", op, t, aux);

                }

            }

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Aux = aux;
            v.Args = v.argstorage[..0L];
            return _addr_v!;

        }

        // NewValue returns a new value in the block with no arguments and both an auxint and aux values.
        private static ptr<Value> NewValue0IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, object aux)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Aux = aux;
            v.Args = v.argstorage[..0L];
            return _addr_v!;
        }

        // NewValue1 returns a new value in the block with one argument and zero aux values.
        private static ptr<Value> NewValue1(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg = ref _addr_arg.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return _addr_v!;
        }

        // NewValue1I returns a new value in the block with one argument and an auxint value.
        private static ptr<Value> NewValue1I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg = ref _addr_arg.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return _addr_v!;
        }

        // NewValue1A returns a new value in the block with one argument and an aux value.
        private static ptr<Value> NewValue1A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, object aux, ptr<Value> _addr_arg)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg = ref _addr_arg.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Aux = aux;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return _addr_v!;
        }

        // NewValue1IA returns a new value in the block with one argument and both an auxint and aux values.
        private static ptr<Value> NewValue1IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, object aux, ptr<Value> _addr_arg)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg = ref _addr_arg.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Aux = aux;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return _addr_v!;
        }

        // NewValue2 returns a new value in the block with two arguments and zero aux values.
        private static ptr<Value> NewValue2(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return _addr_v!;
        }

        // NewValue2A returns a new value in the block with two arguments and one aux values.
        private static ptr<Value> NewValue2A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, object aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Aux = aux;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return _addr_v!;
        }

        // NewValue2I returns a new value in the block with two arguments and an auxint value.
        private static ptr<Value> NewValue2I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return _addr_v!;
        }

        // NewValue2IA returns a new value in the block with two arguments and both an auxint and aux values.
        private static ptr<Value> NewValue2IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, object aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Aux = aux;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return _addr_v!;
        }

        // NewValue3 returns a new value in the block with three arguments and zero aux values.
        private static ptr<Value> NewValue3(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;
            ref Value arg2 = ref _addr_arg2.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..3L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            v.argstorage[2L] = arg2;
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            return _addr_v!;
        }

        // NewValue3I returns a new value in the block with three arguments and an auxint value.
        private static ptr<Value> NewValue3I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;
            ref Value arg2 = ref _addr_arg2.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..3L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            v.argstorage[2L] = arg2;
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            return _addr_v!;
        }

        // NewValue3A returns a new value in the block with three argument and an aux value.
        private static ptr<Value> NewValue3A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, object aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;
            ref Value arg2 = ref _addr_arg2.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Aux = aux;
            v.Args = v.argstorage[..3L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            v.argstorage[2L] = arg2;
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            return _addr_v!;
        }

        // NewValue4 returns a new value in the block with four arguments and zero aux values.
        private static ptr<Value> NewValue4(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2, ptr<Value> _addr_arg3)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;
            ref Value arg2 = ref _addr_arg2.val;
            ref Value arg3 = ref _addr_arg3.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = new slice<ptr<Value>>(new ptr<Value>[] { arg0, arg1, arg2, arg3 });
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            arg3.Uses++;
            return _addr_v!;
        }

        // NewValue4I returns a new value in the block with four arguments and and auxint value.
        private static ptr<Value> NewValue4I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2, ptr<Value> _addr_arg3)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;
            ref Value arg0 = ref _addr_arg0.val;
            ref Value arg1 = ref _addr_arg1.val;
            ref Value arg2 = ref _addr_arg2.val;
            ref Value arg3 = ref _addr_arg3.val;

            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = new slice<ptr<Value>>(new ptr<Value>[] { arg0, arg1, arg2, arg3 });
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            arg3.Uses++;
            return _addr_v!;
        }

        // constVal returns a constant value for c.
        private static ptr<Value> constVal(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, long c, bool setAuxInt) => func((_, panic, __) =>
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            if (f.constants == null)
            {
                f.constants = make_map<long, slice<ptr<Value>>>();
            }

            var vv = f.constants[c];
            {
                var v__prev1 = v;

                foreach (var (_, __v) in vv)
                {
                    v = __v;
                    if (v.Op == op && v.Type.Compare(t) == types.CMPeq)
                    {
                        if (setAuxInt && v.AuxInt != c)
                        {
                            panic(fmt.Sprintf("cached const %s should have AuxInt of %d", v.LongString(), c));
                        }

                        return _addr_v!;

                    }

                }

                v = v__prev1;
            }

            ptr<Value> v;
            if (setAuxInt)
            {
                v = f.Entry.NewValue0I(src.NoXPos, op, t, c);
            }
            else
            {
                v = f.Entry.NewValue0(src.NoXPos, op, t);
            }

            f.constants[c] = append(vv, v);
            return _addr_v!;

        });

        // These magic auxint values let us easily cache non-numeric constants
        // using the same constants map while making collisions unlikely.
        // These values are unlikely to occur in regular code and
        // are easy to grep for in case of bugs.
        private static readonly long constSliceMagic = (long)1122334455L;
        private static readonly long constInterfaceMagic = (long)2233445566L;
        private static readonly long constNilMagic = (long)3344556677L;
        private static readonly long constEmptyStringMagic = (long)4455667788L;


        // ConstInt returns an int constant representing its argument.
        private static ptr<Value> ConstBool(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, bool c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            var i = int64(0L);
            if (c)
            {
                i = 1L;
            }

            return _addr_f.constVal(OpConstBool, t, i, true)!;

        }
        private static ptr<Value> ConstInt8(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, sbyte c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst8, t, int64(c), true)!;
        }
        private static ptr<Value> ConstInt16(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, short c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst16, t, int64(c), true)!;
        }
        private static ptr<Value> ConstInt32(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, int c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst32, t, int64(c), true)!;
        }
        private static ptr<Value> ConstInt64(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, long c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst64, t, c, true)!;
        }
        private static ptr<Value> ConstFloat32(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, double c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst32F, t, int64(math.Float64bits(float64(float32(c)))), true)!;
        }
        private static ptr<Value> ConstFloat64(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, double c)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConst64F, t, int64(math.Float64bits(c)), true)!;
        }

        private static ptr<Value> ConstSlice(this ptr<Func> _addr_f, ptr<types.Type> _addr_t)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConstSlice, t, constSliceMagic, false)!;
        }
        private static ptr<Value> ConstInterface(this ptr<Func> _addr_f, ptr<types.Type> _addr_t)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConstInterface, t, constInterfaceMagic, false)!;
        }
        private static ptr<Value> ConstNil(this ptr<Func> _addr_f, ptr<types.Type> _addr_t)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_f.constVal(OpConstNil, t, constNilMagic, false)!;
        }
        private static ptr<Value> ConstEmptyString(this ptr<Func> _addr_f, ptr<types.Type> _addr_t)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;

            var v = f.constVal(OpConstString, t, constEmptyStringMagic, false);
            v.Aux = "";
            return _addr_v!;
        }
        private static ptr<Value> ConstOffPtrSP(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, long c, ptr<Value> _addr_sp)
        {
            ref Func f = ref _addr_f.val;
            ref types.Type t = ref _addr_t.val;
            ref Value sp = ref _addr_sp.val;

            var v = f.constVal(OpOffPtr, t, c, true);
            if (len(v.Args) == 0L)
            {
                v.AddArg(sp);
            }

            return _addr_v!;


        }

        private static Frontend Frontend(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.fe;
        }
        private static void Warnl(this ptr<Func> _addr_f, src.XPos pos, @string msg, params object[] args)
        {
            args = args.Clone();
            ref Func f = ref _addr_f.val;

            f.fe.Warnl(pos, msg, args);
        }
        private static void Logf(this ptr<Func> _addr_f, @string msg, params object[] args)
        {
            args = args.Clone();
            ref Func f = ref _addr_f.val;

            f.fe.Logf(msg, args);
        }
        private static bool Log(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.fe.Log();
        }
        private static void Fatalf(this ptr<Func> _addr_f, @string msg, params object[] args)
        {
            args = args.Clone();
            ref Func f = ref _addr_f.val;

            f.fe.Fatalf(f.Entry.Pos, msg, args);
        }

        // postorder returns the reachable blocks in f in a postorder traversal.
        private static slice<ptr<Block>> postorder(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (f.cachedPostorder == null)
            {
                f.cachedPostorder = postorder(f);
            }

            return f.cachedPostorder;

        }

        private static slice<ptr<Block>> Postorder(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return f.postorder();
        }

        // Idom returns a map from block ID to the immediate dominator of that block.
        // f.Entry.ID maps to nil. Unreachable blocks map to nil as well.
        private static slice<ptr<Block>> Idom(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (f.cachedIdom == null)
            {
                f.cachedIdom = dominators(f);
            }

            return f.cachedIdom;

        }

        // sdom returns a sparse tree representing the dominator relationships
        // among the blocks of f.
        private static SparseTree Sdom(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (f.cachedSdom == null)
            {
                f.cachedSdom = newSparseTree(f, f.Idom());
            }

            return f.cachedSdom;

        }

        // loopnest returns the loop nest information for f.
        private static ptr<loopnest> loopnest(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (f.cachedLoopnest == null)
            {
                f.cachedLoopnest = loopnestfor(f);
            }

            return _addr_f.cachedLoopnest!;

        }

        // invalidateCFG tells f that its CFG has changed.
        private static void invalidateCFG(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            f.cachedPostorder = null;
            f.cachedIdom = null;
            f.cachedSdom = null;
            f.cachedLoopnest = null;
        }

        // DebugHashMatch reports whether environment variable evname
        // 1) is empty (this is a special more-quickly implemented case of 3)
        // 2) is "y" or "Y"
        // 3) is a suffix of the sha1 hash of name
        // 4) is a suffix of the environment variable
        //    fmt.Sprintf("%s%d", evname, n)
        //    provided that all such variables are nonempty for 0 <= i <= n
        // Otherwise it returns false.
        // When true is returned the message
        //  "%s triggered %s\n", evname, name
        // is printed on the file named in environment variable
        //  GSHS_LOGFILE
        // or standard out if that is empty or there is an error
        // opening the file.
        private static bool DebugHashMatch(this ptr<Func> _addr_f, @string evname, @string name)
        {
            ref Func f = ref _addr_f.val;

            var evhash = os.Getenv(evname);
            switch (evhash)
            {
                case "": 
                    return true; // default behavior with no EV is "on"
                    break;
                case "y": 

                case "Y": 
                    f.logDebugHashMatch(evname, name);
                    return true;
                    break;
                case "n": 

                case "N": 
                    return false;
                    break;
            } 
            // Check the hash of the name against a partial input hash.
            // We use this feature to do a binary search to
            // find a function that is incorrectly compiled.
            @string hstr = "";
            foreach (var (_, b) in sha1.Sum((slice<byte>)name))
            {
                hstr += fmt.Sprintf("%08b", b);
            }
            if (strings.HasSuffix(hstr, evhash))
            {
                f.logDebugHashMatch(evname, name);
                return true;
            } 

            // Iteratively try additional hashes to allow tests for multi-point
            // failure.
            for (long i = 0L; true; i++)
            {
                var ev = fmt.Sprintf("%s%d", evname, i);
                var evv = os.Getenv(ev);
                if (evv == "")
                {
                    break;
                }

                if (strings.HasSuffix(hstr, evv))
                {
                    f.logDebugHashMatch(ev, name);
                    return true;
                }

            }

            return false;

        }

        private static void logDebugHashMatch(this ptr<Func> _addr_f, @string evname, @string name)
        {
            ref Func f = ref _addr_f.val;

            if (f.logfiles == null)
            {
                f.logfiles = make_map<@string, writeSyncer>();
            }

            var file = f.logfiles[evname];
            if (file == null)
            {
                file = os.Stdout;
                {
                    var tmpfile = os.Getenv("GSHS_LOGFILE");

                    if (tmpfile != "")
                    {
                        error err = default!;
                        file, err = os.Create(tmpfile);
                        if (err != null)
                        {
                            f.Fatalf("could not open hash-testing logfile %s", tmpfile);
                        }

                    }

                }

                f.logfiles[evname] = file;

            }

            fmt.Fprintf(file, "%s triggered %s\n", evname, name);
            file.Sync();

        }

        public static bool DebugNameMatch(@string evname, @string name)
        {
            return os.Getenv(evname) == name;
        }
    }
}}}}
