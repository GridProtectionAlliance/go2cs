// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2022 March 06 23:20:34 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\sym\compilation_unit.go
using dwarf = go.cmd.@internal.dwarf_package;

namespace go.cmd.link.@internal;

public static partial class sym_package {

    // LoaderSym holds a loader.Sym value. We can't refer to this
    // type from the sym package since loader imports sym.
public partial struct LoaderSym { // : nint
}

// A CompilationUnit represents a set of source files that are compiled
// together. Since all Go sources in a Go package are compiled together,
// there's one CompilationUnit per package that represents all Go sources in
// that package, plus one for each assembly file.
//
// Equivalently, there's one CompilationUnit per object file in each Library
// loaded by the linker.
//
// These are used for both DWARF and pclntab generation.
public partial struct CompilationUnit {
    public ptr<Library> Lib; // Our library
    public nint PclnIndex; // Index of this CU in pclntab
    public slice<dwarf.Range> PCs; // PC ranges, relative to Textp[0]
    public ptr<dwarf.DWDie> DWInfo; // CU root DIE
    public slice<@string> FileTable; // The file table used in this compilation unit.

    public LoaderSym Consts; // Package constants DIEs
    public slice<LoaderSym> FuncDIEs; // Function DIE subtrees
    public slice<LoaderSym> VarDIEs; // Global variable DIEs
    public slice<LoaderSym> AbsFnDIEs; // Abstract function DIE subtrees
    public slice<LoaderSym> RangeSyms; // Symbols for debug_range
    public slice<LoaderSym> Textp; // Text symbols in this CU
}

} // end sym_package
