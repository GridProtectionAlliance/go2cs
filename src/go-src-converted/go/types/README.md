# go.go.types

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package types declares the data types and implements the algorithms for type-checking of Go packages. Use \[Config.Check] to invoke the type checker for a package. Alternatively, create a new type checker with \[NewChecker] and invoke it incrementally by calling \[Checker.Files].

Type-checking consists of several interdependent phases:

Name resolution maps each identifier (\[ast.Ident]) in the program to the symbol (\[Object]) it denotes. Use the Defs and Uses fields of \[Info] or the \[Info.ObjectOf] method to find the symbol for an identifier, and use the Implicits field of \[Info] to find the symbol for certain other kinds of syntax node.

Constant folding computes the exact constant value (\[constant.Value]) of every expression (\[ast.Expr]) that is a compile-time constant. Use the Types field of \[Info] to find the results of constant folding for an expression.

Type deduction computes the type (\[Type]) of every expression (\[ast.Expr]) and checks for compliance with the language specification. Use the Types field of \[Info] for the results of type deduction.

For a tutorial, see [https://go.dev/s/types-tutorial](https://go.dev/s/types-tutorial).

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
