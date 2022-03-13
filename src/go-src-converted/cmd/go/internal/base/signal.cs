// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 13 06:32:32 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\signal.go
namespace go.cmd.go.@internal;

using os = os_package;
using signal = os.signal_package;
using sync = sync_package;


// Interrupted is closed when the go command receives an interrupt signal.

using System;
using System.Threading;
public static partial class @base_package {

public static var Interrupted = make_channel<object>();

// processSignals setups signal handler.
private static void processSignals() {
    var sig = make_channel<os.Signal>(1);
    signal.Notify(sig, signalsToIgnore);
    go_(() => () => {
        sig.Receive();
        close(Interrupted);
    }());
}

private static sync.Once onceProcessSignals = default;

// StartSigHandlers starts the signal handlers.
public static void StartSigHandlers() {
    onceProcessSignals.Do(processSignals);
}

} // end @base_package
