# go.crypto.dsa

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package dsa implements the Digital Signature Algorithm, as defined in FIPS 186-3.

The DSA operations in this package are not implemented using constant-time algorithms.

Deprecated: DSA is a legacy algorithm, and modern alternatives such as Ed25519 (implemented by package crypto/ed25519) should be used instead. Keys with 1024-bit moduli (L1024N160 parameters) are cryptographically weak, while bigger keys are not widely supported. Note that FIPS 186-5 no longer approves DSA for signature generation.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
