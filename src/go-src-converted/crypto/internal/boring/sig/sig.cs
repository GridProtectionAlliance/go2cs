// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sig holds “code signatures” that can be called
// and will result in certain code sequences being linked into
// the final binary. The functions themselves are no-ops.
namespace go.crypto.@internal.boring;

partial class sig_package {

// BoringCrypto indicates that the BoringCrypto module is present.
public static partial void BoringCrypto();

// FIPSOnly indicates that package crypto/tls/fipsonly is present.
public static partial void FIPSOnly();

// StandardCrypto indicates that standard Go crypto is present.
public static partial void StandardCrypto();

} // end sig_package
