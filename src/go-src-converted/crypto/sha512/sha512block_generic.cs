// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !ppc64 && !ppc64le && !riscv64 && !s390x) || purego
namespace go.crypto;

partial class sha512_package {

internal static void block(ж<digest> Ꮡdig, slice<byte> p) {
    blockGeneric(Ꮡdig, p);
}

} // end sha512_package
