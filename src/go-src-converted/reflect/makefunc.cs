// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// MakeFunc implementation.

// package reflect -- go2cs converted at 2020 October 09 05:06:35 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Go\src\reflect\makefunc.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class reflect_package
    {
        // makeFuncImpl is the closure value implementing the function
        // returned by MakeFunc.
        // The first three words of this type must be kept in sync with
        // methodValue and runtime.reflectMethodValue.
        // Any changes should be reflected in all three.
        private partial struct makeFuncImpl
        {
            public System.UIntPtr code;
            public ptr<bitVector> stack; // ptrmap for both args and results
            public System.UIntPtr argLen; // just args
            public ptr<funcType> ftyp;
            public Func<slice<Value>, slice<Value>> fn;
        }

        // MakeFunc returns a new function of the given Type
        // that wraps the function fn. When called, that new function
        // does the following:
        //
        //    - converts its arguments to a slice of Values.
        //    - runs results := fn(args).
        //    - returns the results as a slice of Values, one per formal result.
        //
        // The implementation fn can assume that the argument Value slice
        // has the number and type of arguments given by typ.
        // If typ describes a variadic function, the final Value is itself
        // a slice representing the variadic arguments, as in the
        // body of a variadic function. The result Value slice returned by fn
        // must have the number and type of results given by typ.
        //
        // The Value.Call method allows the caller to invoke a typed function
        // in terms of Values; in contrast, MakeFunc allows the caller to implement
        // a typed function in terms of Values.
        //
        // The Examples section of the documentation includes an illustration
        // of how to use MakeFunc to build a swap function for different types.
        //
        public static Value MakeFunc(Type typ, Func<slice<Value>, slice<Value>> fn) => func((_, panic, __) =>
        {
            if (typ.Kind() != Func)
            {
                panic("reflect: call of MakeFunc with non-Func type");
            }

            var t = typ.common();
            var ftyp = (funcType.val)(@unsafe.Pointer(t)); 

            // Indirect Go func value (dummy) to obtain
            // actual code address. (A Go func value is a pointer
            // to a C function pointer. https://golang.org/s/go11func.)
            ref var dummy = ref heap(makeFuncStub, out ptr<var> _addr_dummy);
            ptr<ptr<ptr<ptr<System.UIntPtr>>>> code = new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_dummy)); 

            // makeFuncImpl contains a stack map for use by the runtime
            var (_, argLen, _, stack, _) = funcLayout(ftyp, null);

            ptr<makeFuncImpl> impl = addr(new makeFuncImpl(code:code,stack:stack,argLen:argLen,ftyp:ftyp,fn:fn));

            return new Value(t,unsafe.Pointer(impl),flag(Func));

        });

        // makeFuncStub is an assembly function that is the code half of
        // the function returned from MakeFunc. It expects a *callReflectFunc
        // as its context register, and its job is to invoke callReflect(ctxt, frame)
        // where ctxt is the context register and frame is a pointer to the first
        // word in the passed-in argument frame.
        private static void makeFuncStub()
;

        // The first 3 words of this type must be kept in sync with
        // makeFuncImpl and runtime.reflectMethodValue.
        // Any changes should be reflected in all three.
        private partial struct methodValue
        {
            public System.UIntPtr fn;
            public ptr<bitVector> stack; // ptrmap for both args and results
            public System.UIntPtr argLen; // just args
            public long method;
            public Value rcvr;
        }

        // makeMethodValue converts v from the rcvr+method index representation
        // of a method value to an actual method func value, which is
        // basically the receiver value with a special bit set, into a true
        // func value - a value holding an actual func. The output is
        // semantically equivalent to the input as far as the user of package
        // reflect can tell, but the true func representation can be handled
        // by code like Convert and Interface and Assign.
        private static Value makeMethodValue(@string op, Value v) => func((_, panic, __) =>
        {
            if (v.flag & flagMethod == 0L)
            {>>MARKER:FUNCTION_makeFuncStub_BLOCK_PREFIX<<
                panic("reflect: internal error: invalid use of makeMethodValue");
            } 

            // Ignoring the flagMethod bit, v describes the receiver, not the method type.
            var fl = v.flag & (flagRO | flagAddr | flagIndir);
            fl |= flag(v.typ.Kind());
            Value rcvr = new Value(v.typ,v.ptr,fl); 

            // v.Type returns the actual type of the method value.
            var ftyp = (funcType.val)(@unsafe.Pointer(v.Type()._<ptr<rtype>>())); 

            // Indirect Go func value (dummy) to obtain
            // actual code address. (A Go func value is a pointer
            // to a C function pointer. https://golang.org/s/go11func.)
            ref var dummy = ref heap(methodValueCall, out ptr<var> _addr_dummy);
            ptr<ptr<ptr<ptr<System.UIntPtr>>>> code = new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_dummy)); 

            // methodValue contains a stack map for use by the runtime
            var (_, argLen, _, stack, _) = funcLayout(ftyp, null);

            ptr<methodValue> fv = addr(new methodValue(fn:code,stack:stack,argLen:argLen,method:int(v.flag)>>flagMethodShift,rcvr:rcvr,)); 

            // Cause panic if method is not appropriate.
            // The panic would still happen during the call if we omit this,
            // but we want Interface() and other operations to fail early.
            methodReceiver(op, fv.rcvr, fv.method);

            return new Value(&ftyp.rtype,unsafe.Pointer(fv),v.flag&flagRO|flag(Func));

        });

        // methodValueCall is an assembly function that is the code half of
        // the function returned from makeMethodValue. It expects a *methodValue
        // as its context register, and its job is to invoke callMethod(ctxt, frame)
        // where ctxt is the context register and frame is a pointer to the first
        // word in the passed-in argument frame.
        private static void methodValueCall()
;
    }
}
