// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package @base -- go2cs converted at 2020 August 29 10:01:59 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\signal_unix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        private static os.Signal signalsToIgnore = new slice<os.Signal>(new os.Signal[] { os.Interrupt, syscall.SIGQUIT });

        // SignalTrace is the signal to send to make a Go program
        // crash with a stack trace.
        public static os.Signal SignalTrace = syscall.SIGQUIT;
    }
}}}}
