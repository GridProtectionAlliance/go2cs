// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !netgo
// +build cgo,!netgo

// package net -- go2cs converted at 2022 March 06 22:15:16 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_windows.go


namespace go;

public static partial class net_package {

private partial struct addrinfoErrno { // : nint
}

private static @string Error(this addrinfoErrno eai) {
    return "<nil>";
}
private static bool Temporary(this addrinfoErrno eai) {
    return false;
}
private static bool Timeout(this addrinfoErrno eai) {
    return false;
}

} // end net_package
