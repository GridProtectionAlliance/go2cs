// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !s390x && !ppc64 && !ppc64le && !arm64) || purego
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using go.crypto;

partial class aes_package {

// newCipher calls the newCipherGeneric function
// directly. Platforms with hardware accelerated
// implementations of AES should implement their
// own version of newCipher (which may then call
// newCipherGeneric if needed).
internal static (cipher.Block, error) newCipher(slice<byte> key) {
    return newCipherGeneric(key);
}

// expandKey is used by BenchmarkExpand and should
// call an assembly implementation if one is available.
internal static void expandKey(slice<byte> key, slice<uint32> enc, slice<uint32> dec) {
    expandKeyGo(key, enc, dec);
}

} // end aes_package
