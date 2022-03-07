// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd_arm64.go


namespace go;

public static partial class runtime_package {

    //go:nosplit
private static long cputicks() { 
    // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
    // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();

}

} // end runtime_package
