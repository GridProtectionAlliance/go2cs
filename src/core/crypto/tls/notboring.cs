// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !boringcrypto
namespace go.crypto;

partial class tls_package {

internal static bool needFIPS() {
    return false;
}

} // end tls_package
