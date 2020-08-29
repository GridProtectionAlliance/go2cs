// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build race

// package runtime -- go2cs converted at 2020 August 29 08:19:43 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\race.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Public race detection API, present iff build with -race.
        public static void RaceRead(unsafe.Pointer addr)
;
        public static void RaceWrite(unsafe.Pointer addr)
;
        public static void RaceReadRange(unsafe.Pointer addr, long len)
;
        public static void RaceWriteRange(unsafe.Pointer addr, long len)
;

        public static long RaceErrors()
        {
            ulong n = default;
            racecall(ref __tsan_report_count, uintptr(@unsafe.Pointer(ref n)), 0L, 0L, 0L);
            return int(n);
        }

        //go:nosplit

        // RaceAcquire/RaceRelease/RaceReleaseMerge establish happens-before relations
        // between goroutines. These inform the race detector about actual synchronization
        // that it can't see for some reason (e.g. synchronization within RaceDisable/RaceEnable
        // sections of code).
        // RaceAcquire establishes a happens-before relation with the preceding
        // RaceReleaseMerge on addr up to and including the last RaceRelease on addr.
        // In terms of the C memory model (C11 §5.1.2.4, §7.17.3),
        // RaceAcquire is equivalent to atomic_load(memory_order_acquire).
        public static void RaceAcquire(unsafe.Pointer addr)
        {
            raceacquire(addr);
        }

        //go:nosplit

        // RaceRelease performs a release operation on addr that
        // can synchronize with a later RaceAcquire on addr.
        //
        // In terms of the C memory model, RaceRelease is equivalent to
        // atomic_store(memory_order_release).
        public static void RaceRelease(unsafe.Pointer addr)
        {
            racerelease(addr);
        }

        //go:nosplit

        // RaceReleaseMerge is like RaceRelease, but also establishes a happens-before
        // relation with the preceding RaceRelease or RaceReleaseMerge on addr.
        //
        // In terms of the C memory model, RaceReleaseMerge is equivalent to
        // atomic_exchange(memory_order_release).
        public static void RaceReleaseMerge(unsafe.Pointer addr)
        {
            racereleasemerge(addr);
        }

        //go:nosplit

        // RaceDisable disables handling of race synchronization events in the current goroutine.
        // Handling is re-enabled with RaceEnable. RaceDisable/RaceEnable can be nested.
        // Non-synchronization events (memory accesses, function entry/exit) still affect
        // the race detector.
        public static void RaceDisable()
        {
            var _g_ = getg();
            if (_g_.raceignore == 0L)
            {>>MARKER:FUNCTION_RaceWriteRange_BLOCK_PREFIX<<
                racecall(ref __tsan_go_ignore_sync_begin, _g_.racectx, 0L, 0L, 0L);
            }
            _g_.raceignore++;
        }

        //go:nosplit

        // RaceEnable re-enables handling of race events in the current goroutine.
        public static void RaceEnable()
        {
            var _g_ = getg();
            _g_.raceignore--;
            if (_g_.raceignore == 0L)
            {>>MARKER:FUNCTION_RaceReadRange_BLOCK_PREFIX<<
                racecall(ref __tsan_go_ignore_sync_end, _g_.racectx, 0L, 0L, 0L);
            }
        }

        // Private interface for the runtime.

        private static readonly var raceenabled = true;

        // For all functions accepting callerpc and pc,
        // callerpc is a return PC of the function that calls this function,
        // pc is start PC of the function that calls this function.


        // For all functions accepting callerpc and pc,
        // callerpc is a return PC of the function that calls this function,
        // pc is start PC of the function that calls this function.
        private static void raceReadObjectPC(ref _type t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            var kind = t.kind & kindMask;
            if (kind == kindArray || kind == kindStruct)
            {>>MARKER:FUNCTION_RaceWrite_BLOCK_PREFIX<< 
                // for composite objects we have to read every address
                // because a write might happen to any subobject.
                racereadrangepc(addr, t.size, callerpc, pc);
            }
            else
            {>>MARKER:FUNCTION_RaceRead_BLOCK_PREFIX<< 
                // for non-composite objects we can read just the start
                // address, as any write must write the first byte.
                racereadpc(addr, callerpc, pc);
            }
        }

        private static void raceWriteObjectPC(ref _type t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc)
        {
            var kind = t.kind & kindMask;
            if (kind == kindArray || kind == kindStruct)
            { 
                // for composite objects we have to write every address
                // because a write might happen to any subobject.
                racewriterangepc(addr, t.size, callerpc, pc);
            }
            else
            { 
                // for non-composite objects we can write just the start
                // address, as any write must write the first byte.
                racewritepc(addr, callerpc, pc);
            }
        }

        //go:noescape
        private static void racereadpc(unsafe.Pointer addr, System.UIntPtr callpc, System.UIntPtr pc)
;

        //go:noescape
        private static void racewritepc(unsafe.Pointer addr, System.UIntPtr callpc, System.UIntPtr pc)
;

        private partial struct symbolizeCodeContext
        {
            public System.UIntPtr pc;
            public ptr<byte> fn;
            public ptr<byte> file;
            public System.UIntPtr line;
            public System.UIntPtr off;
            public System.UIntPtr res;
        }

        private static array<byte> qq = new array<byte>(new byte[] { '?', '?', 0 });
        private static array<byte> dash = new array<byte>(new byte[] { '-', 0 });

        private static readonly var raceGetProcCmd = iota;
        private static readonly var raceSymbolizeCodeCmd = 0;
        private static readonly var raceSymbolizeDataCmd = 1;

        // Callback from C into Go, runs on g0.
        private static void racecallback(System.UIntPtr cmd, unsafe.Pointer ctx)
        {

            if (cmd == raceGetProcCmd) 
                throw("should have been handled by racecallbackthunk");
            else if (cmd == raceSymbolizeCodeCmd) 
                raceSymbolizeCode((symbolizeCodeContext.Value)(ctx));
            else if (cmd == raceSymbolizeDataCmd) 
                raceSymbolizeData((symbolizeDataContext.Value)(ctx));
            else 
                throw("unknown command");
                    }

        private static void raceSymbolizeCode(ref symbolizeCodeContext ctx)
        {
            var f = FuncForPC(ctx.pc);
            if (f != null)
            {>>MARKER:FUNCTION_racewritepc_BLOCK_PREFIX<<
                var (file, line) = f.FileLine(ctx.pc);
                if (line != 0L)
                {>>MARKER:FUNCTION_racereadpc_BLOCK_PREFIX<<
                    ctx.fn = cfuncname(f.funcInfo());
                    ctx.line = uintptr(line);
                    ctx.file = ref bytes(file)[0L]; // assume NUL-terminated
                    ctx.off = ctx.pc - f.Entry();
                    ctx.res = 1L;
                    return;
                }
            }
            ctx.fn = ref qq[0L];
            ctx.file = ref dash[0L];
            ctx.line = 0L;
            ctx.off = ctx.pc;
            ctx.res = 1L;
        }

        private partial struct symbolizeDataContext
        {
            public System.UIntPtr addr;
            public System.UIntPtr heap;
            public System.UIntPtr start;
            public System.UIntPtr size;
            public ptr<byte> name;
            public ptr<byte> file;
            public System.UIntPtr line;
            public System.UIntPtr res;
        }

        private static void raceSymbolizeData(ref symbolizeDataContext ctx)
        {
            {
                var (_, x, n) = findObject(@unsafe.Pointer(ctx.addr));

                if (x != null)
                {
                    ctx.heap = 1L;
                    ctx.start = uintptr(x);
                    ctx.size = n;
                    ctx.res = 1L;
                }

            }
        }

        // Race runtime functions called via runtime·racecall.
        //go:linkname __tsan_init __tsan_init
        private static byte __tsan_init = default;

        //go:linkname __tsan_fini __tsan_fini
        private static byte __tsan_fini = default;

        //go:linkname __tsan_proc_create __tsan_proc_create
        private static byte __tsan_proc_create = default;

        //go:linkname __tsan_proc_destroy __tsan_proc_destroy
        private static byte __tsan_proc_destroy = default;

        //go:linkname __tsan_map_shadow __tsan_map_shadow
        private static byte __tsan_map_shadow = default;

        //go:linkname __tsan_finalizer_goroutine __tsan_finalizer_goroutine
        private static byte __tsan_finalizer_goroutine = default;

        //go:linkname __tsan_go_start __tsan_go_start
        private static byte __tsan_go_start = default;

        //go:linkname __tsan_go_end __tsan_go_end
        private static byte __tsan_go_end = default;

        //go:linkname __tsan_malloc __tsan_malloc
        private static byte __tsan_malloc = default;

        //go:linkname __tsan_free __tsan_free
        private static byte __tsan_free = default;

        //go:linkname __tsan_acquire __tsan_acquire
        private static byte __tsan_acquire = default;

        //go:linkname __tsan_release __tsan_release
        private static byte __tsan_release = default;

        //go:linkname __tsan_release_merge __tsan_release_merge
        private static byte __tsan_release_merge = default;

        //go:linkname __tsan_go_ignore_sync_begin __tsan_go_ignore_sync_begin
        private static byte __tsan_go_ignore_sync_begin = default;

        //go:linkname __tsan_go_ignore_sync_end __tsan_go_ignore_sync_end
        private static byte __tsan_go_ignore_sync_end = default;

        //go:linkname __tsan_report_count __tsan_report_count
        private static byte __tsan_report_count = default;

        // Mimic what cmd/cgo would do.
        //go:cgo_import_static __tsan_init
        //go:cgo_import_static __tsan_fini
        //go:cgo_import_static __tsan_proc_create
        //go:cgo_import_static __tsan_proc_destroy
        //go:cgo_import_static __tsan_map_shadow
        //go:cgo_import_static __tsan_finalizer_goroutine
        //go:cgo_import_static __tsan_go_start
        //go:cgo_import_static __tsan_go_end
        //go:cgo_import_static __tsan_malloc
        //go:cgo_import_static __tsan_free
        //go:cgo_import_static __tsan_acquire
        //go:cgo_import_static __tsan_release
        //go:cgo_import_static __tsan_release_merge
        //go:cgo_import_static __tsan_go_ignore_sync_begin
        //go:cgo_import_static __tsan_go_ignore_sync_end
        //go:cgo_import_static __tsan_report_count

        // These are called from race_amd64.s.
        //go:cgo_import_static __tsan_read
        //go:cgo_import_static __tsan_read_pc
        //go:cgo_import_static __tsan_read_range
        //go:cgo_import_static __tsan_write
        //go:cgo_import_static __tsan_write_pc
        //go:cgo_import_static __tsan_write_range
        //go:cgo_import_static __tsan_func_enter
        //go:cgo_import_static __tsan_func_exit

        //go:cgo_import_static __tsan_go_atomic32_load
        //go:cgo_import_static __tsan_go_atomic64_load
        //go:cgo_import_static __tsan_go_atomic32_store
        //go:cgo_import_static __tsan_go_atomic64_store
        //go:cgo_import_static __tsan_go_atomic32_exchange
        //go:cgo_import_static __tsan_go_atomic64_exchange
        //go:cgo_import_static __tsan_go_atomic32_fetch_add
        //go:cgo_import_static __tsan_go_atomic64_fetch_add
        //go:cgo_import_static __tsan_go_atomic32_compare_exchange
        //go:cgo_import_static __tsan_go_atomic64_compare_exchange

        // start/end of global data (data+bss).
        private static System.UIntPtr racedatastart = default;
        private static System.UIntPtr racedataend = default;

        // start/end of heap for race_amd64.s
        private static System.UIntPtr racearenastart = default;
        private static System.UIntPtr racearenaend = default;

        private static void racefuncenter(System.UIntPtr _p0)
;
        private static void racefuncexit()
;
        private static void racereadrangepc1(System.UIntPtr _p0, System.UIntPtr _p0, System.UIntPtr _p0)
;
        private static void racewriterangepc1(System.UIntPtr _p0, System.UIntPtr _p0, System.UIntPtr _p0)
;
        private static void racecallbackthunk(System.UIntPtr _p0)
;

        // racecall allows calling an arbitrary function f from C race runtime
        // with up to 4 uintptr arguments.
        private static void racecall(ref byte _p0, System.UIntPtr _p0, System.UIntPtr _p0, System.UIntPtr _p0, System.UIntPtr _p0)
;

        // checks if the address has shadow (i.e. heap or data/bss)
        //go:nosplit
        private static bool isvalidaddr(unsafe.Pointer addr)
        {
            return racearenastart <= uintptr(addr) && uintptr(addr) < racearenaend || racedatastart <= uintptr(addr) && uintptr(addr) < racedataend;
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) raceinit()
        { 
            // cgo is required to initialize libc, which is used by race runtime
            if (!iscgo)
            {>>MARKER:FUNCTION_racecall_BLOCK_PREFIX<<
                throw("raceinit: race build must use cgo");
            }
            racecall(ref __tsan_init, uintptr(@unsafe.Pointer(ref gctx)), uintptr(@unsafe.Pointer(ref pctx)), funcPC(racecallbackthunk), 0L); 

            // Round data segment to page boundaries, because it's used in mmap().
            var start = ~uintptr(0L);
            var end = uintptr(0L);
            if (start > firstmoduledata.noptrdata)
            {>>MARKER:FUNCTION_racecallbackthunk_BLOCK_PREFIX<<
                start = firstmoduledata.noptrdata;
            }
            if (start > firstmoduledata.data)
            {>>MARKER:FUNCTION_racewriterangepc1_BLOCK_PREFIX<<
                start = firstmoduledata.data;
            }
            if (start > firstmoduledata.noptrbss)
            {>>MARKER:FUNCTION_racereadrangepc1_BLOCK_PREFIX<<
                start = firstmoduledata.noptrbss;
            }
            if (start > firstmoduledata.bss)
            {>>MARKER:FUNCTION_racefuncexit_BLOCK_PREFIX<<
                start = firstmoduledata.bss;
            }
            if (end < firstmoduledata.enoptrdata)
            {>>MARKER:FUNCTION_racefuncenter_BLOCK_PREFIX<<
                end = firstmoduledata.enoptrdata;
            }
            if (end < firstmoduledata.edata)
            {
                end = firstmoduledata.edata;
            }
            if (end < firstmoduledata.enoptrbss)
            {
                end = firstmoduledata.enoptrbss;
            }
            if (end < firstmoduledata.ebss)
            {
                end = firstmoduledata.ebss;
            }
            var size = round(end - start, _PageSize);
            racecall(ref __tsan_map_shadow, start, size, 0L, 0L);
            racedatastart = start;
            racedataend = start + size;

            return;
        }

        private static mutex raceFiniLock = default;

        //go:nosplit
        private static void racefini()
        { 
            // racefini() can only be called once to avoid races.
            // This eventually (via __tsan_fini) calls C.exit which has
            // undefined behavior if called more than once. If the lock is
            // already held it's assumed that the first caller exits the program
            // so other calls can hang forever without an issue.
            lock(ref raceFiniLock);
            racecall(ref __tsan_fini, 0L, 0L, 0L, 0L);
        }

        //go:nosplit
        private static System.UIntPtr raceproccreate()
        {
            System.UIntPtr ctx = default;
            racecall(ref __tsan_proc_create, uintptr(@unsafe.Pointer(ref ctx)), 0L, 0L, 0L);
            return ctx;
        }

        //go:nosplit
        private static void raceprocdestroy(System.UIntPtr ctx)
        {
            racecall(ref __tsan_proc_destroy, ctx, 0L, 0L, 0L);
        }

        //go:nosplit
        private static void racemapshadow(unsafe.Pointer addr, System.UIntPtr size)
        {
            if (racearenastart == 0L)
            {
                racearenastart = uintptr(addr);
            }
            if (racearenaend < uintptr(addr) + size)
            {
                racearenaend = uintptr(addr) + size;
            }
            racecall(ref __tsan_map_shadow, uintptr(addr), size, 0L, 0L);
        }

        //go:nosplit
        private static void racemalloc(unsafe.Pointer p, System.UIntPtr sz)
        {
            racecall(ref __tsan_malloc, 0L, 0L, uintptr(p), sz);
        }

        //go:nosplit
        private static void racefree(unsafe.Pointer p, System.UIntPtr sz)
        {
            racecall(ref __tsan_free, uintptr(p), sz, 0L, 0L);
        }

        //go:nosplit
        private static System.UIntPtr racegostart(System.UIntPtr pc)
        {
            var _g_ = getg();
            ref g spawng = default;
            if (_g_.m.curg != null)
            {
                spawng = _g_.m.curg;
            }
            else
            {
                spawng = _g_;
            }
            System.UIntPtr racectx = default;
            racecall(ref __tsan_go_start, spawng.racectx, uintptr(@unsafe.Pointer(ref racectx)), pc, 0L);
            return racectx;
        }

        //go:nosplit
        private static void racegoend()
        {
            racecall(ref __tsan_go_end, getg().racectx, 0L, 0L, 0L);
        }

        //go:nosplit
        private static void racewriterangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callpc, System.UIntPtr pc)
        {
            var _g_ = getg();
            if (_g_ != _g_.m.curg)
            { 
                // The call is coming from manual instrumentation of Go code running on g0/gsignal.
                // Not interesting.
                return;
            }
            if (callpc != 0L)
            {
                racefuncenter(callpc);
            }
            racewriterangepc1(uintptr(addr), sz, pc);
            if (callpc != 0L)
            {
                racefuncexit();
            }
        }

        //go:nosplit
        private static void racereadrangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callpc, System.UIntPtr pc)
        {
            var _g_ = getg();
            if (_g_ != _g_.m.curg)
            { 
                // The call is coming from manual instrumentation of Go code running on g0/gsignal.
                // Not interesting.
                return;
            }
            if (callpc != 0L)
            {
                racefuncenter(callpc);
            }
            racereadrangepc1(uintptr(addr), sz, pc);
            if (callpc != 0L)
            {
                racefuncexit();
            }
        }

        //go:nosplit
        private static void raceacquire(unsafe.Pointer addr)
        {
            raceacquireg(getg(), addr);
        }

        //go:nosplit
        private static void raceacquireg(ref g gp, unsafe.Pointer addr)
        {
            if (getg().raceignore != 0L || !isvalidaddr(addr))
            {
                return;
            }
            racecall(ref __tsan_acquire, gp.racectx, uintptr(addr), 0L, 0L);
        }

        //go:nosplit
        private static void racerelease(unsafe.Pointer addr)
        {
            racereleaseg(getg(), addr);
        }

        //go:nosplit
        private static void racereleaseg(ref g gp, unsafe.Pointer addr)
        {
            if (getg().raceignore != 0L || !isvalidaddr(addr))
            {
                return;
            }
            racecall(ref __tsan_release, gp.racectx, uintptr(addr), 0L, 0L);
        }

        //go:nosplit
        private static void racereleasemerge(unsafe.Pointer addr)
        {
            racereleasemergeg(getg(), addr);
        }

        //go:nosplit
        private static void racereleasemergeg(ref g gp, unsafe.Pointer addr)
        {
            if (getg().raceignore != 0L || !isvalidaddr(addr))
            {
                return;
            }
            racecall(ref __tsan_release_merge, gp.racectx, uintptr(addr), 0L, 0L);
        }

        //go:nosplit
        private static void racefingo()
        {
            racecall(ref __tsan_finalizer_goroutine, getg().racectx, 0L, 0L, 0L);
        }
    }
}
