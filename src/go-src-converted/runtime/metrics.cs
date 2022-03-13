// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:25:13 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\metrics.go
namespace go;
// Metrics implementation exported to runtime/metrics.


using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

 
// metrics is a map of runtime/metrics keys to
// data used by the runtime to sample each metric's
// value.
private static uint metricsSema = 1;private static bool metricsInit = default;private static map<@string, metricData> metrics = default;private static slice<double> sizeClassBuckets = default;private static slice<double> timeHistBuckets = default;

private partial struct metricData {
    public statDepSet deps; // compute is a function that populates a metricValue
// given a populated statAggregate structure.
    public Action<ptr<statAggregate>, ptr<metricValue>> compute;
}

// initMetrics initializes the metrics map if it hasn't been yet.
//
// metricsSema must be held.
private static void initMetrics() {
    if (metricsInit) {
        return ;
    }
    sizeClassBuckets = make_slice<double>(_NumSizeClasses, _NumSizeClasses + 1); 
    // Skip size class 0 which is a stand-in for large objects, but large
    // objects are tracked separately (and they actually get placed in
    // the last bucket, not the first).
    sizeClassBuckets[0] = 1; // The smallest allocation is 1 byte in size.
    {
        nint i__prev1 = i;

        for (nint i = 1; i < _NumSizeClasses; i++) { 
            // Size classes have an inclusive upper-bound
            // and exclusive lower bound (e.g. 48-byte size class is
            // (32, 48]) whereas we want and inclusive lower-bound
            // and exclusive upper-bound (e.g. 48-byte size class is
            // [33, 49). We can achieve this by shifting all bucket
            // boundaries up by 1.
            //
            // Also, a float64 can precisely represent integers with
            // value up to 2^53 and size classes are relatively small
            // (nowhere near 2^48 even) so this will give us exact
            // boundaries.
            sizeClassBuckets[i] = float64(class_to_size[i] + 1);
        }

        i = i__prev1;
    }
    sizeClassBuckets = append(sizeClassBuckets, float64Inf());

    timeHistBuckets = timeHistogramMetricsBuckets();
    metrics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, metricData>{"/gc/cycles/automatic:gc-cycles":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.gcCyclesDone-in.sysStats.gcCyclesForced},},"/gc/cycles/forced:gc-cycles":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.gcCyclesForced},},"/gc/cycles/total:gc-cycles":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.gcCyclesDone},},"/gc/heap/allocs-by-size:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){hist:=out.float64HistOrInit(sizeClassBuckets)hist.counts[len(hist.counts)-1]=uint64(in.heapStats.largeAllocCount)fori,count:=rangein.heapStats.smallAllocCount[1:]{hist.counts[i]=uint64(count)}},},"/gc/heap/allocs:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.totalAllocated},},"/gc/heap/allocs:objects":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.totalAllocs},},"/gc/heap/frees-by-size:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){hist:=out.float64HistOrInit(sizeClassBuckets)hist.counts[len(hist.counts)-1]=uint64(in.heapStats.largeFreeCount)fori,count:=rangein.heapStats.smallFreeCount[1:]{hist.counts[i]=uint64(count)}},},"/gc/heap/frees:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.totalFreed},},"/gc/heap/frees:objects":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.totalFrees},},"/gc/heap/goal:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.heapGoal},},"/gc/heap/objects:objects":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.numObjects},},"/gc/heap/tiny/allocs:objects":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.tinyAllocCount)},},"/gc/pauses:seconds":{compute:func(_*statAggregate,out*metricValue){hist:=out.float64HistOrInit(timeHistBuckets)hist.counts[0]=atomic.Load64(&memstats.gcPauseDist.underflow)fori:=rangememstats.gcPauseDist.counts{hist.counts[i+1]=atomic.Load64(&memstats.gcPauseDist.counts[i])}},},"/memory/classes/heap/free:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.committed-in.heapStats.inHeap-in.heapStats.inStacks-in.heapStats.inWorkBufs-in.heapStats.inPtrScalarBits)},},"/memory/classes/heap/objects:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.heapStats.inObjects},},"/memory/classes/heap/released:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.released)},},"/memory/classes/heap/stacks:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.inStacks)},},"/memory/classes/heap/unused:bytes":{deps:makeStatDepSet(heapStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.inHeap)-in.heapStats.inObjects},},"/memory/classes/metadata/mcache/free:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.mCacheSys-in.sysStats.mCacheInUse},},"/memory/classes/metadata/mcache/inuse:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.mCacheInUse},},"/memory/classes/metadata/mspan/free:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.mSpanSys-in.sysStats.mSpanInUse},},"/memory/classes/metadata/mspan/inuse:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.mSpanInUse},},"/memory/classes/metadata/other:bytes":{deps:makeStatDepSet(heapStatsDep,sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.inWorkBufs+in.heapStats.inPtrScalarBits)+in.sysStats.gcMiscSys},},"/memory/classes/os-stacks:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.stacksSys},},"/memory/classes/other:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.otherSys},},"/memory/classes/profiling/buckets:bytes":{deps:makeStatDepSet(sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=in.sysStats.buckHashSys},},"/memory/classes/total:bytes":{deps:makeStatDepSet(heapStatsDep,sysStatsDep),compute:func(in*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(in.heapStats.committed+in.heapStats.released)+in.sysStats.stacksSys+in.sysStats.mSpanSys+in.sysStats.mCacheSys+in.sysStats.buckHashSys+in.sysStats.gcMiscSys+in.sysStats.otherSys},},"/sched/goroutines:goroutines":{compute:func(_*statAggregate,out*metricValue){out.kind=metricKindUint64out.scalar=uint64(gcount())},},"/sched/latencies:seconds":{compute:func(_*statAggregate,out*metricValue){hist:=out.float64HistOrInit(timeHistBuckets)hist.counts[0]=atomic.Load64(&sched.timeToRun.underflow)fori:=rangesched.timeToRun.counts{hist.counts[i+1]=atomic.Load64(&sched.timeToRun.counts[i])}},},};
    metricsInit = true;
}

// statDep is a dependency on a group of statistics
// that a metric might have.
private partial struct statDep { // : nuint
}

private static readonly statDep heapStatsDep = iota; // corresponds to heapStatsAggregate
private static readonly var sysStatsDep = 0; // corresponds to sysStatsAggregate
private static readonly var numStatsDeps = 1;

// statDepSet represents a set of statDeps.
//
// Under the hood, it's a bitmap.
private partial struct statDepSet { // : array<ulong>
}

// makeStatDepSet creates a new statDepSet from a list of statDeps.
private static statDepSet makeStatDepSet(params statDep[] deps) {
    deps = deps.Clone();

    statDepSet s = default;
    foreach (var (_, d) in deps) {
        s[d / 64] |= 1 << (int)((d % 64));
    }    return s;
}

// differennce returns set difference of s from b as a new set.
private static statDepSet difference(this statDepSet s, statDepSet b) {
    statDepSet c = default;
    foreach (var (i) in s) {
        c[i] = s[i] & ~b[i];
    }    return c;
}

// union returns the union of the two sets as a new set.
private static statDepSet union(this statDepSet s, statDepSet b) {
    statDepSet c = default;
    foreach (var (i) in s) {
        c[i] = s[i] | b[i];
    }    return c;
}

// empty returns true if there are no dependencies in the set.
private static bool empty(this ptr<statDepSet> _addr_s) {
    ref statDepSet s = ref _addr_s.val;

    foreach (var (_, c) in s) {
        if (c != 0) {
            return false;
        }
    }    return true;
}

// has returns true if the set contains a given statDep.
private static bool has(this ptr<statDepSet> _addr_s, statDep d) {
    ref statDepSet s = ref _addr_s.val;

    return s[d / 64] & (1 << (int)((d % 64))) != 0;
}

// heapStatsAggregate represents memory stats obtained from the
// runtime. This set of stats is grouped together because they
// depend on each other in some way to make sense of the runtime's
// current heap memory use. They're also sharded across Ps, so it
// makes sense to grab them all at once.
private partial struct heapStatsAggregate {
    public ref heapStatsDelta heapStatsDelta => ref heapStatsDelta_val; // Derived from values in heapStatsDelta.

// inObjects is the bytes of memory occupied by objects,
    public ulong inObjects; // numObjects is the number of live objects in the heap.
    public ulong numObjects; // totalAllocated is the total bytes of heap objects allocated
// over the lifetime of the program.
    public ulong totalAllocated; // totalFreed is the total bytes of heap objects freed
// over the lifetime of the program.
    public ulong totalFreed; // totalAllocs is the number of heap objects allocated over
// the lifetime of the program.
    public ulong totalAllocs; // totalFrees is the number of heap objects freed over
// the lifetime of the program.
    public ulong totalFrees;
}

// compute populates the heapStatsAggregate with values from the runtime.
private static void compute(this ptr<heapStatsAggregate> _addr_a) {
    ref heapStatsAggregate a = ref _addr_a.val;

    memstats.heapStats.read(_addr_a.heapStatsDelta); 

    // Calculate derived stats.
    a.totalAllocs = uint64(a.largeAllocCount);
    a.totalFrees = uint64(a.largeFreeCount);
    a.totalAllocated = uint64(a.largeAlloc);
    a.totalFreed = uint64(a.largeFree);
    foreach (var (i) in a.smallAllocCount) {
        var na = uint64(a.smallAllocCount[i]);
        var nf = uint64(a.smallFreeCount[i]);
        a.totalAllocs += na;
        a.totalFrees += nf;
        a.totalAllocated += na * uint64(class_to_size[i]);
        a.totalFreed += nf * uint64(class_to_size[i]);
    }    a.inObjects = a.totalAllocated - a.totalFreed;
    a.numObjects = a.totalAllocs - a.totalFrees;
}

// sysStatsAggregate represents system memory stats obtained
// from the runtime. This set of stats is grouped together because
// they're all relatively cheap to acquire and generally independent
// of one another and other runtime memory stats. The fact that they
// may be acquired at different times, especially with respect to
// heapStatsAggregate, means there could be some skew, but because of
// these stats are independent, there's no real consistency issue here.
private partial struct sysStatsAggregate {
    public ulong stacksSys;
    public ulong mSpanSys;
    public ulong mSpanInUse;
    public ulong mCacheSys;
    public ulong mCacheInUse;
    public ulong buckHashSys;
    public ulong gcMiscSys;
    public ulong otherSys;
    public ulong heapGoal;
    public ulong gcCyclesDone;
    public ulong gcCyclesForced;
}

// compute populates the sysStatsAggregate with values from the runtime.
private static void compute(this ptr<sysStatsAggregate> _addr_a) {
    ref sysStatsAggregate a = ref _addr_a.val;

    a.stacksSys = memstats.stacks_sys.load();
    a.buckHashSys = memstats.buckhash_sys.load();
    a.gcMiscSys = memstats.gcMiscSys.load();
    a.otherSys = memstats.other_sys.load();
    a.heapGoal = atomic.Load64(_addr_gcController.heapGoal);
    a.gcCyclesDone = uint64(memstats.numgc);
    a.gcCyclesForced = uint64(memstats.numforcedgc);

    systemstack(() => {
        lock(_addr_mheap_.@lock);
        a.mSpanSys = memstats.mspan_sys.load();
        a.mSpanInUse = uint64(mheap_.spanalloc.inuse);
        a.mCacheSys = memstats.mcache_sys.load();
        a.mCacheInUse = uint64(mheap_.cachealloc.inuse);
        unlock(_addr_mheap_.@lock);
    });
}

// statAggregate is the main driver of the metrics implementation.
//
// It contains multiple aggregates of runtime statistics, as well
// as a set of these aggregates that it has populated. The aggergates
// are populated lazily by its ensure method.
private partial struct statAggregate {
    public statDepSet ensured;
    public heapStatsAggregate heapStats;
    public sysStatsAggregate sysStats;
}

// ensure populates statistics aggregates determined by deps if they
// haven't yet been populated.
private static void ensure(this ptr<statAggregate> _addr_a, ptr<statDepSet> _addr_deps) {
    ref statAggregate a = ref _addr_a.val;
    ref statDepSet deps = ref _addr_deps.val;

    var missing = deps.difference(a.ensured);
    if (missing.empty()) {
        return ;
    }
    for (var i = statDep(0); i < numStatsDeps; i++) {
        if (!missing.has(i)) {
            continue;
        }

        if (i == heapStatsDep) 
            a.heapStats.compute();
        else if (i == sysStatsDep) 
            a.sysStats.compute();
            }
    a.ensured = a.ensured.union(missing);
}

// metricValidKind is a runtime copy of runtime/metrics.ValueKind and
// must be kept structurally identical to that type.
private partial struct metricKind { // : nint
}

 
// These values must be kept identical to their corresponding Kind* values
// in the runtime/metrics package.
private static readonly metricKind metricKindBad = iota;
private static readonly var metricKindUint64 = 0;
private static readonly var metricKindFloat64 = 1;
private static readonly var metricKindFloat64Histogram = 2;

// metricSample is a runtime copy of runtime/metrics.Sample and
// must be kept structurally identical to that type.
private partial struct metricSample {
    public @string name;
    public metricValue value;
}

// metricValue is a runtime copy of runtime/metrics.Sample and
// must be kept structurally identical to that type.
private partial struct metricValue {
    public metricKind kind;
    public ulong scalar; // contains scalar values for scalar Kinds.
    public unsafe.Pointer pointer; // contains non-scalar values.
}

// float64HistOrInit tries to pull out an existing float64Histogram
// from the value, but if none exists, then it allocates one with
// the given buckets.
private static ptr<metricFloat64Histogram> float64HistOrInit(this ptr<metricValue> _addr_v, slice<double> buckets) {
    ref metricValue v = ref _addr_v.val;

    ptr<metricFloat64Histogram> hist;
    if (v.kind == metricKindFloat64Histogram && v.pointer != null) {
        hist = (metricFloat64Histogram.val)(v.pointer);
    }
    else
 {
        v.kind = metricKindFloat64Histogram;
        hist = @new<metricFloat64Histogram>();
        v.pointer = @unsafe.Pointer(hist);
    }
    hist.buckets = buckets;
    if (len(hist.counts) != len(hist.buckets) - 1) {
        hist.counts = make_slice<ulong>(len(buckets) - 1);
    }
    return _addr_hist!;
}

// metricFloat64Histogram is a runtime copy of runtime/metrics.Float64Histogram
// and must be kept structurally identical to that type.
private partial struct metricFloat64Histogram {
    public slice<ulong> counts;
    public slice<double> buckets;
}

// agg is used by readMetrics, and is protected by metricsSema.
//
// Managed as a global variable because its pointer will be
// an argument to a dynamically-defined function, and we'd
// like to avoid it escaping to the heap.
private static statAggregate agg = default;

// readMetrics is the implementation of runtime/metrics.Read.
//
//go:linkname readMetrics runtime/metrics.runtime_readMetrics
private static void readMetrics(unsafe.Pointer samplesp, nint len, nint cap) { 
    // Construct a slice from the args.
    ref slice sl = ref heap(new slice(samplesp,len,cap), out ptr<slice> _addr_sl);
    ptr<ptr<slice<metricSample>>> samples = new ptr<ptr<ptr<slice<metricSample>>>>(@unsafe.Pointer(_addr_sl)); 

    // Acquire the metricsSema but with handoff. This operation
    // is expensive enough that queueing up goroutines and handing
    // off between them will be noticeably better-behaved.
    semacquire1(_addr_metricsSema, true, 0, 0); 

    // Ensure the map is initialized.
    initMetrics(); 

    // Clear agg defensively.
    agg = new statAggregate(); 

    // Sample.
    foreach (var (i) in samples) {
        var sample = _addr_samples[i];
        var (data, ok) = metrics[sample.name];
        if (!ok) {
            sample.value.kind = metricKindBad;
            continue;
        }
        agg.ensure(_addr_data.deps); 

        // Compute the value based on the stats we have.
        data.compute(_addr_agg, _addr_sample.value);
    }    semrelease(_addr_metricsSema);
}

} // end runtime_package
