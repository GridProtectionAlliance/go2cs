// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Malloc profiling.
// Patterned after tcmalloc's algorithms; shorter code.

// package runtime -- go2cs converted at 2022 March 13 05:25:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mprof.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;


// NOTE(rsc): Everything here could use cas if contention became an issue.

using System;
public static partial class runtime_package {

private static mutex proflock = default;

// All memory allocations are local and do not escape outside of the profiler.
// The profiler is forbidden from referring to garbage-collected memory.

 
// profile types
private static readonly bucketType memProfile = 1 + iota;
private static readonly var blockProfile = 0;
private static readonly buckHashSize mutexProfile = 179999; 

// max depth of stack to record in bucket
private static readonly nint maxStack = 32;

private partial struct bucketType { // : nint
}

// A bucket holds per-call-stack profiling information.
// The representation is a bit sleazy, inherited from C.
// This struct defines the bucket header. It is followed in
// memory by the stack words and then the actual record
// data, either a memRecord or a blockRecord.
//
// Per-call-stack profiling information.
// Lookup by hashing call stack into a linked-list hash table.
//
// No heap pointers.
//
//go:notinheap
private partial struct bucket {
    public ptr<bucket> next;
    public ptr<bucket> allnext;
    public bucketType typ; // memBucket or blockBucket (includes mutexProfile)
    public System.UIntPtr hash;
    public System.UIntPtr size;
    public System.UIntPtr nstk;
}

// A memRecord is the bucket data for a bucket of type memProfile,
// part of the memory profile.
private partial struct memRecord {
    public memRecordCycle active; // future records the profile events we're counting for cycles
// that have not yet been published. This is ring buffer
// indexed by the global heap profile cycle C and stores
// cycles C, C+1, and C+2. Unlike active, these counts are
// only for a single cycle; they are not cumulative across
// cycles.
//
// We store cycle C here because there's a window between when
// C becomes the active cycle and when we've flushed it to
// active.
    public array<memRecordCycle> future;
}

// memRecordCycle
private partial struct memRecordCycle {
    public System.UIntPtr allocs;
    public System.UIntPtr frees;
    public System.UIntPtr alloc_bytes;
    public System.UIntPtr free_bytes;
}

// add accumulates b into a. It does not zero b.
private static void add(this ptr<memRecordCycle> _addr_a, ptr<memRecordCycle> _addr_b) {
    ref memRecordCycle a = ref _addr_a.val;
    ref memRecordCycle b = ref _addr_b.val;

    a.allocs += b.allocs;
    a.frees += b.frees;
    a.alloc_bytes += b.alloc_bytes;
    a.free_bytes += b.free_bytes;
}

// A blockRecord is the bucket data for a bucket of type blockProfile,
// which is used in blocking and mutex profiles.
private partial struct blockRecord {
    public double count;
    public long cycles;
}

private static ptr<bucket> mbuckets;private static ptr<bucket> bbuckets;private static ptr<bucket> xbuckets;private static ptr<array<ptr<bucket>>> buckhash;private static System.UIntPtr bucketmem = default;private static var mProf = default;

private static readonly var mProfCycleWrap = uint32(len(new memRecord().future)) * (2 << 24);

// newBucket allocates a bucket with the given type and number of stack entries.


// newBucket allocates a bucket with the given type and number of stack entries.
private static ptr<bucket> newBucket(bucketType typ, nint nstk) {
    var size = @unsafe.Sizeof(new bucket()) + uintptr(nstk) * @unsafe.Sizeof(uintptr(0));

    if (typ == memProfile) 
        size += @unsafe.Sizeof(new memRecord());
    else if (typ == blockProfile || typ == mutexProfile) 
        size += @unsafe.Sizeof(new blockRecord());
    else 
        throw("invalid profile bucket type");
        var b = (bucket.val)(persistentalloc(size, 0, _addr_memstats.buckhash_sys));
    bucketmem += size;
    b.typ = typ;
    b.nstk = uintptr(nstk);
    return _addr_b!;
}

// stk returns the slice in b holding the stack.
private static slice<System.UIntPtr> stk(this ptr<bucket> _addr_b) {
    ref bucket b = ref _addr_b.val;

    ptr<array<System.UIntPtr>> stk = new ptr<ptr<array<System.UIntPtr>>>(add(@unsafe.Pointer(b), @unsafe.Sizeof(b.val)));
    return stk.slice(-1, b.nstk, b.nstk);
}

// mp returns the memRecord associated with the memProfile bucket b.
private static ptr<memRecord> mp(this ptr<bucket> _addr_b) {
    ref bucket b = ref _addr_b.val;

    if (b.typ != memProfile) {
        throw("bad use of bucket.mp");
    }
    var data = add(@unsafe.Pointer(b), @unsafe.Sizeof(b.val) + b.nstk * @unsafe.Sizeof(uintptr(0)));
    return _addr_(memRecord.val)(data)!;
}

// bp returns the blockRecord associated with the blockProfile bucket b.
private static ptr<blockRecord> bp(this ptr<bucket> _addr_b) {
    ref bucket b = ref _addr_b.val;

    if (b.typ != blockProfile && b.typ != mutexProfile) {
        throw("bad use of bucket.bp");
    }
    var data = add(@unsafe.Pointer(b), @unsafe.Sizeof(b.val) + b.nstk * @unsafe.Sizeof(uintptr(0)));
    return _addr_(blockRecord.val)(data)!;
}

// Return the bucket for stk[0:nstk], allocating new bucket if needed.
private static ptr<bucket> stkbucket(bucketType typ, System.UIntPtr size, slice<System.UIntPtr> stk, bool alloc) {
    if (buckhash == null) {
        buckhash = new ptr<ptr<array<ptr<bucket>>>>(sysAlloc(@unsafe.Sizeof(buckhash.val), _addr_memstats.buckhash_sys));
        if (buckhash == null) {
            throw("runtime: cannot allocate memory");
        }
    }
    System.UIntPtr h = default;
    foreach (var (_, pc) in stk) {
        h += pc;
        h += h << 10;
        h ^= h >> 6;
    }    h += size;
    h += h << 10;
    h ^= h >> 6; 
    // finalize
    h += h << 3;
    h ^= h >> 11;

    var i = int(h % buckHashSize);
    {
        var b__prev1 = b;

        var b = buckhash[i];

        while (b != null) {
            if (b.typ == typ && b.hash == h && b.size == size && eqslice(b.stk(), stk)) {
                return _addr_b!;
            b = b.next;
            }
        }

        b = b__prev1;
    }

    if (!alloc) {
        return _addr_null!;
    }
    b = newBucket(typ, len(stk));
    copy(b.stk(), stk);
    b.hash = h;
    b.size = size;
    b.next = buckhash[i];
    buckhash[i] = b;
    if (typ == memProfile) {
        b.allnext = mbuckets;
        mbuckets = b;
    }
    else if (typ == mutexProfile) {
        b.allnext = xbuckets;
        xbuckets = b;
    }
    else
 {
        b.allnext = bbuckets;
        bbuckets = b;
    }
    return _addr_b!;
}

private static bool eqslice(slice<System.UIntPtr> x, slice<System.UIntPtr> y) {
    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, xi) in x) {
        if (xi != y[i]) {
            return false;
        }
    }    return true;
}

// mProf_NextCycle publishes the next heap profile cycle and creates a
// fresh heap profile cycle. This operation is fast and can be done
// during STW. The caller must call mProf_Flush before calling
// mProf_NextCycle again.
//
// This is called by mark termination during STW so allocations and
// frees after the world is started again count towards a new heap
// profiling cycle.
private static void mProf_NextCycle() {
    lock(_addr_proflock); 
    // We explicitly wrap mProf.cycle rather than depending on
    // uint wraparound because the memRecord.future ring does not
    // itself wrap at a power of two.
    mProf.cycle = (mProf.cycle + 1) % mProfCycleWrap;
    mProf.flushed = false;
    unlock(_addr_proflock);
}

// mProf_Flush flushes the events from the current heap profiling
// cycle into the active profile. After this it is safe to start a new
// heap profiling cycle with mProf_NextCycle.
//
// This is called by GC after mark termination starts the world. In
// contrast with mProf_NextCycle, this is somewhat expensive, but safe
// to do concurrently.
private static void mProf_Flush() {
    lock(_addr_proflock);
    if (!mProf.flushed) {
        mProf_FlushLocked();
        mProf.flushed = true;
    }
    unlock(_addr_proflock);
}

private static void mProf_FlushLocked() {
    var c = mProf.cycle;
    {
        var b = mbuckets;

        while (b != null) {
            var mp = b.mp(); 

            // Flush cycle C into the published profile and clear
            // it for reuse.
            var mpc = _addr_mp.future[c % uint32(len(mp.future))];
            mp.active.add(mpc);
            mpc.val = new memRecordCycle();
            b = b.allnext;
        }
    }
}

// mProf_PostSweep records that all sweep frees for this GC cycle have
// completed. This has the effect of publishing the heap profile
// snapshot as of the last mark termination without advancing the heap
// profile cycle.
private static void mProf_PostSweep() {
    lock(_addr_proflock); 
    // Flush cycle C+1 to the active profile so everything as of
    // the last mark termination becomes visible. *Don't* advance
    // the cycle, since we're still accumulating allocs in cycle
    // C+2, which have to become C+1 in the next mark termination
    // and so on.
    var c = mProf.cycle;
    {
        var b = mbuckets;

        while (b != null) {
            var mp = b.mp();
            var mpc = _addr_mp.future[(c + 1) % uint32(len(mp.future))];
            mp.active.add(mpc);
            mpc.val = new memRecordCycle();
            b = b.allnext;
        }
    }
    unlock(_addr_proflock);
}

// Called by malloc to record a profiled block.
private static void mProf_Malloc(unsafe.Pointer p, System.UIntPtr size) {
    array<System.UIntPtr> stk = new array<System.UIntPtr>(maxStack);
    var nstk = callers(4, stk[..]);
    lock(_addr_proflock);
    var b = stkbucket(memProfile, size, stk[..(int)nstk], true);
    var c = mProf.cycle;
    var mp = b.mp();
    var mpc = _addr_mp.future[(c + 2) % uint32(len(mp.future))];
    mpc.allocs++;
    mpc.alloc_bytes += size;
    unlock(_addr_proflock); 

    // Setprofilebucket locks a bunch of other mutexes, so we call it outside of proflock.
    // This reduces potential contention and chances of deadlocks.
    // Since the object must be alive during call to mProf_Malloc,
    // it's fine to do this non-atomically.
    systemstack(() => {
        setprofilebucket(p, b);
    });
}

// Called when freeing a profiled block.
private static void mProf_Free(ptr<bucket> _addr_b, System.UIntPtr size) {
    ref bucket b = ref _addr_b.val;

    lock(_addr_proflock);
    var c = mProf.cycle;
    var mp = b.mp();
    var mpc = _addr_mp.future[(c + 1) % uint32(len(mp.future))];
    mpc.frees++;
    mpc.free_bytes += size;
    unlock(_addr_proflock);
}

private static ulong blockprofilerate = default; // in CPU ticks

// SetBlockProfileRate controls the fraction of goroutine blocking events
// that are reported in the blocking profile. The profiler aims to sample
// an average of one blocking event per rate nanoseconds spent blocked.
//
// To include every blocking event in the profile, pass rate = 1.
// To turn off profiling entirely, pass rate <= 0.
public static void SetBlockProfileRate(nint rate) {
    long r = default;
    if (rate <= 0) {
        r = 0; // disable profiling
    }
    else if (rate == 1) {
        r = 1; // profile everything
    }
    else
 { 
        // convert ns to cycles, use float64 to prevent overflow during multiplication
        r = int64(float64(rate) * float64(tickspersecond()) / (1000 * 1000 * 1000));
        if (r == 0) {
            r = 1;
        }
    }
    atomic.Store64(_addr_blockprofilerate, uint64(r));
}

private static void blockevent(long cycles, nint skip) {
    if (cycles <= 0) {
        cycles = 1;
    }
    var rate = int64(atomic.Load64(_addr_blockprofilerate));
    if (blocksampled(cycles, rate)) {
        saveblockevent(cycles, rate, skip + 1, blockProfile);
    }
}

// blocksampled returns true for all events where cycles >= rate. Shorter
// events have a cycles/rate random chance of returning true.
private static bool blocksampled(long cycles, long rate) {
    if (rate <= 0 || (rate > cycles && int64(fastrand()) % rate > cycles)) {
        return false;
    }
    return true;
}

private static void saveblockevent(long cycles, long rate, nint skip, bucketType which) {
    var gp = getg();
    nint nstk = default;
    array<System.UIntPtr> stk = new array<System.UIntPtr>(maxStack);
    if (gp.m.curg == null || gp.m.curg == gp) {
        nstk = callers(skip, stk[..]);
    }
    else
 {
        nstk = gcallers(gp.m.curg, skip, stk[..]);
    }
    lock(_addr_proflock);
    var b = stkbucket(which, 0, stk[..(int)nstk], true);

    if (which == blockProfile && cycles < rate) { 
        // Remove sampling bias, see discussion on http://golang.org/cl/299991.
        b.bp().count += float64(rate) / float64(cycles);
        b.bp().cycles += rate;
    }
    else
 {
        b.bp().count++;
        b.bp().cycles += cycles;
    }
    unlock(_addr_proflock);
}

private static ulong mutexprofilerate = default; // fraction sampled

// SetMutexProfileFraction controls the fraction of mutex contention events
// that are reported in the mutex profile. On average 1/rate events are
// reported. The previous rate is returned.
//
// To turn off profiling entirely, pass rate 0.
// To just read the current rate, pass rate < 0.
// (For n>1 the details of sampling may change.)
public static nint SetMutexProfileFraction(nint rate) {
    if (rate < 0) {
        return int(mutexprofilerate);
    }
    var old = mutexprofilerate;
    atomic.Store64(_addr_mutexprofilerate, uint64(rate));
    return int(old);
}

//go:linkname mutexevent sync.event
private static void mutexevent(long cycles, nint skip) {
    if (cycles < 0) {
        cycles = 0;
    }
    var rate = int64(atomic.Load64(_addr_mutexprofilerate)); 
    // TODO(pjw): measure impact of always calling fastrand vs using something
    // like malloc.go:nextSample()
    if (rate > 0 && int64(fastrand()) % rate == 0) {
        saveblockevent(cycles, rate, skip + 1, mutexProfile);
    }
}

// Go interface to profile data.

// A StackRecord describes a single execution stack.
public partial struct StackRecord {
    public array<System.UIntPtr> Stack0; // stack trace for this record; ends at first 0 entry
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
private static slice<System.UIntPtr> Stack(this ptr<StackRecord> _addr_r) {
    ref StackRecord r = ref _addr_r.val;

    foreach (var (i, v) in r.Stack0) {
        if (v == 0) {
            return r.Stack0[(int)0..(int)i];
        }
    }    return r.Stack0[(int)0..];
}

// MemProfileRate controls the fraction of memory allocations
// that are recorded and reported in the memory profile.
// The profiler aims to sample an average of
// one allocation per MemProfileRate bytes allocated.
//
// To include every allocated block in the profile, set MemProfileRate to 1.
// To turn off profiling entirely, set MemProfileRate to 0.
//
// The tools that process the memory profiles assume that the
// profile rate is constant across the lifetime of the program
// and equal to the current value. Programs that change the
// memory profiling rate should do so just once, as early as
// possible in the execution of the program (for example,
// at the beginning of main).
public static nint MemProfileRate = defaultMemProfileRate(512 * 1024);

// defaultMemProfileRate returns 0 if disableMemoryProfiling is set.
// It exists primarily for the godoc rendering of MemProfileRate
// above.
private static nint defaultMemProfileRate(nint v) {
    if (disableMemoryProfiling) {
        return 0;
    }
    return v;
}

// disableMemoryProfiling is set by the linker if runtime.MemProfile
// is not used and the link type guarantees nobody else could use it
// elsewhere.
private static bool disableMemoryProfiling = default;

// A MemProfileRecord describes the live objects allocated
// by a particular call sequence (stack trace).
public partial struct MemProfileRecord {
    public long AllocBytes; // number of bytes allocated, freed
    public long FreeBytes; // number of bytes allocated, freed
    public long AllocObjects; // number of objects allocated, freed
    public long FreeObjects; // number of objects allocated, freed
    public array<System.UIntPtr> Stack0; // stack trace for this record; ends at first 0 entry
}

// InUseBytes returns the number of bytes in use (AllocBytes - FreeBytes).
private static long InUseBytes(this ptr<MemProfileRecord> _addr_r) {
    ref MemProfileRecord r = ref _addr_r.val;

    return r.AllocBytes - r.FreeBytes;
}

// InUseObjects returns the number of objects in use (AllocObjects - FreeObjects).
private static long InUseObjects(this ptr<MemProfileRecord> _addr_r) {
    ref MemProfileRecord r = ref _addr_r.val;

    return r.AllocObjects - r.FreeObjects;
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
private static slice<System.UIntPtr> Stack(this ptr<MemProfileRecord> _addr_r) {
    ref MemProfileRecord r = ref _addr_r.val;

    foreach (var (i, v) in r.Stack0) {
        if (v == 0) {
            return r.Stack0[(int)0..(int)i];
        }
    }    return r.Stack0[(int)0..];
}

// MemProfile returns a profile of memory allocated and freed per allocation
// site.
//
// MemProfile returns n, the number of records in the current memory profile.
// If len(p) >= n, MemProfile copies the profile into p and returns n, true.
// If len(p) < n, MemProfile does not change p and returns n, false.
//
// If inuseZero is true, the profile includes allocation records
// where r.AllocBytes > 0 but r.AllocBytes == r.FreeBytes.
// These are sites where memory was allocated, but it has all
// been released back to the runtime.
//
// The returned profile may be up to two garbage collection cycles old.
// This is to avoid skewing the profile toward allocations; because
// allocations happen in real time but frees are delayed until the garbage
// collector performs sweeping, the profile only accounts for allocations
// that have had a chance to be freed by the garbage collector.
//
// Most clients should use the runtime/pprof package or
// the testing package's -test.memprofile flag instead
// of calling MemProfile directly.
public static (nint, bool) MemProfile(slice<MemProfileRecord> p, bool inuseZero) {
    nint n = default;
    bool ok = default;

    lock(_addr_proflock); 
    // If we're between mProf_NextCycle and mProf_Flush, take care
    // of flushing to the active profile so we only have to look
    // at the active profile below.
    mProf_FlushLocked();
    var clear = true;
    {
        var b__prev1 = b;

        var b = mbuckets;

        while (b != null) {
            var mp = b.mp();
            if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes) {
                n++;
            b = b.allnext;
            }
            if (mp.active.allocs != 0 || mp.active.frees != 0) {
                clear = false;
            }
        }

        b = b__prev1;
    }
    if (clear) { 
        // Absolutely no data, suggesting that a garbage collection
        // has not yet happened. In order to allow profiling when
        // garbage collection is disabled from the beginning of execution,
        // accumulate all of the cycles, and recount buckets.
        n = 0;
        {
            var b__prev1 = b;

            b = mbuckets;

            while (b != null) {
                mp = b.mp();
                foreach (var (c) in mp.future) {
                    mp.active.add(_addr_mp.future[c]);
                    mp.future[c] = new memRecordCycle();
                }
                if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes) {
                    n++;
                b = b.allnext;
                }
            }


            b = b__prev1;
        }
    }
    if (n <= len(p)) {
        ok = true;
        nint idx = 0;
        {
            var b__prev1 = b;

            b = mbuckets;

            while (b != null) {
                mp = b.mp();
                if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes) {
                    record(_addr_p[idx], _addr_b);
                    idx++;
                b = b.allnext;
                }
            }


            b = b__prev1;
        }
    }
    unlock(_addr_proflock);
    return ;
}

// Write b's data to r.
private static void record(ptr<MemProfileRecord> _addr_r, ptr<bucket> _addr_b) {
    ref MemProfileRecord r = ref _addr_r.val;
    ref bucket b = ref _addr_b.val;

    var mp = b.mp();
    r.AllocBytes = int64(mp.active.alloc_bytes);
    r.FreeBytes = int64(mp.active.free_bytes);
    r.AllocObjects = int64(mp.active.allocs);
    r.FreeObjects = int64(mp.active.frees);
    if (raceenabled) {
        racewriterangepc(@unsafe.Pointer(_addr_r.Stack0[0]), @unsafe.Sizeof(r.Stack0), getcallerpc(), funcPC(MemProfile));
    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer(_addr_r.Stack0[0]), @unsafe.Sizeof(r.Stack0));
    }
    copy(r.Stack0[..], b.stk());
    for (var i = int(b.nstk); i < len(r.Stack0); i++) {
        r.Stack0[i] = 0;
    }
}

private static void iterate_memprof(Action<ptr<bucket>, System.UIntPtr, ptr<System.UIntPtr>, System.UIntPtr, System.UIntPtr, System.UIntPtr> fn) {
    lock(_addr_proflock);
    {
        var b = mbuckets;

        while (b != null) {
            var mp = b.mp();
            fn(b, b.nstk, _addr_b.stk()[0], b.size, mp.active.allocs, mp.active.frees);
            b = b.allnext;
        }
    }
    unlock(_addr_proflock);
}

// BlockProfileRecord describes blocking events originated
// at a particular call sequence (stack trace).
public partial struct BlockProfileRecord {
    public long Count;
    public long Cycles;
    public ref StackRecord StackRecord => ref StackRecord_val;
}

// BlockProfile returns n, the number of records in the current blocking profile.
// If len(p) >= n, BlockProfile copies the profile into p and returns n, true.
// If len(p) < n, BlockProfile does not change p and returns n, false.
//
// Most clients should use the runtime/pprof package or
// the testing package's -test.blockprofile flag instead
// of calling BlockProfile directly.
public static (nint, bool) BlockProfile(slice<BlockProfileRecord> p) {
    nint n = default;
    bool ok = default;

    lock(_addr_proflock);
    {
        var b__prev1 = b;

        var b = bbuckets;

        while (b != null) {
            n++;
            b = b.allnext;
        }

        b = b__prev1;
    }
    if (n <= len(p)) {
        ok = true;
        {
            var b__prev1 = b;

            b = bbuckets;

            while (b != null) {
                var bp = b.bp();
                var r = _addr_p[0];
                r.Count = int64(bp.count); 
                // Prevent callers from having to worry about division by zero errors.
                // See discussion on http://golang.org/cl/299991.
                if (r.Count == 0) {
                    r.Count = 1;
                b = b.allnext;
                }
                r.Cycles = bp.cycles;
                if (raceenabled) {
                    racewriterangepc(@unsafe.Pointer(_addr_r.Stack0[0]), @unsafe.Sizeof(r.Stack0), getcallerpc(), funcPC(BlockProfile));
                }
                if (msanenabled) {
                    msanwrite(@unsafe.Pointer(_addr_r.Stack0[0]), @unsafe.Sizeof(r.Stack0));
                }
                var i = copy(r.Stack0[..], b.stk());
                while (i < len(r.Stack0)) {
                    r.Stack0[i] = 0;
                    i++;
                }

                p = p[(int)1..];
            }


            b = b__prev1;
        }
    }
    unlock(_addr_proflock);
    return ;
}

// MutexProfile returns n, the number of records in the current mutex profile.
// If len(p) >= n, MutexProfile copies the profile into p and returns n, true.
// Otherwise, MutexProfile does not change p, and returns n, false.
//
// Most clients should use the runtime/pprof package
// instead of calling MutexProfile directly.
public static (nint, bool) MutexProfile(slice<BlockProfileRecord> p) {
    nint n = default;
    bool ok = default;

    lock(_addr_proflock);
    {
        var b__prev1 = b;

        var b = xbuckets;

        while (b != null) {
            n++;
            b = b.allnext;
        }

        b = b__prev1;
    }
    if (n <= len(p)) {
        ok = true;
        {
            var b__prev1 = b;

            b = xbuckets;

            while (b != null) {
                var bp = b.bp();
                var r = _addr_p[0];
                r.Count = int64(bp.count);
                r.Cycles = bp.cycles;
                var i = copy(r.Stack0[..], b.stk());
                while (i < len(r.Stack0)) {
                    r.Stack0[i] = 0;
                    i++;
                }

                p = p[(int)1..];
                b = b.allnext;
            }


            b = b__prev1;
        }
    }
    unlock(_addr_proflock);
    return ;
}

// ThreadCreateProfile returns n, the number of records in the thread creation profile.
// If len(p) >= n, ThreadCreateProfile copies the profile into p and returns n, true.
// If len(p) < n, ThreadCreateProfile does not change p and returns n, false.
//
// Most clients should use the runtime/pprof package instead
// of calling ThreadCreateProfile directly.
public static (nint, bool) ThreadCreateProfile(slice<StackRecord> p) {
    nint n = default;
    bool ok = default;

    var first = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));
    {
        var mp__prev1 = mp;

        var mp = first;

        while (mp != null) {
            n++;
            mp = mp.alllink;
        }

        mp = mp__prev1;
    }
    if (n <= len(p)) {
        ok = true;
        nint i = 0;
        {
            var mp__prev1 = mp;

            mp = first;

            while (mp != null) {
                p[i].Stack0 = mp.createstack;
                i++;
                mp = mp.alllink;
            }


            mp = mp__prev1;
        }
    }
    return ;
}

//go:linkname runtime_goroutineProfileWithLabels runtime/pprof.runtime_goroutineProfileWithLabels
private static (nint, bool) runtime_goroutineProfileWithLabels(slice<StackRecord> p, slice<unsafe.Pointer> labels) {
    nint n = default;
    bool ok = default;

    return goroutineProfileWithLabels(p, labels);
}

// labels may be nil. If labels is non-nil, it must have the same length as p.
private static (nint, bool) goroutineProfileWithLabels(slice<StackRecord> p, slice<unsafe.Pointer> labels) {
    nint n = default;
    bool ok = default;

    if (labels != null && len(labels) != len(p)) {
        labels = null;
    }
    var gp = getg();

    Func<ptr<g>, bool> isOK = gp1 => { 
        // Checking isSystemGoroutine here makes GoroutineProfile
        // consistent with both NumGoroutine and Stack.
        return gp1 != gp && readgstatus(gp1) != _Gdead && !isSystemGoroutine(gp1, false);
    };

    stopTheWorld("profile"); 

    // World is stopped, no locking required.
    n = 1;
    forEachGRace(gp1 => {
        if (isOK(gp1)) {
            n++;
        }
    });

    if (n <= len(p)) {
        ok = true;
        var r = p;
        var lbl = labels; 

        // Save current goroutine.
        var sp = getcallersp();
        var pc = getcallerpc();
        systemstack(() => {
            saveg(pc, sp, _addr_gp, _addr_r[0]);
        });
        r = r[(int)1..]; 

        // If we have a place to put our goroutine labelmap, insert it there.
        if (labels != null) {
            lbl[0] = gp.labels;
            lbl = lbl[(int)1..];
        }
        forEachGRace(gp1 => {
            if (!isOK(gp1)) {
                return ;
            }
            if (len(r) == 0) { 
                // Should be impossible, but better to return a
                // truncated profile than to crash the entire process.
                return ;
            }
            saveg(~uintptr(0), ~uintptr(0), _addr_gp1, _addr_r[0]);
            if (labels != null) {
                lbl[0] = gp1.labels;
                lbl = lbl[(int)1..];
            }
            r = r[(int)1..];
        });
    }
    startTheWorld();
    return (n, ok);
}

// GoroutineProfile returns n, the number of records in the active goroutine stack profile.
// If len(p) >= n, GoroutineProfile copies the profile into p and returns n, true.
// If len(p) < n, GoroutineProfile does not change p and returns n, false.
//
// Most clients should use the runtime/pprof package instead
// of calling GoroutineProfile directly.
public static (nint, bool) GoroutineProfile(slice<StackRecord> p) {
    nint n = default;
    bool ok = default;

    return goroutineProfileWithLabels(p, null);
}

private static void saveg(System.UIntPtr pc, System.UIntPtr sp, ptr<g> _addr_gp, ptr<StackRecord> _addr_r) {
    ref g gp = ref _addr_gp.val;
    ref StackRecord r = ref _addr_r.val;

    var n = gentraceback(pc, sp, 0, gp, 0, _addr_r.Stack0[0], len(r.Stack0), null, null, 0);
    if (n < len(r.Stack0)) {
        r.Stack0[n] = 0;
    }
}

// Stack formats a stack trace of the calling goroutine into buf
// and returns the number of bytes written to buf.
// If all is true, Stack formats stack traces of all other goroutines
// into buf after the trace for the current goroutine.
public static nint Stack(slice<byte> buf, bool all) {
    if (all) {
        stopTheWorld("stack trace");
    }
    nint n = 0;
    if (len(buf) > 0) {
        var gp = getg();
        var sp = getcallersp();
        var pc = getcallerpc();
        systemstack(() => {
            var g0 = getg(); 
            // Force traceback=1 to override GOTRACEBACK setting,
            // so that Stack's results are consistent.
            // GOTRACEBACK is only about crash dumps.
            g0.m.traceback = 1;
            g0.writebuf = buf.slice(0, 0, len(buf));
            goroutineheader(gp);
            traceback(pc, sp, 0, gp);
            if (all) {
                tracebackothers(gp);
            }
            g0.m.traceback = 0;
            n = len(g0.writebuf);
            g0.writebuf = null;
        });
    }
    if (all) {
        startTheWorld();
    }
    return n;
}

// Tracing of alloc/free/gc.

private static mutex tracelock = default;

private static void tracealloc(unsafe.Pointer p, System.UIntPtr size, ptr<_type> _addr_typ) {
    ref _type typ = ref _addr_typ.val;

    lock(_addr_tracelock);
    var gp = getg();
    gp.m.traceback = 2;
    if (typ == null) {
        print("tracealloc(", p, ", ", hex(size), ")\n");
    }
    else
 {
        print("tracealloc(", p, ", ", hex(size), ", ", typ.@string(), ")\n");
    }
    if (gp.m.curg == null || gp == gp.m.curg) {
        goroutineheader(gp);
        var pc = getcallerpc();
        var sp = getcallersp();
        systemstack(() => {
            traceback(pc, sp, 0, gp);
        }
    else
);
    } {
        goroutineheader(gp.m.curg);
        traceback(~uintptr(0), ~uintptr(0), 0, gp.m.curg);
    }
    print("\n");
    gp.m.traceback = 0;
    unlock(_addr_tracelock);
}

private static void tracefree(unsafe.Pointer p, System.UIntPtr size) {
    lock(_addr_tracelock);
    var gp = getg();
    gp.m.traceback = 2;
    print("tracefree(", p, ", ", hex(size), ")\n");
    goroutineheader(gp);
    var pc = getcallerpc();
    var sp = getcallersp();
    systemstack(() => {
        traceback(pc, sp, 0, gp);
    });
    print("\n");
    gp.m.traceback = 0;
    unlock(_addr_tracelock);
}

private static void tracegc() {
    lock(_addr_tracelock);
    var gp = getg();
    gp.m.traceback = 2;
    print("tracegc()\n"); 
    // running on m->g0 stack; show all non-g0 goroutines
    tracebackothers(gp);
    print("end tracegc\n");
    print("\n");
    gp.m.traceback = 0;
    unlock(_addr_tracelock);
}

} // end runtime_package
