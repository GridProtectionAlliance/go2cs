// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build purego || !(386 || amd64 || arm || arm64 || ppc64 || ppc64le || riscv64 || s390x)
namespace go.crypto.@internal;

using @unsafe = unsafe_package;

partial class bigmod_package {

internal static nuint /*c*/ addMulVVW1024(ж<nuint> Ꮡz, ж<nuint> Ꮡx, nuint y) {
    nuint c = default!;

    return addMulVVW(@unsafe.Slice(Ꮡz, 1024 / _W), @unsafe.Slice(Ꮡx, 1024 / _W), y);
}

internal static nuint /*c*/ addMulVVW1536(ж<nuint> Ꮡz, ж<nuint> Ꮡx, nuint y) {
    nuint c = default!;

    return addMulVVW(@unsafe.Slice(Ꮡz, 1536 / _W), @unsafe.Slice(Ꮡx, 1536 / _W), y);
}

internal static nuint /*c*/ addMulVVW2048(ж<nuint> Ꮡz, ж<nuint> Ꮡx, nuint y) {
    nuint c = default!;

    return addMulVVW(@unsafe.Slice(Ꮡz, 2048 / _W), @unsafe.Slice(Ꮡx, 2048 / _W), y);
}

} // end bigmod_package
