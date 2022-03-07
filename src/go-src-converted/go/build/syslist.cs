// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package build -- go2cs converted at 2022 March 06 22:42:55 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Program Files\Go\src\go\build\syslist.go


namespace go.go;

public static partial class build_package {

    // List of past, present, and future known GOOS and GOARCH values.
    // Do not remove from this list, as these are used for go/build filename matching.
private static readonly @string goosList = "aix android darwin dragonfly freebsd hurd illumos ios js linux nacl netbsd openbsd plan9 solaris windows zos ";

private static readonly @string goarchList = "386 amd64 amd64p32 arm armbe arm64 arm64be ppc64 ppc64le loong64 mips mipsle mips64 mips64le mips64p32 mips64p32le ppc riscv riscv64 s390 s390x sparc sparc64 wasm ";


} // end build_package
