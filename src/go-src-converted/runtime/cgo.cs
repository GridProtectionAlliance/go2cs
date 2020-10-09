// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cgo.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:cgo_export_static main

        // Filled in by runtime/cgo when linked into binary.

        //go:linkname _cgo_init _cgo_init
        //go:linkname _cgo_thread_start _cgo_thread_start
        //go:linkname _cgo_sys_thread_create _cgo_sys_thread_create
        //go:linkname _cgo_notify_runtime_init_done _cgo_notify_runtime_init_done
        //go:linkname _cgo_callers _cgo_callers
        //go:linkname _cgo_set_context_function _cgo_set_context_function
        //go:linkname _cgo_yield _cgo_yield
        private static unsafe.Pointer _cgo_init = default;        private static unsafe.Pointer _cgo_thread_start = default;        private static unsafe.Pointer _cgo_sys_thread_create = default;        private static unsafe.Pointer _cgo_notify_runtime_init_done = default;        private static unsafe.Pointer _cgo_callers = default;        private static unsafe.Pointer _cgo_set_context_function = default;        private static unsafe.Pointer _cgo_yield = default;

        // iscgo is set to true by the runtime/cgo package
        private static bool iscgo = default;

        // cgoHasExtraM is set on startup when an extra M is created for cgo.
        // The extra M must be created before any C/C++ code calls cgocallback.
        private static bool cgoHasExtraM = default;

        // cgoUse is called by cgo-generated code (using go:linkname to get at
        // an unexported name). The calls serve two purposes:
        // 1) they are opaque to escape analysis, so the argument is considered to
        // escape to the heap.
        // 2) they keep the argument alive until the call site; the call is emitted after
        // the end of the (presumed) use of the argument by C.
        // cgoUse should not actually be called (see cgoAlwaysFalse).
        private static void cgoUse(object _p0)
        {
            throw("cgoUse should not be called");
        }

        // cgoAlwaysFalse is a boolean value that is always false.
        // The cgo-generated code says if cgoAlwaysFalse { cgoUse(p) }.
        // The compiler cannot see that cgoAlwaysFalse is always false,
        // so it emits the test and keeps the call, giving the desired
        // escape analysis result. The test is cheaper than the call.
        private static bool cgoAlwaysFalse = default;

        private static var cgo_yield = _addr__cgo_yield;
    }
}
