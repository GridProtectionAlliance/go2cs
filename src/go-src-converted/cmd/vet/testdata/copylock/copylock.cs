// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package copylock -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata/copylock" ==> using copylock = go.cmd.vet.testdata.copylock_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\copylock\copylock.go
using sync = go.sync_package;

namespace go.cmd.vet.testdata;

public static partial class copylock_package {

public static void BadFunc() {
    ptr<sync.Mutex> x;
    var p = x;
    ref sync.Mutex y = ref heap(out ptr<sync.Mutex> _addr_y);
    _addr_p = _addr_y;
    p = ref _addr_p.val;
    p.val = x.val; // ERROR "assignment copies lock value to \*p: sync.Mutex"
}

} // end copylock_package
