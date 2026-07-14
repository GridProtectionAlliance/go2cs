# go.index.suffixarray

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package suffixarray implements substring search in logarithmic time using an in-memory suffix array.

Example use:

	// create index for some data
	index := suffixarray.New(data)

	// lookup byte slice s
	offsets1 := index.Lookup(s, -1) // the list of all indices where s occurs in data
	offsets2 := index.Lookup(s, 3)  // the list of at most 3 indices where s occurs in data

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
