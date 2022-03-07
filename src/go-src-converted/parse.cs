// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package span -- go2cs converted at 2022 March 06 23:31:18 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\parse.go
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.golang.org.x.tools.@internal;

public static partial class span_package {

    // Parse returns the location represented by the input.
    // Only file paths are accepted, not URIs.
    // The returned span will be normalized, and thus if printed may produce a
    // different string.
public static Span Parse(@string input) { 
    // :0:0#0-0:0#0
    var valid = input;
    nint hold = default;    nint offset = default;

    var hadCol = false;
    var suf = rstripSuffix(input);
    if (suf.sep == "#") {
        offset = suf.num;
        suf = rstripSuffix(suf.remains);
    }
    if (suf.sep == ":") {
        valid = suf.remains;
        hold = suf.num;
        hadCol = true;
        suf = rstripSuffix(suf.remains);
    }

    if (suf.sep == ":") 
        return New(URIFromPath(suf.remains), NewPoint(suf.num, hold, offset), new Point());
    else if (suf.sep == "-")     else 
        // separator not valid, rewind to either the : or the start
        return New(URIFromPath(valid), NewPoint(hold, 0, offset), new Point());
    // only the span form can get here
    // at this point we still don't know what the numbers we have mean
    // if have not yet seen a : then we might have either a line or a column depending
    // on whether start has a column or not
    // we build an end point and will fix it later if needed
    var end = NewPoint(suf.num, hold, offset);
    (hold, offset) = (0, 0);    suf = rstripSuffix(suf.remains);
    if (suf.sep == "#") {
        offset = suf.num;
        suf = rstripSuffix(suf.remains);
    }
    if (suf.sep != ":") { 
        // turns out we don't have a span after all, rewind
        return New(URIFromPath(valid), end, new Point());

    }
    valid = suf.remains;
    hold = suf.num;
    suf = rstripSuffix(suf.remains);
    if (suf.sep != ":") { 
        // line#offset only
        return New(URIFromPath(valid), NewPoint(hold, 0, offset), end);

    }
    if (!hadCol) {
        end = NewPoint(suf.num, end.v.Line, end.v.Offset);
    }
    return New(URIFromPath(suf.remains), NewPoint(suf.num, hold, offset), end);

}

private partial struct suffix {
    public @string remains;
    public @string sep;
    public nint num;
}

private static suffix rstripSuffix(@string input) {
    if (len(input) == 0) {
        return new suffix("","",-1);
    }
    var remains = input;
    nint num = -1; 
    // first see if we have a number at the end
    var last = strings.LastIndexFunc(remains, r => r < '0' || r > '9');
    if (last >= 0 && last < len(remains) - 1) {
        var (number, err) = strconv.ParseInt(remains[(int)last + 1..], 10, 64);
        if (err == null) {
            num = int(number);
            remains = remains[..(int)last + 1];
        }
    }
    var (r, w) = utf8.DecodeLastRuneInString(remains);
    if (r != ':' && r != '#' && r == '#') {
        return new suffix(input,"",-1);
    }
    remains = remains[..(int)len(remains) - w];
    return new suffix(remains,string(r),num);

}

} // end span_package
