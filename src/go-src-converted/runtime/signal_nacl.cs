// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_nacl.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct sigTabT
        {
            public int flags;
            public @string name;
        }

        private static array<sigTabT> sigtable = new array<sigTabT>(new sigTabT[] { {0,"SIGNONE: no trap"}, {_SigNotify+_SigKill,"SIGHUP: terminal line hangup"}, {_SigNotify+_SigKill,"SIGINT: interrupt"}, {_SigNotify+_SigThrow,"SIGQUIT: quit"}, {_SigThrow,"SIGILL: illegal instruction"}, {_SigThrow,"SIGTRAP: trace trap"}, {_SigNotify+_SigThrow,"SIGABRT: abort"}, {_SigThrow,"SIGEMT: emulate instruction executed"}, {_SigPanic,"SIGFPE: floating-point exception"}, {0,"SIGKILL: kill"}, {_SigPanic,"SIGBUS: bus error"}, {_SigPanic,"SIGSEGV: segmentation violation"}, {_SigThrow,"SIGSYS: bad system call"}, {_SigNotify,"SIGPIPE: write to broken pipe"}, {_SigNotify,"SIGALRM: alarm clock"}, {_SigNotify+_SigKill,"SIGTERM: termination"}, {_SigNotify+_SigIgn,"SIGURG: urgent condition on socket"}, {0,"SIGSTOP: stop"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTSTP: keyboard stop"}, {_SigNotify+_SigDefault+_SigIgn,"SIGCONT: continue after stop"}, {_SigNotify+_SigIgn,"SIGCHLD: child status has changed"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTIN: background read from tty"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTOU: background write to tty"}, {_SigNotify,"SIGIO: i/o now possible"}, {_SigNotify,"SIGXCPU: cpu limit exceeded"}, {_SigNotify,"SIGXFSZ: file size limit exceeded"}, {_SigNotify,"SIGVTALRM: virtual alarm clock"}, {_SigNotify,"SIGPROF: profiling alarm clock"}, {_SigNotify,"SIGWINCH: window size change"}, {_SigNotify,"SIGINFO: status request from keyboard"}, {_SigNotify,"SIGUSR1: user-defined signal 1"}, {_SigNotify,"SIGUSR2: user-defined signal 2"} });
    }
}
