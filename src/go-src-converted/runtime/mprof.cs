// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Malloc profiling.
// Patterned after tcmalloc's algorithms; shorter code.

// package runtime -- go2cs converted at 2020 August 29 08:18:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mprof.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // NOTE(rsc): Everything here could use cas if contention became an issue.
        private static mutex proflock = default;

        // All memory allocations are local and do not escape outside of the profiler.
        // The profiler is forbidden from referring to garbage-collected memory.

 
        // profile types
        private static readonly bucketType memProfile = 1L + iota;
        private static readonly var blockProfile = 0;
        private static readonly buckHashSize mutexProfile = 179999L; 

        // max depth of stack to record in bucket
        private static readonly long maxStack = 32L;

        private partial struct bucketType // : long
        {
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
        private partial struct bucket
        {
            public ptr<bucket> next;
            public ptr<bucket> allnext;
            public bucketType typ; // memBucket or blockBucket (includes mutexProfile)
            public System.UIntPtr hash;
            public System.UIntPtr size;
            public System.UIntPtr nstk;
        }

        // A memRecord is the bucket data for a bucket of type memProfile,
        // part of the memory profile.
        private partial struct memRecord
        {
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
        private partial struct memRecordCycle
        {
            public System.UIntPtr allocs;
            public System.UIntPtr frees;
            public System.UIntPtr alloc_bytes;
            public System.UIntPtr free_bytes;
        }

        // add accumulates b into a. It does not zero b.
        private static void add(this ref memRecordCycle a, ref memRecordCycle b)
        {
            a.allocs += b.allocs;
            a.frees += b.frees;
            a.alloc_bytes += b.alloc_bytes;
            a.free_bytes += b.free_bytes;
        }

        // A blockRecord is the bucket data for a bucket of type blockProfile,
        // which is used in blocking and mutex profiles.
        private partial struct blockRecord
        {
            public long count;
            public long cycles;
        }

        private static ref bucket mbuckets = default;        private static ref bucket bbuckets = default;        private static ref bucket xbuckets = default;        private static ref array<ref bucket> buckhash = default;        private static System.UIntPtr bucketmem = default;        private static var mProf = default;

        private static readonly var mProfCycleWrap = uint32(len(new memRecord().future)) * (2L << (int)(24L));

        // newBucket allocates a bucket with the given type and number of stack entries.


        // newBucket allocates a bucket with the given type and number of stack entries.
        private static ref bucket newBucket(bucketType typ, long nstk)
        {
            var size = @unsafe.Sizeof(new bucket()) + uintptr(nstk) * @unsafe.Sizeof(uintptr(0L));

            if (typ == memProfile) 
                size += @unsafe.Sizeof(new memRecord());
            else if (typ == blockProfile || typ == mutexProfile) 
                size += @unsafe.Sizeof(new blockRecord());
            else 
                throw("invalid profile bucket type");
                        var b = (bucket.Value)(persistentalloc(size, 0L, ref memstats.buckhash_sys));
            bucketmem += size;
            b.typ = typ;
            b.nstk = uintptr(nstk);
            return b;
        }

        // stk returns the slice in b holding the stack.
        private static slice<System.UIntPtr> stk(this ref bucket b)
        {
            ref array<System.UIntPtr> stk = new ptr<ref array<System.UIntPtr>>(add(@unsafe.Pointer(b), @unsafe.Sizeof(b.Value)));
            return stk.slice(-1, b.nstk, b.nstk);
        }

        // mp returns the memRecord associated with the memProfile bucket b.
        private static ref memRecord mp(this ref bucket b)
        {
            if (b.typ != memProfile)
            {
                throw("bad use of bucket.mp");
            }
            var data = add(@unsafe.Pointer(b), @unsafe.Sizeof(b.Value) + b.nstk * @unsafe.Sizeof(uintptr(0L)));
            return (memRecord.Value)(data);
        }

        // bp returns the blockRecord associated with the blockProfile bucket b.
        private static ref blockRecord bp(this ref bucket b)
        {
            if (b.typ != blockProfile && b.typ != mutexProfile)
            {
                throw("bad use of bucket.bp");
            }
            var data = add(@unsafe.Pointer(b), @unsafe.Sizeof(b.Value) + b.nstk * @unsafe.Sizeof(uintptr(0L)));
            return (blockRecord.Value)(data);
        }

        // Return the bucket for stk[0:nstk], allocating new bucket if needed.
        private static ref bucket stkbucket(bucketType typ, System.UIntPtr size, slice<System.UIntPtr> stk, bool alloc)
        {
            if (buckhash == null)
            {
                buckhash = new ptr<ref array<ref bucket>>(sysAlloc(@unsafe.Sizeof(buckhash.Value), ref memstats.buckhash_sys));
                if (buckhash == null)
                {
                    throw("runtime: cannot allocate memory");
                }
            } 

            // Hash stack.
            System.UIntPtr h = default;
            foreach (var (_, pc) in stk)
            {
                h += pc;
                h += h << (int)(10L);
                h ^= h >> (int)(6L);
            } 
            // hash in size
            h += size;
            h += h << (int)(10L);
            h ^= h >> (int)(6L); 
            // finalize
            h += h << (int)(3L);
            h ^= h >> (int)(11L);

            var i = int(h % buckHashSize);
            {
                var b__prev1 = b;

                var b = buckhash[i];

                while (b != null)
                {
                    if (b.typ == typ && b.hash == h && b.size == size && eqslice(b.stk(), stk))
                    {
                        return b;
                    b = b.next;
                    }
                }


                b = b__prev1;
            }

            if (!alloc)
            {
                return null;
            } 

            // Create new bucket.
            b = newBucket(typ, len(stk));
            copy(b.stk(), stk);
            b.hash = h;
            b.size = size;
            b.next = buckhash[i];
            buckhash[i] = b;
            if (typ == memProfile)
            {
                b.allnext = mbuckets;
                mbuckets = b;
            }
            else if (typ == mutexProfile)
            {
                b.allnext = xbuckets;
                xbuckets = b;
            }
            else
            {
                b.allnext = bbuckets;
                bbuckets = b;
            }
            return b;
        }

        private static bool eqslice(slice<System.UIntPtr> x, slice<System.UIntPtr> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, xi) in x)
            {
                if (xi != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        // mProf_NextCycle publishes the next heap profile cycle and creates a
        // fresh heap profile cycle. This operation is fast and can be done
        // during STW. The caller must call mProf_Flush before calling
        // mProf_NextCycle again.
        //
        // This is called by mark termination during STW so allocations and
        // frees after the world is started again count towards a new heap
        // profiling cycle.
        private static void mProf_NextCycle()
        {
            lock(ref proflock); 
            // We explicitly wrap mProf.cycle rather than depending on
            // uint wraparound because the memRecord.future ring does not
            // itself wrap at a power of two.
            mProf.cycle = (mProf.cycle + 1L) % mProfCycleWrap;
            mProf.flushed = false;
            unlock(ref proflock);
        }

        // mProf_Flush flushes the events from the current heap profiling
        // cycle into the active profile. After this it is safe to start a new
        // heap profiling cycle with mProf_NextCycle.
        //
        // This is called by GC after mark termination starts the world. In
        // contrast with mProf_NextCycle, this is somewhat expensive, but safe
        // to do concurrently.
        private static void mProf_Flush()
        {
            lock(ref proflock);
            if (!mProf.flushed)
            {
                mProf_FlushLocked();
                mProf.flushed = true;
            }
            unlock(ref proflock);
        }

        private static void mProf_FlushLocked()
        {
            var c = mProf.cycle;
            {
                var b = mbuckets;

                while (b != null)
                {
                    var mp = b.mp(); 

                    // Flush cycle C into the published profile and clear
                    // it for reuse.
                    var mpc = ref mp.future[c % uint32(len(mp.future))];
                    mp.active.add(mpc);
                    mpc.Value = new memRecordCycle();
                    b = b.allnext;
                }

            }
        }

        // mProf_PostSweep records that all sweep frees for this GC cycle have
        // completed. This has the effect of publishing the heap profile
        // snapshot as of the last mark termination without advancing the heap
        // profile cycle.
        private static void mProf_PostSweep()
        {
            lock(ref proflock); 
            // Flush cycle C+1 to the active profile so everything as of
            // the last mark termination becomes visible. *Don't* advance
            // the cycle, since we're still accumulating allocs in cycle
            // C+2, which have to become C+1 in the next mark termination
            // and so on.
            var c = mProf.cycle;
            {
                var b = mbuckets;

                while (b != null)
                {
                    var mp = b.mp();
                    var mpc = ref mp.future[(c + 1L) % uint32(len(mp.future))];
                    mp.active.add(mpc);
                    mpc.Value = new memRecordCycle();
                    b = b.allnext;
                }

            }
            unlock(ref proflock);
        }

        // Called by malloc to record a profiled block.
        private static void mProf_Malloc(unsafe.Pointer p, System.UIntPtr size)
        {
            array<System.UIntPtr> stk = new array<System.UIntPtr>(maxStack);
            var nstk = callers(4L, stk[..]);
            lock(ref proflock);
            var b = stkbucket(memProfile, size, stk[..nstk], true);
            var c = mProf.cycle;
            var mp = b.mp();
            var mpc = ref mp.future[(c + 2L) % uint32(len(mp.future))];
            mpc.allocs++;
            mpc.alloc_bytes += size;
            unlock(ref proflock); 

            // Setprofilebucket locks a bunch of other mutexes, so we call it outside of proflock.
            // This reduces potential contention and chances of deadlocks.
            // Since the object must be alive during call to mProf_Malloc,
            // it's fine to do this non-atomically.
            systemstack(() =>
            {
                setprofilebucket(p, b);
            });
        }

        // Called when freeing a profiled block.
        private static void mProf_Free(ref bucket b, System.UIntPtr size)
        {
            lock(ref proflock);
            var c = mProf.cycle;
            var mp = b.mp();
            var mpc = ref mp.future[(c + 1L) % uint32(len(mp.future))];
            mpc.frees++;
            mpc.free_bytes += size;
            unlock(ref proflock);
        }

        private static ulong blockprofilerate = default; // in CPU ticks

        // SetBlockProfileRate controls the fraction of goroutine blocking events
        // that are reported in the blocking profile. The profiler aims to sample
        // an average of one blocking event per rate nanoseconds spent blocked.
        //
        // To include every blocking event in the profile, pass rate = 1.
        // To turn off profiling entirely, pass rate <= 0.
        public static void SetBlockProfileRate(long rate)
        {
            long r = default;
            if (rate <= 0L)
            {
                r = 0L; // disable profiling
            }
            else if (rate == 1L)
            {
                r = 1L; // profile everything
            }
            else
            { 
                // convert ns to cycles, use float64 to prevent overflow during multiplication
                r = int64(float64(rate) * float64(tickspersecond()) / (1000L * 1000L * 1000L));
                if (r == 0L)
                {
                    r = 1L;
                }
            }
            atomic.Store64(ref blockprofilerate, uint64(r));
        }

        private static void blockevent(long cycles, long skip)
        {
            if (cycles <= 0L)
            {
                cycles = 1L;
            }
            if (blocksampled(cycles))
            {
                saveblockevent(cycles, skip + 1L, blockProfile);
            }
        }

        private static bool blocksampled(long cycles)
        {
            var rate = int64(atomic.Load64(ref blockprofilerate));
            if (rate <= 0L || (rate > cycles && int64(fastrand()) % rate > cycles))
            {
                return false;
            }
            return true;
        }

        private static void saveblockevent(long cycles, long skip, bucketType which)
        {
            var gp = getg();
            long nstk = default;
            array<System.UIntPtr> stk = new array<System.UIntPtr>(maxStack);
            if (gp.m.curg == null || gp.m.curg == gp)
            {
                nstk = callers(skip, stk[..]);
            }
            else
            {
                nstk = gcallers(gp.m.curg, skip, stk[..]);
            }
            lock(ref proflock);
            var b = stkbucket(which, 0L, stk[..nstk], true);
            b.bp().count++;
            b.bp().cycles += cycles;
            unlock(ref proflock);
        }

        private static ulong mutexprofilerate = default; // fraction sampled

        // SetMutexProfileFraction controls the fraction of mutex contention events
        // that are reported in the mutex profile. On average 1/rate events are
        // reported. The previous rate is returned.
        //
        // To turn off profiling entirely, pass rate 0.
        // To just read the current rate, pass rate -1.
        // (For n>1 the details of sampling may change.)
        public static long SetMutexProfileFraction(long rate)
        {
            if (rate < 0L)
            {
                return int(mutexprofilerate);
            }
            var old = mutexprofilerate;
            atomic.Store64(ref mutexprofilerate, uint64(rate));
            return int(old);
        }

        //go:linkname mutexevent sync.event
        private static void mutexevent(long cycles, long skip)
        {
            if (cycles < 0L)
            {
                cycles = 0L;
            }
            var rate = int64(atomic.Load64(ref mutexprofilerate)); 
            // TODO(pjw): measure impact of always calling fastrand vs using something
            // like malloc.go:nextSample()
            if (rate > 0L && int64(fastrand()) % rate == 0L)
            {
                saveblockevent(cycles, skip + 1L, mutexProfile);
            }
        }

        // Go interface to profile data.

        // A StackRecord describes a single execution stack.
        public partial struct StackRecord
        {
            public array<System.UIntPtr> Stack0; // stack trace for this record; ends at first 0 entry
        }

        // Stack returns the stack trace associated with the record,
        // a prefix of r.Stack0.
        private static slice<System.UIntPtr> Stack(this ref StackRecord r)
        {
            foreach (var (i, v) in r.Stack0)
            {
                if (v == 0L)
                {
                    return r.Stack0[0L..i];
                }
            }
            return r.Stack0[0L..];
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
        public static long MemProfileRate = 512L * 1024L;

        // A MemProfileRecord describes the live objects allocated
        // by a particular call sequence (stack trace).
        public partial struct MemProfileRecord
        {
            public long AllocBytes; // number of bytes allocated, freed
            public long FreeBytes; // number of bytes allocated, freed
            public long AllocObjects; // number of objects allocated, freed
            public long FreeObjects; // number of objects allocated, freed
            public array<System.UIntPtr> Stack0; // stack trace for this record; ends at first 0 entry
        }

        // InUseBytes returns the number of bytes in use (AllocBytes - FreeBytes).
        private static long InUseBytes(this ref MemProfileRecord r)
        {
            return r.AllocBytes - r.FreeBytes;
        }

        // InUseObjects returns the number of objects in use (AllocObjects - FreeObjects).
        private static long InUseObjects(this ref MemProfileRecord r)
        {
            return r.AllocObjects - r.FreeObjects;
        }

        // Stack returns the stack trace associated with the record,
        // a prefix of r.Stack0.
        private static slice<System.UIntPtr> Stack(this ref MemProfileRecord r)
        {
            foreach (var (i, v) in r.Stack0)
            {
                if (v == 0L)
                {
                    return r.Stack0[0L..i];
                }
            }
            return r.Stack0[0L..];
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
        public static (long, bool) MemProfile(slice<MemProfileRecord> p, bool inuseZero)
        {
            lock(ref proflock); 
            // If we're between mProf_NextCycle and mProf_Flush, take care
            // of flushing to the active profile so we only have to look
            // at the active profile below.
            mProf_FlushLocked();
            var clear = true;
            {
                var b__prev1 = b;

                var b = mbuckets;

                while (b != null)
                {
                    var mp = b.mp();
                    if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes)
                    {
                        n++;
                    b = b.allnext;
                    }
                    if (mp.active.allocs != 0L || mp.active.frees != 0L)
                    {
                        clear = false;
                    }
                }


                b = b__prev1;
            }
            if (clear)
            { 
                // Absolutely no data, suggesting that a garbage collection
                // has not yet happened. In order to allow profiling when
                // garbage collection is disabled from the beginning of execution,
                // accumulate all of the cycles, and recount buckets.
                n = 0L;
                {
                    var b__prev1 = b;

                    b = mbuckets;

                    while (b != null)
                    {
                        mp = b.mp();
                        foreach (var (c) in mp.future)
                        {
                            mp.active.add(ref mp.future[c]);
                            mp.future[c] = new memRecordCycle();
                        }
                        if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes)
                        {
                            n++;
                        b = b.allnext;
                        }
                    }


                    b = b__prev1;
                }
            }
            if (n <= len(p))
            {
                ok = true;
                long idx = 0L;
                {
                    var b__prev1 = b;

                    b = mbuckets;

                    while (b != null)
                    {
                        mp = b.mp();
                        if (inuseZero || mp.active.alloc_bytes != mp.active.free_bytes)
                        {
                            record(ref p[idx], b);
                            idx++;
                        b = b.allnext;
                        }
                    }


                    b = b__prev1;
                }
            }
            unlock(ref proflock);
            return;
        }

        // Write b's data to r.
        private static void record(ref MemProfileRecord r, ref bucket b)
        {
            var mp = b.mp();
            r.AllocBytes = int64(mp.active.alloc_bytes);
            r.FreeBytes = int64(mp.active.free_bytes);
            r.AllocObjects = int64(mp.active.allocs);
            r.FreeObjects = int64(mp.active.frees);
            if (raceenabled)
            {
                racewriterangepc(@unsafe.Pointer(ref r.Stack0[0L]), @unsafe.Sizeof(r.Stack0), getcallerpc(), funcPC(MemProfile));
            }
            if (msanenabled)
            {
                msanwrite(@unsafe.Pointer(ref r.Stack0[0L]), @unsafe.Sizeof(r.Stack0));
            }
            copy(r.Stack0[..], b.stk());
            for (var i = int(b.nstk); i < len(r.Stack0); i++)
            {
                r.Stack0[i] = 0L;
            }

        }

        private static void iterate_memprof(Action<ref bucket, System.UIntPtr, ref System.UIntPtr, System.UIntPtr, System.UIntPtr, System.UIntPtr> fn)
        {
            lock(ref proflock);
            {
                var b = mbuckets;

                while (b != null)
                {
                    var mp = b.mp();
                    fn(b, b.nstk, ref b.stk()[0L], b.size, mp.active.allocs, mp.active.frees);
                    b = b.allnext;
                }

            }
            unlock(ref proflock);
        }

        // BlockProfileRecord describes blocking events originated
        // at a particular call sequence (stack trace).
        public partial struct BlockProfileRecord
        {
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
        public static (long, bool) BlockProfile(slice<BlockProfileRecord> p)
        {
            lock(ref proflock);
            {
                var b__prev1 = b;

                var b = bbuckets;

                while (b != null)
                {
                    n++;
                    b = b.allnext;
                }


                b = b__prev1;
            }
            if (n <= len(p))
            {
                ok = true;
                {
                    var b__prev1 = b;

                    b = bbuckets;

                    while (b != null)
                    {
                        var bp = b.bp();
                        var r = ref p[0L];
                        r.Count = bp.count;
                        r.Cycles = bp.cycles;
                        if (raceenabled)
                        {
                            racewriterangepc(@unsafe.Pointer(ref r.Stack0[0L]), @unsafe.Sizeof(r.Stack0), getcallerpc(), funcPC(BlockProfile));
                        b = b.allnext;
                        }
                        if (msanenabled)
                        {
                            msanwrite(@unsafe.Pointer(ref r.Stack0[0L]), @unsafe.Sizeof(r.Stack0));
                        }
                        var i = copy(r.Stack0[..], b.stk());
                        while (i < len(r.Stack0))
                        {
                            r.Stack0[i] = 0L;
                            i++;
                        }

                        p = p[1L..];
                    }


                    b = b__prev1;
                }
            }
            unlock(ref proflock);
            return;
        }

        // MutexProfile returns n, the number of records in the current mutex profile.
        // If len(p) >= n, MutexProfile copies the profile into p and returns n, true.
        // Otherwise, MutexProfile does not change p, and returns n, false.
        //
        // Most clients should use the runtime/pprof package
        // instead of calling MutexProfile directly.
        public static (long, bool) MutexProfile(slice<BlockProfileRecord> p)
        {
            lock(ref proflock);
            {
                var b__prev1 = b;

                var b = xbuckets;

                while (b != null)
                {
                    n++;
                    b = b.allnext;
                }


                b = b__prev1;
            }
            if (n <= len(p))
            {
                ok = true;
                {
                    var b__prev1 = b;

                    b = xbuckets;

                    while (b != null)
                    {
                        var bp = b.bp();
                        var r = ref p[0L];
                        r.Count = int64(bp.count);
                        r.Cycles = bp.cycles;
                        var i = copy(r.Stack0[..], b.stk());
                        while (i < len(r.Stack0))
                        {
                            r.Stack0[i] = 0L;
                            i++;
                        }

                        p = p[1L..];
                        b = b.allnext;
                    }


                    b = b__prev1;
                }
            }
            unlock(ref proflock);
            return;
        }

        // ThreadCreateProfile returns n, the number of records in the thread creation profile.
        // If len(p) >= n, ThreadCreateProfile copies the profile into p and returns n, true.
        // If len(p) < n, ThreadCreateProfile does not change p and returns n, false.
        //
        // Most clients should use the runtime/pprof package instead
        // of calling ThreadCreateProfile directly.
        public static (long, bool) ThreadCreateProfile(slice<StackRecord> p)
        {
            var first = (m.Value)(atomic.Loadp(@unsafe.Pointer(ref allm)));
            {
                var mp__prev1 = mp;

                var mp = first;

                while (mp != null)
                {
                    n++;
                    mp = mp.alllink;
                }


                mp = mp__prev1;
            }
            if (n <= len(p))
            {
                ok = true;
                long i = 0L;
                {
                    var mp__prev1 = mp;

                    mp = first;

                    while (mp != null)
                    {
                        p[i].Stack0 = mp.createstack;
                        i++;
                        mp = mp.alllink;
                    }


                    mp = mp__prev1;
                }
            }
            return;
        }

        // GoroutineProfile returns n, the number of records in the active goroutine stack profile.
        // If len(p) >= n, GoroutineProfile copies the profile into p and returns n, true.
        // If len(p) < n, GoroutineProfile does not change p and returns n, false.
        //
        // Most clients should use the runtime/pprof package instead
        // of calling GoroutineProfile directly.
        public static (long, bool) GoroutineProfile(slice<StackRecord> p)
        {
            var gp = getg();

            Func<ref g, bool> isOK = gp1 =>
            { 
                // Checking isSystemGoroutine here makes GoroutineProfile
                // consistent with both NumGoroutine and Stack.
                return gp1 != gp && readgstatus(gp1) != _Gdead && !isSystemGoroutine(gp1);
            }
;

            stopTheWorld("profile");

            n = 1L;
            {
                var gp1__prev1 = gp1;

                foreach (var (_, __gp1) in allgs)
                {
                    gp1 = __gp1;
                    if (isOK(gp1))
                    {
                        n++;
                    }
                }

                gp1 = gp1__prev1;
            }

            if (n <= len(p))
            {
                ok = true;
                var r = p; 

                // Save current goroutine.
                var sp = getcallersp(@unsafe.Pointer(ref p));
                var pc = getcallerpc();
                systemstack(() =>
                {
                    saveg(pc, sp, gp, ref r[0L]);
                });
                r = r[1L..]; 

                // Save other goroutines.
                {
                    var gp1__prev1 = gp1;

                    foreach (var (_, __gp1) in allgs)
                    {
                        gp1 = __gp1;
                        if (isOK(gp1))
                        {
                            if (len(r) == 0L)
                            { 
                                // Should be impossible, but better to return a
                                // truncated profile than to crash the entire process.
                                break;
                            }
                            saveg(~uintptr(0L), ~uintptr(0L), gp1, ref r[0L]);
                            r = r[1L..];
                        }
                    }

                    gp1 = gp1__prev1;
                }

            }
            startTheWorld();

            return (n, ok);
        }

        private static void saveg(System.UIntPtr pc, System.UIntPtr sp, ref g gp, ref StackRecord r)
        {
            var n = gentraceback(pc, sp, 0L, gp, 0L, ref r.Stack0[0L], len(r.Stack0), null, null, 0L);
            if (n < len(r.Stack0))
            {
                r.Stack0[n] = 0L;
            }
        }

        // Stack formats a stack trace of the calling goroutine into buf
        // and returns the number of bytes written to buf.
        // If all is true, Stack formats stack traces of all other goroutines
        // into buf after the trace for the current goroutine.
        public static long Stack(slice<byte> buf, bool all)
        {
            if (all)
            {
                stopTheWorld("stack trace");
            }
            long n = 0L;
            if (len(buf) > 0L)
            {
                var gp = getg();
                var sp = getcallersp(@unsafe.Pointer(ref buf));
                var pc = getcallerpc();
                systemstack(() =>
                {
                    var g0 = getg(); 
                    // Force traceback=1 to override GOTRACEBACK setting,
                    // so that Stack's results are consistent.
                    // GOTRACEBACK is only about crash dumps.
                    g0.m.traceback = 1L;
                    g0.writebuf = buf.slice(0L, 0L, len(buf));
                    goroutineheader(gp);
                    traceback(pc, sp, 0L, gp);
                    if (all)
                    {
                        tracebackothers(gp);
                    }
                    g0.m.traceback = 0L;
                    n = len(g0.writebuf);
                    g0.writebuf = null;
                });
            }
            if (all)
            {
                startTheWorld();
            }
            return n;
        }

        // Tracing of alloc/free/gc.

        private static mutex tracelock = default;

        private static void tracealloc(unsafe.Pointer p, System.UIntPtr size, ref _type typ)
        {
            lock(ref tracelock);
            var gp = getg();
            gp.m.traceback = 2L;
            if (typ == null)
            {
                print("tracealloc(", p, ", ", hex(size), ")\n");
            }
            else
            {
                print("tracealloc(", p, ", ", hex(size), ", ", typ.@string(), ")\n");
            }
            if (gp.m.curg == null || gp == gp.m.curg)
            {
                goroutineheader(gp);
                var pc = getcallerpc();
                var sp = getcallersp(@unsafe.Pointer(ref p));
                systemstack(() =>
                {
                    traceback(pc, sp, 0L, gp);
                }
            else
);
            }            {
                goroutineheader(gp.m.curg);
                traceback(~uintptr(0L), ~uintptr(0L), 0L, gp.m.curg);
            }
            print("\n");
            gp.m.traceback = 0L;
            unlock(ref tracelock);
        }

        private static void tracefree(unsafe.Pointer p, System.UIntPtr size)
        {
            lock(ref tracelock);
            var gp = getg();
            gp.m.traceback = 2L;
            print("tracefree(", p, ", ", hex(size), ")\n");
            goroutineheader(gp);
            var pc = getcallerpc();
            var sp = getcallersp(@unsafe.Pointer(ref p));
            systemstack(() =>
            {
                traceback(pc, sp, 0L, gp);
            });
            print("\n");
            gp.m.traceback = 0L;
            unlock(ref tracelock);
        }

        private static void tracegc()
        {
            lock(ref tracelock);
            var gp = getg();
            gp.m.traceback = 2L;
            print("tracegc()\n"); 
            // running on m->g0 stack; show all non-g0 goroutines
            tracebackothers(gp);
            print("end tracegc\n");
            print("\n");
            gp.m.traceback = 0L;
            unlock(ref tracelock);
        }
    }
}
