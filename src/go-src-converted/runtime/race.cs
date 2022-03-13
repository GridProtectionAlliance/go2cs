// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build race
// +build race

// package runtime -- go2cs converted at 2022 March 13 05:26:43 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\race.go
namespace go;

using @unsafe = @unsafe_package;


// Public race detection API, present iff build with -race.

public static partial class runtime_package {

public static void RaceRead(unsafe.Pointer addr);
public static void RaceWrite(unsafe.Pointer addr);
public static void RaceReadRange(unsafe.Pointer addr, nint len);
public static void RaceWriteRange(unsafe.Pointer addr, nint len);

public static nint RaceErrors() {
    ref ulong n = ref heap(out ptr<ulong> _addr_n);
    racecall(_addr___tsan_report_count, uintptr(@unsafe.Pointer(_addr_n)), 0, 0, 0);
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
public static void RaceAcquire(unsafe.Pointer addr) {
    raceacquire(addr);
}

//go:nosplit

// RaceRelease performs a release operation on addr that
// can synchronize with a later RaceAcquire on addr.
//
// In terms of the C memory model, RaceRelease is equivalent to
// atomic_store(memory_order_release).
public static void RaceRelease(unsafe.Pointer addr) {
    racerelease(addr);
}

//go:nosplit

// RaceReleaseMerge is like RaceRelease, but also establishes a happens-before
// relation with the preceding RaceRelease or RaceReleaseMerge on addr.
//
// In terms of the C memory model, RaceReleaseMerge is equivalent to
// atomic_exchange(memory_order_release).
public static void RaceReleaseMerge(unsafe.Pointer addr) {
    racereleasemerge(addr);
}

//go:nosplit

// RaceDisable disables handling of race synchronization events in the current goroutine.
// Handling is re-enabled with RaceEnable. RaceDisable/RaceEnable can be nested.
// Non-synchronization events (memory accesses, function entry/exit) still affect
// the race detector.
public static void RaceDisable() {
    var _g_ = getg();
    if (_g_.raceignore == 0) {>>MARKER:FUNCTION_RaceWriteRange_BLOCK_PREFIX<<
        racecall(_addr___tsan_go_ignore_sync_begin, _g_.racectx, 0, 0, 0);
    }
    _g_.raceignore++;
}

//go:nosplit

// RaceEnable re-enables handling of race events in the current goroutine.
public static void RaceEnable() {
    var _g_ = getg();
    _g_.raceignore--;
    if (_g_.raceignore == 0) {>>MARKER:FUNCTION_RaceReadRange_BLOCK_PREFIX<<
        racecall(_addr___tsan_go_ignore_sync_end, _g_.racectx, 0, 0, 0);
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
private static void raceReadObjectPC(ptr<_type> _addr_t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    ref _type t = ref _addr_t.val;

    var kind = t.kind & kindMask;
    if (kind == kindArray || kind == kindStruct) {>>MARKER:FUNCTION_RaceWrite_BLOCK_PREFIX<< 
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

private static void raceWriteObjectPC(ptr<_type> _addr_t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    ref _type t = ref _addr_t.val;

    var kind = t.kind & kindMask;
    if (kind == kindArray || kind == kindStruct) { 
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
private static void racereadpc(unsafe.Pointer addr, System.UIntPtr callpc, System.UIntPtr pc);

//go:noescape
private static void racewritepc(unsafe.Pointer addr, System.UIntPtr callpc, System.UIntPtr pc);

private partial struct symbolizeCodeContext {
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
private static void racecallback(System.UIntPtr cmd, unsafe.Pointer ctx) {

    if (cmd == raceGetProcCmd) 
        throw("should have been handled by racecallbackthunk");
    else if (cmd == raceSymbolizeCodeCmd) 
        raceSymbolizeCode(_addr_(symbolizeCodeContext.val)(ctx));
    else if (cmd == raceSymbolizeDataCmd) 
        raceSymbolizeData(_addr_(symbolizeDataContext.val)(ctx));
    else 
        throw("unknown command");
    }

// raceSymbolizeCode reads ctx.pc and populates the rest of *ctx with
// information about the code at that pc.
//
// The race detector has already subtracted 1 from pcs, so they point to the last
// byte of call instructions (including calls to runtime.racewrite and friends).
//
// If the incoming pc is part of an inlined function, *ctx is populated
// with information about the inlined function, and on return ctx.pc is set
// to a pc in the logically containing function. (The race detector should call this
// function again with that pc.)
//
// If the incoming pc is not part of an inlined function, the return pc is unchanged.
private static void raceSymbolizeCode(ptr<symbolizeCodeContext> _addr_ctx) {
    ref symbolizeCodeContext ctx = ref _addr_ctx.val;

    var pc = ctx.pc;
    var fi = findfunc(pc);
    var f = fi._Func();
    if (f != null) {>>MARKER:FUNCTION_racewritepc_BLOCK_PREFIX<<
        var (file, line) = f.FileLine(pc);
        if (line != 0) {>>MARKER:FUNCTION_racereadpc_BLOCK_PREFIX<<
            {
                var inldata = funcdata(fi, _FUNCDATA_InlTree);

                if (inldata != null) {
                    ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                    while (true) {
                        var ix = pcdatavalue(fi, _PCDATA_InlTreeIndex, pc, null);
                        if (ix >= 0) {
                            if (inltree[ix].funcID == funcID_wrapper) { 
                                // ignore wrappers
                                // Back up to an instruction in the "caller".
                                pc = f.Entry() + uintptr(inltree[ix].parentPc);
                                continue;
                            }
                            ctx.pc = f.Entry() + uintptr(inltree[ix].parentPc); // "caller" pc
                            ctx.fn = cfuncnameFromNameoff(fi, inltree[ix].func_);
                            ctx.line = uintptr(line);
                            ctx.file = _addr_bytes(file)[0]; // assume NUL-terminated
                            ctx.off = pc - f.Entry();
                            ctx.res = 1;
                            return ;
                        }
                        break;
                    }
                }

            }
            ctx.fn = cfuncname(fi);
            ctx.line = uintptr(line);
            ctx.file = _addr_bytes(file)[0]; // assume NUL-terminated
            ctx.off = pc - f.Entry();
            ctx.res = 1;
            return ;
        }
    }
    ctx.fn = _addr_qq[0];
    ctx.file = _addr_dash[0];
    ctx.line = 0;
    ctx.off = ctx.pc;
    ctx.res = 1;
}

private partial struct symbolizeDataContext {
    public System.UIntPtr addr;
    public System.UIntPtr heap;
    public System.UIntPtr start;
    public System.UIntPtr size;
    public ptr<byte> name;
    public ptr<byte> file;
    public System.UIntPtr line;
    public System.UIntPtr res;
}

private static void raceSymbolizeData(ptr<symbolizeDataContext> _addr_ctx) {
    ref symbolizeDataContext ctx = ref _addr_ctx.val;

    {
        var (base, span, _) = findObject(ctx.addr, 0, 0);

        if (base != 0) {
            ctx.heap = 1;
            ctx.start = base;
            ctx.size = span.elemsize;
            ctx.res = 1;
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

//go:linkname __tsan_release_acquire __tsan_release_acquire
private static byte __tsan_release_acquire = default;

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
//go:cgo_import_static __tsan_release_acquire
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

private static void racefuncenter(System.UIntPtr callpc);
private static void racefuncenterfp(System.UIntPtr fp);
private static void racefuncexit();
private static void raceread(System.UIntPtr addr);
private static void racewrite(System.UIntPtr addr);
private static void racereadrange(System.UIntPtr addr, System.UIntPtr size);
private static void racewriterange(System.UIntPtr addr, System.UIntPtr size);
private static void racereadrangepc1(System.UIntPtr addr, System.UIntPtr size, System.UIntPtr pc);
private static void racewriterangepc1(System.UIntPtr addr, System.UIntPtr size, System.UIntPtr pc);
private static void racecallbackthunk(System.UIntPtr _p0);

// racecall allows calling an arbitrary function fn from C race runtime
// with up to 4 uintptr arguments.
private static void racecall(ptr<byte> fn, System.UIntPtr arg0, System.UIntPtr arg1, System.UIntPtr arg2, System.UIntPtr arg3);

// checks if the address has shadow (i.e. heap or data/bss)
//go:nosplit
private static bool isvalidaddr(unsafe.Pointer addr) {
    return racearenastart <= uintptr(addr) && uintptr(addr) < racearenaend || racedatastart <= uintptr(addr) && uintptr(addr) < racedataend;
}

//go:nosplit
private static (System.UIntPtr, System.UIntPtr) raceinit() {
    System.UIntPtr gctx = default;
    System.UIntPtr pctx = default;
 
    // cgo is required to initialize libc, which is used by race runtime
    if (!iscgo) {>>MARKER:FUNCTION_racecall_BLOCK_PREFIX<<
        throw("raceinit: race build must use cgo");
    }
    racecall(_addr___tsan_init, uintptr(@unsafe.Pointer(_addr_gctx)), uintptr(@unsafe.Pointer(_addr_pctx)), funcPC(racecallbackthunk), 0); 

    // Round data segment to page boundaries, because it's used in mmap().
    var start = ~uintptr(0);
    var end = uintptr(0);
    if (start > firstmoduledata.noptrdata) {>>MARKER:FUNCTION_racecallbackthunk_BLOCK_PREFIX<<
        start = firstmoduledata.noptrdata;
    }
    if (start > firstmoduledata.data) {>>MARKER:FUNCTION_racewriterangepc1_BLOCK_PREFIX<<
        start = firstmoduledata.data;
    }
    if (start > firstmoduledata.noptrbss) {>>MARKER:FUNCTION_racereadrangepc1_BLOCK_PREFIX<<
        start = firstmoduledata.noptrbss;
    }
    if (start > firstmoduledata.bss) {>>MARKER:FUNCTION_racewriterange_BLOCK_PREFIX<<
        start = firstmoduledata.bss;
    }
    if (end < firstmoduledata.enoptrdata) {>>MARKER:FUNCTION_racereadrange_BLOCK_PREFIX<<
        end = firstmoduledata.enoptrdata;
    }
    if (end < firstmoduledata.edata) {>>MARKER:FUNCTION_racewrite_BLOCK_PREFIX<<
        end = firstmoduledata.edata;
    }
    if (end < firstmoduledata.enoptrbss) {>>MARKER:FUNCTION_raceread_BLOCK_PREFIX<<
        end = firstmoduledata.enoptrbss;
    }
    if (end < firstmoduledata.ebss) {>>MARKER:FUNCTION_racefuncexit_BLOCK_PREFIX<<
        end = firstmoduledata.ebss;
    }
    var size = alignUp(end - start, _PageSize);
    racecall(_addr___tsan_map_shadow, start, size, 0, 0);
    racedatastart = start;
    racedataend = start + size;

    return ;
}

private static mutex raceFiniLock = default;

//go:nosplit
private static void racefini() { 
    // racefini() can only be called once to avoid races.
    // This eventually (via __tsan_fini) calls C.exit which has
    // undefined behavior if called more than once. If the lock is
    // already held it's assumed that the first caller exits the program
    // so other calls can hang forever without an issue.
    lock(_addr_raceFiniLock); 
    // We're entering external code that may call ExitProcess on
    // Windows.
    osPreemptExtEnter(getg().m);
    racecall(_addr___tsan_fini, 0, 0, 0, 0);
}

//go:nosplit
private static System.UIntPtr raceproccreate() {
    ref System.UIntPtr ctx = ref heap(out ptr<System.UIntPtr> _addr_ctx);
    racecall(_addr___tsan_proc_create, uintptr(@unsafe.Pointer(_addr_ctx)), 0, 0, 0);
    return ctx;
}

//go:nosplit
private static void raceprocdestroy(System.UIntPtr ctx) {
    racecall(_addr___tsan_proc_destroy, ctx, 0, 0, 0);
}

//go:nosplit
private static void racemapshadow(unsafe.Pointer addr, System.UIntPtr size) {
    if (racearenastart == 0) {>>MARKER:FUNCTION_racefuncenterfp_BLOCK_PREFIX<<
        racearenastart = uintptr(addr);
    }
    if (racearenaend < uintptr(addr) + size) {>>MARKER:FUNCTION_racefuncenter_BLOCK_PREFIX<<
        racearenaend = uintptr(addr) + size;
    }
    racecall(_addr___tsan_map_shadow, uintptr(addr), size, 0, 0);
}

//go:nosplit
private static void racemalloc(unsafe.Pointer p, System.UIntPtr sz) {
    racecall(_addr___tsan_malloc, 0, 0, uintptr(p), sz);
}

//go:nosplit
private static void racefree(unsafe.Pointer p, System.UIntPtr sz) {
    racecall(_addr___tsan_free, uintptr(p), sz, 0, 0);
}

//go:nosplit
private static System.UIntPtr racegostart(System.UIntPtr pc) {
    var _g_ = getg();
    ptr<g> spawng;
    if (_g_.m.curg != null) {
        spawng = _g_.m.curg;
    }
    else
 {
        spawng = _g_;
    }
    ref System.UIntPtr racectx = ref heap(out ptr<System.UIntPtr> _addr_racectx);
    racecall(_addr___tsan_go_start, spawng.racectx, uintptr(@unsafe.Pointer(_addr_racectx)), pc, 0);
    return racectx;
}

//go:nosplit
private static void racegoend() {
    racecall(_addr___tsan_go_end, getg().racectx, 0, 0, 0);
}

//go:nosplit
private static void racectxend(System.UIntPtr racectx) {
    racecall(_addr___tsan_go_end, racectx, 0, 0, 0);
}

//go:nosplit
private static void racewriterangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callpc, System.UIntPtr pc) {
    var _g_ = getg();
    if (_g_ != _g_.m.curg) { 
        // The call is coming from manual instrumentation of Go code running on g0/gsignal.
        // Not interesting.
        return ;
    }
    if (callpc != 0) {
        racefuncenter(callpc);
    }
    racewriterangepc1(uintptr(addr), sz, pc);
    if (callpc != 0) {
        racefuncexit();
    }
}

//go:nosplit
private static void racereadrangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callpc, System.UIntPtr pc) {
    var _g_ = getg();
    if (_g_ != _g_.m.curg) { 
        // The call is coming from manual instrumentation of Go code running on g0/gsignal.
        // Not interesting.
        return ;
    }
    if (callpc != 0) {
        racefuncenter(callpc);
    }
    racereadrangepc1(uintptr(addr), sz, pc);
    if (callpc != 0) {
        racefuncexit();
    }
}

//go:nosplit
private static void raceacquire(unsafe.Pointer addr) {
    raceacquireg(_addr_getg(), addr);
}

//go:nosplit
private static void raceacquireg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    if (getg().raceignore != 0 || !isvalidaddr(addr)) {
        return ;
    }
    racecall(_addr___tsan_acquire, gp.racectx, uintptr(addr), 0, 0);
}

//go:nosplit
private static void raceacquirectx(System.UIntPtr racectx, unsafe.Pointer addr) {
    if (!isvalidaddr(addr)) {
        return ;
    }
    racecall(_addr___tsan_acquire, racectx, uintptr(addr), 0, 0);
}

//go:nosplit
private static void racerelease(unsafe.Pointer addr) {
    racereleaseg(_addr_getg(), addr);
}

//go:nosplit
private static void racereleaseg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    if (getg().raceignore != 0 || !isvalidaddr(addr)) {
        return ;
    }
    racecall(_addr___tsan_release, gp.racectx, uintptr(addr), 0, 0);
}

//go:nosplit
private static void racereleaseacquire(unsafe.Pointer addr) {
    racereleaseacquireg(_addr_getg(), addr);
}

//go:nosplit
private static void racereleaseacquireg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    if (getg().raceignore != 0 || !isvalidaddr(addr)) {
        return ;
    }
    racecall(_addr___tsan_release_acquire, gp.racectx, uintptr(addr), 0, 0);
}

//go:nosplit
private static void racereleasemerge(unsafe.Pointer addr) {
    racereleasemergeg(_addr_getg(), addr);
}

//go:nosplit
private static void racereleasemergeg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    if (getg().raceignore != 0 || !isvalidaddr(addr)) {
        return ;
    }
    racecall(_addr___tsan_release_merge, gp.racectx, uintptr(addr), 0, 0);
}

//go:nosplit
private static void racefingo() {
    racecall(_addr___tsan_finalizer_goroutine, getg().racectx, 0, 0, 0);
}

// The declarations below generate ABI wrappers for functions
// implemented in assembly in this package but declared in another
// package.

//go:linkname abigen_sync_atomic_LoadInt32 sync/atomic.LoadInt32
private static int abigen_sync_atomic_LoadInt32(ptr<int> addr);

//go:linkname abigen_sync_atomic_LoadInt64 sync/atomic.LoadInt64
private static long abigen_sync_atomic_LoadInt64(ptr<long> addr);

//go:linkname abigen_sync_atomic_LoadUint32 sync/atomic.LoadUint32
private static uint abigen_sync_atomic_LoadUint32(ptr<uint> addr);

//go:linkname abigen_sync_atomic_LoadUint64 sync/atomic.LoadUint64
private static ulong abigen_sync_atomic_LoadUint64(ptr<ulong> addr);

//go:linkname abigen_sync_atomic_LoadUintptr sync/atomic.LoadUintptr
private static System.UIntPtr abigen_sync_atomic_LoadUintptr(ptr<System.UIntPtr> addr);

//go:linkname abigen_sync_atomic_LoadPointer sync/atomic.LoadPointer
private static unsafe.Pointer abigen_sync_atomic_LoadPointer(ptr<unsafe.Pointer> addr);

//go:linkname abigen_sync_atomic_StoreInt32 sync/atomic.StoreInt32
private static void abigen_sync_atomic_StoreInt32(ptr<int> addr, int val);

//go:linkname abigen_sync_atomic_StoreInt64 sync/atomic.StoreInt64
private static void abigen_sync_atomic_StoreInt64(ptr<long> addr, long val);

//go:linkname abigen_sync_atomic_StoreUint32 sync/atomic.StoreUint32
private static void abigen_sync_atomic_StoreUint32(ptr<uint> addr, uint val);

//go:linkname abigen_sync_atomic_StoreUint64 sync/atomic.StoreUint64
private static void abigen_sync_atomic_StoreUint64(ptr<ulong> addr, ulong val);

//go:linkname abigen_sync_atomic_SwapInt32 sync/atomic.SwapInt32
private static int abigen_sync_atomic_SwapInt32(ptr<int> addr, int @new);

//go:linkname abigen_sync_atomic_SwapInt64 sync/atomic.SwapInt64
private static long abigen_sync_atomic_SwapInt64(ptr<long> addr, long @new);

//go:linkname abigen_sync_atomic_SwapUint32 sync/atomic.SwapUint32
private static uint abigen_sync_atomic_SwapUint32(ptr<uint> addr, uint @new);

//go:linkname abigen_sync_atomic_SwapUint64 sync/atomic.SwapUint64
private static ulong abigen_sync_atomic_SwapUint64(ptr<ulong> addr, ulong @new);

//go:linkname abigen_sync_atomic_AddInt32 sync/atomic.AddInt32
private static int abigen_sync_atomic_AddInt32(ptr<int> addr, int delta);

//go:linkname abigen_sync_atomic_AddUint32 sync/atomic.AddUint32
private static uint abigen_sync_atomic_AddUint32(ptr<uint> addr, uint delta);

//go:linkname abigen_sync_atomic_AddInt64 sync/atomic.AddInt64
private static long abigen_sync_atomic_AddInt64(ptr<long> addr, long delta);

//go:linkname abigen_sync_atomic_AddUint64 sync/atomic.AddUint64
private static ulong abigen_sync_atomic_AddUint64(ptr<ulong> addr, ulong delta);

//go:linkname abigen_sync_atomic_AddUintptr sync/atomic.AddUintptr
private static System.UIntPtr abigen_sync_atomic_AddUintptr(ptr<System.UIntPtr> addr, System.UIntPtr delta);

//go:linkname abigen_sync_atomic_CompareAndSwapInt32 sync/atomic.CompareAndSwapInt32
private static bool abigen_sync_atomic_CompareAndSwapInt32(ptr<int> addr, int old, int @new);

//go:linkname abigen_sync_atomic_CompareAndSwapInt64 sync/atomic.CompareAndSwapInt64
private static bool abigen_sync_atomic_CompareAndSwapInt64(ptr<long> addr, long old, long @new);

//go:linkname abigen_sync_atomic_CompareAndSwapUint32 sync/atomic.CompareAndSwapUint32
private static bool abigen_sync_atomic_CompareAndSwapUint32(ptr<uint> addr, uint old, uint @new);

//go:linkname abigen_sync_atomic_CompareAndSwapUint64 sync/atomic.CompareAndSwapUint64
private static bool abigen_sync_atomic_CompareAndSwapUint64(ptr<ulong> addr, ulong old, ulong @new);

} // end runtime_package
