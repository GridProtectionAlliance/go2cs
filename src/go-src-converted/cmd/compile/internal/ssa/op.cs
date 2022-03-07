// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:50:18 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\op.go
using abi = go.cmd.compile.@internal.abi_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using strings = go.strings_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // An Op encodes the specific operation that a Value performs.
    // Opcodes' semantics can be modified by the type and aux fields of the Value.
    // For instance, OpAdd can be 32 or 64 bit, signed or unsigned, float or complex, depending on Value.Type.
    // Semantics of each op are described in the opcode files in gen/*Ops.go.
    // There is one file for generic (architecture-independent) ops and one file
    // for each architecture.
public partial struct Op { // : int
}

private partial struct opInfo {
    public @string name;
    public regInfo reg;
    public auxType auxType;
    public int argLen; // the number of arguments, -1 if variable length
    public obj.As asm;
    public bool generic; // this is a generic (arch-independent) opcode
    public bool rematerializeable; // this op is rematerializeable
    public bool commutative; // this operation is commutative (e.g. addition)
    public bool resultInArg0; // (first, if a tuple) output of v and v.Args[0] must be allocated to the same register
    public bool resultNotInArgs; // outputs must not be allocated to the same registers as inputs
    public bool clobberFlags; // this op clobbers flags register
    public bool call; // is a function call
    public bool nilCheck; // this op is a nil check on arg0
    public bool faultOnNilArg0; // this op will fault if arg0 is nil (and aux encodes a small offset)
    public bool faultOnNilArg1; // this op will fault if arg1 is nil (and aux encodes a small offset)
    public bool usesScratch; // this op requires scratch memory space
    public bool hasSideEffects; // for "reasons", not to be eliminated.  E.g., atomic store, #19182.
    public bool zeroWidth; // op never translates into any machine code. example: copy, which may sometimes translate to machine code, is not zero-width.
    public bool unsafePoint; // this op is an unsafe point, i.e. not safe for async preemption
    public SymEffect symEffect; // effect this op has on symbol in aux
    public byte scale; // amd64/386 indexed load scale
}

private partial struct inputInfo {
    public nint idx; // index in Args array
    public regMask regs; // allowed input registers
}

private partial struct outputInfo {
    public nint idx; // index in output tuple
    public regMask regs; // allowed output registers
}

private partial struct regInfo {
    public slice<inputInfo> inputs; // clobbers encodes the set of registers that are overwritten by
// the instruction (other than the output registers).
    public regMask clobbers; // outputs is the same as inputs, but for the outputs of the instruction.
    public slice<outputInfo> outputs;
}

private static @string String(this ptr<regInfo> _addr_r) {
    ref regInfo r = ref _addr_r.val;

    @string s = "";
    s += "INS:\n";
    {
        var i__prev1 = i;

        foreach (var (_, __i) in r.inputs) {
            i = __i;
            var mask = fmt.Sprintf("%64b", i.regs);
            mask = strings.Replace(mask, "0", ".", -1);
            s += fmt.Sprintf("%2d |%s|\n", i.idx, mask);
        }
        i = i__prev1;
    }

    s += "OUTS:\n";
    {
        var i__prev1 = i;

        foreach (var (_, __i) in r.outputs) {
            i = __i;
            mask = fmt.Sprintf("%64b", i.regs);
            mask = strings.Replace(mask, "0", ".", -1);
            s += fmt.Sprintf("%2d |%s|\n", i.idx, mask);
        }
        i = i__prev1;
    }

    s += "CLOBBERS:\n";
    mask = fmt.Sprintf("%64b", r.clobbers);
    mask = strings.Replace(mask, "0", ".", -1);
    s += fmt.Sprintf("   |%s|\n", mask);
    return s;

}

private partial struct auxType { // : sbyte
}

public partial struct AuxNameOffset {
    public ptr<ir.Name> Name;
    public long Offset;
}

private static void CanBeAnSSAAux(this ptr<AuxNameOffset> _addr_a) {
    ref AuxNameOffset a = ref _addr_a.val;

}
private static @string String(this ptr<AuxNameOffset> _addr_a) {
    ref AuxNameOffset a = ref _addr_a.val;

    return fmt.Sprintf("%s+%d", a.Name.Sym().Name, a.Offset);
}

public partial struct AuxCall {
    public ptr<obj.LSym> Fn;
    public ptr<regInfo> reg; // regInfo for this call
    public ptr<abi.ABIParamResultInfo> abiInfo;
}

// Reg returns the regInfo for a given call, combining the derived in/out register masks
// with the machine-specific register information in the input i.  (The machine-specific
// regInfo is much handier at the call site than it is when the AuxCall is being constructed,
// therefore do this lazily).
//
// TODO: there is a Clever Hack that allows pre-generation of a small-ish number of the slices
// of inputInfo and outputInfo used here, provided that we are willing to reorder the inputs
// and outputs from calls, so that all integer registers come first, then all floating registers.
// At this point (active development of register ABI) that is very premature,
// but if this turns out to be a cost, we could do it.
private static ptr<regInfo> Reg(this ptr<AuxCall> _addr_a, ptr<regInfo> _addr_i, ptr<Config> _addr_c) {
    ref AuxCall a = ref _addr_a.val;
    ref regInfo i = ref _addr_i.val;
    ref Config c = ref _addr_c.val;

    if (a.reg.clobbers != 0) { 
        // Already updated
        return _addr_a.reg!;

    }
    if (a.abiInfo.InRegistersUsed() + a.abiInfo.OutRegistersUsed() == 0) { 
        // Shortcut for zero case, also handles old ABI.
        a.reg = i;
        return _addr_a.reg!;

    }
    var k = len(i.inputs);
    {
        var p__prev1 = p;

        foreach (var (_, __p) in a.abiInfo.InParams()) {
            p = __p;
            {
                var r__prev2 = r;

                foreach (var (_, __r) in p.Registers) {
                    r = __r;
                    var m = archRegForAbiReg(r, _addr_c);
                    a.reg.inputs = append(a.reg.inputs, new inputInfo(idx:k,regs:(1<<m)));
                    k++;
                }

                r = r__prev2;
            }
        }
        p = p__prev1;
    }

    a.reg.inputs = append(a.reg.inputs, i.inputs); // These are less constrained, thus should come last
    k = len(i.outputs);
    {
        var p__prev1 = p;

        foreach (var (_, __p) in a.abiInfo.OutParams()) {
            p = __p;
            {
                var r__prev2 = r;

                foreach (var (_, __r) in p.Registers) {
                    r = __r;
                    m = archRegForAbiReg(r, _addr_c);
                    a.reg.outputs = append(a.reg.outputs, new outputInfo(idx:k,regs:(1<<m)));
                    k++;
                }

                r = r__prev2;
            }
        }
        p = p__prev1;
    }

    a.reg.outputs = append(a.reg.outputs, i.outputs);
    a.reg.clobbers = i.clobbers;
    return _addr_a.reg!;

}
private static ptr<abi.ABIConfig> ABI(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    return _addr_a.abiInfo.Config()!;
}
private static ptr<abi.ABIParamResultInfo> ABIInfo(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    return _addr_a.abiInfo!;
}
private static ptr<regInfo> ResultReg(this ptr<AuxCall> _addr_a, ptr<Config> _addr_c) {
    ref AuxCall a = ref _addr_a.val;
    ref Config c = ref _addr_c.val;

    if (a.abiInfo.OutRegistersUsed() == 0) {
        return _addr_a.reg!;
    }
    if (len(a.reg.inputs) > 0) {
        return _addr_a.reg!;
    }
    nint k = 0;
    foreach (var (_, p) in a.abiInfo.OutParams()) {
        foreach (var (_, r) in p.Registers) {
            var m = archRegForAbiReg(r, _addr_c);
            a.reg.inputs = append(a.reg.inputs, new inputInfo(idx:k,regs:(1<<m)));
            k++;
        }
    }    return _addr_a.reg!;

}

// For ABI register index r, returns the (dense) register number used in
// SSA backend.
private static byte archRegForAbiReg(abi.RegIndex r, ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    sbyte m = default;
    if (int(r) < len(c.intParamRegs)) {
        m = c.intParamRegs[r];
    }
    else
 {
        m = c.floatParamRegs[int(r) - len(c.intParamRegs)];
    }
    return uint8(m);

}

// For ABI register index r, returns the register number used in the obj
// package (assembler).
public static short ObjRegForAbiReg(abi.RegIndex r, ptr<Config> _addr_c) {
    ref Config c = ref _addr_c.val;

    var m = archRegForAbiReg(r, _addr_c);
    return c.registers[m].objNum;
}

// ArgWidth returns the amount of stack needed for all the inputs
// and outputs of a function or method, including ABI-defined parameter
// slots and ABI-defined spill slots for register-resident parameters.
//
// The name is taken from the types package's ArgWidth(<function type>),
// which predated changes to the ABI; this version handles those changes.
private static long ArgWidth(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    return a.abiInfo.ArgWidth();
}

// ParamAssignmentForResult returns the ABI Parameter assignment for result which (indexed 0, 1, etc).
private static ptr<abi.ABIParamAssignment> ParamAssignmentForResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return _addr_a.abiInfo.OutParam(int(which))!;
}

// OffsetOfResult returns the SP offset of result which (indexed 0, 1, etc).
private static long OffsetOfResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    var n = int64(a.abiInfo.OutParam(int(which)).Offset());
    return n;
}

// OffsetOfArg returns the SP offset of argument which (indexed 0, 1, etc).
// If the call is to a method, the receiver is the first argument (i.e., index 0)
private static long OffsetOfArg(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    var n = int64(a.abiInfo.InParam(int(which)).Offset());
    return n;
}

// RegsOfResult returns the register(s) used for result which (indexed 0, 1, etc).
private static slice<abi.RegIndex> RegsOfResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return a.abiInfo.OutParam(int(which)).Registers;
}

// RegsOfArg returns the register(s) used for argument which (indexed 0, 1, etc).
// If the call is to a method, the receiver is the first argument (i.e., index 0)
private static slice<abi.RegIndex> RegsOfArg(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return a.abiInfo.InParam(int(which)).Registers;
}

// NameOfResult returns the type of result which (indexed 0, 1, etc).
private static ptr<ir.Name> NameOfResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    var name = a.abiInfo.OutParam(int(which)).Name;
    if (name == null) {
        return _addr_null!;
    }
    return name._<ptr<ir.Name>>();

}

// TypeOfResult returns the type of result which (indexed 0, 1, etc).
private static ptr<types.Type> TypeOfResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return _addr_a.abiInfo.OutParam(int(which)).Type!;
}

// TypeOfArg returns the type of argument which (indexed 0, 1, etc).
// If the call is to a method, the receiver is the first argument (i.e., index 0)
private static ptr<types.Type> TypeOfArg(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return _addr_a.abiInfo.InParam(int(which)).Type!;
}

// SizeOfResult returns the size of result which (indexed 0, 1, etc).
private static long SizeOfResult(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return a.TypeOfResult(which).Width;
}

// SizeOfArg returns the size of argument which (indexed 0, 1, etc).
// If the call is to a method, the receiver is the first argument (i.e., index 0)
private static long SizeOfArg(this ptr<AuxCall> _addr_a, long which) {
    ref AuxCall a = ref _addr_a.val;

    return a.TypeOfArg(which).Width;
}

// NResults returns the number of results
private static long NResults(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    return int64(len(a.abiInfo.OutParams()));
}

// LateExpansionResultType returns the result type (including trailing mem)
// for a call that will be expanded later in the SSA phase.
private static ptr<types.Type> LateExpansionResultType(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    slice<ptr<types.Type>> tys = default;
    for (var i = int64(0); i < a.NResults(); i++) {
        tys = append(tys, a.TypeOfResult(i));
    }
    tys = append(tys, types.TypeMem);
    return _addr_types.NewResults(tys)!;
}

// NArgs returns the number of arguments (including receiver, if there is one).
private static long NArgs(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    return int64(len(a.abiInfo.InParams()));
}

// String returns "AuxCall{<fn>}"
private static @string String(this ptr<AuxCall> _addr_a) {
    ref AuxCall a = ref _addr_a.val;

    @string fn = default;
    if (a.Fn == null) {
        fn = "AuxCall{nil"; // could be interface/closure etc.
    }
    else
 {
        fn = fmt.Sprintf("AuxCall{%v", a.Fn);
    }
    return fn + "}";

}

// StaticAuxCall returns an AuxCall for a static call.
public static ptr<AuxCall> StaticAuxCall(ptr<obj.LSym> _addr_sym, ptr<abi.ABIParamResultInfo> _addr_paramResultInfo) => func((_, panic, _) => {
    ref obj.LSym sym = ref _addr_sym.val;
    ref abi.ABIParamResultInfo paramResultInfo = ref _addr_paramResultInfo.val;

    if (paramResultInfo == null) {
        panic(fmt.Errorf("Nil paramResultInfo, sym=%v", sym));
    }
    ptr<regInfo> reg;
    if (paramResultInfo.InRegistersUsed() + paramResultInfo.OutRegistersUsed() > 0) {
        reg = addr(new regInfo());
    }
    return addr(new AuxCall(Fn:sym,abiInfo:paramResultInfo,reg:reg));

});

// InterfaceAuxCall returns an AuxCall for an interface call.
public static ptr<AuxCall> InterfaceAuxCall(ptr<abi.ABIParamResultInfo> _addr_paramResultInfo) {
    ref abi.ABIParamResultInfo paramResultInfo = ref _addr_paramResultInfo.val;

    ptr<regInfo> reg;
    if (paramResultInfo.InRegistersUsed() + paramResultInfo.OutRegistersUsed() > 0) {
        reg = addr(new regInfo());
    }
    return addr(new AuxCall(Fn:nil,abiInfo:paramResultInfo,reg:reg));

}

// ClosureAuxCall returns an AuxCall for a closure call.
public static ptr<AuxCall> ClosureAuxCall(ptr<abi.ABIParamResultInfo> _addr_paramResultInfo) {
    ref abi.ABIParamResultInfo paramResultInfo = ref _addr_paramResultInfo.val;

    ptr<regInfo> reg;
    if (paramResultInfo.InRegistersUsed() + paramResultInfo.OutRegistersUsed() > 0) {
        reg = addr(new regInfo());
    }
    return addr(new AuxCall(Fn:nil,abiInfo:paramResultInfo,reg:reg));

}

private static void CanBeAnSSAAux(this ptr<AuxCall> _addr__p0) {
    ref AuxCall _p0 = ref _addr__p0.val;

}

// OwnAuxCall returns a function's own AuxCall
public static ptr<AuxCall> OwnAuxCall(ptr<obj.LSym> _addr_fn, ptr<abi.ABIParamResultInfo> _addr_paramResultInfo) {
    ref obj.LSym fn = ref _addr_fn.val;
    ref abi.ABIParamResultInfo paramResultInfo = ref _addr_paramResultInfo.val;
 
    // TODO if this remains identical to ClosureAuxCall above after new ABI is done, should deduplicate.
    ptr<regInfo> reg;
    if (paramResultInfo.InRegistersUsed() + paramResultInfo.OutRegistersUsed() > 0) {
        reg = addr(new regInfo());
    }
    return addr(new AuxCall(Fn:fn,abiInfo:paramResultInfo,reg:reg));

}

private static readonly auxType auxNone = iota;
private static readonly var auxBool = 0; // auxInt is 0/1 for false/true
private static readonly var auxInt8 = 1; // auxInt is an 8-bit integer
private static readonly var auxInt16 = 2; // auxInt is a 16-bit integer
private static readonly var auxInt32 = 3; // auxInt is a 32-bit integer
private static readonly var auxInt64 = 4; // auxInt is a 64-bit integer
private static readonly var auxInt128 = 5; // auxInt represents a 128-bit integer.  Always 0.
private static readonly var auxUInt8 = 6; // auxInt is an 8-bit unsigned integer
private static readonly var auxFloat32 = 7; // auxInt is a float32 (encoded with math.Float64bits)
private static readonly var auxFloat64 = 8; // auxInt is a float64 (encoded with math.Float64bits)
private static readonly var auxFlagConstant = 9; // auxInt is a flagConstant
private static readonly var auxNameOffsetInt8 = 10; // aux is a &struct{Name ir.Name, Offset int64}; auxInt is index in parameter registers array
private static readonly var auxString = 11; // aux is a string
private static readonly var auxSym = 12; // aux is a symbol (a *gc.Node for locals, an *obj.LSym for globals, or nil for none)
private static readonly var auxSymOff = 13; // aux is a symbol, auxInt is an offset
private static readonly var auxSymValAndOff = 14; // aux is a symbol, auxInt is a ValAndOff
private static readonly var auxTyp = 15; // aux is a type
private static readonly var auxTypSize = 16; // aux is a type, auxInt is a size, must have Aux.(Type).Size() == AuxInt
private static readonly var auxCCop = 17; // aux is a ssa.Op that represents a flags-to-bool conversion (e.g. LessThan)
private static readonly var auxCall = 18; // aux is a *ssa.AuxCall
private static readonly var auxCallOff = 19; // aux is a *ssa.AuxCall, AuxInt is int64 param (in+out) size

// architecture specific aux types
private static readonly var auxARM64BitField = 20; // aux is an arm64 bitfield lsb and width packed into auxInt
private static readonly var auxS390XRotateParams = 21; // aux is a s390x rotate parameters object encoding start bit, end bit and rotate amount
private static readonly var auxS390XCCMask = 22; // aux is a s390x 4-bit condition code mask
private static readonly var auxS390XCCMaskInt8 = 23; // aux is a s390x 4-bit condition code mask, auxInt is a int8 immediate
private static readonly var auxS390XCCMaskUint8 = 24; // aux is a s390x 4-bit condition code mask, auxInt is a uint8 immediate

// A SymEffect describes the effect that an SSA Value has on the variable
// identified by the symbol in its Aux field.
public partial struct SymEffect { // : sbyte
}

public static readonly SymEffect SymRead = 1 << (int)(iota);
public static readonly var SymWrite = 0;
public static readonly SymRdWr SymAddr = SymRead | SymWrite;

public static readonly SymEffect SymNone = 0;


// A Sym represents a symbolic offset from a base register.
// Currently a Sym can be one of 3 things:
//  - a *gc.Node, for an offset from SP (the stack pointer)
//  - a *obj.LSym, for an offset from SB (the global pointer)
//  - nil, for no offset
public partial interface Sym {
    void CanBeAnSSASym();
    void CanBeAnSSAAux();
}

// A ValAndOff is used by the several opcodes. It holds
// both a value and a pointer offset.
// A ValAndOff is intended to be encoded into an AuxInt field.
// The zero ValAndOff encodes a value of 0 and an offset of 0.
// The high 32 bits hold a value.
// The low 32 bits hold a pointer offset.
public partial struct ValAndOff { // : long
}

public static int Val(this ValAndOff x) {
    return int32(int64(x) >> 32);
}
public static long Val64(this ValAndOff x) {
    return int64(x) >> 32;
}
public static short Val16(this ValAndOff x) {
    return int16(int64(x) >> 32);
}
public static sbyte Val8(this ValAndOff x) {
    return int8(int64(x) >> 32);
}

public static long Off64(this ValAndOff x) {
    return int64(int32(x));
}
public static int Off(this ValAndOff x) {
    return int32(x);
}

public static @string String(this ValAndOff x) {
    return fmt.Sprintf("val=%d,off=%d", x.Val(), x.Off());
}

// validVal reports whether the value can be used
// as an argument to makeValAndOff.
private static bool validVal(long val) {
    return val == int64(int32(val));
}

private static ValAndOff makeValAndOff(int val, int off) {
    return ValAndOff(int64(val) << 32 + int64(uint32(off)));
}

public static bool canAdd32(this ValAndOff x, int off) {
    var newoff = x.Off64() + int64(off);
    return newoff == int64(int32(newoff));
}
public static bool canAdd64(this ValAndOff x, long off) {
    var newoff = x.Off64() + off;
    return newoff == int64(int32(newoff));
}

public static ValAndOff addOffset32(this ValAndOff x, int off) => func((_, panic, _) => {
    if (!x.canAdd32(off)) {
        panic("invalid ValAndOff.addOffset32");
    }
    return makeValAndOff(x.Val(), x.Off() + off);

});
public static ValAndOff addOffset64(this ValAndOff x, long off) => func((_, panic, _) => {
    if (!x.canAdd64(off)) {
        panic("invalid ValAndOff.addOffset64");
    }
    return makeValAndOff(x.Val(), x.Off() + int32(off));

});

// int128 is a type that stores a 128-bit constant.
// The only allowed constant right now is 0, so we can cheat quite a bit.
private partial struct int128 { // : long
}

public partial struct BoundsKind { // : byte
}

public static readonly BoundsKind BoundsIndex = iota; // indexing operation, 0 <= idx < len failed
public static readonly var BoundsIndexU = 0; // ... with unsigned idx
public static readonly var BoundsSliceAlen = 1; // 2-arg slicing operation, 0 <= high <= len failed
public static readonly var BoundsSliceAlenU = 2; // ... with unsigned high
public static readonly var BoundsSliceAcap = 3; // 2-arg slicing operation, 0 <= high <= cap failed
public static readonly var BoundsSliceAcapU = 4; // ... with unsigned high
public static readonly var BoundsSliceB = 5; // 2-arg slicing operation, 0 <= low <= high failed
public static readonly var BoundsSliceBU = 6; // ... with unsigned low
public static readonly var BoundsSlice3Alen = 7; // 3-arg slicing operation, 0 <= max <= len failed
public static readonly var BoundsSlice3AlenU = 8; // ... with unsigned max
public static readonly var BoundsSlice3Acap = 9; // 3-arg slicing operation, 0 <= max <= cap failed
public static readonly var BoundsSlice3AcapU = 10; // ... with unsigned max
public static readonly var BoundsSlice3B = 11; // 3-arg slicing operation, 0 <= high <= max failed
public static readonly var BoundsSlice3BU = 12; // ... with unsigned high
public static readonly var BoundsSlice3C = 13; // 3-arg slicing operation, 0 <= low <= high failed
public static readonly var BoundsSlice3CU = 14; // ... with unsigned low
public static readonly var BoundsConvert = 15; // conversion to array pointer failed
public static readonly var BoundsKindCount = 16;


// boundsAPI determines which register arguments a bounds check call should use. For an [a:b:c] slice, we do:
//   CMPQ c, cap
//   JA   fail1
//   CMPQ b, c
//   JA   fail2
//   CMPQ a, b
//   JA   fail3
//
// fail1: CALL panicSlice3Acap (c, cap)
// fail2: CALL panicSlice3B (b, c)
// fail3: CALL panicSlice3C (a, b)
//
// When we register allocate that code, we want the same register to be used for
// the first arg of panicSlice3Acap and the second arg to panicSlice3B. That way,
// initializing that register once will satisfy both calls.
// That desire ends up dividing the set of bounds check calls into 3 sets. This function
// determines which set to use for a given panic call.
// The first arg for set 0 should be the second arg for set 1.
// The first arg for set 1 should be the second arg for set 2.
private static nint boundsABI(long b) => func((_, panic, _) => {

    if (BoundsKind(b) == BoundsSlice3Alen || BoundsKind(b) == BoundsSlice3AlenU || BoundsKind(b) == BoundsSlice3Acap || BoundsKind(b) == BoundsSlice3AcapU || BoundsKind(b) == BoundsConvert) 
        return 0;
    else if (BoundsKind(b) == BoundsSliceAlen || BoundsKind(b) == BoundsSliceAlenU || BoundsKind(b) == BoundsSliceAcap || BoundsKind(b) == BoundsSliceAcapU || BoundsKind(b) == BoundsSlice3B || BoundsKind(b) == BoundsSlice3BU) 
        return 1;
    else if (BoundsKind(b) == BoundsIndex || BoundsKind(b) == BoundsIndexU || BoundsKind(b) == BoundsSliceB || BoundsKind(b) == BoundsSliceBU || BoundsKind(b) == BoundsSlice3C || BoundsKind(b) == BoundsSlice3CU) 
        return 2;
    else 
        panic("bad BoundsKind");
    
});

// arm64BitFileld is the GO type of ARM64BitField auxInt.
// if x is an ARM64BitField, then width=x&0xff, lsb=(x>>8)&0xff, and
// width+lsb<64 for 64-bit variant, width+lsb<32 for 32-bit variant.
// the meaning of width and lsb are instruction-dependent.
private partial struct arm64BitField { // : short
}

} // end ssa_package
