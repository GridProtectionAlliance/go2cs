// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego
namespace go.crypto;

using cpu = @internal.cpu_package;
using @internal;

partial class sha256_package {

internal static bool useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI2;

internal static bool useSHA = useAVX2 && cpu.X86.HasSHA;

} // end sha256_package
