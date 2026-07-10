// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (386 || amd64 || s390x || ppc64le || ppc64) && !purego
namespace go.crypto;

partial class sha256_package {

//go:noescape
internal static partial void block(ж<digest> dig, slice<byte> p);

} // end sha256_package
