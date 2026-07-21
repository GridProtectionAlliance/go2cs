# go.go.parser

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package parser implements a parser for Go source files. Input may be provided in a variety of forms (see the various Parse\* functions); the output is an abstract syntax tree (AST) representing the Go source. The parser is invoked through one of the Parse\* functions.

The parser accepts a larger language than is syntactically permitted by the Go spec, for simplicity, and for improved robustness in the presence of syntax errors. For instance, in method declarations, the receiver is treated like an ordinary parameter list and thus may contain multiple entries where the spec permits exactly one. Consequently, the corresponding field in the AST (ast.FuncDecl.Recv) field is not restricted to one entry.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
