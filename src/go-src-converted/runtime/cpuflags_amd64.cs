// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class runtime_package {

internal static bool useAVXmemmove;

[GoInit] internal static void initΔ1() {
    // Let's remove stepping and reserved fields
    var processor = (uint32)(processorVersionInfo & 0x0FFF3FF0);
    var isIntelBridgeFamily = isIntel && processor == 0x206A0 || processor == 0x206D0 || processor == 0x306A0 || processor == 0x306E0;
    useAVXmemmove = cpu.X86.HasAVX && !isIntelBridgeFamily;
}

} // end runtime_package
