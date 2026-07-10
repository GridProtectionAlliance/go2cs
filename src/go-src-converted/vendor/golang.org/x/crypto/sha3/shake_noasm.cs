// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !gc || purego || !s390x
namespace go.vendor.golang.org.x.crypto;

partial class sha3_package {

internal static ж<state> newShake128() {
    return newShake128Generic();
}

internal static ж<state> newShake256() {
    return newShake256Generic();
}

} // end sha3_package
