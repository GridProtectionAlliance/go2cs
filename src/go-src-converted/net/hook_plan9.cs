// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:51:31 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\hook_plan9.go
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        private static Action testHookDialChannel = () =>
        {
            time.Sleep(time.Millisecond);
        }; // see golang.org/issue/5349
    }
}
