// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using profilerecord = @internal.profilerecord_package;
using io = io_package;
using math = math_package;
using runtime = runtime_package;
using strings = strings_package;
using @internal;

partial class pprof_package {

// writeHeapProto writes the current heap profile in protobuf format to w.
internal static error writeHeapProto(io.Writer w, slice<profilerecord.MemProfileRecord> p, int64 rate, @string defaultSampleType) {
    var b = newProfileBuilder(w);
    b.pbValueType(tagProfile_PeriodType, "space"u8, "bytes"u8);
    (~b).pb.int64Opt(tagProfile_Period, rate);
    b.pbValueType(tagProfile_SampleType, "alloc_objects"u8, "count"u8);
    b.pbValueType(tagProfile_SampleType, "alloc_space"u8, "bytes"u8);
    b.pbValueType(tagProfile_SampleType, "inuse_objects"u8, "count"u8);
    b.pbValueType(tagProfile_SampleType, "inuse_space"u8, "bytes"u8);
    if (defaultSampleType != ""u8) {
        (~b).pb.int64Opt(tagProfile_DefaultSampleType, b.stringIndex(defaultSampleType));
    }
    var values = new int64[]{0, 0, 0, 0}.slice();
    slice<uint64> locs = default!;
    foreach (var (_, r) in p) {
        var hideRuntime = true;
        for (nint tries = 0; tries < 2; tries++) {
            var stk = r.Stack;
            // For heap profiles, all stack
            // addresses are return PCs, which is
            // what appendLocsForStack expects.
            if (hideRuntime) {
                foreach (var (i, addr) in stk) {
                    {
                        var f = runtime.FuncForPC(addr); if (f != nil && strings.HasPrefix(f.Name(), "runtime."u8)) {
                            continue;
                        }
                    }
                    // Found non-runtime. Show any runtime uses above it.
                    stk = stk[(int)(i)..];
                    break;
                }
            }
            locs = b.appendLocsForStack(locs[..0], stk);
            if (len(locs) > 0) {
                break;
            }
            hideRuntime = false;
        }
        // try again, and show all frames next time.
        (values[0], values[1]) = scaleHeapSample(r.AllocObjects, r.AllocBytes, rate);
        (values[2], values[3]) = scaleHeapSample(r.InUseObjects(), r.InUseBytes(), rate);
        int64 blockSize = default!;
        if (r.AllocObjects > 0) {
            blockSize = r.AllocBytes / r.AllocObjects;
        }
        b.pbSample(values, locs, 
        var bʗ1 = b;
        () => {
            if (blockSize != 0) {
                bʗ1.pbLabel(tagSample_Label, "bytes"u8, ""u8, blockSize);
            }
        });
    }
    b.build();
    return default!;
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
internal static (int64, int64) scaleHeapSample(int64 count, int64 size, int64 rate) {
    if (count == 0 || size == 0) {
        return (0, 0);
    }
    if (rate <= 1) {
        // if rate==1 all samples were collected so no adjustment is needed.
        // if rate<1 treat as unknown and skip scaling.
        return (count, size);
    }
    var avgSize = ((float64)size) / ((float64)count);
    var scale = 1 / (1 - math.Exp(-avgSize / ((float64)rate)));
    return (((int64)(((float64)count) * scale)), ((int64)(((float64)size) * scale)));
}

} // end pprof_package
