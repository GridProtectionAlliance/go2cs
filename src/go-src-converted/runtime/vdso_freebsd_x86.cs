// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build freebsd && (386 || amd64)
// +build freebsd
// +build 386 amd64

// package runtime -- go2cs converted at 2022 March 13 05:27:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_freebsd_x86.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;
using System;

public static partial class runtime_package {

private static readonly nint _VDSO_TH_ALGO_X86_TSC = 1;
private static readonly nint _VDSO_TH_ALGO_X86_HPET = 2;

private static readonly nint _HPET_DEV_MAP_MAX = 10;
private static readonly nuint _HPET_MAIN_COUNTER = 0xf0;/* Main counter register */

private static readonly @string hpetDevPath = "/dev/hpetX\x00";

private static array<System.UIntPtr> hpetDevMap = new array<System.UIntPtr>(_HPET_DEV_MAP_MAX);

//go:nosplit
private static uint getTSCTimecounter(this ptr<vdsoTimehands> _addr_th) {
    ref vdsoTimehands th = ref _addr_th.val;

    var tsc = cputicks();
    if (th.x86_shift > 0) {
        tsc>>=th.x86_shift;
    }
    return uint32(tsc);
}

//go:systemstack
private static (uint, bool) getHPETTimecounter(this ptr<vdsoTimehands> _addr_th) {
    uint _p0 = default;
    bool _p0 = default;
    ref vdsoTimehands th = ref _addr_th.val;

    const @string digits = "0123456789";



    var idx = int(th.x86_hpet_idx);
    if (idx >= len(hpetDevMap)) {
        return (0, false);
    }
    var p = atomic.Loaduintptr(_addr_hpetDevMap[idx]);
    if (p == 0) {
        array<byte> devPath = new array<byte>(len(hpetDevPath));
        copy(devPath[..], hpetDevPath);
        devPath[9] = digits[idx];

        var fd = open(_addr_devPath[0], 0, 0);
        if (fd < 0) {
            atomic.Casuintptr(_addr_hpetDevMap[idx], 0, ~uintptr(0));
            return (0, false);
        }
        var (addr, mmapErr) = mmap(null, physPageSize, _PROT_READ, _MAP_SHARED, fd, 0);
        closefd(fd);
        var newP = uintptr(addr);
        if (mmapErr != 0) {
            newP = ~uintptr(0);
        }
        if (!atomic.Casuintptr(_addr_hpetDevMap[idx], 0, newP) && mmapErr == 0) {
            munmap(addr, physPageSize);
        }
        p = atomic.Loaduintptr(_addr_hpetDevMap[idx]);
    }
    if (p == ~uintptr(0)) {
        return (0, false);
    }
    return (new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(p + _HPET_MAIN_COUNTER)), true);
}

//go:nosplit
private static (uint, bool) getTimecounter(this ptr<vdsoTimehands> _addr_th) {
    uint _p0 = default;
    bool _p0 = default;
    ref vdsoTimehands th = ref _addr_th.val;


    if (th.algo == _VDSO_TH_ALGO_X86_TSC) 
        return (th.getTSCTimecounter(), true);
    else if (th.algo == _VDSO_TH_ALGO_X86_HPET) 
        uint tc = default;        bool ok = default;
        systemstack(() => {
            tc, ok = th.getHPETTimecounter();
        });
        return (tc, ok);
    else 
        return (0, false);
    }

} // end runtime_package
