// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !386 && !arm && !s390x && !arm64) || purego
namespace go.crypto;

partial class sha1_package {

internal static void block(ж<digest> Ꮡdig, slice<byte> p) {
    blockGeneric(Ꮡdig, p);
}

} // end sha1_package
