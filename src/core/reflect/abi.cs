// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class reflect_package {

// These variables are used by the register assignment
// algorithm in this file.
//
// They should be modified with care (no other reflect code
// may be executing) and are generally only modified
// when testing this package.
//
// They should never be set higher than their internal/abi
// constant counterparts, because the system relies on a
// structure that is at least large enough to hold the
// registers the system supports.
//
// Currently they're set to zero because using the actual
// constants will break every part of the toolchain that
// uses reflect to call functions (e.g. go test, or anything
// that uses text/template). The values that are currently
// commented out there should be the actual values once
// we're ready to use the register ABI everywhere.
internal static nint intArgRegs = abi.IntArgRegs;

internal static nint floatArgRegs = abi.FloatArgRegs;

internal static uintptr floatRegSize = ((uintptr)abi.EffectiveFloatRegSize);

// abiStep represents an ABI "instruction." Each instruction
// describes one part of how to translate between a Go value
// in memory and a call frame.
[GoType] partial struct abiStep {
    internal abiStepKind kind;
    // offset and size together describe a part of a Go value
    // in memory.
    internal uintptr offset;
    internal uintptr size; // size in bytes of the part
    // These fields describe the ABI side of the translation.
    internal uintptr stkOff; // stack offset, used if kind == abiStepStack
    internal nint ireg;    // integer register index, used if kind == abiStepIntReg or kind == abiStepPointer
    internal nint freg;    // FP register index, used if kind == abiStepFloatReg
}

[GoType("num:nint")] partial struct abiStepKind;

internal static readonly abiStepKind abiStepBad = /* iota */ 0;
internal static readonly abiStepKind abiStepStack = 1; // copy to/from stack
internal static readonly abiStepKind abiStepIntReg = 2; // copy to/from integer register
internal static readonly abiStepKind abiStepPointer = 3; // copy pointer to/from integer register
internal static readonly abiStepKind abiStepFloatReg = 4; // copy to/from FP register

// abiSeq represents a sequence of ABI instructions for copying
// from a series of reflect.Values to a call frame (for call arguments)
// or vice-versa (for call results).
//
// An abiSeq should be populated by calling its addArg method.
[GoType] partial struct abiSeq {
    // steps is the set of instructions.
    //
    // The instructions are grouped together by whole arguments,
    // with the starting index for the instructions
    // of the i'th Go value available in valueStart.
    //
    // For instance, if this abiSeq represents 3 arguments
    // passed to a function, then the 2nd argument's steps
    // begin at steps[valueStart[1]].
    //
    // Because reflect accepts Go arguments in distinct
    // Values and each Value is stored separately, each abiStep
    // that begins a new argument will have its offset
    // field == 0.
    internal slice<abiStep> steps;
    internal slice<nint> valueStart;
    internal uintptr stackBytes; // stack space used
    internal nint iregs;    // registers used
    internal nint fregs;
}

[GoRecv] internal static void dump(this ref abiSeq a) {
    foreach (var (i, p) in a.steps) {
        println("part", i, p.kind, p.offset, p.size, p.stkOff, p.ireg, p.freg);
    }
    print("values ");
    foreach (var (_, i) in a.valueStart) {
        print(i, " ");
    }
    println();
    println("stack", a.stackBytes);
    println("iregs", a.iregs);
    println("fregs", a.fregs);
}

// stepsForValue returns the ABI instructions for translating
// the i'th Go argument or return value represented by this
// abiSeq to the Go ABI.
[GoRecv] internal static slice<abiStep> stepsForValue(this ref abiSeq a, nint i) {
    nint s = a.valueStart[i];
    nint e = default!;
    if (i == len(a.valueStart) - 1){
        e = len(a.steps);
    } else {
        e = a.valueStart[i + 1];
    }
    return a.steps[(int)(s)..(int)(e)];
}

// addArg extends the abiSeq with a new Go value of type t.
//
// If the value was stack-assigned, returns the single
// abiStep describing that translation, and nil otherwise.
[GoRecv] internal static ж<abiStep> addArg(this ref abiSeq a, ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.val;

    // We'll always be adding a new value, so do that first.
    nint pStart = len(a.steps);
    a.valueStart = append(a.valueStart, pStart);
    if (t.Size() == 0) {
        // If the size of the argument type is zero, then
        // in order to degrade gracefully into ABI0, we need
        // to stack-assign this type. The reason is that
        // although zero-sized types take up no space on the
        // stack, they do cause the next argument to be aligned.
        // So just do that here, but don't bother actually
        // generating a new ABI step for it (there's nothing to
        // actually copy).
        //
        // We cannot handle this in the recursive case of
        // regAssign because zero-sized *fields* of a
        // non-zero-sized struct do not cause it to be
        // stack-assigned. So we need a special case here
        // at the top.
        a.stackBytes = align(a.stackBytes, ((uintptr)t.Align()));
        return default!;
    }
    // Hold a copy of "a" so that we can roll back if
    // register assignment fails.
    var aOld = a;
    if (!a.regAssign(Ꮡt, 0)) {
        // Register assignment failed. Roll back any changes
        // and stack-assign.
        a = aOld;
        a.stackAssign(t.Size(), ((uintptr)t.Align()));
        return Ꮡ(a.steps[len(a.steps) - 1]);
    }
    return default!;
}

// addRcvr extends the abiSeq with a new method call
// receiver according to the interface calling convention.
//
// If the receiver was stack-assigned, returns the single
// abiStep describing that translation, and nil otherwise.
// Returns true if the receiver is a pointer.
[GoRecv] internal static (ж<abiStep>, bool) addRcvr(this ref abiSeq a, ж<abi.Type> Ꮡrcvr) {
    ref var rcvr = ref Ꮡrcvr.val;

    // The receiver is always one word.
    a.valueStart = append(a.valueStart, len(a.steps));
    bool ok = default!;
    bool ptr = default!;
    if (rcvr.IfaceIndir() || rcvr.Pointers()){
        ok = a.assignIntN(0, goarch.PtrSize, 1, 1);
        ptr = true;
    } else {
        // TODO(mknyszek): Is this case even possible?
        // The interface data work never contains a non-pointer
        // value. This case was copied over from older code
        // in the reflect package which only conditionally added
        // a pointer bit to the reflect.(Value).Call stack frame's
        // GC bitmap.
        ok = a.assignIntN(0, goarch.PtrSize, 1, 0);
        ptr = false;
    }
    if (!ok) {
        a.stackAssign(goarch.PtrSize, goarch.PtrSize);
        return (Ꮡ(a.steps[len(a.steps) - 1]), ptr);
    }
    return (default!, ptr);
}

// regAssign attempts to reserve argument registers for a value of
// type t, stored at some offset.
//
// It returns whether or not the assignment succeeded, but
// leaves any changes it made to a.steps behind, so the caller
// must undo that work by adjusting a.steps if it fails.
//
// This method along with the assign* methods represent the
// complete register-assignment algorithm for the Go ABI.
[GoRecv] internal static bool regAssign(this ref abiSeq a, ж<abi.Type> Ꮡt, uintptr offset) {
    ref var t = ref Ꮡt.val;

    var exprᴛ1 = ((ΔKind)t.Kind());
    if (exprᴛ1 == ΔUnsafePointer || exprᴛ1 == ΔPointer || exprᴛ1 == Chan || exprᴛ1 == Map || exprᴛ1 == Func) {
        return a.assignIntN(offset, t.Size(), 1, 1);
    }
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == ΔUint || exprᴛ1 == Int8 || exprᴛ1 == Uint8 || exprᴛ1 == Int16 || exprᴛ1 == Uint16 || exprᴛ1 == Int32 || exprᴛ1 == Uint32 || exprᴛ1 == Uintptr) {
        return a.assignIntN(offset, t.Size(), 1, 0);
    }
    if (exprᴛ1 == Int64 || exprᴛ1 == Uint64) {
        switch (goarch.PtrSize) {
        case 4: {
            return a.assignIntN(offset, 4, 2, 0);
        }
        case 8: {
            return a.assignIntN(offset, 8, 1, 0);
        }}

    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return a.assignFloatN(offset, t.Size(), 1);
    }
    if (exprᴛ1 == Complex64) {
        return a.assignFloatN(offset, 4, 2);
    }
    if (exprᴛ1 == Complex128) {
        return a.assignFloatN(offset, 8, 2);
    }
    if (exprᴛ1 == ΔString) {
        return a.assignIntN(offset, goarch.PtrSize, 2, 1);
    }
    if (exprᴛ1 == ΔInterface) {
        return a.assignIntN(offset, goarch.PtrSize, 2, 2);
    }
    if (exprᴛ1 == ΔSlice) {
        return a.assignIntN(offset, goarch.PtrSize, 3, 1);
    }
    if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        switch ((~tt).Len) {
        case 0: {
            return true;
        }
        case 1: {
            return a.regAssign((~tt).Elem, // There's nothing to assign, so don't modify
 // a.steps but succeed so the caller doesn't
 // try to stack-assign this value.
 offset);
        }
        default: {
            return false;
        }}

    }
    if (exprᴛ1 == Struct) {
        var st = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (i, _) in st.Fields) {
            var f = Ꮡ(st.Fields, i);
            if (!a.regAssign((~f).Typ, offset + (~f).Offset)) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        print("t.Kind == ", t.Kind(), "\n");
        throw panic("unknown type kind");
    }

    throw panic("unhandled register assignment path");
}

// assignIntN assigns n values to registers, each "size" bytes large,
// from the data at [offset, offset+n*size) in memory. Each value at
// [offset+i*size, offset+(i+1)*size) for i < n is assigned to the
// next n integer registers.
//
// Bit i in ptrMap indicates whether the i'th value is a pointer.
// n must be <= 8.
//
// Returns whether assignment succeeded.
[GoRecv] internal static bool assignIntN(this ref abiSeq a, uintptr offset, uintptr size, nint n, uint8 ptrMap) {
    if (n > 8 || n < 0) {
        throw panic("invalid n");
    }
    if (ptrMap != 0 && size != goarch.PtrSize) {
        throw panic("non-empty pointer map passed for non-pointer-size values");
    }
    if (a.iregs + n > intArgRegs) {
        return false;
    }
    for (nint i = 0; i < n; i++) {
        abiStepKind kind = abiStepIntReg;
        if ((uint8)(ptrMap & (((uint8)1) << (int)(i))) != 0) {
            kind = abiStepPointer;
        }
        a.steps = append(a.steps, new abiStep(
            kind: kind,
            offset: offset + ((uintptr)i) * size,
            size: size,
            ireg: a.iregs
        ));
        a.iregs++;
    }
    return true;
}

// assignFloatN assigns n values to registers, each "size" bytes large,
// from the data at [offset, offset+n*size) in memory. Each value at
// [offset+i*size, offset+(i+1)*size) for i < n is assigned to the
// next n floating-point registers.
//
// Returns whether assignment succeeded.
[GoRecv] internal static bool assignFloatN(this ref abiSeq a, uintptr offset, uintptr size, nint n) {
    if (n < 0) {
        throw panic("invalid n");
    }
    if (a.fregs + n > floatArgRegs || floatRegSize < size) {
        return false;
    }
    for (nint i = 0; i < n; i++) {
        a.steps = append(a.steps, new abiStep(
            kind: abiStepFloatReg,
            offset: offset + ((uintptr)i) * size,
            size: size,
            freg: a.fregs
        ));
        a.fregs++;
    }
    return true;
}

// stackAssign reserves space for one value that is "size" bytes
// large with alignment "alignment" to the stack.
//
// Should not be called directly; use addArg instead.
[GoRecv] internal static void stackAssign(this ref abiSeq a, uintptr size, uintptr alignment) {
    a.stackBytes = align(a.stackBytes, alignment);
    a.steps = append(a.steps, new abiStep(
        kind: abiStepStack,
        offset: 0, // Only used for whole arguments, so the memory offset is 0.

        size: size,
        stkOff: a.stackBytes
    ));
    a.stackBytes += size;
}

// abiDesc describes the ABI for a function or method.
[GoType] partial struct abiDesc {
    // call and ret represent the translation steps for
    // the call and return paths of a Go function.
    internal abiSeq call;
    internal abiSeq ret;
    // These fields describe the stack space allocated
    // for the call. stackCallArgsSize is the amount of space
    // reserved for arguments but not return values. retOffset
    // is the offset at which return values begin, and
    // spill is the size in bytes of additional space reserved
    // to spill argument registers into in case of preemption in
    // reflectcall's stack frame.
    internal uintptr stackCallArgsSize;
    internal uintptr retOffset;
    internal uintptr spill;
    // stackPtrs is a bitmap that indicates whether
    // each word in the ABI stack space (stack-assigned
    // args + return values) is a pointer. Used
    // as the heap pointer bitmap for stack space
    // passed to reflectcall.
    internal ж<bitVector> stackPtrs;
    // inRegPtrs is a bitmap whose i'th bit indicates
    // whether the i'th integer argument register contains
    // a pointer. Used by makeFuncStub and methodValueCall
    // to make result pointers visible to the GC.
    //
    // outRegPtrs is the same, but for result values.
    // Used by reflectcall to make result pointers visible
    // to the GC.
    internal @internal.abi_package.IntArgRegBitmap inRegPtrs;
    internal @internal.abi_package.IntArgRegBitmap outRegPtrs;
}

[GoRecv] internal static void dump(this ref abiDesc a) {
    println("ABI");
    println("call");
    a.call.dump();
    println("ret");
    a.ret.dump();
    println("stackCallArgsSize", a.stackCallArgsSize);
    println("retOffset", a.retOffset);
    println("spill", a.spill);
    print("inRegPtrs:");
    dumpPtrBitMap(a.inRegPtrs);
    println();
    print("outRegPtrs:");
    dumpPtrBitMap(a.outRegPtrs);
    println();
}

internal static void dumpPtrBitMap(abi.IntArgRegBitmap b) {
    for (nint i = 0; i < intArgRegs; i++) {
        nint x = 0;
        if (b.Get(i)) {
            x = 1;
        }
        print(" ", x);
    }
}

internal static abiDesc newAbiDesc(ж<funcType> Ꮡt, ж<abi.Type> Ꮡrcvr) {
    ref var t = ref Ꮡt.val;
    ref var rcvr = ref Ꮡrcvr.val;

    // We need to add space for this argument to
    // the frame so that it can spill args into it.
    //
    // The size of this space is just the sum of the sizes
    // of each register-allocated type.
    //
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    var spill = ((uintptr)0);
    // Compute gc program & stack bitmap for stack arguments
    var stackPtrs = @new<bitVector>();
    // Compute the stack frame pointer bitmap and register
    // pointer bitmap for arguments.
    var inRegPtrs = new abi.IntArgRegBitmap{nil};
    // Compute abiSeq for input parameters.
    abiSeq @in = default!;
    if (rcvr != nil) {
        var (stkStep, isPtr) = @in.addRcvr(Ꮡrcvr);
        if (stkStep != nil){
            if (isPtr){
                stackPtrs.append(1);
            } else {
                stackPtrs.append(0);
            }
        } else {
            spill += goarch.PtrSize;
        }
    }
    foreach (var (i, arg) in t.InSlice()) {
        var stkStep = @in.addArg(arg);
        if (stkStep != nil){
            addTypeBits(stackPtrs, (~stkStep).stkOff, arg);
        } else {
            spill = align(spill, ((uintptr)arg.Align()));
            spill += arg.Size();
            foreach (var (_, st) in @in.stepsForValue(i)) {
                if (st.kind == abiStepPointer) {
                    inRegPtrs.Set(st.ireg);
                }
            }
        }
    }
    spill = align(spill, goarch.PtrSize);
    // From the input parameters alone, we now know
    // the stackCallArgsSize and retOffset.
    var stackCallArgsSize = @in.stackBytes;
    var retOffset = align(@in.stackBytes, goarch.PtrSize);
    // Compute the stack frame pointer bitmap and register
    // pointer bitmap for return values.
    var outRegPtrs = new abi.IntArgRegBitmap{nil};
    // Compute abiSeq for output parameters.
    abiSeq @out = default!;
    // Stack-assigned return values do not share
    // space with arguments like they do with registers,
    // so we need to inject a stack offset here.
    // Fake it by artificially extending stackBytes by
    // the return offset.
    @out.stackBytes = retOffset;
    foreach (var (i, res) in t.OutSlice()) {
        var stkStep = @out.addArg(res);
        if (stkStep != nil){
            addTypeBits(stackPtrs, (~stkStep).stkOff, res);
        } else {
            foreach (var (_, st) in @out.stepsForValue(i)) {
                if (st.kind == abiStepPointer) {
                    outRegPtrs.Set(st.ireg);
                }
            }
        }
    }
    // Undo the faking from earlier so that stackBytes
    // is accurate.
    @out.stackBytes -= retOffset;
    return new abiDesc(@in, @out, stackCallArgsSize, retOffset, spill, stackPtrs, inRegPtrs, outRegPtrs);
}

// intFromReg loads an argSize sized integer from reg and places it at to.
//
// argSize must be non-zero, fit in a register, and a power-of-two.
internal static void intFromReg(ж<abi.RegArgs> Ꮡr, nint reg, uintptr argSize, @unsafe.Pointer to) {
    ref var r = ref Ꮡr.val;

    memmove(to.val, (uintptr)r.IntRegArgAddr(reg, argSize), argSize);
}

// intToReg loads an argSize sized integer and stores it into reg.
//
// argSize must be non-zero, fit in a register, and a power-of-two.
internal static void intToReg(ж<abi.RegArgs> Ꮡr, nint reg, uintptr argSize, @unsafe.Pointer from) {
    ref var r = ref Ꮡr.val;

    memmove((uintptr)r.IntRegArgAddr(reg, argSize), from.val, argSize);
}

// floatFromReg loads a float value from its register representation in r.
//
// argSize must be 4 or 8.
internal static void floatFromReg(ж<abi.RegArgs> Ꮡr, nint reg, uintptr argSize, @unsafe.Pointer to) {
    ref var r = ref Ꮡr.val;

    switch (argSize) {
    case 4: {
        ((ж<float32>)(uintptr)(to)).val = archFloat32FromReg(r.Floats[reg]);
        break;
    }
    case 8: {
        ((ж<float64>)(uintptr)(to)).val = ((ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡr.Floats.at<uint64>(reg)))).val;
        break;
    }
    default: {
        throw panic("bad argSize");
        break;
    }}

}

// floatToReg stores a float value in its register representation in r.
//
// argSize must be either 4 or 8.
internal static void floatToReg(ж<abi.RegArgs> Ꮡr, nint reg, uintptr argSize, @unsafe.Pointer from) {
    ref var r = ref Ꮡr.val;

    switch (argSize) {
    case 4: {
        r.Floats[reg] = archFloat32ToReg(~(ж<float32>)(uintptr)(from));
        break;
    }
    case 8: {
        r.Floats[reg] = ~(ж<uint64>)(uintptr)(from);
        break;
    }
    default: {
        throw panic("bad argSize");
        break;
    }}

}

} // end reflect_package
