// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:48 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\hook_plan9.go
using time = go.time_package;
using System;


namespace go;

public static partial class net_package {

private static Action testHookDialChannel = () => {
    time.Sleep(time.Millisecond);
}; // see golang.org/issue/5349

} // end net_package
