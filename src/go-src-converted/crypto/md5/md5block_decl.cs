// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (amd64 || 386 || arm || ppc64le || ppc64 || s390x || arm64) && !purego
namespace go.crypto;

partial class md5_package {

internal const bool haveAsm = true;

//go:noescape
internal static partial void block(ж<digest> dig, slice<byte> p);

} // end md5_package
