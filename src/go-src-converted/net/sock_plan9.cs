// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:40 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_plan9.go


namespace go;

public static partial class net_package {

private static nint maxListenerBacklog() { 
    // /sys/include/ape/sys/socket.h:/SOMAXCONN
    return 5;

}

} // end net_package
