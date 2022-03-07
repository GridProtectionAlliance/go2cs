// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable.go


namespace go;

public static partial class os_package {

    // Executable returns the path name for the executable that started
    // the current process. There is no guarantee that the path is still
    // pointing to the correct executable. If a symlink was used to start
    // the process, depending on the operating system, the result might
    // be the symlink or the path it pointed to. If a stable result is
    // needed, path/filepath.EvalSymlinks might help.
    //
    // Executable returns an absolute path unless an error occurred.
    //
    // The main use case is finding resources located relative to an
    // executable.
public static (@string, error) Executable() {
    @string _p0 = default;
    error _p0 = default!;

    return executable();
}

} // end os_package
