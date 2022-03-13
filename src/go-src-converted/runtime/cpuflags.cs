// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:24:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cpuflags.go
namespace go;

using cpu = @internal.cpu_package;
using @unsafe = @unsafe_package;


// Offsets into internal/cpu records for use in assembly.

public static partial class runtime_package {

private static readonly var offsetX86HasAVX = @unsafe.Offsetof(cpu.X86.HasAVX);
private static readonly var offsetX86HasAVX2 = @unsafe.Offsetof(cpu.X86.HasAVX2);
private static readonly var offsetX86HasERMS = @unsafe.Offsetof(cpu.X86.HasERMS);
private static readonly var offsetX86HasSSE2 = @unsafe.Offsetof(cpu.X86.HasSSE2);

private static readonly var offsetARMHasIDIVA = @unsafe.Offsetof(cpu.ARM.HasIDIVA);

private static readonly var offsetMIPS64XHasMSA = @unsafe.Offsetof(cpu.MIPS64X.HasMSA);

 
// Set in runtime.cpuinit.
// TODO: deprecate these; use internal/cpu directly.
private static bool x86HasPOPCNT = default;private static bool x86HasSSE41 = default;private static bool x86HasFMA = default;private static bool armHasVFPv4 = default;private static bool arm64HasATOMICS = default;

} // end runtime_package
