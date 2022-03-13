// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9
// +build plan9

// package socktest -- go2cs converted at 2022 March 13 05:40:14 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\switch_stub.go
namespace go.net.@internal;

public static partial class socktest_package {

// Sockets maps a socket descriptor to the status of socket.
public partial struct Sockets { // : map<nint, Status>
}

private static @string familyString(nint family) {
    return "<nil>";
}

private static @string typeString(nint sotype) {
    return "<nil>";
}

private static @string protocolString(nint proto) {
    return "<nil>";
}

} // end socktest_package
