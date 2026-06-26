// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @internal;

partial class runtime_package {

internal static bool useAVXmemmove;

[GoInit] internal static void init() {
    // Let's remove stepping and reserved fields
    var processor = (uint32)(processorVersionInfo & 268386288);
    var isIntelBridgeFamily = isIntel && processor == 132768 || processor == 132816 || processor == 198304 || processor == 198368;
    useAVXmemmove = cpu.X86.HasAVX && !isIntelBridgeFamily;
}

} // end runtime_package
