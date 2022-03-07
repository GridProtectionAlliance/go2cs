// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.12

// package route -- go2cs converted at 2022 March 06 22:15:58 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\syscall_go1_12_darwin.go
using _@unsafe_ = go.@unsafe_package;

namespace go.golang.org.x.net;

public static partial class route_package {
 // for linkname

    //go:linkname sysctl syscall.sysctl
private static error sysctl(slice<int> mib, ptr<byte> old, ptr<System.UIntPtr> oldlen, ptr<byte> @new, System.UIntPtr newlen);

} // end route_package
