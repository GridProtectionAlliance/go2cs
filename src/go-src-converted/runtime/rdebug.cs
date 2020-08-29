// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\rdebug.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    { // for go:linkname

        //go:linkname setMaxStack runtime/debug.setMaxStack
        private static long setMaxStack(long @in)
        {
            out = int(maxstacksize);
            maxstacksize = uintptr(in);
            return out;
        }

        //go:linkname setPanicOnFault runtime/debug.setPanicOnFault
        private static bool setPanicOnFault(bool @new)
        {
            var _g_ = getg();
            old = _g_.paniconfault;
            _g_.paniconfault = new;
            return old;
        }
    }
}
