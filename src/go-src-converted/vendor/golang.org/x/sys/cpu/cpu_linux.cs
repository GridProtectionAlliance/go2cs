// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64 && !amd64p32 && !arm64
// +build !386,!amd64,!amd64p32,!arm64

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_linux.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static void archInit() {
    {
        var err = readHWCAP();

        if (err != null) {
            return ;
        }
    }

    doinit();
    Initialized = true;

}

} // end cpu_package
