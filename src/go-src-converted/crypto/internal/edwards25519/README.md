# go.crypto.internal.edwards25519

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package edwards25519 implements group logic for the twisted Edwards curve

	-x^2 + y^2 = 1 + -(121665/121666)*x^2*y^2

This is better known as the Edwards curve equivalent to Curve25519, and is the curve used by the Ed25519 signature scheme.

Most users don't need this package, and should instead use crypto/ed25519 for signatures, golang.org/x/crypto/curve25519 for Diffie-Hellman, or github.com/gtank/ristretto255 for prime order group logic.

However, developers who do need to interact with low-level edwards25519 operations can use filippo.io/edwards25519, an extended version of this package repackaged as an importable module.

(Note that filippo.io/edwards25519 and github.com/gtank/ristretto255 are not maintained by the Go team and are not covered by the Go 1 Compatibility Promise.)

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
