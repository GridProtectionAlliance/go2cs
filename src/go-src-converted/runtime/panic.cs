// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\panic.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        // Calling panic with one of the errors below will call errorString.Error
        // which will call mallocgc to concatenate strings. That will fail if
        // malloc is locked, causing a confusing error message. Throw a better
        // error message instead.
        private static void panicCheckMalloc(error err)
        {
            var gp = getg();
            if (gp != null && gp.m != null && gp.m.mallocing != 0L)
            {
                throw(string(err._<errorString>()));
            }
        }

        private static var indexError = error(errorString("index out of range"));

        private static void panicindex() => func((_, panic, __) =>
        {
            panicCheckMalloc(indexError);
            panic(indexError);
        });

        private static var sliceError = error(errorString("slice bounds out of range"));

        private static void panicslice() => func((_, panic, __) =>
        {
            panicCheckMalloc(sliceError);
            panic(sliceError);
        });

        private static var divideError = error(errorString("integer divide by zero"));

        private static void panicdivide() => func((_, panic, __) =>
        {
            panicCheckMalloc(divideError);
            panic(divideError);
        });

        private static var overflowError = error(errorString("integer overflow"));

        private static void panicoverflow() => func((_, panic, __) =>
        {
            panicCheckMalloc(overflowError);
            panic(overflowError);
        });

        private static var floatError = error(errorString("floating point error"));

        private static void panicfloat() => func((_, panic, __) =>
        {
            panicCheckMalloc(floatError);
            panic(floatError);
        });

        private static var memoryError = error(errorString("invalid memory address or nil pointer dereference"));

        private static void panicmem() => func((_, panic, __) =>
        {
            panicCheckMalloc(memoryError);
            panic(memoryError);
        });

        private static void throwinit()
        {
            throw("recursive call during initialization - linker skew");
        }

        // Create a new deferred function fn with siz bytes of arguments.
        // The compiler turns a defer statement into a call to this.
        //go:nosplit
        private static void deferproc(int siz, ref funcval fn)
        { // arguments of fn follow fn
            if (getg().m.curg != getg())
            { 
                // go code on the system stack can't defer
                throw("defer on system stack");
            } 

            // the arguments of fn are in a perilous state. The stack map
            // for deferproc does not describe them. So we can't let garbage
            // collection or stack copying trigger until we've copied them out
            // to somewhere safe. The memmove below does that.
            // Until the copy completes, we can only call nosplit routines.
            var sp = getcallersp(@unsafe.Pointer(ref siz));
            var argp = uintptr(@unsafe.Pointer(ref fn)) + @unsafe.Sizeof(fn);
            var callerpc = getcallerpc();

            var d = newdefer(siz);
            if (d._panic != null)
            {
                throw("deferproc: d.panic != nil after newdefer");
            }
            d.fn = fn;
            d.pc = callerpc;
            d.sp = sp;

            if (siz == 0L)             else if (siz == sys.PtrSize) 
                (uintptr.Value)(deferArgs(d)).Value;

                @unsafe.Pointer(argp).Value;
            else 
                memmove(deferArgs(d), @unsafe.Pointer(argp), uintptr(siz));
            // deferproc returns 0 normally.
            // a deferred func that stops a panic
            // makes the deferproc return 1.
            // the code the compiler generates always
            // checks the return value and jumps to the
            // end of the function if deferproc returns != 0.
            return0(); 
            // No code can go here - the C return register has
            // been set and must not be clobbered.
        }

        // Small malloc size classes >= 16 are the multiples of 16: 16, 32, 48, 64, 80, 96, 112, 128, 144, ...
        // Each P holds a pool for defers with small arg sizes.
        // Assign defer allocations to pools by rounding to 16, to match malloc size classes.

        private static readonly var deferHeaderSize = @unsafe.Sizeof(new _defer());
        private static readonly var minDeferAlloc = (deferHeaderSize + 15L) & ~15L;
        private static readonly var minDeferArgs = minDeferAlloc - deferHeaderSize;

        // defer size class for arg size sz
        //go:nosplit
        private static System.UIntPtr deferclass(System.UIntPtr siz)
        {
            if (siz <= minDeferArgs)
            {
                return 0L;
            }
            return (siz - minDeferArgs + 15L) / 16L;
        }

        // total size of memory block for defer with arg size sz
        private static System.UIntPtr totaldefersize(System.UIntPtr siz)
        {
            if (siz <= minDeferArgs)
            {
                return minDeferAlloc;
            }
            return deferHeaderSize + siz;
        }

        // Ensure that defer arg sizes that map to the same defer size class
        // also map to the same malloc size class.
        private static void testdefersizes()
        {
            array<int> m = new array<int>(len(new p().deferpool));

            {
                var i__prev1 = i;

                foreach (var (__i) in m)
                {
                    i = __i;
                    m[i] = -1L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (var i = uintptr(0L); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
                {
                    var defersc = deferclass(i);
                    if (defersc >= uintptr(len(m)))
                    {
                        break;
                    }
                    var siz = roundupsize(totaldefersize(i));
                    if (m[defersc] < 0L)
                    {
                        m[defersc] = int32(siz);
                        continue;
                    }
                    if (m[defersc] != int32(siz))
                    {
                        print("bad defer size class: i=", i, " siz=", siz, " defersc=", defersc, "\n");
                        throw("bad defer size class");
                    }
                }


                i = i__prev1;
            }
        }

        // The arguments associated with a deferred call are stored
        // immediately after the _defer header in memory.
        //go:nosplit
        private static unsafe.Pointer deferArgs(ref _defer d)
        {
            if (d.siz == 0L)
            { 
                // Avoid pointer past the defer allocation.
                return null;
            }
            return add(@unsafe.Pointer(d), @unsafe.Sizeof(d.Value));
        }

        private static ref _type deferType = default; // type of _defer struct

        private static void init()
        {
            var x = default;
            x = (_defer.Value)(null);
            deferType = (new ptr<*(ptr<ptr<ptrtype>>)>(@unsafe.Pointer(ref x))).elem;
        }

        // Allocate a Defer, usually using per-P pool.
        // Each defer must be released with freedefer.
        //
        // This must not grow the stack because there may be a frame without
        // stack map information when this is called.
        //
        //go:nosplit
        private static ref _defer newdefer(int siz)
        {
            ref _defer d = default;
            var sc = deferclass(uintptr(siz));
            var gp = getg();
            if (sc < uintptr(len(new p().deferpool)))
            {
                var pp = gp.m.p.ptr();
                if (len(pp.deferpool[sc]) == 0L && sched.deferpool[sc] != null)
                { 
                    // Take the slow path on the system stack so
                    // we don't grow newdefer's stack.
                    systemstack(() =>
                    {
                        lock(ref sched.deferlock);
                        while (len(pp.deferpool[sc]) < cap(pp.deferpool[sc]) / 2L && sched.deferpool[sc] != null)
                        {
                            d = sched.deferpool[sc];
                            sched.deferpool[sc] = d.link;
                            d.link = null;
                            pp.deferpool[sc] = append(pp.deferpool[sc], d);
                        }

                        unlock(ref sched.deferlock);
                    });
                }
                {
                    var n = len(pp.deferpool[sc]);

                    if (n > 0L)
                    {
                        d = pp.deferpool[sc][n - 1L];
                        pp.deferpool[sc][n - 1L] = null;
                        pp.deferpool[sc] = pp.deferpool[sc][..n - 1L];
                    }

                }
            }
            if (d == null)
            { 
                // Allocate new defer+args.
                systemstack(() =>
                {
                    var total = roundupsize(totaldefersize(uintptr(siz)));
                    d = (_defer.Value)(mallocgc(total, deferType, true));
                });
            }
            d.siz = siz;
            d.link = gp._defer;
            gp._defer = d;
            return d;
        }

        // Free the given defer.
        // The defer cannot be used after this call.
        //
        // This must not grow the stack because there may be a frame without a
        // stack map when this is called.
        //
        //go:nosplit
        private static void freedefer(ref _defer d)
        {
            if (d._panic != null)
            {
                freedeferpanic();
            }
            if (d.fn != null)
            {
                freedeferfn();
            }
            var sc = deferclass(uintptr(d.siz));
            if (sc >= uintptr(len(new p().deferpool)))
            {
                return;
            }
            var pp = getg().m.p.ptr();
            if (len(pp.deferpool[sc]) == cap(pp.deferpool[sc]))
            { 
                // Transfer half of local cache to the central cache.
                //
                // Take this slow path on the system stack so
                // we don't grow freedefer's stack.
                systemstack(() =>
                {
                    ref _defer first = default;                    ref _defer last = default;

                    while (len(pp.deferpool[sc]) > cap(pp.deferpool[sc]) / 2L)
                    {
                        var n = len(pp.deferpool[sc]);
                        var d = pp.deferpool[sc][n - 1L];
                        pp.deferpool[sc][n - 1L] = null;
                        pp.deferpool[sc] = pp.deferpool[sc][..n - 1L];
                        if (first == null)
                        {
                            first = d;
                        }
                        else
                        {
                            last.link = d;
                        }
                        last = d;
                    }

                    lock(ref sched.deferlock);
                    last.link = sched.deferpool[sc];
                    sched.deferpool[sc] = first;
                    unlock(ref sched.deferlock);
                });
            } 

            // These lines used to be simply `*d = _defer{}` but that
            // started causing a nosplit stack overflow via typedmemmove.
            d.siz = 0L;
            d.started = false;
            d.sp = 0L;
            d.pc = 0L;
            d.fn = null;
            d._panic = null;
            d.link = null;

            pp.deferpool[sc] = append(pp.deferpool[sc], d);
        }

        // Separate function so that it can split stack.
        // Windows otherwise runs out of stack space.
        private static void freedeferpanic()
        { 
            // _panic must be cleared before d is unlinked from gp.
            throw("freedefer with d._panic != nil");
        }

        private static void freedeferfn()
        { 
            // fn must be cleared before d is unlinked from gp.
            throw("freedefer with d.fn != nil");
        }

        // Run a deferred function if there is one.
        // The compiler inserts a call to this at the end of any
        // function which calls defer.
        // If there is a deferred function, this will call runtime·jmpdefer,
        // which will jump to the deferred function such that it appears
        // to have been called by the caller of deferreturn at the point
        // just before deferreturn was called. The effect is that deferreturn
        // is called again and again until there are no more deferred functions.
        // Cannot split the stack because we reuse the caller's frame to
        // call the deferred function.

        // The single argument isn't actually used - it just has its address
        // taken so it can be matched against pending defers.
        //go:nosplit
        private static void deferreturn(System.UIntPtr arg0)
        {
            var gp = getg();
            var d = gp._defer;
            if (d == null)
            {
                return;
            }
            var sp = getcallersp(@unsafe.Pointer(ref arg0));
            if (d.sp != sp)
            {
                return;
            } 

            // Moving arguments around.
            //
            // Everything called after this point must be recursively
            // nosplit because the garbage collector won't know the form
            // of the arguments until the jmpdefer can flip the PC over to
            // fn.

            if (d.siz == 0L)             else if (d.siz == sys.PtrSize) 
                (uintptr.Value)(@unsafe.Pointer(ref arg0)).Value;

                deferArgs(d).Value;
            else 
                memmove(@unsafe.Pointer(ref arg0), deferArgs(d), uintptr(d.siz));
                        var fn = d.fn;
            d.fn = null;
            gp._defer = d.link;
            freedefer(d);
            jmpdefer(fn, uintptr(@unsafe.Pointer(ref arg0)));
        }

        // Goexit terminates the goroutine that calls it. No other goroutine is affected.
        // Goexit runs all deferred calls before terminating the goroutine. Because Goexit
        // is not a panic, any recover calls in those deferred functions will return nil.
        //
        // Calling Goexit from the main goroutine terminates that goroutine
        // without func main returning. Since func main has not returned,
        // the program continues execution of other goroutines.
        // If all other goroutines exit, the program crashes.
        public static void Goexit()
        { 
            // Run all deferred functions for the current goroutine.
            // This code is similar to gopanic, see that implementation
            // for detailed comments.
            var gp = getg();
            while (true)
            {
                var d = gp._defer;
                if (d == null)
                {
                    break;
                }
                if (d.started)
                {
                    if (d._panic != null)
                    {
                        d._panic.aborted = true;
                        d._panic = null;
                    }
                    d.fn = null;
                    gp._defer = d.link;
                    freedefer(d);
                    continue;
                }
                d.started = true;
                reflectcall(null, @unsafe.Pointer(d.fn), deferArgs(d), uint32(d.siz), uint32(d.siz));
                if (gp._defer != d)
                {
                    throw("bad defer entry in Goexit");
                }
                d._panic = null;
                d.fn = null;
                gp._defer = d.link;
                freedefer(d); 
                // Note: we ignore recovers here because Goexit isn't a panic
            }

            goexit1();
        }

        // Call all Error and String methods before freezing the world.
        // Used when crashing with panicking.
        private static void preprintpanics(ref _panic _p) => func(_p, (ref _panic p, Defer defer, Panic _, Recover __) =>
        {
            defer(() =>
            {
                if (recover() != null)
                {
                    throw("panic while printing panic value");
                }
            }());
            while (p != null)
            {
                switch (p.arg.type())
                {
                    case error v:
                        p.arg = v.Error();
                        break;
                    case stringer v:
                        p.arg = v.String();
                        break;
                }
                p = p.link;
            }

        });

        // Print all currently active panics. Used when crashing.
        // Should only be called after preprintpanics.
        private static void printpanics(ref _panic p)
        {
            if (p.link != null)
            {
                printpanics(p.link);
                print("\t");
            }
            print("panic: ");
            printany(p.arg);
            if (p.recovered)
            {
                print(" [recovered]");
            }
            print("\n");
        }

        // The implementation of the predeclared function panic.
        private static void gopanic(object e)
        {
            var gp = getg();
            if (gp.m.curg != gp)
            {
                print("panic: ");
                printany(e);
                print("\n");
                throw("panic on system stack");
            } 

            // m.softfloat is set during software floating point.
            // It increments m.locks to avoid preemption.
            // We moved the memory loads out, so there shouldn't be
            // any reason for it to panic anymore.
            if (gp.m.softfloat != 0L)
            {
                gp.m.locks--;
                gp.m.softfloat = 0L;
                throw("panic during softfloat");
            }
            if (gp.m.mallocing != 0L)
            {
                print("panic: ");
                printany(e);
                print("\n");
                throw("panic during malloc");
            }
            if (gp.m.preemptoff != "")
            {
                print("panic: ");
                printany(e);
                print("\n");
                print("preempt off reason: ");
                print(gp.m.preemptoff);
                print("\n");
                throw("panic during preemptoff");
            }
            if (gp.m.locks != 0L)
            {
                print("panic: ");
                printany(e);
                print("\n");
                throw("panic holding locks");
            }
            _panic p = default;
            p.arg = e;
            p.link = gp._panic;
            gp._panic = (_panic.Value)(noescape(@unsafe.Pointer(ref p)));

            atomic.Xadd(ref runningPanicDefers, 1L);

            while (true)
            {
                var d = gp._defer;
                if (d == null)
                {
                    break;
                } 

                // If defer was started by earlier panic or Goexit (and, since we're back here, that triggered a new panic),
                // take defer off list. The earlier panic or Goexit will not continue running.
                if (d.started)
                {
                    if (d._panic != null)
                    {
                        d._panic.aborted = true;
                    }
                    d._panic = null;
                    d.fn = null;
                    gp._defer = d.link;
                    freedefer(d);
                    continue;
                } 

                // Mark defer as started, but keep on list, so that traceback
                // can find and update the defer's argument frame if stack growth
                // or a garbage collection happens before reflectcall starts executing d.fn.
                d.started = true; 

                // Record the panic that is running the defer.
                // If there is a new panic during the deferred call, that panic
                // will find d in the list and will mark d._panic (this panic) aborted.
                d._panic = (_panic.Value)(noescape(@unsafe.Pointer(ref p)));

                p.argp = @unsafe.Pointer(getargp(0L));
                reflectcall(null, @unsafe.Pointer(d.fn), deferArgs(d), uint32(d.siz), uint32(d.siz));
                p.argp = null; 

                // reflectcall did not panic. Remove d.
                if (gp._defer != d)
                {
                    throw("bad defer entry in panic");
                }
                d._panic = null;
                d.fn = null;
                gp._defer = d.link; 

                // trigger shrinkage to test stack copy. See stack_test.go:TestStackPanic
                //GC()

                var pc = d.pc;
                var sp = @unsafe.Pointer(d.sp); // must be pointer so it gets adjusted during stack copy
                freedefer(d);
                if (p.recovered)
                {
                    atomic.Xadd(ref runningPanicDefers, -1L);

                    gp._panic = p.link; 
                    // Aborted panics are marked but remain on the g.panic list.
                    // Remove them from the list.
                    while (gp._panic != null && gp._panic.aborted)
                    {
                        gp._panic = gp._panic.link;
                    }

                    if (gp._panic == null)
                    { // must be done with signal
                        gp.sig = 0L;
                    } 
                    // Pass information about recovering frame to recovery.
                    gp.sigcode0 = uintptr(sp);
                    gp.sigcode1 = pc;
                    mcall(recovery);
                    throw("recovery failed"); // mcall should not return
                }
            } 

            // ran out of deferred calls - old-school panic now
            // Because it is unsafe to call arbitrary user code after freezing
            // the world, we call preprintpanics to invoke all necessary Error
            // and String methods to prepare the panic strings before startpanic.
 

            // ran out of deferred calls - old-school panic now
            // Because it is unsafe to call arbitrary user code after freezing
            // the world, we call preprintpanics to invoke all necessary Error
            // and String methods to prepare the panic strings before startpanic.
            preprintpanics(gp._panic);
            startpanic(); 

            // startpanic set panicking, which will block main from exiting,
            // so now OK to decrement runningPanicDefers.
            atomic.Xadd(ref runningPanicDefers, -1L);

            printpanics(gp._panic);
            dopanic(0L) * (int.Value)(null);

            0L; // not reached
        }

        // getargp returns the location where the caller
        // writes outgoing function call arguments.
        //go:nosplit
        //go:noinline
        private static System.UIntPtr getargp(long x)
        { 
            // x is an argument mainly so that we can return its address.
            return uintptr(noescape(@unsafe.Pointer(ref x)));
        }

        // The implementation of the predeclared function recover.
        // Cannot split the stack because it needs to reliably
        // find the stack segment of its caller.
        //
        // TODO(rsc): Once we commit to CopyStackAlways,
        // this doesn't need to be nosplit.
        //go:nosplit
        private static void gorecover(System.UIntPtr argp)
        { 
            // Must be in a function running as part of a deferred call during the panic.
            // Must be called from the topmost function of the call
            // (the function used in the defer statement).
            // p.argp is the argument pointer of that topmost deferred function call.
            // Compare against argp reported by caller.
            // If they match, the caller is the one who can recover.
            var gp = getg();
            var p = gp._panic;
            if (p != null && !p.recovered && argp == uintptr(p.argp))
            {
                p.recovered = true;
                return p.arg;
            }
            return null;
        }

        //go:nosplit
        private static void startpanic()
        {
            systemstack(startpanic_m);
        }

        //go:nosplit
        private static void dopanic(long unused)
        {
            var pc = getcallerpc();
            var sp = getcallersp(@unsafe.Pointer(ref unused));
            var gp = getg();
            systemstack(() =>
            {
                dopanic_m(gp, pc, sp); // should never return
            }) * (int.Value)(null);

            0L;
        }

        //go:linkname sync_throw sync.throw
        private static void sync_throw(@string s)
        {
            throw(s);
        }

        //go:nosplit
        private static void @throw(@string s)
        {
            print("fatal error: ", s, "\n");
            var gp = getg();
            if (gp.m.throwing == 0L)
            {
                gp.m.throwing = 1L;
            }
            startpanic();
            dopanic(0L) * (int.Value)(null);

            0L; // not reached
        }

        // runningPanicDefers is non-zero while running deferred functions for panic.
        // runningPanicDefers is incremented and decremented atomically.
        // This is used to try hard to get a panic stack trace out when exiting.
        private static uint runningPanicDefers = default;

        // panicking is non-zero when crashing the program for an unrecovered panic.
        // panicking is incremented and decremented atomically.
        private static uint panicking = default;

        // paniclk is held while printing the panic information and stack trace,
        // so that two concurrent panics don't overlap their output.
        private static mutex paniclk = default;

        // Unwind the stack after a deferred function calls recover
        // after a panic. Then arrange to continue running as though
        // the caller of the deferred function returned normally.
        private static void recovery(ref g gp)
        { 
            // Info about defer passed in G struct.
            var sp = gp.sigcode0;
            var pc = gp.sigcode1; 

            // d's arguments need to be in the stack.
            if (sp != 0L && (sp < gp.stack.lo || gp.stack.hi < sp))
            {
                print("recover: ", hex(sp), " not in [", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n");
                throw("bad recovery");
            } 

            // Make the deferproc for this d return again,
            // this time returning 1.  The calling function will
            // jump to the standard return epilogue.
            gp.sched.sp = sp;
            gp.sched.pc = pc;
            gp.sched.lr = 0L;
            gp.sched.ret = 1L;
            gogo(ref gp.sched);
        }

        // startpanic_m prepares for an unrecoverable panic.
        //
        // It can have write barriers because the write barrier explicitly
        // ignores writes once dying > 0.
        //
        //go:yeswritebarrierrec
        private static void startpanic_m()
        {
            var _g_ = getg();
            if (mheap_.cachealloc.size == 0L)
            { // very early
                print("runtime: panic before malloc heap initialized\n");
            } 
            // Disallow malloc during an unrecoverable panic. A panic
            // could happen in a signal handler, or in a throw, or inside
            // malloc itself. We want to catch if an allocation ever does
            // happen (even if we're not in one of these situations).
            _g_.m.mallocing++;


            if (_g_.m.dying == 0L)
            {
                _g_.m.dying = 1L;
                _g_.writebuf = null;
                atomic.Xadd(ref panicking, 1L);
                lock(ref paniclk);
                if (debug.schedtrace > 0L || debug.scheddetail > 0L)
                {
                    schedtrace(true);
                }
                freezetheworld();
                return;
                goto __switch_break0;
            }
            if (_g_.m.dying == 1L) 
            {
                // Something failed while panicking, probably the print of the
                // argument to panic().  Just print a stack trace and exit.
                _g_.m.dying = 2L;
                print("panic during panic\n");
                dopanic(0L);
                exit(3L);
                fallthrough = true;
            }
            if (fallthrough || _g_.m.dying == 2L) 
            {
                // This is a genuine bug in the runtime, we couldn't even
                // print the stack trace successfully.
                _g_.m.dying = 3L;
                print("stack trace unavailable\n");
                exit(4L);
            }
            // default: 
                // Can't even print! Just exit.
                exit(5L);

            __switch_break0:;
        }

        private static bool didothers = default;
        private static mutex deadlock = default;

        private static void dopanic_m(ref g gp, System.UIntPtr pc, System.UIntPtr sp)
        {
            if (gp.sig != 0L)
            {
                var signame = signame(gp.sig);
                if (signame != "")
                {
                    print("[signal ", signame);
                }
                else
                {
                    print("[signal ", hex(gp.sig));
                }
                print(" code=", hex(gp.sigcode0), " addr=", hex(gp.sigcode1), " pc=", hex(gp.sigpc), "]\n");
            }
            var (level, all, docrash) = gotraceback();
            var _g_ = getg();
            if (level > 0L)
            {
                if (gp != gp.m.curg)
                {
                    all = true;
                }
                if (gp != gp.m.g0)
                {
                    print("\n");
                    goroutineheader(gp);
                    traceback(pc, sp, 0L, gp);
                }
                else if (level >= 2L || _g_.m.throwing > 0L)
                {
                    print("\nruntime stack:\n");
                    traceback(pc, sp, 0L, gp);
                }
                if (!didothers && all)
                {
                    didothers = true;
                    tracebackothers(gp);
                }
            }
            unlock(ref paniclk);

            if (atomic.Xadd(ref panicking, -1L) != 0L)
            { 
                // Some other m is panicking too.
                // Let it print what it needs to print.
                // Wait forever without chewing up cpu.
                // It will exit when it's done.
                lock(ref deadlock);
                lock(ref deadlock);
            }
            if (docrash)
            {
                crash();
            }
            exit(2L);
        }

        // canpanic returns false if a signal should throw instead of
        // panicking.
        //
        //go:nosplit
        private static bool canpanic(ref g gp)
        { 
            // Note that g is m->gsignal, different from gp.
            // Note also that g->m can change at preemption, so m can go stale
            // if this function ever makes a function call.
            var _g_ = getg();
            var _m_ = _g_.m; 

            // Is it okay for gp to panic instead of crashing the program?
            // Yes, as long as it is running Go code, not runtime code,
            // and not stuck in a system call.
            if (gp == null || gp != _m_.curg)
            {
                return false;
            }
            if (_m_.locks - _m_.softfloat != 0L || _m_.mallocing != 0L || _m_.throwing != 0L || _m_.preemptoff != "" || _m_.dying != 0L)
            {
                return false;
            }
            var status = readgstatus(gp);
            if (status & ~_Gscan != _Grunning || gp.syscallsp != 0L)
            {
                return false;
            }
            if (GOOS == "windows" && _m_.libcallsp != 0L)
            {
                return false;
            }
            return true;
        }
    }
}
