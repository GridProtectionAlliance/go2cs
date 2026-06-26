// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// Offsets into internal/cpu records for use in assembly.
internal const uintptr offsetX86HasAVX = /* unsafe.Offsetof(cpu.X86.HasAVX) */ 66;

internal const uintptr offsetX86HasAVX2 = /* unsafe.Offsetof(cpu.X86.HasAVX2) */ 67;

internal const uintptr offsetX86HasERMS = /* unsafe.Offsetof(cpu.X86.HasERMS) */ 73;

internal const uintptr offsetX86HasRDTSCP = /* unsafe.Offsetof(cpu.X86.HasRDTSCP) */ 78;

internal const uintptr offsetARMHasIDIVA = /* unsafe.Offsetof(cpu.ARM.HasIDIVA) */ 65;

internal const uintptr offsetMIPS64XHasMSA = /* unsafe.Offsetof(cpu.MIPS64X.HasMSA) */ 64;

internal static bool x86HasPOPCNT;
internal static bool x86HasSSE41;
internal static bool x86HasFMA;
internal static bool armHasVFPv4;
internal static bool arm64HasATOMICS;

} // end runtime_package
