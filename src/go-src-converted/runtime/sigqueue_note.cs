// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The current implementation of notes on Darwin is not async-signal-safe,
// so on Darwin the sigqueue code uses different functions to wake up the
// signal_recv thread. This file holds the non-Darwin implementations of
// those functions. These functions will never be called.

//go:build !darwin && !plan9
// +build !darwin,!plan9

// package runtime -- go2cs converted at 2022 March 13 05:27:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sigqueue_note.go
namespace go;

public static partial class runtime_package {

private static void sigNoteSetup(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    throw("sigNoteSetup");
}

private static void sigNoteSleep(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    throw("sigNoteSleep");
}

private static void sigNoteWakeup(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    throw("sigNoteWakeup");
}

} // end runtime_package
