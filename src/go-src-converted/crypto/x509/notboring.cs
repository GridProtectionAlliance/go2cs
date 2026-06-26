// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !boringcrypto
namespace go.crypto;

partial class x509_package {

internal static bool boringAllowCert(ж<Certificate> Ꮡc) {
    ref var c = ref Ꮡc.val;

    return true;
}

} // end x509_package
