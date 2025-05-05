// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using itoa = @internal.itoa_package;
using unsafeheader = @internal.unsafeheader_package;
using math = math_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using @internal;
using ꓸꓸꓸΔValue = Span<ΔValue>;

partial class reflect_package {

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its [Value.IsValid] method returns false, its Kind method returns [Invalid],
// its String method returns "<invalid Value>", and all other methods panic.
// Most functions and methods never return an invalid value.
// If one does, its documentation states the conditions explicitly.
//
// A Value can be used concurrently by multiple goroutines provided that
// the underlying Go value can be used concurrently for the equivalent
// direct operations.
//
// To compare two Values, compare the results of the Interface method.
// Using == on two Values does not compare the underlying values
// they represent.
[GoType] partial struct ΔValue {
    // typ_ holds the type of the value represented by a Value.
    // Access using the typ method to avoid escape of v.
    internal ж<@internal.abi_package.Type> typ_;
    // Pointer-valued data or, if flagIndir is set, pointer to data.
    // Valid when either flagIndir is set or typ.pointers() is true.
    internal @unsafe.Pointer ptr;
    // flag holds metadata about the value.
    //
    // The lowest five bits give the Kind of the value, mirroring typ.Kind().
    //
    // The next set of bits are flag bits:
    //	- flagStickyRO: obtained via unexported not embedded field, so read-only
    //	- flagEmbedRO: obtained via unexported embedded field, so read-only
    //	- flagIndir: val holds a pointer to the data
    //	- flagAddr: v.CanAddr is true (implies flagIndir and ptr is non-nil)
    //	- flagMethod: v is a method value.
    // If ifaceIndir(typ), code can assume that flagIndir is set.
    //
    // The remaining 22+ bits give a method number for method values.
    // If flag.kind() != Func, code can assume that flagMethod is unset.
    internal partial ref flag flag { get; }
}

[GoType("num:uintptr")] partial struct flag;

// A method value represents a curried method invocation
// like r.Read for some receiver r. The typ+val+flag bits describe
// the receiver r, but the flag's Kind bits say Func (methods are
// functions), and the top bits of the flag give the method number
// in r's type's method table.
internal static readonly UntypedInt flagKindWidth = 5; // there are 27 kinds
internal static readonly flag flagKindMask = /* 1<<flagKindWidth - 1 */ 31;
internal static readonly flag flagStickyRO = /* 1 << 5 */ 32;
internal static readonly flag flagEmbedRO = /* 1 << 6 */ 64;
internal static readonly flag flagIndir = /* 1 << 7 */ 128;
internal static readonly flag flagAddr = /* 1 << 8 */ 256;
internal static readonly flag flagMethod = /* 1 << 9 */ 512;
internal static readonly UntypedInt flagMethodShift = 10;
internal static readonly flag flagRO = /* flagStickyRO | flagEmbedRO */ 96;

internal static ΔKind kind(this flag f) {
    return ((ΔKind)((flag)(f & flagKindMask)));
}

internal static flag ro(this flag f) {
    if ((flag)(f & flagRO) != 0) {
        return flagStickyRO;
    }
    return 0;
}

internal static ж<abi.Type> typ(this ΔValue v) {
    // Types are either static (for compiler-created types) or
    // heap-allocated but always reachable (for reflection-created
    // types, held in the central map). So there is no need to
    // escape types. noescape here help avoid unnecessary escape
    // of v.
    return (ж<abi.Type>)(uintptr)(abi.NoEscape(new @unsafe.Pointer(v.typ_)));
}

// pointer returns the underlying pointer represented by v.
// v.Kind() must be Pointer, Map, Chan, Func, or UnsafePointer
// if v.Kind() == Pointer, the base type must not be not-in-heap.
internal static @unsafe.Pointer pointer(this ΔValue v) {
    if (v.typ().Size() != goarch.PtrSize || !v.typ().Pointers()) {
        throw panic("can't call pointer on a non-pointer Value");
    }
    if ((flag)(v.flag & flagIndir) != 0) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    }
    return v.ptr;
}

// packEface converts v to the empty interface.
internal static any packEface(ΔValue v) {
    var t = v.typ();
    any i = default!;
    var e = (ж<abi.EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(i)));
    // First, fill in the data portion of the interface.
    switch (ᐧ) {
    case {} when t.IfaceIndir(): {
        if ((flag)(v.flag & flagIndir) == 0) {
            throw panic("bad indir");
        }
        @unsafe.Pointer ptr = v.ptr;
        if ((flag)(v.flag & flagAddr) != 0) {
            // Value is indirect, and so is the interface we're making.
            @unsafe.Pointer c = (uintptr)unsafe_New(t);
            typedmemmove(t, c, ptr);
            ptr = c;
        }
        e.val.Data = ptr;
        break;
    }
    case {} when (flag)(v.flag & flagIndir) != 0: {
        e.val.Data = ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
        break;
    }
    default: {
        e.val.Data = v.ptr;
        break;
    }}

    // Value is indirect, but interface is direct. We need
    // to load the data at v.ptr into the interface data word.
    // Value is direct, and so is the interface.
    // Now, fill in the type portion. We're very careful here not
    // to have any operation between the e.word and e.typ assignments
    // that would let the garbage collector observe the partially-built
    // interface value.
    e.val.Type = t;
    return i;
}

// unpackEface converts the empty interface i to a Value.
internal static ΔValue unpackEface(any i) {
    var e = (ж<abi.EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(i)));
    // NOTE: don't read e.word until we know whether it is really a pointer or not.
    var t = e.val.Type;
    if (t == nil) {
        return new ΔValue(nil);
    }
    var f = ((flag)t.Kind());
    if (t.IfaceIndir()) {
        f |= (flag)(flagIndir);
    }
    return new ΔValue(t, (~e).Data, f);
}

// A ValueError occurs when a Value method is invoked on
// a [Value] that does not support it. Such cases are documented
// in the description of each method.
[GoType] partial struct ValueError {
    public @string Method;
    public ΔKind Kind;
}

[GoRecv] public static @string Error(this ref ValueError e) {
    if (e.Kind == 0) {
        return "reflect: call of "u8 + e.Method + " on zero Value"u8;
    }
    return "reflect: call of "u8 + e.Method + " on "u8 + e.Kind.String() + " Value"u8;
}

// valueMethodName returns the name of the exported calling method on Value.
internal static @string valueMethodName() {
    array<uintptr> pc = new(5);
    nint n = runtime.Callers(1, pc[..]);
    var frames = runtime.CallersFrames(pc[..(int)(n)]);
    runtime.Frame frame = default!;
    for (var more = true; more; ) {
        @string prefix = "reflect.Value."u8;
        (frame, more) = frames.Next();
        @string name = frame.Function;
        if (len(name) > len(prefix) && name[..(int)(len(prefix))] == prefix) {
            @string methodName = name[(int)(len(prefix))..];
            if (len(methodName) > 0 && (rune)'A' <= methodName[0] && methodName[0] <= (rune)'Z') {
                return name;
            }
        }
    }
    return "unknown method"u8;
}

// nonEmptyInterface is the header for an interface value with methods.
[GoType] partial struct nonEmptyInterface {
    internal ж<@internal.abi_package.ITab> itab;
    internal @unsafe.Pointer word;
}

// mustBe panics if f's kind is not expected.
// Making this a method on flag instead of on Value
// (and embedding flag in Value) means that we can write
// the very clear v.mustBe(Bool) and have it compile into
// v.flag.mustBe(Bool), which will only bother to copy the
// single important word for the receiver.
internal static void mustBe(this flag f, ΔKind expected) {
    // TODO(mvdan): use f.kind() again once mid-stack inlining gets better
    if (((ΔKind)((flag)(f & flagKindMask))) != expected) {
        throw panic(Ꮡ(new ValueError(valueMethodName(), f.kind())));
    }
}

// mustBeExported panics if f records that the value was obtained using
// an unexported field.
internal static void mustBeExported(this flag f) {
    if (f == 0 || (flag)(f & flagRO) != 0) {
        f.mustBeExportedSlow();
    }
}

internal static void mustBeExportedSlow(this flag f) {
    if (f == 0) {
        throw panic(Ꮡ(new ValueError(valueMethodName(), Invalid)));
    }
    if ((flag)(f & flagRO) != 0) {
        throw panic("reflect: "u8 + valueMethodName() + " using value obtained using unexported field"u8);
    }
}

// mustBeAssignable panics if f records that the value is not assignable,
// which is to say that either it was obtained using an unexported field
// or it is not addressable.
internal static void mustBeAssignable(this flag f) {
    if ((flag)(f & flagRO) != 0 || (flag)(f & flagAddr) == 0) {
        f.mustBeAssignableSlow();
    }
}

internal static void mustBeAssignableSlow(this flag f) {
    if (f == 0) {
        throw panic(Ꮡ(new ValueError(valueMethodName(), Invalid)));
    }
    // Assignable if addressable and not read-only.
    if ((flag)(f & flagRO) != 0) {
        throw panic("reflect: "u8 + valueMethodName() + " using value obtained using unexported field"u8);
    }
    if ((flag)(f & flagAddr) == 0) {
        throw panic("reflect: "u8 + valueMethodName() + " using unaddressable value"u8);
    }
}

// Addr returns a pointer value representing the address of v.
// It panics if [Value.CanAddr] returns false.
// Addr is typically used to obtain a pointer to a struct field
// or slice element in order to call a method that requires a
// pointer receiver.
public static ΔValue Addr(this ΔValue v) {
    if ((flag)(v.flag & flagAddr) == 0) {
        throw panic("reflect.Value.Addr of unaddressable value");
    }
    // Preserve flagRO instead of using v.flag.ro() so that
    // v.Addr().Elem() is equivalent to v (#32772)
    var fl = (flag)(v.flag & flagRO);
    return new ΔValue(ptrTo(v.typ()), v.ptr, (flag)(fl | ((flag)ΔPointer)));
}

// Bool returns v's underlying value.
// It panics if v's kind is not [Bool].
public static bool Bool(this ΔValue v) {
    // panicNotBool is split out to keep Bool inlineable.
    if (v.kind() != ΔBool) {
        v.panicNotBool();
    }
    return ~(ж<bool>)(uintptr)(v.ptr);
}

internal static void panicNotBool(this ΔValue v) {
    v.mustBe(ΔBool);
}

internal static ж<abi.Type> bytesType = rtypeOf((slice<byte>)(default!));

// Bytes returns v's underlying value.
// It panics if v's underlying value is not a slice of bytes or
// an addressable array of bytes.
public static slice<byte> Bytes(this ΔValue v) {
    // bytesSlow is split out to keep Bytes inlineable for unnamed []byte.
    if (v.typ_ == bytesType) {
        // ok to use v.typ_ directly as comparison doesn't cause escape
        return ~(ж<slice<byte>>)(uintptr)(v.ptr);
    }
    return v.bytesSlow();
}

internal static slice<byte> bytesSlow(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔSlice) {
        if (v.typ().Elem().Kind() != abi.Uint8) {
            throw panic("reflect.Value.Bytes of non-byte slice");
        }
        return ~(ж<slice<byte>>)(uintptr)(v.ptr);
    }
    if (exprᴛ1 == Array) {
        if (v.typ().Elem().Kind() != abi.Uint8) {
            // Slice is always bigger than a word; assume flagIndir.
            throw panic("reflect.Value.Bytes of non-byte array");
        }
        if (!v.CanAddr()) {
            throw panic("reflect.Value.Bytes of unaddressable byte array");
        }
        var p = (ж<byte>)(uintptr)(v.ptr);
        nint n = ((nint)((ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()))).val.Len);
        return @unsafe.Slice(p, n);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Bytes", v.kind())));
}

// runes returns v's underlying value.
// It panics if v's underlying value is not a slice of runes (int32s).
internal static slice<rune> runes(this ΔValue v) {
    v.mustBe(ΔSlice);
    if (v.typ().Elem().Kind() != abi.Int32) {
        throw panic("reflect.Value.Bytes of non-rune slice");
    }
    // Slice is always bigger than a word; assume flagIndir.
    return ~(ж<slice<rune>>)(uintptr)(v.ptr);
}

// CanAddr reports whether the value's address can be obtained with [Value.Addr].
// Such values are called addressable. A value is addressable if it is
// an element of a slice, an element of an addressable array,
// a field of an addressable struct, or the result of dereferencing a pointer.
// If CanAddr returns false, calling [Value.Addr] will panic.
public static bool CanAddr(this ΔValue v) {
    return (flag)(v.flag & flagAddr) != 0;
}

// CanSet reports whether the value of v can be changed.
// A [Value] can be changed only if it is addressable and was not
// obtained by the use of unexported struct fields.
// If CanSet returns false, calling [Value.Set] or any type-specific
// setter (e.g., [Value.SetBool], [Value.SetInt]) will panic.
public static bool CanSet(this ΔValue v) {
    return (flag)(v.flag & ((flag)(flagAddr | flagRO))) == flagAddr;
}

// Call calls the function v with the input arguments in.
// For example, if len(in) == 3, v.Call(in) represents the Go call v(in[0], in[1], in[2]).
// Call panics if v's Kind is not [Func].
// It returns the output results as Values.
// As in Go, each input argument must be assignable to the
// type of the function's corresponding input parameter.
// If v is a variadic function, Call creates the variadic slice parameter
// itself, copying in the corresponding values.
public static slice<ΔValue> Call(this ΔValue v, slice<ΔValue> @in) {
    v.mustBe(Func);
    v.mustBeExported();
    return v.call("Call"u8, @in);
}

// CallSlice calls the variadic function v with the input arguments in,
// assigning the slice in[len(in)-1] to v's final variadic argument.
// For example, if len(in) == 3, v.CallSlice(in) represents the Go call v(in[0], in[1], in[2]...).
// CallSlice panics if v's Kind is not [Func] or if v is not variadic.
// It returns the output results as Values.
// As in Go, each input argument must be assignable to the
// type of the function's corresponding input parameter.
public static slice<ΔValue> CallSlice(this ΔValue v, slice<ΔValue> @in) {
    v.mustBe(Func);
    v.mustBeExported();
    return v.call("CallSlice"u8, @in);
}

internal static bool callGC; // for testing; see TestCallMethodJump and TestCallArgLive

internal const bool debugReflectCall = false;

internal static slice<ΔValue> call(this ΔValue v, @string op, slice<ΔValue> @in) {
    // Get function pointer, type.
    var t = (ж<funcType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    @unsafe.Pointer fn = default!;
    ΔValue rcvr = default!;
    ж<abi.Type> rcvrtype = default!;
    if ((flag)(v.flag & flagMethod) != 0){
        rcvr = v;
        (rcvrtype, t, fn) = methodReceiver(op, v, ((nint)v.flag) >> (int)(flagMethodShift));
    } else 
    if ((flag)(v.flag & flagIndir) != 0){
        fn = ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    } else {
        fn = v.ptr;
    }
    if (fn == nil) {
        throw panic("reflect.Value.Call: call of nil function");
    }
    var isSlice = op == "CallSlice"u8;
    nint n = t.NumIn();
    var isVariadic = t.IsVariadic();
    if (isSlice){
        if (!isVariadic) {
            throw panic("reflect: CallSlice of non-variadic function");
        }
        if (len(@in) < n) {
            throw panic("reflect: CallSlice with too few input arguments");
        }
        if (len(@in) > n) {
            throw panic("reflect: CallSlice with too many input arguments");
        }
    } else {
        if (isVariadic) {
            n--;
        }
        if (len(@in) < n) {
            throw panic("reflect: Call with too few input arguments");
        }
        if (!isVariadic && len(@in) > n) {
            throw panic("reflect: Call with too many input arguments");
        }
    }
    foreach (var (_, x) in @in) {
        if (x.Kind() == Invalid) {
            throw panic("reflect: "u8 + op + " using zero Value argument"u8);
        }
    }
    for (nint i = 0; i < n; i++) {
        {
            var xt = @in[i].Type();
            var targ = t.In(i); if (!xt.AssignableTo(~toRType(targ))) {
                throw panic("reflect: "u8 + op + " using "u8 + xt.String() + " as type "u8 + stringFor(targ));
            }
        }
    }
    if (!isSlice && isVariadic) {
        // prepare slice for remaining values
        nint m = len(@in) - n;
        var Δslice = MakeSlice(~toRType(t.In(n)), m, m);
        var elem = toRType(t.In(n)).Elem();
        // FIXME cast to slice type and Elem()
        for (nint i = 0; i < m; i++) {
            var x = @in[n + i];
            {
                var xt = x.Type(); if (!xt.AssignableTo(elem)) {
                    throw panic("reflect: cannot use "u8 + xt.String() + " as type "u8 + elem.String() + " in "u8 + op);
                }
            }
            Δslice.Index(i).Set(x);
        }
        var origIn = @in;
        @in = new slice<ΔValue>(n + 1);
        copy(@in[..(int)(n)], origIn);
        @in[n] = Δslice;
    }
    nint nin = len(@in);
    if (nin != t.NumIn()) {
        throw panic("reflect.Value.Call: wrong argument count");
    }
    nint nout = t.NumOut();
    // Register argument space.
    ref var regArgs = ref heap(new @internal.abi_package.RegArgs(), out var ᏑregArgs);
    // Compute frame type.
    var (frametype, framePool, abid) = funcLayout(t, rcvrtype);
    // Allocate a chunk of memory for frame if needed.
    @unsafe.Pointer stackArgs = default!;
    if (frametype.Size() != 0) {
        if (nout == 0){
            stackArgs = framePool.Get()._<@unsafe.Pointer>();
        } else {
            // Can't use pool if the function has return values.
            // We will leak pointer to args in ret, so its lifetime is not scoped.
            stackArgs = (uintptr)unsafe_New(frametype);
        }
    }
    var frameSize = frametype.Size();
    if (debugReflectCall) {
        println("reflect.call", stringFor(Ꮡ((~t).Type)));
        abid.dump();
    }
    // Copy inputs into args.
    // Handle receiver.
    nint inStart = 0;
    if (rcvrtype != nil) {
        // Guaranteed to only be one word in size,
        // so it will only take up exactly 1 abiStep (either
        // in a register or on the stack).
        {
            var st = abid.call.steps[0];
            var exprᴛ1 = st.kind;
            var matchᴛ1 = false;
            if (exprᴛ1 == abiStepStack) { matchᴛ1 = true;
                storeRcvr(rcvr, stackArgs);
            }
            else if (exprᴛ1 == abiStepPointer) { matchᴛ1 = true;
                storeRcvr(rcvr, ((@unsafe.Pointer)(ᏑregArgs.Ptrs.at<@unsafe.Pointer>(st.ireg))));
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == abiStepIntReg) { matchᴛ1 = true;
                storeRcvr(rcvr, ((@unsafe.Pointer)(ᏑregArgs.Ints.at<uintptr>(st.ireg))));
            }
            else if (exprᴛ1 == abiStepFloatReg) {
                storeRcvr(rcvr, new @unsafe.Pointer(ᏑregArgs.Floats.at<uint64>(st.freg)));
            }
            else { /* default: */
                throw panic("unknown ABI parameter kind");
            }
        }

        inStart = 1;
    }
    // Handle arguments.
    foreach (var (i, vΔ1) in @in) {
        vΔ1.mustBeExported();
        var targ = toRType(t.In(i));
        // TODO(mknyszek): Figure out if it's possible to get some
        // scratch space for this assignment check. Previously, it
        // was possible to use space in the argument frame.
        vΔ1 = vΔ1.assignTo("reflect.Value.Call"u8, Ꮡ((~targ).t), nil);
stepsLoop:
        foreach (var (_, st) in abid.call.stepsForValue(i + inStart)) {
            var exprᴛ2 = st.kind;
            if (exprᴛ2 == abiStepStack) {
                @unsafe.Pointer addr = (uintptr)add(stackArgs, // Copy values to the "stack."
 st.stkOff, "precomputed stack arg offset"u8);
                if ((flag)(vΔ1.flag & flagIndir) != 0){
                    typedmemmove(Ꮡ((~targ).t), addr, vΔ1.ptr);
                } else {
                    ((ж<@unsafe.Pointer>)(uintptr)(addr)).val = vΔ1.ptr;
                }
                goto break_stepsLoop;
            }
            else if (exprᴛ2 == abiStepIntReg || exprᴛ2 == abiStepPointer) {
                if ((flag)(vΔ1.flag & flagIndir) != 0){
                    // There's only one step for a stack-allocated value.
                    // Copy values to "integer registers."
                    @unsafe.Pointer offset = (uintptr)add(vΔ1.ptr, st.offset, "precomputed value offset"u8);
                    if (st.kind == abiStepPointer) {
                        // Duplicate this pointer in the pointer area of the
                        // register space. Otherwise, there's the potential for
                        // this to be the last reference to v.ptr.
                        regArgs.Ptrs[st.ireg] = ~(ж<@unsafe.Pointer>)(uintptr)(offset);
                    }
                    intToReg(ᏑregArgs, st.ireg, st.size, offset);
                } else {
                    if (st.kind == abiStepPointer) {
                        // See the comment in abiStepPointer case above.
                        regArgs.Ptrs[st.ireg] = vΔ1.ptr;
                    }
                    regArgs.Ints[st.ireg] = ((uintptr)vΔ1.ptr);
                }
            }
            else if (exprᴛ2 == abiStepFloatReg) {
                if ((flag)(vΔ1.flag & flagIndir) == 0) {
                    // Copy values to "float registers."
                    throw panic("attempted to copy pointer to FP register");
                }
                @unsafe.Pointer offset = (uintptr)add(vΔ1.ptr, st.offset, "precomputed value offset"u8);
                floatToReg(ᏑregArgs, st.freg, st.size, offset);
            }
            else { /* default: */
                throw panic("unknown ABI part kind");
            }

        }
    }
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    frameSize = align(frameSize, goarch.PtrSize);
    frameSize += abid.spill;
    // Mark pointers in registers for the return path.
    regArgs.ReturnIsPtr = abid.outRegPtrs;
    if (debugReflectCall) {
        regArgs.Dump();
    }
    // For testing; see TestCallArgLive.
    if (callGC) {
        runtime.GC();
    }
    // Call.
    call(frametype, fn, stackArgs, ((uint32)frametype.Size()), ((uint32)abid.retOffset), ((uint32)frameSize), ᏑregArgs);
    // For testing; see TestCallMethodJump.
    if (callGC) {
        runtime.GC();
    }
    slice<ΔValue> ret = default!;
    if (nout == 0){
        if (stackArgs != nil) {
            typedmemclr(frametype, stackArgs);
            framePool.Put(stackArgs);
        }
    } else {
        if (stackArgs != nil) {
            // Zero the now unused input area of args,
            // because the Values returned by this function contain pointers to the args object,
            // and will thus keep the args object alive indefinitely.
            typedmemclrpartial(frametype, stackArgs, 0, abid.retOffset);
        }
        // Wrap Values around return values in args.
        ret = new slice<ΔValue>(nout);
        for (nint i = 0; i < nout; i++) {
            var tv = t.Out(i);
            if (tv.Size() == 0) {
                // For zero-sized return value, args+off may point to the next object.
                // In this case, return the zero value instead.
                ret[i] = Zero(~toRType(tv));
                continue;
            }
            var steps = abid.ret.stepsForValue(i);
            {
                var st = steps[0]; if (st.kind == abiStepStack) {
                    // This value is on the stack. If part of a value is stack
                    // allocated, the entire value is according to the ABI. So
                    // just make an indirection into the allocated frame.
                    var fl = (flag)(flagIndir | ((flag)tv.Kind()));
                    ret[i] = new ΔValue(tv, (uintptr)add(stackArgs, st.stkOff, "tv.Size() != 0"u8), fl);
                    // Note: this does introduce false sharing between results -
                    // if any result is live, they are all live.
                    // (And the space for the args is live as well, but as we've
                    // cleared that space it isn't as big a deal.)
                    continue;
                }
            }
            // Handle pointers passed in registers.
            if (!tv.IfaceIndir()) {
                // Pointer-valued data gets put directly
                // into v.ptr.
                if (steps[0].kind != abiStepPointer) {
                    print("kind=", steps[0].kind, ", type=", stringFor(tv), "\n");
                    throw panic("mismatch between ABI description and types");
                }
                ret[i] = new ΔValue(tv, regArgs.Ptrs[steps[0].ireg], ((flag)tv.Kind()));
                continue;
            }
            // All that's left is values passed in registers that we need to
            // create space for and copy values back into.
            //
            // TODO(mknyszek): We make a new allocation for each register-allocated
            // value, but previously we could always point into the heap-allocated
            // stack frame. This is a regression that could be fixed by adding
            // additional space to the allocated stack frame and storing the
            // register-allocated return values into the allocated stack frame and
            // referring there in the resulting Value.
            @unsafe.Pointer s = (uintptr)unsafe_New(tv);
            foreach (var (_, st) in steps) {
                var exprᴛ3 = st.kind;
                if (exprᴛ3 == abiStepIntReg) {
                    @unsafe.Pointer offset = (uintptr)add(s, st.offset, "precomputed value offset"u8);
                    intFromReg(ᏑregArgs, st.ireg, st.size, offset);
                }
                else if (exprᴛ3 == abiStepPointer) {
                    @unsafe.Pointer sΔ2 = (uintptr)add(s, st.offset, "precomputed value offset"u8);
                    ((ж<@unsafe.Pointer>)(uintptr)(s)).val = regArgs.Ptrs[st.ireg];
                }
                else if (exprᴛ3 == abiStepFloatReg) {
                    @unsafe.Pointer offset = (uintptr)add(s, st.offset, "precomputed value offset"u8);
                    floatFromReg(ᏑregArgs, st.freg, st.size, offset);
                }
                else if (exprᴛ3 == abiStepStack) {
                    throw panic("register-based return value has stack component");
                }
                else { /* default: */
                    throw panic("unknown ABI part kind");
                }

            }
            ret[i] = new ΔValue(tv, s.val, (flag)(flagIndir | ((flag)tv.Kind())));
        }
    }
    return ret;
}

// callReflect is the call implementation used by a function
// returned by MakeFunc. In many ways it is the opposite of the
// method Value.call above. The method above converts a call using Values
// into a call of a function with a concrete argument frame, while
// callReflect converts a call of a function with a concrete argument
// frame into a call using Values.
// It is in this file so that it can be next to the call method above.
// The remainder of the MakeFunc implementation is in makefunc.go.
//
// NOTE: This function must be marked as a "wrapper" in the generated code,
// so that the linker can make it work correctly for panic and recover.
// The gc compilers know to do that for the name "reflect.callReflect".
//
// ctxt is the "closure" generated by MakeFunc.
// frame is a pointer to the arguments to that closure on the stack.
// retValid points to a boolean which should be set when the results
// section of frame is set.
//
// regs contains the argument values passed in registers and will contain
// the values returned from ctxt.fn in registers.
internal static void callReflect(ж<makeFuncImpl> Ꮡctxt, @unsafe.Pointer frame, ж<bool> ᏑretValid, ж<abi.RegArgs> Ꮡregs) {
    ref var ctxt = ref Ꮡctxt.val;
    ref var retValid = ref ᏑretValid.val;
    ref var regs = ref Ꮡregs.val;

    if (callGC) {
        // Call GC upon entry during testing.
        // Getting our stack scanned here is the biggest hazard, because
        // our caller (makeFuncStub) could have failed to place the last
        // pointer to a value in regs' pointer space, in which case it
        // won't be visible to the GC.
        runtime.GC();
    }
    var ftyp = ctxt.ftyp;
    var f = ctxt.fn;
    var (_, _, abid) = funcLayout(ftyp, nil);
    // Copy arguments into Values.
    @unsafe.Pointer ptr = frame;
    var @in = new slice<ΔValue>(0, ((nint)(~ftyp).InCount));
    foreach (var (i, typ) in ftyp.InSlice()) {
        if (typ.Size() == 0) {
            @in = append(@in, Zero(~toRType(typ)));
            continue;
        }
        var v = new ΔValue(typ, nil, ((flag)typ.Kind()));
        var steps = abid.call.stepsForValue(i);
        {
            var st = steps[0]; if (st.kind == abiStepStack){
                if (typ.IfaceIndir()){
                    // value cannot be inlined in interface data.
                    // Must make a copy, because f might keep a reference to it,
                    // and we cannot let f keep a reference to the stack frame
                    // after this function returns, not even a read-only reference.
                    v.ptr = (uintptr)unsafe_New(typ);
                    if (typ.Size() > 0) {
                        typedmemmove(typ, v.ptr, (uintptr)add(ptr, st.stkOff, "typ.size > 0"u8));
                    }
                    v.flag |= (flag)(flagIndir);
                } else {
                    v.ptr = ~(ж<@unsafe.Pointer>)(uintptr)(add(ptr, st.stkOff, "1-ptr"u8));
                }
            } else {
                if (typ.IfaceIndir()){
                    // All that's left is values passed in registers that we need to
                    // create space for the values.
                    v.flag |= (flag)(flagIndir);
                    v.ptr = (uintptr)unsafe_New(typ);
                    foreach (var (_, stΔ1) in steps) {
                        var exprᴛ1 = stΔ1.kind;
                        if (exprᴛ1 == abiStepIntReg) {
                            @unsafe.Pointer offset = (uintptr)add(v.ptr, stΔ1.offset, "precomputed value offset"u8);
                            intFromReg(Ꮡregs, stΔ1.ireg, stΔ1.size, offset);
                        }
                        else if (exprᴛ1 == abiStepPointer) {
                            @unsafe.Pointer s = (uintptr)add(v.ptr, stΔ1.offset, "precomputed value offset"u8);
                            ((ж<@unsafe.Pointer>)(uintptr)(s)).val = regs.Ptrs[stΔ1.ireg];
                        }
                        else if (exprᴛ1 == abiStepFloatReg) {
                            @unsafe.Pointer offset = (uintptr)add(v.ptr, stΔ1.offset, "precomputed value offset"u8);
                            floatFromReg(Ꮡregs, stΔ1.freg, stΔ1.size, offset);
                        }
                        else if (exprᴛ1 == abiStepStack) {
                            throw panic("register-based return value has stack component");
                        }
                        else { /* default: */
                            throw panic("unknown ABI part kind");
                        }

                    }
                } else {
                    // Pointer-valued data gets put directly
                    // into v.ptr.
                    if (steps[0].kind != abiStepPointer) {
                        print("kind=", steps[0].kind, ", type=", stringFor(typ), "\n");
                        throw panic("mismatch between ABI description and types");
                    }
                    v.ptr = regs.Ptrs[steps[0].ireg];
                }
            }
        }
        @in = append(@in, v);
    }
    // Call underlying function.
    var @out = f(@in);
    nint numOut = ftyp.NumOut();
    if (len(@out) != numOut) {
        throw panic("reflect: wrong return count from function created by MakeFunc");
    }
    // Copy results back into argument frame and register space.
    if (numOut > 0) {
        foreach (var (i, typ) in ftyp.OutSlice()) {
            var v = @out[i];
            if (v.typ() == nil) {
                throw panic("reflect: function created by MakeFunc using "u8 + funcName(f) + " returned zero Value"u8);
            }
            if ((flag)(v.flag & flagRO) != 0) {
                throw panic("reflect: function created by MakeFunc using "u8 + funcName(f) + " returned value obtained from unexported field"u8);
            }
            if (typ.Size() == 0) {
                continue;
            }
            // Convert v to type typ if v is assignable to a variable
            // of type t in the language spec.
            // See issue 28761.
            //
            //
            // TODO(mknyszek): In the switch to the register ABI we lost
            // the scratch space here for the register cases (and
            // temporarily for all the cases).
            //
            // If/when this happens, take note of the following:
            //
            // We must clear the destination before calling assignTo,
            // in case assignTo writes (with memory barriers) to the
            // target location used as scratch space. See issue 39541.
            v = v.assignTo("reflect.MakeFunc"u8, typ, nil);
stepsLoop:
            foreach (var (_, st) in abid.ret.stepsForValue(i)) {
                var exprᴛ2 = st.kind;
                if (exprᴛ2 == abiStepStack) {
                    @unsafe.Pointer addr = (uintptr)add(ptr, // Copy values to the "stack."
 st.stkOff, "precomputed stack arg offset"u8);
                    if ((flag)(v.flag & flagIndir) != 0){
                        // Do not use write barriers. The stack space used
                        // for this call is not adequately zeroed, and we
                        // are careful to keep the arguments alive until we
                        // return to makeFuncStub's caller.
                        memmove(addr, v.ptr, st.size);
                    } else {
                        // This case must be a pointer type.
                        ((ж<uintptr>)(uintptr)(addr)).val = ((uintptr)v.ptr);
                    }
                    goto break_stepsLoop;
                }
                else if (exprᴛ2 == abiStepIntReg || exprᴛ2 == abiStepPointer) {
                    if ((flag)(v.flag & flagIndir) != 0){
                        // There's only one step for a stack-allocated value.
                        // Copy values to "integer registers."
                        @unsafe.Pointer offset = (uintptr)add(v.ptr, st.offset, "precomputed value offset"u8);
                        intToReg(Ꮡregs, st.ireg, st.size, offset);
                    } else {
                        // Only populate the Ints space on the return path.
                        // This is safe because out is kept alive until the
                        // end of this function, and the return path through
                        // makeFuncStub has no preemption, so these pointers
                        // are always visible to the GC.
                        regs.Ints[st.ireg] = ((uintptr)v.ptr);
                    }
                }
                else if (exprᴛ2 == abiStepFloatReg) {
                    if ((flag)(v.flag & flagIndir) == 0) {
                        // Copy values to "float registers."
                        throw panic("attempted to copy pointer to FP register");
                    }
                    @unsafe.Pointer offset = (uintptr)add(v.ptr, st.offset, "precomputed value offset"u8);
                    floatToReg(Ꮡregs, st.freg, st.size, offset);
                }
                else { /* default: */
                    throw panic("unknown ABI part kind");
                }

            }
        }
    }
    // Announce that the return values are valid.
    // After this point the runtime can depend on the return values being valid.
    retValid = true;
    // We have to make sure that the out slice lives at least until
    // the runtime knows the return values are valid. Otherwise, the
    // return values might not be scanned by anyone during a GC.
    // (out would be dead, and the return slots not yet alive.)
    runtime.KeepAlive(@out);
    // runtime.getArgInfo expects to be able to find ctxt on the
    // stack when it finds our caller, makeFuncStub. Make sure it
    // doesn't get garbage collected.
    runtime.KeepAlive(ctxt);
}

// methodReceiver returns information about the receiver
// described by v. The Value v may or may not have the
// flagMethod bit set, so the kind cached in v.flag should
// not be used.
// The return value rcvrtype gives the method's actual receiver type.
// The return value t gives the method type signature (without the receiver).
// The return value fn is a pointer to the method code.
internal static (ж<abi.Type> rcvrtype, ж<funcType> t, @unsafe.Pointer fn) methodReceiver(@string op, ΔValue v, nint methodIndex) {
    ж<abi.Type> rcvrtype = default!;
    ж<funcType> t = default!;
    @unsafe.Pointer fn = default!;

    nint i = methodIndex;
    if (v.typ().Kind() == abi.Interface){
        var tt = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        if (((nuint)i) >= ((nuint)len(tt.Methods))) {
            throw panic("reflect: internal error: invalid method index");
        }
        var m = Ꮡ(tt.Methods, i);
        if (!tt.nameOff((~m).Name).IsExported()) {
            throw panic("reflect: "u8 + op + " of unexported method"u8);
        }
        var iface = (ж<nonEmptyInterface>)(uintptr)(v.ptr);
        if ((~iface).itab == nil) {
            throw panic("reflect: "u8 + op + " of method on nil interface value"u8);
        }
        rcvrtype = (~iface).itab.val.Type;
        fn = ((@unsafe.Pointer)(Ꮡ@unsafe.Slice(Ꮡ(~(~iface).itab).Fun.at<uintptr>(0), i + 1).at<uintptr>(i)));
        t = (ж<funcType>)(uintptr)(new @unsafe.Pointer(tt.typeOff((~m).Typ)));
    } else {
        rcvrtype = v.typ();
        var ms = v.typ().ExportedMethods();
        if (((nuint)i) >= ((nuint)len(ms))) {
            throw panic("reflect: internal error: invalid method index");
        }
        var m = ms[i];
        if (!nameOffFor(v.typ(), m.Name).IsExported()) {
            throw panic("reflect: "u8 + op + " of unexported method"u8);
        }
        ref var ifn = ref heap<@unsafe.Pointer>(out var Ꮡifn);
        ifn = (uintptr)textOffFor(v.typ(), m.Ifn);
        fn = ((@unsafe.Pointer)(Ꮡifn));
        t = (ж<funcType>)(uintptr)(new @unsafe.Pointer(typeOffFor(v.typ(), m.Mtyp)));
    }
    return (rcvrtype, t, fn);
}

// v is a method receiver. Store at p the word which is used to
// encode that receiver at the start of the argument list.
// Reflect uses the "interface" calling convention for
// methods, which always uses one word to record the receiver.
internal static void storeRcvr(ΔValue v, @unsafe.Pointer p) {
    var t = v.typ();
    if (t.Kind() == abi.Interface){
        // the interface data word becomes the receiver word
        var iface = (ж<nonEmptyInterface>)(uintptr)(v.ptr);
        ((ж<@unsafe.Pointer>)(uintptr)(p)).val = iface.val.word;
    } else 
    if ((flag)(v.flag & flagIndir) != 0 && !t.IfaceIndir()){
        ((ж<@unsafe.Pointer>)(uintptr)(p)).val = ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).val;
    } else {
        ((ж<@unsafe.Pointer>)(uintptr)(p)).val = v.ptr;
    }
}

// align returns the result of rounding x up to a multiple of n.
// n must be a power of two.
internal static uintptr align(uintptr x, uintptr n) {
    return (uintptr)((x + n - 1) & ~(n - 1));
}

// callMethod is the call implementation used by a function returned
// by makeMethodValue (used by v.Method(i).Interface()).
// It is a streamlined version of the usual reflect call: the caller has
// already laid out the argument frame for us, so we don't have
// to deal with individual Values for each argument.
// It is in this file so that it can be next to the two similar functions above.
// The remainder of the makeMethodValue implementation is in makefunc.go.
//
// NOTE: This function must be marked as a "wrapper" in the generated code,
// so that the linker can make it work correctly for panic and recover.
// The gc compilers know to do that for the name "reflect.callMethod".
//
// ctxt is the "closure" generated by makeMethodValue.
// frame is a pointer to the arguments to that closure on the stack.
// retValid points to a boolean which should be set when the results
// section of frame is set.
//
// regs contains the argument values passed in registers and will contain
// the values returned from ctxt.fn in registers.
internal static void callMethod(ж<methodValue> Ꮡctxt, @unsafe.Pointer frame, ж<bool> ᏑretValid, ж<abi.RegArgs> Ꮡregs) {
    ref var ctxt = ref Ꮡctxt.val;
    ref var retValid = ref ᏑretValid.val;
    ref var regs = ref Ꮡregs.val;

    var rcvr = ctxt.rcvr;
    var (rcvrType, valueFuncType, methodFn) = methodReceiver("call"u8, rcvr, ctxt.method);
    // There are two ABIs at play here.
    //
    // methodValueCall was invoked with the ABI assuming there was no
    // receiver ("value ABI") and that's what frame and regs are holding.
    //
    // Meanwhile, we need to actually call the method with a receiver, which
    // has its own ABI ("method ABI"). Everything that follows is a translation
    // between the two.
    var (_, _, valueABI) = funcLayout(valueFuncType, nil);
    @unsafe.Pointer valueFrame = frame;
    var valueRegs = regs;
    var (methodFrameType, methodFramePool, methodABI) = funcLayout(valueFuncType, rcvrType);
    // Make a new frame that is one word bigger so we can store the receiver.
    // This space is used for both arguments and return values.
    @unsafe.Pointer methodFrame = methodFramePool.Get()._<@unsafe.Pointer>();
    ref var methodRegs = ref heap(new @internal.abi_package.RegArgs(), out var ᏑmethodRegs);
    // Deal with the receiver. It's guaranteed to only be one word in size.
    {
        var st = methodABI.call.steps[0];
        var exprᴛ1 = st.kind;
        var matchᴛ1 = false;
        if (exprᴛ1 == abiStepStack) { matchᴛ1 = true;
            storeRcvr(rcvr, // Only copy the receiver to the stack if the ABI says so.
 // Otherwise, it'll be in a register already.
 methodFrame);
        }
        else if (exprᴛ1 == abiStepPointer) { matchᴛ1 = true;
            storeRcvr(rcvr, // Put the receiver in a register.
 ((@unsafe.Pointer)(ᏑmethodRegs.Ptrs.at<@unsafe.Pointer>(st.ireg))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == abiStepIntReg) { matchᴛ1 = true;
            storeRcvr(rcvr, ((@unsafe.Pointer)(ᏑmethodRegs.Ints.at<uintptr>(st.ireg))));
        }
        else if (exprᴛ1 == abiStepFloatReg) {
            storeRcvr(rcvr, new @unsafe.Pointer(ᏑmethodRegs.Floats.at<uint64>(st.freg)));
        }
        else { /* default: */
            throw panic("unknown ABI parameter kind");
        }
    }

    // Translate the rest of the arguments.
    foreach (var (i, t) in valueFuncType.InSlice()) {
        var valueSteps = valueABI.call.stepsForValue(i);
        var methodSteps = methodABI.call.stepsForValue(i + 1);
        // Zero-sized types are trivial: nothing to do.
        if (len(valueSteps) == 0) {
            if (len(methodSteps) != 0) {
                throw panic("method ABI and value ABI do not align");
            }
            continue;
        }
        // There are four cases to handle in translating each
        // argument:
        // 1. Stack -> stack translation.
        // 2. Stack -> registers translation.
        // 3. Registers -> stack translation.
        // 4. Registers -> registers translation.
        // If the value ABI passes the value on the stack,
        // then the method ABI does too, because it has strictly
        // fewer arguments. Simply copy between the two.
        {
            var vStep = valueSteps[0]; if (vStep.kind == abiStepStack) {
                var mStep = methodSteps[0];
                // Handle stack -> stack translation.
                if (mStep.kind == abiStepStack) {
                    if (vStep.size != mStep.size) {
                        throw panic("method ABI and value ABI do not align");
                    }
                    typedmemmove(t,
                        (uintptr)add(methodFrame, mStep.stkOff, "precomputed stack offset"u8),
                        (uintptr)add(valueFrame, vStep.stkOff, "precomputed stack offset"u8));
                    continue;
                }
                // Handle stack -> register translation.
                foreach (var (_, mStepΔ1) in methodSteps) {
                    @unsafe.Pointer from = (uintptr)add(valueFrame, vStep.stkOff + mStepΔ1.offset, "precomputed stack offset"u8);
                    var exprᴛ2 = mStepΔ1.kind;
                    var matchᴛ2 = false;
                    if (exprᴛ2 == abiStepPointer) { matchᴛ2 = true;
                        methodRegs.Ptrs[mStepΔ1.ireg] = ~(ж<@unsafe.Pointer>)(uintptr)(from);
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ2 && exprᴛ2 == abiStepIntReg) { matchᴛ2 = true;
                        intToReg(ᏑmethodRegs, // Do the pointer copy directly so we get a write barrier.
 // We need to make sure this ends up in Ints, too.
 mStepΔ1.ireg, mStepΔ1.size, from);
                    }
                    else if (exprᴛ2 == abiStepFloatReg) {
                        floatToReg(ᏑmethodRegs, mStepΔ1.freg, mStepΔ1.size, from);
                    }
                    else { /* default: */
                        throw panic("unexpected method step");
                    }

                }
                continue;
            }
        }
        // Handle register -> stack translation.
        {
            var mStep = methodSteps[0]; if (mStep.kind == abiStepStack) {
                foreach (var (_, vStep) in valueSteps) {
                    @unsafe.Pointer to = (uintptr)add(methodFrame, mStep.stkOff + vStep.offset, "precomputed stack offset"u8);
                    var exprᴛ3 = vStep.kind;
                    if (exprᴛ3 == abiStepPointer) {
                        ((ж<@unsafe.Pointer>)(uintptr)(to)).val = (~valueRegs).Ptrs[vStep.ireg];
                    }
                    else if (exprᴛ3 == abiStepIntReg) {
                        intFromReg(valueRegs, // Do the pointer copy directly so we get a write barrier.
 vStep.ireg, vStep.size, to);
                    }
                    else if (exprᴛ3 == abiStepFloatReg) {
                        floatFromReg(valueRegs, vStep.freg, vStep.size, to);
                    }
                    else { /* default: */
                        throw panic("unexpected value step");
                    }

                }
                continue;
            }
        }
        // Handle register -> register translation.
        if (len(valueSteps) != len(methodSteps)) {
            // Because it's the same type for the value, and it's assigned
            // to registers both times, it should always take up the same
            // number of registers for each ABI.
            throw panic("method ABI and value ABI don't align");
        }
        foreach (var (iΔ1, vStep) in valueSteps) {
            var mStep = methodSteps[iΔ1];
            if (mStep.kind != vStep.kind) {
                throw panic("method ABI and value ABI don't align");
            }
            var exprᴛ4 = vStep.kind;
            var matchᴛ3 = false;
            if (exprᴛ4 == abiStepPointer) { matchᴛ3 = true;
                methodRegs.Ptrs[mStep.ireg] = (~valueRegs).Ptrs[vStep.ireg];
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ3 && exprᴛ4 == abiStepIntReg) { matchᴛ3 = true;
                methodRegs.Ints[mStep.ireg] = (~valueRegs).Ints[vStep.ireg];
            }
            else if (exprᴛ4 == abiStepFloatReg) {
                methodRegs.Floats[mStep.freg] = (~valueRegs).Floats[vStep.freg];
            }
            else { /* default: */
                throw panic("unexpected value step");
            }

        }
    }
    // Copy this too, so we get a write barrier.
    var methodFrameSize = methodFrameType.Size();
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    methodFrameSize = align(methodFrameSize, goarch.PtrSize);
    methodFrameSize += methodABI.spill;
    // Mark pointers in registers for the return path.
    methodRegs.ReturnIsPtr = methodABI.outRegPtrs;
    // Call.
    // Call copies the arguments from scratch to the stack, calls fn,
    // and then copies the results back into scratch.
    call(methodFrameType, methodFn, methodFrame, ((uint32)methodFrameType.Size()), ((uint32)methodABI.retOffset), ((uint32)methodFrameSize), ᏑmethodRegs);
    // Copy return values.
    //
    // This is somewhat simpler because both ABIs have an identical
    // return value ABI (the types are identical). As a result, register
    // results can simply be copied over. Stack-allocated values are laid
    // out the same, but are at different offsets from the start of the frame
    // Ignore any changes to args.
    // Avoid constructing out-of-bounds pointers if there are no return values.
    // because the arguments may be laid out differently.
    if (valueRegs != nil) {
        valueRegs.val = methodRegs;
    }
    {
        var retSize = methodFrameType.Size() - methodABI.retOffset; if (retSize > 0) {
            @unsafe.Pointer valueRet = (uintptr)add(valueFrame, valueABI.retOffset, "valueFrame's size > retOffset"u8);
            @unsafe.Pointer methodRet = (uintptr)add(methodFrame, methodABI.retOffset, "methodFrame's size > retOffset"u8);
            // This copies to the stack. Write barriers are not needed.
            memmove(valueRet, methodRet, retSize);
        }
    }
    // Tell the runtime it can now depend on the return values
    // being properly initialized.
    retValid = true;
    // Clear the scratch space and put it back in the pool.
    // This must happen after the statement above, so that the return
    // values will always be scanned by someone.
    typedmemclr(methodFrameType, methodFrame);
    methodFramePool.Put(methodFrame);
    // See the comment in callReflect.
    runtime.KeepAlive(ctxt);
    // Keep valueRegs alive because it may hold live pointer results.
    // The caller (methodValueCall) has it as a stack object, which is only
    // scanned when there is a reference to it.
    runtime.KeepAlive(valueRegs);
}

// funcName returns the name of f, for use in error messages.
internal static @string funcName(Func<slice<ΔValue>, slice<reflect.Value>> f) {
    var pc = ~(ж<uintptr>)(uintptr)(new @unsafe.Pointer(Ꮡ(f)));
    var rf = runtime.FuncForPC(pc);
    if (rf != nil) {
        return rf.Name();
    }
    return "closure"u8;
}

// Cap returns v's capacity.
// It panics if v's Kind is not [Array], [Chan], [Slice] or pointer to [Array].
public static nint Cap(this ΔValue v) {
    // capNonSlice is split out to keep Cap inlineable for slice kinds.
    if (v.kind() == ΔSlice) {
        return ((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val.Cap;
    }
    return v.capNonSlice();
}

internal static nint capNonSlice(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Array) {
        return v.typ().Len();
    }
    if (exprᴛ1 == Chan) {
        return chancap((uintptr)v.pointer());
    }
    if (exprᴛ1 == Ptr) {
        if (v.typ().Elem().Kind() == abi.Array) {
            return v.typ().Elem().Len();
        }
        throw panic("reflect: call of reflect.Value.Cap on ptr to non-array Value");
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Cap", v.kind())));
}

// Close closes the channel v.
// It panics if v's Kind is not [Chan] or
// v is a receive-only channel.
public static void Close(this ΔValue v) {
    v.mustBe(Chan);
    v.mustBeExported();
    var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    if ((ΔChanDir)(((ΔChanDir)(~tt).Dir) & SendDir) == 0) {
        throw panic("reflect: close of receive-only channel");
    }
    chanclose((uintptr)v.pointer());
}

// CanComplex reports whether [Value.Complex] can be used without panicking.
public static bool CanComplex(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// Complex returns v's underlying value, as a complex128.
// It panics if v's Kind is not [Complex64] or [Complex128]
public static complex128 Complex(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Complex64) {
        return ((complex128)(~(ж<complex64>)(uintptr)(v.ptr)));
    }
    if (exprᴛ1 == Complex128) {
        return ~(ж<complex128>)(uintptr)(v.ptr);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Complex", v.kind())));
}

[GoType("dyn")] partial interface Elem_type {
    void M();
}

// Elem returns the value that the interface v contains
// or that the pointer v points to.
// It panics if v's Kind is not [Interface] or [Pointer].
// It returns the zero Value if v is nil.
public static ΔValue Elem(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInterface) {
        any eface = default!;
        if (v.typ().NumMethod() == 0){
            eface = ~(ж<any>)(uintptr)(v.ptr);
        } else {
            eface = ((any)((ж<Elem_type>)(uintptr)(v.ptr).val));
        }
        var x = unpackEface(eface);
        if (x.flag != 0) {
            x.flag |= (flag)(v.flag.ro());
        }
        return x;
    }
    if (exprᴛ1 == ΔPointer) {
        @unsafe.Pointer ptr = v.ptr;
        if ((flag)(v.flag & flagIndir) != 0) {
            if (v.typ().IfaceIndir()) {
                // This is a pointer to a not-in-heap object. ptr points to a uintptr
                // in the heap. That uintptr is the address of a not-in-heap object.
                // In general, pointers to not-in-heap objects can be total junk.
                // But Elem() is asking to dereference it, so the user has asserted
                // that at least it is a valid pointer (not just an integer stored in
                // a pointer slot). So let's check, to make sure that it isn't a pointer
                // that the runtime will crash on if it sees it during GC or write barriers.
                // Since it is a not-in-heap pointer, all pointers to the heap are
                // forbidden! That makes the test pretty easy.
                // See issue 48399.
                if (!verifyNotInHeapPtr(~(ж<uintptr>)(uintptr)(ptr))) {
                    throw panic("reflect: reflect.Value.Elem on an invalid notinheap pointer");
                }
            }
            ptr = ~(ж<@unsafe.Pointer>)(uintptr)(ptr);
        }
        if (ptr == nil) {
            // The returned value's address is v's value.
            return new ΔValue(nil);
        }
        var tt = (ж<ptrType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        var typ = tt.Elem;
        var fl = (flag)((flag)((flag)(v.flag & flagRO) | flagIndir) | flagAddr);
        fl |= (flag)(((flag)typ.Kind()));
        return new ΔValue(typ, ptr.val, fl);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Elem", v.kind())));
}

// Field returns the i'th field of the struct v.
// It panics if v's Kind is not [Struct] or i is out of range.
public static ΔValue Field(this ΔValue v, nint i) {
    if (v.kind() != Struct) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Field", v.kind())));
    }
    var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    if (((nuint)i) >= ((nuint)len(tt.Fields))) {
        throw panic("reflect: Field index out of range");
    }
    var field = Ꮡ(tt.Fields, i);
    var typ = field.val.Typ;
    // Inherit permission bits from v, but clear flagEmbedRO.
    var fl = (flag)((flag)(v.flag & ((flag)((flag)(flagStickyRO | flagIndir) | flagAddr))) | ((flag)typ.Kind()));
    // Using an unexported field forces flagRO.
    if (!(~field).Name.IsExported()) {
        if (field.Embedded()){
            fl |= (flag)(flagEmbedRO);
        } else {
            fl |= (flag)(flagStickyRO);
        }
    }
    // Either flagIndir is set and v.ptr points at struct,
    // or flagIndir is not set and v.ptr is the actual struct data.
    // In the former case, we want v.ptr + offset.
    // In the latter case, we must have field.offset = 0,
    // so v.ptr + field.offset is still the correct address.
    @unsafe.Pointer ptr = (uintptr)add(v.ptr, (~field).Offset, "same as non-reflect &v.field"u8);
    return new ΔValue(typ, ptr.val, fl);
}

// FieldByIndex returns the nested field corresponding to index.
// It panics if evaluation requires stepping through a nil
// pointer or a field that is not a struct.
public static ΔValue FieldByIndex(this ΔValue v, slice<nint> index) {
    if (len(index) == 1) {
        return v.Field(index[0]);
    }
    v.mustBe(Struct);
    foreach (var (i, x) in index) {
        if (i > 0) {
            if (v.Kind() == ΔPointer && v.typ().Elem().Kind() == abi.Struct) {
                if (v.IsNil()) {
                    throw panic("reflect: indirection through nil pointer to embedded struct");
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }
    return v;
}

// FieldByIndexErr returns the nested field corresponding to index.
// It returns an error if evaluation requires stepping through a nil
// pointer, but panics if it must step through a field that
// is not a struct.
public static (ΔValue, error) FieldByIndexErr(this ΔValue v, slice<nint> index) {
    if (len(index) == 1) {
        return (v.Field(index[0]), default!);
    }
    v.mustBe(Struct);
    foreach (var (i, x) in index) {
        if (i > 0) {
            if (v.Kind() == Ptr && v.typ().Elem().Kind() == abi.Struct) {
                if (v.IsNil()) {
                    return (new ΔValue(nil), errors.New("reflect: indirection through nil pointer to embedded struct field "u8 + nameFor(v.typ().Elem())));
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }
    return (v, default!);
}

// FieldByName returns the struct field with the given name.
// It returns the zero Value if no field was found.
// It panics if v's Kind is not [Struct].
public static ΔValue FieldByName(this ΔValue v, @string name) {
    v.mustBe(Struct);
    {
        var (f, ok) = toRType(v.typ()).FieldByName(name); if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new ΔValue(nil);
}

// FieldByNameFunc returns the struct field with a name
// that satisfies the match function.
// It panics if v's Kind is not [Struct].
// It returns the zero Value if no field was found.
public static ΔValue FieldByNameFunc(this ΔValue v, Func<@string, bool> match) {
    {
        var (f, ok) = toRType(v.typ()).FieldByNameFunc(match); if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new ΔValue(nil);
}

// CanFloat reports whether [Value.Float] can be used without panicking.
public static bool CanFloat(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// Float returns v's underlying value, as a float64.
// It panics if v's Kind is not [Float32] or [Float64]
public static float64 Float(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Float32) {
        return ((float64)(~(ж<float32>)(uintptr)(v.ptr)));
    }
    if (exprᴛ1 == Float64) {
        return ~(ж<float64>)(uintptr)(v.ptr);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Float", v.kind())));
}

internal static ж<abi.Type> uint8Type = rtypeOf(((uint8)0));

// Index returns v's i'th element.
// It panics if v's Kind is not [Array], [Slice], or [String] or i is out of range.
public static ΔValue Index(this ΔValue v, nint i) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        if (((nuint)i) >= ((nuint)(~tt).Len)) {
            throw panic("reflect: array index out of range");
        }
        var typ = tt.val.Elem;
        var offset = ((uintptr)i) * typ.Size();
        @unsafe.Pointer val = (uintptr)add(v.ptr, // Either flagIndir is set and v.ptr points at array,
 // or flagIndir is not set and v.ptr is the actual array data.
 // In the former case, we want v.ptr + offset.
 // In the latter case, we must be doing Index(0), so offset = 0,
 // so v.ptr + offset is still the correct address.
 offset, "same as &v[i], i < tt.len"u8);
        var fl = (flag)((flag)((flag)(v.flag & ((flag)(flagIndir | flagAddr))) | v.flag.ro()) | ((flag)typ.Kind()));
        return new ΔValue( // bits same as overall array
typ, val.val, fl);
    }
    if (exprᴛ1 == ΔSlice) {
        var s = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
        if (((nuint)i) >= ((nuint)(~s).Len)) {
            // Element flag same as Elem of Pointer.
            // Addressable, indirect, possibly read-only.
            throw panic("reflect: slice index out of range");
        }
        var tt = (ж<sliceType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        var typ = tt.Elem;
        @unsafe.Pointer val = (uintptr)arrayAt((~s).Data, i, typ.Size(), "i < s.Len"u8);
        var fl = (flag)((flag)((flag)(flagAddr | flagIndir) | v.flag.ro()) | ((flag)typ.Kind()));
        return new ΔValue(typ, val.val, fl);
    }
    if (exprᴛ1 == ΔString) {
        var s = (ж<unsafeheader.String>)(uintptr)(v.ptr);
        if (((nuint)i) >= ((nuint)(~s).Len)) {
            throw panic("reflect: string index out of range");
        }
        @unsafe.Pointer p = (uintptr)arrayAt((~s).Data, i, 1, "i < s.Len"u8);
        var fl = (flag)((flag)(v.flag.ro() | ((flag)Uint8)) | flagIndir);
        return new ΔValue(uint8Type, p.val, fl);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Index", v.kind())));
}

// CanInt reports whether Int can be used without panicking.
public static bool CanInt(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// Int returns v's underlying value, as an int64.
// It panics if v's Kind is not [Int], [Int8], [Int16], [Int32], or [Int64].
public static int64 Int(this ΔValue v) {
    ΔKind k = v.kind();
    @unsafe.Pointer p = v.ptr;
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInt) {
        return ((int64)(~(ж<nint>)(uintptr)(p)));
    }
    if (exprᴛ1 == Int8) {
        return ((int64)(~(ж<int8>)(uintptr)(p)));
    }
    if (exprᴛ1 == Int16) {
        return ((int64)(~(ж<int16>)(uintptr)(p)));
    }
    if (exprᴛ1 == Int32) {
        return ((int64)(~(ж<int32>)(uintptr)(p)));
    }
    if (exprᴛ1 == Int64) {
        return ~(ж<int64>)(uintptr)(p);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Int", v.kind())));
}

// CanInterface reports whether [Value.Interface] can be used without panicking.
public static bool CanInterface(this ΔValue v) {
    if (v.flag == 0) {
        throw panic(Ꮡ(new ValueError("reflect.Value.CanInterface", Invalid)));
    }
    return (flag)(v.flag & flagRO) == 0;
}

// Interface returns v's current value as an interface{}.
// It is equivalent to:
//
//	var i interface{} = (v's underlying value)
//
// It panics if the Value was obtained by accessing
// unexported struct fields.
public static any /*i*/ Interface(this ΔValue v) {
    any i = default!;

    return valueInterface(v, true);
}

[GoType("dyn")] partial interface valueInterface_type {
    void M();
}

internal static any valueInterface(ΔValue v, bool safe) {
    if (v.flag == 0) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Interface", Invalid)));
    }
    if (safe && (flag)(v.flag & flagRO) != 0) {
        // Do not allow access to unexported values via Interface,
        // because they might be pointers that should not be
        // writable or methods or function that should not be callable.
        throw panic("reflect.Value.Interface: cannot return value obtained from unexported field or method");
    }
    if ((flag)(v.flag & flagMethod) != 0) {
        v = makeMethodValue("Interface"u8, v);
    }
    if (v.kind() == ΔInterface) {
        // Special case: return the element inside the interface.
        // Empty interface has one layout, all interfaces with
        // methods have a second layout.
        if (v.NumMethod() == 0) {
            return ~(ж<any>)(uintptr)(v.ptr);
        }
        return (ж<valueInterface_type>)(uintptr)(v.ptr).val;
    }
    return packEface(v);
}

// InterfaceData returns a pair of unspecified uintptr values.
// It panics if v's Kind is not Interface.
//
// In earlier versions of Go, this function returned the interface's
// value as a uintptr pair. As of Go 1.4, the implementation of
// interface values precludes any defined use of InterfaceData.
//
// Deprecated: The memory representation of interface values is not
// compatible with InterfaceData.
public static array<uintptr> InterfaceData(this ΔValue v) {
    v.mustBe(ΔInterface);
    // The compiler loses track as it converts to uintptr. Force escape.
    escapes(v.ptr);
    // We treat this as a read operation, so we allow
    // it even for unexported data, because the caller
    // has to import "unsafe" to turn it into something
    // that can be abused.
    // Interface value is always bigger than a word; assume flagIndir.
    return ~(ж<array<uintptr>>)(uintptr)(v.ptr);
}

// IsNil reports whether its argument v is nil. The argument must be
// a chan, func, interface, map, pointer, or slice value; if it is
// not, IsNil panics. Note that IsNil is not always equivalent to a
// regular comparison with nil in Go. For example, if v was created
// by calling [ValueOf] with an uninitialized interface variable i,
// i==nil will be true but v.IsNil will panic as v will be the zero
// Value.
public static bool IsNil(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Chan || exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        if ((flag)(v.flag & flagMethod) != 0) {
            return false;
        }
        @unsafe.Pointer ptr = v.ptr;
        if ((flag)(v.flag & flagIndir) != 0) {
            ptr = ~(ж<@unsafe.Pointer>)(uintptr)(ptr);
        }
        return ptr == nil;
    }
    if (exprᴛ1 == ΔInterface || exprᴛ1 == ΔSlice) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr) == nil;
    }

    // Both interface and slice are nil if first word is 0.
    // Both are always bigger than a word; assume flagIndir.
    throw panic(Ꮡ(new ValueError("reflect.Value.IsNil", v.kind())));
}

// IsValid reports whether v represents a value.
// It returns false if v is the zero Value.
// If [Value.IsValid] returns false, all other methods except String panic.
// Most functions and methods never return an invalid Value.
// If one does, its documentation states the conditions explicitly.
public static bool IsValid(this ΔValue v) {
    return v.flag != 0;
}

// IsZero reports whether v is the zero value for its type.
// It panics if the argument is invalid.
public static bool IsZero(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔBool) {
        return !v.Bool();
    }
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return v.Int() == 0;
    }
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return v.Uint() == 0;
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return v.Float() == 0;
    }
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        return v.Complex() == 0;
    }
    if (exprᴛ1 == Array) {
        if ((flag)(v.flag & flagIndir) == 0) {
            return v.ptr == nil;
        }
        var typ = (ж<abiꓸArrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        if (typ.Equal != default! && typ.Size() <= abi.ZeroValSize) {
            // If the type is comparable, then compare directly with zero.
            // v.ptr doesn't escape, as Equal functions are compiler generated
            // and never escape. The escape analysis doesn't know, as it is a
            // function pointer call.
            return typ.Equal((uintptr)abi.NoEscape(v.ptr), new @unsafe.Pointer(ᏑzeroVal.at<byte>(0)));
        }
        if ((abi.TFlag)(typ.TFlag & abi.TFlagRegularMemory) != 0) {
            // For some types where the zero value is a value where all bits of this type are 0
            // optimize it.
            return isZero(@unsafe.Slice(((ж<byte>)(uintptr)(v.ptr)), typ.Size()));
        }
        nint n = ((nint)(~typ).Len);
        for (nint i = 0; i < n; i++) {
            if (!v.Index(i).IsZero()) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == Chan || exprᴛ1 == Func || exprᴛ1 == ΔInterface || exprᴛ1 == Map || exprᴛ1 == ΔPointer || exprᴛ1 == ΔSlice || exprᴛ1 == ΔUnsafePointer) {
        return v.IsNil();
    }
    if (exprᴛ1 == ΔString) {
        return v.Len() == 0;
    }
    if (exprᴛ1 == Struct) {
        if ((flag)(v.flag & flagIndir) == 0) {
            return v.ptr == nil;
        }
        var typ = (ж<abiꓸStructType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        if (typ.Equal != default! && typ.Size() <= abi.ZeroValSize) {
            // If the type is comparable, then compare directly with zero.
            // See noescape justification above.
            return typ.Equal((uintptr)abi.NoEscape(v.ptr), new @unsafe.Pointer(ᏑzeroVal.at<byte>(0)));
        }
        if ((abi.TFlag)(typ.TFlag & abi.TFlagRegularMemory) != 0) {
            // For some types where the zero value is a value where all bits of this type are 0
            // optimize it.
            return isZero(@unsafe.Slice(((ж<byte>)(uintptr)(v.ptr)), typ.Size()));
        }
        nint n = v.NumField();
        for (nint i = 0; i < n; i++) {
            if (!v.Field(i).IsZero() && v.Type().Field(i).Name != "_"u8) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        throw panic(Ꮡ(new ValueError( // This should never happen, but will act as a safeguard for later,
 // as a default value doesn't makes sense here.
"reflect.Value.IsZero", v.Kind())));
    }

}

// isZero For all zeros, performance is not as good as
// return bytealg.Count(b, byte(0)) == len(b)
internal static bool isZero(slice<byte> b) {
    if (len(b) == 0) {
        return true;
    }
    static readonly UntypedInt n = 32;
    // Align memory addresses to 8 bytes.
    while (((uintptr)new @unsafe.Pointer(Ꮡ(b, 0))) % 8 != 0) {
        if (b[0] != 0) {
            return false;
        }
        b = b[1..];
        if (len(b) == 0) {
            return true;
        }
    }
    while (len(b) % 8 != 0) {
        if (b[len(b) - 1] != 0) {
            return false;
        }
        b = b[..(int)(len(b) - 1)];
    }
    if (len(b) == 0) {
        return true;
    }
    var w = @unsafe.Slice((ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0))), len(b) / 8);
    while (len(w) % n != 0) {
        if (w[0] != 0) {
            return false;
        }
        w = w[1..];
    }
    while (len(w) >= n) {
        if (w[0] != 0 || w[1] != 0 || w[2] != 0 || w[3] != 0 || w[4] != 0 || w[5] != 0 || w[6] != 0 || w[7] != 0 || w[8] != 0 || w[9] != 0 || w[10] != 0 || w[11] != 0 || w[12] != 0 || w[13] != 0 || w[14] != 0 || w[15] != 0 || w[16] != 0 || w[17] != 0 || w[18] != 0 || w[19] != 0 || w[20] != 0 || w[21] != 0 || w[22] != 0 || w[23] != 0 || w[24] != 0 || w[25] != 0 || w[26] != 0 || w[27] != 0 || w[28] != 0 || w[29] != 0 || w[30] != 0 || w[31] != 0) {
            return false;
        }
        w = w[(int)(n)..];
    }
    return true;
}

// SetZero sets v to be the zero value of v's type.
// It panics if [Value.CanSet] returns false.
public static void SetZero(this ΔValue v) {
    v.mustBeAssignable();
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔBool) {
        ((ж<bool>)(uintptr)(v.ptr)).val = false;
    }
    else if (exprᴛ1 == ΔInt) {
        ((ж<nint>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Int8) {
        ((ж<int8>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Int16) {
        ((ж<int16>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Int32) {
        ((ж<int32>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Int64) {
        ((ж<int64>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == ΔUint) {
        ((ж<nuint>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Uint8) {
        ((ж<uint8>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Uint16) {
        ((ж<uint16>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Uint32) {
        ((ж<uint32>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Uint64) {
        ((ж<uint64>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Uintptr) {
        ((ж<uintptr>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Float32) {
        ((ж<float32>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Float64) {
        ((ж<float64>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Complex64) {
        ((ж<complex64>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == Complex128) {
        ((ж<complex128>)(uintptr)(v.ptr)).val = 0;
    }
    else if (exprᴛ1 == ΔString) {
        ((ж<@string>)(uintptr)(v.ptr)).val = ""u8;
    }
    else if (exprᴛ1 == ΔSlice) {
        ((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val = new unsafeheader.Slice(nil);
    }
    else if (exprᴛ1 == ΔInterface) {
        ((ж<abi.EmptyInterface>)(uintptr)(v.ptr)).val = new abi.EmptyInterface(nil);
    }
    else if (exprᴛ1 == Chan || exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).val = default!;
    }
    else if (exprᴛ1 == Array || exprᴛ1 == Struct) {
        typedmemclr(v.typ(), v.ptr);
    }
    else { /* default: */
        throw panic(Ꮡ(new ValueError( // This should never happen, but will act as a safeguard for later,
 // as a default value doesn't makes sense here.
"reflect.Value.SetZero", v.Kind())));
    }

}

// Kind returns v's Kind.
// If v is the zero Value ([Value.IsValid] returns false), Kind returns Invalid.
public static ΔKind Kind(this ΔValue v) {
    return v.kind();
}

// Len returns v's length.
// It panics if v's Kind is not [Array], [Chan], [Map], [Slice], [String], or pointer to [Array].
public static nint Len(this ΔValue v) {
    // lenNonSlice is split out to keep Len inlineable for slice kinds.
    if (v.kind() == ΔSlice) {
        return ((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val.Len;
    }
    return v.lenNonSlice();
}

internal static nint lenNonSlice(this ΔValue v) {
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        if (exprᴛ1 == Array) {
            var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
            return ((nint)(~tt).Len);
        }
        if (exprᴛ1 == Chan) {
            return chanlen((uintptr)v.pointer());
        }
        if (exprᴛ1 == Map) {
            return maplen((uintptr)v.pointer());
        }
        if (exprᴛ1 == ΔString) {
            return ((ж<unsafeheader.String>)(uintptr)(v.ptr)).val.Len;
        }
        if (exprᴛ1 == Ptr) {
            if (v.typ().Elem().Kind() == abi.Array) {
                // String is bigger than a word; assume flagIndir.
                return v.typ().Elem().Len();
            }
            throw panic("reflect: call of reflect.Value.Len on ptr to non-array Value");
        }
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Len", v.kind())));
}

internal static ж<abi.Type> stringType = rtypeOf("");

// MapIndex returns the value associated with key in the map v.
// It panics if v's Kind is not [Map].
// It returns the zero Value if key is not found in the map or if v represents a nil map.
// As in Go, the key's value must be assignable to the map's key type.
public static ΔValue MapIndex(this ΔValue v, ΔValue key) {
    v.mustBe(Map);
    var tt = (ж<mapType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    // Do not require key to be exported, so that DeepEqual
    // and other programs can use all the keys returned by
    // MapKeys as arguments to MapIndex. If either the map
    // or the key is unexported, though, the result will be
    // considered unexported. This is consistent with the
    // behavior for structs, which allow read but not write
    // of unexported fields.
    @unsafe.Pointer e = default!;
    if ((tt.Key == stringType || key.kind() == ΔString) && tt.Key == key.typ() && tt.Elem.Size() <= abi.MapMaxElemBytes){
        @string kΔ1 = ~(ж<@string>)(uintptr)(key.ptr);
        e = (uintptr)mapaccess_faststr(v.typ(), (uintptr)v.pointer(), kΔ1);
    } else {
        key = key.assignTo("reflect.Value.MapIndex"u8, tt.Key, nil);
        @unsafe.Pointer k = default!;
        if ((flag)(key.flag & flagIndir) != 0){
            k = key.ptr;
        } else {
            k = ((@unsafe.Pointer)(Ꮡkey.of(Value.Ꮡptr)));
        }
        e = (uintptr)mapaccess(v.typ(), (uintptr)v.pointer(), k);
    }
    if (e == nil) {
        return new ΔValue(nil);
    }
    var typ = tt.Elem;
    var fl = ((flag)(v.flag | key.flag)).ro();
    fl |= (flag)(((flag)typ.Kind()));
    return copyVal(typ, fl, e);
}

// MapKeys returns a slice containing all the keys present in the map,
// in unspecified order.
// It panics if v's Kind is not [Map].
// It returns an empty slice if v represents a nil map.
public static slice<ΔValue> MapKeys(this ΔValue v) {
    v.mustBe(Map);
    var tt = (ж<mapType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    var keyType = tt.Key;
    var fl = (flag)(v.flag.ro() | ((flag)keyType.Kind()));
    @unsafe.Pointer m = (uintptr)v.pointer();
    nint mlen = ((nint)0);
    if (m != nil) {
        mlen = maplen(m);
    }
    ref var it = ref heap(new hiter(), out var Ꮡit);
    mapiterinit(v.typ(), m, Ꮡit);
    var a = new slice<ΔValue>(mlen);
    nint i = default!;
    for (i = 0; i < len(a); i++) {
        @unsafe.Pointer key = (uintptr)mapiterkey(Ꮡit);
        if (key == nil) {
            // Someone deleted an entry from the map since we
            // called maplen above. It's a data race, but nothing
            // we can do about it.
            break;
        }
        a[i] = copyVal(keyType, fl, key);
        mapiternext(Ꮡit);
    }
    return a[..(int)(i)];
}

// hiter's structure matches runtime.hiter's structure.
// Having a clone here allows us to embed a map iterator
// inside type MapIter so that MapIters can be re-used
// without doing any allocations.
[GoType] partial struct hiter {
    internal @unsafe.Pointer key;
    internal @unsafe.Pointer elem;
    internal @unsafe.Pointer t;
    internal @unsafe.Pointer h;
    internal @unsafe.Pointer buckets;
    internal @unsafe.Pointer bptr;
    internal ж<slice<@unsafe.Pointer>> overflow;
    internal ж<slice<@unsafe.Pointer>> oldoverflow;
    internal uintptr startBucket;
    internal uint8 offset;
    internal bool wrapped;
    public uint8 B;
    internal uint8 i;
    internal uintptr bucket;
    internal uintptr checkBucket;
}

[GoRecv] internal static bool initialized(this ref hiter h) {
    return h.t != nil;
}

// A MapIter is an iterator for ranging over a map.
// See [Value.MapRange].
[GoType] partial struct MapIter {
    internal ΔValue m;
    internal hiter hiter;
}

// Key returns the key of iter's current map entry.
[GoRecv] public static ΔValue Key(this ref MapIter iter) {
    if (!iter.hiter.initialized()) {
        throw panic("MapIter.Key called before Next");
    }
    @unsafe.Pointer iterkey = (uintptr)mapiterkey(Ꮡ(iter.hiter));
    if (iterkey == nil) {
        throw panic("MapIter.Key called on exhausted iterator");
    }
    var t = (ж<mapType>)(uintptr)(new @unsafe.Pointer(iter.m.typ()));
    var ktype = t.Key;
    return copyVal(ktype, (flag)(iter.m.flag.ro() | ((flag)ktype.Kind())), iterkey);
}

// SetIterKey assigns to v the key of iter's current map entry.
// It is equivalent to v.Set(iter.Key()), but it avoids allocating a new Value.
// As in Go, the key must be assignable to v's type and
// must not be derived from an unexported field.
public static void SetIterKey(this ΔValue v, ж<MapIter> Ꮡiter) {
    ref var iter = ref Ꮡiter.val;

    if (!iter.hiter.initialized()) {
        throw panic("reflect: Value.SetIterKey called before Next");
    }
    @unsafe.Pointer iterkey = (uintptr)mapiterkey(Ꮡ(iter.hiter));
    if (iterkey == nil) {
        throw panic("reflect: Value.SetIterKey called on exhausted iterator");
    }
    v.mustBeAssignable();
    @unsafe.Pointer target = default!;
    if (v.kind() == ΔInterface) {
        target = v.ptr;
    }
    var t = (ж<mapType>)(uintptr)(new @unsafe.Pointer(iter.m.typ()));
    var ktype = t.Key;
    iter.m.mustBeExported();
    // do not let unexported m leak
    var key = new ΔValue(ktype, iterkey.val, (flag)((flag)(iter.m.flag | ((flag)ktype.Kind())) | flagIndir));
    key = key.assignTo("reflect.MapIter.SetKey"u8, v.typ(), target);
    typedmemmove(v.typ(), v.ptr, key.ptr);
}

// Value returns the value of iter's current map entry.
[GoRecv] public static ΔValue Value(this ref MapIter iter) {
    if (!iter.hiter.initialized()) {
        throw panic("MapIter.Value called before Next");
    }
    @unsafe.Pointer iterelem = (uintptr)mapiterelem(Ꮡ(iter.hiter));
    if (iterelem == nil) {
        throw panic("MapIter.Value called on exhausted iterator");
    }
    var t = (ж<mapType>)(uintptr)(new @unsafe.Pointer(iter.m.typ()));
    var vtype = t.Elem;
    return copyVal(vtype, (flag)(iter.m.flag.ro() | ((flag)vtype.Kind())), iterelem);
}

// SetIterValue assigns to v the value of iter's current map entry.
// It is equivalent to v.Set(iter.Value()), but it avoids allocating a new Value.
// As in Go, the value must be assignable to v's type and
// must not be derived from an unexported field.
public static void SetIterValue(this ΔValue v, ж<MapIter> Ꮡiter) {
    ref var iter = ref Ꮡiter.val;

    if (!iter.hiter.initialized()) {
        throw panic("reflect: Value.SetIterValue called before Next");
    }
    @unsafe.Pointer iterelem = (uintptr)mapiterelem(Ꮡ(iter.hiter));
    if (iterelem == nil) {
        throw panic("reflect: Value.SetIterValue called on exhausted iterator");
    }
    v.mustBeAssignable();
    @unsafe.Pointer target = default!;
    if (v.kind() == ΔInterface) {
        target = v.ptr;
    }
    var t = (ж<mapType>)(uintptr)(new @unsafe.Pointer(iter.m.typ()));
    var vtype = t.Elem;
    iter.m.mustBeExported();
    // do not let unexported m leak
    var elem = new ΔValue(vtype, iterelem.val, (flag)((flag)(iter.m.flag | ((flag)vtype.Kind())) | flagIndir));
    elem = elem.assignTo("reflect.MapIter.SetValue"u8, v.typ(), target);
    typedmemmove(v.typ(), v.ptr, elem.ptr);
}

// Next advances the map iterator and reports whether there is another
// entry. It returns false when iter is exhausted; subsequent
// calls to [MapIter.Key], [MapIter.Value], or [MapIter.Next] will panic.
[GoRecv] public static bool Next(this ref MapIter iter) {
    if (!iter.m.IsValid()) {
        throw panic("MapIter.Next called on an iterator that does not have an associated map Value");
    }
    if (!iter.hiter.initialized()){
        mapiterinit(iter.m.typ(), (uintptr)iter.m.pointer(), Ꮡ(iter.hiter));
    } else {
        if ((uintptr)mapiterkey(Ꮡ(iter.hiter)) == nil) {
            throw panic("MapIter.Next called on exhausted iterator");
        }
        mapiternext(Ꮡ(iter.hiter));
    }
    return (uintptr)mapiterkey(Ꮡ(iter.hiter)) != nil;
}

// Reset modifies iter to iterate over v.
// It panics if v's Kind is not [Map] and v is not the zero Value.
// Reset(Value{}) causes iter to not to refer to any map,
// which may allow the previously iterated-over map to be garbage collected.
[GoRecv] public static void Reset(this ref MapIter iter, ΔValue v) {
    if (v.IsValid()) {
        v.mustBe(Map);
    }
    iter.m = v;
    iter.hiter = new hiter(nil);
}

// MapRange returns a range iterator for a map.
// It panics if v's Kind is not [Map].
//
// Call [MapIter.Next] to advance the iterator, and [MapIter.Key]/[MapIter.Value] to access each entry.
// [MapIter.Next] returns false when the iterator is exhausted.
// MapRange follows the same iteration semantics as a range statement.
//
// Example:
//
//	iter := reflect.ValueOf(m).MapRange()
//	for iter.Next() {
//		k := iter.Key()
//		v := iter.Value()
//		...
//	}
public static ж<MapIter> MapRange(this ΔValue v) {
    // This is inlinable to take advantage of "function outlining".
    // The allocation of MapIter can be stack allocated if the caller
    // does not allow it to escape.
    // See https://blog.filippo.io/efficient-go-apis-with-the-inliner/
    if (v.kind() != Map) {
        v.panicNotMap();
    }
    return Ꮡ(new MapIter(m: v));
}

// Force slow panicking path not inlined, so it won't add to the
// inlining budget of the caller.
// TODO: undo when the inliner is no longer bottom-up only.
//
//go:noinline
internal static void panicNotMap(this flag f) {
    f.mustBe(Map);
}

// copyVal returns a Value containing the map key or value at ptr,
// allocating a new variable as needed.
internal static ΔValue copyVal(ж<abi.Type> Ꮡtyp, flag fl, @unsafe.Pointer ptr) {
    ref var typ = ref Ꮡtyp.val;

    if (typ.IfaceIndir()) {
        // Copy result so future changes to the map
        // won't change the underlying value.
        @unsafe.Pointer c = (uintptr)unsafe_New(Ꮡtyp);
        typedmemmove(Ꮡtyp, c, ptr.val);
        return new ΔValue(Ꮡtyp, c.val, (flag)(fl | flagIndir));
    }
    return new ΔValue(Ꮡtyp, ~(ж<@unsafe.Pointer>)(uintptr)(ptr), fl);
}

// Method returns a function value corresponding to v's i'th method.
// The arguments to a Call on the returned function should not include
// a receiver; the returned function will always use v as the receiver.
// Method panics if i is out of range or if v is a nil interface value.
public static ΔValue Method(this ΔValue v, nint i) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Method", Invalid)));
    }
    if ((flag)(v.flag & flagMethod) != 0 || ((nuint)i) >= ((nuint)toRType(v.typ()).NumMethod())) {
        throw panic("reflect: Method index out of range");
    }
    if (v.typ().Kind() == abi.Interface && v.IsNil()) {
        throw panic("reflect: Method on nil interface value");
    }
    var fl = (flag)(v.flag.ro() | ((flag)(v.flag & flagIndir)));
    fl |= (flag)(((flag)Func));
    fl |= (flag)((flag)(((flag)i) << (int)(flagMethodShift) | flagMethod));
    return new ΔValue(v.typ(), v.ptr, fl);
}

// NumMethod returns the number of methods in the value's method set.
//
// For a non-interface type, it returns the number of exported methods.
//
// For an interface type, it returns the number of exported and unexported methods.
public static nint NumMethod(this ΔValue v) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.NumMethod", Invalid)));
    }
    if ((flag)(v.flag & flagMethod) != 0) {
        return 0;
    }
    return toRType(v.typ()).NumMethod();
}

// MethodByName returns a function value corresponding to the method
// of v with the given name.
// The arguments to a Call on the returned function should not include
// a receiver; the returned function will always use v as the receiver.
// It returns the zero Value if no method was found.
public static ΔValue MethodByName(this ΔValue v, @string name) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.MethodByName", Invalid)));
    }
    if ((flag)(v.flag & flagMethod) != 0) {
        return new ΔValue(nil);
    }
    var (m, ok) = toRType(v.typ()).MethodByName(name);
    if (!ok) {
        return new ΔValue(nil);
    }
    return v.Method(m.Index);
}

// NumField returns the number of fields in the struct v.
// It panics if v's Kind is not [Struct].
public static nint NumField(this ΔValue v) {
    v.mustBe(Struct);
    var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    return len(tt.Fields);
}

// OverflowComplex reports whether the complex128 x cannot be represented by v's type.
// It panics if v's Kind is not [Complex64] or [Complex128].
public static bool OverflowComplex(this ΔValue v, complex128 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Complex64) {
        return overflowFloat32(real(x)) || overflowFloat32(imag(x));
    }
    if (exprᴛ1 == Complex128) {
        return false;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowComplex", v.kind())));
}

// OverflowFloat reports whether the float64 x cannot be represented by v's type.
// It panics if v's Kind is not [Float32] or [Float64].
public static bool OverflowFloat(this ΔValue v, float64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Float32) {
        return overflowFloat32(x);
    }
    if (exprᴛ1 == Float64) {
        return false;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowFloat", v.kind())));
}

internal static bool overflowFloat32(float64 x) {
    if (x < 0) {
        x = -x;
    }
    return math.MaxFloat32 < x && x <= math.MaxFloat64;
}

// OverflowInt reports whether the int64 x cannot be represented by v's type.
// It panics if v's Kind is not [Int], [Int8], [Int16], [Int32], or [Int64].
public static bool OverflowInt(this ΔValue v, int64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var bitSize = v.typ().Size() * 8;
        var trunc = (x << (int)((64 - bitSize))) >> (int)((64 - bitSize));
        return x != trunc;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowInt", v.kind())));
}

// OverflowUint reports whether the uint64 x cannot be represented by v's type.
// It panics if v's Kind is not [Uint], [Uintptr], [Uint8], [Uint16], [Uint32], or [Uint64].
public static bool OverflowUint(this ΔValue v, uint64 x) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64) {
        var bitSize = v.typ_.Size() * 8;
        var trunc = (x << (int)((64 - bitSize))) >> (int)((64 - bitSize));
        return x != trunc;
    }

    // ok to use v.typ_ directly as Size doesn't escape
    throw panic(Ꮡ(new ValueError("reflect.Value.OverflowUint", v.kind())));
}

//go:nocheckptr
// This prevents inlining Value.Pointer when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.Pointer())
// and make an exception.

// Pointer returns v's value as a uintptr.
// It panics if v's Kind is not [Chan], [Func], [Map], [Pointer], [Slice], [String], or [UnsafePointer].
//
// If v's Kind is [Func], the returned pointer is an underlying
// code pointer, but not necessarily enough to identify a
// single function uniquely. The only guarantee is that the
// result is zero if and only if v is a nil func Value.
//
// If v's Kind is [Slice], the returned pointer is to the first
// element of the slice. If the slice is nil the returned value
// is 0.  If the slice is empty but non-nil the return value is non-zero.
//
// If v's Kind is [String], the returned pointer is to the first
// element of the underlying bytes of string.
//
// It's preferred to use uintptr(Value.UnsafePointer()) to get the equivalent result.
public static uintptr Pointer(this ΔValue v) {
    // The compiler loses track as it converts to uintptr. Force escape.
    escapes(v.ptr);
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    var matchᴛ1 = false;
    if (exprᴛ1 == ΔPointer) { matchᴛ1 = true;
        if (!v.typ().Pointers()) {
            var val = ~(ж<uintptr>)(uintptr)(v.ptr);
            // Since it is a not-in-heap pointer, all pointers to the heap are
            // forbidden! See comment in Value.Elem and issue #48399.
            if (!verifyNotInHeapPtr(val)) {
                throw panic("reflect: reflect.Value.Pointer on an invalid notinheap pointer");
            }
            return val;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && (exprᴛ1 == Chan || exprᴛ1 == Map || exprᴛ1 == ΔUnsafePointer)) { matchᴛ1 = true;
        return ((uintptr)(uintptr)v.pointer());
    }
    if (exprᴛ1 == Func) { matchᴛ1 = true;
        if ((flag)(v.flag & flagMethod) != 0) {
            // As the doc comment says, the returned pointer is an
            // underlying code pointer but not necessarily enough to
            // identify a single function uniquely. All method expressions
            // created via reflect have the same underlying code pointer,
            // so their Pointers are equal. The function used here must
            // match the one used in makeMethodValue.
            return methodValueCallCodePtr();
        }
        @unsafe.Pointer p = (uintptr)v.pointer();
        if (p != nil) {
            // Non-nil func value points at data block.
            // First word of data block is actual code.
            p = ~(ж<@unsafe.Pointer>)(uintptr)(p);
        }
        return ((uintptr)p);
    }
    if (exprᴛ1 == ΔSlice) {
        return ((uintptr)((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val.Data);
    }
    if (exprᴛ1 == ΔString) { matchᴛ1 = true;
        return ((uintptr)((ж<unsafeheader.String>)(uintptr)(v.ptr)).val.Data);
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Pointer", v.kind())));
}

// Recv receives and returns a value from the channel v.
// It panics if v's Kind is not [Chan].
// The receive blocks until a value is ready.
// The boolean value ok is true if the value x corresponds to a send
// on the channel, false if it is a zero value received because the channel is closed.
public static (ΔValue x, bool ok) Recv(this ΔValue v) {
    ΔValue x = default!;
    bool ok = default!;

    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(false);
}

// internal recv, possibly non-blocking (nb).
// v is known to be a channel.
internal static (ΔValue val, bool ok) recv(this ΔValue v, bool nb) {
    ΔValue val = default!;
    bool ok = default!;

    var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    if ((ΔChanDir)(((ΔChanDir)(~tt).Dir) & RecvDir) == 0) {
        throw panic("reflect: recv on send-only channel");
    }
    var t = tt.val.Elem;
    val = new ΔValue(t, nil, ((flag)t.Kind()));
    @unsafe.Pointer p = default!;
    if (t.IfaceIndir()){
        p = (uintptr)unsafe_New(t);
        val.ptr = p;
        val.flag |= (flag)(flagIndir);
    } else {
        p = ((@unsafe.Pointer)(Ꮡval.of(Value.Ꮡptr)));
    }
    var (selected, ok) = chanrecv((uintptr)v.pointer(), nb, p);
    if (!selected) {
        val = new ΔValue(nil);
    }
    return (val, ok);
}

// Send sends x on the channel v.
// It panics if v's kind is not [Chan] or if x's type is not the same type as v's element type.
// As in Go, x's value must be assignable to the channel's element type.
public static void Send(this ΔValue v, ΔValue x) {
    v.mustBe(Chan);
    v.mustBeExported();
    v.send(x, false);
}

// internal send, possibly non-blocking.
// v is known to be a channel.
internal static bool /*selected*/ send(this ΔValue v, ΔValue x, bool nb) {
    bool selected = default!;

    var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    if ((ΔChanDir)(((ΔChanDir)(~tt).Dir) & SendDir) == 0) {
        throw panic("reflect: send on recv-only channel");
    }
    x.mustBeExported();
    x = x.assignTo("reflect.Value.Send"u8, (~tt).Elem, nil);
    @unsafe.Pointer p = default!;
    if ((flag)(x.flag & flagIndir) != 0){
        p = x.ptr;
    } else {
        p = ((@unsafe.Pointer)(Ꮡx.of(Value.Ꮡptr)));
    }
    return chansend((uintptr)v.pointer(), p, nb);
}

// Set assigns x to the value v.
// It panics if [Value.CanSet] returns false.
// As in Go, x's value must be assignable to v's type and
// must not be derived from an unexported field.
public static void Set(this ΔValue v, ΔValue x) {
    v.mustBeAssignable();
    x.mustBeExported();
    // do not let unexported x leak
    @unsafe.Pointer target = default!;
    if (v.kind() == ΔInterface) {
        target = v.ptr;
    }
    x = x.assignTo("reflect.Set"u8, v.typ(), target);
    if ((flag)(x.flag & flagIndir) != 0){
        if (x.ptr == new @unsafe.Pointer(ᏑzeroVal.at<byte>(0))){
            typedmemclr(v.typ(), v.ptr);
        } else {
            typedmemmove(v.typ(), v.ptr, x.ptr);
        }
    } else {
        ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).val = x.ptr;
    }
}

// SetBool sets v's underlying value.
// It panics if v's Kind is not [Bool] or if [Value.CanSet] returns false.
public static void SetBool(this ΔValue v, bool x) {
    v.mustBeAssignable();
    v.mustBe(ΔBool);
    ((ж<bool>)(uintptr)(v.ptr)).val = x;
}

// SetBytes sets v's underlying value.
// It panics if v's underlying value is not a slice of bytes.
public static void SetBytes(this ΔValue v, slice<byte> x) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    if (toRType(v.typ()).Elem().Kind() != Uint8) {
        // TODO add Elem method, fix mustBe(Slice) to return slice.
        throw panic("reflect.Value.SetBytes of non-byte slice");
    }
    ((ж<slice<byte>>)(uintptr)(v.ptr)).val = x;
}

// setRunes sets v's underlying value.
// It panics if v's underlying value is not a slice of runes (int32s).
internal static void setRunes(this ΔValue v, slice<rune> x) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    if (v.typ().Elem().Kind() != abi.Int32) {
        throw panic("reflect.Value.setRunes of non-rune slice");
    }
    ((ж<slice<rune>>)(uintptr)(v.ptr)).val = x;
}

// SetComplex sets v's underlying value to x.
// It panics if v's Kind is not [Complex64] or [Complex128], or if [Value.CanSet] returns false.
public static void SetComplex(this ΔValue v, complex128 x) {
    v.mustBeAssignable();
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.SetComplex", v.kind())));
        }
        else if (exprᴛ1 == Complex64) {
            ((ж<complex64>)(uintptr)(v.ptr)).val = ((complex64)x);
        }
        else if (exprᴛ1 == Complex128) {
            ((ж<complex128>)(uintptr)(v.ptr)).val = x;
        }
    }

}

// SetFloat sets v's underlying value to x.
// It panics if v's Kind is not [Float32] or [Float64], or if [Value.CanSet] returns false.
public static void SetFloat(this ΔValue v, float64 x) {
    v.mustBeAssignable();
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.SetFloat", v.kind())));
        }
        else if (exprᴛ1 == Float32) {
            ((ж<float32>)(uintptr)(v.ptr)).val = ((float32)x);
        }
        else if (exprᴛ1 == Float64) {
            ((ж<float64>)(uintptr)(v.ptr)).val = x;
        }
    }

}

// SetInt sets v's underlying value to x.
// It panics if v's Kind is not [Int], [Int8], [Int16], [Int32], or [Int64], or if [Value.CanSet] returns false.
public static void SetInt(this ΔValue v, int64 x) {
    v.mustBeAssignable();
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.SetInt", v.kind())));
        }
        else if (exprᴛ1 == ΔInt) {
            ((ж<nint>)(uintptr)(v.ptr)).val = ((nint)x);
        }
        else if (exprᴛ1 == Int8) {
            ((ж<int8>)(uintptr)(v.ptr)).val = ((int8)x);
        }
        else if (exprᴛ1 == Int16) {
            ((ж<int16>)(uintptr)(v.ptr)).val = ((int16)x);
        }
        else if (exprᴛ1 == Int32) {
            ((ж<int32>)(uintptr)(v.ptr)).val = ((int32)x);
        }
        else if (exprᴛ1 == Int64) {
            ((ж<int64>)(uintptr)(v.ptr)).val = x;
        }
    }

}

// SetLen sets v's length to n.
// It panics if v's Kind is not [Slice] or if n is negative or
// greater than the capacity of the slice.
public static void SetLen(this ΔValue v, nint n) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    var s = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    if (((nuint)n) > ((nuint)(~s).Cap)) {
        throw panic("reflect: slice length out of range in SetLen");
    }
    s.val.Len = n;
}

// SetCap sets v's capacity to n.
// It panics if v's Kind is not [Slice] or if n is smaller than the length or
// greater than the capacity of the slice.
public static void SetCap(this ΔValue v, nint n) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    var s = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    if (n < (~s).Len || n > (~s).Cap) {
        throw panic("reflect: slice capacity out of range in SetCap");
    }
    s.val.Cap = n;
}

// SetMapIndex sets the element associated with key in the map v to elem.
// It panics if v's Kind is not [Map].
// If elem is the zero Value, SetMapIndex deletes the key from the map.
// Otherwise if v holds a nil map, SetMapIndex will panic.
// As in Go, key's elem must be assignable to the map's key type,
// and elem's value must be assignable to the map's elem type.
public static void SetMapIndex(this ΔValue v, ΔValue key, ΔValue elem) {
    v.mustBe(Map);
    v.mustBeExported();
    key.mustBeExported();
    var tt = (ж<mapType>)(uintptr)(new @unsafe.Pointer(v.typ()));
    if ((tt.Key == stringType || key.kind() == ΔString) && tt.Key == key.typ() && tt.Elem.Size() <= abi.MapMaxElemBytes) {
        @string kΔ1 = ~(ж<@string>)(uintptr)(key.ptr);
        if (elem.typ() == nil) {
            mapdelete_faststr(v.typ(), (uintptr)v.pointer(), kΔ1);
            return;
        }
        elem.mustBeExported();
        elem = elem.assignTo("reflect.Value.SetMapIndex"u8, tt.Elem, nil);
        @unsafe.Pointer eΔ1 = default!;
        if ((flag)(elem.flag & flagIndir) != 0){
             = elem.ptr;
        } else {
             = ((@unsafe.Pointer)(Ꮡelem.of(Value.Ꮡptr)));
        }
        mapassign_faststr(v.typ(), (uintptr)v.pointer(), kΔ1, eΔ1);
        return;
    }
    key = key.assignTo("reflect.Value.SetMapIndex"u8, tt.Key, nil);
    @unsafe.Pointer k = default!;
    if ((flag)(key.flag & flagIndir) != 0){
        k = key.ptr;
    } else {
        k = ((@unsafe.Pointer)(Ꮡkey.of(Value.Ꮡptr)));
    }
    if (elem.typ() == nil) {
        mapdelete(v.typ(), (uintptr)v.pointer(), k);
        return;
    }
    elem.mustBeExported();
    elem = elem.assignTo("reflect.Value.SetMapIndex"u8, tt.Elem, nil);
    @unsafe.Pointer e = default!;
    if ((flag)(elem.flag & flagIndir) != 0){
        e = elem.ptr;
    } else {
        e = ((@unsafe.Pointer)(Ꮡelem.of(Value.Ꮡptr)));
    }
    mapassign(v.typ(), (uintptr)v.pointer(), k, e);
}

// SetUint sets v's underlying value to x.
// It panics if v's Kind is not [Uint], [Uintptr], [Uint8], [Uint16], [Uint32], or [Uint64], or if [Value.CanSet] returns false.
public static void SetUint(this ΔValue v, uint64 x) {
    v.mustBeAssignable();
    {
        ΔKind k = v.kind();
        var exprᴛ1 = k;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.SetUint", v.kind())));
        }
        else if (exprᴛ1 == ΔUint) {
            ((ж<nuint>)(uintptr)(v.ptr)).val = ((nuint)x);
        }
        else if (exprᴛ1 == Uint8) {
            ((ж<uint8>)(uintptr)(v.ptr)).val = ((uint8)x);
        }
        else if (exprᴛ1 == Uint16) {
            ((ж<uint16>)(uintptr)(v.ptr)).val = ((uint16)x);
        }
        else if (exprᴛ1 == Uint32) {
            ((ж<uint32>)(uintptr)(v.ptr)).val = ((uint32)x);
        }
        else if (exprᴛ1 == Uint64) {
            ((ж<uint64>)(uintptr)(v.ptr)).val = x;
        }
        else if (exprᴛ1 == Uintptr) {
            ((ж<uintptr>)(uintptr)(v.ptr)).val = ((uintptr)x);
        }
    }

}

// SetPointer sets the [unsafe.Pointer] value v to x.
// It panics if v's Kind is not [UnsafePointer].
public static void SetPointer(this ΔValue v, @unsafe.Pointer x) {
    v.mustBeAssignable();
    v.mustBe(ΔUnsafePointer);
    ((ж<@unsafe.Pointer>)(uintptr)(v.ptr)).val = x;
}

// SetString sets v's underlying value to x.
// It panics if v's Kind is not [String] or if [Value.CanSet] returns false.
public static void SetString(this ΔValue v, @string x) {
    v.mustBeAssignable();
    v.mustBe(ΔString);
    ((ж<@string>)(uintptr)(v.ptr)).val = x;
}

// Slice returns v[i:j].
// It panics if v's Kind is not [Array], [Slice] or [String], or if v is an unaddressable array,
// or if the indexes are out of bounds.
public static ΔValue Slice(this ΔValue v, nint i, nint j) {
    nint cap = default!;
    ж<sliceType> typ = default!;
    @unsafe.Pointer @base = default!;
    {
        ΔKind kind = v.kind();
        var exprᴛ1 = kind;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.Slice", v.kind())));
        }
        else if (exprᴛ1 == Array) {
            if ((flag)(v.flag & flagAddr) == 0) {
                throw panic("reflect.Value.Slice: slice of unaddressable array");
            }
            var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
            cap = ((nint)(~tt).Len);
            typ = (ж<sliceType>)(uintptr)(new @unsafe.Pointer((~tt).Slice));
            @base = v.ptr;
        }
        else if (exprᴛ1 == ΔSlice) {
            typ = (ж<sliceType>)(uintptr)(new @unsafe.Pointer(v.typ()));
            var sΔ2 = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
            @base = sΔ2.val.Data;
            cap = sΔ2.val.Cap;
        }
        else if (exprᴛ1 == ΔString) {
            var sΔ3 = (ж<unsafeheader.String>)(uintptr)(v.ptr);
            if (i < 0 || j < i || j > (~sΔ3).Len) {
                throw panic("reflect.Value.Slice: string slice index out of bounds");
            }
            ref var t = ref heap(new @internal.unsafeheader_package.String(), out var Ꮡt);
            if (i < (~sΔ3).Len) {
                t = new unsafeheader.String(Data: (uintptr)arrayAt((~sΔ3).Data, i, 1, "i < s.Len"u8), Len: j - i);
            }
            return new ΔValue(v.typ(), new @unsafe.Pointer(Ꮡt), v.flag);
        }
    }

    if (i < 0 || j < i || j > cap) {
        throw panic("reflect.Value.Slice: slice index out of bounds");
    }
    // Declare slice so that gc can see the base pointer in it.
    slice<@unsafe.Pointer> x = default!;
    // Reinterpret as *unsafeheader.Slice to edit.
    var s = (ж<unsafeheader.Slice>)(uintptr)(new @unsafe.Pointer(Ꮡ(x)));
    s.val.Len = j - i;
    s.val.Cap = cap - i;
    if (cap - i > 0){
        s.val.Data = (uintptr)arrayAt(@base, i, typ.Elem.Size(), "i < cap"u8);
    } else {
        // do not advance pointer, to avoid pointing beyond end of slice
        s.val.Data = @base;
    }
    var fl = (flag)((flag)(v.flag.ro() | flagIndir) | ((flag)ΔSlice));
    return new ΔValue(typ.Common(), new @unsafe.Pointer(Ꮡ(x)), fl);
}

// Slice3 is the 3-index form of the slice operation: it returns v[i:j:k].
// It panics if v's Kind is not [Array] or [Slice], or if v is an unaddressable array,
// or if the indexes are out of bounds.
public static ΔValue Slice3(this ΔValue v, nint i, nint j, nint k) {
    nint cap = default!;
    ж<sliceType> typ = default!;
    @unsafe.Pointer @base = default!;
    {
        ΔKind kind = v.kind();
        var exprᴛ1 = kind;
        { /* default: */
            throw panic(Ꮡ(new ValueError("reflect.Value.Slice3", v.kind())));
        }
        else if (exprᴛ1 == Array) {
            if ((flag)(v.flag & flagAddr) == 0) {
                throw panic("reflect.Value.Slice3: slice of unaddressable array");
            }
            var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
            cap = ((nint)(~tt).Len);
            typ = (ж<sliceType>)(uintptr)(new @unsafe.Pointer((~tt).Slice));
            @base = v.ptr;
        }
        else if (exprᴛ1 == ΔSlice) {
            typ = (ж<sliceType>)(uintptr)(new @unsafe.Pointer(v.typ()));
            var sΔ2 = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
            @base = sΔ2.val.Data;
            cap = sΔ2.val.Cap;
        }
    }

    if (i < 0 || j < i || k < j || k > cap) {
        throw panic("reflect.Value.Slice3: slice index out of bounds");
    }
    // Declare slice so that the garbage collector
    // can see the base pointer in it.
    slice<@unsafe.Pointer> x = default!;
    // Reinterpret as *unsafeheader.Slice to edit.
    var s = (ж<unsafeheader.Slice>)(uintptr)(new @unsafe.Pointer(Ꮡ(x)));
    s.val.Len = j - i;
    s.val.Cap = k - i;
    if (k - i > 0){
        s.val.Data = (uintptr)arrayAt(@base, i, typ.Elem.Size(), "i < k <= cap"u8);
    } else {
        // do not advance pointer, to avoid pointing beyond end of slice
        s.val.Data = @base;
    }
    var fl = (flag)((flag)(v.flag.ro() | flagIndir) | ((flag)ΔSlice));
    return new ΔValue(typ.Common(), new @unsafe.Pointer(Ꮡ(x)), fl);
}

// String returns the string v's underlying value, as a string.
// String is a special case because of Go's String method convention.
// Unlike the other getters, it does not panic if v's Kind is not [String].
// Instead, it returns a string of the form "<T value>" where T is v's type.
// The fmt package treats Values specially. It does not call their String
// method implicitly but instead prints the concrete values they hold.
public static @string String(this ΔValue v) {
    // stringNonString is split out to keep String inlineable for string kinds.
    if (v.kind() == ΔString) {
        return ~(ж<@string>)(uintptr)(v.ptr);
    }
    return v.stringNonString();
}

internal static @string stringNonString(this ΔValue v) {
    if (v.kind() == Invalid) {
        return "<invalid Value>"u8;
    }
    // If you call String on a reflect.Value of other type, it's better to
    // print something than to panic. Useful in debugging.
    return "<"u8 + v.Type().String() + " Value>"u8;
}

// TryRecv attempts to receive a value from the channel v but will not block.
// It panics if v's Kind is not [Chan].
// If the receive delivers a value, x is the transferred value and ok is true.
// If the receive cannot finish without blocking, x is the zero Value and ok is false.
// If the channel is closed, x is the zero value for the channel's element type and ok is false.
public static (ΔValue x, bool ok) TryRecv(this ΔValue v) {
    ΔValue x = default!;
    bool ok = default!;

    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(true);
}

// TrySend attempts to send x on the channel v but will not block.
// It panics if v's Kind is not [Chan].
// It reports whether the value was sent.
// As in Go, x's value must be assignable to the channel's element type.
public static bool TrySend(this ΔValue v, ΔValue x) {
    v.mustBe(Chan);
    v.mustBeExported();
    return v.send(x, true);
}

// Type returns v's type.
public static ΔType Type(this ΔValue v) {
    if (v.flag != 0 && (flag)(v.flag & flagMethod) == 0) {
        return ~(ж<rtype>)(uintptr)(noescape(new @unsafe.Pointer(v.typ_)));
    }
    // inline of toRType(v.typ()), for own inlining in inline test
    return v.typeSlow();
}

internal static ΔType typeSlow(this ΔValue v) {
    if (v.flag == 0) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Type", Invalid)));
    }
    var typ = v.typ();
    if ((flag)(v.flag & flagMethod) == 0) {
        return ~toRType(v.typ());
    }
    // Method value.
    // v.typ describes the receiver, not the method type.
    nint i = ((nint)v.flag) >> (int)(flagMethodShift);
    if (v.typ().Kind() == abi.Interface) {
        // Method on interface.
        var tt = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(typ));
        if (((nuint)i) >= ((nuint)len(tt.Methods))) {
            throw panic("reflect: internal error: invalid method index");
        }
        var mΔ1 = Ꮡ(tt.Methods, i);
        return ~toRType(typeOffFor(typ, (~mΔ1).Typ));
    }
    // Method on concrete type.
    var ms = typ.ExportedMethods();
    if (((nuint)i) >= ((nuint)len(ms))) {
        throw panic("reflect: internal error: invalid method index");
    }
    var m = ms[i];
    return ~toRType(typeOffFor(typ, m.Mtyp));
}

// CanUint reports whether [Value.Uint] can be used without panicking.
public static bool CanUint(this ΔValue v) {
    var exprᴛ1 = v.kind();
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// Uint returns v's underlying value, as a uint64.
// It panics if v's Kind is not [Uint], [Uintptr], [Uint8], [Uint16], [Uint32], or [Uint64].
public static uint64 Uint(this ΔValue v) {
    ΔKind k = v.kind();
    @unsafe.Pointer p = v.ptr;
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔUint) {
        return ((uint64)(~(ж<nuint>)(uintptr)(p)));
    }
    if (exprᴛ1 == Uint8) {
        return ((uint64)(~(ж<uint8>)(uintptr)(p)));
    }
    if (exprᴛ1 == Uint16) {
        return ((uint64)(~(ж<uint16>)(uintptr)(p)));
    }
    if (exprᴛ1 == Uint32) {
        return ((uint64)(~(ж<uint32>)(uintptr)(p)));
    }
    if (exprᴛ1 == Uint64) {
        return ~(ж<uint64>)(uintptr)(p);
    }
    if (exprᴛ1 == Uintptr) {
        return ((uint64)(~(ж<uintptr>)(uintptr)(p)));
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.Uint", v.kind())));
}

//go:nocheckptr
// This prevents inlining Value.UnsafeAddr when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.UnsafeAddr())
// and make an exception.

// UnsafeAddr returns a pointer to v's data, as a uintptr.
// It panics if v is not addressable.
//
// It's preferred to use uintptr(Value.Addr().UnsafePointer()) to get the equivalent result.
public static uintptr UnsafeAddr(this ΔValue v) {
    if (v.typ() == nil) {
        throw panic(Ꮡ(new ValueError("reflect.Value.UnsafeAddr", Invalid)));
    }
    if ((flag)(v.flag & flagAddr) == 0) {
        throw panic("reflect.Value.UnsafeAddr of unaddressable value");
    }
    // The compiler loses track as it converts to uintptr. Force escape.
    escapes(v.ptr);
    return ((uintptr)v.ptr);
}

// UnsafePointer returns v's value as a [unsafe.Pointer].
// It panics if v's Kind is not [Chan], [Func], [Map], [Pointer], [Slice], [String] or [UnsafePointer].
//
// If v's Kind is [Func], the returned pointer is an underlying
// code pointer, but not necessarily enough to identify a
// single function uniquely. The only guarantee is that the
// result is zero if and only if v is a nil func Value.
//
// If v's Kind is [Slice], the returned pointer is to the first
// element of the slice. If the slice is nil the returned value
// is nil.  If the slice is empty but non-nil the return value is non-nil.
//
// If v's Kind is [String], the returned pointer is to the first
// element of the underlying bytes of string.
public static @unsafe.Pointer UnsafePointer(this ΔValue v) {
    ΔKind k = v.kind();
    var exprᴛ1 = k;
    var matchᴛ1 = false;
    if (exprᴛ1 == ΔPointer) { matchᴛ1 = true;
        if (!v.typ().Pointers()) {
            // Since it is a not-in-heap pointer, all pointers to the heap are
            // forbidden! See comment in Value.Elem and issue #48399.
            if (!verifyNotInHeapPtr(~(ж<uintptr>)(uintptr)(v.ptr))) {
                throw panic("reflect: reflect.Value.UnsafePointer on an invalid notinheap pointer");
            }
            return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && (exprᴛ1 == Chan || exprᴛ1 == Map || exprᴛ1 == ΔUnsafePointer)) { matchᴛ1 = true;
        return (uintptr)v.pointer();
    }
    if (exprᴛ1 == Func) { matchᴛ1 = true;
        if ((flag)(v.flag & flagMethod) != 0) {
            // As the doc comment says, the returned pointer is an
            // underlying code pointer but not necessarily enough to
            // identify a single function uniquely. All method expressions
            // created via reflect have the same underlying code pointer,
            // so their Pointers are equal. The function used here must
            // match the one used in makeMethodValue.
            ref var code = ref heap<uintptr>(out var Ꮡcode);
            code = methodValueCallCodePtr();
            return ~(ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡcode)));
        }
        @unsafe.Pointer p = (uintptr)v.pointer();
        if (p != nil) {
            // Non-nil func value points at data block.
            // First word of data block is actual code.
            p = ~(ж<@unsafe.Pointer>)(uintptr)(p);
        }
        return p;
    }
    if (exprᴛ1 == ΔSlice) {
        return ((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val.Data;
    }
    if (exprᴛ1 == ΔString) { matchᴛ1 = true;
        return ((ж<unsafeheader.String>)(uintptr)(v.ptr)).val.Data;
    }

    throw panic(Ꮡ(new ValueError("reflect.Value.UnsafePointer", v.kind())));
}

// StringHeader is the runtime representation of a string.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
//
// Deprecated: Use unsafe.String or unsafe.StringData instead.
[GoType] partial struct StringHeader {
    public uintptr Data;
    public nint Len;
}

// SliceHeader is the runtime representation of a slice.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
//
// Deprecated: Use unsafe.Slice or unsafe.SliceData instead.
[GoType] partial struct SliceHeader {
    public uintptr Data;
    public nint Len;
    public nint Cap;
}

internal static void typesMustMatch(@string what, ΔType t1, ΔType t2) {
    if (!AreEqual(t1, t2)) {
        throw panic(what + ": "u8 + t1.String() + " != "u8 + t2.String());
    }
}

// arrayAt returns the i-th element of p,
// an array whose elements are eltSize bytes wide.
// The array pointed at by p must have at least i+1 elements:
// it is invalid (but impossible to check here) to pass i >= len,
// because then the result will point outside the array.
// whySafe must explain why i < len. (Passing "i < len" is fine;
// the benefit is to surface this assumption at the call site.)
internal static @unsafe.Pointer arrayAt(@unsafe.Pointer p, nint i, uintptr eltSize, @string whySafe) {
    return (uintptr)add(p.val, ((uintptr)i) * eltSize, "i < len"u8);
}

// Grow increases the slice's capacity, if necessary, to guarantee space for
// another n elements. After Grow(n), at least n elements can be appended
// to the slice without another allocation.
//
// It panics if v's Kind is not a [Slice] or if n is negative or too large to
// allocate the memory.
public static void Grow(this ΔValue v, nint n) {
    v.mustBeAssignable();
    v.mustBe(ΔSlice);
    v.grow(n);
}

// grow is identical to Grow but does not check for assignability.
internal static void grow(this ΔValue v, nint n) {
    var p = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    switch (ᐧ) {
    case {} when n is < 0: {
        throw panic("reflect.Value.Grow: negative len");
        break;
    }
    case {} when (~p).Len + n is < 0: {
        throw panic("reflect.Value.Grow: slice overflow");
        break;
    }
    case {} when (~p).Len + n is > (~p).Cap: {
        var t = v.typ().Elem();
        p.val = growslice(t, p.val, n);
        break;
    }}

}

// extendSlice extends a slice by n elements.
//
// Unlike Value.grow, which modifies the slice in place and
// does not change the length of the slice in place,
// extendSlice returns a new slice value with the length
// incremented by the number of specified elements.
internal static ΔValue extendSlice(this ΔValue v, nint n) {
    v.mustBeExported();
    v.mustBe(ΔSlice);
    // Shallow copy the slice header to avoid mutating the source slice.
    ref var sh = ref heap<@internal.unsafeheader_package.Slice>(out var Ꮡsh);
    sh = ~(ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    var s = Ꮡsh;
    v.ptr = new @unsafe.Pointer(s);
    v.flag = (flag)(flagIndir | ((flag)ΔSlice));
    // equivalent flag to MakeSlice
    v.grow(n);
    // fine to treat as assignable since we allocate a new slice header
    s.val.Len += n;
    return v;
}

// Clear clears the contents of a map or zeros the contents of a slice.
//
// It panics if v's Kind is not [Map] or [Slice].
public static void Clear(this ΔValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔSlice) {
        var sh = ~(ж<unsafeheader.Slice>)(uintptr)(v.ptr);
        var st = (ж<sliceType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        typedarrayclear(st.Elem, sh.Data, sh.Len);
    }
    else if (exprᴛ1 == Map) {
        mapclear(v.typ(), (uintptr)v.pointer());
    }
    else { /* default: */
        throw panic(Ꮡ(new ValueError("reflect.Value.Clear", v.Kind())));
    }

}

// Append appends the values x to a slice s and returns the resulting slice.
// As in Go, each x's value must be assignable to the slice's element type.
public static ΔValue Append(ΔValue s, params ꓸꓸꓸΔValue xʗp) {
    var x = xʗp.slice();

    s.mustBe(ΔSlice);
    nint n = s.Len();
    s = s.extendSlice(len(x));
    foreach (var (i, v) in x) {
        s.Index(n + i).Set(v);
    }
    return s;
}

// AppendSlice appends a slice t to a slice s and returns the resulting slice.
// The slices s and t must have the same element type.
public static ΔValue AppendSlice(ΔValue s, ΔValue t) {
    s.mustBe(ΔSlice);
    t.mustBe(ΔSlice);
    typesMustMatch("reflect.AppendSlice"u8, s.Type().Elem(), t.Type().Elem());
    nint ns = s.Len();
    nint nt = t.Len();
    s = s.extendSlice(nt);
    Copy(s.Slice(ns, ns + nt), t);
    return s;
}

// Copy copies the contents of src into dst until either
// dst has been filled or src has been exhausted.
// It returns the number of elements copied.
// Dst and src each must have kind [Slice] or [Array], and
// dst and src must have the same element type.
//
// As a special case, src can have kind [String] if the element type of dst is kind [Uint8].
public static nint Copy(ΔValue dst, ΔValue src) {
    ref var dk = ref heap<ΔKind>(out var Ꮡdk);
    dk = dst.kind();
    if (dk != Array && dk != ΔSlice) {
        throw panic(Ꮡ(new ValueError("reflect.Copy", dk)));
    }
    if (dk == Array) {
        dst.mustBeAssignable();
    }
    dst.mustBeExported();
    ref var sk = ref heap<ΔKind>(out var Ꮡsk);
    sk = src.kind();
    bool stringCopy = default!;
    if (sk != Array && sk != ΔSlice) {
        stringCopy = sk == ΔString && dst.typ().Elem().Kind() == abi.Uint8;
        if (!stringCopy) {
            throw panic(Ꮡ(new ValueError("reflect.Copy", sk)));
        }
    }
    src.mustBeExported();
    var de = dst.typ().Elem();
    if (!stringCopy) {
        var se = src.typ().Elem();
        typesMustMatch("reflect.Copy"u8, toType(de), toType(se));
    }
    unsafeheader.Slice ds = default!;
    unsafeheader.Slice ss = default!;
    if (dk == Array){
        ds.Data = dst.ptr;
        ds.Len = dst.Len();
        ds.Cap = ds.Len;
    } else {
        ds = ~(ж<unsafeheader.Slice>)(uintptr)(dst.ptr);
    }
    if (sk == Array){
        ss.Data = src.ptr;
        ss.Len = src.Len();
        ss.Cap = ss.Len;
    } else 
    if (sk == ΔSlice){
        ss = ~(ж<unsafeheader.Slice>)(uintptr)(src.ptr);
    } else {
        var sh = ~(ж<unsafeheader.String>)(uintptr)(src.ptr);
        ss.Data = sh.Data;
        ss.Len = sh.Len;
        ss.Cap = sh.Len;
    }
    return typedslicecopy(de.Common(), ds, ss);
}

// A runtimeSelect is a single case passed to rselect.
// This must match ../runtime/select.go:/runtimeSelect
[GoType] partial struct runtimeSelect {
    internal SelectDir dir;      // SelectSend, SelectRecv or SelectDefault
    internal ж<rtype> typ;      // channel type
    internal @unsafe.Pointer ch; // channel
    internal @unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
}

// rselect runs a select. It returns the index of the chosen case.
// If the case was a receive, val is filled in with the received value.
// The conventional OK bool indicates whether the receive corresponds
// to a sent value.
//
// rselect generally doesn't escape the runtimeSelect slice, except
// that for the send case the value to send needs to escape. We don't
// have a way to represent that in the function signature. So we handle
// that with a forced escape in function Select.
//
//go:noescape
internal static partial (nint chosen, bool recvOK) rselect(slice<runtimeSelect> _);

[GoType("num:nint")] partial struct SelectDir;

// NOTE: These values must match ../runtime/select.go:/selectDir.
internal static readonly SelectDir _ = /* iota */ 0;
public static readonly SelectDir SelectSend = 1; // case Chan <- Send
public static readonly SelectDir SelectRecv = 2; // case <-Chan:
public static readonly SelectDir SelectDefault = 3; // default

// A SelectCase describes a single case in a select operation.
// The kind of case depends on Dir, the communication direction.
//
// If Dir is SelectDefault, the case represents a default case.
// Chan and Send must be zero Values.
//
// If Dir is SelectSend, the case represents a send operation.
// Normally Chan's underlying value must be a channel, and Send's underlying value must be
// assignable to the channel's element type. As a special case, if Chan is a zero Value,
// then the case is ignored, and the field Send will also be ignored and may be either zero
// or non-zero.
//
// If Dir is [SelectRecv], the case represents a receive operation.
// Normally Chan's underlying value must be a channel and Send must be a zero Value.
// If Chan is a zero Value, then the case is ignored, but Send must still be a zero Value.
// When a receive operation is selected, the received Value is returned by Select.
[GoType] partial struct SelectCase {
    public SelectDir Dir; // direction of case
    public ΔValue Chan;   // channel to use (for send or receive)
    public ΔValue Send;   // value to send (for send)
}

// Select executes a select operation described by the list of cases.
// Like the Go select statement, it blocks until at least one of the cases
// can proceed, makes a uniform pseudo-random choice,
// and then executes that case. It returns the index of the chosen case
// and, if that case was a receive operation, the value received and a
// boolean indicating whether the value corresponds to a send on the channel
// (as opposed to a zero value received because the channel is closed).
// Select supports a maximum of 65536 cases.
public static (nint chosen, ΔValue recv, bool recvOK) Select(slice<SelectCase> cases) {
    nint chosen = default!;
    ΔValue recv = default!;
    bool recvOK = default!;

    if (len(cases) > 65536) {
        throw panic("reflect.Select: too many cases (max 65536)");
    }
    // NOTE: Do not trust that caller is not modifying cases data underfoot.
    // The range is safe because the caller cannot modify our copy of the len
    // and each iteration makes its own copy of the value c.
    slice<runtimeSelect> runcases = default!;
    if (len(cases) > 4){
        // Slice is heap allocated due to runtime dependent capacity.
        runcases = new slice<runtimeSelect>(len(cases));
    } else {
        // Slice can be stack allocated due to constant capacity.
        runcases = new slice<runtimeSelect>(len(cases), 4);
    }
    var haveDefault = false;
    foreach (var (i, c) in cases) {
        var rc = Ꮡ(runcases, i);
        rc.val.dir = c.Dir;
        var exprᴛ1 = c.Dir;
        { /* default: */
            throw panic("reflect.Select: invalid Dir");
        }
        else if (exprᴛ1 == SelectDefault) {
            if (haveDefault) {
                // default
                throw panic("reflect.Select: multiple default cases");
            }
            haveDefault = true;
            if (c.Chan.IsValid()) {
                throw panic("reflect.Select: default case has Chan value");
            }
            if (c.Send.IsValid()) {
                throw panic("reflect.Select: default case has Send value");
            }
        }
        else if (exprᴛ1 == SelectSend) {
            var ch = c.Chan;
            if (!ch.IsValid()) {
                break;
            }
            ch.mustBe(Chan);
            ch.mustBeExported();
            var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(ch.typ()));
            if ((ΔChanDir)(((ΔChanDir)(~tt).Dir) & SendDir) == 0) {
                throw panic("reflect.Select: SendDir case using recv-only channel");
            }
            rc.val.ch = (uintptr)ch.pointer();
            rc.val.typ = toRType(Ꮡ((~tt).Type));
            var v = c.Send;
            if (!v.IsValid()) {
                throw panic("reflect.Select: SendDir case missing Send value");
            }
            v.mustBeExported();
            v = v.assignTo("reflect.Select"u8, (~tt).Elem, nil);
            if ((flag)(v.flag & flagIndir) != 0){
                rc.val.val = v.ptr;
            } else {
                rc.val.val = ((@unsafe.Pointer)(Ꮡv.of(Value.Ꮡptr)));
            }
            escapes((~rc).val);
        }
        else if (exprᴛ1 == SelectRecv) {
            if (c.Send.IsValid()) {
                // The value to send needs to escape. See the comment at rselect for
                // why we need forced escape.
                throw panic("reflect.Select: RecvDir case has Send value");
            }
            var ch = c.Chan;
            if (!ch.IsValid()) {
                break;
            }
            ch.mustBe(Chan);
            ch.mustBeExported();
            var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(ch.typ()));
            if ((ΔChanDir)(((ΔChanDir)(~tt).Dir) & RecvDir) == 0) {
                throw panic("reflect.Select: RecvDir case using send-only channel");
            }
            rc.val.ch = (uintptr)ch.pointer();
            rc.val.typ = toRType(Ꮡ((~tt).Type));
            rc.val.val = (uintptr)unsafe_New((~tt).Elem);
        }

    }
    (chosen, recvOK) = rselect(runcases);
    if (runcases[chosen].dir == SelectRecv) {
        var tt = (ж<chanType>)(uintptr)(new @unsafe.Pointer(runcases[chosen].typ));
        var t = tt.val.Elem;
        @unsafe.Pointer p = runcases[chosen].val;
        var fl = ((flag)t.Kind());
        if (t.IfaceIndir()){
            recv = new ΔValue(t, p.val, (flag)(fl | flagIndir));
        } else {
            recv = new ΔValue(t, ~(ж<@unsafe.Pointer>)(uintptr)(p), fl);
        }
    }
    return (chosen, recv, recvOK);
}

/*
 * constructors
 */
// implemented in package runtime

//go:noescape
internal static partial @unsafe.Pointer unsafe_New(ж<abi.Type> _);

//go:noescape
internal static partial @unsafe.Pointer unsafe_NewArray(ж<abi.Type> _, nint _);

// MakeSlice creates a new zero-initialized slice value
// for the specified slice type, length, and capacity.
public static ΔValue MakeSlice(ΔType typ, nint len, nint cap) {
    if (typ.Kind() != ΔSlice) {
        throw panic("reflect.MakeSlice of non-slice type");
    }
    if (len < 0) {
        throw panic("reflect.MakeSlice: negative len");
    }
    if (cap < 0) {
        throw panic("reflect.MakeSlice: negative cap");
    }
    if (len > cap) {
        throw panic("reflect.MakeSlice: len > cap");
    }
    ref var s = ref heap<@internal.unsafeheader_package.Slice>(out var Ꮡs);
    s = new unsafeheader.Slice(Data: (uintptr)unsafe_NewArray(Ꮡ((typ.Elem()._<rtype.val>().t)), cap), Len: len, Cap: cap);
    return new ΔValue(Ꮡ(typ._<rtype.val>().t), new @unsafe.Pointer(Ꮡs), (flag)(flagIndir | ((flag)ΔSlice)));
}

// SliceAt returns a [Value] representing a slice whose underlying
// data starts at p, with length and capacity equal to n.
//
// This is like [unsafe.Slice].
public static ΔValue SliceAt(ΔType typ, @unsafe.Pointer p, nint n) {
    unsafeslice(typ.common(), p.val, n);
    ref var s = ref heap<@internal.unsafeheader_package.Slice>(out var Ꮡs);
    s = new unsafeheader.Slice(Data: p, Len: n, Cap: n);
    return new ΔValue(SliceOf(typ).common(), new @unsafe.Pointer(Ꮡs), (flag)(flagIndir | ((flag)ΔSlice)));
}

// MakeChan creates a new channel with the specified type and buffer size.
public static ΔValue MakeChan(ΔType typ, nint buffer) {
    if (typ.Kind() != Chan) {
        throw panic("reflect.MakeChan of non-chan type");
    }
    if (buffer < 0) {
        throw panic("reflect.MakeChan: negative buffer size");
    }
    if (typ.ChanDir() != BothDir) {
        throw panic("reflect.MakeChan: unidirectional channel type");
    }
    var t = typ.common();
    @unsafe.Pointer ch = (uintptr)makechan(t, buffer);
    return new ΔValue(t, ch.val, ((flag)Chan));
}

// MakeMap creates a new map with the specified type.
public static ΔValue MakeMap(ΔType typ) {
    return MakeMapWithSize(typ, 0);
}

// MakeMapWithSize creates a new map with the specified type
// and initial space for approximately n elements.
public static ΔValue MakeMapWithSize(ΔType typ, nint n) {
    if (typ.Kind() != Map) {
        throw panic("reflect.MakeMapWithSize of non-map type");
    }
    var t = typ.common();
    @unsafe.Pointer m = (uintptr)makemap(t, n);
    return new ΔValue(t, m.val, ((flag)Map));
}

// Indirect returns the value that v points to.
// If v is a nil pointer, Indirect returns a zero Value.
// If v is not a pointer, Indirect returns v.
public static ΔValue Indirect(ΔValue v) {
    if (v.Kind() != ΔPointer) {
        return v;
    }
    return v.Elem();
}

// ValueOf returns a new Value initialized to the concrete value
// stored in the interface i. ValueOf(nil) returns the zero Value.
public static ΔValue ValueOf(any i) {
    if (i == default!) {
        return new ΔValue(nil);
    }
    return unpackEface(i);
}

// Zero returns a Value representing the zero value for the specified type.
// The result is different from the zero value of the Value struct,
// which represents no value at all.
// For example, Zero(TypeOf(42)) returns a Value with Kind [Int] and value 0.
// The returned value is neither addressable nor settable.
public static ΔValue Zero(ΔType typ) {
    if (typ == default!) {
        throw panic("reflect: Zero(nil)");
    }
    var t = Ꮡ(typ._<rtype.val>().t);
    var fl = ((flag)t.Kind());
    if (t.IfaceIndir()) {
        @unsafe.Pointer p = default!;
        if (t.Size() <= abi.ZeroValSize){
            p = new @unsafe.Pointer(ᏑzeroVal.at<byte>(0));
        } else {
            p = (uintptr)unsafe_New(t);
        }
        return new ΔValue(t, p.val, (flag)(fl | flagIndir));
    }
    return new ΔValue(t, nil, fl);
}

//go:linkname zeroVal runtime.zeroVal
internal static array<byte> zeroVal;

// New returns a Value representing a pointer to a new zero value
// for the specified type. That is, the returned Value's Type is [PointerTo](typ).
public static ΔValue New(ΔType typ) {
    if (typ == default!) {
        throw panic("reflect: New(nil)");
    }
    var t = Ꮡ(typ._<rtype.val>().t);
    var pt = ptrTo(t);
    if (pt.IfaceIndir()) {
        // This is a pointer to a not-in-heap type.
        throw panic("reflect: New of type that may not be allocated in heap (possibly undefined cgo C type)");
    }
    @unsafe.Pointer ptr = (uintptr)unsafe_New(t);
    var fl = ((flag)ΔPointer);
    return new ΔValue(pt, ptr.val, fl);
}

// NewAt returns a Value representing a pointer to a value of the
// specified type, using p as that pointer.
public static ΔValue NewAt(ΔType typ, @unsafe.Pointer p) {
    var fl = ((flag)ΔPointer);
    var t = typ._<rtype.val>();
    return new ΔValue(t.ptrTo(), p.val, fl);
}

// assignTo returns a value v that can be assigned directly to dst.
// It panics if v is not assignable to dst.
// For a conversion to an interface type, target, if not nil,
// is a suggested scratch space to use.
// target must be initialized memory (or nil).
public static ΔValue assignTo(this ΔValue v, @string context, ж<abi.Type> Ꮡdst, @unsafe.Pointer target) {
    ref var dst = ref Ꮡdst.val;

    if ((flag)(v.flag & flagMethod) != 0) {
        v = makeMethodValue(context, v);
    }
    switch (ᐧ) {
    case {} when directlyAssignable(Ꮡdst, v.typ()): {
        var fl = (flag)((flag)(v.flag & ((flag)(flagAddr | flagIndir))) | v.flag.ro());
        fl |= (flag)(((flag)dst.Kind()));
        return new ΔValue( // Overwrite type so that they match.
 // Same memory layout, so no harm done.
Ꮡdst, v.ptr, fl);
    }
    case {} when implements(Ꮡdst, v.typ()): {
        if (v.Kind() == ΔInterface && v.IsNil()) {
            // A nil ReadWriter passed to nil Reader is OK,
            // but using ifaceE2I below will panic.
            // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
            return new ΔValue(Ꮡdst, nil, ((flag)ΔInterface));
        }
        var x = valueInterface(v, false);
        if (target == nil) {
            target = (uintptr)unsafe_New(Ꮡdst);
        }
        if (dst.NumMethod() == 0){
            ((ж<any>)(uintptr)(target)).val = x;
        } else {
            ifaceE2I(Ꮡdst, x, target.val);
        }
        return new ΔValue(Ꮡdst, target.val, (flag)(flagIndir | ((flag)ΔInterface)));
    }}

    // Failed.
    throw panic(context + ": value of type "u8 + stringFor(v.typ()) + " is not assignable to type "u8 + stringFor(Ꮡdst));
}

// Convert returns the value v converted to type t.
// If the usual Go conversion rules do not allow conversion
// of the value v to type t, or if converting v to type t panics, Convert panics.
public static ΔValue Convert(this ΔValue v, ΔType t) {
    if ((flag)(v.flag & flagMethod) != 0) {
        v = makeMethodValue("Convert"u8, v);
    }
    var op = convertOp(t.common(), v.typ());
    if (op == default!) {
        throw panic("reflect.Value.Convert: value of type "u8 + stringFor(v.typ()) + " cannot be converted to type "u8 + t.String());
    }
    return op(v, t);
}

// CanConvert reports whether the value v can be converted to type t.
// If v.CanConvert(t) returns true then v.Convert(t) will not panic.
public static bool CanConvert(this ΔValue v, ΔType t) {
    var vt = v.Type();
    if (!vt.ConvertibleTo(t)) {
        return false;
    }
    // Converting from slice to array or to pointer-to-array can panic
    // depending on the value.
    switch (ᐧ) {
    case {} when vt.Kind() == ΔSlice && t.Kind() == Array: {
        if (t.Len() > v.Len()) {
            return false;
        }
        break;
    }
    case {} when vt.Kind() == ΔSlice && t.Kind() == ΔPointer && t.Elem().Kind() == Array: {
        nint n = t.Elem().Len();
        if (n > v.Len()) {
            return false;
        }
        break;
    }}

    return true;
}

// Comparable reports whether the value v is comparable.
// If the type of v is an interface, this checks the dynamic type.
// If this reports true then v.Interface() == x will not panic for any x,
// nor will v.Equal(u) for any Value u.
public static bool Comparable(this ΔValue v) {
    ΔKind k = v.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Invalid) {
        return false;
    }
    if (exprᴛ1 == Array) {
        var exprᴛ2 = v.Type().Elem().Kind();
        if (exprᴛ2 == ΔInterface || exprᴛ2 == Array || exprᴛ2 == Struct) {
            for (nint i = 0; i < v.Type().Len(); i++) {
                if (!v.Index(i).Comparable()) {
                    return false;
                }
            }
            return true;
        }

        return v.Type().Comparable();
    }
    if (exprᴛ1 == ΔInterface) {
        return v.IsNil() || v.Elem().Comparable();
    }
    if (exprᴛ1 == Struct) {
        for (nint i = 0; i < v.NumField(); i++) {
            if (!v.Field(i).Comparable()) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        return v.Type().Comparable();
    }

}

// Equal reports true if v is equal to u.
// For two invalid values, Equal will report true.
// For an interface value, Equal will compare the value within the interface.
// Otherwise, If the values have different types, Equal will report false.
// Otherwise, for arrays and structs Equal will compare each element in order,
// and report false if it finds non-equal elements.
// During all comparisons, if values of the same type are compared,
// and the type is not comparable, Equal will panic.
public static bool Equal(this ΔValue v, ΔValue u) {
    if (v.Kind() == ΔInterface) {
        v = v.Elem();
    }
    if (u.Kind() == ΔInterface) {
        u = u.Elem();
    }
    if (!v.IsValid() || !u.IsValid()) {
        return v.IsValid() == u.IsValid();
    }
    if (v.Kind() != u.Kind() || !AreEqual(v.Type(), u.Type())) {
        return false;
    }
    // Handle each Kind directly rather than calling valueInterface
    // to avoid allocating.
    var exprᴛ1 = v.Kind();
    { /* default: */
        throw panic("reflect.Value.Equal: invalid Kind");
    }
    else if (exprᴛ1 == ΔBool) {
        return v.Bool() == u.Bool();
    }
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        return v.Int() == u.Int();
    }
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        return v.Uint() == u.Uint();
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        return v.Float() == u.Float();
    }
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        return v.Complex() == u.Complex();
    }
    if (exprᴛ1 == ΔString) {
        return v.String() == u.String();
    }
    if (exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        return v.Pointer() == u.Pointer();
    }
    if (exprᴛ1 == Array) {
        nint vl = v.Len();
        if (vl == 0) {
            // u and v have the same type so they have the same length
            // panic on [0]func()
            if (!v.Type().Elem().Comparable()) {
                break;
            }
            return true;
        }
        for (nint i = 0; i < vl; i++) {
            if (!v.Index(i).Equal(u.Index(i))) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == Struct) {
        nint nf = v.NumField();
        for (nint i = 0; i < nf; i++) {
            // u and v have the same type so they have the same fields
            if (!v.Field(i).Equal(u.Field(i))) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔSlice) {
        break;
    }

    throw panic("reflect.Value.Equal: values of type "u8 + v.Type().String() + " are not comparable"u8);
}

// convertOp returns the function to convert a value of type src
// to a value of type dst. If the conversion is illegal, convertOp returns nil.
internal static Func<ΔValue, reflect.Type, reflect.Value> convertOp(ж<abi.Type> Ꮡdst, ж<abi.Type> Ꮡsrc) {
    ref var dst = ref Ꮡdst.val;
    ref var src = ref Ꮡsrc.val;

    var exprᴛ1 = ((ΔKind)src.Kind());
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var exprᴛ2 = ((ΔKind)dst.Kind());
        if (exprᴛ2 == ΔInt || exprᴛ2 == Int8 || exprᴛ2 == Int16 || exprᴛ2 == Int32 || exprᴛ2 == Int64 || exprᴛ2 == ΔUint || exprᴛ2 == Uint8 || exprᴛ2 == Uint16 || exprᴛ2 == Uint32 || exprᴛ2 == Uint64 || exprᴛ2 == Uintptr) {
            return cvtInt;
        }
        if (exprᴛ2 == Float32 || exprᴛ2 == Float64) {
            return cvtIntFloat;
        }
        if (exprᴛ2 == ΔString) {
            return cvtIntString;
        }

    }
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr) {
        var exprᴛ3 = ((ΔKind)dst.Kind());
        if (exprᴛ3 == ΔInt || exprᴛ3 == Int8 || exprᴛ3 == Int16 || exprᴛ3 == Int32 || exprᴛ3 == Int64 || exprᴛ3 == ΔUint || exprᴛ3 == Uint8 || exprᴛ3 == Uint16 || exprᴛ3 == Uint32 || exprᴛ3 == Uint64 || exprᴛ3 == Uintptr) {
            return cvtUint;
        }
        if (exprᴛ3 == Float32 || exprᴛ3 == Float64) {
            return cvtUintFloat;
        }
        if (exprᴛ3 == ΔString) {
            return cvtUintString;
        }

    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64) {
        var exprᴛ4 = ((ΔKind)dst.Kind());
        if (exprᴛ4 == ΔInt || exprᴛ4 == Int8 || exprᴛ4 == Int16 || exprᴛ4 == Int32 || exprᴛ4 == Int64) {
            return cvtFloatInt;
        }
        if (exprᴛ4 == ΔUint || exprᴛ4 == Uint8 || exprᴛ4 == Uint16 || exprᴛ4 == Uint32 || exprᴛ4 == Uint64 || exprᴛ4 == Uintptr) {
            return cvtFloatUint;
        }
        if (exprᴛ4 == Float32 || exprᴛ4 == Float64) {
            return cvtFloat;
        }

    }
    if (exprᴛ1 == Complex64 || exprᴛ1 == Complex128) {
        var exprᴛ5 = ((ΔKind)dst.Kind());
        if (exprᴛ5 == Complex64 || exprᴛ5 == Complex128) {
            return cvtComplex;
        }

    }
    if (exprᴛ1 == ΔString) {
        if (dst.Kind() == abi.Slice && pkgPathFor(dst.Elem()) == ""u8) {
            var exprᴛ6 = ((ΔKind)dst.Elem().Kind());
            if (exprᴛ6 == Uint8) {
                return cvtStringBytes;
            }
            if (exprᴛ6 == Int32) {
                return cvtStringRunes;
            }

        }
    }
    if (exprᴛ1 == ΔSlice) {
        if (dst.Kind() == abi.ΔString && pkgPathFor(src.Elem()) == ""u8) {
            var exprᴛ7 = ((ΔKind)src.Elem().Kind());
            if (exprᴛ7 == Uint8) {
                return cvtBytesString;
            }
            if (exprᴛ7 == Int32) {
                return cvtRunesString;
            }

        }
        if (dst.Kind() == abi.Pointer && dst.Elem().Kind() == abi.Array && src.Elem() == dst.Elem().Elem()) {
            // "x is a slice, T is a pointer-to-array type,
            // and the slice and array types have identical element types."
            return cvtSliceArrayPtr;
        }
        if (dst.Kind() == abi.Array && src.Elem() == dst.Elem()) {
            // "x is a slice, T is an array type,
            // and the slice and array types have identical element types."
            return cvtSliceArray;
        }
    }
    if (exprᴛ1 == Chan) {
        if (dst.Kind() == abi.Chan && specialChannelAssignability(Ꮡdst, Ꮡsrc)) {
            return cvtDirect;
        }
    }

    // dst and src have same underlying type.
    if (haveIdenticalUnderlyingType(Ꮡdst, Ꮡsrc, false)) {
        return cvtDirect;
    }
    // dst and src are non-defined pointer types with same underlying base type.
    if (dst.Kind() == abi.Pointer && nameFor(Ꮡdst) == ""u8 && src.Kind() == abi.Pointer && nameFor(Ꮡsrc) == ""u8 && haveIdenticalUnderlyingType(elem(Ꮡdst), elem(Ꮡsrc), false)) {
        return cvtDirect;
    }
    if (implements(Ꮡdst, Ꮡsrc)) {
        if (src.Kind() == abi.Interface) {
            return cvtI2I;
        }
        return cvtT2I;
    }
    return default!;
}

// makeInt returns a Value of type t equal to bits (possibly truncated),
// where t is a signed or unsigned int type.
internal static ΔValue makeInt(flag f, uint64 bits, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    switch (typ.Size()) {
    case 1: {
        ((ж<uint8>)(uintptr)(ptr)).val = ((uint8)bits);
        break;
    }
    case 2: {
        ((ж<uint16>)(uintptr)(ptr)).val = ((uint16)bits);
        break;
    }
    case 4: {
        ((ж<uint32>)(uintptr)(ptr)).val = ((uint32)bits);
        break;
    }
    case 8: {
        ((ж<uint64>)(uintptr)(ptr)).val = bits;
        break;
    }}

    return new ΔValue(typ, ptr.val, (flag)((flag)(f | flagIndir) | ((flag)typ.Kind())));
}

// makeFloat returns a Value of type t equal to v (possibly truncated to float32),
// where t is a float32 or float64 type.
internal static ΔValue makeFloat(flag f, float64 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    switch (typ.Size()) {
    case 4: {
        ((ж<float32>)(uintptr)(ptr)).val = ((float32)v);
        break;
    }
    case 8: {
        ((ж<float64>)(uintptr)(ptr)).val = v;
        break;
    }}

    return new ΔValue(typ, ptr.val, (flag)((flag)(f | flagIndir) | ((flag)typ.Kind())));
}

// makeFloat32 returns a Value of type t equal to v, where t is a float32 type.
internal static ΔValue makeFloat32(flag f, float32 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    ((ж<float32>)(uintptr)(ptr)).val = v;
    return new ΔValue(typ, ptr.val, (flag)((flag)(f | flagIndir) | ((flag)typ.Kind())));
}

// makeComplex returns a Value of type t equal to v (possibly truncated to complex64),
// where t is a complex64 or complex128 type.
internal static ΔValue makeComplex(flag f, complex128 v, ΔType t) {
    var typ = t.common();
    @unsafe.Pointer ptr = (uintptr)unsafe_New(typ);
    switch (typ.Size()) {
    case 8: {
        ((ж<complex64>)(uintptr)(ptr)).val = ((complex64)v);
        break;
    }
    case 16: {
        ((ж<complex128>)(uintptr)(ptr)).val = v;
        break;
    }}

    return new ΔValue(typ, ptr.val, (flag)((flag)(f | flagIndir) | ((flag)typ.Kind())));
}

internal static ΔValue makeString(flag f, @string v, ΔType t) {
    var ret = New(t).Elem();
    ret.SetString(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

internal static ΔValue makeBytes(flag f, slice<byte> v, ΔType t) {
    var ret = New(t).Elem();
    ret.SetBytes(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

internal static ΔValue makeRunes(flag f, slice<rune> v, ΔType t) {
    var ret = New(t).Elem();
    ret.setRunes(v);
    ret.flag = (flag)((flag)(ret.flag & ~flagAddr) | f);
    return ret;
}

// These conversion functions are returned by convertOp
// for classes of conversions. For example, the first function, cvtInt,
// takes any value v of signed int type and returns the value converted
// to type t, where t is any signed or unsigned int type.

// convertOp: intXX -> [u]intXX
internal static ΔValue cvtInt(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), ((uint64)v.Int()), t);
}

// convertOp: uintXX -> [u]intXX
internal static ΔValue cvtUint(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), v.Uint(), t);
}

// convertOp: floatXX -> intXX
internal static ΔValue cvtFloatInt(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), ((uint64)((int64)v.Float())), t);
}

// convertOp: floatXX -> uintXX
internal static ΔValue cvtFloatUint(ΔValue v, ΔType t) {
    return makeInt(v.flag.ro(), ((uint64)v.Float()), t);
}

// convertOp: intXX -> floatXX
internal static ΔValue cvtIntFloat(ΔValue v, ΔType t) {
    return makeFloat(v.flag.ro(), ((float64)v.Int()), t);
}

// convertOp: uintXX -> floatXX
internal static ΔValue cvtUintFloat(ΔValue v, ΔType t) {
    return makeFloat(v.flag.ro(), ((float64)v.Uint()), t);
}

// convertOp: floatXX -> floatXX
internal static ΔValue cvtFloat(ΔValue v, ΔType t) {
    if (v.Type().Kind() == Float32 && t.Kind() == Float32) {
        // Don't do any conversion if both types have underlying type float32.
        // This avoids converting to float64 and back, which will
        // convert a signaling NaN to a quiet NaN. See issue 36400.
        return makeFloat32(v.flag.ro(), ~(ж<float32>)(uintptr)(v.ptr), t);
    }
    return makeFloat(v.flag.ro(), v.Float(), t);
}

// convertOp: complexXX -> complexXX
internal static ΔValue cvtComplex(ΔValue v, ΔType t) {
    return makeComplex(v.flag.ro(), v.Complex(), t);
}

// convertOp: intXX -> string
internal static ΔValue cvtIntString(ΔValue v, ΔType t) {
    @string s = "\uFFFD"u8;
    {
        var x = v.Int(); if (((int64)((rune)x)) == x) {
            s = ((@string)((rune)x));
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: uintXX -> string
internal static ΔValue cvtUintString(ΔValue v, ΔType t) {
    @string s = "\uFFFD"u8;
    {
        var x = v.Uint(); if (((uint64)((rune)x)) == x) {
            s = ((@string)((rune)x));
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: []byte -> string
internal static ΔValue cvtBytesString(ΔValue v, ΔType t) {
    return makeString(v.flag.ro(), ((@string)v.Bytes()), t);
}

// convertOp: string -> []byte
internal static ΔValue cvtStringBytes(ΔValue v, ΔType t) {
    return makeBytes(v.flag.ro(), slice<byte>(v.String()), t);
}

// convertOp: []rune -> string
internal static ΔValue cvtRunesString(ΔValue v, ΔType t) {
    return makeString(v.flag.ro(), ((@string)v.runes()), t);
}

// convertOp: string -> []rune
internal static ΔValue cvtStringRunes(ΔValue v, ΔType t) {
    return makeRunes(v.flag.ro(), slice<rune>(v.String()), t);
}

// convertOp: []T -> *[N]T
internal static ΔValue cvtSliceArrayPtr(ΔValue v, ΔType t) {
    nint n = t.Elem().Len();
    if (n > v.Len()) {
        throw panic("reflect: cannot convert slice with length "u8 + itoa.Itoa(v.Len()) + " to pointer to array with length "u8 + itoa.Itoa(n));
    }
    var h = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    return new ΔValue(t.common(), (~h).Data, (flag)((flag)(v.flag & ~((flag)((flag)(flagIndir | flagAddr) | flagKindMask))) | ((flag)ΔPointer)));
}

// convertOp: []T -> [N]T
internal static ΔValue cvtSliceArray(ΔValue v, ΔType t) {
    nint n = t.Len();
    if (n > v.Len()) {
        throw panic("reflect: cannot convert slice with length "u8 + itoa.Itoa(v.Len()) + " to array with length "u8 + itoa.Itoa(n));
    }
    var h = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    var typ = t.common();
    @unsafe.Pointer ptr = h.val.Data;
    @unsafe.Pointer c = (uintptr)unsafe_New(typ);
    typedmemmove(typ, c, ptr);
    ptr = c;
    return new ΔValue(typ, ptr.val, (flag)((flag)(v.flag & ~((flag)(flagAddr | flagKindMask))) | ((flag)Array)));
}

// convertOp: direct copy
internal static ΔValue cvtDirect(ΔValue v, ΔType typ) {
    var f = v.flag;
    var t = typ.common();
    @unsafe.Pointer ptr = v.ptr;
    if ((flag)(f & flagAddr) != 0) {
        // indirect, mutable word - make a copy
        @unsafe.Pointer c = (uintptr)unsafe_New(t);
        typedmemmove(t, c, ptr);
        ptr = c;
        f &= ~(flag)(flagAddr);
    }
    return new ΔValue(t, ptr.val, (flag)(v.flag.ro() | f));
}

// v.flag.ro()|f == f?

// convertOp: concrete -> interface
internal static ΔValue cvtT2I(ΔValue v, ΔType typ) {
    @unsafe.Pointer target = (uintptr)unsafe_New(typ.common());
    var x = valueInterface(v, false);
    if (typ.NumMethod() == 0){
        ((ж<any>)(uintptr)(target)).val = x;
    } else {
        ifaceE2I(typ.common(), x, target);
    }
    return new ΔValue(typ.common(), target.val, (flag)((flag)(v.flag.ro() | flagIndir) | ((flag)ΔInterface)));
}

// convertOp: interface -> interface
internal static ΔValue cvtI2I(ΔValue v, ΔType typ) {
    if (v.IsNil()) {
        var ret = Zero(typ);
        ret.flag |= (flag)(v.flag.ro());
        return ret;
    }
    return cvtT2I(v.Elem(), typ);
}

// implemented in ../runtime
//
//go:noescape
internal static partial nint chancap(@unsafe.Pointer ch);

//go:noescape
internal static partial void chanclose(@unsafe.Pointer ch);

//go:noescape
internal static partial nint chanlen(@unsafe.Pointer ch);

// Note: some of the noescape annotations below are technically a lie,
// but safe in the context of this package. Functions like chansend0
// and mapassign0 don't escape the referent, but may escape anything
// the referent points to (they do shallow copies of the referent).
// We add a 0 to their names and wrap them in functions with the
// proper escape behavior.

//go:noescape
internal static partial (bool selected, bool received) chanrecv(@unsafe.Pointer ch, bool nb, @unsafe.Pointer val);

//go:noescape
internal static partial bool chansend0(@unsafe.Pointer ch, @unsafe.Pointer val, bool nb);

internal static bool chansend(@unsafe.Pointer ch, @unsafe.Pointer val, bool nb) {
    contentEscapes(val.val);
    return chansend0(ch.val, val.val, nb);
}

internal static partial @unsafe.Pointer /*ch*/ makechan(ж<abi.Type> typ, nint size);

internal static partial @unsafe.Pointer /*m*/ makemap(ж<abi.Type> t, nint cap);

//go:noescape
internal static partial @unsafe.Pointer /*val*/ mapaccess(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key);

//go:noescape
internal static partial @unsafe.Pointer /*val*/ mapaccess_faststr(ж<abi.Type> t, @unsafe.Pointer m, @string key);

//go:noescape
internal static partial void mapassign0(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key, @unsafe.Pointer val);

// mapassign should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/modern-go/reflect2
//   - github.com/goccy/go-json
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mapassign
internal static void mapassign(ж<abi.Type> Ꮡt, @unsafe.Pointer m, @unsafe.Pointer key, @unsafe.Pointer val) {
    ref var t = ref Ꮡt.val;

    contentEscapes(key.val);
    contentEscapes(val.val);
    mapassign0(Ꮡt, m.val, key.val, val.val);
}

//go:noescape
internal static partial void mapassign_faststr0(ж<abi.Type> t, @unsafe.Pointer m, @string key, @unsafe.Pointer val);

internal static void mapassign_faststr(ж<abi.Type> Ꮡt, @unsafe.Pointer m, @string key, @unsafe.Pointer val) {
    ref var t = ref Ꮡt.val;

    contentEscapes(((ж<unsafeheader.String>)(uintptr)(new @unsafe.Pointer(Ꮡ(key)))).val.Data);
    contentEscapes(val.val);
    mapassign_faststr0(Ꮡt, m.val, key, val.val);
}

//go:noescape
internal static partial void mapdelete(ж<abi.Type> t, @unsafe.Pointer m, @unsafe.Pointer key);

//go:noescape
internal static partial void mapdelete_faststr(ж<abi.Type> t, @unsafe.Pointer m, @string key);

//go:noescape
internal static partial void mapiterinit(ж<abi.Type> t, @unsafe.Pointer m, ж<hiter> it);

//go:noescape
internal static partial @unsafe.Pointer /*key*/ mapiterkey(ж<hiter> it);

//go:noescape
internal static partial @unsafe.Pointer /*elem*/ mapiterelem(ж<hiter> it);

//go:noescape
internal static partial void mapiternext(ж<hiter> it);

//go:noescape
internal static partial nint maplen(@unsafe.Pointer m);

internal static partial void mapclear(ж<abi.Type> t, @unsafe.Pointer m);

// call calls fn with "stackArgsSize" bytes of stack arguments laid out
// at stackArgs and register arguments laid out in regArgs. frameSize is
// the total amount of stack space that will be reserved by call, so this
// should include enough space to spill register arguments to the stack in
// case of preemption.
//
// After fn returns, call copies stackArgsSize-stackRetOffset result bytes
// back into stackArgs+stackRetOffset before returning, for any return
// values passed on the stack. Register-based return values will be found
// in the same regArgs structure.
//
// regArgs must also be prepared with an appropriate ReturnIsPtr bitmap
// indicating which registers will contain pointer-valued return values. The
// purpose of this bitmap is to keep pointers visible to the GC between
// returning from reflectcall and actually using them.
//
// If copying result bytes back from the stack, the caller must pass the
// argument frame type as stackArgsType, so that call can execute appropriate
// write barriers during the copy.
//
// Arguments passed through to call do not escape. The type is used only in a
// very limited callee of call, the stackArgs are copied, and regArgs is only
// used in the call frame.
//
//go:noescape
//go:linkname call runtime.reflectcall
internal static partial void call(ж<abi.Type> stackArgsType, @unsafe.Pointer f, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void ifaceE2I(ж<abi.Type> t, any src, @unsafe.Pointer dst);

// memmove copies size bytes to dst from src. No write barriers are used.
//
//go:noescape
internal static partial void memmove(@unsafe.Pointer dst, @unsafe.Pointer src, uintptr size);

// typedmemmove copies a value of type t to dst from src.
//
//go:noescape
internal static partial void typedmemmove(ж<abi.Type> t, @unsafe.Pointer dst, @unsafe.Pointer src);

// typedmemclr zeros the value at ptr of type t.
//
//go:noescape
internal static partial void typedmemclr(ж<abi.Type> t, @unsafe.Pointer ptr);

// typedmemclrpartial is like typedmemclr but assumes that
// dst points off bytes into the value and only clears size bytes.
//
//go:noescape
internal static partial void typedmemclrpartial(ж<abi.Type> t, @unsafe.Pointer ptr, uintptr off, uintptr size);

// typedslicecopy copies a slice of elemType values from src to dst,
// returning the number of elements copied.
//
//go:noescape
internal static partial nint typedslicecopy(ж<abi.Type> t, unsafeheader.Slice dst, unsafeheader.Slice src);

// typedarrayclear zeroes the value at ptr of an array of elemType,
// only clears len elem.
//
//go:noescape
internal static partial void typedarrayclear(ж<abi.Type> elemType, @unsafe.Pointer ptr, nint len);

//go:noescape
internal static partial uintptr typehash(ж<abi.Type> t, @unsafe.Pointer p, uintptr h);

internal static partial bool verifyNotInHeapPtr(uintptr p);

//go:noescape
internal static partial unsafeheader.Slice growslice(ж<abi.Type> t, unsafeheader.Slice old, nint num);

//go:noescape
internal static partial void unsafeslice(ж<abi.Type> t, @unsafe.Pointer ptr, nint len);

// Dummy annotation marking that the value x escapes,
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
internal static void escapes(any x) {
    if (dummy.b) {
        dummy.x = x;
    }
}


[GoType("dyn")] partial struct dummyᴛ1 {
    internal bool b;
    internal any x;
}
internal static dummyᴛ1 dummy;

// Dummy annotation marking that the content of value x
// escapes (i.e. modeling roughly heap=*x),
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
internal static void contentEscapes(@unsafe.Pointer x) {
    if (dummy.b) {
        escapes(~(ж<any>)(uintptr)(x));
    }
}

// the dereference may not always be safe, but never executed

// This is just a wrapper around abi.NoEscape. The inlining heuristics are
// finnicky and for whatever reason treat the local call to noescape as much
// lower cost with respect to the inliner budget. (That is, replacing calls to
// noescape with abi.NoEscape will cause inlining tests to fail.)
//
//go:nosplit
internal static @unsafe.Pointer noescape(@unsafe.Pointer p) {
    return (uintptr)abi.NoEscape(p.val);
}

} // end reflect_package
