// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:23:46 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Should be a built-in for unsafe.Pointer?
        //go:nosplit
        private static unsafe.Pointer add(unsafe.Pointer p, System.UIntPtr x)
        {
            return @unsafe.Pointer(uintptr(p) + x);
        }

        // getg returns the pointer to the current g.
        // The compiler rewrites calls to this function into instructions
        // that fetch the g directly (from TLS or from the dedicated register).
        private static ptr<g> getg()
;

        // mcall switches from the g to the g0 stack and invokes fn(g),
        // where g is the goroutine that made the call.
        // mcall saves g's current PC/SP in g->sched so that it can be restored later.
        // It is up to fn to arrange for that later execution, typically by recording
        // g in a data structure, causing something to call ready(g) later.
        // mcall returns to the original goroutine g later, when g has been rescheduled.
        // fn must not return at all; typically it ends by calling schedule, to let the m
        // run other goroutines.
        //
        // mcall can only be called from g stacks (not g0, not gsignal).
        //
        // This must NOT be go:noescape: if fn is a stack-allocated closure,
        // fn puts g on a run queue, and g executes before fn returns, the
        // closure will be invalidated while it is still executing.
        private static void mcall(Action<ptr<g>> fn)
;

        // systemstack runs fn on a system stack.
        // If systemstack is called from the per-OS-thread (g0) stack, or
        // if systemstack is called from the signal handling (gsignal) stack,
        // systemstack calls fn directly and returns.
        // Otherwise, systemstack is being called from the limited stack
        // of an ordinary goroutine. In this case, systemstack switches
        // to the per-OS-thread stack, calls fn, and switches back.
        // It is common to use a func literal as the argument, in order
        // to share inputs and outputs with the code around the call
        // to system stack:
        //
        //    ... set up y ...
        //    systemstack(func() {
        //        x = bigcall(y)
        //    })
        //    ... use x ...
        //
        //go:noescape
        private static void systemstack(Action fn)
;

        private static @string badsystemstackMsg = "fatal: systemstack called from unexpected goroutine";

        //go:nosplit
        //go:nowritebarrierrec
        private static void badsystemstack()
        {
            var sp = stringStructOf(_addr_badsystemstackMsg);
            write(2L, sp.str, int32(sp.len));
        }

        // memclrNoHeapPointers clears n bytes starting at ptr.
        //
        // Usually you should use typedmemclr. memclrNoHeapPointers should be
        // used only when the caller knows that *ptr contains no heap pointers
        // because either:
        //
        // *ptr is initialized memory and its type is pointer-free, or
        //
        // *ptr is uninitialized memory (e.g., memory that's being reused
        // for a new allocation) and hence contains only "junk".
        //
        // The (CPU-specific) implementations of this function are in memclr_*.s.
        //go:noescape
        private static void memclrNoHeapPointers(unsafe.Pointer ptr, System.UIntPtr n)
;

        //go:linkname reflect_memclrNoHeapPointers reflect.memclrNoHeapPointers
        private static void reflect_memclrNoHeapPointers(unsafe.Pointer ptr, System.UIntPtr n)
        {
            memclrNoHeapPointers(ptr, n);
        }

        // memmove copies n bytes from "from" to "to".
        //
        // memmove ensures that any pointer in "from" is written to "to" with
        // an indivisible write, so that racy reads cannot observe a
        // half-written pointer. This is necessary to prevent the garbage
        // collector from observing invalid pointers, and differs from memmove
        // in unmanaged languages. However, memmove is only required to do
        // this if "from" and "to" may contain pointers, which can only be the
        // case if "from", "to", and "n" are all be word-aligned.
        //
        // Implementations are in memmove_*.s.
        //
        //go:noescape
        private static void memmove(unsafe.Pointer to, unsafe.Pointer from, System.UIntPtr n)
;

        //go:linkname reflect_memmove reflect.memmove
        private static void reflect_memmove(unsafe.Pointer to, unsafe.Pointer from, System.UIntPtr n)
        {
            memmove(to, from, n);
        }

        // exported value for testing
        private static var hashLoad = float32(loadFactorNum) / float32(loadFactorDen);

        //go:nosplit
        private static uint fastrand()
        {
            var mp = getg().m; 
            // Implement xorshift64+: 2 32-bit xorshift sequences added together.
            // Shift triplet [17,7,16] was calculated as indicated in Marsaglia's
            // Xorshift paper: https://www.jstatsoft.org/article/view/v008i14/xorshift.pdf
            // This generator passes the SmallCrush suite, part of TestU01 framework:
            // http://simul.iro.umontreal.ca/testu01/tu01.html
            var s1 = mp.fastrand[0L];
            var s0 = mp.fastrand[1L];
            s1 ^= s1 << (int)(17L);
            s1 = s1 ^ s0 ^ s1 >> (int)(7L) ^ s0 >> (int)(16L);
            mp.fastrand[0L] = s0;
            mp.fastrand[1L] = s1;
            return s0 + s1;

        }

        //go:nosplit
        private static uint fastrandn(uint n)
        { 
            // This is similar to fastrand() % n, but faster.
            // See https://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/
            return uint32(uint64(fastrand()) * uint64(n) >> (int)(32L));

        }

        //go:linkname sync_fastrand sync.fastrand
        private static uint sync_fastrand()
        {
            return fastrand();
        }

        // in internal/bytealg/equal_*.s
        //go:noescape
        private static bool memequal(unsafe.Pointer a, unsafe.Pointer b, System.UIntPtr size)
;

        // noescape hides a pointer from escape analysis.  noescape is
        // the identity function but escape analysis doesn't think the
        // output depends on the input.  noescape is inlined and currently
        // compiles down to zero instructions.
        // USE CAREFULLY!
        //go:nosplit
        private static unsafe.Pointer noescape(unsafe.Pointer p)
        {
            var x = uintptr(p);
            return @unsafe.Pointer(x ^ 0L);
        }

        private static void cgocallback(unsafe.Pointer fn, unsafe.Pointer frame, System.UIntPtr framesize, System.UIntPtr ctxt)
;
        private static void gogo(ptr<gobuf> buf)
;
        private static void gosave(ptr<gobuf> buf)
;

        //go:noescape
        private static void jmpdefer(ptr<funcval> fv, System.UIntPtr argp)
;
        private static void asminit()
;
        private static void setg(ptr<g> gg)
;
        private static void breakpoint()
;

        // reflectcall calls fn with a copy of the n argument bytes pointed at by arg.
        // After fn returns, reflectcall copies n-retoffset result bytes
        // back into arg+retoffset before returning. If copying result bytes back,
        // the caller should pass the argument frame type as argtype, so that
        // call can execute appropriate write barriers during the copy.
        // Package reflect passes a frame type. In package runtime, there is only
        // one call that copies results back, in cgocallbackg1, and it does NOT pass a
        // frame type, meaning there are no write barriers invoked. See that call
        // site for justification.
        //
        // Package reflect accesses this symbol through a linkname.
        private static void reflectcall(ptr<_type> argtype, unsafe.Pointer fn, unsafe.Pointer arg, uint argsize, uint retoffset)
;

        private static void procyield(uint cycles)
;

        private partial struct neverCallThisFunction
        {
        }

        // goexit is the return stub at the top of every goroutine call stack.
        // Each goroutine stack is constructed as if goexit called the
        // goroutine's entry point function, so that when the entry point
        // function returns, it will return to goexit, which will call goexit1
        // to perform the actual exit.
        //
        // This function must never be called directly. Call goexit1 instead.
        // gentraceback assumes that goexit terminates the stack. A direct
        // call on the stack will cause gentraceback to stop walking the stack
        // prematurely and if there is leftover state it may panic.
        private static void goexit(neverCallThisFunction _p0)
;

        // Not all cgocallback_gofunc frames are actually cgocallback_gofunc,
        // so not all have these arguments. Mark them uintptr so that the GC
        // does not misinterpret memory when the arguments are not present.
        // cgocallback_gofunc is not called from go, only from cgocallback,
        // so the arguments will be found via cgocallback's pointer-declared arguments.
        // See the assembly implementations for more details.
        private static void cgocallback_gofunc(System.UIntPtr fv, System.UIntPtr frame, System.UIntPtr framesize, System.UIntPtr ctxt)
;

        // publicationBarrier performs a store/store barrier (a "publication"
        // or "export" barrier). Some form of synchronization is required
        // between initializing an object and making that object accessible to
        // another processor. Without synchronization, the initialization
        // writes and the "publication" write may be reordered, allowing the
        // other processor to follow the pointer and observe an uninitialized
        // object. In general, higher-level synchronization should be used,
        // such as locking or an atomic pointer write. publicationBarrier is
        // for when those aren't an option, such as in the implementation of
        // the memory manager.
        //
        // There's no corresponding barrier for the read side because the read
        // side naturally has a data dependency order. All architectures that
        // Go supports or seems likely to ever support automatically enforce
        // data dependency ordering.
        private static void publicationBarrier()
;

        // getcallerpc returns the program counter (PC) of its caller's caller.
        // getcallersp returns the stack pointer (SP) of its caller's caller.
        // The implementation may be a compiler intrinsic; there is not
        // necessarily code implementing this on every platform.
        //
        // For example:
        //
        //    func f(arg1, arg2, arg3 int) {
        //        pc := getcallerpc()
        //        sp := getcallersp()
        //    }
        //
        // These two lines find the PC and SP immediately following
        // the call to f (where f will return).
        //
        // The call to getcallerpc and getcallersp must be done in the
        // frame being asked about.
        //
        // The result of getcallersp is correct at the time of the return,
        // but it may be invalidated by any subsequent call to a function
        // that might relocate the stack in order to grow or shrink it.
        // A general rule is that the result of getcallersp should be used
        // immediately and can only be passed to nosplit functions.

        //go:noescape
        private static System.UIntPtr getcallerpc()
;

        //go:noescape
        private static System.UIntPtr getcallersp()
; // implemented as an intrinsic on all platforms

        // getclosureptr returns the pointer to the current closure.
        // getclosureptr can only be used in an assignment statement
        // at the entry of a function. Moreover, go:nosplit directive
        // must be specified at the declaration of caller function,
        // so that the function prolog does not clobber the closure register.
        // for example:
        //
        //    //go:nosplit
        //    func f(arg1, arg2, arg3 int) {
        //        dx := getclosureptr()
        //    }
        //
        // The compiler rewrites calls to this function into instructions that fetch the
        // pointer from a well-known register (DX on x86 architecture, etc.) directly.
        private static System.UIntPtr getclosureptr()
;

        //go:noescape
        private static int asmcgocall(unsafe.Pointer fn, unsafe.Pointer arg)
;

        private static void morestack()
;
        private static void morestack_noctxt()
;
        private static void rt0_go()
;

        // return0 is a stub used to return 0 from deferproc.
        // It is called at the very end of deferproc to signal
        // the calling Go function that it should not jump
        // to deferreturn.
        // in asm_*.s
        private static void return0()
;

        // in asm_*.s
        // not called directly; definitions here supply type information for traceback.
        private static void call32(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call64(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call128(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call256(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call512(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call1024(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call2048(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call4096(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call8192(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call16384(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call32768(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call65536(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call131072(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call262144(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call524288(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call1048576(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call2097152(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call4194304(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call8388608(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call16777216(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call33554432(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call67108864(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call134217728(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call268435456(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call536870912(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;
        private static void call1073741824(unsafe.Pointer typ, unsafe.Pointer fn, unsafe.Pointer arg, uint n, uint retoffset)
;

        private static void systemstack_switch()
;

        // alignUp rounds n up to a multiple of a. a must be a power of 2.
        private static System.UIntPtr alignUp(System.UIntPtr n, System.UIntPtr a)
        {
            return (n + a - 1L) & ~(a - 1L);
        }

        // alignDown rounds n down to a multiple of a. a must be a power of 2.
        private static System.UIntPtr alignDown(System.UIntPtr n, System.UIntPtr a)
        {
            return n & ~(a - 1L);
        }

        // divRoundUp returns ceil(n / a).
        private static System.UIntPtr divRoundUp(System.UIntPtr n, System.UIntPtr a)
        { 
            // a is generally a power of two. This will get inlined and
            // the compiler will optimize the division.
            return (n + a - 1L) / a;

        }

        // checkASM reports whether assembly runtime checks have passed.
        private static bool checkASM()
;

        private static bool memequal_varlen(unsafe.Pointer a, unsafe.Pointer b)
;

        // bool2int returns 0 if x is false or 1 if x is true.
        private static long bool2int(bool x)
        { 
            // Avoid branches. In the SSA compiler, this compiles to
            // exactly what you would want it to.
            return int(uint8(new ptr<ptr<ptr<byte>>>(@unsafe.Pointer(_addr_x))));

        }

        // abort crashes the runtime in situations where even throw might not
        // work. In general it should do something a debugger will recognize
        // (e.g., an INT3 on x86). A crash in abort is recognized by the
        // signal handler, which will attempt to tear down the runtime
        // immediately.
        private static void abort()
;

        // Called from compiled code; declared for vet; do NOT call from Go.
        private static void gcWriteBarrier()
;
        private static void duffzero()
;
        private static void duffcopy()
;

        // Called from linker-generated .initarray; declared for go vet; do NOT call from Go.
        private static void addmoduledata()
;
    }
}
