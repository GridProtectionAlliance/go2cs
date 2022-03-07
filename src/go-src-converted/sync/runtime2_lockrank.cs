// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build goexperiment.staticlockranking
// +build goexperiment.staticlockranking

// package sync -- go2cs converted at 2022 March 06 22:26:23 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\runtime2_lockrank.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class sync_package {

    // Approximation of notifyList in runtime/sema.go. Size and alignment must
    // agree.
private partial struct notifyList {
    public uint wait;
    public uint notify;
    public nint rank; // rank field of the mutex
    public nint pad; // pad field of the mutex
    public System.UIntPtr @lock; // key field of the mutex

    public unsafe.Pointer head;
    public unsafe.Pointer tail;
}

} // end sync_package
