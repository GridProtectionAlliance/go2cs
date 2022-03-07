// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows architecture-independent definitions.

// package runtime -- go2cs converted at 2022 March 06 22:08:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_windows.go


namespace go;

public static partial class runtime_package {

private static readonly nint _PROT_NONE = 0;
private static readonly nint _PROT_READ = 1;
private static readonly nint _PROT_WRITE = 2;
private static readonly nint _PROT_EXEC = 4;

private static readonly nint _MAP_ANON = 1;
private static readonly nint _MAP_PRIVATE = 2;

private static readonly nuint _DUPLICATE_SAME_ACCESS = 0x2;
private static readonly nuint _THREAD_PRIORITY_HIGHEST = 0x2;

private static readonly nuint _SIGINT = 0x2;
private static readonly nuint _SIGTERM = 0xF;
private static readonly nuint _CTRL_C_EVENT = 0x0;
private static readonly nuint _CTRL_BREAK_EVENT = 0x1;
private static readonly nuint _CTRL_CLOSE_EVENT = 0x2;
private static readonly nuint _CTRL_LOGOFF_EVENT = 0x5;
private static readonly nuint _CTRL_SHUTDOWN_EVENT = 0x6;

private static readonly nuint _EXCEPTION_ACCESS_VIOLATION = 0xc0000005;
private static readonly nuint _EXCEPTION_BREAKPOINT = 0x80000003;
private static readonly nuint _EXCEPTION_ILLEGAL_INSTRUCTION = 0xc000001d;
private static readonly nuint _EXCEPTION_FLT_DENORMAL_OPERAND = 0xc000008d;
private static readonly nuint _EXCEPTION_FLT_DIVIDE_BY_ZERO = 0xc000008e;
private static readonly nuint _EXCEPTION_FLT_INEXACT_RESULT = 0xc000008f;
private static readonly nuint _EXCEPTION_FLT_OVERFLOW = 0xc0000091;
private static readonly nuint _EXCEPTION_FLT_UNDERFLOW = 0xc0000093;
private static readonly nuint _EXCEPTION_INT_DIVIDE_BY_ZERO = 0xc0000094;
private static readonly nuint _EXCEPTION_INT_OVERFLOW = 0xc0000095;

private static readonly nuint _INFINITE = 0xffffffff;
private static readonly nuint _WAIT_TIMEOUT = 0x102;

private static readonly nuint _EXCEPTION_CONTINUE_EXECUTION = -0x1;
private static readonly nuint _EXCEPTION_CONTINUE_SEARCH = 0x0;


private partial struct systeminfo {
    public array<byte> anon0;
    public uint dwpagesize;
    public ptr<byte> lpminimumapplicationaddress;
    public ptr<byte> lpmaximumapplicationaddress;
    public System.UIntPtr dwactiveprocessormask;
    public uint dwnumberofprocessors;
    public uint dwprocessortype;
    public uint dwallocationgranularity;
    public ushort wprocessorlevel;
    public ushort wprocessorrevision;
}

private partial struct exceptionrecord {
    public uint exceptioncode;
    public uint exceptionflags;
    public ptr<exceptionrecord> exceptionrecord;
    public ptr<byte> exceptionaddress;
    public uint numberparameters;
    public array<System.UIntPtr> exceptioninformation;
}

private partial struct overlapped {
    public System.UIntPtr @internal;
    public System.UIntPtr internalhigh;
    public array<byte> anon0;
    public ptr<byte> hevent;
}

private partial struct memoryBasicInformation {
    public System.UIntPtr baseAddress;
    public System.UIntPtr allocationBase;
    public uint allocationProtect;
    public System.UIntPtr regionSize;
    public uint state;
    public uint protect;
    public uint type_;
}

} // end runtime_package
