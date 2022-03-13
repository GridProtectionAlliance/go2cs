// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 13 06:32:32 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\path.go
namespace go.cmd.go.@internal;

using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using System;

public static partial class @base_package {

private static @string cwd = default;
private static sync.Once cwdOnce = default;

// Cwd returns the current working directory at the time of the first call.
public static @string Cwd() {
    cwdOnce.Do(() => {
        error err = default!;
        cwd, err = os.Getwd();
        if (err != null) {
            Fatalf("cannot determine current directory: %v", err);
        }
    });
    return cwd;
}

// ShortPath returns an absolute or relative name for path, whatever is shorter.
public static @string ShortPath(@string path) {
    {
        var (rel, err) = filepath.Rel(Cwd(), path);

        if (err == null && len(rel) < len(path)) {
            return rel;
        }
    }
    return path;
}

// RelPaths returns a copy of paths with absolute paths
// made relative to the current directory if they would be shorter.
public static slice<@string> RelPaths(slice<@string> paths) {
    slice<@string> @out = default;
    foreach (var (_, p) in paths) {
        var (rel, err) = filepath.Rel(Cwd(), p);
        if (err == null && len(rel) < len(p)) {
            p = rel;
        }
        out = append(out, p);
    }    return out;
}

// IsTestFile reports whether the source file is a set of tests and should therefore
// be excluded from coverage analysis.
public static bool IsTestFile(@string file) { 
    // We don't cover tests, only the code they test.
    return strings.HasSuffix(file, "_test.go");
}

} // end @base_package
