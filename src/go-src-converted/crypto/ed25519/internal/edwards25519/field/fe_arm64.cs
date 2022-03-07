// Copyright (c) 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build arm64 && gc && !purego
// +build arm64,gc,!purego

// package field -- go2cs converted at 2022 March 06 22:17:26 UTC
// import "crypto/ed25519/internal/edwards25519/field" ==> using field = go.crypto.ed25519.@internal.edwards25519.field_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\field\fe_arm64.go


namespace go.crypto.ed25519.@internal.edwards25519;

public static partial class field_package {

    //go:noescape
private static void carryPropagate(ptr<Element> v);

private static ptr<Element> carryPropagate(this ptr<Element> _addr_v) {
    ref Element v = ref _addr_v.val;

    carryPropagate(_addr_v);
    return _addr_v!;
}

} // end field_package
