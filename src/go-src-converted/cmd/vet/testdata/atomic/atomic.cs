// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the atomic checker.

// package atomic -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata/atomic" ==> using atomic = go.cmd.vet.testdata.atomic_package
// Original source: C:\Go\src\cmd\vet\testdata\atomic\atomic.go
using atomic = go.sync.atomic_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class atomic_package
    {
        public static void AtomicTests()
        {
            ref var x = ref heap(uint64(1L), out ptr<var> _addr_x);
            x = atomic.AddUint64(_addr_x, 1L); // ERROR "direct assignment to atomic value"
        }
    }
}}}}
