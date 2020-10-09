// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.12

// package span -- go2cs converted at 2020 October 09 06:01:32 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\token111.go
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
        // lineStart is the pre-Go 1.12 version of (*token.File).LineStart. For Go
        // versions <= 1.11, we borrow logic from the analysisutil package.
        // TODO(rstambler): Delete this file when we no longer support Go 1.11.
        private static token.Pos lineStart(ptr<token.File> _addr_f, long line)
        {
            ref token.File f = ref _addr_f.val;
 
            // Use binary search to find the start offset of this line.

            long min = 0L; // inclusive
            var max = f.Size(); // exclusive
            while (true)
            {
                var offset = (min + max) / 2L;
                var pos = f.Pos(offset);
                var posn = f.Position(pos);
                if (posn.Line == line)
                {
                    return pos - (token.Pos(posn.Column) - 1L);
                }
                if (min + 1L >= max)
                {
                    return token.NoPos;
                }
                if (posn.Line < line)
                {
                    min = offset;
                }
                else
                {
                    max = offset;
                }
            }

        }
    }
}}}}}
