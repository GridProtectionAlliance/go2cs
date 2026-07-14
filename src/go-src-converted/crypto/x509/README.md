# go.crypto.x509

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package x509 implements a subset of the X.509 standard.

It allows parsing and generating certificates, certificate signing requests, certificate revocation lists, and encoded public and private keys. It provides a certificate verifier, complete with a chain builder.

The package targets the X.509 technical profile defined by the IETF (RFC 2459/3280/5280), and as further restricted by the CA/Browser Forum Baseline Requirements. There is minimal support for features outside of these profiles, as the primary goal of the package is to provide compatibility with the publicly trusted TLS certificate ecosystem and its policies and constraints.

On macOS and Windows, certificate verification is handled by system APIs, but the package aims to apply consistent validation rules across operating systems.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
