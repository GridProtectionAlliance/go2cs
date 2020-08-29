// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 August 29 08:23:39 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\protomem.go
using io = go.io_package;
using math = go.math_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        // writeHeapProto writes the current heap profile in protobuf format to w.
        private static error writeHeapProto(io.Writer w, slice<runtime.MemProfileRecord> p, long rate)
        {
            var b = newProfileBuilder(w);
            b.pbValueType(tagProfile_PeriodType, "space", "bytes");
            b.pb.int64Opt(tagProfile_Period, rate);
            b.pbValueType(tagProfile_SampleType, "alloc_objects", "count");
            b.pbValueType(tagProfile_SampleType, "alloc_space", "bytes");
            b.pbValueType(tagProfile_SampleType, "inuse_objects", "count");
            b.pbValueType(tagProfile_SampleType, "inuse_space", "bytes");

            long values = new slice<long>(new long[] { 0, 0, 0, 0 });
            slice<ulong> locs = default;
            foreach (var (_, r) in p)
            {
                locs = locs[..0L];
                var hideRuntime = true;
                for (long tries = 0L; tries < 2L; tries++)
                {
                    foreach (var (_, addr) in r.Stack())
                    { 
                        // For heap profiles, all stack
                        // addresses are return PCs, which is
                        // what locForPC expects.
                        if (hideRuntime)
                        {
                            {
                                var f = runtime.FuncForPC(addr);

                                if (f != null && strings.HasPrefix(f.Name(), "runtime."))
                                {
                                    continue;
                                }
                            } 
                            // Found non-runtime. Show any runtime uses above it.
                            hideRuntime = false;
                        }
                        var l = b.locForPC(addr);
                        if (l == 0L)
                        { // runtime.goexit
                            continue;
                        }
                        locs = append(locs, l);
                    }                    if (len(locs) > 0L)
                    {
                        break;
                    }
                    hideRuntime = false; // try again, and show all frames
                }

                values[0L], values[1L] = scaleHeapSample(r.AllocObjects, r.AllocBytes, rate);
                values[2L], values[3L] = scaleHeapSample(r.InUseObjects(), r.InUseBytes(), rate);
                long blockSize = default;
                if (values[0L] > 0L)
                {
                    blockSize = values[1L] / values[0L];
                }
                b.pbSample(values, locs, () =>
                {
                    if (blockSize != 0L)
                    {
                        b.pbLabel(tagSample_Label, "bytes", "", blockSize);
                    }
                });
            }            b.build();
            return error.As(null);
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
        private static (long, long) scaleHeapSample(long count, long size, long rate)
        {
            if (count == 0L || size == 0L)
            {
                return (0L, 0L);
            }
            if (rate <= 1L)
            { 
                // if rate==1 all samples were collected so no adjustment is needed.
                // if rate<1 treat as unknown and skip scaling.
                return (count, size);
            }
            var avgSize = float64(size) / float64(count);
            long scale = 1L / (1L - math.Exp(-avgSize / float64(rate)));

            return (int64(float64(count) * scale), int64(float64(size) * scale));
        }
    }
}}
