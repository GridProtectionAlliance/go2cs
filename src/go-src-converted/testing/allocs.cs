// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;

partial class testing_package {

// AllocsPerRun returns the average number of allocations during calls to f.
// Although the return value has type float64, it will always be an integral value.
//
// To compute the number of allocations, the function will first be run once as
// a warm-up. The average number of allocations over the specified number of
// runs will then be measured and returned.
//
// AllocsPerRun sets GOMAXPROCS to 1 during its measurement and will restore
// it before returning.
public static float64 /*avg*/ AllocsPerRun(nint runs, Action f) {
    float64 avg = default!;
    func((defer, recover) => {
        deferǃ(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), defer);
        // Warm up the function
        f();
        // Measure the starting statistics
        ref var memstats = ref heap(new Δruntime.MemStats(), out var Ꮡmemstats);
        Δruntime.ReadMemStats(Ꮡmemstats);
        var mallocs = 0 - memstats.Mallocs;
        // Run the function the specified number of times
        for (nint i = 0; i < runs; i++) {
            f();
        }
        // Read the final statistics
        Δruntime.ReadMemStats(Ꮡmemstats);
        mallocs += memstats.Mallocs;
        // Average the mallocs over the runs (not counting the warm-up).
        // We are forced to return a float64 because the API is silly, but do
        // the division as integers so we can ask if AllocsPerRun()==1
        // instead of AllocsPerRun()<2.
        avg = (float64)(mallocs / (uint64)runs);
    });
    return avg;
}

} // end testing_package
