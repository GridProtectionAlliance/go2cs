// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 09 05:48:05 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\signal.go
using os = go.os_package;
using signal = go.os.signal_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // Interrupted is closed when the go command receives an interrupt signal.
        public static var Interrupted = make_channel<object>();

        // processSignals setups signal handler.
        private static void processSignals()
        {
            var sig = make_channel<os.Signal>();
            signal.Notify(sig, signalsToIgnore);
            go_(() => () =>
            {
                sig.Receive();
                close(Interrupted);
            }());

        }

        private static sync.Once onceProcessSignals = default;

        // StartSigHandlers starts the signal handlers.
        public static void StartSigHandlers()
        {
            onceProcessSignals.Do(processSignals);
        }
    }
}}}}
