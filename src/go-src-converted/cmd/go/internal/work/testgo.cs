// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains extra hooks for testing the go command.

// +build testgo

// package work -- go2cs converted at 2020 August 29 10:01:38 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\testgo.go
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        private static void init()
        {
            {
                var v = os.Getenv("TESTGO_VERSION");

                if (v != "")
                {
                    runtimeVersion = v;
                }
            }
        }
    }
}}}}
