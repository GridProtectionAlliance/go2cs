// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 13 05:28:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys.go
namespace go;

public static partial class os_package {

// Hostname returns the host name reported by the kernel.
public static (@string, error) Hostname() {
    @string name = default;
    error err = default!;

    return hostname();
}

} // end os_package
