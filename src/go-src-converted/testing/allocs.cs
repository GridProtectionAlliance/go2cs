// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 August 29 10:05:44 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\allocs.go
using runtime = go.runtime_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class testing_package
    {
        // AllocsPerRun returns the average number of allocations during calls to f.
        // Although the return value has type float64, it will always be an integral value.
        //
        // To compute the number of allocations, the function will first be run once as
        // a warm-up. The average number of allocations over the specified number of
        // runs will then be measured and returned.
        //
        // AllocsPerRun sets GOMAXPROCS to 1 during its measurement and will restore
        // it before returning.
        public static double AllocsPerRun(long runs, Action f) => func((defer, _, __) =>
        {
            defer(runtime.GOMAXPROCS(runtime.GOMAXPROCS(1L))); 

            // Warm up the function
            f(); 

            // Measure the starting statistics
            runtime.MemStats memstats = default;
            runtime.ReadMemStats(ref memstats);
            long mallocs = 0L - memstats.Mallocs; 

            // Run the function the specified number of times
            for (long i = 0L; i < runs; i++)
            {
                f();
            } 

            // Read the final statistics
            runtime.ReadMemStats(ref memstats);
            mallocs += memstats.Mallocs; 

            // Average the mallocs over the runs (not counting the warm-up).
            // We are forced to return a float64 because the API is silly, but do
            // the division as integers so we can ask if AllocsPerRun()==1
            // instead of AllocsPerRun()<2.
            return float64(mallocs / uint64(runs));
        });
    }
}
