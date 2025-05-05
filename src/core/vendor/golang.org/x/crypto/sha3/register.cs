// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build go1.4
namespace go.vendor.golang.org.x.crypto;

using crypto = crypto_package;

partial class sha3_package {

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA3_224, New224);
    crypto.RegisterHash(crypto.SHA3_256, New256);
    crypto.RegisterHash(crypto.SHA3_384, New384);
    crypto.RegisterHash(crypto.SHA3_512, New512);
}

} // end sha3_package
