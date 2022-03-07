// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package poll supports non-blocking I/O on file descriptors with polling.
// This supports I/O operations that block only a goroutine, not a thread.
// This is used by the net and os packages.
// It uses a poller built into the runtime, with support from the
// runtime scheduler.
// package poll -- go2cs converted at 2022 March 06 22:12:58 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fd.go
using errors = go.errors_package;
using System;


namespace go.@internal;

public static partial class poll_package {

    // errNetClosing is the type of the variable ErrNetClosing.
    // This is used to implement the net.Error interface.
private partial struct errNetClosing {
}

// Error returns the error message for ErrNetClosing.
// Keep this string consistent because of issue #4373:
// since historically programs have not been able to detect
// this error, they look for the string.
private static @string Error(this errNetClosing e) {
    return "use of closed network connection";
}

private static bool Timeout(this errNetClosing e) {
    return false;
}
private static bool Temporary(this errNetClosing e) {
    return false;
}

// ErrNetClosing is returned when a network descriptor is used after
// it has been closed.
public static errNetClosing ErrNetClosing = new errNetClosing();

// ErrFileClosing is returned when a file descriptor is used after it
// has been closed.
public static var ErrFileClosing = errors.New("use of closed file");

// ErrNoDeadline is returned when a request is made to set a deadline
// on a file type that does not use the poller.
public static var ErrNoDeadline = errors.New("file type does not support deadline");

// Return the appropriate closing error based on isFile.
private static error errClosing(bool isFile) {
    if (isFile) {
        return error.As(ErrFileClosing)!;
    }
    return error.As(ErrNetClosing)!;

}

// ErrDeadlineExceeded is returned for an expired deadline.
// This is exported by the os package as os.ErrDeadlineExceeded.
public static error ErrDeadlineExceeded = error.As(addr(new DeadlineExceededError()))!;

// DeadlineExceededError is returned for an expired deadline.
public partial struct DeadlineExceededError {
}

// Implement the net.Error interface.
// The string is "i/o timeout" because that is what was returned
// by earlier Go versions. Changing it may break programs that
// match on error strings.
private static @string Error(this ptr<DeadlineExceededError> _addr_e) {
    ref DeadlineExceededError e = ref _addr_e.val;

    return "i/o timeout";
}
private static bool Timeout(this ptr<DeadlineExceededError> _addr_e) {
    ref DeadlineExceededError e = ref _addr_e.val;

    return true;
}
private static bool Temporary(this ptr<DeadlineExceededError> _addr_e) {
    ref DeadlineExceededError e = ref _addr_e.val;

    return true;
}

// ErrNotPollable is returned when the file or socket is not suitable
// for event notification.
public static var ErrNotPollable = errors.New("not pollable");

// consume removes data from a slice of byte slices, for writev.
private static void consume(ptr<slice<slice<byte>>> _addr_v, long n) {
    ref slice<slice<byte>> v = ref _addr_v.val;

    while (len(v) > 0) {
        var ln0 = int64(len((v)[0]));
        if (ln0 > n) {
            (v)[0] = (v)[0][(int)n..];
            return ;
        }
        n -= ln0;
        v = (v)[(int)1..];

    }

}

// TestHookDidWritev is a hook for testing writev.
public static Action<nint> TestHookDidWritev = wrote => {
};

} // end poll_package
