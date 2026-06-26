// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !purego
namespace go.crypto;

partial class subtle_package {

//go:noescape
internal static partial void xorBytes(ж<byte> dst, ж<byte> a, ж<byte> b, nint n);

} // end subtle_package
