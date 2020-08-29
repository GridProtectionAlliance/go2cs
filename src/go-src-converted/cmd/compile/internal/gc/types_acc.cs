// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements convertions between *types.Node and *Node.
// TODO(gri) try to eliminate these soon

// package gc -- go2cs converted at 2020 August 29 09:29:47 UTC
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
        private static ref Node asNode(ref types.Node n)
        {
            return (Node.Value)(@unsafe.Pointer(n));
        }
        private static ref types.Node asTypesNode(ref Node n)
        {
            return (types.Node.Value)(@unsafe.Pointer(n));
        }
    }
}}}}
