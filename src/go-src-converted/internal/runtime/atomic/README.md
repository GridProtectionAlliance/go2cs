# go.internal.runtime.atomic

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package atomic provides atomic operations, independent of sync/atomic, to the runtime.

On most platforms, the compiler is aware of the functions defined in this package, and they're replaced with platform-specific intrinsics. On other platforms, generic implementations are made available.

Unless otherwise noted, operations defined in this package are sequentially consistent across threads with respect to the values they manipulate. More specifically, operations that happen in a specific order on one thread, will always be observed to happen in exactly that order by another thread.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
