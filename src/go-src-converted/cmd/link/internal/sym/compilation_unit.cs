// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 08 04:37:53 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\compilation_unit.go
using dwarf = go.cmd.@internal.dwarf_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class sym_package
    {
        // LoaderSym holds a loader.Sym value. We can't refer to this
        // type from the sym package since loader imports sym.
        public partial struct LoaderSym // : long
        {
        }

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

            public LoaderSym Consts2; // Package constants DIEs (loader)
            public slice<LoaderSym> FuncDIEs2; // Function DIE subtrees (loader)
            public slice<LoaderSym> AbsFnDIEs2; // Abstract function DIE subtrees (loader)
            public slice<LoaderSym> RangeSyms2; // Symbols for debug_range (loader)
            public slice<LoaderSym> Textp2; // Text symbols in this CU (loader)
        }
    }
}}}}
