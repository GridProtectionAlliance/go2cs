// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_windows.go

// package runtime -- go2cs converted at 2020 October 09 04:45:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_windows_386.go

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

        private partial struct floatingsavearea
        {
            public uint controlword;
            public uint statusword;
            public uint tagword;
            public uint erroroffset;
            public uint errorselector;
            public uint dataoffset;
            public uint dataselector;
            public array<byte> registerarea;
            public uint cr0npxstate;
        }

        private partial struct context
        {
            public uint contextflags;
            public uint dr0;
            public uint dr1;
            public uint dr2;
            public uint dr3;
            public uint dr6;
            public uint dr7;
            public floatingsavearea floatsave;
            public uint seggs;
            public uint segfs;
            public uint seges;
            public uint segds;
            public uint edi;
            public uint esi;
            public uint ebx;
            public uint edx;
            public uint ecx;
            public uint eax;
            public uint ebp;
            public uint eip;
            public uint segcs;
            public uint eflags;
            public uint esp;
            public uint segss;
            public array<byte> extendedregisters;
        }

        private static System.UIntPtr ip(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.eip);
        }
        private static System.UIntPtr sp(this ptr<context> _addr_c)
        {
            ref context c = ref _addr_c.val;

            return uintptr(c.esp);
        }

        // 386 does not have link register, so this returns 0.
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

            c.eip = uint32(x);
        }
        private static void set_sp(this ptr<context> _addr_c, System.UIntPtr x)
        {
            ref context c = ref _addr_c.val;

            c.esp = uint32(x);
        }

        private static void dumpregs(ptr<context> _addr_r)
        {
            ref context r = ref _addr_r.val;

            print("eax     ", hex(r.eax), "\n");
            print("ebx     ", hex(r.ebx), "\n");
            print("ecx     ", hex(r.ecx), "\n");
            print("edx     ", hex(r.edx), "\n");
            print("edi     ", hex(r.edi), "\n");
            print("esi     ", hex(r.esi), "\n");
            print("ebp     ", hex(r.ebp), "\n");
            print("esp     ", hex(r.esp), "\n");
            print("eip     ", hex(r.eip), "\n");
            print("eflags  ", hex(r.eflags), "\n");
            print("cs      ", hex(r.segcs), "\n");
            print("fs      ", hex(r.segfs), "\n");
            print("gs      ", hex(r.seggs), "\n");
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
    }
}
