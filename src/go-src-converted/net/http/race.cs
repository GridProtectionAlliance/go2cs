// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build race

// package http -- go2cs converted at 2020 August 29 08:33:24 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\race.go

using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static void init()
        {
            raceEnabled = true;
        }
    }
}}
