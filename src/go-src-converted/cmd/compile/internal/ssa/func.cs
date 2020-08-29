// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:51 UTC
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
            public @string Name; // e.g. bytes·Compare
            public ptr<types.Type> Type; // type signature of the function.
            public slice<ref Block> Blocks; // unordered set of all basic blocks (note: not indexable by ID)
            public ptr<Block> Entry; // the entry basic block
            public idAlloc bid; // block ID allocator
            public idAlloc vid; // value ID allocator

// Given an environment variable used for debug hash match,
// what file (if any) receives the yes/no logging?
            public map<@string, writeSyncer> logfiles;
            public ptr<HTMLWriter> HTMLWriter; // html writer, for debugging
            public bool DebugTest; // default true unless $GOSSAHASH != ""; as a debugging aid, make new code conditional on this and use GOSSAHASH to binary search for failing cases

            public bool scheduled; // Values in Blocks are in final order
            public bool NoSplit; // true if function is marked as nosplit.  Used by schedule check pass.

// when register allocation is done, maps value ids to locations
            public slice<Location> RegAlloc; // map from LocalSlot to set of Values that we want to store in that slot.
            public map<LocalSlot, slice<ref Value>> NamedValues; // Names is a copy of NamedValues.Keys. We keep a separate list
// of keys to make iteration order deterministic.
            public slice<LocalSlot> Names;
            public ptr<Value> freeValues; // free Values linked by argstorage[0].  All other fields except ID are 0/nil.
            public ptr<Block> freeBlocks; // free Blocks linked by succstorage[0].b.  All other fields except ID are 0/nil.

            public slice<ref Block> cachedPostorder; // cached postorder traversal
            public slice<ref Block> cachedIdom; // cached immediate dominators
            public SparseTree cachedSdom; // cached dominator tree
            public ptr<loopnest> cachedLoopnest; // cached loop nest information

            public auxmap auxmap; // map from aux values to opaque ids used by CSE

            public map<long, slice<ref Value>> constants; // constants cache, keyed by constant value; users must check value's Op and Type
        }

        // NewFunc returns a new, empty function object.
        // Caller must set f.Config and f.Cache before using f.
        public static ref Func NewFunc(Frontend fe)
        {
            return ref new Func(fe:fe,NamedValues:make(map[LocalSlot][]*Value));
        }

        // NumBlocks returns an integer larger than the id of any Block in the Func.
        private static long NumBlocks(this ref Func f)
        {
            return f.bid.num();
        }

        // NumValues returns an integer larger than the id of any Value in the Func.
        private static long NumValues(this ref Func f)
        {
            return f.vid.num();
        }

        // newSparseSet returns a sparse set that can store at least up to n integers.
        private static ref sparseSet newSparseSet(this ref Func f, long n)
        {
            foreach (var (i, scr) in f.Cache.scrSparse)
            {
                if (scr != null && scr.cap() >= n)
                {
                    f.Cache.scrSparse[i] = null;
                    scr.clear();
                    return scr;
                }
            }
            return newSparseSet(n);
        }

        // retSparseSet returns a sparse set to the config's cache of sparse sets to be reused by f.newSparseSet.
        private static void retSparseSet(this ref Func f, ref sparseSet ss)
        {
            foreach (var (i, scr) in f.Cache.scrSparse)
            {
                if (scr == null)
                {
                    f.Cache.scrSparse[i] = ss;
                    return;
                }
            }
            f.Cache.scrSparse = append(f.Cache.scrSparse, ss);
        }

        // newValue allocates a new Value with the given fields and places it at the end of b.Values.
        private static ref Value newValue(this ref Func f, Op op, ref types.Type t, ref Block b, src.XPos pos)
        {
            ref Value v = default;
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
                    v = ref f.Cache.values[ID];
                    v.ID = ID;
                }
                else
                {
                    v = ref new Value(ID:ID);
                }
            }
            v.Op = op;
            v.Type = t;
            v.Block = b;
            v.Pos = pos;
            b.Values = append(b.Values, v);
            return v;
        }

        // newValueNoBlock allocates a new Value with the given fields.
        // The returned value is not placed in any block.  Once the caller
        // decides on a block b, it must set b.Block and append
        // the returned value to b.Values.
        private static ref Value newValueNoBlock(this ref Func f, Op op, ref types.Type t, src.XPos pos)
        {
            ref Value v = default;
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
                    v = ref f.Cache.values[ID];
                    v.ID = ID;
                }
                else
                {
                    v = ref new Value(ID:ID);
                }
            }
            v.Op = op;
            v.Type = t;
            v.Block = null; // caller must fix this.
            v.Pos = pos;
            return v;
        }

        // logPassStat writes a string key and int value as a warning in a
        // tab-separated format easily handled by spreadsheets or awk.
        // file names, lines, and function names are included to provide enough (?)
        // context to allow item-by-item comparisons across runs.
        // For example:
        // awk 'BEGIN {FS="\t"} $3~/TIME/{sum+=$4} END{print "t(ns)=",sum}' t.log
        private static void LogStat(this ref Func f, @string key, params object[] args)
        {
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
        private static void freeValue(this ref Func f, ref Value v)
        {
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
            v.Value = new Value();
            v.ID = id;
            v.argstorage[0L] = f.freeValues;
            f.freeValues = v;
        }

        // newBlock allocates a new Block of the given kind and places it at the end of f.Blocks.
        private static ref Block NewBlock(this ref Func f, BlockKind kind)
        {
            ref Block b = default;
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
                    b = ref f.Cache.blocks[ID];
                    b.ID = ID;
                }
                else
                {
                    b = ref new Block(ID:ID);
                }
            }
            b.Kind = kind;
            b.Func = f;
            b.Preds = b.predstorage[..0L];
            b.Succs = b.succstorage[..0L];
            b.Values = b.valstorage[..0L];
            f.Blocks = append(f.Blocks, b);
            f.invalidateCFG();
            return b;
        }

        private static void freeBlock(this ref Func f, ref Block b)
        {
            if (b.Func == null)
            {
                f.Fatalf("trying to free an already freed block");
            } 
            // Clear everything but ID (which we reuse).
            var id = b.ID;
            b.Value = new Block();
            b.ID = id;
            b.succstorage[0L].b = f.freeBlocks;
            f.freeBlocks = b;
        }

        // NewValue0 returns a new value in the block with no arguments and zero aux values.
        private static ref Value NewValue0(this ref Block b, src.XPos pos, Op op, ref types.Type t)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..0L];
            return v;
        }

        // NewValue returns a new value in the block with no arguments and an auxint value.
        private static ref Value NewValue0I(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..0L];
            return v;
        }

        // NewValue returns a new value in the block with no arguments and an aux value.
        private static ref Value NewValue0A(this ref Block b, src.XPos pos, Op op, ref types.Type t, object aux)
        {
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
            return v;
        }

        // NewValue returns a new value in the block with no arguments and both an auxint and aux values.
        private static ref Value NewValue0IA(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint, object aux)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Aux = aux;
            v.Args = v.argstorage[..0L];
            return v;
        }

        // NewValue1 returns a new value in the block with one argument and zero aux values.
        private static ref Value NewValue1(this ref Block b, src.XPos pos, Op op, ref types.Type t, ref Value arg)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return v;
        }

        // NewValue1I returns a new value in the block with one argument and an auxint value.
        private static ref Value NewValue1I(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint, ref Value arg)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return v;
        }

        // NewValue1A returns a new value in the block with one argument and an aux value.
        private static ref Value NewValue1A(this ref Block b, src.XPos pos, Op op, ref types.Type t, object aux, ref Value arg)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Aux = aux;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return v;
        }

        // NewValue1IA returns a new value in the block with one argument and both an auxint and aux values.
        private static ref Value NewValue1IA(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint, object aux, ref Value arg)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Aux = aux;
            v.Args = v.argstorage[..1L];
            v.argstorage[0L] = arg;
            arg.Uses++;
            return v;
        }

        // NewValue2 returns a new value in the block with two arguments and zero aux values.
        private static ref Value NewValue2(this ref Block b, src.XPos pos, Op op, ref types.Type t, ref Value arg0, ref Value arg1)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return v;
        }

        // NewValue2I returns a new value in the block with two arguments and an auxint value.
        private static ref Value NewValue2I(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint, ref Value arg0, ref Value arg1)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..2L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            arg0.Uses++;
            arg1.Uses++;
            return v;
        }

        // NewValue3 returns a new value in the block with three arguments and zero aux values.
        private static ref Value NewValue3(this ref Block b, src.XPos pos, Op op, ref types.Type t, ref Value arg0, ref Value arg1, ref Value arg2)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = v.argstorage[..3L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            v.argstorage[2L] = arg2;
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            return v;
        }

        // NewValue3I returns a new value in the block with three arguments and an auxint value.
        private static ref Value NewValue3I(this ref Block b, src.XPos pos, Op op, ref types.Type t, long auxint, ref Value arg0, ref Value arg1, ref Value arg2)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = auxint;
            v.Args = v.argstorage[..3L];
            v.argstorage[0L] = arg0;
            v.argstorage[1L] = arg1;
            v.argstorage[2L] = arg2;
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            return v;
        }

        // NewValue3A returns a new value in the block with three argument and an aux value.
        private static ref Value NewValue3A(this ref Block b, src.XPos pos, Op op, ref types.Type t, object aux, ref Value arg0, ref Value arg1, ref Value arg2)
        {
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
            return v;
        }

        // NewValue4 returns a new value in the block with four arguments and zero aux values.
        private static ref Value NewValue4(this ref Block b, src.XPos pos, Op op, ref types.Type t, ref Value arg0, ref Value arg1, ref Value arg2, ref Value arg3)
        {
            var v = b.Func.newValue(op, t, b, pos);
            v.AuxInt = 0L;
            v.Args = new slice<ref Value>(new ref Value[] { arg0, arg1, arg2, arg3 });
            arg0.Uses++;
            arg1.Uses++;
            arg2.Uses++;
            arg3.Uses++;
            return v;
        }

        // constVal returns a constant value for c.
        private static ref Value constVal(this ref Func _f, src.XPos pos, Op op, ref types.Type _t, long c, bool setAuxInt) => func(_f, _t, (ref Func f, ref types.Type t, Defer _, Panic panic, Recover __) =>
        { 
            // TODO remove unused pos parameter, both here and in *func.ConstXXX callers.
            if (f.constants == null)
            {
                f.constants = make_map<long, slice<ref Value>>();
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
                        return v;
                    }
                }

                v = v__prev1;
            }

            ref Value v = default;
            if (setAuxInt)
            {
                v = f.Entry.NewValue0I(src.NoXPos, op, t, c);
            }
            else
            {
                v = f.Entry.NewValue0(src.NoXPos, op, t);
            }
            f.constants[c] = append(vv, v);
            return v;
        });

        // These magic auxint values let us easily cache non-numeric constants
        // using the same constants map while making collisions unlikely.
        // These values are unlikely to occur in regular code and
        // are easy to grep for in case of bugs.
        private static readonly long constSliceMagic = 1122334455L;
        private static readonly long constInterfaceMagic = 2233445566L;
        private static readonly long constNilMagic = 3344556677L;
        private static readonly long constEmptyStringMagic = 4455667788L;

        // ConstInt returns an int constant representing its argument.
        private static ref Value ConstBool(this ref Func f, src.XPos pos, ref types.Type t, bool c)
        {
            var i = int64(0L);
            if (c)
            {
                i = 1L;
            }
            return f.constVal(pos, OpConstBool, t, i, true);
        }
        private static ref Value ConstInt8(this ref Func f, src.XPos pos, ref types.Type t, sbyte c)
        {
            return f.constVal(pos, OpConst8, t, int64(c), true);
        }
        private static ref Value ConstInt16(this ref Func f, src.XPos pos, ref types.Type t, short c)
        {
            return f.constVal(pos, OpConst16, t, int64(c), true);
        }
        private static ref Value ConstInt32(this ref Func f, src.XPos pos, ref types.Type t, int c)
        {
            return f.constVal(pos, OpConst32, t, int64(c), true);
        }
        private static ref Value ConstInt64(this ref Func f, src.XPos pos, ref types.Type t, long c)
        {
            return f.constVal(pos, OpConst64, t, c, true);
        }
        private static ref Value ConstFloat32(this ref Func f, src.XPos pos, ref types.Type t, double c)
        {
            return f.constVal(pos, OpConst32F, t, int64(math.Float64bits(float64(float32(c)))), true);
        }
        private static ref Value ConstFloat64(this ref Func f, src.XPos pos, ref types.Type t, double c)
        {
            return f.constVal(pos, OpConst64F, t, int64(math.Float64bits(c)), true);
        }

        private static ref Value ConstSlice(this ref Func f, src.XPos pos, ref types.Type t)
        {
            return f.constVal(pos, OpConstSlice, t, constSliceMagic, false);
        }
        private static ref Value ConstInterface(this ref Func f, src.XPos pos, ref types.Type t)
        {
            return f.constVal(pos, OpConstInterface, t, constInterfaceMagic, false);
        }
        private static ref Value ConstNil(this ref Func f, src.XPos pos, ref types.Type t)
        {
            return f.constVal(pos, OpConstNil, t, constNilMagic, false);
        }
        private static ref Value ConstEmptyString(this ref Func f, src.XPos pos, ref types.Type t)
        {
            var v = f.constVal(pos, OpConstString, t, constEmptyStringMagic, false);
            v.Aux = "";
            return v;
        }
        private static ref Value ConstOffPtrSP(this ref Func f, src.XPos pos, ref types.Type t, long c, ref Value sp)
        {
            var v = f.constVal(pos, OpOffPtr, t, c, true);
            if (len(v.Args) == 0L)
            {
                v.AddArg(sp);
            }
            return v;

        }

        private static Frontend Frontend(this ref Func f)
        {
            return f.fe;
        }
        private static void Warnl(this ref Func f, src.XPos pos, @string msg, params object[] args)
        {
            f.fe.Warnl(pos, msg, args);

        }
        private static void Logf(this ref Func f, @string msg, params object[] args)
        {
            f.fe.Logf(msg, args);

        }
        private static bool Log(this ref Func f)
        {
            return f.fe.Log();
        }
        private static void Fatalf(this ref Func f, @string msg, params object[] args)
        {
            f.fe.Fatalf(f.Entry.Pos, msg, args);

        }

        // postorder returns the reachable blocks in f in a postorder traversal.
        private static slice<ref Block> postorder(this ref Func f)
        {
            if (f.cachedPostorder == null)
            {
                f.cachedPostorder = postorder(f);
            }
            return f.cachedPostorder;
        }

        private static slice<ref Block> Postorder(this ref Func f)
        {
            return f.postorder();
        }

        // Idom returns a map from block ID to the immediate dominator of that block.
        // f.Entry.ID maps to nil. Unreachable blocks map to nil as well.
        private static slice<ref Block> Idom(this ref Func f)
        {
            if (f.cachedIdom == null)
            {
                f.cachedIdom = dominators(f);
            }
            return f.cachedIdom;
        }

        // sdom returns a sparse tree representing the dominator relationships
        // among the blocks of f.
        private static SparseTree sdom(this ref Func f)
        {
            if (f.cachedSdom == null)
            {
                f.cachedSdom = newSparseTree(f, f.Idom());
            }
            return f.cachedSdom;
        }

        // loopnest returns the loop nest information for f.
        private static ref loopnest loopnest(this ref Func f)
        {
            if (f.cachedLoopnest == null)
            {
                f.cachedLoopnest = loopnestfor(f);
            }
            return f.cachedLoopnest;
        }

        // invalidateCFG tells f that its CFG has changed.
        private static void invalidateCFG(this ref Func f)
        {
            f.cachedPostorder = null;
            f.cachedIdom = null;
            f.cachedSdom = null;
            f.cachedLoopnest = null;
        }

        // DebugHashMatch returns true if environment variable evname
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
        private static bool DebugHashMatch(this ref Func f, @string evname, @string name)
        {
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

        private static void logDebugHashMatch(this ref Func f, @string evname, @string name)
        {
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
                        error err = default;
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
