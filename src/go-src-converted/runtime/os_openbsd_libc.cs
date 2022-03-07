// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd_libc.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static slice<byte> failThreadCreate = (slice<byte>)"runtime: failed to create new OS thread\n";

// mstart_stub provides glue code to call mstart from pthread_create.
private static void mstart_stub();

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrierrec
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (false) {>>MARKER:FUNCTION_mstart_stub_BLOCK_PREFIX<<
        print("newosproc m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
    }
    ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);
    {
        var err__prev1 = err;

        var err = pthread_attr_init(_addr_attr);

        if (err != 0) {
            write(2, @unsafe.Pointer(_addr_failThreadCreate[0]), int32(len(failThreadCreate)));
            exit(1);
        }
        err = err__prev1;

    } 

    // Find out OS stack size for our own stack guard.
    ref System.UIntPtr stacksize = ref heap(out ptr<System.UIntPtr> _addr_stacksize);
    if (pthread_attr_getstacksize(_addr_attr, _addr_stacksize) != 0) {
        write(2, @unsafe.Pointer(_addr_failThreadCreate[0]), int32(len(failThreadCreate)));
        exit(1);
    }
    mp.g0.stack.hi = stacksize; // for mstart

    // Tell the pthread library we won't join with this thread.
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        write(2, @unsafe.Pointer(_addr_failThreadCreate[0]), int32(len(failThreadCreate)));
        exit(1);
    }
    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    err = pthread_create(_addr_attr, funcPC(mstart_stub), @unsafe.Pointer(mp));
    sigprocmask(_SIG_SETMASK, _addr_oset, null);
    if (err != 0) {
        write(2, @unsafe.Pointer(_addr_failThreadCreate[0]), int32(len(failThreadCreate)));
        exit(1);
    }
    pthread_attr_destroy(_addr_attr);

}

} // end runtime_package
