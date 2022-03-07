// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the atomic checker.

// package atomic -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata/atomic" ==> using atomic = go.cmd.vet.testdata.atomic_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\atomic\atomic.go
using atomic = go.sync.atomic_package;

namespace go.cmd.vet.testdata;

public static partial class atomic_package {

public static void AtomicTests() {
    ref var x = ref heap(uint64(1), out ptr<var> _addr_x);
    x = atomic.AddUint64(_addr_x, 1); // ERROR "direct assignment to atomic value"
}

} // end atomic_package
