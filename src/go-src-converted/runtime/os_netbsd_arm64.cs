// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_netbsd_arm64.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static void lwp_mcontext_init(ptr<mcontextt> _addr_mc, unsafe.Pointer stk, ptr<m> _addr_mp, ptr<g> _addr_gp, System.UIntPtr fn) {
    ref mcontextt mc = ref _addr_mc.val;
    ref m mp = ref _addr_mp.val;
    ref g gp = ref _addr_gp.val;
 
    // Machine dependent mcontext initialisation for LWP.
    mc.__gregs[_REG_ELR] = uint64(funcPC(lwp_tramp));
    mc.__gregs[_REG_X31] = uint64(uintptr(stk));
    mc.__gregs[_REG_X0] = uint64(uintptr(@unsafe.Pointer(mp)));
    mc.__gregs[_REG_X1] = uint64(uintptr(@unsafe.Pointer(mp.g0)));
    mc.__gregs[_REG_X2] = uint64(fn);

}

//go:nosplit
private static long cputicks() { 
    // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
    // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();

}

} // end runtime_package
