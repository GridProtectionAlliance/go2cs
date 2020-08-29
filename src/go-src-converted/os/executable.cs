// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
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
        //
        // Executable is not supported on nacl.
        public static (@string, error) Executable()
        {
            return executable();
        }
    }
}
