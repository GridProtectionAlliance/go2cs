# go.internal.runtime.exithook

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package exithook provides limited support for on-exit cleanup.

CAREFUL! The expectation is that Add should only be called from a safe context (e.g. not an error/panic path or signal handler, preemption enabled, allocation allowed, write barriers allowed, etc), and that the exit function F will be invoked under similar circumstances. That is the say, we are expecting that F uses normal / high-level Go code as opposed to one of the more restricted dialects used for the trickier parts of the runtime.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
