// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 09 05:51:12 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\library.go

using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
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
            public slice<@string> ImportStrings;
            public slice<ptr<Library>> Imports;
            public slice<ptr<Symbol>> Textp; // text symbols defined in this library
            public slice<ptr<Symbol>> DupTextSyms; // dupok text symbols defined in this library
            public bool Main;
            public bool Safe;
            public slice<ptr<CompilationUnit>> Units;
        }

        public static @string String(this Library l)
        {
            return l.Pkg;
        }
    }
}}}}
