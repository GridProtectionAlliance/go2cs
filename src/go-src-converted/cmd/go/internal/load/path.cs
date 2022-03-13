// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2022 March 13 06:30:19 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\load\path.go
namespace go.cmd.go.@internal;

using filepath = path.filepath_package;


// expandPath returns the symlink-expanded form of path.

public static partial class load_package {

private static @string expandPath(@string p) {
    var (x, err) = filepath.EvalSymlinks(p);
    if (err == null) {
        return x;
    }
    return p;
}

} // end load_package
