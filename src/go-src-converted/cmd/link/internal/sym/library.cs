// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 08 04:37:53 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\library.go
using goobj2 = go.cmd.@internal.goobj2_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class sym_package
    {
        public partial struct Library
        {
            public @string Objref;
            public @string Srcref;
            public @string File;
            public @string Pkg;
            public @string Shlib;
            public @string Hash;
            public goobj2.FingerprintType Fingerprint;
            public slice<goobj2.ImportedPkg> Autolib;
            public slice<ptr<Library>> Imports;
            public bool Main;
            public bool Safe;
            public slice<ptr<CompilationUnit>> Units;
            public slice<LoaderSym> Textp2; // text syms defined in this library
            public slice<LoaderSym> DupTextSyms2; // dupok text syms defined in this library
        }

        public static @string String(this Library l)
        {
            return l.Pkg;
        }
    }
}}}}
