// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the atomic checker.

// package testdata -- go2cs converted at 2020 August 29 10:09:34 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\atomic.go
using atomic = go.sync.atomic_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public partial struct Counter // : ulong
        {
        }

        public static void AtomicTests()
        {
            var x = uint64(1L);
            x = atomic.AddUint64(ref x, 1L); // ERROR "direct assignment to atomic value"
            _ = 10L;
            x = atomic.AddUint64(ref x, 1L); // ERROR "direct assignment to atomic value"
            x = atomic.AddUint64(ref x, 1L);
            _ = 10L; // ERROR "direct assignment to atomic value"

            var y = ref x;
            y.Value = atomic.AddUint64(y, 1L); // ERROR "direct assignment to atomic value"

            var su = default;
            su.Counter = atomic.AddUint64(ref su.Counter, 1L); // ERROR "direct assignment to atomic value"
            var z1 = atomic.AddUint64(ref su.Counter, 1L);
            _ = z1; // Avoid err "z declared and not used"

            var sp = default;
            sp.Counter.Value = atomic.AddUint64(sp.Counter, 1L); // ERROR "direct assignment to atomic value"
            var z2 = atomic.AddUint64(sp.Counter, 1L);
            _ = z2; // Avoid err "z declared and not used"

            ulong au = new slice<ulong>(new ulong[] { 10, 20 });
            au[0L] = atomic.AddUint64(ref au[0L], 1L); // ERROR "direct assignment to atomic value"
            au[1L] = atomic.AddUint64(ref au[0L], 1L);

            ref ulong ap = new slice<ref ulong>(new ref ulong[] { &au[0], &au[1] });
            ap[0L].Value = atomic.AddUint64(ap[0L], 1L); // ERROR "direct assignment to atomic value"
            ap[1L].Value = atomic.AddUint64(ap[0L], 1L);

            x = atomic.AddUint64(); // Used to make vet crash; now silently ignored.

            { 
                // A variable declaration creates a new variable in the current scope.
                x = atomic.AddUint64(ref x, 1L); // ERROR "declaration of .x. shadows declaration at testdata/atomic.go:16"

                // Re-declaration assigns a new value.
                x = atomic.AddUint64(ref x, 1L);
                long w = 10L; // ERROR "direct assignment to atomic value"
                _ = w;
            }
        }
    }
}}}
