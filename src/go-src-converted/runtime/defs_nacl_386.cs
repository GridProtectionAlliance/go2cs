// package runtime -- go2cs converted at 2020 August 29 08:16:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_nacl_386.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // These values are referred to in the source code
        // but really don't matter. Even so, use the standard numbers.
        private static readonly long _SIGQUIT = 3L;
        private static readonly long _SIGSEGV = 11L;
        private static readonly long _SIGPROF = 27L;

        private partial struct timespec
        {
            public long tv_sec;
            public int tv_nsec;
        }

        private partial struct excregs386
        {
            public uint eax;
            public uint ecx;
            public uint edx;
            public uint ebx;
            public uint esp;
            public uint ebp;
            public uint esi;
            public uint edi;
            public uint eip;
            public uint eflags;
        }

        private partial struct exccontext
        {
            public uint size;
            public uint portable_context_offset;
            public uint portable_context_size;
            public uint arch;
            public uint regs_size;
            public array<uint> reserved;
            public excregs386 regs;
        }

        private partial struct excportablecontext
        {
            public uint pc;
            public uint sp;
            public uint fp;
        }
    }
}
