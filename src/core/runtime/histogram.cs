// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static readonly UntypedInt timeHistMinBucketBits = 9;
internal static readonly UntypedInt timeHistMaxBucketBits = 48; // Note that this is exclusive; 1 higher than the actual range.
internal static readonly UntypedInt timeHistSubBucketBits = 2;
internal static readonly UntypedInt timeHistNumSubBuckets = /* 1 << timeHistSubBucketBits */ 4;
internal static readonly UntypedInt timeHistNumBuckets = /* timeHistMaxBucketBits - timeHistMinBucketBits + 1 */ 40;
internal static readonly UntypedInt timeHistTotalBuckets = /* timeHistNumBuckets*timeHistNumSubBuckets + 2 */ 162;

// timeHistogram represents a distribution of durations in
// nanoseconds.
//
// The accuracy and range of the histogram is defined by the
// timeHistSubBucketBits and timeHistNumBuckets constants.
//
// It is an HDR histogram with exponentially-distributed
// buckets and linearly distributed sub-buckets.
//
// The histogram is safe for concurrent reads and writes.
[GoType] partial struct timeHistogram {
    internal atomic.Uint64 counts = new(timeHistNumBuckets * timeHistNumSubBuckets);
    // underflow counts all the times we got a negative duration
    // sample. Because of how time works on some platforms, it's
    // possible to measure negative durations. We could ignore them,
    // but we record them anyway because it's better to have some
    // signal that it's happening than just missing samples.
    internal @internal.runtime.atomic_package.Uint64 underflow;
    // overflow counts all the times we got a duration that exceeded
    // the range counts represents.
    internal @internal.runtime.atomic_package.Uint64 overflow;
}

// record adds the given duration to the distribution.
//
// Disallow preemptions and stack growths because this function
// may run in sensitive locations.
//
//go:nosplit
[GoRecv] internal static void record(this ref timeHistogram h, int64 duration) {
    // If the duration is negative, capture that in underflow.
    if (duration < 0) {
        h.underflow.Add(1);
        return;
    }
    // bucketBit is the target bit for the bucket which is usually the
    // highest 1 bit, but if we're less than the minimum, is the highest
    // 1 bit of the minimum (which will be zero in the duration).
    //
    // bucket is the bucket index, which is the bucketBit minus the
    // highest bit of the minimum, plus one to leave room for the catch-all
    // bucket for samples lower than the minimum.
    nuint bucketBit = default!;
    nuint bucket = default!;
    {
        nint l = sys.Len64(((uint64)duration)); if (l < timeHistMinBucketBits){
            bucketBit = timeHistMinBucketBits;
            bucket = 0;
        } else {
            // bucketBit - timeHistMinBucketBits
            bucketBit = ((nuint)l);
            bucket = bucketBit - timeHistMinBucketBits + 1;
        }
    }
    // If the bucket we computed is greater than the number of buckets,
    // count that in overflow.
    if (bucket >= timeHistNumBuckets) {
        h.overflow.Add(1);
        return;
    }
    // The sub-bucket index is just next timeHistSubBucketBits after the bucketBit.
    nuint subBucket = ((nuint)(duration >> (int)((bucketBit - 1 - timeHistSubBucketBits)))) % timeHistNumSubBuckets;
    h.counts[bucket * timeHistNumSubBuckets + subBucket].Add(1);
}

// write dumps the histogram to the passed metricValue as a float64 histogram.
[GoRecv] internal static void write(this ref timeHistogram h, ж<metricValue> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    var hist = @out.float64HistOrInit(timeHistBuckets);
    // The bottom-most bucket, containing negative values, is tracked
    // separately as underflow, so fill that in manually and then iterate
    // over the rest.
    (~hist).counts[0] = h.underflow.Load();
    foreach (var (i, _) in h.counts) {
        (~hist).counts[i + 1] = h.counts[i].Load();
    }
    (~hist).counts[len((~hist).counts) - 1] = h.overflow.Load();
}

internal static readonly UntypedInt fInf = /* 0x7FF0000000000000 */ 9218868437227405312;
internal static readonly UntypedInt fNegInf = /* 0xFFF0000000000000 */ 18442240474082181120;

internal static float64 float64Inf() {
    ref var inf = ref heap<uint64>(out var Ꮡinf);
    inf = ((uint64)fInf);
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡinf));
}

internal static float64 float64NegInf() {
    ref var inf = ref heap<uint64>(out var Ꮡinf);
    inf = ((uint64)fNegInf);
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡinf));
}

// timeHistogramMetricsBuckets generates a slice of boundaries for
// the timeHistogram. These boundaries are represented in seconds,
// not nanoseconds like the timeHistogram represents durations.
internal static slice<float64> timeHistogramMetricsBuckets() {
    var b = new slice<float64>(timeHistTotalBuckets + 1);
    // Underflow bucket.
    b[0] = float64NegInf();
    for (nint j = 0; j < timeHistNumSubBuckets; j++) {
        // No bucket bit for the first few buckets. Just sub-bucket bits after the
        // min bucket bit.
        var bucketNanos = ((uint64)j) << (int)((timeHistMinBucketBits - 1 - timeHistSubBucketBits));
        // Convert nanoseconds to seconds via a division.
        // These values will all be exactly representable by a float64.
        b[j + 1] = ((float64)bucketNanos) / 1e9F;
    }
    // Generate the rest of the buckets. It's easier to reason
    // about if we cut out the 0'th bucket.
    for (nint i = timeHistMinBucketBits; i < timeHistMaxBucketBits; i++) {
        for (nint j = 0; j < timeHistNumSubBuckets; j++) {
            // Set the bucket bit.
            var bucketNanos = ((uint64)1) << (int)((i - 1));
            // Set the sub-bucket bits.
            bucketNanos |= (uint64)(((uint64)j) << (int)((i - 1 - timeHistSubBucketBits)));
            // The index for this bucket is going to be the (i+1)'th bucket
            // (note that we're starting from zero, but handled the first bucket
            // earlier, so we need to compensate), and the j'th sub bucket.
            // Add 1 because we left space for -Inf.
            nint bucketIndex = (i - timeHistMinBucketBits + 1) * timeHistNumSubBuckets + j + 1;
            // Convert nanoseconds to seconds via a division.
            // These values will all be exactly representable by a float64.
            b[bucketIndex] = ((float64)bucketNanos) / 1e9F;
        }
    }
    // Overflow bucket.
    b[len(b) - 2] = ((float64)(((uint64)1) << (int)((timeHistMaxBucketBits - 1)))) / 1e9F;
    b[len(b) - 1] = float64Inf();
    return b;
}

} // end runtime_package
