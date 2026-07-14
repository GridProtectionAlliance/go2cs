# go.log.slog.internal.benchmarks

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package benchmarks contains benchmarks for slog.

These benchmarks are loosely based on github.com/uber-go/zap/benchmarks. They have the following desirable properties:

  - They test a complete log event, from the user's call to its return.

  - The benchmarked code is run concurrently in multiple goroutines, to better simulate a real server (the most common environment for structured logs).

  - Some handlers are optimistic versions of real handlers, doing real-world tasks as fast as possible (and sometimes faster, in that an implementation may not be concurrency-safe). This gives us an upper bound on handler performance, so we can evaluate the (handler-independent) core activity of the package in an end-to-end context without concern that a slow handler implementation is skewing the results.

  - We also test the built-in handlers, for comparison.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
