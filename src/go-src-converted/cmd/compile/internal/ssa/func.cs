// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:25 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\func.go
namespace go.cmd.compile.@internal;

using abi = cmd.compile.@internal.abi_package;
using @base = cmd.compile.@internal.@base_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;
using sha1 = crypto.sha1_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using os = os_package;
using strings = strings_package;

public static partial class ssa_package {

private partial interface writeSyncer {
    error Sync();
}

// A Func represents a Go func declaration (or function literal) and its body.
// This package compiles each Func independently.
// Funcs are single-use; a new Func must be created for every compiled function.
public partial struct Func {
    public ptr<Config> Config; // architecture information
    public ptr<Cache> Cache; // re-usable cache
    public Frontend fe; // frontend state associated with this Func, callbacks into compiler frontend
    public ptr<pass> pass; // current pass information (name, options, etc.)
    public @string Name; // e.g. NewFunc or (*Func).NumBlocks (no package prefix)
    public ptr<types.Type> Type; // type signature of the function.
    public slice<ptr<Block>> Blocks; // unordered set of all basic blocks (note: not indexable by ID)
    public ptr<Block> Entry; // the entry basic block

    public idAlloc bid; // block ID allocator
    public idAlloc vid; // value ID allocator

// Given an environment variable used for debug hash match,
// what file (if any) receives the yes/no logging?
    public map<@string, writeSyncer> logfiles;
    public ptr<HTMLWriter> HTMLWriter; // html writer, for debugging
    public bool DebugTest; // default true unless $GOSSAHASH != ""; as a debugging aid, make new code conditional on this and use GOSSAHASH to binary search for failing cases
    public bool PrintOrHtmlSSA; // true if GOSSAFUNC matches, true even if fe.Log() (spew phase results to stdout) is false.
    public map<@string, nint> ruleMatches; // number of times countRule was called during compilation for any given string
    public ptr<abi.ABIConfig> ABI0; // A copy, for no-sync access
    public ptr<abi.ABIConfig> ABI1; // A copy, for no-sync access
    public ptr<abi.ABIConfig> ABISelf; // ABI for function being compiled
    public ptr<abi.ABIConfig> ABIDefault; // ABI for rtcall and other no-parsed-signature/pragma functions.

    public bool scheduled; // Values in Blocks are in final order
    public bool laidout; // Blocks are ordered
    public bool NoSplit; // true if function is marked as nosplit.  Used by schedule check pass.
    public byte dumpFileSeq; // the sequence numbers of dump file. (%s_%02d__%s.dump", funcname, dumpFileSeq, phaseName)

// when register allocation is done, maps value ids to locations
    public slice<Location> RegAlloc; // map from LocalSlot to set of Values that we want to store in that slot.
    public map<LocalSlot, slice<ptr<Value>>> NamedValues; // Names is a copy of NamedValues.Keys. We keep a separate list
// of keys to make iteration order deterministic.
    public slice<ptr<LocalSlot>> Names; // Canonicalize root/top-level local slots, and canonicalize their pieces.
// Because LocalSlot pieces refer to their parents with a pointer, this ensures that equivalent slots really are equal.
    public map<LocalSlot, ptr<LocalSlot>> CanonicalLocalSlots;
    public map<LocalSlotSplitKey, ptr<LocalSlot>> CanonicalLocalSplits; // RegArgs is a slice of register-memory pairs that must be spilled and unspilled in the uncommon path of function entry.
    public slice<Spill> RegArgs; // AuxCall describing parameters and results for this function.
    public ptr<AuxCall> OwnAux; // WBLoads is a list of Blocks that branch on the write
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

public partial struct LocalSlotSplitKey {
    public ptr<LocalSlot> parent;
    public long Off; // offset of slot in N
    public ptr<types.Type> Type; // type of slot
}

// NewFunc returns a new, empty function object.
// Caller must set f.Config and f.Cache before using f.
public static ptr<Func> NewFunc(Frontend fe) {
    return addr(new Func(fe:fe,NamedValues:make(map[LocalSlot][]*Value),CanonicalLocalSlots:make(map[LocalSlot]*LocalSlot),CanonicalLocalSplits:make(map[LocalSlotSplitKey]*LocalSlot)));
}

// NumBlocks returns an integer larger than the id of any Block in the Func.
private static nint NumBlocks(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.bid.num();
}

// NumValues returns an integer larger than the id of any Value in the Func.
private static nint NumValues(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.vid.num();
}

// newSparseSet returns a sparse set that can store at least up to n integers.
private static ptr<sparseSet> newSparseSet(this ptr<Func> _addr_f, nint n) {
    ref Func f = ref _addr_f.val;

    foreach (var (i, scr) in f.Cache.scrSparseSet) {
        if (scr != null && scr.cap() >= n) {
            f.Cache.scrSparseSet[i] = null;
            scr.clear();
            return _addr_scr!;
        }
    }    return _addr_newSparseSet(n)!;
}

// retSparseSet returns a sparse set to the config's cache of sparse
// sets to be reused by f.newSparseSet.
private static void retSparseSet(this ptr<Func> _addr_f, ptr<sparseSet> _addr_ss) {
    ref Func f = ref _addr_f.val;
    ref sparseSet ss = ref _addr_ss.val;

    foreach (var (i, scr) in f.Cache.scrSparseSet) {
        if (scr == null) {
            f.Cache.scrSparseSet[i] = ss;
            return ;
        }
    }    f.Cache.scrSparseSet = append(f.Cache.scrSparseSet, ss);
}

// newSparseMap returns a sparse map that can store at least up to n integers.
private static ptr<sparseMap> newSparseMap(this ptr<Func> _addr_f, nint n) {
    ref Func f = ref _addr_f.val;

    foreach (var (i, scr) in f.Cache.scrSparseMap) {
        if (scr != null && scr.cap() >= n) {
            f.Cache.scrSparseMap[i] = null;
            scr.clear();
            return _addr_scr!;
        }
    }    return _addr_newSparseMap(n)!;
}

// retSparseMap returns a sparse map to the config's cache of sparse
// sets to be reused by f.newSparseMap.
private static void retSparseMap(this ptr<Func> _addr_f, ptr<sparseMap> _addr_ss) {
    ref Func f = ref _addr_f.val;
    ref sparseMap ss = ref _addr_ss.val;

    foreach (var (i, scr) in f.Cache.scrSparseMap) {
        if (scr == null) {
            f.Cache.scrSparseMap[i] = ss;
            return ;
        }
    }    f.Cache.scrSparseMap = append(f.Cache.scrSparseMap, ss);
}

// newPoset returns a new poset from the internal cache
private static ptr<poset> newPoset(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (len(f.Cache.scrPoset) > 0) {
        var po = f.Cache.scrPoset[len(f.Cache.scrPoset) - 1];
        f.Cache.scrPoset = f.Cache.scrPoset[..(int)len(f.Cache.scrPoset) - 1];
        return _addr_po!;
    }
    return _addr_newPoset()!;
}

// retPoset returns a poset to the internal cache
private static void retPoset(this ptr<Func> _addr_f, ptr<poset> _addr_po) {
    ref Func f = ref _addr_f.val;
    ref poset po = ref _addr_po.val;

    f.Cache.scrPoset = append(f.Cache.scrPoset, po);
}

// newDeadcodeLive returns a slice for the
// deadcode pass to use to indicate which values are live.
private static slice<bool> newDeadcodeLive(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var r = f.Cache.deadcode.live;
    f.Cache.deadcode.live = null;
    return r;
}

// retDeadcodeLive returns a deadcode live value slice for re-use.
private static void retDeadcodeLive(this ptr<Func> _addr_f, slice<bool> live) {
    ref Func f = ref _addr_f.val;

    f.Cache.deadcode.live = live;
}

// newDeadcodeLiveOrderStmts returns a slice for the
// deadcode pass to use to indicate which values
// need special treatment for statement boundaries.
private static slice<ptr<Value>> newDeadcodeLiveOrderStmts(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var r = f.Cache.deadcode.liveOrderStmts;
    f.Cache.deadcode.liveOrderStmts = null;
    return r;
}

// retDeadcodeLiveOrderStmts returns a deadcode liveOrderStmts slice for re-use.
private static void retDeadcodeLiveOrderStmts(this ptr<Func> _addr_f, slice<ptr<Value>> liveOrderStmts) {
    ref Func f = ref _addr_f.val;

    f.Cache.deadcode.liveOrderStmts = liveOrderStmts;
}

private static ptr<LocalSlot> localSlotAddr(this ptr<Func> _addr_f, LocalSlot slot) {
    ref Func f = ref _addr_f.val;

    var (a, ok) = f.CanonicalLocalSlots[slot];
    if (!ok) {
        a = @new<LocalSlot>();
        a.val = slot; // don't escape slot
        f.CanonicalLocalSlots[slot] = a;
    }
    return _addr_a!;
}

private static (ptr<LocalSlot>, ptr<LocalSlot>) SplitString(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var ptrType = types.NewPtr(types.Types[types.TUINT8]);
    var lenType = types.Types[types.TINT]; 
    // Split this string up into two separate variables.
    var p = f.SplitSlot(name, ".ptr", 0, ptrType);
    var l = f.SplitSlot(name, ".len", ptrType.Size(), lenType);
    return (_addr_p!, _addr_l!);
}

private static (ptr<LocalSlot>, ptr<LocalSlot>) SplitInterface(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var n = name.N;
    var u = types.Types[types.TUINTPTR];
    var t = types.NewPtr(types.Types[types.TUINT8]); 
    // Split this interface up into two separate variables.
    @string sfx = ".itab";
    if (n.Type().IsEmptyInterface()) {
        sfx = ".type";
    }
    var c = f.SplitSlot(name, sfx, 0, u); // see comment in typebits.Set
    var d = f.SplitSlot(name, ".data", u.Size(), t);
    return (_addr_c!, _addr_d!);
}

private static (ptr<LocalSlot>, ptr<LocalSlot>, ptr<LocalSlot>) SplitSlice(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var ptrType = types.NewPtr(name.Type.Elem());
    var lenType = types.Types[types.TINT];
    var p = f.SplitSlot(name, ".ptr", 0, ptrType);
    var l = f.SplitSlot(name, ".len", ptrType.Size(), lenType);
    var c = f.SplitSlot(name, ".cap", ptrType.Size() + lenType.Size(), lenType);
    return (_addr_p!, _addr_l!, _addr_c!);
}

private static (ptr<LocalSlot>, ptr<LocalSlot>) SplitComplex(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var s = name.Type.Size() / 2;
    ptr<types.Type> t;
    if (s == 8) {
        t = types.Types[types.TFLOAT64];
    }
    else
 {
        t = types.Types[types.TFLOAT32];
    }
    var r = f.SplitSlot(name, ".real", 0, t);
    var i = f.SplitSlot(name, ".imag", t.Size(), t);
    return (_addr_r!, _addr_i!);
}

private static (ptr<LocalSlot>, ptr<LocalSlot>) SplitInt64(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ptr<LocalSlot> _p0 = default!;
    ptr<LocalSlot> _p0 = default!;
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    ptr<types.Type> t;
    if (name.Type.IsSigned()) {
        t = types.Types[types.TINT32];
    }
    else
 {
        t = types.Types[types.TUINT32];
    }
    if (f.Config.BigEndian) {
        return (_addr_f.SplitSlot(name, ".hi", 0, t)!, _addr_f.SplitSlot(name, ".lo", t.Size(), types.Types[types.TUINT32])!);
    }
    return (_addr_f.SplitSlot(name, ".hi", t.Size(), t)!, _addr_f.SplitSlot(name, ".lo", 0, types.Types[types.TUINT32])!);
}

private static ptr<LocalSlot> SplitStruct(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name, nint i) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var st = name.Type;
    return _addr_f.SplitSlot(name, st.FieldName(i), st.FieldOff(i), st.FieldType(i))!;
}
private static ptr<LocalSlot> SplitArray(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var n = name.N;
    var at = name.Type;
    if (at.NumElem() != 1) {
        @base.FatalfAt(n.Pos(), "bad array size");
    }
    var et = at.Elem();
    return _addr_f.SplitSlot(name, "[0]", 0, et)!;
}

private static ptr<LocalSlot> SplitSlot(this ptr<Func> _addr_f, ptr<LocalSlot> _addr_name, @string sfx, long offset, ptr<types.Type> _addr_t) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;
    ref types.Type t = ref _addr_t.val;

    LocalSlotSplitKey lssk = new LocalSlotSplitKey(name,offset,t);
    {
        var (als, ok) = f.CanonicalLocalSplits[lssk];

        if (ok) {
            return _addr_als!;
        }
    } 
    // Note: the _ field may appear several times.  But
    // have no fear, identically-named but distinct Autos are
    // ok, albeit maybe confusing for a debugger.
    ref var ls = ref heap(f.fe.SplitSlot(name, sfx, offset, t), out ptr<var> _addr_ls);
    _addr_f.CanonicalLocalSplits[lssk] = _addr_ls;
    f.CanonicalLocalSplits[lssk] = ref _addr_f.CanonicalLocalSplits[lssk].val;
    return _addr__addr_ls!;
}

// newValue allocates a new Value with the given fields and places it at the end of b.Values.
private static ptr<Value> newValue(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, ptr<Block> _addr_b, src.XPos pos) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;
    ref Block b = ref _addr_b.val;

    ptr<Value> v;
    if (f.freeValues != null) {
        v = f.freeValues;
        f.freeValues = v.argstorage[0];
        v.argstorage[0] = null;
    }
    else
 {
        var ID = f.vid.get();
        if (int(ID) < len(f.Cache.values)) {
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
    if (notStmtBoundary(op)) {
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
private static ptr<Value> newValueNoBlock(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, src.XPos pos) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    ptr<Value> v;
    if (f.freeValues != null) {
        v = f.freeValues;
        f.freeValues = v.argstorage[0];
        v.argstorage[0] = null;
    }
    else
 {
        var ID = f.vid.get();
        if (int(ID) < len(f.Cache.values)) {
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
    if (notStmtBoundary(op)) {
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
private static void LogStat(this ptr<Func> _addr_f, @string key, params object[] args) {
    args = args.Clone();
    ref Func f = ref _addr_f.val;

    @string value = "";
    foreach (var (_, a) in args) {
        value += fmt.Sprintf("\t%v", a);
    }    @string n = "missing_pass";
    if (f.pass != null) {
        n = strings.Replace(f.pass.name, " ", "_", -1);
    }
    f.Warnl(f.Entry.Pos, "\t%s\t%s%s\t%s", n, key, value, f.Name);
}

// unCacheLine removes v from f's constant cache "line" for aux,
// resets v.InCache when it is found (and removed),
// and returns whether v was found in that line.
private static bool unCacheLine(this ptr<Func> _addr_f, ptr<Value> _addr_v, long aux) {
    ref Func f = ref _addr_f.val;
    ref Value v = ref _addr_v.val;

    var vv = f.constants[aux];
    foreach (var (i, cv) in vv) {
        if (v == cv) {
            vv[i] = vv[len(vv) - 1];
            vv[len(vv) - 1] = null;
            f.constants[aux] = vv[(int)0..(int)len(vv) - 1];
            v.InCache = false;
            return true;
        }
    }    return false;
}

// unCache removes v from f's constant cache.
private static void unCache(this ptr<Func> _addr_f, ptr<Value> _addr_v) {
    ref Func f = ref _addr_f.val;
    ref Value v = ref _addr_v.val;

    if (v.InCache) {
        var aux = v.AuxInt;
        if (f.unCacheLine(v, aux)) {
            return ;
        }
        if (aux == 0) {

            if (v.Op == OpConstNil) 
                aux = constNilMagic;
            else if (v.Op == OpConstSlice) 
                aux = constSliceMagic;
            else if (v.Op == OpConstString) 
                aux = constEmptyStringMagic;
            else if (v.Op == OpConstInterface) 
                aux = constInterfaceMagic;
                        if (aux != 0 && f.unCacheLine(v, aux)) {
                return ;
            }
        }
        f.Fatalf("unCached value %s not found in cache, auxInt=0x%x, adjusted aux=0x%x", v.LongString(), v.AuxInt, aux);
    }
}

// freeValue frees a value. It must no longer be referenced or have any args.
private static void freeValue(this ptr<Func> _addr_f, ptr<Value> _addr_v) {
    ref Func f = ref _addr_f.val;
    ref Value v = ref _addr_v.val;

    if (v.Block == null) {
        f.Fatalf("trying to free an already freed value");
    }
    if (v.Uses != 0) {
        f.Fatalf("value %s still has %d uses", v, v.Uses);
    }
    if (len(v.Args) != 0) {
        f.Fatalf("value %s still has %d args", v, len(v.Args));
    }
    var id = v.ID;
    if (v.InCache) {
        f.unCache(v);
    }
    v = new Value();
    v.ID = id;
    v.argstorage[0] = f.freeValues;
    f.freeValues = v;
}

// newBlock allocates a new Block of the given kind and places it at the end of f.Blocks.
private static ptr<Block> NewBlock(this ptr<Func> _addr_f, BlockKind kind) {
    ref Func f = ref _addr_f.val;

    ptr<Block> b;
    if (f.freeBlocks != null) {
        b = f.freeBlocks;
        f.freeBlocks = b.succstorage[0].b;
        b.succstorage[0].b = null;
    }
    else
 {
        var ID = f.bid.get();
        if (int(ID) < len(f.Cache.blocks)) {
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
    b.Preds = b.predstorage[..(int)0];
    b.Succs = b.succstorage[..(int)0];
    b.Values = b.valstorage[..(int)0];
    f.Blocks = append(f.Blocks, b);
    f.invalidateCFG();
    return _addr_b!;
}

private static void freeBlock(this ptr<Func> _addr_f, ptr<Block> _addr_b) {
    ref Func f = ref _addr_f.val;
    ref Block b = ref _addr_b.val;

    if (b.Func == null) {
        f.Fatalf("trying to free an already freed block");
    }
    var id = b.ID;
    b = new Block();
    b.ID = id;
    b.succstorage[0].b = f.freeBlocks;
    f.freeBlocks = b;
}

// NewValue0 returns a new value in the block with no arguments and zero aux values.
private static ptr<Value> NewValue0(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Args = v.argstorage[..(int)0];
    return _addr_v!;
}

// NewValue returns a new value in the block with no arguments and an auxint value.
private static ptr<Value> NewValue0I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Args = v.argstorage[..(int)0];
    return _addr_v!;
}

// NewValue returns a new value in the block with no arguments and an aux value.
private static ptr<Value> NewValue0A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, Aux aux) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)0];
    return _addr_v!;
}

// NewValue returns a new value in the block with no arguments and both an auxint and aux values.
private static ptr<Value> NewValue0IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, Aux aux) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)0];
    return _addr_v!;
}

// NewValue1 returns a new value in the block with one argument and zero aux values.
private static ptr<Value> NewValue1(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg = ref _addr_arg.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Args = v.argstorage[..(int)1];
    v.argstorage[0] = arg;
    arg.Uses++;
    return _addr_v!;
}

// NewValue1I returns a new value in the block with one argument and an auxint value.
private static ptr<Value> NewValue1I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg = ref _addr_arg.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Args = v.argstorage[..(int)1];
    v.argstorage[0] = arg;
    arg.Uses++;
    return _addr_v!;
}

// NewValue1A returns a new value in the block with one argument and an aux value.
private static ptr<Value> NewValue1A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, Aux aux, ptr<Value> _addr_arg) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg = ref _addr_arg.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)1];
    v.argstorage[0] = arg;
    arg.Uses++;
    return _addr_v!;
}

// NewValue1IA returns a new value in the block with one argument and both an auxint and aux values.
private static ptr<Value> NewValue1IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, Aux aux, ptr<Value> _addr_arg) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg = ref _addr_arg.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)1];
    v.argstorage[0] = arg;
    arg.Uses++;
    return _addr_v!;
}

// NewValue2 returns a new value in the block with two arguments and zero aux values.
private static ptr<Value> NewValue2(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Args = v.argstorage[..(int)2];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    arg0.Uses++;
    arg1.Uses++;
    return _addr_v!;
}

// NewValue2A returns a new value in the block with two arguments and one aux values.
private static ptr<Value> NewValue2A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, Aux aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)2];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    arg0.Uses++;
    arg1.Uses++;
    return _addr_v!;
}

// NewValue2I returns a new value in the block with two arguments and an auxint value.
private static ptr<Value> NewValue2I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Args = v.argstorage[..(int)2];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    arg0.Uses++;
    arg1.Uses++;
    return _addr_v!;
}

// NewValue2IA returns a new value in the block with two arguments and both an auxint and aux values.
private static ptr<Value> NewValue2IA(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, Aux aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)2];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    arg0.Uses++;
    arg1.Uses++;
    return _addr_v!;
}

// NewValue3 returns a new value in the block with three arguments and zero aux values.
private static ptr<Value> NewValue3(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;
    ref Value arg2 = ref _addr_arg2.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Args = v.argstorage[..(int)3];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    v.argstorage[2] = arg2;
    arg0.Uses++;
    arg1.Uses++;
    arg2.Uses++;
    return _addr_v!;
}

// NewValue3I returns a new value in the block with three arguments and an auxint value.
private static ptr<Value> NewValue3I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;
    ref Value arg2 = ref _addr_arg2.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = auxint;
    v.Args = v.argstorage[..(int)3];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    v.argstorage[2] = arg2;
    arg0.Uses++;
    arg1.Uses++;
    arg2.Uses++;
    return _addr_v!;
}

// NewValue3A returns a new value in the block with three argument and an aux value.
private static ptr<Value> NewValue3A(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, Aux aux, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;
    ref Value arg2 = ref _addr_arg2.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Aux = aux;
    v.Args = v.argstorage[..(int)3];
    v.argstorage[0] = arg0;
    v.argstorage[1] = arg1;
    v.argstorage[2] = arg2;
    arg0.Uses++;
    arg1.Uses++;
    arg2.Uses++;
    return _addr_v!;
}

// NewValue4 returns a new value in the block with four arguments and zero aux values.
private static ptr<Value> NewValue4(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2, ptr<Value> _addr_arg3) {
    ref Block b = ref _addr_b.val;
    ref types.Type t = ref _addr_t.val;
    ref Value arg0 = ref _addr_arg0.val;
    ref Value arg1 = ref _addr_arg1.val;
    ref Value arg2 = ref _addr_arg2.val;
    ref Value arg3 = ref _addr_arg3.val;

    var v = b.Func.newValue(op, t, b, pos);
    v.AuxInt = 0;
    v.Args = new slice<ptr<Value>>(new ptr<Value>[] { arg0, arg1, arg2, arg3 });
    arg0.Uses++;
    arg1.Uses++;
    arg2.Uses++;
    arg3.Uses++;
    return _addr_v!;
}

// NewValue4I returns a new value in the block with four arguments and auxint value.
private static ptr<Value> NewValue4I(this ptr<Block> _addr_b, src.XPos pos, Op op, ptr<types.Type> _addr_t, long auxint, ptr<Value> _addr_arg0, ptr<Value> _addr_arg1, ptr<Value> _addr_arg2, ptr<Value> _addr_arg3) {
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
private static ptr<Value> constVal(this ptr<Func> _addr_f, Op op, ptr<types.Type> _addr_t, long c, bool setAuxInt) => func((_, panic, _) => {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    if (f.constants == null) {
        f.constants = make_map<long, slice<ptr<Value>>>();
    }
    var vv = f.constants[c];
    {
        var v__prev1 = v;

        foreach (var (_, __v) in vv) {
            v = __v;
            if (v.Op == op && v.Type.Compare(t) == types.CMPeq) {
                if (setAuxInt && v.AuxInt != c) {
                    panic(fmt.Sprintf("cached const %s should have AuxInt of %d", v.LongString(), c));
                }
                return _addr_v!;
            }
        }
        v = v__prev1;
    }

    ptr<Value> v;
    if (setAuxInt) {
        v = f.Entry.NewValue0I(src.NoXPos, op, t, c);
    }
    else
 {
        v = f.Entry.NewValue0(src.NoXPos, op, t);
    }
    f.constants[c] = append(vv, v);
    v.InCache = true;
    return _addr_v!;
});

// These magic auxint values let us easily cache non-numeric constants
// using the same constants map while making collisions unlikely.
// These values are unlikely to occur in regular code and
// are easy to grep for in case of bugs.
private static readonly nint constSliceMagic = 1122334455;
private static readonly nint constInterfaceMagic = (nint)2233445566L;
private static readonly nint constNilMagic = (nint)3344556677L;
private static readonly nint constEmptyStringMagic = (nint)4455667788L;

// ConstInt returns an int constant representing its argument.
private static ptr<Value> ConstBool(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, bool c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    var i = int64(0);
    if (c) {
        i = 1;
    }
    return _addr_f.constVal(OpConstBool, t, i, true)!;
}
private static ptr<Value> ConstInt8(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, sbyte c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst8, t, int64(c), true)!;
}
private static ptr<Value> ConstInt16(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, short c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst16, t, int64(c), true)!;
}
private static ptr<Value> ConstInt32(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, int c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst32, t, int64(c), true)!;
}
private static ptr<Value> ConstInt64(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, long c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst64, t, c, true)!;
}
private static ptr<Value> ConstFloat32(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, double c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst32F, t, int64(math.Float64bits(float64(float32(c)))), true)!;
}
private static ptr<Value> ConstFloat64(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, double c) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConst64F, t, int64(math.Float64bits(c)), true)!;
}

private static ptr<Value> ConstSlice(this ptr<Func> _addr_f, ptr<types.Type> _addr_t) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConstSlice, t, constSliceMagic, false)!;
}
private static ptr<Value> ConstInterface(this ptr<Func> _addr_f, ptr<types.Type> _addr_t) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConstInterface, t, constInterfaceMagic, false)!;
}
private static ptr<Value> ConstNil(this ptr<Func> _addr_f, ptr<types.Type> _addr_t) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_f.constVal(OpConstNil, t, constNilMagic, false)!;
}
private static ptr<Value> ConstEmptyString(this ptr<Func> _addr_f, ptr<types.Type> _addr_t) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;

    var v = f.constVal(OpConstString, t, constEmptyStringMagic, false);
    v.Aux = StringToAux("");
    return _addr_v!;
}
private static ptr<Value> ConstOffPtrSP(this ptr<Func> _addr_f, ptr<types.Type> _addr_t, long c, ptr<Value> _addr_sp) {
    ref Func f = ref _addr_f.val;
    ref types.Type t = ref _addr_t.val;
    ref Value sp = ref _addr_sp.val;

    var v = f.constVal(OpOffPtr, t, c, true);
    if (len(v.Args) == 0) {
        v.AddArg(sp);
    }
    return _addr_v!;
}

private static Frontend Frontend(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.fe;
}
private static void Warnl(this ptr<Func> _addr_f, src.XPos pos, @string msg, params object[] args) {
    args = args.Clone();
    ref Func f = ref _addr_f.val;

    f.fe.Warnl(pos, msg, args);
}
private static void Logf(this ptr<Func> _addr_f, @string msg, params object[] args) {
    args = args.Clone();
    ref Func f = ref _addr_f.val;

    f.fe.Logf(msg, args);
}
private static bool Log(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.fe.Log();
}

private static void Fatalf(this ptr<Func> _addr_f, @string msg, params object[] args) {
    args = args.Clone();
    ref Func f = ref _addr_f.val;

    @string stats = "crashed";
    if (f.Log()) {
        f.Logf("  pass %s end %s\n", f.pass.name, stats);
        printFunc(f);
    }
    if (f.HTMLWriter != null) {
        f.HTMLWriter.WritePhase(f.pass.name, fmt.Sprintf("%s <span class=\"stats\">%s</span>", f.pass.name, stats));
        f.HTMLWriter.flushPhases();
    }
    f.fe.Fatalf(f.Entry.Pos, msg, args);
}

// postorder returns the reachable blocks in f in a postorder traversal.
private static slice<ptr<Block>> postorder(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f.cachedPostorder == null) {
        f.cachedPostorder = postorder(f);
    }
    return f.cachedPostorder;
}

private static slice<ptr<Block>> Postorder(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return f.postorder();
}

// Idom returns a map from block ID to the immediate dominator of that block.
// f.Entry.ID maps to nil. Unreachable blocks map to nil as well.
private static slice<ptr<Block>> Idom(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f.cachedIdom == null) {
        f.cachedIdom = dominators(f);
    }
    return f.cachedIdom;
}

// Sdom returns a sparse tree representing the dominator relationships
// among the blocks of f.
private static SparseTree Sdom(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f.cachedSdom == null) {
        f.cachedSdom = newSparseTree(f, f.Idom());
    }
    return f.cachedSdom;
}

// loopnest returns the loop nest information for f.
private static ptr<loopnest> loopnest(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f.cachedLoopnest == null) {
        f.cachedLoopnest = loopnestfor(f);
    }
    return _addr_f.cachedLoopnest!;
}

// invalidateCFG tells f that its CFG has changed.
private static void invalidateCFG(this ptr<Func> _addr_f) {
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
private static bool DebugHashMatch(this ptr<Func> _addr_f, @string evname) {
    ref Func f = ref _addr_f.val;

    var name = f.fe.MyImportPath() + "." + f.Name;
    var evhash = os.Getenv(evname);
    switch (evhash) {
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
    foreach (var (_, b) in sha1.Sum((slice<byte>)name)) {
        hstr += fmt.Sprintf("%08b", b);
    }    if (strings.HasSuffix(hstr, evhash)) {
        f.logDebugHashMatch(evname, name);
        return true;
    }
    for (nint i = 0; true; i++) {
        var ev = fmt.Sprintf("%s%d", evname, i);
        var evv = os.Getenv(ev);
        if (evv == "") {
            break;
        }
        if (strings.HasSuffix(hstr, evv)) {
            f.logDebugHashMatch(ev, name);
            return true;
        }
    }
    return false;
}

private static void logDebugHashMatch(this ptr<Func> _addr_f, @string evname, @string name) {
    ref Func f = ref _addr_f.val;

    if (f.logfiles == null) {
        f.logfiles = make_map<@string, writeSyncer>();
    }
    var file = f.logfiles[evname];
    if (file == null) {
        file = os.Stdout;
        {
            var tmpfile = os.Getenv("GSHS_LOGFILE");

            if (tmpfile != "") {
                error err = default!;
                file, err = os.OpenFile(tmpfile, os.O_RDWR | os.O_CREATE | os.O_APPEND, 0666);
                if (err != null) {
                    f.Fatalf("could not open hash-testing logfile %s", tmpfile);
                }
            }

        }
        f.logfiles[evname] = file;
    }
    fmt.Fprintf(file, "%s triggered %s\n", evname, name);
    file.Sync();
}

public static bool DebugNameMatch(@string evname, @string name) {
    return os.Getenv(evname) == name;
}

private static (ptr<Value>, ptr<Value>) spSb(this ptr<Func> _addr_f) {
    ptr<Value> sp = default!;
    ptr<Value> sb = default!;
    ref Func f = ref _addr_f.val;

    var initpos = src.NoXPos; // These are originally created with no position in ssa.go; if they are optimized out then recreated, should be the same.
    foreach (var (_, v) in f.Entry.Values) {
        if (v.Op == OpSB) {
            sb = v;
        }
        if (v.Op == OpSP) {
            sp = v;
        }
        if (sb != null && sp != null) {
            return ;
        }
    }    if (sb == null) {
        sb = f.Entry.NewValue0(initpos.WithNotStmt(), OpSB, f.Config.Types.Uintptr);
    }
    if (sp == null) {
        sp = f.Entry.NewValue0(initpos.WithNotStmt(), OpSP, f.Config.Types.Uintptr);
    }
    return ;
}

} // end ssa_package
