// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 386

// package runtime -- go2cs converted at 2020 October 08 03:24:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\sys_x86.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // adjust Gobuf as if it executed a call to fn with context ctxt
        // and then did an immediate gosave.
        private static void gostartcall(ptr<gobuf> _addr_buf, unsafe.Pointer fn, unsafe.Pointer ctxt)
        {
            ref gobuf buf = ref _addr_buf.val;

            var sp = buf.sp;
            if (sys.RegSize > sys.PtrSize)
            {
                sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

                0L;

            }
            sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

            buf.pc;
            buf.sp = sp;
            buf.pc = uintptr(fn);
            buf.ctxt = ctxt;

        }
    }
}
