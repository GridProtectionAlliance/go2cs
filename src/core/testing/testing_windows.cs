// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using errors = errors_package;
using windows = @internal.syscall.windows_package;
using bits = math.bits_package;
using syscall = syscall_package;
using time = time_package;
using @internal.syscall;
using math;

partial class testing_package {

// isWindowsRetryable reports whether err is a Windows error code
// that may be fixed by retrying a failed filesystem operation.
internal static bool isWindowsRetryable(error err) {
    while (ᐧ) {
        var unwrapped = errors.Unwrap(err);
        if (unwrapped == default!) {
            break;
        }
        err = unwrapped;
    }
    if (err == syscall.ERROR_ACCESS_DENIED) {
        return true;
    }
    // Observed in https://go.dev/issue/50051.
    if (err == windows.ERROR_SHARING_VIOLATION) {
        return true;
    }
    // Observed in https://go.dev/issue/51442.
    return false;
}

// highPrecisionTime represents a single point in time with query performance counter.
// time.Time on Windows has low system granularity, which is not suitable for
// measuring short time intervals.
//
// TODO: If Windows runtime implements high resolution timing then highPrecisionTime
// can be removed.
[GoType] partial struct highPrecisionTime {
    internal int64 now;
}

// highPrecisionTimeNow returns high precision time for benchmarking.
internal static highPrecisionTime highPrecisionTimeNow() {
    highPrecisionTime t = default!;
    // This should always succeed for Windows XP and above.
    t.now = windows.QueryPerformanceCounter();
    return t;
}

internal static time.Duration sub(this highPrecisionTime a, highPrecisionTime b) {
    var delta = a.now - b.now;
    if (queryPerformanceFrequency == 0) {
        queryPerformanceFrequency = windows.QueryPerformanceFrequency();
    }
    var (hi, lo) = bits.Mul64(((uint64)delta), ((uint64)time.ΔSecond) / ((uint64)time.ΔNanosecond));
    var (quo, _) = bits.Div64(hi, lo, ((uint64)queryPerformanceFrequency));
    return ((time.Duration)quo);
}

internal static int64 queryPerformanceFrequency;

// highPrecisionTimeSince returns duration since a.
internal static time.Duration highPrecisionTimeSince(highPrecisionTime a) {
    return highPrecisionTimeNow().sub(a);
}

} // end testing_package
