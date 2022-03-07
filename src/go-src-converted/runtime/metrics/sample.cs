// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package metrics -- go2cs converted at 2022 March 06 22:14:35 UTC
// import "runtime/metrics" ==> using metrics = go.runtime.metrics_package
// Original source: C:\Program Files\Go\src\runtime\metrics\sample.go
using _runtime_ = go.runtime_package; // depends on the runtime via a linkname'd function
using @unsafe = go.@unsafe_package;

namespace go.runtime;

public static partial class metrics_package {

    // Sample captures a single metric sample.
public partial struct Sample {
    public @string Name; // Value is the value of the metric sample.
    public Value Value;
}

// Implemented in the runtime.
private static void runtime_readMetrics(unsafe.Pointer _p0, nint _p0, nint _p0);

// Read populates each Value field in the given slice of metric samples.
//
// Desired metrics should be present in the slice with the appropriate name.
// The user of this API is encouraged to re-use the same slice between calls for
// efficiency, but is not required to do so.
//
// Note that re-use has some caveats. Notably, Values should not be read or
// manipulated while a Read with that value is outstanding; that is a data race.
// This property includes pointer-typed Values (for example, Float64Histogram)
// whose underlying storage will be reused by Read when possible. To safely use
// such values in a concurrent setting, all data must be deep-copied.
//
// It is safe to execute multiple Read calls concurrently, but their arguments
// must share no underlying memory. When in doubt, create a new []Sample from
// scratch, which is always safe, though may be inefficient.
//
// Sample values with names not appearing in All will have their Value populated
// as KindBad to indicate that the name is unknown.
public static void Read(slice<Sample> m) {
    runtime_readMetrics(@unsafe.Pointer(_addr_m[0]), len(m), cap(m));
}

} // end metrics_package
