// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_windows.go

// package runtime -- go2cs converted at 2020 October 09 04:45:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_windows_amd64.go

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

        private static readonly ulong _CONTEXT_CONTROL = (ulong)0x100001UL;
        private static readonly ulong _CONTEXT_FULL = (ulong)0x10000bUL;

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
            public ulong dwactiveprocessormask;
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
            public array<byte> pad_cgo_0;
            public array<ulong> exceptioninformation;
        }

        private partial struct m128a
        {
            public ulong low;
            public long high;
        }

        private partial struct context
        {
            public ulong p1home;
            public ulong p2home;
            public ulong p3home;
            public ulong p4home;
            public ulong p5home;
            public ulong p6home;
            public uint contextflags;
            public uint mxcsr;
            public ushort segcs;
            public ushort segds;
            public ushort seges;
            public ushort segfs;
            public ushort seggs;
            public ushort segss;
            public uint eflags;
            public ulong dr0;
            public ulong dr1;
            public ulong dr2;
            public ulong dr3;
            public ulong dr6;
            public ulong dr7;
            public ulong rax;
            public ulong rcx;
            public ulong rdx;
            public ulong rbx;
            public ulong rsp;
            public ulong rbp;
            public ulong rsi;
            public ulong rdi;
            public ulong r8;
            public ulong r9;
            public ulong r10;
            public ulong r11;
            public ulong r12;
            public ulong r13;
            public ulong r14;
            public ulong r15;
            public ulong rip;
            public array<byte> anon0;
            public array<m128a> vectorregister;
            public ulong vectorcontrol;
            public ulong debugcontrol;
            public ulong lastbranchtorip;
            public ulong lastbranchfromrip;
            public ulong lastexceptiontorip;
            public ulong lastexceptionfromrip;
        }

        private static System.UIntPtr ip(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.rip);
        }
        private static System.UIntPtr sp(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.rsp);
        }

        // Amd64 does not have link register, so this returns 0.
        private static System.UIntPtr lr(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return 0L;
        }
        private static void set_lr(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

        }

        private static void set_ip(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.rip = uint64(x);
        }
        private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.rsp = uint64(x);
        }

        private static void dumpregs(ptr<context> _addr_r)
        {
            ref context r = ref _addr_r.val;

            print("rax     ", hex(r.rax), "\n");
            print("rbx     ", hex(r.rbx), "\n");
            print("rcx     ", hex(r.rcx), "\n");
            print("rdi     ", hex(r.rdi), "\n");
            print("rsi     ", hex(r.rsi), "\n");
            print("rbp     ", hex(r.rbp), "\n");
            print("rsp     ", hex(r.rsp), "\n");
            print("r8      ", hex(r.r8), "\n");
            print("r9      ", hex(r.r9), "\n");
            print("r10     ", hex(r.r10), "\n");
            print("r11     ", hex(r.r11), "\n");
            print("r12     ", hex(r.r12), "\n");
            print("r13     ", hex(r.r13), "\n");
            print("r14     ", hex(r.r14), "\n");
            print("r15     ", hex(r.r15), "\n");
            print("rip     ", hex(r.rip), "\n");
            print("rflags  ", hex(r.eflags), "\n");
            print("cs      ", hex(r.segcs), "\n");
            print("fs      ", hex(r.segfs), "\n");
            print("gs      ", hex(r.seggs), "\n");
        }

        private partial struct overlapped
        {
            public ulong @internal;
            public ulong internalhigh;
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
    }
}
