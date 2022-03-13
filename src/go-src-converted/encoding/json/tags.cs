// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2022 March 13 05:39:56 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\tags.go
namespace go.encoding;

using strings = strings_package;


// tagOptions is the string following a comma in a struct field's "json"
// tag, or the empty string. It does not include the leading comma.

public static partial class json_package {

private partial struct tagOptions { // : @string
}

// parseTag splits a struct field's json tag into its name and
// comma-separated options.
private static (@string, tagOptions) parseTag(@string tag) {
    @string _p0 = default;
    tagOptions _p0 = default;

    {
        var idx = strings.Index(tag, ",");

        if (idx != -1) {
            return (tag[..(int)idx], tagOptions(tag[(int)idx + 1..]));
        }
    }
    return (tag, tagOptions(""));
}

// Contains reports whether a comma-separated list of options
// contains a particular substr flag. substr must be surrounded by a
// string boundary or commas.
private static bool Contains(this tagOptions o, @string optionName) {
    if (len(o) == 0) {
        return false;
    }
    var s = string(o);
    while (s != "") {
        @string next = default;
        var i = strings.Index(s, ",");
        if (i >= 0) {
            (s, next) = (s[..(int)i], s[(int)i + 1..]);
        }
        if (s == optionName) {
            return true;
        }
        s = next;
    }
    return false;
}

} // end json_package
