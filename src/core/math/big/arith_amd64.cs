// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !math_big_pure_go
namespace go.math;

using cpu = @internal.cpu_package;
using @internal;

partial class big_package {

internal static bool support_adx = cpu.X86.HasADX && cpu.X86.HasBMI2;

} // end big_package
