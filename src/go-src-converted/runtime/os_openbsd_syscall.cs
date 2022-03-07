// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && mips64
// +build openbsd,mips64

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd_syscall.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    //go:noescape
private static int tfork(ptr<tforkt> param, System.UIntPtr psize, ptr<m> mm, ptr<g> gg, System.UIntPtr fn);

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    var stk = @unsafe.Pointer(mp.g0.stack.hi);
    if (false) {>>MARKER:FUNCTION_tfork_BLOCK_PREFIX<<
        print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
    }
    ref tforkt param = ref heap(new tforkt(tf_tcb:unsafe.Pointer(&mp.tls[0]),tf_tid:nil,tf_stack:uintptr(stk)-sys.PtrSize,), out ptr<tforkt> _addr_param);

    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    var ret = tfork(_addr_param, @unsafe.Sizeof(param), _addr_mp, _addr_mp.g0, funcPC(mstart));
    sigprocmask(_SIG_SETMASK, _addr_oset, null);

    if (ret < 0) {
        print("runtime: failed to create new OS thread (have ", mcount() - 1, " already; errno=", -ret, ")\n");
        if (ret == -_EAGAIN) {
            println("runtime: may need to increase max user processes (ulimit -p)");
        }
        throw("runtime.newosproc");

    }
}

} // end runtime_package
