// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows

// package testenv -- go2cs converted at 2020 October 09 06:06:11 UTC
// import "internal/testenv" ==> using testenv = go.@internal.testenv_package
// Original source: C:\Go\src\internal\testenv\testenv_notwin.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class testenv_package
    {
        private static (bool, @string) hasSymlink()
        {
            bool ok = default;
            @string reason = default;

            switch (runtime.GOOS)
            {
                case "android": 

                case "plan9": 
                    return (false, "");
                    break;
            }

            return (true, "");

        }
    }
}}
