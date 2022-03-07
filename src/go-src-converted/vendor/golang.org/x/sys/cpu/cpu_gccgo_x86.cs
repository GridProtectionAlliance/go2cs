// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (386 || amd64 || amd64p32) && gccgo
// +build 386 amd64 amd64p32
// +build gccgo

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_gccgo_x86.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    //extern gccgoGetCpuidCount
private static void gccgoGetCpuidCount(uint eaxArg, uint ecxArg, ptr<uint> eax, ptr<uint> ebx, ptr<uint> ecx, ptr<uint> edx);

private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg) {
    uint eax = default;
    uint ebx = default;
    uint ecx = default;
    uint edx = default;

    ref uint a = ref heap(out ptr<uint> _addr_a);    ref uint b = ref heap(out ptr<uint> _addr_b);    ref uint c = ref heap(out ptr<uint> _addr_c);    ref uint d = ref heap(out ptr<uint> _addr_d);

    gccgoGetCpuidCount(eaxArg, ecxArg, _addr_a, _addr_b, _addr_c, _addr_d);
    return (a, b, c, d);
}

//extern gccgoXgetbv
private static void gccgoXgetbv(ptr<uint> eax, ptr<uint> edx);

private static (uint, uint) xgetbv() {
    uint eax = default;
    uint edx = default;

    ref uint a = ref heap(out ptr<uint> _addr_a);    ref uint d = ref heap(out ptr<uint> _addr_d);

    gccgoXgetbv(_addr_a, _addr_d);
    return (a, d);
}

// gccgo doesn't build on Darwin, per:
// https://github.com/Homebrew/homebrew-core/blob/HEAD/Formula/gcc.rb#L76
private static bool darwinSupportsAVX512() {
    return false;
}

} // end cpu_package
