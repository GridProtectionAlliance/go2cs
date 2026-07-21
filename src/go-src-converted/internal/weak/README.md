# go.internal.weak

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

The weak package is a package for managing weak pointers.

Weak pointers are pointers that explicitly do not keep a value live and must be queried for a regular Go pointer. The result of such a query may be observed as nil at any point after a weakly-pointed-to object becomes eligible for reclamation by the garbage collector. More specifically, weak pointers become nil as soon as the garbage collector identifies that the object is unreachable, before it is made reachable again by a finalizer. In terms of the C# language, these semantics are roughly equivalent to the the semantics of "short" weak references. In terms of the Java language, these semantics are roughly equivalent to the semantics of the WeakReference type.

Using go:linkname to access this package and the functions it references is explicitly forbidden by the toolchain because the semantics of this package have not gone through the proposal process. By exposing this functionality, we risk locking in the existing semantics due to Hyrum's Law.

If you believe you have a good use-case for weak references not already covered by the standard library, file a proposal issue at [https://github.com/golang/go/issues](https://github.com/golang/go/issues) instead of relying on this package.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
