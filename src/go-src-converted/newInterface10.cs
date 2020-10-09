// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.11

// package gcimporter -- go2cs converted at 2020 October 09 06:02:20 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\newInterface10.go
using types = go.go.types_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        private static ptr<types.Interface> newInterface(slice<ptr<types.Func>> methods, slice<types.Type> embeddeds) => func((_, panic, __) =>
        {
            var named = make_slice<ptr<types.Named>>(len(embeddeds));
            foreach (var (i, e) in embeddeds)
            {
                bool ok = default;
                named[i], ok = e._<ptr<types.Named>>();
                if (!ok)
                {
                    panic("embedding of non-defined interfaces in interfaces is not supported before Go 1.11");
                }
            }            return _addr_types.NewInterface(methods, named)!;

        });
    }
}}}}}}
