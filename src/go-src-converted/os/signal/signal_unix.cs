// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows

// package signal -- go2cs converted at 2020 October 09 05:01:00 UTC
// import "os/signal" ==> using signal = go.os.signal_package
// Original source: C:\Go\src\os\signal\signal_unix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class signal_package
    {
        // Defined by the runtime package.
        private static void signal_disable(uint _p0)
;
        private static void signal_enable(uint _p0)
;
        private static void signal_ignore(uint _p0)
;
        private static bool signal_ignored(uint _p0)
;
        private static uint signal_recv()
;

        private static void loop()
        {
            while (true)
            {>>MARKER:FUNCTION_signal_recv_BLOCK_PREFIX<<
                process(syscall.Signal(signal_recv()));
            }


        }

        private static void init()
        {
            watchSignalLoop = loop;
        }

        private static readonly long numSig = (long)65L; // max across all systems

        private static long signum(os.Signal sig)
        {
            switch (sig.type())
            {
                case syscall.Signal sig:
                    var i = int(sig);
                    if (i < 0L || i >= numSig)
                    {>>MARKER:FUNCTION_signal_ignored_BLOCK_PREFIX<<
                        return -1L;
                    }

                    return i;
                    break;
                default:
                {
                    var sig = sig.type();
                    return -1L;
                    break;
                }
            }

        }

        private static void enableSignal(long sig)
        {
            signal_enable(uint32(sig));
        }

        private static void disableSignal(long sig)
        {
            signal_disable(uint32(sig));
        }

        private static void ignoreSignal(long sig)
        {
            signal_ignore(uint32(sig));
        }

        private static bool signalIgnored(long sig)
        {
            return signal_ignored(uint32(sig));
        }
    }
}}
