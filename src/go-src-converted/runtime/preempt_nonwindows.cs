// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !windows
// +build !windows

// package runtime -- go2cs converted at 2022 March 06 22:10:50 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\preempt_nonwindows.go


namespace go;

public static partial class runtime_package {

    //go:nosplit
private static void osPreemptExtEnter(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//go:nosplit
private static void osPreemptExtExit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

} // end runtime_package
