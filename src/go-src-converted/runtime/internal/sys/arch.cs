// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2022 March 13 05:24:01 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Program Files\Go\src\runtime\internal\sys\arch.go
namespace go.runtime.@internal;

public static partial class sys_package {

public partial struct ArchFamilyType { // : nint
}

public static readonly ArchFamilyType AMD64 = iota;
public static readonly var ARM = 0;
public static readonly var ARM64 = 1;
public static readonly var I386 = 2;
public static readonly var MIPS = 3;
public static readonly var MIPS64 = 4;
public static readonly var PPC64 = 5;
public static readonly var RISCV64 = 6;
public static readonly var S390X = 7;
public static readonly var WASM = 8;

// PtrSize is the size of a pointer in bytes - unsafe.Sizeof(uintptr(0)) but as an ideal constant.
// It is also the size of the machine's native word size (that is, 4 on 32-bit systems, 8 on 64-bit).
public static readonly nint PtrSize = 4 << (int)((~uintptr(0) >> 63));

// AIX requires a larger stack for syscalls.


// AIX requires a larger stack for syscalls.
public static readonly var StackGuardMultiplier = StackGuardMultiplierDefault * (1 - GoosAix) + 2 * GoosAix;

// ArchFamily is the architecture family (AMD64, ARM, ...)


// ArchFamily is the architecture family (AMD64, ARM, ...)
public static readonly ArchFamilyType ArchFamily = _ArchFamily;

// BigEndian reports whether the architecture is big-endian.


// BigEndian reports whether the architecture is big-endian.
public static readonly var BigEndian = GoarchArmbe | GoarchArm64be | GoarchMips | GoarchMips64 | GoarchPpc | GoarchPpc64 | GoarchS390 | GoarchS390x | GoarchSparc | GoarchSparc64 == 1;

// DefaultPhysPageSize is the default physical page size.


// DefaultPhysPageSize is the default physical page size.
public static readonly var DefaultPhysPageSize = _DefaultPhysPageSize;

// PCQuantum is the minimal unit for a program counter (1 on x86, 4 on most other systems).
// The various PC tables record PC deltas pre-divided by PCQuantum.


// PCQuantum is the minimal unit for a program counter (1 on x86, 4 on most other systems).
// The various PC tables record PC deltas pre-divided by PCQuantum.
public static readonly var PCQuantum = _PCQuantum;

// Int64Align is the required alignment for a 64-bit integer (4 on 32-bit systems, 8 on 64-bit).


// Int64Align is the required alignment for a 64-bit integer (4 on 32-bit systems, 8 on 64-bit).
public static readonly var Int64Align = PtrSize;

// MinFrameSize is the size of the system-reserved words at the bottom
// of a frame (just above the architectural stack pointer).
// It is zero on x86 and PtrSize on most non-x86 (LR-based) systems.
// On PowerPC it is larger, to cover three more reserved words:
// the compiler word, the link editor word, and the TOC save word.


// MinFrameSize is the size of the system-reserved words at the bottom
// of a frame (just above the architectural stack pointer).
// It is zero on x86 and PtrSize on most non-x86 (LR-based) systems.
// On PowerPC it is larger, to cover three more reserved words:
// the compiler word, the link editor word, and the TOC save word.
public static readonly var MinFrameSize = _MinFrameSize;

// StackAlign is the required alignment of the SP register.
// The stack must be at least word aligned, but some architectures require more.


// StackAlign is the required alignment of the SP register.
// The stack must be at least word aligned, but some architectures require more.
public static readonly var StackAlign = _StackAlign;


} // end sys_package
