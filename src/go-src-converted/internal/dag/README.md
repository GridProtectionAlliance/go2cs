# go.internal.dag

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package dag implements a language for expressing directed acyclic graphs.

The general syntax of a rule is:

	a, b < c, d;

which means c and d come after a and b in the partial order (that is, there are edges from c and d to a and b), but doesn't provide a relative order between a vs b or c vs d.

The rules can chain together, as in:

	e < f, g < h;

which is equivalent to

	e < f, g;
	f, g < h;

Except for the special bottom element "NONE", each name must appear exactly once on the right-hand side of any rule. That rule serves as the definition of the allowed successor for that name. The definition must appear before any uses of the name on the left-hand side of a rule. (That is, the rules themselves must be ordered according to the partial order, for easier reading by people.)

Negative assertions double-check the partial order:

	i !< j

means that it must NOT be the case that i \< j. Negative assertions may appear anywhere in the rules, even before i and j have been defined.

Comments begin with #.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
