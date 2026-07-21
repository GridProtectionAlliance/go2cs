# go.go.doc.comment

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package comment implements parsing and reformatting of Go doc comments, (documentation comments), which are comments that immediately precede a top-level declaration of a package, const, func, type, or var.

Go doc comment syntax is a simplified subset of Markdown that supports links, headings, paragraphs, lists (without nesting), and preformatted text blocks. The details of the syntax are documented at [https://go.dev/doc/comment](https://go.dev/doc/comment).

To parse the text associated with a doc comment (after removing comment markers), use a \[Parser]:

	var p comment.Parser
	doc := p.Parse(text)

The result is a \[\*Doc]. To reformat it as a doc comment, HTML, Markdown, or plain text, use a \[Printer]:

	var pr comment.Printer
	os.Stdout.Write(pr.Text(doc))

The \[Parser] and \[Printer] types are structs whose fields can be modified to customize the operations. For details, see the documentation for those types.

Use cases that need additional control over reformatting can implement their own logic by inspecting the parsed syntax itself. See the documentation for \[Doc], \[Block], \[Text] for an overview and links to additional types.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
