# go.crypto.internal.nistec

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package nistec implements the NIST P elliptic curves from FIPS 186-4.

This package uses fiat-crypto or specialized assembly and Go code for its backend field arithmetic (not math/big) and exposes constant-time, heap allocation-free, byte slice-based safe APIs. Group operations use modern and safe complete addition formulas where possible. The point at infinity is handled and encoded according to SEC 1, Version 2.0, and invalid curve points can't be represented.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
