# Background

I've been programming in C# for many years, even so, I try to keep an eye on new tech being developed, regardless of language -- one common choice for new applications these days is Go. Often the easiest way for me to experiment with new technology is to integrate it with some of my existing C# code, hence the desire for this project -- besides, I figured I might learn something about building apps in Go in the process.

Hopefully (once it's complete) for those of you that code in Go all the time and have an occasion where you need to build a C# app, this conversion tool will provide you with a head start. You can pull in some of your existing code and even use the Go Standard Library functions that you are used to.

## Converted Code

TL;DR: Do not expect converted C# code to run as fast as the original Go code.

The .NET runtime environment is very different from Go -- a compiled .NET application actually consists of an abstract byte-code based instruction set called [Common Intermediate Language](https://en.wikipedia.org/wiki/Common_Intermediate_Language) (CIL). At runtime the CIL code is compiled to native machine code using a [just-in-time compilation](https://en.wikipedia.org/wiki/Just-in-time_compilation) process. Compared to Go, which is compiled directly to native machine code, .NET incurs some compile based processing delays at startup. However, you can remove the just-in-time compilation startup delays for a .NET application by pre-compiling the application to native machine code using the native image generation tool in .NET [`crossgen`](https://github.com/dotnet/runtime/blob/master/docs/workflow/building/coreclr/crossgen.md).

For this project, the philosophy taken for converted code is to produce code that is very visually similar to the original Go code as well as being as behaviorally close as possible to Go at a purely "source code" level. The focus is to make converted code more usable and understandable to a Go programmer inside of a C# environment without being hyper-focused on optimizations. That doesn't mean your converted code is going to be super slow -- it should run as fast as other comparable .NET applications -- it just may not run as fast as the original Go code. However, because of the simplicity of [Go's design](https://talks.golang.org/2012/splash.slide), converted code may have a natural performance advantage over more traditionally developed C# applications just because of its static nature.

If you are looking for a more _binary_ integration, you might consider using native compiled Go code directly within your .NET application. One option would be would be [exporting Go functions](https://golang.org/cmd/cgo/#hdr-C_references_to_Go) as C code, including the C code exports in a DLL and then [importing the functions](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.dllimportattribute%28v=vs.110%29.aspx) in C#.

## ANTLR4 Grammar

Go code is converted using ANTLR. Several improvements have been made to the [Golang ANTLR4 grammar](https://github.com/antlr/grammars-v4/tree/master/golang). Even so, initial attempts to convert all the Go Standard Library code still finds that the grammar needs some more work. If you update the grammar file [in this project](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go2cs/Parser), please kindly make a PR to update the original grammar on the [ANTLR site](https://github.com/antlr/grammars-v4) as well.
