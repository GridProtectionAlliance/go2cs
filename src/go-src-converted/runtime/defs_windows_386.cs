// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_windows.go

// package runtime -- go2cs converted at 2020 August 29 08:16:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_windows_386.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _PROT_NONE = 0L;
        private static readonly long _PROT_READ = 1L;
        private static readonly long _PROT_WRITE = 2L;
        private static readonly long _PROT_EXEC = 4L;

        private static readonly long _MAP_ANON = 1L;
        private static readonly long _MAP_PRIVATE = 2L;

        private static readonly ulong _DUPLICATE_SAME_ACCESS = 0x2UL;
        private static readonly ulong _THREAD_PRIORITY_HIGHEST = 0x2UL;

        private static readonly ulong _SIGINT = 0x2UL;
        private static readonly ulong _CTRL_C_EVENT = 0x0UL;
        private static readonly ulong _CTRL_BREAK_EVENT = 0x1UL;

        private static readonly ulong _CONTEXT_CONTROL = 0x10001UL;
        private static readonly ulong _CONTEXT_FULL = 0x10007UL;

        private static readonly ulong _EXCEPTION_ACCESS_VIOLATION = 0xc0000005UL;
        private static readonly ulong _EXCEPTION_BREAKPOINT = 0x80000003UL;
        private static readonly ulong _EXCEPTION_FLT_DENORMAL_OPERAND = 0xc000008dUL;
        private static readonly ulong _EXCEPTION_FLT_DIVIDE_BY_ZERO = 0xc000008eUL;
        private static readonly ulong _EXCEPTION_FLT_INEXACT_RESULT = 0xc000008fUL;
        private static readonly ulong _EXCEPTION_FLT_OVERFLOW = 0xc0000091UL;
        private static readonly ulong _EXCEPTION_FLT_UNDERFLOW = 0xc0000093UL;
        private static readonly ulong _EXCEPTION_INT_DIVIDE_BY_ZERO = 0xc0000094UL;
        private static readonly ulong _EXCEPTION_INT_OVERFLOW = 0xc0000095UL;

        private static readonly ulong _INFINITE = 0xffffffffUL;
        private static readonly ulong _WAIT_TIMEOUT = 0x102UL;

        private static readonly ulong _EXCEPTION_CONTINUE_EXECUTION = -0x1UL;
        private static readonly ulong _EXCEPTION_CONTINUE_SEARCH = 0x0UL;

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

        private static System.UIntPtr ip(this ref context c)
        {
            return uintptr(c.eip);
        }
        private static System.UIntPtr sp(this ref context c)
        {
            return uintptr(c.esp);
        }

        private static void setip(this ref context c, System.UIntPtr x)
        {
            c.eip = uint32(x);

        }
        private static void setsp(this ref context c, System.UIntPtr x)
        {
            c.esp = uint32(x);

        }

        private static void dumpregs(ref context r)
        {
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
    }
}
