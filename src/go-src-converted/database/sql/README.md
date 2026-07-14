# go.database.sql

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package sql provides a generic interface around SQL (or SQL-like) databases.

The sql package must be used in conjunction with a database driver. See [https://golang.org/s/sqldrivers](https://golang.org/s/sqldrivers) for a list of drivers.

Drivers that do not support context cancellation will not return until after the query is completed.

For usage examples, see the wiki page at [https://golang.org/s/sqlwiki](https://golang.org/s/sqlwiki).

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
