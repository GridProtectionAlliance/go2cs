// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (js && wasm) || plan9
// +build js,wasm plan9

// On plan9, per http://9p.io/magic/man2html/2/access: “Since file permissions
// are checked by the server and group information is not known to the client,
// access must open the file to check permissions.”
//
// aix and js,wasm are similar, in that they do not define syscall.Access.

// package modload -- go2cs converted at 2022 March 06 23:18:30 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\stat_openfile.go
using fs = go.io.fs_package;
using os = go.os_package;

namespace go.cmd.go.@internal;

public static partial class modload_package {

    // hasWritePerm reports whether the current user has permission to write to the
    // file with the given info.
private static bool hasWritePerm(@string path, fs.FileInfo _) {
    {
        var (f, err) = os.OpenFile(path, os.O_WRONLY, 0);

        if (err == null) {
            f.Close();
            return true;
        }
    }

    return false;

}

} // end modload_package
