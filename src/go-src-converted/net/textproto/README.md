# go.net.textproto

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package textproto implements generic support for text-based request/response protocols in the style of HTTP, NNTP, and SMTP.

The package provides:

\[Error], which represents a numeric error response from a server.

\[Pipeline], to manage pipelined requests and responses in a client.

\[Reader], to read numeric response code lines, key: value headers, lines wrapped with leading spaces on continuation lines, and whole text blocks ending with a dot on a line by itself.

\[Writer], to write dot-encoded text blocks.

\[Conn], a convenient packaging of \[Reader], \[Writer], and \[Pipeline] for use with a single network connection.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
