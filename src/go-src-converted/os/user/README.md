# go.os.user

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package user allows user account lookups by name or id.

For most Unix systems, this package has two internal implementations of resolving user and group ids to names, and listing supplementary group IDs. One is written in pure Go and parses /etc/passwd and /etc/group. The other is cgo-based and relies on the standard C library (libc) routines such as getpwuid\_r, getgrnam\_r, and getgrouplist.

When cgo is available, and the required routines are implemented in libc for a particular platform, cgo-based (libc-backed) code is used. This can be overridden by using osusergo build tag, which enforces the pure Go implementation.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
