// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

public static readonly UntypedInt StackNosplitBase = 800;
// We have three different sequences for stack bounds checks, depending on
// whether the stack frame of a function is small, big, or huge.
public static readonly UntypedInt StackSmall = 128;
public static readonly UntypedInt StackBig = 4096;

} // end abi_package
