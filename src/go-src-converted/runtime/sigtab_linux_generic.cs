// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !mips && !mipsle && !mips64 && !mips64le && linux
// +build !mips,!mipsle,!mips64,!mips64le,linux

// package runtime -- go2cs converted at 2022 March 13 05:27:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sigtab_linux_generic.go
namespace go;

public static partial class runtime_package {

private static array<sigTabT> sigtable = new array<sigTabT>(new sigTabT[] { {0,"SIGNONE: no trap"}, {_SigNotify+_SigKill,"SIGHUP: terminal line hangup"}, {_SigNotify+_SigKill,"SIGINT: interrupt"}, {_SigNotify+_SigThrow,"SIGQUIT: quit"}, {_SigThrow+_SigUnblock,"SIGILL: illegal instruction"}, {_SigThrow+_SigUnblock,"SIGTRAP: trace trap"}, {_SigNotify+_SigThrow,"SIGABRT: abort"}, {_SigPanic+_SigUnblock,"SIGBUS: bus error"}, {_SigPanic+_SigUnblock,"SIGFPE: floating-point exception"}, {0,"SIGKILL: kill"}, {_SigNotify,"SIGUSR1: user-defined signal 1"}, {_SigPanic+_SigUnblock,"SIGSEGV: segmentation violation"}, {_SigNotify,"SIGUSR2: user-defined signal 2"}, {_SigNotify,"SIGPIPE: write to broken pipe"}, {_SigNotify,"SIGALRM: alarm clock"}, {_SigNotify+_SigKill,"SIGTERM: termination"}, {_SigThrow+_SigUnblock,"SIGSTKFLT: stack fault"}, {_SigNotify+_SigUnblock+_SigIgn,"SIGCHLD: child status has changed"}, {_SigNotify+_SigDefault+_SigIgn,"SIGCONT: continue"}, {0,"SIGSTOP: stop, unblockable"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTSTP: keyboard stop"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTIN: background read from tty"}, {_SigNotify+_SigDefault+_SigIgn,"SIGTTOU: background write to tty"}, {_SigNotify+_SigIgn,"SIGURG: urgent condition on socket"}, {_SigNotify,"SIGXCPU: cpu limit exceeded"}, {_SigNotify,"SIGXFSZ: file size limit exceeded"}, {_SigNotify,"SIGVTALRM: virtual alarm clock"}, {_SigNotify+_SigUnblock,"SIGPROF: profiling alarm clock"}, {_SigNotify+_SigIgn,"SIGWINCH: window size change"}, {_SigNotify,"SIGIO: i/o now possible"}, {_SigNotify,"SIGPWR: power failure restart"}, {_SigThrow,"SIGSYS: bad system call"}, {_SigSetStack+_SigUnblock,"signal 32"}, {_SigSetStack+_SigUnblock,"signal 33"}, {_SigSetStack+_SigUnblock,"signal 34"}, {_SigNotify,"signal 35"}, {_SigNotify,"signal 36"}, {_SigNotify,"signal 37"}, {_SigNotify,"signal 38"}, {_SigNotify,"signal 39"}, {_SigNotify,"signal 40"}, {_SigNotify,"signal 41"}, {_SigNotify,"signal 42"}, {_SigNotify,"signal 43"}, {_SigNotify,"signal 44"}, {_SigNotify,"signal 45"}, {_SigNotify,"signal 46"}, {_SigNotify,"signal 47"}, {_SigNotify,"signal 48"}, {_SigNotify,"signal 49"}, {_SigNotify,"signal 50"}, {_SigNotify,"signal 51"}, {_SigNotify,"signal 52"}, {_SigNotify,"signal 53"}, {_SigNotify,"signal 54"}, {_SigNotify,"signal 55"}, {_SigNotify,"signal 56"}, {_SigNotify,"signal 57"}, {_SigNotify,"signal 58"}, {_SigNotify,"signal 59"}, {_SigNotify,"signal 60"}, {_SigNotify,"signal 61"}, {_SigNotify,"signal 62"}, {_SigNotify,"signal 63"}, {_SigNotify,"signal 64"} });

} // end runtime_package
