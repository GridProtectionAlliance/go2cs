# go.vendor.golang.org.x.crypto.internal.poly1305

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package poly1305 implements Poly1305 one-time message authentication code as specified in [https://cr.yp.to/mac/poly1305-20050329.pdf](https://cr.yp.to/mac/poly1305-20050329.pdf).

Poly1305 is a fast, one-time authentication function. It is infeasible for an attacker to generate an authenticator for a message without the key. However, a key must only be used for a single message. Authenticating two different messages with the same key allows an attacker to forge authenticators for other messages with the same key.

Poly1305 was originally coupled with AES in order to make Poly1305-AES. AES was used with a fixed key in order to generate one-time keys from an nonce. However, in this package AES isn't used and the one-time key is specified directly.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
