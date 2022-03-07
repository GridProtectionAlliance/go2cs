// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (386 || amd64 || amd64p32) && gc
// +build 386 amd64 amd64p32
// +build gc

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_gc_x86.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // cpuid is implemented in cpu_x86.s for gc compiler
    // and in cpu_gccgo.c for gccgo.
private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg);

// xgetbv with ecx = 0 is implemented in cpu_x86.s for gc compiler
// and in cpu_gccgo.c for gccgo.
private static (uint, uint) xgetbv();

// darwinSupportsAVX512 is implemented in cpu_x86.s for gc compiler
// and in cpu_gccgo_x86.go for gccgo.
private static bool darwinSupportsAVX512();

} // end cpu_package
