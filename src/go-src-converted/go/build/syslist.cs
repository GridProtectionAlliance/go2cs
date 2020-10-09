// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package build -- go2cs converted at 2020 October 09 05:20:01 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Go\src\go\build\syslist.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class build_package
    {
        // List of past, present, and future known GOOS and GOARCH values.
        // Do not remove from this list, as these are used for go/build filename matching.
        private static readonly @string goosList = (@string)"aix android darwin dragonfly freebsd hurd illumos js linux nacl netbsd openbsd plan9 solaris windows zos ";

        private static readonly @string goarchList = (@string)"386 amd64 amd64p32 arm armbe arm64 arm64be ppc64 ppc64le mips mipsle mips64 mips64le mips64p32 mips64p32le ppc riscv riscv64 s390 s390x sparc sparc64 wasm ";

    }
}}
