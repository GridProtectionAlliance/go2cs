// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package signal -- go2cs converted at 2022 March 06 22:14:26 UTC
// import "os/signal" ==> using signal = go.os.signal_package
// Original source: C:\Program Files\Go\src\os\signal\signal_plan9.go
using os = go.os_package;
using syscall = go.syscall_package;

namespace go.os;

public static partial class signal_package {

private static var sigtab = make_map<os.Signal, nint>();

// Defined by the runtime package.
private static void signal_disable(uint _p0);
private static void signal_enable(uint _p0);
private static void signal_ignore(uint _p0);
private static bool signal_ignored(uint _p0);
private static @string signal_recv();

private static void init() {
    watchSignalLoop = loop;
}

private static void loop() {
    while (true) {>>MARKER:FUNCTION_signal_recv_BLOCK_PREFIX<<
        process(syscall.Note(signal_recv()));
    }
}

private static readonly nint numSig = 256;



private static nint signum(os.Signal sig) {
    switch (sig.type()) {
        case syscall.Note sig:
            var (n, ok) = sigtab[sig];
            if (!ok) {>>MARKER:FUNCTION_signal_ignored_BLOCK_PREFIX<<
                n = len(sigtab) + 1;
                if (n > numSig) {>>MARKER:FUNCTION_signal_ignore_BLOCK_PREFIX<<
                    return -1;
                }
                sigtab[sig] = n;
            }
            return n;
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
