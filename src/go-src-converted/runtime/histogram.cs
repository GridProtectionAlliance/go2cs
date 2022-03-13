// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:24:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\histogram.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;

public static partial class runtime_package {

 
// For the time histogram type, we use an HDR histogram.
// Values are placed in super-buckets based solely on the most
// significant set bit. Thus, super-buckets are power-of-2 sized.
// Values are then placed into sub-buckets based on the value of
// the next timeHistSubBucketBits most significant bits. Thus,
// sub-buckets are linear within a super-bucket.
//
// Therefore, the number of sub-buckets (timeHistNumSubBuckets)
// defines the error. This error may be computed as
// 1/timeHistNumSubBuckets*100%. For example, for 16 sub-buckets
// per super-bucket the error is approximately 6%.
//
// The number of super-buckets (timeHistNumSuperBuckets), on the
// other hand, defines the range. To reserve room for sub-buckets,
// bit timeHistSubBucketBits is the first bit considered for
// super-buckets, so super-bucket indices are adjusted accordingly.
//
// As an example, consider 45 super-buckets with 16 sub-buckets.
//
//    00110
//    ^----
//    │  ^
//    │  └---- Lowest 4 bits -> sub-bucket 6
//    └------- Bit 4 unset -> super-bucket 0
//
//    10110
//    ^----
//    │  ^
//    │  └---- Next 4 bits -> sub-bucket 6
//    └------- Bit 4 set -> super-bucket 1
//    100010
//    ^----^
//    │  ^ └-- Lower bits ignored
//    │  └---- Next 4 bits -> sub-bucket 1
//    └------- Bit 5 set -> super-bucket 2
//
// Following this pattern, bucket 45 will have the bit 48 set. We don't
// have any buckets for higher values, so the highest sub-bucket will
// contain values of 2^48-1 nanoseconds or approx. 3 days. This range is
// more than enough to handle durations produced by the runtime.
private static readonly nint timeHistSubBucketBits = 4;
private static readonly nint timeHistNumSubBuckets = 1 << (int)(timeHistSubBucketBits);
private static readonly nint timeHistNumSuperBuckets = 45;
private static readonly var timeHistTotalBuckets = timeHistNumSuperBuckets * timeHistNumSubBuckets + 1;

// timeHistogram represents a distribution of durations in
// nanoseconds.
//
// The accuracy and range of the histogram is defined by the
// timeHistSubBucketBits and timeHistNumSuperBuckets constants.
//
// It is an HDR histogram with exponentially-distributed
// buckets and linearly distributed sub-buckets.
//
// Counts in the histogram are updated atomically, so it is safe
// for concurrent use. It is also safe to read all the values
// atomically.
private partial struct timeHistogram {
    public array<ulong> counts; // underflow counts all the times we got a negative duration
// sample. Because of how time works on some platforms, it's
// possible to measure negative durations. We could ignore them,
// but we record them anyway because it's better to have some
// signal that it's happening than just missing samples.
    public ulong underflow;
}

// record adds the given duration to the distribution.
//
// Disallow preemptions and stack growths because this function
// may run in sensitive locations.
//go:nosplit
private static void record(this ptr<timeHistogram> _addr_h, long duration) {
    ref timeHistogram h = ref _addr_h.val;

    if (duration < 0) {
        atomic.Xadd64(_addr_h.underflow, 1);
        return ;
    }
    nuint superBucket = default;    nuint subBucket = default;

    if (duration >= timeHistNumSubBuckets) { 
        // At this point, we know the duration value will always be
        // at least timeHistSubBucketsBits long.
        superBucket = uint(sys.Len64(uint64(duration))) - timeHistSubBucketBits;
        if (superBucket * timeHistNumSubBuckets >= uint(len(h.counts))) { 
            // The bucket index we got is larger than what we support, so
            // include this count in the highest bucket, which extends to
            // infinity.
            superBucket = timeHistNumSuperBuckets - 1;
            subBucket = timeHistNumSubBuckets - 1;
        }
        else
 { 
            // The linear subbucket index is just the timeHistSubBucketsBits
            // bits after the top bit. To extract that value, shift down
            // the duration such that we leave the top bit and the next bits
            // intact, then extract the index.
            subBucket = uint((duration >> (int)((superBucket - 1))) % timeHistNumSubBuckets);
        }
    }
    else
 {
        subBucket = uint(duration);
    }
    atomic.Xadd64(_addr_h.counts[superBucket * timeHistNumSubBuckets + subBucket], 1);
}

private static readonly nuint fInf = 0x7FF0000000000000;
private static readonly nuint fNegInf = 0xFFF0000000000000;

private static double float64Inf() {
    ref var inf = ref heap(uint64(fInf), out ptr<var> _addr_inf);
    return new ptr<ptr<ptr<double>>>(@unsafe.Pointer(_addr_inf));
}

private static double float64NegInf() {
    ref var inf = ref heap(uint64(fNegInf), out ptr<var> _addr_inf);
    return new ptr<ptr<ptr<double>>>(@unsafe.Pointer(_addr_inf));
}

// timeHistogramMetricsBuckets generates a slice of boundaries for
// the timeHistogram. These boundaries are represented in seconds,
// not nanoseconds like the timeHistogram represents durations.
private static slice<double> timeHistogramMetricsBuckets() {
    var b = make_slice<double>(timeHistTotalBuckets + 1);
    b[0] = float64NegInf();
    for (nint i = 0; i < timeHistNumSuperBuckets; i++) {
        var superBucketMin = uint64(0); 
        // The (inclusive) minimum for the first non-negative bucket is 0.
        if (i > 0) { 
            // The minimum for the second bucket will be
            // 1 << timeHistSubBucketBits, indicating that all
            // sub-buckets are represented by the next timeHistSubBucketBits
            // bits.
            // Thereafter, we shift up by 1 each time, so we can represent
            // this pattern as (i-1)+timeHistSubBucketBits.
            superBucketMin = uint64(1) << (int)(uint(i - 1 + timeHistSubBucketBits));
        }
        var subBucketShift = uint(0);
        if (i > 1) { 
            // The first two super buckets are exact with respect to integers,
            // so we'll never have to shift the sub-bucket index. Thereafter,
            // we shift up by 1 with each subsequent bucket.
            subBucketShift = uint(i - 2);
        }
        for (nint j = 0; j < timeHistNumSubBuckets; j++) { 
            // j is the sub-bucket index. By shifting the index into position to
            // combine with the bucket minimum, we obtain the minimum value for that
            // sub-bucket.
            var subBucketMin = superBucketMin + (uint64(j) << (int)(subBucketShift)); 

            // Convert the subBucketMin which is in nanoseconds to a float64 seconds value.
            // These values will all be exactly representable by a float64.
            b[i * timeHistNumSubBuckets + j + 1] = float64(subBucketMin) / 1e9F;
        }
    }
    b[len(b) - 1] = float64Inf();
    return b;
}

} // end runtime_package
