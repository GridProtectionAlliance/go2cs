// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package metrics -- go2cs converted at 2022 March 06 22:14:35 UTC
// import "runtime/metrics" ==> using metrics = go.runtime.metrics_package
// Original source: C:\Program Files\Go\src\runtime\metrics\histogram.go


namespace go.runtime;

public static partial class metrics_package {

    // Float64Histogram represents a distribution of float64 values.
public partial struct Float64Histogram {
    public slice<ulong> Counts; // Buckets contains the boundaries of the histogram buckets, in increasing order.
//
// Buckets[0] is the inclusive lower bound of the minimum bucket while
// Buckets[len(Buckets)-1] is the exclusive upper bound of the maximum bucket.
// Hence, there are len(Buckets)-1 counts. Furthermore, len(Buckets) != 1, always,
// since at least two boundaries are required to describe one bucket (and 0
// boundaries are used to describe 0 buckets).
//
// Buckets[0] is permitted to have value -Inf and Buckets[len(Buckets)-1] is
// permitted to have value Inf.
//
// For a given metric name, the value of Buckets is guaranteed not to change
// between calls until program exit.
//
// This slice value is permitted to alias with other Float64Histograms' Buckets
// fields, so the values within should only ever be read. If they need to be
// modified, the user must make a copy.
    public slice<double> Buckets;
}

} // end metrics_package
