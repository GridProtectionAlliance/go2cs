// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64) || purego
namespace go.crypto.@internal;

using errors = errors_package;

partial class nistec_package {

public static (slice<byte>, error) P256OrdInverse(slice<byte> k) {
    return (default!, errors.New("unimplemented"u8));
}

} // end nistec_package
