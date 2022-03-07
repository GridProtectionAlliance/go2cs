// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package metrics -- go2cs converted at 2022 March 06 22:14:35 UTC
// import "runtime/metrics" ==> using metrics = go.runtime.metrics_package
// Original source: C:\Program Files\Go\src\runtime\metrics\value.go
using math = go.math_package;
using @unsafe = go.@unsafe_package;

namespace go.runtime;

public static partial class metrics_package {

    // ValueKind is a tag for a metric Value which indicates its type.
public partial struct ValueKind { // : nint
}

 
// KindBad indicates that the Value has no type and should not be used.
public static readonly ValueKind KindBad = iota; 

// KindUint64 indicates that the type of the Value is a uint64.
public static readonly var KindUint64 = 0; 

// KindFloat64 indicates that the type of the Value is a float64.
public static readonly var KindFloat64 = 1; 

// KindFloat64Histogram indicates that the type of the Value is a *Float64Histogram.
public static readonly var KindFloat64Histogram = 2;


// Value represents a metric value returned by the runtime.
public partial struct Value {
    public ValueKind kind;
    public ulong scalar; // contains scalar values for scalar Kinds.
    public unsafe.Pointer pointer; // contains non-scalar values.
}

// Kind returns the tag representing the kind of value this is.
public static ValueKind Kind(this Value v) {
    return v.kind;
}

// Uint64 returns the internal uint64 value for the metric.
//
// If v.Kind() != KindUint64, this method panics.
public static ulong Uint64(this Value v) => func((_, panic, _) => {
    if (v.kind != KindUint64) {
        panic("called Uint64 on non-uint64 metric value");
    }
    return v.scalar;

});

// Float64 returns the internal float64 value for the metric.
//
// If v.Kind() != KindFloat64, this method panics.
public static double Float64(this Value v) => func((_, panic, _) => {
    if (v.kind != KindFloat64) {
        panic("called Float64 on non-float64 metric value");
    }
    return math.Float64frombits(v.scalar);

});

// Float64Histogram returns the internal *Float64Histogram value for the metric.
//
// If v.Kind() != KindFloat64Histogram, this method panics.
public static ptr<Float64Histogram> Float64Histogram(this Value v) => func((_, panic, _) => {
    if (v.kind != KindFloat64Histogram) {
        panic("called Float64Histogram on non-Float64Histogram metric value");
    }
    return _addr_(Float64Histogram.val)(v.pointer)!;

});

} // end metrics_package
