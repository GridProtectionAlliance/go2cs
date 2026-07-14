# go.net.mail

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package mail implements parsing of mail messages.

For the most part, this package follows the syntax as specified by RFC 5322 and extended by RFC 6532. Notable divergences:

  - Obsolete address formats are not parsed, including addresses with embedded route information.
  - The full range of spacing (the CFWS syntax element) is not supported, such as breaking addresses across lines.
  - No unicode normalization is performed.
  - A leading From line is permitted, as in mbox format (RFC 4155).

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
