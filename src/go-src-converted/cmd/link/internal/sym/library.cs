// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 August 29 10:02:53 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\library.go

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
            public slice<@string> ImportStrings;
            public slice<ref Library> Imports;
            public slice<ref Symbol> Textp; // text symbols defined in this library
            public slice<ref Symbol> DupTextSyms; // dupok text symbols defined in this library
        }

        public static @string String(this Library l)
        {
            return l.Pkg;
        }
    }
}}}}
