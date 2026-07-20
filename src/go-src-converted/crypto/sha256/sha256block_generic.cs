// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !386 && !s390x && !ppc64le && !ppc64 && !arm64) || purego
namespace go.crypto;

partial class sha256_package {

internal static void block(ж<digest> Ꮡdig, slice<byte> p) {
    blockGeneric(Ꮡdig, p);
}

} // end sha256_package
