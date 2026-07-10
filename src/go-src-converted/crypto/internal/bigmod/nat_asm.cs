// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego && (386 || amd64 || arm || arm64 || ppc64 || ppc64le || riscv64 || s390x)
namespace go.crypto.@internal;

using cpu = go.@internal.cpu_package;
using go.@internal;

partial class bigmod_package {

// amd64 assembly uses ADCX/ADOX/MULX if ADX is available to run two carry
// chains in the flags in parallel across the whole operation, and aggressively
// unrolls loops. arm64 processes four words at a time.
//
// It's unclear why the assembly for all other architectures, as well as for
// amd64 without ADX, perform better than the compiler output.
// TODO(filippo): file cmd/compile performance issue.
internal static bool supportADX = cpu.X86.HasADX && cpu.X86.HasBMI2;

//go:noescape
internal static partial nuint /*c*/ addMulVVW1024(ж<nuint> z, ж<nuint> x, nuint y);

//go:noescape
internal static partial nuint /*c*/ addMulVVW1536(ж<nuint> z, ж<nuint> x, nuint y);

//go:noescape
internal static partial nuint /*c*/ addMulVVW2048(ж<nuint> z, ж<nuint> x, nuint y);

} // end bigmod_package
