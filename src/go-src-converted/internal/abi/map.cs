// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

// Map constants common to several packages
// runtime/runtime-gdb.py:MapTypePrinter contains its own copy
public static readonly UntypedInt MapBucketCountBits = 3; // log2 of number of elements in a bucket.

public static readonly UntypedInt MapBucketCount = /* 1 << MapBucketCountBits */ 8;

public static readonly UntypedInt MapMaxKeyBytes = 128;

public static readonly UntypedInt MapMaxElemBytes = 128; // Must fit in a uint8.

} // end abi_package
