// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2022 March 06 23:20:34 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\sym\library.go
using goobj = go.cmd.@internal.goobj_package;

namespace go.cmd.link.@internal;

public static partial class sym_package {

public partial struct Library {
    public @string Objref;
    public @string Srcref;
    public @string File;
    public @string Pkg;
    public @string Shlib;
    public goobj.FingerprintType Fingerprint;
    public slice<goobj.ImportedPkg> Autolib;
    public slice<ptr<Library>> Imports;
    public bool Main;
    public slice<ptr<CompilationUnit>> Units;
    public slice<LoaderSym> Textp; // text syms defined in this library
    public slice<LoaderSym> DupTextSyms; // dupok text syms defined in this library
}

public static @string String(this Library l) {
    return l.Pkg;
}

} // end sym_package
