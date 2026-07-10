// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// The current implementation of notes on Darwin is not async-signal-safe,
// so on Darwin the sigqueue code uses different functions to wake up the
// signal_recv thread. This file holds the non-Darwin implementations of
// those functions. These functions will never be called.
//go:build !darwin && !plan9
namespace go;

partial class runtime_package {

internal static void sigNoteSetup(ж<note> _) {
    @throw("sigNoteSetup"u8);
}

internal static void sigNoteSleep(ж<note> _) {
    @throw("sigNoteSleep"u8);
}

internal static void sigNoteWakeup(ж<note> _) {
    @throw("sigNoteWakeup"u8);
}

} // end runtime_package
