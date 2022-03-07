// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:49:57 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\expand_calls.go
using abi = go.cmd.compile.@internal.abi_package;
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private partial struct selKey {
    public ptr<Value> from; // what is selected from
    public long offsetOrIndex; // whatever is appropriate for the selector
    public long size;
    public ptr<types.Type> typ;
}

public partial struct Abi1RO { // : byte
} // An offset within a parameter's slice of register indices, for abi1.

private static bool isBlockMultiValueExit(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return (b.Kind == BlockRet || b.Kind == BlockRetJmp) && len(b.Controls) > 0 && b.Controls[0].Op == OpMakeResult;
}

private static error badVal(@string s, ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    return error.As(fmt.Errorf("%s %s", s, v.LongString()))!;
}

// removeTrivialWrapperTypes unwraps layers of
// struct { singleField SomeType } and [1]SomeType
// until a non-wrapper type is reached.  This is useful
// for working with assignments to/from interface data
// fields (either second operand to OpIMake or OpIData)
// where the wrapping or type conversion can be elided
// because of type conversions/assertions in source code
// that do not appear in SSA.
private static ptr<types.Type> removeTrivialWrapperTypes(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    while (true) {
        if (t.IsStruct() && t.NumFields() == 1) {
            t = t.Field(0).Type;
            continue;
        }
        if (t.IsArray() && t.NumElem() == 1) {
            t = t.Elem();
            continue;
        }
        break;

    }
    return _addr_t!;

}

// A registerCursor tracks which register is used for an Arg or regValues, or a piece of such.
private partial struct registerCursor {
    public ptr<Value> storeDest; // if there are no register targets, then this is the base of the store.
    public nint regsLen; // the number of registers available for this Arg/result (which is all in registers or not at all)
    public Abi1RO nextSlice; // the next register/register-slice offset
    public ptr<abi.ABIConfig> config;
    public ptr<slice<ptr<Value>>> regValues; // values assigned to registers accumulate here
}

private static @string String(this ptr<registerCursor> _addr_rc) {
    ref registerCursor rc = ref _addr_rc.val;

    @string dest = "<none>";
    if (rc.storeDest != null) {
        dest = rc.storeDest.String();
    }
    @string regs = "<none>";
    if (rc.regValues != null) {
        regs = "";
        foreach (var (i, x) in rc.regValues.val) {
            if (i > 0) {
                regs = regs + "; ";
            }
            regs = regs + x.LongString();
        }
    }
    return fmt.Sprintf("RCSR{storeDest=%v, regsLen=%d, nextSlice=%d, regValues=[%s]}", dest, rc.regsLen, rc.nextSlice, regs);

}

// next effectively post-increments the register cursor; the receiver is advanced,
// the old value is returned.
private static registerCursor next(this ptr<registerCursor> _addr_c, ptr<types.Type> _addr_t) {
    ref registerCursor c = ref _addr_c.val;
    ref types.Type t = ref _addr_t.val;

    var rc = c.val;
    if (int(c.nextSlice) < c.regsLen) {
        var w = c.config.NumParamRegs(t);
        c.nextSlice += Abi1RO(w);
    }
    return rc;

}

// plus returns a register cursor offset from the original, without modifying the original.
private static registerCursor plus(this ptr<registerCursor> _addr_c, Abi1RO regWidth) {
    ref registerCursor c = ref _addr_c.val;

    var rc = c.val;
    rc.nextSlice += regWidth;
    return rc;
}

 
// Register offsets for fields of built-in aggregate types; the ones not listed are zero.
public static readonly nint RO_complex_imag = 1;
public static readonly nint RO_string_len = 1;
public static readonly nint RO_slice_len = 1;
public static readonly nint RO_slice_cap = 2;
public static readonly nint RO_iface_data = 1;


private static Abi1RO regWidth(this ptr<expandState> _addr_x, ptr<types.Type> _addr_t) {
    ref expandState x = ref _addr_x.val;
    ref types.Type t = ref _addr_t.val;

    return Abi1RO(x.abi1.NumParamRegs(t));
}

// regOffset returns the register offset of the i'th element of type t
private static Abi1RO regOffset(this ptr<expandState> _addr_x, ptr<types.Type> _addr_t, nint i) => func((_, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref types.Type t = ref _addr_t.val;
 
    // TODO maybe cache this in a map if profiling recommends.
    if (i == 0) {
        return 0;
    }
    if (t.IsArray()) {
        return Abi1RO(i) * x.regWidth(t.Elem());
    }
    if (t.IsStruct()) {
        var k = Abi1RO(0);
        for (nint j = 0; j < i; j++) {
            k += x.regWidth(t.FieldType(j));
        }
        return k;
    }
    panic("Haven't implemented this case yet, do I need to?");

});

// at returns the register cursor for component i of t, where the first
// component is numbered 0.
private static registerCursor at(this ptr<registerCursor> _addr_c, ptr<types.Type> _addr_t, nint i) => func((_, panic, _) => {
    ref registerCursor c = ref _addr_c.val;
    ref types.Type t = ref _addr_t.val;

    var rc = c.val;
    if (i == 0 || c.regsLen == 0) {
        return rc;
    }
    if (t.IsArray()) {
        var w = c.config.NumParamRegs(t.Elem());
        rc.nextSlice += Abi1RO(i * w);
        return rc;
    }
    if (t.IsStruct()) {
        for (nint j = 0; j < i; j++) {
            rc.next(t.FieldType(j));
        }
        return rc;
    }
    panic("Haven't implemented this case yet, do I need to?");

});

private static void init(this ptr<registerCursor> _addr_c, slice<abi.RegIndex> regs, ptr<abi.ABIParamResultInfo> _addr_info, ptr<slice<ptr<Value>>> _addr_result, ptr<Value> _addr_storeDest) {
    ref registerCursor c = ref _addr_c.val;
    ref abi.ABIParamResultInfo info = ref _addr_info.val;
    ref slice<ptr<Value>> result = ref _addr_result.val;
    ref Value storeDest = ref _addr_storeDest.val;

    c.regsLen = len(regs);
    c.nextSlice = 0;
    if (len(regs) == 0) {
        c.storeDest = storeDest; // only save this if there are no registers, will explode if misused.
        return ;

    }
    c.config = info.Config();
    c.regValues = result;

}

private static void addArg(this ptr<registerCursor> _addr_c, ptr<Value> _addr_v) {
    ref registerCursor c = ref _addr_c.val;
    ref Value v = ref _addr_v.val;

    c.regValues.val = append(c.regValues.val, v);
}

private static bool hasRegs(this ptr<registerCursor> _addr_c) {
    ref registerCursor c = ref _addr_c.val;

    return c.regsLen > 0;
}

private partial struct expandState {
    public ptr<Func> f;
    public ptr<abi.ABIConfig> abi1;
    public bool debug;
    public Func<ptr<types.Type>, bool> canSSAType;
    public long regSize;
    public ptr<Value> sp;
    public ptr<Types> typs;
    public long ptrSize;
    public long hiOffset;
    public long lowOffset;
    public Abi1RO hiRo;
    public Abi1RO loRo;
    public map<ptr<Value>, slice<namedVal>> namedSelects;
    public SparseTree sdom;
    public map<selKey, ptr<Value>> commonSelectors; // used to de-dupe selectors
    public map<selKey, ptr<Value>> commonArgs; // used to de-dupe OpArg/OpArgIntReg/OpArgFloatReg
    public map<ID, ptr<Value>> memForCall; // For a call, need to know the unique selector that gets the mem.
    public map<ID, bool> transformedSelects; // OpSelectN after rewriting, either created or renumbered.
    public nint indentLevel; // Indentation for debugging recursion
}

// intPairTypes returns the pair of 32-bit int types needed to encode a 64-bit integer type on a target
// that has no 64-bit integer registers.
private static (ptr<types.Type>, ptr<types.Type>) intPairTypes(this ptr<expandState> _addr_x, types.Kind et) {
    ptr<types.Type> tHi = default!;
    ptr<types.Type> tLo = default!;
    ref expandState x = ref _addr_x.val;

    tHi = x.typs.UInt32;
    if (et == types.TINT64) {
        tHi = x.typs.Int32;
    }
    tLo = x.typs.UInt32;
    return ;

}

// isAlreadyExpandedAggregateType returns whether a type is an SSA-able "aggregate" (multiple register) type
// that was expanded in an earlier phase (currently, expand_calls is intended to run after decomposeBuiltin,
// so this is all aggregate types -- small struct and array, complex, interface, string, slice, and 64-bit
// integer on 32-bit).
private static bool isAlreadyExpandedAggregateType(this ptr<expandState> _addr_x, ptr<types.Type> _addr_t) {
    ref expandState x = ref _addr_x.val;
    ref types.Type t = ref _addr_t.val;

    if (!x.canSSAType(t)) {
        return false;
    }
    return t.IsStruct() || t.IsArray() || t.IsComplex() || t.IsInterface() || t.IsString() || t.IsSlice() || t.Size() > x.regSize && t.IsInteger();

}

// offsetFrom creates an offset from a pointer, simplifying chained offsets and offsets from SP
// TODO should also optimize offsets from SB?
private static ptr<Value> offsetFrom(this ptr<expandState> _addr_x, ptr<Block> _addr_b, ptr<Value> _addr_from, long offset, ptr<types.Type> _addr_pt) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value from = ref _addr_from.val;
    ref types.Type pt = ref _addr_pt.val;

    var ft = from.Type;
    if (offset == 0) {
        if (ft == pt) {
            return _addr_from!;
        }
        if ((ft.IsPtr() || ft.IsUnsafePtr()) && pt.IsPtr()) {
            return _addr_from!;
        }
    }
    while (from.Op == OpOffPtr) {
        offset += from.AuxInt;
        from = from.Args[0];
    }
    if (from == x.sp) {
        return _addr_x.f.ConstOffPtrSP(pt, offset, x.sp)!;
    }
    return _addr_b.NewValue1I(from.Pos.WithNotStmt(), OpOffPtr, pt, offset, from)!;

}

// splitSlots splits one "field" (specified by sfx, offset, and ty) out of the LocalSlots in ls and returns the new LocalSlots this generates.
private static slice<ptr<LocalSlot>> splitSlots(this ptr<expandState> _addr_x, slice<ptr<LocalSlot>> ls, @string sfx, long offset, ptr<types.Type> _addr_ty) {
    ref expandState x = ref _addr_x.val;
    ref types.Type ty = ref _addr_ty.val;

    slice<ptr<LocalSlot>> locs = default;
    foreach (var (i) in ls) {
        locs = append(locs, x.f.SplitSlot(ls[i], sfx, offset, ty));
    }    return locs;
}

// prAssignForArg returns the ABIParamAssignment for v, assumed to be an OpArg.
private static ptr<abi.ABIParamAssignment> prAssignForArg(this ptr<expandState> _addr_x, ptr<Value> _addr_v) => func((_, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref Value v = ref _addr_v.val;

    if (v.Op != OpArg) {
        panic(badVal("Wanted OpArg, instead saw", _addr_v));
    }
    return _addr_ParamAssignmentForArgName(_addr_x.f, v.Aux._<ptr<ir.Name>>())!;

});

// ParamAssignmentForArgName returns the ABIParamAssignment for f's arg with matching name.
public static ptr<abi.ABIParamAssignment> ParamAssignmentForArgName(ptr<Func> _addr_f, ptr<ir.Name> _addr_name) => func((_, panic, _) => {
    ref Func f = ref _addr_f.val;
    ref ir.Name name = ref _addr_name.val;

    var abiInfo = f.OwnAux.abiInfo;
    var ip = abiInfo.InParams();
    foreach (var (i, a) in ip) {
        if (a.Name == name) {
            return _addr__addr_ip[i]!;
        }
    }    panic(fmt.Errorf("Did not match param %v in prInfo %+v", name, abiInfo.InParams()));

});

// indent increments (or decrements) the indentation.
private static void indent(this ptr<expandState> _addr_x, nint n) {
    ref expandState x = ref _addr_x.val;

    x.indentLevel += n;
}

// Printf does an indented fmt.Printf on te format and args.
private static (nint, error) Printf(this ptr<expandState> _addr_x, @string format, params object[] a) {
    nint n = default;
    error err = default!;
    a = a.Clone();
    ref expandState x = ref _addr_x.val;

    if (x.indentLevel > 0) {
        fmt.Printf("%[1]*s", x.indentLevel, "");
    }
    return fmt.Printf(format, a);

}

// Calls that need lowering have some number of inputs, including a memory input,
// and produce a tuple of (value1, value2, ..., mem) where valueK may or may not be SSA-able.

// With the current ABI those inputs need to be converted into stores to memory,
// rethreading the call's memory input to the first, and the new call now receiving the last.

// With the current ABI, the outputs need to be converted to loads, which will all use the call's
// memory output as their input.

// rewriteSelect recursively walks from leaf selector to a root (OpSelectN, OpLoad, OpArg)
// through a chain of Struct/Array/builtin Select operations.  If the chain of selectors does not
// end in an expected root, it does nothing (this can happen depending on compiler phase ordering).
// The "leaf" provides the type, the root supplies the container, and the leaf-to-root path
// accumulates the offset.
// It emits the code necessary to implement the leaf select operation that leads to the root.
//
// TODO when registers really arrive, must also decompose anything split across two registers or registers and memory.
private static slice<ptr<LocalSlot>> rewriteSelect(this ptr<expandState> _addr_x, ptr<Value> _addr_leaf, ptr<Value> _addr_selector, long offset, Abi1RO regOffset) => func((defer, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref Value leaf = ref _addr_leaf.val;
    ref Value selector = ref _addr_selector.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("rewriteSelect(%s; %s; memOff=%d; regOff=%d)\n", leaf.LongString(), selector.LongString(), offset, regOffset);
    }
    slice<ptr<LocalSlot>> locs = default;
    var leafType = leaf.Type;
    if (len(selector.Args) > 0) {
        var w = selector.Args[0];
        if (w.Op == OpCopy) {
            while (w.Op == OpCopy) {
                w = w.Args[0];
            }

            selector.SetArg(0, w);
        }
    }

    if (selector.Op == OpArgIntReg || selector.Op == OpArgFloatReg) 
        if (leafType == selector.Type) { // OpIData leads us here, sometimes.
            leaf.copyOf(selector);

        }
        else
 {
            x.f.Fatalf("Unexpected %s type, selector=%s, leaf=%s\n", selector.Op.String(), selector.LongString(), leaf.LongString());
        }
        if (x.debug) {
            x.Printf("---%s, break\n", selector.Op.String());
        }
    else if (selector.Op == OpArg) 
        if (!x.isAlreadyExpandedAggregateType(selector.Type)) {
            if (leafType == selector.Type) { // OpIData leads us here, sometimes.
                x.newArgToMemOrRegs(selector, leaf, offset, regOffset, leafType, leaf.Pos);

            }
            else
 {
                x.f.Fatalf("Unexpected OpArg type, selector=%s, leaf=%s\n", selector.LongString(), leaf.LongString());
            }

            if (x.debug) {
                x.Printf("---OpArg, break\n");
            }

            break;

        }

        if (leaf.Op == OpIData || leaf.Op == OpStructSelect || leaf.Op == OpArraySelect) 
            leafType = removeTrivialWrapperTypes(_addr_leaf.Type);
                x.newArgToMemOrRegs(selector, leaf, offset, regOffset, leafType, leaf.Pos);

        {
            var s__prev1 = s;

            foreach (var (_, __s) in x.namedSelects[selector]) {
                s = __s;
                locs = append(locs, x.f.Names[s.locIndex]);
            }

            s = s__prev1;
        }
    else if (selector.Op == OpLoad) // We end up here because of IData of immediate structures.
        // Failure case:
        // (note the failure case is very rare; w/o this case, make.bash and run.bash both pass, as well as
        // the hard cases of building {syscall,math,math/cmplx,math/bits,go/constant} on ppc64le and mips-softfloat).
        //
        // GOSSAFUNC='(*dumper).dump' go build -gcflags=-l -tags=math_big_pure_go cmd/compile/internal/gc
        // cmd/compile/internal/gc/dump.go:136:14: internal compiler error: '(*dumper).dump': not lowered: v827, StructSelect PTR PTR
        // b2: ← b1
        // v20 (+142) = StaticLECall <interface {},mem> {AuxCall{reflect.Value.Interface([reflect.Value,0])[interface {},24]}} [40] v8 v1
        // v21 (142) = SelectN <mem> [1] v20
        // v22 (142) = SelectN <interface {}> [0] v20
        // b15: ← b8
        // v71 (+143) = IData <Nodes> v22 (v[Nodes])
        // v73 (+146) = StaticLECall <[]*Node,mem> {AuxCall{"".Nodes.Slice([Nodes,0])[[]*Node,8]}} [32] v71 v21
        //
        // translates (w/o the "case OpLoad:" above) to:
        //
        // b2: ← b1
        // v20 (+142) = StaticCall <mem> {AuxCall{reflect.Value.Interface([reflect.Value,0])[interface {},24]}} [40] v715
        // v23 (142) = Load <*uintptr> v19 v20
        // v823 (142) = IsNonNil <bool> v23
        // v67 (+143) = Load <*[]*Node> v880 v20
        // b15: ← b8
        // v827 (146) = StructSelect <*[]*Node> [0] v67
        // v846 (146) = Store <mem> {*[]*Node} v769 v827 v20
        // v73 (+146) = StaticCall <mem> {AuxCall{"".Nodes.Slice([Nodes,0])[[]*Node,8]}} [32] v846
        // i.e., the struct select is generated and remains in because it is not applied to an actual structure.
        // The OpLoad was created to load the single field of the IData
        // This case removes that StructSelect.
        if (leafType != selector.Type) {
            x.f.Fatalf("Unexpected Load as selector, leaf=%s, selector=%s\n", leaf.LongString(), selector.LongString());
        }
        leaf.copyOf(selector);
        {
            var s__prev1 = s;

            foreach (var (_, __s) in x.namedSelects[selector]) {
                s = __s;
                locs = append(locs, x.f.Names[s.locIndex]);
            }

            s = s__prev1;
        }
    else if (selector.Op == OpSelectN) 
        // TODO(register args) result case
        // if applied to Op-mumble-call, the Aux tells us which result, regOffset specifies offset within result.  If a register, should rewrite to OpSelectN for new call.
        // TODO these may be duplicated. Should memoize. Intermediate selectors will go dead, no worries there.
        var call = selector.Args[0];
        var call0 = call;
        ptr<AuxCall> aux = call.Aux._<ptr<AuxCall>>();
        var which = selector.AuxInt;
        if (x.transformedSelects[selector.ID]) { 
            // This is a minor hack.  Either this select has had its operand adjusted (mem) or
            // it is some other intermediate node that was rewritten to reference a register (not a generic arg).
            // This can occur with chains of selection/indexing from single field/element aggregates.
            leaf.copyOf(selector);
            break;

        }
        if (which == aux.NResults()) { // mem is after the results.
            // rewrite v as a Copy of call -- the replacement call will produce a mem.
            if (leaf != selector) {
                panic(fmt.Errorf("Unexpected selector of memory, selector=%s, call=%s, leaf=%s", selector.LongString(), call.LongString(), leaf.LongString()));
            }

            if (aux.abiInfo == null) {
                panic(badVal("aux.abiInfo nil for call", _addr_call));
            }

            {
                var existing = x.memForCall[call.ID];

                if (existing == null) {
                    selector.AuxInt = int64(aux.abiInfo.OutRegistersUsed());
                    x.memForCall[call.ID] = selector;
                    x.transformedSelects[selector.ID] = true; // operand adjusted
                }
                else
 {
                    selector.copyOf(existing);
                }

            }


        }
        else
 {
            leafType = removeTrivialWrapperTypes(_addr_leaf.Type);
            if (x.canSSAType(leafType)) {
                var pt = types.NewPtr(leafType); 
                // Any selection right out of the arg area/registers has to be same Block as call, use call as mem input.
                // Create a "mem" for any loads that need to occur.
                {
                    var mem = x.memForCall[call.ID];

                    if (mem != null) {
                        if (mem.Block != call.Block) {
                            panic(fmt.Errorf("selector and call need to be in same block, selector=%s; call=%s", selector.LongString(), call.LongString()));
                        }
                        call = mem;
                    }
                    else
 {
                        mem = call.Block.NewValue1I(call.Pos.WithNotStmt(), OpSelectN, types.TypeMem, int64(aux.abiInfo.OutRegistersUsed()), call);
                        x.transformedSelects[mem.ID] = true; // select uses post-expansion indexing
                        x.memForCall[call.ID] = mem;
                        call = mem;

                    }

                }

                var outParam = aux.abiInfo.OutParam(int(which));
                if (len(outParam.Registers) > 0) {
                    var firstReg = uint32(0);
                    for (nint i = 0; i < int(which); i++) {
                        firstReg += uint32(len(aux.abiInfo.OutParam(i).Registers));
                    }
                else

                    var reg = int64(regOffset + Abi1RO(firstReg));
                    if (leaf.Block == call.Block) {
                        leaf.reset(OpSelectN);
                        leaf.SetArgs1(call0);
                        leaf.Type = leafType;
                        leaf.AuxInt = reg;
                        x.transformedSelects[leaf.ID] = true; // leaf, rewritten to use post-expansion indexing.
                    }
                    else
 {
                        w = call.Block.NewValue1I(leaf.Pos, OpSelectN, leafType, reg, call0);
                        x.transformedSelects[w.ID] = true; // select, using post-expansion indexing.
                        leaf.copyOf(w);

                    }

                } {
                    var off = x.offsetFrom(x.f.Entry, x.sp, offset + aux.OffsetOfResult(which), pt);
                    if (leaf.Block == call.Block) {
                        leaf.reset(OpLoad);
                        leaf.SetArgs2(off, call);
                        leaf.Type = leafType;
                    }
                    else
 {
                        w = call.Block.NewValue2(leaf.Pos, OpLoad, leafType, off, call);
                        leaf.copyOf(w);
                        if (x.debug) {
                            x.Printf("---new %s\n", w.LongString());
                        }
                    }

                }
            else
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in x.namedSelects[selector]) {
                        s = __s;
                        locs = append(locs, x.f.Names[s.locIndex]);
                    }

                    s = s__prev1;
                }
            } {
                x.f.Fatalf("Should not have non-SSA-able OpSelectN, selector=%s", selector.LongString());
            }

        }
    else if (selector.Op == OpStructSelect) 
        w = selector.Args[0];
        slice<ptr<LocalSlot>> ls = default;
        if (w.Type.Kind() != types.TSTRUCT) { // IData artifact
            ls = x.rewriteSelect(leaf, w, offset, regOffset);

        }
        else
 {
            var fldi = int(selector.AuxInt);
            ls = x.rewriteSelect(leaf, w, offset + w.Type.FieldOff(fldi), regOffset + x.regOffset(w.Type, fldi));
            if (w.Op != OpIData) {
                foreach (var (_, l) in ls) {
                    locs = append(locs, x.f.SplitStruct(l, int(selector.AuxInt)));
                }
            }
        }
    else if (selector.Op == OpArraySelect) 
        w = selector.Args[0];
        var index = selector.AuxInt;
        x.rewriteSelect(leaf, w, offset + selector.Type.Size() * index, regOffset + x.regOffset(w.Type, int(index)));
    else if (selector.Op == OpInt64Hi) 
        w = selector.Args[0];
        ls = x.rewriteSelect(leaf, w, offset + x.hiOffset, regOffset + x.hiRo);
        locs = x.splitSlots(ls, ".hi", x.hiOffset, leafType);
    else if (selector.Op == OpInt64Lo) 
        w = selector.Args[0];
        ls = x.rewriteSelect(leaf, w, offset + x.lowOffset, regOffset + x.loRo);
        locs = x.splitSlots(ls, ".lo", x.lowOffset, leafType);
    else if (selector.Op == OpStringPtr) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset, regOffset);
        locs = x.splitSlots(ls, ".ptr", 0, x.typs.BytePtr);
    else if (selector.Op == OpSlicePtr || selector.Op == OpSlicePtrUnchecked) 
        w = selector.Args[0];
        ls = x.rewriteSelect(leaf, w, offset, regOffset);
        locs = x.splitSlots(ls, ".ptr", 0, types.NewPtr(w.Type.Elem()));
    else if (selector.Op == OpITab) 
        w = selector.Args[0];
        ls = x.rewriteSelect(leaf, w, offset, regOffset);
        @string sfx = ".itab";
        if (w.Type.IsEmptyInterface()) {
            sfx = ".type";
        }
        locs = x.splitSlots(ls, sfx, 0, x.typs.Uintptr);
    else if (selector.Op == OpComplexReal) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset, regOffset);
        locs = x.splitSlots(ls, ".real", 0, leafType);
    else if (selector.Op == OpComplexImag) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset + leafType.Width, regOffset + RO_complex_imag); // result is FloatNN, width of result is offset of imaginary part.
        locs = x.splitSlots(ls, ".imag", leafType.Width, leafType);
    else if (selector.Op == OpStringLen || selector.Op == OpSliceLen) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset + x.ptrSize, regOffset + RO_slice_len);
        locs = x.splitSlots(ls, ".len", x.ptrSize, leafType);
    else if (selector.Op == OpIData) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset + x.ptrSize, regOffset + RO_iface_data);
        locs = x.splitSlots(ls, ".data", x.ptrSize, leafType);
    else if (selector.Op == OpSliceCap) 
        ls = x.rewriteSelect(leaf, selector.Args[0], offset + 2 * x.ptrSize, regOffset + RO_slice_cap);
        locs = x.splitSlots(ls, ".cap", 2 * x.ptrSize, leafType);
    else if (selector.Op == OpCopy) // If it's an intermediate result, recurse
        locs = x.rewriteSelect(leaf, selector.Args[0], offset, regOffset);
        {
            var s__prev1 = s;

            foreach (var (_, __s) in x.namedSelects[selector]) {
                s = __s; 
                // this copy may have had its own name, preserve that, too.
                locs = append(locs, x.f.Names[s.locIndex]);

            }

            s = s__prev1;
        }
    else         return locs;

});

private static ptr<Value> rewriteDereference(this ptr<expandState> _addr_x, ptr<Block> _addr_b, ptr<Value> _addr_@base, ptr<Value> _addr_a, ptr<Value> _addr_mem, long offset, long size, ptr<types.Type> _addr_typ, src.XPos pos) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value @base = ref _addr_@base.val;
    ref Value a = ref _addr_a.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type typ = ref _addr_typ.val;

    var source = a.Args[0];
    var dst = x.offsetFrom(b, base, offset, source.Type);
    if (a.Uses == 1 && a.Block == b) {
        a.reset(OpMove);
        a.Pos = pos;
        a.Type = types.TypeMem;
        a.Aux = typ;
        a.AuxInt = size;
        a.SetArgs3(dst, source, mem);
        mem = a;
    }
    else
 {
        mem = b.NewValue3A(pos, OpMove, types.TypeMem, typ, dst, source, mem);
        mem.AuxInt = size;
    }
    return _addr_mem!;

}

private static array<@string> indexNames = new array<@string>(new @string[] { "[0]" });

// pathTo returns the selection path to the leaf type at offset within container.
// e.g. len(thing.field[0]) => ".field[0].len"
// this is for purposes of generating names ultimately fed to a debugger.
private static @string pathTo(this ptr<expandState> _addr_x, ptr<types.Type> _addr_container, ptr<types.Type> _addr_leaf, long offset) {
    ref expandState x = ref _addr_x.val;
    ref types.Type container = ref _addr_container.val;
    ref types.Type leaf = ref _addr_leaf.val;

    if (container == leaf || offset == 0 && container.Size() == leaf.Size()) {
        return "";
    }
    @string path = "";
outer:
    while (true) {

        if (container.Kind() == types.TARRAY)
        {
            container = container.Elem();
            if (container.Size() == 0) {
                return path;
            }
            var i = offset / container.Size();
            offset = offset % container.Size(); 
            // If a future compiler/ABI supports larger SSA/Arg-able arrays, expand indexNames.
            path = path + indexNames[i];
            continue;
            goto __switch_break0;
        }
        if (container.Kind() == types.TSTRUCT)
        {
            {
                var i__prev2 = i;

                for (i = 0; i < container.NumFields(); i++) {
                    var fld = container.Field(i);
                    if (fld.Offset + fld.Type.Size() > offset) {
                        offset -= fld.Offset;
                        path += "." + fld.Sym.Name;
                        container = fld.Type;
                        _continueouter = true;
                        break;
                    }

                }


                i = i__prev2;
            }
            return path;
            goto __switch_break0;
        }
        if (container.Kind() == types.TINT64 || container.Kind() == types.TUINT64)
        {
            if (container.Width == x.regSize) {
                return path;
            }
            if (offset == x.hiOffset) {
                return path + ".hi";
            }
            return path + ".lo";
            goto __switch_break0;
        }
        if (container.Kind() == types.TINTER)
        {
            if (offset != 0) {
                return path + ".data";
            }
            if (container.IsEmptyInterface()) {
                return path + ".type";
            }
            return path + ".itab";
            goto __switch_break0;
        }
        if (container.Kind() == types.TSLICE)
        {
            if (offset == 2 * x.regSize) {
                return path + ".cap";
            }
            fallthrough = true;
        }
        if (fallthrough || container.Kind() == types.TSTRING)
        {
            if (offset == 0) {
                return path + ".ptr";
            }
            return path + ".len";
            goto __switch_break0;
        }
        if (container.Kind() == types.TCOMPLEX64 || container.Kind() == types.TCOMPLEX128)
        {
            if (offset == 0) {
                return path + ".real";
            }
            return path + ".imag";
            goto __switch_break0;
        }

        __switch_break0:;
        return path;

    }

}

// decomposeArg is a helper for storeArgOrLoad.
// It decomposes a Load or an Arg into smaller parts and returns the new mem.
// If the type does not match one of the expected aggregate types, it returns nil instead.
// Parameters:
//  pos           -- the location of any generated code.
//  b             -- the block into which any generated code should normally be placed
//  source        -- the value, possibly an aggregate, to be stored.
//  mem           -- the mem flowing into this decomposition (loads depend on it, stores updated it)
//  t             -- the type of the value to be stored
//  storeOffset   -- if the value is stored in memory, it is stored at base (see storeRc) + storeOffset
//  loadRegOffset -- regarding source as a value in registers, the register offset in ABI1.  Meaningful only if source is OpArg.
//  storeRc       -- storeRC; if the value is stored in registers, this specifies the registers.
//                   StoreRc also identifies whether the target is registers or memory, and has the base for the store operation.
private static ptr<Value> decomposeArg(this ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t, long storeOffset, Abi1RO loadRegOffset, registerCursor storeRc) => func((_, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t = ref _addr_t.val;

    var pa = x.prAssignForArg(source);
    slice<ptr<LocalSlot>> locs = default;
    foreach (var (_, s) in x.namedSelects[source]) {
        locs = append(locs, x.f.Names[s.locIndex]);
    }    if (len(pa.Registers) > 0) { 
        // Handle the in-registers case directly
        var (rts, offs) = pa.RegisterTypesAndOffsets();
        var last = loadRegOffset + x.regWidth(t);
        if (offs[loadRegOffset] != 0) { 
            // Document the problem before panicking.
            {
                nint i__prev1 = i;

                for (nint i = 0; i < len(rts); i++) {
                    var rt = rts[i];
                    var off = offs[i];
                    fmt.Printf("rt=%s, off=%d, rt.Width=%d, rt.Align=%d\n", rt.String(), off, rt.Width, rt.Align);
                }


                i = i__prev1;
            }
            panic(fmt.Errorf("offset %d of requested register %d should be zero, source=%s", offs[loadRegOffset], loadRegOffset, source.LongString()));

        }
        if (x.debug) {
            x.Printf("decompose arg %s has %d locs\n", source.LongString(), len(locs));
        }
        {
            nint i__prev1 = i;

            for (i = loadRegOffset; i < last; i++) {
                rt = rts[i];
                off = offs[i];
                var w = x.commonArgs[new selKey(source,off,rt.Width,rt)];
                if (w == null) {
                    w = x.newArgToMemOrRegs(source, w, off, i, rt, pos);
                    var suffix = x.pathTo(source.Type, rt, off);
                    if (suffix != "") {
                        x.splitSlotsIntoNames(locs, suffix, off, rt, w);
                    }
                }
                if (t.IsPtrShaped()) { 
                    // Preserve the original store type. This ensures pointer type
                    // properties aren't discarded (e.g, notinheap).
                    if (rt.Width != t.Width || len(pa.Registers) != 1 || i != loadRegOffset) {
                        b.Func.Fatalf("incompatible store type %v and %v, i=%d", t, rt, i);
                    }

                    rt = t;

                }

                mem = x.storeArgOrLoad(pos, b, w, mem, rt, storeOffset + off, i, storeRc.next(rt));

            }


            i = i__prev1;
        }
        return _addr_mem!;

    }
    var u = source.Type;

    if (u.Kind() == types.TARRAY) 
        var elem = u.Elem();
        var elemRO = x.regWidth(elem);
        {
            nint i__prev1 = i;

            for (i = int64(0); i < u.NumElem(); i++) {
                var elemOff = i * elem.Size();
                mem = storeOneArg(_addr_x, pos, _addr_b, locs, indexNames[i], _addr_source, _addr_mem, _addr_elem, elemOff, storeOffset + elemOff, loadRegOffset, storeRc.next(elem));
                loadRegOffset += elemRO;
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (u.Kind() == types.TSTRUCT) 
        {
            nint i__prev1 = i;

            for (i = 0; i < u.NumFields(); i++) {
                var fld = u.Field(i);
                mem = storeOneArg(_addr_x, pos, _addr_b, locs, "." + fld.Sym.Name, _addr_source, _addr_mem, _addr_fld.Type, fld.Offset, storeOffset + fld.Offset, loadRegOffset, storeRc.next(fld.Type));
                loadRegOffset += x.regWidth(fld.Type);
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (u.Kind() == types.TINT64 || u.Kind() == types.TUINT64) 
        if (t.Width == x.regSize) {
            break;
        }
        var (tHi, tLo) = x.intPairTypes(t.Kind());
        mem = storeOneArg(_addr_x, pos, _addr_b, locs, ".hi", _addr_source, _addr_mem, _addr_tHi, x.hiOffset, storeOffset + x.hiOffset, loadRegOffset + x.hiRo, storeRc.plus(x.hiRo));
        pos = pos.WithNotStmt();
        return _addr_storeOneArg(_addr_x, pos, _addr_b, locs, ".lo", _addr_source, _addr_mem, _addr_tLo, x.lowOffset, storeOffset + x.lowOffset, loadRegOffset + x.loRo, storeRc.plus(x.loRo))!;
    else if (u.Kind() == types.TINTER) 
        @string sfx = ".itab";
        if (u.IsEmptyInterface()) {
            sfx = ".type";
        }
        return _addr_storeTwoArg(_addr_x, pos, _addr_b, locs, sfx, ".idata", _addr_source, _addr_mem, _addr_x.typs.Uintptr, _addr_x.typs.BytePtr, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TSTRING) 
        return _addr_storeTwoArg(_addr_x, pos, _addr_b, locs, ".ptr", ".len", _addr_source, _addr_mem, _addr_x.typs.BytePtr, _addr_x.typs.Int, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TCOMPLEX64) 
        return _addr_storeTwoArg(_addr_x, pos, _addr_b, locs, ".real", ".imag", _addr_source, _addr_mem, _addr_x.typs.Float32, _addr_x.typs.Float32, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TCOMPLEX128) 
        return _addr_storeTwoArg(_addr_x, pos, _addr_b, locs, ".real", ".imag", _addr_source, _addr_mem, _addr_x.typs.Float64, _addr_x.typs.Float64, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TSLICE) 
        mem = storeOneArg(_addr_x, pos, _addr_b, locs, ".ptr", _addr_source, _addr_mem, _addr_x.typs.BytePtr, 0, storeOffset, loadRegOffset, storeRc.next(x.typs.BytePtr));
        return _addr_storeTwoArg(_addr_x, pos, _addr_b, locs, ".len", ".cap", _addr_source, _addr_mem, _addr_x.typs.Int, _addr_x.typs.Int, x.ptrSize, storeOffset + x.ptrSize, loadRegOffset + RO_slice_len, storeRc)!;
        return _addr_null!;

});

private static void splitSlotsIntoNames(this ptr<expandState> _addr_x, slice<ptr<LocalSlot>> locs, @string suffix, long off, ptr<types.Type> _addr_rt, ptr<Value> _addr_w) {
    ref expandState x = ref _addr_x.val;
    ref types.Type rt = ref _addr_rt.val;
    ref Value w = ref _addr_w.val;

    var wlocs = x.splitSlots(locs, suffix, off, rt);
    foreach (var (_, l) in wlocs) {
        var (old, ok) = x.f.NamedValues[l.val];
        x.f.NamedValues[l.val] = append(old, w);
        if (!ok) {
            x.f.Names = append(x.f.Names, l);
        }
    }
}

// decomposeLoad is a helper for storeArgOrLoad.
// It decomposes a Load  into smaller parts and returns the new mem.
// If the type does not match one of the expected aggregate types, it returns nil instead.
// Parameters:
//  pos           -- the location of any generated code.
//  b             -- the block into which any generated code should normally be placed
//  source        -- the value, possibly an aggregate, to be stored.
//  mem           -- the mem flowing into this decomposition (loads depend on it, stores updated it)
//  t             -- the type of the value to be stored
//  storeOffset   -- if the value is stored in memory, it is stored at base (see storeRc) + offset
//  loadRegOffset -- regarding source as a value in registers, the register offset in ABI1.  Meaningful only if source is OpArg.
//  storeRc       -- storeRC; if the value is stored in registers, this specifies the registers.
//                   StoreRc also identifies whether the target is registers or memory, and has the base for the store operation.
//
// TODO -- this needs cleanup; it just works for SSA-able aggregates, and won't fully generalize to register-args aggregates.
private static ptr<Value> decomposeLoad(this ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t, long storeOffset, Abi1RO loadRegOffset, registerCursor storeRc) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t = ref _addr_t.val;

    var u = source.Type;

    if (u.Kind() == types.TARRAY) 
        var elem = u.Elem();
        var elemRO = x.regWidth(elem);
        {
            var i__prev1 = i;

            for (var i = int64(0); i < u.NumElem(); i++) {
                var elemOff = i * elem.Size();
                mem = storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_elem, elemOff, storeOffset + elemOff, loadRegOffset, storeRc.next(elem));
                loadRegOffset += elemRO;
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (u.Kind() == types.TSTRUCT) 
        {
            var i__prev1 = i;

            for (i = 0; i < u.NumFields(); i++) {
                var fld = u.Field(i);
                mem = storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_fld.Type, fld.Offset, storeOffset + fld.Offset, loadRegOffset, storeRc.next(fld.Type));
                loadRegOffset += x.regWidth(fld.Type);
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (u.Kind() == types.TINT64 || u.Kind() == types.TUINT64) 
        if (t.Width == x.regSize) {
            break;
        }
        var (tHi, tLo) = x.intPairTypes(t.Kind());
        mem = storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_tHi, x.hiOffset, storeOffset + x.hiOffset, loadRegOffset + x.hiRo, storeRc.plus(x.hiRo));
        pos = pos.WithNotStmt();
        return _addr_storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_tLo, x.lowOffset, storeOffset + x.lowOffset, loadRegOffset + x.loRo, storeRc.plus(x.loRo))!;
    else if (u.Kind() == types.TINTER) 
        return _addr_storeTwoLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.Uintptr, _addr_x.typs.BytePtr, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TSTRING) 
        return _addr_storeTwoLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.BytePtr, _addr_x.typs.Int, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TCOMPLEX64) 
        return _addr_storeTwoLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.Float32, _addr_x.typs.Float32, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TCOMPLEX128) 
        return _addr_storeTwoLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.Float64, _addr_x.typs.Float64, 0, storeOffset, loadRegOffset, storeRc)!;
    else if (u.Kind() == types.TSLICE) 
        mem = storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.BytePtr, 0, storeOffset, loadRegOffset, storeRc.next(x.typs.BytePtr));
        return _addr_storeTwoLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_x.typs.Int, _addr_x.typs.Int, x.ptrSize, storeOffset + x.ptrSize, loadRegOffset + RO_slice_len, storeRc)!;
        return _addr_null!;

}

// storeOneArg creates a decomposed (one step) arg that is then stored.
// pos and b locate the store instruction, source is the "base" of the value input,
// mem is the input mem, t is the type in question, and offArg and offStore are the offsets from the respective bases.
private static ptr<Value> storeOneArg(ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, slice<ptr<LocalSlot>> locs, @string suffix, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t, long argOffset, long storeOffset, Abi1RO loadRegOffset, registerCursor storeRc) => func((defer, _, _) => {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t = ref _addr_t.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("storeOneArg(%s;  %s;  %s; aO=%d; sO=%d; lrO=%d; %s)\n", source.LongString(), mem.String(), t.String(), argOffset, storeOffset, loadRegOffset, storeRc.String());
    }
    var w = x.commonArgs[new selKey(source,argOffset,t.Width,t)];
    if (w == null) {
        w = x.newArgToMemOrRegs(source, w, argOffset, loadRegOffset, t, pos);
        x.splitSlotsIntoNames(locs, suffix, argOffset, t, w);
    }
    return _addr_x.storeArgOrLoad(pos, b, w, mem, t, storeOffset, loadRegOffset, storeRc)!;

});

// storeOneLoad creates a decomposed (one step) load that is then stored.
private static ptr<Value> storeOneLoad(ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t, long offArg, long offStore, Abi1RO loadRegOffset, registerCursor storeRc) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t = ref _addr_t.val;

    var from = x.offsetFrom(b, source.Args[0], offArg, types.NewPtr(t));
    var w = source.Block.NewValue2(source.Pos, OpLoad, t, from, mem);
    return _addr_x.storeArgOrLoad(pos, b, w, mem, t, offStore, loadRegOffset, storeRc)!;
}

private static ptr<Value> storeTwoArg(ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, slice<ptr<LocalSlot>> locs, @string suffix1, @string suffix2, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t1, ptr<types.Type> _addr_t2, long offArg, long offStore, Abi1RO loadRegOffset, registerCursor storeRc) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t1 = ref _addr_t1.val;
    ref types.Type t2 = ref _addr_t2.val;

    mem = storeOneArg(_addr_x, pos, _addr_b, locs, suffix1, _addr_source, _addr_mem, _addr_t1, offArg, offStore, loadRegOffset, storeRc.next(t1));
    pos = pos.WithNotStmt();
    var t1Size = t1.Size();
    return _addr_storeOneArg(_addr_x, pos, _addr_b, locs, suffix2, _addr_source, _addr_mem, _addr_t2, offArg + t1Size, offStore + t1Size, loadRegOffset + 1, storeRc)!;
}

// storeTwoLoad creates a pair of decomposed (one step) loads that are then stored.
// the elements of the pair must not require any additional alignment.
private static ptr<Value> storeTwoLoad(ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t1, ptr<types.Type> _addr_t2, long offArg, long offStore, Abi1RO loadRegOffset, registerCursor storeRc) {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t1 = ref _addr_t1.val;
    ref types.Type t2 = ref _addr_t2.val;

    mem = storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_t1, offArg, offStore, loadRegOffset, storeRc.next(t1));
    pos = pos.WithNotStmt();
    var t1Size = t1.Size();
    return _addr_storeOneLoad(_addr_x, pos, _addr_b, _addr_source, _addr_mem, _addr_t2, offArg + t1Size, offStore + t1Size, loadRegOffset + 1, storeRc)!;
}

// storeArgOrLoad converts stores of SSA-able potentially aggregatable arguments (passed to a call) into a series of primitive-typed
// stores of non-aggregate types.  It recursively walks up a chain of selectors until it reaches a Load or an Arg.
// If it does not reach a Load or an Arg, nothing happens; this allows a little freedom in phase ordering.
private static ptr<Value> storeArgOrLoad(this ptr<expandState> _addr_x, src.XPos pos, ptr<Block> _addr_b, ptr<Value> _addr_source, ptr<Value> _addr_mem, ptr<types.Type> _addr_t, long storeOffset, Abi1RO loadRegOffset, registerCursor storeRc) => func((defer, _, _) => {
    ref expandState x = ref _addr_x.val;
    ref Block b = ref _addr_b.val;
    ref Value source = ref _addr_source.val;
    ref Value mem = ref _addr_mem.val;
    ref types.Type t = ref _addr_t.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("storeArgOrLoad(%s;  %s;  %s; %d; %s)\n", source.LongString(), mem.String(), t.String(), storeOffset, storeRc.String());
    }

    if (source.Op == OpCopy) 
        return _addr_x.storeArgOrLoad(pos, b, source.Args[0], mem, t, storeOffset, loadRegOffset, storeRc)!;
    else if (source.Op == OpLoad || source.Op == OpDereference) 
        var ret = x.decomposeLoad(pos, b, source, mem, t, storeOffset, loadRegOffset, storeRc);
        if (ret != null) {
            return _addr_ret!;
        }
    else if (source.Op == OpArg) 
        ret = x.decomposeArg(pos, b, source, mem, t, storeOffset, loadRegOffset, storeRc);
        if (ret != null) {
            return _addr_ret!;
        }
    else if (source.Op == OpArrayMake0 || source.Op == OpStructMake0) 
        // TODO(register args) is this correct for registers?
        return _addr_mem!;
    else if (source.Op == OpStructMake1 || source.Op == OpStructMake2 || source.Op == OpStructMake3 || source.Op == OpStructMake4) 
        {
            nint i__prev1 = i;

            for (nint i = 0; i < t.NumFields(); i++) {
                var fld = t.Field(i);
                mem = x.storeArgOrLoad(pos, b, source.Args[i], mem, fld.Type, storeOffset + fld.Offset, 0, storeRc.next(fld.Type));
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (source.Op == OpArrayMake1) 
        return _addr_x.storeArgOrLoad(pos, b, source.Args[0], mem, t.Elem(), storeOffset, 0, storeRc.at(t, 0))!;
    else if (source.Op == OpInt64Make) 
        var (tHi, tLo) = x.intPairTypes(t.Kind());
        mem = x.storeArgOrLoad(pos, b, source.Args[0], mem, tHi, storeOffset + x.hiOffset, 0, storeRc.next(tHi));
        pos = pos.WithNotStmt();
        return _addr_x.storeArgOrLoad(pos, b, source.Args[1], mem, tLo, storeOffset + x.lowOffset, 0, storeRc)!;
    else if (source.Op == OpComplexMake) 
        var tPart = x.typs.Float32;
        var wPart = t.Width / 2;
        if (wPart == 8) {
            tPart = x.typs.Float64;
        }
        mem = x.storeArgOrLoad(pos, b, source.Args[0], mem, tPart, storeOffset, 0, storeRc.next(tPart));
        pos = pos.WithNotStmt();
        return _addr_x.storeArgOrLoad(pos, b, source.Args[1], mem, tPart, storeOffset + wPart, 0, storeRc)!;
    else if (source.Op == OpIMake) 
        mem = x.storeArgOrLoad(pos, b, source.Args[0], mem, x.typs.Uintptr, storeOffset, 0, storeRc.next(x.typs.Uintptr));
        pos = pos.WithNotStmt();
        return _addr_x.storeArgOrLoad(pos, b, source.Args[1], mem, x.typs.BytePtr, storeOffset + x.ptrSize, 0, storeRc)!;
    else if (source.Op == OpStringMake) 
        mem = x.storeArgOrLoad(pos, b, source.Args[0], mem, x.typs.BytePtr, storeOffset, 0, storeRc.next(x.typs.BytePtr));
        pos = pos.WithNotStmt();
        return _addr_x.storeArgOrLoad(pos, b, source.Args[1], mem, x.typs.Int, storeOffset + x.ptrSize, 0, storeRc)!;
    else if (source.Op == OpSliceMake) 
        mem = x.storeArgOrLoad(pos, b, source.Args[0], mem, x.typs.BytePtr, storeOffset, 0, storeRc.next(x.typs.BytePtr));
        pos = pos.WithNotStmt();
        mem = x.storeArgOrLoad(pos, b, source.Args[1], mem, x.typs.Int, storeOffset + x.ptrSize, 0, storeRc.next(x.typs.Int));
        return _addr_x.storeArgOrLoad(pos, b, source.Args[2], mem, x.typs.Int, storeOffset + 2 * x.ptrSize, 0, storeRc)!;
    // For nodes that cannot be taken apart -- OpSelectN, other structure selectors.

    if (t.Kind() == types.TARRAY) 
        var elt = t.Elem();
        if (source.Type != t && t.NumElem() == 1 && elt.Width == t.Width && t.Width == x.regSize) {
            t = removeTrivialWrapperTypes(_addr_t); 
            // it could be a leaf type, but the "leaf" could be complex64 (for example)
            return _addr_x.storeArgOrLoad(pos, b, source, mem, t, storeOffset, loadRegOffset, storeRc)!;

        }
        var eltRO = x.regWidth(elt);
        {
            nint i__prev1 = i;

            for (i = int64(0); i < t.NumElem(); i++) {
                var sel = source.Block.NewValue1I(pos, OpArraySelect, elt, i, source);
                mem = x.storeArgOrLoad(pos, b, sel, mem, elt, storeOffset + i * elt.Width, loadRegOffset, storeRc.at(t, 0));
                loadRegOffset += eltRO;
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (t.Kind() == types.TSTRUCT) 
        if (source.Type != t && t.NumFields() == 1 && t.Field(0).Type.Width == t.Width && t.Width == x.regSize) { 
            // This peculiar test deals with accesses to immediate interface data.
            // It works okay because everything is the same size.
            // Example code that triggers this can be found in go/constant/value.go, function ToComplex
            // v119 (+881) = IData <intVal> v6
            // v121 (+882) = StaticLECall <floatVal,mem> {AuxCall{"".itof([intVal,0])[floatVal,8]}} [16] v119 v1
            // This corresponds to the generic rewrite rule "(StructSelect [0] (IData x)) => (IData x)"
            // Guard against "struct{struct{*foo}}"
            // Other rewriting phases create minor glitches when they transform IData, for instance the
            // interface-typed Arg "x" of ToFloat in go/constant/value.go
            //   v6 (858) = Arg <Value> {x} (x[Value], x[Value])
            // is rewritten by decomposeArgs into
            //   v141 (858) = Arg <uintptr> {x}
            //   v139 (858) = Arg <*uint8> {x} [8]
            // because of a type case clause on line 862 of go/constant/value.go
            //      case intVal:
            //           return itof(x)
            // v139 is later stored as an intVal == struct{val *big.Int} which naively requires the fields of
            // of a *uint8, which does not succeed.
            t = removeTrivialWrapperTypes(_addr_t); 
            // it could be a leaf type, but the "leaf" could be complex64 (for example)
            return _addr_x.storeArgOrLoad(pos, b, source, mem, t, storeOffset, loadRegOffset, storeRc)!;

        }
        {
            nint i__prev1 = i;

            for (i = 0; i < t.NumFields(); i++) {
                fld = t.Field(i);
                sel = source.Block.NewValue1I(pos, OpStructSelect, fld.Type, int64(i), source);
                mem = x.storeArgOrLoad(pos, b, sel, mem, fld.Type, storeOffset + fld.Offset, loadRegOffset, storeRc.next(fld.Type));
                loadRegOffset += x.regWidth(fld.Type);
                pos = pos.WithNotStmt();
            }


            i = i__prev1;
        }
        return _addr_mem!;
    else if (t.Kind() == types.TINT64 || t.Kind() == types.TUINT64) 
        if (t.Width == x.regSize) {
            break;
        }
        (tHi, tLo) = x.intPairTypes(t.Kind());
        sel = source.Block.NewValue1(pos, OpInt64Hi, tHi, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, tHi, storeOffset + x.hiOffset, loadRegOffset + x.hiRo, storeRc.plus(x.hiRo));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpInt64Lo, tLo, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, tLo, storeOffset + x.lowOffset, loadRegOffset + x.loRo, storeRc.plus(x.hiRo))!;
    else if (t.Kind() == types.TINTER) 
        sel = source.Block.NewValue1(pos, OpITab, x.typs.BytePtr, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, x.typs.BytePtr, storeOffset, loadRegOffset, storeRc.next(x.typs.BytePtr));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpIData, x.typs.BytePtr, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, x.typs.BytePtr, storeOffset + x.ptrSize, loadRegOffset + RO_iface_data, storeRc)!;
    else if (t.Kind() == types.TSTRING) 
        sel = source.Block.NewValue1(pos, OpStringPtr, x.typs.BytePtr, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, x.typs.BytePtr, storeOffset, loadRegOffset, storeRc.next(x.typs.BytePtr));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpStringLen, x.typs.Int, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, x.typs.Int, storeOffset + x.ptrSize, loadRegOffset + RO_string_len, storeRc)!;
    else if (t.Kind() == types.TSLICE) 
        var et = types.NewPtr(t.Elem());
        sel = source.Block.NewValue1(pos, OpSlicePtr, et, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, et, storeOffset, loadRegOffset, storeRc.next(et));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpSliceLen, x.typs.Int, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, x.typs.Int, storeOffset + x.ptrSize, loadRegOffset + RO_slice_len, storeRc.next(x.typs.Int));
        sel = source.Block.NewValue1(pos, OpSliceCap, x.typs.Int, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, x.typs.Int, storeOffset + 2 * x.ptrSize, loadRegOffset + RO_slice_cap, storeRc)!;
    else if (t.Kind() == types.TCOMPLEX64) 
        sel = source.Block.NewValue1(pos, OpComplexReal, x.typs.Float32, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, x.typs.Float32, storeOffset, loadRegOffset, storeRc.next(x.typs.Float32));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpComplexImag, x.typs.Float32, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, x.typs.Float32, storeOffset + 4, loadRegOffset + RO_complex_imag, storeRc)!;
    else if (t.Kind() == types.TCOMPLEX128) 
        sel = source.Block.NewValue1(pos, OpComplexReal, x.typs.Float64, source);
        mem = x.storeArgOrLoad(pos, b, sel, mem, x.typs.Float64, storeOffset, loadRegOffset, storeRc.next(x.typs.Float64));
        pos = pos.WithNotStmt();
        sel = source.Block.NewValue1(pos, OpComplexImag, x.typs.Float64, source);
        return _addr_x.storeArgOrLoad(pos, b, sel, mem, x.typs.Float64, storeOffset + 8, loadRegOffset + RO_complex_imag, storeRc)!;
        var s = mem;
    if (source.Op == OpDereference) {
        source.Op = OpLoad; // For purposes of parameter passing expansion, a Dereference is a Load.
    }
    if (storeRc.hasRegs()) {
        storeRc.addArg(source);
    }
    else
 {
        var dst = x.offsetFrom(b, storeRc.storeDest, storeOffset, types.NewPtr(t));
        s = b.NewValue3A(pos, OpStore, types.TypeMem, t, dst, source, mem);
    }
    if (x.debug) {
        x.Printf("-->storeArg returns %s, storeRc=%s\n", s.LongString(), storeRc.String());
    }
    return _addr_s!;

});

// rewriteArgs replaces all the call-parameter Args to a call with their register translation (if any).
// Preceding parameters (code pointers, closure pointer) are preserved, and the memory input is modified
// to account for any parameter stores required.
// Any of the old Args that have their use count fall to zero are marked OpInvalid.
private static void rewriteArgs(this ptr<expandState> _addr_x, ptr<Value> _addr_v, nint firstArg) => func((defer, _, _) => {
    ref expandState x = ref _addr_x.val;
    ref Value v = ref _addr_v.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("rewriteArgs(%s; %d)\n", v.LongString(), firstArg);
    }
    ptr<AuxCall> aux = v.Aux._<ptr<AuxCall>>();
    var pos = v.Pos.WithNotStmt();
    var m0 = v.MemoryArg();
    var mem = m0;
    ref ptr<Value> newArgs = ref heap(new slice<ptr<Value>>(new ptr<Value>[] {  }), out ptr<ptr<Value>> _addr_newArgs);
    ptr<Value> oldArgs = new slice<ptr<Value>>(new ptr<Value>[] {  });
    {
        var a__prev1 = a;

        foreach (var (__i, __a) in v.Args[(int)firstArg..(int)len(v.Args) - 1]) {
            i = __i;
            a = __a; // skip leading non-parameter SSA Args and trailing mem SSA Arg.
            oldArgs = append(oldArgs, a);
            var auxI = int64(i);
            var aRegs = aux.RegsOfArg(auxI);
            var aType = aux.TypeOfArg(auxI);
            if (len(aRegs) == 0 && a.Op == OpDereference) {
                var aOffset = aux.OffsetOfArg(auxI);
                if (a.MemoryArg() != m0) {
                    x.f.Fatalf("Op...LECall and OpDereference have mismatched mem, %s and %s", v.LongString(), a.LongString());
                } 
                // "Dereference" of addressed (probably not-SSA-eligible) value becomes Move
                // TODO(register args) this will be more complicated with registers in the picture.
                mem = x.rewriteDereference(v.Block, x.sp, a, mem, aOffset, aux.SizeOfArg(auxI), aType, pos);

            }
            else
 {
                registerCursor rc = default;
                ptr<slice<ptr<Value>>> result;
                aOffset = default;
                if (len(aRegs) > 0) {
                    result = _addr_newArgs;
                }
                else
 {
                    aOffset = aux.OffsetOfArg(auxI);
                }

                if (x.debug) {
                    x.Printf("...storeArg %s, %v, %d\n", a.LongString(), aType, aOffset);
                }

                rc.init(aRegs, aux.abiInfo, result, x.sp);
                mem = x.storeArgOrLoad(pos, v.Block, a, mem, aType, aOffset, 0, rc);

            }

        }
        a = a__prev1;
    }

    array<ptr<Value>> preArgStore = new array<ptr<Value>>(2);
    var preArgs = append(preArgStore[..(int)0], v.Args[(int)0..(int)firstArg]);
    v.resetArgs();
    v.AddArgs(preArgs);
    v.AddArgs(newArgs);
    v.AddArg(mem);
    {
        var a__prev1 = a;

        foreach (var (_, __a) in oldArgs) {
            a = __a;
            if (a.Uses == 0) {
                if (x.debug) {
                    x.Printf("...marking %v unused\n", a.LongString());
                }
                a.invalidateRecursively();
            }
        }
        a = a__prev1;
    }

    return ;

});

// expandCalls converts LE (Late Expansion) calls that act like they receive value args into a lower-level form
// that is more oriented to a platform's ABI.  The SelectN operations that extract results are rewritten into
// more appropriate forms, and any StructMake or ArrayMake inputs are decomposed until non-struct values are
// reached.  On the callee side, OpArg nodes are not decomposed until this phase is run.
// TODO results should not be lowered until this phase.
private static void expandCalls(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Calls that need lowering have some number of inputs, including a memory input,
    // and produce a tuple of (value1, value2, ..., mem) where valueK may or may not be SSA-able.

    // With the current ABI those inputs need to be converted into stores to memory,
    // rethreading the call's memory input to the first, and the new call now receiving the last.

    // With the current ABI, the outputs need to be converted to loads, which will all use the call's
    // memory output as their input.
    var (sp, _) = f.spSb();
    ptr<expandState> x = addr(new expandState(f:f,abi1:f.ABI1,debug:f.pass.debug>0,canSSAType:f.fe.CanSSA,regSize:f.Config.RegSize,sp:sp,typs:&f.Config.Types,ptrSize:f.Config.PtrSize,namedSelects:make(map[*Value][]namedVal),sdom:f.Sdom(),commonArgs:make(map[selKey]*Value),memForCall:make(map[ID]*Value),transformedSelects:make(map[ID]bool),)); 

    // For 32-bit, need to deal with decomposition of 64-bit integers, which depends on endianness.
    if (f.Config.BigEndian) {
        (x.lowOffset, x.hiOffset) = (4, 0);        (x.loRo, x.hiRo) = (1, 0);
    }
    else
 {
        (x.lowOffset, x.hiOffset) = (0, 4);        (x.loRo, x.hiRo) = (0, 1);
    }
    if (x.debug) {
        x.Printf("\nexpandsCalls(%s)\n", f.Name);
    }
    {
        var i__prev1 = i;
        var name__prev1 = name;

        foreach (var (__i, __name) in f.Names) {
            i = __i;
            name = __name;
            var t = name.Type;
            if (x.isAlreadyExpandedAggregateType(t)) {
                {
                    var j__prev2 = j;
                    var v__prev2 = v;

                    foreach (var (__j, __v) in f.NamedValues[name.val]) {
                        j = __j;
                        v = __v;
                        if (v.Op == OpSelectN || v.Op == OpArg && x.isAlreadyExpandedAggregateType(v.Type)) {
                            var ns = x.namedSelects[v];
                            x.namedSelects[v] = append(ns, new namedVal(locIndex:i,valIndex:j));
                        }
                    }

                    j = j__prev2;
                    v = v__prev2;
                }
            }

        }
        i = i__prev1;
        name = name__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    nint firstArg = 0;

                    if (v.Op == OpStaticLECall)                     else if (v.Op == OpInterLECall) 
                        firstArg = 1;
                    else if (v.Op == OpClosureLECall) 
                        firstArg = 2;
                    else 
                        continue;
                                        x.rewriteArgs(v, firstArg);

                }

                v = v__prev2;
            }

            if (isBlockMultiValueExit(_addr_b)) {
                x.indent(3); 
                // Very similar to code in rewriteArgs, but results instead of args.
                var v = b.Controls[0];
                var m0 = v.MemoryArg();
                var mem = m0;
                var aux = f.OwnAux;
                var pos = v.Pos.WithNotStmt();
                ref ptr<Value> allResults = ref heap(new slice<ptr<Value>>(new ptr<Value>[] {  }), out ptr<ptr<Value>> _addr_allResults);
                if (x.debug) {
                    x.Printf("multiValueExit rewriting %s\n", v.LongString());
                }

                slice<ptr<Value>> oldArgs = default;
                {
                    var j__prev2 = j;
                    var a__prev2 = a;

                    foreach (var (__j, __a) in v.Args[..(int)len(v.Args) - 1]) {
                        j = __j;
                        a = __a;
                        oldArgs = append(oldArgs, a);
                        var i = int64(j);
                        var auxType = aux.TypeOfResult(i);
                        var auxBase = b.NewValue2A(v.Pos, OpLocalAddr, types.NewPtr(auxType), aux.NameOfResult(i), x.sp, mem);
                        var auxOffset = int64(0);
                        var auxSize = aux.SizeOfResult(i);
                        var aRegs = aux.RegsOfResult(int64(j));
                        if (len(aRegs) == 0 && a.Op == OpDereference) { 
                            // Avoid a self-move, and if one is detected try to remove the already-inserted VarDef for the assignment that won't happen.
                            {
                                var dAddr = a.Args[0];
                                var dMem = a.Args[1];

                                if (dAddr.Op == OpLocalAddr && dAddr.Args[0].Op == OpSP && dAddr.Args[1] == dMem && dAddr.Aux == aux.NameOfResult(i)) {
                                    if (dMem.Op == OpVarDef && dMem.Aux == dAddr.Aux) {
                                        dMem.copyOf(dMem.MemoryArg()); // elide the VarDef
                                    }

                                    continue;

                                }

                            }

                            mem = x.rewriteDereference(v.Block, auxBase, a, mem, auxOffset, auxSize, auxType, pos);

                        }
                        else
 {
                            if (a.Op == OpLoad && a.Args[0].Op == OpLocalAddr) {
                                var addr = a.Args[0]; // This is a self-move. // TODO(register args) do what here for registers?
                                if (addr.MemoryArg() == a.MemoryArg() && addr.Aux == aux.NameOfResult(i)) {
                                    continue;
                                }

                            }

                            registerCursor rc = default;
                            ptr<slice<ptr<Value>>> result;
                            if (len(aRegs) > 0) {
                                result = _addr_allResults;
                            }

                            rc.init(aRegs, aux.abiInfo, result, auxBase);
                            mem = x.storeArgOrLoad(v.Pos, b, a, mem, aux.TypeOfResult(i), auxOffset, 0, rc);

                        }

                    }

                    j = j__prev2;
                    a = a__prev2;
                }

                v.resetArgs();
                v.AddArgs(allResults);
                v.AddArg(mem);
                v.Type = types.NewResults(append(abi.RegisterTypes(aux.abiInfo.OutParams()), types.TypeMem));
                b.SetControl(v);
                {
                    var a__prev2 = a;

                    foreach (var (_, __a) in oldArgs) {
                        a = __a;
                        if (a.Uses == 0) {
                            if (x.debug) {
                                x.Printf("...marking %v unused\n", a.LongString());
                            }
                            a.invalidateRecursively();
                        }
                    }

                    a = a__prev2;
                }

                if (x.debug) {
                    x.Printf("...multiValueExit new result %s\n", v.LongString());
                }

                x.indent(-3);

            }

        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op == OpStore) {
                        t = v.Aux._<ptr<types.Type>>();
                        var source = v.Args[1];
                        var tSrc = source.Type;
                        var iAEATt = x.isAlreadyExpandedAggregateType(t);

                        if (!iAEATt) { 
                            // guarding against store immediate struct into interface data field -- store type is *uint8
                            // TODO can this happen recursively?
                            iAEATt = x.isAlreadyExpandedAggregateType(tSrc);
                            if (iAEATt) {
                                t = tSrc;
                            }

                        }

                        var dst = v.Args[0];
                        mem = v.Args[2];
                        mem = x.storeArgOrLoad(v.Pos, b, source, mem, t, 0, 0, new registerCursor(storeDest:dst));
                        v.copyOf(mem);

                    }

                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    var val2Preds = make_map<ptr<Value>, int>(); // Used to accumulate dependency graph of selection operations for topological ordering.

    // Step 2: transform or accumulate selection operations for rewrite in topological order.
    //
    // Aggregate types that have already (in earlier phases) been transformed must be lowered comprehensively to finish
    // the transformation (user-defined structs and arrays, slices, strings, interfaces, complex, 64-bit on 32-bit architectures),
    //
    // Any select-for-addressing applied to call results can be transformed directly.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v; 
                    // Accumulate chains of selectors for processing in topological order

                    if (v.Op == OpStructSelect || v.Op == OpArraySelect || v.Op == OpIData || v.Op == OpITab || v.Op == OpStringPtr || v.Op == OpStringLen || v.Op == OpSlicePtr || v.Op == OpSliceLen || v.Op == OpSliceCap || v.Op == OpSlicePtrUnchecked || v.Op == OpComplexReal || v.Op == OpComplexImag || v.Op == OpInt64Hi || v.Op == OpInt64Lo)
                    {
                        var w = v.Args[0];

                        if (w.Op == OpStructSelect || w.Op == OpArraySelect || w.Op == OpSelectN || w.Op == OpArg) 
                            val2Preds[w] += 1;
                            if (x.debug) {
                                x.Printf("v2p[%s] = %d\n", w.LongString(), val2Preds[w]);
                            }
                                                fallthrough = true;

                    }
                    if (fallthrough || v.Op == OpSelectN)
                    {
                        {
                            var (_, ok) = val2Preds[v];

                            if (!ok) {
                                val2Preds[v] = 0;
                                if (x.debug) {
                                    x.Printf("v2p[%s] = %d\n", v.LongString(), val2Preds[v]);
                                }
                            }

                        }


                        goto __switch_break1;
                    }
                    if (v.Op == OpArg)
                    {
                        if (!x.isAlreadyExpandedAggregateType(v.Type)) {
                            continue;
                        }
                        {
                            (_, ok) = val2Preds[v];

                            if (!ok) {
                                val2Preds[v] = 0;
                                if (x.debug) {
                                    x.Printf("v2p[%s] = %d\n", v.LongString(), val2Preds[v]);
                                }
                            }

                        }


                        goto __switch_break1;
                    }
                    if (v.Op == OpSelectNAddr) 
                    {
                        // Do these directly, there are no chains of selectors.
                        var call = v.Args[0];
                        var which = v.AuxInt;
                        aux = call.Aux._<ptr<AuxCall>>();
                        var pt = v.Type;
                        var off = x.offsetFrom(x.f.Entry, x.sp, aux.OffsetOfResult(which), pt);
                        v.copyOf(off);
                        goto __switch_break1;
                    }

                    __switch_break1:;

                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    slice<ptr<Value>> toProcess = default;
    Func<nint, nint, bool> less = (i, j) => {
        var vi = toProcess[i];
        var vj = toProcess[j];
        var bi = vi.Block;
        var bj = vj.Block;
        if (bi == bj) {
            return vi.ID < vj.ID;
        }
        return x.sdom.domorder(bi) > x.sdom.domorder(bj); // reverse the order to put dominators last.
    }; 

    // Accumulate order in allOrdered
    slice<ptr<Value>> allOrdered = default;
    {
        var v__prev1 = v;
        var n__prev1 = n;

        foreach (var (__v, __n) in val2Preds) {
            v = __v;
            n = __n;
            if (n == 0) {
                allOrdered = append(allOrdered, v);
            }
        }
        v = v__prev1;
        n = n__prev1;
    }

    nint last = 0; // allOrdered[0:last] has been top-sorted and processed
    while (len(val2Preds) > 0) {
        toProcess = allOrdered[(int)last..];
        last = len(allOrdered);
        sort.SliceStable(toProcess, less);
        {
            var v__prev2 = v;

            foreach (var (_, __v) in toProcess) {
                v = __v;
                delete(val2Preds, v);
                if (v.Op == OpArg) {
                    continue; // no Args[0], hence done.
                }

                w = v.Args[0];
                var (n, ok) = val2Preds[w];
                if (!ok) {
                    continue;
                }

                if (n == 1) {
                    allOrdered = append(allOrdered, w);
                    delete(val2Preds, w);
                    continue;
                }

                val2Preds[w] = n - 1;

            }

            v = v__prev2;
        }
    }

    x.commonSelectors = make_map<selKey, ptr<Value>>(); 
    // Rewrite duplicate selectors as copies where possible.
    {
        var i__prev1 = i;

        for (i = len(allOrdered) - 1; i >= 0; i--) {
            v = allOrdered[i];
            if (v.Op == OpArg) {
                continue;
            }
            w = v.Args[0];
            if (w.Op == OpCopy) {
                while (w.Op == OpCopy) {
                    w = w.Args[0];
                }

                v.SetArg(0, w);
            }
            var typ = v.Type;
            if (typ.IsMemory()) {
                continue; // handled elsewhere, not an indexable result
            }

            var size = typ.Width;
            var offset = int64(0);

            if (v.Op == OpStructSelect) 
                if (w.Type.Kind() == types.TSTRUCT) {
                    offset = w.Type.FieldOff(int(v.AuxInt));
                }
                else
 { // Immediate interface data artifact, offset is zero.
                    f.Fatalf("Expand calls interface data problem, func %s, v=%s, w=%s\n", f.Name, v.LongString(), w.LongString());

                }

            else if (v.Op == OpArraySelect) 
                offset = size * v.AuxInt;
            else if (v.Op == OpSelectN) 
                offset = v.AuxInt; // offset is just a key, really.
            else if (v.Op == OpInt64Hi) 
                offset = x.hiOffset;
            else if (v.Op == OpInt64Lo) 
                offset = x.lowOffset;
            else if (v.Op == OpStringLen || v.Op == OpSliceLen || v.Op == OpIData) 
                offset = x.ptrSize;
            else if (v.Op == OpSliceCap) 
                offset = 2 * x.ptrSize;
            else if (v.Op == OpComplexImag) 
                offset = size;
                        selKey sk = new selKey(from:w,size:size,offsetOrIndex:offset,typ:typ);
            var dupe = x.commonSelectors[sk];
            if (dupe == null) {
                x.commonSelectors[sk] = v;
            }
            else if (x.sdom.IsAncestorEq(dupe.Block, v.Block)) {
                if (x.debug) {
                    x.Printf("Duplicate, make %s copy of %s\n", v, dupe);
                }
                v.copyOf(dupe);
            }
            else
 { 
                // Because values are processed in dominator order, the old common[s] will never dominate after a miss is seen.
                // Installing the new value might match some future values.
                x.commonSelectors[sk] = v;

            }

        }

        i = i__prev1;
    } 

    // Indices of entries in f.Names that need to be deleted.
    slice<namedVal> toDelete = default; 

    // Rewrite selectors.
    {
        var i__prev1 = i;
        var v__prev1 = v;

        foreach (var (__i, __v) in allOrdered) {
            i = __i;
            v = __v;
            if (x.debug) {
                var b = v.Block;
                x.Printf("allOrdered[%d] = b%d, %s, uses=%d\n", i, b.ID, v.LongString(), v.Uses);
            }
            if (v.Uses == 0) {
                v.invalidateRecursively();
                continue;
            }
            if (v.Op == OpCopy) {
                continue;
            }
            var locs = x.rewriteSelect(v, v, 0, 0); 
            // Install new names.
            if (v.Type.IsMemory()) {
                continue;
            } 
            // Leaf types may have debug locations
            if (!x.isAlreadyExpandedAggregateType(v.Type)) {
                foreach (var (_, l) in locs) {
                    {
                        (_, ok) = f.NamedValues[l.val];

                        if (!ok) {
                            f.Names = append(f.Names, l);
                        }

                    }

                    f.NamedValues[l.val] = append(f.NamedValues[l.val], v);

                }
                continue;

            }

            {
                var ns__prev1 = ns;

                var (ns, ok) = x.namedSelects[v];

                if (ok) { 
                    // Not-leaf types that had debug locations need to lose them.

                    toDelete = append(toDelete, ns);

                }

                ns = ns__prev1;

            }

        }
        i = i__prev1;
        v = v__prev1;
    }

    deleteNamedVals(f, toDelete); 

    // Step 4: rewrite the calls themselves, correcting the type.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;

                    if (v.Op == OpArg) 
                        x.rewriteArgToMemOrRegs(v);
                    else if (v.Op == OpStaticLECall) 
                        v.Op = OpStaticCall;
                        var rts = abi.RegisterTypes(v.Aux._<ptr<AuxCall>>().abiInfo.OutParams());
                        v.Type = types.NewResults(append(rts, types.TypeMem));
                    else if (v.Op == OpClosureLECall) 
                        v.Op = OpClosureCall;
                        rts = abi.RegisterTypes(v.Aux._<ptr<AuxCall>>().abiInfo.OutParams());
                        v.Type = types.NewResults(append(rts, types.TypeMem));
                    else if (v.Op == OpInterLECall) 
                        v.Op = OpInterCall;
                        rts = abi.RegisterTypes(v.Aux._<ptr<AuxCall>>().abiInfo.OutParams());
                        v.Type = types.NewResults(append(rts, types.TypeMem));
                    
                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    array<ptr<Value>> IArg = new array<ptr<Value>>(32);    array<ptr<Value>> FArg = new array<ptr<Value>>(32);

    {
        var v__prev1 = v;

        foreach (var (_, __v) in f.Entry.Values) {
            v = __v;

            if (v.Op == OpArgIntReg) 
                i = v.AuxInt;
                {
                    var w__prev1 = w;

                    w = IArg[i];

                    if (w != null) {
                        if (w.Type.Width != v.Type.Width) {
                            f.Fatalf("incompatible OpArgIntReg [%d]: %s and %s", i, v.LongString(), w.LongString());
                        }
                        if (w.Type.IsUnsafePtr() && !v.Type.IsUnsafePtr()) { 
                            // Update unsafe.Pointer type if we know the actual pointer type.
                            w.Type = v.Type;

                        } 
                        // TODO: don't dedup pointer and scalar? Rewrite to OpConvert? Can it happen?
                        v.copyOf(w);

                    }
                    else
 {
                        IArg[i] = v;
                    }

                    w = w__prev1;

                }

            else if (v.Op == OpArgFloatReg) 
                i = v.AuxInt;
                {
                    var w__prev1 = w;

                    w = FArg[i];

                    if (w != null) {
                        if (w.Type.Width != v.Type.Width) {
                            f.Fatalf("incompatible OpArgFloatReg [%d]: %v and %v", i, v, w);
                        }
                        v.copyOf(w);
                    }
                    else
 {
                        FArg[i] = v;
                    }

                    w = w__prev1;

                }

                    }
        v = v__prev1;
    }

    {
        var name__prev1 = name;

        foreach (var (_, __name) in f.Names) {
            name = __name;
            var values = f.NamedValues[name.val];
            {
                var i__prev2 = i;
                var v__prev2 = v;

                foreach (var (__i, __v) in values) {
                    i = __i;
                    v = __v;
                    if (v.Op == OpCopy) {
                        var a = v.Args[0];
                        while (a.Op == OpCopy) {
                            a = a.Args[0];
                        }

                        values[i] = a;
                    }
                }

                i = i__prev2;
                v = v__prev2;
            }
        }
        name = name__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    {
                        var i__prev3 = i;
                        var a__prev3 = a;

                        foreach (var (__i, __a) in v.Args) {
                            i = __i;
                            a = __a;
                            if (a.Op != OpCopy) {
                                continue;
                            }
                            var aa = copySource(a);
                            v.SetArg(i, aa);
                            while (a.Uses == 0) {
                                b = a.Args[0];
                                a.invalidateRecursively();
                                a = b;
                            }
                        }

                        i = i__prev3;
                        a = a__prev3;
                    }
                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    {
                        var a__prev3 = a;

                        foreach (var (_, __a) in v.Args) {
                            a = __a;
                            if (a.Pos.IsStmt() != src.PosIsStmt) {
                                continue;
                            }
                            if (a.Type.IsMemory()) {
                                continue;
                            }
                            if (a.Pos.Line() != v.Pos.Line()) {
                                continue;
                            }
                            if (!a.Pos.SameFile(v.Pos)) {
                                continue;
                            }

                            if (a.Op == OpArgIntReg || a.Op == OpArgFloatReg || a.Op == OpSelectN) 
                                v.Pos = v.Pos.WithIsStmt();
                                a.Pos = a.Pos.WithDefaultStmt();
                            
                        }

                        a = a__prev3;
                    }
                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }
}

// rewriteArgToMemOrRegs converts OpArg v in-place into the register version of v,
// if that is appropriate.
private static ptr<Value> rewriteArgToMemOrRegs(this ptr<expandState> _addr_x, ptr<Value> _addr_v) => func((defer, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref Value v = ref _addr_v.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("rewriteArgToMemOrRegs(%s)\n", v.LongString());
    }
    var pa = x.prAssignForArg(v);
    switch (len(pa.Registers)) {
        case 0: 
            ptr<ir.Name> frameOff = v.Aux._<ptr<ir.Name>>().FrameOffset();
            if (pa.Offset() != int32(frameOff + x.f.ABISelf.LocalsOffset())) {
                panic(fmt.Errorf("Parameter assignment %d and OpArg.Aux frameOffset %d disagree, op=%s", pa.Offset(), frameOff, v.LongString()));
            }
            break;
        case 1: 
            var t = v.Type;
            selKey key = new selKey(v,0,t.Width,t);
            var w = x.commonArgs[key];
            if (w != null) {
                v.copyOf(w);
                break;
            }
            var r = pa.Registers[0];
            long i = default;
            v.Op, i = ArgOpAndRegisterFor(r, _addr_x.f.ABISelf);
            v.Aux = addr(new AuxNameOffset(v.Aux.(*ir.Name),0));
            v.AuxInt = i;
            x.commonArgs[key] = v;

            break;
        default: 
            panic(badVal("Saw unexpanded OpArg", _addr_v));
            break;
    }
    if (x.debug) {
        x.Printf("-->%s\n", v.LongString());
    }
    return _addr_v!;

});

// newArgToMemOrRegs either rewrites toReplace into an OpArg referencing memory or into an OpArgXXXReg to a register,
// or rewrites it into a copy of the appropriate OpArgXXX.  The actual OpArgXXX is determined by combining baseArg (an OpArg)
// with offset, regOffset, and t to determine which portion of it to reference (either all or a part, in memory or in registers).
private static ptr<Value> newArgToMemOrRegs(this ptr<expandState> _addr_x, ptr<Value> _addr_baseArg, ptr<Value> _addr_toReplace, long offset, Abi1RO regOffset, ptr<types.Type> _addr_t, src.XPos pos) => func((defer, panic, _) => {
    ref expandState x = ref _addr_x.val;
    ref Value baseArg = ref _addr_baseArg.val;
    ref Value toReplace = ref _addr_toReplace.val;
    ref types.Type t = ref _addr_t.val;

    if (x.debug) {
        x.indent(3);
        defer(x.indent(-3));
        x.Printf("newArgToMemOrRegs(base=%s; toReplace=%s; t=%s; memOff=%d; regOff=%d)\n", baseArg.String(), toReplace.LongString(), t.String(), offset, regOffset);
    }
    selKey key = new selKey(baseArg,offset,t.Width,t);
    var w = x.commonArgs[key];
    if (w != null) {
        if (toReplace != null) {
            toReplace.copyOf(w);
        }
        return _addr_w!;

    }
    var pa = x.prAssignForArg(baseArg);
    if (len(pa.Registers) == 0) { // Arg is on stack
        ptr<ir.Name> frameOff = baseArg.Aux._<ptr<ir.Name>>().FrameOffset();
        if (pa.Offset() != int32(frameOff + x.f.ABISelf.LocalsOffset())) {
            panic(fmt.Errorf("Parameter assignment %d and OpArg.Aux frameOffset %d disagree, op=%s", pa.Offset(), frameOff, baseArg.LongString()));
        }
        var aux = baseArg.Aux;
        var auxInt = baseArg.AuxInt + offset;
        if (toReplace != null && toReplace.Block == baseArg.Block) {
            toReplace.reset(OpArg);
            toReplace.Aux = aux;
            toReplace.AuxInt = auxInt;
            toReplace.Type = t;
            w = toReplace;
        }
        else
 {
            w = baseArg.Block.NewValue0IA(pos, OpArg, t, auxInt, aux);
        }
        x.commonArgs[key] = w;
        if (toReplace != null) {
            toReplace.copyOf(w);
        }
        if (x.debug) {
            x.Printf("-->%s\n", w.LongString());
        }
        return _addr_w!;

    }
    var r = pa.Registers[regOffset];
    var (op, auxInt) = ArgOpAndRegisterFor(r, _addr_x.f.ABISelf);
    if (op == OpArgIntReg && t.IsFloat() || op == OpArgFloatReg && t.IsInteger()) {
        fmt.Printf("pa=%v\nx.f.OwnAux.abiInfo=%s\n", pa.ToString(x.f.ABISelf, true), x.f.OwnAux.abiInfo.String());
        panic(fmt.Errorf("Op/Type mismatch, op=%s, type=%s", op.String(), t.String()));
    }
    if (baseArg.AuxInt != 0) {
        @base.Fatalf("BaseArg %s bound to registers has non-zero AuxInt", baseArg.LongString());
    }
    aux = addr(new AuxNameOffset(baseArg.Aux.(*ir.Name),offset));
    if (toReplace != null && toReplace.Block == baseArg.Block) {
        toReplace.reset(op);
        toReplace.Aux = aux;
        toReplace.AuxInt = auxInt;
        toReplace.Type = t;
        w = toReplace;
    }
    else
 {
        w = baseArg.Block.NewValue0IA(pos, op, t, auxInt, aux);
    }
    x.commonArgs[key] = w;
    if (toReplace != null) {
        toReplace.copyOf(w);
    }
    if (x.debug) {
        x.Printf("-->%s\n", w.LongString());
    }
    return _addr_w!;


});

// argOpAndRegisterFor converts an abi register index into an ssa Op and corresponding
// arg register index.
public static (Op, long) ArgOpAndRegisterFor(abi.RegIndex r, ptr<abi.ABIConfig> _addr_abiConfig) {
    Op _p0 = default;
    long _p0 = default;
    ref abi.ABIConfig abiConfig = ref _addr_abiConfig.val;

    var i = abiConfig.FloatIndexFor(r);
    if (i >= 0) { // float PR
        return (OpArgFloatReg, i);

    }
    return (OpArgIntReg, int64(r));

}

} // end ssa_package
