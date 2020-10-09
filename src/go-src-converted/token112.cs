// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.12

// package span -- go2cs converted at 2020 October 09 06:01:32 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\token112.go
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class span_package
    {
        // TODO(rstambler): Delete this file when we no longer support Go 1.11.
        private static token.Pos lineStart(ptr<token.File> _addr_f, long line)
        {
            ref token.File f = ref _addr_f.val;

            return f.LineStart(line);
        }
    }
}}}}}
