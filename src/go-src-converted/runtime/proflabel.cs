// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:26:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\proflabel.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

private static System.UIntPtr labelSync = default;

//go:linkname runtime_setProfLabel runtime/pprof.runtime_setProfLabel
private static void runtime_setProfLabel(unsafe.Pointer labels) { 
    // Introduce race edge for read-back via profile.
    // This would more properly use &getg().labels as the sync address,
    // but we do the read in a signal handler and can't call the race runtime then.
    //
    // This uses racereleasemerge rather than just racerelease so
    // the acquire in profBuf.read synchronizes with *all* prior
    // setProfLabel operations, not just the most recent one. This
    // is important because profBuf.read will observe different
    // labels set by different setProfLabel operations on
    // different goroutines, so it needs to synchronize with all
    // of them (this wouldn't be an issue if we could synchronize
    // on &getg().labels since we would synchronize with each
    // most-recent labels write separately.)
    //
    // racereleasemerge is like a full read-modify-write on
    // labelSync, rather than just a store-release, so it carries
    // a dependency on the previous racereleasemerge, which
    // ultimately carries forward to the acquire in profBuf.read.
    if (raceenabled) {
        racereleasemerge(@unsafe.Pointer(_addr_labelSync));
    }
    getg().labels = labels;
}

//go:linkname runtime_getProfLabel runtime/pprof.runtime_getProfLabel
private static unsafe.Pointer runtime_getProfLabel() {
    return getg().labels;
}

} // end runtime_package
