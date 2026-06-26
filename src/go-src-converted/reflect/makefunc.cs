// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// MakeFunc implementation.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class reflect_package {

// makeFuncImpl is the closure value implementing the function
// returned by MakeFunc.
// The first three words of this type must be kept in sync with
// methodValue and runtime.reflectMethodValue.
// Any changes should be reflected in all three.
[GoType] partial struct makeFuncImpl {
    internal partial ref makeFuncCtxt makeFuncCtxt { get; }
    internal ж<funcType> ftyp;
    internal Func<slice<ΔValue>, slice<reflect.Value>> fn;
}

// MakeFunc returns a new function of the given [Type]
// that wraps the function fn. When called, that new function
// does the following:
//
//   - converts its arguments to a slice of Values.
//   - runs results := fn(args).
//   - returns the results as a slice of Values, one per formal result.
//
// The implementation fn can assume that the argument [Value] slice
// has the number and type of arguments given by typ.
// If typ describes a variadic function, the final Value is itself
// a slice representing the variadic arguments, as in the
// body of a variadic function. The result Value slice returned by fn
// must have the number and type of results given by typ.
//
// The [Value.Call] method allows the caller to invoke a typed function
// in terms of Values; in contrast, MakeFunc allows the caller to implement
// a typed function in terms of Values.
//
// The Examples section of the documentation includes an illustration
// of how to use MakeFunc to build a swap function for different types.
public static ΔValue MakeFunc(ΔType typ, Func<slice<ΔValue>, (results <>reflect.Value)> fn) {
    if (typ.Kind() != Func) {
        throw panic("reflect: call of MakeFunc with non-Func type");
    }
    var t = typ.common();
    var ftyp = (ж<funcType>)(uintptr)(new @unsafe.Pointer(t));
    var code = abi.FuncPCABI0(makeFuncStub);
    // makeFuncImpl contains a stack map for use by the runtime
    var (_, _, abid) = funcLayout(ftyp, nil);
    var impl = Ꮡ(new makeFuncImpl(
        makeFuncCtxt: new makeFuncCtxt(
            fn: code,
            stack: abid.stackPtrs,
            argLen: abid.stackCallArgsSize,
            regPtrs: abid.inRegPtrs
        ),
        ftyp: ftyp,
        fn: fn
    ));
    return new ΔValue(t, new @unsafe.Pointer(impl), ((flag)Func));
}

// makeFuncStub is an assembly function that is the code half of
// the function returned from MakeFunc. It expects a *callReflectFunc
// as its context register, and its job is to invoke callReflect(ctxt, frame)
// where ctxt is the context register and frame is a pointer to the first
// word in the passed-in argument frame.
internal static partial void makeFuncStub();

// The first 3 words of this type must be kept in sync with
// makeFuncImpl and runtime.reflectMethodValue.
// Any changes should be reflected in all three.
[GoType] partial struct methodValue {
    internal partial ref makeFuncCtxt makeFuncCtxt { get; }
    internal nint method;
    internal ΔValue rcvr;
}

// makeMethodValue converts v from the rcvr+method index representation
// of a method value to an actual method func value, which is
// basically the receiver value with a special bit set, into a true
// func value - a value holding an actual func. The output is
// semantically equivalent to the input as far as the user of package
// reflect can tell, but the true func representation can be handled
// by code like Convert and Interface and Assign.
internal static ΔValue makeMethodValue(@string op, ΔValue v) {
    if ((flag)(v.flag & flagMethod) == 0) {
        throw panic("reflect: internal error: invalid use of makeMethodValue");
    }
    // Ignoring the flagMethod bit, v describes the receiver, not the method type.
    var fl = (flag)(v.flag & ((flag)((flag)(flagRO | flagAddr) | flagIndir)));
    fl |= (flag)(((flag)v.typ().Kind()));
    ref var rcvr = ref heap<ΔValue>(out var Ꮡrcvr);
    rcvr = new ΔValue(v.typ(), v.ptr, fl);
    // v.Type returns the actual type of the method value.
    var ftyp = (ж<funcType>)(uintptr)(new @unsafe.Pointer(v.Type()._<rtype.val>()));
    var code = methodValueCallCodePtr();
    // methodValue contains a stack map for use by the runtime
    var (_, _, abid) = funcLayout(ftyp, nil);
    var fv = Ꮡ(new methodValue(
        makeFuncCtxt: new makeFuncCtxt(
            fn: code,
            stack: abid.stackPtrs,
            argLen: abid.stackCallArgsSize,
            regPtrs: abid.inRegPtrs
        ),
        method: ((nint)v.flag) >> (int)(flagMethodShift),
        rcvr: rcvr
    ));
    // Cause panic if method is not appropriate.
    // The panic would still happen during the call if we omit this,
    // but we want Interface() and other operations to fail early.
    methodReceiver(op, (~fv).rcvr, (~fv).method);
    return new ΔValue(ftyp.Common(), new @unsafe.Pointer(fv), (flag)((flag)(v.flag & flagRO) | ((flag)Func)));
}

internal static uintptr methodValueCallCodePtr() {
    return abi.FuncPCABI0(methodValueCall);
}

// methodValueCall is an assembly function that is the code half of
// the function returned from makeMethodValue. It expects a *methodValue
// as its context register, and its job is to invoke callMethod(ctxt, frame)
// where ctxt is the context register and frame is a pointer to the first
// word in the passed-in argument frame.
internal static partial void methodValueCall();

// This structure must be kept in sync with runtime.reflectMethodValue.
// Any changes should be reflected in all both.
[GoType] partial struct makeFuncCtxt {
    internal uintptr fn;
    internal ж<bitVector> stack; // ptrmap for both stack args and results
    internal uintptr argLen;    // just args
    internal @internal.abi_package.IntArgRegBitmap regPtrs;
}

// moveMakeFuncArgPtrs uses ctxt.regPtrs to copy integer pointer arguments
// in args.Ints to args.Ptrs where the GC can see them.
//
// This is similar to what reflectcallmove does in the runtime, except
// that happens on the return path, whereas this happens on the call path.
//
// nosplit because pointers are being held in uintptr slots in args, so
// having our stack scanned now could lead to accidentally freeing
// memory.
//
//go:nosplit
internal static void moveMakeFuncArgPtrs(ж<makeFuncCtxt> Ꮡctxt, ж<abi.RegArgs> Ꮡargs) {
    ref var ctxt = ref Ꮡctxt.val;
    ref var args = ref Ꮡargs.val;

    foreach (var (i, arg) in args.Ints) {
        // Avoid write barriers! Because our write barrier enqueues what
        // was there before, we might enqueue garbage.
        if (ctxt.regPtrs.Get(i)){
            ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡargs.Ptrs.at<@unsafe.Pointer>(i))))).val = arg;
        } else {
            // We *must* zero this space ourselves because it's defined in
            // assembly code and the GC will scan these pointers. Otherwise,
            // there will be garbage here.
            ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡargs.Ptrs.at<@unsafe.Pointer>(i))))).val = 0;
        }
    }
}

} // end reflect_package
