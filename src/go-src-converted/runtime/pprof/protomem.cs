// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 06 22:15:10 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\protomem.go
using io = go.io_package;
using math = go.math_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using System;


namespace go.runtime;

public static partial class pprof_package {

    // writeHeapProto writes the current heap profile in protobuf format to w.
private static error writeHeapProto(io.Writer w, slice<runtime.MemProfileRecord> p, long rate, @string defaultSampleType) {
    var b = newProfileBuilder(w);
    b.pbValueType(tagProfile_PeriodType, "space", "bytes");
    b.pb.int64Opt(tagProfile_Period, rate);
    b.pbValueType(tagProfile_SampleType, "alloc_objects", "count");
    b.pbValueType(tagProfile_SampleType, "alloc_space", "bytes");
    b.pbValueType(tagProfile_SampleType, "inuse_objects", "count");
    b.pbValueType(tagProfile_SampleType, "inuse_space", "bytes");
    if (defaultSampleType != "") {
        b.pb.int64Opt(tagProfile_DefaultSampleType, b.stringIndex(defaultSampleType));
    }
    long values = new slice<long>(new long[] { 0, 0, 0, 0 });
    slice<ulong> locs = default;
    foreach (var (_, r) in p) {
        var hideRuntime = true;
        for (nint tries = 0; tries < 2; tries++) {
            var stk = r.Stack(); 
            // For heap profiles, all stack
            // addresses are return PCs, which is
            // what appendLocsForStack expects.
            if (hideRuntime) {
                foreach (var (i, addr) in stk) {
                    {
                        var f = runtime.FuncForPC(addr);

                        if (f != null && strings.HasPrefix(f.Name(), "runtime.")) {
                            continue;
                        }
                    } 
                    // Found non-runtime. Show any runtime uses above it.
                    stk = stk[(int)i..];
                    break;

                }
            }
            locs = b.appendLocsForStack(locs[..(int)0], stk);
            if (len(locs) > 0) {
                break;
            }
            hideRuntime = false; // try again, and show all frames next time.
        }

        values[0], values[1] = scaleHeapSample(r.AllocObjects, r.AllocBytes, rate);
        values[2], values[3] = scaleHeapSample(r.InUseObjects(), r.InUseBytes(), rate);
        long blockSize = default;
        if (r.AllocObjects > 0) {
            blockSize = r.AllocBytes / r.AllocObjects;
        }
        b.pbSample(values, locs, () => {
            if (blockSize != 0) {
                b.pbLabel(tagSample_Label, "bytes", "", blockSize);
            }
        });

    }    b.build();
    return error.As(null!)!;

}

// scaleHeapSample adjusts the data from a heap Sample to
// account for its probability of appearing in the collected
// data. heap profiles are a sampling of the memory allocations
// requests in a program. We estimate the unsampled value by dividing
// each collected sample by its probability of appearing in the
// profile. heap profiles rely on a poisson process to determine
// which samples to collect, based on the desired average collection
// rate R. The probability of a sample of size S to appear in that
// profile is 1-exp(-S/R).
private static (long, long) scaleHeapSample(long count, long size, long rate) {
    long _p0 = default;
    long _p0 = default;

    if (count == 0 || size == 0) {
        return (0, 0);
    }
    if (rate <= 1) { 
        // if rate==1 all samples were collected so no adjustment is needed.
        // if rate<1 treat as unknown and skip scaling.
        return (count, size);

    }
    var avgSize = float64(size) / float64(count);
    nint scale = 1 / (1 - math.Exp(-avgSize / float64(rate)));

    return (int64(float64(count) * scale), int64(float64(size) * scale));

}

} // end pprof_package
