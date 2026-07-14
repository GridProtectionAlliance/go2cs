# go.archive.zip

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package zip provides support for reading and writing ZIP archives.

See the [ZIP specification](https://support.pkware.com/pkzip/appnote) for details.

This package does not support disk spanning.

A note about ZIP64:

To be backwards compatible the FileHeader has both 32 and 64 bit Size fields. The 64 bit fields will always contain the correct value and for normal archives both fields will be the same. For files requiring the ZIP64 format the 32 bit fields will be 0xffffffff and the 64 bit fields must be used instead.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
