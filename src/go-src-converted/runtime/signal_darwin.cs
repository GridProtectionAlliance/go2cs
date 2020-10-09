// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:16 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_darwin.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static array<sigTabT> sigtable = new array<sigTabT>(new sigTabT[] { {0,"SIGNONE: no trap"}, {_SigNotify+_SigKill,"SIGHUP: terminal line hangup"}, {_SigNotify+_SigKill,"SIGINT: interrupt"}, {_SigNotify+_SigThrow,"SIGQUIT: quit"}, {_SigThrow+_SigUnblock,"SIGILL: illegal instruction"}, {_SigThrow+_SigUnblock,"SIGTRAP: trace trap"}, {_SigNotify+_SigThrow,"SIGABRT: abort"}, {_SigThrow,"SIGEMT: emulate instruction executed"}, {_SigPanic+_SigUnblock,"SIGFPE: floating-point exception"}, {0,"SIGKILL: kill"}, {_SigPanic+_SigUnblock,"SIGBUS: bus error"}, {_SigPanic+_SigUnblock,"SIGSEGV: segmentation violation"}, {_SigThrow,"SIGSYS: bad system call"}, {_SigNotify,"SIGPIPE: write to broken pipe"}, {_SigNotify,"SIGALRM: alarm clock"}, {_SigNotify+_SigKill,"SIGTERM: termination"}, {_SigNotify+_SigIgn,"SIGURG: urgent condition on socket"}, {0,"SIGSTOP: stop"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTSTP: keyboard stop"}, {_SigNotify+_SigDefault+_SigIgn,"SIGCONT: continue after stop"}, {_SigNotify+_SigUnblock+_SigIgn,"SIGCHLD: child status has changed"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTIN: background read from tty"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTOU: background write to tty"}, {_SigNotify+_SigIgn,"SIGIO: i/o now possible"}, {_SigNotify,"SIGXCPU: cpu limit exceeded"}, {_SigNotify,"SIGXFSZ: file size limit exceeded"}, {_SigNotify,"SIGVTALRM: virtual alarm clock"}, {_SigNotify+_SigUnblock,"SIGPROF: profiling alarm clock"}, {_SigNotify+_SigIgn,"SIGWINCH: window size change"}, {_SigNotify+_SigIgn,"SIGINFO: status request from keyboard"}, {_SigNotify,"SIGUSR1: user-defined signal 1"}, {_SigNotify,"SIGUSR2: user-defined signal 2"} });
    }
}
