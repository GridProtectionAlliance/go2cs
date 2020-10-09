// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector (GC).
//
// The GC runs concurrently with mutator threads, is type accurate (aka precise), allows multiple
// GC thread to run in parallel. It is a concurrent mark and sweep that uses a write barrier. It is
// non-generational and non-compacting. Allocation is done using size segregated per P allocation
// areas to minimize fragmentation while eliminating locks in the common case.
//
// The algorithm decomposes into several steps.
// This is a high level description of the algorithm being used. For an overview of GC a good
// place to start is Richard Jones' gchandbook.org.
//
// The algorithm's intellectual heritage includes Dijkstra's on-the-fly algorithm, see
// Edsger W. Dijkstra, Leslie Lamport, A. J. Martin, C. S. Scholten, and E. F. M. Steffens. 1978.
// On-the-fly garbage collection: an exercise in cooperation. Commun. ACM 21, 11 (November 1978),
// 966-975.
// For journal quality proofs that these steps are complete, correct, and terminate see
// Hudson, R., and Moss, J.E.B. Copying Garbage Collection without stopping the world.
// Concurrency and Computation: Practice and Experience 15(3-5), 2003.
//
// 1. GC performs sweep termination.
//
//    a. Stop the world. This causes all Ps to reach a GC safe-point.
//
//    b. Sweep any unswept spans. There will only be unswept spans if
//    this GC cycle was forced before the expected time.
//
// 2. GC performs the mark phase.
//
//    a. Prepare for the mark phase by setting gcphase to _GCmark
//    (from _GCoff), enabling the write barrier, enabling mutator
//    assists, and enqueueing root mark jobs. No objects may be
//    scanned until all Ps have enabled the write barrier, which is
//    accomplished using STW.
//
//    b. Start the world. From this point, GC work is done by mark
//    workers started by the scheduler and by assists performed as
//    part of allocation. The write barrier shades both the
//    overwritten pointer and the new pointer value for any pointer
//    writes (see mbarrier.go for details). Newly allocated objects
//    are immediately marked black.
//
//    c. GC performs root marking jobs. This includes scanning all
//    stacks, shading all globals, and shading any heap pointers in
//    off-heap runtime data structures. Scanning a stack stops a
//    goroutine, shades any pointers found on its stack, and then
//    resumes the goroutine.
//
//    d. GC drains the work queue of grey objects, scanning each grey
//    object to black and shading all pointers found in the object
//    (which in turn may add those pointers to the work queue).
//
//    e. Because GC work is spread across local caches, GC uses a
//    distributed termination algorithm to detect when there are no
//    more root marking jobs or grey objects (see gcMarkDone). At this
//    point, GC transitions to mark termination.
//
// 3. GC performs mark termination.
//
//    a. Stop the world.
//
//    b. Set gcphase to _GCmarktermination, and disable workers and
//    assists.
//
//    c. Perform housekeeping like flushing mcaches.
//
// 4. GC performs the sweep phase.
//
//    a. Prepare for the sweep phase by setting gcphase to _GCoff,
//    setting up sweep state and disabling the write barrier.
//
//    b. Start the world. From this point on, newly allocated objects
//    are white, and allocating sweeps spans before use if necessary.
//
//    c. GC does concurrent sweeping in the background and in response
//    to allocation. See description below.
//
// 5. When sufficient allocation has taken place, replay the sequence
// starting with 1 above. See discussion of GC rate below.

// Concurrent sweep.
//
// The sweep phase proceeds concurrently with normal program execution.
// The heap is swept span-by-span both lazily (when a goroutine needs another span)
// and concurrently in a background goroutine (this helps programs that are not CPU bound).
// At the end of STW mark termination all spans are marked as "needs sweeping".
//
// The background sweeper goroutine simply sweeps spans one-by-one.
//
// To avoid requesting more OS memory while there are unswept spans, when a
// goroutine needs another span, it first attempts to reclaim that much memory
// by sweeping. When a goroutine needs to allocate a new small-object span, it
// sweeps small-object spans for the same object size until it frees at least
// one object. When a goroutine needs to allocate large-object span from heap,
// it sweeps spans until it frees at least that many pages into heap. There is
// one case where this may not suffice: if a goroutine sweeps and frees two
// nonadjacent one-page spans to the heap, it will allocate a new two-page
// span, but there can still be other one-page unswept spans which could be
// combined into a two-page span.
//
// It's critical to ensure that no operations proceed on unswept spans (that would corrupt
// mark bits in GC bitmap). During GC all mcaches are flushed into the central cache,
// so they are empty. When a goroutine grabs a new span into mcache, it sweeps it.
// When a goroutine explicitly frees an object or sets a finalizer, it ensures that
// the span is swept (either by sweeping it, or by waiting for the concurrent sweep to finish).
// The finalizer goroutine is kicked off only when all spans are swept.
// When the next GC starts, it sweeps all not-yet-swept spans (if any).

// GC rate.
// Next GC is after we've allocated an extra amount of memory proportional to
// the amount already in use. The proportion is controlled by GOGC environment variable
// (100 by default). If GOGC=100 and we're using 4M, we'll GC again when we get to 8M
// (this mark is tracked in next_gc variable). This keeps the GC cost in linear
// proportion to the allocation cost. Adjusting GOGC just changes the linear constant
// (and also the amount of extra memory used).

// Oblets
//
// In order to prevent long pauses while scanning large objects and to
// improve parallelism, the garbage collector breaks up scan jobs for
// objects larger than maxObletBytes into "oblets" of at most
// maxObletBytes. When scanning encounters the beginning of a large
// object, it scans only the first oblet and enqueues the remaining
// oblets as new scan jobs.

// package runtime -- go2cs converted at 2020 October 09 04:46:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgc.go
using cpu = go.@internal.cpu_package;
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System.Threading;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _DebugGC = (long)0L;
        private static readonly var _ConcurrentSweep = true;
        private static readonly long _FinBlockSize = (long)4L * 1024L; 

        // debugScanConservative enables debug logging for stack
        // frames that are scanned conservatively.
        private static readonly var debugScanConservative = false; 

        // sweepMinHeapDistance is a lower bound on the heap distance
        // (in bytes) reserved for concurrent sweeping between GC
        // cycles.
        private static readonly long sweepMinHeapDistance = (long)1024L * 1024L;


        // heapminimum is the minimum heap size at which to trigger GC.
        // For small heaps, this overrides the usual GOGC*live set rule.
        //
        // When there is a very small live set but a lot of allocation, simply
        // collecting when the heap reaches GOGC*live results in many GC
        // cycles and high total per-GC overhead. This minimum amortizes this
        // per-GC overhead while keeping the heap reasonably small.
        //
        // During initialization this is set to 4MB*GOGC/100. In the case of
        // GOGC==0, this will set heapminimum to 0, resulting in constant
        // collection even when the heap size is small, which is useful for
        // debugging.
        private static ulong heapminimum = defaultHeapMinimum;

        // defaultHeapMinimum is the value of heapminimum for GOGC==100.
        private static readonly long defaultHeapMinimum = (long)4L << (int)(20L);

        // Initialized from $GOGC.  GOGC=off means no GC.


        // Initialized from $GOGC.  GOGC=off means no GC.
        private static int gcpercent = default;

        private static void gcinit()
        {
            if (@unsafe.Sizeof(new workbuf()) != _WorkbufSize)
            {
                throw("size of Workbuf is suboptimal");
            } 

            // No sweep on the first cycle.
            mheap_.sweepdone = 1L; 

            // Set a reasonable initial GC trigger.
            memstats.triggerRatio = 7L / 8.0F; 

            // Fake a heap_marked value so it looks like a trigger at
            // heapminimum is the appropriate growth from heap_marked.
            // This will go into computing the initial GC goal.
            memstats.heap_marked = uint64(float64(heapminimum) / (1L + memstats.triggerRatio)); 

            // Set gcpercent from the environment. This will also compute
            // and set the GC trigger and goal.
            _ = setGCPercent(readgogc());

            work.startSema = 1L;
            work.markDoneSema = 1L;
            lockInit(_addr_work.sweepWaiters.@lock, lockRankSweepWaiters);
            lockInit(_addr_work.assistQueue.@lock, lockRankAssistQueue);
            lockInit(_addr_work.wbufSpans.@lock, lockRankWbufSpans);

        }

        private static int readgogc()
        {
            var p = gogetenv("GOGC");
            if (p == "off")
            {
                return -1L;
            }

            {
                var (n, ok) = atoi32(p);

                if (ok)
                {
                    return n;
                }

            }

            return 100L;

        }

        // gcenable is called after the bulk of the runtime initialization,
        // just before we're about to start letting user code run.
        // It kicks off the background sweeper goroutine, the background
        // scavenger goroutine, and enables GC.
        private static void gcenable()
        { 
            // Kick off sweeping and scavenging.
            var c = make_channel<long>(2L);
            go_(() => bgsweep(c));
            go_(() => bgscavenge(c));
            c.Receive().Send(c);
            memstats.enablegc = true; // now that runtime is initialized, GC is okay
        }

        //go:linkname setGCPercent runtime/debug.setGCPercent
        private static int setGCPercent(int @in)
        {
            int @out = default;
 
            // Run on the system stack since we grab the heap lock.
            systemstack(() =>
            {
                lock(_addr_mheap_.@lock);
                out = gcpercent;
                if (in < 0L)
                {
                    in = -1L;
                }

                gcpercent = in;
                heapminimum = defaultHeapMinimum * uint64(gcpercent) / 100L; 
                // Update pacing in response to gcpercent change.
                gcSetTriggerRatio(memstats.triggerRatio);
                unlock(_addr_mheap_.@lock);

            }); 

            // If we just disabled GC, wait for any concurrent GC mark to
            // finish so we always return with no GC running.
            if (in < 0L)
            {
                gcWaitOnMark(atomic.Load(_addr_work.cycles));
            }

            return out;

        }

        // Garbage collector phase.
        // Indicates to write barrier and synchronization task to perform.
        private static uint gcphase = default;

        // The compiler knows about this variable.
        // If you change it, you must change builtin/runtime.go, too.
        // If you change the first four bytes, you must also change the write
        // barrier insertion code.
        private static var writeBarrier = default;

        // gcBlackenEnabled is 1 if mutator assists and background mark
        // workers are allowed to blacken objects. This must only be set when
        // gcphase == _GCmark.
        private static uint gcBlackenEnabled = default;

        private static readonly var _GCoff = iota; // GC not running; sweeping in background, write barrier disabled
        private static readonly var _GCmark = 0; // GC marking roots and workbufs: allocate black, write barrier ENABLED
        private static readonly var _GCmarktermination = 1; // GC mark termination: allocate black, P's help GC, write barrier ENABLED

        //go:nosplit
        private static void setGCPhase(uint x)
        {
            atomic.Store(_addr_gcphase, x);
            writeBarrier.needed = gcphase == _GCmark || gcphase == _GCmarktermination;
            writeBarrier.enabled = writeBarrier.needed || writeBarrier.cgo;
        }

        // gcMarkWorkerMode represents the mode that a concurrent mark worker
        // should operate in.
        //
        // Concurrent marking happens through four different mechanisms. One
        // is mutator assists, which happen in response to allocations and are
        // not scheduled. The other three are variations in the per-P mark
        // workers and are distinguished by gcMarkWorkerMode.
        private partial struct gcMarkWorkerMode // : long
        {
        }

 
        // gcMarkWorkerDedicatedMode indicates that the P of a mark
        // worker is dedicated to running that mark worker. The mark
        // worker should run without preemption.
        private static readonly gcMarkWorkerMode gcMarkWorkerDedicatedMode = (gcMarkWorkerMode)iota; 

        // gcMarkWorkerFractionalMode indicates that a P is currently
        // running the "fractional" mark worker. The fractional worker
        // is necessary when GOMAXPROCS*gcBackgroundUtilization is not
        // an integer. The fractional worker should run until it is
        // preempted and will be scheduled to pick up the fractional
        // part of GOMAXPROCS*gcBackgroundUtilization.
        private static readonly var gcMarkWorkerFractionalMode = 0; 

        // gcMarkWorkerIdleMode indicates that a P is running the mark
        // worker because it has nothing else to do. The idle worker
        // should run until it is preempted and account its time
        // against gcController.idleMarkTime.
        private static readonly var gcMarkWorkerIdleMode = 1;


        // gcMarkWorkerModeStrings are the strings labels of gcMarkWorkerModes
        // to use in execution traces.
        private static array<@string> gcMarkWorkerModeStrings = new array<@string>(new @string[] { "GC (dedicated)", "GC (fractional)", "GC (idle)" });

        // gcController implements the GC pacing controller that determines
        // when to trigger concurrent garbage collection and how much marking
        // work to do in mutator assists and background marking.
        //
        // It uses a feedback control algorithm to adjust the memstats.gc_trigger
        // trigger based on the heap growth and GC CPU utilization each cycle.
        // This algorithm optimizes for heap growth to match GOGC and for CPU
        // utilization between assist and background marking to be 25% of
        // GOMAXPROCS. The high-level design of this algorithm is documented
        // at https://golang.org/s/go15gcpacing.
        //
        // All fields of gcController are used only during a single mark
        // cycle.
        private static gcControllerState gcController = default;

        private partial struct gcControllerState
        {
            public long scanWork; // bgScanCredit is the scan work credit accumulated by the
// concurrent background scan. This credit is accumulated by
// the background scan and stolen by mutator assists. This is
// updated atomically. Updates occur in bounded batches, since
// it is both written and read throughout the cycle.
            public long bgScanCredit; // assistTime is the nanoseconds spent in mutator assists
// during this cycle. This is updated atomically. Updates
// occur in bounded batches, since it is both written and read
// throughout the cycle.
            public long assistTime; // dedicatedMarkTime is the nanoseconds spent in dedicated
// mark workers during this cycle. This is updated atomically
// at the end of the concurrent mark phase.
            public long dedicatedMarkTime; // fractionalMarkTime is the nanoseconds spent in the
// fractional mark worker during this cycle. This is updated
// atomically throughout the cycle and will be up-to-date if
// the fractional mark worker is not currently running.
            public long fractionalMarkTime; // idleMarkTime is the nanoseconds spent in idle marking
// during this cycle. This is updated atomically throughout
// the cycle.
            public long idleMarkTime; // markStartTime is the absolute start time in nanoseconds
// that assists and background mark workers started.
            public long markStartTime; // dedicatedMarkWorkersNeeded is the number of dedicated mark
// workers that need to be started. This is computed at the
// beginning of each cycle and decremented atomically as
// dedicated mark workers get started.
            public long dedicatedMarkWorkersNeeded; // assistWorkPerByte is the ratio of scan work to allocated
// bytes that should be performed by mutator assists. This is
// computed at the beginning of each cycle and updated every
// time heap_scan is updated.
            public double assistWorkPerByte; // assistBytesPerWork is 1/assistWorkPerByte.
            public double assistBytesPerWork; // fractionalUtilizationGoal is the fraction of wall clock
// time that should be spent in the fractional mark worker on
// each P that isn't running a dedicated worker.
//
// For example, if the utilization goal is 25% and there are
// no dedicated workers, this will be 0.25. If the goal is
// 25%, there is one dedicated worker, and GOMAXPROCS is 5,
// this will be 0.05 to make up the missing 5%.
//
// If this is zero, no fractional workers are needed.
            public double fractionalUtilizationGoal;
            public cpu.CacheLinePad _;
        }

        // startCycle resets the GC controller's state and computes estimates
        // for a new GC cycle. The caller must hold worldsema.
        private static void startCycle(this ptr<gcControllerState> _addr_c)
        {
            ref gcControllerState c = ref _addr_c.val;

            c.scanWork = 0L;
            c.bgScanCredit = 0L;
            c.assistTime = 0L;
            c.dedicatedMarkTime = 0L;
            c.fractionalMarkTime = 0L;
            c.idleMarkTime = 0L; 

            // Ensure that the heap goal is at least a little larger than
            // the current live heap size. This may not be the case if GC
            // start is delayed or if the allocation that pushed heap_live
            // over gc_trigger is large or if the trigger is really close to
            // GOGC. Assist is proportional to this distance, so enforce a
            // minimum distance, even if it means going over the GOGC goal
            // by a tiny bit.
            if (memstats.next_gc < memstats.heap_live + 1024L * 1024L)
            {
                memstats.next_gc = memstats.heap_live + 1024L * 1024L;
            } 

            // Compute the background mark utilization goal. In general,
            // this may not come out exactly. We round the number of
            // dedicated workers so that the utilization is closest to
            // 25%. For small GOMAXPROCS, this would introduce too much
            // error, so we add fractional workers in that case.
            var totalUtilizationGoal = float64(gomaxprocs) * gcBackgroundUtilization;
            c.dedicatedMarkWorkersNeeded = int64(totalUtilizationGoal + 0.5F);
            var utilError = float64(c.dedicatedMarkWorkersNeeded) / totalUtilizationGoal - 1L;
            const float maxUtilError = (float)0.3F;

            if (utilError < -maxUtilError || utilError > maxUtilError)
            { 
                // Rounding put us more than 30% off our goal. With
                // gcBackgroundUtilization of 25%, this happens for
                // GOMAXPROCS<=3 or GOMAXPROCS=6. Enable fractional
                // workers to compensate.
                if (float64(c.dedicatedMarkWorkersNeeded) > totalUtilizationGoal)
                { 
                    // Too many dedicated workers.
                    c.dedicatedMarkWorkersNeeded--;

                }

                c.fractionalUtilizationGoal = (totalUtilizationGoal - float64(c.dedicatedMarkWorkersNeeded)) / float64(gomaxprocs);

            }
            else
            {
                c.fractionalUtilizationGoal = 0L;
            } 

            // In STW mode, we just want dedicated workers.
            if (debug.gcstoptheworld > 0L)
            {
                c.dedicatedMarkWorkersNeeded = int64(gomaxprocs);
                c.fractionalUtilizationGoal = 0L;
            } 

            // Clear per-P state
            foreach (var (_, p) in allp)
            {
                p.gcAssistTime = 0L;
                p.gcFractionalMarkTime = 0L;
            } 

            // Compute initial values for controls that are updated
            // throughout the cycle.
            c.revise();

            if (debug.gcpacertrace > 0L)
            {
                print("pacer: assist ratio=", c.assistWorkPerByte, " (scan ", memstats.heap_scan >> (int)(20L), " MB in ", work.initialHeapLive >> (int)(20L), "->", memstats.next_gc >> (int)(20L), " MB)", " workers=", c.dedicatedMarkWorkersNeeded, "+", c.fractionalUtilizationGoal, "\n");
            }

        }

        // revise updates the assist ratio during the GC cycle to account for
        // improved estimates. This should be called either under STW or
        // whenever memstats.heap_scan, memstats.heap_live, or
        // memstats.next_gc is updated (with mheap_.lock held).
        //
        // It should only be called when gcBlackenEnabled != 0 (because this
        // is when assists are enabled and the necessary statistics are
        // available).
        private static void revise(this ptr<gcControllerState> _addr_c)
        {
            ref gcControllerState c = ref _addr_c.val;

            var gcpercent = gcpercent;
            if (gcpercent < 0L)
            { 
                // If GC is disabled but we're running a forced GC,
                // act like GOGC is huge for the below calculations.
                gcpercent = 100000L;

            }

            var live = atomic.Load64(_addr_memstats.heap_live); 

            // Assume we're under the soft goal. Pace GC to complete at
            // next_gc assuming the heap is in steady-state.
            var heapGoal = int64(memstats.next_gc); 

            // Compute the expected scan work remaining.
            //
            // This is estimated based on the expected
            // steady-state scannable heap. For example, with
            // GOGC=100, only half of the scannable heap is
            // expected to be live, so that's what we target.
            //
            // (This is a float calculation to avoid overflowing on
            // 100*heap_scan.)
            var scanWorkExpected = int64(float64(memstats.heap_scan) * 100L / float64(100L + gcpercent));

            if (live > memstats.next_gc || c.scanWork > scanWorkExpected)
            { 
                // We're past the soft goal, or we've already done more scan
                // work than we expected. Pace GC so that in the worst case it
                // will complete by the hard goal.
                const float maxOvershoot = (float)1.1F;

                heapGoal = int64(float64(memstats.next_gc) * maxOvershoot); 

                // Compute the upper bound on the scan work remaining.
                scanWorkExpected = int64(memstats.heap_scan);

            } 

            // Compute the remaining scan work estimate.
            //
            // Note that we currently count allocations during GC as both
            // scannable heap (heap_scan) and scan work completed
            // (scanWork), so allocation will change this difference
            // slowly in the soft regime and not at all in the hard
            // regime.
            var scanWorkRemaining = scanWorkExpected - c.scanWork;
            if (scanWorkRemaining < 1000L)
            { 
                // We set a somewhat arbitrary lower bound on
                // remaining scan work since if we aim a little high,
                // we can miss by a little.
                //
                // We *do* need to enforce that this is at least 1,
                // since marking is racy and double-scanning objects
                // may legitimately make the remaining scan work
                // negative, even in the hard goal regime.
                scanWorkRemaining = 1000L;

            } 

            // Compute the heap distance remaining.
            var heapRemaining = heapGoal - int64(live);
            if (heapRemaining <= 0L)
            { 
                // This shouldn't happen, but if it does, avoid
                // dividing by zero or setting the assist negative.
                heapRemaining = 1L;

            } 

            // Compute the mutator assist ratio so by the time the mutator
            // allocates the remaining heap bytes up to next_gc, it will
            // have done (or stolen) the remaining amount of scan work.
            c.assistWorkPerByte = float64(scanWorkRemaining) / float64(heapRemaining);
            c.assistBytesPerWork = float64(heapRemaining) / float64(scanWorkRemaining);

        }

        // endCycle computes the trigger ratio for the next cycle.
        private static double endCycle(this ptr<gcControllerState> _addr_c)
        {
            ref gcControllerState c = ref _addr_c.val;

            if (work.userForced)
            { 
                // Forced GC means this cycle didn't start at the
                // trigger, so where it finished isn't good
                // information about how to adjust the trigger.
                // Just leave it where it is.
                return memstats.triggerRatio;

            } 

            // Proportional response gain for the trigger controller. Must
            // be in [0, 1]. Lower values smooth out transient effects but
            // take longer to respond to phase changes. Higher values
            // react to phase changes quickly, but are more affected by
            // transient changes. Values near 1 may be unstable.
            const float triggerGain = (float)0.5F; 

            // Compute next cycle trigger ratio. First, this computes the
            // "error" for this cycle; that is, how far off the trigger
            // was from what it should have been, accounting for both heap
            // growth and GC CPU utilization. We compute the actual heap
            // growth during this cycle and scale that by how far off from
            // the goal CPU utilization we were (to estimate the heap
            // growth if we had the desired CPU utilization). The
            // difference between this estimate and the GOGC-based goal
            // heap growth is the error.
 

            // Compute next cycle trigger ratio. First, this computes the
            // "error" for this cycle; that is, how far off the trigger
            // was from what it should have been, accounting for both heap
            // growth and GC CPU utilization. We compute the actual heap
            // growth during this cycle and scale that by how far off from
            // the goal CPU utilization we were (to estimate the heap
            // growth if we had the desired CPU utilization). The
            // difference between this estimate and the GOGC-based goal
            // heap growth is the error.
            var goalGrowthRatio = gcEffectiveGrowthRatio();
            var actualGrowthRatio = float64(memstats.heap_live) / float64(memstats.heap_marked) - 1L;
            var assistDuration = nanotime() - c.markStartTime; 

            // Assume background mark hit its utilization goal.
            var utilization = gcBackgroundUtilization; 
            // Add assist utilization; avoid divide by zero.
            if (assistDuration > 0L)
            {
                utilization += float64(c.assistTime) / float64(assistDuration * int64(gomaxprocs));
            }

            var triggerError = goalGrowthRatio - memstats.triggerRatio - utilization / gcGoalUtilization * (actualGrowthRatio - memstats.triggerRatio); 

            // Finally, we adjust the trigger for next time by this error,
            // damped by the proportional gain.
            var triggerRatio = memstats.triggerRatio + triggerGain * triggerError;

            if (debug.gcpacertrace > 0L)
            { 
                // Print controller state in terms of the design
                // document.
                var H_m_prev = memstats.heap_marked;
                var h_t = memstats.triggerRatio;
                var H_T = memstats.gc_trigger;
                var h_a = actualGrowthRatio;
                var H_a = memstats.heap_live;
                var h_g = goalGrowthRatio;
                var H_g = int64(float64(H_m_prev) * (1L + h_g));
                var u_a = utilization;
                var u_g = gcGoalUtilization;
                var W_a = c.scanWork;
                print("pacer: H_m_prev=", H_m_prev, " h_t=", h_t, " H_T=", H_T, " h_a=", h_a, " H_a=", H_a, " h_g=", h_g, " H_g=", H_g, " u_a=", u_a, " u_g=", u_g, " W_a=", W_a, " goalΔ=", goalGrowthRatio - h_t, " actualΔ=", h_a - h_t, " u_a/u_g=", u_a / u_g, "\n");

            }

            return triggerRatio;

        }

        // enlistWorker encourages another dedicated mark worker to start on
        // another P if there are spare worker slots. It is used by putfull
        // when more work is made available.
        //
        //go:nowritebarrier
        private static void enlistWorker(this ptr<gcControllerState> _addr_c)
        {
            ref gcControllerState c = ref _addr_c.val;
 
            // If there are idle Ps, wake one so it will run an idle worker.
            // NOTE: This is suspected of causing deadlocks. See golang.org/issue/19112.
            //
            //    if atomic.Load(&sched.npidle) != 0 && atomic.Load(&sched.nmspinning) == 0 {
            //        wakep()
            //        return
            //    }

            // There are no idle Ps. If we need more dedicated workers,
            // try to preempt a running P so it will switch to a worker.
            if (c.dedicatedMarkWorkersNeeded <= 0L)
            {
                return ;
            } 
            // Pick a random other P to preempt.
            if (gomaxprocs <= 1L)
            {
                return ;
            }

            var gp = getg();
            if (gp == null || gp.m == null || gp.m.p == 0L)
            {
                return ;
            }

            var myID = gp.m.p.ptr().id;
            for (long tries = 0L; tries < 5L; tries++)
            {
                var id = int32(fastrandn(uint32(gomaxprocs - 1L)));
                if (id >= myID)
                {
                    id++;
                }

                var p = allp[id];
                if (p.status != _Prunning)
                {
                    continue;
                }

                if (preemptone(p))
                {
                    return ;
                }

            }


        }

        // findRunnableGCWorker returns the background mark worker for _p_ if it
        // should be run. This must only be called when gcBlackenEnabled != 0.
        private static ptr<g> findRunnableGCWorker(this ptr<gcControllerState> _addr_c, ptr<p> _addr__p_)
        {
            ref gcControllerState c = ref _addr_c.val;
            ref p _p_ = ref _addr__p_.val;

            if (gcBlackenEnabled == 0L)
            {
                throw("gcControllerState.findRunnable: blackening not enabled");
            }

            if (_p_.gcBgMarkWorker == 0L)
            { 
                // The mark worker associated with this P is blocked
                // performing a mark transition. We can't run it
                // because it may be on some other run or wait queue.
                return _addr_null!;

            }

            if (!gcMarkWorkAvailable(_addr__p_))
            { 
                // No work to be done right now. This can happen at
                // the end of the mark phase when there are still
                // assists tapering off. Don't bother running a worker
                // now because it'll just return immediately.
                return _addr_null!;

            }

            Func<ptr<long>, bool> decIfPositive = ptr =>
            {
                if (ptr > 0L.val)
                {
                    if (atomic.Xaddint64(ptr, -1L) >= 0L)
                    {
                        return _addr_true!;
                    } 
                    // We lost a race
                    atomic.Xaddint64(ptr, +1L);

                }

                return _addr_false!;

            }
;

            if (decIfPositive(_addr_c.dedicatedMarkWorkersNeeded))
            { 
                // This P is now dedicated to marking until the end of
                // the concurrent mark phase.
                _p_.gcMarkWorkerMode = gcMarkWorkerDedicatedMode;

            }
            else if (c.fractionalUtilizationGoal == 0L)
            { 
                // No need for fractional workers.
                return _addr_null!;

            }
            else
            { 
                // Is this P behind on the fractional utilization
                // goal?
                //
                // This should be kept in sync with pollFractionalWorkerExit.
                var delta = nanotime() - gcController.markStartTime;
                if (delta > 0L && float64(_p_.gcFractionalMarkTime) / float64(delta) > c.fractionalUtilizationGoal)
                { 
                    // Nope. No need to run a fractional worker.
                    return _addr_null!;

                } 
                // Run a fractional worker.
                _p_.gcMarkWorkerMode = gcMarkWorkerFractionalMode;

            } 

            // Run the background mark worker
            var gp = _p_.gcBgMarkWorker.ptr();
            casgstatus(gp, _Gwaiting, _Grunnable);
            if (trace.enabled)
            {
                traceGoUnpark(gp, 0L);
            }

            return _addr_gp!;

        }

        // pollFractionalWorkerExit reports whether a fractional mark worker
        // should self-preempt. It assumes it is called from the fractional
        // worker.
        private static bool pollFractionalWorkerExit()
        { 
            // This should be kept in sync with the fractional worker
            // scheduler logic in findRunnableGCWorker.
            var now = nanotime();
            var delta = now - gcController.markStartTime;
            if (delta <= 0L)
            {
                return true;
            }

            var p = getg().m.p.ptr();
            var selfTime = p.gcFractionalMarkTime + (now - p.gcMarkWorkerStartTime); 
            // Add some slack to the utilization goal so that the
            // fractional worker isn't behind again the instant it exits.
            return float64(selfTime) / float64(delta) > 1.2F * gcController.fractionalUtilizationGoal;

        }

        // gcSetTriggerRatio sets the trigger ratio and updates everything
        // derived from it: the absolute trigger, the heap goal, mark pacing,
        // and sweep pacing.
        //
        // This can be called any time. If GC is the in the middle of a
        // concurrent phase, it will adjust the pacing of that phase.
        //
        // This depends on gcpercent, memstats.heap_marked, and
        // memstats.heap_live. These must be up to date.
        //
        // mheap_.lock must be held or the world must be stopped.
        private static void gcSetTriggerRatio(double triggerRatio)
        { 
            // Compute the next GC goal, which is when the allocated heap
            // has grown by GOGC/100 over the heap marked by the last
            // cycle.
            var goal = ~uint64(0L);
            if (gcpercent >= 0L)
            {
                goal = memstats.heap_marked + memstats.heap_marked * uint64(gcpercent) / 100L;
            } 

            // Set the trigger ratio, capped to reasonable bounds.
            if (gcpercent >= 0L)
            {
                var scalingFactor = float64(gcpercent) / 100L; 
                // Ensure there's always a little margin so that the
                // mutator assist ratio isn't infinity.
                float maxTriggerRatio = 0.95F * scalingFactor;
                if (triggerRatio > maxTriggerRatio)
                {
                    triggerRatio = maxTriggerRatio;
                } 

                // If we let triggerRatio go too low, then if the application
                // is allocating very rapidly we might end up in a situation
                // where we're allocating black during a nearly always-on GC.
                // The result of this is a growing heap and ultimately an
                // increase in RSS. By capping us at a point >0, we're essentially
                // saying that we're OK using more CPU during the GC to prevent
                // this growth in RSS.
                //
                // The current constant was chosen empirically: given a sufficiently
                // fast/scalable allocator with 48 Ps that could drive the trigger ratio
                // to <0.05, this constant causes applications to retain the same peak
                // RSS compared to not having this allocator.
                float minTriggerRatio = 0.6F * scalingFactor;
                if (triggerRatio < minTriggerRatio)
                {
                    triggerRatio = minTriggerRatio;
                }

            }
            else if (triggerRatio < 0L)
            { 
                // gcpercent < 0, so just make sure we're not getting a negative
                // triggerRatio. This case isn't expected to happen in practice,
                // and doesn't really matter because if gcpercent < 0 then we won't
                // ever consume triggerRatio further on in this function, but let's
                // just be defensive here; the triggerRatio being negative is almost
                // certainly undesirable.
                triggerRatio = 0L;

            }

            memstats.triggerRatio = triggerRatio; 

            // Compute the absolute GC trigger from the trigger ratio.
            //
            // We trigger the next GC cycle when the allocated heap has
            // grown by the trigger ratio over the marked heap size.
            var trigger = ~uint64(0L);
            if (gcpercent >= 0L)
            {
                trigger = uint64(float64(memstats.heap_marked) * (1L + triggerRatio)); 
                // Don't trigger below the minimum heap size.
                var minTrigger = heapminimum;
                if (!isSweepDone())
                { 
                    // Concurrent sweep happens in the heap growth
                    // from heap_live to gc_trigger, so ensure
                    // that concurrent sweep has some heap growth
                    // in which to perform sweeping before we
                    // start the next GC cycle.
                    var sweepMin = atomic.Load64(_addr_memstats.heap_live) + sweepMinHeapDistance;
                    if (sweepMin > minTrigger)
                    {
                        minTrigger = sweepMin;
                    }

                }

                if (trigger < minTrigger)
                {
                    trigger = minTrigger;
                }

                if (int64(trigger) < 0L)
                {
                    print("runtime: next_gc=", memstats.next_gc, " heap_marked=", memstats.heap_marked, " heap_live=", memstats.heap_live, " initialHeapLive=", work.initialHeapLive, "triggerRatio=", triggerRatio, " minTrigger=", minTrigger, "\n");
                    throw("gc_trigger underflow");
                }

                if (trigger > goal)
                { 
                    // The trigger ratio is always less than GOGC/100, but
                    // other bounds on the trigger may have raised it.
                    // Push up the goal, too.
                    goal = trigger;

                }

            } 

            // Commit to the trigger and goal.
            memstats.gc_trigger = trigger;
            memstats.next_gc = goal;
            if (trace.enabled)
            {
                traceNextGC();
            } 

            // Update mark pacing.
            if (gcphase != _GCoff)
            {
                gcController.revise();
            } 

            // Update sweep pacing.
            if (isSweepDone())
            {
                mheap_.sweepPagesPerByte = 0L;
            }
            else
            { 
                // Concurrent sweep needs to sweep all of the in-use
                // pages by the time the allocated heap reaches the GC
                // trigger. Compute the ratio of in-use pages to sweep
                // per byte allocated, accounting for the fact that
                // some might already be swept.
                var heapLiveBasis = atomic.Load64(_addr_memstats.heap_live);
                var heapDistance = int64(trigger) - int64(heapLiveBasis); 
                // Add a little margin so rounding errors and
                // concurrent sweep are less likely to leave pages
                // unswept when GC starts.
                heapDistance -= 1024L * 1024L;
                if (heapDistance < _PageSize)
                { 
                    // Avoid setting the sweep ratio extremely high
                    heapDistance = _PageSize;

                }

                var pagesSwept = atomic.Load64(_addr_mheap_.pagesSwept);
                var pagesInUse = atomic.Load64(_addr_mheap_.pagesInUse);
                var sweepDistancePages = int64(pagesInUse) - int64(pagesSwept);
                if (sweepDistancePages <= 0L)
                {
                    mheap_.sweepPagesPerByte = 0L;
                }
                else
                {
                    mheap_.sweepPagesPerByte = float64(sweepDistancePages) / float64(heapDistance);
                    mheap_.sweepHeapLiveBasis = heapLiveBasis; 
                    // Write pagesSweptBasis last, since this
                    // signals concurrent sweeps to recompute
                    // their debt.
                    atomic.Store64(_addr_mheap_.pagesSweptBasis, pagesSwept);

                }

            }

            gcPaceScavenger();

        }

        // gcEffectiveGrowthRatio returns the current effective heap growth
        // ratio (GOGC/100) based on heap_marked from the previous GC and
        // next_gc for the current GC.
        //
        // This may differ from gcpercent/100 because of various upper and
        // lower bounds on gcpercent. For example, if the heap is smaller than
        // heapminimum, this can be higher than gcpercent/100.
        //
        // mheap_.lock must be held or the world must be stopped.
        private static double gcEffectiveGrowthRatio()
        {
            var egogc = float64(memstats.next_gc - memstats.heap_marked) / float64(memstats.heap_marked);
            if (egogc < 0L)
            { 
                // Shouldn't happen, but just in case.
                egogc = 0L;

            }

            return egogc;

        }

        // gcGoalUtilization is the goal CPU utilization for
        // marking as a fraction of GOMAXPROCS.
        private static readonly float gcGoalUtilization = (float)0.30F;

        // gcBackgroundUtilization is the fixed CPU utilization for background
        // marking. It must be <= gcGoalUtilization. The difference between
        // gcGoalUtilization and gcBackgroundUtilization will be made up by
        // mark assists. The scheduler will aim to use within 50% of this
        // goal.
        //
        // Setting this to < gcGoalUtilization avoids saturating the trigger
        // feedback controller when there are no assists, which allows it to
        // better control CPU and heap growth. However, the larger the gap,
        // the more mutator assists are expected to happen, which impact
        // mutator latency.


        // gcBackgroundUtilization is the fixed CPU utilization for background
        // marking. It must be <= gcGoalUtilization. The difference between
        // gcGoalUtilization and gcBackgroundUtilization will be made up by
        // mark assists. The scheduler will aim to use within 50% of this
        // goal.
        //
        // Setting this to < gcGoalUtilization avoids saturating the trigger
        // feedback controller when there are no assists, which allows it to
        // better control CPU and heap growth. However, the larger the gap,
        // the more mutator assists are expected to happen, which impact
        // mutator latency.
        private static readonly float gcBackgroundUtilization = (float)0.25F;

        // gcCreditSlack is the amount of scan work credit that can
        // accumulate locally before updating gcController.scanWork and,
        // optionally, gcController.bgScanCredit. Lower values give a more
        // accurate assist ratio and make it more likely that assists will
        // successfully steal background credit. Higher values reduce memory
        // contention.


        // gcCreditSlack is the amount of scan work credit that can
        // accumulate locally before updating gcController.scanWork and,
        // optionally, gcController.bgScanCredit. Lower values give a more
        // accurate assist ratio and make it more likely that assists will
        // successfully steal background credit. Higher values reduce memory
        // contention.
        private static readonly long gcCreditSlack = (long)2000L;

        // gcAssistTimeSlack is the nanoseconds of mutator assist time that
        // can accumulate on a P before updating gcController.assistTime.


        // gcAssistTimeSlack is the nanoseconds of mutator assist time that
        // can accumulate on a P before updating gcController.assistTime.
        private static readonly long gcAssistTimeSlack = (long)5000L;

        // gcOverAssistWork determines how many extra units of scan work a GC
        // assist does when an assist happens. This amortizes the cost of an
        // assist by pre-paying for this many bytes of future allocations.


        // gcOverAssistWork determines how many extra units of scan work a GC
        // assist does when an assist happens. This amortizes the cost of an
        // assist by pre-paying for this many bytes of future allocations.
        private static readonly long gcOverAssistWork = (long)64L << (int)(10L);



        private static var work = default;

        // GC runs a garbage collection and blocks the caller until the
        // garbage collection is complete. It may also block the entire
        // program.
        public static void GC()
        { 
            // We consider a cycle to be: sweep termination, mark, mark
            // termination, and sweep. This function shouldn't return
            // until a full cycle has been completed, from beginning to
            // end. Hence, we always want to finish up the current cycle
            // and start a new one. That means:
            //
            // 1. In sweep termination, mark, or mark termination of cycle
            // N, wait until mark termination N completes and transitions
            // to sweep N.
            //
            // 2. In sweep N, help with sweep N.
            //
            // At this point we can begin a full cycle N+1.
            //
            // 3. Trigger cycle N+1 by starting sweep termination N+1.
            //
            // 4. Wait for mark termination N+1 to complete.
            //
            // 5. Help with sweep N+1 until it's done.
            //
            // This all has to be written to deal with the fact that the
            // GC may move ahead on its own. For example, when we block
            // until mark termination N, we may wake up in cycle N+2.

            // Wait until the current sweep termination, mark, and mark
            // termination complete.
            var n = atomic.Load(_addr_work.cycles);
            gcWaitOnMark(n); 

            // We're now in sweep N or later. Trigger GC cycle N+1, which
            // will first finish sweep N if necessary and then enter sweep
            // termination N+1.
            gcStart(new gcTrigger(kind:gcTriggerCycle,n:n+1)); 

            // Wait for mark termination N+1 to complete.
            gcWaitOnMark(n + 1L); 

            // Finish sweep N+1 before returning. We do this both to
            // complete the cycle and because runtime.GC() is often used
            // as part of tests and benchmarks to get the system into a
            // relatively stable and isolated state.
            while (atomic.Load(_addr_work.cycles) == n + 1L && sweepone() != ~uintptr(0L))
            {
                sweep.nbgsweep++;
                Gosched();
            } 

            // Callers may assume that the heap profile reflects the
            // just-completed cycle when this returns (historically this
            // happened because this was a STW GC), but right now the
            // profile still reflects mark termination N, not N+1.
            //
            // As soon as all of the sweep frees from cycle N+1 are done,
            // we can go ahead and publish the heap profile.
            //
            // First, wait for sweeping to finish. (We know there are no
            // more spans on the sweep queue, but we may be concurrently
            // sweeping spans, so we have to wait.)
 

            // Callers may assume that the heap profile reflects the
            // just-completed cycle when this returns (historically this
            // happened because this was a STW GC), but right now the
            // profile still reflects mark termination N, not N+1.
            //
            // As soon as all of the sweep frees from cycle N+1 are done,
            // we can go ahead and publish the heap profile.
            //
            // First, wait for sweeping to finish. (We know there are no
            // more spans on the sweep queue, but we may be concurrently
            // sweeping spans, so we have to wait.)
            while (atomic.Load(_addr_work.cycles) == n + 1L && atomic.Load(_addr_mheap_.sweepers) != 0L)
            {
                Gosched();
            } 

            // Now we're really done with sweeping, so we can publish the
            // stable heap profile. Only do this if we haven't already hit
            // another mark termination.
 

            // Now we're really done with sweeping, so we can publish the
            // stable heap profile. Only do this if we haven't already hit
            // another mark termination.
            var mp = acquirem();
            var cycle = atomic.Load(_addr_work.cycles);
            if (cycle == n + 1L || (gcphase == _GCmark && cycle == n + 2L))
            {
                mProf_PostSweep();
            }

            releasem(mp);

        }

        // gcWaitOnMark blocks until GC finishes the Nth mark phase. If GC has
        // already completed this mark phase, it returns immediately.
        private static void gcWaitOnMark(uint n)
        {
            while (true)
            { 
                // Disable phase transitions.
                lock(_addr_work.sweepWaiters.@lock);
                var nMarks = atomic.Load(_addr_work.cycles);
                if (gcphase != _GCmark)
                { 
                    // We've already completed this cycle's mark.
                    nMarks++;

                }

                if (nMarks > n)
                { 
                    // We're done.
                    unlock(_addr_work.sweepWaiters.@lock);
                    return ;

                } 

                // Wait until sweep termination, mark, and mark
                // termination of cycle N complete.
                work.sweepWaiters.list.push(getg());
                goparkunlock(_addr_work.sweepWaiters.@lock, waitReasonWaitForGCCycle, traceEvGoBlock, 1L);

            }


        }

        // gcMode indicates how concurrent a GC cycle should be.
        private partial struct gcMode // : long
        {
        }

        private static readonly gcMode gcBackgroundMode = (gcMode)iota; // concurrent GC and sweep
        private static readonly var gcForceMode = 0; // stop-the-world GC now, concurrent sweep
        private static readonly var gcForceBlockMode = 1; // stop-the-world GC now and STW sweep (forced by user)

        // A gcTrigger is a predicate for starting a GC cycle. Specifically,
        // it is an exit condition for the _GCoff phase.
        private partial struct gcTrigger
        {
            public gcTriggerKind kind;
            public long now; // gcTriggerTime: current time
            public uint n; // gcTriggerCycle: cycle number to start
        }

        private partial struct gcTriggerKind // : long
        {
        }

 
        // gcTriggerHeap indicates that a cycle should be started when
        // the heap size reaches the trigger heap size computed by the
        // controller.
        private static readonly gcTriggerKind gcTriggerHeap = (gcTriggerKind)iota; 

        // gcTriggerTime indicates that a cycle should be started when
        // it's been more than forcegcperiod nanoseconds since the
        // previous GC cycle.
        private static readonly var gcTriggerTime = 0; 

        // gcTriggerCycle indicates that a cycle should be started if
        // we have not yet started cycle number gcTrigger.n (relative
        // to work.cycles).
        private static readonly var gcTriggerCycle = 1;


        // test reports whether the trigger condition is satisfied, meaning
        // that the exit condition for the _GCoff phase has been met. The exit
        // condition should be tested when allocating.
        private static bool test(this gcTrigger t)
        {
            if (!memstats.enablegc || panicking != 0L || gcphase != _GCoff)
            {
                return false;
            }


            if (t.kind == gcTriggerHeap) 
                // Non-atomic access to heap_live for performance. If
                // we are going to trigger on this, this thread just
                // atomically wrote heap_live anyway and we'll see our
                // own write.
                return memstats.heap_live >= memstats.gc_trigger;
            else if (t.kind == gcTriggerTime) 
                if (gcpercent < 0L)
                {
                    return false;
                }

                var lastgc = int64(atomic.Load64(_addr_memstats.last_gc_nanotime));
                return lastgc != 0L && t.now - lastgc > forcegcperiod;
            else if (t.kind == gcTriggerCycle) 
                // t.n > work.cycles, but accounting for wraparound.
                return int32(t.n - work.cycles) > 0L;
                        return true;

        }

        // gcStart starts the GC. It transitions from _GCoff to _GCmark (if
        // debug.gcstoptheworld == 0) or performs all of GC (if
        // debug.gcstoptheworld != 0).
        //
        // This may return without performing this transition in some cases,
        // such as when called on a system stack or with locks held.
        private static void gcStart(gcTrigger trigger)
        { 
            // Since this is called from malloc and malloc is called in
            // the guts of a number of libraries that might be holding
            // locks, don't attempt to start GC in non-preemptible or
            // potentially unstable situations.
            var mp = acquirem();
            {
                var gp = getg();

                if (gp == mp.g0 || mp.locks > 1L || mp.preemptoff != "")
                {
                    releasem(mp);
                    return ;
                }

            }

            releasem(mp);
            mp = null; 

            // Pick up the remaining unswept/not being swept spans concurrently
            //
            // This shouldn't happen if we're being invoked in background
            // mode since proportional sweep should have just finished
            // sweeping everything, but rounding errors, etc, may leave a
            // few spans unswept. In forced mode, this is necessary since
            // GC can be forced at any point in the sweeping cycle.
            //
            // We check the transition condition continuously here in case
            // this G gets delayed in to the next GC cycle.
            while (trigger.test() && sweepone() != ~uintptr(0L))
            {
                sweep.nbgsweep++;
            } 

            // Perform GC initialization and the sweep termination
            // transition.
 

            // Perform GC initialization and the sweep termination
            // transition.
            semacquire(_addr_work.startSema); 
            // Re-check transition condition under transition lock.
            if (!trigger.test())
            {
                semrelease(_addr_work.startSema);
                return ;
            } 

            // For stats, check if this GC was forced by the user.
            work.userForced = trigger.kind == gcTriggerCycle; 

            // In gcstoptheworld debug mode, upgrade the mode accordingly.
            // We do this after re-checking the transition condition so
            // that multiple goroutines that detect the heap trigger don't
            // start multiple STW GCs.
            var mode = gcBackgroundMode;
            if (debug.gcstoptheworld == 1L)
            {
                mode = gcForceMode;
            }
            else if (debug.gcstoptheworld == 2L)
            {
                mode = gcForceBlockMode;
            } 

            // Ok, we're doing it! Stop everybody else
            semacquire(_addr_gcsema);
            semacquire(_addr_worldsema);

            if (trace.enabled)
            {
                traceGCStart();
            } 

            // Check that all Ps have finished deferred mcache flushes.
            foreach (var (_, p) in allp)
            {
                {
                    var fg = atomic.Load(_addr_p.mcache.flushGen);

                    if (fg != mheap_.sweepgen)
                    {
                        println("runtime: p", p.id, "flushGen", fg, "!= sweepgen", mheap_.sweepgen);
                        throw("p mcache not flushed");
                    }

                }

            }
            gcBgMarkStartWorkers();

            systemstack(gcResetMarkState);

            work.stwprocs = gomaxprocs;
            work.maxprocs = gomaxprocs;
            if (work.stwprocs > ncpu)
            { 
                // This is used to compute CPU time of the STW phases,
                // so it can't be more than ncpu, even if GOMAXPROCS is.
                work.stwprocs = ncpu;

            }

            work.heap0 = atomic.Load64(_addr_memstats.heap_live);
            work.pauseNS = 0L;
            work.mode = mode;

            var now = nanotime();
            work.tSweepTerm = now;
            work.pauseStart = now;
            if (trace.enabled)
            {
                traceGCSTWStart(1L);
            }

            systemstack(stopTheWorldWithSema); 
            // Finish sweep before we start concurrent scan.
            systemstack(() =>
            {
                finishsweep_m();
            }); 

            // clearpools before we start the GC. If we wait they memory will not be
            // reclaimed until the next GC cycle.
            clearpools();

            work.cycles++;

            gcController.startCycle();
            work.heapGoal = memstats.next_gc; 

            // In STW mode, disable scheduling of user Gs. This may also
            // disable scheduling of this goroutine, so it may block as
            // soon as we start the world again.
            if (mode != gcBackgroundMode)
            {
                schedEnableUser(false);
            } 

            // Enter concurrent mark phase and enable
            // write barriers.
            //
            // Because the world is stopped, all Ps will
            // observe that write barriers are enabled by
            // the time we start the world and begin
            // scanning.
            //
            // Write barriers must be enabled before assists are
            // enabled because they must be enabled before
            // any non-leaf heap objects are marked. Since
            // allocations are blocked until assists can
            // happen, we want enable assists as early as
            // possible.
            setGCPhase(_GCmark);

            gcBgMarkPrepare(); // Must happen before assist enable.
            gcMarkRootPrepare(); 

            // Mark all active tinyalloc blocks. Since we're
            // allocating from these, they need to be black like
            // other allocations. The alternative is to blacken
            // the tiny block on every allocation from it, which
            // would slow down the tiny allocator.
            gcMarkTinyAllocs(); 

            // At this point all Ps have enabled the write
            // barrier, thus maintaining the no white to
            // black invariant. Enable mutator assists to
            // put back-pressure on fast allocating
            // mutators.
            atomic.Store(_addr_gcBlackenEnabled, 1L); 

            // Assists and workers can start the moment we start
            // the world.
            gcController.markStartTime = now; 

            // In STW mode, we could block the instant systemstack
            // returns, so make sure we're not preemptible.
            mp = acquirem(); 

            // Concurrent mark.
            systemstack(() =>
            {
                now = startTheWorldWithSema(trace.enabled);
                work.pauseNS += now - work.pauseStart;
                work.tMark = now;
            }); 

            // Release the world sema before Gosched() in STW mode
            // because we will need to reacquire it later but before
            // this goroutine becomes runnable again, and we could
            // self-deadlock otherwise.
            semrelease(_addr_worldsema);
            releasem(mp); 

            // Make sure we block instead of returning to user code
            // in STW mode.
            if (mode != gcBackgroundMode)
            {
                Gosched();
            }

            semrelease(_addr_work.startSema);

        }

        // gcMarkDoneFlushed counts the number of P's with flushed work.
        //
        // Ideally this would be a captured local in gcMarkDone, but forEachP
        // escapes its callback closure, so it can't capture anything.
        //
        // This is protected by markDoneSema.
        private static uint gcMarkDoneFlushed = default;

        // debugCachedWork enables extra checks for debugging premature mark
        // termination.
        //
        // For debugging issue #27993.
        private static readonly var debugCachedWork = false;

        // gcWorkPauseGen is for debugging the mark completion algorithm.
        // gcWork put operations spin while gcWork.pauseGen == gcWorkPauseGen.
        // Only used if debugCachedWork is true.
        //
        // For debugging issue #27993.


        // gcWorkPauseGen is for debugging the mark completion algorithm.
        // gcWork put operations spin while gcWork.pauseGen == gcWorkPauseGen.
        // Only used if debugCachedWork is true.
        //
        // For debugging issue #27993.
        private static uint gcWorkPauseGen = 1L;

        // gcMarkDone transitions the GC from mark to mark termination if all
        // reachable objects have been marked (that is, there are no grey
        // objects and can be no more in the future). Otherwise, it flushes
        // all local work to the global queues where it can be discovered by
        // other workers.
        //
        // This should be called when all local mark work has been drained and
        // there are no remaining workers. Specifically, when
        //
        //   work.nwait == work.nproc && !gcMarkWorkAvailable(p)
        //
        // The calling context must be preemptible.
        //
        // Flushing local work is important because idle Ps may have local
        // work queued. This is the only way to make that work visible and
        // drive GC to completion.
        //
        // It is explicitly okay to have write barriers in this function. If
        // it does transition to mark termination, then all reachable objects
        // have been marked, so the write barrier cannot shade any more
        // objects.
        private static void gcMarkDone()
        { 
            // Ensure only one thread is running the ragged barrier at a
            // time.
            semacquire(_addr_work.markDoneSema);

top: 

            // forEachP needs worldsema to execute, and we'll need it to
            // stop the world later, so acquire worldsema now.
            if (!(gcphase == _GCmark && work.nwait == work.nproc && !gcMarkWorkAvailable(_addr_null)))
            {
                semrelease(_addr_work.markDoneSema);
                return ;
            } 

            // forEachP needs worldsema to execute, and we'll need it to
            // stop the world later, so acquire worldsema now.
            semacquire(_addr_worldsema); 

            // Flush all local buffers and collect flushedWork flags.
            gcMarkDoneFlushed = 0L;
            systemstack(() =>
            {
                var gp = getg().m.curg; 
                // Mark the user stack as preemptible so that it may be scanned.
                // Otherwise, our attempt to force all P's to a safepoint could
                // result in a deadlock as we attempt to preempt a worker that's
                // trying to preempt us (e.g. for a stack scan).
                casgstatus(gp, _Grunning, _Gwaiting);
                forEachP(_p_ =>
                { 
                    // Flush the write barrier buffer, since this may add
                    // work to the gcWork.
                    wbBufFlush1(_p_); 
                    // For debugging, shrink the write barrier
                    // buffer so it flushes immediately.
                    // wbBuf.reset will keep it at this size as
                    // long as throwOnGCWork is set.
                    if (debugCachedWork)
                    {
                        var b = _addr__p_.wbBuf;
                        b.end = uintptr(@unsafe.Pointer(_addr_b.buf[wbBufEntryPointers]));
                        b.debugGen = gcWorkPauseGen;
                    } 
                    // Flush the gcWork, since this may create global work
                    // and set the flushedWork flag.
                    //
                    // TODO(austin): Break up these workbufs to
                    // better distribute work.
                    _p_.gcw.dispose(); 
                    // Collect the flushedWork flag.
                    if (_p_.gcw.flushedWork)
                    {
                        atomic.Xadd(_addr_gcMarkDoneFlushed, 1L);
                        _p_.gcw.flushedWork = false;
                    }
                    else if (debugCachedWork)
                    { 
                        // For debugging, freeze the gcWork
                        // until we know whether we've reached
                        // completion or not. If we think
                        // we've reached completion, but
                        // there's a paused gcWork, then
                        // that's a bug.
                        _p_.gcw.pauseGen = gcWorkPauseGen; 
                        // Capture the G's stack.
                        foreach (var (i) in _p_.gcw.pauseStack)
                        {
                            _p_.gcw.pauseStack[i] = 0L;
                        }
                        callers(1L, _p_.gcw.pauseStack[..]);

                    }

                });
                casgstatus(gp, _Gwaiting, _Grunning);

            });

            if (gcMarkDoneFlushed != 0L)
            {
                if (debugCachedWork)
                { 
                    // Release paused gcWorks.
                    atomic.Xadd(_addr_gcWorkPauseGen, 1L);

                } 
                // More grey objects were discovered since the
                // previous termination check, so there may be more
                // work to do. Keep going. It's possible the
                // transition condition became true again during the
                // ragged barrier, so re-check it.
                semrelease(_addr_worldsema);
                goto top;

            }

            if (debugCachedWork)
            {
                throwOnGCWork = true; 
                // Release paused gcWorks. If there are any, they
                // should now observe throwOnGCWork and panic.
                atomic.Xadd(_addr_gcWorkPauseGen, 1L);

            } 

            // There was no global work, no local work, and no Ps
            // communicated work since we took markDoneSema. Therefore
            // there are no grey objects and no more objects can be
            // shaded. Transition to mark termination.
            var now = nanotime();
            work.tMarkTerm = now;
            work.pauseStart = now;
            getg().m.preemptoff = "gcing";
            if (trace.enabled)
            {
                traceGCSTWStart(0L);
            }

            systemstack(stopTheWorldWithSema); 
            // The gcphase is _GCmark, it will transition to _GCmarktermination
            // below. The important thing is that the wb remains active until
            // all marking is complete. This includes writes made by the GC.

            if (debugCachedWork)
            { 
                // For debugging, double check that no work was added after we
                // went around above and disable write barrier buffering.
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in allp)
                    {
                        p = __p;
                        var gcw = _addr_p.gcw;
                        if (!gcw.empty())
                        {
                            printlock();
                            print("runtime: P ", p.id, " flushedWork ", gcw.flushedWork);
                            if (gcw.wbuf1 == null)
                            {
                                print(" wbuf1=<nil>");
                            }
                            else
                            {
                                print(" wbuf1.n=", gcw.wbuf1.nobj);
                            }

                            if (gcw.wbuf2 == null)
                            {
                                print(" wbuf2=<nil>");
                            }
                            else
                            {
                                print(" wbuf2.n=", gcw.wbuf2.nobj);
                            }

                            print("\n");
                            if (gcw.pauseGen == gcw.putGen)
                            {
                                println("runtime: checkPut already failed at this generation");
                            }

                            throw("throwOnGCWork");

                        }

                    }
            else

                    p = p__prev1;
                }
            }            { 
                // For unknown reasons (see issue #27993), there is
                // sometimes work left over when we enter mark
                // termination. Detect this and resume concurrent
                // mark. This is obviously unfortunate.
                //
                // Switch to the system stack to call wbBufFlush1,
                // though in this case it doesn't matter because we're
                // non-preemptible anyway.
                var restart = false;
                systemstack(() =>
                {
                    {
                        var p__prev1 = p;

                        foreach (var (_, __p) in allp)
                        {
                            p = __p;
                            wbBufFlush1(p);
                            if (!p.gcw.empty())
                            {
                                restart = true;
                                break;
                            }

                        }

                        p = p__prev1;
                    }
                });
                if (restart)
                {
                    getg().m.preemptoff = "";
                    systemstack(() =>
                    {
                        now = startTheWorldWithSema(true);
                        work.pauseNS += now - work.pauseStart;
                    });
                    semrelease(_addr_worldsema);
                    goto top;

                }

            } 

            // Disable assists and background workers. We must do
            // this before waking blocked assists.
            atomic.Store(_addr_gcBlackenEnabled, 0L); 

            // Wake all blocked assists. These will run when we
            // start the world again.
            gcWakeAllAssists(); 

            // Likewise, release the transition lock. Blocked
            // workers and assists will run when we start the
            // world again.
            semrelease(_addr_work.markDoneSema); 

            // In STW mode, re-enable user goroutines. These will be
            // queued to run after we start the world.
            schedEnableUser(true); 

            // endCycle depends on all gcWork cache stats being flushed.
            // The termination algorithm above ensured that up to
            // allocations since the ragged barrier.
            var nextTriggerRatio = gcController.endCycle(); 

            // Perform mark termination. This will restart the world.
            gcMarkTermination(nextTriggerRatio);

        }

        private static void gcMarkTermination(double nextTriggerRatio)
        { 
            // World is stopped.
            // Start marktermination which includes enabling the write barrier.
            atomic.Store(_addr_gcBlackenEnabled, 0L);
            setGCPhase(_GCmarktermination);

            work.heap1 = memstats.heap_live;
            var startTime = nanotime();

            var mp = acquirem();
            mp.preemptoff = "gcing";
            var _g_ = getg();
            _g_.m.traceback = 2L;
            var gp = _g_.m.curg;
            casgstatus(gp, _Grunning, _Gwaiting);
            gp.waitreason = waitReasonGarbageCollection; 

            // Run gc on the g0 stack. We do this so that the g stack
            // we're currently running on will no longer change. Cuts
            // the root set down a bit (g0 stacks are not scanned, and
            // we don't need to scan gc's internal state).  We also
            // need to switch to g0 so we can shrink the stack.
            systemstack(() =>
            {
                gcMark(startTime); 
                // Must return immediately.
                // The outer function's stack may have moved
                // during gcMark (it shrinks stacks, including the
                // outer function's stack), so we must not refer
                // to any of its variables. Return back to the
                // non-system stack to pick up the new addresses
                // before continuing.
            });

            systemstack(() =>
            {
                work.heap2 = work.bytesMarked;
                if (debug.gccheckmark > 0L)
                { 
                    // Run a full non-parallel, stop-the-world
                    // mark using checkmark bits, to check that we
                    // didn't forget to mark anything during the
                    // concurrent mark process.
                    gcResetMarkState();
                    initCheckmarks();
                    var gcw = _addr_getg().m.p.ptr().gcw;
                    gcDrain(gcw, 0L);
                    wbBufFlush1(getg().m.p.ptr());
                    gcw.dispose();
                    clearCheckmarks();

                } 

                // marking is complete so we can turn the write barrier off
                setGCPhase(_GCoff);
                gcSweep(work.mode);

            });

            _g_.m.traceback = 0L;
            casgstatus(gp, _Gwaiting, _Grunning);

            if (trace.enabled)
            {
                traceGCDone();
            } 

            // all done
            mp.preemptoff = "";

            if (gcphase != _GCoff)
            {
                throw("gc done but gcphase != _GCoff");
            } 

            // Record next_gc and heap_inuse for scavenger.
            memstats.last_next_gc = memstats.next_gc;
            memstats.last_heap_inuse = memstats.heap_inuse; 

            // Update GC trigger and pacing for the next cycle.
            gcSetTriggerRatio(nextTriggerRatio); 

            // Update timing memstats
            var now = nanotime();
            var (sec, nsec, _) = time_now();
            var unixNow = sec * 1e9F + int64(nsec);
            work.pauseNS += now - work.pauseStart;
            work.tEnd = now;
            atomic.Store64(_addr_memstats.last_gc_unix, uint64(unixNow)); // must be Unix time to make sense to user
            atomic.Store64(_addr_memstats.last_gc_nanotime, uint64(now)); // monotonic time for us
            memstats.pause_ns[memstats.numgc % uint32(len(memstats.pause_ns))] = uint64(work.pauseNS);
            memstats.pause_end[memstats.numgc % uint32(len(memstats.pause_end))] = uint64(unixNow);
            memstats.pause_total_ns += uint64(work.pauseNS); 

            // Update work.totaltime.
            var sweepTermCpu = int64(work.stwprocs) * (work.tMark - work.tSweepTerm); 
            // We report idle marking time below, but omit it from the
            // overall utilization here since it's "free".
            var markCpu = gcController.assistTime + gcController.dedicatedMarkTime + gcController.fractionalMarkTime;
            var markTermCpu = int64(work.stwprocs) * (work.tEnd - work.tMarkTerm);
            var cycleCpu = sweepTermCpu + markCpu + markTermCpu;
            work.totaltime += cycleCpu; 

            // Compute overall GC CPU utilization.
            var totalCpu = sched.totaltime + (now - sched.procresizetime) * int64(gomaxprocs);
            memstats.gc_cpu_fraction = float64(work.totaltime) / float64(totalCpu); 

            // Reset sweep state.
            sweep.nbgsweep = 0L;
            sweep.npausesweep = 0L;

            if (work.userForced)
            {
                memstats.numforcedgc++;
            } 

            // Bump GC cycle count and wake goroutines waiting on sweep.
            lock(_addr_work.sweepWaiters.@lock);
            memstats.numgc++;
            injectglist(_addr_work.sweepWaiters.list);
            unlock(_addr_work.sweepWaiters.@lock); 

            // Finish the current heap profiling cycle and start a new
            // heap profiling cycle. We do this before starting the world
            // so events don't leak into the wrong cycle.
            mProf_NextCycle();

            systemstack(() =>
            {
                startTheWorldWithSema(true);
            }); 

            // Flush the heap profile so we can start a new cycle next GC.
            // This is relatively expensive, so we don't do it with the
            // world stopped.
            mProf_Flush(); 

            // Prepare workbufs for freeing by the sweeper. We do this
            // asynchronously because it can take non-trivial time.
            prepareFreeWorkbufs(); 

            // Free stack spans. This must be done between GC cycles.
            systemstack(freeStackSpans); 

            // Ensure all mcaches are flushed. Each P will flush its own
            // mcache before allocating, but idle Ps may not. Since this
            // is necessary to sweep all spans, we need to ensure all
            // mcaches are flushed before we start the next GC cycle.
            systemstack(() =>
            {
                forEachP(_p_ =>
                {
                    _p_.mcache.prepareForSweep();
                });

            }); 

            // Print gctrace before dropping worldsema. As soon as we drop
            // worldsema another cycle could start and smash the stats
            // we're trying to print.
            if (debug.gctrace > 0L)
            {
                var util = int(memstats.gc_cpu_fraction * 100L);

                array<byte> sbuf = new array<byte>(24L);
                printlock();
                print("gc ", memstats.numgc, " @", string(itoaDiv(sbuf[..], uint64(work.tSweepTerm - runtimeInitTime) / 1e6F, 3L)), "s ", util, "%: ");
                var prev = work.tSweepTerm;
                {
                    long i__prev1 = i;
                    long ns__prev1 = ns;

                    foreach (var (__i, __ns) in new slice<long>(new long[] { work.tMark, work.tMarkTerm, work.tEnd }))
                    {
                        i = __i;
                        ns = __ns;
                        if (i != 0L)
                        {
                            print("+");
                        }

                        print(string(fmtNSAsMS(sbuf[..], uint64(ns - prev))));
                        prev = ns;

                    }

                    i = i__prev1;
                    ns = ns__prev1;
                }

                print(" ms clock, ");
                {
                    long i__prev1 = i;
                    long ns__prev1 = ns;

                    foreach (var (__i, __ns) in new slice<long>(new long[] { sweepTermCpu, gcController.assistTime, gcController.dedicatedMarkTime+gcController.fractionalMarkTime, gcController.idleMarkTime, markTermCpu }))
                    {
                        i = __i;
                        ns = __ns;
                        if (i == 2L || i == 3L)
                        { 
                            // Separate mark time components with /.
                            print("/");

                        }
                        else if (i != 0L)
                        {
                            print("+");
                        }

                        print(string(fmtNSAsMS(sbuf[..], uint64(ns))));

                    }

                    i = i__prev1;
                    ns = ns__prev1;
                }

                print(" ms cpu, ", work.heap0 >> (int)(20L), "->", work.heap1 >> (int)(20L), "->", work.heap2 >> (int)(20L), " MB, ", work.heapGoal >> (int)(20L), " MB goal, ", work.maxprocs, " P");
                if (work.userForced)
                {
                    print(" (forced)");
                }

                print("\n");
                printunlock();

            }

            semrelease(_addr_worldsema);
            semrelease(_addr_gcsema); 
            // Careful: another GC cycle may start now.

            releasem(mp);
            mp = null; 

            // now that gc is done, kick off finalizer thread if needed
            if (!concurrentSweep)
            { 
                // give the queued finalizers, if any, a chance to run
                Gosched();

            }

        }

        // gcBgMarkStartWorkers prepares background mark worker goroutines.
        // These goroutines will not run until the mark phase, but they must
        // be started while the work is not stopped and from a regular G
        // stack. The caller must hold worldsema.
        private static void gcBgMarkStartWorkers()
        { 
            // Background marking is performed by per-P G's. Ensure that
            // each P has a background GC G.
            foreach (var (_, p) in allp)
            {
                if (p.gcBgMarkWorker == 0L)
                {
                    go_(() => gcBgMarkWorker(_addr_p));
                    notetsleepg(_addr_work.bgMarkReady, -1L);
                    noteclear(_addr_work.bgMarkReady);
                }

            }

        }

        // gcBgMarkPrepare sets up state for background marking.
        // Mutator assists must not yet be enabled.
        private static void gcBgMarkPrepare()
        { 
            // Background marking will stop when the work queues are empty
            // and there are no more workers (note that, since this is
            // concurrent, this may be a transient state, but mark
            // termination will clean it up). Between background workers
            // and assists, we don't really know how many workers there
            // will be, so we pretend to have an arbitrarily large number
            // of workers, almost all of which are "waiting". While a
            // worker is working it decrements nwait. If nproc == nwait,
            // there are no workers.
            work.nproc = ~uint32(0L);
            work.nwait = ~uint32(0L);

        }

        private static void gcBgMarkWorker(ptr<p> _addr__p_)
        {
            ref p _p_ = ref _addr__p_.val;

            var gp = getg();

            private partial struct parkInfo
            {
                public muintptr m; // Release this m on park.
                public puintptr attach; // If non-nil, attach to this p on park.
            } 
            // We pass park to a gopark unlock function, so it can't be on
            // the stack (see gopark). Prevent deadlock from recursively
            // starting GC by disabling preemption.
            gp.m.preemptoff = "GC worker init";
            ptr<parkInfo> park = @new<parkInfo>();
            gp.m.preemptoff = "";

            park.m.set(acquirem());
            park.attach.set(_p_); 
            // Inform gcBgMarkStartWorkers that this worker is ready.
            // After this point, the background mark worker is scheduled
            // cooperatively by gcController.findRunnable. Hence, it must
            // never be preempted, as this would put it into _Grunnable
            // and put it on a run queue. Instead, when the preempt flag
            // is set, this puts itself into _Gwaiting to be woken up by
            // gcController.findRunnable at the appropriate time.
            notewakeup(_addr_work.bgMarkReady);

            while (true)
            { 
                // Go to sleep until woken by gcController.findRunnable.
                // We can't releasem yet since even the call to gopark
                // may be preempted.
                gopark((g, parkp) =>
                {
                    park = (parkInfo.val)(parkp); 

                    // The worker G is no longer running, so it's
                    // now safe to allow preemption.
                    releasem(park.m.ptr()); 

                    // If the worker isn't attached to its P,
                    // attach now. During initialization and after
                    // a phase change, the worker may have been
                    // running on a different P. As soon as we
                    // attach, the owner P may schedule the
                    // worker, so this must be done after the G is
                    // stopped.
                    if (park.attach != 0L)
                    {
                        var p = park.attach.ptr();
                        park.attach.set(null); 
                        // cas the worker because we may be
                        // racing with a new worker starting
                        // on this P.
                        if (!p.gcBgMarkWorker.cas(0L, guintptr(@unsafe.Pointer(g))))
                        { 
                            // The P got a new worker.
                            // Exit this worker.
                            return false;

                        }

                    }

                    return true;

                }, @unsafe.Pointer(park), waitReasonGCWorkerIdle, traceEvGoBlock, 0L); 

                // Loop until the P dies and disassociates this
                // worker (the P may later be reused, in which case
                // it will get a new worker) or we failed to associate.
                if (_p_.gcBgMarkWorker.ptr() != gp)
                {
                    break;
                } 

                // Disable preemption so we can use the gcw. If the
                // scheduler wants to preempt us, we'll stop draining,
                // dispose the gcw, and then preempt.
                park.m.set(acquirem());

                if (gcBlackenEnabled == 0L)
                {
                    throw("gcBgMarkWorker: blackening not enabled");
                }

                var startTime = nanotime();
                _p_.gcMarkWorkerStartTime = startTime;

                var decnwait = atomic.Xadd(_addr_work.nwait, -1L);
                if (decnwait == work.nproc)
                {
                    println("runtime: work.nwait=", decnwait, "work.nproc=", work.nproc);
                    throw("work.nwait was > work.nproc");
                }

                systemstack(() =>
                { 
                    // Mark our goroutine preemptible so its stack
                    // can be scanned. This lets two mark workers
                    // scan each other (otherwise, they would
                    // deadlock). We must not modify anything on
                    // the G stack. However, stack shrinking is
                    // disabled for mark workers, so it is safe to
                    // read from the G stack.
                    casgstatus(gp, _Grunning, _Gwaiting);

                    if (_p_.gcMarkWorkerMode == gcMarkWorkerDedicatedMode) 
                        gcDrain(_addr__p_.gcw, gcDrainUntilPreempt | gcDrainFlushBgCredit);
                        if (gp.preempt)
                        { 
                            // We were preempted. This is
                            // a useful signal to kick
                            // everything out of the run
                            // queue so it can run
                            // somewhere else.
                            lock(_addr_sched.@lock);
                            while (true)
                            {
                                var (gp, _) = runqget(_p_);
                                if (gp == null)
                                {
                                    break;
                                }

                                globrunqput(gp);

                            }

                            unlock(_addr_sched.@lock);

                        } 
                        // Go back to draining, this time
                        // without preemption.
                        gcDrain(_addr__p_.gcw, gcDrainFlushBgCredit);
                    else if (_p_.gcMarkWorkerMode == gcMarkWorkerFractionalMode) 
                        gcDrain(_addr__p_.gcw, gcDrainFractional | gcDrainUntilPreempt | gcDrainFlushBgCredit);
                    else if (_p_.gcMarkWorkerMode == gcMarkWorkerIdleMode) 
                        gcDrain(_addr__p_.gcw, gcDrainIdle | gcDrainUntilPreempt | gcDrainFlushBgCredit);
                    else 
                        throw("gcBgMarkWorker: unexpected gcMarkWorkerMode");
                                        casgstatus(gp, _Gwaiting, _Grunning);

                }); 

                // Account for time.
                var duration = nanotime() - startTime;

                if (_p_.gcMarkWorkerMode == gcMarkWorkerDedicatedMode) 
                    atomic.Xaddint64(_addr_gcController.dedicatedMarkTime, duration);
                    atomic.Xaddint64(_addr_gcController.dedicatedMarkWorkersNeeded, 1L);
                else if (_p_.gcMarkWorkerMode == gcMarkWorkerFractionalMode) 
                    atomic.Xaddint64(_addr_gcController.fractionalMarkTime, duration);
                    atomic.Xaddint64(_addr__p_.gcFractionalMarkTime, duration);
                else if (_p_.gcMarkWorkerMode == gcMarkWorkerIdleMode) 
                    atomic.Xaddint64(_addr_gcController.idleMarkTime, duration);
                // Was this the last worker and did we run out
                // of work?
                var incnwait = atomic.Xadd(_addr_work.nwait, +1L);
                if (incnwait > work.nproc)
                {
                    println("runtime: p.gcMarkWorkerMode=", _p_.gcMarkWorkerMode, "work.nwait=", incnwait, "work.nproc=", work.nproc);
                    throw("work.nwait > work.nproc");
                } 

                // If this worker reached a background mark completion
                // point, signal the main GC goroutine.
                if (incnwait == work.nproc && !gcMarkWorkAvailable(_addr_null))
                { 
                    // Make this G preemptible and disassociate it
                    // as the worker for this P so
                    // findRunnableGCWorker doesn't try to
                    // schedule it.
                    _p_.gcBgMarkWorker.set(null);
                    releasem(park.m.ptr());

                    gcMarkDone(); 

                    // Disable preemption and prepare to reattach
                    // to the P.
                    //
                    // We may be running on a different P at this
                    // point, so we can't reattach until this G is
                    // parked.
                    park.m.set(acquirem());
                    park.attach.set(_p_);

                }

            }


        }

        // gcMarkWorkAvailable reports whether executing a mark worker
        // on p is potentially useful. p may be nil, in which case it only
        // checks the global sources of work.
        private static bool gcMarkWorkAvailable(ptr<p> _addr_p)
        {
            ref p p = ref _addr_p.val;

            if (p != null && !p.gcw.empty())
            {
                return true;
            }

            if (!work.full.empty())
            {
                return true; // global work available
            }

            if (work.markrootNext < work.markrootJobs)
            {
                return true; // root scan work available
            }

            return false;

        }

        // gcMark runs the mark (or, for concurrent GC, mark termination)
        // All gcWork caches must be empty.
        // STW is in effect at this point.
        private static void gcMark(long start_time) => func((_, panic, __) =>
        {
            if (debug.allocfreetrace > 0L)
            {
                tracegc();
            }

            if (gcphase != _GCmarktermination)
            {
                throw("in gcMark expecting to see gcphase as _GCmarktermination");
            }

            work.tstart = start_time; 

            // Check that there's no marking work remaining.
            if (work.full != 0L || work.markrootNext < work.markrootJobs)
            {
                print("runtime: full=", hex(work.full), " next=", work.markrootNext, " jobs=", work.markrootJobs, " nDataRoots=", work.nDataRoots, " nBSSRoots=", work.nBSSRoots, " nSpanRoots=", work.nSpanRoots, " nStackRoots=", work.nStackRoots, "\n");
                panic("non-empty mark queue after concurrent mark");
            }

            if (debug.gccheckmark > 0L)
            { 
                // This is expensive when there's a large number of
                // Gs, so only do it if checkmark is also enabled.
                gcMarkRootCheck();

            }

            if (work.full != 0L)
            {
                throw("work.full != 0");
            } 

            // Clear out buffers and double-check that all gcWork caches
            // are empty. This should be ensured by gcMarkDone before we
            // enter mark termination.
            //
            // TODO: We could clear out buffers just before mark if this
            // has a non-negligible impact on STW time.
            foreach (var (_, p) in allp)
            { 
                // The write barrier may have buffered pointers since
                // the gcMarkDone barrier. However, since the barrier
                // ensured all reachable objects were marked, all of
                // these must be pointers to black objects. Hence we
                // can just discard the write barrier buffer.
                if (debug.gccheckmark > 0L || throwOnGCWork)
                { 
                    // For debugging, flush the buffer and make
                    // sure it really was all marked.
                    wbBufFlush1(p);

                }
                else
                {
                    p.wbBuf.reset();
                }

                var gcw = _addr_p.gcw;
                if (!gcw.empty())
                {
                    printlock();
                    print("runtime: P ", p.id, " flushedWork ", gcw.flushedWork);
                    if (gcw.wbuf1 == null)
                    {
                        print(" wbuf1=<nil>");
                    }
                    else
                    {
                        print(" wbuf1.n=", gcw.wbuf1.nobj);
                    }

                    if (gcw.wbuf2 == null)
                    {
                        print(" wbuf2=<nil>");
                    }
                    else
                    {
                        print(" wbuf2.n=", gcw.wbuf2.nobj);
                    }

                    print("\n");
                    throw("P has cached GC work at end of mark termination");

                } 
                // There may still be cached empty buffers, which we
                // need to flush since we're going to free them. Also,
                // there may be non-zero stats because we allocated
                // black after the gcMarkDone barrier.
                gcw.dispose();

            }
            throwOnGCWork = false;

            cachestats(); 

            // Update the marked heap stat.
            memstats.heap_marked = work.bytesMarked; 

            // Update other GC heap size stats. This must happen after
            // cachestats (which flushes local statistics to these) and
            // flushallmcaches (which modifies heap_live).
            memstats.heap_live = work.bytesMarked;
            memstats.heap_scan = uint64(gcController.scanWork);

            if (trace.enabled)
            {
                traceHeapAlloc();
            }

        });

        // gcSweep must be called on the system stack because it acquires the heap
        // lock. See mheap for details.
        //
        // The world must be stopped.
        //
        //go:systemstack
        private static void gcSweep(gcMode mode)
        {
            if (gcphase != _GCoff)
            {
                throw("gcSweep being done but phase is not GCoff");
            }

            lock(_addr_mheap_.@lock);
            mheap_.sweepgen += 2L;
            mheap_.sweepdone = 0L;
            if (!go115NewMCentralImpl && mheap_.sweepSpans[mheap_.sweepgen / 2L % 2L].index != 0L)
            { 
                // We should have drained this list during the last
                // sweep phase. We certainly need to start this phase
                // with an empty swept list.
                throw("non-empty swept list");

            }

            mheap_.pagesSwept = 0L;
            mheap_.sweepArenas = mheap_.allArenas;
            mheap_.reclaimIndex = 0L;
            mheap_.reclaimCredit = 0L;
            unlock(_addr_mheap_.@lock);

            if (go115NewMCentralImpl)
            {
                sweep.centralIndex.clear();
            }

            if (!_ConcurrentSweep || mode == gcForceBlockMode)
            { 
                // Special case synchronous sweep.
                // Record that no proportional sweeping has to happen.
                lock(_addr_mheap_.@lock);
                mheap_.sweepPagesPerByte = 0L;
                unlock(_addr_mheap_.@lock); 
                // Sweep all spans eagerly.
                while (sweepone() != ~uintptr(0L))
                {
                    sweep.npausesweep++;
                } 
                // Free workbufs eagerly.
 
                // Free workbufs eagerly.
                prepareFreeWorkbufs();
                while (freeSomeWbufs(false))
                {
                } 
                // All "free" events for this mark/sweep cycle have
                // now happened, so we can make this profile cycle
                // available immediately.
 
                // All "free" events for this mark/sweep cycle have
                // now happened, so we can make this profile cycle
                // available immediately.
                mProf_NextCycle();
                mProf_Flush();
                return ;

            } 

            // Background sweep.
            lock(_addr_sweep.@lock);
            if (sweep.parked)
            {
                sweep.parked = false;
                ready(sweep.g, 0L, true);
            }

            unlock(_addr_sweep.@lock);

        }

        // gcResetMarkState resets global state prior to marking (concurrent
        // or STW) and resets the stack scan state of all Gs.
        //
        // This is safe to do without the world stopped because any Gs created
        // during or after this will start out in the reset state.
        //
        // gcResetMarkState must be called on the system stack because it acquires
        // the heap lock. See mheap for details.
        //
        //go:systemstack
        private static void gcResetMarkState()
        { 
            // This may be called during a concurrent phase, so make sure
            // allgs doesn't change.
            lock(_addr_allglock);
            foreach (var (_, gp) in allgs)
            {
                gp.gcscandone = false; // set to true in gcphasework
                gp.gcAssistBytes = 0L;

            }
            unlock(_addr_allglock); 

            // Clear page marks. This is just 1MB per 64GB of heap, so the
            // time here is pretty trivial.
            lock(_addr_mheap_.@lock);
            var arenas = mheap_.allArenas;
            unlock(_addr_mheap_.@lock);
            foreach (var (_, ai) in arenas)
            {
                var ha = mheap_.arenas[ai.l1()][ai.l2()];
                foreach (var (i) in ha.pageMarks)
                {
                    ha.pageMarks[i] = 0L;
                }

            }
            work.bytesMarked = 0L;
            work.initialHeapLive = atomic.Load64(_addr_memstats.heap_live);

        }

        // Hooks for other packages

        private static Action poolcleanup = default;

        //go:linkname sync_runtime_registerPoolCleanup sync.runtime_registerPoolCleanup
        private static void sync_runtime_registerPoolCleanup(Action f)
        {
            poolcleanup = f;
        }

        private static void clearpools()
        { 
            // clear sync.Pools
            if (poolcleanup != null)
            {
                poolcleanup();
            } 

            // Clear central sudog cache.
            // Leave per-P caches alone, they have strictly bounded size.
            // Disconnect cached list before dropping it on the floor,
            // so that a dangling ref to one entry does not pin all of them.
            lock(_addr_sched.sudoglock);
            ptr<sudog> sg;            ptr<sudog> sgnext;

            sg = sched.sudogcache;

            while (sg != null)
            {
                sgnext = sg.next;
                sg.next = null;
                sg = addr(sgnext);
            }

            sched.sudogcache = null;
            unlock(_addr_sched.sudoglock); 

            // Clear central defer pools.
            // Leave per-P pools alone, they have strictly bounded size.
            lock(_addr_sched.deferlock);
            foreach (var (i) in sched.deferpool)
            { 
                // disconnect cached list before dropping it on the floor,
                // so that a dangling ref to one entry does not pin all of them.
                ptr<_defer> d;                ptr<_defer> dlink;

                d = sched.deferpool[i];

                while (d != null)
                {
                    dlink = d.link;
                    d.link = null;
                    d = addr(dlink);
                }

                sched.deferpool[i] = null;

            }
            unlock(_addr_sched.deferlock);

        }

        // Timing

        // itoaDiv formats val/(10**dec) into buf.
        private static slice<byte> itoaDiv(slice<byte> buf, ulong val, long dec)
        {
            var i = len(buf) - 1L;
            var idec = i - dec;
            while (val >= 10L || i >= idec)
            {
                buf[i] = byte(val % 10L + '0');
                i--;
                if (i == idec)
                {
                    buf[i] = '.';
                    i--;
                }

                val /= 10L;

            }

            buf[i] = byte(val + '0');
            return buf[i..];

        }

        // fmtNSAsMS nicely formats ns nanoseconds as milliseconds.
        private static slice<byte> fmtNSAsMS(slice<byte> buf, ulong ns)
        {
            if (ns >= 10e6F)
            { 
                // Format as whole milliseconds.
                return itoaDiv(buf, ns / 1e6F, 0L);

            } 
            // Format two digits of precision, with at most three decimal places.
            var x = ns / 1e3F;
            if (x == 0L)
            {
                buf[0L] = '0';
                return buf[..1L];
            }

            long dec = 3L;
            while (x >= 100L)
            {
                x /= 10L;
                dec--;
            }

            return itoaDiv(buf, x, dec);

        }
    }
}
