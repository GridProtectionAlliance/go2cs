// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the useless-assignment checker.

// package assign -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata/assign" ==> using assign = go.cmd.vet.testdata.assign_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\assign\assign.go
using rand = go.math.rand_package;

namespace go.cmd.vet.testdata;

public static partial class assign_package {

public partial struct ST {
    public nint x;
    public slice<nint> l;
}

private static void SetX(this ptr<ST> _addr_s, nint x, channel<nint> ch) {
    ref ST s = ref _addr_s.val;
 
    // Accidental self-assignment; it should be "s.x = x"
    x = x; // ERROR "self-assignment of x to x"
    // Another mistake
    s.x = s.x; // ERROR "self-assignment of s.x to s.x"

    s.l[0] = s.l[0]; // ERROR "self-assignment of s.l.0. to s.l.0."

    // Bail on any potential side effects to avoid false positives
    s.l[num()] = s.l[num()];
    var rng = rand.New(rand.NewSource(0));
    s.l[rng.Intn(len(s.l))] = s.l[rng.Intn(len(s.l))];
    s.l[ch.Receive()] = s.l[ch.Receive()];

}

private static nint num() {
    return 2;
}

} // end assign_package
