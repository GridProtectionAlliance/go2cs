// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_netbsd_amd64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void lwp_mcontext_init(ref mcontextt mc, unsafe.Pointer stk, ref m mp, ref g gp, System.UIntPtr fn)
        { 
            // Machine dependent mcontext initialisation for LWP.
            mc.__gregs[_REG_RIP] = uint64(funcPC(lwp_tramp));
            mc.__gregs[_REG_RSP] = uint64(uintptr(stk));
            mc.__gregs[_REG_R8] = uint64(uintptr(@unsafe.Pointer(mp)));
            mc.__gregs[_REG_R9] = uint64(uintptr(@unsafe.Pointer(gp)));
            mc.__gregs[_REG_R12] = uint64(fn);
        }
    }
}
