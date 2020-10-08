// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Cgo call and callback support.
//
// To call into the C function f from Go, the cgo-generated code calls
// runtime.cgocall(_cgo_Cfunc_f, frame), where _cgo_Cfunc_f is a
// gcc-compiled function written by cgo.
//
// runtime.cgocall (below) calls entersyscall so as not to block
// other goroutines or the garbage collector, and then calls
// runtime.asmcgocall(_cgo_Cfunc_f, frame).
//
// runtime.asmcgocall (in asm_$GOARCH.s) switches to the m->g0 stack
// (assumed to be an operating system-allocated stack, so safe to run
// gcc-compiled code on) and calls _cgo_Cfunc_f(frame).
//
// _cgo_Cfunc_f invokes the actual C function f with arguments
// taken from the frame structure, records the results in the frame,
// and returns to runtime.asmcgocall.
//
// After it regains control, runtime.asmcgocall switches back to the
// original g (m->curg)'s stack and returns to runtime.cgocall.
//
// After it regains control, runtime.cgocall calls exitsyscall, which blocks
// until this m can run Go code without violating the $GOMAXPROCS limit,
// and then unlocks g from m.
//
// The above description skipped over the possibility of the gcc-compiled
// function f calling back into Go. If that happens, we continue down
// the rabbit hole during the execution of f.
//
// To make it possible for gcc-compiled C code to call a Go function p.GoF,
// cgo writes a gcc-compiled function named GoF (not p.GoF, since gcc doesn't
// know about packages).  The gcc-compiled C function f calls GoF.
//
// GoF calls crosscall2(_cgoexp_GoF, frame, framesize).  Crosscall2
// (in cgo/gcc_$GOARCH.S, a gcc-compiled assembly file) is a two-argument
// adapter from the gcc function call ABI to the 6c function call ABI.
// It is called from gcc to call 6c functions. In this case it calls
// _cgoexp_GoF(frame, framesize), still running on m->g0's stack
// and outside the $GOMAXPROCS limit. Thus, this code cannot yet
// call arbitrary Go code directly and must be careful not to allocate
// memory or use up m->g0's stack.
//
// _cgoexp_GoF calls runtime.cgocallback(p.GoF, frame, framesize, ctxt).
// (The reason for having _cgoexp_GoF instead of writing a crosscall3
// to make this call directly is that _cgoexp_GoF, because it is compiled
// with 6c instead of gcc, can refer to dotted names like
// runtime.cgocallback and p.GoF.)
//
// runtime.cgocallback (in asm_$GOARCH.s) switches from m->g0's
// stack to the original g (m->curg)'s stack, on which it calls
// runtime.cgocallbackg(p.GoF, frame, framesize).
// As part of the stack switch, runtime.cgocallback saves the current
// SP as m->g0->sched.sp, so that any use of m->g0's stack during the
// execution of the callback will be done below the existing stack frames.
// Before overwriting m->g0->sched.sp, it pushes the old value on the
// m->g0 stack, so that it can be restored later.
//
// runtime.cgocallbackg (below) is now running on a real goroutine
// stack (not an m->g0 stack).  First it calls runtime.exitsyscall, which will
// block until the $GOMAXPROCS limit allows running this goroutine.
// Once exitsyscall has returned, it is safe to do things like call the memory
// allocator or invoke the Go callback function p.GoF.  runtime.cgocallbackg
// first defers a function to unwind m->g0.sched.sp, so that if p.GoF
// panics, m->g0.sched.sp will be restored to its old value: the m->g0 stack
// and the m->curg stack will be unwound in lock step.
// Then it calls p.GoF.  Finally it pops but does not execute the deferred
// function, calls runtime.entersyscall, and returns to runtime.cgocallback.
//
// After it regains control, runtime.cgocallback switches back to
// m->g0's stack (the pointer is still in m->g0.sched.sp), restores the old
// m->g0.sched.sp value from the stack, and returns to _cgoexp_GoF.
//
// _cgoexp_GoF immediately returns to crosscall2, which restores the
// callee-save registers for gcc and returns to GoF, which returns to f.

// package runtime -- go2cs converted at 2020 October 08 03:19:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cgocall.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Addresses collected in a cgo backtrace when crashing.
        // Length must match arg.Max in x_cgo_callers in runtime/cgo/gcc_traceback.c.
        private partial struct cgoCallers // : array<System.UIntPtr>
        {
        }

        // Call from Go to C.
        //
        // This must be nosplit because it's used for syscalls on some
        // platforms. Syscalls may have untyped arguments on the stack, so
        // it's not safe to grow or scan the stack.
        //
        //go:nosplit
        private static int cgocall(unsafe.Pointer fn, unsafe.Pointer arg)
        {
            if (!iscgo && GOOS != "solaris" && GOOS != "illumos" && GOOS != "windows")
            {
                throw("cgocall unavailable");
            }

            if (fn == null)
            {
                throw("cgocall nil");
            }

            if (raceenabled)
            {
                racereleasemerge(@unsafe.Pointer(_addr_racecgosync));
            }

            var mp = getg().m;
            mp.ncgocall++;
            mp.ncgo++; 

            // Reset traceback.
            mp.cgoCallers[0L] = 0L; 

            // Announce we are entering a system call
            // so that the scheduler knows to create another
            // M to run goroutines while we are in the
            // foreign code.
            //
            // The call to asmcgocall is guaranteed not to
            // grow the stack and does not allocate memory,
            // so it is safe to call while "in a system call", outside
            // the $GOMAXPROCS accounting.
            //
            // fn may call back into Go code, in which case we'll exit the
            // "system call", run the Go code (which may grow the stack),
            // and then re-enter the "system call" reusing the PC and SP
            // saved by entersyscall here.
            entersyscall(); 

            // Tell asynchronous preemption that we're entering external
            // code. We do this after entersyscall because this may block
            // and cause an async preemption to fail, but at this point a
            // sync preemption will succeed (though this is not a matter
            // of correctness).
            osPreemptExtEnter(mp);

            mp.incgo = true;
            var errno = asmcgocall(fn, arg); 

            // Update accounting before exitsyscall because exitsyscall may
            // reschedule us on to a different M.
            mp.incgo = false;
            mp.ncgo--;

            osPreemptExtExit(mp);

            exitsyscall(); 

            // Note that raceacquire must be called only after exitsyscall has
            // wired this M to a P.
            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(_addr_racecgosync));
            } 

            // From the garbage collector's perspective, time can move
            // backwards in the sequence above. If there's a callback into
            // Go code, GC will see this function at the call to
            // asmcgocall. When the Go call later returns to C, the
            // syscall PC/SP is rolled back and the GC sees this function
            // back at the call to entersyscall. Normally, fn and arg
            // would be live at entersyscall and dead at asmcgocall, so if
            // time moved backwards, GC would see these arguments as dead
            // and then live. Prevent these undead arguments from crashing
            // GC by forcing them to stay live across this time warp.
            KeepAlive(fn);
            KeepAlive(arg);
            KeepAlive(mp);

            return errno;

        }

        // Call from C back to Go.
        //go:nosplit
        private static void cgocallbackg(System.UIntPtr ctxt)
        {
            var gp = getg();
            if (gp != gp.m.curg)
            {
                println("runtime: bad g in cgocallback");
                exit(2L);
            } 

            // The call from C is on gp.m's g0 stack, so we must ensure
            // that we stay on that M. We have to do this before calling
            // exitsyscall, since it would otherwise be free to move us to
            // a different M. The call to unlockOSThread is in unwindm.
            lockOSThread(); 

            // Save current syscall parameters, so m.syscall can be
            // used again if callback decide to make syscall.
            var syscall = gp.m.syscall; 

            // entersyscall saves the caller's SP to allow the GC to trace the Go
            // stack. However, since we're returning to an earlier stack frame and
            // need to pair with the entersyscall() call made by cgocall, we must
            // save syscall* and let reentersyscall restore them.
            var savedsp = @unsafe.Pointer(gp.syscallsp);
            var savedpc = gp.syscallpc;
            exitsyscall(); // coming out of cgo call
            gp.m.incgo = false;

            osPreemptExtExit(gp.m);

            cgocallbackg1(ctxt); 

            // At this point unlockOSThread has been called.
            // The following code must not change to a different m.
            // This is enforced by checking incgo in the schedule function.

            osPreemptExtEnter(gp.m);

            gp.m.incgo = true; 
            // going back to cgo call
            reentersyscall(savedpc, uintptr(savedsp));

            gp.m.syscall = syscall;

        }

        private static void cgocallbackg1(System.UIntPtr ctxt) => func((defer, _, __) =>
        {
            var gp = getg();
            if (gp.m.needextram || atomic.Load(_addr_extraMWaiters) > 0L)
            {
                gp.m.needextram = false;
                systemstack(newextram);
            }

            if (ctxt != 0L)
            {
                var s = append(gp.cgoCtxt, ctxt); 

                // Now we need to set gp.cgoCtxt = s, but we could get
                // a SIGPROF signal while manipulating the slice, and
                // the SIGPROF handler could pick up gp.cgoCtxt while
                // tracing up the stack.  We need to ensure that the
                // handler always sees a valid slice, so set the
                // values in an order such that it always does.
                var p = (slice.val)(@unsafe.Pointer(_addr_gp.cgoCtxt));
                atomicstorep(@unsafe.Pointer(_addr_p.array), @unsafe.Pointer(_addr_s[0L]));
                p.cap = cap(s);
                p.len = len(s);

                defer(gp =>
                { 
                    // Decrease the length of the slice by one, safely.
                    p = (slice.val)(@unsafe.Pointer(_addr_gp.cgoCtxt));
                    p.len--;

                }(gp));

            }

            if (gp.m.ncgo == 0L)
            { 
                // The C call to Go came from a thread not currently running
                // any Go. In the case of -buildmode=c-archive or c-shared,
                // this call may be coming in before package initialization
                // is complete. Wait until it is.
                main_init_done.Receive();

            } 

            // Add entry to defer stack in case of panic.
            ref var restore = ref heap(true, out ptr<var> _addr_restore);
            defer(unwindm(_addr_restore));

            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(_addr_racecgosync));
            }

            private partial struct args
            {
                public ptr<funcval> fn;
                public unsafe.Pointer arg;
                public System.UIntPtr argsize;
            }
            ptr<args> cb; 

            // Location of callback arguments depends on stack frame layout
            // and size of stack frame of cgocallback_gofunc.
            var sp = gp.m.g0.sched.sp;
            switch (GOARCH)
            {
                case "arm": 
                    // On arm, stack frame is two words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.
                    cb = (args.val)(@unsafe.Pointer(sp + 4L * sys.PtrSize));
                    break;
                case "arm64": 
                    // On arm64, stack frame is four words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.
                    // Additional two words (16-byte alignment) are for saving FP.
                    cb = (args.val)(@unsafe.Pointer(sp + 7L * sys.PtrSize));
                    break;
                case "amd64": 
                    // On amd64, stack frame is two words, plus caller PC.
                    if (framepointer_enabled)
                    { 
                        // In this case, there's also saved BP.
                        cb = (args.val)(@unsafe.Pointer(sp + 4L * sys.PtrSize));
                        break;

                    }

                    cb = (args.val)(@unsafe.Pointer(sp + 3L * sys.PtrSize));
                    break;
                case "386": 
                    // On 386, stack frame is three words, plus caller PC.
                    cb = (args.val)(@unsafe.Pointer(sp + 4L * sys.PtrSize));
                    break;
                case "ppc64": 
                    // On ppc64 and s390x, the callback arguments are in the arguments area of
                    // cgocallback's stack frame. The stack looks like this:
                    // +--------------------+------------------------------+
                    // |                    | ...                          |
                    // | cgoexp_$fn         +------------------------------+
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+
                    // |                    | arguments area               |
                    // | cgocallback        +------------------------------+ <- sp + 2*minFrameSize + 2*ptrSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp + minFrameSize + 2*ptrSize
                    // |                    | local variables (2 pointers) |
                    // | cgocallback_gofunc +------------------------------+ <- sp + minFrameSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp

                case "ppc64le": 
                    // On ppc64 and s390x, the callback arguments are in the arguments area of
                    // cgocallback's stack frame. The stack looks like this:
                    // +--------------------+------------------------------+
                    // |                    | ...                          |
                    // | cgoexp_$fn         +------------------------------+
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+
                    // |                    | arguments area               |
                    // | cgocallback        +------------------------------+ <- sp + 2*minFrameSize + 2*ptrSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp + minFrameSize + 2*ptrSize
                    // |                    | local variables (2 pointers) |
                    // | cgocallback_gofunc +------------------------------+ <- sp + minFrameSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp

                case "s390x": 
                    // On ppc64 and s390x, the callback arguments are in the arguments area of
                    // cgocallback's stack frame. The stack looks like this:
                    // +--------------------+------------------------------+
                    // |                    | ...                          |
                    // | cgoexp_$fn         +------------------------------+
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+
                    // |                    | arguments area               |
                    // | cgocallback        +------------------------------+ <- sp + 2*minFrameSize + 2*ptrSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp + minFrameSize + 2*ptrSize
                    // |                    | local variables (2 pointers) |
                    // | cgocallback_gofunc +------------------------------+ <- sp + minFrameSize
                    // |                    | fixed frame area             |
                    // +--------------------+------------------------------+ <- sp
                    cb = (args.val)(@unsafe.Pointer(sp + 2L * sys.MinFrameSize + 2L * sys.PtrSize));
                    break;
                case "mips64": 
                    // On mips64x, stack frame is two words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.

                case "mips64le": 
                    // On mips64x, stack frame is two words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.
                    cb = (args.val)(@unsafe.Pointer(sp + 4L * sys.PtrSize));
                    break;
                case "mips": 
                    // On mipsx, stack frame is two words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.

                case "mipsle": 
                    // On mipsx, stack frame is two words and there's a saved LR between
                    // SP and the stack frame and between the stack frame and the arguments.
                    cb = (args.val)(@unsafe.Pointer(sp + 4L * sys.PtrSize));
                    break;
                default: 
                    throw("cgocallbackg is unimplemented on arch");
                    break;
            } 

            // Invoke callback.
            // NOTE(rsc): passing nil for argtype means that the copying of the
            // results back into cb.arg happens without any corresponding write barriers.
            // For cgo, cb.arg points into a C stack frame and therefore doesn't
            // hold any pointers that the GC can find anyway - the write barrier
            // would be a no-op.
            reflectcall(null, @unsafe.Pointer(cb.fn), cb.arg, uint32(cb.argsize), 0L);

            if (raceenabled)
            {
                racereleasemerge(@unsafe.Pointer(_addr_racecgosync));
            }

            if (msanenabled)
            { 
                // Tell msan that we wrote to the entire argument block.
                // This tells msan that we set the results.
                // Since we have already called the function it doesn't
                // matter that we are writing to the non-result parameters.
                msanwrite(cb.arg, cb.argsize);

            } 

            // Do not unwind m->g0->sched.sp.
            // Our caller, cgocallback, will do that.
            restore = false;

        });

        private static void unwindm(ptr<bool> _addr_restore)
        {
            ref bool restore = ref _addr_restore.val;

            if (restore)
            { 
                // Restore sp saved by cgocallback during
                // unwind of g's stack (see comment at top of file).
                var mp = acquirem();
                var sched = _addr_mp.g0.sched;
                switch (GOARCH)
                {
                    case "386": 

                    case "amd64": 

                    case "arm": 

                    case "ppc64": 

                    case "ppc64le": 

                    case "mips64": 

                    case "mips64le": 

                    case "s390x": 

                    case "mips": 

                    case "mipsle": 
                        sched.sp = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(sched.sp + sys.MinFrameSize));
                        break;
                    case "arm64": 
                        sched.sp = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(sched.sp + 16L));
                        break;
                    default: 
                        throw("unwindm not implemented");
                        break;
                } 

                // Do the accounting that cgocall will not have a chance to do
                // during an unwind.
                //
                // In the case where a Go call originates from C, ncgo is 0
                // and there is no matching cgocall to end.
                if (mp.ncgo > 0L)
                {
                    mp.incgo = false;
                    mp.ncgo--;
                    osPreemptExtExit(mp);
                }

                releasem(mp);

            } 

            // Undo the call to lockOSThread in cgocallbackg.
            // We must still stay on the same m.
            unlockOSThread();

        }

        // called from assembly
        private static void badcgocallback()
        {
            throw("misaligned stack in cgocallback");
        }

        // called from (incomplete) assembly
        private static void cgounimpl()
        {
            throw("cgo not implemented");
        }

        private static ulong racecgosync = default; // represents possible synchronization in C code

        // Pointer checking for cgo code.

        // We want to detect all cases where a program that does not use
        // unsafe makes a cgo call passing a Go pointer to memory that
        // contains a Go pointer. Here a Go pointer is defined as a pointer
        // to memory allocated by the Go runtime. Programs that use unsafe
        // can evade this restriction easily, so we don't try to catch them.
        // The cgo program will rewrite all possibly bad pointer arguments to
        // call cgoCheckPointer, where we can catch cases of a Go pointer
        // pointing to a Go pointer.

        // Complicating matters, taking the address of a slice or array
        // element permits the C program to access all elements of the slice
        // or array. In that case we will see a pointer to a single element,
        // but we need to check the entire data structure.

        // The cgoCheckPointer call takes additional arguments indicating that
        // it was called on an address expression. An additional argument of
        // true means that it only needs to check a single element. An
        // additional argument of a slice or array means that it needs to
        // check the entire slice/array, but nothing else. Otherwise, the
        // pointer could be anything, and we check the entire heap object,
        // which is conservative but safe.

        // When and if we implement a moving garbage collector,
        // cgoCheckPointer will pin the pointer for the duration of the cgo
        // call.  (This is necessary but not sufficient; the cgo program will
        // also have to change to pin Go pointers that cannot point to Go
        // pointers.)

        // cgoCheckPointer checks if the argument contains a Go pointer that
        // points to a Go pointer, and panics if it does.
        private static void cgoCheckPointer(object ptr, object arg)
        {
            if (debug.cgocheck == 0L)
            {
                return ;
            }

            var ep = efaceOf(_addr_ptr);
            var t = ep._type;

            var top = true;
            if (arg != null && (t.kind & kindMask == kindPtr || t.kind & kindMask == kindUnsafePointer))
            {
                var p = ep.data;
                if (t.kind & kindDirectIface == 0L)
                {
                    p = new ptr<ptr<ptr<unsafe.Pointer>>>(p);
                }

                if (p == null || !cgoIsGoPointer(p))
                {
                    return ;
                }

                var aep = efaceOf(_addr_arg);

                if (aep._type.kind & kindMask == kindBool) 
                    if (t.kind & kindMask == kindUnsafePointer)
                    { 
                        // We don't know the type of the element.
                        break;

                    }

                    var pt = (ptrtype.val)(@unsafe.Pointer(t));
                    cgoCheckArg(_addr_pt.elem, p, true, false, cgoCheckPointerFail);
                    return ;
                else if (aep._type.kind & kindMask == kindSlice) 
                    // Check the slice rather than the pointer.
                    ep = aep;
                    t = ep._type;
                else if (aep._type.kind & kindMask == kindArray) 
                    // Check the array rather than the pointer.
                    // Pass top as false since we have a pointer
                    // to the array.
                    ep = aep;
                    t = ep._type;
                    top = false;
                else 
                    throw("can't happen");
                
            }

            cgoCheckArg(_addr_t, ep.data, t.kind & kindDirectIface == 0L, top, cgoCheckPointerFail);

        }

        private static readonly @string cgoCheckPointerFail = (@string)"cgo argument has Go pointer to Go pointer";

        private static readonly @string cgoResultFail = (@string)"cgo result has Go pointer";

        // cgoCheckArg is the real work of cgoCheckPointer. The argument p
        // is either a pointer to the value (of type t), or the value itself,
        // depending on indir. The top parameter is whether we are at the top
        // level, where Go pointers are allowed.


        // cgoCheckArg is the real work of cgoCheckPointer. The argument p
        // is either a pointer to the value (of type t), or the value itself,
        // depending on indir. The top parameter is whether we are at the top
        // level, where Go pointers are allowed.
        private static void cgoCheckArg(ptr<_type> _addr_t, unsafe.Pointer p, bool indir, bool top, @string msg) => func((_, panic, __) =>
        {
            ref _type t = ref _addr_t.val;

            if (t.ptrdata == 0L || p == null)
            { 
                // If the type has no pointers there is nothing to do.
                return ;

            }


            if (t.kind & kindMask == kindArray) 
                var at = (arraytype.val)(@unsafe.Pointer(t));
                if (!indir)
                {
                    if (at.len != 1L)
                    {
                        throw("can't happen");
                    }

                    cgoCheckArg(_addr_at.elem, p, at.elem.kind & kindDirectIface == 0L, top, msg);
                    return ;

                }

                {
                    var i__prev1 = i;

                    for (var i = uintptr(0L); i < at.len; i++)
                    {
                        cgoCheckArg(_addr_at.elem, p, true, top, msg);
                        p = add(p, at.elem.size);
                    }


                    i = i__prev1;
                }
            else if (t.kind & kindMask == kindChan || t.kind & kindMask == kindMap) 
                // These types contain internal pointers that will
                // always be allocated in the Go heap. It's never OK
                // to pass them to C.
                panic(errorString(msg));
            else if (t.kind & kindMask == kindFunc) 
                if (indir)
                {
                    p = new ptr<ptr<ptr<unsafe.Pointer>>>(p);
                }

                if (!cgoIsGoPointer(p))
                {
                    return ;
                }

                panic(errorString(msg));
            else if (t.kind & kindMask == kindInterface) 
                ptr<ptr<ptr<_type>>> it = new ptr<ptr<ptr<ptr<_type>>>>(p);
                if (it == null)
                {
                    return ;
                } 
                // A type known at compile time is OK since it's
                // constant. A type not known at compile time will be
                // in the heap and will not be OK.
                if (inheap(uintptr(@unsafe.Pointer(it))))
                {
                    panic(errorString(msg));
                }

                p = new ptr<ptr<ptr<unsafe.Pointer>>>(add(p, sys.PtrSize));
                if (!cgoIsGoPointer(p))
                {
                    return ;
                }

                if (!top)
                {
                    panic(errorString(msg));
                }

                cgoCheckArg(it, p, it.kind & kindDirectIface == 0L, false, msg);
            else if (t.kind & kindMask == kindSlice) 
                var st = (slicetype.val)(@unsafe.Pointer(t));
                var s = (slice.val)(p);
                p = s.array;
                if (p == null || !cgoIsGoPointer(p))
                {
                    return ;
                }

                if (!top)
                {
                    panic(errorString(msg));
                }

                if (st.elem.ptrdata == 0L)
                {
                    return ;
                }

                {
                    var i__prev1 = i;

                    for (i = 0L; i < s.cap; i++)
                    {
                        cgoCheckArg(_addr_st.elem, p, true, false, msg);
                        p = add(p, st.elem.size);
                    }


                    i = i__prev1;
                }
            else if (t.kind & kindMask == kindString) 
                var ss = (stringStruct.val)(p);
                if (!cgoIsGoPointer(ss.str))
                {
                    return ;
                }

                if (!top)
                {
                    panic(errorString(msg));
                }

            else if (t.kind & kindMask == kindStruct) 
                st = (structtype.val)(@unsafe.Pointer(t));
                if (!indir)
                {
                    if (len(st.fields) != 1L)
                    {
                        throw("can't happen");
                    }

                    cgoCheckArg(_addr_st.fields[0L].typ, p, st.fields[0L].typ.kind & kindDirectIface == 0L, top, msg);
                    return ;

                }

                foreach (var (_, f) in st.fields)
                {
                    if (f.typ.ptrdata == 0L)
                    {
                        continue;
                    }

                    cgoCheckArg(_addr_f.typ, add(p, f.offset()), true, top, msg);

                }
            else if (t.kind & kindMask == kindPtr || t.kind & kindMask == kindUnsafePointer) 
                if (indir)
                {
                    p = new ptr<ptr<ptr<unsafe.Pointer>>>(p);
                    if (p == null)
                    {
                        return ;
                    }

                }

                if (!cgoIsGoPointer(p))
                {
                    return ;
                }

                if (!top)
                {
                    panic(errorString(msg));
                }

                cgoCheckUnknownPointer(p, msg);
            else 
                throw("can't happen");
            
        });

        // cgoCheckUnknownPointer is called for an arbitrary pointer into Go
        // memory. It checks whether that Go memory contains any other
        // pointer into Go memory. If it does, we panic.
        // The return values are unused but useful to see in panic tracebacks.
        private static (System.UIntPtr, System.UIntPtr) cgoCheckUnknownPointer(unsafe.Pointer p, @string msg) => func((_, panic, __) =>
        {
            System.UIntPtr @base = default;
            System.UIntPtr i = default;

            if (inheap(uintptr(p)))
            {
                var (b, span, _) = findObject(uintptr(p), 0L, 0L);
                base = b;
                if (base == 0L)
                {
                    return ;
                }

                var hbits = heapBitsForAddr(base);
                var n = span.elemsize;
                i = uintptr(0L);

                while (i < n)
                {
                    if (i != 1L * sys.PtrSize && !hbits.morePointers())
                    { 
                        // No more possible pointers.
                        break;
                    i += sys.PtrSize;
                    }

                    if (hbits.isPointer() && cgoIsGoPointer(new ptr<ptr<ptr<unsafe.Pointer>>>(@unsafe.Pointer(base + i))))
                    {
                        panic(errorString(msg));
                    }

                    hbits = hbits.next();

                }


                return ;

            }

            foreach (var (_, datap) in activeModules())
            {
                if (cgoInRange(p, datap.data, datap.edata) || cgoInRange(p, datap.bss, datap.ebss))
                { 
                    // We have no way to know the size of the object.
                    // We have to assume that it might contain a pointer.
                    panic(errorString(msg));

                } 
                // In the text or noptr sections, we know that the
                // pointer does not point to a Go pointer.
            }
            return ;

        });

        // cgoIsGoPointer reports whether the pointer is a Go pointer--a
        // pointer to Go memory. We only care about Go memory that might
        // contain pointers.
        //go:nosplit
        //go:nowritebarrierrec
        private static bool cgoIsGoPointer(unsafe.Pointer p)
        {
            if (p == null)
            {
                return false;
            }

            if (inHeapOrStack(uintptr(p)))
            {
                return true;
            }

            foreach (var (_, datap) in activeModules())
            {
                if (cgoInRange(p, datap.data, datap.edata) || cgoInRange(p, datap.bss, datap.ebss))
                {
                    return true;
                }

            }
            return false;

        }

        // cgoInRange reports whether p is between start and end.
        //go:nosplit
        //go:nowritebarrierrec
        private static bool cgoInRange(unsafe.Pointer p, System.UIntPtr start, System.UIntPtr end)
        {
            return start <= uintptr(p) && uintptr(p) < end;
        }

        // cgoCheckResult is called to check the result parameter of an
        // exported Go function. It panics if the result is or contains a Go
        // pointer.
        private static void cgoCheckResult(object val)
        {
            if (debug.cgocheck == 0L)
            {
                return ;
            }

            var ep = efaceOf(_addr_val);
            var t = ep._type;
            cgoCheckArg(_addr_t, ep.data, t.kind & kindDirectIface == 0L, false, cgoResultFail);

        }
    }
}
