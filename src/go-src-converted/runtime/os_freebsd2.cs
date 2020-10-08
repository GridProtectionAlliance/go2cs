// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd,!amd64

// package runtime -- go2cs converted at 2020 October 08 03:21:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_freebsd2.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = sigset_all;
            if (fn == funcPC(sighandler))
            {
                fn = funcPC(sigtramp);
            }
            sa.sa_handler = fn;
            sigaction(i, _addr_sa, null);

        }
    }
}
