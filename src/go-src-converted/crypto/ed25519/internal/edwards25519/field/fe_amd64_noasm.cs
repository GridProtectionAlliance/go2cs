// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 || !gc || purego
// +build !amd64 !gc purego

// package field -- go2cs converted at 2022 March 13 05:30:47 UTC
// import "crypto/ed25519/internal/edwards25519/field" ==> using field = go.crypto.ed25519.@internal.edwards25519.field_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\field\fe_amd64_noasm.go
namespace go.crypto.ed25519.@internal.edwards25519;

public static partial class field_package {

private static void feMul(ptr<Element> _addr_v, ptr<Element> _addr_x, ptr<Element> _addr_y) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;
    ref Element y = ref _addr_y.val;

    feMulGeneric(v, x, y);
}

private static void feSquare(ptr<Element> _addr_v, ptr<Element> _addr_x) {
    ref Element v = ref _addr_v.val;
    ref Element x = ref _addr_x.val;

    feSquareGeneric(v, x);
}

} // end field_package
