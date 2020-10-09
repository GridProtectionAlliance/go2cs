// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:40:37 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bimport.go
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // numImport tracks how often a package with a given name is imported.
        // It is used to provide a better error message (by using the package
        // path to disambiguate) if a package that appears multiple times with
        // the same name appears in an error message.
        private static var numImport = make_map<@string, long>();

        private static ptr<Node> npos(src.XPos pos, ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            n.Pos = pos;
            return _addr_n!;
        }

        private static ptr<Node> builtinCall(Op op)
        {
            return _addr_nod(OCALL, mkname(builtinpkg.Lookup(goopnames[op])), null)!;
        }
    }
}}}}
