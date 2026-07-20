// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 || purego
namespace go.crypto.@internal.edwards25519;

partial class field_package {

internal static void feMul(ж<Element> Ꮡv, ж<Element> Ꮡx, ж<Element> Ꮡy) {
    feMulGeneric(Ꮡv, Ꮡx, Ꮡy);
}

internal static void feSquare(ж<Element> Ꮡv, ж<Element> Ꮡx) {
    feSquareGeneric(Ꮡv, Ꮡx);
}

} // end field_package
