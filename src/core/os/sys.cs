// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class os_package {

// Hostname returns the host name reported by the kernel.
public static (@string name, error err) Hostname() {
    @string name = default!;
    error err = default!;

    return hostname();
}

} // end os_package
