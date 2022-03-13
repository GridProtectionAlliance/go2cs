// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux && (ppc64 || ppc64le)
// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2022 March 13 05:26:05 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux_ppc64x.go
namespace go;

using cpu = @internal.cpu_package;

public static partial class runtime_package {

private static void archauxv(System.UIntPtr tag, System.UIntPtr val) {

    if (tag == _AT_HWCAP) 
        // ppc64x doesn't have a 'cpuid' instruction
        // equivalent and relies on HWCAP/HWCAP2 bits for
        // hardware capabilities.
        cpu.HWCap = uint(val);
    else if (tag == _AT_HWCAP2) 
        cpu.HWCap2 = uint(val);
    }

private static void osArchInit() {
}

} // end runtime_package
