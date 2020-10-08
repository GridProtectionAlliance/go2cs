// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9 windows

// package @base -- go2cs converted at 2020 October 08 04:36:58 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\signal_notunix.go
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        private static os.Signal signalsToIgnore = new slice<os.Signal>(new os.Signal[] { os.Interrupt });

        // SignalTrace is the signal to send to make a Go program
        // crash with a stack trace (no such signal in this case).
        public static os.Signal SignalTrace = null;
    }
}}}}
