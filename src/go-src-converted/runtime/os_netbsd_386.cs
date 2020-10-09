// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_netbsd_386.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void lwp_mcontext_init(ptr<mcontextt> _addr_mc, unsafe.Pointer stk, ptr<m> _addr_mp, ptr<g> _addr_gp, System.UIntPtr fn)
        {
            ref mcontextt mc = ref _addr_mc.val;
            ref m mp = ref _addr_mp.val;
            ref g gp = ref _addr_gp.val;
 
            // Machine dependent mcontext initialisation for LWP.
            mc.__gregs[_REG_EIP] = uint32(funcPC(lwp_tramp));
            mc.__gregs[_REG_UESP] = uint32(uintptr(stk));
            mc.__gregs[_REG_EBX] = uint32(uintptr(@unsafe.Pointer(mp)));
            mc.__gregs[_REG_EDX] = uint32(uintptr(@unsafe.Pointer(gp)));
            mc.__gregs[_REG_ESI] = uint32(fn);

        }
    }
}
