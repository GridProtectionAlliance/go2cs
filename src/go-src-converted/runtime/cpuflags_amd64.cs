// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cpuflags_amd64.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class runtime_package {

private static bool useAVXmemmove = default;

private static void init() { 
    // Let's remove stepping and reserved fields
    var processor = processorVersionInfo & 0x0FFF3FF0;

    var isIntelBridgeFamily = isIntel && processor == 0x206A0 || processor == 0x206D0 || processor == 0x306A0 || processor == 0x306E0;

    useAVXmemmove = cpu.X86.HasAVX && !isIntelBridgeFamily;

}

} // end runtime_package
