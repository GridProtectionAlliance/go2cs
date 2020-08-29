// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package web defines helper routines for accessing HTTP/HTTPS resources.
// package web -- go2cs converted at 2020 August 29 10:01:09 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\security.go

using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        // SecurityMode specifies whether a function should make network
        // calls using insecure transports (eg, plain text HTTP).
        // The zero value is "secure".
        public partial struct SecurityMode // : long
        {
        }

        public static readonly SecurityMode Secure = iota;
        public static readonly var Insecure = 0;
    }
}}}}
