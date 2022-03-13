// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package signal -- go2cs converted at 2022 March 13 05:28:33 UTC
// import "os/signal" ==> using signal = go.os.signal_package
// Original source: C:\Program Files\Go\src\os\signal\signal_unix.go
namespace go.os;

using os = os_package;
using syscall = syscall_package;


// Defined by the runtime package.

public static partial class signal_package {

private static void signal_disable(uint _p0);
private static void signal_enable(uint _p0);
private static void signal_ignore(uint _p0);
private static bool signal_ignored(uint _p0);
private static uint signal_recv();

private static void loop() {
    while (true) {>>MARKER:FUNCTION_signal_recv_BLOCK_PREFIX<<
        process(syscall.Signal(signal_recv()));
    }
}

private static void init() {
    watchSignalLoop = loop;
}

private static readonly nint numSig = 65; // max across all systems

private static nint signum(os.Signal sig) {
    switch (sig.type()) {
        case syscall.Signal sig:
            var i = int(sig);
            if (i < 0 || i >= numSig) {>>MARKER:FUNCTION_signal_ignored_BLOCK_PREFIX<<
                return -1;
            }
            return i;
            break;
        default:
        {
            var sig = sig.type();
            return -1;
            break;
        }
    }
}

private static void enableSignal(nint sig) {
    signal_enable(uint32(sig));
}

private static void disableSignal(nint sig) {
    signal_disable(uint32(sig));
}

private static void ignoreSignal(nint sig) {
    signal_ignore(uint32(sig));
}

private static bool signalIgnored(nint sig) {
    return signal_ignored(uint32(sig));
}

} // end signal_package
