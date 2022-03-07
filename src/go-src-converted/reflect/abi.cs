// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflect -- go2cs converted at 2022 March 06 22:08:02 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Program Files\Go\src\reflect\abi.go
using abi = go.@internal.abi_package;
using goexperiment = go.@internal.goexperiment_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class reflect_package {

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
private static var intArgRegs = abi.IntArgRegs * goexperiment.RegabiArgsInt;private static var floatArgRegs = abi.FloatArgRegs * goexperiment.RegabiArgsInt;private static var floatRegSize = uintptr(abi.EffectiveFloatRegSize * goexperiment.RegabiArgsInt);

// abiStep represents an ABI "instruction." Each instruction
// describes one part of how to translate between a Go value
// in memory and a call frame.
private partial struct abiStep {
    public abiStepKind kind; // offset and size together describe a part of a Go value
// in memory.
    public System.UIntPtr offset;
    public System.UIntPtr size; // size in bytes of the part

// These fields describe the ABI side of the translation.
    public System.UIntPtr stkOff; // stack offset, used if kind == abiStepStack
    public nint ireg; // integer register index, used if kind == abiStepIntReg or kind == abiStepPointer
    public nint freg; // FP register index, used if kind == abiStepFloatReg
}

// abiStepKind is the "op-code" for an abiStep instruction.
private partial struct abiStepKind { // : nint
}

private static readonly abiStepKind abiStepBad = iota;
private static readonly var abiStepStack = 0; // copy to/from stack
private static readonly var abiStepIntReg = 1; // copy to/from integer register
private static readonly var abiStepPointer = 2; // copy pointer to/from integer register
private static readonly var abiStepFloatReg = 3; // copy to/from FP register

// abiSeq represents a sequence of ABI instructions for copying
// from a series of reflect.Values to a call frame (for call arguments)
// or vice-versa (for call results).
//
// An abiSeq should be populated by calling its addArg method.
private partial struct abiSeq {
    public slice<abiStep> steps;
    public slice<nint> valueStart;
    public System.UIntPtr stackBytes; // stack space used
    public nint iregs; // registers used
    public nint fregs; // registers used
}

private static void dump(this ptr<abiSeq> _addr_a) {
    ref abiSeq a = ref _addr_a.val;

    {
        var i__prev1 = i;

        foreach (var (__i, __p) in a.steps) {
            i = __i;
            p = __p;
            println("part", i, p.kind, p.offset, p.size, p.stkOff, p.ireg, p.freg);
        }
        i = i__prev1;
    }

    print("values ");
    {
        var i__prev1 = i;

        foreach (var (_, __i) in a.valueStart) {
            i = __i;
            print(i, " ");
        }
        i = i__prev1;
    }

    println();
    println("stack", a.stackBytes);
    println("iregs", a.iregs);
    println("fregs", a.fregs);

}

// stepsForValue returns the ABI instructions for translating
// the i'th Go argument or return value represented by this
// abiSeq to the Go ABI.
private static slice<abiStep> stepsForValue(this ptr<abiSeq> _addr_a, nint i) {
    ref abiSeq a = ref _addr_a.val;

    var s = a.valueStart[i];
    nint e = default;
    if (i == len(a.valueStart) - 1) {
        e = len(a.steps);
    }
    else
 {
        e = a.valueStart[i + 1];
    }
    return a.steps[(int)s..(int)e];

}

// addArg extends the abiSeq with a new Go value of type t.
//
// If the value was stack-assigned, returns the single
// abiStep describing that translation, and nil otherwise.
private static ptr<abiStep> addArg(this ptr<abiSeq> _addr_a, ptr<rtype> _addr_t) {
    ref abiSeq a = ref _addr_a.val;
    ref rtype t = ref _addr_t.val;
 
    // We'll always be adding a new value, so do that first.
    var pStart = len(a.steps);
    a.valueStart = append(a.valueStart, pStart);
    if (t.size == 0) { 
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
        a.stackBytes = align(a.stackBytes, uintptr(t.align));
        return _addr_null!;

    }
    var aOld = a.val;
    if (!a.regAssign(t, 0)) { 
        // Register assignment failed. Roll back any changes
        // and stack-assign.
        a.val = aOld;
        a.stackAssign(t.size, uintptr(t.align));
        return _addr__addr_a.steps[len(a.steps) - 1]!;

    }
    return _addr_null!;

}

// addRcvr extends the abiSeq with a new method call
// receiver according to the interface calling convention.
//
// If the receiver was stack-assigned, returns the single
// abiStep describing that translation, and nil otherwise.
// Returns true if the receiver is a pointer.
private static (ptr<abiStep>, bool) addRcvr(this ptr<abiSeq> _addr_a, ptr<rtype> _addr_rcvr) {
    ptr<abiStep> _p0 = default!;
    bool _p0 = default;
    ref abiSeq a = ref _addr_a.val;
    ref rtype rcvr = ref _addr_rcvr.val;
 
    // The receiver is always one word.
    a.valueStart = append(a.valueStart, len(a.steps));
    bool ok = default;    bool ptr = default;

    if (ifaceIndir(rcvr) || rcvr.pointers()) {
        ok = a.assignIntN(0, ptrSize, 1, 0b1);
        ptr = true;
    }
    else
 { 
        // TODO(mknyszek): Is this case even possible?
        // The interface data work never contains a non-pointer
        // value. This case was copied over from older code
        // in the reflect package which only conditionally added
        // a pointer bit to the reflect.(Value).Call stack frame's
        // GC bitmap.
        ok = a.assignIntN(0, ptrSize, 1, 0b0);
        ptr = false;

    }
    if (!ok) {
        a.stackAssign(ptrSize, ptrSize);
        return (_addr__addr_a.steps[len(a.steps) - 1]!, ptr);
    }
    return (_addr_null!, ptr);

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
private static bool regAssign(this ptr<abiSeq> _addr_a, ptr<rtype> _addr_t, System.UIntPtr offset) => func((_, panic, _) => {
    ref abiSeq a = ref _addr_a.val;
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == UnsafePointer || t.Kind() == Ptr || t.Kind() == Chan || t.Kind() == Map || t.Kind() == Func) 
        return a.assignIntN(offset, t.size, 1, 0b1);
    else if (t.Kind() == Bool || t.Kind() == Int || t.Kind() == Uint || t.Kind() == Int8 || t.Kind() == Uint8 || t.Kind() == Int16 || t.Kind() == Uint16 || t.Kind() == Int32 || t.Kind() == Uint32 || t.Kind() == Uintptr) 
        return a.assignIntN(offset, t.size, 1, 0b0);
    else if (t.Kind() == Int64 || t.Kind() == Uint64) 
        switch (ptrSize) {
            case 4: 
                return a.assignIntN(offset, 4, 2, 0b0);
                break;
            case 8: 
                return a.assignIntN(offset, 8, 1, 0b0);
                break;
        }
    else if (t.Kind() == Float32 || t.Kind() == Float64) 
        return a.assignFloatN(offset, t.size, 1);
    else if (t.Kind() == Complex64) 
        return a.assignFloatN(offset, 4, 2);
    else if (t.Kind() == Complex128) 
        return a.assignFloatN(offset, 8, 2);
    else if (t.Kind() == String) 
        return a.assignIntN(offset, ptrSize, 2, 0b01);
    else if (t.Kind() == Interface) 
        return a.assignIntN(offset, ptrSize, 2, 0b10);
    else if (t.Kind() == Slice) 
        return a.assignIntN(offset, ptrSize, 3, 0b001);
    else if (t.Kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        switch (tt.len) {
            case 0: 
                // There's nothing to assign, so don't modify
                // a.steps but succeed so the caller doesn't
                // try to stack-assign this value.
                return true;
                break;
            case 1: 
                return a.regAssign(tt.elem, offset);
                break;
            default: 
                return false;
                break;
        }
    else if (t.Kind() == Struct) 
        var st = (structType.val)(@unsafe.Pointer(t));
        foreach (var (i) in st.fields) {
            var f = _addr_st.fields[i];
            if (!a.regAssign(f.typ, offset + f.offset())) {
                return false;
            }
        }        return true;
    else 
        print("t.Kind == ", t.Kind(), "\n");
        panic("unknown type kind");
        panic("unhandled register assignment path");

});

// assignIntN assigns n values to registers, each "size" bytes large,
// from the data at [offset, offset+n*size) in memory. Each value at
// [offset+i*size, offset+(i+1)*size) for i < n is assigned to the
// next n integer registers.
//
// Bit i in ptrMap indicates whether the i'th value is a pointer.
// n must be <= 8.
//
// Returns whether assignment succeeded.
private static bool assignIntN(this ptr<abiSeq> _addr_a, System.UIntPtr offset, System.UIntPtr size, nint n, byte ptrMap) => func((_, panic, _) => {
    ref abiSeq a = ref _addr_a.val;

    if (n > 8 || n < 0) {
        panic("invalid n");
    }
    if (ptrMap != 0 && size != ptrSize) {
        panic("non-empty pointer map passed for non-pointer-size values");
    }
    if (a.iregs + n > intArgRegs) {
        return false;
    }
    for (nint i = 0; i < n; i++) {
        var kind = abiStepIntReg;
        if (ptrMap & (uint8(1) << (int)(i)) != 0) {
            kind = abiStepPointer;
        }
        a.steps = append(a.steps, new abiStep(kind:kind,offset:offset+uintptr(i)*size,size:size,ireg:a.iregs,));
        a.iregs++;

    }
    return true;

});

// assignFloatN assigns n values to registers, each "size" bytes large,
// from the data at [offset, offset+n*size) in memory. Each value at
// [offset+i*size, offset+(i+1)*size) for i < n is assigned to the
// next n floating-point registers.
//
// Returns whether assignment succeeded.
private static bool assignFloatN(this ptr<abiSeq> _addr_a, System.UIntPtr offset, System.UIntPtr size, nint n) => func((_, panic, _) => {
    ref abiSeq a = ref _addr_a.val;

    if (n < 0) {
        panic("invalid n");
    }
    if (a.fregs + n > floatArgRegs || floatRegSize < size) {
        return false;
    }
    for (nint i = 0; i < n; i++) {
        a.steps = append(a.steps, new abiStep(kind:abiStepFloatReg,offset:offset+uintptr(i)*size,size:size,freg:a.fregs,));
        a.fregs++;
    }
    return true;

});

// stackAssign reserves space for one value that is "size" bytes
// large with alignment "alignment" to the stack.
//
// Should not be called directly; use addArg instead.
private static void stackAssign(this ptr<abiSeq> _addr_a, System.UIntPtr size, System.UIntPtr alignment) {
    ref abiSeq a = ref _addr_a.val;

    a.stackBytes = align(a.stackBytes, alignment);
    a.steps = append(a.steps, new abiStep(kind:abiStepStack,offset:0,size:size,stkOff:a.stackBytes,));
    a.stackBytes += size;
}

// abiDesc describes the ABI for a function or method.
private partial struct abiDesc {
    public abiSeq call; // These fields describe the stack space allocated
// for the call. stackCallArgsSize is the amount of space
// reserved for arguments but not return values. retOffset
// is the offset at which return values begin, and
// spill is the size in bytes of additional space reserved
// to spill argument registers into in case of preemption in
// reflectcall's stack frame.
    public abiSeq ret; // These fields describe the stack space allocated
// for the call. stackCallArgsSize is the amount of space
// reserved for arguments but not return values. retOffset
// is the offset at which return values begin, and
// spill is the size in bytes of additional space reserved
// to spill argument registers into in case of preemption in
// reflectcall's stack frame.
    public System.UIntPtr stackCallArgsSize; // stackPtrs is a bitmap that indicates whether
// each word in the ABI stack space (stack-assigned
// args + return values) is a pointer. Used
// as the heap pointer bitmap for stack space
// passed to reflectcall.
    public System.UIntPtr retOffset; // stackPtrs is a bitmap that indicates whether
// each word in the ABI stack space (stack-assigned
// args + return values) is a pointer. Used
// as the heap pointer bitmap for stack space
// passed to reflectcall.
    public System.UIntPtr spill; // stackPtrs is a bitmap that indicates whether
// each word in the ABI stack space (stack-assigned
// args + return values) is a pointer. Used
// as the heap pointer bitmap for stack space
// passed to reflectcall.
    public ptr<bitVector> stackPtrs; // inRegPtrs is a bitmap whose i'th bit indicates
// whether the i'th integer argument register contains
// a pointer. Used by makeFuncStub and methodValueCall
// to make result pointers visible to the GC.
//
// outRegPtrs is the same, but for result values.
// Used by reflectcall to make result pointers visible
// to the GC.
    public abi.IntArgRegBitmap inRegPtrs;
    public abi.IntArgRegBitmap outRegPtrs;
}

private static void dump(this ptr<abiDesc> _addr_a) {
    ref abiDesc a = ref _addr_a.val;

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

private static void dumpPtrBitMap(abi.IntArgRegBitmap b) {
    for (nint i = 0; i < intArgRegs; i++) {
        nint x = 0;
        if (b.Get(i)) {
            x = 1;
        }
        print(" ", x);

    }

}

private static abiDesc newAbiDesc(ptr<funcType> _addr_t, ptr<rtype> _addr_rcvr) {
    ref funcType t = ref _addr_t.val;
    ref rtype rcvr = ref _addr_rcvr.val;
 
    // We need to add space for this argument to
    // the frame so that it can spill args into it.
    //
    // The size of this space is just the sum of the sizes
    // of each register-allocated type.
    //
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    var spill = uintptr(0); 

    // Compute gc program & stack bitmap for stack arguments
    ptr<bitVector> stackPtrs = @new<bitVector>(); 

    // Compute the stack frame pointer bitmap and register
    // pointer bitmap for arguments.
    abi.IntArgRegBitmap inRegPtrs = new abi.IntArgRegBitmap(); 

    // Compute abiSeq for input parameters.
    abiSeq @in = default;
    if (rcvr != null) {
        var (stkStep, isPtr) = @in.addRcvr(rcvr);
        if (stkStep != null) {
            if (isPtr) {
                stackPtrs.append(1);
            }
            else
 {
                stackPtrs.append(0);
            }

        }
        else
 {
            spill += ptrSize;
        }
    }
    {
        var i__prev1 = i;

        foreach (var (__i, __arg) in t.@in()) {
            i = __i;
            arg = __arg;
            var stkStep = @in.addArg(arg);
            if (stkStep != null) {
                addTypeBits(stackPtrs, stkStep.stkOff, arg);
            }
            else
 {
                spill = align(spill, uintptr(arg.align));
                spill += arg.size;
                {
                    var st__prev2 = st;

                    foreach (var (_, __st) in @in.stepsForValue(i)) {
                        st = __st;
                        if (st.kind == abiStepPointer) {
                            inRegPtrs.Set(st.ireg);
                        }
                    }

                    st = st__prev2;
                }
            }

        }
        i = i__prev1;
    }

    spill = align(spill, ptrSize); 

    // From the input parameters alone, we now know
    // the stackCallArgsSize and retOffset.
    var stackCallArgsSize = @in.stackBytes;
    var retOffset = align(@in.stackBytes, ptrSize); 

    // Compute the stack frame pointer bitmap and register
    // pointer bitmap for return values.
    abi.IntArgRegBitmap outRegPtrs = new abi.IntArgRegBitmap(); 

    // Compute abiSeq for output parameters.
    abiSeq @out = default; 
    // Stack-assigned return values do not share
    // space with arguments like they do with registers,
    // so we need to inject a stack offset here.
    // Fake it by artificially extending stackBytes by
    // the return offset.
    @out.stackBytes = retOffset;
    {
        var i__prev1 = i;

        foreach (var (__i, __res) in t.@out()) {
            i = __i;
            res = __res;
            stkStep = @out.addArg(res);
            if (stkStep != null) {
                addTypeBits(stackPtrs, stkStep.stkOff, res);
            }
            else
 {
                {
                    var st__prev2 = st;

                    foreach (var (_, __st) in @out.stepsForValue(i)) {
                        st = __st;
                        if (st.kind == abiStepPointer) {
                            outRegPtrs.Set(st.ireg);
                        }
                    }

                    st = st__prev2;
                }
            }

        }
        i = i__prev1;
    }

    @out.stackBytes -= retOffset;
    return new abiDesc(in,out,stackCallArgsSize,retOffset,spill,stackPtrs,inRegPtrs,outRegPtrs);

}

} // end reflect_package
