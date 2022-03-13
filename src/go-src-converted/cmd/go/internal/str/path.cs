// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package str -- go2cs converted at 2022 March 13 06:29:59 UTC
// import "cmd/go/internal/str" ==> using str = go.cmd.go.@internal.str_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\str\path.go
namespace go.cmd.go.@internal;

using filepath = path.filepath_package;
using strings = strings_package;


// HasPathPrefix reports whether the slash-separated path s
// begins with the elements in prefix.

public static partial class str_package {

public static bool HasPathPrefix(@string s, @string prefix) {
    if (len(s) == len(prefix)) {
        return s == prefix;
    }
    if (prefix == "") {
        return true;
    }
    if (len(s) > len(prefix)) {
        if (prefix[len(prefix) - 1] == '/' || s[len(prefix)] == '/') {
            return s[..(int)len(prefix)] == prefix;
        }
    }
    return false;
}

// HasFilePathPrefix reports whether the filesystem path s
// begins with the elements in prefix.
public static bool HasFilePathPrefix(@string s, @string prefix) {
    var sv = strings.ToUpper(filepath.VolumeName(s));
    var pv = strings.ToUpper(filepath.VolumeName(prefix));
    s = s[(int)len(sv)..];
    prefix = prefix[(int)len(pv)..];

    if (sv != pv) 
        return false;
    else if (len(s) == len(prefix)) 
        return s == prefix;
    else if (prefix == "") 
        return true;
    else if (len(s) > len(prefix)) 
        if (prefix[len(prefix) - 1] == filepath.Separator) {
            return strings.HasPrefix(s, prefix);
        }
        return s[len(prefix)] == filepath.Separator && s[..(int)len(prefix)] == prefix;
    else 
        return false;
    }

} // end str_package
