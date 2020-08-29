// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the useless-assignment checker.

// package testdata -- go2cs converted at 2020 August 29 10:09:33 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\assign.go
using rand = go.math.rand_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public partial struct ST
        {
            public long x;
            public slice<long> l;
        }

        private static void SetX(this ref ST s, long x, channel<long> ch)
        { 
            // Accidental self-assignment; it should be "s.x = x"
            x = x; // ERROR "self-assignment of x to x"
            // Another mistake
            s.x = s.x; // ERROR "self-assignment of s.x to s.x"

            s.l[0L] = s.l[0L]; // ERROR "self-assignment of s.l.0. to s.l.0."

            // Bail on any potential side effects to avoid false positives
            s.l[num()] = s.l[num()];
            var rng = rand.New(rand.NewSource(0L));
            s.l[rng.Intn(len(s.l))] = s.l[rng.Intn(len(s.l))];
            s.l[ch.Receive()] = s.l[ch.Receive()];
        }

        private static long num()
        {
            return 2L;
        }
    }
}}}
