// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package printer -- go2cs converted at 2022 March 06 22:46:57 UTC
// import "go/printer" ==> using printer = go.go.printer_package
// Original source: C:\Program Files\Go\src\go\printer\gobuild.go
using constraint = go.go.build.constraint_package;
using sort = go.sort_package;
using tabwriter = go.text.tabwriter_package;

namespace go.go;

public static partial class printer_package {

private static void fixGoBuildLines(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;

    if (len(p.goBuild) + len(p.plusBuild) == 0) {
        return ;
    }
    nint insert = 0;
    {
        nint pos__prev1 = pos;

        nint pos = 0;

        while (toDelete) { 
            // Skip leading space at beginning of line.
            var blank = true;
            while (pos < len(p.output) && (p.output[pos] == ' ' || p.output[pos] == '\t')) {
                pos++;
            } 
            // Skip over // comment if any.
            if (pos + 3 < len(p.output) && p.output[pos] == tabwriter.Escape && p.output[pos + 1] == '/' && p.output[pos + 2] == '/') {
                blank = false;
                while (pos < len(p.output) && !isNL(p.output[pos])) {
                    pos++;
                }
            }
            if (pos >= len(p.output) || !isNL(p.output[pos])) {
                break;
            }
            pos++;

            if (blank) {
                insert = pos;
            }
        }

        pos = pos__prev1;
    } 

    // If there is a //go:build comment before the place we identified,
    // use that point instead. (Earlier in the file is always fine.)
    if (len(p.goBuild) > 0 && p.goBuild[0] < insert) {
        insert = p.goBuild[0];
    }
    else if (len(p.plusBuild) > 0 && p.plusBuild[0] < insert) {
        insert = p.plusBuild[0];
    }
    constraint.Expr x = default;
    switch (len(p.goBuild)) {
        case 0: 
            // Synthesize //go:build expression from // +build lines.
                   {
                       nint pos__prev1 = pos;

                       foreach (var (_, __pos) in p.plusBuild) {
                           pos = __pos;
                           var (y, err) = constraint.Parse(p.commentTextAt(pos));
                           if (err != null) {
                               x = null;
                               break;
                           }
                           if (x == null) {
                               x = y;
                           }
                           else
            {
                               x = addr(new constraint.AndExpr(X:x,Y:y));
                           }
                       }
                       pos = pos__prev1;
                   }
            break;
        case 1: 
            // Parse //go:build expression.
            x, _ = constraint.Parse(p.commentTextAt(p.goBuild[0]));
            break;
    }

    slice<byte> block = default;
    if (x == null) { 
        // Don't have a valid //go:build expression to treat as truth.
        // Bring all the lines together but leave them alone.
        // Note that these are already tabwriter-escaped.
        {
            nint pos__prev1 = pos;

            foreach (var (_, __pos) in p.goBuild) {
                pos = __pos;
                block = append(block, p.lineAt(pos));
            }
    else

            pos = pos__prev1;
        }

        {
            nint pos__prev1 = pos;

            foreach (var (_, __pos) in p.plusBuild) {
                pos = __pos;
                block = append(block, p.lineAt(pos));
            }
            pos = pos__prev1;
        }
    } {
        block = append(block, tabwriter.Escape);
        block = append(block, "//go:build ");
        block = append(block, x.String());
        block = append(block, tabwriter.Escape, '\n');
        if (len(p.plusBuild) > 0) {
            var (lines, err) = constraint.PlusBuildLines(x);
            if (err != null) {
                lines = new slice<@string>(new @string[] { "// +build error: "+err.Error() });
            }
            foreach (var (_, line) in lines) {
                block = append(block, tabwriter.Escape);
                block = append(block, line);
                block = append(block, tabwriter.Escape, '\n');
            }
        }
    }
    block = append(block, '\n'); 

    // Build sorted list of lines to delete from remainder of output.
    var toDelete = append(p.goBuild, p.plusBuild);
    sort.Ints(toDelete); 

    // Collect output after insertion point, with lines deleted, into after.
    slice<byte> after = default;
    var start = insert;
    foreach (var (_, end) in toDelete) {
        if (end < start) {
            continue;
        }
        after = appendLines(after, p.output[(int)start..(int)end]);
        start = end + len(p.lineAt(end));

    }    after = appendLines(after, p.output[(int)start..]);
    {
        var n = len(after);

        if (n >= 2 && isNL(after[n - 1]) && isNL(after[n - 2])) {
            after = after[..(int)n - 1];
        }
    }


    p.output = p.output[..(int)insert];
    p.output = append(p.output, block);
    p.output = append(p.output, after);

}

// appendLines is like append(x, y...)
// but it avoids creating doubled blank lines,
// which would not be gofmt-standard output.
// It assumes that only whole blocks of lines are being appended,
// not line fragments.
private static slice<byte> appendLines(slice<byte> x, slice<byte> y) {
    if (len(y) > 0 && isNL(y[0]) && (len(x) == 0 || len(x) >= 2 && isNL(x[len(x) - 1]) && isNL(x[len(x) - 2]))) { // x is empty or ends in blank line
        y = y[(int)1..]; // delete y's leading blank line
    }
    return append(x, y);

}

private static slice<byte> lineAt(this ptr<printer> _addr_p, nint start) {
    ref printer p = ref _addr_p.val;

    var pos = start;
    while (pos < len(p.output) && !isNL(p.output[pos])) {
        pos++;
    }
    if (pos < len(p.output)) {
        pos++;
    }
    return p.output[(int)start..(int)pos];

}

private static @string commentTextAt(this ptr<printer> _addr_p, nint start) {
    ref printer p = ref _addr_p.val;

    if (start < len(p.output) && p.output[start] == tabwriter.Escape) {
        start++;
    }
    var pos = start;
    while (pos < len(p.output) && p.output[pos] != tabwriter.Escape && !isNL(p.output[pos])) {
        pos++;
    }
    return string(p.output[(int)start..(int)pos]);

}

private static bool isNL(byte b) {
    return b == '\n' || b == '\f';
}

} // end printer_package
