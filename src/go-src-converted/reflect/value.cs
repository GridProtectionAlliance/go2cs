// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflect -- go2cs converted at 2022 March 13 05:41:52 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Program Files\Go\src\reflect\value.go
namespace go;

using abi = @internal.abi_package;
using itoa = @internal.itoa_package;
using unsafeheader = @internal.unsafeheader_package;
using math = math_package;
using runtime = runtime_package;
using @unsafe = @unsafe_package;
using System;

public static partial class reflect_package {

private static readonly nint ptrSize = 4 << (int)((~uintptr(0) >> 63)); // unsafe.Sizeof(uintptr(0)) but an ideal const

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its IsValid method returns false, its Kind method returns Invalid,
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
 // unsafe.Sizeof(uintptr(0)) but an ideal const

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its IsValid method returns false, its Kind method returns Invalid,
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
public partial struct Value {
    public ptr<rtype> typ; // Pointer-valued data or, if flagIndir is set, pointer to data.
// Valid when either flagIndir is set or typ.pointers() is true.
    public unsafe.Pointer ptr; // flag holds metadata about the value.
// The lowest bits are flag bits:
//    - flagStickyRO: obtained via unexported not embedded field, so read-only
//    - flagEmbedRO: obtained via unexported embedded field, so read-only
//    - flagIndir: val holds a pointer to the data
//    - flagAddr: v.CanAddr is true (implies flagIndir)
//    - flagMethod: v is a method value.
// The next five bits give the Kind of the value.
// This repeats typ.Kind() except for method values.
// The remaining 23+ bits give a method number for method values.
// If flag.kind() != Func, code can assume that flagMethod is unset.
// If ifaceIndir(typ), code can assume that flagIndir is set.
    public ref flag flag => ref flag_val; // A method value represents a curried method invocation
// like r.Read for some receiver r. The typ+val+flag bits describe
// the receiver r, but the flag's Kind bits say Func (methods are
// functions), and the top bits of the flag give the method number
// in r's type's method table.
}

private partial struct flag { // : System.UIntPtr
}

private static readonly nint flagKindWidth = 5; // there are 27 kinds
private static readonly flag flagKindMask = 1 << (int)(flagKindWidth) - 1;
private static readonly flag flagStickyRO = 1 << 5;
private static readonly flag flagEmbedRO = 1 << 6;
private static readonly flag flagIndir = 1 << 7;
private static readonly flag flagAddr = 1 << 8;
private static readonly flag flagMethod = 1 << 9;
private static readonly nint flagMethodShift = 10;
private static readonly flag flagRO = flagStickyRO | flagEmbedRO;

private static Kind kind(this flag f) {
    return Kind(f & flagKindMask);
}

private static flag ro(this flag f) {
    if (f & flagRO != 0) {
        return flagStickyRO;
    }
    return 0;
}

// pointer returns the underlying pointer represented by v.
// v.Kind() must be Ptr, Map, Chan, Func, or UnsafePointer
// if v.Kind() == Ptr, the base type must not be go:notinheap.
public static unsafe.Pointer pointer(this Value v) => func((_, panic, _) => {
    if (v.typ.size != ptrSize || !v.typ.pointers()) {
        panic("can't call pointer on a non-pointer Value");
    }
    if (v.flag & flagIndir != 0) {
        return new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
    }
    return v.ptr;
});

// packEface converts v to the empty interface.
private static void packEface(Value v) => func((_, panic, _) => {
    var t = v.typ;
    ref var i = ref heap(out ptr<var> _addr_i);
    var e = (emptyInterface.val)(@unsafe.Pointer(_addr_i)); 
    // First, fill in the data portion of the interface.

    if (ifaceIndir(t)) 
        if (v.flag & flagIndir == 0) {
            panic("bad indir");
        }
        var ptr = v.ptr;
        if (v.flag & flagAddr != 0) { 
            // TODO: pass safe boolean from valueInterface so
            // we don't need to copy if safe==true?
            var c = unsafe_New(_addr_t);
            typedmemmove(_addr_t, c, ptr);
            ptr = c;
        }
        e.word = ptr;
    else if (v.flag & flagIndir != 0) 
        // Value is indirect, but interface is direct. We need
        // to load the data at v.ptr into the interface data word.
        e.word = new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
    else 
        // Value is direct, and so is the interface.
        e.word = v.ptr;
    // Now, fill in the type portion. We're very careful here not
    // to have any operation between the e.word and e.typ assignments
    // that would let the garbage collector observe the partially-built
    // interface value.
    e.typ = t;
    return i;
});

// unpackEface converts the empty interface i to a Value.
private static Value unpackEface(object i) {
    var e = (emptyInterface.val)(@unsafe.Pointer(_addr_i)); 
    // NOTE: don't read e.word until we know whether it is really a pointer or not.
    var t = e.typ;
    if (t == null) {
        return new Value();
    }
    var f = flag(t.Kind());
    if (ifaceIndir(t)) {
        f |= flagIndir;
    }
    return new Value(t,e.word,f);
}

// A ValueError occurs when a Value method is invoked on
// a Value that does not support it. Such cases are documented
// in the description of each method.
public partial struct ValueError {
    public @string Method;
    public Kind Kind;
}

private static @string Error(this ptr<ValueError> _addr_e) {
    ref ValueError e = ref _addr_e.val;

    if (e.Kind == 0) {
        return "reflect: call of " + e.Method + " on zero Value";
    }
    return "reflect: call of " + e.Method + " on " + e.Kind.String() + " Value";
}

// methodName returns the name of the calling method,
// assumed to be two stack frames above.
private static @string methodName() {
    var (pc, _, _, _) = runtime.Caller(2);
    var f = runtime.FuncForPC(pc);
    if (f == null) {
        return "unknown method";
    }
    return f.Name();
}

// methodNameSkip is like methodName, but skips another stack frame.
// This is a separate function so that reflect.flag.mustBe will be inlined.
private static @string methodNameSkip() {
    var (pc, _, _, _) = runtime.Caller(3);
    var f = runtime.FuncForPC(pc);
    if (f == null) {
        return "unknown method";
    }
    return f.Name();
}

// emptyInterface is the header for an interface{} value.
private partial struct emptyInterface {
    public ptr<rtype> typ;
    public unsafe.Pointer word;
}

// nonEmptyInterface is the header for an interface value with methods.
private partial struct nonEmptyInterface {
    public unsafe.Pointer word;
}

// mustBe panics if f's kind is not expected.
// Making this a method on flag instead of on Value
// (and embedding flag in Value) means that we can write
// the very clear v.mustBe(Bool) and have it compile into
// v.flag.mustBe(Bool), which will only bother to copy the
// single important word for the receiver.
private static void mustBe(this flag f, Kind expected) => func((_, panic, _) => { 
    // TODO(mvdan): use f.kind() again once mid-stack inlining gets better
    if (Kind(f & flagKindMask) != expected) {
        panic(addr(new ValueError(methodName(),f.kind())));
    }
});

// mustBeExported panics if f records that the value was obtained using
// an unexported field.
private static void mustBeExported(this flag f) {
    if (f == 0 || f & flagRO != 0) {
        f.mustBeExportedSlow();
    }
}

private static void mustBeExportedSlow(this flag f) => func((_, panic, _) => {
    if (f == 0) {
        panic(addr(new ValueError(methodNameSkip(),Invalid)));
    }
    if (f & flagRO != 0) {
        panic("reflect: " + methodNameSkip() + " using value obtained using unexported field");
    }
});

// mustBeAssignable panics if f records that the value is not assignable,
// which is to say that either it was obtained using an unexported field
// or it is not addressable.
private static void mustBeAssignable(this flag f) {
    if (f & flagRO != 0 || f & flagAddr == 0) {
        f.mustBeAssignableSlow();
    }
}

private static void mustBeAssignableSlow(this flag f) => func((_, panic, _) => {
    if (f == 0) {
        panic(addr(new ValueError(methodNameSkip(),Invalid)));
    }
    if (f & flagRO != 0) {
        panic("reflect: " + methodNameSkip() + " using value obtained using unexported field");
    }
    if (f & flagAddr == 0) {
        panic("reflect: " + methodNameSkip() + " using unaddressable value");
    }
});

// Addr returns a pointer value representing the address of v.
// It panics if CanAddr() returns false.
// Addr is typically used to obtain a pointer to a struct field
// or slice element in order to call a method that requires a
// pointer receiver.
public static Value Addr(this Value v) => func((_, panic, _) => {
    if (v.flag & flagAddr == 0) {
        panic("reflect.Value.Addr of unaddressable value");
    }
    var fl = v.flag & flagRO;
    return new Value(v.typ.ptrTo(),v.ptr,fl|flag(Ptr));
});

// Bool returns v's underlying value.
// It panics if v's kind is not Bool.
public static bool Bool(this Value v) {
    v.mustBe(Bool);
    return new ptr<ptr<ptr<bool>>>(v.ptr);
}

// Bytes returns v's underlying value.
// It panics if v's underlying value is not a slice of bytes.
public static slice<byte> Bytes(this Value v) => func((_, panic, _) => {
    v.mustBe(Slice);
    if (v.typ.Elem().Kind() != Uint8) {
        panic("reflect.Value.Bytes of non-byte slice");
    }
    return new ptr<ptr<ptr<slice<byte>>>>(v.ptr);
});

// runes returns v's underlying value.
// It panics if v's underlying value is not a slice of runes (int32s).
public static slice<int> runes(this Value v) => func((_, panic, _) => {
    v.mustBe(Slice);
    if (v.typ.Elem().Kind() != Int32) {
        panic("reflect.Value.Bytes of non-rune slice");
    }
    return new ptr<ptr<ptr<slice<int>>>>(v.ptr);
});

// CanAddr reports whether the value's address can be obtained with Addr.
// Such values are called addressable. A value is addressable if it is
// an element of a slice, an element of an addressable array,
// a field of an addressable struct, or the result of dereferencing a pointer.
// If CanAddr returns false, calling Addr will panic.
public static bool CanAddr(this Value v) {
    return v.flag & flagAddr != 0;
}

// CanSet reports whether the value of v can be changed.
// A Value can be changed only if it is addressable and was not
// obtained by the use of unexported struct fields.
// If CanSet returns false, calling Set or any type-specific
// setter (e.g., SetBool, SetInt) will panic.
public static bool CanSet(this Value v) {
    return v.flag & (flagAddr | flagRO) == flagAddr;
}

// Call calls the function v with the input arguments in.
// For example, if len(in) == 3, v.Call(in) represents the Go call v(in[0], in[1], in[2]).
// Call panics if v's Kind is not Func.
// It returns the output results as Values.
// As in Go, each input argument must be assignable to the
// type of the function's corresponding input parameter.
// If v is a variadic function, Call creates the variadic slice parameter
// itself, copying in the corresponding values.
public static slice<Value> Call(this Value v, slice<Value> @in) {
    v.mustBe(Func);
    v.mustBeExported();
    return v.call("Call", in);
}

// CallSlice calls the variadic function v with the input arguments in,
// assigning the slice in[len(in)-1] to v's final variadic argument.
// For example, if len(in) == 3, v.CallSlice(in) represents the Go call v(in[0], in[1], in[2]...).
// CallSlice panics if v's Kind is not Func or if v is not variadic.
// It returns the output results as Values.
// As in Go, each input argument must be assignable to the
// type of the function's corresponding input parameter.
public static slice<Value> CallSlice(this Value v, slice<Value> @in) {
    v.mustBe(Func);
    v.mustBeExported();
    return v.call("CallSlice", in);
}

private static bool callGC = default; // for testing; see TestCallMethodJump

private static readonly var debugReflectCall = false;



public static slice<Value> call(this Value v, @string op, slice<Value> @in) => func((_, panic, _) => { 
    // Get function pointer, type.
    var t = (funcType.val)(@unsafe.Pointer(v.typ));
    unsafe.Pointer fn = default;    Value rcvr = default;    ptr<rtype> rcvrtype;
    if (v.flag & flagMethod != 0) {
        rcvr = v;
        rcvrtype, t, fn = methodReceiver(op, v, int(v.flag) >> (int)(flagMethodShift));
    }
    else if (v.flag & flagIndir != 0) {
        fn = new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
    }
    else
 {
        fn = v.ptr;
    }
    if (fn == null) {
        panic("reflect.Value.Call: call of nil function");
    }
    var isSlice = op == "CallSlice";
    var n = t.NumIn();
    var isVariadic = t.IsVariadic();
    if (isSlice) {
        if (!isVariadic) {
            panic("reflect: CallSlice of non-variadic function");
        }
        if (len(in) < n) {
            panic("reflect: CallSlice with too few input arguments");
        }
        if (len(in) > n) {
            panic("reflect: CallSlice with too many input arguments");
        }
    }
    else
 {
        if (isVariadic) {
            n--;
        }
        if (len(in) < n) {
            panic("reflect: Call with too few input arguments");
        }
        if (!isVariadic && len(in) > n) {
            panic("reflect: Call with too many input arguments");
        }
    }
    {
        var x__prev1 = x;

        foreach (var (_, __x) in in) {
            x = __x;
            if (x.Kind() == Invalid) {
                panic("reflect: " + op + " using zero Value argument");
            }
        }
        x = x__prev1;
    }

    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            {
                var xt__prev1 = xt;
                var targ__prev1 = targ;

                var xt = in[i].Type();
                var targ = t.In(i);

                if (!xt.AssignableTo(targ)) {
                    panic("reflect: " + op + " using " + xt.String() + " as type " + targ.String());
                }

                xt = xt__prev1;
                targ = targ__prev1;

            }
        }

        i = i__prev1;
    }
    if (!isSlice && isVariadic) { 
        // prepare slice for remaining values
        var m = len(in) - n;
        var slice = MakeSlice(t.In(n), m, m);
        var elem = t.In(n).Elem();
        {
            nint i__prev1 = i;

            for (i = 0; i < m; i++) {
                var x = in[n + i];
                {
                    var xt__prev2 = xt;

                    xt = x.Type();

                    if (!xt.AssignableTo(elem)) {
                        panic("reflect: cannot use " + xt.String() + " as type " + elem.String() + " in " + op);
                    }

                    xt = xt__prev2;

                }
                slice.Index(i).Set(x);
            }


            i = i__prev1;
        }
        var origIn = in;
        in = make_slice<Value>(n + 1);
        copy(in[..(int)n], origIn);
        in[n] = slice;
    }
    var nin = len(in);
    if (nin != t.NumIn()) {
        panic("reflect.Value.Call: wrong argument count");
    }
    var nout = t.NumOut(); 

    // Register argument space.
    ref abi.RegArgs regArgs = ref heap(out ptr<abi.RegArgs> _addr_regArgs); 

    // Compute frame type.
    var (frametype, framePool, abi) = funcLayout(t, rcvrtype); 

    // Allocate a chunk of memory for frame if needed.
    unsafe.Pointer stackArgs = default;
    if (frametype.size != 0) {
        if (nout == 0) {
            stackArgs = framePool.Get()._<unsafe.Pointer>();
        }
        else
 { 
            // Can't use pool if the function has return values.
            // We will leak pointer to args in ret, so its lifetime is not scoped.
            stackArgs = unsafe_New(_addr_frametype);
        }
    }
    var frameSize = frametype.size;

    if (debugReflectCall) {
        println("reflect.call", t.String());
        abi.dump();
    }
    nint inStart = 0;
    if (rcvrtype != null) { 
        // Guaranteed to only be one word in size,
        // so it will only take up exactly 1 abiStep (either
        // in a register or on the stack).
        {
            var st__prev1 = st;

            var st = abi.call.steps[0];


            if (st.kind == abiStepStack) 
                storeRcvr(rcvr, stackArgs);
            else if (st.kind == abiStepIntReg || st.kind == abiStepPointer) 
                // Even pointers can go into the uintptr slot because
                // they'll be kept alive by the Values referenced by
                // this frame. Reflection forces these to be heap-allocated,
                // so we don't need to worry about stack copying.
                storeRcvr(rcvr, @unsafe.Pointer(_addr_regArgs.Ints[st.ireg]));
            else if (st.kind == abiStepFloatReg) 
                storeRcvr(rcvr, @unsafe.Pointer(_addr_regArgs.Floats[st.freg]));
            else 
                panic("unknown ABI parameter kind");


            st = st__prev1;
        }
        inStart = 1;
    }
    {
        nint i__prev1 = i;

        foreach (var (__i, __v) in in) {
            i = __i;
            v = __v;
            v.mustBeExported();
            targ = t.In(i)._<ptr<rtype>>(); 
            // TODO(mknyszek): Figure out if it's possible to get some
            // scratch space for this assignment check. Previously, it
            // was possible to use space in the argument frame.
            v = v.assignTo("reflect.Value.Call", targ, null);
stepsLoop:
            {
                var st__prev2 = st;

                foreach (var (_, __st) in abi.call.stepsForValue(i + inStart)) {
                    st = __st;

                    if (st.kind == abiStepStack) 
                        // Copy values to the "stack."
                        var addr = add(stackArgs, st.stkOff, "precomputed stack arg offset");
                        if (v.flag & flagIndir != 0) {
                            typedmemmove(_addr_targ, addr, v.ptr);
                        }
                        else
 {
                            (@unsafe.Pointer.val).val;

                            (addr) = v.ptr;
                        } 
                        // There's only one step for a stack-allocated value.
                        _breakstepsLoop = true;
                        break;
                    else if (st.kind == abiStepIntReg || st.kind == abiStepPointer) 
                        // Copy values to "integer registers."
                        if (v.flag & flagIndir != 0) {
                            var offset = add(v.ptr, st.offset, "precomputed value offset");
                            memmove(@unsafe.Pointer(_addr_regArgs.Ints[st.ireg]), offset, st.size);
                        }
                        else
 {
                            if (st.kind == abiStepPointer) { 
                                // Duplicate this pointer in the pointer area of the
                                // register space. Otherwise, there's the potential for
                                // this to be the last reference to v.ptr.
                                regArgs.Ptrs[st.ireg] = v.ptr;
                            }
                            regArgs.Ints[st.ireg] = uintptr(v.ptr);
                        }
                    else if (st.kind == abiStepFloatReg) 
                        // Copy values to "float registers."
                        if (v.flag & flagIndir == 0) {
                            panic("attempted to copy pointer to FP register");
                        }
                        offset = add(v.ptr, st.offset, "precomputed value offset");
                        memmove(@unsafe.Pointer(_addr_regArgs.Floats[st.freg]), offset, st.size);
                    else 
                        panic("unknown ABI part kind");
                                    }

                st = st__prev2;
            }
        }
        i = i__prev1;
    }

    frameSize = align(frameSize, ptrSize);
    frameSize += abi.spill; 

    // Mark pointers in registers for the return path.
    regArgs.ReturnIsPtr = abi.outRegPtrs; 

    // Call.
    call(_addr_frametype, fn, stackArgs, uint32(frametype.size), uint32(abi.retOffset), uint32(frameSize), _addr_regArgs); 

    // For testing; see TestCallMethodJump.
    if (callGC) {
        runtime.GC();
    }
    slice<Value> ret = default;
    if (nout == 0) {
        if (stackArgs != null) {
            typedmemclr(_addr_frametype, stackArgs);
            framePool.Put(stackArgs);
        }
    }
    else
 {
        if (stackArgs != null) { 
            // Zero the now unused input area of args,
            // because the Values returned by this function contain pointers to the args object,
            // and will thus keep the args object alive indefinitely.
            typedmemclrpartial(_addr_frametype, stackArgs, 0, abi.retOffset);
        }
        ret = make_slice<Value>(nout);
        {
            nint i__prev1 = i;

            for (i = 0; i < nout; i++) {
                var tv = t.Out(i);
                if (tv.Size() == 0) { 
                    // For zero-sized return value, args+off may point to the next object.
                    // In this case, return the zero value instead.
                    ret[i] = Zero(tv);
                    continue;
                }
                var steps = abi.ret.stepsForValue(i);
                {
                    var st__prev2 = st;

                    st = steps[0];

                    if (st.kind == abiStepStack) { 
                        // This value is on the stack. If part of a value is stack
                        // allocated, the entire value is according to the ABI. So
                        // just make an indirection into the allocated frame.
                        var fl = flagIndir | flag(tv.Kind());
                        ret[i] = new Value(tv.common(),add(stackArgs,st.stkOff,"tv.Size() != 0"),fl); 
                        // Note: this does introduce false sharing between results -
                        // if any result is live, they are all live.
                        // (And the space for the args is live as well, but as we've
                        // cleared that space it isn't as big a deal.)
                        continue;
                    } 

                    // Handle pointers passed in registers.

                    st = st__prev2;

                } 

                // Handle pointers passed in registers.
                if (!ifaceIndir(tv.common())) { 
                    // Pointer-valued data gets put directly
                    // into v.ptr.
                    if (steps[0].kind != abiStepPointer) {
                        print("kind=", steps[0].kind, ", type=", tv.String(), "\n");
                        panic("mismatch between ABI description and types");
                    }
                    ret[i] = new Value(tv.common(),regArgs.Ptrs[steps[0].ireg],flag(tv.Kind()));
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
                var s = unsafe_New(_addr_tv.common());
                {
                    var st__prev2 = st;

                    foreach (var (_, __st) in steps) {
                        st = __st;

                        if (st.kind == abiStepIntReg) 
                            offset = add(s, st.offset, "precomputed value offset");
                            memmove(offset, @unsafe.Pointer(_addr_regArgs.Ints[st.ireg]), st.size);
                        else if (st.kind == abiStepPointer) 
                            s = add(s, st.offset, "precomputed value offset");
                            ((@unsafe.Pointer.val)(s)).val = regArgs.Ptrs[st.ireg];
                        else if (st.kind == abiStepFloatReg) 
                            offset = add(s, st.offset, "precomputed value offset");
                            memmove(offset, @unsafe.Pointer(_addr_regArgs.Floats[st.freg]), st.size);
                        else if (st.kind == abiStepStack) 
                            panic("register-based return value has stack component");
                        else 
                            panic("unknown ABI part kind");
                                            }

                    st = st__prev2;
                }

                ret[i] = new Value(tv.common(),s,flagIndir|flag(tv.Kind()));
            }


            i = i__prev1;
        }
    }
    return ret;
});

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
private static void callReflect(ptr<makeFuncImpl> _addr_ctxt, unsafe.Pointer frame, ptr<bool> _addr_retValid, ptr<abi.RegArgs> _addr_regs) => func((_, panic, _) => {
    ref makeFuncImpl ctxt = ref _addr_ctxt.val;
    ref bool retValid = ref _addr_retValid.val;
    ref abi.RegArgs regs = ref _addr_regs.val;

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

    var (_, _, abi) = funcLayout(ftyp, null); 

    // Copy arguments into Values.
    var ptr = frame;
    var @in = make_slice<Value>(0, int(ftyp.inCount));
    {
        var i__prev1 = i;
        var typ__prev1 = typ;

        foreach (var (__i, __typ) in ftyp.@in()) {
            i = __i;
            typ = __typ;
            if (typ.Size() == 0) {
                in = append(in, Zero(typ));
                continue;
            }
            Value v = new Value(typ,nil,flag(typ.Kind()));
            var steps = abi.call.stepsForValue(i);
            {
                var st__prev1 = st;

                var st = steps[0];

                if (st.kind == abiStepStack) {
                    if (ifaceIndir(typ)) { 
                        // value cannot be inlined in interface data.
                        // Must make a copy, because f might keep a reference to it,
                        // and we cannot let f keep a reference to the stack frame
                        // after this function returns, not even a read-only reference.
                        v.ptr = unsafe_New(_addr_typ);
                        if (typ.size > 0) {
                            typedmemmove(_addr_typ, v.ptr, add(ptr, st.stkOff, "typ.size > 0"));
                        }
                        v.flag |= flagIndir;
                    }
                    else
 {
                        v.ptr = new ptr<ptr<ptr<unsafe.Pointer>>>(add(ptr, st.stkOff, "1-ptr"));
                    }
                }
                else
 {
                    if (ifaceIndir(typ)) { 
                        // All that's left is values passed in registers that we need to
                        // create space for the values.
                        v.flag |= flagIndir;
                        v.ptr = unsafe_New(_addr_typ);
                        {
                            var st__prev2 = st;

                            foreach (var (_, __st) in steps) {
                                st = __st;

                                if (st.kind == abiStepIntReg) 
                                    var offset = add(v.ptr, st.offset, "precomputed value offset");
                                    memmove(offset, @unsafe.Pointer(_addr_regs.Ints[st.ireg]), st.size);
                                else if (st.kind == abiStepPointer) 
                                    var s = add(v.ptr, st.offset, "precomputed value offset");
                                    ((@unsafe.Pointer.val)(s)).val = regs.Ptrs[st.ireg];
                                else if (st.kind == abiStepFloatReg) 
                                    offset = add(v.ptr, st.offset, "precomputed value offset");
                                    memmove(offset, @unsafe.Pointer(_addr_regs.Floats[st.freg]), st.size);
                                else if (st.kind == abiStepStack) 
                                    panic("register-based return value has stack component");
                                else 
                                    panic("unknown ABI part kind");
                                                            }
                    else

                            st = st__prev2;
                        }
                    } { 
                        // Pointer-valued data gets put directly
                        // into v.ptr.
                        if (steps[0].kind != abiStepPointer) {
                            print("kind=", steps[0].kind, ", type=", typ.String(), "\n");
                            panic("mismatch between ABI description and types");
                        }
                        v.ptr = regs.Ptrs[steps[0].ireg];
                    }
                }

                st = st__prev1;

            }
            in = append(in, v);
        }
        i = i__prev1;
        typ = typ__prev1;
    }

    var @out = f(in);
    var numOut = ftyp.NumOut();
    if (len(out) != numOut) {
        panic("reflect: wrong return count from function created by MakeFunc");
    }
    if (numOut > 0) {
        {
            var i__prev1 = i;
            var typ__prev1 = typ;

            foreach (var (__i, __typ) in ftyp.@out()) {
                i = __i;
                typ = __typ;
                v = out[i];
                if (v.typ == null) {
                    panic("reflect: function created by MakeFunc using " + funcName(f) + " returned zero Value");
                }
                if (v.flag & flagRO != 0) {
                    panic("reflect: function created by MakeFunc using " + funcName(f) + " returned value obtained from unexported field");
                }
                if (typ.size == 0) {
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
                v = v.assignTo("reflect.MakeFunc", typ, null);
stepsLoop:
                {
                    var st__prev2 = st;

                    foreach (var (_, __st) in abi.ret.stepsForValue(i)) {
                        st = __st;

                        if (st.kind == abiStepStack) 
                            // Copy values to the "stack."
                            var addr = add(ptr, st.stkOff, "precomputed stack arg offset"); 
                            // Do not use write barriers. The stack space used
                            // for this call is not adequately zeroed, and we
                            // are careful to keep the arguments alive until we
                            // return to makeFuncStub's caller.
                            if (v.flag & flagIndir != 0) {
                                memmove(addr, v.ptr, st.size);
                            }
                            else
 { 
                                // This case must be a pointer type.
                                (uintptr.val).val;

                                (addr) = uintptr(v.ptr);
                            } 
                            // There's only one step for a stack-allocated value.
                            _breakstepsLoop = true;
                            break;
                        else if (st.kind == abiStepIntReg || st.kind == abiStepPointer) 
                            // Copy values to "integer registers."
                            if (v.flag & flagIndir != 0) {
                                offset = add(v.ptr, st.offset, "precomputed value offset");
                                memmove(@unsafe.Pointer(_addr_regs.Ints[st.ireg]), offset, st.size);
                            }
                            else
 { 
                                // Only populate the Ints space on the return path.
                                // This is safe because out is kept alive until the
                                // end of this function, and the return path through
                                // makeFuncStub has no preemption, so these pointers
                                // are always visible to the GC.
                                regs.Ints[st.ireg] = uintptr(v.ptr);
                            }
                        else if (st.kind == abiStepFloatReg) 
                            // Copy values to "float registers."
                            if (v.flag & flagIndir == 0) {
                                panic("attempted to copy pointer to FP register");
                            }
                            offset = add(v.ptr, st.offset, "precomputed value offset");
                            memmove(@unsafe.Pointer(_addr_regs.Floats[st.freg]), offset, st.size);
                        else 
                            panic("unknown ABI part kind");
                                            }

                    st = st__prev2;
                }
            }

            i = i__prev1;
            typ = typ__prev1;
        }
    }
    retValid = true; 

    // We have to make sure that the out slice lives at least until
    // the runtime knows the return values are valid. Otherwise, the
    // return values might not be scanned by anyone during a GC.
    // (out would be dead, and the return slots not yet alive.)
    runtime.KeepAlive(out); 

    // runtime.getArgInfo expects to be able to find ctxt on the
    // stack when it finds our caller, makeFuncStub. Make sure it
    // doesn't get garbage collected.
    runtime.KeepAlive(ctxt);
});

// methodReceiver returns information about the receiver
// described by v. The Value v may or may not have the
// flagMethod bit set, so the kind cached in v.flag should
// not be used.
// The return value rcvrtype gives the method's actual receiver type.
// The return value t gives the method type signature (without the receiver).
// The return value fn is a pointer to the method code.
private static (ptr<rtype>, ptr<funcType>, unsafe.Pointer) methodReceiver(@string op, Value v, nint methodIndex) => func((_, panic, _) => {
    ptr<rtype> rcvrtype = default!;
    ptr<funcType> t = default!;
    unsafe.Pointer fn = default;

    var i = methodIndex;
    if (v.typ.Kind() == Interface) {
        var tt = (interfaceType.val)(@unsafe.Pointer(v.typ));
        if (uint(i) >= uint(len(tt.methods))) {
            panic("reflect: internal error: invalid method index");
        }
        var m = _addr_tt.methods[i];
        if (!tt.nameOff(m.name).isExported()) {
            panic("reflect: " + op + " of unexported method");
        }
        var iface = (nonEmptyInterface.val)(v.ptr);
        if (iface.itab == null) {
            panic("reflect: " + op + " of method on nil interface value");
        }
        rcvrtype = iface.itab.typ;
        fn = @unsafe.Pointer(_addr_iface.itab.fun[i]);
        t = (funcType.val)(@unsafe.Pointer(tt.typeOff(m.typ)));
    }
    else
 {
        rcvrtype = v.typ;
        var ms = v.typ.exportedMethods();
        if (uint(i) >= uint(len(ms))) {
            panic("reflect: internal error: invalid method index");
        }
        m = ms[i];
        if (!v.typ.nameOff(m.name).isExported()) {
            panic("reflect: " + op + " of unexported method");
        }
        ref var ifn = ref heap(v.typ.textOff(m.ifn), out ptr<var> _addr_ifn);
        fn = @unsafe.Pointer(_addr_ifn);
        t = (funcType.val)(@unsafe.Pointer(v.typ.typeOff(m.mtyp)));
    }
    return ;
});

// v is a method receiver. Store at p the word which is used to
// encode that receiver at the start of the argument list.
// Reflect uses the "interface" calling convention for
// methods, which always uses one word to record the receiver.
private static void storeRcvr(Value v, unsafe.Pointer p) {
    var t = v.typ;
    if (t.Kind() == Interface) { 
        // the interface data word becomes the receiver word
        var iface = (nonEmptyInterface.val)(v.ptr) * (@unsafe.Pointer.val);

        (p) = iface.word;
    }
    else if (v.flag & flagIndir != 0 && !ifaceIndir(t)) {
        (@unsafe.Pointer.val).val;

        (p) = new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
    }
    else
 {
        (@unsafe.Pointer.val).val;

        (p) = v.ptr;
    }
}

// align returns the result of rounding x up to a multiple of n.
// n must be a power of two.
private static System.UIntPtr align(System.UIntPtr x, System.UIntPtr n) {
    return (x + n - 1) & ~(n - 1);
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
// ctxt is the "closure" generated by makeVethodValue.
// frame is a pointer to the arguments to that closure on the stack.
// retValid points to a boolean which should be set when the results
// section of frame is set.
//
// regs contains the argument values passed in registers and will contain
// the values returned from ctxt.fn in registers.
private static void callMethod(ptr<methodValue> _addr_ctxt, unsafe.Pointer frame, ptr<bool> _addr_retValid, ptr<abi.RegArgs> _addr_regs) => func((_, panic, _) => {
    ref methodValue ctxt = ref _addr_ctxt.val;
    ref bool retValid = ref _addr_retValid.val;
    ref abi.RegArgs regs = ref _addr_regs.val;

    var rcvr = ctxt.rcvr;
    var (rcvrType, valueFuncType, methodFn) = methodReceiver("call", rcvr, ctxt.method); 

    // There are two ABIs at play here.
    //
    // methodValueCall was invoked with the ABI assuming there was no
    // receiver ("value ABI") and that's what frame and regs are holding.
    //
    // Meanwhile, we need to actually call the method with a receiver, which
    // has its own ABI ("method ABI"). Everything that follows is a translation
    // between the two.
    var (_, _, valueABI) = funcLayout(valueFuncType, null);
    var valueFrame = frame;
    var valueRegs = regs;
    var (methodFrameType, methodFramePool, methodABI) = funcLayout(valueFuncType, rcvrType); 

    // Make a new frame that is one word bigger so we can store the receiver.
    // This space is used for both arguments and return values.
    unsafe.Pointer methodFrame = methodFramePool.Get()._<unsafe.Pointer>();
    ref abi.RegArgs methodRegs = ref heap(out ptr<abi.RegArgs> _addr_methodRegs); 

    // Deal with the receiver. It's guaranteed to only be one word in size.
    {
        var st = methodABI.call.steps[0];

        if (st.kind == abiStepStack) { 
            // Only copy the reciever to the stack if the ABI says so.
            // Otherwise, it'll be in a register already.
            storeRcvr(rcvr, methodFrame);
        }
        else
 { 
            // Put the receiver in a register.
            storeRcvr(rcvr, @unsafe.Pointer(_addr_methodRegs.Ints));
        }
    } 

    // Translate the rest of the arguments.
    {
        var i__prev1 = i;

        foreach (var (__i, __t) in valueFuncType.@in()) {
            i = __i;
            t = __t;
            var valueSteps = valueABI.call.stepsForValue(i);
            var methodSteps = methodABI.call.stepsForValue(i + 1); 

            // Zero-sized types are trivial: nothing to do.
            if (len(valueSteps) == 0) {
                if (len(methodSteps) != 0) {
                    panic("method ABI and value ABI do not align");
                }
                continue;
            } 

            // There are four cases to handle in translating each
            // argument:
            // 1. Stack -> stack translation.
            // 2. Stack -> registers translation.
            // 3. Registers -> stack translation.
            // 4. Registers -> registers translation.
            // TODO(mknyszek): Cases 2 and 3 below only work on little endian
            // architectures. This is OK for now, but this needs to be fixed
            // before supporting the register ABI on big endian architectures.

            // If the value ABI passes the value on the stack,
            // then the method ABI does too, because it has strictly
            // fewer arguments. Simply copy between the two.
            {
                var vStep__prev1 = vStep;

                var vStep = valueSteps[0];

                if (vStep.kind == abiStepStack) {
                    var mStep = methodSteps[0]; 
                    // Handle stack -> stack translation.
                    if (mStep.kind == abiStepStack) {
                        if (vStep.size != mStep.size) {
                            panic("method ABI and value ABI do not align");
                        }
                        typedmemmove(_addr_t, add(methodFrame, mStep.stkOff, "precomputed stack offset"), add(valueFrame, vStep.stkOff, "precomputed stack offset"));
                        continue;
                    } 
                    // Handle stack -> register translation.
                    {
                        var mStep__prev2 = mStep;

                        foreach (var (_, __mStep) in methodSteps) {
                            mStep = __mStep;
                            var from = add(valueFrame, vStep.stkOff + mStep.offset, "precomputed stack offset");

                            if (mStep.kind == abiStepPointer) 
                            {
                                // Do the pointer copy directly so we get a write barrier.
                                methodRegs.Ptrs[mStep.ireg] = new ptr<ptr<ptr<unsafe.Pointer>>>(from);
                                fallthrough = true; // We need to make sure this ends up in Ints, too.
                            }
                            if (fallthrough || mStep.kind == abiStepIntReg)
                            {
                                memmove(@unsafe.Pointer(_addr_methodRegs.Ints[mStep.ireg]), from, mStep.size);
                                goto __switch_break0;
                            }
                            if (mStep.kind == abiStepFloatReg)
                            {
                                memmove(@unsafe.Pointer(_addr_methodRegs.Floats[mStep.freg]), from, mStep.size);
                                goto __switch_break0;
                            }
                            // default: 
                                panic("unexpected method step");

                            __switch_break0:;
                        }

                        mStep = mStep__prev2;
                    }

                    continue;
                } 
                // Handle register -> stack translation.

                vStep = vStep__prev1;

            } 
            // Handle register -> stack translation.
            {
                var mStep__prev1 = mStep;

                mStep = methodSteps[0];

                if (mStep.kind == abiStepStack) {
                    {
                        var vStep__prev2 = vStep;

                        foreach (var (_, __vStep) in valueSteps) {
                            vStep = __vStep;
                            var to = add(methodFrame, mStep.stkOff + vStep.offset, "precomputed stack offset");

                            if (vStep.kind == abiStepPointer) 
                                // Do the pointer copy directly so we get a write barrier.
                                (@unsafe.Pointer.val).val;

                                (to) = valueRegs.Ptrs[vStep.ireg];
                            else if (vStep.kind == abiStepIntReg) 
                                memmove(to, @unsafe.Pointer(_addr_valueRegs.Ints[vStep.ireg]), vStep.size);
                            else if (vStep.kind == abiStepFloatReg) 
                                memmove(to, @unsafe.Pointer(_addr_valueRegs.Floats[vStep.freg]), vStep.size);
                            else 
                                panic("unexpected value step");
                                                    }

                        vStep = vStep__prev2;
                    }

                    continue;
                } 
                // Handle register -> register translation.

                mStep = mStep__prev1;

            } 
            // Handle register -> register translation.
            if (len(valueSteps) != len(methodSteps)) { 
                // Because it's the same type for the value, and it's assigned
                // to registers both times, it should always take up the same
                // number of registers for each ABI.
                panic("method ABI and value ABI don't align");
            }
            {
                var i__prev2 = i;
                var vStep__prev2 = vStep;

                foreach (var (__i, __vStep) in valueSteps) {
                    i = __i;
                    vStep = __vStep;
                    mStep = methodSteps[i];
                    if (mStep.kind != vStep.kind) {
                        panic("method ABI and value ABI don't align");
                    }

                    if (vStep.kind == abiStepPointer) 
                    {
                        // Copy this too, so we get a write barrier.
                        methodRegs.Ptrs[mStep.ireg] = valueRegs.Ptrs[vStep.ireg];
                        fallthrough = true;
                    }
                    if (fallthrough || vStep.kind == abiStepIntReg)
                    {
                        methodRegs.Ints[mStep.ireg] = valueRegs.Ints[vStep.ireg];
                        goto __switch_break1;
                    }
                    if (vStep.kind == abiStepFloatReg)
                    {
                        methodRegs.Floats[mStep.freg] = valueRegs.Floats[vStep.freg];
                        goto __switch_break1;
                    }
                    // default: 
                        panic("unexpected value step");

                    __switch_break1:;
                }

                i = i__prev2;
                vStep = vStep__prev2;
            }
        }
        i = i__prev1;
    }

    var methodFrameSize = methodFrameType.size; 
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    methodFrameSize = align(methodFrameSize, ptrSize);
    methodFrameSize += methodABI.spill; 

    // Mark pointers in registers for the return path.
    methodRegs.ReturnIsPtr = methodABI.outRegPtrs; 

    // Call.
    // Call copies the arguments from scratch to the stack, calls fn,
    // and then copies the results back into scratch.
    call(_addr_methodFrameType, methodFn, methodFrame, uint32(methodFrameType.size), uint32(methodABI.retOffset), uint32(methodFrameSize), _addr_methodRegs); 

    // Copy return values.
    //
    // This is somewhat simpler because both ABIs have an identical
    // return value ABI (the types are identical). As a result, register
    // results can simply be copied over. Stack-allocated values are laid
    // out the same, but are at different offsets from the start of the frame
    // Ignore any changes to args.
    // Avoid constructing out-of-bounds pointers if there are no return values.
    // because the arguments may be laid out differently.
    if (valueRegs != null) {
        valueRegs.val = methodRegs;
    }
    {
        var retSize = methodFrameType.size - methodABI.retOffset;

        if (retSize > 0) {
            var valueRet = add(valueFrame, valueABI.retOffset, "valueFrame's size > retOffset");
            var methodRet = add(methodFrame, methodABI.retOffset, "methodFrame's size > retOffset"); 
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
    typedmemclr(_addr_methodFrameType, methodFrame);
    methodFramePool.Put(methodFrame); 

    // See the comment in callReflect.
    runtime.KeepAlive(ctxt); 

    // Keep valueRegs alive because it may hold live pointer results.
    // The caller (methodValueCall) has it as a stack object, which is only
    // scanned when there is a reference to it.
    runtime.KeepAlive(valueRegs);
});

// funcName returns the name of f, for use in error messages.
private static @string funcName(Func<slice<Value>, slice<Value>> f) {
    ptr<ptr<System.UIntPtr>> pc = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_f));
    var rf = runtime.FuncForPC(pc);
    if (rf != null) {
        return rf.Name();
    }
    return "closure";
}

// Cap returns v's capacity.
// It panics if v's Kind is not Array, Chan, or Slice.
public static nint Cap(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Array) 
        return v.typ.Len();
    else if (k == Chan) 
        return chancap(v.pointer());
    else if (k == Slice) 
        // Slice is always bigger than a word; assume flagIndir.
        return (unsafeheader.Slice.val)(v.ptr).Cap;
        panic(addr(new ValueError("reflect.Value.Cap",v.kind())));
});

// Close closes the channel v.
// It panics if v's Kind is not Chan.
public static void Close(this Value v) {
    v.mustBe(Chan);
    v.mustBeExported();
    chanclose(v.pointer());
}

// Complex returns v's underlying value, as a complex128.
// It panics if v's Kind is not Complex64 or Complex128
public static System.Numerics.Complex128 Complex(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Complex64) 
        return complex128(new ptr<ptr<ptr<complex64>>>(v.ptr));
    else if (k == Complex128) 
        return new ptr<ptr<ptr<System.Numerics.Complex128>>>(v.ptr);
        panic(addr(new ValueError("reflect.Value.Complex",v.kind())));
});

// Elem returns the value that the interface v contains
// or that the pointer v points to.
// It panics if v's Kind is not Interface or Ptr.
// It returns the zero Value if v is nil.
public static Value Elem(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Interface) 
        var eface = default;
        if (v.typ.NumMethod() == 0) {
        }
        else
 {
        }
        var x = unpackEface(eface);
        if (x.flag != 0) {
            x.flag |= v.flag.ro();
        }
        return x;
    else if (k == Ptr) 
        var ptr = v.ptr;
        if (v.flag & flagIndir != 0) {
            ptr = new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
        }
        if (ptr == null) {
            return new Value();
        }
        var tt = (ptrType.val)(@unsafe.Pointer(v.typ));
        var typ = tt.elem;
        var fl = v.flag & flagRO | flagIndir | flagAddr;
        fl |= flag(typ.Kind());
        return new Value(typ,ptr,fl);
        panic(addr(new ValueError("reflect.Value.Elem",v.kind())));
});

// Field returns the i'th field of the struct v.
// It panics if v's Kind is not Struct or i is out of range.
public static Value Field(this Value v, nint i) => func((_, panic, _) => {
    if (v.kind() != Struct) {
        panic(addr(new ValueError("reflect.Value.Field",v.kind())));
    }
    var tt = (structType.val)(@unsafe.Pointer(v.typ));
    if (uint(i) >= uint(len(tt.fields))) {
        panic("reflect: Field index out of range");
    }
    var field = _addr_tt.fields[i];
    var typ = field.typ; 

    // Inherit permission bits from v, but clear flagEmbedRO.
    var fl = v.flag & (flagStickyRO | flagIndir | flagAddr) | flag(typ.Kind()); 
    // Using an unexported field forces flagRO.
    if (!field.name.isExported()) {
        if (field.embedded()) {
            fl |= flagEmbedRO;
        }
        else
 {
            fl |= flagStickyRO;
        }
    }
    var ptr = add(v.ptr, field.offset(), "same as non-reflect &v.field");
    return new Value(typ,ptr,fl);
});

// FieldByIndex returns the nested field corresponding to index.
// It panics if v's Kind is not struct.
public static Value FieldByIndex(this Value v, slice<nint> index) => func((_, panic, _) => {
    if (len(index) == 1) {
        return v.Field(index[0]);
    }
    v.mustBe(Struct);
    foreach (var (i, x) in index) {
        if (i > 0) {
            if (v.Kind() == Ptr && v.typ.Elem().Kind() == Struct) {
                if (v.IsNil()) {
                    panic("reflect: indirection through nil pointer to embedded struct");
                }
                v = v.Elem();
            }
        }
        v = v.Field(x);
    }    return v;
});

// FieldByName returns the struct field with the given name.
// It returns the zero Value if no field was found.
// It panics if v's Kind is not struct.
public static Value FieldByName(this Value v, @string name) {
    v.mustBe(Struct);
    {
        var (f, ok) = v.typ.FieldByName(name);

        if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new Value();
}

// FieldByNameFunc returns the struct field with a name
// that satisfies the match function.
// It panics if v's Kind is not struct.
// It returns the zero Value if no field was found.
public static Value FieldByNameFunc(this Value v, Func<@string, bool> match) {
    {
        var (f, ok) = v.typ.FieldByNameFunc(match);

        if (ok) {
            return v.FieldByIndex(f.Index);
        }
    }
    return new Value();
}

// Float returns v's underlying value, as a float64.
// It panics if v's Kind is not Float32 or Float64
public static double Float(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Float32) 
        return float64(new ptr<ptr<ptr<float>>>(v.ptr));
    else if (k == Float64) 
        return new ptr<ptr<ptr<double>>>(v.ptr);
        panic(addr(new ValueError("reflect.Value.Float",v.kind())));
});

private static ptr<rtype> uint8Type = TypeOf(uint8(0))._<ptr<rtype>>();

// Index returns v's i'th element.
// It panics if v's Kind is not Array, Slice, or String or i is out of range.
public static Value Index(this Value v, nint i) => func((_, panic, _) => {

    if (v.kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(v.typ));
        if (uint(i) >= uint(tt.len)) {
            panic("reflect: array index out of range");
        }
        var typ = tt.elem;
        var offset = uintptr(i) * typ.size; 

        // Either flagIndir is set and v.ptr points at array,
        // or flagIndir is not set and v.ptr is the actual array data.
        // In the former case, we want v.ptr + offset.
        // In the latter case, we must be doing Index(0), so offset = 0,
        // so v.ptr + offset is still the correct address.
        var val = add(v.ptr, offset, "same as &v[i], i < tt.len");
        var fl = v.flag & (flagIndir | flagAddr) | v.flag.ro() | flag(typ.Kind()); // bits same as overall array
        return new Value(typ,val,fl);
    else if (v.kind() == Slice) 
        // Element flag same as Elem of Ptr.
        // Addressable, indirect, possibly read-only.
        var s = (unsafeheader.Slice.val)(v.ptr);
        if (uint(i) >= uint(s.Len)) {
            panic("reflect: slice index out of range");
        }
        tt = (sliceType.val)(@unsafe.Pointer(v.typ));
        typ = tt.elem;
        val = arrayAt(s.Data, i, typ.size, "i < s.Len");
        fl = flagAddr | flagIndir | v.flag.ro() | flag(typ.Kind());
        return new Value(typ,val,fl);
    else if (v.kind() == String) 
        s = (unsafeheader.String.val)(v.ptr);
        if (uint(i) >= uint(s.Len)) {
            panic("reflect: string index out of range");
        }
        var p = arrayAt(s.Data, i, 1, "i < s.Len");
        fl = v.flag.ro() | flag(Uint8) | flagIndir;
        return new Value(uint8Type,p,fl);
        panic(addr(new ValueError("reflect.Value.Index",v.kind())));
});

// Int returns v's underlying value, as an int64.
// It panics if v's Kind is not Int, Int8, Int16, Int32, or Int64.
public static long Int(this Value v) => func((_, panic, _) => {
    var k = v.kind();
    var p = v.ptr;

    if (k == Int) 
        return int64(new ptr<ptr<ptr<nint>>>(p));
    else if (k == Int8) 
        return int64(new ptr<ptr<ptr<sbyte>>>(p));
    else if (k == Int16) 
        return int64(new ptr<ptr<ptr<short>>>(p));
    else if (k == Int32) 
        return int64(new ptr<ptr<ptr<int>>>(p));
    else if (k == Int64) 
        return new ptr<ptr<ptr<long>>>(p);
        panic(addr(new ValueError("reflect.Value.Int",v.kind())));
});

// CanInterface reports whether Interface can be used without panicking.
public static bool CanInterface(this Value v) => func((_, panic, _) => {
    if (v.flag == 0) {
        panic(addr(new ValueError("reflect.Value.CanInterface",Invalid)));
    }
    return v.flag & flagRO == 0;
});

// Interface returns v's current value as an interface{}.
// It is equivalent to:
//    var i interface{} = (v's underlying value)
// It panics if the Value was obtained by accessing
// unexported struct fields.
public static object Interface(this Value v) {
    object i = default;

    return valueInterface(v, true);
}

private static void valueInterface(Value v, bool safe) => func((_, panic, _) => {
    if (v.flag == 0) {
        panic(addr(new ValueError("reflect.Value.Interface",Invalid)));
    }
    if (safe && v.flag & flagRO != 0) { 
        // Do not allow access to unexported values via Interface,
        // because they might be pointers that should not be
        // writable or methods or function that should not be callable.
        panic("reflect.Value.Interface: cannot return value obtained from unexported field or method");
    }
    if (v.flag & flagMethod != 0) {
        v = makeMethodValue("Interface", v);
    }
    if (v.kind() == Interface) { 
        // Special case: return the element inside the interface.
        // Empty interface has one layout, all interfaces with
        // methods have a second layout.
        if (v.NumMethod() == 0) {
            return ;
        }
        return ;
    }
    return packEface(v);
});

// InterfaceData returns a pair of unspecified uintptr values.
// It panics if v's Kind is not Interface.
//
// In earlier versions of Go, this function returned the interface's
// value as a uintptr pair. As of Go 1.4, the implementation of
// interface values precludes any defined use of InterfaceData.
//
// Deprecated: The memory representation of interface values is not
// compatible with InterfaceData.
public static array<System.UIntPtr> InterfaceData(this Value v) {
    v.mustBe(Interface); 
    // We treat this as a read operation, so we allow
    // it even for unexported data, because the caller
    // has to import "unsafe" to turn it into something
    // that can be abused.
    // Interface value is always bigger than a word; assume flagIndir.
    return new ptr<ptr<ptr<array<System.UIntPtr>>>>(v.ptr);
}

// IsNil reports whether its argument v is nil. The argument must be
// a chan, func, interface, map, pointer, or slice value; if it is
// not, IsNil panics. Note that IsNil is not always equivalent to a
// regular comparison with nil in Go. For example, if v was created
// by calling ValueOf with an uninitialized interface variable i,
// i==nil will be true but v.IsNil will panic as v will be the zero
// Value.
public static bool IsNil(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Chan || k == Func || k == Map || k == Ptr || k == UnsafePointer) 
        if (v.flag & flagMethod != 0) {
            return false;
        }
        var ptr = v.ptr;
        if (v.flag & flagIndir != 0) {
            ptr = new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
        }
        return ptr == null;
    else if (k == Interface || k == Slice) 
        // Both interface and slice are nil if first word is 0.
        // Both are always bigger than a word; assume flagIndir.
        return new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr) == null;
        panic(addr(new ValueError("reflect.Value.IsNil",v.kind())));
});

// IsValid reports whether v represents a value.
// It returns false if v is the zero Value.
// If IsValid returns false, all other methods except String panic.
// Most functions and methods never return an invalid Value.
// If one does, its documentation states the conditions explicitly.
public static bool IsValid(this Value v) {
    return v.flag != 0;
}

// IsZero reports whether v is the zero value for its type.
// It panics if the argument is invalid.
public static bool IsZero(this Value v) => func((_, panic, _) => {

    if (v.kind() == Bool) 
        return !v.Bool();
    else if (v.kind() == Int || v.kind() == Int8 || v.kind() == Int16 || v.kind() == Int32 || v.kind() == Int64) 
        return v.Int() == 0;
    else if (v.kind() == Uint || v.kind() == Uint8 || v.kind() == Uint16 || v.kind() == Uint32 || v.kind() == Uint64 || v.kind() == Uintptr) 
        return v.Uint() == 0;
    else if (v.kind() == Float32 || v.kind() == Float64) 
        return math.Float64bits(v.Float()) == 0;
    else if (v.kind() == Complex64 || v.kind() == Complex128) 
        var c = v.Complex();
        return math.Float64bits(real(c)) == 0 && math.Float64bits(imag(c)) == 0;
    else if (v.kind() == Array) 
        {
            nint i__prev1 = i;

            for (nint i = 0; i < v.Len(); i++) {
                if (!v.Index(i).IsZero()) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (v.kind() == Chan || v.kind() == Func || v.kind() == Interface || v.kind() == Map || v.kind() == Ptr || v.kind() == Slice || v.kind() == UnsafePointer) 
        return v.IsNil();
    else if (v.kind() == String) 
        return v.Len() == 0;
    else if (v.kind() == Struct) 
        {
            nint i__prev1 = i;

            for (i = 0; i < v.NumField(); i++) {
                if (!v.Field(i).IsZero()) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else 
        // This should never happens, but will act as a safeguard for
        // later, as a default value doesn't makes sense here.
        panic(addr(new ValueError("reflect.Value.IsZero",v.Kind())));
    });

// Kind returns v's Kind.
// If v is the zero Value (IsValid returns false), Kind returns Invalid.
public static Kind Kind(this Value v) {
    return v.kind();
}

// Len returns v's length.
// It panics if v's Kind is not Array, Chan, Map, Slice, or String.
public static nint Len(this Value v) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(v.typ));
        return int(tt.len);
    else if (k == Chan) 
        return chanlen(v.pointer());
    else if (k == Map) 
        return maplen(v.pointer());
    else if (k == Slice) 
        // Slice is bigger than a word; assume flagIndir.
        return (unsafeheader.Slice.val)(v.ptr).Len;
    else if (k == String) 
        // String is bigger than a word; assume flagIndir.
        return (unsafeheader.String.val)(v.ptr).Len;
        panic(addr(new ValueError("reflect.Value.Len",v.kind())));
});

// MapIndex returns the value associated with key in the map v.
// It panics if v's Kind is not Map.
// It returns the zero Value if key is not found in the map or if v represents a nil map.
// As in Go, the key's value must be assignable to the map's key type.
public static Value MapIndex(this Value v, Value key) {
    v.mustBe(Map);
    var tt = (mapType.val)(@unsafe.Pointer(v.typ)); 

    // Do not require key to be exported, so that DeepEqual
    // and other programs can use all the keys returned by
    // MapKeys as arguments to MapIndex. If either the map
    // or the key is unexported, though, the result will be
    // considered unexported. This is consistent with the
    // behavior for structs, which allow read but not write
    // of unexported fields.
    key = key.assignTo("reflect.Value.MapIndex", tt.key, null);

    unsafe.Pointer k = default;
    if (key.flag & flagIndir != 0) {
        k = key.ptr;
    }
    else
 {
        k = @unsafe.Pointer(_addr_key.ptr);
    }
    var e = mapaccess(_addr_v.typ, v.pointer(), k);
    if (e == null) {
        return new Value();
    }
    var typ = tt.elem;
    var fl = (v.flag | key.flag).ro();
    fl |= flag(typ.Kind());
    return copyVal(_addr_typ, fl, e);
}

// MapKeys returns a slice containing all the keys present in the map,
// in unspecified order.
// It panics if v's Kind is not Map.
// It returns an empty slice if v represents a nil map.
public static slice<Value> MapKeys(this Value v) {
    v.mustBe(Map);
    var tt = (mapType.val)(@unsafe.Pointer(v.typ));
    var keyType = tt.key;

    var fl = v.flag.ro() | flag(keyType.Kind());

    var m = v.pointer();
    var mlen = int(0);
    if (m != null) {
        mlen = maplen(m);
    }
    var it = mapiterinit(_addr_v.typ, m);
    var a = make_slice<Value>(mlen);
    nint i = default;
    for (i = 0; i < len(a); i++) {
        var key = mapiterkey(it);
        if (key == null) { 
            // Someone deleted an entry from the map since we
            // called maplen above. It's a data race, but nothing
            // we can do about it.
            break;
        }
        a[i] = copyVal(_addr_keyType, fl, key);
        mapiternext(it);
    }
    return a[..(int)i];
}

// A MapIter is an iterator for ranging over a map.
// See Value.MapRange.
public partial struct MapIter {
    public Value m;
    public unsafe.Pointer it;
}

// Key returns the key of the iterator's current map entry.
private static Value Key(this ptr<MapIter> _addr_it) => func((_, panic, _) => {
    ref MapIter it = ref _addr_it.val;

    if (it.it == null) {
        panic("MapIter.Key called before Next");
    }
    if (mapiterkey(it.it) == null) {
        panic("MapIter.Key called on exhausted iterator");
    }
    var t = (mapType.val)(@unsafe.Pointer(it.m.typ));
    var ktype = t.key;
    return copyVal(_addr_ktype, it.m.flag.ro() | flag(ktype.Kind()), mapiterkey(it.it));
});

// Value returns the value of the iterator's current map entry.
private static Value Value(this ptr<MapIter> _addr_it) => func((_, panic, _) => {
    ref MapIter it = ref _addr_it.val;

    if (it.it == null) {
        panic("MapIter.Value called before Next");
    }
    if (mapiterkey(it.it) == null) {
        panic("MapIter.Value called on exhausted iterator");
    }
    var t = (mapType.val)(@unsafe.Pointer(it.m.typ));
    var vtype = t.elem;
    return copyVal(_addr_vtype, it.m.flag.ro() | flag(vtype.Kind()), mapiterelem(it.it));
});

// Next advances the map iterator and reports whether there is another
// entry. It returns false when the iterator is exhausted; subsequent
// calls to Key, Value, or Next will panic.
private static bool Next(this ptr<MapIter> _addr_it) => func((_, panic, _) => {
    ref MapIter it = ref _addr_it.val;

    if (it.it == null) {
        it.it = mapiterinit(_addr_it.m.typ, it.m.pointer());
    }
    else
 {
        if (mapiterkey(it.it) == null) {
            panic("MapIter.Next called on exhausted iterator");
        }
        mapiternext(it.it);
    }
    return mapiterkey(it.it) != null;
});

// MapRange returns a range iterator for a map.
// It panics if v's Kind is not Map.
//
// Call Next to advance the iterator, and Key/Value to access each entry.
// Next returns false when the iterator is exhausted.
// MapRange follows the same iteration semantics as a range statement.
//
// Example:
//
//    iter := reflect.ValueOf(m).MapRange()
//     for iter.Next() {
//        k := iter.Key()
//        v := iter.Value()
//        ...
//    }
//
public static ptr<MapIter> MapRange(this Value v) {
    v.mustBe(Map);
    return addr(new MapIter(m:v));
}

// copyVal returns a Value containing the map key or value at ptr,
// allocating a new variable as needed.
private static Value copyVal(ptr<rtype> _addr_typ, flag fl, unsafe.Pointer ptr) {
    ref rtype typ = ref _addr_typ.val;

    if (ifaceIndir(typ)) { 
        // Copy result so future changes to the map
        // won't change the underlying value.
        var c = unsafe_New(_addr_typ);
        typedmemmove(_addr_typ, c, ptr);
        return new Value(typ,c,fl|flagIndir);
    }
    return new Value(typ,*(*unsafe.Pointer)(ptr),fl);
}

// Method returns a function value corresponding to v's i'th method.
// The arguments to a Call on the returned function should not include
// a receiver; the returned function will always use v as the receiver.
// Method panics if i is out of range or if v is a nil interface value.
public static Value Method(this Value v, nint i) => func((_, panic, _) => {
    if (v.typ == null) {
        panic(addr(new ValueError("reflect.Value.Method",Invalid)));
    }
    if (v.flag & flagMethod != 0 || uint(i) >= uint(v.typ.NumMethod())) {
        panic("reflect: Method index out of range");
    }
    if (v.typ.Kind() == Interface && v.IsNil()) {
        panic("reflect: Method on nil interface value");
    }
    var fl = v.flag.ro() | (v.flag & flagIndir);
    fl |= flag(Func);
    fl |= flag(i) << (int)(flagMethodShift) | flagMethod;
    return new Value(v.typ,v.ptr,fl);
});

// NumMethod returns the number of exported methods in the value's method set.
public static nint NumMethod(this Value v) => func((_, panic, _) => {
    if (v.typ == null) {
        panic(addr(new ValueError("reflect.Value.NumMethod",Invalid)));
    }
    if (v.flag & flagMethod != 0) {
        return 0;
    }
    return v.typ.NumMethod();
});

// MethodByName returns a function value corresponding to the method
// of v with the given name.
// The arguments to a Call on the returned function should not include
// a receiver; the returned function will always use v as the receiver.
// It returns the zero Value if no method was found.
public static Value MethodByName(this Value v, @string name) => func((_, panic, _) => {
    if (v.typ == null) {
        panic(addr(new ValueError("reflect.Value.MethodByName",Invalid)));
    }
    if (v.flag & flagMethod != 0) {
        return new Value();
    }
    var (m, ok) = v.typ.MethodByName(name);
    if (!ok) {
        return new Value();
    }
    return v.Method(m.Index);
});

// NumField returns the number of fields in the struct v.
// It panics if v's Kind is not Struct.
public static nint NumField(this Value v) {
    v.mustBe(Struct);
    var tt = (structType.val)(@unsafe.Pointer(v.typ));
    return len(tt.fields);
}

// OverflowComplex reports whether the complex128 x cannot be represented by v's type.
// It panics if v's Kind is not Complex64 or Complex128.
public static bool OverflowComplex(this Value v, System.Numerics.Complex128 x) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Complex64) 
        return overflowFloat32(real(x)) || overflowFloat32(imag(x));
    else if (k == Complex128) 
        return false;
        panic(addr(new ValueError("reflect.Value.OverflowComplex",v.kind())));
});

// OverflowFloat reports whether the float64 x cannot be represented by v's type.
// It panics if v's Kind is not Float32 or Float64.
public static bool OverflowFloat(this Value v, double x) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Float32) 
        return overflowFloat32(x);
    else if (k == Float64) 
        return false;
        panic(addr(new ValueError("reflect.Value.OverflowFloat",v.kind())));
});

private static bool overflowFloat32(double x) {
    if (x < 0) {
        x = -x;
    }
    return math.MaxFloat32 < x && x <= math.MaxFloat64;
}

// OverflowInt reports whether the int64 x cannot be represented by v's type.
// It panics if v's Kind is not Int, Int8, Int16, Int32, or Int64.
public static bool OverflowInt(this Value v, long x) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Int || k == Int8 || k == Int16 || k == Int32 || k == Int64) 
        var bitSize = v.typ.size * 8;
        var trunc = (x << (int)((64 - bitSize))) >> (int)((64 - bitSize));
        return x != trunc;
        panic(addr(new ValueError("reflect.Value.OverflowInt",v.kind())));
});

// OverflowUint reports whether the uint64 x cannot be represented by v's type.
// It panics if v's Kind is not Uint, Uintptr, Uint8, Uint16, Uint32, or Uint64.
public static bool OverflowUint(this Value v, ulong x) => func((_, panic, _) => {
    var k = v.kind();

    if (k == Uint || k == Uintptr || k == Uint8 || k == Uint16 || k == Uint32 || k == Uint64) 
        var bitSize = v.typ.size * 8;
        var trunc = (x << (int)((64 - bitSize))) >> (int)((64 - bitSize));
        return x != trunc;
        panic(addr(new ValueError("reflect.Value.OverflowUint",v.kind())));
});

//go:nocheckptr
// This prevents inlining Value.Pointer when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.Pointer())
// and make an exception.

// Pointer returns v's value as a uintptr.
// It returns uintptr instead of unsafe.Pointer so that
// code using reflect cannot obtain unsafe.Pointers
// without importing the unsafe package explicitly.
// It panics if v's Kind is not Chan, Func, Map, Ptr, Slice, or UnsafePointer.
//
// If v's Kind is Func, the returned pointer is an underlying
// code pointer, but not necessarily enough to identify a
// single function uniquely. The only guarantee is that the
// result is zero if and only if v is a nil func Value.
//
// If v's Kind is Slice, the returned pointer is to the first
// element of the slice. If the slice is nil the returned value
// is 0.  If the slice is empty but non-nil the return value is non-zero.
public static System.UIntPtr Pointer(this Value v) => func((_, panic, _) => { 
    // TODO: deprecate
    var k = v.kind();

    if (k == Ptr)
    {
        if (v.typ.ptrdata == 0) { 
            // Handle pointers to go:notinheap types directly,
            // so we never materialize such pointers as an
            // unsafe.Pointer. (Such pointers are always indirect.)
            // See issue 42076.
            return new ptr<ptr<ptr<System.UIntPtr>>>(v.ptr);
        }
        fallthrough = true;
    }
    if (fallthrough || k == Chan || k == Map || k == UnsafePointer)
    {
        return uintptr(v.pointer());
        goto __switch_break2;
    }
    if (k == Func)
    {
        if (v.flag & flagMethod != 0) { 
            // As the doc comment says, the returned pointer is an
            // underlying code pointer but not necessarily enough to
            // identify a single function uniquely. All method expressions
            // created via reflect have the same underlying code pointer,
            // so their Pointers are equal. The function used here must
            // match the one used in makeMethodValue.
            ref var f = ref heap(methodValueCall, out ptr<var> _addr_f);
            return new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_f));
        }
        var p = v.pointer(); 
        // Non-nil func value points at data block.
        // First word of data block is actual code.
        if (p != null) {
            p = new ptr<ptr<ptr<unsafe.Pointer>>>(p);
        }
        return uintptr(p);
        goto __switch_break2;
    }
    if (k == Slice)
    {
        return (SliceHeader.val)(v.ptr).Data;
        goto __switch_break2;
    }

    __switch_break2:;
    panic(addr(new ValueError("reflect.Value.Pointer",v.kind())));
});

// Recv receives and returns a value from the channel v.
// It panics if v's Kind is not Chan.
// The receive blocks until a value is ready.
// The boolean value ok is true if the value x corresponds to a send
// on the channel, false if it is a zero value received because the channel is closed.
public static (Value, bool) Recv(this Value v) {
    Value x = default;
    bool ok = default;

    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(false);
}

// internal recv, possibly non-blocking (nb).
// v is known to be a channel.
public static (Value, bool) recv(this Value v, bool nb) => func((_, panic, _) => {
    Value val = default;
    bool ok = default;

    var tt = (chanType.val)(@unsafe.Pointer(v.typ));
    if (ChanDir(tt.dir) & RecvDir == 0) {
        panic("reflect: recv on send-only channel");
    }
    var t = tt.elem;
    val = new Value(t,nil,flag(t.Kind()));
    unsafe.Pointer p = default;
    if (ifaceIndir(t)) {
        p = unsafe_New(_addr_t);
        val.ptr = p;
        val.flag |= flagIndir;
    }
    else
 {
        p = @unsafe.Pointer(_addr_val.ptr);
    }
    var (selected, ok) = chanrecv(v.pointer(), nb, p);
    if (!selected) {
        val = new Value();
    }
    return ;
});

// Send sends x on the channel v.
// It panics if v's kind is not Chan or if x's type is not the same type as v's element type.
// As in Go, x's value must be assignable to the channel's element type.
public static void Send(this Value v, Value x) {
    v.mustBe(Chan);
    v.mustBeExported();
    v.send(x, false);
}

// internal send, possibly non-blocking.
// v is known to be a channel.
public static bool send(this Value v, Value x, bool nb) => func((_, panic, _) => {
    bool selected = default;

    var tt = (chanType.val)(@unsafe.Pointer(v.typ));
    if (ChanDir(tt.dir) & SendDir == 0) {
        panic("reflect: send on recv-only channel");
    }
    x.mustBeExported();
    x = x.assignTo("reflect.Value.Send", tt.elem, null);
    unsafe.Pointer p = default;
    if (x.flag & flagIndir != 0) {
        p = x.ptr;
    }
    else
 {
        p = @unsafe.Pointer(_addr_x.ptr);
    }
    return chansend(v.pointer(), p, nb);
});

// Set assigns x to the value v.
// It panics if CanSet returns false.
// As in Go, x's value must be assignable to v's type.
public static void Set(this Value v, Value x) {
    v.mustBeAssignable();
    x.mustBeExported(); // do not let unexported x leak
    unsafe.Pointer target = default;
    if (v.kind() == Interface) {
        target = v.ptr;
    }
    x = x.assignTo("reflect.Set", v.typ, target);
    if (x.flag & flagIndir != 0) {
        if (x.ptr == @unsafe.Pointer(_addr_zeroVal[0])) {
            typedmemclr(_addr_v.typ, v.ptr);
        }
        else
 {
            typedmemmove(_addr_v.typ, v.ptr, x.ptr);
        }
    }
    else
 {
        (@unsafe.Pointer.val).val;

        (v.ptr) = x.ptr;
    }
}

// SetBool sets v's underlying value.
// It panics if v's Kind is not Bool or if CanSet() is false.
public static void SetBool(this Value v, bool x) {
    v.mustBeAssignable();
    v.mustBe(Bool) * (bool.val);

    (v.ptr) = x;
}

// SetBytes sets v's underlying value.
// It panics if v's underlying value is not a slice of bytes.
public static void SetBytes(this Value v, slice<byte> x) => func((_, panic, _) => {
    v.mustBeAssignable();
    v.mustBe(Slice);
    if (v.typ.Elem().Kind() != Uint8) {
        panic("reflect.Value.SetBytes of non-byte slice");
    }
    new ptr<ptr<ptr<slice<byte>>>>(v.ptr) = x;
});

// setRunes sets v's underlying value.
// It panics if v's underlying value is not a slice of runes (int32s).
public static void setRunes(this Value v, slice<int> x) => func((_, panic, _) => {
    v.mustBeAssignable();
    v.mustBe(Slice);
    if (v.typ.Elem().Kind() != Int32) {
        panic("reflect.Value.setRunes of non-rune slice");
    }
    new ptr<ptr<ptr<slice<int>>>>(v.ptr) = x;
});

// SetComplex sets v's underlying value to x.
// It panics if v's Kind is not Complex64 or Complex128, or if CanSet() is false.
public static void SetComplex(this Value v, System.Numerics.Complex128 x) => func((_, panic, _) => {
    v.mustBeAssignable();
    {
        var k = v.kind();


        if (k == Complex64) 
            (complex64.val).val;

            (v.ptr) = complex64(x);
        else if (k == Complex128) 
            (complex128.val).val;

            (v.ptr) = x;
        else 
            panic(addr(new ValueError("reflect.Value.SetComplex",v.kind())));

    }
});

// SetFloat sets v's underlying value to x.
// It panics if v's Kind is not Float32 or Float64, or if CanSet() is false.
public static void SetFloat(this Value v, double x) => func((_, panic, _) => {
    v.mustBeAssignable();
    {
        var k = v.kind();


        if (k == Float32) 
            (float32.val).val;

            (v.ptr) = float32(x);
        else if (k == Float64) 
            (float64.val).val;

            (v.ptr) = x;
        else 
            panic(addr(new ValueError("reflect.Value.SetFloat",v.kind())));

    }
});

// SetInt sets v's underlying value to x.
// It panics if v's Kind is not Int, Int8, Int16, Int32, or Int64, or if CanSet() is false.
public static void SetInt(this Value v, long x) => func((_, panic, _) => {
    v.mustBeAssignable();
    {
        var k = v.kind();


        if (k == Int) 
            (int.val).val;

            (v.ptr) = int(x);
        else if (k == Int8) 
            (int8.val).val;

            (v.ptr) = int8(x);
        else if (k == Int16) 
            (int16.val).val;

            (v.ptr) = int16(x);
        else if (k == Int32) 
            (int32.val).val;

            (v.ptr) = int32(x);
        else if (k == Int64) 
            (int64.val).val;

            (v.ptr) = x;
        else 
            panic(addr(new ValueError("reflect.Value.SetInt",v.kind())));

    }
});

// SetLen sets v's length to n.
// It panics if v's Kind is not Slice or if n is negative or
// greater than the capacity of the slice.
public static void SetLen(this Value v, nint n) => func((_, panic, _) => {
    v.mustBeAssignable();
    v.mustBe(Slice);
    var s = (unsafeheader.Slice.val)(v.ptr);
    if (uint(n) > uint(s.Cap)) {
        panic("reflect: slice length out of range in SetLen");
    }
    s.Len = n;
});

// SetCap sets v's capacity to n.
// It panics if v's Kind is not Slice or if n is smaller than the length or
// greater than the capacity of the slice.
public static void SetCap(this Value v, nint n) => func((_, panic, _) => {
    v.mustBeAssignable();
    v.mustBe(Slice);
    var s = (unsafeheader.Slice.val)(v.ptr);
    if (n < s.Len || n > s.Cap) {
        panic("reflect: slice capacity out of range in SetCap");
    }
    s.Cap = n;
});

// SetMapIndex sets the element associated with key in the map v to elem.
// It panics if v's Kind is not Map.
// If elem is the zero Value, SetMapIndex deletes the key from the map.
// Otherwise if v holds a nil map, SetMapIndex will panic.
// As in Go, key's elem must be assignable to the map's key type,
// and elem's value must be assignable to the map's elem type.
public static void SetMapIndex(this Value v, Value key, Value elem) {
    v.mustBe(Map);
    v.mustBeExported();
    key.mustBeExported();
    var tt = (mapType.val)(@unsafe.Pointer(v.typ));
    key = key.assignTo("reflect.Value.SetMapIndex", tt.key, null);
    unsafe.Pointer k = default;
    if (key.flag & flagIndir != 0) {
        k = key.ptr;
    }
    else
 {
        k = @unsafe.Pointer(_addr_key.ptr);
    }
    if (elem.typ == null) {
        mapdelete(_addr_v.typ, v.pointer(), k);
        return ;
    }
    elem.mustBeExported();
    elem = elem.assignTo("reflect.Value.SetMapIndex", tt.elem, null);
    unsafe.Pointer e = default;
    if (elem.flag & flagIndir != 0) {
        e = elem.ptr;
    }
    else
 {
        e = @unsafe.Pointer(_addr_elem.ptr);
    }
    mapassign(_addr_v.typ, v.pointer(), k, e);
}

// SetUint sets v's underlying value to x.
// It panics if v's Kind is not Uint, Uintptr, Uint8, Uint16, Uint32, or Uint64, or if CanSet() is false.
public static void SetUint(this Value v, ulong x) => func((_, panic, _) => {
    v.mustBeAssignable();
    {
        var k = v.kind();


        if (k == Uint) 
            (uint.val).val;

            (v.ptr) = uint(x);
        else if (k == Uint8) 
            (uint8.val).val;

            (v.ptr) = uint8(x);
        else if (k == Uint16) 
            (uint16.val).val;

            (v.ptr) = uint16(x);
        else if (k == Uint32) 
            (uint32.val).val;

            (v.ptr) = uint32(x);
        else if (k == Uint64) 
            (uint64.val).val;

            (v.ptr) = x;
        else if (k == Uintptr) 
            (uintptr.val).val;

            (v.ptr) = uintptr(x);
        else 
            panic(addr(new ValueError("reflect.Value.SetUint",v.kind())));

    }
});

// SetPointer sets the unsafe.Pointer value v to x.
// It panics if v's Kind is not UnsafePointer.
public static void SetPointer(this Value v, unsafe.Pointer x) {
    v.mustBeAssignable();
    v.mustBe(UnsafePointer) * (@unsafe.Pointer.val);

    (v.ptr) = x;
}

// SetString sets v's underlying value to x.
// It panics if v's Kind is not String or if CanSet() is false.
public static void SetString(this Value v, @string x) {
    v.mustBeAssignable();
    v.mustBe(String) * (string.val);

    (v.ptr) = x;
}

// Slice returns v[i:j].
// It panics if v's Kind is not Array, Slice or String, or if v is an unaddressable array,
// or if the indexes are out of bounds.
public static Value Slice(this Value v, nint i, nint j) => func((_, panic, _) => {
    nint cap = default;    ptr<sliceType> typ;    unsafe.Pointer @base = default;
    {
        var kind = v.kind();


        if (kind == Array) 
            if (v.flag & flagAddr == 0) {
                panic("reflect.Value.Slice: slice of unaddressable array");
            }
            var tt = (arrayType.val)(@unsafe.Pointer(v.typ));
            cap = int(tt.len);
            typ = (sliceType.val)(@unsafe.Pointer(tt.slice));
            base = v.ptr;
        else if (kind == Slice) 
            typ = (sliceType.val)(@unsafe.Pointer(v.typ));
            var s = (unsafeheader.Slice.val)(v.ptr);
            base = s.Data;
            cap = s.Cap;
        else if (kind == String) 
            s = (unsafeheader.String.val)(v.ptr);
            if (i < 0 || j < i || j > s.Len) {
                panic("reflect.Value.Slice: string slice index out of bounds");
            }
            ref unsafeheader.String t = ref heap(out ptr<unsafeheader.String> _addr_t);
            if (i < s.Len) {
                t = new unsafeheader.String(Data:arrayAt(s.Data,i,1,"i < s.Len"),Len:j-i);
            }
            return new Value(v.typ,unsafe.Pointer(&t),v.flag);
        else 
            panic(addr(new ValueError("reflect.Value.Slice",v.kind())));

    }

    if (i < 0 || j < i || j > cap) {
        panic("reflect.Value.Slice: slice index out of bounds");
    }
    ref slice<unsafe.Pointer> x = ref heap(out ptr<slice<unsafe.Pointer>> _addr_x); 

    // Reinterpret as *unsafeheader.Slice to edit.
    s = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_x));
    s.Len = j - i;
    s.Cap = cap - i;
    if (cap - i > 0) {
        s.Data = arrayAt(base, i, typ.elem.Size(), "i < cap");
    }
    else
 { 
        // do not advance pointer, to avoid pointing beyond end of slice
        s.Data = base;
    }
    var fl = v.flag.ro() | flagIndir | flag(Slice);
    return new Value(typ.common(),unsafe.Pointer(&x),fl);
});

// Slice3 is the 3-index form of the slice operation: it returns v[i:j:k].
// It panics if v's Kind is not Array or Slice, or if v is an unaddressable array,
// or if the indexes are out of bounds.
public static Value Slice3(this Value v, nint i, nint j, nint k) => func((_, panic, _) => {
    nint cap = default;    ptr<sliceType> typ;    unsafe.Pointer @base = default;
    {
        var kind = v.kind();


        if (kind == Array) 
            if (v.flag & flagAddr == 0) {
                panic("reflect.Value.Slice3: slice of unaddressable array");
            }
            var tt = (arrayType.val)(@unsafe.Pointer(v.typ));
            cap = int(tt.len);
            typ = (sliceType.val)(@unsafe.Pointer(tt.slice));
            base = v.ptr;
        else if (kind == Slice) 
            typ = (sliceType.val)(@unsafe.Pointer(v.typ));
            var s = (unsafeheader.Slice.val)(v.ptr);
            base = s.Data;
            cap = s.Cap;
        else 
            panic(addr(new ValueError("reflect.Value.Slice3",v.kind())));

    }

    if (i < 0 || j < i || k < j || k > cap) {
        panic("reflect.Value.Slice3: slice index out of bounds");
    }
    ref slice<unsafe.Pointer> x = ref heap(out ptr<slice<unsafe.Pointer>> _addr_x); 

    // Reinterpret as *unsafeheader.Slice to edit.
    s = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_x));
    s.Len = j - i;
    s.Cap = k - i;
    if (k - i > 0) {
        s.Data = arrayAt(base, i, typ.elem.Size(), "i < k <= cap");
    }
    else
 { 
        // do not advance pointer, to avoid pointing beyond end of slice
        s.Data = base;
    }
    var fl = v.flag.ro() | flagIndir | flag(Slice);
    return new Value(typ.common(),unsafe.Pointer(&x),fl);
});

// String returns the string v's underlying value, as a string.
// String is a special case because of Go's String method convention.
// Unlike the other getters, it does not panic if v's Kind is not String.
// Instead, it returns a string of the form "<T value>" where T is v's type.
// The fmt package treats Values specially. It does not call their String
// method implicitly but instead prints the concrete values they hold.
public static @string String(this Value v) {
    {
        var k = v.kind();


        if (k == Invalid) 
            return "<invalid Value>";
        else if (k == String) 
            return new ptr<ptr<ptr<@string>>>(v.ptr);

    } 
    // If you call String on a reflect.Value of other type, it's better to
    // print something than to panic. Useful in debugging.
    return "<" + v.Type().String() + " Value>";
}

// TryRecv attempts to receive a value from the channel v but will not block.
// It panics if v's Kind is not Chan.
// If the receive delivers a value, x is the transferred value and ok is true.
// If the receive cannot finish without blocking, x is the zero Value and ok is false.
// If the channel is closed, x is the zero value for the channel's element type and ok is false.
public static (Value, bool) TryRecv(this Value v) {
    Value x = default;
    bool ok = default;

    v.mustBe(Chan);
    v.mustBeExported();
    return v.recv(true);
}

// TrySend attempts to send x on the channel v but will not block.
// It panics if v's Kind is not Chan.
// It reports whether the value was sent.
// As in Go, x's value must be assignable to the channel's element type.
public static bool TrySend(this Value v, Value x) {
    v.mustBe(Chan);
    v.mustBeExported();
    return v.send(x, true);
}

// Type returns v's type.
public static Type Type(this Value v) => func((_, panic, _) => {
    var f = v.flag;
    if (f == 0) {
        panic(addr(new ValueError("reflect.Value.Type",Invalid)));
    }
    if (f & flagMethod == 0) { 
        // Easy case
        return v.typ;
    }
    var i = int(v.flag) >> (int)(flagMethodShift);
    if (v.typ.Kind() == Interface) { 
        // Method on interface.
        var tt = (interfaceType.val)(@unsafe.Pointer(v.typ));
        if (uint(i) >= uint(len(tt.methods))) {
            panic("reflect: internal error: invalid method index");
        }
        var m = _addr_tt.methods[i];
        return v.typ.typeOff(m.typ);
    }
    var ms = v.typ.exportedMethods();
    if (uint(i) >= uint(len(ms))) {
        panic("reflect: internal error: invalid method index");
    }
    m = ms[i];
    return v.typ.typeOff(m.mtyp);
});

// Uint returns v's underlying value, as a uint64.
// It panics if v's Kind is not Uint, Uintptr, Uint8, Uint16, Uint32, or Uint64.
public static ulong Uint(this Value v) => func((_, panic, _) => {
    var k = v.kind();
    var p = v.ptr;

    if (k == Uint) 
        return uint64(new ptr<ptr<ptr<nuint>>>(p));
    else if (k == Uint8) 
        return uint64(new ptr<ptr<ptr<byte>>>(p));
    else if (k == Uint16) 
        return uint64(new ptr<ptr<ptr<ushort>>>(p));
    else if (k == Uint32) 
        return uint64(new ptr<ptr<ptr<uint>>>(p));
    else if (k == Uint64) 
        return new ptr<ptr<ptr<ulong>>>(p);
    else if (k == Uintptr) 
        return uint64(new ptr<ptr<ptr<System.UIntPtr>>>(p));
        panic(addr(new ValueError("reflect.Value.Uint",v.kind())));
});

//go:nocheckptr
// This prevents inlining Value.UnsafeAddr when -d=checkptr is enabled,
// which ensures cmd/compile can recognize unsafe.Pointer(v.UnsafeAddr())
// and make an exception.

// UnsafeAddr returns a pointer to v's data.
// It is for advanced clients that also import the "unsafe" package.
// It panics if v is not addressable.
public static System.UIntPtr UnsafeAddr(this Value v) => func((_, panic, _) => { 
    // TODO: deprecate
    if (v.typ == null) {
        panic(addr(new ValueError("reflect.Value.UnsafeAddr",Invalid)));
    }
    if (v.flag & flagAddr == 0) {
        panic("reflect.Value.UnsafeAddr of unaddressable value");
    }
    return uintptr(v.ptr);
});

// StringHeader is the runtime representation of a string.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
public partial struct StringHeader {
    public System.UIntPtr Data;
    public nint Len;
}

// SliceHeader is the runtime representation of a slice.
// It cannot be used safely or portably and its representation may
// change in a later release.
// Moreover, the Data field is not sufficient to guarantee the data
// it references will not be garbage collected, so programs must keep
// a separate, correctly typed pointer to the underlying data.
public partial struct SliceHeader {
    public System.UIntPtr Data;
    public nint Len;
    public nint Cap;
}

private static void typesMustMatch(@string what, Type t1, Type t2) => func((_, panic, _) => {
    if (t1 != t2) {
        panic(what + ": " + t1.String() + " != " + t2.String());
    }
});

// arrayAt returns the i-th element of p,
// an array whose elements are eltSize bytes wide.
// The array pointed at by p must have at least i+1 elements:
// it is invalid (but impossible to check here) to pass i >= len,
// because then the result will point outside the array.
// whySafe must explain why i < len. (Passing "i < len" is fine;
// the benefit is to surface this assumption at the call site.)
private static unsafe.Pointer arrayAt(unsafe.Pointer p, nint i, System.UIntPtr eltSize, @string whySafe) {
    return add(p, uintptr(i) * eltSize, "i < len");
}

// grow grows the slice s so that it can hold extra more values, allocating
// more capacity if needed. It also returns the old and new slice lengths.
private static (Value, nint, nint) grow(Value s, nint extra) => func((_, panic, _) => {
    Value _p0 = default;
    nint _p0 = default;
    nint _p0 = default;

    var i0 = s.Len();
    var i1 = i0 + extra;
    if (i1 < i0) {
        panic("reflect.Append: slice overflow");
    }
    var m = s.Cap();
    if (i1 <= m) {
        return (s.Slice(0, i1), i0, i1);
    }
    if (m == 0) {
        m = extra;
    }
    else
 {
        while (m < i1) {
            if (i0 < 1024) {
                m += m;
            }
            else
 {
                m += m / 4;
            }
        }
    }
    var t = MakeSlice(s.Type(), i1, m);
    Copy(t, s);
    return (t, i0, i1);
});

// Append appends the values x to a slice s and returns the resulting slice.
// As in Go, each x's value must be assignable to the slice's element type.
public static Value Append(Value s, params Value[] x) {
    x = x.Clone();

    s.mustBe(Slice);
    var (s, i0, i1) = grow(s, len(x));
    {
        var i = i0;
        nint j = 0;

        while (i < i1) {
            s.Index(i).Set(x[j]);
            (i, j) = (i + 1, j + 1);
        }
    }
    return s;
}

// AppendSlice appends a slice t to a slice s and returns the resulting slice.
// The slices s and t must have the same element type.
public static Value AppendSlice(Value s, Value t) {
    s.mustBe(Slice);
    t.mustBe(Slice);
    typesMustMatch("reflect.AppendSlice", s.Type().Elem(), t.Type().Elem());
    var (s, i0, i1) = grow(s, t.Len());
    Copy(s.Slice(i0, i1), t);
    return s;
}

// Copy copies the contents of src into dst until either
// dst has been filled or src has been exhausted.
// It returns the number of elements copied.
// Dst and src each must have kind Slice or Array, and
// dst and src must have the same element type.
//
// As a special case, src can have kind String if the element type of dst is kind Uint8.
public static nint Copy(Value dst, Value src) => func((_, panic, _) => {
    var dk = dst.kind();
    if (dk != Array && dk != Slice) {
        panic(addr(new ValueError("reflect.Copy",dk)));
    }
    if (dk == Array) {
        dst.mustBeAssignable();
    }
    dst.mustBeExported();

    var sk = src.kind();
    bool stringCopy = default;
    if (sk != Array && sk != Slice) {
        stringCopy = sk == String && dst.typ.Elem().Kind() == Uint8;
        if (!stringCopy) {
            panic(addr(new ValueError("reflect.Copy",sk)));
        }
    }
    src.mustBeExported();

    var de = dst.typ.Elem();
    if (!stringCopy) {
        var se = src.typ.Elem();
        typesMustMatch("reflect.Copy", de, se);
    }
    unsafeheader.Slice ds = default;    unsafeheader.Slice ss = default;

    if (dk == Array) {
        ds.Data = dst.ptr;
        ds.Len = dst.Len();
        ds.Cap = ds.Len;
    }
    else
 {
        ds = new ptr<ptr<ptr<unsafeheader.Slice>>>(dst.ptr);
    }
    if (sk == Array) {
        ss.Data = src.ptr;
        ss.Len = src.Len();
        ss.Cap = ss.Len;
    }
    else if (sk == Slice) {
        ss = new ptr<ptr<ptr<unsafeheader.Slice>>>(src.ptr);
    }
    else
 {
        ptr<ptr<unsafeheader.String>> sh = new ptr<ptr<ptr<unsafeheader.String>>>(src.ptr);
        ss.Data = sh.Data;
        ss.Len = sh.Len;
        ss.Cap = sh.Len;
    }
    return typedslicecopy(_addr_de.common(), ds, ss);
});

// A runtimeSelect is a single case passed to rselect.
// This must match ../runtime/select.go:/runtimeSelect
private partial struct runtimeSelect {
    public SelectDir dir; // SelectSend, SelectRecv or SelectDefault
    public ptr<rtype> typ; // channel type
    public unsafe.Pointer ch; // channel
    public unsafe.Pointer val; // ptr to data (SendDir) or ptr to receive buffer (RecvDir)
}

// rselect runs a select. It returns the index of the chosen case.
// If the case was a receive, val is filled in with the received value.
// The conventional OK bool indicates whether the receive corresponds
// to a sent value.
//go:noescape
private static (nint, bool) rselect(slice<runtimeSelect> _p0);

// A SelectDir describes the communication direction of a select case.
public partial struct SelectDir { // : nint
}

// NOTE: These values must match ../runtime/select.go:/selectDir.

private static readonly SelectDir _ = iota;
public static readonly var SelectSend = 0; // case Chan <- Send
public static readonly var SelectRecv = 1; // case <-Chan:
public static readonly var SelectDefault = 2; // default

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
// If Dir is SelectRecv, the case represents a receive operation.
// Normally Chan's underlying value must be a channel and Send must be a zero Value.
// If Chan is a zero Value, then the case is ignored, but Send must still be a zero Value.
// When a receive operation is selected, the received Value is returned by Select.
//
public partial struct SelectCase {
    public SelectDir Dir; // direction of case
    public Value Chan; // channel to use (for send or receive)
    public Value Send; // value to send (for send)
}

// Select executes a select operation described by the list of cases.
// Like the Go select statement, it blocks until at least one of the cases
// can proceed, makes a uniform pseudo-random choice,
// and then executes that case. It returns the index of the chosen case
// and, if that case was a receive operation, the value received and a
// boolean indicating whether the value corresponds to a send on the channel
// (as opposed to a zero value received because the channel is closed).
// Select supports a maximum of 65536 cases.
public static (nint, Value, bool) Select(slice<SelectCase> cases) => func((_, panic, _) => {
    nint chosen = default;
    Value recv = default;
    bool recvOK = default;

    if (len(cases) > 65536) {>>MARKER:FUNCTION_rselect_BLOCK_PREFIX<<
        panic("reflect.Select: too many cases (max 65536)");
    }
    slice<runtimeSelect> runcases = default;
    if (len(cases) > 4) { 
        // Slice is heap allocated due to runtime dependent capacity.
        runcases = make_slice<runtimeSelect>(len(cases));
    }
    else
 { 
        // Slice can be stack allocated due to constant capacity.
        runcases = make_slice<runtimeSelect>(len(cases), 4);
    }
    var haveDefault = false;
    foreach (var (i, c) in cases) {
        var rc = _addr_runcases[i];
        rc.dir = c.Dir;

        if (c.Dir == SelectDefault) // default
            if (haveDefault) {
                panic("reflect.Select: multiple default cases");
            }
            haveDefault = true;
            if (c.Chan.IsValid()) {
                panic("reflect.Select: default case has Chan value");
            }
            if (c.Send.IsValid()) {
                panic("reflect.Select: default case has Send value");
            }
        else if (c.Dir == SelectSend) 
            var ch = c.Chan;
            if (!ch.IsValid()) {
                break;
            }
            ch.mustBe(Chan);
            ch.mustBeExported();
            var tt = (chanType.val)(@unsafe.Pointer(ch.typ));
            if (ChanDir(tt.dir) & SendDir == 0) {
                panic("reflect.Select: SendDir case using recv-only channel");
            }
            rc.ch = ch.pointer();
            rc.typ = _addr_tt.rtype;
            var v = c.Send;
            if (!v.IsValid()) {
                panic("reflect.Select: SendDir case missing Send value");
            }
            v.mustBeExported();
            v = v.assignTo("reflect.Select", tt.elem, null);
            if (v.flag & flagIndir != 0) {
                rc.val = v.ptr;
            }
            else
 {
                rc.val = @unsafe.Pointer(_addr_v.ptr);
            }
        else if (c.Dir == SelectRecv) 
            if (c.Send.IsValid()) {
                panic("reflect.Select: RecvDir case has Send value");
            }
            ch = c.Chan;
            if (!ch.IsValid()) {
                break;
            }
            ch.mustBe(Chan);
            ch.mustBeExported();
            tt = (chanType.val)(@unsafe.Pointer(ch.typ));
            if (ChanDir(tt.dir) & RecvDir == 0) {
                panic("reflect.Select: RecvDir case using send-only channel");
            }
            rc.ch = ch.pointer();
            rc.typ = _addr_tt.rtype;
            rc.val = unsafe_New(_addr_tt.elem);
        else 
            panic("reflect.Select: invalid Dir");
            }    chosen, recvOK = rselect(runcases);
    if (runcases[chosen].dir == SelectRecv) {
        tt = (chanType.val)(@unsafe.Pointer(runcases[chosen].typ));
        var t = tt.elem;
        var p = runcases[chosen].val;
        var fl = flag(t.Kind());
        if (ifaceIndir(t)) {
            recv = new Value(t,p,fl|flagIndir);
        }
        else
 {
            recv = new Value(t,*(*unsafe.Pointer)(p),fl);
        }
    }
    return (chosen, recv, recvOK);
});

/*
 * constructors
 */

// implemented in package runtime
private static unsafe.Pointer unsafe_New(ptr<rtype> _p0);
private static unsafe.Pointer unsafe_NewArray(ptr<rtype> _p0, nint _p0);

// MakeSlice creates a new zero-initialized slice value
// for the specified slice type, length, and capacity.
public static Value MakeSlice(Type typ, nint len, nint cap) => func((_, panic, _) => {
    if (typ.Kind() != Slice) {>>MARKER:FUNCTION_unsafe_NewArray_BLOCK_PREFIX<<
        panic("reflect.MakeSlice of non-slice type");
    }
    if (len < 0) {>>MARKER:FUNCTION_unsafe_New_BLOCK_PREFIX<<
        panic("reflect.MakeSlice: negative len");
    }
    if (cap < 0) {
        panic("reflect.MakeSlice: negative cap");
    }
    if (len > cap) {
        panic("reflect.MakeSlice: len > cap");
    }
    ref unsafeheader.Slice s = ref heap(new unsafeheader.Slice(Data:unsafe_NewArray(typ.Elem().(*rtype),cap),Len:len,Cap:cap), out ptr<unsafeheader.Slice> _addr_s);
    return new Value(typ.(*rtype),unsafe.Pointer(&s),flagIndir|flag(Slice));
});

// MakeChan creates a new channel with the specified type and buffer size.
public static Value MakeChan(Type typ, nint buffer) => func((_, panic, _) => {
    if (typ.Kind() != Chan) {
        panic("reflect.MakeChan of non-chan type");
    }
    if (buffer < 0) {
        panic("reflect.MakeChan: negative buffer size");
    }
    if (typ.ChanDir() != BothDir) {
        panic("reflect.MakeChan: unidirectional channel type");
    }
    ptr<rtype> t = typ._<ptr<rtype>>();
    var ch = makechan(t, buffer);
    return new Value(t,ch,flag(Chan));
});

// MakeMap creates a new map with the specified type.
public static Value MakeMap(Type typ) {
    return MakeMapWithSize(typ, 0);
}

// MakeMapWithSize creates a new map with the specified type
// and initial space for approximately n elements.
public static Value MakeMapWithSize(Type typ, nint n) => func((_, panic, _) => {
    if (typ.Kind() != Map) {
        panic("reflect.MakeMapWithSize of non-map type");
    }
    ptr<rtype> t = typ._<ptr<rtype>>();
    var m = makemap(t, n);
    return new Value(t,m,flag(Map));
});

// Indirect returns the value that v points to.
// If v is a nil pointer, Indirect returns a zero Value.
// If v is not a pointer, Indirect returns v.
public static Value Indirect(Value v) {
    if (v.Kind() != Ptr) {
        return v;
    }
    return v.Elem();
}

// ValueOf returns a new Value initialized to the concrete value
// stored in the interface i. ValueOf(nil) returns the zero Value.
public static Value ValueOf(object i) {
    if (i == null) {
        return new Value();
    }
    escapes(i);

    return unpackEface(i);
}

// Zero returns a Value representing the zero value for the specified type.
// The result is different from the zero value of the Value struct,
// which represents no value at all.
// For example, Zero(TypeOf(42)) returns a Value with Kind Int and value 0.
// The returned value is neither addressable nor settable.
public static Value Zero(Type typ) => func((_, panic, _) => {
    if (typ == null) {
        panic("reflect: Zero(nil)");
    }
    ptr<rtype> t = typ._<ptr<rtype>>();
    var fl = flag(t.Kind());
    if (ifaceIndir(t)) {
        unsafe.Pointer p = default;
        if (t.size <= maxZero) {
            p = @unsafe.Pointer(_addr_zeroVal[0]);
        }
        else
 {
            p = unsafe_New(t);
        }
        return new Value(t,p,fl|flagIndir);
    }
    return new Value(t,nil,fl);
});

// must match declarations in runtime/map.go.
private static readonly nint maxZero = 1024;

//go:linkname zeroVal runtime.zeroVal


//go:linkname zeroVal runtime.zeroVal
private static array<byte> zeroVal = new array<byte>(maxZero);

// New returns a Value representing a pointer to a new zero value
// for the specified type. That is, the returned Value's Type is PtrTo(typ).
public static Value New(Type typ) => func((_, panic, _) => {
    if (typ == null) {
        panic("reflect: New(nil)");
    }
    ptr<rtype> t = typ._<ptr<rtype>>();
    var pt = t.ptrTo();
    if (ifaceIndir(pt)) { 
        // This is a pointer to a go:notinheap type.
        panic("reflect: New of type that may not be allocated in heap (possibly undefined cgo C type)");
    }
    var ptr = unsafe_New(t);
    var fl = flag(Ptr);
    return new Value(pt,ptr,fl);
});

// NewAt returns a Value representing a pointer to a value of the
// specified type, using p as that pointer.
public static Value NewAt(Type typ, unsafe.Pointer p) {
    var fl = flag(Ptr);
    ptr<rtype> t = typ._<ptr<rtype>>();
    return new Value(t.ptrTo(),p,fl);
}

// assignTo returns a value v that can be assigned directly to typ.
// It panics if v is not assignable to typ.
// For a conversion to an interface type, target is a suggested scratch space to use.
// target must be initialized memory (or nil).
public static Value assignTo(this Value v, @string context, ptr<rtype> _addr_dst, unsafe.Pointer target) => func((_, panic, _) => {
    ref rtype dst = ref _addr_dst.val;

    if (v.flag & flagMethod != 0) {
        v = makeMethodValue(context, v);
    }

    if (directlyAssignable(dst, v.typ)) 
        // Overwrite type so that they match.
        // Same memory layout, so no harm done.
        var fl = v.flag & (flagAddr | flagIndir) | v.flag.ro();
        fl |= flag(dst.Kind());
        return new Value(dst,v.ptr,fl);
    else if (implements(dst, v.typ)) 
        if (target == null) {
            target = unsafe_New(_addr_dst);
        }
        if (v.Kind() == Interface && v.IsNil()) { 
            // A nil ReadWriter passed to nil Reader is OK,
            // but using ifaceE2I below will panic.
            // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
            return new Value(dst,nil,flag(Interface));
        }
        var x = valueInterface(v, false);
        if (dst.NumMethod() == 0) {
        }
        else
 {
            ifaceE2I(_addr_dst, x, target);
        }
        return new Value(dst,target,flagIndir|flag(Interface));
    // Failed.
    panic(context + ": value of type " + v.typ.String() + " is not assignable to type " + dst.String());
});

// Convert returns the value v converted to type t.
// If the usual Go conversion rules do not allow conversion
// of the value v to type t, or if converting v to type t panics, Convert panics.
public static Value Convert(this Value v, Type t) => func((_, panic, _) => {
    if (v.flag & flagMethod != 0) {
        v = makeMethodValue("Convert", v);
    }
    var op = convertOp(_addr_t.common(), _addr_v.typ);
    if (op == null) {
        panic("reflect.Value.Convert: value of type " + v.typ.String() + " cannot be converted to type " + t.String());
    }
    return op(v, t);
});

// CanConvert reports whether the value v can be converted to type t.
// If v.CanConvert(t) returns true then v.Convert(t) will not panic.
public static bool CanConvert(this Value v, Type t) {
    var vt = v.Type();
    if (!vt.ConvertibleTo(t)) {
        return false;
    }
    if (vt.Kind() == Slice && t.Kind() == Ptr && t.Elem().Kind() == Array) {
        var n = t.Elem().Len();
        var h = (unsafeheader.Slice.val)(v.ptr);
        if (n > h.Len) {
            return false;
        }
    }
    return true;
}

// convertOp returns the function to convert a value of type src
// to a value of type dst. If the conversion is illegal, convertOp returns nil.
private static Func<Value, Type, Value> convertOp(ptr<rtype> _addr_dst, ptr<rtype> _addr_src) {
    ref rtype dst = ref _addr_dst.val;
    ref rtype src = ref _addr_src.val;


    if (src.Kind() == Int || src.Kind() == Int8 || src.Kind() == Int16 || src.Kind() == Int32 || src.Kind() == Int64) 

        if (dst.Kind() == Int || dst.Kind() == Int8 || dst.Kind() == Int16 || dst.Kind() == Int32 || dst.Kind() == Int64 || dst.Kind() == Uint || dst.Kind() == Uint8 || dst.Kind() == Uint16 || dst.Kind() == Uint32 || dst.Kind() == Uint64 || dst.Kind() == Uintptr) 
            return cvtInt;
        else if (dst.Kind() == Float32 || dst.Kind() == Float64) 
            return cvtIntFloat;
        else if (dst.Kind() == String) 
            return cvtIntString;
            else if (src.Kind() == Uint || src.Kind() == Uint8 || src.Kind() == Uint16 || src.Kind() == Uint32 || src.Kind() == Uint64 || src.Kind() == Uintptr) 

        if (dst.Kind() == Int || dst.Kind() == Int8 || dst.Kind() == Int16 || dst.Kind() == Int32 || dst.Kind() == Int64 || dst.Kind() == Uint || dst.Kind() == Uint8 || dst.Kind() == Uint16 || dst.Kind() == Uint32 || dst.Kind() == Uint64 || dst.Kind() == Uintptr) 
            return cvtUint;
        else if (dst.Kind() == Float32 || dst.Kind() == Float64) 
            return cvtUintFloat;
        else if (dst.Kind() == String) 
            return cvtUintString;
            else if (src.Kind() == Float32 || src.Kind() == Float64) 

        if (dst.Kind() == Int || dst.Kind() == Int8 || dst.Kind() == Int16 || dst.Kind() == Int32 || dst.Kind() == Int64) 
            return cvtFloatInt;
        else if (dst.Kind() == Uint || dst.Kind() == Uint8 || dst.Kind() == Uint16 || dst.Kind() == Uint32 || dst.Kind() == Uint64 || dst.Kind() == Uintptr) 
            return cvtFloatUint;
        else if (dst.Kind() == Float32 || dst.Kind() == Float64) 
            return cvtFloat;
            else if (src.Kind() == Complex64 || src.Kind() == Complex128) 

        if (dst.Kind() == Complex64 || dst.Kind() == Complex128) 
            return cvtComplex;
            else if (src.Kind() == String) 
        if (dst.Kind() == Slice && dst.Elem().PkgPath() == "") {

            if (dst.Elem().Kind() == Uint8) 
                return cvtStringBytes;
            else if (dst.Elem().Kind() == Int32) 
                return cvtStringRunes;
                    }
    else if (src.Kind() == Slice) 
        if (dst.Kind() == String && src.Elem().PkgPath() == "") {

            if (src.Elem().Kind() == Uint8) 
                return cvtBytesString;
            else if (src.Elem().Kind() == Int32) 
                return cvtRunesString;
                    }
        if (dst.Kind() == Ptr && dst.Elem().Kind() == Array && src.Elem() == dst.Elem().Elem()) {
            return cvtSliceArrayPtr;
        }
    else if (src.Kind() == Chan) 
        if (dst.Kind() == Chan && specialChannelAssignability(dst, src)) {
            return cvtDirect;
        }
    // dst and src have same underlying type.
    if (haveIdenticalUnderlyingType(dst, src, false)) {
        return cvtDirect;
    }
    if (dst.Kind() == Ptr && dst.Name() == "" && src.Kind() == Ptr && src.Name() == "" && haveIdenticalUnderlyingType(dst.Elem().common(), src.Elem().common(), false)) {
        return cvtDirect;
    }
    if (implements(dst, src)) {
        if (src.Kind() == Interface) {
            return cvtI2I;
        }
        return cvtT2I;
    }
    return null;
}

// makeInt returns a Value of type t equal to bits (possibly truncated),
// where t is a signed or unsigned int type.
private static Value makeInt(flag f, ulong bits, Type t) {
    var typ = t.common();
    var ptr = unsafe_New(_addr_typ);
    switch (typ.size) {
        case 1: 
            (uint8.val).val;

            (ptr) = uint8(bits);
            break;
        case 2: 
            (uint16.val).val;

            (ptr) = uint16(bits);
            break;
        case 4: 
            (uint32.val).val;

            (ptr) = uint32(bits);
            break;
        case 8: 
            (uint64.val).val;

            (ptr) = bits;
            break;
    }
    return new Value(typ,ptr,f|flagIndir|flag(typ.Kind()));
}

// makeFloat returns a Value of type t equal to v (possibly truncated to float32),
// where t is a float32 or float64 type.
private static Value makeFloat(flag f, double v, Type t) {
    var typ = t.common();
    var ptr = unsafe_New(_addr_typ);
    switch (typ.size) {
        case 4: 
            (float32.val).val;

            (ptr) = float32(v);
            break;
        case 8: 
            (float64.val).val;

            (ptr) = v;
            break;
    }
    return new Value(typ,ptr,f|flagIndir|flag(typ.Kind()));
}

// makeFloat returns a Value of type t equal to v, where t is a float32 type.
private static Value makeFloat32(flag f, float v, Type t) {
    var typ = t.common();
    var ptr = unsafe_New(_addr_typ) * (float32.val);

    (ptr) = v;
    return new Value(typ,ptr,f|flagIndir|flag(typ.Kind()));
}

// makeComplex returns a Value of type t equal to v (possibly truncated to complex64),
// where t is a complex64 or complex128 type.
private static Value makeComplex(flag f, System.Numerics.Complex128 v, Type t) {
    var typ = t.common();
    var ptr = unsafe_New(_addr_typ);
    switch (typ.size) {
        case 8: 
            (complex64.val).val;

            (ptr) = complex64(v);
            break;
        case 16: 
            (complex128.val).val;

            (ptr) = v;
            break;
    }
    return new Value(typ,ptr,f|flagIndir|flag(typ.Kind()));
}

private static Value makeString(flag f, @string v, Type t) {
    var ret = New(t).Elem();
    ret.SetString(v);
    ret.flag = ret.flag & ~flagAddr | f;
    return ret;
}

private static Value makeBytes(flag f, slice<byte> v, Type t) {
    var ret = New(t).Elem();
    ret.SetBytes(v);
    ret.flag = ret.flag & ~flagAddr | f;
    return ret;
}

private static Value makeRunes(flag f, slice<int> v, Type t) {
    var ret = New(t).Elem();
    ret.setRunes(v);
    ret.flag = ret.flag & ~flagAddr | f;
    return ret;
}

// These conversion functions are returned by convertOp
// for classes of conversions. For example, the first function, cvtInt,
// takes any value v of signed int type and returns the value converted
// to type t, where t is any signed or unsigned int type.

// convertOp: intXX -> [u]intXX
private static Value cvtInt(Value v, Type t) {
    return makeInt(v.flag.ro(), uint64(v.Int()), t);
}

// convertOp: uintXX -> [u]intXX
private static Value cvtUint(Value v, Type t) {
    return makeInt(v.flag.ro(), v.Uint(), t);
}

// convertOp: floatXX -> intXX
private static Value cvtFloatInt(Value v, Type t) {
    return makeInt(v.flag.ro(), uint64(int64(v.Float())), t);
}

// convertOp: floatXX -> uintXX
private static Value cvtFloatUint(Value v, Type t) {
    return makeInt(v.flag.ro(), uint64(v.Float()), t);
}

// convertOp: intXX -> floatXX
private static Value cvtIntFloat(Value v, Type t) {
    return makeFloat(v.flag.ro(), float64(v.Int()), t);
}

// convertOp: uintXX -> floatXX
private static Value cvtUintFloat(Value v, Type t) {
    return makeFloat(v.flag.ro(), float64(v.Uint()), t);
}

// convertOp: floatXX -> floatXX
private static Value cvtFloat(Value v, Type t) {
    if (v.Type().Kind() == Float32 && t.Kind() == Float32) { 
        // Don't do any conversion if both types have underlying type float32.
        // This avoids converting to float64 and back, which will
        // convert a signaling NaN to a quiet NaN. See issue 36400.
        return makeFloat32(v.flag.ro(), new ptr<ptr<ptr<float>>>(v.ptr), t);
    }
    return makeFloat(v.flag.ro(), v.Float(), t);
}

// convertOp: complexXX -> complexXX
private static Value cvtComplex(Value v, Type t) {
    return makeComplex(v.flag.ro(), v.Complex(), t);
}

// convertOp: intXX -> string
private static Value cvtIntString(Value v, Type t) {
    @string s = "\uFFFD";
    {
        var x = v.Int();

        if (int64(rune(x)) == x) {
            s = string(rune(x));
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: uintXX -> string
private static Value cvtUintString(Value v, Type t) {
    @string s = "\uFFFD";
    {
        var x = v.Uint();

        if (uint64(rune(x)) == x) {
            s = string(rune(x));
        }
    }
    return makeString(v.flag.ro(), s, t);
}

// convertOp: []byte -> string
private static Value cvtBytesString(Value v, Type t) {
    return makeString(v.flag.ro(), string(v.Bytes()), t);
}

// convertOp: string -> []byte
private static Value cvtStringBytes(Value v, Type t) {
    return makeBytes(v.flag.ro(), (slice<byte>)v.String(), t);
}

// convertOp: []rune -> string
private static Value cvtRunesString(Value v, Type t) {
    return makeString(v.flag.ro(), string(v.runes()), t);
}

// convertOp: string -> []rune
private static Value cvtStringRunes(Value v, Type t) {
    return makeRunes(v.flag.ro(), (slice<int>)v.String(), t);
}

// convertOp: []T -> *[N]T
private static Value cvtSliceArrayPtr(Value v, Type t) => func((_, panic, _) => {
    var n = t.Elem().Len();
    var h = (unsafeheader.Slice.val)(v.ptr);
    if (n > h.Len) {
        panic("reflect: cannot convert slice with length " + itoa.Itoa(h.Len) + " to pointer to array with length " + itoa.Itoa(n));
    }
    return new Value(t.common(),h.Data,v.flag&^(flagIndir|flagAddr|flagKindMask)|flag(Ptr));
});

// convertOp: direct copy
private static Value cvtDirect(Value v, Type typ) {
    var f = v.flag;
    var t = typ.common();
    var ptr = v.ptr;
    if (f & flagAddr != 0) { 
        // indirect, mutable word - make a copy
        var c = unsafe_New(_addr_t);
        typedmemmove(_addr_t, c, ptr);
        ptr = c;
        f &= flagAddr;
    }
    return new Value(t,ptr,v.flag.ro()|f); // v.flag.ro()|f == f?
}

// convertOp: concrete -> interface
private static Value cvtT2I(Value v, Type typ) {
    var target = unsafe_New(_addr_typ.common());
    var x = valueInterface(v, false);
    if (typ.NumMethod() == 0) {
    }
    else
 {
        ifaceE2I(typ._<ptr<rtype>>(), x, target);
    }
    return new Value(typ.common(),target,v.flag.ro()|flagIndir|flag(Interface));
}

// convertOp: interface -> interface
private static Value cvtI2I(Value v, Type typ) {
    if (v.IsNil()) {
        var ret = Zero(typ);
        ret.flag |= v.flag.ro();
        return ret;
    }
    return cvtT2I(v.Elem(), typ);
}

// implemented in ../runtime
private static nint chancap(unsafe.Pointer ch);
private static void chanclose(unsafe.Pointer ch);
private static nint chanlen(unsafe.Pointer ch);

// Note: some of the noescape annotations below are technically a lie,
// but safe in the context of this package. Functions like chansend
// and mapassign don't escape the referent, but may escape anything
// the referent points to (they do shallow copies of the referent).
// It is safe in this package because the referent may only point
// to something a Value may point to, and that is always in the heap
// (due to the escapes() call in ValueOf).

//go:noescape
private static (bool, bool) chanrecv(unsafe.Pointer ch, bool nb, unsafe.Pointer val);

//go:noescape
private static bool chansend(unsafe.Pointer ch, unsafe.Pointer val, bool nb);

private static unsafe.Pointer makechan(ptr<rtype> typ, nint size);
private static unsafe.Pointer makemap(ptr<rtype> t, nint cap);

//go:noescape
private static unsafe.Pointer mapaccess(ptr<rtype> t, unsafe.Pointer m, unsafe.Pointer key);

//go:noescape
private static void mapassign(ptr<rtype> t, unsafe.Pointer m, unsafe.Pointer key, unsafe.Pointer val);

//go:noescape
private static void mapdelete(ptr<rtype> t, unsafe.Pointer m, unsafe.Pointer key);

// m escapes into the return value, but the caller of mapiterinit
// doesn't let the return value escape.
//go:noescape
private static unsafe.Pointer mapiterinit(ptr<rtype> t, unsafe.Pointer m);

//go:noescape
private static unsafe.Pointer mapiterkey(unsafe.Pointer it);

//go:noescape
private static unsafe.Pointer mapiterelem(unsafe.Pointer it);

//go:noescape
private static void mapiternext(unsafe.Pointer it);

//go:noescape
private static nint maplen(unsafe.Pointer m);

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
//go:noescape
//go:linkname call runtime.reflectcall
private static void call(ptr<rtype> stackArgsType, unsafe.Pointer f, unsafe.Pointer stackArgs, uint stackArgsSize, uint stackRetOffset, uint frameSize, ptr<abi.RegArgs> regArgs);

private static void ifaceE2I(ptr<rtype> t, object src, unsafe.Pointer dst);

// memmove copies size bytes to dst from src. No write barriers are used.
//go:noescape
private static void memmove(unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr size);

// typedmemmove copies a value of type t to dst from src.
//go:noescape
private static void typedmemmove(ptr<rtype> t, unsafe.Pointer dst, unsafe.Pointer src);

// typedmemmovepartial is like typedmemmove but assumes that
// dst and src point off bytes into the value and only copies size bytes.
//go:noescape
private static void typedmemmovepartial(ptr<rtype> t, unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr off, System.UIntPtr size);

// typedmemclr zeros the value at ptr of type t.
//go:noescape
private static void typedmemclr(ptr<rtype> t, unsafe.Pointer ptr);

// typedmemclrpartial is like typedmemclr but assumes that
// dst points off bytes into the value and only clears size bytes.
//go:noescape
private static void typedmemclrpartial(ptr<rtype> t, unsafe.Pointer ptr, System.UIntPtr off, System.UIntPtr size);

// typedslicecopy copies a slice of elemType values from src to dst,
// returning the number of elements copied.
//go:noescape
private static nint typedslicecopy(ptr<rtype> elemType, unsafeheader.Slice dst, unsafeheader.Slice src);

//go:noescape
private static System.UIntPtr typehash(ptr<rtype> t, unsafe.Pointer p, System.UIntPtr h);

// Dummy annotation marking that the value x escapes,
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
private static void escapes(object x) {
    if (dummy.b) {>>MARKER:FUNCTION_typehash_BLOCK_PREFIX<<
        dummy.x = x;
    }
}

private static var dummy = default;

} // end reflect_package
