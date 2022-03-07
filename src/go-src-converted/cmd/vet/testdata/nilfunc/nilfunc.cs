// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nilfunc -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata/nilfunc" ==> using nilfunc = go.cmd.vet.testdata.nilfunc_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\nilfunc\nilfunc.go


namespace go.cmd.vet.testdata;

public static partial class nilfunc_package {

public static void F() {
}

public static void Comparison() => func((_, panic, _) => {
    if (F == null) { // ERROR "comparison of function F == nil is always false"
        panic("can't happen");

    }
});

} // end nilfunc_package
