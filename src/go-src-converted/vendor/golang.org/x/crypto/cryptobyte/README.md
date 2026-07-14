# go.vendor.golang.org.x.crypto.cryptobyte

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package cryptobyte contains types that help with parsing and constructing length-prefixed, binary messages, including ASN.1 DER. (The asn1 subpackage contains useful ASN.1 constants.)

The String type is for parsing. It wraps a \[]byte slice and provides helper functions for consuming structures, value by value.

The Builder type is for constructing messages. It providers helper functions for appending values and also for appending length-prefixed submessages – without having to worry about calculating the length prefix ahead of time.

See the documentation and examples for the Builder and String types to get started.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
