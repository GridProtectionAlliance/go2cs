// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package abi -- go2cs converted at 2022 March 13 06:00:54 UTC
// import "cmd/compile/internal/abi" ==> using abi = go.cmd.compile.@internal.abi_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\abi\abiutils.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using sync = sync_package;


//......................................................................
//
// Public/exported bits of the ABI utilities.
//

// ABIParamResultInfo stores the results of processing a given
// function type to compute stack layout and register assignments. For
// each input and output parameter we capture whether the param was
// register-assigned (and to which register(s)) or the stack offset
// for the param if is not going to be passed in registers according
// to the rules in the Go internal ABI specification (1.17).

using System;
public static partial class abi_package {

public partial struct ABIParamResultInfo {
    public slice<ABIParamAssignment> inparams; // Includes receiver for method calls.  Does NOT include hidden closure pointer.
    public slice<ABIParamAssignment> outparams;
    public long offsetToSpillArea;
    public long spillAreaSize;
    public nint inRegistersUsed;
    public nint outRegistersUsed;
    public ptr<ABIConfig> config; // to enable String() method
}

private static ptr<ABIConfig> Config(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return _addr_a.config!;
}

private static slice<ABIParamAssignment> InParams(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.inparams;
}

private static slice<ABIParamAssignment> OutParams(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.outparams;
}

private static nint InRegistersUsed(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.inRegistersUsed;
}

private static nint OutRegistersUsed(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.outRegistersUsed;
}

private static ptr<ABIParamAssignment> InParam(this ptr<ABIParamResultInfo> _addr_a, nint i) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return _addr__addr_a.inparams[i]!;
}

private static ptr<ABIParamAssignment> OutParam(this ptr<ABIParamResultInfo> _addr_a, nint i) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return _addr__addr_a.outparams[i]!;
}

private static long SpillAreaOffset(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.offsetToSpillArea;
}

private static long SpillAreaSize(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.spillAreaSize;
}

// ArgWidth returns the amount of stack needed for all the inputs
// and outputs of a function or method, including ABI-defined parameter
// slots and ABI-defined spill slots for register-resident parameters.
// The name is inherited from (*Type).ArgWidth(), which it replaces.
private static long ArgWidth(this ptr<ABIParamResultInfo> _addr_a) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    return a.spillAreaSize + a.offsetToSpillArea - a.config.LocalsOffset();
}

// RegIndex stores the index into the set of machine registers used by
// the ABI on a specific architecture for parameter passing.  RegIndex
// values 0 through N-1 (where N is the number of integer registers
// used for param passing according to the ABI rules) describe integer
// registers; values N through M (where M is the number of floating
// point registers used).  Thus if the ABI says there are 5 integer
// registers and 7 floating point registers, then RegIndex value of 4
// indicates the 5th integer register, and a RegIndex value of 11
// indicates the 7th floating point register.
public partial struct RegIndex { // : byte
}

// ABIParamAssignment holds information about how a specific param or
// result will be passed: in registers (in which case 'Registers' is
// populated) or on the stack (in which case 'Offset' is set to a
// non-negative stack offset. The values in 'Registers' are indices
// (as described above), not architected registers.
public partial struct ABIParamAssignment {
    public ptr<types.Type> Type;
    public types.Object Name; // should always be *ir.Name, used to match with a particular ssa.OpArg.
    public slice<RegIndex> Registers;
    public int offset;
}

// Offset returns the stack offset for addressing the parameter that "a" describes.
// This will panic if "a" describes a register-allocated parameter.
private static int Offset(this ptr<ABIParamAssignment> _addr_a) {
    ref ABIParamAssignment a = ref _addr_a.val;

    if (len(a.Registers) > 0) {
        @base.Fatalf("register allocated parameters have no offset");
    }
    return a.offset;
}

// RegisterTypes returns a slice of the types of the registers
// corresponding to a slice of parameters.  The returned slice
// has capacity for one more, likely a memory type.
public static slice<ptr<types.Type>> RegisterTypes(slice<ABIParamAssignment> apa) {
    nint rcount = 0;
    {
        var pa__prev1 = pa;

        foreach (var (_, __pa) in apa) {
            pa = __pa;
            rcount += len(pa.Registers);
        }
        pa = pa__prev1;
    }

    if (rcount == 0) { 
        // Note that this catches top-level struct{} and [0]Foo, which are stack allocated.
        return make_slice<ptr<types.Type>>(0, 1);
    }
    var rts = make_slice<ptr<types.Type>>(0, rcount + 1);
    {
        var pa__prev1 = pa;

        foreach (var (_, __pa) in apa) {
            pa = __pa;
            if (len(pa.Registers) == 0) {
                continue;
            }
            rts = appendParamTypes(rts, _addr_pa.Type);
        }
        pa = pa__prev1;
    }

    return rts;
}

private static (slice<ptr<types.Type>>, slice<long>) RegisterTypesAndOffsets(this ptr<ABIParamAssignment> _addr_pa) {
    slice<ptr<types.Type>> _p0 = default;
    slice<long> _p0 = default;
    ref ABIParamAssignment pa = ref _addr_pa.val;

    var l = len(pa.Registers);
    if (l == 0) {
        return (null, null);
    }
    var typs = make_slice<ptr<types.Type>>(0, l);
    var offs = make_slice<long>(0, l);
    offs, _ = appendParamOffsets(offs, 0, _addr_pa.Type);
    return (appendParamTypes(typs, _addr_pa.Type), offs);
}

private static slice<ptr<types.Type>> appendParamTypes(slice<ptr<types.Type>> rts, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var w = t.Width;
    if (w == 0) {
        return rts;
    }
    if (t.IsScalar() || t.IsPtrShaped()) {
        if (t.IsComplex()) {
            var c = types.FloatForComplex(t);
            return append(rts, c, c);
        }
        else
 {
            if (int(t.Size()) <= types.RegSize) {
                return append(rts, t);
            } 
            // assume 64bit int on 32-bit machine
            // TODO endianness? Should high-order (sign bits) word come first?
            if (t.IsSigned()) {
                rts = append(rts, types.Types[types.TINT32]);
            }
            else
 {
                rts = append(rts, types.Types[types.TUINT32]);
            }
            return append(rts, types.Types[types.TUINT32]);
        }
    }
    else
 {
        var typ = t.Kind();

        if (typ == types.TARRAY) 
            for (var i = int64(0); i < t.NumElem(); i++) { // 0 gets no registers, plus future-proofing.
                rts = appendParamTypes(rts, _addr_t.Elem());
            }
        else if (typ == types.TSTRUCT) 
            foreach (var (_, f) in t.FieldSlice()) {
                if (f.Type.Size() > 0) { // embedded zero-width types receive no registers
                    rts = appendParamTypes(rts, _addr_f.Type);
                }
            }
        else if (typ == types.TSLICE) 
            return appendParamTypes(rts, _addr_synthSlice);
        else if (typ == types.TSTRING) 
            return appendParamTypes(rts, _addr_synthString);
        else if (typ == types.TINTER) 
            return appendParamTypes(rts, _addr_synthIface);
            }
    return rts;
}

// appendParamOffsets appends the offset(s) of type t, starting from "at",
// to input offsets, and returns the longer slice and the next unused offset.
private static (slice<long>, long) appendParamOffsets(slice<long> offsets, long at, ptr<types.Type> _addr_t) {
    slice<long> _p0 = default;
    long _p0 = default;
    ref types.Type t = ref _addr_t.val;

    at = align(at, _addr_t);
    var w = t.Width;
    if (w == 0) {
        return (offsets, at);
    }
    if (t.IsScalar() || t.IsPtrShaped()) {
        if (t.IsComplex() || int(t.Width) > types.RegSize) { // complex and *int64 on 32-bit
            var s = w / 2;
            return (append(offsets, at, at + s), at + w);
        }
        else
 {
            return (append(offsets, at), at + w);
        }
    }
    else
 {
        var typ = t.Kind();

        if (typ == types.TARRAY) 
            {
                var i__prev1 = i;

                for (var i = int64(0); i < t.NumElem(); i++) {
                    offsets, at = appendParamOffsets(offsets, at, _addr_t.Elem());
                }


                i = i__prev1;
            }
        else if (typ == types.TSTRUCT) 
            {
                var i__prev1 = i;

                foreach (var (__i, __f) in t.FieldSlice()) {
                    i = __i;
                    f = __f;
                    offsets, at = appendParamOffsets(offsets, at, _addr_f.Type);
                    if (f.Type.Width == 0 && i == t.NumFields() - 1) {
                        at++; // last field has zero width
                    }
                }

                i = i__prev1;
            }

            at = align(at, _addr_t); // type size is rounded up to its alignment
        else if (typ == types.TSLICE) 
            return appendParamOffsets(offsets, at, _addr_synthSlice);
        else if (typ == types.TSTRING) 
            return appendParamOffsets(offsets, at, _addr_synthString);
        else if (typ == types.TINTER) 
            return appendParamOffsets(offsets, at, _addr_synthIface);
            }
    return (offsets, at);
}

// FrameOffset returns the frame-pointer-relative location that a function
// would spill its input or output parameter to, if such a spill slot exists.
// If there is none defined (e.g., register-allocated outputs) it panics.
// For register-allocated inputs that is their spill offset reserved for morestack;
// for stack-allocated inputs and outputs, that is their location on the stack.
// (In a future version of the ABI, register-resident inputs may lose their defined
// spill area to help reduce stack sizes.)
private static long FrameOffset(this ptr<ABIParamAssignment> _addr_a, ptr<ABIParamResultInfo> _addr_i) {
    ref ABIParamAssignment a = ref _addr_a.val;
    ref ABIParamResultInfo i = ref _addr_i.val;

    if (a.offset == -1) {
        @base.Fatalf("function parameter has no ABI-defined frame-pointer offset");
    }
    if (len(a.Registers) == 0) { // passed on stack
        return int64(a.offset) - i.config.LocalsOffset();
    }
    return int64(a.offset) + i.SpillAreaOffset() - i.config.LocalsOffset();
}

// RegAmounts holds a specified number of integer/float registers.
public partial struct RegAmounts {
    public nint intRegs;
    public nint floatRegs;
}

// ABIConfig captures the number of registers made available
// by the ABI rules for parameter passing and result returning.
public partial struct ABIConfig {
    public long offsetForLocals; // e.g., obj.(*Link).FixedFrameSize() -- extra linkage information on some architectures.
    public RegAmounts regAmounts;
    public map<ptr<types.Type>, nint> regsForTypeCache;
}

// NewABIConfig returns a new ABI configuration for an architecture with
// iRegsCount integer/pointer registers and fRegsCount floating point registers.
public static ptr<ABIConfig> NewABIConfig(nint iRegsCount, nint fRegsCount, long offsetForLocals) {
    return addr(new ABIConfig(offsetForLocals:offsetForLocals,regAmounts:RegAmounts{iRegsCount,fRegsCount},regsForTypeCache:make(map[*types.Type]int)));
}

// Copy returns a copy of an ABIConfig for use in a function's compilation so that access to the cache does not need to be protected with a mutex.
private static ptr<ABIConfig> Copy(this ptr<ABIConfig> _addr_a) {
    ref ABIConfig a = ref _addr_a.val;

    ref var b = ref heap(a.val, out ptr<var> _addr_b);
    b.regsForTypeCache = make_map<ptr<types.Type>, nint>();
    return _addr__addr_b!;
}

// LocalsOffset returns the architecture-dependent offset from SP for args and results.
// In theory this is only used for debugging; it ought to already be incorporated into
// results from the ABI-related methods
private static long LocalsOffset(this ptr<ABIConfig> _addr_a) {
    ref ABIConfig a = ref _addr_a.val;

    return a.offsetForLocals;
}

// FloatIndexFor translates r into an index in the floating point parameter
// registers.  If the result is negative, the input index was actually for the
// integer parameter registers.
private static long FloatIndexFor(this ptr<ABIConfig> _addr_a, RegIndex r) {
    ref ABIConfig a = ref _addr_a.val;

    return int64(r) - int64(a.regAmounts.intRegs);
}

// NumParamRegs returns the number of parameter registers used for a given type,
// without regard for the number available.
private static nint NumParamRegs(this ptr<ABIConfig> _addr_a, ptr<types.Type> _addr_t) {
    ref ABIConfig a = ref _addr_a.val;
    ref types.Type t = ref _addr_t.val;

    nint n = default;
    {
        nint n__prev1 = n;

        var (n, ok) = a.regsForTypeCache[t];

        if (ok) {
            return n;
        }
        n = n__prev1;

    }

    if (t.IsScalar() || t.IsPtrShaped()) {
        if (t.IsComplex()) {
            n = 2;
        }
        else
 {
            n = (int(t.Size()) + types.RegSize - 1) / types.RegSize;
        }
    }
    else
 {
        var typ = t.Kind();

        if (typ == types.TARRAY) 
            n = a.NumParamRegs(t.Elem()) * int(t.NumElem());
        else if (typ == types.TSTRUCT) 
            foreach (var (_, f) in t.FieldSlice()) {
                n += a.NumParamRegs(f.Type);
            }
        else if (typ == types.TSLICE) 
            n = a.NumParamRegs(synthSlice);
        else if (typ == types.TSTRING) 
            n = a.NumParamRegs(synthString);
        else if (typ == types.TINTER) 
            n = a.NumParamRegs(synthIface);
            }
    a.regsForTypeCache[t] = n;

    return n;
}

// preAllocateParams gets the slice sizes right for inputs and outputs.
private static void preAllocateParams(this ptr<ABIParamResultInfo> _addr_a, bool hasRcvr, nint nIns, nint nOuts) {
    ref ABIParamResultInfo a = ref _addr_a.val;

    if (hasRcvr) {
        nIns++;
    }
    a.inparams = make_slice<ABIParamAssignment>(0, nIns);
    a.outparams = make_slice<ABIParamAssignment>(0, nOuts);
}

// ABIAnalyzeTypes takes an optional receiver type, arrays of ins and outs, and returns an ABIParamResultInfo,
// based on the given configuration.  This is the same result computed by config.ABIAnalyze applied to the
// corresponding method/function type, except that all the embedded parameter names are nil.
// This is intended for use by ssagen/ssa.go:(*state).rtcall, for runtime functions that lack a parsed function type.
private static ptr<ABIParamResultInfo> ABIAnalyzeTypes(this ptr<ABIConfig> _addr_config, ptr<types.Type> _addr_rcvr, slice<ptr<types.Type>> ins, slice<ptr<types.Type>> outs) {
    ref ABIConfig config = ref _addr_config.val;
    ref types.Type rcvr = ref _addr_rcvr.val;

    setup();
    assignState s = new assignState(stackOffset:config.offsetForLocals,rTotal:config.regAmounts,);
    ptr<ABIParamResultInfo> result = addr(new ABIParamResultInfo(config:config));
    result.preAllocateParams(rcvr != null, len(ins), len(outs)); 

    // Receiver
    if (rcvr != null) {
        result.inparams = append(result.inparams, s.assignParamOrReturn(rcvr, null, false));
    }
    {
        var t__prev1 = t;

        foreach (var (_, __t) in ins) {
            t = __t;
            result.inparams = append(result.inparams, s.assignParamOrReturn(t, null, false));
        }
        t = t__prev1;
    }

    s.stackOffset = types.Rnd(s.stackOffset, int64(types.RegSize));
    result.inRegistersUsed = s.rUsed.intRegs + s.rUsed.floatRegs; 

    // Outputs
    s.rUsed = new RegAmounts();
    {
        var t__prev1 = t;

        foreach (var (_, __t) in outs) {
            t = __t;
            result.outparams = append(result.outparams, s.assignParamOrReturn(t, null, true));
        }
        t = t__prev1;
    }

    result.offsetToSpillArea = alignTo(s.stackOffset, types.RegSize);
    result.spillAreaSize = alignTo(s.spillOffset, types.RegSize);
    result.outRegistersUsed = s.rUsed.intRegs + s.rUsed.floatRegs;

    return _addr_result!;
}

// ABIAnalyzeFuncType takes a function type 'ft' and an ABI rules description
// 'config' and analyzes the function to determine how its parameters
// and results will be passed (in registers or on the stack), returning
// an ABIParamResultInfo object that holds the results of the analysis.
private static ptr<ABIParamResultInfo> ABIAnalyzeFuncType(this ptr<ABIConfig> _addr_config, ptr<types.Func> _addr_ft) {
    ref ABIConfig config = ref _addr_config.val;
    ref types.Func ft = ref _addr_ft.val;

    setup();
    assignState s = new assignState(stackOffset:config.offsetForLocals,rTotal:config.regAmounts,);
    ptr<ABIParamResultInfo> result = addr(new ABIParamResultInfo(config:config));
    result.preAllocateParams(ft.Receiver != null, ft.Params.NumFields(), ft.Results.NumFields()); 

    // Receiver
    // TODO(register args) ? seems like "struct" and "fields" is not right anymore for describing function parameters
    if (ft.Receiver != null && ft.Receiver.NumFields() != 0) {
        var r = ft.Receiver.FieldSlice()[0];
        result.inparams = append(result.inparams, s.assignParamOrReturn(r.Type, r.Nname, false));
    }
    var ifsl = ft.Params.FieldSlice();
    {
        var f__prev1 = f;

        foreach (var (_, __f) in ifsl) {
            f = __f;
            result.inparams = append(result.inparams, s.assignParamOrReturn(f.Type, f.Nname, false));
        }
        f = f__prev1;
    }

    s.stackOffset = types.Rnd(s.stackOffset, int64(types.RegSize));
    result.inRegistersUsed = s.rUsed.intRegs + s.rUsed.floatRegs; 

    // Outputs
    s.rUsed = new RegAmounts();
    var ofsl = ft.Results.FieldSlice();
    {
        var f__prev1 = f;

        foreach (var (_, __f) in ofsl) {
            f = __f;
            result.outparams = append(result.outparams, s.assignParamOrReturn(f.Type, f.Nname, true));
        }
        f = f__prev1;
    }

    result.offsetToSpillArea = alignTo(s.stackOffset, types.RegSize);
    result.spillAreaSize = alignTo(s.spillOffset, types.RegSize);
    result.outRegistersUsed = s.rUsed.intRegs + s.rUsed.floatRegs;
    return _addr_result!;
}

// ABIAnalyze returns the same result as ABIAnalyzeFuncType, but also
// updates the offsets of all the receiver, input, and output fields.
// If setNname is true, it also sets the FrameOffset of the Nname for
// the field(s); this is for use when compiling a function and figuring out
// spill locations.  Doing this for callers can cause races for register
// outputs because their frame location transitions from BOGUS_FUNARG_OFFSET
// to zero to an as-if-AUTO offset that has no use for callers.
private static ptr<ABIParamResultInfo> ABIAnalyze(this ptr<ABIConfig> _addr_config, ptr<types.Type> _addr_t, bool setNname) {
    ref ABIConfig config = ref _addr_config.val;
    ref types.Type t = ref _addr_t.val;

    var ft = t.FuncType();
    var result = config.ABIAnalyzeFuncType(ft); 

    // Fill in the frame offsets for receiver, inputs, results
    nint k = 0;
    if (t.NumRecvs() != 0) {
        config.updateOffset(result, ft.Receiver.FieldSlice()[0], result.inparams[0], false, setNname);
        k++;
    }
    {
        var i__prev1 = i;
        var f__prev1 = f;

        foreach (var (__i, __f) in ft.Params.FieldSlice()) {
            i = __i;
            f = __f;
            config.updateOffset(result, f, result.inparams[k + i], false, setNname);
        }
        i = i__prev1;
        f = f__prev1;
    }

    {
        var i__prev1 = i;
        var f__prev1 = f;

        foreach (var (__i, __f) in ft.Results.FieldSlice()) {
            i = __i;
            f = __f;
            config.updateOffset(result, f, result.outparams[i], true, setNname);
        }
        i = i__prev1;
        f = f__prev1;
    }

    return _addr_result!;
}

private static void updateOffset(this ptr<ABIConfig> _addr_config, ptr<ABIParamResultInfo> _addr_result, ptr<types.Field> _addr_f, ABIParamAssignment a, bool isReturn, bool setNname) {
    ref ABIConfig config = ref _addr_config.val;
    ref ABIParamResultInfo result = ref _addr_result.val;
    ref types.Field f = ref _addr_f.val;
 
    // Everything except return values in registers has either a frame home (if not in a register) or a frame spill location.
    if (!isReturn || len(a.Registers) == 0) { 
        // The type frame offset DOES NOT show effects of minimum frame size.
        // Getting this wrong breaks stackmaps, see liveness/plive.go:WriteFuncMap and typebits/typebits.go:Set
        var off = a.FrameOffset(result);
        var fOffset = f.Offset;
        if (fOffset == types.BOGUS_FUNARG_OFFSET) {
            if (setNname && f.Nname != null) {
                f.Nname._<ptr<ir.Name>>().SetFrameOffset(off);
                f.Nname._<ptr<ir.Name>>().SetIsOutputParamInRegisters(false);
            }
        }
        else
 {
            @base.Fatalf("field offset for %s at %s has been set to %d", f.Sym.Name, @base.FmtPos(f.Pos), fOffset);
        }
    }
    else
 {
        if (setNname && f.Nname != null) {
            ptr<ir.Name> fname = f.Nname._<ptr<ir.Name>>();
            fname.SetIsOutputParamInRegisters(true);
            fname.SetFrameOffset(0);
        }
    }
}

//......................................................................
//
// Non-public portions.

// regString produces a human-readable version of a RegIndex.
private static @string regString(this ptr<RegAmounts> _addr_c, RegIndex r) {
    ref RegAmounts c = ref _addr_c.val;

    if (int(r) < c.intRegs) {
        return fmt.Sprintf("I%d", int(r));
    }
    else if (int(r) < c.intRegs + c.floatRegs) {
        return fmt.Sprintf("F%d", int(r) - c.intRegs);
    }
    return fmt.Sprintf("<?>%d", r);
}

// ToString method renders an ABIParamAssignment in human-readable
// form, suitable for debugging or unit testing.
private static @string ToString(this ptr<ABIParamAssignment> _addr_ri, ptr<ABIConfig> _addr_config, bool extra) {
    ref ABIParamAssignment ri = ref _addr_ri.val;
    ref ABIConfig config = ref _addr_config.val;

    @string regs = "R{";
    @string offname = "spilloffset"; // offset is for spill for register(s)
    if (len(ri.Registers) == 0) {
        offname = "offset"; // offset is for memory arg
    }
    foreach (var (_, r) in ri.Registers) {
        regs += " " + config.regAmounts.regString(r);
        if (extra) {
            regs += fmt.Sprintf("(%d)", r);
        }
    }    if (extra) {
        regs += fmt.Sprintf(" | #I=%d, #F=%d", config.regAmounts.intRegs, config.regAmounts.floatRegs);
    }
    return fmt.Sprintf("%s } %s: %d typ: %v", regs, offname, ri.offset, ri.Type);
}

// String method renders an ABIParamResultInfo in human-readable
// form, suitable for debugging or unit testing.
private static @string String(this ptr<ABIParamResultInfo> _addr_ri) {
    ref ABIParamResultInfo ri = ref _addr_ri.val;

    @string res = "";
    {
        var k__prev1 = k;

        foreach (var (__k, __p) in ri.inparams) {
            k = __k;
            p = __p;
            res += fmt.Sprintf("IN %d: %s\n", k, p.ToString(ri.config, false));
        }
        k = k__prev1;
    }

    {
        var k__prev1 = k;

        foreach (var (__k, __r) in ri.outparams) {
            k = __k;
            r = __r;
            res += fmt.Sprintf("OUT %d: %s\n", k, r.ToString(ri.config, false));
        }
        k = k__prev1;
    }

    res += fmt.Sprintf("offsetToSpillArea: %d spillAreaSize: %d", ri.offsetToSpillArea, ri.spillAreaSize);
    return res;
}

// assignState holds intermediate state during the register assigning process
// for a given function signature.
private partial struct assignState {
    public RegAmounts rTotal; // total reg amounts from ABI rules
    public RegAmounts rUsed; // regs used by params completely assigned so far
    public RegAmounts pUsed; // regs used by the current param (or pieces therein)
    public long stackOffset; // current stack offset
    public long spillOffset; // current spill offset
}

// align returns a rounded up to t's alignment
private static long align(long a, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return alignTo(a, int(t.Align));
}

// alignTo returns a rounded up to t, where t must be 0 or a power of 2.
private static long alignTo(long a, nint t) {
    if (t == 0) {
        return a;
    }
    return types.Rnd(a, int64(t));
}

// stackSlot returns a stack offset for a param or result of the
// specified type.
private static long stackSlot(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    var rv = align(state.stackOffset, _addr_t);
    state.stackOffset = rv + t.Width;
    return rv;
}

// allocateRegs returns an ordered list of register indices for a parameter or result
// that we've just determined to be register-assignable. The number of registers
// needed is assumed to be stored in state.pUsed.
private static slice<RegIndex> allocateRegs(this ptr<assignState> _addr_state, slice<RegIndex> regs, ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    if (t.Width == 0) {
        return regs;
    }
    var ri = state.rUsed.intRegs;
    var rf = state.rUsed.floatRegs;
    if (t.IsScalar() || t.IsPtrShaped()) {
        if (t.IsComplex()) {
            regs = append(regs, RegIndex(rf + state.rTotal.intRegs), RegIndex(rf + 1 + state.rTotal.intRegs));
            rf += 2;
        }
        else if (t.IsFloat()) {
            regs = append(regs, RegIndex(rf + state.rTotal.intRegs));
            rf += 1;
        }
        else
 {
            var n = (int(t.Size()) + types.RegSize - 1) / types.RegSize;
            {
                nint i__prev1 = i;

                for (nint i = 0; i < n; i++) { // looking ahead to really big integers
                    regs = append(regs, RegIndex(ri));
                    ri += 1;
                }


                i = i__prev1;
            }
        }
    else
        state.rUsed.intRegs = ri;
        state.rUsed.floatRegs = rf;
        return regs;
    } {
        var typ = t.Kind();

        if (typ == types.TARRAY) 
            {
                nint i__prev1 = i;

                for (i = int64(0); i < t.NumElem(); i++) {
                    regs = state.allocateRegs(regs, t.Elem());
                }


                i = i__prev1;
            }
            return regs;
        else if (typ == types.TSTRUCT) 
            foreach (var (_, f) in t.FieldSlice()) {
                regs = state.allocateRegs(regs, f.Type);
            }
            return regs;
        else if (typ == types.TSLICE) 
            return state.allocateRegs(regs, synthSlice);
        else if (typ == types.TSTRING) 
            return state.allocateRegs(regs, synthString);
        else if (typ == types.TINTER) 
            return state.allocateRegs(regs, synthIface);
            }
    @base.Fatalf("was not expecting type %s", t);
    panic("unreachable");
});

// regAllocate creates a register ABIParamAssignment object for a param
// or result with the specified type, as a final step (this assumes
// that all of the safety/suitability analysis is complete).
private static ABIParamAssignment regAllocate(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t, types.Object name, bool isReturn) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    var spillLoc = int64(-1);
    if (!isReturn) { 
        // Spill for register-resident t must be aligned for storage of a t.
        spillLoc = align(state.spillOffset, _addr_t);
        state.spillOffset = spillLoc + t.Size();
    }
    return new ABIParamAssignment(Type:t,Name:name,Registers:state.allocateRegs([]RegIndex{},t),offset:int32(spillLoc),);
}

// stackAllocate creates a stack memory ABIParamAssignment object for
// a param or result with the specified type, as a final step (this
// assumes that all of the safety/suitability analysis is complete).
private static ABIParamAssignment stackAllocate(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t, types.Object name) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    return new ABIParamAssignment(Type:t,Name:name,offset:int32(state.stackSlot(t)),);
}

// intUsed returns the number of integer registers consumed
// at a given point within an assignment stage.
private static nint intUsed(this ptr<assignState> _addr_state) {
    ref assignState state = ref _addr_state.val;

    return state.rUsed.intRegs + state.pUsed.intRegs;
}

// floatUsed returns the number of floating point registers consumed at
// a given point within an assignment stage.
private static nint floatUsed(this ptr<assignState> _addr_state) {
    ref assignState state = ref _addr_state.val;

    return state.rUsed.floatRegs + state.pUsed.floatRegs;
}

// regassignIntegral examines a param/result of integral type 't' to
// determines whether it can be register-assigned. Returns TRUE if we
// can register allocate, FALSE otherwise (and updates state
// accordingly).
private static bool regassignIntegral(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    var regsNeeded = int(types.Rnd(t.Width, int64(types.PtrSize)) / int64(types.PtrSize));
    if (t.IsComplex()) {
        regsNeeded = 2;
    }
    if (t.IsFloat() || t.IsComplex()) {
        if (regsNeeded + state.floatUsed() > state.rTotal.floatRegs) { 
            // not enough regs
            return false;
        }
        state.pUsed.floatRegs += regsNeeded;
        return true;
    }
    if (regsNeeded + state.intUsed() > state.rTotal.intRegs) { 
        // not enough regs
        return false;
    }
    state.pUsed.intRegs += regsNeeded;
    return true;
}

// regassignArray processes an array type (or array component within some
// other enclosing type) to determine if it can be register assigned.
// Returns TRUE if we can register allocate, FALSE otherwise.
private static bool regassignArray(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    var nel = t.NumElem();
    if (nel == 0) {
        return true;
    }
    if (nel > 1) { 
        // Not an array of length 1: stack assign
        return false;
    }
    return state.regassign(t.Elem());
}

// regassignStruct processes a struct type (or struct component within
// some other enclosing type) to determine if it can be register
// assigned. Returns TRUE if we can register allocate, FALSE otherwise.
private static bool regassignStruct(this ptr<assignState> _addr_state, ptr<types.Type> _addr_t) {
    ref assignState state = ref _addr_state.val;
    ref types.Type t = ref _addr_t.val;

    foreach (var (_, field) in t.FieldSlice()) {
        if (!state.regassign(field.Type)) {
            return false;
        }
    }    return true;
}

// synthOnce ensures that we only create the synth* fake types once.
private static sync.Once synthOnce = default;

// synthSlice, synthString, and syncIface are synthesized struct types
// meant to capture the underlying implementations of string/slice/interface.
private static ptr<types.Type> synthSlice;
private static ptr<types.Type> synthString;
private static ptr<types.Type> synthIface;

// setup performs setup for the register assignment utilities, manufacturing
// a small set of synthesized types that we'll need along the way.
private static void setup() {
    synthOnce.Do(() => {
        var fname = types.BuiltinPkg.Lookup;
        var nxp = src.NoXPos;
        var unsp = types.Types[types.TUNSAFEPTR];
        var ui = types.Types[types.TUINTPTR];
        synthSlice = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(nxp,fname("ptr"),unsp), types.NewField(nxp,fname("len"),ui), types.NewField(nxp,fname("cap"),ui) }));
        synthString = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(nxp,fname("data"),unsp), types.NewField(nxp,fname("len"),ui) }));
        synthIface = types.NewStruct(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(nxp,fname("f1"),unsp), types.NewField(nxp,fname("f2"),unsp) }));
    });
}

// regassign examines a given param type (or component within some
// composite) to determine if it can be register assigned.  Returns
// TRUE if we can register allocate, FALSE otherwise.
private static bool regassign(this ptr<assignState> _addr_state, ptr<types.Type> _addr_pt) => func((_, panic, _) => {
    ref assignState state = ref _addr_state.val;
    ref types.Type pt = ref _addr_pt.val;

    var typ = pt.Kind();
    if (pt.IsScalar() || pt.IsPtrShaped()) {
        return state.regassignIntegral(pt);
    }

    if (typ == types.TARRAY) 
        return state.regassignArray(pt);
    else if (typ == types.TSTRUCT) 
        return state.regassignStruct(pt);
    else if (typ == types.TSLICE) 
        return state.regassignStruct(synthSlice);
    else if (typ == types.TSTRING) 
        return state.regassignStruct(synthString);
    else if (typ == types.TINTER) 
        return state.regassignStruct(synthIface);
    else 
        @base.Fatalf("not expected");
        panic("unreachable");
    });

// assignParamOrReturn processes a given receiver, param, or result
// of field f to determine whether it can be register assigned.
// The result of the analysis is recorded in the result
// ABIParamResultInfo held in 'state'.
private static ABIParamAssignment assignParamOrReturn(this ptr<assignState> _addr_state, ptr<types.Type> _addr_pt, types.Object n, bool isReturn) => func((_, panic, _) => {
    ref assignState state = ref _addr_state.val;
    ref types.Type pt = ref _addr_pt.val;

    state.pUsed = new RegAmounts();
    if (pt.Width == types.BADWIDTH) {
        @base.Fatalf("should never happen");
        panic("unreachable");
    }
    else if (pt.Width == 0) {
        return state.stackAllocate(pt, n);
    }
    else if (state.regassign(pt)) {
        return state.regAllocate(pt, n, isReturn);
    }
    else
 {
        return state.stackAllocate(pt, n);
    }
});

// ComputePadding returns a list of "post element" padding values in
// the case where we have a structure being passed in registers. Give
// a param assignment corresponding to a struct, it returns a list of
// contaning padding values for each field, e.g. the Kth element in
// the list is the amount of padding between field K and the following
// field. For things that are not struct (or structs without padding)
// it returns a list of zeros. Example:
//
// type small struct {
//   x uint16
//   y uint8
//   z int32
//   w int32
// }
//
// For this struct we would return a list [0, 1, 0, 0], meaning that
// we have one byte of padding after the second field, and no bytes of
// padding after any of the other fields. Input parameter "storage"
// is with enough capacity to accommodate padding elements for
// the architected register set in question.
private static slice<ulong> ComputePadding(this ptr<ABIParamAssignment> _addr_pa, slice<ulong> storage) => func((_, panic, _) => {
    ref ABIParamAssignment pa = ref _addr_pa.val;

    var nr = len(pa.Registers);
    var padding = storage[..(int)nr];
    for (nint i = 0; i < nr; i++) {
        padding[i] = 0;
    }
    if (pa.Type.Kind() != types.TSTRUCT || nr == 0) {
        return padding;
    }
    var types = make_slice<ptr<types.Type>>(0, nr);
    types = appendParamTypes(types, _addr_pa.Type);
    if (len(types) != nr) {
        panic("internal error");
    }
    var off = int64(0);
    foreach (var (idx, t) in types) {
        var ts = t.Size();
        off += int64(ts);
        if (idx < len(types) - 1) {
            var noff = align(off, _addr_types[idx + 1]);
            if (noff != off) {
                padding[idx] = uint64(noff - off);
            }
        }
    }    return padding;
});

} // end abi_package
