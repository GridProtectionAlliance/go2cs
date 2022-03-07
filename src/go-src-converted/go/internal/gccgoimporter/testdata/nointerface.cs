// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nointerface -- go2cs converted at 2022 March 06 23:32:49 UTC
// import "go/internal/gccgoimporter.nointerface" ==> using nointerface = go.go.@internal.gccgoimporter.nointerface_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\nointerface.go


namespace go.go.@internal;

public static partial class nointerface_package {

public partial struct I { // : nint
}

//go:nointerface
private static nint Get(this ptr<I> _addr_p) {
    ref I p = ref _addr_p.val;

    return int(p.val);
}

private static void Set(this ptr<I> _addr_p, nint v) {
    ref I p = ref _addr_p.val;

    p.val = I(v);
}

} // end nointerface_package
