// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements convertions between *types.Node and *Node.
// TODO(gri) try to eliminate these soon

// package gc -- go2cs converted at 2020 October 08 04:31:37 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\types_acc.go
using types = go.cmd.compile.@internal.types_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ptr<Node> asNode(ptr<types.Node> _addr_n)
        {
            ref types.Node n = ref _addr_n.val;

            return _addr_(Node.val)(@unsafe.Pointer(n))!;
        }
        private static ptr<types.Node> asTypesNode(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return _addr_(types.Node.val)(@unsafe.Pointer(n))!;
        }
    }
}}}}
