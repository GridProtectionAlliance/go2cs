// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typeutil -- go2cs converted at 2020 October 08 04:55:43 UTC
// import "golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\types\typeutil\imports.go
using types = go.go.types_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace types
{
    public static partial class typeutil_package
    {
        // Dependencies returns all dependencies of the specified packages.
        //
        // Dependent packages appear in topological order: if package P imports
        // package Q, Q appears earlier than P in the result.
        // The algorithm follows import statements in the order they
        // appear in the source code, so the result is a total order.
        //
        public static slice<ptr<types.Package>> Dependencies(params ptr<ptr<types.Package>>[] _addr_pkgs)
        {
            pkgs = pkgs.Clone();
            ref types.Package pkgs = ref _addr_pkgs.val;

            slice<ptr<types.Package>> result = default;
            var seen = make_map<ptr<types.Package>, bool>();
            Action<slice<ptr<types.Package>>> visit = default;
            visit = pkgs =>
            {
                foreach (var (_, p) in pkgs)
                {
                    if (!seen[p])
                    {
                        seen[p] = true;
                        visit(p.Imports());
                        result = append(result, p);
                    }
                }
            };
            visit(pkgs);
            return result;

        }
    }
}}}}}}
