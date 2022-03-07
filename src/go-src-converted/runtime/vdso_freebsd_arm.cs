// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\vdso_freebsd_arm.go


namespace go;

public static partial class runtime_package {

private static readonly nint _VDSO_TH_ALGO_ARM_GENTIM = 1;


private static uint getCntxct(bool physical);

//go:nosplit
private static (uint, bool) getTimecounter(this ptr<vdsoTimehands> _addr_th) {
    uint _p0 = default;
    bool _p0 = default;
    ref vdsoTimehands th = ref _addr_th.val;


    if (th.algo == _VDSO_TH_ALGO_ARM_GENTIM) 
        return (getCntxct(th.physical != 0), true);
    else 
        return (0, false);
    
}

} // end runtime_package
