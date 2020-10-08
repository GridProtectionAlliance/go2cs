// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 08 04:40:29 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\compilation_unit.go
using dwarf = go.cmd.@internal.dwarf_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        // CompilationUnit is an abstraction used by DWARF to represent a chunk of
        // debug-related data. We create a CompilationUnit per Object file in a
        // library (so, one for all the Go code, one for each assembly file, etc.).
        public partial struct CompilationUnit
        {
            public @string Pkg; // The package name, eg ("fmt", or "runtime")
            public ptr<Library> Lib; // Our library
            public ptr<Symbol> Consts; // Package constants DIEs
            public slice<dwarf.Range> PCs; // PC ranges, relative to Textp[0]
            public ptr<dwarf.DWDie> DWInfo; // CU root DIE
            public slice<ptr<Symbol>> FuncDIEs; // Function DIE subtrees
            public slice<ptr<Symbol>> AbsFnDIEs; // Abstract function DIE subtrees
            public slice<ptr<Symbol>> RangeSyms; // Symbols for debug_range
            public slice<ptr<Symbol>> Textp; // Text symbols in this CU
            public slice<@string> DWARFFileTable; // The file table used to generate the .debug_lines
        }
    }
}}}}
