// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 && !purego && gc
namespace go.vendor.golang.org.x.crypto;

partial class sha3_package {

// This function is implemented in keccakf_amd64.s.
//go:noescape
internal static partial void keccakF1600(ж<array<uint64>> a);

} // end sha3_package
