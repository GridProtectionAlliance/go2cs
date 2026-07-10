// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (linux && !386 && !amd64 && !arm && !arm64 && !loong64 && !mips64 && !mips64le && !ppc64 && !ppc64le && !riscv64 && !s390x) || !linux
namespace go;

partial class runtime_package {

// A dummy version of inVDSOPage for targets that don't use a VDSO.
internal static bool inVDSOPage(uintptr pc) {
    return false;
}

} // end runtime_package
