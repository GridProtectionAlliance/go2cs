// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2022 March 13 06:30:31 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\load\search.go
namespace go.cmd.go.@internal;

using filepath = path.filepath_package;
using strings = strings_package;

using search = cmd.go.@internal.search_package;


// MatchPackage(pattern, cwd)(p) reports whether package p matches pattern in the working directory cwd.

using System;
public static partial class load_package {

public static Func<ptr<Package>, bool> MatchPackage(@string pattern, @string cwd) {

    if (search.IsRelativePath(pattern)) 
        // Split pattern into leading pattern-free directory path
        // (including all . and .. elements) and the final pattern.
        @string dir = default;
        var i = strings.Index(pattern, "...");
        if (i < 0) {
            (dir, pattern) = (pattern, "");
        }
        else
 {
            var j = strings.LastIndex(pattern[..(int)i], "/");
            (dir, pattern) = (pattern[..(int)j], pattern[(int)j + 1..]);
        }
        dir = filepath.Join(cwd, dir);
        if (pattern == "") {
            return p => p.Dir == dir;
        }
        var matchPath = search.MatchPattern(pattern);
        return p => { 
            // Compute relative path to dir and see if it matches the pattern.
            var (rel, err) = filepath.Rel(dir, p.Dir);
            if (err != null) { 
                // Cannot make relative - e.g. different drive letters on Windows.
                return false;
            }
            rel = filepath.ToSlash(rel);
            if (rel == ".." || strings.HasPrefix(rel, "../")) {
                return false;
            }
            return matchPath(rel);
        };
    else if (pattern == "all") 
        return p => true;
    else if (pattern == "std") 
        return p => p.Standard;
    else if (pattern == "cmd") 
        return p => p.Standard && strings.HasPrefix(p.ImportPath, "cmd/");
    else 
        matchPath = search.MatchPattern(pattern);
        return p => matchPath(p.ImportPath);
    }

} // end load_package
