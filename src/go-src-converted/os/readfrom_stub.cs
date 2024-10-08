// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !linux
// +build !linux

// package os -- go2cs converted at 2022 March 13 05:28:04 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\readfrom_stub.go
namespace go;

using io = io_package;

public static partial class os_package {

private static (long, bool, error) readFrom(this ptr<File> _addr_f, io.Reader r) {
    long n = default;
    bool handled = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    return (0, false, error.As(null!)!);
}

} // end os_package
