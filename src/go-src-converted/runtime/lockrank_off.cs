// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.staticlockranking
namespace go;

partial class runtime_package {

internal const bool staticLockRanking = false;

// // lockRankStruct is embedded in mutex, but is empty when staticklockranking is
// disabled (the default)
[GoType] partial struct lockRankStruct {
}

internal static void lockInit(ж<mutex> Ꮡl, lockRank rank) {
    ref var l = ref Ꮡl.Value;

}

internal static lockRank getLockRank(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.Value;

    return 0;
}

internal static void lockWithRank(ж<mutex> Ꮡl, lockRank rank) {
    ref var l = ref Ꮡl.Value;

    lock2(Ꮡl);
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void acquireLockRankAndM(lockRank rank) {
    acquirem();
}

internal static void unlockWithRank(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.Value;

    unlock2(Ꮡl);
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void releaseLockRankAndM(lockRank rank) {
    releasem((~getg()).m);
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void lockWithRankMayAcquire(ж<mutex> Ꮡl, lockRank rank) {
    ref var l = ref Ꮡl.Value;

}

//go:nosplit
internal static void assertLockHeld(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.Value;

}

//go:nosplit
internal static void assertRankHeld(lockRank r) {
}

//go:nosplit
internal static void worldStopped() {
}

//go:nosplit
internal static void worldStarted() {
}

//go:nosplit
internal static void assertWorldStopped() {
}

//go:nosplit
internal static void assertWorldStoppedOrLockHeld(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.Value;

}

} // end runtime_package
