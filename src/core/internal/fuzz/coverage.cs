// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using bits = math.bits_package;
using math;

partial class fuzz_package {

// ResetCoverage sets all of the counters for each edge of the instrumented
// source code to 0.
public static void ResetCoverage() {
    var cov = coverage();
    clear(cov);
}

// SnapshotCoverage copies the current counter values into coverageSnapshot,
// preserving them for later inspection. SnapshotCoverage also rounds each
// counter down to the nearest power of two. This lets the coordinator store
// multiple values for each counter by OR'ing them together.
public static void SnapshotCoverage() {
    var cov = coverage();
    foreach (var (i, b) in cov) {
        b |= (byte)(b >> (int)(1));
        b |= (byte)(b >> (int)(2));
        b |= (byte)(b >> (int)(4));
        b -= b >> (int)(1);
        coverageSnapshot[i] = b;
    }
}

// diffCoverage returns a set of bits set in snapshot but not in base.
// If there are no new bits set, diffCoverage returns nil.
internal static slice<byte> diffCoverage(slice<byte> @base, slice<byte> snapshot) {
    if (len(@base) != len(snapshot)) {
        throw panic(fmt.Sprintf("the number of coverage bits changed: before=%d, after=%d"u8, len(@base), len(snapshot)));
    }
    var found = false;
    foreach (var (i, _) in snapshot) {
        if ((byte)(snapshot[i] & ~@base[i]) != 0) {
            found = true;
            break;
        }
    }
    if (!found) {
        return default!;
    }
    var diff = new slice<byte>(len(snapshot));
    foreach (var (i, _) in diff) {
        diff[i] = (byte)(snapshot[i] & ~@base[i]);
    }
    return diff;
}

// countNewCoverageBits returns the number of bits set in snapshot that are not
// set in base.
internal static nint countNewCoverageBits(slice<byte> @base, slice<byte> snapshot) {
    nint n = 0;
    foreach (var (i, _) in snapshot) {
        n += bits.OnesCount8((byte)(snapshot[i] & ~@base[i]));
    }
    return n;
}

// isCoverageSubset returns true if all the base coverage bits are set in
// snapshot.
internal static bool isCoverageSubset(slice<byte> @base, slice<byte> snapshot) {
    foreach (var (i, v) in @base) {
        if ((byte)(v & snapshot[i]) != v) {
            return false;
        }
    }
    return true;
}

// hasCoverageBit returns true if snapshot has at least one bit set that is
// also set in base.
internal static bool hasCoverageBit(slice<byte> @base, slice<byte> snapshot) {
    foreach (var (i, _) in snapshot) {
        if ((byte)(snapshot[i] & @base[i]) != 0) {
            return true;
        }
    }
    return false;
}

internal static nint countBits(slice<byte> cov) {
    nint n = 0;
    foreach (var (_, c) in cov) {
        n += bits.OnesCount8(c);
    }
    return n;
}

internal static bool coverageEnabled = len(coverage()) > 0;
internal static slice<byte> coverageSnapshot = new slice<byte>(len(coverage()));
internal static array<byte> _counters;
internal static array<byte> _ecounters;

} // end fuzz_package
