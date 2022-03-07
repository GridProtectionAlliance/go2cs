// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cpuflags_arm64.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class runtime_package {

private static bool arm64UseAlignedLoads = default;

private static void init() {
    if (cpu.ARM64.IsNeoverseN1 || cpu.ARM64.IsZeus) {
        arm64UseAlignedLoads = true;
    }
}

} // end runtime_package
