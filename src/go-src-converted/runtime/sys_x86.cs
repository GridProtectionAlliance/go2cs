// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 amd64p32 386

// package runtime -- go2cs converted at 2020 August 29 08:21:14 UTC
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
        private static void gostartcall(ref gobuf buf, unsafe.Pointer fn, unsafe.Pointer ctxt)
        {
            var sp = buf.sp;
            if (sys.RegSize > sys.PtrSize)
            {
                sp -= sys.PtrSize * (uintptr.Value)(@unsafe.Pointer(sp));

                0L;
            }
            sp -= sys.PtrSize * (uintptr.Value)(@unsafe.Pointer(sp));

            buf.pc;
            buf.sp = sp;
            buf.pc = uintptr(fn);
            buf.ctxt = ctxt;
        }
    }
}
