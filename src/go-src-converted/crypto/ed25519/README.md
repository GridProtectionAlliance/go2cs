# go.crypto.ed25519

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package ed25519 implements the Ed25519 signature algorithm. See [https://ed25519.cr.yp.to/](https://ed25519.cr.yp.to/).

These functions are also compatible with the “Ed25519” function defined in RFC 8032. However, unlike RFC 8032's formulation, this package's private key representation includes a public key suffix to make multiple signing operations with the same key more efficient. This package refers to the RFC 8032 private key as the “seed”.

Operations involving private keys are implemented using constant-time algorithms.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
