# go.internal.saferio

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package saferio provides I/O functions that avoid allocating large amounts of memory unnecessarily. This is intended for packages that read data from an [io.Reader](/io#Reader) where the size is part of the input data but the input may be corrupt, or may be provided by an untrustworthy attacker.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
