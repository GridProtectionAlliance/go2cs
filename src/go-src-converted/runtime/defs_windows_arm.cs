// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_windows_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _PROT_NONE = (long)0L;
        private static readonly long _PROT_READ = (long)1L;
        private static readonly long _PROT_WRITE = (long)2L;
        private static readonly long _PROT_EXEC = (long)4L;

        private static readonly long _MAP_ANON = (long)1L;
        private static readonly long _MAP_PRIVATE = (long)2L;

        private static readonly ulong _DUPLICATE_SAME_ACCESS = (ulong)0x2UL;
        private static readonly ulong _THREAD_PRIORITY_HIGHEST = (ulong)0x2UL;

        private static readonly ulong _SIGINT = (ulong)0x2UL;
        private static readonly ulong _SIGTERM = (ulong)0xFUL;
        private static readonly ulong _CTRL_C_EVENT = (ulong)0x0UL;
        private static readonly ulong _CTRL_BREAK_EVENT = (ulong)0x1UL;
        private static readonly ulong _CTRL_CLOSE_EVENT = (ulong)0x2UL;
        private static readonly ulong _CTRL_LOGOFF_EVENT = (ulong)0x5UL;
        private static readonly ulong _CTRL_SHUTDOWN_EVENT = (ulong)0x6UL;

        private static readonly ulong _CONTEXT_CONTROL = (ulong)0x10001UL;
        private static readonly ulong _CONTEXT_FULL = (ulong)0x10007UL;

        private static readonly ulong _EXCEPTION_ACCESS_VIOLATION = (ulong)0xc0000005UL;
        private static readonly ulong _EXCEPTION_BREAKPOINT = (ulong)0x80000003UL;
        private static readonly ulong _EXCEPTION_FLT_DENORMAL_OPERAND = (ulong)0xc000008dUL;
        private static readonly ulong _EXCEPTION_FLT_DIVIDE_BY_ZERO = (ulong)0xc000008eUL;
        private static readonly ulong _EXCEPTION_FLT_INEXACT_RESULT = (ulong)0xc000008fUL;
        private static readonly ulong _EXCEPTION_FLT_OVERFLOW = (ulong)0xc0000091UL;
        private static readonly ulong _EXCEPTION_FLT_UNDERFLOW = (ulong)0xc0000093UL;
        private static readonly ulong _EXCEPTION_INT_DIVIDE_BY_ZERO = (ulong)0xc0000094UL;
        private static readonly ulong _EXCEPTION_INT_OVERFLOW = (ulong)0xc0000095UL;

        private static readonly ulong _INFINITE = (ulong)0xffffffffUL;
        private static readonly ulong _WAIT_TIMEOUT = (ulong)0x102UL;

        private static readonly ulong _EXCEPTION_CONTINUE_EXECUTION = (ulong)-0x1UL;
        private static readonly ulong _EXCEPTION_CONTINUE_SEARCH = (ulong)0x0UL;


        private partial struct systeminfo
        {
            public array<byte> anon0;
            public uint dwpagesize;
            public ptr<byte> lpminimumapplicationaddress;
            public ptr<byte> lpmaximumapplicationaddress;
            public uint dwactiveprocessormask;
            public uint dwnumberofprocessors;
            public uint dwprocessortype;
            public uint dwallocationgranularity;
            public ushort wprocessorlevel;
            public ushort wprocessorrevision;
        }

        private partial struct exceptionrecord
        {
            public uint exceptioncode;
            public uint exceptionflags;
            public ptr<exceptionrecord> exceptionrecord;
            public ptr<byte> exceptionaddress;
            public uint numberparameters;
            public array<uint> exceptioninformation;
        }

        private partial struct neon128
        {
            public ulong low;
            public long high;
        }

        private partial struct context
        {
            public uint contextflags;
            public uint r0;
            public uint r1;
            public uint r2;
            public uint r3;
            public uint r4;
            public uint r5;
            public uint r6;
            public uint r7;
            public uint r8;
            public uint r9;
            public uint r10;
            public uint r11;
            public uint r12;
            public uint spr;
            public uint lrr;
            public uint pc;
            public uint cpsr;
            public uint fpscr;
            public uint padding;
            public array<neon128> floatNeon;
            public array<uint> bvr;
            public array<uint> bcr;
            public array<uint> wvr;
            public array<uint> wcr;
            public array<uint> padding2;
        }

        private static System.UIntPtr ip(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.pc);
        }
        private static System.UIntPtr sp(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.spr);
        }
        private static System.UIntPtr lr(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.lrr);
        }

        private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.pc = uint32(x);
        }
        private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.spr = uint32(x);
        }
        private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.lrr = uint32(x);
        }

        private static void dumpregs(ptr<context> _addr_r)
        {
            ref context r = ref _addr_r.val;

            print("r0   ", hex(r.r0), "\n");
            print("r1   ", hex(r.r1), "\n");
            print("r2   ", hex(r.r2), "\n");
            print("r3   ", hex(r.r3), "\n");
            print("r4   ", hex(r.r4), "\n");
            print("r5   ", hex(r.r5), "\n");
            print("r6   ", hex(r.r6), "\n");
            print("r7   ", hex(r.r7), "\n");
            print("r8   ", hex(r.r8), "\n");
            print("r9   ", hex(r.r9), "\n");
            print("r10  ", hex(r.r10), "\n");
            print("r11  ", hex(r.r11), "\n");
            print("r12  ", hex(r.r12), "\n");
            print("sp   ", hex(r.spr), "\n");
            print("lr   ", hex(r.lrr), "\n");
            print("pc   ", hex(r.pc), "\n");
            print("cpsr ", hex(r.cpsr), "\n");
        }

        private partial struct overlapped
        {
            public uint @internal;
            public uint internalhigh;
            public array<byte> anon0;
            public ptr<byte> hevent;
        }

        private partial struct memoryBasicInformation
        {
            public System.UIntPtr baseAddress;
            public System.UIntPtr allocationBase;
            public uint allocationProtect;
            public System.UIntPtr regionSize;
            public uint state;
            public uint protect;
            public uint type_;
        }

        private static void stackcheck()
        { 
            // TODO: not implemented on ARM
        }
    }
}
