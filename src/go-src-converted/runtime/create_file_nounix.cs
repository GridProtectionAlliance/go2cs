// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !unix
namespace go;

partial class runtime_package {

internal const bool canCreateFile = false;

internal static int32 create(ж<byte> Ꮡname, int32 perm) {
    @throw("unimplemented"u8);
    return -1;
}

} // end runtime_package
