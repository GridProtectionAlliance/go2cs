// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9-specific system calls

// package runtime -- go2cs converted at 2022 March 06 22:10:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os2_plan9.go


namespace go;

public static partial class runtime_package {

    // open
private static readonly nint _OREAD = 0;
private static readonly nint _OWRITE = 1;
private static readonly nint _ORDWR = 2;
private static readonly nint _OEXEC = 3;
private static readonly nint _OTRUNC = 16;
private static readonly nint _OCEXEC = 32;
private static readonly nint _ORCLOSE = 64;
private static readonly nuint _OEXCL = 0x1000;


// rfork
private static readonly nint _RFNAMEG = 1 << 0;
private static readonly nint _RFENVG = 1 << 1;
private static readonly nint _RFFDG = 1 << 2;
private static readonly nint _RFNOTEG = 1 << 3;
private static readonly nint _RFPROC = 1 << 4;
private static readonly nint _RFMEM = 1 << 5;
private static readonly nint _RFNOWAIT = 1 << 6;
private static readonly nint _RFCNAMEG = 1 << 10;
private static readonly nint _RFCENVG = 1 << 11;
private static readonly nint _RFCFDG = 1 << 12;
private static readonly nint _RFREND = 1 << 13;
private static readonly nint _RFNOMNT = 1 << 14;


// notify
private static readonly nint _NCONT = 0;
private static readonly nint _NDFLT = 1;


private partial struct uinptr { // : _Plink
}

private partial struct tos {
    public ulong cyclefreq; // cycle clock frequency if there is one, 0 otherwise
    public long kcycles; // cycles spent in kernel
    public long pcycles; // cycles spent in process (kernel + user)
    public uint pid; // might as well put the pid here
    public uint clock; // top of stack is here
}

private static readonly nint _NSIG = 14; // number of signals in sigtable array
private static readonly nint _ERRMAX = 128; // max length of note string

// Notes in runtime·sigtab that are handled by runtime·sigpanic.
private static readonly nint _SIGRFAULT = 2;
private static readonly nint _SIGWFAULT = 3;
private static readonly nint _SIGINTDIV = 4;
private static readonly nint _SIGFLOAT = 5;
private static readonly nint _SIGTRAP = 6;
private static readonly nint _SIGPROF = 0; // dummy value defined for badsignal
private static readonly nint _SIGQUIT = 0; // dummy value defined for sighandler

} // end runtime_package
