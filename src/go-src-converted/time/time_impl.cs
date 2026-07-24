// time_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

using System.Diagnostics;

partial class time_package
{
    // time's now() and runtimeNano() are //go:linkname'd into the Go runtime (runtime.now /
    // runtime.nanotime), so the converter emitted them as bodyless partials — throwing stubs. That
    // made the package UNUSABLE at load: the static initializer `startNano = runtimeNano() - 1`
    // (time.cs) runs runtimeNano() during the type's cctor, so merely IMPORTING time — or the first
    // touch of time.Now / Since / Sub — died with "runtimeNano: external (assembly or cgo) function
    // is not implemented" (surfaced by io/fs, crypto/subtle, and go/doc/comment, whose test-package
    // static init reaches time_package..cctor). These supply the equivalent managed bodies so the
    // clock RUNS. Living in an _impl.cs companion (never emitted by the converter) makes this durable
    // by construction: a -stdlib reconvert regenerates the pristine bodyless partials and these
    // bodies remain. Only the wall/monotonic clock reads are realized here — Go's runtime timers
    // (Sleep / newTimer / stopTimer / resetTimer) stay bodyless partials until a timer consumer
    // needs them.

    // runtimeNano returns the current value of a monotonic clock in nanoseconds. Go's
    // runtime.nanotime reads a monotonic source (QueryPerformanceCounter on Windows); Stopwatch is
    // exactly that source on .NET. The tick count is scaled to nanoseconds with the seconds/remainder
    // split so the result stays EXACT and monotonic without overflowing: `ticks * 1e9` would overflow
    // int64 for any real uptime, whereas `seconds * 1e9` (seconds = ticks/Frequency) and
    // `rem * 1e9 / Frequency` (rem < Frequency) each stay well inside int64 and together preserve full
    // sub-tick nanosecond resolution. The absolute epoch is arbitrary — only differences are observed
    // (startNano rebases it), and Now().mono / Sub / Since all read this same source, so the monotonic
    // component is coherent across the package.
    internal static partial int64 runtimeNano()
    {
        int64 ticks = Stopwatch.GetTimestamp();
        int64 freq = Stopwatch.Frequency;
        int64 seconds = ticks / freq;
        int64 rem = ticks % freq;
        return seconds * 1_000_000_000L + rem * 1_000_000_000L / freq;
    }

    // now returns the current wall-clock time as (Unix seconds, sub-second nanoseconds) plus a
    // monotonic reading. Go's runtime.now returns seconds since the Unix epoch (Now() then rebases
    // them by unixToInternal - minWall = 2682288000); DateTime.UtcNow is the managed wall clock
    // (.NET uses GetSystemTimePreciseAsFileTime on modern Windows, ~100 ns granularity). nsec is the
    // sub-second remainder converted from 100 ns Ticks to nanoseconds. mono uses the same monotonic
    // source as runtimeNano() so Now()'s monotonic component agrees with Since / Sub.
    internal static partial (int64 sec, int32 nsec, int64 mono) now()
    {
        // `DateTime` (unqualified) binds to time's own layout constant here, so the wall-clock type
        // is spelled out fully.
        int64 unixTicks = System.DateTime.UtcNow.Ticks - System.DateTime.UnixEpoch.Ticks;
        int64 sec = unixTicks / System.TimeSpan.TicksPerSecond;
        int32 nsec = ((int32)(unixTicks % System.TimeSpan.TicksPerSecond)) * 100;
        int64 mono = runtimeNano();
        return (sec, nsec, mono);
    }
}
