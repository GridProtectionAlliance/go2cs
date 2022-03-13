// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !goexperiment.staticlockranking
// +build !goexperiment.staticlockranking

// package runtime -- go2cs converted at 2022 March 13 05:24:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lockrank_off.go
namespace go;

public static partial class runtime_package {

// // lockRankStruct is embedded in mutex, but is empty when staticklockranking is
// disabled (the default)
private partial struct lockRankStruct {
}

private static void lockInit(ptr<mutex> _addr_l, lockRank rank) {
    ref mutex l = ref _addr_l.val;

}

private static lockRank getLockRank(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    return 0;
}

private static void lockWithRank(ptr<mutex> _addr_l, lockRank rank) {
    ref mutex l = ref _addr_l.val;

    lock2(l);
}

// This function may be called in nosplit context and thus must be nosplit.
//go:nosplit
private static void acquireLockRank(lockRank rank) {
}

private static void unlockWithRank(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    unlock2(l);
}

// This function may be called in nosplit context and thus must be nosplit.
//go:nosplit
private static void releaseLockRank(lockRank rank) {
}

private static void lockWithRankMayAcquire(ptr<mutex> _addr_l, lockRank rank) {
    ref mutex l = ref _addr_l.val;

}

//go:nosplit
private static void assertLockHeld(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

}

//go:nosplit
private static void assertRankHeld(lockRank r) {
}

//go:nosplit
private static void worldStopped() {
}

//go:nosplit
private static void worldStarted() {
}

//go:nosplit
private static void assertWorldStopped() {
}

//go:nosplit
private static void assertWorldStoppedOrLockHeld(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

}

} // end runtime_package
